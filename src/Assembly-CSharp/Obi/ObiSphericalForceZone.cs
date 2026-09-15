using UnityEngine;

namespace Obi
{
	public class ObiSphericalForceZone : ObiExternalForce
	{
		public float radius = 5f;

		public bool radial = true;

		public void OnEnable()
		{
			ObiSolver[] array = affectedSolvers;
			foreach (ObiSolver obiSolver in array)
			{
				obiSolver.RequireRenderablePositions();
			}
		}

		public void OnDisable()
		{
			ObiSolver[] array = affectedSolvers;
			foreach (ObiSolver obiSolver in array)
			{
				obiSolver.RelinquishRenderablePositions();
			}
		}

		public override void ApplyForcesToActor(ObiActor actor)
		{
			Vector4 vector = ((!actor.Solver.simulateInLocalSpace) ? base.transform.localToWorldMatrix : (actor.Solver.transform.worldToLocalMatrix * base.transform.localToWorldMatrix)).MultiplyVector(Vector3.forward * (intensity + GetTurbulence(turbulence)));
			float num = radius * radius;
			Vector4[] array = new Vector4[actor.particleIndices.Length];
			Vector4 vector2 = new Vector4(base.transform.position.x, base.transform.position.y, base.transform.position.z);
			for (int i = 0; i < array.Length; i++)
			{
				Vector4 vector3 = actor.Solver.renderablePositions[actor.particleIndices[i]] - vector2;
				float sqrMagnitude = vector3.sqrMagnitude;
				float num2 = Mathf.Clamp01((num - sqrMagnitude) / num);
				if (radial)
				{
					array[i] = vector3 / (Mathf.Sqrt(sqrMagnitude) + float.Epsilon) * num2 * intensity;
				}
				else
				{
					array[i] = vector * num2;
				}
				array[i][3] = (actor.UsesCustomExternalForces ? 1 : 0);
			}
			Oni.AddParticleExternalForces(actor.Solver.OniSolver, array, actor.particleIndices, actor.particleIndices.Length);
		}

		public void OnDrawGizmosSelected()
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = new Color(0f, 0.7f, 1f, 1f);
			Gizmos.DrawWireSphere(Vector3.zero, radius);
			float num = GetTurbulence(1f);
			if (!radial)
			{
				ObiUtils.DrawArrowGizmo(radius + num, radius * 0.2f, radius * 0.3f, radius * 0.2f);
				return;
			}
			Gizmos.DrawLine(new Vector3(0f, 0f, (0f - radius) * 0.5f) * num, new Vector3(0f, 0f, radius * 0.5f) * num);
			Gizmos.DrawLine(new Vector3(0f, (0f - radius) * 0.5f, 0f) * num, new Vector3(0f, radius * 0.5f, 0f) * num);
			Gizmos.DrawLine(new Vector3((0f - radius) * 0.5f, 0f, 0f) * num, new Vector3(radius * 0.5f, 0f, 0f) * num);
		}
	}
}
