using Obi;
using UnityEngine;

[RequireComponent(typeof(ObiSolver))]
public class ColliderHighlighter : MonoBehaviour
{
	private ObiSolver solver;

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
		Oni.Contact[] contacts = e.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			Oni.Contact contact = contacts[i];
			if (contact.distance < 0.01f)
			{
				Collider collider = ObiColliderBase.idToCollider[contact.other] as Collider;
				Blinker component = collider.GetComponent<Blinker>();
				if ((bool)component)
				{
					component.Blink();
				}
			}
		}
	}
}
