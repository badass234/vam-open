using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	public class ObiParticleHandle : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		private ObiActor actor;

		[SerializeField]
		[HideInInspector]
		private List<int> handledParticleIndices = new List<int>();

		[SerializeField]
		[HideInInspector]
		private List<Vector3> handledParticlePositions = new List<Vector3>();

		[SerializeField]
		[HideInInspector]
		private List<float> handledParticleInvMasses = new List<float>();

		private const float HANDLED_PARTICLE_MASS = 0.0001f;

		public int ParticleCount => handledParticleIndices.Count;

		public ObiActor Actor
		{
			get
			{
				return actor;
			}
			set
			{
				if (actor != value)
				{
					if (actor != null && actor.Solver != null)
					{
						actor.Solver.OnFrameBegin -= Actor_solver_OnFrameBegin;
					}
					actor = value;
					if (actor != null && actor.Solver != null)
					{
						actor.Solver.OnFrameBegin += Actor_solver_OnFrameBegin;
					}
				}
			}
		}

		private void OnEnable()
		{
			if (actor != null && actor.Solver != null)
			{
				actor.Solver.OnFrameBegin += Actor_solver_OnFrameBegin;
			}
		}

		private void OnDisable()
		{
			if (actor != null && actor.Solver != null)
			{
				actor.Solver.OnFrameBegin -= Actor_solver_OnFrameBegin;
				ResetInvMasses();
			}
		}

		private void ResetInvMasses()
		{
			if (actor.InSolver)
			{
				float[] array = new float[1];
				for (int i = 0; i < handledParticleIndices.Count; i++)
				{
					int destOffset = actor.particleIndices[handledParticleIndices[i]];
					array[0] = (actor.invMasses[handledParticleIndices[i]] = handledParticleInvMasses[i]);
					Oni.SetParticleInverseMasses(actor.Solver.OniSolver, array, 1, destOffset);
				}
			}
		}

		public void Clear()
		{
			ResetInvMasses();
			handledParticleIndices.Clear();
			handledParticlePositions.Clear();
			handledParticleInvMasses.Clear();
		}

		public void AddParticle(int index, Vector3 position, float invMass)
		{
			handledParticleIndices.Add(index);
			handledParticlePositions.Add(base.transform.InverseTransformPoint(position));
			handledParticleInvMasses.Add(invMass);
		}

		public void RemoveParticle(int index)
		{
			int num = handledParticleIndices.IndexOf(index);
			if (num > -1)
			{
				if (actor.InSolver)
				{
					int destOffset = actor.particleIndices[index];
					float[] invMasses = new float[1] { actor.invMasses[index] = handledParticleInvMasses[num] };
					Oni.SetParticleInverseMasses(actor.Solver.OniSolver, invMasses, 1, destOffset);
				}
				handledParticleIndices.RemoveAt(num);
				handledParticlePositions.RemoveAt(num);
				handledParticleInvMasses.RemoveAt(num);
			}
		}

		private void Actor_solver_OnFrameBegin(object sender, EventArgs e)
		{
			if (actor.InSolver)
			{
				Vector4[] array = new Vector4[1];
				Vector4[] velocities = new Vector4[1] { -actor.Solver.parameters.gravity * Time.fixedDeltaTime };
				float[] invMasses = new float[1] { 0.0001f };
				Matrix4x4 matrix4x = ((!actor.Solver.simulateInLocalSpace) ? base.transform.localToWorldMatrix : (actor.Solver.transform.worldToLocalMatrix * base.transform.localToWorldMatrix));
				for (int i = 0; i < handledParticleIndices.Count; i++)
				{
					int destOffset = actor.particleIndices[handledParticleIndices[i]];
					Oni.SetParticleVelocities(actor.Solver.OniSolver, velocities, 1, destOffset);
					Oni.SetParticleInverseMasses(actor.Solver.OniSolver, invMasses, 1, destOffset);
					array[0] = matrix4x.MultiplyPoint3x4(handledParticlePositions[i]);
					Oni.SetParticlePositions(actor.Solver.OniSolver, array, 1, destOffset);
				}
			}
		}
	}
}
