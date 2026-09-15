using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Commands
{
	public class BuildParticles : BuildChainCommand
	{
		private readonly ClothSettings settings;

		public BuildParticles(ClothSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			GPParticle[] array = new GPParticle[settings.GeometryData.Particles.Length];
			ComputeParticles(array);
			settings.Runtime.Particles = new GpuBuffer<GPParticle>(array, GPParticle.Size());
		}

		protected override void OnUpdateSettings()
		{
			ComputeParticles(settings.Runtime.Particles.Data);
			settings.Runtime.Particles.PushData();
			settings.builder.physics.ResetPhysics();
		}

		private void ComputeParticles(GPParticle[] particles)
		{
			Vector3[] particles2 = settings.GeometryData.Particles;
			Matrix4x4 toWorldMatrix = settings.MeshProvider.ToWorldMatrix;
			int[] physicsToMeshVerticesMap = settings.GeometryData.PhysicsToMeshVerticesMap;
			float[] particlesStrength = settings.GeometryData.ParticlesStrength;
			float radius = settings.ParticleRadius * settings.transform.lossyScale.x;
			if (particles == null)
			{
				particles = new GPParticle[particles2.Length];
			}
			for (int i = 0; i < particles2.Length; i++)
			{
				Vector3 position = toWorldMatrix.MultiplyPoint3x4(particles2[i]);
				int num = physicsToMeshVerticesMap[i];
				particles[i] = new GPParticle(position, radius);
				float b = particlesStrength[num];
				b = Mathf.Max(0.1f, b);
				particles[i].Strength = b;
			}
		}

		protected override void OnDispose()
		{
			settings.Runtime.Particles.Dispose();
		}
	}
}
