using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[DisallowMultipleComponent]
	public class ObiPinConstraints : ObiBatchedConstraints
	{
		[Range(0f, 1f)]
		[Tooltip("Pin resistance to stretching. Lower values will yield more elastic pin constraints.")]
		public float stiffness = 1f;

		[SerializeField]
		[HideInInspector]
		private List<ObiPinConstraintBatch> batches = new List<ObiPinConstraintBatch>();

		public override Oni.ConstraintType GetConstraintType()
		{
			return Oni.ConstraintType.Pin;
		}

		public override List<ObiConstraintBatch> GetBatches()
		{
			return batches.ConvertAll((Converter<ObiPinConstraintBatch, ObiConstraintBatch>)((ObiPinConstraintBatch x) => x));
		}

		public override void Clear()
		{
			RemoveFromSolver(null);
			batches.Clear();
		}

		public void AddBatch(ObiPinConstraintBatch batch)
		{
			if (batch != null && batch.GetConstraintType() == GetConstraintType())
			{
				batches.Add(batch);
			}
		}

		public void RemoveBatch(ObiPinConstraintBatch batch)
		{
			batches.Remove(batch);
		}

		public void OnDrawGizmosSelected()
		{
			if (!visualize)
			{
				return;
			}
			Gizmos.color = Color.cyan;
			foreach (ObiPinConstraintBatch batch in batches)
			{
				foreach (int activeConstraint in batch.ActiveConstraints)
				{
					Vector3 to = batch.pinBodies[activeConstraint].transform.TransformPoint(batch.pinOffsets[activeConstraint]);
					Gizmos.DrawLine(actor.GetParticlePosition(batch.pinIndices[activeConstraint]), to);
				}
			}
		}
	}
}
