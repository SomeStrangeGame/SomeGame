using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Novels.Catalog.View;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Uses the actual loaded bundle. Destructive gestures run only on a detached clone
// configured with a counter callback, never on the user's episode/save handlers.
public static class CatalogVisualValidation
{
    private static bool _checkingDownloads;
    private static bool _checkingCovers;

    [MenuItem("Novels/Validation/Check Catalog Authors")]
    public static void CheckCatalogAuthors()
    {
        Require(Application.isPlaying, "Play Mode required.");
        var oldCard = Novels.Catalog.Contracts.CatalogContractCodec.DeserializeCard(
            "{\"schemaVersion\":2,\"storyId\":\"fixture\",\"title\":\"Original title\",\"genre\":\"Fixture\",\"cover\":\"cover.png\"}", "fixture");
        Require(string.IsNullOrWhiteSpace(oldCard.author) && oldCard.title == "Original title", "Legacy title/author compatibility failed.");
        oldCard.author = " Story Author ";
        var cardData = Novels.Catalog.Contracts.CatalogContractCodec.DeserializeCard(JsonUtility.ToJson(oldCard), "fixture");
        Require(cardData.author == "Story Author" && cardData.title == "Original title", "Author metadata changed title or did not normalize.");
        var inherited = new Novels.Catalog.CatalogEpisodeItem("1", "Unchanged episode title", storyAuthor: cardData.author);
        var own = new Novels.Catalog.CatalogEpisodeItem("2", "Unchanged episode title", author: " Episode Author ", storyAuthor: cardData.author);
        var unsigned = new Novels.Catalog.CatalogEpisodeItem("3", "Unchanged episode title", author: " ", storyAuthor: "\t");
        Require(inherited.Author == "Story Author" && own.Author == "Episode Author" && unsigned.Author == "", "Author override/fallback/blank rules failed.");
        var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
        var originalBylines = screen.GetComponentsInChildren<Text>(true).Where(label => label.name == "Author")
            .Select(label => (label, text: label.text, active: label.gameObject.activeSelf)).ToArray();
        var template = screen.GetComponentsInChildren<Button>().First(button => button.name == "Primary Action").GetComponentInParent<Card>();
        var fixture = UnityEngine.Object.Instantiate(template, screen.transform);
        try
        {
            fixture.gameObject.SetActive(false);
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var bind = typeof(Card).GetMethod("BindEpisode", flags);
            var author = (Text)typeof(Card).GetField("_author", flags).GetValue(fixture);
            Require(author != null, "Authored byline reference is missing.");
            foreach (var model in new[] { inherited, own, unsigned, own })
            {
                bind.Invoke(fixture, new object[] { null, model, null, null });
                Require(author.text == model.Author && author.gameObject.activeSelf == !string.IsNullOrWhiteSpace(model.Author), "Byline rebind did not clear/show correctly.");
                Require(((Text)typeof(Card).GetField("_title", flags).GetValue(fixture)).text == model.Title, "Byline replaced episode title.");
            }
            bind.Invoke(fixture, new object[] { null, new Novels.Catalog.CatalogEpisodeItem("4", "Title", author: "Очень длинный авторский псевдоним"), null, null });
            Require(!author.supportRichText && !author.raycastTarget && author.resizeTextForBestFit, "Byline must be plain non-interactive fitted text.");
            var restart = (Button)typeof(Card).GetField("_restartButton", flags).GetValue(fixture);
            Require(author.rectTransform.offsetMax.y < ((RectTransform)restart.transform).offsetMin.y,
                "Author overlaps the restart control.");
            Require(originalBylines.All(value => value.label.text == value.text && value.label.gameObject.activeSelf == value.active),
                "Author validation modified a production byline.");
            Debug.Log("CATALOG_AUTHORS PASS: legacy metadata/title preservation, story fallback, episode override, empty/rebind, authored plain label and restart separation; production data untouched.");
        }
        finally { UnityEngine.Object.DestroyImmediate(fixture.gameObject); }
    }

    [MenuItem("Novels/Validation/Check Episode Covers")]
    public static async void CheckEpisodeCovers()
    {
        Require(Application.isPlaying && !_checkingCovers, "Play Mode required; cover check already running.");
        _checkingCovers = true;
        using var frames = new ValidationFrames();
        var root = Path.Combine(Application.temporaryCachePath, "catalog-cover-validation-" + Guid.NewGuid().ToString("N"));
        Texture2D texture = null;
        Sprite storyCover = null, episodeCover = null;
        Card fixture = null;
        try
        {
            const string oldJson = "{\"schemaVersion\":1,\"storyId\":\"fixture\",\"releaseId\":\"r1\",\"contentVersion\":\"1\",\"episodes\":[{\"id\":\"s01e01\",\"title\":\"Episode\"}]}";
            var preview = Novels.Catalog.Contracts.CatalogContractCodec.DeserializePreview(oldJson, "fixture");
            Require(string.IsNullOrEmpty(preview.episodes[0].cover), "Old preview lost fallback compatibility.");
            preview.episodes[0].cover = "episode.png";
            var roundTrip = Novels.Catalog.Contracts.CatalogContractCodec.DeserializePreview(JsonUtility.ToJson(preview), "fixture");
            Require(roundTrip.episodes[0].cover == "episode.png", "Episode cover did not round-trip.");
            foreach (var invalid in new[] { "../cover.png", "/cover.png", "folder/cover.png", "https:cover.png", "a%2f.png", "a?.png", "file.txt" })
            {
                preview.episodes[0].cover = invalid;
                var rejected = false;
                try { Novels.Catalog.Contracts.CatalogContractCodec.DeserializePreview(JsonUtility.ToJson(preview), "fixture"); }
                catch (ArgumentException) { rejected = true; }
                Require(rejected, "Unsafe cover was accepted: " + invalid);
            }
            var path = Novels.ContentAddressing.ContentPackageConvention.StoryEpisodeCoverPath("fixture", "Mac", "episode.png");
            Require(path == "stories/fixture/Remote/Mac/episode-covers/episode.png", "Wrong platform cover address.");
            var file = Path.Combine(root, path);
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            texture = new Texture2D(16, 32, TextureFormat.RGBA32, false);
            texture.SetPixels(Enumerable.Repeat(Color.cyan, 16 * 32).ToArray());
            texture.Apply();
            var authoringCovers = Path.Combine(root, "Config", "EpisodeCovers");
            Directory.CreateDirectory(authoringCovers);
            File.WriteAllBytes(Path.Combine(authoringCovers, "episode.png"), texture.EncodeToPNG());
            var definition = new Novels.Content.NovelDefinition("fixture", "maincharacter", "1", "", null,
                new[] {
                    new Novels.Content.EpisodeDefinition("fixture", "s01e01", "Own", "", "episode.png", "Episode Author"),
                    new Novels.Content.EpisodeDefinition("fixture", "s01e02", "Fallback", ""),
                    new Novels.Content.EpisodeDefinition("fixture", "s01e03", "Shared image", "", "episode.png"),
                });
            var exporter = Type.GetType("Novels.ContentSdk.Editor.ContentPipeline, Novels.ContentSdk.Editor")
                .GetMethod("WriteStoryCatalogPreview", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var platformDirectory = Path.GetDirectoryName(Path.GetDirectoryName(file));
            exporter.Invoke(null, new object[] { definition, new Bundles.ContentReleaseDto { releaseId = "r1" }, platformDirectory, authoringCovers });
            var exported = Novels.Catalog.Contracts.CatalogContractCodec.DeserializePreview(
                File.ReadAllText(Path.Combine(platformDirectory, "catalog-preview.json")), "fixture");
            Require(exported.episodes[0].cover == "episode.png" && string.IsNullOrEmpty(exported.episodes[1].cover)
                && exported.episodes[2].cover == "episode.png" && exported.releaseId == "r1",
                "Exporter lost cover/fallback associations or release pin.");
            Require(exported.episodes[0].author == "Episode Author" && string.IsNullOrEmpty(exported.episodes[1].author),
                "Exporter lost optional episode authors.");
            Require(File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(Path.Combine(authoringCovers, "episode.png")))
                && Directory.GetFiles(Path.GetDirectoryName(file)).Length == 1, "Exporter did not preserve/deduplicate the cover file.");
            storyCover = Sprite.Create(texture, new Rect(0, 0, 16, 32), new Vector2(.5f, .5f));
            var source = new Bundles.FileSystemContentSource(root, CancellationToken.None);
            var warnings = 0;
            episodeCover = await Novels.CatalogFlow.LoadOptionalEpisodeCover(path, source, CancellationToken.None, _ => warnings++);
            Require(episodeCover != null && episodeCover.texture.width == 16 && warnings == 0,
                "Valid episode art did not load independently of story bundles.");
            var missing = await Novels.CatalogFlow.LoadOptionalEpisodeCover(path + ".missing", source, CancellationToken.None, _ => warnings++);
            File.WriteAllText(file + ".broken", "not an image");
            var broken = await Novels.CatalogFlow.LoadOptionalEpisodeCover(path + ".broken", source, CancellationToken.None, _ => warnings++);
            Require(missing == null && broken == null && warnings == 2, "Optional art failure blocked fallback.");
            using (var cancel = new CancellationTokenSource())
            {
                cancel.Cancel();
                var cancelled = false;
                try { await Novels.CatalogFlow.LoadOptionalEpisodeCover(path, source, cancel.Token, _ => warnings++); }
                catch (OperationCanceledException) { cancelled = true; }
                Require(cancelled && warnings == 2, "Cancellation was swallowed as fallback.");
            }
            var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
            var template = screen.GetComponentsInChildren<Button>().First(button => button.name == "Primary Action")
                .GetComponentInParent<Card>();
            fixture = UnityEngine.Object.Instantiate(template, screen.transform);
            fixture.gameObject.SetActive(false);
            var bind = typeof(Card).GetMethod("BindEpisode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var image = (Image)typeof(Card).GetField("_cover", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(fixture);
            var own = new Novels.Catalog.CatalogEpisodeItem("one", "Own art", cover: episodeCover);
            var fallback = new Novels.Catalog.CatalogEpisodeItem("two", "Story art");
            bind.Invoke(fixture, new object[] { storyCover, own, null, null });
            Require(image.sprite == episodeCover, "Episode art did not override story art.");
            bind.Invoke(fixture, new object[] { storyCover, fallback, null, null });
            Require(image.sprite == storyCover, "Rebound card retained another episode's art.");
            var downloading = new Novels.Catalog.CatalogDownloadState();
            bind.Invoke(fixture, new object[] { storyCover, new Novels.Catalog.CatalogEpisodeItem("three", "Downloading", download: downloading, cover: episodeCover), null, null });
            Require(image.sprite == episodeCover && !fixture.GetComponentsInChildren<Button>(true).Single(button => button.name == "Primary Action").interactable,
                "Episode art readiness incorrectly unlocked the story payload.");
            Debug.Log("CATALOG_EPISODE_COVERS PASS: actual preview/file export, legacy/new preview, safe paths, image load, missing/corrupt fallback, cancellation, real card override/rebind and download lockout; user saves untouched.");
        }
        finally
        {
            if (fixture != null) UnityEngine.Object.DestroyImmediate(fixture.gameObject);
            if (episodeCover != null)
            {
                UnityEngine.Object.DestroyImmediate(episodeCover.texture);
                UnityEngine.Object.DestroyImmediate(episodeCover);
            }
            if (storyCover != null) UnityEngine.Object.DestroyImmediate(storyCover);
            if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            if (Directory.Exists(root)) Directory.Delete(root, true);
            _checkingCovers = false;
        }
    }

    [MenuItem("Novels/Validation/Check Catalog Downloads")]
    public static async void CheckDownloads()
    {
        using var frames = new ValidationFrames();
        Require(Application.isPlaying && !_checkingDownloads, "Play Mode required; download check already running.");
        _checkingDownloads = true;
        var cache = Path.Combine(Application.temporaryCachePath, "catalog-download-validation-" + Guid.NewGuid().ToString("N"));
        Novels.CatalogDownloads queue = null;
        Card fixture = null;
        try
        {
            var sources = new Dictionary<string, DownloadFixtureSource>
            {
                ["first"] = new("first") { FailOnce = true, Allow = true },
                ["second"] = new("second"),
            };
            var previews = sources.ToDictionary(pair => pair.Key, pair => pair.Value.Preview);
            var entries = sources.Keys.Select(id => new Novels.Catalog.NovelCatalogEntry(id, id, "fixture", "",
                new[] { new Novels.Catalog.NovelCatalogEpisodeEntry("1", "Episode", "Description") })).ToArray();
            var order = new List<string>();
            var ctx = new Novels.CatalogFlow.Dependencies
            {
                ClientVersion = "0.2.0",
                CreateStoryBundles = (id, token) =>
                {
                    order.Add(id);
                    return new Bundles.Entity(new Bundles.Entity.Ctx
                    {
                        ContentSource = sources[id], PersistentDataPath = cache,
                        CacheNamespace = id, Platform = "editor", CancellationToken = token,
                    });
                },
            };
            queue = new Novels.CatalogDownloads(ctx, entries, previews);
            Require(order.Count == 0 && sources.Values.All(source => source.PayloadRequests == 0),
                "Payload/release work started before catalog display.");
            queue.Start(); queue.Start();
            await UniTask.WaitUntil(() => queue.GetState("first").Status == Novels.Catalog.CatalogDownloadStatus.Failed
                && sources["second"].PayloadRequests == 1).Timeout(TimeSpan.FromSeconds(8));
            Require(order.SequenceEqual(new[] { "first", "second" }), "Queue order/concurrency is incorrect.");
            Require(!queue.GetState("second").IsReady, "Partial download was marked ready.");
            var denied = false;
            try { queue.GetReadyBundles("second"); } catch (InvalidOperationException) { denied = true; }
            Require(denied, "Incomplete content was handed to the player.");
            queue.GetState("first").Retry(); queue.GetState("first").Retry();
            sources["second"].Allow = true;
            await UniTask.WaitUntil(() => queue.GetState("first").IsReady && queue.GetState("second").IsReady)
                .Timeout(TimeSpan.FromSeconds(8));
            Require(sources["first"].PayloadRequests == 2 && sources["second"].PayloadRequests == 1,
                "Retry duplicated a request or did not recover.");
            Require(queue.GetReadyBundles("first").ReleaseId == previews["first"].releaseId,
                "Launch did not retain the verified release.");
            queue.Dispose(); queue = null;

            // A mismatched preview must fail closed before any payload or save mutation.
            sources["first"] = new DownloadFixtureSource("first") { Allow = true };
            previews["first"] = new Novels.Catalog.Contracts.StoryCatalogPreview { releaseId = "stale" };
            queue = new Novels.CatalogDownloads(ctx, entries.Take(1).ToArray(), previews);
            queue.Start();
            await UniTask.WaitUntil(() => queue.GetState("first").Status == Novels.Catalog.CatalogDownloadStatus.Failed)
                .Timeout(TimeSpan.FromSeconds(8));
            Require(sources["first"].PayloadRequests == 0, "Mismatched preview downloaded content.");
            queue.Dispose(); queue = null;

            sources["second"] = new DownloadFixtureSource("cancelled");
            previews["second"] = sources["second"].Preview;
            queue = new Novels.CatalogDownloads(ctx, entries.Skip(1).ToArray(), previews);
            var cancelledState = queue.GetState("second");
            queue.Start();
            await UniTask.WaitUntil(() => sources["second"].PayloadRequests == 1).Timeout(TimeSpan.FromSeconds(8));
            queue.Dispose(); queue = null;
            await HoldWait(.2f);
            Require(!cancelledState.IsReady, "Cancellation unlocked incomplete content.");

            // Exercise the actual authored episode controls with isolated callbacks.
            var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
            var template = screen.GetComponentsInChildren<Button>().First(button => button.name == "Primary Action")
                .GetComponentInParent<Card>();
            fixture = UnityEngine.Object.Instantiate(template, screen.transform);
            fixture.gameObject.SetActive(false);
            var state = new Novels.Catalog.CatalogDownloadState();
            var model = new Novels.Catalog.CatalogEpisodeItem("fixture", "Fixture", "Description", "Доступно",
                "Продолжить", "Начать заново", "Fixture only", true, state);
            var opened = 0; var restarted = 0; var retries = 0;
            state.RetryRequested += () => retries++;
            typeof(Card).GetMethod("BindEpisode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(fixture, new object[] { null, model,
                    (Action<Novels.Catalog.CatalogEpisodeItem>)(_ => opened++),
                    (Action<Novels.Catalog.CatalogEpisodeItem>)(_ => restarted++) });
            var button = fixture.GetComponentsInChildren<Button>(true).First(value => value.name == "Primary Action");
            var restart = fixture.GetComponentsInChildren<Button>(true).First(value => value.name == "Restart Episode");
            Require(!button.interactable && !restart.interactable, "Queued episode is playable/resettable.");
            state.Update(Novels.Catalog.CatalogDownloadStatus.Downloading, .37f);
            Require(button.GetComponentInChildren<Text>(true).text.Contains("37%"), "Missing per-episode download percentage.");
            button.onClick.Invoke(); restart.onClick.Invoke();
            Require(opened == 0 && restarted == 0, "Loading controls triggered story actions.");
            state.Update(Novels.Catalog.CatalogDownloadStatus.Failed);
            Require(button.interactable, "Failed download has no retry control.");
            button.onClick.Invoke();
            Require(retries == 1 && opened == 0, "Retry launched the story.");
            state.Update(Novels.Catalog.CatalogDownloadStatus.Ready, 1f);
            Require(button.interactable && restart.interactable
                && button.GetComponentInChildren<Text>(true).text == "Продолжить", "Ready controls were not restored.");
            button.onClick.Invoke();
            Require(opened == 1 && restarted == 0, "Ready episode cannot open.");
            Debug.Log("CATALOG_DOWNLOAD_VALIDATION PASS: metadata-first, serial order, partial lockout, failure isolation, retry, pinned release, version mismatch, cancellation and authored progress/Continue controls; isolated cache/callbacks, no user saves touched.");
        }
        catch (Exception exception) { Debug.LogException(exception); }
        finally
        {
            queue?.Dispose();
            if (fixture != null) UnityEngine.Object.Destroy(fixture.gameObject);
            // Only this uniquely created fixture directory, never the application's cache.
            if (Directory.Exists(cache)) Directory.Delete(cache, true);
            _checkingDownloads = false;
        }
    }

    private sealed class DownloadFixtureSource : Bundles.IContentSource
    {
        private readonly byte[] _bytes;
        private readonly string _release;
        internal bool Allow;
        internal bool FailOnce;
        internal int PayloadRequests;
        internal readonly Novels.Catalog.Contracts.StoryCatalogPreview Preview;

        internal DownloadFixtureSource(string id)
        {
            _bytes = System.Text.Encoding.UTF8.GetBytes("catalog-delivery-fixture-" + id);
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = BitConverter.ToString(sha.ComputeHash(_bytes)).Replace("-", "").ToLowerInvariant();
            var release = new Bundles.ContentReleaseDto
            {
                contentSchemaVersion = 5, minimumClientVersion = "0.2.0", deliveryMode = Bundles.ContentDeliveryMode.Remote,
                bundles = new[] { new Bundles.BundleReleaseEntry { name = "fixture", version = hash.Substring(0, 32),
                    size = _bytes.Length, sha256 = hash, deliveryGroup = id == "cancelled" ? "second" : id } },
                files = Array.Empty<Bundles.ContentFileEntry>(),
                deliveryGroups = new[] { new Bundles.ContentDeliveryGroupEntry { id = id == "cancelled" ? "second" : id,
                    payloadCount = 1, size = _bytes.Length } },
            };
            release.releaseId = Bundles.ContentReleaseFingerprint.Compute(release);
            _release = Bundles.ContentReleaseCodec.Serialize(release);
            Preview = new Novels.Catalog.Contracts.StoryCatalogPreview { storyId = id, releaseId = release.releaseId, contentVersion = "1" };
        }

        public string ResolveFilePayloadPath(string logicalPath, string payloadPath) => payloadPath;
        public string GetUrl(string relativePath) => "file:///catalog-validation/" + relativePath;
        public UniTask<string> DownloadText(string path, CancellationToken token) => UniTask.FromResult(_release);
        public async UniTask DownloadFile(string path, string destinationPath, Action<long> progress, CancellationToken token)
        {
            PayloadRequests++;
            if (FailOnce) { FailOnce = false; throw new Bundles.ContentSourceException("Expected validation failure."); }
            await UniTask.WaitUntil(() => Allow, cancellationToken: token);
            progress?.Invoke(_bytes.Length / 2);
            await UniTask.Delay(100, ignoreTimeScale: true, cancellationToken: token);
            token.ThrowIfCancellationRequested();
            await File.WriteAllBytesAsync(destinationPath, _bytes, token);
            progress?.Invoke(_bytes.Length);
        }
    }

    private static bool _checkingSnap;

    [MenuItem("Novels/Validation/Check Catalog Snapping")]
    public static async void CheckSnapping()
    {
        using var frames = new ValidationFrames();
        Require(Application.isPlaying && !_checkingSnap, "Play Mode required; snapping check already running.");
        _checkingSnap = true;
        var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
        var scrolls = screen.GetComponentsInChildren<ScrollRect>();
        var positions = scrolls.Select(scroll => scroll.content.anchoredPosition).ToArray();
        var settings = screen.GetComponent<CatalogSettingsPopup>();
        var wasOpen = settings.IsOpen;
        try
        {
            settings.Close();
            ShowStart();
            Canvas.ForceUpdateCanvases();
            var vertical = scrolls.Single(scroll => scroll.vertical);
            var stories = vertical.content.GetComponentsInChildren<Card>()
                .Where(card => card.transform.parent == vertical.content).ToArray();
            var horizontal = stories[0].GetComponentInChildren<ScrollRect>();
            var episodes = horizontal.content.GetComponentsInChildren<Card>();
            Require(episodes.Length >= 4 && stories.Length >= 2, "Need two stories and four episodes.");
            Require(scrolls.All(scroll => scroll.GetComponent<CatalogScrollSnap>() != null), "Snap not installed.");
            var system = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
            var step = episodes[1].RectTransform.anchoredPosition.x - episodes[0].RectTransform.anchoredPosition.x;

            Drag(episodes[0].gameObject, system, new Vector2(-step * .7f, 0));
            await HoldWait(.45f);
            Require(Mathf.Abs(horizontal.content.anchoredPosition.x + step) < 1f,
                $"Episode did not settle left-aligned: x={horizontal.content.anchoredPosition.x}, step={step}.");
            Require(Mathf.Abs(vertical.content.anchoredPosition.y) < 1f, "Horizontal snap moved stories.");

            // Velocity is controlled explicitly to make a repeatable fast-fling probe.
            var pointer = new PointerEventData(system) { button = PointerEventData.InputButton.Left, delta = Vector2.left };
            ExecuteEvents.Execute(episodes[0].gameObject, pointer, ExecuteEvents.initializePotentialDrag);
            ExecuteEvents.Execute(episodes[0].gameObject, pointer, ExecuteEvents.beginDragHandler);
            horizontal.velocity = new Vector2(-step * 16f, 0);
            ExecuteEvents.Execute(episodes[0].gameObject, pointer, ExecuteEvents.endDragHandler);
            await HoldWait(.45f);
            Require(horizontal.content.anchoredPosition.x < -step * 2.5f, "Fast fling cannot skip multiple episodes.");

            // Last card must align identically, without elastic rebound on the next frame.
            horizontal.horizontalNormalizedPosition = 1f;
            ExecuteEvents.Execute(episodes[^1].gameObject, pointer, ExecuteEvents.initializePotentialDrag);
            ExecuteEvents.Execute(episodes[^1].gameObject, pointer, ExecuteEvents.beginDragHandler);
            ExecuteEvents.Execute(episodes[^1].gameObject, pointer, ExecuteEvents.endDragHandler);
            await HoldWait(.45f);
            var lastEdge = horizontal.viewport.InverseTransformPoint(episodes[^1].RectTransform.TransformPoint(
                new Vector3(episodes[^1].RectTransform.rect.xMin, 0, 0))).x;
            Require(Mathf.Abs(lastEdge - horizontal.viewport.rect.xMin) < 1f, "Last episode has insufficient trailing space.");

            ShowStart();
            Drag(episodes[0].gameObject, system, new Vector2(0, vertical.viewport.rect.height * .9f));
            await HoldWait(.6f);
            var top = vertical.viewport.InverseTransformPoint(stories[^1].RectTransform.TransformPoint(
                new Vector3(0, stories[^1].RectTransform.rect.yMax, 0))).y;
            Require(Mathf.Abs(top - (vertical.viewport.rect.yMax - CatalogScrollSnap.StoryInset)) < 1f,
                "Last story did not settle at its top inset.");

            ShowStart();
            Drag(episodes[0].gameObject, system, new Vector2(-step * .7f, 0));
            ExecuteEvents.Execute(episodes[0].gameObject, pointer, ExecuteEvents.initializePotentialDrag);
            var stopped = horizontal.content.anchoredPosition;
            await HoldWait(.45f);
            Require(Vector2.Distance(stopped, horizontal.content.anchoredPosition) < 1f, "New press did not interrupt snap.");
            ExecuteEvents.Execute(episodes[0].gameObject, pointer, ExecuteEvents.beginDragHandler);
            await HoldWait(.45f);
            Require(Vector2.Distance(stopped, horizontal.content.anchoredPosition) < 1f, "Snap fights an active drag.");
            ExecuteEvents.Execute(episodes[0].gameObject, pointer, ExecuteEvents.endDragHandler);
            settings.Open();
            stopped = horizontal.content.anchoredPosition;
            await HoldWait(.45f);
            Require(Vector2.Distance(stopped, horizontal.content.anchoredPosition) < 1f, "Catalog moves behind settings.");
            settings.Close();

            ShowStart();
            horizontal.content.anchoredPosition = new Vector2(-step * .7f, 0);
            var wheel = new PointerEventData(system) { scrollDelta = new Vector2(.01f, 0) };
            ExecuteEvents.Execute(episodes[0].gameObject, wheel, ExecuteEvents.scrollHandler);
            await HoldWait(.55f);
            Require(Mathf.Abs(horizontal.content.anchoredPosition.x + step) < 1f, "Wheel gesture did not settle.");

            // Opening inline confirmation cancels animation; never press its reset control.
            var restart = screen.GetComponentsInChildren<Button>()
                .FirstOrDefault(button => button.name == "Restart Episode" && button.interactable);
            if (restart != null)
            {
                var row = restart.GetComponentInParent<ScrollRect>();
                var card = restart.GetComponentInParent<Card>();
                Drag(card.gameObject, system, new Vector2(-80, 0));
                restart.onClick.Invoke();
                stopped = row.content.anchoredPosition;
                await HoldWait(.45f);
                Require(Vector2.Distance(stopped, row.content.anchoredPosition) < 1f, "Inline confirmation did not cancel snap.");
                CancelRestartConfirmation();
            }
            Debug.Log("CATALOG_SNAPPING_VALIDATION PASS: both axes, last-card alignment, fast fling, interruption, held drag, wheel, settings and inline confirmation; saves untouched.");
        }
        catch (Exception exception) { Debug.LogException(exception); }
        finally
        {
            for (var i = 0; i < scrolls.Length; i++)
            {
                if (scrolls[i] == null) continue;
                scrolls[i].GetComponent<CatalogScrollSnap>()?.Cancel();
                scrolls[i].content.anchoredPosition = positions[i];
            }
            if (wasOpen) settings.Open();
            _checkingSnap = false;
        }
    }

    [MenuItem("Novels/Validation/Check Live Catalog")]
    public static void CheckLiveCatalog()
    {
        Require(Application.isPlaying, "Enter Play Mode and wait for the catalog.");
        var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
        Require(screen != null, "Catalog is not loaded.");
        Canvas.ForceUpdateCanvases();
        var vertical = screen.GetComponentsInChildren<ScrollRect>().Single(s => s.vertical);
        var stories = vertical.content.GetComponentsInChildren<Card>()
            .Where(c => c.transform.parent == vertical.content).ToArray();
        Require(stories.Length >= 2, "Need at least two downloaded stories.");
        Require(vertical.content.rect.height > vertical.viewport.rect.height, "No vertical overflow.");
        var background = vertical.content.Find("Long Background") as RectTransform;
        Require(background != null && background.rect.height >= vertical.content.rect.height - 1,
            "Background must extend with the story list.");
        Require(background.GetComponent<Image>().color == Color.white, "Background art is tinted.");
        foreach (var label in screen.GetComponentsInChildren<Text>())
        {
            Require(label.rectTransform.rect.height > 0, "Non-positive text height: " + label.name);
            if (label.name == "Story Title" || label.name == "Episode Title")
                Require(label.preferredHeight <= label.rectTransform.rect.height + 1,
                    "Title is clipped: " + label.text);
        }
        var horizontal = stories[0].GetComponentInChildren<ScrollRect>();
        var episode = horizontal.content.GetComponentsInChildren<Card>().First();
        Require(episode.RectTransform.rect.width >= horizontal.viewport.rect.width * .85f,
            "Episode card is too narrow.");
        Require(episode.RectTransform.rect.width < horizontal.viewport.rect.width - 12,
            "Missing next-episode peek.");
        Require(episode.RectTransform.rect.height > vertical.viewport.rect.height * .55f,
            "Episode card is too short.");
        var artwork = episode.transform.Find("Artwork").GetComponent<Image>();
        Require(artwork.sprite != null && artwork.color == Color.white, "Cover sprite missing or tinted.");
        var eventSystem = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
        Require(eventSystem != null, "EventSystem missing.");
        var savedVertical = vertical.content.anchoredPosition;
        var savedHorizontal = horizontal.content.anchoredPosition;
        try
        {
            Drag(episode.gameObject, eventSystem, new Vector2(0, 120));
            Require(Vector2.Distance(savedVertical, vertical.content.anchoredPosition) > 1,
                "Vertical drag on episode did not scroll stories.");
            Require(Vector2.Distance(savedHorizontal, horizontal.content.anchoredPosition) < 1,
                "Vertical drag also moved the episode carousel.");
            vertical.StopMovement(); vertical.GetComponent<CatalogScrollSnap>()?.Cancel();
            vertical.content.anchoredPosition = savedVertical;
            Drag(episode.gameObject, eventSystem, new Vector2(-120, 0));
            Require(Vector2.Distance(savedHorizontal, horizontal.content.anchoredPosition) > 1,
                "Horizontal drag did not move episodes.");
            Require(Vector2.Distance(savedVertical, vertical.content.anchoredPosition) < 1,
                "Horizontal drag also moved stories.");
        }
        finally
        {
            vertical.StopMovement(); horizontal.StopMovement();
            vertical.GetComponent<CatalogScrollSnap>()?.Cancel();
            horizontal.GetComponent<CatalogScrollSnap>()?.Cancel();
            vertical.content.anchoredPosition = savedVertical;
            horizontal.content.anchoredPosition = savedHorizontal;
        }
        var lockedCount = 0;
        foreach (var action in screen.GetComponentsInChildren<Button>()
            .Where(button => button.name == "Primary Action" && !button.interactable))
        {
            var description = action.transform.parent.Find("Description").GetComponent<Text>();
            Require(description.text == "Чтобы открыть этот эпизод, дочитайте предыдущие эпизоды.",
                "Locked episode does not explain the reading prerequisite.");
            Require(description.preferredHeight <= description.rectTransform.rect.height + 1,
                "Locked episode instruction is clipped.");
            lockedCount++;
        }
        var continueCount = 0;
        foreach (var action in screen.GetComponentsInChildren<Button>()
            .Where(button => button.name == "Primary Action" && button.interactable))
        {
            var reset = action.transform.parent.Find("Restart Episode").GetComponent<Button>();
            Require(reset.gameObject.activeSelf, "Restart control is hidden on an unlocked episode.");
            Require(reset.GetComponentInChildren<Text>().text == "Начать заново",
                "Restart control must have a visible label, not only an icon.");
            var expected = reset.interactable ? "Продолжить" : "Открыть";
            Require(action.GetComponentInChildren<Text>().text == expected,
                "Episode action does not match saved progress: expected " + expected);
            if (reset.interactable) continueCount++;
            else
            {
                reset.onClick.Invoke();
                Require(!reset.transform.parent.Find("Restart Confirmation").gameObject.activeSelf,
                    "Unread episode must not open a reset confirmation.");
            }
        }
        var restart = screen.GetComponentsInChildren<Button>()
            .FirstOrDefault(button => button.name == "Restart Episode" && button.interactable);
        var restartResult = "not available on current saves";
        if (restart != null)
        {
            var panel = restart.transform.parent.Find("Restart Confirmation").gameObject;
            try
            {
                restart.onClick.Invoke();
                Require(panel.activeSelf, "Inline restart confirmation did not open.");
                Require(panel.transform.Find("Restart Warning Icon").GetComponent<Image>().sprite != null,
                    "Warning icon missing.");
                panel.transform.Find("Cancel").GetComponent<Button>().onClick.Invoke();
                Require(!panel.activeSelf, "Cancel did not close inline confirmation.");
                restartResult = "open/cancel passed; saved progress untouched";
            }
            finally { panel.SetActive(false); }
        }
        Canvas.ForceUpdateCanvases();
        Debug.Log($"CATALOG_VISUAL_VALIDATION passed: stories={stories.Length}; " +
            $"viewport={vertical.viewport.rect.size}; episode={episode.RectTransform.rect.size}; " +
            $"continueButtons={continueCount}; lockedDescriptions={lockedCount}; " +
            $"vertical/horizontal drag isolated; background attached; {restartResult}.");
    }

    [MenuItem("Novels/Validation/Show Catalog Settings")]
    public static void ShowSettings()
    {
        Require(Application.isPlaying, "Play Mode required.");
        UnityEngine.Object.FindFirstObjectByType<CatalogSettingsPopup>().Open();
    }

    [MenuItem("Novels/Validation/Check Catalog Settings")]
    public static void CheckSettings()
    {
        Require(Application.isPlaying, "Play Mode required.");
        var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
        var popup = screen.GetComponent<CatalogSettingsPopup>();
        Require(popup != null, "Settings popup component missing from loaded bundle.");
        var vertical = screen.GetComponentsInChildren<ScrollRect>().Single(s => s.vertical);
        var position = vertical.content.anchoredPosition;
        var gear = screen.GetComponentsInChildren<Button>().Single(b => b.name == "Settings Button");
        var gearPosition = gear.transform.position;
        var originalVolume = AudioListener.volume;
        const string key = "Novels.Settings.MasterVolume.v1";
        var hadPreference = PlayerPrefs.HasKey(key);
        var originalPreference = PlayerPrefs.GetFloat(key, 1f);
        Slider slider = null;
        try
        {
            vertical.StopMovement();
            vertical.verticalNormalizedPosition = 0;
            Canvas.ForceUpdateCanvases();
            Require(Vector3.Distance(gearPosition, gear.transform.position) < .1f,
                "Settings gear moves with story scrolling.");
            gear.onClick.Invoke();
            Require(popup.IsOpen, "Gear did not open settings.");
            var group = vertical.GetComponent<CanvasGroup>();
            Require(group != null && !group.interactable && !group.blocksRaycasts,
                "Catalog interaction is not blocked by popup.");
            slider = popup.GetComponentInChildren<Slider>();
            Require(slider != null, "Volume slider missing.");
            var handle = slider.handleRect;
            Require(Mathf.Abs(handle.rect.height - handle.rect.width) < 1,
                "Volume handle must be round, not vertically stretched.");
            var area = (RectTransform)handle.parent;
            var sample = new Vector3(Mathf.Lerp(area.rect.xMin, area.rect.xMax, .25f), area.rect.center.y);
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, area.TransformPoint(sample))
            };
            ExecuteEvents.Execute(slider.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(slider.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            Require(Mathf.Abs(AudioListener.volume - .25f) < .02f,
                "Pointer input did not change global volume.");
            foreach (var label in slider.transform.parent.GetComponentsInChildren<Text>())
                Require(label.preferredHeight <= label.rectTransform.rect.height + 1,
                    "Settings text clipped: " + label.name);
            slider.value = 0;
            Require(AudioListener.volume == 0, "Zero volume does not mute global audio.");
            slider.value = .35f;
            Require(Mathf.Abs(AudioListener.volume - .35f) < .001f, "Slider does not apply volume.");
            popup.Close();
            Require(!popup.IsOpen && group.interactable && group.blocksRaycasts,
                "Close did not restore catalog interaction.");
            Require(Mathf.Abs(PlayerPrefs.GetFloat(key, -1f) - .35f) < .001f,
                "Volume preference was not saved.");
            popup.Open();
            Require(Mathf.Abs(slider.value - .35f) < .001f, "Reopen lost volume.");
            var backdrop = popup.GetComponentsInChildren<Button>()
                .Single(b => b.name == "Settings Backdrop");
            backdrop.onClick.Invoke();
            Require(!popup.IsOpen, "Backdrop did not dismiss popup.");
            popup.Open();
            ExecuteEvents.Execute(popup.gameObject, new BaseEventData(EventSystem.current),
                ExecuteEvents.cancelHandler);
            Require(!popup.IsOpen, "Cancel did not dismiss popup.");
            for (var i = 0; i < 3; i++) { popup.Open(); popup.Close(); }
            Debug.Log("CATALOG_SETTINGS_VALIDATION passed: fixed gear, authored popup, modal block/restore, " +
                "all labels fit, global mute/35% volume/save/reopen, backdrop/cancel/repeated open. " +
                "Original preference restored; no story saves or external links touched.");
        }
        finally
        {
            if (slider != null)
            {
                popup.Open();
                slider.value = originalVolume;
            }
            popup.Close();
            AudioListener.volume = originalVolume;
            if (hadPreference) PlayerPrefs.SetFloat(key, originalPreference);
            else PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            vertical.StopMovement(); vertical.content.anchoredPosition = position;
        }
    }

    private static bool _checkingHold;

    [MenuItem("Novels/Validation/Check Hold Confirmation")]
    public static async void CheckHoldConfirmation()
    {
        if (_checkingHold) return;
        _checkingHold = true;
        Button fixture = null;
        try
        {
            Require(Application.isPlaying, "Play Mode required.");
            var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
            Require(screen != null, "Catalog is not loaded.");
            var original = screen.GetComponentsInChildren<Button>(true)
                .First(b => b.name == "Confirm" && b.transform.parent.parent.gameObject.activeInHierarchy);
            Require(original.GetComponent<HoldToConfirm>() != null, "Bundle has no hold control.");
            Require(original.onClick.GetPersistentEventCount() == 0, "Confirm has serialized click callbacks.");
            fixture = UnityEngine.Object.Instantiate(original, screen.transform);
            fixture.name = "Isolated Hold Validation";
            fixture.onClick.RemoveAllListeners();
            var rect = (RectTransform)fixture.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(240, 60);
            fixture.gameObject.SetActive(true);
            var hold = fixture.GetComponent<HoldToConfirm>();
            var fill = fixture.transform.Find("Hold Progress").GetComponent<Image>();
            Require(fill.type == Image.Type.Filled && fill.fillMethod == Image.FillMethod.Horizontal
                && !fill.raycastTarget, "Hold progress is not a non-blocking horizontal fill.");
            var label = fixture.GetComponentInChildren<Text>();
            Require(label.text.Contains("Удерживайте 2 с"), "Missing hold instruction.");
            Canvas.ForceUpdateCanvases();
            Require(label.preferredHeight <= label.rectTransform.rect.height, "Hold label clipped.");
            var completed = 0;
            hold.Configure(() => completed++);
            var system = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
            var position = RectTransformUtility.WorldToScreenPoint(null, rect.position);
            var pointer = new PointerEventData(system)
            {
                position = position, pressPosition = position, pointerId = -1,
                button = PointerEventData.InputButton.Left
            };
            var target = fixture.gameObject;
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerDownHandler);
            await HoldWait(.15f);
            Require(fill.fillAmount > 0 && fill.fillAmount < 1, "No progressive feedback.");
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute(target, new BaseEventData(system), ExecuteEvents.submitHandler);
            Require(completed == 0 && fill.fillAmount == 0, "Click/submit confirmed a reset.");

            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerDownHandler);
            await HoldWait(.1f);
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerExitHandler);
            Require(fill.fillAmount == 0, "Pointer exit kept progress.");
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerDownHandler);
            await HoldWait(.1f);
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.beginDragHandler);
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.endDragHandler);
            Require(fill.fillAmount == 0, "Drag kept progress.");
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerDownHandler);
            await HoldWait(.1f);
            target.SetActive(false); target.SetActive(true);
            Require(fill.fillAmount == 0, "Disable kept progress.");
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerDownHandler);
            await HoldWait(.1f);
            target.SendMessage("OnApplicationFocus", false);
            Require(fill.fillAmount == 0, "Focus loss kept progress.");
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerDownHandler);
            await HoldWait(.1f);
            pointer.position += Vector2.right * 30;
            await HoldWait(.1f);
            Require(fill.fillAmount == 0, "Pointer movement did not cancel.");
            pointer.position = position;
            await HoldWait(2.1f);
            Require(completed == 0, "Cancelled hold fired later.");

            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerDownHandler);
            await HoldWait(.5f);
            var otherPointer = new PointerEventData(system)
            {
                pointerId = 5, position = position, button = PointerEventData.InputButton.Left
            };
            ExecuteEvents.Execute(target, otherPointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, otherPointer, ExecuteEvents.pointerUpHandler);
            Require(fill.fillAmount > 0 && fill.fillAmount < 1 && completed == 0,
                "Another pointer cancelled/completed the owned hold.");
            await HoldWait(1.65f);
            Require(completed == 1 && fill.fillAmount == 1, "Full hold did not confirm exactly once.");
            await HoldWait(.25f);
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerClickHandler);
            Require(completed == 1, "Held pointer confirmed twice.");
            Debug.Log("CATALOG_HOLD_VALIDATION passed: short click/submit, release, exit, drag, " +
                "disable, focus loss, movement, second pointer; 2-second hold confirms once. " +
                "Isolated clone/counter only; user saves untouched.");
        }
        catch (Exception exception) { Debug.LogException(exception); }
        finally
        {
            if (fixture != null) UnityEngine.Object.Destroy(fixture.gameObject);
            _checkingHold = false;
        }
    }

    private static async System.Threading.Tasks.Task HoldWait(float seconds)
    {
        var until = Time.unscaledTime + seconds;
        while (Time.unscaledTime < until)
        {
            await Awaitable.NextFrameAsync();
            Require(Application.isPlaying, "Play Mode stopped during hold validation.");
        }
        // Inspect after ScrollRect/snap LateUpdate, including when the Editor was unfocused.
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
    }

    private sealed class ValidationFrames : IDisposable
    {
        private readonly bool _background = Application.runInBackground;
        internal ValidationFrames()
        {
            Application.runInBackground = true;
            EditorApplication.update += Tick;
        }
        private static void Tick() => EditorApplication.QueuePlayerLoopUpdate();
        public void Dispose()
        {
            EditorApplication.update -= Tick;
            Application.runInBackground = _background;
        }
    }

    [MenuItem("Novels/Validation/Show Catalog Restart Confirmation")]
    public static void ShowRestartConfirmation()
    {
        Require(Application.isPlaying, "Play Mode required.");
        var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
        var viewport = screen.GetComponentsInChildren<ScrollRect>().Single(s => s.vertical).viewport;
        var restart = screen.GetComponentsInChildren<Button>()
            .First(button => button.name == "Restart Episode" && button.interactable
                && RectTransformUtility.RectangleContainsScreenPoint(viewport,
                    RectTransformUtility.WorldToScreenPoint(null, button.transform.position)));
        restart.onClick.Invoke(); // Presentation only; never invoke the destructive Confirm action.
    }

    [MenuItem("Novels/Validation/Cancel Catalog Restart Confirmation")]
    public static void CancelRestartConfirmation()
    {
        Require(Application.isPlaying, "Play Mode required.");
        var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
        foreach (var cancel in screen.GetComponentsInChildren<Button>()
            .Where(button => button.name == "Cancel" && button.transform.parent.name == "Restart Confirmation"))
            cancel.onClick.Invoke();
    }

    [MenuItem("Novels/Validation/Show Next Catalog Story")]
    public static void ShowNextStory()
    {
        Require(Application.isPlaying, "Play Mode required.");
        var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
        var vertical = screen.GetComponentsInChildren<ScrollRect>().Single(s => s.vertical);
        vertical.StopMovement(); vertical.GetComponent<CatalogScrollSnap>()?.Cancel();
        vertical.verticalNormalizedPosition = 0;
    }

    [MenuItem("Novels/Validation/Show Locked Catalog Episode")]
    public static void ShowLockedEpisode()
    {
        ShowStart();
        var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
        var vertical = screen.GetComponentsInChildren<ScrollRect>().Single(s => s.vertical);
        var story = vertical.content.GetComponentsInChildren<Card>()
            .First(c => c.transform.parent == vertical.content);
        var horizontal = story.GetComponentInChildren<ScrollRect>();
        var action = horizontal.content.GetComponentsInChildren<Button>()
            .First(b => b.name == "Primary Action" && !b.interactable);
        Canvas.ForceUpdateCanvases();
        horizontal.StopMovement();
        var position = horizontal.content.anchoredPosition;
        var card = (RectTransform)action.transform.parent;
        position.x = -card.anchoredPosition.x + card.rect.width * card.pivot.x;
        horizontal.content.anchoredPosition = position;
    }

    [MenuItem("Novels/Validation/Show Catalog Start")]
    public static void ShowStart()
    {
        Require(Application.isPlaying, "Play Mode required.");
        var screen = UnityEngine.Object.FindFirstObjectByType<CatalogScreen>();
        foreach (var scroll in screen.GetComponentsInChildren<ScrollRect>())
        {
            scroll.StopMovement();
            scroll.GetComponent<CatalogScrollSnap>()?.Cancel();
            if (scroll.vertical) scroll.verticalNormalizedPosition = 1;
            else scroll.horizontalNormalizedPosition = 0;
        }
    }

    private static void Drag(GameObject target, EventSystem system, Vector2 delta)
    {
        var start = RectTransformUtility.WorldToScreenPoint(null, target.transform.position);
        // Gesture distances are authored in canvas units, not Game View pixels.
        delta = RectTransformUtility.WorldToScreenPoint(null, target.transform.TransformPoint(delta)) - start;
        var pointer = new PointerEventData(system)
        {
            position = start, pressPosition = start, delta = delta,
            button = PointerEventData.InputButton.Left
        };
        ExecuteEvents.Execute(target, pointer, ExecuteEvents.initializePotentialDrag);
        ExecuteEvents.Execute(target, pointer, ExecuteEvents.beginDragHandler);
        pointer.position += delta;
        ExecuteEvents.Execute(target, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(target, pointer, ExecuteEvents.endDragHandler);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("CATALOG_VISUAL_VALIDATION: " + message);
    }
}
