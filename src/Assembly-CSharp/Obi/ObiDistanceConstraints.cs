using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[DisallowMultipleComponent]
	public class ObiDistanceConstraints : ObiBatchedConstraints
	{
		[Tooltip("Scale of stretching constraints. Values > 1 will expand initial cloth size, values < 1 will make it shrink.")]
		public float stretchingScale = 1f;

		[Range(0f, 1f)]
		[Tooltip("Cloth resistance to stretching. Lower values will yield more elastic cloth.")]
		public float stiffness = 1f;

		[Range(0f, 1f)]
		[Tooltip("Amount of compression slack. 0 means total resistance to compression, 1 no resistance at all. 0.5 means constraints will allow a compression of up to 50% of their rest length.")]
		public float slack;

		[SerializeField]
		[HideInInspector]
		private List<ObiDistanceConstraintBatch> batches = new List<ObiDistanceConstraintBatch>();

		public override Oni.ConstraintType GetConstraintType()
		{
			return Oni.ConstraintType.Distance;
		}

		public override List<ObiConstraintBatch> GetBatches()
		{
			return batches.ConvertAll((Converter<ObiDistanceConstraintBatch, ObiConstraintBatch>)((ObiDistanceConstraintBatch x) => x));
		}

		public override void Clear()
		{
			RemoveFromSolver(null);
			batches.Clear();
		}

		public void AddBatch(ObiDistanceConstraintBatch batch)
		{
			if (batch != null && batch.GetConstraintType() == GetConstraintType())
			{
				batches.Add(batch);
			}
		}

		public void RemoveBatch(ObiDistanceConstraintBatch batch)
		{
			batches.Remove(batch);
		}

		public void OnDrawGizmosSelected()
		{
			if (!visualize)
			{
				return;
			}
			Gizmos.color = Color.green;
			foreach (ObiDistanceConstraintBatch batch in batches)
			{
				foreach (int activeConstraint in batch.ActiveConstraints)
				{
					Gizmos.DrawLine(actor.GetParticlePosition(batch.springIndices[activeConstraint * 2]), actor.GetParticlePosition(batch.springIndices[activeConstraint * 2 + 1]));
				}
			}
		}
	}
}
