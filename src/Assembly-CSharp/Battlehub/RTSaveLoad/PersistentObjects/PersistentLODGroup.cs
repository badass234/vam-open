using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentLODGroup : PersistentComponent
	{
		public Vector3 localReferencePoint;

		public float size;

		public uint fadeMode;

		public bool animateCrossFading;

		public bool enabled;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			LODGroup lODGroup = (LODGroup)obj;
			lODGroup.localReferencePoint = localReferencePoint;
			lODGroup.size = size;
			lODGroup.fadeMode = (LODFadeMode)fadeMode;
			lODGroup.animateCrossFading = animateCrossFading;
			lODGroup.enabled = enabled;
			return lODGroup;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				LODGroup lODGroup = (LODGroup)obj;
				localReferencePoint = lODGroup.localReferencePoint;
				size = lODGroup.size;
				fadeMode = (uint)lODGroup.fadeMode;
				animateCrossFading = lODGroup.animateCrossFading;
				enabled = lODGroup.enabled;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
