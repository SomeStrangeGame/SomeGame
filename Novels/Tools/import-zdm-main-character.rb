#!/usr/bin/env ruby

require "fileutils"
require "pathname"

source_root = ARGV.fetch(0) do
  abort "Usage: #{File.basename($PROGRAM_NAME)} <source-main-character-root> [project-root]"
end
project_root = File.expand_path(ARGV.fetch(1, File.join(__dir__, "..")))
source_root = File.expand_path(source_root)
target_root = File.join(
  project_root,
  "Assets/RemoteAssets/Content/ZDM/Shared/Character/Characters/MainCharacter"
)

abort "Source directory does not exist: #{source_root}" unless Dir.exist?(source_root)

def normalized(value)
  value.unicode_normalize(:nfc).strip
end

def image_files(directory)
  return [] unless Dir.exist?(directory)

  Dir.children(directory)
    .map { |name| File.join(directory, name) }
    .select { |path| File.file?(path) && File.extname(path).casecmp?(".png") }
    .sort
end

def directories(directory)
  return [] unless Dir.exist?(directory)

  Dir.children(directory)
    .map { |name| File.join(directory, name) }
    .select { |path| File.directory?(path) }
    .sort
end

def copy_asset(source, destination, copied)
  return if File.expand_path(source) == File.expand_path(destination)

  FileUtils.mkdir_p(File.dirname(destination))
  FileUtils.cp(source, destination)
  copied << Pathname.new(destination)
end

copied = []
warnings = []

emotion_aliases = {
  "лёгкая улыбка" => ["лёгкая улыбка", "легкая улыбка"],
  "слёзы" => ["слёзы", "слезы"],
  "улыбка" => ["лёгкая улыбка", "легкая улыбка"],
  "ухмылка" => ["хитрая улыбка"],
  "недоверчивость" => ["недоверие"],
  "задумчиво" => ["задумчивость"],
  "закатила глаза" => ["закатить глаза"],
  "испуг" => ["страх"],
}.freeze

directories(File.join(source_root, "Внешность".unicode_normalize(:nfd))).each do |view_dir|
  view = normalized(File.basename(view_dir)).downcase
  body = image_files(view_dir).first
  if body
    copy_asset(body, File.join(target_root, "View", view, "Main.png"), copied)
  else
    warnings << "Missing body for appearance '#{view}'"
  end

  emotions_dir = ["Новые эмоции", "Эмоции"]
    .map { |name| File.join(view_dir, name.unicode_normalize(:nfd)) }
    .find { |path| Dir.exist?(path) }
  unless emotions_dir
    warnings << "Missing emotions for appearance '#{view}'"
    next
  end

  image_files(emotions_dir).each do |source|
    emotion = normalized(File.basename(source, ".*")).tr("_", " ").downcase
    copy_asset(
      source,
      File.join(target_root, "View", view, "Emotions", "#{emotion}.png"),
      copied
    )
  end
  emotion_aliases.each do |target, candidates|
    source = candidates
      .map { |name| File.join(target_root, "View", view, "Emotions", "#{name}.png") }
      .find { |path| File.file?(path) }
    if source
      copy_asset(
        source,
        File.join(target_root, "View", view, "Emotions", "#{target}.png"),
        copied
      )
    else
      warnings << "Missing source emotion for alias '#{target}' in '#{view}'"
    end
  end
end

directories(File.join(source_root, "Одежда")).each do |clothes_dir|
  name = normalized(File.basename(clothes_dir)).downcase
  source = image_files(clothes_dir).find { |path| File.basename(path, ".*") == "1" }
  if source
    copy_asset(source, File.join(target_root, "Clothes", name, "1.png"), copied)
  else
    warnings << "Missing color 1 for clothes '#{name}'"
  end
end

hair_color_suffixes = {
  "Чёрный" => "чёрные",
  "Медный" => "медные",
  "Каштан" => "каштановые",
}.freeze
hair_layers = {
  "Назад" => "Back",
  "Вперёд" => "Front",
}.freeze
hair_root = File.join(source_root, "Причёски".unicode_normalize(:nfd))
directories(hair_root).each do |layer_dir|
  source_layer = normalized(File.basename(layer_dir))
  target_layer = hair_layers[source_layer]
  unless target_layer
    warnings << "Unknown hair layer '#{source_layer}'"
    next
  end

  directories(layer_dir).each do |style_dir|
    style = normalized(File.basename(style_dir))
    image_files(style_dir).each do |source|
      color = normalized(File.basename(source, ".*"))
      suffix = hair_color_suffixes[color]
      unless suffix
        warnings << "Unknown hair color '#{color}' for '#{style}'"
        next
      end
      choice_name = "#{style} #{suffix}".downcase
      copy_asset(
        source,
        File.join(target_root, "Hairs", target_layer, choice_name, "блонд.png"),
        copied
      )
    end
  end
end

accessory_layers = {
  "0 (За ГГ)" => "Back",
  "5 (По середине)" => "Middle",
  "7 (Перед ГГ)" => "Front",
}.freeze
accessories_root = File.join(source_root, "Аксессуары")
directories(accessories_root).each do |layer_dir|
  source_layer = normalized(File.basename(layer_dir))
  target_layer = accessory_layers[source_layer]
  unless target_layer
    warnings << "Unknown accessory layer '#{source_layer}'"
    next
  end

  directories(layer_dir).each do |accessory_dir|
    name = normalized(File.basename(accessory_dir)).downcase
    images = image_files(accessory_dir)
    source = images.find { |path| normalized(File.basename(path, ".*")).casecmp?(name) }
    source ||= images.find { |path| File.basename(path, ".*") == "1" }
    if source
      copy_asset(
        source,
        File.join(target_root, "Accessories", target_layer, "#{name}.png"),
        copied
      )
    else
      warnings << "Missing primary image for accessory '#{name}' in '#{source_layer}'"
    end
  end
end

puts "Imported #{copied.length} main-character sprites into #{target_root}."
warnings.each { |warning| warn "Warning: #{warning}" }
exit(warnings.empty? ? 0 : 2)
