using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentMinMaxCurve : PersistentData
	{
		public uint mode;

		public float curveMultiplier;

		public PersistentAnimationCurve curveMax;

		public PersistentAnimationCurve curveMin;

		public float constantMax;

		public float constantMin;

		public float constant;

		public PersistentAnimationCurve curve;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.MinMaxCurve minMaxCurve = (ParticleSystem.MinMaxCurve)obj;
			minMaxCurve.mode = (ParticleSystemCurveMode)mode;
			minMaxCurve.curveMultiplier = curveMultiplier;
			minMaxCurve.curveMax = Write(minMaxCurve.curveMax, curveMax, objects);
			minMaxCurve.curveMin = Write(minMaxCurve.curveMin, curveMin, objects);
			minMaxCurve.constantMax = constantMax;
			minMaxCurve.constantMin = constantMin;
			minMaxCurve.constant = constant;
			minMaxCurve.curve = Write(minMaxCurve.curve, curve, objects);
			return minMaxCurve;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.MinMaxCurve minMaxCurve = (ParticleSystem.MinMaxCurve)obj;
				mode = (uint)minMaxCurve.mode;
				curveMultiplier = minMaxCurve.curveMultiplier;
				curveMax = Read(curveMax, minMaxCurve.curveMax);
				curveMin = Read(curveMin, minMaxCurve.curveMin);
				constantMax = minMaxCurve.constantMax;
				constantMin = minMaxCurve.constantMin;
				constant = minMaxCurve.constant;
				curve = Read(curve, minMaxCurve.curve);
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(curveMax, dependencies, objects, allowNulls);
			FindDependencies(curveMin, dependencies, objects, allowNulls);
			FindDependencies(curve, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.MinMaxCurve minMaxCurve = (ParticleSystem.MinMaxCurve)obj;
				GetDependencies(curveMax, minMaxCurve.curveMax, dependencies);
				GetDependencies(curveMin, minMaxCurve.curveMin, dependencies);
				GetDependencies(curve, minMaxCurve.curve, dependencies);
			}
		}
	}
}
