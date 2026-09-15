using UnityEngine;
using UnityEngine.Rendering;

namespace Obi
{
	public class ShadowmapExposer : MonoBehaviour
	{
		private Light light;

		private CommandBuffer afterShadow;

		public ObiParticleRenderer[] particleRenderers;

		public void Awake()
		{
			light = GetComponent<Light>();
		}

		public void OnEnable()
		{
			Cleanup();
			afterShadow = new CommandBuffer();
			afterShadow.name = "FluidShadows";
			light.AddCommandBuffer(LightEvent.AfterShadowMapPass, afterShadow);
		}

		public void OnDisable()
		{
			Cleanup();
		}

		private void Cleanup()
		{
			if (afterShadow != null)
			{
				light.RemoveCommandBuffer(LightEvent.AfterShadowMapPass, afterShadow);
				afterShadow = null;
			}
		}

		public void SetupFluidShadowsCommandBuffer()
		{
			afterShadow.Clear();
			if (particleRenderers == null)
			{
				return;
			}
			ObiParticleRenderer[] array = particleRenderers;
			foreach (ObiParticleRenderer obiParticleRenderer in array)
			{
				if (!(obiParticleRenderer != null))
				{
					continue;
				}
				foreach (Mesh particleMesh in obiParticleRenderer.ParticleMeshes)
				{
					afterShadow.DrawMesh(particleMesh, Matrix4x4.identity, obiParticleRenderer.ParticleMaterial, 0, 1);
				}
			}
		}

		private void Update()
		{
			if (!base.gameObject.activeInHierarchy || !base.enabled || particleRenderers == null || particleRenderers.Length == 0)
			{
				Cleanup();
			}
			else if (afterShadow != null)
			{
				SetupFluidShadowsCommandBuffer();
			}
		}
	}
}
