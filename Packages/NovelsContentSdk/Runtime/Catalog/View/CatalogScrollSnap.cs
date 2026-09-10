using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Novels.Catalog.View
{
    // Native ScrollRect owns dragging; this component only settles a completed gesture.
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public sealed class CatalogScrollSnap : MonoBehaviour, IInitializePotentialDragHandler,
        IBeginDragHandler, IEndDragHandler, IScrollHandler
    {
        public const float StoryInset = 16f;
        private readonly List<RectTransform> _items = new();
        private ScrollRect _scroll;
        private bool _vertical;
        private bool _dragging;
        private bool _settling;
        private int _pointer;
        private float _wheelDeadline = -1f;
        private float _started;
        private float _from;
        private float _to;

        public static CatalogScrollSnap Ensure(ScrollRect scroll)
        {
            var snap = scroll.GetComponent<CatalogScrollSnap>();
            return snap != null ? snap : scroll.gameObject.AddComponent<CatalogScrollSnap>();
        }

        private void Awake()
        {
            _scroll = GetComponent<ScrollRect>();
            _vertical = _scroll.vertical;
        }

        public void SetItems(IEnumerable<RectTransform> items)
        {
            Cancel();
            _items.Clear();
            _items.AddRange(items);
        }

        public void Focus(RectTransform item)
        {
            if (item == null || _scroll == null || _scroll.viewport == null)
                return;
            Cancel();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scroll.content);
            var target = TargetPosition(item);
            var position = _scroll.content.anchoredPosition;
            if (_vertical) position.y = target;
            else position.x = -target;
            _scroll.content.anchoredPosition = position;
        }

        public void Cancel()
        {
            _settling = false;
            _dragging = false;
            _wheelDeadline = -1f;
            if (_scroll != null) _scroll.StopMovement();
        }

        public void OnInitializePotentialDrag(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left && !_dragging) Cancel();
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left || _dragging) return;
            Cancel();
            _dragging = true;
            _pointer = data.pointerId;
        }

        public void OnEndDrag(PointerEventData data)
        {
            if (!_dragging || data.pointerId != _pointer) return;
            _dragging = false;
            var velocity = _vertical ? _scroll.velocity.y : -_scroll.velocity.x;
            var span = _vertical ? _scroll.viewport.rect.height : _scroll.viewport.rect.width;
            Settle(Mathf.Clamp(velocity * .14f, -span * 3f, span * 3f));
        }

        public void OnScroll(PointerEventData data)
        {
            if (_dragging || data.scrollDelta.sqrMagnitude < .0001f) return;
            _settling = false;
            _wheelDeadline = Time.unscaledTime + .15f;
        }

        private float Position => _vertical ? _scroll.content.anchoredPosition.y
            : -_scroll.content.anchoredPosition.x;

        private void Settle(float projection)
        {
            _wheelDeadline = -1f;
            if (_items.Count == 0 || _scroll.viewport == null) return;
            var viewport = _scroll.viewport;
            var maximum = Mathf.Max(0f, _vertical
                ? _scroll.content.rect.height - viewport.rect.height
                : _scroll.content.rect.width - viewport.rect.width);
            var predicted = Mathf.Clamp(Position + projection, 0f, maximum);
            var best = 0f; // Keep the catalog header reachable above the first story.
            var distance = Mathf.Abs(predicted);
            foreach (var item in _items)
            {
                if (item == null || !item.gameObject.activeInHierarchy) continue;
                var target = TargetPosition(item);
                var candidateDistance = Mathf.Abs(target - predicted);
                if (candidateDistance >= distance) continue;
                best = target;
                distance = candidateDistance;
            }
            _from = Position;
            _to = best;
            _started = Time.unscaledTime;
            _settling = true;
            _scroll.StopMovement();
        }

        private float TargetPosition(RectTransform item)
        {
            var viewport = _scroll.viewport;
            var maximum = Mathf.Max(0f, _vertical
                ? _scroll.content.rect.height - viewport.rect.height
                : _scroll.content.rect.width - viewport.rect.width);
            // Only the card rect: descendant episode content can be much wider.
            var edge = viewport.InverseTransformPoint(item.TransformPoint(
                new Vector3(item.rect.xMin, item.rect.yMax, 0f)));
            return Mathf.Clamp(Position + (_vertical
                ? viewport.rect.yMax - StoryInset - edge.y
                : edge.x - viewport.rect.xMin), 0f, maximum);
        }

        private void LateUpdate()
        {
            if (_dragging) return;
            if (_wheelDeadline >= 0f && Time.unscaledTime >= _wheelDeadline) Settle(0f);
            if (!_settling) return;
            _scroll.StopMovement();
            var t = Mathf.Clamp01((Time.unscaledTime - _started) / (_vertical ? .36f : .22f));
            var value = Mathf.Lerp(_from, _to, 1f - Mathf.Pow(1f - t, 3f));
            var position = _scroll.content.anchoredPosition;
            if (_vertical) position.y = value;
            else position.x = -value;
            _scroll.content.anchoredPosition = position;
            if (t >= 1f) _settling = false;
        }

        private void OnDisable() => Cancel();
        private void OnApplicationFocus(bool focused) { if (!focused) Cancel(); }
    }
}
