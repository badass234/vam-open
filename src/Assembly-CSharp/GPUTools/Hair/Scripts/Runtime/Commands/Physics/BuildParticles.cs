using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Hair.Scripts.Geometry.Abstract;
using GPUTools.Physics.Scripts.Types.Dynamic;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Commands.Physics
{
	public class BuildParticles : BuildChainCommand
	{
		private readonly HairSettings settings;

		private readonly GeometryProviderBase provider;

		protected GPParticle[] gpuState;

		public BuildParticles(HairSettings settings)
		{
			this.settings = settings;
			provider = settings.StandsSettings.Provider;
		}

		protected override void OnBuild()
		{
			GPParticle[] array = new GPParticle[provider.GetVertices().Count];
			float[] particleRootToTipRatios = new float[provider.GetVertices().Count];
			ComputeParticles(array, particleRootToTipRatios);
			if (settings.RuntimeData.Particles != null)
			{
				settings.RuntimeData.Particles.Dispose();
			}
			if (array.Length > 0)
			{
				settings.RuntimeData.Particles = new GpuBuffer<GPParticle>(array, GPParticle.Size());
			}
			else
			{
				settings.RuntimeData.Particles = null;
			}
			settings.RuntimeData.ParticleRootToTipRatios = particleRootToTipRatios;
		}

		public void UpdateParticleRadius()
		{
			if (settings.RuntimeData.Particles == null)
			{
				return;
			}
			GPParticle[] array = (GPParticle[])settings.RuntimeData.Particles.Data.Clone();
			settings.RuntimeData.Particles.PullData();
			float num;
			float radius;
			if (settings.PhysicsSettings.StyleMode)
			{
				num = settings.PhysicsSettings.StyleModeStrandRadius * provider.transform.lossyScale.x;
				radius = ((!settings.PhysicsSettings.UseSeparateRootRadius) ? num : (settings.PhysicsSettings.StyleModeStrandRootRadius * provider.transform.lossyScale.x));
			}
			else
			{
				num = settings.PhysicsSettings.StandRadius * provider.transform.lossyScale.x;
				radius = ((!settings.PhysicsSettings.UseSeparateRootRadius) ? num : (settings.PhysicsSettings.StandRootRadius * provider.transform.lossyScale.x));
			}
			int segments = settings.StandsSettings.Segments;
			GPParticle[] data = settings.RuntimeData.Particles.Data;
			for (int i = 0; i < data.Length; i++)
			{
				int num2 = i % segments;
				if (num2 == 1)
				{
					data[i].Radius = radius;
					array[i].Radius = radius;
				}
				else
				{
					data[i].Radius = num;
					array[i].Radius = num;
				}
			}
			settings.RuntimeData.Particles.PushData();
			settings.RuntimeData.Particles.Data = array;
		}

		public void SaveGPUState()
		{
			if (settings.RuntimeData.Particles != null)
			{
				GPParticle[] data = (GPParticle[])settings.RuntimeData.Particles.Data.Clone();
				settings.RuntimeData.Particles.PullData();
				gpuState = settings.RuntimeData.Particles.Data;
				settings.RuntimeData.Particles.Data = data;
			}
		}

		public void RestoreGPUState()
		{
			if (settings.RuntimeData.Particles != null && gpuState != null)
			{
				settings.RuntimeData.Particles.Data = gpuState;
				settings.RuntimeData.Particles.PushData();
			}
		}

		protected override void OnUpdateSettings()
		{
			float[] particleRootToTipRatios = new float[provider.GetVertices().Count];
			if (settings.RuntimeData.Particles != null)
			{
				ComputeParticles(settings.RuntimeData.Particles.Data, particleRootToTipRatios);
				settings.RuntimeData.Particles.PushData();
				settings.RuntimeData.ParticleRootToTipRatios = particleRootToTipRatios;
			}
		}

		private void ComputeParticles(GPParticle[] particles, float[] particleRootToTipRatios)
		{
			Matrix4x4 toWorldMatrix = provider.GetToWorldMatrix();
			float num = settings.PhysicsSettings.StandRadius * provider.transform.lossyScale.x;
			float radius = ((!settings.PhysicsSettings.UseSeparateRootRadius) ? num : (settings.PhysicsSettings.StandRootRadius * provider.transform.lossyScale.x));
			List<Vector3> vertices = provider.GetVertices();
			int segments = settings.StandsSettings.Segments;
			float num2 = 0f;
			float num3 = 0f;
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < vertices.Count; i++)
			{
				if (i % segments == 0)
				{
					if (num2 > num3)
					{
						num3 = num2;
					}
					num2 = 0f;
				}
				else
				{
					float magnitude = (vertices[i] - vector).magnitude;
					num2 += magnitude;
				}
				vector = vertices[i];
			}
			for (int j = 0; j < vertices.Count; j++)
			{
				int num4 = j % segments;
				if (num4 == 0)
				{
					num2 = 0f;
				}
				else
				{
					float magnitude2 = (vertices[j] - vector).magnitude;
					num2 += magnitude2;
				}
				vector = vertices[j];
				float num5 = num2 / num3;
				Vector3 position = toWorldMatrix.MultiplyPoint3x4(vertices[j]);
				switch (num4)
				{
				case 0:
				{
					particles[j] = new GPParticle(position, num);
					particles[j].CollisionEnabled = 0;
					break;
				}
				case 1:
				{
					particles[j] = new GPParticle(position, radius);
					break;
				}
				default:
				{
					particles[j] = new GPParticle(position, num);
					break;
				}
				}
				particleRootToTipRatios[j] = num5;
			}
		}

		protected override void OnDispose()
		{
			if (settings.RuntimeData.Particles != null)
			{
				settings.RuntimeData.Particles.Dispose();
			}
		}
	}
}
