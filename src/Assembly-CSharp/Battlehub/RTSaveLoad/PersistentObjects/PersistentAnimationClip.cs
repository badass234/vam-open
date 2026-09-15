using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAnimationClip : PersistentMotion
	{
		public float frameRate;

		public uint wrapMode;

		public Bounds localBounds;

		public bool legacy;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AnimationClip animationClip = (AnimationClip)obj;
			animationClip.frameRate = frameRate;
			animationClip.wrapMode = (WrapMode)wrapMode;
			animationClip.localBounds = localBounds;
			animationClip.legacy = legacy;
			return animationClip;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AnimationClip animationClip = (AnimationClip)obj;
				frameRate = animationClip.frameRate;
				wrapMode = (uint)animationClip.wrapMode;
				localBounds = animationClip.localBounds;
				legacy = animationClip.legacy;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
