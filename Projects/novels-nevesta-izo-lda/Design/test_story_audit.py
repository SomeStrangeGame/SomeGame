"""Regression checks for the read-only story audit; no fixture writes/Unity."""

from contextlib import redirect_stdout
from io import StringIO
from pathlib import Path
import runpy
import unittest
from unittest.mock import patch


AUDIT = Path(__file__).with_name('audit_story.py')
ROOT = AUDIT.parents[1]
SOURCE = ROOT / 'Assets/Ink/s01e01.ink'
DEFINITION = ROOT / 'Assets/nevesta-izo-lda.asset'
READ_TEXT = Path.read_text


def run_audit(target=None, before=None, after=None, return_namespace=False):
    def read(path, *args, **kwargs):
        value = READ_TEXT(path, *args, **kwargs)
        if path == target:
            assert before in value, 'Test fixture no longer matches source'
            return value.replace(before, after, 1)
        return value

    with patch.object(Path, 'read_text', read), redirect_stdout(StringIO()) as output:
        namespace = runpy.run_path(str(AUDIT))
    return namespace if return_namespace else output.getvalue()


class StoryAuditTests(unittest.TestCase):
    def test_current_candidate(self):
        self.assertIn('"assetsResolved": 45', run_audit())

    def test_rejects_source_regressions(self):
        cases = [
            ('-> ferry_house', '-> absent_knot', 'Undefined divert'),
            ('(call_witnesses)', '(take_hand)', 'Duplicate choice label'),
            ('Локация: glass-mere-dusk', 'Локация: absent_location', 'Missing runtime address'),
            ('Музыка: hearth-under-ice', 'Музыка: absent_audio', 'Missing/ambiguous audio'),
            ('trust_leda += 1', 'trust_leda += 2', 'State contract drift'),
            ('{ trust_leda >= 2 && ribbon_free }',
             '{ trust_leda >= 1 && ribbon_free }', 'Final menu contract drift'),
            ('{ trust_leda >= 2 && ribbon_free }',
             '{ trust_leda >= 2 && truth_osip && ribbon_free }', 'Final menu contract drift'),
            ('Мира (winter, main):', 'Мира (winter, main, unknown):', None),
            ('...: КОНЕЦ СЕРИИ', '...: Конец', 'Missing or duplicate episode end marker'),
        ]
        for before, after, message in cases:
            with self.subTest(change=after):
                with self.assertRaises(AssertionError) as error:
                    run_audit(SOURCE, before, after)
                if message:
                    self.assertIn(message, str(error.exception))

    def test_requires_protagonist_alias(self):
        with self.assertRaisesRegex(AssertionError, 'Missing runtime address'):
            run_audit(DEFINITION,
                      '_alias: story/character/characters/maincharacter/view/whole/winter/main.png',
                      '_alias: story/character/characters/unused/view/whole/winter/main.png')

    def test_narrative_continuity_on_every_route(self):
        for route in run_audit(return_namespace=True)['RESULTS']:
            with self.subTest(choices=route['choices']):
                rendered = '\n'.join(route['lines'])
                self.assertIn('Это мой детский почерк.', rendered)
                self.assertIn('Пока это только мечта о доме.', rendered)
                self.assertIn('Если потеряем управление', rendered)
                self.assertIn('условие Леды услышали заранее и теперь выполнили', rendered)
                self.assertNotIn('Нет. Спросите меня!', rendered)
                confessed = route['state']['truth_osip']
                self.assertEqual('повторил признание в седьмую трубу' in rendered, confessed)
                self.assertEqual('И любовь — правом решать за любимого.' in rendered, confessed)
                savva_route = route['choices'][3] == 'follow_savva'
                self.assertEqual('знакомый путевой сигнал услышали' in rendered, savva_route)
                if route['state']['promise_shared']:
                    leda_agrees = route['state']['trust_leda'] >= 2
                    self.assertEqual('Я согласна. Буду хранить' in rendered, leda_agrees)
                    self.assertTrue(leda_agrees)
                    self.assertIn('Остальные решат за себя.', rendered)
                if route['ending'] == 'ending_thaw':
                    self.assertLess(rendered.index('трещина в бронзе сомкнулась'),
                                    rendered.index('колокол ударил чисто'))
                    self.assertLess(rendered.index('Все пятеро вышли по ним к пристани'),
                                    rendered.index('Я думала, утро будет белым.'))
                    self.assertIn('Зима только началась', rendered)
                    self.assertIn('За будущий отказ я не взыщу', rendered)
                if route['ending'] == 'ending_lantern':
                    self.assertNotIn('увидела Леду в последний раз', rendered)
                    self.assertIn('Меня зовут Леда. Ты спасла меня.', rendered)

    def test_dialogue_and_stage_direction_continuity(self):
        for route in run_audit(return_namespace=True)['RESULTS']:
            with self.subTest(choices=route['choices']):
                lines = route['lines']
                question = next(i for i, line in enumerate(lines)
                                if 'Почему письмо осталось здесь?' in line)
                reply = lines[question + 1]
                self.assertTrue(reply.startswith('Савва (courier, main):'))
                self.assertIn('поручение: отдать тебе конверт', reply)
                self.assertIn('когда снова зазвонит остров', reply)
                order = next(i for i, line in enumerate(lines)
                             if 'Савва, третий штифт! Аграфена, называй отметки!' in line)
                self.assertIn('Савва вогнал запасной штифт', lines[order + 1])
                self.assertTrue(lines[order + 2].startswith('Аграфена (keeper, commanding):'))
                if route['ending'] == 'ending_lantern':
                    question = next(i for i, line in enumerate(lines)
                                    if 'Тогда почему я не помню тебя?' in line)
                    self.assertIn('Озеро взяло твою память об этой ночи.', lines[question + 1])
                    self.assertFalse(any('Я не помню обещания.' in line for line in lines))

    def test_autonomy_and_common_deadline(self):
        data = run_audit(return_namespace=True)
        refusals = [r for r in data['RESULTS']
                    if r['ending'] == 'ending_thaw' and not r['state']['truth_osip']]
        self.assertEqual(len(refusals), 3, 'Ledger-first must retain three thaw routes')
        for route in data['RESULTS']:
            with self.subTest(choices=route['choices']):
                lines = route['lines']
                text = '\n'.join(lines)
                self.assertIn('Имя исключено из согласившихся.', text)
                self.assertIn('права отказаться мне не нужно ждать от него.', text)
                self.assertNotIn('Показать подлог оказалось недостаточно, чтобы отозвать его.', text)
                self.assertNotIn('Первое солнце коснулось льда над ними.', text)
                seal = [i for i, line in enumerate(lines) if 'Они дождались первого света.' in line]
                self.assertEqual(len(seal), 1)
                preparation = next(i for i, line in enumerate(lines) if 'Строку о своём окончательном решении' in line)
                self.assertLess(preparation, seal[0])
                ending_location = {'ending_thaw': 'Локация: thaw-sunrise',
                                   'ending_glass': 'Локация: sealed-dawn',
                                   'ending_lantern': 'Локация: sealed-dawn'}[route['ending']]
                self.assertLess(seal[0], lines.index(ending_location))
                if route['ending'] == 'ending_glass':
                    self.assertEqual(route['choices'][-1], 'return_leda')
                if route['ending'] == 'ending_thaw' and not route['state']['truth_osip']:
                    self.assertIn('Когда будешь готов отвечать за свой поступок.', text)
                    self.assertNotIn('И обещаю больше не подменять', text)
                if route['ending'] == 'ending_lantern':
                    written = next(i for i, line in enumerate(lines) if 'Мира дописала в подготовленном письме' in line)
                    forgotten = next(i for i, line in enumerate(lines) if 'Ночь вышла из Миры' in line)
                    self.assertLess(written, seal[0])
                    self.assertLess(seal[0], forgotten)

    def test_notifications_follow_evidence_and_stay_branch_local(self):
        for route in run_audit(return_namespace=True)['RESULTS']:
            with self.subTest(choices=route['choices']):
                lines = route['lines']
                text = '\n'.join(lines)
                observation = next(i for i, line in enumerate(lines)
                                   if 'внутри девушки жили чужие воспоминания' in line)
                confirmation = next(i for i, line in enumerate(lines)
                                    if 'Подтверждено: Леда несёт чужие воспоминания' in line)
                self.assertLess(observation, confirmation)
                self.assertEqual('Подлог признан.' in text,
                                 route['state']['truth_osip'])
                self.assertEqual('Подлог доказан.' in text,
                                 not route['state']['truth_osip'])
                ending_notice = {
                    'ending_thaw': 'Договор изменён.',
                    'ending_glass': 'Прежний договор продлён',
                    'ending_lantern': 'Леда свободна.',
                }[route['ending']]
                self.assertIn(ending_notice, text)
                for other in ({'Договор изменён.', 'Прежний договор продлён',
                               'Леда свободна.'} - {ending_notice}):
                    self.assertNotIn(other, text)


if __name__ == '__main__':
    unittest.main()
