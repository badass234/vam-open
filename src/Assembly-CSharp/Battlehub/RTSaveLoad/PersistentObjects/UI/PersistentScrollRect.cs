using System;
using System.Collections.Generic;
using Battlehub.RTSaveLoad.PersistentObjects.EventSystems;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentScrollRect : PersistentUIBehaviour
	{
		public long content;

		public bool horizontal;

		public bool vertical;

		public ScrollRect.MovementType movementType;

		public float elasticity;

		public bool inertia;

		public float decelerationRate;

		public float scrollSensitivity;

		public long viewport;

		public long horizontalScrollbar;

		public long verticalScrollbar;

		public ScrollRect.ScrollbarVisibility horizontalScrollbarVisibility;

		public ScrollRect.ScrollbarVisibility verticalScrollbarVisibility;

		public float horizontalScrollbarSpacing;

		public float verticalScrollbarSpacing;

		public PersistentUnityEventBase onValueChanged;

		public Vector2 velocity;

		public Vector2 normalizedPosition;

		public float horizontalNormalizedPosition;

		public float verticalNormalizedPosition;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ScrollRect scrollRect = (ScrollRect)obj;
			scrollRect.content = (RectTransform)objects.Get(content);
			scrollRect.horizontal = horizontal;
			scrollRect.vertical = vertical;
			scrollRect.movementType = movementType;
			scrollRect.elasticity = elasticity;
			scrollRect.inertia = inertia;
			scrollRect.decelerationRate = decelerationRate;
			scrollRect.scrollSensitivity = scrollSensitivity;
			scrollRect.viewport = (RectTransform)objects.Get(viewport);
			scrollRect.horizontalScrollbar = (Scrollbar)objects.Get(horizontalScrollbar);
			scrollRect.verticalScrollbar = (Scrollbar)objects.Get(verticalScrollbar);
			scrollRect.horizontalScrollbarVisibility = horizontalScrollbarVisibility;
			scrollRect.verticalScrollbarVisibility = verticalScrollbarVisibility;
			scrollRect.horizontalScrollbarSpacing = horizontalScrollbarSpacing;
			scrollRect.verticalScrollbarSpacing = verticalScrollbarSpacing;
			onValueChanged.WriteTo(scrollRect.onValueChanged, objects);
			scrollRect.velocity = velocity;
			scrollRect.normalizedPosition = normalizedPosition;
			scrollRect.horizontalNormalizedPosition = horizontalNormalizedPosition;
			scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
			return scrollRect;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ScrollRect scrollRect = (ScrollRect)obj;
				content = scrollRect.content.GetMappedInstanceID();
				horizontal = scrollRect.horizontal;
				vertical = scrollRect.vertical;
				movementType = scrollRect.movementType;
				elasticity = scrollRect.elasticity;
				inertia = scrollRect.inertia;
				decelerationRate = scrollRect.decelerationRate;
				scrollSensitivity = scrollRect.scrollSensitivity;
				viewport = scrollRect.viewport.GetMappedInstanceID();
				horizontalScrollbar = scrollRect.horizontalScrollbar.GetMappedInstanceID();
				verticalScrollbar = scrollRect.verticalScrollbar.GetMappedInstanceID();
				horizontalScrollbarVisibility = scrollRect.horizontalScrollbarVisibility;
				verticalScrollbarVisibility = scrollRect.verticalScrollbarVisibility;
				horizontalScrollbarSpacing = scrollRect.horizontalScrollbarSpacing;
				verticalScrollbarSpacing = scrollRect.verticalScrollbarSpacing;
				onValueChanged = new PersistentUnityEventBase();
				onValueChanged.ReadFrom(scrollRect.onValueChanged);
				velocity = scrollRect.velocity;
				normalizedPosition = scrollRect.normalizedPosition;
				horizontalNormalizedPosition = scrollRect.horizontalNormalizedPosition;
				verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(content, dependencies, objects, allowNulls);
			AddDependency(viewport, dependencies, objects, allowNulls);
			AddDependency(horizontalScrollbar, dependencies, objects, allowNulls);
			AddDependency(verticalScrollbar, dependencies, objects, allowNulls);
			onValueChanged.FindDependencies(dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ScrollRect scrollRect = (ScrollRect)obj;
				AddDependency(scrollRect.content, dependencies);
				AddDependency(scrollRect.viewport, dependencies);
				AddDependency(scrollRect.horizontalScrollbar, dependencies);
				AddDependency(scrollRect.verticalScrollbar, dependencies);
				PersistentUnityEventBase persistentUnityEventBase = new PersistentUnityEventBase();
				persistentUnityEventBase.GetDependencies(scrollRect.onValueChanged, dependencies);
			}
		}
	}
}
