#!/usr/bin/env ruby

require "digest"
require "json"
require "net/http"
require "uri"

if ARGV.length < 2 || ARGV.length > 3
  warn "Usage: ruby Tools/verify-server-root.rb <https://content-root> <Android|iOS> [deployment-id]"
  exit 2
end

root = ARGV[0].sub(%r{/+\z}, "")
platform = ARGV[1]
expected_deployment_id = ARGV[2]
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

def download_text(root, relative_path)
  request(URI.parse("#{root}/#{relative_path}")) do |response|
    response.body
  end
end

deployment = JSON.parse(download_text(root, "deployment.json"))
platforms = deployment["platforms"] || []
deployment_platform = platforms.find { |value| value["platform"] == platform }
raise "Deployment does not contain platform #{platform}" if deployment_platform.nil?

deployment_payloads = {}
(deployment["payloads"] || []).each do |payload|
  path = payload["path"]
  metadata = [payload["size"], payload["sha256"], payload["activateLast"] == true]
  raise "Duplicate deployment payload #{path}" if deployment_payloads.key?(path)
  deployment_payloads[path] = metadata
end

canonical = platforms.sort_by { |value| value["platform"] }.map do |value|
  "P:#{value["platform"]}:#{value["releaseId"]}:#{value["releasePath"]}"
end
canonical.concat((deployment["payloads"] || [])
  .sort_by { |value| [value["activateLast"] == true ? 1 : 0, value["path"]] }
  .map do |value|
    active = value["activateLast"] == true ? "True" : "False"
    "F:#{value["path"]}:#{value["size"]}:#{value["sha256"]}:#{active}"
  end)
actual_deployment_id = Digest::SHA256.hexdigest(canonical.join("\n"))
unless actual_deployment_id.casecmp(deployment["deploymentId"].to_s).zero?
  raise "Deployment fingerprint mismatch"
end
if expected_deployment_id \
    && !expected_deployment_id.casecmp(deployment["deploymentId"].to_s).zero?
  raise "Remote deployment does not match local deployment #{expected_deployment_id}"
end

release_path = deployment_platform["releasePath"]
release_metadata = deployment_payloads[release_path]
raise "Deployment does not describe #{release_path}" if release_metadata.nil?
raise "Release #{release_path} is not marked activateLast" unless release_metadata[2]
release_json = download_text(root, release_path)
release_size = release_json.bytesize
release_sha = Digest::SHA256.hexdigest(release_json)
raise "Size mismatch for #{release_path}" unless release_size == release_metadata[0]
unless release_sha.casecmp(release_metadata[1]).zero?
  raise "SHA-256 mismatch for #{release_path}"
end
release = JSON.parse(release_json)
raise "Unsupported content schema: #{release["contentSchemaVersion"]}" \
  unless release["contentSchemaVersion"] == 5
unless release["releaseId"] == deployment_platform["releaseId"]
  raise "Deployment release ID mismatch for #{platform}"
end

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
  deployed = deployment_payloads[path]
  raise "Deployment does not describe #{path}" if deployed.nil?
  unless deployed[0] == metadata[0] && deployed[1].casecmp(metadata[1]).zero?
    raise "Deployment metadata differs for #{path}"
  end
  raise "Immutable payload is marked activateLast: #{path}" if deployed[2]
  verify_payload(root, path, metadata[0], metadata[1])
end
puts "Verified deployment #{deployment["deploymentId"]}."
puts "Verified release and #{payloads.length} payloads for #{platform}."
