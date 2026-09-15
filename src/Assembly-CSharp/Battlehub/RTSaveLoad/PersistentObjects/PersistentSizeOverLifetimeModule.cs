using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentSizeOverLifetimeModule : PersistentData
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

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule = (ParticleSystem.SizeOverLifetimeModule)obj;
			sizeOverLifetimeModule.enabled = enabled;
			sizeOverLifetimeModule.size = Write(sizeOverLifetimeModule.size, size, objects);
			sizeOverLifetimeModule.sizeMultiplier = sizeMultiplier;
			sizeOverLifetimeModule.x = Write(sizeOverLifetimeModule.x, x, objects);
			sizeOverLifetimeModule.xMultiplier = xMultiplier;
			sizeOverLifetimeModule.y = Write(sizeOverLifetimeModule.y, y, objects);
			sizeOverLifetimeModule.yMultiplier = yMultiplier;
			sizeOverLifetimeModule.z = Write(sizeOverLifetimeModule.z, z, objects);
			sizeOverLifetimeModule.zMultiplier = zMultiplier;
			sizeOverLifetimeModule.separateAxes = separateAxes;
			return sizeOverLifetimeModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule = (ParticleSystem.SizeOverLifetimeModule)obj;
				enabled = sizeOverLifetimeModule.enabled;
				size = Read(size, sizeOverLifetimeModule.size);
				sizeMultiplier = sizeOverLifetimeModule.sizeMultiplier;
				x = Read(x, sizeOverLifetimeModule.x);
				xMultiplier = sizeOverLifetimeModule.xMultiplier;
				y = Read(y, sizeOverLifetimeModule.y);
				yMultiplier = sizeOverLifetimeModule.yMultiplier;
				z = Read(z, sizeOverLifetimeModule.z);
				zMultiplier = sizeOverLifetimeModule.zMultiplier;
				separateAxes = sizeOverLifetimeModule.separateAxes;
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
				ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule = (ParticleSystem.SizeOverLifetimeModule)obj;
				GetDependencies(size, sizeOverLifetimeModule.size, dependencies);
				GetDependencies(x, sizeOverLifetimeModule.x, dependencies);
				GetDependencies(y, sizeOverLifetimeModule.y, dependencies);
				GetDependencies(z, sizeOverLifetimeModule.z, dependencies);
			}
		}
	}
}
