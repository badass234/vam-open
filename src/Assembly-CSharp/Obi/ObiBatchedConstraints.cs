using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	public abstract class ObiBatchedConstraints : MonoBehaviour, IObiSolverClient
	{
		public bool visualize;

		[NonSerialized]
		protected ObiActor actor;

		[NonSerialized]
		protected bool inSolver;

		public ObiActor Actor => actor;

		public bool InSolver => inSolver;

		public abstract Oni.ConstraintType GetConstraintType();

		public abstract List<ObiConstraintBatch> GetBatches();

		public abstract void Clear();

		protected void OnAddToSolver(object info)
		{
			foreach (ObiConstraintBatch batch in GetBatches())
			{
				batch.AddToSolver(this);
			}
		}

		protected void OnRemoveFromSolver(object info)
		{
			foreach (ObiConstraintBatch batch in GetBatches())
			{
				batch.RemoveFromSolver(this);
			}
		}

		public void PushDataToSolver(ParticleData data = ParticleData.NONE)
		{
			foreach (ObiConstraintBatch batch in GetBatches())
			{
				batch.PushDataToSolver(this);
			}
		}

		public void PullDataFromSolver(ParticleData data = ParticleData.NONE)
		{
			foreach (ObiConstraintBatch batch in GetBatches())
			{
				batch.PullDataFromSolver(this);
			}
		}

		public void SetActiveConstraints()
		{
			foreach (ObiConstraintBatch batch in GetBatches())
			{
				batch.SetActiveConstraints();
			}
		}

		public void Enable()
		{
			foreach (ObiConstraintBatch batch in GetBatches())
			{
				batch.Enable();
			}
		}

		public void Disable()
		{
			foreach (ObiConstraintBatch batch in GetBatches())
			{
				batch.Disable();
			}
		}

		public bool AddToSolver(object info)
		{
			if (inSolver || actor == null || !actor.InSolver)
			{
				return false;
			}
			OnAddToSolver(info);
			inSolver = true;
			PushDataToSolver();
			SetActiveConstraints();
			if (base.isActiveAndEnabled)
			{
				Enable();
			}
			else
			{
				Disable();
			}
			return true;
		}

		public bool RemoveFromSolver(object info)
		{
			if (!inSolver || actor == null || !actor.InSolver)
			{
				return false;
			}
			OnRemoveFromSolver(null);
			inSolver = false;
			return true;
		}

		public void GrabActor()
		{
			actor = GetComponent<ObiActor>();
		}

		public void OnEnable()
		{
			Enable();
		}

		public void OnDisable()
		{
			if (!(actor == null) && actor.InSolver)
			{
				Disable();
			}
		}

		public void OnDestroy()
		{
			RemoveFromSolver(null);
		}
	}
}
