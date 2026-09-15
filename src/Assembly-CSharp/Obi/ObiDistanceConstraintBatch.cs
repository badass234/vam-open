using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Obi
{
	[Serializable]
	public class ObiDistanceConstraintBatch : ObiConstraintBatch
	{
		public enum DistanceIndexType
		{
			First,
			Second
		}

		[HideInInspector]
		public List<int> springIndices = new List<int>();

		[HideInInspector]
		public List<float> restLengths = new List<float>();

		[HideInInspector]
		public List<Vector2> stiffnesses = new List<Vector2>();

		private int[] solverIndices = new int[0];

		public ObiDistanceConstraintBatch(bool cooked, bool sharesParticles)
			: base(cooked, sharesParticles)
		{
		}

		public ObiDistanceConstraintBatch(bool cooked, bool sharesParticles, float minYoungModulus, float maxYoungModulus)
			: base(cooked, sharesParticles, minYoungModulus, maxYoungModulus)
		{
		}

		public override Oni.ConstraintType GetConstraintType()
		{
			return Oni.ConstraintType.Distance;
		}

		public override void Clear()
		{
			activeConstraints.Clear();
			springIndices.Clear();
			restLengths.Clear();
			stiffnesses.Clear();
			constraintCount = 0;
		}

		public void AddConstraint(int index1, int index2, float restLength, float stretchStiffness, float compressionStiffness)
		{
			activeConstraints.Add(constraintCount);
			springIndices.Add(index1);
			springIndices.Add(index2);
			restLengths.Add(restLength);
			stiffnesses.Add(new Vector2(stretchStiffness, compressionStiffness));
			constraintCount++;
		}

		public void InsertConstraint(int constraintIndex, int index1, int index2, float restLength, float stretchStiffness, float compressionStiffness)
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
			springIndices.Insert(constraintIndex * 2, index1);
			springIndices.Insert(constraintIndex * 2 + 1, index2);
			restLengths.Insert(constraintIndex, restLength);
			stiffnesses.Insert(constraintIndex, new Vector2(stretchStiffness, compressionStiffness));
			constraintCount++;
		}

		public void SetParticleIndex(int constraintIndex, int particleIndex, DistanceIndexType type, bool wraparound)
		{
			if (!wraparound)
			{
				if (constraintIndex >= 0 && constraintIndex < base.ConstraintCount)
				{
					springIndices[(int)(constraintIndex * 2 + type)] = particleIndex;
				}
			}
			else
			{
				springIndices[(int)((int)ObiUtils.Mod(constraintIndex, base.ConstraintCount) * 2 + type)] = particleIndex;
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
			springIndices.RemoveRange(index * 2, 2);
			restLengths.RemoveAt(index);
			stiffnesses.RemoveAt(index);
			constraintCount--;
		}

		public override List<int> GetConstraintsInvolvingParticle(int particleIndex)
		{
			List<int> list = new List<int>(10);
			for (int i = 0; i < base.ConstraintCount; i++)
			{
				if (springIndices[i * 2] == particleIndex || springIndices[i * 2 + 1] == particleIndex)
				{
					list.Add(i);
				}
			}
			return list;
		}

		public override void Cook()
		{
			batch = Oni.CreateBatch(4, true);
			Oni.SetDistanceConstraints(batch, springIndices.ToArray(), restLengths.ToArray(), stiffnesses.ToArray(), base.ConstraintCount);
			if (Oni.CookBatch(batch))
			{
				constraintCount = Oni.GetBatchConstraintCount(batch);
				activeConstraints = Enumerable.Range(0, constraintCount).ToList();
				int[] array = new int[constraintCount * 2];
				float[] collection = new float[constraintCount];
				Vector2[] collection2 = new Vector2[constraintCount];
				Oni.GetDistanceConstraints(batch, array, collection, collection2);
				springIndices = new List<int>(array);
				restLengths = new List<float>(collection);
				stiffnesses = new List<Vector2>(collection2);
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
			solverIndices = new int[springIndices.Count];
			for (int i = 0; i < restLengths.Count; i++)
			{
				solverIndices[i * 2] = constraints.Actor.particleIndices[springIndices[i * 2]];
				solverIndices[i * 2 + 1] = constraints.Actor.particleIndices[springIndices[i * 2 + 1]];
			}
		}

		protected override void OnRemoveFromSolver(ObiBatchedConstraints constraints)
		{
		}

		public override void PushDataToSolver(ObiBatchedConstraints constraints)
		{
			if (!(constraints == null) && !(constraints.Actor == null) && constraints.Actor.InSolver)
			{
				ObiDistanceConstraints obiDistanceConstraints = (ObiDistanceConstraints)constraints;
				float[] array = new float[restLengths.Count];
				for (int i = 0; i < restLengths.Count; i++)
				{
					array[i] = restLengths[i] * obiDistanceConstraints.stretchingScale;
					stiffnesses[i] = new Vector2(StiffnessToCompliance(obiDistanceConstraints.stiffness), obiDistanceConstraints.slack * array[i]);
				}
				Oni.SetDistanceConstraints(batch, solverIndices, array, stiffnesses.ToArray(), base.ConstraintCount);
				Oni.SetBatchPhaseSizes(batch, phaseSizes.ToArray(), phaseSizes.Count);
			}
		}

		public override void PullDataFromSolver(ObiBatchedConstraints constraints)
		{
		}
	}
}
