using UnityEngine;

namespace Obi
{
	public class ObiEmitterMaterialGranular : ObiEmitterMaterial
	{
		public float randomness;

		public void OnValidate()
		{
			resolution = Mathf.Max(0.001f, resolution);
			restDensity = Mathf.Max(0.001f, restDensity);
			randomness = Mathf.Max(0f, randomness);
		}

		public override Oni.FluidMaterial GetEquivalentOniMaterial(Oni.SolverParameters.Mode mode)
		{
			Oni.FluidMaterial result = default(Oni.FluidMaterial);
			result.smoothingRadius = GetParticleSize(mode);
			result.restDensity = restDensity;
			result.viscosity = 0f;
			result.surfaceTension = 0f;
			result.buoyancy = -1f;
			result.atmosphericDrag = 0f;
			result.atmosphericPressure = 0f;
			result.vorticity = 0f;
			return result;
		}
	}
}
