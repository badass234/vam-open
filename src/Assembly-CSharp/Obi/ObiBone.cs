using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	[AddComponentMenu("Physics/Obi/Obi Bone")]
	[RequireComponent(typeof(ObiDistanceConstraints))]
	[RequireComponent(typeof(ObiBendingConstraints))]
	[RequireComponent(typeof(ObiSkinConstraints))]
	public class ObiBone : ObiActor
	{
		public const float DEFAULT_PARTICLE_MASS = 0.1f;

		public const float MAX_YOUNG_MODULUS = 20f;

		public const float MIN_YOUNG_MODULUS = 0.0001f;

		[Tooltip("Initial particle radius.")]
		public float particleRadius = 0.05f;

		[HideInInspector]
		[SerializeField]
		private List<Transform> bones;

		[HideInInspector]
		[SerializeField]
		private int[] parentIndices;

		[HideInInspector]
		public bool[] frozen;

		protected ObiAnimatorController animatorController;

		public ObiSkinConstraints SkinConstraints => constraints[Oni.ConstraintType.Skin] as ObiSkinConstraints;

		public ObiDistanceConstraints DistanceConstraints => constraints[Oni.ConstraintType.Distance] as ObiDistanceConstraints;

		public ObiBendingConstraints BendingConstraints => constraints[Oni.ConstraintType.Bending] as ObiBendingConstraints;

		public override void Awake()
		{
			base.Awake();
			SetupAnimatorController();
		}

		public void OnValidate()
		{
			particleRadius = Mathf.Max(0f, particleRadius);
		}

		public override void OnSolverFrameEnd()
		{
			base.OnSolverFrameEnd();
			UpdateBones();
		}

		public override bool AddToSolver(object info)
		{
			if (base.Initialized && base.AddToSolver(info))
			{
				solver.RequireRenderablePositions();
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

		private void SetupAnimatorController()
		{
			Animator componentInParent = GetComponentInParent<Animator>();
			if (componentInParent != null)
			{
				animatorController = componentInParent.GetComponent<ObiAnimatorController>();
				if (animatorController == null)
				{
					animatorController = componentInParent.gameObject.AddComponent<ObiAnimatorController>();
				}
			}
		}

		private IEnumerable EnumerateBonesBreadthFirst()
		{
			int count = 0;
			Queue<Transform> queue = new Queue<Transform>();
			queue.Enqueue(base.transform);
			while (queue.Count > 0)
			{
				Transform current = queue.Dequeue();
				if (!(current != null))
				{
					continue;
				}
				count++;
				yield return current;
				IEnumerator enumerator = current.transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Transform item = (Transform)enumerator.Current;
						queue.Enqueue(item);
					}
				}
				finally
				{
					IDisposable disposable;
					IDisposable disposable2 = (disposable = enumerator as IDisposable);
					if (disposable != null)
					{
						disposable2.Dispose();
					}
				}
				if (current == base.transform && count > 1)
				{
					yield return null;
				}
			}
		}

		public IEnumerator GeneratePhysicRepresentationForBones()
		{
			initialized = false;
			initializing = true;
			RemoveFromSolver(null);
			bones = new List<Transform>();
			IEnumerator enumerator = EnumerateBonesBreadthFirst().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform item = (Transform)enumerator.Current;
					bones.Add(item);
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable2 = (disposable = enumerator as IDisposable);
				if (disposable != null)
				{
					disposable2.Dispose();
				}
			}
			parentIndices = new int[bones.Count];
			active = new bool[bones.Count];
			positions = new Vector3[bones.Count];
			velocities = new Vector3[bones.Count];
			invMasses = new float[bones.Count];
			solidRadii = new float[bones.Count];
			phases = new int[bones.Count];
			restPositions = new Vector4[bones.Count];
			frozen = new bool[bones.Count];
			DistanceConstraints.Clear();
			ObiDistanceConstraintBatch distanceBatch = new ObiDistanceConstraintBatch(false, false, 0.0001f, 20f);
			DistanceConstraints.AddBatch(distanceBatch);
			BendingConstraints.Clear();
			ObiBendConstraintBatch bendingBatch = new ObiBendConstraintBatch(false, false, 0.0001f, 20f);
			BendingConstraints.AddBatch(bendingBatch);
			SkinConstraints.Clear();
			ObiSkinConstraintBatch skinBatch = new ObiSkinConstraintBatch(true, false, 0.0001f, 20f);
			SkinConstraints.AddBatch(skinBatch);
			for (int i = 0; i < bones.Count; i++)
			{
				active[i] = true;
				invMasses[i] = 10f;
				positions[i] = base.transform.InverseTransformPoint(bones[i].position);
				restPositions[i] = positions[i];
				restPositions[i][3] = 0f;
				solidRadii[i] = particleRadius;
				frozen[i] = false;
				phases[i] = Oni.MakePhase(1, selfCollisions ? Oni.ParticlePhase.SelfCollide : ((Oni.ParticlePhase)0));
				parentIndices[i] = -1;
				if (bones[i].parent != null)
				{
					parentIndices[i] = bones.IndexOf(bones[i].parent);
				}
				skinBatch.AddConstraint(i, positions[i], Vector3.up, 0.05f, 0f, 0f, 1f);
				IEnumerator enumerator2 = bones[i].GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						Transform transform = (Transform)enumerator2.Current;
						int num = bones.IndexOf(transform);
						if (num >= 0)
						{
							distanceBatch.AddConstraint(i, num, Vector3.Distance(bones[i].position, transform.position), 1f, 1f);
							if (parentIndices[i] >= 0)
							{
								Transform transform2 = bones[parentIndices[i]];
								float[] constraintCoordinates = new float[9]
								{
									transform2.position[0],
									transform2.position[1],
									transform2.position[2],
									transform.position[0],
									transform.position[1],
									transform.position[2],
									bones[i].position[0],
									bones[i].position[1],
									bones[i].position[2]
								};
								float restBend = Oni.BendingConstraintRest(constraintCoordinates);
								bendingBatch.AddConstraint(parentIndices[i], num, i, restBend, 0f, 0f);
							}
						}
					}
				}
				finally
				{
					IDisposable disposable;
					IDisposable disposable3 = (disposable = enumerator2 as IDisposable);
					if (disposable != null)
					{
						disposable3.Dispose();
					}
				}
				if (i % 10 == 0)
				{
					yield return new CoroutineJob.ProgressInfo("ObiBone: generating particles...", (float)i / (float)bones.Count);
				}
			}
			skinBatch.Cook();
			initializing = false;
			initialized = true;
		}

		public override void OnSolverStepBegin()
		{
			bool flag = base.enabled;
			if (animatorController != null)
			{
				animatorController.UpdateAnimation();
			}
			Vector4[] array = new Vector4[1] { Vector4.zero };
			Matrix4x4 matrix4x = ((!base.Solver.simulateInLocalSpace) ? ActorLocalToWorldMatrix : (base.Solver.transform.worldToLocalMatrix * ActorLocalToWorldMatrix));
			Matrix4x4 matrix4x2 = base.Solver.transform.worldToLocalMatrix * base.Solver.LastTransform;
			ObiSkinConstraintBatch obiSkinConstraintBatch = (ObiSkinConstraintBatch)SkinConstraints.GetBatches()[0];
			for (int i = 0; i < particleIndices.Length; i++)
			{
				Vector3 vector = matrix4x.MultiplyPoint3x4(base.transform.InverseTransformPoint(bones[i].position));
				if (!flag || invMasses[i] == 0f)
				{
					array[0] = vector;
					Oni.SetParticlePositions(solver.OniSolver, array, 1, particleIndices[i]);
				}
				else if (base.Solver.simulateInLocalSpace)
				{
					Oni.GetParticlePositions(solver.OniSolver, array, 1, particleIndices[i]);
					array[0] = Vector3.Lerp(array[0], matrix4x2.MultiplyPoint3x4(array[0]), worldVelocityScale);
					Oni.SetParticlePositions(solver.OniSolver, array, 1, particleIndices[i]);
				}
				obiSkinConstraintBatch.skinPoints[i] = vector;
			}
			obiSkinConstraintBatch.PushDataToSolver(SkinConstraints);
		}

		public override void OnSolverStepEnd()
		{
			base.OnSolverStepEnd();
			if (animatorController != null)
			{
				animatorController.ResetUpdateFlag();
			}
		}

		public void UpdateBones()
		{
			for (int i = 0; i < bones.Count; i++)
			{
				if (frozen[i])
				{
					continue;
				}
				Vector3 particlePosition = GetParticlePosition(i);
				if (parentIndices[i] >= 0 && !frozen[parentIndices[i]])
				{
					Transform transform = bones[parentIndices[i]];
					if (transform.childCount <= 1)
					{
						Vector3 fromDirection = transform.TransformDirection(bones[i].localPosition);
						Vector3 toDirection = particlePosition - GetParticlePosition(parentIndices[i]);
						transform.rotation = Quaternion.FromToRotation(fromDirection, toDirection) * transform.rotation;
					}
				}
				bones[i].position = particlePosition;
			}
		}

		public override void ResetActor()
		{
			PushDataToSolver(ParticleData.POSITIONS | ParticleData.VELOCITIES);
			if (particleIndices != null)
			{
				for (int i = 0; i < particleIndices.Length; i++)
				{
					solver.renderablePositions[particleIndices[i]] = positions[i];
					bones[i].position = base.transform.TransformPoint(positions[i]);
				}
			}
		}
	}
}
