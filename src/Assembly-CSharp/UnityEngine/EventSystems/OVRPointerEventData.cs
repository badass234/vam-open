using System.Text;

namespace UnityEngine.EventSystems
{
	public class OVRPointerEventData : PointerEventData
	{
		public Ray worldSpaceRay;

		public Vector2 swipeStart;

		public OVRPointerEventData(EventSystem eventSystem)
			: base(eventSystem)
		{
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("<b>Position</b>: " + base.position);
			stringBuilder.AppendLine("<b>delta</b>: " + base.delta);
			stringBuilder.AppendLine("<b>eligibleForClick</b>: " + base.eligibleForClick);
			stringBuilder.AppendLine("<b>pointerEnter</b>: " + base.pointerEnter);
			stringBuilder.AppendLine("<b>pointerPress</b>: " + base.pointerPress);
			stringBuilder.AppendLine("<b>lastPointerPress</b>: " + base.lastPress);
			stringBuilder.AppendLine("<b>pointerDrag</b>: " + base.pointerDrag);
			stringBuilder.AppendLine("<b>worldSpaceRay</b>: " + worldSpaceRay);
			stringBuilder.AppendLine("<b>swipeStart</b>: " + swipeStart);
			stringBuilder.AppendLine("<b>Use Drag Threshold</b>: " + base.useDragThreshold);
			return stringBuilder.ToString();
		}
	}
}
