using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(RectTransform))]
	public class ReorderableListElement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IEventSystemHandler
	{
		[Tooltip("Can this element be dragged?")]
		public bool IsGrabbable = true;

		[Tooltip("Can this element be transfered to another list")]
		public bool IsTransferable = true;

		[Tooltip("Can this element be dropped in space?")]
		public bool isDroppableInSpace;

		private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

		private ReorderableList _currentReorderableListRaycasted;

		private RectTransform _draggingObject;

		private LayoutElement _draggingObjectLE;

		private Vector2 _draggingObjectOriginalSize;

		private RectTransform _fakeElement;

		private LayoutElement _fakeElementLE;

		private int _fromIndex;

		private bool _isDragging;

		private RectTransform _rect;

		private ReorderableList _reorderableList;

		internal bool isValid;

		public void OnBeginDrag(PointerEventData eventData)
		{
			isValid = true;
			if (_reorderableList == null)
			{
				return;
			}
			if (!_reorderableList.IsDraggable || !IsGrabbable)
			{
				_draggingObject = null;
				return;
			}
			if (!_reorderableList.CloneDraggedObject)
			{
				_draggingObject = _rect;
				_fromIndex = _rect.GetSiblingIndex();
				if (_reorderableList.OnElementRemoved != null)
				{
					_reorderableList.OnElementRemoved.Invoke(new ReorderableList.ReorderableListEventStruct
					{
						DroppedObject = _draggingObject.gameObject,
						IsAClone = _reorderableList.CloneDraggedObject,
						SourceObject = ((!_reorderableList.CloneDraggedObject) ? _draggingObject.gameObject : base.gameObject),
						FromList = _reorderableList,
						FromIndex = _fromIndex
					});
				}
				if (!isValid)
				{
					_draggingObject = null;
					return;
				}
			}
			else
			{
				GameObject gameObject = Object.Instantiate(base.gameObject);
				_draggingObject = gameObject.GetComponent<RectTransform>();
			}
			_draggingObjectOriginalSize = base.gameObject.GetComponent<RectTransform>().rect.size;
			_draggingObjectLE = _draggingObject.GetComponent<LayoutElement>();
			_draggingObject.SetParent(_reorderableList.DraggableArea, true);
			_draggingObject.SetAsLastSibling();
			_fakeElement = new GameObject("Fake").AddComponent<RectTransform>();
			_fakeElementLE = _fakeElement.gameObject.AddComponent<LayoutElement>();
			RefreshSizes();
			if (_reorderableList.OnElementGrabbed != null)
			{
				_reorderableList.OnElementGrabbed.Invoke(new ReorderableList.ReorderableListEventStruct
				{
					DroppedObject = _draggingObject.gameObject,
					IsAClone = _reorderableList.CloneDraggedObject,
					SourceObject = ((!_reorderableList.CloneDraggedObject) ? _draggingObject.gameObject : base.gameObject),
					FromList = _reorderableList,
					FromIndex = _fromIndex
				});
				if (!isValid)
				{
					CancelDrag();
					return;
				}
			}
			_isDragging = true;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (!_isDragging)
			{
				return;
			}
			if (!isValid)
			{
				CancelDrag();
				return;
			}
			Canvas componentInParent = _draggingObject.GetComponentInParent<Canvas>();
			Vector3 worldPoint;
			RectTransformUtility.ScreenPointToWorldPointInRectangle(componentInParent.GetComponent<RectTransform>(), eventData.position, componentInParent.worldCamera, out worldPoint);
			_draggingObject.position = worldPoint;
			EventSystem.current.RaycastAll(eventData, _raycastResults);
			for (int i = 0; i < _raycastResults.Count; i++)
			{
				_currentReorderableListRaycasted = _raycastResults[i].gameObject.GetComponent<ReorderableList>();
				if (_currentReorderableListRaycasted != null)
				{
					break;
				}
			}
			if (_currentReorderableListRaycasted == null || !_currentReorderableListRaycasted.IsDropable)
			{
				RefreshSizes();
				_fakeElement.transform.SetParent(_reorderableList.DraggableArea, false);
				return;
			}
			if (_fakeElement.parent != _currentReorderableListRaycasted)
			{
				_fakeElement.SetParent(_currentReorderableListRaycasted.Content, false);
			}
			float num = float.PositiveInfinity;
			int siblingIndex = 0;
			float num2 = 0f;
			for (int j = 0; j < _currentReorderableListRaycasted.Content.childCount; j++)
			{
				RectTransform component = _currentReorderableListRaycasted.Content.GetChild(j).GetComponent<RectTransform>();
				if (_currentReorderableListRaycasted.ContentLayout is VerticalLayoutGroup)
				{
					num2 = Mathf.Abs(component.position.y - worldPoint.y);
				}
				else if (_currentReorderableListRaycasted.ContentLayout is HorizontalLayoutGroup)
				{
					num2 = Mathf.Abs(component.position.x - worldPoint.x);
				}
				else if (_currentReorderableListRaycasted.ContentLayout is GridLayoutGroup)
				{
					num2 = Mathf.Abs(component.position.x - worldPoint.x) + Mathf.Abs(component.position.y - worldPoint.y);
				}
				if (num2 < num)
				{
					num = num2;
					siblingIndex = j;
				}
			}
			RefreshSizes();
			_fakeElement.SetSiblingIndex(siblingIndex);
			_fakeElement.gameObject.SetActive(true);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_isDragging = false;
			if (_draggingObject != null)
			{
				if (_currentReorderableListRaycasted != null && _currentReorderableListRaycasted.IsDropable && (IsTransferable || _currentReorderableListRaycasted == _reorderableList))
				{
					ReorderableList.ReorderableListEventStruct reorderableListEventStruct = default(ReorderableList.ReorderableListEventStruct);
					reorderableListEventStruct.DroppedObject = _draggingObject.gameObject;
					reorderableListEventStruct.IsAClone = _reorderableList.CloneDraggedObject;
					reorderableListEventStruct.SourceObject = ((!_reorderableList.CloneDraggedObject) ? _draggingObject.gameObject : base.gameObject);
					reorderableListEventStruct.FromList = _reorderableList;
					reorderableListEventStruct.FromIndex = _fromIndex;
					reorderableListEventStruct.ToList = _currentReorderableListRaycasted;
					reorderableListEventStruct.ToIndex = _fakeElement.GetSiblingIndex();
					ReorderableList.ReorderableListEventStruct arg = reorderableListEventStruct;
					if ((bool)_reorderableList && _reorderableList.OnElementDropped != null)
					{
						_reorderableList.OnElementDropped.Invoke(arg);
					}
					if (!isValid)
					{
						CancelDrag();
						return;
					}
					RefreshSizes();
					_draggingObject.SetParent(_currentReorderableListRaycasted.Content, false);
					_draggingObject.rotation = _currentReorderableListRaycasted.transform.rotation;
					_draggingObject.SetSiblingIndex(_fakeElement.GetSiblingIndex());
					_reorderableList.OnElementAdded.Invoke(arg);
					if (!isValid)
					{
						throw new Exception("It's too late to cancel the Transfer! Do so in OnElementDropped!");
					}
				}
				else if (isDroppableInSpace)
				{
					_reorderableList.OnElementDropped.Invoke(new ReorderableList.ReorderableListEventStruct
					{
						DroppedObject = _draggingObject.gameObject,
						IsAClone = _reorderableList.CloneDraggedObject,
						SourceObject = ((!_reorderableList.CloneDraggedObject) ? _draggingObject.gameObject : base.gameObject),
						FromList = _reorderableList,
						FromIndex = _fromIndex
					});
				}
				else
				{
					CancelDrag();
				}
			}
			if (_fakeElement != null)
			{
				Object.Destroy(_fakeElement.gameObject);
			}
		}

		private void CancelDrag()
		{
			_isDragging = false;
			if (_reorderableList.CloneDraggedObject)
			{
				Object.Destroy(_draggingObject.gameObject);
			}
			else
			{
				RefreshSizes();
				_draggingObject.SetParent(_reorderableList.Content, false);
				_draggingObject.rotation = _reorderableList.Content.transform.rotation;
				_draggingObject.SetSiblingIndex(_fromIndex);
				ReorderableList.ReorderableListEventStruct reorderableListEventStruct = default(ReorderableList.ReorderableListEventStruct);
				reorderableListEventStruct.DroppedObject = _draggingObject.gameObject;
				reorderableListEventStruct.IsAClone = _reorderableList.CloneDraggedObject;
				reorderableListEventStruct.SourceObject = ((!_reorderableList.CloneDraggedObject) ? _draggingObject.gameObject : base.gameObject);
				reorderableListEventStruct.FromList = _reorderableList;
				reorderableListEventStruct.FromIndex = _fromIndex;
				reorderableListEventStruct.ToList = _reorderableList;
				reorderableListEventStruct.ToIndex = _fromIndex;
				ReorderableList.ReorderableListEventStruct arg = reorderableListEventStruct;
				_reorderableList.OnElementAdded.Invoke(arg);
				if (!isValid)
				{
					throw new Exception("Transfer is already Cancelled.");
				}
			}
			if (_fakeElement != null)
			{
				Object.Destroy(_fakeElement.gameObject);
			}
		}

		private void RefreshSizes()
		{
			Vector2 sizeDelta = _draggingObjectOriginalSize;
			if (_currentReorderableListRaycasted != null && _currentReorderableListRaycasted.IsDropable && _currentReorderableListRaycasted.Content.childCount > 0)
			{
				Transform child = _currentReorderableListRaycasted.Content.GetChild(0);
				if (child != null)
				{
					sizeDelta = child.GetComponent<RectTransform>().rect.size;
				}
			}
			_draggingObject.sizeDelta = sizeDelta;
			LayoutElement fakeElementLE = _fakeElementLE;
			float y = sizeDelta.y;
			_draggingObjectLE.preferredHeight = y;
			fakeElementLE.preferredHeight = y;
			LayoutElement fakeElementLE2 = _fakeElementLE;
			y = sizeDelta.x;
			_draggingObjectLE.preferredWidth = y;
			fakeElementLE2.preferredWidth = y;
		}

		public void Init(ReorderableList reorderableList)
		{
			_reorderableList = reorderableList;
			_rect = GetComponent<RectTransform>();
		}
	}
}
