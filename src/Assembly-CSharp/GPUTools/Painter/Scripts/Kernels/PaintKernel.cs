using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using UnityEngine;

namespace GPUTools.Painter.Scripts.Kernels
{
	public class PaintKernel : KernelBase
	{
		[GpuData("vertices")]
		public GpuBuffer<Vector3> Vertices { get; set; }

		[GpuData("normals")]
		public GpuBuffer<Vector3> Normals { get; set; }

		[GpuData("colors")]
		public GpuBuffer<Color> Colors { get; set; }

		[GpuData("rayOrigin")]
		public GpuValue<Vector3> RayOrigin { get; set; }

		[GpuData("rayDirection")]
		public GpuValue<Vector3> RayDirection { get; set; }

		[GpuData("brushColor")]
		public GpuValue<Color> BrushColor { get; set; }

		[GpuData("brushRadius")]
		public GpuValue<float> BrushRadius { get; set; }

		[GpuData("brushStrength")]
		public GpuValue<float> BrushStrength { get; set; }

		[GpuData("channel")]
		public GpuValue<int> Channel { get; set; }

		public PaintKernel()
			: base("Compute/Paint", "CSPaint")
		{
		}

		public override int GetGroupsNumX()
		{
			return Mathf.CeilToInt((float)Vertices.Count / 256f);
		}
	}
}
