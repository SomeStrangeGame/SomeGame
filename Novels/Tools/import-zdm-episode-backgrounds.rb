#!/usr/bin/env ruby

require "fileutils"

CONTENT_ID = "ZDM"
EPISODES_SEGMENT = "Episodes"
STORY_CONTENT_ROOT = "Assets/StreamingAssets/NovelTexts"
REMOTE_CONTENT_ROOT = "Assets/RemoteAssets/Content"
INLINE_COMMENT = "//"
VARIABLE_PREFIX = "{"
CUT_SCENE_COMMANDS = ["Кат-сцена", "cut-scene"].freeze
SOURCE_LOCATIONS_SEGMENT = "/Локации/"
SOURCE_CUT_SCENES_SEGMENT = "/Кат-сцены/"
SOURCE_STATIC_SEGMENT = "/Статичные/"
PREFERRED_CUT_SCENE_SEGMENT = "/Голубой лотос/"
FALLBACK_CUT_SCENE_SEGMENT = "/Без ГГ/"
TARGET_LOCATION_SEGMENTS = ["Location", "Locations"].freeze
BUILT_IN_BACKGROUNDS = ["Темнота", "Чёрный экран"].freeze
STORY_BACKGROUND_PATTERN =
  /^\s*(Локация|Кат-сцена|location|cut-scene)(?:\s*\([^)]*\))?\s*:\s*(.+?)\s*$/i

source_root = File.expand_path(ARGV.fetch(0) do
  abort "Usage: #{File.basename($PROGRAM_NAME)} <visual-source-root> [project-root]"
end)
project_root = File.expand_path(ARGV.fetch(1, File.join(__dir__, "..")))
story_root = File.join(project_root, STORY_CONTENT_ROOT, CONTENT_ID)
target_root = File.join(
  project_root,
  REMOTE_CONTENT_ROOT,
  CONTENT_ID,
  EPISODES_SEGMENT
)

abort "Source directory does not exist: #{source_root}" unless Dir.exist?(source_root)
abort "ZDM story directory does not exist: #{story_root}" unless Dir.exist?(story_root)

def normalized(value)
  value.unicode_normalize(:nfc).strip.downcase
end

def path_contains?(path, segment)
  path.unicode_normalize(:nfc).include?(segment.unicode_normalize(:nfc))
end

def supported_source?(path)
  path_contains?(path, SOURCE_LOCATIONS_SEGMENT) ||
    path_contains?(path, SOURCE_CUT_SCENES_SEGMENT)
end

def source_index(source_root)
  Dir.glob(File.join(source_root, "**", "*.png"))
    .select { |path| supported_source?(path) }
    .group_by { |path| normalized(File.basename(path, ".*")) }
end

def select_source(candidates, cut_scene)
  return nil if candidates.empty?

  if cut_scene
    cut_scenes = candidates.select do |path|
      path_contains?(path, SOURCE_CUT_SCENES_SEGMENT)
    end
    return cut_scenes.find { |path| path_contains?(path, PREFERRED_CUT_SCENE_SEGMENT) } ||
      cut_scenes.find { |path| path_contains?(path, FALLBACK_CUT_SCENE_SEGMENT) } ||
      cut_scenes.first
  end

  locations = candidates.select do |path|
    path_contains?(path, SOURCE_LOCATIONS_SEGMENT) &&
      path_contains?(path, SOURCE_STATIC_SEGMENT)
  end
  locations.first || select_source(candidates, true)
end

def built_in_background?(name)
  BUILT_IN_BACKGROUNDS.any? do |candidate|
    normalized(candidate) == normalized(name)
  end
end

references = Hash.new { |episodes, episode| episodes[episode] = {} }
Dir.glob(File.join(story_root, "*.ink")).sort.each do |story_path|
  match = File.basename(story_path).match(/s(\d{2})e(\d{2})/i)
  next unless match

  episode = "s#{match[1]}e#{match[2]}".downcase
  File.foreach(story_path) do |line|
    command = line.match(STORY_BACKGROUND_PATTERN)
    next unless command

    name = command[2].split(INLINE_COMMENT, 2).first.to_s.strip.unicode_normalize(:nfc)
    next if name.empty? || name.start_with?(VARIABLE_PREFIX) || built_in_background?(name)

    cut_scene = CUT_SCENE_COMMANDS.any? do |candidate|
      normalized(command[1]) == normalized(candidate)
    end
    previous = references[episode][name]
    references[episode][name] = previous == true || cut_scene
  end
end

index = source_index(source_root)
copied = 0
missing = []

references.sort.each do |episode, entries|
  entries.sort.each do |name, cut_scene|
    source = select_source(index.fetch(normalized(name), []), cut_scene)
    unless source
      missing << "#{episode}: #{name}"
      next
    end

    destination = File.join(
      target_root,
      episode,
      *TARGET_LOCATION_SEGMENTS,
      "#{name}.png"
    )
    FileUtils.mkdir_p(File.dirname(destination))
    FileUtils.cp(source, destination)
    copied += 1
  end
end

puts "Imported #{copied} ZDM episode backgrounds into #{target_root}."
unless missing.empty?
  warn "Missing #{missing.length} background source(s):"
  missing.each { |value| warn "- #{value}" }
end
