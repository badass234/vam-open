using UnityEngine;

namespace Obi
{
	public class ObiAmbientForceZone : ObiExternalForce
	{
		public override void ApplyForcesToActor(ObiActor actor)
		{
			Vector4 force = ((!actor.Solver.simulateInLocalSpace) ? base.transform.localToWorldMatrix : (actor.Solver.transform.worldToLocalMatrix * base.transform.localToWorldMatrix)).MultiplyVector(Vector3.forward * (intensity + GetTurbulence(turbulence)));
			force[3] = (actor.UsesCustomExternalForces ? 1 : 0);
			Oni.AddParticleExternalForce(actor.Solver.OniSolver, ref force, actor.particleIndices, actor.particleIndices.Length);
		}

		public void OnDrawGizmosSelected()
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = new Color(0f, 0.7f, 1f, 1f);
			ObiUtils.DrawArrowGizmo(0.5f + GetTurbulence(1f), 0.2f, 0.3f, 0.2f);
		}
	}
}
