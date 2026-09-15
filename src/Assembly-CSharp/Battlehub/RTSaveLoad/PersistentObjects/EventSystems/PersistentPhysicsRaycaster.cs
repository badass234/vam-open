using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTSaveLoad.PersistentObjects.EventSystems
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1128, typeof(PersistentPhysics2DRaycaster))]
	public class PersistentPhysicsRaycaster : PersistentBaseRaycaster
	{
		public LayerMask eventMask;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			PhysicsRaycaster physicsRaycaster = (PhysicsRaycaster)obj;
			physicsRaycaster.eventMask = eventMask;
			return physicsRaycaster;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				PhysicsRaycaster physicsRaycaster = (PhysicsRaycaster)obj;
				eventMask = physicsRaycaster.eventMask;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
