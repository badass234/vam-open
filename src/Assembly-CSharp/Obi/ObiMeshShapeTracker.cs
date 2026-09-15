using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Obi
{
	public class ObiMeshShapeTracker : ObiShapeTracker
	{
		private class MeshDataHandles
		{
			private int refCount = 1;

			private GCHandle verticesHandle;

			private GCHandle indicesHandle;

			public int RefCount => refCount;

			public IntPtr VerticesAddress => verticesHandle.AddrOfPinnedObject();

			public IntPtr IndicesAddress => indicesHandle.AddrOfPinnedObject();

			public void FromMesh(Mesh mesh)
			{
				Oni.UnpinMemory(verticesHandle);
				Oni.UnpinMemory(indicesHandle);
				verticesHandle = Oni.PinMemory(mesh.vertices);
				indicesHandle = Oni.PinMemory(mesh.triangles);
			}

			public void Ref()
			{
				refCount++;
			}

			public void Unref()
			{
				refCount--;
				if (refCount <= 0)
				{
					refCount = 0;
					Oni.UnpinMemory(verticesHandle);
					Oni.UnpinMemory(indicesHandle);
				}
			}
		}

		private static Dictionary<Mesh, MeshDataHandles> meshDataCache = new Dictionary<Mesh, MeshDataHandles>();

		private bool meshDataHasChanged;

		private MeshDataHandles handles;

		public ObiMeshShapeTracker(MeshCollider collider)
		{
			base.collider = collider;
			adaptor.is2D = false;
			oniShape = Oni.CreateShape(Oni.ShapeType.TriangleMesh);
			UpdateMeshData();
		}

		public void UpdateMeshData()
		{
			MeshCollider meshCollider = collider as MeshCollider;
			if (meshCollider != null)
			{
				Mesh sharedMesh = meshCollider.sharedMesh;
				if (handles != null)
				{
					handles.Unref();
				}
				MeshDataHandles value;
				if (!meshDataCache.TryGetValue(sharedMesh, out value))
				{
					handles = new MeshDataHandles();
					meshDataCache[sharedMesh] = handles;
				}
				else
				{
					value.Ref();
					handles = value;
				}
				handles.FromMesh(meshCollider.sharedMesh);
				meshDataHasChanged = true;
			}
		}

		public override void UpdateIfNeeded()
		{
			MeshCollider meshCollider = collider as MeshCollider;
			if (meshCollider != null)
			{
				Mesh sharedMesh = meshCollider.sharedMesh;
				if (sharedMesh != null && meshDataHasChanged)
				{
					meshDataHasChanged = false;
					adaptor.Set(handles.VerticesAddress, handles.IndicesAddress, sharedMesh.vertexCount, sharedMesh.triangles.Length);
					Oni.UpdateShape(oniShape, ref adaptor);
				}
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			MeshCollider meshCollider = collider as MeshCollider;
			if (meshCollider != null && handles != null)
			{
				handles.Unref();
				if (handles.RefCount <= 0)
				{
					meshDataCache.Remove(meshCollider.sharedMesh);
				}
			}
		}
	}
}
