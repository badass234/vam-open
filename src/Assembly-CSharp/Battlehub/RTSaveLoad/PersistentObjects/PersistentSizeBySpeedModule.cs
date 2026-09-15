using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentSizeBySpeedModule : PersistentData
	{
		public bool enabled;

		public PersistentMinMaxCurve size;

		public float sizeMultiplier;

		public PersistentMinMaxCurve x;

		public float xMultiplier;

		public PersistentMinMaxCurve y;

		public float yMultiplier;

		public PersistentMinMaxCurve z;

		public float zMultiplier;

		public bool separateAxes;

		public Vector2 range;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.SizeBySpeedModule sizeBySpeedModule = (ParticleSystem.SizeBySpeedModule)obj;
			sizeBySpeedModule.enabled = enabled;
			sizeBySpeedModule.size = Write(sizeBySpeedModule.size, size, objects);
			sizeBySpeedModule.sizeMultiplier = sizeMultiplier;
			sizeBySpeedModule.x = Write(sizeBySpeedModule.x, x, objects);
			sizeBySpeedModule.xMultiplier = xMultiplier;
			sizeBySpeedModule.y = Write(sizeBySpeedModule.y, y, objects);
			sizeBySpeedModule.yMultiplier = yMultiplier;
			sizeBySpeedModule.z = Write(sizeBySpeedModule.z, z, objects);
			sizeBySpeedModule.zMultiplier = zMultiplier;
			sizeBySpeedModule.separateAxes = separateAxes;
			sizeBySpeedModule.range = range;
			return sizeBySpeedModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.SizeBySpeedModule sizeBySpeedModule = (ParticleSystem.SizeBySpeedModule)obj;
				enabled = sizeBySpeedModule.enabled;
				size = Read(size, sizeBySpeedModule.size);
				sizeMultiplier = sizeBySpeedModule.sizeMultiplier;
				x = Read(x, sizeBySpeedModule.x);
				xMultiplier = sizeBySpeedModule.xMultiplier;
				y = Read(y, sizeBySpeedModule.y);
				yMultiplier = sizeBySpeedModule.yMultiplier;
				z = Read(z, sizeBySpeedModule.z);
				zMultiplier = sizeBySpeedModule.zMultiplier;
				separateAxes = sizeBySpeedModule.separateAxes;
				range = sizeBySpeedModule.range;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(size, dependencies, objects, allowNulls);
			FindDependencies(x, dependencies, objects, allowNulls);
			FindDependencies(y, dependencies, objects, allowNulls);
			FindDependencies(z, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.SizeBySpeedModule sizeBySpeedModule = (ParticleSystem.SizeBySpeedModule)obj;
				GetDependencies(size, sizeBySpeedModule.size, dependencies);
				GetDependencies(x, sizeBySpeedModule.x, dependencies);
				GetDependencies(y, sizeBySpeedModule.y, dependencies);
				GetDependencies(z, sizeBySpeedModule.z, dependencies);
			}
		}
	}
}
