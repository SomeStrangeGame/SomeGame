using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Novels.Catalog;
using Novels.Catalog.Contracts;
using Novels.Catalog.View;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

// Bounded live-catalog check. Uses a generated test-pattern MP4 in Build/Logs,
// restores all production models/scroll positions and never invokes reading/reset actions.
public static class CatalogVideoValidation
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static bool _running;
    private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private static RawImage Surface(Card card) => Field<RawImage>(card, "_video");

    [MenuItem("Novels/Validation/Check Catalog Media Priority")]
    public static void CheckMediaPriority()
    {
        var preview = new StoryCatalogPreview { episodes = new[] { new StoryCatalogEpisodePreview() } };
        for (var bits = 0; bits < 8; bits++)
        {
            var episodeVideo = (bits & 1) != 0;
            var episodeCover = (bits & 2) != 0;
            var storyVideo = (bits & 4) != 0;
            preview.episodes[0].video = episodeVideo ? "episode.mp4" : null;
            preview.video = storyVideo ? "story.mp4" : null;
            var actual = Novels.CatalogFlow.SelectCatalogVideo(preview, 0, episodeCover);
            var expected = episodeVideo ? "episode.mp4" : !episodeCover && storyVideo ? "story.mp4" : null;
            Require(actual == expected, $"Wrong media priority for combination {bits}: {actual}.");
        }
        preview.episodes[0].video = " \t";
        preview.video = "story.mp4";
        Require(Novels.CatalogFlow.SelectCatalogVideo(preview, 0, true) == null,
            "Own cover did not suppress inherited video when episode video is blank.");
        Require(Novels.CatalogFlow.SelectCatalogVideo(preview, 0, false) == "story.mp4",
            "Missing/failed episode cover did not allow story video fallback.");
        preview.video = " \t";
        Require(Novels.CatalogFlow.SelectCatalogVideo(preview, 0, false) == null,
            "Empty videos must keep the story image.");
        Debug.Log("CATALOG_MEDIA_PRIORITY PASS: all 8 combinations plus whitespace/missing-cover cases; episode video > episode image > story video > story image.");
    }

    [MenuItem("Novels/Validation/Check Catalog Video")]
    public static async void Check()
    {
        Require(Application.isPlaying && !_running, "Play Mode required; video check must run alone.");
        var file = Path.GetFullPath(Path.Combine(Application.dataPath, "../Build/Logs/catalog-video-fixture-20260908.mp4"));
        Require(File.Exists(file), "Generate the bounded H.264/AAC test-pattern fixture first.");
        _running = true;
        var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
        var scrolls = screen.GetComponentsInChildren<ScrollRect>();
        var positions = scrolls.Select(scroll => scroll.content.anchoredPosition).ToArray();
        var previousBackground = Application.runInBackground;
        var previousVolume = AudioListener.volume;
        var previousFocus = Field<bool>(screen, "_applicationFocused");
        var saved = new Dictionary<Card, CatalogEpisodeItem>();
        var temp = Path.Combine(Application.temporaryCachePath, "catalog-video-validation-" + Guid.NewGuid().ToString("N"));
        var popup = screen.GetComponent<CatalogSettingsPopup>();
        var popupWasOpen = popup != null && popup.IsOpen;
        try
        {
            CheckMediaPriority();
            Application.runInBackground = true;
            EditorApplication.update += Pump;
            popup?.Close();
            typeof(CatalogScreen).GetField("_applicationFocused", Private).SetValue(screen, true);
            ValidateExport(file, temp);
            var stories = Field<Dictionary<string, Card>>(screen, "_cards").Values.ToArray();
            Require(stories.Length >= 2, "Two stories required for both-axis playback check.");
            var first = Field<List<Card>>(stories[0], "_episodeCards");
            var second = Field<List<Card>>(stories[1], "_episodeCards")[0];
            foreach (var card in new[] { first[0], first[1], second })
            {
                var model = Field<CatalogEpisodeItem>(card, "_boundEpisode");
                saved.Add(card, model);
                Bind(card, Copy(model, new Uri(file).AbsoluteUri));
            }
            CatalogVisualValidation.ShowStart();
            await Until(() => Surface(first[0]).enabled);
            var player = screen.GetComponent<VideoPlayer>();
            var audio = screen.GetComponent<AudioSource>();
            Require(player != null && player.audioTrackCount == 1 && audio != null, "No decoded video/audio track.");
            Require(audio.volume < .15f, "Video sound did not start quietly.");
            Require(!Surface(first[1]).enabled && !Surface(second).enabled, "Offscreen/peek episode started video.");
            await Wait(.55f);
            Require(audio.volume > .1f && audio.volume < .95f, "Missing gradual volume fade.");
            await Wait(.8f);
            Require(audio.volume > .99f && player.isLooping, "Fade did not reach normal loop volume.");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath(Path.Combine(Application.dataPath,
                "../Build/Logs/catalog-video-fixture-visible-20260908.png")));
            AudioListener.volume = 0f;
            Require(!audio.ignoreListenerVolume && !audio.ignoreListenerPause, "Video bypasses application audio settings.");
            await Wait(.2f);
            AudioListener.volume = previousVolume;

            var horizontal = stories[0].GetComponentInChildren<ScrollRect>();
            horizontal.StopMovement(); horizontal.GetComponent<CatalogScrollSnap>()?.Cancel();
            var position = horizontal.content.anchoredPosition;
            position.x = -first[1].RectTransform.anchoredPosition.x + first[1].RectTransform.rect.width * first[1].RectTransform.pivot.x;
            horizontal.content.anchoredPosition = position;
            await Until(() => Surface(first[1]).enabled);
            Require(!Surface(first[0]).enabled && !Surface(second).enabled, "Horizontal switch left old video visible.");
            Require(screen.GetComponents<VideoPlayer>().Length == 1, "More than one decoder in catalog.");
            CatalogVisualValidation.ShowNextStory();
            await Until(() => Surface(second).enabled);
            Require(!Surface(first[1]).enabled, "Vertical switch left old video visible.");
            popup.Open();
            await Wait(.1f);
            Require(!player.isPlaying && audio.volume == 0f && !Surface(second).enabled, "Settings popup did not silence video.");
            popup.Close();
            await Until(() => Surface(second).enabled);
            screen.SendMessage("OnApplicationPause", true);
            Require(!player.isPlaying && audio.volume == 0f, "Application pause left audio playing.");
            screen.SendMessage("OnApplicationPause", false);
            await Until(() => Surface(second).enabled);
            screen.SendMessage("OnApplicationFocus", false);
            Require(!player.isPlaying && audio.volume == 0f, "Focus loss left audio playing.");
            screen.SendMessage("OnApplicationFocus", true);
            await Until(() => Surface(second).enabled);
            screen.enabled = false;
            Require(!player.isPlaying && audio.volume == 0f, "Disabled catalog left audio playing.");
            screen.enabled = true;
            screen.SendMessage("OnApplicationFocus", true);
            Bind(second, Copy(saved[second], new Uri(Path.Combine(temp, "missing.mp4")).AbsoluteUri));
            await Until(() => Field<bool>(Field<object>(screen, "_videoPlayback"), "_failed"));
            Require(!Surface(second).enabled && !player.isPlaying && audio.volume == 0f, "Bad video did not fall back to image/silence.");
            Bind(second, Copy(saved[second], ""));
            await Wait(.2f);
            Require(!Surface(second).enabled && !player.isPlaying, "Empty video did not keep image fallback.");
            Debug.Log("CATALOG_VIDEO PASS: actual H.264/AAC decode, authored surface, quiet start/fade, single player, horizontal/vertical visibility, peek excluded, settings/pause/focus/disable silence, missing/empty fallback, preview export and safe paths. Models/volume/scroll restored; saves untouched.");
        }
        catch (Exception exception) { Debug.LogException(exception); }
        finally
        {
            if (screen != null)
            {
                screen.enabled = true;
                screen.SendMessage("OnApplicationPause", false);
                screen.SendMessage("OnApplicationFocus", previousFocus);
                foreach (var pair in saved) if (pair.Key != null) Bind(pair.Key, pair.Value);
                for (var i = 0; i < scrolls.Length; i++)
                    if (scrolls[i] != null) { scrolls[i].StopMovement(); scrolls[i].content.anchoredPosition = positions[i]; }
                if (popupWasOpen) popup.Open();
            }
            AudioListener.volume = previousVolume;
            EditorApplication.update -= Pump;
            Application.runInBackground = previousBackground;
            if (Directory.Exists(temp)) Directory.Delete(temp, true); // Own unique fixture only.
            _running = false;
        }
    }

    private static CatalogEpisodeItem Copy(CatalogEpisodeItem model, string video) => new(model.Id, model.Title,
        model.Description, model.Status, model.ActionLabel, model.RestartLabel, model.RestartWarning,
        model.IsEnabled, model.Download, model.Cover, model.Author, videoUrl: video);

    private static void Bind(Card card, CatalogEpisodeItem model)
    {
        // Preserve existing open/reset callbacks by replacing only the media model used by selection.
        typeof(Card).GetField("_boundEpisode", Private).SetValue(card, model);
    }

    private static void ValidateExport(string videoFile, string root)
    {
        const string legacy = "{\"schemaVersion\":1,\"storyId\":\"fixture\",\"releaseId\":\"r1\",\"contentVersion\":\"1\",\"episodes\":[{\"id\":\"e1\",\"title\":\"First\"}]}";
        var preview = CatalogContractCodec.DeserializePreview(legacy, "fixture");
        Require(preview.video == null && preview.episodes[0].video == null, "Legacy preview lost compatibility.");
        foreach (var invalid in new[] { "../clip.mp4", "dir/clip.mp4", "a%2f.mp4", "https:clip.mp4", "clip.webm" })
        {
            preview.episodes[0].video = invalid;
            var rejected = false;
            try { CatalogContractCodec.DeserializePreview(JsonUtility.ToJson(preview), "fixture"); }
            catch (ArgumentException) { rejected = true; }
            Require(rejected, "Unsafe video name accepted: " + invalid);
        }
        var covers = Path.Combine(root, "Config/EpisodeCovers");
        var videos = Path.Combine(root, "Config/CatalogVideos");
        var output = Path.Combine(root, "output");
        Directory.CreateDirectory(covers); Directory.CreateDirectory(videos); Directory.CreateDirectory(output);
        File.Copy(videoFile, Path.Combine(videos, "story.mp4"));
        File.Copy(videoFile, Path.Combine(videos, "episode.mp4"));
        var definition = new Novels.Content.NovelDefinition("fixture", "maincharacter", "1", "", null,
            new[] {
                new Novels.Content.EpisodeDefinition("fixture", "e1", "First", "", catalogVideo: "episode.mp4"),
                new Novels.Content.EpisodeDefinition("fixture", "e2", "Second", ""),
                new Novels.Content.EpisodeDefinition("fixture", "e3", "Third", "", catalogVideo: "episode.mp4"),
            }, catalogVideo: "story.mp4");
        Type.GetType("Novels.ContentSdk.Editor.ContentPipeline, Novels.ContentSdk.Editor")
            .GetMethod("WriteStoryCatalogPreview", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, new object[] { definition, new Bundles.ContentReleaseDto { releaseId = "r1" }, output, covers });
        preview = CatalogContractCodec.DeserializePreview(File.ReadAllText(Path.Combine(output, "catalog-preview.json")), "fixture");
        Require(preview.video == "story.mp4" && preview.episodes[0].video == "episode.mp4"
            && string.IsNullOrEmpty(preview.episodes[1].video), "Story/episode video associations lost in export.");
        Require(Directory.GetFiles(Path.Combine(output, "catalog-videos")).Length == 2
            && File.ReadAllBytes(Path.Combine(output, "catalog-videos/episode.mp4")).SequenceEqual(File.ReadAllBytes(videoFile)),
            "Export did not copy/deduplicate videos intact.");
        Require(Novels.ContentAddressing.ContentPackageConvention.StoryCatalogVideoPath("fixture", "Mac", "story.mp4")
            == "stories/fixture/Remote/Mac/catalog-videos/story.mp4", "Wrong video address.");
    }

    private static void Pump() => EditorApplication.QueuePlayerLoopUpdate();
    private static async UniTask Wait(float seconds)
    {
        var until = Time.unscaledTime + seconds;
        while (Time.unscaledTime < until) await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
    }
    private static async UniTask Until(Func<bool> ready)
    {
        var until = Time.realtimeSinceStartup + 20f;
        while (!ready())
        {
            Require(Application.isPlaying && Time.realtimeSinceStartup < until, "Catalog video check timed out.");
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }
    }
}
