#!/usr/bin/env ruby

require "json"
require "pathname"

if ARGV.empty?
  warn "Usage: ruby Tools/plan-server-root-gc.rb <ServerRoot> [retained-release.json ...]"
  exit 2
end

root = Pathname.new(ARGV.shift).expand_path
raise "ServerRoot does not exist: #{root}" unless root.directory?

retained = { "deployment.json" => true }
release_paths = Dir.glob(root.join("Remote", "*", "release.json").to_s)
release_paths.concat(ARGV.map { |value| Pathname.new(value).expand_path.to_s })
release_paths.uniq.each do |release_path|
  release_file = Pathname.new(release_path)
  raise "Retained release does not exist: #{release_file}" unless release_file.file?
  release = JSON.parse(release_file.read)
  if release_file.to_s.start_with?(root.to_s + File::SEPARATOR)
    retained[release_file.relative_path_from(root).to_s] = true
  end
  platform = release_file.dirname.basename.to_s
  (release["bundles"] || []).each do |bundle|
    retained["Remote/#{platform}/#{bundle["name"]}/#{bundle["version"]}"] = true
  end
  (release["files"] || []).each do |file|
    retained[file.fetch("payloadPath")] = true
  end
end

candidates = []
Dir.glob(root.join("**", "*").to_s, File::FNM_DOTMATCH).each do |path|
  file = Pathname.new(path)
  next unless file.file?
  relative = file.relative_path_from(root).to_s
  next if relative == ".DS_Store" || relative.end_with?("/.DS_Store")
  candidates << [relative, file.size] unless retained.key?(relative)
end

if candidates.empty?
  puts "No unreachable files found."
  exit 0
end

puts "Unreachable files (read-only plan):"
candidates.sort_by(&:first).each do |relative, size|
  puts "#{size}\t#{relative}"
end
total = candidates.sum { |value| value[1] }
puts "Candidates: #{candidates.length}; bytes: #{total}"
puts "No files were deleted."
