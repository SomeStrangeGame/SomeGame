using System;
using System.IO;
using System.Text;

namespace Novels.Save
{
    internal static class SaveDataCodec
    {
        private static readonly byte[] _magic = { 0x4E, 0x53, 0x56, 0x31 };
        private const byte _formatVersion = 1;

        internal readonly struct DecodedSave
        {
            internal DecodedSave(
                string contentId,
                string contentVersion,
                byte[] choices)
            {
                ContentId = contentId;
                ContentVersion = contentVersion;
                Choices = choices ?? Array.Empty<byte>();
            }

            internal string ContentId { get; }
            internal string ContentVersion { get; }
            internal byte[] Choices { get; }
        }

        internal static byte[] Encode(
            string contentId,
            string contentVersion,
            byte[] choices)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(_magic);
            writer.Write(_formatVersion);
            writer.Write(contentId ?? string.Empty);
            writer.Write(contentVersion ?? string.Empty);
            writer.Write(choices?.Length ?? 0);
            if (choices != null)
                writer.Write(choices);
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
                var choiceCount = reader.ReadInt32();
                if (choiceCount < 0 || choiceCount > stream.Length - stream.Position)
                    throw new InvalidDataException("Save choice payload length is invalid.");

                var choices = reader.ReadBytes(choiceCount);
                if (stream.Position != stream.Length)
                    throw new InvalidDataException("Save contains unexpected trailing data.");

                return new DecodedSave(contentId, contentVersion, choices);
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
