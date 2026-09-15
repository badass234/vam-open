using System;
using System.Collections.Generic;
using Battlehub.RTSaveLoad.PersistentObjects.EventSystems;
using Battlehub.RTSaveLoad.PersistentObjects.Networking.Match;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1069, typeof(PersistentNetworkMatch))]
	[ProtoInclude(1070, typeof(PersistentEventTrigger))]
	[ProtoInclude(1071, typeof(PersistentUIBehaviour))]
	public class PersistentMonoBehaviour : PersistentBehaviour
	{
		public bool useGUILayout;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			MonoBehaviour monoBehaviour = (MonoBehaviour)obj;
			monoBehaviour.useGUILayout = useGUILayout;
			return monoBehaviour;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				MonoBehaviour monoBehaviour = (MonoBehaviour)obj;
				useGUILayout = monoBehaviour.useGUILayout;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
