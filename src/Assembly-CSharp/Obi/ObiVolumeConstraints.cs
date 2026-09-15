using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[DisallowMultipleComponent]
	public class ObiVolumeConstraints : ObiBatchedConstraints
	{
		[Tooltip("Amount of pressure applied to the cloth.")]
		public float overpressure = 1f;

		[Range(0f, 1f)]
		[Tooltip("Stiffness of the volume constraints. Higher values will make the constraints to try harder to enforce the set volume.")]
		public float stiffness = 1f;

		[SerializeField]
		[HideInInspector]
		private List<ObiVolumeConstraintBatch> batches = new List<ObiVolumeConstraintBatch>();

		public override Oni.ConstraintType GetConstraintType()
		{
			return Oni.ConstraintType.Volume;
		}

		public override List<ObiConstraintBatch> GetBatches()
		{
			return batches.ConvertAll((Converter<ObiVolumeConstraintBatch, ObiConstraintBatch>)((ObiVolumeConstraintBatch x) => x));
		}

		public override void Clear()
		{
			RemoveFromSolver(null);
			batches.Clear();
		}

		public void AddBatch(ObiVolumeConstraintBatch batch)
		{
			if (batch != null && batch.GetConstraintType() == GetConstraintType())
			{
				batches.Add(batch);
			}
		}

		public void RemoveBatch(ObiVolumeConstraintBatch batch)
		{
			batches.Remove(batch);
		}

		public void OnDrawGizmosSelected()
		{
			if (!visualize)
			{
				return;
			}
			Gizmos.color = Color.red;
			foreach (ObiVolumeConstraintBatch batch in batches)
			{
				foreach (int activeConstraint in batch.ActiveConstraints)
				{
					int num = batch.firstTriangle[activeConstraint];
					for (int i = 0; i < batch.numTriangles[activeConstraint]; i++)
					{
						int num2 = num + i;
						Vector3 particlePosition = actor.GetParticlePosition(batch.triangleIndices[num2 * 3]);
						Vector3 particlePosition2 = actor.GetParticlePosition(batch.triangleIndices[num2 * 3 + 1]);
						Vector3 particlePosition3 = actor.GetParticlePosition(batch.triangleIndices[num2 * 3 + 2]);
						Gizmos.DrawLine(particlePosition, particlePosition2);
						Gizmos.DrawLine(particlePosition, particlePosition3);
						Gizmos.DrawLine(particlePosition2, particlePosition3);
					}
				}
			}
		}
	}
}
