using UnityEngine.EventSystems;

namespace SimpleInput
{
	public interface ISimpleInputDraggable
	{
		void OnPointerDown( PointerEventData eventData );
		void OnDrag( PointerEventData eventData );
		void OnPointerUp( PointerEventData eventData );
	}
}