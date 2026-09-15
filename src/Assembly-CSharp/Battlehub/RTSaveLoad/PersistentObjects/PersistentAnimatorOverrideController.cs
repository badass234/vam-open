using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAnimatorOverrideController : PersistentRuntimeAnimatorController
	{
		public long runtimeAnimatorController;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AnimatorOverrideController animatorOverrideController = (AnimatorOverrideController)obj;
			animatorOverrideController.runtimeAnimatorController = (RuntimeAnimatorController)objects.Get(runtimeAnimatorController);
			return animatorOverrideController;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AnimatorOverrideController animatorOverrideController = (AnimatorOverrideController)obj;
				runtimeAnimatorController = animatorOverrideController.runtimeAnimatorController.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(runtimeAnimatorController, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				AnimatorOverrideController animatorOverrideController = (AnimatorOverrideController)obj;
				AddDependency(animatorOverrideController.runtimeAnimatorController, dependencies);
			}
		}
	}
}
