using UnityEngine;

namespace VikingCrewDevelopment
{
	public class RandomMovementBehaviour : MonoBehaviour
	{
		public Rect bounds = new Rect(-10f, -10f, 20f, 20f);

		public float speed = 1f;

		private Vector2 nextWaypoint;

		private void Start()
		{
			nextWaypoint = GetNextWaypoint();
		}

		private void Update()
		{
			base.transform.position = Vector2.MoveTowards(base.transform.position, nextWaypoint, speed * Time.deltaTime);
			if ((double)Vector2.Distance(base.transform.position, nextWaypoint) < 0.25)
			{
				nextWaypoint = GetNextWaypoint();
			}
		}

		private Vector2 GetNextWaypoint()
		{
			return new Vector2(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax));
		}
	}
}
