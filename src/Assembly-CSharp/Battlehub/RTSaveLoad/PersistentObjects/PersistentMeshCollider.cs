using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentMeshCollider : PersistentCollider
	{
		public long sharedMesh;

		public bool convex;

		public bool inflateMesh;

		public float skinWidth;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			MeshCollider meshCollider = (MeshCollider)obj;
			meshCollider.sharedMesh = (Mesh)objects.Get(sharedMesh);
			meshCollider.convex = convex;
			meshCollider.inflateMesh = inflateMesh;
			meshCollider.skinWidth = skinWidth;
			return meshCollider;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				MeshCollider meshCollider = (MeshCollider)obj;
				sharedMesh = meshCollider.sharedMesh.GetMappedInstanceID();
				convex = meshCollider.convex;
				inflateMesh = meshCollider.inflateMesh;
				skinWidth = meshCollider.skinWidth;
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
				MeshCollider meshCollider = (MeshCollider)obj;
				AddDependency(meshCollider.sharedMesh, dependencies);
			}
		}
	}
}
