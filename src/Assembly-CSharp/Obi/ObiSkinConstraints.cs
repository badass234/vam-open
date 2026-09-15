using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[DisallowMultipleComponent]
	public class ObiSkinConstraints : ObiBatchedConstraints
	{
		[Range(0f, 1f)]
		[Tooltip("Skin constraints stiffness.")]
		public float stiffness = 1f;

		[SerializeField]
		[HideInInspector]
		private List<ObiSkinConstraintBatch> batches = new List<ObiSkinConstraintBatch>();

		public override Oni.ConstraintType GetConstraintType()
		{
			return Oni.ConstraintType.Skin;
		}

		public override List<ObiConstraintBatch> GetBatches()
		{
			return batches.ConvertAll((Converter<ObiSkinConstraintBatch, ObiConstraintBatch>)((ObiSkinConstraintBatch x) => x));
		}

		public override void Clear()
		{
			RemoveFromSolver(null);
			batches.Clear();
		}

		public void AddBatch(ObiSkinConstraintBatch batch)
		{
			if (batch != null && batch.GetConstraintType() == GetConstraintType())
			{
				batches.Add(batch);
			}
		}

		public void RemoveBatch(ObiSkinConstraintBatch batch)
		{
			batches.Remove(batch);
		}

		public void OnDrawGizmosSelected()
		{
			if (!visualize)
			{
				return;
			}
			Gizmos.color = Color.magenta;
			Matrix4x4 localToWorldMatrix = actor.Solver.transform.localToWorldMatrix;
			foreach (ObiSkinConstraintBatch batch in batches)
			{
				batch.PullDataFromSolver(this);
				foreach (int activeConstraint in batch.ActiveConstraints)
				{
					Vector3 vector = batch.GetSkinPosition(activeConstraint);
					if (!base.InSolver)
					{
						vector = base.transform.TransformPoint(vector);
					}
					else if (actor.Solver.simulateInLocalSpace)
					{
						vector = localToWorldMatrix.MultiplyPoint3x4(vector);
					}
					if (actor.invMasses[batch.skinIndices[activeConstraint]] > 0f)
					{
						Gizmos.DrawLine(vector, actor.GetParticlePosition(batch.skinIndices[activeConstraint]));
					}
				}
			}
		}
	}
}
