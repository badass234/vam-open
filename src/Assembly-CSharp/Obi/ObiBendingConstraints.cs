using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[DisallowMultipleComponent]
	public class ObiBendingConstraints : ObiBatchedConstraints
	{
		[Tooltip("Bending offset. Leave at zero to keep the original bending amount.")]
		public float maxBending;

		[Range(0f, 1f)]
		[Tooltip("Cloth resistance to bending. Higher values will yield more stiff cloth.")]
		public float stiffness = 1f;

		[SerializeField]
		[HideInInspector]
		private List<ObiBendConstraintBatch> batches = new List<ObiBendConstraintBatch>();

		public override Oni.ConstraintType GetConstraintType()
		{
			return Oni.ConstraintType.Bending;
		}

		public override List<ObiConstraintBatch> GetBatches()
		{
			return batches.ConvertAll((Converter<ObiBendConstraintBatch, ObiConstraintBatch>)((ObiBendConstraintBatch x) => x));
		}

		public override void Clear()
		{
			RemoveFromSolver(null);
			batches.Clear();
		}

		public void AddBatch(ObiBendConstraintBatch batch)
		{
			if (batch != null && batch.GetConstraintType() == GetConstraintType())
			{
				batches.Add(batch);
			}
		}

		public void RemoveBatch(ObiBendConstraintBatch batch)
		{
			batches.Remove(batch);
		}

		public void OnDrawGizmosSelected()
		{
			if (!visualize)
			{
				return;
			}
			Gizmos.color = new Color(0.5f, 0f, 1f, 1f);
			foreach (ObiBendConstraintBatch batch in batches)
			{
				foreach (int activeConstraint in batch.ActiveConstraints)
				{
					Gizmos.DrawLine(actor.GetParticlePosition(batch.bendingIndices[activeConstraint * 3]), actor.GetParticlePosition(batch.bendingIndices[activeConstraint * 3 + 1]));
				}
			}
		}
	}
}
