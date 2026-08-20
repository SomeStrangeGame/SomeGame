#!/usr/bin/env ruby

TEXT_SCRIPT = "5f7201a12d95ffc409449d95f23cf332"
IMAGE_SCRIPT = "fe87c0e1cc204ed48ad3b37840f39efc"
BUTTON_SCRIPT = "4e29b1a8efbd4b44bb3f3716e73f07ff"

PANEL = "{r: 0.18, g: 0.2, b: 0.23, a: 0.96}"
SECONDARY = "{r: 0.34, g: 0.38, b: 0.44, a: 1}"
TEXT = "{r: 0.95, g: 0.96, b: 0.98, a: 1}"
MUTED = "{r: 0.7, g: 0.74, b: 0.8, a: 1}"

BUTTON_COLORS = {
  "m_NormalColor" => "{r: 0.34, g: 0.38, b: 0.44, a: 1}",
  "m_HighlightedColor" => "{r: 0.46, g: 0.51, b: 0.58, a: 1}",
  "m_PressedColor" => "{r: 0.23, g: 0.26, b: 0.31, a: 1}",
  "m_SelectedColor" => "{r: 0.46, g: 0.51, b: 0.58, a: 1}",
  "m_DisabledColor" => "{r: 0.24, g: 0.27, b: 0.31, a: 0.55}",
}.freeze

ROOT = File.expand_path("../Assets/Novels/Fallbacks/EpisodeUI", __dir__)

def component_script(block)
  block[/m_Script: \{fileID: 11500000, guid: ([0-9a-f]{32}), type: 3\}/, 1]
end

def replace_color(block, property, color)
  block.sub(/^(\s*#{Regexp.escape(property)}:) \{[^\n]+\}$/) do
    "#{Regexp.last_match(1)} #{color}"
  end
end

def image_color(feature, name, block)
  return nil unless ["bubble", "loading", "notification"].include?(feature)

  case feature
  when "bubble"
    return SECONDARY if ["Header", "DefaultButton", "Point"].include?(name)
    if name == "Background"
      return PANEL unless block.match?(/m_Color: \{[^\n]*a: 0\}/)
      return "{r: 0.18, g: 0.2, b: 0.23, a: 0}"
    end
  when "loading"
    return PANEL if name == "Background"
    return MUTED if name == "Image"
    return SECONDARY if name == "Header"
  when "notification"
    return PANEL if name == "Background"
    return SECONDARY if name == "Sticker"
  end
  nil
end

Dir.glob(File.join(ROOT, "*", "screen.prefab")).sort.each do |path|
  feature = File.basename(File.dirname(path))
  blocks = File.read(path).split(/(?=^--- !u!)/)
  names = {}
  blocks.each do |block|
    next unless block.start_with?("--- !u!1 ")

    id = block[/^--- !u!1 &(-?\d+)/, 1]
    names[id] = block[/^  m_Name: (.*)$/, 1]
  end

  changed = false
  blocks.map! do |block|
    game_object = block[/^  m_GameObject: \{fileID: (-?\d+)\}/, 1]
    name = names[game_object]
    script = component_script(block)
    updated = block
    if script == TEXT_SCRIPT && ["bubble", "notification"].include?(feature)
      updated = replace_color(updated, "m_Color", TEXT)
    elsif script == IMAGE_SCRIPT
      color = image_color(feature, name, block)
      updated = replace_color(updated, "m_Color", color) if color
    elsif script == BUTTON_SCRIPT && ["bubble", "notification"].include?(feature)
      BUTTON_COLORS.each do |property, color|
        updated = replace_color(updated, property, color)
      end
    end
    changed ||= updated != block
    updated
  end

  next unless changed

  File.write(path, blocks.join)
  puts "Styled #{path}"
end
