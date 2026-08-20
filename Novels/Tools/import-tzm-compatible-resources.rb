#!/usr/bin/env ruby

require "fileutils"
require "securerandom"

EPISODES = (1..7).map { |number| format("s01e%02d", number) }.freeze
BACKGROUND_PATTERN =
  /^\s*(Локация|Кат-сцена|location|cut-scene)(?:\s*\([^)]*\))?\s*:\s*(.+?)\s*$/i
BUILT_IN_BACKGROUNDS = ["Темнота", "Чёрный экран"].freeze
LOCATION_ALIASES = {
  "Гардероб суша день" => "Wardrobe",
  "Номер в отеле" => "HotelRoom",
  "Вид из окна" => "HotelRoom",
  "Кафе" => "Cafe",
  "Причал" => "Pier",
  "Катер в движении" => "BoatInMove",
  "Катер в море" => "Boat",
  "Под водой" => "Underwater_Water",
  "Катер в море закат" => "BoatSunset",
  "Катер в море ночь" => "BoatAtNight_Dark",
  "Светящийся остров" => "LightingIsland_Dark",
  "Остров под водой" => "IslandUnderwater_Water",
  "Светящаяся пещера с бассейном" => "CavePoolNight",
  "Светящаяся пещера водоворот" => "WhirlpoolNight",
  "Тоннель" => "TonelClosed_Water",
  "У тоннеля с проходом" => "TonelWithPath_Water",
  "Комната Алексы" => "AlexasRoom_Water",
  "Старый город" => "OldTown_Water",
  "Мотоцикл" => "Bike_Water",
  "Мотоцикл в движении" => "BikeInMove_Water",
  "Мотоцикл остановка" => "Bike stop",
  "Мост в город" => "Bridge1_Water",
  "Мост в Атлантиду" => "BridgeToAtlantis_Water",
  "Атлантида" => "Atlantis_Water",
  "Атлантида на суше" => "AtlantisInGround",
  "Метеорит" => "Meteor",
  "Светящийся метеорит" => "LightingMeteor",
  "Главный зал" => "MainHallEmpty_Water",
  "Разрушение Атлантиды" => "AtlantisDestroy",
  "Разрушенная Атлантида под водой" => "AtlantisDestroyUnderwater_Water_Dark",
  "Дворец" => "MainRoom",
  "Главный зал дворца" => "MainHall_Water",
  "Песчаный пляж" => "SandyBeach",
  "Бутик" => "Boutique",
  "Набережная вечер" => "EmbankmentEvening",
  "Набережная день" => "EmbankmentDay",
  "Набережная ночь" => "EmbankmentNight",
  "Остров днём" => "UnLightingIsland",
  "Пещера днём" => "CavePoolDay",
  "Пещера днём водоворот" => "WhirlpoolDay",
  "У тоннеля" => "Tonel_Water",
  "Причал с красивой яхтой" => "PierDamonDay",
  "Яхта в движении" => "Yacht",
  "Яхта день" => "Yacht",
  "Яхта закат" => "YachtSunset",
  "Яхта ночь" => "YachtTableNight",
  "Каюта" => "YachtSofa",
  "Номер ГГ" => "HotelRoom2",
  "Пляж" => "SandyBeach",
  "Морской сад" => "SeaGarden1_Water",
  "Морской сад чуть выше" => "SeaGarden2_Water",
  "Морской сад чуть повыше" => "SeaGarden2_Water",
  "Вода" => "Underwater_Water",
  "Набережная с мороженым" => "Icecream",
  "Причал с катерами" => "Pier",
  "Комната Дэймона" => "DamonsRoom",
  "Домик у моря" => "PhilsHouse",
  "Кухня в домике у моря" => "PhilsKitchen",
  "Скамейка на набережной ночь" => "Bench",
  "Акула" => "Shark"
}.freeze
CUT_SCENE_ALIASES = {
  "Вид из окна" => "Window",
  "акула" => "Shark1",
  "Алекса на подводном мотоцикле" => "Alexa1",
  "с Алексой на мотоцикле" => "Vvodovorote_Euro",
  "метеорит засветился" => "LightingMeteor",
  "Погружение города" => "ImmersionAtlantis",
  "Лили в гневе" => "Lily",
  "Рождение медузы" => "Jellyfish"
}.freeze

def alias_index(aliases)
  aliases.each_with_object({}) do |(logical, technical), index|
    index[logical.unicode_normalize(:nfc).downcase] = [logical, technical]
  end
end

LOCATION_ALIAS_INDEX = alias_index(LOCATION_ALIASES).freeze
CUT_SCENE_ALIAS_INDEX = alias_index(CUT_SCENE_ALIASES).freeze

source_root = File.expand_path(ARGV.fetch(0) do
  abort "Usage: #{File.basename($PROGRAM_NAME)} <tzm-source-root> [project-root]"
end)
project_root = File.expand_path(ARGV.fetch(1, File.join(__dir__, "..")))
ink_root = File.join(source_root, "ink")
legacy_content = File.join(
  project_root,
  "Assets/RemoteAssets/Content/tzm_1/Episodes/s01e01/Location/Locations"
)
source_visuals = File.join(source_root, "Визуал ТЗМ")
target_content = File.join(project_root, "Assets/RemoteAssets/Content/tzm/Episodes")

def ensure_folder_meta(directory, bundle_name = nil)
  FileUtils.mkdir_p(directory)
  meta = "#{directory}.meta"
  return if File.exist?(meta)

  File.write(meta, <<~META)
    fileFormatVersion: 2
    guid: #{SecureRandom.hex(16)}
    folderAsset: yes
    DefaultImporter:
      externalObjects: {}
      userData:
      assetBundleName: #{bundle_name}
      assetBundleVariant:
  META
end

def normalized(value)
  value.to_s.unicode_normalize(:nfc).strip
end

def key(value)
  normalized(value).downcase
end

def images(root)
  return [] unless Dir.exist?(root)

  Dir.glob(File.join(root, "**", "*"))
    .select { |path| File.file?(path) && File.extname(path).casecmp?(".png") }
end

def copy_tree_files(source_root, target_root, extension)
  return 0 unless Dir.exist?(source_root)

  copied = 0
  Dir.glob(File.join(source_root, "**", "*#{extension}"), File::FNM_CASEFOLD)
    .sort.each do |source|
      next unless File.file?(source)

      extension = File.extname(source).downcase
      basename = key(File.basename(source, ".*"))
      destination = File.join(target_root, "#{basename}#{extension}")
      FileUtils.mkdir_p(File.dirname(destination))
      FileUtils.cp(source, destination)
      copied += 1
    end
  copied
end

resource_roots = [
  legacy_content,
  File.join(source_visuals, "Locations"),
  File.join(source_visuals, "Cut-Scenes")
]
index = resource_roots.flat_map { |root| images(root) }
  .group_by { |path| key(File.basename(path, ".*")) }
copied_backgrounds = 0
missing = []
ensure_folder_meta(target_content)
EPISODES.each do |episode|
  ensure_folder_meta(
    File.join(target_content, episode),
    "novels_episode_tzm_#{episode}"
  )
  File.foreach(File.join(ink_root, "#{episode}.ink")) do |line|
    command = line.match(BACKGROUND_PATTERN)
    next unless command

    name = normalized(command[2].split("//", 2).first)
    next if name.empty? || name.start_with?("{")
    next if BUILT_IN_BACKGROUNDS.any? { |candidate| key(candidate) == key(name) }

    aliases = key(command[1]).start_with?("кат") || key(command[1]) == "cut-scene" ?
      CUT_SCENE_ALIAS_INDEX : LOCATION_ALIAS_INDEX
    alias_entry = aliases[key(name)]
    _, technical_name = alias_entry || [name, name]
    source = index.fetch(key(technical_name), []).first
    unless source
      missing << "#{episode}: #{name}"
      next
    end
    destination = File.join(
      target_content,
      episode,
      "Location",
      "Locations",
      "#{key(name)}.png"
    )
    next if File.exist?(destination) && alias_entry.nil?

    FileUtils.mkdir_p(File.dirname(destination))
    FileUtils.cp(source, destination)
    copied_backgrounds += 1
  end
end

legacy_audio = File.join(project_root, "Assets/StreamingAssets/NovelsAudio/tzm_1")
target_audio = File.join(project_root, "Assets/StreamingAssets/NovelsAudio/tzm")
legacy_video = File.join(project_root, "Assets/StreamingAssets/NovelsVideos/tzm_1")
target_video = File.join(project_root, "Assets/StreamingAssets/NovelsVideos/tzm")
copied_audio = %w[.wav .mp3 .ogg].sum do |extension|
  copy_tree_files(legacy_audio, target_audio, extension)
end
copied_video = copy_tree_files(legacy_video, target_video, ".mp4")

video_roots = [
  File.join(source_visuals, "Locations", "Videos"),
  File.join(source_visuals, "Cut-Scenes", "Videos")
]
video_index = video_roots.flat_map do |root|
  Dir.exist?(root) ? Dir.glob(File.join(root, "*.mp4"), File::FNM_CASEFOLD) : []
end.group_by { |path| key(File.basename(path, ".*")) }
[LOCATION_ALIAS_INDEX, CUT_SCENE_ALIAS_INDEX].each do |aliases|
aliases.each_value do |logical_name, technical_name|
  source = video_index.fetch(key(technical_name), []).first
  next unless source

  destination = File.join(target_video, "#{key(logical_name)}.mp4")
  next if File.exist?(destination)

  FileUtils.mkdir_p(target_video)
  FileUtils.cp(source, destination)
  copied_video += 1
end
end

puts "Imported #{copied_backgrounds} compatible TZM backgrounds."
puts "Imported #{copied_audio} compatible TZM audio files."
puts "Imported #{copied_video} compatible TZM video files."
unless missing.empty?
  warn "Still missing #{missing.uniq.length} background(s):"
  missing.uniq.sort.each { |entry| warn "- #{entry}" }
end
