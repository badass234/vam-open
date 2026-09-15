using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTSaveLoad.PersistentObjects.EventSystems
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentEventSystem : PersistentUIBehaviour
	{
		public bool sendNavigationEvents;

		public int pixelDragThreshold;

		public long firstSelectedGameObject;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			EventSystem eventSystem = (EventSystem)obj;
			eventSystem.sendNavigationEvents = sendNavigationEvents;
			eventSystem.pixelDragThreshold = pixelDragThreshold;
			eventSystem.firstSelectedGameObject = (GameObject)objects.Get(firstSelectedGameObject);
			return eventSystem;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				EventSystem eventSystem = (EventSystem)obj;
				sendNavigationEvents = eventSystem.sendNavigationEvents;
				pixelDragThreshold = eventSystem.pixelDragThreshold;
				firstSelectedGameObject = eventSystem.firstSelectedGameObject.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(firstSelectedGameObject, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				EventSystem eventSystem = (EventSystem)obj;
				AddDependency(eventSystem.firstSelectedGameObject, dependencies);
			}
		}
	}
}
