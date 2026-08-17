using System;
using System.Collections.Generic;
using System.Linq;

namespace Editor
{
    internal sealed class StoryDependencyManifest
    {
        internal StoryDependencyManifest(
            IEnumerable<string> audioIds,
            IEnumerable<string> backgrounds,
            IEnumerable<string> speakers,
            IEnumerable<Novels.StoryContracts.StoryCameraAction> cameraActions,
            IEnumerable<ContentValidationIssue> issues)
        {
            AudioIds = Distinct(audioIds);
            Backgrounds = Distinct(backgrounds);
            Speakers = Distinct(speakers);
            CameraActions = Array.AsReadOnly(cameraActions.Distinct().ToArray());
            Issues = Array.AsReadOnly(issues.ToArray());
        }

        internal IReadOnlyList<string> AudioIds { get; }
        internal IReadOnlyList<string> Backgrounds { get; }
        internal IReadOnlyList<string> Speakers { get; }
        internal IReadOnlyList<Novels.StoryContracts.StoryCameraAction> CameraActions { get; }
        internal IReadOnlyList<ContentValidationIssue> Issues { get; }

        private static IReadOnlyList<string> Distinct(IEnumerable<string> values) =>
            Array.AsReadOnly(values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }
}
