using UnityEngine;
using UnityEngine.UI;

namespace Game.SomeBattleScene1.View
{
    public interface ISomeBattleScene1Screen
    {
        public void Release();
        public Slider.SliderEvent SliderEvent { get; }
    }

    public class Screen : MonoBehaviour, ISomeBattleScene1Screen
    {
        [SerializeField] private Slider _slider;

        public Slider.SliderEvent SliderEvent => _slider.onValueChanged;

        public void Release() 
        {
            if (this != null)
                GameObject.Destroy(gameObject);
        }
    }

}

