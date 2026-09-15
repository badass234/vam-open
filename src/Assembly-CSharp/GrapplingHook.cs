using System.Collections;
using Obi;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
	public Collider character;

	public float hookExtendRetractSpeed = 2f;

	public Material material;

	private ObiRope rope;

	private ObiCatmullRomCurve curve;

	private ObiSolver solver;

	private ObiRopeCursor cursor;

	private RaycastHit hookAttachment;

	private bool attached;

	private void Awake()
	{
		rope = base.gameObject.AddComponent<ObiRope>();
		curve = base.gameObject.AddComponent<ObiCatmullRomCurve>();
		solver = base.gameObject.AddComponent<ObiSolver>();
		rope.Solver = solver;
		rope.ropePath = curve;
		rope.GetComponent<MeshRenderer>().material = material;
		rope.resolution = 0.1f;
		rope.BendingConstraints.stiffness = 0.2f;
		rope.UVScale = new Vector2(1f, 5f);
		rope.NormalizeV = false;
		rope.UVAnchor = 1f;
		solver.distanceConstraintParameters.iterations = 15;
		solver.pinConstraintParameters.iterations = 15;
		solver.bendingConstraintParameters.iterations = 1;
		cursor = rope.gameObject.AddComponent<ObiRopeCursor>();
		cursor.rope = rope;
		cursor.normalizedCoord = 0f;
		cursor.direction = true;
	}

	private void LaunchHook()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = base.transform.position.z - Camera.main.transform.position.z;
		Vector3 vector = Camera.main.ScreenToWorldPoint(mousePosition);
		Ray ray = new Ray(base.transform.position, vector - base.transform.position);
		if (Physics.Raycast(ray, out hookAttachment))
		{
			StartCoroutine(AttachHook());
		}
	}

	private IEnumerator AttachHook()
	{
		Vector3 localHit = curve.transform.InverseTransformPoint(hookAttachment.point);
		curve.controlPoints.Clear();
		curve.controlPoints.Add(Vector3.zero);
		curve.controlPoints.Add(Vector3.zero);
		curve.controlPoints.Add(localHit);
		curve.controlPoints.Add(localHit);
		yield return rope.GeneratePhysicRepresentationForMesh();
		rope.AddToSolver(null);
		rope.GetComponent<MeshRenderer>().enabled = true;
		attached = true;
	}

	private void DetachHook()
	{
		rope.RemoveFromSolver(null);
		rope.GetComponent<MeshRenderer>().enabled = false;
		attached = false;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (!attached)
			{
				LaunchHook();
			}
			else
			{
				DetachHook();
			}
		}
		if (Input.GetKey(KeyCode.W))
		{
			cursor.ChangeLength(rope.RestLength - hookExtendRetractSpeed * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.S))
		{
			cursor.ChangeLength(rope.RestLength + hookExtendRetractSpeed * Time.deltaTime);
		}
	}
}
