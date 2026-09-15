using System;
using System.Collections.Generic;

namespace Obi
{
	public class ObiArbiter
	{
		private static List<ObiSolver> solvers = new List<ObiSolver>();

		private static int solverCounter = 0;

		private static int profileThrottle = 30;

		private static int stepCounter = 0;

		private static bool frameStarted = false;

		public static event EventHandler OnFrameStart;

		public static event EventHandler OnFrameEnd;

		public static void RegisterSolver(ObiSolver solver)
		{
			if (solver != null)
			{
				solvers.Add(solver);
			}
		}

		public static void UnregisterSolver(ObiSolver solver)
		{
			if (solver != null)
			{
				solvers.Remove(solver);
			}
		}

		public static void FrameStart()
		{
			if (!frameStarted)
			{
				frameStarted = true;
				if (ObiArbiter.OnFrameStart != null)
				{
					ObiArbiter.OnFrameStart(null, null);
				}
				Oni.SignalFrameStart();
			}
		}

		public static double FrameEnd()
		{
			return Oni.SignalFrameEnd();
		}

		public static Oni.ProfileInfo[] GetProfileInfo()
		{
			int profilingInfoCount = Oni.GetProfilingInfoCount();
			Oni.ProfileInfo[] array = new Oni.ProfileInfo[profilingInfoCount];
			Oni.GetProfilingInfo(array, profilingInfoCount);
			return array;
		}

		public static void WaitForAllSolvers()
		{
			solverCounter++;
			if (solverCounter < solvers.Count)
			{
				return;
			}
			solverCounter = 0;
			Oni.WaitForAllTasks();
			stepCounter--;
			if (stepCounter <= 0)
			{
				ObiProfiler.frameDuration = FrameEnd();
				ObiProfiler.info = GetProfileInfo();
				stepCounter = profileThrottle;
			}
			if (ObiArbiter.OnFrameEnd != null)
			{
				ObiArbiter.OnFrameEnd(null, null);
			}
			foreach (ObiSolver solver in solvers)
			{
				solver.AllSolversStepEnd();
			}
			frameStarted = false;
		}
	}
}
