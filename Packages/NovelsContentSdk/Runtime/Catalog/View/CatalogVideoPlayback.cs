using System;
using UnityEngine;
using UnityEngine.Video;

namespace Novels.Catalog.View
{
    // One decoder/audio source per catalog, not per instantiated episode.
    // Presentation remains in the authored card prefab; this owns only media lifetime.
    internal sealed class CatalogVideoPlayback : IDisposable
    {
        private const float FadeSeconds = 1.2f;
        private const float PrepareTimeout = 15f;
        private readonly GameObject _owner;
        private VideoPlayer _player;
        private AudioSource _audio;
        private Card _selected;
        private string _url;
        private float _selectedAt;
        private float _visibleAt;
        private int _firstFrame = -1;
        private bool _started;
        private bool _failed;
        private bool _visible;

        internal CatalogVideoPlayback(GameObject owner) => _owner = owner;

        internal void Tick(Card selected)
        {
            if (_selected != selected || _url != selected?.VideoUrl)
            {
                Stop();
                _selected = selected;
                _url = selected?.VideoUrl;
                _selectedAt = Time.unscaledTime;
            }
            if (_selected == null || !_selected.HasVideoSurface || string.IsNullOrWhiteSpace(_url) || _failed)
                return;
            // Debounce fast flicks; the cover is already visible throughout preparation.
            if (!_started && Time.unscaledTime - _selectedAt >= .15f)
            {
                EnsurePlayer();
                _started = true;
                _player.url = _url;
                _player.Prepare();
            }
            if (!_started) return;
            if (!_visible && _firstFrame >= 0 && Time.frameCount > _firstFrame && _player.texture != null)
            {
                _visible = true;
                _visibleAt = Time.unscaledTime;
                _player.sendFrameReadyEvents = false;
            }
            if (_visible)
            {
                _selected.SetVideoTexture(_player.texture);
                _audio.volume = Mathf.SmoothStep(0f, 1f, (Time.unscaledTime - _visibleAt) / FadeSeconds);
            }
            else if (Time.unscaledTime - _selectedAt > PrepareTimeout)
                Fail("preparation/first frame timed out");
        }

        private void EnsurePlayer()
        {
            if (_player != null) return;
            _audio = _owner.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;
            _audio.volume = 0f;
            _audio.ignoreListenerVolume = false;
            _audio.ignoreListenerPause = false;
            _player = _owner.AddComponent<VideoPlayer>();
            _player.playOnAwake = false;
            _player.source = VideoSource.Url;
            _player.renderMode = VideoRenderMode.APIOnly;
            _player.audioOutputMode = VideoAudioOutputMode.AudioSource;
            _player.controlledAudioTrackCount = 1;
            _player.EnableAudioTrack(0, true);
            _player.SetTargetAudioSource(0, _audio);
            _player.isLooping = true;
            _player.waitForFirstFrame = true;
            _player.prepareCompleted += OnPrepared;
            _player.frameReady += OnFrameReady;
            _player.errorReceived += OnError;
        }

        private void OnPrepared(VideoPlayer player)
        {
            if (!_started || _failed || _selected == null || player.url != _url) return;
            player.sendFrameReadyEvents = true;
            player.Play();
        }

        private void OnFrameReady(VideoPlayer player, long frame)
        {
            if (_started && !_failed && _firstFrame < 0 && player.url == _url)
                _firstFrame = Time.frameCount;
        }

        private void OnError(VideoPlayer player, string message)
        {
            if (_started && !_failed) Fail(message);
        }

        private void Fail(string message)
        {
            _failed = true;
            StopMedia();
            Debug.LogWarning($"Catalog video unavailable; keeping cover. {message}");
        }

        private void StopMedia()
        {
            if (_audio != null) { _audio.volume = 0f; _audio.Stop(); }
            if (_player != null) { _player.sendFrameReadyEvents = false; _player.Stop(); }
            if (_selected != null) _selected.SetVideoTexture(null);
            _firstFrame = -1;
            _visible = false;
        }

        internal void Stop()
        {
            _started = false;
            StopMedia();
            _selected = null;
            _url = null;
            _failed = false;
        }

        public void Dispose()
        {
            Stop();
            if (_player != null)
            {
                _player.prepareCompleted -= OnPrepared;
                _player.frameReady -= OnFrameReady;
                _player.errorReceived -= OnError;
                UnityEngine.Object.Destroy(_player);
            }
            if (_audio != null) UnityEngine.Object.Destroy(_audio);
        }

        internal static Rect WorldRect(RectTransform transform, Vector3[] corners)
        {
            transform.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        internal static Rect Intersection(Rect a, Rect b)
        {
            var minX = Mathf.Max(a.xMin, b.xMin);
            var minY = Mathf.Max(a.yMin, b.yMin);
            return Rect.MinMaxRect(minX, minY, Mathf.Max(minX, Mathf.Min(a.xMax, b.xMax)),
                Mathf.Max(minY, Mathf.Min(a.yMax, b.yMax)));
        }
    }
}
