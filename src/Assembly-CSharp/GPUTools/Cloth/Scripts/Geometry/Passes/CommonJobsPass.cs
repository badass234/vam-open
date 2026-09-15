using System.Collections.Generic;
using System.Linq;
using GPUTools.Common.Scripts.Tools.Commands;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Geometry.Passes
{
	public class CommonJobsPass : ICacheCommand
	{
		private readonly ClothSettings settings;

		public CommonJobsPass(ClothSettings settings)
		{
			this.settings = settings;
		}

		public void Cache()
		{
			settings.GeometryData.ParticlesBlend = Enumerable.Repeat(0f, settings.MeshProvider.MeshForImport.vertexCount).ToArray();
			settings.GeometryData.ParticlesStrength = Enumerable.Repeat(0.1f, settings.MeshProvider.MeshForImport.vertexCount).ToArray();
			settings.GeometryData.AllTringles = GetAllTriangles();
		}

		private int[] GetAllTriangles()
		{
			Mesh meshForImport = settings.MeshProvider.MeshForImport;
			List<int> list = new List<int>();
			for (int i = 0; i < meshForImport.subMeshCount; i++)
			{
				list.AddRange(meshForImport.GetTriangles(i));
			}
			return list.ToArray();
		}
	}
}
