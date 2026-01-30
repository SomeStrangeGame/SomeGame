using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace SimpleInput
{
	public interface ISimpleInputDraggableMultiTouch
	{
		int Priority { get; }

		bool OnUpdate( List<PointerEventData> mousePointers, List<PointerEventData> touchPointers, ISimpleInputDraggableMultiTouch activeListener );
	}
}