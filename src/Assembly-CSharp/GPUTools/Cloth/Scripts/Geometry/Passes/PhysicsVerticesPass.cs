using System.Collections.Generic;
using System.Linq;
using GPUTools.Cloth.Scripts.Geometry.Data;
using GPUTools.Common.Scripts.Tools.Commands;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Geometry.Passes
{
	public class PhysicsVerticesPass : ICacheCommand
	{
		private readonly Mesh mesh;

		private readonly ClothGeometryData data;

		private int[] meshToPhysicsVerticesMap;

		private int[] physicsToMeshVerticesMap;

		public PhysicsVerticesPass(ClothSettings settings)
		{
			data = settings.GeometryData;
			mesh = settings.MeshProvider.MeshForImport;
		}

		public void Cache()
		{
			if (data != null && mesh != null)
			{
				Vector3[] vertices = mesh.vertices;
				data.Particles = CreatePhysicsVerticesArray(vertices);
				CreateMeshToPhysicsVerticesMap(vertices, data.Particles);
				data.MeshToPhysicsVerticesMap = meshToPhysicsVerticesMap;
				data.PhysicsToMeshVerticesMap = physicsToMeshVerticesMap;
			}
		}

		private Vector3[] CreatePhysicsVerticesArray(Vector3[] vertices)
		{
			HashSet<Vector3> hashSet = new HashSet<Vector3>();
			for (int i = 0; i < vertices.Length; i++)
			{
				hashSet.Add(vertices[i]);
			}
			return hashSet.ToArray();
		}

		private void CreateMeshToPhysicsVerticesMap(Vector3[] vertices, Vector3[] physicsVertices)
		{
			Dictionary<Vector3, int> dictionary = new Dictionary<Vector3, int>();
			for (int i = 0; i < physicsVertices.Length; i++)
			{
				Vector3 key = physicsVertices[i];
				dictionary.Add(key, i);
			}
			meshToPhysicsVerticesMap = new int[vertices.Length];
			physicsToMeshVerticesMap = new int[physicsVertices.Length];
			for (int j = 0; j < vertices.Length; j++)
			{
				Vector3 key2 = vertices[j];
				if (dictionary.ContainsKey(key2))
				{
					int num = dictionary[key2];
					meshToPhysicsVerticesMap[j] = num;
					physicsToMeshVerticesMap[num] = j;
				}
				else
				{
					meshToPhysicsVerticesMap[j] = -1;
				}
			}
		}
	}
}
