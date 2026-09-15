using Obi;
using UnityEngine;

[RequireComponent(typeof(ObiSolver))]
public class CollisionEventHandler : MonoBehaviour
{
	private ObiSolver solver;

	private ObiSolver.ObiCollisionEventArgs frame;

	private void Awake()
	{
		solver = GetComponent<ObiSolver>();
	}

	private void OnEnable()
	{
		solver.OnCollision += Solver_OnCollision;
	}

	private void OnDisable()
	{
		solver.OnCollision -= Solver_OnCollision;
	}

	private void Solver_OnCollision(object sender, ObiSolver.ObiCollisionEventArgs e)
	{
		frame = e;
	}

	private void OnDrawGizmos()
	{
		if (!(solver == null) && frame != null && frame.contacts != null)
		{
			Gizmos.color = Color.yellow;
			for (int i = 0; i < frame.contacts.Length; i++)
			{
				Gizmos.color = ((!(frame.contacts[i].distance < 0.01f)) ? Color.green : Color.red);
				Vector3 vector = frame.contacts[i].point;
				Vector3 vector2 = frame.contacts[i].normal;
				Gizmos.DrawSphere(vector, 0.025f);
				Gizmos.DrawRay(vector, vector2.normalized * 0.1f);
			}
		}
	}
}
