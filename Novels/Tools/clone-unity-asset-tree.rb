#!/usr/bin/env ruby

require "fileutils"
require "pathname"
require "securerandom"

source = File.expand_path(ARGV.fetch(0) do
  abort "Usage: #{File.basename($PROGRAM_NAME)} <source> <target> [--prefab-only]"
end)
target = File.expand_path(ARGV.fetch(1))
prefab_only = ARGV.include?("--prefab-only")

abort "Source directory does not exist: #{source}" unless Dir.exist?(source)
abort "Target already exists: #{target}" if File.exist?(target) || File.exist?("#{target}.meta")
abort "Source meta file does not exist: #{source}.meta" unless File.file?("#{source}.meta")

source_assets = if prefab_only
  screen = File.join(source, "Screen.prefab")
  abort "Source screen prefab does not exist: #{screen}" unless File.file?(screen)
  [source, screen]
else
  [source] + Dir.glob(File.join(source, "**", "*"), File::FNM_DOTMATCH)
    .reject { |path| [".", ".."].include?(File.basename(path)) }
    .reject { |path| path.end_with?(".meta") }
end

target_for = lambda do |path|
  if path == source
    target
  else
    File.join(
      target,
      Pathname.new(path).relative_path_from(Pathname.new(source)).to_s
    )
  end
end

source_assets.select { |path| File.directory?(path) }.each do |directory|
  FileUtils.mkdir_p(target_for.call(directory))
end
source_assets.select { |path| File.file?(path) }.each do |file|
  destination = target_for.call(file)
  FileUtils.mkdir_p(File.dirname(destination))
  FileUtils.cp(file, destination)
end

meta_sources = source_assets
  .map { |path| "#{path}.meta" }
  .select { |path| File.file?(path) }
guid_map = {}
meta_sources.each do |meta|
  old_guid = File.foreach(meta).find { |line| line.start_with?("guid: ") }
  next unless old_guid
  guid_map[old_guid.split.last] = SecureRandom.hex(16)
end

meta_sources.each do |meta|
  asset = meta.delete_suffix(".meta")
  destination = "#{target_for.call(asset)}.meta"
  contents = File.binread(meta)
  guid_map.each { |old_guid, new_guid| contents.gsub!(old_guid, new_guid) }
  File.binwrite(destination, contents)
end

Dir.glob(File.join(target, "**", "*.{prefab,asset,mat,controller,anim}"), File::FNM_EXTGLOB)
  .each do |asset|
    contents = File.binread(asset)
    guid_map.each { |old_guid, new_guid| contents.gsub!(old_guid, new_guid) }
    File.binwrite(asset, contents)
  end

puts "Cloned #{source} to #{target} with #{guid_map.length} remapped GUIDs."
