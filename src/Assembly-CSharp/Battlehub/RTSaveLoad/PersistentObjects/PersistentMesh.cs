using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentMesh : PersistentObject
	{
		public IntArray[] m_tris;

		public Bounds bounds;

		public int subMeshCount;

		public BoneWeight[] boneWeights;

		public Matrix4x4[] bindposes;

		public Vector3[] vertices;

		public Vector3[] normals;

		public Vector4[] tangents;

		public Vector2[] uv;

		public Vector2[] uv2;

		public Vector2[] uv3;

		public Vector2[] uv4;

		public Color[] colors;

		public int[] triangles;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Mesh mesh = (Mesh)obj;
			mesh.vertices = vertices;
			mesh.subMeshCount = subMeshCount;
			if (m_tris != null)
			{
				for (int i = 0; i < subMeshCount; i++)
				{
					mesh.SetTriangles(m_tris[i].Array, i);
				}
			}
			mesh.bounds = bounds;
			mesh.boneWeights = boneWeights;
			mesh.bindposes = bindposes;
			mesh.normals = normals;
			mesh.tangents = tangents;
			mesh.uv = uv;
			mesh.uv2 = uv2;
			mesh.uv3 = uv3;
			mesh.uv4 = uv4;
			mesh.colors = colors;
			return mesh;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Mesh mesh = (Mesh)obj;
				bounds = mesh.bounds;
				subMeshCount = mesh.subMeshCount;
				boneWeights = mesh.boneWeights;
				bindposes = mesh.bindposes;
				vertices = mesh.vertices;
				normals = mesh.normals;
				tangents = mesh.tangents;
				uv = mesh.uv;
				uv2 = mesh.uv2;
				uv3 = mesh.uv3;
				uv4 = mesh.uv4;
				colors = mesh.colors;
				m_tris = new IntArray[subMeshCount];
				for (int i = 0; i < subMeshCount; i++)
				{
					m_tris[i] = new IntArray();
					m_tris[i].Array = mesh.GetTriangles(i);
				}
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
