using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentInheritVelocityModule : PersistentData
	{
		public bool enabled;

		public uint mode;

		public PersistentMinMaxCurve curve;

		public float curveMultiplier;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.InheritVelocityModule inheritVelocityModule = (ParticleSystem.InheritVelocityModule)obj;
			inheritVelocityModule.enabled = enabled;
			inheritVelocityModule.mode = (ParticleSystemInheritVelocityMode)mode;
			inheritVelocityModule.curve = Write(inheritVelocityModule.curve, curve, objects);
			inheritVelocityModule.curveMultiplier = curveMultiplier;
			return inheritVelocityModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.InheritVelocityModule inheritVelocityModule = (ParticleSystem.InheritVelocityModule)obj;
				enabled = inheritVelocityModule.enabled;
				mode = (uint)inheritVelocityModule.mode;
				curve = Read(curve, inheritVelocityModule.curve);
				curveMultiplier = inheritVelocityModule.curveMultiplier;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(curve, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.InheritVelocityModule inheritVelocityModule = (ParticleSystem.InheritVelocityModule)obj;
				GetDependencies(curve, inheritVelocityModule.curve, dependencies);
			}
		}
	}
}
