using UnityEngine;
using UnityEngine.UI;

namespace Novels.Bubble.View
{
    internal sealed class ChoiceButtonIcon : MonoBehaviour
    {
        [SerializeField] private Image _image;

        internal void SetSprite(Sprite sprite)
        {
            _image ??= GetComponent<Image>();
            _image.sprite = sprite;
            _image.gameObject.SetActive(sprite != null);
        }
    }
}
