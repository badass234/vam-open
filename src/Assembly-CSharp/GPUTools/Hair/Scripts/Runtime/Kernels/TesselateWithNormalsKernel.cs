using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Hair.Scripts.Runtime.Render;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Kernels
{
	public class TesselateWithNormalsKernel : KernelBase
	{
		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("renderParticles")]
		public GpuBuffer<RenderParticle> RenderParticles { get; set; }

		[GpuData("tessRenderParticles")]
		public GpuBuffer<TessRenderParticle> TessRenderParticles { get; set; }

		[GpuData("tessRenderParticlesCount")]
		public GpuValue<int> TessRenderParticlesCount { get; set; }

		[GpuData("randomsPerStrand")]
		public GpuBuffer<Vector3> RandomsPerStrand { get; set; }

		[GpuData("transforms")]
		public GpuBuffer<Matrix4x4> Transforms { get; set; }

		[GpuData("normals")]
		public GpuBuffer<Vector3> Normals { get; set; }

		[GpuData("lightCenter")]
		public GpuValue<Vector3> LightCenter { get; set; }

		[GpuData("pointJoints")]
		public GpuBuffer<GPPointJoint> PointJoints { get; set; }

		[GpuData("normalRandomize")]
		public GpuValue<float> NormalRandomize { get; set; }

		[GpuData("segments")]
		public GpuValue<int> Segments { get; set; }

		[GpuData("tessSegments")]
		public GpuValue<int> TessSegments { get; set; }

		[GpuData("wavinessAxis")]
		public GpuValue<Vector3> WavinessAxis { get; set; }

		[GpuData("wavinessFrequencyRandomness")]
		public GpuValue<float> WavinessFrequencyRandomness { get; set; }

		[GpuData("wavinessScaleRandomness")]
		public GpuValue<float> WavinessScaleRandomness { get; set; }

		[GpuData("wavinessAllowReverse")]
		public GpuValue<bool> WavinessAllowReverse { get; set; }

		[GpuData("wavinessAllowFlipAxis")]
		public GpuValue<bool> WavinessAllowFlipAxis { get; set; }

		[GpuData("wavinessNormalAdjust")]
		public GpuValue<float> WavinessNormalAdjust { get; set; }

		public TesselateWithNormalsKernel()
			: base("Compute/TesselateWithNormals", "CSTesselateWithNormals")
		{
		}

		public override int GetGroupsNumX()
		{
			if (TessRenderParticles != null)
			{
				return Mathf.CeilToInt((float)TessRenderParticlesCount.Value / 256f);
			}
			return 0;
		}
	}
}
