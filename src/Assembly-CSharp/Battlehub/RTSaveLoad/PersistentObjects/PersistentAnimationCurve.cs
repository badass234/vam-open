using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAnimationCurve : PersistentData
	{
		public PersistentKeyframe[] keys;

		public uint preWrapMode;

		public uint postWrapMode;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AnimationCurve animationCurve = (AnimationCurve)obj;
			animationCurve.keys = Write(animationCurve.keys, keys, objects);
			animationCurve.preWrapMode = (WrapMode)preWrapMode;
			animationCurve.postWrapMode = (WrapMode)postWrapMode;
			return animationCurve;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AnimationCurve animationCurve = (AnimationCurve)obj;
				keys = Read(keys, animationCurve.keys);
				preWrapMode = (uint)animationCurve.preWrapMode;
				postWrapMode = (uint)animationCurve.postWrapMode;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(keys, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				AnimationCurve animationCurve = (AnimationCurve)obj;
				GetDependencies(keys, animationCurve.keys, dependencies);
			}
		}
	}
}
