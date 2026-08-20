#!/usr/bin/env ruby

require "digest"
require "fileutils"
require "pathname"
require "rbconfig"
require "securerandom"

CONTENT_ID = "tzm"
EPISODES = (1..7).map { |number| format("s01e%02d", number) }.freeze
SOURCE_VIEW_NAMES = {
  "Латиноамериканская" => "Latin",
  "Азиатская" => "Asia",
  "Европейская" => "Euro",
  "Афроамериканская" => "Afro",
}.freeze
CHARACTER_NAMES = {
  "Атлан ll" => "Атлан II",
}.freeze
IGNORED_SOURCE_SEGMENTS = ["На проверку", "Locations", "Cut-Scenes"].freeze
BACKGROUND_PATTERN =
  /^\s*(Локация|Кат-сцена|location|cut-scene)(?:\s*\([^)]*\))?\s*:\s*(.+?)\s*$/i
BUILT_IN_BACKGROUNDS = ["Темнота", "Чёрный экран"].freeze

source_root = File.expand_path(ARGV.fetch(0) do
  abort "Usage: #{File.basename($PROGRAM_NAME)} <tzm-source-root> [project-root]"
end)
project_root = File.expand_path(ARGV.fetch(1, File.join(__dir__, "..")))
visual_root = File.join(source_root, "Визуал ТЗМ")
ink_root = File.join(source_root, "ink")
content_root = File.join(project_root, "Assets/RemoteAssets/content", CONTENT_ID)
story_root = File.join(project_root, "Assets/StreamingAssets/noveltexts", CONTENT_ID)
video_root = File.join(project_root, "Assets/StreamingAssets/novelsvideos", CONTENT_ID)

abort "TZM source does not exist: #{source_root}" unless Dir.exist?(source_root)
abort "TZM visual source does not exist: #{visual_root}" unless Dir.exist?(visual_root)
abort "TZM Ink source does not exist: #{ink_root}" unless Dir.exist?(ink_root)
abort "Target content already exists: #{content_root}" if File.exist?(content_root)
abort "Target story already exists: #{story_root}" if File.exist?(story_root)

def normalized(value)
  value.to_s.unicode_normalize(:nfc).strip
end

def key(value)
  normalized(value).downcase
end

def image?(path)
  File.file?(path) && File.extname(path).casecmp?(".png")
end

def image_files(path)
  return [] unless path && Dir.exist?(path)

  Dir.glob(File.join(path, "**", "*"), File::FNM_DOTMATCH)
    .select { |entry| image?(entry) }
    .sort
end

def direct_images(path)
  return [] unless path && Dir.exist?(path)

  Dir.children(path)
    .map { |name| File.join(path, name) }
    .select { |entry| image?(entry) }
    .sort
end

def directories(path)
  return [] unless path && Dir.exist?(path)

  Dir.children(path)
    .map { |name| File.join(path, name) }
    .select { |entry| File.directory?(entry) }
    .sort
end

def copy_file(source, target, copied)
  FileUtils.mkdir_p(File.dirname(target))
  FileUtils.cp(source, target)
  copied << target
end

def unique_guid
  SecureRandom.hex(16)
end

def guid_from_meta(path)
  return nil unless File.file?(path)

  File.foreach(path) do |line|
    match = line.match(/^guid:\s*([0-9a-f]{32})/)
    return match[1] if match
  end
  nil
end

def rewrite_guid(meta_text, guid)
  meta_text.sub(/^guid:\s*[0-9a-f]{32}/, "guid: #{guid}")
end

def folder_meta(bundle_name = nil)
  <<~META
    fileFormatVersion: 2
    guid: #{unique_guid}
    folderAsset: yes
    DefaultImporter:
      externalObjects: {}
      userData:
      assetBundleName: #{bundle_name}
      assetBundleVariant:
  META
end

def write_text(path, text)
  FileUtils.mkdir_p(File.dirname(path))
  File.write(path, text)
end

def ensure_folder_metas(root, bundle_names = {})
  ([root] + Dir.glob(File.join(root, "**", "*/"))).each do |directory|
    directory = directory.delete_suffix("/")
    meta = "#{directory}.meta"
    next if File.exist?(meta)

    relative = Pathname.new(directory).relative_path_from(Pathname.new(root)).to_s
    relative = "." if relative.empty?
    write_text(meta, folder_meta(bundle_names[relative]))
  end
end

def clone_unity_tree(source, target)
  guid_map = {}
  Dir.glob(File.join(source, "**", "*.meta")).sort.each do |source_meta|
    source_guid = guid_from_meta(source_meta)
    guid_map[source_guid] = unique_guid if source_guid
  end

  Dir.glob(File.join(source, "**", "*"), File::FNM_DOTMATCH).sort.each do |entry|
    next if [".", ".."].include?(File.basename(entry))

    relative = Pathname.new(entry).relative_path_from(Pathname.new(source)).to_s
    destination = File.join(target, relative)
    if File.directory?(entry)
      FileUtils.mkdir_p(destination)
    elsif entry.end_with?(".meta")
      source_guid = guid_from_meta(entry)
      text = File.read(entry)
      text = rewrite_guid(text, guid_map.fetch(source_guid)) if source_guid
      write_text(destination, text)
    else
      FileUtils.mkdir_p(File.dirname(destination))
      FileUtils.cp(entry, destination)
    end
  end

  Dir.glob(File.join(target, "**", "*.{prefab,asset,mat}")).each do |path|
    text = File.read(path)
    guid_map.each { |old_guid, new_guid| text = text.gsub(old_guid, new_guid) }
    File.write(path, text)
  end
end

copied = []
warnings = []

# Ink is copied verbatim. Displayed text and authored names remain untouched.
copy_file(File.join(ink_root, "TZM.ink"), File.join(story_root, "TZM.ink"), copied)
copy_file(
  File.join(ink_root, "TZM.ink.json"),
  File.join(story_root, "TZM.ink.json"),
  copied
)
EPISODES.each do |episode|
  copy_file(
    File.join(ink_root, "#{episode}.ink"),
    File.join(story_root, "#{episode}.ink"),
    copied
  )
end

# Reuse only presentation implementation, not TZM_1 story identity or content.
legacy_episode = File.join(
  project_root,
  "Assets/RemoteAssets/content/tzm_1/episodes/s01e01"
)
presentation_root = File.join(content_root, "shared/presentation")
%w[Loading Notification Character Location Bubble].each do |feature|
  clone_unity_tree(
    File.join(legacy_episode, feature),
    File.join(presentation_root, feature)
  )
end
clone_unity_tree(
  File.join(project_root, "Assets/RemoteAssets/content/tzm_1/application"),
  File.join(content_root, "application")
)

# Main character.
main_source = File.join(visual_root, "Персонажи", "ГГ")
main_target = File.join(content_root, "shared/character/characters/maincharacter")
appearance_root = File.join(main_source, "Внешность")
SOURCE_VIEW_NAMES.each do |source_name, target_name|
  source_view = directories(appearance_root)
    .find { |path| key(File.basename(path)) == key(source_name) }
  unless source_view
    warnings << "Missing main-character view: #{source_name}"
    next
  end
  body = direct_images(source_view).first
  if body
    copy_file(body, File.join(main_target, "view", key(target_name), "main.png"), copied)
  else
    warnings << "Missing main-character body: #{source_name}"
  end
  emotions = directories(source_view)
    .find { |path| key(File.basename(path)) == key("Эмоции") }
  image_files(emotions).each do |source|
    name = key(File.basename(source, ".*"))
    copy_file(
      source,
      File.join(main_target, "view", key(target_name), "emotions", "#{name}.png"),
      copied
    )
  end

  child_root = directories(appearance_root)
    .find { |path| key(File.basename(path)) == key("Ребёнок") }
  child_view = child_root && directories(child_root)
    .find { |path| key(File.basename(path)) == key(source_name) }
  next unless child_view

  child_body = direct_images(child_view).first
  copy_file(
    child_body,
    File.join(main_target, "view", key(target_name), "child", "main.png"),
    copied
  ) if child_body
  child_emotions = directories(child_view)
    .find { |path| key(File.basename(path)) == key("Эмоции") }
  image_files(child_emotions).each do |source|
    name = key(File.basename(source, ".*"))
    copy_file(
      source,
      File.join(main_target, "view", key(target_name), "child", "emotions", "#{name}.png"),
      copied
    )
  end
end

clothes_root = File.join(main_source, "Одежда")
directories(clothes_root).each do |environment|
  directories(environment).each do |clothes|
    source = image_files(clothes).find { |path| File.basename(path, ".*") == "1" }
    next unless source

    name = key(File.basename(clothes))
    copy_file(source, File.join(main_target, "clothes", name, "1.png"), copied)
  end
end

hair_root = directories(main_source)
  .find { |path| key(File.basename(path)) == key("Причёски") }
directories(hair_root).each do |environment|
  directories(environment).each do |layer|
    layer_name = key(File.basename(layer)).include?("назад") ? "back" : "front"
    directories(layer).each do |style|
      source = direct_images(style)
        .find { |path| key(File.basename(path, ".*")) == key("Блонд") }
      next unless source

      copy_file(
        source,
        File.join(
          main_target,
          "hairs",
          layer_name,
          key(File.basename(style)),
          "блонд.png"
        ),
        copied
      )
    end
  end
end

accessory_root = File.join(main_source, "Аксессуары")
directories(accessory_root).each do |layer|
  layer_key = key(File.basename(layer))
  target_layer = if layer_key.start_with?("0")
                   "back"
                 elsif layer_key.start_with?("5")
                   "middle"
                 else
                   "front"
                 end
  direct_images(layer).each do |source|
    name = key(File.basename(source, ".*"))
    copy_file(source, File.join(main_target, "accessories", target_layer, "#{name}.png"), copied)
  end
  directories(layer).each do |accessory|
    source = image_files(accessory).find { |path| File.basename(path, ".*") == "1" }
    source ||= image_files(accessory).first
    next unless source

    name = key(File.basename(accessory))
    copy_file(source, File.join(main_target, "accessories", target_layer, "#{name}.png"), copied)
  end
end

# Supporting characters. Source identities are retained unless a known typo in
# the supplied folder name would violate the exact Ink-to-content contract.
characters_root = File.join(visual_root, "Персонажи")
directories(characters_root).each do |character_source|
  source_name = normalized(File.basename(character_source))
  next if source_name == "ГГ"

  character_name = CHARACTER_NAMES.fetch(source_name, source_name)
  target = File.join(content_root, "shared/character/characters", key(character_name))
  all = image_files(character_source)
  body_candidates = all.reject do |path|
    path.split(File::SEPARATOR).any? do |segment|
      ["Эмоции", "Одежда", "Аксессуары"].include?(normalized(segment))
    end
  end
  body = body_candidates.find do |path|
    key(File.basename(path, ".*")) == key(character_name) &&
      path.split(File::SEPARATOR).any? { |segment| key(segment) == key("Взрослая") }
  end
  body ||= body_candidates.find do |path|
    key(File.basename(path, ".*")) == key(character_name) ||
      key(File.basename(path, ".*")) == key(source_name)
  end
  body ||= body_candidates.first
  if body
    copy_file(body, File.join(target, "view", "main.png"), copied)
  else
    warnings << "Missing body candidate for character: #{character_name}"
  end

  all.each do |source|
    relative_segments = Pathname.new(source)
      .relative_path_from(Pathname.new(character_source)).each_filename.to_a
    basename = normalized(File.basename(source, ".*"))
    if relative_segments.any? { |segment| key(segment) == key("Эмоции") }
      copy_file(
        source,
        File.join(target, "view", "emotions", "#{key(basename)}.png"),
        copied
      )
    elsif relative_segments.any? { |segment| key(segment) == key("Одежда") }
      copy_file(
        source,
        File.join(target, "clothes", key(basename), "1.png"),
        copied
      )
    elsif source != body
      copy_file(source, File.join(target, "view", "#{key(basename)}.png"), copied)
    end
  end

  directories(character_source).each do |variant|
    next unless ["Подросток", "Ребёнок"].include?(normalized(File.basename(variant)))

    variant_body = image_files(variant).find do |path|
      !path.split(File::SEPARATOR).any? { |segment| key(segment) == key("Эмоции") }
    end
    next unless variant_body

    copy_file(
      variant_body,
      File.join(target, "view", "#{key(File.basename(variant))}.png"),
      copied
    )
  end
end

# Episode-scoped backgrounds are selected only when Ink references them.
background_sources = image_files(File.join(visual_root, "Локации")) +
  image_files(File.join(visual_root, "Кат-сцены"))
background_index = background_sources.group_by { |path| key(File.basename(path, ".*")) }
EPISODES.each do |episode|
  story = File.join(ink_root, "#{episode}.ink")
  references = {}
  File.foreach(story) do |line|
    command = line.match(BACKGROUND_PATTERN)
    next unless command

    name = normalized(command[2].split("//", 2).first)
    next if name.empty? || name.start_with?("{")
    next if BUILT_IN_BACKGROUNDS.any? { |candidate| key(candidate) == key(name) }

    references[name] ||= command[1].downcase.include?("кат") ||
      command[1].downcase.include?("cut")
  end
  references.each do |name, cut_scene|
    candidates = background_index.fetch(key(name), [])
    source = if cut_scene
               candidates.find { |path| path.include?("Европ") } ||
                 candidates.find { |path| path.include?("Без ГГ") } || candidates.first
             else
               candidates.find { |path| path.include?("Статич") } || candidates.first
             end
    unless source
      warnings << "Missing background #{episode}: #{name}"
      next
    end
    copy_file(
      source,
      File.join(content_root, "episodes", episode, "location", "locations", "#{key(name)}.png"),
      copied
    )
  end
end

# Videos remain external release payloads, as in the existing content pipeline.
Dir.glob(File.join(visual_root, "**", "*.mp4")).sort.each do |source|
  copy_file(
    source,
    File.join(video_root, "#{key(File.basename(source, ".*"))}.mp4"),
    copied
  )
end

# Authoring definition and bundle ownership boundaries.
definition = <<~ASSET
  %YAML 1.1
  %TAG !u! tag:unity3d.com,2011:
  --- !u!114 &11400000
  MonoBehaviour:
    m_ObjectHideFlags: 0
    m_CorrespondingSourceObject: {fileID: 0}
    m_PrefabInstance: {fileID: 0}
    m_PrefabAsset: {fileID: 0}
    m_GameObject: {fileID: 0}
    m_Enabled: 1
    m_EditorHideFlags: 0
    m_Script: {fileID: 11500000, guid: 9ef49ee7dc364b55bd46cdf47f47db65, type: 3}
    m_Name: tzm
    m_EditorClassIdentifier:
    _id: tzm
    _mainCharacter: Салли
    _audioMixer: {fileID: 24100000, guid: 204ee88a2e2cb4a8088452efc54aa66c, type: 2}
    _episodes:
ASSET
EPISODES.each_with_index do |episode, index|
  entry = <<~ENTRY
    - _id: #{episode}
      _title: Сезон 1, эпизод #{index + 1}
      _storyPath: TZM.ink.json
      _contentVersion: 1
      _endMarker: #{index == EPISODES.length - 1 ? "" : "КОНЕЦ СЕРИИ"}
      _sourcePath: #{episode}.ink
      _silentAudioIds: [тишина]
  ENTRY
  definition << entry.lines.map { |line| "  #{line}" }.join
end
definition_path = File.join(content_root, "definition", "tzm.asset")
write_text(definition_path, definition)
write_text(
  "#{definition_path}.meta",
  <<~META
    fileFormatVersion: 2
    guid: #{unique_guid}
    NativeFormatImporter:
      externalObjects: {}
      mainObjectFileID: 11400000
      userData:
      assetBundleName:
      assetBundleVariant:
  META
)

bundle_names = { "." => "novels_content_tzm" }
EPISODES.each do |episode|
  bundle_names[File.join("episodes", episode)] = "novels_episode_tzm_#{episode}"
end
ensure_folder_metas(content_root, bundle_names)
ensure_folder_metas(story_root)
ensure_folder_metas(video_root)

puts "Imported #{copied.length} TZM files."
puts "Content: #{content_root}"
puts "Ink: #{story_root}"
puts "Videos: #{video_root}"
unless warnings.empty?
  warn "Warnings (#{warnings.length}):"
  warnings.uniq.sort.each { |warning| warn "- #{warning}" }
end

compatible_importer = File.join(__dir__, "import-tzm-compatible-resources.rb")
unless system(RbConfig.ruby, compatible_importer, source_root, project_root)
  abort "Compatible TZM resource import failed."
end
