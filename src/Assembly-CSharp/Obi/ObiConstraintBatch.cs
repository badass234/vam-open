using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[Serializable]
	public abstract class ObiConstraintBatch
	{
		protected IntPtr batch;

		public float maxYoungModulus = 0.02f;

		public float minYoungModulus = 0.0001f;

		[SerializeField]
		[HideInInspector]
		protected int constraintCount;

		[SerializeField]
		[HideInInspector]
		protected bool cooked;

		[SerializeField]
		[HideInInspector]
		protected bool sharesParticles;

		[SerializeField]
		[HideInInspector]
		protected List<int> activeConstraints = new List<int>();

		[SerializeField]
		[HideInInspector]
		protected List<int> phaseSizes = new List<int>();

		public IntPtr OniBatch => batch;

		public int ConstraintCount => constraintCount;

		public bool IsCooked => cooked;

		public bool SharesParticles => sharesParticles;

		public IEnumerable<int> ActiveConstraints => activeConstraints.AsReadOnly();

		public ObiConstraintBatch(bool cooked, bool sharesParticles)
		{
			this.cooked = cooked;
			this.sharesParticles = sharesParticles;
		}

		public ObiConstraintBatch(bool cooked, bool sharesParticles, float minYoungModulus, float maxYoungModulus)
		{
			this.cooked = cooked;
			this.sharesParticles = sharesParticles;
			this.maxYoungModulus = maxYoungModulus;
			this.minYoungModulus = minYoungModulus;
		}

		public abstract Oni.ConstraintType GetConstraintType();

		public abstract void Clear();

		public virtual void Cook()
		{
		}

		protected abstract void OnAddToSolver(ObiBatchedConstraints constraints);

		protected abstract void OnRemoveFromSolver(ObiBatchedConstraints constraints);

		public abstract void PushDataToSolver(ObiBatchedConstraints constraints);

		public abstract void PullDataFromSolver(ObiBatchedConstraints constraints);

		public abstract List<int> GetConstraintsInvolvingParticle(int particleIndex);

		protected float StiffnessToCompliance(float stiffness)
		{
			float a = 1f / Mathf.Max(minYoungModulus, 1E-05f);
			float b = 1f / Mathf.Max(maxYoungModulus, minYoungModulus);
			return Mathf.Lerp(a, b, stiffness);
		}

		public void ActivateConstraint(int index)
		{
			if (!activeConstraints.Contains(index))
			{
				activeConstraints.Add(index);
			}
		}

		public void DeactivateConstraint(int index)
		{
			activeConstraints.Remove(index);
		}

		public void AddToSolver(ObiBatchedConstraints constraints)
		{
			batch = Oni.CreateBatch((int)GetConstraintType(), cooked);
			Oni.AddBatch(constraints.Actor.Solver.OniSolver, batch, sharesParticles);
			OnAddToSolver(constraints);
		}

		public void RemoveFromSolver(ObiBatchedConstraints constraints)
		{
			OnRemoveFromSolver(constraints);
			Oni.RemoveBatch(constraints.Actor.Solver.OniSolver, batch);
			batch = IntPtr.Zero;
		}

		public void SetActiveConstraints()
		{
			Oni.SetActiveConstraints(batch, activeConstraints.ToArray(), activeConstraints.Count);
		}

		public void Enable()
		{
			Oni.EnableBatch(batch, true);
		}

		public void Disable()
		{
			Oni.EnableBatch(batch, false);
		}
	}
}
