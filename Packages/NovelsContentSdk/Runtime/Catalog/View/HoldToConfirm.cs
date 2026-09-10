using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Novels.Catalog.View
{
    [RequireComponent(typeof(Button))]
    public sealed class HoldToConfirm : MonoBehaviour, IPointerDownHandler,
        IPointerUpHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler,
        IEndDragHandler, IDeselectHandler
    {
        [SerializeField, Min(.25f)] private float _duration = 2f;
        [SerializeField] private Image _fill;

        private Button _button;
        private Action _onConfirmed;
        private PointerEventData _pointer;
        private Vector2 _pressPosition;
        private float _startedAt;
        private int _pointerId;
        private bool _completed;

        public void Configure(Action onConfirmed)
        {
            Cancel();
            _completed = false;
            _onConfirmed = onConfirmed;
        }

        private void Awake() => _button = GetComponent<Button>();

        private void OnEnable()
        {
            _completed = false;
            Cancel();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointer != null || _completed || !isActiveAndEnabled
                || !_button.IsInteractable() || eventData.button != PointerEventData.InputButton.Left
                || !Contains(eventData))
                return;
            _pointer = eventData;
            _pointerId = eventData.pointerId;
            _pressPosition = eventData.position;
            _startedAt = Time.unscaledTime;
        }

        private void Update()
        {
            if (_pointer == null) return;
            var dragThreshold = EventSystem.current != null ? EventSystem.current.pixelDragThreshold : 10;
            if (!_button.IsInteractable() || !Contains(_pointer) || _pointer.dragging
                || Vector2.Distance(_pressPosition, _pointer.position) > dragThreshold)
            {
                Cancel();
                return;
            }
            var progress = Mathf.Clamp01((Time.unscaledTime - _startedAt) / Mathf.Max(.25f, _duration));
            if (_fill != null) _fill.fillAmount = progress;
            if (progress < 1f) return;
            // Clear ownership before invoking code that can disable/rebind/destroy the card.
            _pointer = null;
            _completed = true;
            _onConfirmed?.Invoke();
        }

        private bool Contains(PointerEventData pointer) =>
            RectTransformUtility.RectangleContainsScreenPoint(
                (RectTransform)transform, pointer.position, pointer.pressEventCamera);

        private void CancelPointer(PointerEventData pointer)
        {
            if (_pointer != null && pointer.pointerId == _pointerId) Cancel();
        }

        // A drag over a destructive control cancels instead of becoming a carousel swipe.
        public void OnPointerUp(PointerEventData eventData) => CancelPointer(eventData);
        public void OnPointerExit(PointerEventData eventData) => CancelPointer(eventData);
        public void OnBeginDrag(PointerEventData eventData) => CancelPointer(eventData);
        public void OnDrag(PointerEventData eventData) => CancelPointer(eventData);
        public void OnEndDrag(PointerEventData eventData) => CancelPointer(eventData);
        public void OnDeselect(BaseEventData eventData) => Cancel();
        private void OnDisable() => Cancel();
        private void OnApplicationFocus(bool focused) { if (!focused) Cancel(); }
        private void OnApplicationPause(bool paused) { if (paused) Cancel(); }

        private void Cancel()
        {
            _pointer = null;
            if (_fill != null) _fill.fillAmount = 0f;
        }
    }
}
