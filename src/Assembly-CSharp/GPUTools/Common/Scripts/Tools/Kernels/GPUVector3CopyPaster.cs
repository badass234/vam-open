using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using UnityEngine;

namespace GPUTools.Common.Scripts.Tools.Kernels
{
	public class GPUVector3CopyPaster : KernelBase
	{
		[GpuData("vector3From")]
		public GpuBuffer<Vector3> Vector3From { get; set; }

		[GpuData("vector3To")]
		public GpuBuffer<Vector3> Vector3To { get; set; }

		public GPUVector3CopyPaster(GpuBuffer<Vector3> vector3From, GpuBuffer<Vector3> vector3To)
			: base("Compute/Vector3CopyPaster", "CSVector3CopyPaster")
		{
			Vector3From = vector3From;
			Vector3To = vector3To;
		}

		public override int GetGroupsNumX()
		{
			return Mathf.CeilToInt((float)Vector3From.Count / 256f);
		}
	}
}
