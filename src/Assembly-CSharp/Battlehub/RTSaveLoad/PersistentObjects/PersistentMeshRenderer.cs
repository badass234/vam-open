using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentMeshRenderer : PersistentRenderer
	{
		public long additionalVertexStreams;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			MeshRenderer meshRenderer = (MeshRenderer)obj;
			meshRenderer.additionalVertexStreams = (Mesh)objects.Get(additionalVertexStreams);
			return meshRenderer;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				MeshRenderer meshRenderer = (MeshRenderer)obj;
				additionalVertexStreams = meshRenderer.additionalVertexStreams.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(additionalVertexStreams, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				MeshRenderer meshRenderer = (MeshRenderer)obj;
				AddDependency(meshRenderer.additionalVertexStreams, dependencies);
			}
		}
	}
}
