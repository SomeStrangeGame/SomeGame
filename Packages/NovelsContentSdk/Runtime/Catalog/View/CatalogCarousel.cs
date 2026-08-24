using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Novels.Catalog.View
{
    public sealed class CatalogCarousel : MonoBehaviour,
        IBeginDragHandler,
        IEndDragHandler
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private RectTransform _content;
        [SerializeField] private HorizontalLayoutGroup _layout;
        [SerializeField] private float _snapSpeed = 12f;
        [SerializeField] private float _sideScale = 0.85f;
        [SerializeField] private float _sideOpacity = 0.65f;
        [SerializeField, Range(0.1f, 1f)] private float _cardViewportRatio = 0.8f;

        private readonly List<Item> _items = new();
        private Card _focusedCard;
        private bool _dragging;
        private bool _snapping;
        private Vector2 _viewportSize;

        public void Register(Card card, bool canOpen, Action open)
        {
            var item = Find(card);
            if (item == null)
            {
                item = new Item(card);
                _items.Add(item);
            }

            item.CanOpen = canOpen;
            item.Open = open;
            card.SetClick(() => SelectOrOpen(card));

            RefreshLayout();
            if (_focusedCard == null)
                Focus(card, true);
            else
                UpdateCardVisuals();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragging = true;
            _snapping = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _dragging = false;
            Focus(FindNearestCard(), false);
        }

        private void LateUpdate()
        {
            if (_items.Count == 0)
                return;

            if ((_viewportSize - _viewport.rect.size).sqrMagnitude > 0.01f)
                RefreshLayout();

            if (_dragging)
                _focusedCard = FindNearestCard();
            else if (_snapping)
                MoveFocusedCardToCenter();

            UpdateCardVisuals();
        }

        private void SelectOrOpen(Card card)
        {
            if (card != _focusedCard)
            {
                Focus(card, false);
                return;
            }

            var item = Find(card);
            if (item?.CanOpen == true)
                item.Open?.Invoke();
        }

        private void Focus(Card card, bool immediately)
        {
            if (card == null)
                return;

            _focusedCard = card;
            _scrollRect.StopMovement();
            _snapping = !immediately;
            if (immediately)
            {
                Center(card, 1f);
                UpdateCardVisuals();
            }
        }

        private void MoveFocusedCardToCenter()
        {
            var amount = 1f - Mathf.Exp(-_snapSpeed * Time.unscaledDeltaTime);
            if (Mathf.Abs(Center(_focusedCard, amount)) < 0.5f)
                _snapping = false;
        }

        private float Center(Card card, float amount)
        {
            var offset = CardCenter(card);
            var position = _content.anchoredPosition;
            position.x -= offset * amount;
            _content.anchoredPosition = position;
            return offset;
        }

        private Card FindNearestCard()
        {
            Card nearest = null;
            var nearestDistance = float.MaxValue;
            foreach (var item in _items)
            {
                var distance = Mathf.Abs(CardCenter(item.Card));
                if (distance >= nearestDistance)
                    continue;
                nearest = item.Card;
                nearestDistance = distance;
            }
            return nearest;
        }

        private void UpdateCardVisuals()
        {
            var step = CardWidth() + _layout.spacing;
            foreach (var item in _items)
            {
                var focus = 1f - Mathf.Clamp01(Mathf.Abs(CardCenter(item.Card)) / step);
                item.Card.SetFocus(
                    Mathf.Lerp(_sideScale, 1f, focus),
                    Mathf.Lerp(_sideOpacity, 1f, focus));
            }
        }

        private float CardCenter(Card card) =>
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                _viewport,
                card.RectTransform).center.x;

        private void RefreshLayout()
        {
            if (_viewport.parent is RectTransform parent)
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            Canvas.ForceUpdateCanvases();
            _viewportSize = _viewport.rect.size;
            _content.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                _viewportSize.y);
            ResizeCards();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
            var sidePadding = Mathf.Max(
                0,
                Mathf.RoundToInt((_viewportSize.x - CardWidth()) * 0.5f));
            _layout.padding.left = sidePadding;
            _layout.padding.right = sidePadding;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        }

        private void ResizeCards()
        {
            var bounds = _viewportSize * _cardViewportRatio;
            foreach (var item in _items)
            {
                var height = bounds.x / item.AspectRatio;
                var width = bounds.x;
                if (height > bounds.y)
                {
                    height = bounds.y;
                    width = height * item.AspectRatio;
                }

                item.Layout.preferredWidth = width;
                item.Layout.preferredHeight = height;
            }
        }

        private float CardWidth()
        {
            if (_items.Count == 0)
                return 0f;
            return _items[0].Card.RectTransform.rect.width;
        }

        private Item Find(Card card) =>
            _items.Find(item => item.Card == card);

        private sealed class Item
        {
            internal Item(Card card)
            {
                Card = card;
                Layout = card.GetComponent<LayoutElement>();
                AspectRatio = Layout.preferredWidth / Layout.preferredHeight;
            }

            internal Card Card { get; }
            internal LayoutElement Layout { get; }
            internal float AspectRatio { get; }
            internal bool CanOpen { get; set; }
            internal Action Open { get; set; }
        }
    }
}
