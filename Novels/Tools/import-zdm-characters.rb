#!/usr/bin/env ruby

require "fileutils"

SOURCE_CHARACTERS = {
  "Бастет" => { source: "Бастет", body: "Бастет.PNG", aliases: ["Женщина"] },
  "Захра" => { source: "Захра", body: "Захра.png" },
  "Джосер" => { source: "Джосер ", body: "Джосер.png" },
  "Старшая жрица" => { source: "Старшая жрица", body: "Старшая жрица.png" },
  "Гор" => {
    source: "Гор (Анпу)",
    body: "Основной.png",
    aliases: ["Анпу", "Незнакомец"],
  },
  "Жрица" => { source: "Жрица", body: "Жрица.png" },
  "Вторая жрица" => { source: "Вторая жрица", body: "Вторая жрица.png" },
  "Фараон" => { source: "Фараон ", body: "Фараон.png" },
  "Верховная жрица" => {
    source: "Верховная жрица",
    body: "Верховная_жрица.png",
  },
  "Воин" => { source: "Воин", body: "Воин.png" },
  "Второй воин" => { source: "Второй воин", body: "Второй воин.png" },
  "Контар" => { source: "Контар", body: "Контар.png" },
  "Куибила" => { source: "Куибила", body: "Куибила.png" },
  "Тёмная фигура" => { source: "Тёмная фигура", body: "Тёмная_фигура.PNG" },
  "Анпу" => { source: "Гор (Анпу)", body: "Основной.png" },
  "Сет" => { source: "Сет", body: "Сет.PNG" },
  "Стражник" => { source: "Стражник", body: "Стражник.png" },
  "Второй стражник" => {
    source: "Второй стражник",
    body: "Второй стражник.png",
  },
  "Кисса" => { source: "Кисса", body: "Кисса.png" },
  "Горожанка" => { source: "Горожанка", body: "Горожанка.png" },
  "Рамос" => { source: "Рамос", body: "Рамос.png" },
  "Стражники" => { source: "Стражник", body: "Стражник.png" },
}.freeze

EMOTION_ALIASES = {
  "Захра" => {
    "закатила глаза" => "недовольство",
    "лёгкая улыбка" => "радость",
    "счастье" => "радость",
  },
  "Джосер" => {
    "лёгкая улыбка" => "легкая улыбка",
    "недоверчивость" => "недоверие",
    "раздражение" => "недовольство",
  },
  "Гор" => {
    "закатил глаза" => "закатить глаза",
    "раздражение" => "недовольство",
    "хитрая ухмылка" => "хитрая улыбка",
    "сияющие глаза" => "светящиеся глаза",
  },
  "Анпу" => {
    "закатил глаза" => "закатить глаза",
    "раздражение" => "недовольство",
    "хитрая ухмылка" => "хитрая улыбка",
    "сияющие глаза" => "светящиеся глаза",
  },
  "Куибила" => {
    "недовольно" => "недовольство",
    "недоверчивость" => "недоверие",
  },
  "Воин" => { "недовольство" => "злость" },
  "Тёмная фигура" => {
    "зловещая улыбка" => "злобный оскал",
    "зловещий смех" => "злобный смех",
    "открытая пасть" => "злобный оскал",
    "ужасное лицо" => "злобный оскал",
  },
  "Рамос" => {
    "размышление" => "задумчивость",
    "закатить глаза" => "закатил глаза",
  },
  "Горожанка" => { "недовольство" => "злость" },
}.freeze

BODY_ALIASES = {
  "Контар" => { "накидка" => "в накидке" },
}.freeze

source_root = File.expand_path(ARGV.fetch(0) do
  abort "Usage: #{File.basename($PROGRAM_NAME)} <source-characters-root> [project-root]"
end)
project_root = File.expand_path(ARGV.fetch(1, File.join(__dir__, "..")))
target_root = File.join(
  project_root,
  "Assets/RemoteAssets/content/zdm/shared/character/characters"
)

abort "Source directory does not exist: #{source_root}" unless Dir.exist?(source_root)

def normalized(value)
  value.unicode_normalize(:nfc).strip
end

def source_entry(directory, expected)
  Dir.children(directory)
    .map { |name| [name, File.join(directory, name)] }
    .find { |name, _| normalized(name) == normalized(expected) }
    &.last
end

def copy(source, target, copied)
  abort "Source asset does not exist: #{source}" unless File.file?(source)
  FileUtils.mkdir_p(File.dirname(target))
  FileUtils.cp(source, target)
  copied << target
end

copied = []
SOURCE_CHARACTERS.each do |target_name, definition|
  source_character = source_entry(source_root, definition.fetch(:source))
  abort "Source character does not exist: #{definition.fetch(:source)}" unless source_character

  body = source_entry(source_character, definition.fetch(:body))
  abort "Character body does not exist: #{definition.fetch(:body)}" unless body
  view_root = File.join(target_root, normalized(target_name).downcase, "view")
  copy(body, File.join(view_root, "main.png"), copied)
  Array(definition[:aliases]).each do |name|
    copy(body, File.join(view_root, "#{name}.png"), copied)
  end

  Dir.children(source_character).each do |name|
    source = File.join(source_character, name)
    next unless File.file?(source) && File.extname(source).casecmp?(".png")
    next if source == body

    candidate = normalized(File.basename(source, ".*"))
      .sub(/^#{Regexp.escape(target_name)}[_ ]*/i, "")
      .tr("_", " ")
      .downcase
    copy(source, File.join(view_root, "#{candidate}.png"), copied)
  end
  BODY_ALIASES.fetch(target_name, {}).each do |target, source_name|
    source = File.join(view_root, "#{source_name}.png")
    copy(source, File.join(view_root, "#{target}.png"), copied)
  end

  emotions = source_entry(source_character, "Эмоции")
  next unless emotions && Dir.exist?(emotions)
  Dir.children(emotions).sort.each do |name|
    source = File.join(emotions, name)
    next unless File.file?(source) && File.extname(source).casecmp?(".png")

    emotion = normalized(File.basename(source, ".*"))
      .tr("_", " ")
      .downcase
    copy(source, File.join(view_root, "emotions", "#{emotion}.png"), copied)
  end
  EMOTION_ALIASES.fetch(target_name, {}).each do |target, source_name|
    source = File.join(view_root, "emotions", "#{source_name}.png")
    copy(source, File.join(view_root, "emotions", "#{target}.png"), copied)
  end
end

puts "Imported #{copied.length} shared ZDM character sprites into #{target_root}."
