using System.Collections;
using UnityEngine;

namespace Obi
{
	[RequireComponent(typeof(ObiRope))]
	[RequireComponent(typeof(ObiCatmullRomCurve))]
	public class ObiRopeHelper : MonoBehaviour
	{
		public ObiSolver solver;

		public ObiRopeSection section;

		public Material material;

		public Transform start;

		public Transform end;

		private ObiRope rope;

		private ObiCatmullRomCurve path;

		private void Start()
		{
			rope = GetComponent<ObiRope>();
			path = GetComponent<ObiCatmullRomCurve>();
			rope.Solver = solver;
			rope.ropePath = path;
			rope.Section = section;
			GetComponent<MeshRenderer>().material = material;
			Vector3 vector = base.transform.InverseTransformPoint(start.position);
			Vector3 vector2 = base.transform.InverseTransformPoint(end.position);
			Vector3 normalized = (vector2 - vector).normalized;
			path.controlPoints.Clear();
			path.controlPoints.Add(vector - normalized);
			path.controlPoints.Add(vector);
			path.controlPoints.Add(vector2);
			path.controlPoints.Add(vector2 + normalized);
			StartCoroutine(Setup());
		}

		private IEnumerator Setup()
		{
			yield return StartCoroutine(rope.GeneratePhysicRepresentationForMesh());
			rope.AddToSolver(null);
			rope.invMasses[0] = 0f;
			rope.invMasses[rope.UsedParticles - 1] = 0f;
			Oni.SetParticleInverseMasses(solver.OniSolver, new float[1], 1, rope.particleIndices[0]);
			Oni.SetParticleInverseMasses(solver.OniSolver, new float[1], 1, rope.particleIndices[rope.UsedParticles - 1]);
		}
	}
}
