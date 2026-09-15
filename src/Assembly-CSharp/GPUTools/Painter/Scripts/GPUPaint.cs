using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Painter.Scripts.Kernels;
using UnityEngine;

namespace GPUTools.Painter.Scripts
{
	public class GPUPaint : PrimitiveBase
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

		public GPUPaint(Vector3[] vertices, Vector3[] normals, Color[] colors)
		{
			Vertices = new GpuBuffer<Vector3>(vertices, 12);
			Normals = new GpuBuffer<Vector3>(normals, 12);
			Colors = new GpuBuffer<Color>(colors, 16);
			RayOrigin = new GpuValue<Vector3>();
			RayDirection = new GpuValue<Vector3>();
			BrushColor = new GpuValue<Color>();
			BrushRadius = new GpuValue<float>(0f);
			BrushStrength = new GpuValue<float>(0f);
			Channel = new GpuValue<int>(0);
			AddPass(new PaintKernel());
			Bind();
		}

		public void Draw(ColorBrush brush, Ray ray)
		{
			RayOrigin.Value = ray.origin;
			RayDirection.Value = ray.direction;
			BrushColor.Value = brush.Color;
			BrushRadius.Value = brush.Radius;
			BrushStrength.Value = brush.Strength;
			Channel.Value = (int)brush.Channel;
			Dispatch();
		}

		public override void Dispose()
		{
			base.Dispose();
			Vertices.Dispose();
			Normals.Dispose();
			Colors.Dispose();
		}
	}
}
