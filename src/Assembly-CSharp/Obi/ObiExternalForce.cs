using UnityEngine;

namespace Obi
{
	public abstract class ObiExternalForce : MonoBehaviour
	{
		public float intensity;

		public float turbulence;

		public float turbulenceFrequency = 1f;

		public float turbulenceSeed;

		public ObiSolver[] affectedSolvers;

		public void LateUpdate()
		{
			ObiSolver[] array = affectedSolvers;
			foreach (ObiSolver obiSolver in array)
			{
				if (!(obiSolver != null))
				{
					continue;
				}
				foreach (ObiActor actor in obiSolver.actors)
				{
					if (actor != null)
					{
						ApplyForcesToActor(actor);
					}
				}
			}
		}

		protected float GetTurbulence(float turbulenceIntensity)
		{
			return Mathf.PerlinNoise(Time.fixedTime * turbulenceFrequency, turbulenceSeed) * turbulenceIntensity;
		}

		public abstract void ApplyForcesToActor(ObiActor actor);
	}
}
