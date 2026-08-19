using System;
using System.IO;
using System.Text;

namespace Novels.Save
{
    internal static class SaveDataCodec
    {
        private static readonly byte[] _magic = { 0x4E, 0x53, 0x56, 0x31 };
        private const byte _formatVersion = 2;

        internal readonly struct DecodedSave
        {
            internal DecodedSave(
                string contentId,
                string contentVersion,
                StoryContracts.StoryDecision[] decisions)
            {
                ContentId = contentId;
                ContentVersion = contentVersion;
                Decisions = decisions ?? Array.Empty<StoryContracts.StoryDecision>();
            }

            internal string ContentId { get; }
            internal string ContentVersion { get; }
            internal StoryContracts.StoryDecision[] Decisions { get; }
        }

        internal static byte[] Encode(
            string contentId,
            string contentVersion,
            StoryContracts.StoryDecision[] decisions)
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
                if (version != _formatVersion)
                    throw new InvalidDataException($"Unsupported save format version '{version}'.");

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
                if (stream.Position != stream.Length)
                    throw new InvalidDataException("Save contains unexpected trailing data.");

                return new DecodedSave(contentId, contentVersion, decisions);
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
