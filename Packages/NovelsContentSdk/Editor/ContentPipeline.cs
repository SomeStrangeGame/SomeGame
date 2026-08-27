using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bundles;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    internal static class ContentPipeline
    {
        private const int _contentSchemaVersion = 5;
        private const string _outputPath = "Build/LocalContent";
        private const string _stagingPath = "Build/AtomicContentStaging";

        internal static void Validate() => ContentValidator.Validate();

        internal static void Build(string platform)
        {
            var plan = ContentValidator.Validate();
            var target = ContentPlatform.Resolve(platform);
            var streamingExperiment = plan.Kind == ContentProjectKind.Story
                && string.Equals(
                    Environment.GetEnvironmentVariable("NOVELS_STREAMING_EXPERIMENT"),
                    "1",
                    StringComparison.Ordinal);
            Directory.CreateDirectory(_outputPath);
            RecreateDirectory(_stagingPath);
            RecreateDirectory(Path.Combine(
                _outputPath,
                "Remote",
                ContentPlatform.Name(target)));
            try
            {
                var files = BuildFilePayloads(plan, streamingExperiment);
                BuildTargetRelease(plan, files, target, streamingExperiment);
                Debug.Log(
                    $"Atomic content '{plan.DeliveryGroup}' built for "
                    + $"{platform} to {Path.GetFullPath(_outputPath)}");
            }
            finally
            {
                if (Directory.Exists(_stagingPath))
                    Directory.Delete(_stagingPath, true);
                AssetDatabase.Refresh();
            }
        }

        private static ContentFileEntry[] BuildFilePayloads(
            ContentBuildPlan plan,
            bool streamingExperiment)
        {
            var result = new List<ContentFileEntry>();
            foreach (var file in ContentAssets.FindContentFiles(plan))
            {
                var source = file.SourcePath;
                var relative = file.ContentPath;
                if (!ShouldPublishStreamingAsset(relative))
                    continue;
                var hash = ContentHash.ComputeSha256(source);
                var payloadPath = ContentAddressing.ContentPackageConvention
                    .ContentPayload(hash);
                var destination = Path.Combine(
                    _outputPath,
                    payloadPath.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                if (!File.Exists(destination))
                    File.Copy(source, destination);
                result.Add(new ContentFileEntry
                {
                    path = relative,
                    payloadPath = payloadPath,
                    size = new FileInfo(source).Length,
                    sha256 = hash,
                    deliveryGroup = DeliveryGroupForFile(
                        plan,
                        relative,
                        streamingExperiment),
                });
            }
            return result.ToArray();
        }

        private static bool ShouldPublishStreamingAsset(string relativePath)
        {
            if (!relativePath.StartsWith(
                    "noveltexts/",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Authoring sources stay in the project for build-time analysis.
            // Runtime needs only compiled Ink and its analytics source map.
            return relativePath.EndsWith(
                    ".ink.json",
                    StringComparison.OrdinalIgnoreCase)
                || relativePath.EndsWith(
                    ".source-map.json",
                    StringComparison.OrdinalIgnoreCase);
        }

        private static string DeliveryGroupForFile(
            ContentBuildPlan plan,
            string relativePath,
            bool streamingExperiment)
        {
            if (!streamingExperiment)
                return plan.DeliveryGroup;
            if (relativePath.StartsWith("noveltexts/", StringComparison.OrdinalIgnoreCase))
            {
                return ContentAddressing.ContentPackageConvention
                    .StoryChunkDeliveryGroup(plan.DeliveryGroup, 0);
            }
            if (relativePath.StartsWith("novelsvideos/", StringComparison.OrdinalIgnoreCase)
                || relativePath.StartsWith("novelsaudio/", StringComparison.OrdinalIgnoreCase))
            {
                return ContentAddressing.ContentPackageConvention
                    .StoryMediaDeliveryGroup(plan.DeliveryGroup);
            }
            return plan.DeliveryGroup;
        }

        private static void BuildTargetRelease(
            ContentBuildPlan plan,
            ContentFileEntry[] files,
            BuildTarget target,
            bool streamingExperiment)
        {
            var platform = ContentPlatform.Name(target);
            var staging = Path.Combine(_stagingPath, platform);
            Directory.CreateDirectory(staging);
            var assets = ContentAssets.FindBundleAssets();
            if (streamingExperiment)
            {
                BuildStreamingTargetRelease(plan, files, target, platform, staging, assets);
                return;
            }
            var build = ContentAssets.BundleBuild(plan, plan.BundleName, assets);
            var manifest = BuildPipeline.BuildAssetBundles(
                    staging,
                    new[] {build},
                    BuildAssetBundleOptions.None,
                    target)
                ?? throw new InvalidOperationException(
                    $"AssetBundle build failed for {target}.");
            var source = Path.Combine(staging, plan.BundleName);
            var version = manifest.GetAssetBundleHash(plan.BundleName).ToString();
            if (!BuildPipeline.GetCRCForAssetBundle(source, out var crc))
                throw new InvalidOperationException(
                    "AssetBundle CRC cannot be calculated.");
            var destinationDirectory = Path.Combine(
                _outputPath,
                "Remote",
                platform,
                plan.BundleName);
            Directory.CreateDirectory(destinationDirectory);
            var destination = Path.Combine(destinationDirectory, version);
            File.Copy(source, destination, true);
            var bundle = new BundleReleaseEntry
            {
                name = plan.BundleName,
                version = version,
                size = new FileInfo(destination).Length,
                sha256 = ContentHash.ComputeSha256(destination),
                crc = crc,
                deliveryGroup = plan.DeliveryGroup,
            };
            ContentBundleAudit.Audit(
                plan,
                build.assetNames,
                destination,
                bundle.size);
            var release = CreateRelease(plan, files, new[] {bundle});
            var releasePath = Path.Combine(
                _outputPath,
                "Remote",
                platform,
                "release.json");
            File.WriteAllText(
                releasePath,
                ContentReleaseCodec.Serialize(release),
                new UTF8Encoding(false));
        }

        private static void BuildStreamingTargetRelease(
            ContentBuildPlan plan,
            ContentFileEntry[] files,
            BuildTarget target,
            string platform,
            string staging,
            string[] assets)
        {
            var streaming = ExperimentalStreamingPlan.Create(
                plan.DeliveryGroup,
                assets,
                files.Select(value => value.path).ToArray());
            var mediaGroups = streaming.Media.ToDictionary(
                value => value.path,
                value => value.deliveryGroup,
                StringComparer.OrdinalIgnoreCase);
            foreach (var file in files)
            {
                if (mediaGroups.TryGetValue(file.path, out var group))
                    file.deliveryGroup = group;
            }

            var chunkBuilds = streaming.Chunks
                .Select((chunkAssets, index) => ContentAssets.BundleBuild(
                    plan,
                    ContentAddressing.ContentPackageConvention
                        .StoryChunkBundle(plan.DeliveryGroup, index),
                    chunkAssets))
                .ToArray();
            var manifest = BuildPipeline.BuildAssetBundles(
                    staging,
                    chunkBuilds,
                    BuildAssetBundleOptions.None,
                    target)
                ?? throw new InvalidOperationException(
                    $"Streaming AssetBundle build failed for {target}.");
            var bundles = new List<BundleReleaseEntry>();
            var chunks = new ContentStreamingChunkEntry[chunkBuilds.Length];
            for (var index = 0; index < chunkBuilds.Length; index++)
            {
                var build = chunkBuilds[index];
                var group = ContentAddressing.ContentPackageConvention
                    .StoryChunkDeliveryGroup(plan.DeliveryGroup, index);
                bundles.Add(CopyBuiltBundle(
                    plan,
                    manifest,
                    staging,
                    platform,
                    build,
                    group));
                chunks[index] = new ContentStreamingChunkEntry
                {
                    index = index,
                    bundle = build.assetBundleName,
                    deliveryGroup = group,
                    assets = build.addressableNames ?? build.assetNames,
                };
            }
            var release = CreateRelease(
                plan,
                files,
                bundles.ToArray(),
                new ContentStreamingPlanEntry
                {
                    chunks = chunks,
                    media = streaming.Media.ToArray(),
                });
            var releasePath = Path.Combine(
                _outputPath,
                "Remote",
                platform,
                "release.json");
            File.WriteAllText(
                releasePath,
                ContentReleaseCodec.Serialize(release),
                new UTF8Encoding(false));
        }

        private static BundleReleaseEntry CopyBuiltBundle(
            ContentBuildPlan plan,
            AssetBundleManifest manifest,
            string staging,
            string platform,
            AssetBundleBuild build,
            string deliveryGroup)
        {
            var source = Path.Combine(staging, build.assetBundleName);
            var version = manifest.GetAssetBundleHash(build.assetBundleName).ToString();
            if (!BuildPipeline.GetCRCForAssetBundle(source, out var crc))
                throw new InvalidOperationException(
                    $"AssetBundle CRC cannot be calculated for '{build.assetBundleName}'.");
            var directory = Path.Combine(
                _outputPath,
                "Remote",
                platform,
                build.assetBundleName);
            Directory.CreateDirectory(directory);
            var destination = Path.Combine(directory, version);
            File.Copy(source, destination, true);
            var result = new BundleReleaseEntry
            {
                name = build.assetBundleName,
                version = version,
                size = new FileInfo(destination).Length,
                sha256 = ContentHash.ComputeSha256(destination),
                crc = crc,
                deliveryGroup = deliveryGroup,
            };
            ContentBundleAudit.Audit(
                plan,
                build.assetNames,
                destination,
                result.size);
            return result;
        }

        private static ContentReleaseDto CreateRelease(
            ContentBuildPlan plan,
            ContentFileEntry[] files,
            BundleReleaseEntry[] bundles,
            ContentStreamingPlanEntry streamingPlan = null)
        {
            var release = new ContentReleaseDto
            {
                minimumClientVersion = plan.MinimumClientVersion,
                contentSchemaVersion = _contentSchemaVersion,
                deliveryMode = ContentDeliveryMode.Remote,
                bundles = bundles,
                files = files,
                streamingPlan = streamingPlan,
                deliveryGroups = bundles
                    .Select(value => value.deliveryGroup)
                    .Concat(files.Select(value => value.deliveryGroup))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(group => new ContentDeliveryGroupEntry
                    {
                        id = group,
                        payloadCount = bundles.Count(value => string.Equals(
                                value.deliveryGroup, group, StringComparison.OrdinalIgnoreCase))
                            + files.Count(value => string.Equals(
                                value.deliveryGroup, group, StringComparison.OrdinalIgnoreCase)),
                        size = bundles.Where(value => string.Equals(
                                value.deliveryGroup, group, StringComparison.OrdinalIgnoreCase))
                            .Sum(value => value.size)
                            + files.Where(value => string.Equals(
                                value.deliveryGroup, group, StringComparison.OrdinalIgnoreCase))
                            .Sum(value => value.size),
                    })
                    .ToArray(),
            };
            release.releaseId = ContentReleaseFingerprint.Compute(release);
            ContentReleaseValidator.Validate(
                release,
                plan.MinimumClientVersion,
                _contentSchemaVersion,
                _contentSchemaVersion);
            return release;
        }

        private static void RecreateDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
    }

    internal static class ContentPlatform
    {
        internal static BuildTarget Resolve(string value) =>
            value?.ToLowerInvariant() switch
            {
                "editor" => BuildTarget.StandaloneOSX,
                "android" => BuildTarget.Android,
                "ios" => BuildTarget.iOS,
                _ => throw new ArgumentException(
                    $"Unknown content platform '{value}'. "
                    + "Use editor, android or ios."),
            };

        internal static string Name(BuildTarget target) => target switch
        {
            BuildTarget.StandaloneOSX => "Mac",
            BuildTarget.Android => "Android",
            BuildTarget.iOS => "iOS",
            _ => throw new NotSupportedException(
                $"Unsupported content target: {target}"),
        };
    }

    internal static class ContentAssets
    {
        private const string _legacyBundleRoot = "Assets/RemoteAssets";
        private const string _simpleInkRoot = "Assets/Ink";
        private const string _simpleVideoRoot = "Assets/Video";
        private const string _simpleAudioRoot = "Assets/Audio";
        private static readonly string[] _simpleBundleRoots =
        {
            "Assets/Characters",
            "Assets/Locations",
            "Assets/Choices",
            "Assets/Presentation",
        };

        internal sealed class ContentFileSource
        {
            internal ContentFileSource(string sourcePath, string contentPath)
            {
                SourcePath = sourcePath;
                ContentPath = contentPath;
            }

            internal string SourcePath { get; }
            internal string ContentPath { get; }
        }

        internal static string[] FindBundleAssets()
        {
            if (!UsesSimpleStoryLayout())
                return FindAssets(new[] {_legacyBundleRoot});

            var roots = _simpleBundleRoots
                .Where(Directory.Exists)
                .ToArray();
            return FindAssets(roots)
                .Concat(AssetDatabase.FindAssets("t:NovelContentAsset", new[] {"Assets"})
                    .Select(AssetDatabase.GUIDToAssetPath))
                .Where(path => !string.IsNullOrEmpty(path))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
        }

        internal static ContentFileSource[] FindContentFiles(ContentBuildPlan plan) =>
            plan.Kind == ContentProjectKind.Story && UsesSimpleStoryLayout()
                ? FindSimpleContentFiles(plan.DeliveryGroup)
                : FindLegacyContentFiles();

        internal static ContentFileSource[] FindContentFiles(string storyId) =>
            UsesSimpleStoryLayout()
                ? FindSimpleContentFiles(storyId)
                : FindLegacyContentFiles();

        internal static AssetBundleBuild BundleBuild(
            ContentBuildPlan plan,
            string bundleName,
            string[] assets) =>
            new()
            {
                assetBundleName = bundleName,
                assetNames = assets,
                addressableNames = plan.Kind == ContentProjectKind.Story
                    && UsesSimpleStoryLayout()
                    ? assets.Select(path => BundleAddress(plan.DeliveryGroup, path)).ToArray()
                    : null,
            };

        internal static bool IsBundleSource(string path) =>
            path.StartsWith(_legacyBundleRoot + "/", StringComparison.Ordinal)
            || UsesSimpleStoryLayout()
            && (path.StartsWith("Assets/", StringComparison.Ordinal)
                && (path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)
                    || _simpleBundleRoots.Any(root => path.StartsWith(
                        root + "/",
                        StringComparison.Ordinal))));

        internal static string BundleAddress(string storyId, string sourcePath)
        {
            if (sourcePath.StartsWith(_legacyBundleRoot + "/", StringComparison.Ordinal))
                return sourcePath;
            if (string.Equals(
                    sourcePath,
                    $"Assets/{storyId}.asset",
                    StringComparison.OrdinalIgnoreCase))
            {
                return ContentAddressing.ContentPackageConvention.DefinitionAsset(storyId);
            }
            if (sourcePath.StartsWith("Assets/Characters/", StringComparison.Ordinal))
            {
                var relative = sourcePath.Substring("Assets/Characters/".Length);
                return string.Equals(
                        relative,
                        "sprite-trim-manifest.asset",
                        StringComparison.OrdinalIgnoreCase)
                    ? ContentAddressing.ContentAddressConvention
                        .CharacterSpriteTrimManifest(storyId)
                    : $"{ContentAddressing.ContentPackageConvention.StoryRoot(storyId)}"
                      + $"/character/characters/{relative}";
            }
            if (sourcePath.StartsWith("Assets/Locations/", StringComparison.Ordinal))
            {
                return $"{ContentAddressing.ContentPackageConvention.StoryRoot(storyId)}"
                       + "/location/locations/"
                       + sourcePath.Substring("Assets/Locations/".Length);
            }
            if (sourcePath.StartsWith("Assets/Choices/", StringComparison.Ordinal))
            {
                return $"{ContentAddressing.ContentPackageConvention.StoryRoot(storyId)}"
                       + "/choose/items/"
                       + sourcePath.Substring("Assets/Choices/".Length);
            }
            if (sourcePath.StartsWith("Assets/Presentation/setting/", StringComparison.Ordinal))
            {
                return $"{ContentAddressing.ContentPackageConvention.ContentRoot(storyId)}"
                       + "/application/setting/"
                       + sourcePath.Substring("Assets/Presentation/setting/".Length);
            }
            if (sourcePath.StartsWith("Assets/Presentation/", StringComparison.Ordinal))
            {
                return $"{ContentAddressing.ContentPackageConvention.StoryRoot(storyId)}"
                       + "/presentation/"
                       + sourcePath.Substring("Assets/Presentation/".Length);
            }
            throw new InvalidOperationException(
                $"Story bundle asset is outside the supported layout: {sourcePath}");
        }

        internal static string InkDirectory(string storyId) =>
            UsesSimpleStoryLayout()
                ? Absolute(_simpleInkRoot)
                : Path.Combine(
                    Application.streamingAssetsPath,
                    "noveltexts",
                    storyId);

        internal static string InkPath(string storyId, string fileName) =>
            Path.Combine(
                InkDirectory(storyId),
                fileName.Replace('/', Path.DirectorySeparatorChar));

        internal static string SourcePath(string contentPath)
        {
            if (!UsesSimpleStoryLayout())
            {
                return Path.Combine(
                    Application.streamingAssetsPath,
                    contentPath.Replace('/', Path.DirectorySeparatorChar));
            }
            var parts = contentPath.Split('/');
            if (parts.Length < 3)
                return string.Empty;
            var root = parts[0].ToLowerInvariant() switch
            {
                "noveltexts" => _simpleInkRoot,
                "novelsvideos" => _simpleVideoRoot,
                "novelsaudio" => _simpleAudioRoot,
                _ => string.Empty,
            };
            return string.IsNullOrEmpty(root)
                ? string.Empty
                : Absolute(root + "/" + string.Join("/", parts.Skip(2)));
        }

        internal static string UnityAssetPath(string contentPath)
        {
            if (contentPath.StartsWith("Assets/", StringComparison.Ordinal))
                return contentPath;
            if (!UsesSimpleStoryLayout())
                return "Assets/StreamingAssets/" + contentPath;
            var absolute = SourcePath(contentPath).Replace('\\', '/');
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?.Replace('\\', '/');
            return !string.IsNullOrEmpty(projectRoot)
                   && absolute.StartsWith(projectRoot + "/", StringComparison.OrdinalIgnoreCase)
                ? absolute.Substring(projectRoot.Length + 1)
                : string.Empty;
        }

        internal static string ContentPath(string assetPath)
        {
            if (IsBundleSource(assetPath))
                return assetPath;
            const string legacyStreamingRoot = "Assets/StreamingAssets/";
            if (assetPath.StartsWith(
                    legacyStreamingRoot,
                    StringComparison.OrdinalIgnoreCase))
            {
                var relative = assetPath.Substring(legacyStreamingRoot.Length);
                if (IsMedia(relative))
                    return relative;
            }
            if (UsesSimpleStoryLayout())
            {
                var storyId = CurrentStoryId();
                if (assetPath.StartsWith(_simpleVideoRoot + "/", StringComparison.Ordinal))
                {
                    return $"novelsvideos/{storyId}/"
                           + assetPath.Substring((_simpleVideoRoot + "/").Length);
                }
                if (assetPath.StartsWith(_simpleAudioRoot + "/", StringComparison.Ordinal))
                {
                    return $"novelsaudio/{storyId}/"
                           + assetPath.Substring((_simpleAudioRoot + "/").Length);
                }
            }
            throw new InvalidOperationException(
                "В чанк можно добавить только Unity-ассет истории или её Video/Audio: "
                + assetPath);
        }

        private static string[] FindAssets(string[] roots) =>
            roots.Length == 0
                ? Array.Empty<string>()
                : AssetDatabase.FindAssets(string.Empty, roots)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !AssetDatabase.IsValidFolder(path))
                .Where(path => !path.EndsWith(
                    ".cs",
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

        private static ContentFileSource[] FindSimpleContentFiles(string storyId) =>
            new[]
                {
                    (Root: _simpleInkRoot, Prefix: "noveltexts"),
                    (Root: _simpleVideoRoot, Prefix: "novelsvideos"),
                    (Root: _simpleAudioRoot, Prefix: "novelsaudio"),
                }
                .Where(value => Directory.Exists(value.Root))
                .SelectMany(value => Directory.EnumerateFiles(
                        value.Root,
                        "*",
                        SearchOption.AllDirectories)
                    .Where(path => !path.EndsWith(
                        ".meta",
                        StringComparison.OrdinalIgnoreCase))
                    .Select(path => new ContentFileSource(
                        Path.GetFullPath(path),
                        $"{value.Prefix}/{storyId}/"
                        + Path.GetRelativePath(value.Root, path).Replace('\\', '/'))))
                .OrderBy(value => value.ContentPath, StringComparer.Ordinal)
                .ToArray();

        private static ContentFileSource[] FindLegacyContentFiles()
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
                return Array.Empty<ContentFileSource>();
            return Directory.EnumerateFiles(
                    Application.streamingAssetsPath,
                    "*",
                    SearchOption.AllDirectories)
                .Where(path => !path.EndsWith(
                    ".meta",
                    StringComparison.OrdinalIgnoreCase))
                .Select(path => new ContentFileSource(
                    path,
                    Path.GetRelativePath(Application.streamingAssetsPath, path)
                        .Replace('\\', '/')))
                .OrderBy(value => value.ContentPath, StringComparer.Ordinal)
                .ToArray();
        }

        private static bool UsesSimpleStoryLayout() =>
            Directory.Exists(_simpleInkRoot);

        private static bool IsMedia(string path) =>
            path.StartsWith("novelsvideos/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("novelsaudio/", StringComparison.OrdinalIgnoreCase);

        private static string CurrentStoryId()
        {
            var definitions = AssetDatabase.FindAssets("t:NovelContentAsset", new[] {"Assets"})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Content.NovelContentAsset>)
                .Where(value => value != null)
                .ToArray();
            if (definitions.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected one story definition, found {definitions.Length}.");
            }
            return definitions[0].ToDefinition().Id;
        }

        private static string Absolute(string assetPath) =>
            Path.Combine(
                Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project root is unavailable."),
                assetPath.Replace('/', Path.DirectorySeparatorChar));
    }
}
