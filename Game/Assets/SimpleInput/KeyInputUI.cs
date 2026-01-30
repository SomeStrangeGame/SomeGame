using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleInput
{
	public class KeyInputUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		public EntryPoint.KeyInput key = new EntryPoint.KeyInput();

		private void Awake()
		{
			Graphic graphic = GetComponent<Graphic>();
			if( graphic != null )
				graphic.raycastTarget = true;
		}

		private void OnEnable()
		{
			key.StartTracking();
		}

		private void OnDisable()
		{
			key.StopTracking();
		}

		public void OnPointerDown( PointerEventData eventData )
		{
			key.value = true;
		}

		public void OnPointerUp( PointerEventData eventData )
		{
			key.value = false;
		}
	}
}