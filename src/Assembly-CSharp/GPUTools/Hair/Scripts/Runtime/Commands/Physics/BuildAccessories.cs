using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Commands.Physics
{
	public class BuildAccessories : BuildChainCommand
	{
		private readonly HairSettings settings;

		private CacheProvider<SphereCollider> sphereCollidersCache;

		public BuildAccessories(HairSettings settings)
		{
			this.settings = settings;
			sphereCollidersCache = new CacheProvider<SphereCollider>(settings.PhysicsSettings.AccessoriesProviders);
		}

		protected override void OnBuild()
		{
			if (sphereCollidersCache.Items.Count != 0)
			{
				GPParticle[] data = new GPParticle[sphereCollidersCache.Items.Count];
				settings.RuntimeData.OutParticles = new GpuBuffer<GPParticle>(data, GPParticle.Size());
				float[] array = new float[sphereCollidersCache.Items.Count];
				CalculateOutParticlesMap(array);
				settings.RuntimeData.OutParticlesMap = new GpuBuffer<float>(array, 4);
			}
		}

		protected override void OnDispatch()
		{
			if (settings.RuntimeData.OutParticles != null)
			{
				settings.RuntimeData.OutParticles.PullData();
				for (int i = 0; i < settings.RuntimeData.OutParticles.Data.Length; i++)
				{
					GPParticle gPParticle = settings.RuntimeData.OutParticles.Data[i];
					sphereCollidersCache.Items[i].transform.position = gPParticle.Position;
				}
			}
		}

		private void CalculateOutParticlesMap(float[] outParticlesMap)
		{
			float[] array = new float[sphereCollidersCache.Items.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = float.PositiveInfinity;
			}
			GPParticle[] data = settings.RuntimeData.Particles.Data;
			for (int j = 0; j < data.Length; j++)
			{
				GPParticle gPParticle = data[j];
				for (int k = 0; k < sphereCollidersCache.Items.Count; k++)
				{
					SphereCollider sphereCollider = sphereCollidersCache.Items[k];
					float num = Vector3.Distance(sphereCollider.transform.position, gPParticle.Position);
					if (num < sphereCollider.radius && num < array[k])
					{
						array[k] = num;
						outParticlesMap[k] = j;
					}
				}
			}
		}

		protected override void OnDispose()
		{
			if (settings.RuntimeData.OutParticles != null)
			{
				settings.RuntimeData.OutParticles.Dispose();
			}
			if (settings.RuntimeData.OutParticlesMap != null)
			{
				settings.RuntimeData.OutParticlesMap.Dispose();
			}
		}
	}
}
