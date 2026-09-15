using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAnimation : PersistentBehaviour
	{
		public long clip;

		public bool playAutomatically;

		public uint wrapMode;

		public bool animatePhysics;

		public uint cullingType;

		public Bounds localBounds;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Animation animation = (Animation)obj;
			animation.clip = (AnimationClip)objects.Get(clip);
			animation.playAutomatically = playAutomatically;
			animation.wrapMode = (WrapMode)wrapMode;
			animation.animatePhysics = animatePhysics;
			animation.cullingType = (AnimationCullingType)cullingType;
			animation.localBounds = localBounds;
			return animation;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Animation animation = (Animation)obj;
				clip = animation.clip.GetMappedInstanceID();
				playAutomatically = animation.playAutomatically;
				wrapMode = (uint)animation.wrapMode;
				animatePhysics = animation.animatePhysics;
				cullingType = (uint)animation.cullingType;
				localBounds = animation.localBounds;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(clip, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Animation animation = (Animation)obj;
				AddDependency(animation.clip, dependencies);
			}
		}
	}
}
