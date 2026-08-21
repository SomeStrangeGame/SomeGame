using System;
using System.Collections.Generic;
using System.Linq;

namespace Editor
{
    internal enum StoryDependencyKind
    {
        Audio,
        Background,
        Speaker,
    }

    internal sealed class StoryDependencyReference
    {
        internal StoryDependencyReference(
            StoryDependencyKind kind,
            string id,
            string sourcePath,
            int lineNumber,
            string sourceText)
        {
            Kind = kind;
            Id = id ?? string.Empty;
            SourcePath = sourcePath ?? string.Empty;
            LineNumber = lineNumber;
            SourceText = sourceText ?? string.Empty;
        }

        internal StoryDependencyKind Kind { get; }
        internal string Id { get; }
        internal string SourcePath { get; }
        internal int LineNumber { get; }
        internal string SourceText { get; }
        internal string Location => $"{SourcePath}:{LineNumber}";
    }

    internal sealed class StoryCameraReference
    {
        internal StoryCameraReference(
            Novels.StoryContracts.StoryCameraAction action,
            string sourcePath,
            int lineNumber,
            string sourceText)
        {
            Action = action;
            SourcePath = sourcePath ?? string.Empty;
            LineNumber = lineNumber;
            SourceText = sourceText ?? string.Empty;
        }

        internal Novels.StoryContracts.StoryCameraAction Action { get; }
        internal string SourcePath { get; }
        internal int LineNumber { get; }
        internal string SourceText { get; }
        internal string Location => $"{SourcePath}:{LineNumber}";
    }

    internal sealed class StoryCharacterAssetReference
    {
        internal StoryCharacterAssetReference(
            string speaker,
            Novels.StoryContracts.StorySpeakerRole role,
            string candidate,
            bool isChild,
            string sourcePath,
            int lineNumber,
            string sourceText)
        {
            Speaker = speaker ?? string.Empty;
            Role = role;
            Candidate = candidate ?? string.Empty;
            IsChild = isChild;
            SourcePath = sourcePath ?? string.Empty;
            LineNumber = lineNumber;
            SourceText = sourceText ?? string.Empty;
        }

        internal string Speaker { get; }
        internal Novels.StoryContracts.StorySpeakerRole Role { get; }
        internal string Candidate { get; }
        internal bool IsChild { get; }
        internal string SourcePath { get; }
        internal int LineNumber { get; }
        internal string SourceText { get; }
        internal string Location => $"{SourcePath}:{LineNumber}";
    }

    internal sealed class StoryDependencyManifest
    {
        internal StoryDependencyManifest(
            IEnumerable<StoryDependencyReference> dependencies,
            IEnumerable<StoryCameraReference> cameras,
            IEnumerable<StoryCharacterAssetReference> characterAssets,
            IEnumerable<ContentValidationIssue> issues)
        {
            var values = (dependencies ?? Array.Empty<StoryDependencyReference>())
                .Where(value => value != null && !string.IsNullOrWhiteSpace(value.Id))
                .ToArray();
            AudioReferences = Filter(values, StoryDependencyKind.Audio);
            BackgroundReferences = Filter(values, StoryDependencyKind.Background);
            SpeakerReferences = Filter(values, StoryDependencyKind.Speaker);
            CameraReferences = Array.AsReadOnly(
                (cameras ?? Array.Empty<StoryCameraReference>())
                    .Where(value => value != null)
                    .ToArray());
            CharacterAssetReferences = Array.AsReadOnly(
                (characterAssets ?? Array.Empty<StoryCharacterAssetReference>())
                    .Where(value => value != null
                        && !string.IsNullOrWhiteSpace(value.Speaker)
                        && (value.IsChild
                            || !string.IsNullOrWhiteSpace(value.Candidate)))
                    .ToArray());
            Issues = Array.AsReadOnly(
                (issues ?? Array.Empty<ContentValidationIssue>()).ToArray());
        }

        internal IReadOnlyList<StoryDependencyReference> AudioReferences { get; }
        internal IReadOnlyList<StoryDependencyReference> BackgroundReferences { get; }
        internal IReadOnlyList<StoryDependencyReference> SpeakerReferences { get; }
        internal IReadOnlyList<StoryCameraReference> CameraReferences { get; }
        internal IReadOnlyList<StoryCharacterAssetReference> CharacterAssetReferences { get; }
        internal IReadOnlyList<ContentValidationIssue> Issues { get; }

        private static IReadOnlyList<StoryDependencyReference> Filter(
            IEnumerable<StoryDependencyReference> values,
            StoryDependencyKind kind) =>
            Array.AsReadOnly(values.Where(value => value.Kind == kind).ToArray());
    }
}
