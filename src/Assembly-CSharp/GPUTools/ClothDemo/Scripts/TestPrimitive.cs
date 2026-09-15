using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Kernels;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.ClothDemo.Scripts
{
	public class TestPrimitive : PrimitiveBase
	{
		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("gravity")]
		public GpuValue<Vector3> Gravity { get; set; }

		[GpuData("invDrag")]
		public GpuValue<float> InvDrag { get; set; }

		[GpuData("dt")]
		public GpuValue<float> Dt { get; set; }

		[GpuData("wind")]
		public GpuValue<Vector3> Wind { get; set; }

		public TestPrimitive()
		{
			AddPass(new IntegrateKernel());
		}

		public void Start()
		{
			Bind();
		}
	}
}
