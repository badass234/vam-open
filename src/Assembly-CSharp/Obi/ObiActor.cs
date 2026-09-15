using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public abstract class ObiActor : MonoBehaviour, IObiSolverClient
	{
		public class ObiActorSolverArgs : EventArgs
		{
			private ObiSolver solver;

			public ObiSolver Solver => solver;

			public ObiActorSolverArgs(ObiSolver solver)
			{
				this.solver = solver;
			}
		}

		public enum TetherType
		{
			AnchorToFixed,
			Hierarchical
		}

		[Range(0f, 1f)]
		public float worldVelocityScale;

		[HideInInspector]
		public ObiCollisionMaterial collisionMaterial;

		[NonSerialized]
		[HideInInspector]
		public int[] particleIndices;

		protected Dictionary<Oni.ConstraintType, ObiBatchedConstraints> constraints = new Dictionary<Oni.ConstraintType, ObiBatchedConstraints>();

		[HideInInspector]
		public bool[] active;

		[HideInInspector]
		public Vector3[] positions;

		[HideInInspector]
		public Vector4[] restPositions;

		[HideInInspector]
		public Vector3[] velocities;

		[HideInInspector]
		public float[] invMasses;

		[HideInInspector]
		public float[] solidRadii;

		[HideInInspector]
		public int[] phases;

		[HideInInspector]
		public Color[] colors;

		[HideInInspector]
		public int[] deformableTriangles = new int[0];

		[NonSerialized]
		protected int trianglesOffset;

		private bool inSolver;

		protected bool initializing;

		[HideInInspector]
		[SerializeField]
		protected ObiSolver solver;

		[HideInInspector]
		[SerializeField]
		protected bool selfCollisions;

		[HideInInspector]
		[SerializeField]
		protected bool initialized;

		public ObiSolver Solver
		{
			get
			{
				return solver;
			}
			set
			{
				if (solver != value)
				{
					RemoveFromSolver(null);
					solver = value;
				}
			}
		}

		public ObiCollisionMaterial CollisionMaterial
		{
			get
			{
				return collisionMaterial;
			}
			set
			{
				if (collisionMaterial != value)
				{
					collisionMaterial = value;
					PushDataToSolver(ParticleData.COLLISION_MATERIAL);
				}
			}
		}

		public bool Initializing => initializing;

		public bool Initialized => initialized;

		public bool InSolver => inSolver;

		public virtual bool SelfCollisions
		{
			get
			{
				return selfCollisions;
			}
			set
			{
				if (value != selfCollisions)
				{
					selfCollisions = value;
					UpdateParticlePhases();
				}
			}
		}

		public virtual Matrix4x4 ActorLocalToWorldMatrix => base.transform.localToWorldMatrix;

		public virtual Matrix4x4 ActorWorldToLocalMatrix => base.transform.worldToLocalMatrix;

		public virtual bool UsesCustomExternalForces => false;

		public event EventHandler<ObiActorSolverArgs> OnAddedToSolver;

		public event EventHandler<ObiActorSolverArgs> OnRemovedFromSolver;

		public virtual void Awake()
		{
		}

		public virtual void Start()
		{
			if (Application.isPlaying)
			{
				AddToSolver(null);
			}
		}

		public virtual void OnDestroy()
		{
			RemoveFromSolver(null);
		}

		public virtual void DestroyRequiredComponents()
		{
		}

		public virtual void OnEnable()
		{
			constraints.Clear();
			ObiBatchedConstraints[] components = GetComponents<ObiBatchedConstraints>();
			ObiBatchedConstraints[] array = components;
			foreach (ObiBatchedConstraints obiBatchedConstraints in array)
			{
				constraints[obiBatchedConstraints.GetConstraintType()] = obiBatchedConstraints;
				obiBatchedConstraints.GrabActor();
				if (obiBatchedConstraints.isActiveAndEnabled)
				{
					obiBatchedConstraints.OnEnable();
				}
			}
			if (InSolver)
			{
				solver.UpdateActiveParticles();
			}
		}

		public virtual void OnDisable()
		{
			if (!InSolver)
			{
				return;
			}
			solver.UpdateActiveParticles();
			PullDataFromSolver(ParticleData.POSITIONS | ParticleData.VELOCITIES);
			foreach (ObiBatchedConstraints value in constraints.Values)
			{
				value.OnDisable();
			}
		}

		public virtual void ResetActor()
		{
		}

		public virtual void UpdateParticlePhases()
		{
			if (InSolver)
			{
				for (int i = 0; i < phases.Length; i++)
				{
					phases[i] = Oni.MakePhase(Oni.GetGroupFromPhase(phases[i]), selfCollisions ? Oni.ParticlePhase.SelfCollide : ((Oni.ParticlePhase)0));
				}
				PushDataToSolver(ParticleData.PHASES);
			}
		}

		public virtual bool AddToSolver(object info)
		{
			if (solver != null && !InSolver)
			{
				if (!solver.AddActor(this, positions.Length))
				{
					Debug.LogWarning("Obi: Solver could not allocate enough particles for this actor. Please increase max particles.");
					return false;
				}
				inSolver = true;
				UpdateParticlePhases();
				trianglesOffset = Oni.GetDeformableTriangleCount(solver.OniSolver);
				UpdateDeformableTriangles();
				PushDataToSolver(ParticleData.ALL);
				foreach (ObiBatchedConstraints value in constraints.Values)
				{
					value.AddToSolver(null);
				}
				if (this.OnAddedToSolver != null)
				{
					this.OnAddedToSolver(this, new ObiActorSolverArgs(solver));
				}
				return true;
			}
			return false;
		}

		public void UpdateDeformableTriangles()
		{
			int[] array = new int[deformableTriangles.Length];
			for (int i = 0; i < deformableTriangles.Length; i++)
			{
				array[i] = particleIndices[deformableTriangles[i]];
			}
			Oni.SetDeformableTriangles(solver.OniSolver, array, array.Length / 3, trianglesOffset);
		}

		public virtual bool RemoveFromSolver(object info)
		{
			if (solver != null && InSolver)
			{
				foreach (ObiBatchedConstraints value in constraints.Values)
				{
					value.RemoveFromSolver(null);
				}
				Vector4[] array = new Vector4[1]
				{
					new Vector4(0f, 0f, 0f, 1f)
				};
				for (int i = 0; i < particleIndices.Length; i++)
				{
					Oni.SetRestPositions(solver.OniSolver, array, 1, particleIndices[i]);
				}
				int num = solver.RemoveActor(this);
				particleIndices = null;
				for (int j = num; j < solver.actors.Count; j++)
				{
					solver.actors[j].trianglesOffset -= deformableTriangles.Length / 3;
				}
				Oni.RemoveDeformableTriangles(solver.OniSolver, deformableTriangles.Length / 3, trianglesOffset);
				inSolver = false;
				if (this.OnRemovedFromSolver != null)
				{
					this.OnRemovedFromSolver(this, new ObiActorSolverArgs(solver));
				}
				return true;
			}
			return false;
		}

		public virtual void PushDataToSolver(ParticleData data = ParticleData.NONE)
		{
			if (!InSolver)
			{
				return;
			}
			Matrix4x4 matrix4x = ((!Solver.simulateInLocalSpace) ? ActorLocalToWorldMatrix : (Solver.transform.worldToLocalMatrix * ActorLocalToWorldMatrix));
			for (int i = 0; i < particleIndices.Length; i++)
			{
				int destOffset = particleIndices[i];
				if ((data & ParticleData.POSITIONS) != 0 && i < positions.Length)
				{
					Oni.SetParticlePositions(solver.OniSolver, new Vector4[1] { matrix4x.MultiplyPoint3x4(positions[i]) }, 1, destOffset);
				}
				if ((data & ParticleData.VELOCITIES) != 0 && i < velocities.Length)
				{
					Oni.SetParticleVelocities(solver.OniSolver, new Vector4[1] { matrix4x.MultiplyVector(velocities[i]) }, 1, destOffset);
				}
				if ((data & ParticleData.INV_MASSES) != 0 && i < invMasses.Length)
				{
					Oni.SetParticleInverseMasses(solver.OniSolver, new float[1] { invMasses[i] }, 1, destOffset);
				}
				if ((data & ParticleData.SOLID_RADII) != 0 && i < solidRadii.Length)
				{
					Oni.SetParticleSolidRadii(solver.OniSolver, new float[1] { solidRadii[i] }, 1, destOffset);
				}
				if ((data & ParticleData.PHASES) != 0 && i < phases.Length)
				{
					Oni.SetParticlePhases(solver.OniSolver, new int[1] { phases[i] }, 1, destOffset);
				}
				if ((data & ParticleData.REST_POSITIONS) != 0 && i < restPositions.Length)
				{
					Oni.SetRestPositions(solver.OniSolver, new Vector4[1] { restPositions[i] }, 1, destOffset);
				}
			}
			if ((data & ParticleData.COLLISION_MATERIAL) != 0)
			{
				IntPtr[] array = new IntPtr[particleIndices.Length];
				for (int j = 0; j < particleIndices.Length; j++)
				{
					array[j] = ((!(collisionMaterial != null)) ? IntPtr.Zero : collisionMaterial.OniCollisionMaterial);
				}
				Oni.SetCollisionMaterials(solver.OniSolver, array, particleIndices, particleIndices.Length);
			}
			if ((data & ParticleData.ACTIVE_STATUS) != 0)
			{
				solver.UpdateActiveParticles();
			}
		}

		public virtual void PullDataFromSolver(ParticleData data = ParticleData.NONE)
		{
			if (!InSolver)
			{
				return;
			}
			for (int i = 0; i < particleIndices.Length; i++)
			{
				int sourceOffset = particleIndices[i];
				if ((data & ParticleData.POSITIONS) != 0)
				{
					Vector4[] array = new Vector4[1] { positions[i] };
					Oni.GetParticlePositions(solver.OniSolver, array, 1, sourceOffset);
					positions[i] = base.transform.InverseTransformPoint(array[0]);
				}
				if ((data & ParticleData.VELOCITIES) != 0)
				{
					Vector4[] array2 = new Vector4[1] { velocities[i] };
					Oni.GetParticleVelocities(solver.OniSolver, array2, 1, sourceOffset);
					velocities[i] = base.transform.InverseTransformVector(array2[0]);
				}
			}
		}

		public Vector3 GetParticlePosition(int index)
		{
			if (InSolver)
			{
				return solver.renderablePositions[particleIndices[index]];
			}
			return ActorLocalToWorldMatrix.MultiplyPoint3x4(positions[index]);
		}

		public virtual bool GenerateTethers(TetherType type)
		{
			return true;
		}

		public void ClearTethers()
		{
			ObiBatchedConstraints value;
			if (constraints.TryGetValue(Oni.ConstraintType.Tether, out value))
			{
				((ObiTetherConstraints)value).Clear();
			}
		}

		public virtual void OnSolverPreInterpolation()
		{
		}

		public virtual void OnSolverStepBegin()
		{
			if (!base.transform.hasChanged && !Solver.transform.hasChanged)
			{
				return;
			}
			base.transform.hasChanged = false;
			Solver.transform.hasChanged = false;
			bool flag = base.enabled;
			int num = particleIndices.Length;
			Vector4[] array = new Vector4[1] { Vector4.zero };
			Matrix4x4 matrix4x = ((!Solver.simulateInLocalSpace) ? ActorLocalToWorldMatrix : (Solver.transform.worldToLocalMatrix * ActorLocalToWorldMatrix));
			Matrix4x4 matrix4x2 = Solver.transform.worldToLocalMatrix * Solver.LastTransform;
			for (int i = 0; i < num; i++)
			{
				if (!flag || invMasses[i] == 0f)
				{
					array[0] = matrix4x.MultiplyPoint3x4(positions[i]);
					Oni.SetParticlePositions(solver.OniSolver, array, 1, particleIndices[i]);
				}
				else if (Solver.simulateInLocalSpace)
				{
					Oni.GetParticlePositions(solver.OniSolver, array, 1, particleIndices[i]);
					array[0] = Vector3.Lerp(array[0], matrix4x2.MultiplyPoint3x4(array[0]), worldVelocityScale);
					Oni.SetParticlePositions(solver.OniSolver, array, 1, particleIndices[i]);
				}
			}
		}

		public virtual void OnSolverStepEnd()
		{
		}

		public virtual void OnSolverFrameBegin()
		{
		}

		public virtual void OnSolverFrameEnd()
		{
		}

		public virtual bool ReadParticlePropertyFromTexture(Texture2D source, Action<int, Color> onReadProperty)
		{
			return false;
		}
	}
}
