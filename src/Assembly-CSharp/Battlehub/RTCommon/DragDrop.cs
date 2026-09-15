using UnityEngine;

namespace Battlehub.RTCommon
{
	public static class DragDrop
	{
		public static Object[] DragItems { get; private set; }

		public static Object DragItem
		{
			get
			{
				if (DragItems == null || DragItems.Length == 0)
				{
					return null;
				}
				return DragItems[0];
			}
		}

		public static event BeginDragEventHandler BeginDrag;

		public static event DropEventHandler Drop;

		public static void Reset()
		{
			DragItems = null;
		}

		public static void RaiseBeginDrag(Object[] dragItems)
		{
			DragItems = dragItems;
			if (DragDrop.BeginDrag != null)
			{
				DragDrop.BeginDrag();
			}
		}

		public static void RaiseDrop()
		{
			if (DragDrop.Drop != null)
			{
				DragDrop.Drop();
			}
			DragItems = null;
		}
	}
}
