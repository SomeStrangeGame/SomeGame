using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Novels.StoryContracts;

namespace Novels
{
    // A catalog-only estimate, never authoritative for replay, unlocking or completion.
    internal static class EpisodeReadingProgress
    {
        private const int Magic = 0x31525045; // EPR1, optional sidecar format version 1.

        internal static string Key(string saveKey) => saveKey + ".reading-progress";

        internal static float? Read(Cache.Entity cache, string saveKey, string version)
        {
            if (!cache.Exists(saveKey)) return 0f;
            try
            {
                using var stream = new MemoryStream(cache.ReadBytes(Key(saveKey)), false);
                using var reader = new BinaryReader(stream, Encoding.UTF8);
                if (reader.ReadInt32() != Magic || reader.ReadString() != version
                    || reader.ReadString() != Hash(cache.ReadBytes(saveKey))) return null;
                var ratio = reader.ReadSingle();
                if (stream.Position != stream.Length || float.IsNaN(ratio)
                    || float.IsInfinity(ratio) || ratio < 0f || ratio >= 1f) return null;
                return ratio;
            }
            catch (Exception) { return null; } // Optional metadata must never discard a save.
        }

        internal static void Write(Cache.Entity cache, string saveKey, string version,
            byte[] persistedSave, float ratio)
        {
            if (float.IsNaN(ratio) || float.IsInfinity(ratio)) return;
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(Magic);
            writer.Write(version ?? string.Empty);
            writer.Write(Hash(persistedSave));
            writer.Write(Math.Max(0f, Math.Min(.99f, ratio)));
            writer.Flush();
            cache.WriteBytes(Key(saveKey), stream.ToArray());
        }

        private static string Hash(byte[] bytes)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        // Replay actual saved decisions, then forecast ONE possible remaining route.
        // Future choices use the first available option: the UI explicitly says ≈.
        // This neither sums mutually exclusive branches nor executes presentation/media.
        internal static float? Estimate(string storyText, string initialState,
            StoryDecision[] decisions, string endMarker)
        {
            if (decisions == null) return null;
            if (decisions.Length == 0) return 0f;
            var clock = Stopwatch.StartNew();
            try
            {
                using var story = new StoryProcessor.Entity(new StoryProcessor.Entity.Ctx
                {
                    StoryText = storyText,
                    InitialState = initialState,
                    ReadTimeLimitMilliseconds = 5f,
                });
                var parser = new StoryCommands.Entity();
                var dialogues = 0;
                for (var reads = 0; reads < 20000 && clock.ElapsedMilliseconds < 100; reads++)
                {
                    var read = story.ReadNext();
                    if (read.Status == StoryProcessor.StoryReadStatus.Completed)
                        return Ratio(decisions.Length, dialogues);
                    var parsed = parser.ParseStep(read.Source, read.Choices);
                    if (!parsed.IsSuccess) return null;
                    if (parsed.Step.Command is not StoryCommands.DialogueStoryCommand)
                        continue;

                    var choices = parsed.Step.Choices;
                    var decision = dialogues < decisions.Length ? decisions[dialogues]
                        : choices.Length > 0 ? StoryDecision.Choice(choices[0].Id)
                        : StoryDecision.Advance;
                    if (decision.HasChoice != (choices.Length > 0)) return null;
                    if (decision.HasChoice)
                    {
                        var found = false;
                        foreach (var choice in choices) found |= choice.Id == decision.ChoiceId;
                        if (!found) return null;
                        story.SetChoice(decision.ChoiceId);
                    }
                    dialogues++;
                    if (!string.IsNullOrWhiteSpace(endMarker)
                        && (read.Source ?? string.Empty).TrimStart().StartsWith(
                            endMarker, StringComparison.OrdinalIgnoreCase))
                        return Ratio(decisions.Length, dialogues);
                }
            }
            catch (Exception) { /* Invalid/unbounded content: unknown, not a fabricated percent. */ }
            return null;
        }

        private static float? Ratio(int read, int total) => total < read || total == 0
            ? null : Math.Min(.99f, (float)read / total);
    }
}
