using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[Serializable]
	public class ObiPinConstraintBatch : ObiConstraintBatch
	{
		[HideInInspector]
		public List<int> pinIndices = new List<int>();

		[HideInInspector]
		public List<ObiCollider> pinBodies = new List<ObiCollider>();

		[HideInInspector]
		public List<Vector4> pinOffsets = new List<Vector4>();

		[HideInInspector]
		public List<float> stiffnesses = new List<float>();

		[HideInInspector]
		public List<float> pinBreakResistance = new List<float>();

		private int[] solverIndices = new int[0];

		private IntPtr[] solverColliders = new IntPtr[0];

		public ObiPinConstraintBatch(bool cooked, bool sharesParticles)
			: base(cooked, sharesParticles)
		{
		}

		public ObiPinConstraintBatch(bool cooked, bool sharesParticles, float minYoungModulus, float maxYoungModulus)
			: base(cooked, sharesParticles, minYoungModulus, maxYoungModulus)
		{
		}

		public override Oni.ConstraintType GetConstraintType()
		{
			return Oni.ConstraintType.Pin;
		}

		public override void Clear()
		{
			activeConstraints.Clear();
			pinIndices.Clear();
			pinBodies.Clear();
			pinOffsets.Clear();
			stiffnesses.Clear();
			pinBreakResistance.Clear();
			constraintCount = 0;
		}

		public void AddConstraint(int index1, ObiCollider body, Vector3 offset, float stiffness)
		{
			activeConstraints.Add(constraintCount);
			pinIndices.Add(index1);
			pinBodies.Add(body);
			pinOffsets.Add(offset);
			stiffnesses.Add(stiffness);
			pinBreakResistance.Add(float.MaxValue);
			constraintCount++;
		}

		public void RemoveConstraint(int index)
		{
			if (index < 0 || index >= base.ConstraintCount)
			{
				return;
			}
			activeConstraints.Remove(index);
			for (int i = 0; i < activeConstraints.Count; i++)
			{
				if (activeConstraints[i] > index)
				{
					activeConstraints[i]--;
				}
			}
			pinIndices.RemoveAt(index);
			pinBodies.RemoveAt(index);
			pinOffsets.RemoveAt(index);
			stiffnesses.RemoveAt(index);
			pinBreakResistance.RemoveAt(index);
			constraintCount--;
		}

		public override List<int> GetConstraintsInvolvingParticle(int particleIndex)
		{
			List<int> list = new List<int>(5);
			for (int i = 0; i < base.ConstraintCount; i++)
			{
				if (pinIndices[i] == particleIndex)
				{
					list.Add(i);
				}
			}
			return list;
		}

		protected override void OnAddToSolver(ObiBatchedConstraints constraints)
		{
			solverIndices = new int[pinIndices.Count];
			solverColliders = new IntPtr[pinIndices.Count];
			for (int i = 0; i < pinOffsets.Count; i++)
			{
				solverIndices[i] = constraints.Actor.particleIndices[pinIndices[i]];
				solverColliders[i] = ((!(pinBodies[i] != null)) ? IntPtr.Zero : pinBodies[i].OniCollider);
			}
		}

		protected override void OnRemoveFromSolver(ObiBatchedConstraints constraints)
		{
		}

		public override void PushDataToSolver(ObiBatchedConstraints constraints)
		{
			if (!(constraints == null) && !(constraints.Actor == null) && constraints.Actor.InSolver)
			{
				ObiPinConstraints obiPinConstraints = (ObiPinConstraints)constraints;
				for (int i = 0; i < stiffnesses.Count; i++)
				{
					stiffnesses[i] = StiffnessToCompliance(obiPinConstraints.stiffness);
				}
				Oni.SetPinConstraints(batch, solverIndices, pinOffsets.ToArray(), solverColliders, stiffnesses.ToArray(), base.ConstraintCount);
			}
		}

		public override void PullDataFromSolver(ObiBatchedConstraints constraints)
		{
		}

		public void BreakConstraints()
		{
			float[] array = new float[base.ConstraintCount];
			Oni.GetBatchConstraintForces(batch, array, base.ConstraintCount, 0);
			for (int i = 0; i < array.Length; i++)
			{
				if ((0f - array[i]) * 1000f > pinBreakResistance[i])
				{
					activeConstraints.Remove(i);
				}
			}
			SetActiveConstraints();
		}
	}
}
