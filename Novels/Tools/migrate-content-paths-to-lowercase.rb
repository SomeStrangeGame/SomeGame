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
streaming_root = File.join(project_root, "Assets/StreamingAssets")
$planned_moves = []

def canonical(value)
  value.unicode_normalize(:nfc).strip.downcase
end

remote_assets_root = File.join(project_root, "Assets/RemoteAssets")
content_root_name = Dir.children(remote_assets_root).find do |name|
  !name.end_with?(".meta") && canonical(name) == "content"
end
abort "Remote content root does not exist" if content_root_name.nil?
content_root = File.join(remote_assets_root, content_root_name)

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

streaming_directories = %w[noveltexts novelsaudio novelsvideos].flat_map do |kind|
  root_name = Dir.children(streaming_root).find do |name|
    !name.end_with?(".meta") && canonical(name) == kind
  end
  next [] if root_name.nil?
  root = File.join(streaming_root, root_name)
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
directories_to_check.concat(
  streaming_directories.flat_map { |root| Dir.glob(File.join(root, "**", "*")) }
    .select { |path| File.directory?(path) }
)
assert_no_collisions!(directories_to_check)

def rename_tree(directory)
  return unless Dir.exist?(directory)
  Dir.children(directory).sort.each do |name|
    next if name.end_with?(".meta")
    child = File.join(directory, name)
    rename_tree(child) if File.directory?(child)
  end
  rename_children(directory)
end

# Everything below a content/media namespace is a technical address. Player-facing
# text remains authored in Ink and is not derived from these file-system names.
content_directories.each { |root| rename_tree(root) }
streaming_directories.each { |root| rename_tree(root) }

# Content and media namespaces are technical IDs.
content_directories.each do |root|
  move_with_meta(root, File.join(content_root, canonical(File.basename(root))))
end
streaming_directories.each do |root|
  move_with_meta(root, File.join(File.dirname(root), canonical(File.basename(root))))
end
streaming_directories.map { |root| File.dirname(root) }.uniq.each do |root|
  move_with_meta(root, File.join(File.dirname(root), canonical(File.basename(root))))
end
%w[catalog loading].each do |name|
  actual_name = Dir.children(remote_assets_root).find do |entry|
    !entry.end_with?(".meta") && canonical(entry) == name
  end
  next if actual_name.nil?
  root = File.join(remote_assets_root, actual_name)
  rename_tree(root)
  move_with_meta(root, File.join(remote_assets_root, name))
end
move_with_meta(content_root, File.join(remote_assets_root, "content"))

if $check_only
  puts "Dry run: #{$planned_moves.length} paths would be renamed."
  $planned_moves.first(40).each { |source, target| puts "- #{source} -> #{target}" }
  puts "- ..." if $planned_moves.length > 40
else
  puts "Content technical paths were migrated to canonical lower-case."
end
