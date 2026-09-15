using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	[AddComponentMenu("Physics/Obi/Obi Stitcher")]
	public class ObiStitcher : MonoBehaviour, IObiSolverClient
	{
		[Serializable]
		public class Stitch
		{
			public int particleIndex1;

			public int particleIndex2;

			public Stitch(int particleIndex1, int particleIndex2)
			{
				this.particleIndex1 = particleIndex1;
				this.particleIndex2 = particleIndex2;
			}
		}

		[SerializeField]
		[HideInInspector]
		private List<Stitch> stitches = new List<Stitch>();

		[SerializeField]
		[HideInInspector]
		private ObiActor actor1;

		[SerializeField]
		[HideInInspector]
		private ObiActor actor2;

		private IntPtr batch;

		private bool inSolver;

		public ObiActor Actor1
		{
			get
			{
				return actor1;
			}
			set
			{
				if (!(actor1 != value))
				{
					return;
				}
				if (actor1 != null)
				{
					actor1.OnAddedToSolver -= Actor_OnAddedToSolver;
					actor1.OnRemovedFromSolver -= Actor_OnRemovedFromSolver;
					if (actor1.InSolver)
					{
						Actor_OnRemovedFromSolver(actor1, new ObiActor.ObiActorSolverArgs(actor1.Solver));
					}
				}
				actor1 = value;
				if (actor1 != null)
				{
					actor1.OnAddedToSolver += Actor_OnAddedToSolver;
					actor1.OnRemovedFromSolver += Actor_OnRemovedFromSolver;
					if (actor1.InSolver)
					{
						Actor_OnAddedToSolver(actor1, new ObiActor.ObiActorSolverArgs(actor1.Solver));
					}
				}
			}
		}

		public ObiActor Actor2
		{
			get
			{
				return actor2;
			}
			set
			{
				if (!(actor2 != value))
				{
					return;
				}
				if (actor2 != null)
				{
					actor2.OnAddedToSolver -= Actor_OnAddedToSolver;
					actor2.OnRemovedFromSolver -= Actor_OnRemovedFromSolver;
					if (actor2.InSolver)
					{
						Actor_OnRemovedFromSolver(actor2, new ObiActor.ObiActorSolverArgs(actor2.Solver));
					}
				}
				actor2 = value;
				if (actor2 != null)
				{
					actor2.OnAddedToSolver += Actor_OnAddedToSolver;
					actor2.OnRemovedFromSolver += Actor_OnRemovedFromSolver;
					if (actor2.InSolver)
					{
						Actor_OnAddedToSolver(actor2, new ObiActor.ObiActorSolverArgs(actor2.Solver));
					}
				}
			}
		}

		public int StitchCount => stitches.Count;

		public IEnumerable<Stitch> Stitches => stitches.AsReadOnly();

		public void OnEnable()
		{
			if (actor1 != null)
			{
				actor1.OnAddedToSolver += Actor_OnAddedToSolver;
				actor1.OnRemovedFromSolver += Actor_OnRemovedFromSolver;
			}
			if (actor2 != null)
			{
				actor2.OnAddedToSolver += Actor_OnAddedToSolver;
				actor2.OnRemovedFromSolver += Actor_OnRemovedFromSolver;
			}
			if (actor1 != null && actor2 != null)
			{
				Oni.EnableBatch(batch, true);
			}
		}

		public void OnDisable()
		{
			Oni.EnableBatch(batch, false);
		}

		public int AddStitch(int particle1, int particle2)
		{
			stitches.Add(new Stitch(particle1, particle2));
			return stitches.Count - 1;
		}

		public void RemoveStitch(int index)
		{
			if (index >= 0 && index < stitches.Count)
			{
				stitches.RemoveAt(index);
			}
		}

		public void Clear()
		{
			stitches.Clear();
			PushDataToSolver();
		}

		private void Actor_OnRemovedFromSolver(object sender, ObiActor.ObiActorSolverArgs e)
		{
			RemoveFromSolver(null);
		}

		private void Actor_OnAddedToSolver(object sender, ObiActor.ObiActorSolverArgs e)
		{
			if (actor1.InSolver && actor2.InSolver)
			{
				if (actor1.Solver != actor2.Solver)
				{
					Debug.LogError("ObiStitcher cannot handle actors in different solvers.");
				}
				else
				{
					AddToSolver(null);
				}
			}
		}

		public bool AddToSolver(object info)
		{
			batch = Oni.CreateBatch(10, false);
			Oni.AddBatch(actor1.Solver.OniSolver, batch, false);
			inSolver = true;
			PushDataToSolver();
			if (base.isActiveAndEnabled)
			{
				OnEnable();
			}
			else
			{
				OnDisable();
			}
			return true;
		}

		public bool RemoveFromSolver(object info)
		{
			Oni.RemoveBatch(actor1.Solver.OniSolver, batch);
			batch = IntPtr.Zero;
			inSolver = false;
			return true;
		}

		public void PushDataToSolver(ParticleData data = ParticleData.NONE)
		{
			if (inSolver)
			{
				int[] array = new int[stitches.Count * 2];
				float[] array2 = new float[stitches.Count];
				for (int i = 0; i < stitches.Count; i++)
				{
					array[i * 2] = actor1.particleIndices[stitches[i].particleIndex1];
					array[i * 2 + 1] = actor2.particleIndices[stitches[i].particleIndex2];
					array2[i] = 0f;
				}
				Oni.SetStitchConstraints(batch, array, array2, stitches.Count);
				int[] array3 = new int[stitches.Count];
				for (int j = 0; j < stitches.Count; j++)
				{
					array3[j] = j;
				}
				Oni.SetActiveConstraints(batch, array3, stitches.Count);
			}
		}

		public void PullDataFromSolver(ParticleData data = ParticleData.NONE)
		{
		}
	}
}
