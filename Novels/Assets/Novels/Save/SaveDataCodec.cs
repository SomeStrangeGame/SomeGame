using System;
using System.IO;
using System.Text;

namespace Novels.Save
{
    internal sealed class UnsupportedSaveFormatException : Exception
    {
        internal UnsupportedSaveFormatException(byte version)
            : base($"Unsupported save format version '{version}'.")
        {
            Version = version;
        }

        internal byte Version { get; }
    }

    internal static class SaveDataCodec
    {
        private static readonly byte[] _magic = { 0x4E, 0x53, 0x56, 0x31 };
        private const byte _formatVersion = 3;
        private const byte _legacyFormatVersion = 2;

        internal readonly struct WardrobeItem
        {
            internal WardrobeItem(
                string character,
                byte category,
                string value,
                bool equipped)
            {
                Character = character ?? string.Empty;
                Category = category;
                Value = value ?? string.Empty;
                Equipped = equipped;
            }

            internal string Character { get; }
            internal byte Category { get; }
            internal string Value { get; }
            internal bool Equipped { get; }
        }

        internal readonly struct DecodedSave
        {
            internal DecodedSave(
                string contentId,
                string contentVersion,
                StoryContracts.StoryDecision[] decisions,
                WardrobeItem[] wardrobeItems)
            {
                ContentId = contentId;
                ContentVersion = contentVersion;
                Decisions = decisions ?? Array.Empty<StoryContracts.StoryDecision>();
                WardrobeItems = wardrobeItems ?? Array.Empty<WardrobeItem>();
            }

            internal string ContentId { get; }
            internal string ContentVersion { get; }
            internal StoryContracts.StoryDecision[] Decisions { get; }
            internal WardrobeItem[] WardrobeItems { get; }
        }

        internal static byte[] Encode(
            string contentId,
            string contentVersion,
            StoryContracts.StoryDecision[] decisions,
            WardrobeItem[] wardrobeItems)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(_magic);
            writer.Write(_formatVersion);
            writer.Write(contentId ?? string.Empty);
            writer.Write(contentVersion ?? string.Empty);
            writer.Write(decisions?.Length ?? 0);
            if (decisions != null)
            {
                foreach (var decision in decisions)
                {
                    writer.Write(decision.HasChoice ? (byte)1 : (byte)0);
                    if (decision.HasChoice)
                        writer.Write(decision.ChoiceId);
                }
            }
            writer.Write(wardrobeItems?.Length ?? 0);
            if (wardrobeItems != null)
            {
                foreach (var item in wardrobeItems)
                {
                    writer.Write(item.Character ?? string.Empty);
                    writer.Write(item.Category);
                    writer.Write(item.Value ?? string.Empty);
                    writer.Write(item.Equipped);
                }
            }
            return stream.ToArray();
        }

        internal static DecodedSave Decode(byte[] data)
        {
            data ??= Array.Empty<byte>();
            if (!HasEnvelope(data))
                throw new InvalidDataException("Save envelope signature is invalid.");

            try
            {
                using var stream = new MemoryStream(data, false);
                using var reader = new BinaryReader(stream, Encoding.UTF8);
                reader.ReadBytes(_magic.Length);
                var version = reader.ReadByte();
                if (version != _formatVersion && version != _legacyFormatVersion)
                    throw new UnsupportedSaveFormatException(version);

                var contentId = reader.ReadString();
                var contentVersion = reader.ReadString();
                var decisionCount = reader.ReadInt32();
                if (decisionCount < 0 || decisionCount > stream.Length - stream.Position)
                    throw new InvalidDataException("Save decision count is invalid.");
                var decisions = new StoryContracts.StoryDecision[decisionCount];
                for (var index = 0; index < decisionCount; index++)
                {
                    var kind = reader.ReadByte();
                    decisions[index] = kind switch
                    {
                        0 => StoryContracts.StoryDecision.Advance,
                        1 => StoryContracts.StoryDecision.Choice(reader.ReadInt32()),
                        _ => throw new InvalidDataException(
                            $"Save decision kind '{kind}' is invalid."),
                    };
                }
                var wardrobeItems = Array.Empty<WardrobeItem>();
                if (version >= _formatVersion)
                {
                    var itemCount = reader.ReadInt32();
                    if (itemCount < 0 || itemCount > stream.Length - stream.Position)
                        throw new InvalidDataException("Save wardrobe item count is invalid.");
                    wardrobeItems = new WardrobeItem[itemCount];
                    for (var index = 0; index < itemCount; index++)
                    {
                        wardrobeItems[index] = new WardrobeItem(
                            reader.ReadString(),
                            reader.ReadByte(),
                            reader.ReadString(),
                            reader.ReadBoolean());
                    }
                }
                if (stream.Position != stream.Length)
                    throw new InvalidDataException("Save contains unexpected trailing data.");

                return new DecodedSave(contentId, contentVersion, decisions, wardrobeItems);
            }
            catch (EndOfStreamException exception)
            {
                throw new InvalidDataException("Save envelope is incomplete.", exception);
            }
        }

        private static bool HasEnvelope(byte[] data)
        {
            if (data.Length < _magic.Length)
                return false;

            for (var i = 0; i < _magic.Length; i++)
            {
                if (data[i] != _magic[i])
                    return false;
            }
            return true;
        }
    }
}
