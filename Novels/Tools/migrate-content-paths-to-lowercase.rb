#!/usr/bin/env ruby

require "fileutils"
require "securerandom"

project_root = File.expand_path(ARGV.fetch(0, File.join(__dir__, "..")))
$check_only = ARGV.include?("--check")
content_filter = if (index = ARGV.index("--id"))
                   canonical_argument = ARGV[index + 1]
                   abort "--id requires a value" if canonical_argument.nil?
                   canonical_argument.unicode_normalize(:nfc).strip.downcase
                 end
content_root = File.join(project_root, "Assets/RemoteAssets/Content")
streaming_root = File.join(project_root, "Assets/StreamingAssets")
$planned_moves = []

def canonical(value)
  value.unicode_normalize(:nfc).strip.downcase
end

def collision_groups(directory, entries)
  entries.group_by { |entry| canonical(File.basename(entry)) }
    .values
    .select { |group| group.map { |entry| File.basename(entry) }.uniq.length > 1 }
end

def assert_no_collisions!(directories)
  collisions = directories.flat_map do |directory|
    next [] unless Dir.exist?(directory)
    entries = Dir.children(directory)
      .reject { |name| name.end_with?(".meta") }
      .map { |name| File.join(directory, name) }
    collision_groups(directory, entries)
  end
  return if collisions.empty?

  abort "Canonical path collisions:\n" + collisions
    .map { |group| "- #{group.join("\n  ")}" }
    .join("\n")
end

def move_with_meta(source, target)
  return if source == target
  return unless File.exist?(source)
  abort "Target already exists: #{target}" if File.exist?(target) && !File.identical?(source, target)
  $planned_moves << [source, target]
  return if $check_only

  temporary = File.join(
    File.dirname(source),
    ".__lowercase_migration_#{SecureRandom.hex(8)}"
  )
  FileUtils.mv(source, temporary)
  FileUtils.mv(temporary, target)

  source_meta = "#{source}.meta"
  return unless File.exist?(source_meta)
  target_meta = "#{target}.meta"
  temporary_meta = "#{temporary}.meta"
  FileUtils.mv(source_meta, temporary_meta)
  FileUtils.mv(temporary_meta, target_meta)
end

def rename_children(directory, fixed: [])
  return unless Dir.exist?(directory)
  Dir.children(directory).sort.each do |name|
    next if name.end_with?(".meta") || fixed.include?(name)
    source = File.join(directory, name)
    extension = File.file?(source) ? File.extname(name) : ""
    basename = extension.empty? ? name : File.basename(name, extension)
    target_name = "#{canonical(basename)}#{extension.downcase}"
    move_with_meta(source, File.join(directory, target_name))
  end
end

content_directories = Dir.children(content_root)
  .reject { |name| name.end_with?(".meta") }
  .map { |name| File.join(content_root, name) }
  .select { |path| File.directory?(path) }
content_directories.select! do |path|
  content_filter.nil? || canonical(File.basename(path)) == content_filter
end

streaming_directories = %w[NovelTexts NovelsAudio NovelsVideos].flat_map do |kind|
  root = File.join(streaming_root, kind)
  next [] unless Dir.exist?(root)
  Dir.children(root)
    .reject { |name| name.end_with?(".meta") }
    .map { |name| File.join(root, name) }
    .select { |path| File.directory?(path) }
end
streaming_directories.select! do |path|
  content_filter.nil? || canonical(File.basename(path)) == content_filter
end

directories_to_check = [content_root]
directories_to_check.concat(content_directories)
directories_to_check.concat(streaming_directories)
directories_to_check.concat(
  content_directories.flat_map { |root| Dir.glob(File.join(root, "**", "*")) }
    .select { |path| File.directory?(path) }
)
assert_no_collisions!(directories_to_check)

# Dynamic character path segments. Unity schema folders deliberately keep their casing.
content_directories.each do |root|
  Dir.glob(File.join(root, "{Shared,Episodes/**}/Character/Characters")).each do |characters|
    rename_children(characters)
    Dir.children(characters).reject { |name| name.end_with?(".meta") }.each do |character|
      character_root = File.join(characters, character)
      next unless File.directory?(character_root)

      view_root = File.join(character_root, "View")
      rename_children(view_root, fixed: %w[Child Emotions Main.png])
      Dir.glob(File.join(view_root, "**", "Emotions")).each { |path| rename_children(path) }
      rename_children(File.join(character_root, "Clothes"))
      Dir.glob(File.join(character_root, "Hairs", "{Back,Front}")).each do |layer|
        rename_children(layer)
        Dir.children(layer).reject { |name| name.end_with?(".meta") }.each do |style|
          rename_children(File.join(layer, style))
        end
      end
      Dir.glob(File.join(character_root, "Accessories", "{Back,Middle,Front}"))
        .each { |layer| rename_children(layer) }
    end
  end

  Dir.glob(File.join(root, "Episodes", "*", "Location", "Locations"))
    .each { |locations| rename_children(locations) }

  definition_root = File.join(root, "Definition")
  rename_children(definition_root)
end

# Media names are technical IDs. Ink source/compiled story filenames stay authored.
streaming_directories.each do |root|
  parent = File.basename(File.dirname(root))
  rename_children(root) unless parent == "NovelTexts"
end

# Content and media namespaces are technical IDs.
content_directories.each do |root|
  move_with_meta(root, File.join(content_root, canonical(File.basename(root))))
end
streaming_directories.each do |root|
  move_with_meta(root, File.join(File.dirname(root), canonical(File.basename(root))))
end

if $check_only
  puts "Dry run: #{$planned_moves.length} paths would be renamed."
  $planned_moves.first(40).each { |source, target| puts "- #{source} -> #{target}" }
  puts "- ..." if $planned_moves.length > 40
else
  puts "Content technical paths were migrated to canonical lower-case."
end
