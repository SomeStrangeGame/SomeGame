require 'digest'

abort "usage: #{$PROGRAM_NAME} INPUT OUTPUT_DIR" unless ARGV.length == 2
input, output_dir = ARGV
source = File.read(input, encoding: 'UTF-8')

EPISODES = {
  1 => ['Дом, который ждал', 'bg01-bus-stop-dusk'],
  2 => ['За рекой', 'bg09-village-archive-night'],
  3 => ['Слова под камнями', 'bg11-mill-undercroft'],
  4 => ['Право уйти', 'bg07-wind-gallery']
}.freeze

SCENE_LOCATIONS = {
  '1.1.' => 'bg01-bus-stop-dusk', '1.2.' => 'bg02-house-kitchen-night',
  '1.3.' => 'bg09-village-archive-night',
  '2.1.' => 'bg09-village-archive-night', '2.2.' => 'bg03-forest-path-moon',
  '2.3.' => 'bg10-flooded-ford-night', '2.4.' => 'bg04-mill-yard',
  '3.1.' => 'bg11-mill-undercroft', '3.2.' => 'bg05-grinding-room',
  '3.3.' => 'bg06-sack-loft',
  '4.1.' => 'bg07-wind-gallery', '4.2.' => 'bg08-river-crossing-dawn',
  '4.3А.' => 'bg04-mill-yard', '4.3Б.' => 'bg04-mill-yard',
  '4.3В.' => 'bg05-grinding-room'
}.freeze

CHOICE_KEYS = {
  1 => ['intention', { 'sale' => 'sale', 'search' => 'search', 'mill' => 'mill' }],
  2 => ['trust', { 'yes' => true, 'no' => false }],
  3 => ['letter', { 'read' => true, 'unread' => false }],
  4 => ['help', { 'shared' => true, 'alone' => false }]
}.freeze

def condition(key, value)
  case key
  when 'intention' then "lada_intent == \"#{value}\""
  when 'trust' then value == 'yes' ? 'trust_yakov' : 'not trust_yakov'
  when 'letter' then value == 'read' ? 'letter_read' : 'not letter_read'
  when 'help' then value == 'shared' ? 'shared_burden' : 'not shared_burden'
  when 'ending' then "ending == \"#{value}\""
  when 'dust' then "dust_kind == \"#{value}\""
  when 'exposure' then value == 'yes' ? 'dust_kind != ""' : 'dust_kind == ""'
  else raise "unknown condition #{key}=#{value}"
  end
end

def choice_assignment(choice, key, value)
  case choice
  when 1 then "~ lada_intent = \"#{value}\""
  when 2
    ["~ trust_yakov = #{value}", (value ? nil : '~ dust_kind = "own"')].compact.join("\n    ")
  when 3
    if value
      "~ letter_read = true\n    ~ dust_kind = \"\""
    else
      '~ letter_read = false'
    end
  when 4
    if value
      '~ shared_burden = true'
    else
      "~ shared_burden = false\n    { letter_read:\n        ~ dust_kind = \"other\"\n    - else:\n        ~ dust_kind = \"own\"\n    }"
    end
  else raise "unknown choice #{choice}"
  end
end

def prose_lines(paragraph)
  text = paragraph.lines.map(&:strip).join(' ').gsub(/\s+/, ' ').strip
  return [] if text.empty?
  return [text] if text.match?(/\A(?:Уведомление|Локация|Музыка|Звук):/)
  text.scan(/.{1,260}(?:[.!?](?:[\"»”])?(?=\s|$)|$)/).map do |chunk|
    chunk = chunk.strip
    chunk.empty? ? nil : "...: #{chunk}"
  end.compact
end

Dir.mkdir(output_dir) unless Dir.exist?(output_dir)

(1..4).each do |episode|
  match = source.match(/<!-- episode:#{episode} -->\n(.*?)<!-- \/episode:#{episode} -->/m)
  raise "missing episode #{episode}" unless match
  lines = match[1].lines
  out = []
  if episode == 1
    out.concat([
      '// Persistent story state shared by all four episodes.',
      'VAR lada_intent = ""', 'VAR trust_yakov = false',
      'VAR letter_read = false', 'VAR shared_burden = false',
      'VAR dust_kind = ""', 'VAR ending = ""', '', '-> CHMs01e01', ''
    ])
  end
  out << "// Эпизод #{episode}. #{EPISODES.fetch(episode).first}"
  out << "=== CHMs01e0#{episode} ==="
  out << "Локация: #{EPISODES.fetch(episode).last}"
  out << (episode == 4 ? 'Музыка: dawn-release-loop' : 'Музыка: mill-pressure-loop')

  choice = nil
  top_choice_branch = false
  branch_stack = []
  paragraph = []
  skip_choice_five_prelude = false

  flush = lambda do
    prose_lines(paragraph.join).each { |line| out << ('    ' * branch_stack.length) + line }
    paragraph.clear
  end

  lines.each do |line|
    stripped = line.strip
    if (choice_heading = stripped.match(/\A#### Выбор ([1-5])\./))
      flush.call
      choice = choice_heading[1].to_i
      if choice == 5
        out.concat([
          '* (truth_and_road) [Рассказать жителям, вернуть письма и разобрать подачу]',
          '    ~ ending = "road"',
          '* (quiet_delay) [Остановить мельницу, а раскрытие писем отложить]',
          '    ~ ending = "quiet"',
          '* (keeper) [Взять книгу и письма под свою ответственность]',
          '    ~ ending = "keeper"', '-'
        ])
        skip_choice_five_prelude = true
      end
      next
    end

    if skip_choice_five_prelude
      if stripped == '<!-- branch ending=road -->'
        skip_choice_five_prelude = false
      else
        next
      end
    end

    if (heading = stripped.match(/\A### ([1-4]\.\d(?:[А-Я])?\.)/))
      flush.call
      location = SCENE_LOCATIONS[heading[1]]
      out << "Локация: #{location}" if location
      next
    end

    if stripped == '#### Общий ход'
      flush.call
      out << '-' if choice && choice < 5
      choice = nil
      next
    end

    if (opening = stripped.match(/\A<!-- branch (\w+)=(\w+) -->\z/))
      flush.call
      key, value = opening.captures
      if choice && choice < 5 && branch_stack.empty?
        top_choice_branch = true
        branch_stack << :choice
        out << "* (choice_#{choice}_#{value}) [__LABEL_#{choice}_#{value}__]"
        out << "    #{choice_assignment(choice, key, CHOICE_KEYS.fetch(choice)[1].fetch(value))}"
      else
        branch_stack << :condition
        out << ('    ' * (branch_stack.length - 1)) + "{#{condition(key, value)}:"
      end
      next
    end

    if stripped == '<!-- /branch -->'
      flush.call
      kind = branch_stack.pop or raise "unexpected branch close in episode #{episode}"
      if kind == :condition
        out << ('    ' * branch_stack.length) + '}'
      else
        top_choice_branch = false
      end
      next
    end

    if (bold_label = stripped.match(/\A\*\*(.+)\.\*\*\z/)) && top_choice_branch
      label = bold_label[1]
      token = out.rindex { |item| item.include?("__LABEL_#{choice}_") }
      out[token] = out[token].sub(/__LABEL_#{choice}_[^_]+__/, label) if token
      next
    end

    if stripped.empty?
      flush.call
      next
    end
    if (inline_note = stripped.match(/\A\*\*[^*]+:\*\*\s+(.+)\z/))
      paragraph << inline_note[1] + "\n"
      next
    end
    next if stripped.start_with?('#', '---')
    next if stripped.match?(/\A\*\*Конец эпизода \d+\.\*\*\z/)
    next if stripped.match?(/\A\*\*(?:Общий ход|Если|Продолжение|Конец:)/)
    next if stripped.start_with?('<!--')
    paragraph << line
  end
  flush.call
  raise "unclosed branch in episode #{episode}" unless branch_stack.empty?
  out << '...: КОНЕЦ СЕРИИ'
  out << (episode < 4 ? "-> CHMs01e0#{episode + 1}" : '-> END')
  path = File.join(output_dir, "s01e0#{episode}.ink")
  File.write(path, out.join("\n") + "\n")
end

File.write(File.join(output_dir, 'chernaya-melnitsa.ink'), <<~INK)
  // Чёрная мельница — четыре эпизода.
  INCLUDE s01e01.ink
  INCLUDE s01e02.ink
  INCLUDE s01e03.ink
  INCLUDE s01e04.ink
INK

puts Dir.glob(File.join(output_dir, '*.ink')).sort.map { |path| "#{File.basename(path)} #{Digest::SHA256.file(path).hexdigest}" }
