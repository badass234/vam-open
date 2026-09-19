using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentTriggerModule : PersistentData
	{
		public bool enabled;

		public ParticleSystemOverlapAction inside;

		public ParticleSystemOverlapAction outside;

		public ParticleSystemOverlapAction enter;

		public ParticleSystemOverlapAction exit;

		public float radiusScale;

		public long[] colliders;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.TriggerModule triggerModule = (ParticleSystem.TriggerModule)obj;
			triggerModule.enabled = enabled;
			triggerModule.inside = inside;
			triggerModule.outside = outside;
			triggerModule.enter = enter;
			triggerModule.exit = exit;
			triggerModule.radiusScale = radiusScale;
			if (colliders == null)
			{
				for (int i = 0; i < triggerModule.colliderCount; i++)
				{
					triggerModule.SetCollider(i, null);
				}
			}
			else
			{
				for (int j = 0; j < Mathf.Min(triggerModule.colliderCount, colliders.Length); j++)
				{
					object obj2 = objects.Get(colliders[j]);
					triggerModule.SetCollider(j, (Component)obj2);
				}
			}
			return triggerModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.TriggerModule triggerModule = (ParticleSystem.TriggerModule)obj;
				enabled = triggerModule.enabled;
				inside = triggerModule.inside;
				outside = triggerModule.outside;
				enter = triggerModule.enter;
				exit = triggerModule.exit;
				radiusScale = triggerModule.radiusScale;
				if (triggerModule.colliderCount > 20)
				{
					Debug.LogWarning("maxPlaneCount is expected to be 6 or at least <= 20");
				}
				colliders = new long[triggerModule.colliderCount];
				for (int i = 0; i < triggerModule.colliderCount; i++)
				{
					Component collider = triggerModule.GetCollider(i);
					colliders[i] = collider.GetMappedInstanceID();
				}
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependencies(colliders, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.TriggerModule triggerModule = (ParticleSystem.TriggerModule)obj;
				UnityEngine.Object[] array = new UnityEngine.Object[triggerModule.colliderCount];
				for (int i = 0; i < triggerModule.colliderCount; i++)
				{
					array[i] = triggerModule.GetCollider(i);
				}
				AddDependencies(array, dependencies);
			}
		}
	}
}
