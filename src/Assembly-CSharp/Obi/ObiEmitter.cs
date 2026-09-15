using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	[AddComponentMenu("Physics/Obi/Obi Emitter")]
	public class ObiEmitter : ObiActor
	{
		public int fluidPhase = 1;

		[SerializeField]
		[HideInInspector]
		private ObiEmitterMaterial emitterMaterial;

		[Tooltip("Amount of solver particles used by this emitter.")]
		[SerializeField]
		[HideInInspector]
		private int numParticles = 1000;

		[Tooltip("Speed (in units/second) of emitted particles. Setting it to zero will stop emission. Large values will cause more particles to be emitted.")]
		public float speed = 0.25f;

		[Tooltip("Lifespan of each particle.")]
		public float lifespan = 4f;

		[Range(0f, 1f)]
		[Tooltip("Amount of randomization applied to particles.")]
		public float randomVelocity;

		private ObiEmitterShape emitterShape;

		private int activeParticleCount;

		[HideInInspector]
		public float[] life;

		private float unemittedBursts;

		public int NumParticles
		{
			get
			{
				return numParticles;
			}
			set
			{
				if (numParticles != value)
				{
					numParticles = value;
					GeneratePhysicRepresentation();
				}
			}
		}

		public int ActiveParticles => activeParticleCount;

		public override bool SelfCollisions => selfCollisions;

		public ObiEmitterShape EmitterShape
		{
			get
			{
				return emitterShape;
			}
			set
			{
				if (emitterShape != value)
				{
					emitterShape = value;
					UpdateEmitterDistribution();
				}
			}
		}

		public ObiEmitterMaterial EmitterMaterial
		{
			get
			{
				return emitterMaterial;
			}
			set
			{
				if (emitterMaterial != value)
				{
					if (emitterMaterial != null)
					{
						emitterMaterial.OnChangesMade -= EmitterMaterial_OnChangesMade;
					}
					emitterMaterial = value;
					if (emitterMaterial != null)
					{
						emitterMaterial.OnChangesMade += EmitterMaterial_OnChangesMade;
						EmitterMaterial_OnChangesMade(emitterMaterial, new ObiEmitterMaterial.MaterialChangeEventArgs(ObiEmitterMaterial.MaterialChanges.PER_PARTICLE_DATA));
					}
				}
			}
		}

		public override bool UsesCustomExternalForces => true;

		public override void Awake()
		{
			base.Awake();
			selfCollisions = true;
			GeneratePhysicRepresentation();
		}

		public override void OnEnable()
		{
			if (emitterMaterial != null)
			{
				emitterMaterial.OnChangesMade += EmitterMaterial_OnChangesMade;
			}
			base.OnEnable();
		}

		public override void OnDisable()
		{
			if (emitterMaterial != null)
			{
				emitterMaterial.OnChangesMade -= EmitterMaterial_OnChangesMade;
			}
			base.OnDisable();
		}

		public override void DestroyRequiredComponents()
		{
		}

		public override bool AddToSolver(object info)
		{
			if (base.Initialized && base.AddToSolver(info))
			{
				solver.RequireRenderablePositions();
				CalculateParticleMass();
				return true;
			}
			return false;
		}

		public override bool RemoveFromSolver(object info)
		{
			if (solver != null)
			{
				solver.RelinquishRenderablePositions();
			}
			return base.RemoveFromSolver(info);
		}

		public void CalculateParticleMass()
		{
			float num = ((!(emitterMaterial != null)) ? 0.1f : emitterMaterial.GetParticleMass(solver.parameters.mode));
			for (int i = 0; i < invMasses.Length; i++)
			{
				invMasses[i] = 1f / num;
			}
			PushDataToSolver(ParticleData.INV_MASSES);
		}

		public void SetParticleRestRadius()
		{
			if (base.InSolver)
			{
				float num = ((!(emitterMaterial != null)) ? 0.1f : emitterMaterial.GetParticleSize(solver.parameters.mode));
				for (int i = 0; i < particleIndices.Length; i++)
				{
					solidRadii[i] = num * 0.5f;
				}
				PushDataToSolver(ParticleData.SOLID_RADII);
			}
		}

		public void GeneratePhysicRepresentation()
		{
			initialized = false;
			initializing = true;
			RemoveFromSolver(null);
			active = new bool[numParticles];
			life = new float[numParticles];
			positions = new Vector3[numParticles];
			velocities = new Vector3[numParticles];
			invMasses = new float[numParticles];
			solidRadii = new float[numParticles];
			phases = new int[numParticles];
			colors = new Color[numParticles];
			float num = ((!(emitterMaterial != null)) ? 0.1f : emitterMaterial.GetParticleSize(solver.parameters.mode));
			float num2 = ((!(emitterMaterial != null)) ? 0.1f : emitterMaterial.GetParticleMass(solver.parameters.mode));
			for (int i = 0; i < numParticles; i++)
			{
				active[i] = false;
				life[i] = 0f;
				invMasses[i] = 1f / num2;
				positions[i] = Vector3.zero;
				if (emitterMaterial != null && !(emitterMaterial is ObiEmitterMaterialFluid))
				{
					float num3 = Random.Range(0f, num / 100f * (emitterMaterial as ObiEmitterMaterialGranular).randomness);
					solidRadii[i] = Mathf.Max(0.001f + num * 0.5f - num3);
				}
				else
				{
					solidRadii[i] = num * 0.5f;
				}
				colors[i] = Color.white;
				phases[i] = Oni.MakePhase(fluidPhase, (Oni.ParticlePhase)((selfCollisions ? 16777216 : 0) | ((emitterMaterial != null && emitterMaterial is ObiEmitterMaterialFluid) ? 33554432 : 0)));
			}
			initializing = false;
			initialized = true;
		}

		public override void UpdateParticlePhases()
		{
			if (base.InSolver)
			{
				Oni.ParticlePhase particlePhase = Oni.ParticlePhase.Fluid;
				if (emitterMaterial != null && !(emitterMaterial is ObiEmitterMaterialFluid))
				{
					particlePhase = (Oni.ParticlePhase)0;
				}
				for (int i = 0; i < particleIndices.Length; i++)
				{
					phases[i] = Oni.MakePhase(fluidPhase, (Oni.ParticlePhase)((selfCollisions ? 16777216 : 0) | (int)particlePhase));
				}
				PushDataToSolver(ParticleData.PHASES);
			}
		}

		private void UpdateEmitterDistribution()
		{
			if (emitterShape != null)
			{
				emitterShape.particleSize = ((!(emitterMaterial != null)) ? 0.1f : emitterMaterial.GetParticleSize(solver.parameters.mode));
				emitterShape.GenerateDistribution();
			}
		}

		private void EmitterMaterial_OnChangesMade(object sender, ObiEmitterMaterial.MaterialChangeEventArgs e)
		{
			if ((e.changes & ObiEmitterMaterial.MaterialChanges.PER_PARTICLE_DATA) != 0)
			{
				CalculateParticleMass();
				SetParticleRestRadius();
				UpdateParticlePhases();
			}
			UpdateEmitterDistribution();
		}

		public void ResetParticlePosition(int index, float offset)
		{
			if (emitterShape == null)
			{
				Vector3 vector = Vector3.Lerp(base.transform.forward, Random.onUnitSphere, randomVelocity);
				Vector3 vector2 = vector * (speed * Time.fixedDeltaTime) * offset;
				Vector4[] array = new Vector4[1] { base.transform.position + vector2 };
				Vector4[] array2 = new Vector4[1] { vector * speed };
				Oni.SetParticlePositions(solver.OniSolver, array, 1, particleIndices[index]);
				Oni.SetParticleVelocities(solver.OniSolver, array2, 1, particleIndices[index]);
				colors[index] = Color.white;
			}
			else
			{
				ObiEmitterShape.DistributionPoint distributionPoint = emitterShape.GetDistributionPoint();
				Vector3 vector3 = Vector3.Lerp(base.transform.TransformVector(distributionPoint.velocity), Random.onUnitSphere, randomVelocity);
				Vector3 vector4 = vector3 * (speed * Time.fixedDeltaTime) * offset;
				Vector4[] array3 = new Vector4[1] { base.transform.TransformPoint(distributionPoint.position) + vector4 };
				Vector4[] array4 = new Vector4[1] { vector3 * speed };
				Oni.SetParticlePositions(solver.OniSolver, array3, 1, particleIndices[index]);
				Oni.SetParticleVelocities(solver.OniSolver, array4, 1, particleIndices[index]);
				colors[index] = distributionPoint.color;
			}
		}

		public bool EmitParticle(float offset)
		{
			if (activeParticleCount == numParticles)
			{
				return false;
			}
			life[activeParticleCount] = lifespan;
			ResetParticlePosition(activeParticleCount, offset);
			active[activeParticleCount] = true;
			activeParticleCount++;
			return true;
		}

		public bool KillParticle(int index)
		{
			if (activeParticleCount == 0 || index >= activeParticleCount)
			{
				return false;
			}
			activeParticleCount--;
			active[activeParticleCount] = false;
			int num = particleIndices[activeParticleCount];
			particleIndices[activeParticleCount] = particleIndices[index];
			particleIndices[index] = num;
			float num2 = life[activeParticleCount];
			life[activeParticleCount] = life[index];
			life[index] = num2;
			Color color = colors[activeParticleCount];
			colors[activeParticleCount] = colors[index];
			colors[index] = color;
			return true;
		}

		public void KillAll()
		{
			for (int num = activeParticleCount - 1; num >= 0; num--)
			{
				KillParticle(num);
			}
			PushDataToSolver(ParticleData.ACTIVE_STATUS);
		}

		public override void OnSolverStepBegin()
		{
			base.OnSolverStepBegin();
			bool flag = false;
			bool flag2 = false;
			for (int num = activeParticleCount - 1; num >= 0; num--)
			{
				life[num] -= Time.deltaTime;
				if (life[num] <= 0f)
				{
					flag2 |= KillParticle(num);
				}
			}
			int num2 = ((!(emitterShape != null)) ? 1 : emitterShape.DistributionPointsCount);
			if (emitterShape == null || emitterShape.samplingMethod == ObiEmitterShape.SamplingMethod.SURFACE)
			{
				float num3 = speed * Time.fixedDeltaTime / ((!(emitterMaterial != null)) ? 0.1f : emitterMaterial.GetParticleSize(solver.parameters.mode));
				unemittedBursts += num3;
				int num4 = 0;
				while (unemittedBursts > 0f)
				{
					for (int i = 0; i < num2; i++)
					{
						flag |= EmitParticle((float)num4 / num3);
					}
					unemittedBursts -= 1f;
					num4++;
				}
			}
			else if (activeParticleCount == 0)
			{
				for (int j = 0; j < num2; j++)
				{
					flag |= EmitParticle(0f);
				}
			}
			if (flag || flag2)
			{
				PushDataToSolver(ParticleData.ACTIVE_STATUS);
			}
		}
	}
}
