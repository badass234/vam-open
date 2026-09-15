using System;
using UnityEngine;

namespace Obi
{
	[RequireComponent(typeof(ObiSolver))]
	public class ObiParticleGridDebugger : MonoBehaviour
	{
		private ObiSolver solver;

		private Oni.GridCell[] cells;

		private void OnEnable()
		{
			solver = GetComponent<ObiSolver>();
			solver.OnFrameEnd += Solver_OnFrameEnd;
		}

		private void OnDisable()
		{
			solver.OnFrameEnd -= Solver_OnFrameEnd;
		}

		private void Solver_OnFrameEnd(object sender, EventArgs e)
		{
			int particleGridSize = Oni.GetParticleGridSize(solver.OniSolver);
			cells = new Oni.GridCell[particleGridSize];
			Oni.GetParticleGrid(solver.OniSolver, cells);
		}

		private void OnDrawGizmos()
		{
			if (cells != null)
			{
				Oni.GridCell[] array = cells;
				for (int i = 0; i < array.Length; i++)
				{
					Oni.GridCell gridCell = array[i];
					Gizmos.color = ((gridCell.count <= 0) ? Color.red : Color.yellow);
					Gizmos.DrawWireCube(gridCell.center, gridCell.size);
				}
			}
		}
	}
}
