require 'json'
require 'digest'

path = ARGV.fetch(0)
source = File.read(path, encoding: 'UTF-8')
episodes = (1..4).map do |number|
  match = source.match(/<!-- episode:#{number} -->\n(.*?)<!-- \/episode:#{number} -->/m)
  abort "Missing episode #{number}" unless match
  match[1]
end
abort 'Expected five choices' unless source.scan(/^#### Выбор [1-5]\./).size == 5
abort 'Unexpected placeholder' if source.match?(/\bTODO\b|\bTBD\b|Почему это работает — не могу/)

def render(source, state)
  stack = [true]
  result = []
  source.each_line do |line|
    if (condition = line.strip.match(/\A<!-- branch (\w+)=(\w+) -->\z/))
      key, value = condition.captures
      raise "Unknown state #{key}" unless state.key?(key)
      stack << (stack.last && state.fetch(key) == value)
    elsif line.strip == '<!-- /branch -->'
      raise 'Unexpected branch end' if stack.length == 1
      stack.pop
    elsif stack.last
      result << line
    end
  end
  raise 'Unclosed branch' unless stack == [true]
  result.join
end

def word_count(source)
  source.gsub(/^\#{1,6} .*$/, '').gsub(/^\*\*[^*\n]+\*\*/, '').gsub(/^Уведомление:/, '').scan(/[[:alpha:][:digit:]]+(?:[-’'][[:alpha:][:digit:]]+)*/).size
end

rows = []
%w[sale search mill].product(%w[yes no], %w[read unread], %w[shared alone], %w[road quiet keeper]).each do |intention, trust, letter, help, ending|
  dust = if help == 'alone'
    letter == 'read' ? 'other' : 'own'
  elsif letter == 'unread' && trust == 'no'
    'own'
  else
    'none'
  end
  state = { 'intention' => intention, 'trust' => trust, 'letter' => letter, 'help' => help, 'ending' => ending, 'dust' => dust, 'exposure' => dust == 'none' ? 'no' : 'yes' }
  selected = episodes.map { |episode| render(episode, state) }
  letter_fragment = 'Я снимаю слово, как у нас говорят.'
  raise 'Episode three letter-state mismatch' unless selected[2].include?(letter_fragment) == (letter == 'read')
  raise 'Late letter-state mismatch' unless selected[3].include?(letter_fragment) == (letter == 'unread' && ending == 'road')
  endings = selected[3].scan(/\*\*Конец: «([^»]+)»\.\*\*/).flatten
  expected_ending = { 'road' => 'Дорога без клятвы', 'quiet' => 'Тихая мука', 'keeper' => 'Хранительница свидетелей' }.fetch(ending)
  raise 'Ending routing mismatch' unless endings == [expected_ending]
  raise 'Foreign compulsion lost' if dust == 'other' && !selected[3].include?('Письмо Мити не могло снять чужое обещание') && ending == 'road'
  raise 'Wrong inherited compulsion' if dust != 'other' && selected[3].include?('Письмо Мити не могло снять чужое обещание')
  rows << { state: state, words: selected.map { |episode| word_count(episode) }, notifications: selected.map { |episode| episode.scan(/^Уведомление:/).size } }
end

stats = (0..3).map do |index|
  words = rows.map { |row| row[:words][index] }
  notices = rows.map { |row| row[:notifications][index] }
  { episode: index + 1, min_words: words.min, max_words: words.max, estimate_minutes_at_250_wpm_plus_1_5: [(words.min / 250.0 + 1.5).round(1), (words.max / 250.0 + 1.5).round(1)], notifications: [notices.min, notices.max] }
end
puts JSON.pretty_generate({ checked_routes: rows.length, choices: 5, endings: 3, episode_stats: stats, scope: 'Literary branch checks only; not Ink compilation or measured game duration', sha256: Digest::SHA256.hexdigest(source) })
