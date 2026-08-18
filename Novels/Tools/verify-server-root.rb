#!/usr/bin/env ruby

require "digest"
require "json"
require "net/http"
require "uri"

if ARGV.length != 2
  warn "Usage: ruby Tools/verify-server-root.rb <https://content-root> <Android|iOS>"
  exit 2
end

root = ARGV[0].sub(%r{/+\z}, "")
platform = ARGV[1]
unless root.start_with?("http://", "https://")
  warn "Content root must be an absolute HTTP(S) URL."
  exit 2
end
unless ["Android", "iOS"].include?(platform)
  warn "Platform must be Android or iOS."
  exit 2
end

def request(uri, limit = 5, &block)
  raise "Too many redirects for #{uri}" if limit <= 0
  Net::HTTP.start(
    uri.host,
    uri.port,
    :use_ssl => uri.scheme == "https",
    :open_timeout => 30,
    :read_timeout => 120
  ) do |http|
    http.request_get(uri.request_uri) do |response|
      if response.is_a?(Net::HTTPRedirection)
        return request(URI.join(uri, response["location"]), limit - 1, &block)
      end
      unless response.is_a?(Net::HTTPSuccess)
        raise "HTTP #{response.code} for #{uri}"
      end
      return block.call(response)
    end
  end
end

def verify_payload(root, relative_path, expected_size, expected_sha)
  uri = URI.parse("#{root}/#{relative_path}")
  digest = Digest::SHA256.new
  size = 0
  request(uri) do |response|
    response.read_body do |chunk|
      size += chunk.bytesize
      digest.update(chunk)
    end
  end
  raise "Size mismatch for #{relative_path}: #{size} != #{expected_size}" \
    unless size == expected_size
  actual_sha = digest.hexdigest
  raise "SHA-256 mismatch for #{relative_path}: #{actual_sha} != #{expected_sha}" \
    unless actual_sha.casecmp(expected_sha).zero?
  puts "OK #{relative_path} (#{size} bytes)"
end

release_path = "Remote/#{platform}/release.json"
release_json = request(URI.parse("#{root}/#{release_path}")) do |response|
  response.body
end
release = JSON.parse(release_json)
raise "Unsupported content schema: #{release["contentSchemaVersion"]}" \
  unless release["contentSchemaVersion"] == 5

payloads = {}
(release["bundles"] || []).each do |bundle|
  path = "Remote/#{platform}/#{bundle["name"]}/#{bundle["version"]}"
  payloads[path] = [bundle["size"], bundle["sha256"]]
end
(release["files"] || []).each do |file|
  path = file["payloadPath"]
  raise "Missing payloadPath for #{file["path"]}" if path.nil? || path.empty?
  metadata = [file["size"], file["sha256"]]
  if payloads.key?(path) && payloads[path] != metadata
    raise "Conflicting metadata for #{path}"
  end
  payloads[path] = metadata
end

puts "Release #{release["releaseId"]} (schema #{release["contentSchemaVersion"]})"
payloads.each do |path, metadata|
  verify_payload(root, path, metadata[0], metadata[1])
end
puts "Verified #{payloads.length} payloads for #{platform}."
