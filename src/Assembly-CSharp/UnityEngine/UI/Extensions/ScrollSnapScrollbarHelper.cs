using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	public class ScrollSnapScrollbarHelper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IEventSystemHandler
	{
		internal IScrollSnap ss;

		public void OnBeginDrag(PointerEventData eventData)
		{
			OnScrollBarDown();
		}

		public void OnDrag(PointerEventData eventData)
		{
			ss.CurrentPage();
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			OnScrollBarUp();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			OnScrollBarDown();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			OnScrollBarUp();
		}

		private void OnScrollBarDown()
		{
			if (ss != null)
			{
				ss.SetLerp(false);
				ss.StartScreenChange();
			}
		}

		private void OnScrollBarUp()
		{
			ss.SetLerp(true);
			ss.ChangePage(ss.CurrentPage());
		}
	}
}
