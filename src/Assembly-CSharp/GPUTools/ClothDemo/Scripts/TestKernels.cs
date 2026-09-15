using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.ClothDemo.Scripts
{
	public class TestKernels : MonoBehaviour
	{
		private TestPrimitive primitive;

		private GpuBuffer<GPParticle> buffer;

		private void Start()
		{
			GPParticle[] array = new GPParticle[6849];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new GPParticle(Vector3.left * 0.1f * i, 0.1f);
			}
			buffer = new GpuBuffer<GPParticle>(array, GPParticle.Size());
			primitive = new TestPrimitive();
			primitive.Particles = buffer;
			primitive.Dt = new GpuValue<float>(0.02f);
			primitive.InvDrag = new GpuValue<float>(1f);
			primitive.Gravity = new GpuValue<Vector3>(Vector3.down * 0.001f);
			primitive.Wind = new GpuValue<Vector3>(Vector3.zero);
			primitive.Start();
		}

		private void Update()
		{
			primitive.Dt.Value = Time.deltaTime;
			primitive.Dispatch();
			buffer.PullData();
		}

		private void OnDrawGizmos()
		{
			if (buffer != null)
			{
				for (int i = 0; i < buffer.Data.Length; i++)
				{
					Gizmos.DrawWireSphere(buffer.Data[i].Position, 0.1f);
				}
			}
		}
	}
}
