using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Obi
{
	[Serializable]
	public class ObiTetherConstraintBatch : ObiConstraintBatch
	{
		[HideInInspector]
		public List<int> tetherIndices = new List<int>();

		[HideInInspector]
		public List<Vector2> maxLengthsScales = new List<Vector2>();

		[HideInInspector]
		public List<float> stiffnesses = new List<float>();

		private int[] solverIndices = new int[0];

		public ObiTetherConstraintBatch(bool cooked, bool sharesParticles)
			: base(cooked, sharesParticles)
		{
		}

		public ObiTetherConstraintBatch(bool cooked, bool sharesParticles, float minYoungModulus, float maxYoungModulus)
			: base(cooked, sharesParticles, minYoungModulus, maxYoungModulus)
		{
		}

		public override Oni.ConstraintType GetConstraintType()
		{
			return Oni.ConstraintType.Tether;
		}

		public override void Clear()
		{
			activeConstraints.Clear();
			tetherIndices.Clear();
			maxLengthsScales.Clear();
			stiffnesses.Clear();
			constraintCount = 0;
		}

		public void AddConstraint(int index1, int index2, float maxLength, float scale, float stiffness)
		{
			activeConstraints.Add(constraintCount);
			tetherIndices.Add(index1);
			tetherIndices.Add(index2);
			maxLengthsScales.Add(new Vector2(maxLength, scale));
			stiffnesses.Add(stiffness);
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
			tetherIndices.RemoveRange(index * 2, 2);
			maxLengthsScales.RemoveAt(index);
			stiffnesses.RemoveAt(index);
			constraintCount--;
		}

		public override List<int> GetConstraintsInvolvingParticle(int particleIndex)
		{
			List<int> list = new List<int>(4);
			for (int i = 0; i < base.ConstraintCount; i++)
			{
				if (tetherIndices[i * 2] == particleIndex || tetherIndices[i * 2 + 1] == particleIndex)
				{
					list.Add(i);
				}
			}
			return list;
		}

		public override void Cook()
		{
			batch = Oni.CreateBatch(0, true);
			Oni.SetTetherConstraints(batch, tetherIndices.ToArray(), maxLengthsScales.ToArray(), stiffnesses.ToArray(), base.ConstraintCount);
			if (Oni.CookBatch(batch))
			{
				constraintCount = Oni.GetBatchConstraintCount(batch);
				activeConstraints = Enumerable.Range(0, constraintCount).ToList();
				int[] array = new int[constraintCount * 2];
				Vector2[] array2 = new Vector2[constraintCount];
				float[] collection = new float[constraintCount];
				Oni.GetTetherConstraints(batch, array, array2, collection);
				tetherIndices = new List<int>(array);
				maxLengthsScales = new List<Vector2>(array2);
				stiffnesses = new List<float>(collection);
				int batchPhaseCount = Oni.GetBatchPhaseCount(batch);
				int[] collection2 = new int[batchPhaseCount];
				Oni.GetBatchPhaseSizes(batch, collection2);
				phaseSizes = new List<int>(collection2);
			}
			Oni.DestroyBatch(batch);
			batch = IntPtr.Zero;
		}

		protected override void OnAddToSolver(ObiBatchedConstraints constraints)
		{
			solverIndices = new int[tetherIndices.Count];
			for (int i = 0; i < base.ConstraintCount; i++)
			{
				solverIndices[i * 2] = constraints.Actor.particleIndices[tetherIndices[i * 2]];
				solverIndices[i * 2 + 1] = constraints.Actor.particleIndices[tetherIndices[i * 2 + 1]];
			}
		}

		protected override void OnRemoveFromSolver(ObiBatchedConstraints constraints)
		{
		}

		public override void PushDataToSolver(ObiBatchedConstraints constraints)
		{
			if (!(constraints == null) && !(constraints.Actor == null) && constraints.Actor.InSolver)
			{
				ObiTetherConstraints obiTetherConstraints = (ObiTetherConstraints)constraints;
				for (int i = 0; i < base.ConstraintCount; i++)
				{
					maxLengthsScales[i] = new Vector2(maxLengthsScales[i].x, obiTetherConstraints.tetherScale);
					stiffnesses[i] = StiffnessToCompliance(obiTetherConstraints.stiffness);
				}
				Oni.SetTetherConstraints(batch, solverIndices, maxLengthsScales.ToArray(), stiffnesses.ToArray(), base.ConstraintCount);
				Oni.SetBatchPhaseSizes(batch, phaseSizes.ToArray(), phaseSizes.Count);
			}
		}

		public override void PullDataFromSolver(ObiBatchedConstraints constraints)
		{
		}
	}
}
