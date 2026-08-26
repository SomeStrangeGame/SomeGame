using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Setting.View
{
    public class Screen : MonoBehaviour
    {
        [SerializeField] private Text _description;
        [SerializeField] private Button _buttonPrefab;

        private readonly Dictionary<string, Button> _buttons = new();
        private static Screen _active;

        private void Awake()
        {
            _active = this;
        }

        private void OnDestroy()
        {
            if (_active == this)
                _active = null;
        }

        public void SetDescription(string text)
        {
            _description.text = text;
        }

        public void AddOrUpdateButton(string id, string text, Action onClick)
        {
            _buttonPrefab.gameObject.SetActive(false);
            if (!_buttons.TryGetValue(id, out var button))
                button = Instantiate(_buttonPrefab, _buttonPrefab.transform.parent);

            _buttons[id] = button;
            var label = button.GetComponentInChildren<Text>(true);
            label.text = text;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick.Invoke());
            button.gameObject.SetActive(true);
        }

        public static string GetDebugSnapshot()
        {
            if (_active == null)
                return "Setting · inactive";
            var result = new StringBuilder("Setting · ");
            AppendGraphic(result, "desc", _active._description);
            foreach (var pair in _active._buttons)
            {
                var button = pair.Value;
                result.Append(" · ").Append(pair.Key).Append(' ');
                AppendGraphic(result, "img", button.targetGraphic);
                result.Append(' ');
                AppendGraphic(
                    result,
                    "text",
                    button.GetComponentInChildren<Text>(true));
            }
            return result.ToString();
        }

        private static void AppendGraphic(
            StringBuilder result,
            string label,
            Graphic graphic)
        {
            if (graphic == null)
            {
                result.Append(label).Append(":null");
                return;
            }
            var rect = graphic.rectTransform.rect;
            var material = graphic.material;
            result.Append(label)
                .Append(":on=").Append(graphic.gameObject.activeInHierarchy)
                .Append(" a=").Append(graphic.color.a.ToString("F1"))
                .Append(" rect=").Append(rect.width.ToString("F0"))
                .Append('x').Append(rect.height.ToString("F0"))
                .Append(" mat=").Append(material != null ? material.name : "null")
                .Append(" sh=").Append(
                    material != null && material.shader != null
                        ? material.shader.name
                        : "null");
        }
    }
}
