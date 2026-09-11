"""Source-only continuity regression checks. Run with python3 -B <this file>.

This deliberately supports only this story's linear knots, gathered choices,
assignments and block conditions. Unknown syntax fails. It is NOT an Ink
compiler/runtime and proves neither Unity rendering nor saved-game compatibility.
"""

import ast
from functools import lru_cache
import hashlib
import itertools
from pathlib import Path
import re
import unittest


PROJECT = Path(__file__).resolve().parents[2]
INK = PROJECT / "Assets/Ink"
STORY = "kolodets-kotoryy-zovet"
# Compared to all ten files at b5c73d39 before fixing the story. Independent of
# git history so the check also works in a source archive or shallow checkout.
CHOICE_LABEL_DIGEST = "1168cd8a63e407fd17f11af82026bb68a81692a1d22381af2c211ba72ef9ef6b"
CHOICE = re.compile(r"\* \((\w+)\) \[([^\]]+)\]$")


@lru_cache(None)
def expression(source):
    return ast.parse(source, mode="eval").body


def value(source, state):
    def visit(node):
        if isinstance(node, ast.Constant):
            return node.value
        if isinstance(node, ast.Name):
            if node.id in ("true", "false"):
                return node.id == "true"
            return state[node.id]
        if isinstance(node, ast.UnaryOp) and isinstance(node.op, ast.Not):
            return not visit(node.operand)
        if isinstance(node, ast.BinOp) and isinstance(node.op, ast.Add):
            return visit(node.left) + visit(node.right)
        if isinstance(node, ast.BoolOp):
            vals = [visit(item) for item in node.values]
            if isinstance(node.op, ast.And):
                return all(vals)
            if isinstance(node.op, ast.Or):
                return any(vals)
        if isinstance(node, ast.Compare) and len(node.ops) == 1:
            lhs, rhs = visit(node.left), visit(node.comparators[0])
            op = node.ops[0]
            if isinstance(op, ast.Eq):
                return lhs == rhs
            if isinstance(op, ast.NotEq):
                return lhs != rhs
            if isinstance(op, ast.Gt):
                return lhs > rhs
            if isinstance(op, ast.GtE):
                return lhs >= rhs
            if isinstance(op, ast.Lt):
                return lhs < rhs
        raise AssertionError(f"Unsupported expression: {source}")
    return visit(expression(source))


class SourceModel:
    def __init__(self):
        self.defaults, self.episodes, self.groups = {}, [], []
        for number in range(1, 11):
            source = (INK / f"s01e{number:02}.ink").read_text()
            lines = [line.strip() for line in source.splitlines()
                     if line.strip() and not line.lstrip().startswith("//")]
            while lines and not lines[0].startswith("=== "):
                line = lines.pop(0)
                if line.startswith("VAR "):
                    key, expr = line[4:].split(" = ", 1)
                    assert key not in self.defaults
                    self.defaults[key] = value(expr, self.defaults)
                else:
                    assert number == 1 and line == "-> KZs01e01", line
            assert lines.pop(0) == f"=== KZs01e{number:02} ==="
            expected = "END" if number == 10 else f"KZs01e{number + 1:02}"
            assert lines.pop() == f"-> {expected}", (number, "broken divert")
            nodes, end = self.parse(lines, 0, set())
            assert end == len(lines)
            self.episodes.append(nodes)

    def parse(self, lines, index, stops):
        nodes = []
        while index < len(lines):
            line = lines[index]
            if line in stops or ("*" in stops and line.startswith("* ")):
                break
            if line.startswith("{ ") and line.endswith(":"):
                expr = line[2:-1]
                expression(expr)
                body, index = self.parse(lines, index + 1, {"}"})
                assert index < len(lines) and lines[index] == "}", expr
                nodes.append(("if", expr, body))
                index += 1
            elif line.startswith("* "):
                choices = []
                while index < len(lines) and lines[index].startswith("* "):
                    match = CHOICE.fullmatch(lines[index])
                    assert match, lines[index]
                    label = match[1]
                    body, index = self.parse(lines, index + 1, {"*", "-"})
                    choices.append((label, body))
                assert index < len(lines) and lines[index] == "-", choices
                self.groups.append([label for label, _ in choices])
                nodes.append(("choice", choices))
                index += 1
            elif line.startswith("~ "):
                key, expr = line[2:].split(" = ", 1)
                assert key in self.defaults, key
                expression(expr)
                nodes.append(("set", key, expr))
                index += 1
            else:
                # New Ink constructs require real parser support, not silent skipping.
                assert re.match(r"[^{}*~<>#=]+:", line), line
                assert "{" not in line and "}" not in line, line
                nodes.append(("text", line))
                index += 1
        return nodes, index

    def run(self, selected=(), last=False):
        selected = set(selected)
        known = {label for group in self.groups for label in group}
        assert selected <= known, selected - known
        state, output, taken = dict(self.defaults), [], set()

        def walk(nodes):
            for node in nodes:
                kind = node[0]
                if kind == "text":
                    output.append(node[1])
                elif kind == "set":
                    state[node[1]] = value(node[2], state)
                elif kind == "if":
                    if value(node[1], state):
                        walk(node[2])
                elif kind == "choice":
                    explicit = [option for option in node[1] if option[0] in selected]
                    assert len(explicit) <= 1, explicit
                    label, body = explicit[0] if explicit else node[1][-1 if last else 0]
                    taken.add(label)
                    walk(body)
        for episode in self.episodes:
            walk(episode)
        assert selected <= taken
        return state, "\n".join(output), taken


class ContinuityTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.model = SourceModel()

    def test_preserve_episode_and_choice_addresses(self):
        labels = [label for group in self.model.groups for label in group]
        self.assertEqual(len(labels), len(set(labels)))
        digest = hashlib.sha256("\n".join(labels).encode()).hexdigest()
        self.assertEqual(digest, CHOICE_LABEL_DIGEST)
        self.assertEqual(len(labels), 84)
        root = (INK / f"{STORY}.ink").read_text()
        self.assertEqual(re.findall(r"^INCLUDE (.+)$", root, re.M),
                         [f"s01e{n:02}.ink" for n in range(1, 11)])

    def test_each_choice_reachable_in_source_model(self):
        covered = set()
        for group in self.model.groups:
            for label in group:
                state, text, taken = self.model.run([label])
                covered.update(taken)
                self.assertEqual(text.count("...: КОНЕЦ СЕРИИ"), 10)
                self.assertEqual(text.count("...: ФИНАЛ:"), 1)
                self.assertTrue(state["personal_truth_told"])
        self.assertEqual(len(covered), 84)

    def test_tape_map_and_echo_presentation(self):
        _, left, _ = self.model.run(["leave_tape", "hide_map"])
        self.assertIn("Кассета осталась дома", left)
        self.assertNotIn("включила принесённую кассету", left)
        self.assertNotIn("Звук: cassette-click", left)
        self.assertNotIn("Он узнал карту в руках Сони", left)
        _, taken, _ = self.model.run(["take_tape", "show_sonya_map"])
        self.assertIn("включила принесённую кассету", taken)
        self.assertIn("Звук: cassette-click", taken)
        self.assertNotIn("Кассета осталась дома", taken)
        for n in (1, 2, 3):
            source = (INK / f"s01e{n:02}.ink").read_text()
            spoken = re.findall(r"^Лёша.*$", source, re.M)
            self.assertEqual(len(spoken), 1 if n == 3 else 0)

    def test_recording_and_disclosure_are_independent_of_witness(self):
        _, text, _ = self.model.run(["break_phone", "give_sonya_copy", "mira_witness"])
        self.assertIn("Голоса дождя я не записывала", text)
        self.assertNotIn("Я удалила записи", text)
        self.assertNotIn("Копии останутся у меня", text)
        self.assertIn("Протокол у меня", text)
        self.assertIn("Мира подписала отчёт", text)
        low_trust = ["ask_house", "inspect_jar", "hide_map", "test_phrase",
                     "record_rain", "small_circle", "save_sonya"]
        _, text, _ = self.model.run(low_trust)
        self.assertIn("Копии останутся у меня", text)
        _, text, _ = self.model.run(["record_rain", "show_sonya_map"])
        self.assertIn("Я удалила записи", text)

    def test_confession_is_told_before_final_for_both_initial_routes(self):
        _, withheld, _ = self.model.run(["soften_past", "share_good_memory"])
        self.assertIn("Раньше я не рассказала это до конца", withheld)
        _, told, _ = self.model.run(["tell_father", "share_good_memory"])
        self.assertNotIn("Раньше я не рассказала это до конца", told)
        self.assertIn("я выдала отцу твой тайник", told)

    def test_team_and_previous_attitude_have_callbacks(self):
        _, split, _ = self.model.run(["split_team", "refuse_trade"])
        self.assertIn("Мира и Соня вернулись в баню к Лёше и Тихону", split)
        self.assertIn("Ты уже говорила, что не хочешь обмена", split)
        _, vera, _ = self.model.run(["trust_vera", "ask_cost"])
        self.assertIn("Доверенное ей дело было сделано", vera)
        self.assertIn("Ты спросила меня о цене", vera)
        _, together, _ = self.model.run(["team_together", "promise_memory"])
        self.assertNotIn("Доверенное ей дело было сделано", together)
        self.assertNotIn("Мира и Соня вернулись в баню к Лёше и Тихону", together)
        self.assertIn("Я обещала заплатить, ещё не зная правил", together)

    def test_listening_does_not_restore_childhood(self):
        for ending in ("return_consent", "open_everything", "seal_forever",
                       "take_all", "break_chain"):
            _, text, _ = self.model.run(["take_tape", ending])
            workshop = text.split("...: КОНЕЦ СЕРИИ")[3]
            self.assertIn("не помню ни этого голоса, ни самой просьбы", workshop)
            self.assertNotIn("Я помню, как ты шептала", workshop)
            self.assertNotIn("выслушал историю до конца", workshop)
            self.assertEqual("Лёша увидел одну ночь побега" in text,
                             ending == "return_consent")

    def test_chronology_transport_and_revelation_bridges(self):
        for tape in ("take_tape", "leave_tape"):
            _, text, _ = self.model.run([tape, "save_sonya", "take_all"])
            self.assertIn("Она начиналась годом той засухи", text)
            self.assertIn("К амбулатории добрались под утро", text)
            self.assertIn("По дороге в интернат Тихон свернул к кладбищу", text)
            self.assertLess(text.index("взяла переносной магнитофон с собой"),
                            text.index("Радио перебрало старые обрывки"))
            self.assertIn("старые трубы не разъединены", text)
            self.assertIn("по верхней дороге, в обход повреждённого моста", text)
            self.assertIn("рядом с интернатом, где работал сам", text)
            self.assertNotIn("пансионат", text)
            kitchen = text.index("Локация: bg03-kitchen-storm")
            repair = text.index("С ломом Веры Тихон и Алексей вернулись к пруду")
            letter = text.index("Мира нашла последнее письмо Аграфены")
            self.assertLess(kitchen, repair)
            self.assertLess(repair, letter)

    def test_formula_copies_do_not_replace_or_destroy_protocol(self):
        for disclosure, formula in itertools.product(
                ("share_discovery", "small_circle", "give_sonya_copy"),
                ("write_rule", "destroy_formula", "keep_formula_secret")):
            _, text, _ = self.model.run([disclosure, formula])
            self.assertEqual("оставила лист у себя" in text, formula == "write_rule")
            self.assertEqual("Старый протокол остался цел" in text,
                             formula == "destroy_formula")
            self.assertEqual("Сам протокол остался у Сони вместе с картой" in text,
                             formula == "keep_formula_secret")
            if disclosure == "give_sonya_copy":
                self.assertIn("Протокол у меня", text)

    def test_public_broadcast_has_its_own_recording_and_no_false_attribution(self):
        for recording, ending, last in itertools.product(
                ("record_rain", "break_phone", "record_only_agrafena"),
                ("return_consent", "open_everything", "seal_forever", "take_all", "break_chain"),
                (False, True)):
            _, text, _ = self.model.run([recording, ending, "tell_mechanism"], last=last)
            self.assertIn("Даже когда он их сказал, мы не знаем", text)
            self.assertNotIn("последней минутой его жизни", text)
            self.assertEqual("по голосу человека всё равно можно узнать" in text,
                             recording == "record_rain")
            self.assertEqual("включила новую запись и вывела звук с его микрофона" in text,
                             ending == "open_everything")
            self.assertEqual("Ты можешь удалить свою запись" in text,
                             ending == "open_everything")
            self.assertNotIn("к нашей записи", text)

    def test_endings_respect_or_explicitly_acknowledge_other_peoples_choices(self):
        for confession, witness, ending in itertools.product(
                ("promise_memory", "refuse_trade", "ask_cost"),
                ("mira_witness", "tikhon_witness", "public_witness"),
                ("return_consent", "open_everything", "seal_forever", "take_all", "break_chain")):
            _, text, _ = self.model.run([confession, witness, ending])
            finale = text.split("...: КОНЕЦ СЕРИИ")[9]
            self.assertIn("Можно оставить воду в режиме хранения и решать позже", finale)
            if ending == "return_consent":
                self.assertIn("Кто не готов, сможет решить позже", finale)
                self.assertIn("Молчание отсутствующих не считали разрешением", finale)
                self.assertIn("Невостребованное осталось в изолированном кольце", finale)
                self.assertIn("Мира передала им ключи", finale)
                self.assertNotIn("осушили контур", finale)
            if ending == "seal_forever":
                self.assertLess(finale.index("Мы могли бы подождать"),
                                finale.index("Они опустили железную плиту"))
                self.assertIn("Ты закрываешь нас не от дождя", finale)
                self.assertIn("Тихон отказался считать это общей договорённостью", finale)
            if ending == "break_chain":
                self.assertLess(finale.index("Верните мне причину ухода"),
                                finale.index("Всё-таки я отказываюсь от возврата"))
                self.assertLess(finale.index("Всё-таки я отказываюсь от возврата"),
                                finale.index("Тихон перевёл подготовленные заслонки на отвод"))
                self.assertLess(finale.index("Согласие Алексея не отменяло его просьбу"),
                                finale.index("Тихон перевёл подготовленные заслонки на отвод"))
                self.assertIn("за отнятый у вас выбор отвечать будем оба", finale)
                self.assertIn("не попросила Алексея оправдывать её", finale)

    def test_cross_product_of_physical_state_and_endings(self):
        axes = [
            ["take_tape", "leave_tape"],
            ["turn_dam_wheel", "leave_dam_closed", "ask_vera_consent"],
            ["team_together", "split_team", "trust_vera"],
            ["save_sonya", "hold_rope", "send_lesha"],
            ["open_named_streets", "open_all_streets", "disconnect_tower"],
            ["mira_witness", "tikhon_witness", "public_witness"],
            ["return_consent", "open_everything", "seal_forever", "take_all", "break_chain"],
        ]
        endings = set()
        for selected in itertools.product(*axes):
            state, text, _ = self.model.run(selected)
            self.assertTrue(state["pond_ready"])
            storage = text.index("перевели четыре узла в режим хранения")
            self.assertLess(text.index("Пруд был готов к сбросу") if "save_sonya" not in selected
                            else text.index("С ломом Веры Тихон и Алексей вернулись к пруду"), storage)
            stable = text.index("Приток был остановлен")
            self.assertLess(storage, stable)
            self.assertLess(stable, text.index("В клубе наспех собрали тех"))
            finale = text.split("...: КОНЕЦ СЕРИИ")[9]
            self.assertIn("Опасность переполнения снята", finale)
            self.assertNotIn("К рассвету коллектор переполнится", finale)
            self.assertEqual("никто не уходил собирать жителей" in text,
                             "leave_dam_closed" in selected)
            self.assertIn("Наступило утро, а уровень остался на ночной отметке", finale)
            self.assertIn("Локация: bg04-well-yard-morning", finale)
            self.assertNotIn("Локация: bg04-well-yard-predawn", finale)
            if "open_named_streets" in selected:
                self.assertIn("Теперь молчали все пять", finale)
            self.assertEqual("Третий узел был подготовлен" in text,
                             "trust_vera" not in selected)
            self.assertEqual("на какую отметку выставила заслонку насосной" in text,
                             "trust_vera" in selected)
            self.assertEqual("Вера подошла к башне со стороны колодца" in text,
                             "split_team" in selected)
            self.assertEqual("Вера догнала остальных у башни" in text,
                             "trust_vera" in selected)
            preparation = ("Доверенное ей дело было сделано" if "trust_vera" in selected
                           else "Третий узел был подготовлен")
            self.assertLess(text.index(preparation),
                            text.index("Четыре узла были подготовлены"))
            self.assertLess(text.index("пошла снять доски над колодцем"),
                            text.index("увидели Веру у снятых досок"))
            self.assertEqual("На это ушло почти полчаса" in text, "save_sonya" in selected)
            self.assertEqual("Открытый сброс у плотины понизил уровень" in text,
                             "turn_dam_wheel" in selected)
            self.assertEqual("времени останется меньше" in text,
                             "leave_dam_closed" in selected)
            self.assertNotIn("Мы выиграем время и поднимем давление", text)
            self.assertEqual("С улиц уже успели услышать часть признаний" in text,
                             "open_all_streets" in selected)
            ending = state["final_choice"]
            endings.add(ending)
            self.assertEqual(text.count("...: ФИНАЛ:"), 1)
            self.assertEqual("Дно осталось под плитой и водой" in text, ending == "seal")
            self.assertEqual("Мира не вошла в автобус" in text, ending == "seal")
            self.assertEqual("...: Солнце осветило затихший двор" in text, ending != "seal")
            self.assertEqual("Мира не смогла составить отчёт о себе" in text,
                             ending == "burden" and "mira_witness" in selected)
            if ending == "consent":
                self.assertIn("собственную память, которую соглашался отдать", text)
                self.assertEqual("Я отдам утро" in text, "tikhon_witness" in selected)
                self.assertEqual("Я отдам нашу встречу" in text,
                                 "tikhon_witness" not in selected)
            if ending == "truth":
                self.assertIn("Мира включила громкоговоритель сама", text)
                self.assertNotIn("Соня подключила запись", text)
        self.assertEqual(endings, {"consent", "truth", "seal", "burden", "release"})

    def test_all_asset_references_exist(self):
        defaults = {"Мира": ("maincharacter", "coat"), "Лёша": ("лёша", "jacket"),
                    "Тихон": ("тихон", "raincoat"), "Вера": ("вера", "workcoat"),
                    "Соня": ("соня", "raincoat")}
        for source in INK.glob("s01e*.ink"):
            for line in source.read_text().splitlines():
                if line.lstrip().startswith(("*", "~", "{", "}", "//", "VAR", "===", "->")):
                    continue
                match = re.match(r"\s*(.+?)(?: \(([^)]+)\))?: (.*)", line)
                if not match:
                    continue
                speaker, selector, content = match.groups()
                target = None
                if speaker == "Локация":
                    target = PROJECT / "Assets/Locations" / f"{content}.png"
                elif speaker in ("Музыка", "Звук"):
                    target = PROJECT / "Assets/Audio" / f"{content}.wav"
                elif speaker in defaults:
                    character, outfit = defaults[speaker]
                    target = PROJECT / "Assets/Characters" / character / "view/whole" / outfit / f"{selector or 'main'}.png"
                elif speaker != "...":
                    self.fail(f"Unknown speaker or command in {source.name}: {line}")
                if target:
                    self.assertTrue(target.is_file(), str(target))
                    self.assertTrue(Path(str(target) + ".meta").is_file(), str(target))


if __name__ == "__main__":
    unittest.main(verbosity=2)
