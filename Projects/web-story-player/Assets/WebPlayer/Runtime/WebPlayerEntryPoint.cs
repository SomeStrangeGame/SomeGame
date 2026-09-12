using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novels.WebPlayer
{
    public sealed class WebPlayerEntryPoint : MonoBehaviour
    {
        private const string _gameObjectName = "WebStoryPlayer";
        private static WebPlayerEntryPoint _instance;
        private WebStoryRuntimeSession _session;
        private bool _transitioning;
        private bool _destroyed;
        private bool _episodeCompleted;

        [Serializable]
        private sealed class BrowserEvent
        {
            public string type;
            public string code;
            public string storyId;
            public string storyVersion;
        }

        public WebPlayerLaunchConfiguration Configuration { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_instance != null)
                return;

            var gameObject = new GameObject(_gameObjectName);
            DontDestroyOnLoad(gameObject);
            _instance = gameObject.AddComponent<WebPlayerEntryPoint>();
            gameObject.AddComponent<WebStorySaveStore>();
            _instance.Emit("player_ready");
        }

        public void Launch(string json)
        {
            WebPlayerLaunchConfiguration configuration;
            try
            {
                configuration = JsonUtility.FromJson<WebPlayerLaunchConfiguration>(json);
            }
            catch (Exception)
            {
                Emit("launch_rejected", "invalid_json");
                return;
            }

            if (configuration == null)
            {
                Emit("launch_rejected", "invalid_configuration");
                return;
            }

            if (!configuration.TryValidate(out var errorCode))
            {
                Emit("launch_rejected", errorCode);
                return;
            }

            if (_transitioning)
            {
                EmitFor(configuration, "launch_rejected", "session_stopping");
                return;
            }

            if (_session != null && !_session.CancellationToken.IsCancellationRequested
                && _session.Matches(configuration))
            {
                Emit(
                    "launch_rejected",
                    "duplicate_launch",
                    configuration.storyId,
                    configuration.storyVersion);
                return;
            }

            ReplaceSession(configuration).Forget();
        }

        private async UniTask ReplaceSession(WebPlayerLaunchConfiguration configuration)
        {
            _transitioning = true;
            var stopped = await StopSession("replaced");
            _transitioning = false;
            if (!stopped || _destroyed) return;
            _episodeCompleted = false;

            Configuration = configuration;
            Emit(
                "launch_accepted",
                storyId: configuration.storyId,
                storyVersion: configuration.storyVersion);
            EmitFor(configuration, "story_loading");

            try
            {
                _session = WebStoryRuntimeSession.Create(configuration);
                EmitFor(configuration, "runtime_session_ready");
                EmitFor(configuration, "content_requested");
                Prepare(_session).Forget();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EmitFor(configuration, "runtime_error", "initialization_failed");
                await StopAfterFailure();
            }
        }

        private async UniTask Prepare(WebStoryRuntimeSession session)
        {
            var failureCode = "content_failed";
            try
            {
                var result = await session.Run(GetComponent<WebStorySaveStore>(), Application.absoluteURL,
                    (type, code) =>
                    {
                        if (type == "content_ready") failureCode = "reading_failed";
                        if (!_transitioning && !_destroyed && _session == session)
                            EmitFor(session.Configuration, type, code);
                    });
                if (_session != session || _transitioning || _destroyed) return;
                if (result.Status == EpisodeRunStatus.Failed)
                {
                    Debug.LogError(result.Error?.Message);
                    EmitFor(session.Configuration, "runtime_error", "reading_failed");
                    await StopAfterFailure();
                }
                else if (result.Status == EpisodeRunStatus.Completed)
                {
                    _episodeCompleted = true;
                    EmitFor(session.Configuration, "episode_completed", session.EpisodeId);
                    EmitFor(session.Configuration, session.HasNextEpisode
                        ? "next_episode_available" : "story_completed", session.EpisodeId);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception exception)
            {
                if (_session != session || _transitioning || _destroyed) return;
                Debug.LogException(exception);
                EmitFor(session.Configuration, "runtime_error", failureCode);
                await StopAfterFailure();
            }
        }

        private void OnDestroy()
        {
            _destroyed = true;
            if (_instance == this)
                _instance = null;
            // Browser teardown cannot await IndexedDB. Only acknowledged checkpoints
            // are durable; normal relaunch always uses StopSession instead.
            _session?.Dispose();
            _session = null;
        }

        // Website owns the button. No arbitrary episode id/unlock bypass is accepted.
        public void NextEpisode(string unused)
        {
            if (_transitioning || !_episodeCompleted || _session == null || !_session.HasNextEpisode)
            {
                Emit("launch_rejected", "next_episode_unavailable");
                return;
            }
            ReplaceSession(_session.Configuration).Forget();
        }

        public void ReturnToSite(string unused)
        {
            if (_transitioning || _session == null)
            {
                Emit("navigation_rejected", "session_unavailable");
                return;
            }
            ReturnToSiteAsync().Forget();
        }

        private async UniTask ReturnToSiteAsync()
        {
            var configuration = _session.Configuration;
            _transitioning = true;
            var stopped = await StopSession("return_to_site");
            _transitioning = false;
            if (!stopped || _destroyed) return;
            _episodeCompleted = false;
            EmitFor(configuration, "return_to_site",
                string.IsNullOrEmpty(configuration.returnUrl) ? "/" : configuration.returnUrl);
        }

        private async UniTask StopAfterFailure()
        {
            _transitioning = true;
            await StopSession("failed");
            _transitioning = false;
        }

        private async UniTask<bool> StopSession(string code)
        {
            var session = _session;
            if (session == null) return true;
            EmitFor(session.Configuration, "runtime_session_stopping", code);
            try
            {
                await session.StopAsync();
                if (_destroyed) return false;
                _session = null;
                EmitFor(session.Configuration, "runtime_session_cancelled", code);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (!_destroyed) EmitFor(session.Configuration, "runtime_error", "session_stop_failed");
                return false;
            }
        }

        private void EmitFor(
            WebPlayerLaunchConfiguration configuration,
            string type,
            string code = "")
        {
            Emit(type, code, configuration.storyId, configuration.storyVersion);
        }

        private void Emit(
            string type,
            string code = "",
            string storyId = "",
            string storyVersion = "")
        {
            var json = JsonUtility.ToJson(new BrowserEvent
            {
                type = type,
                code = code,
                storyId = storyId,
                storyVersion = storyVersion,
            });

#if UNITY_WEBGL && !UNITY_EDITOR
            SomeGameWebPlayerEmit(json);
#else
            Debug.Log($"[WEB_PLAYER] {json}");
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SomeGameWebPlayerEmit(string json);
#endif
    }
}
