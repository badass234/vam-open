using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using UnityEngine;

namespace GPUTools.Common.Scripts.Tools.Kernels
{
	public class GPUPointsSelector : KernelBase
	{
		[GpuData("indices")]
		public GpuBuffer<int> Indices { get; set; }

		[GpuData("points")]
		public GpuBuffer<Vector3> Points { get; set; }

		[GpuData("selectedPoints")]
		public GpuBuffer<Vector3> SelectedPoints { get; set; }

		public GPUPointsSelector()
			: base("Compute/Selector", "CSPointsSelector")
		{
		}

		public override int GetGroupsNumX()
		{
			return Mathf.CeilToInt((float)Indices.Count / 256f);
		}
	}
}
