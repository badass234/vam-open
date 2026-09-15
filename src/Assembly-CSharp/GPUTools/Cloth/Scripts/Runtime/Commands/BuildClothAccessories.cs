using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Commands
{
	public class BuildClothAccessories : BuildChainCommand
	{
		private readonly ClothSettings settings;

		private CacheProvider<SphereCollider> sphereCollidersCache;

		public BuildClothAccessories(ClothSettings settings)
		{
			this.settings = settings;
			sphereCollidersCache = new CacheProvider<SphereCollider>(settings.AccessoriesProviders);
		}

		protected override void OnBuild()
		{
			if (sphereCollidersCache.Items.Count != 0)
			{
				GPParticle[] data = new GPParticle[sphereCollidersCache.Items.Count];
				settings.Runtime.OutParticles = new GpuBuffer<GPParticle>(data, GPParticle.Size());
				float[] array = new float[sphereCollidersCache.Items.Count];
				CalculateOutParticlesMap(array);
				settings.Runtime.OutParticlesMap = new GpuBuffer<float>(array, 4);
			}
		}

		protected override void OnDispatch()
		{
			if (settings.Runtime.OutParticles != null)
			{
				settings.Runtime.OutParticles.PullData();
				for (int i = 0; i < settings.Runtime.OutParticles.Data.Length; i++)
				{
					GPParticle gPParticle = settings.Runtime.OutParticles.Data[i];
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
			GPParticle[] data = settings.Runtime.Particles.Data;
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
			if (settings.Runtime.OutParticles != null)
			{
				settings.Runtime.OutParticles.Dispose();
			}
			if (settings.Runtime.OutParticlesMap != null)
			{
				settings.Runtime.OutParticlesMap.Dispose();
			}
		}
	}
}
