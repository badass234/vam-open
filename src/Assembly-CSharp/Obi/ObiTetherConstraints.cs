using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[DisallowMultipleComponent]
	public class ObiTetherConstraints : ObiBatchedConstraints
	{
		[Range(0.1f, 2f)]
		[Tooltip("Scale of tether constraints. Values > 1 will expand initial tether length, values < 1 will make it shrink.")]
		public float tetherScale = 1f;

		[Range(0f, 1f)]
		[Tooltip("Tether resistance to stretching. Lower values will enforce tethers with more strenght.")]
		public float stiffness = 1f;

		[SerializeField]
		[HideInInspector]
		private List<ObiTetherConstraintBatch> batches = new List<ObiTetherConstraintBatch>();

		public override Oni.ConstraintType GetConstraintType()
		{
			return Oni.ConstraintType.Tether;
		}

		public override List<ObiConstraintBatch> GetBatches()
		{
			return batches.ConvertAll((Converter<ObiTetherConstraintBatch, ObiConstraintBatch>)((ObiTetherConstraintBatch x) => x));
		}

		public override void Clear()
		{
			RemoveFromSolver(null);
			batches.Clear();
		}

		public void AddBatch(ObiTetherConstraintBatch batch)
		{
			if (batch != null && batch.GetConstraintType() == GetConstraintType())
			{
				batches.Add(batch);
			}
		}

		public void RemoveBatch(ObiTetherConstraintBatch batch)
		{
			batches.Remove(batch);
		}

		public void OnDrawGizmosSelected()
		{
			if (!visualize)
			{
				return;
			}
			Gizmos.color = Color.yellow;
			foreach (ObiTetherConstraintBatch batch in batches)
			{
				foreach (int activeConstraint in batch.ActiveConstraints)
				{
					Gizmos.DrawLine(actor.GetParticlePosition(batch.tetherIndices[activeConstraint * 2]), actor.GetParticlePosition(batch.tetherIndices[activeConstraint * 2 + 1]));
				}
			}
		}
	}
}
