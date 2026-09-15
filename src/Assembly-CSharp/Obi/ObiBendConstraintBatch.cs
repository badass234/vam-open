using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Obi
{
	[Serializable]
	public class ObiBendConstraintBatch : ObiConstraintBatch
	{
		public enum BendIndexType
		{
			First,
			Second,
			Pivot
		}

		[HideInInspector]
		public List<int> bendingIndices = new List<int>();

		[HideInInspector]
		public List<float> restBends = new List<float>();

		[HideInInspector]
		public List<Vector2> bendingStiffnesses = new List<Vector2>();

		private int[] solverIndices = new int[0];

		public ObiBendConstraintBatch(bool cooked, bool sharesParticles)
			: base(cooked, sharesParticles)
		{
		}

		public ObiBendConstraintBatch(bool cooked, bool sharesParticles, float minYoungModulus, float maxYoungModulus)
			: base(cooked, sharesParticles, minYoungModulus, maxYoungModulus)
		{
		}

		public override Oni.ConstraintType GetConstraintType()
		{
			return Oni.ConstraintType.Bending;
		}

		public override void Clear()
		{
			activeConstraints.Clear();
			bendingIndices.Clear();
			restBends.Clear();
			bendingStiffnesses.Clear();
			constraintCount = 0;
		}

		public void AddConstraint(int index1, int index2, int index3, float restBend, float bending, float stiffness)
		{
			activeConstraints.Add(constraintCount);
			bendingIndices.Add(index1);
			bendingIndices.Add(index2);
			bendingIndices.Add(index3);
			restBends.Add(restBend);
			bendingStiffnesses.Add(new Vector2(bending, stiffness));
			constraintCount++;
		}

		public void InsertConstraint(int constraintIndex, int index1, int index2, int index3, float restBend, float bending, float stiffness)
		{
			if (constraintIndex < 0 || constraintIndex > base.ConstraintCount)
			{
				return;
			}
			for (int i = 0; i < activeConstraints.Count; i++)
			{
				if (activeConstraints[i] >= constraintIndex)
				{
					activeConstraints[i]++;
				}
			}
			activeConstraints.Add(constraintIndex);
			bendingIndices.Insert(constraintIndex * 3, index1);
			bendingIndices.Insert(constraintIndex * 3 + 1, index2);
			bendingIndices.Insert(constraintIndex * 3 + 2, index3);
			restBends.Insert(constraintIndex, restBend);
			bendingStiffnesses.Insert(constraintIndex, new Vector2(bending, stiffness));
			constraintCount++;
		}

		public void SetParticleIndex(int constraintIndex, int particleIndex, BendIndexType type, bool wraparound)
		{
			if (!wraparound)
			{
				if (constraintIndex >= 0 && constraintIndex < base.ConstraintCount)
				{
					bendingIndices[(int)(constraintIndex * 3 + type)] = particleIndex;
				}
			}
			else
			{
				bendingIndices[(int)((int)ObiUtils.Mod(constraintIndex, base.ConstraintCount) * 3 + type)] = particleIndex;
			}
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
			bendingIndices.RemoveRange(index * 3, 3);
			restBends.RemoveAt(index);
			bendingStiffnesses.RemoveAt(index);
			constraintCount--;
		}

		public override List<int> GetConstraintsInvolvingParticle(int particleIndex)
		{
			List<int> list = new List<int>(5);
			for (int i = 0; i < base.ConstraintCount; i++)
			{
				if (bendingIndices[i * 3] == particleIndex || bendingIndices[i * 3 + 1] == particleIndex || bendingIndices[i * 3 + 2] == particleIndex)
				{
					list.Add(i);
				}
			}
			return list;
		}

		public override void Cook()
		{
			batch = Oni.CreateBatch(3, true);
			Oni.SetBendingConstraints(batch, bendingIndices.ToArray(), restBends.ToArray(), bendingStiffnesses.ToArray(), base.ConstraintCount);
			if (Oni.CookBatch(batch))
			{
				constraintCount = Oni.GetBatchConstraintCount(batch);
				activeConstraints = Enumerable.Range(0, constraintCount).ToList();
				int[] array = new int[constraintCount * 3];
				float[] collection = new float[constraintCount];
				Vector2[] collection2 = new Vector2[constraintCount];
				Oni.GetBendingConstraints(batch, array, collection, collection2);
				bendingIndices = new List<int>(array);
				restBends = new List<float>(collection);
				bendingStiffnesses = new List<Vector2>(collection2);
				int batchPhaseCount = Oni.GetBatchPhaseCount(batch);
				int[] collection3 = new int[batchPhaseCount];
				Oni.GetBatchPhaseSizes(batch, collection3);
				phaseSizes = new List<int>(collection3);
			}
			Oni.DestroyBatch(batch);
			batch = IntPtr.Zero;
		}

		protected override void OnAddToSolver(ObiBatchedConstraints constraints)
		{
			solverIndices = new int[bendingIndices.Count];
			for (int i = 0; i < restBends.Count; i++)
			{
				solverIndices[i * 3] = constraints.Actor.particleIndices[bendingIndices[i * 3]];
				solverIndices[i * 3 + 1] = constraints.Actor.particleIndices[bendingIndices[i * 3 + 1]];
				solverIndices[i * 3 + 2] = constraints.Actor.particleIndices[bendingIndices[i * 3 + 2]];
			}
		}

		protected override void OnRemoveFromSolver(ObiBatchedConstraints constraints)
		{
		}

		public override void PushDataToSolver(ObiBatchedConstraints constraints)
		{
			if (!(constraints == null) && !(constraints.Actor == null) && constraints.Actor.InSolver)
			{
				ObiBendingConstraints obiBendingConstraints = (ObiBendingConstraints)constraints;
				for (int i = 0; i < bendingStiffnesses.Count; i++)
				{
					bendingStiffnesses[i] = new Vector2(obiBendingConstraints.maxBending, StiffnessToCompliance(obiBendingConstraints.stiffness));
				}
				Oni.SetBendingConstraints(batch, solverIndices, restBends.ToArray(), bendingStiffnesses.ToArray(), base.ConstraintCount);
				Oni.SetBatchPhaseSizes(batch, phaseSizes.ToArray(), phaseSizes.Count);
			}
		}

		public override void PullDataFromSolver(ObiBatchedConstraints constraints)
		{
		}
	}
}
