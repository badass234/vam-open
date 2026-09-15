using GPUTools.Cloth.Scripts.Types;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Commands
{
	public class BuildVertexData : BuildChainCommand
	{
		private readonly ClothSettings settings;

		public BuildVertexData(ClothSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			settings.Runtime.MeshVertexToNeiborsMap = new GpuBuffer<int>(settings.GeometryData.ParticleToNeibor, 4);
			settings.Runtime.MeshVertexToNeiborsMapCounts = new GpuBuffer<int>(settings.GeometryData.ParticleToNeiborCounts, 4);
			settings.Runtime.MeshToPhysicsVerticesMap = new GpuBuffer<int>(settings.GeometryData.MeshToPhysicsVerticesMap, 4);
			settings.Runtime.ClothVertices = new GpuBuffer<ClothVertex>(new ClothVertex[settings.GeometryData.MeshToPhysicsVerticesMap.Length], ClothVertex.Size());
			settings.Runtime.ClothOnlyVertices = new GpuBuffer<Vector3>(new Vector3[settings.GeometryData.MeshToPhysicsVerticesMap.Length], 12);
		}

		protected override void OnDispose()
		{
			settings.Runtime.MeshVertexToNeiborsMap.Dispose();
			settings.Runtime.MeshVertexToNeiborsMapCounts.Dispose();
			settings.Runtime.MeshToPhysicsVerticesMap.Dispose();
			settings.Runtime.ClothVertices.Dispose();
			settings.Runtime.ClothOnlyVertices.Dispose();
		}
	}
}
