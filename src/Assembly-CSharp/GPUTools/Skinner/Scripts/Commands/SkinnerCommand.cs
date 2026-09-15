using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Common.Scripts.Tools.Kernels;
using GPUTools.Skinner.Scripts.Providers;
using UnityEngine;

namespace GPUTools.Skinner.Scripts.Commands
{
	public class SkinnerCommand : IBuildCommand
	{
		private readonly SkinnedMeshProvider provider;

		private readonly int[] indices;

		private List<KernelBase> kernels = new List<KernelBase>();

		public GpuBuffer<int> Indices { get; set; }

		public GpuBuffer<Matrix4x4> Matrices { get; set; }

		public GpuBuffer<Vector3> LocalPoints { get; set; }

		public GpuBuffer<Vector3> Points { get; set; }

		public GpuBuffer<Matrix4x4> SelectedMatrices { get; set; }

		public GpuBuffer<Vector3> SelectedPoints { get; set; }

		public SkinnerCommand(SkinnedMeshProvider provider, int[] indices)
		{
			this.provider = provider;
			this.indices = indices;
		}

		public void Build()
		{
			Matrices = provider.ToWorldMatricesBuffer;
			LocalPoints = new GpuBuffer<Vector3>(provider.Mesh.vertices, 12);
			Points = new GpuBuffer<Vector3>(provider.Mesh.vertexCount, 12);
			GPUMatrixPointMultiplier gPUMatrixPointMultiplier = new GPUMatrixPointMultiplier();
			gPUMatrixPointMultiplier.InPoints = LocalPoints;
			gPUMatrixPointMultiplier.OutPoints = Points;
			gPUMatrixPointMultiplier.Matrices = Matrices;
			GPUMatrixPointMultiplier item = gPUMatrixPointMultiplier;
			kernels.Add(item);
			if (indices != null && indices.Length > 0)
			{
				Indices = new GpuBuffer<int>(indices, 4);
				SelectedPoints = new GpuBuffer<Vector3>(indices.Length, 12);
				SelectedMatrices = new GpuBuffer<Matrix4x4>(indices.Length, 64);
				GPUMatrixSelector gPUMatrixSelector = new GPUMatrixSelector();
				gPUMatrixSelector.Indices = Indices;
				gPUMatrixSelector.Matrices = Matrices;
				gPUMatrixSelector.SelectedMatrices = SelectedMatrices;
				GPUMatrixSelector item2 = gPUMatrixSelector;
				GPUPointsSelector gPUPointsSelector = new GPUPointsSelector();
				gPUPointsSelector.Indices = Indices;
				gPUPointsSelector.Points = Points;
				gPUPointsSelector.SelectedPoints = SelectedPoints;
				GPUPointsSelector item3 = gPUPointsSelector;
				kernels.Add(item2);
				kernels.Add(item3);
			}
		}

		public void Dispatch()
		{
			for (int i = 0; i < kernels.Count; i++)
			{
				kernels[i].Dispatch();
			}
		}

		public void FixedDispatch()
		{
		}

		public void UpdateSettings()
		{
		}

		public void Dispose()
		{
			if (Indices != null)
			{
				Indices.Dispose();
			}
			if (SelectedPoints != null)
			{
				SelectedPoints.Dispose();
			}
			if (SelectedMatrices != null)
			{
				SelectedMatrices.Dispose();
			}
			LocalPoints.Dispose();
			Points.Dispose();
		}
	}
}
