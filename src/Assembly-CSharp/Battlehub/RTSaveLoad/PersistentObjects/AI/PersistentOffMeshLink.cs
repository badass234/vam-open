using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AI;

namespace Battlehub.RTSaveLoad.PersistentObjects.AI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentOffMeshLink : PersistentBehaviour
	{
		public bool activated;

		public float costOverride;

		public bool biDirectional;

		public int area;

		public bool autoUpdatePositions;

		public long startTransform;

		public long endTransform;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			OffMeshLink offMeshLink = (OffMeshLink)obj;
			offMeshLink.activated = activated;
			offMeshLink.costOverride = costOverride;
			offMeshLink.biDirectional = biDirectional;
			offMeshLink.area = area;
			offMeshLink.autoUpdatePositions = autoUpdatePositions;
			offMeshLink.startTransform = (Transform)objects.Get(startTransform);
			offMeshLink.endTransform = (Transform)objects.Get(endTransform);
			return offMeshLink;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				OffMeshLink offMeshLink = (OffMeshLink)obj;
				activated = offMeshLink.activated;
				costOverride = offMeshLink.costOverride;
				biDirectional = offMeshLink.biDirectional;
				area = offMeshLink.area;
				autoUpdatePositions = offMeshLink.autoUpdatePositions;
				startTransform = offMeshLink.startTransform.GetMappedInstanceID();
				endTransform = offMeshLink.endTransform.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(startTransform, dependencies, objects, allowNulls);
			AddDependency(endTransform, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				OffMeshLink offMeshLink = (OffMeshLink)obj;
				AddDependency(offMeshLink.startTransform, dependencies);
				AddDependency(offMeshLink.endTransform, dependencies);
			}
		}
	}
}
