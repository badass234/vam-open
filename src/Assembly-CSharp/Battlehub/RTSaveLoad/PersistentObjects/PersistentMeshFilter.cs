using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentMeshFilter : PersistentComponent
	{
		public long sharedMesh;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			MeshFilter meshFilter = (MeshFilter)obj;
			meshFilter.sharedMesh = (Mesh)objects.Get(sharedMesh);
			return meshFilter;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				MeshFilter meshFilter = (MeshFilter)obj;
				sharedMesh = meshFilter.sharedMesh.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(sharedMesh, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				MeshFilter meshFilter = (MeshFilter)obj;
				AddDependency(meshFilter.sharedMesh, dependencies);
			}
		}
	}
}
