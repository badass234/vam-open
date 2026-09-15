using UnityEngine;

namespace DynamicCSharp.Demo
{
	public class TankShell : MonoBehaviour
	{
		private Vector2 startPosition = Vector2.zero;

		private Vector2 heading = Vector2.zero;

		private bool hit;

		public float speed = 2f;

		public bool Step()
		{
			if (hit)
			{
				return true;
			}
			if (Vector2.Distance(startPosition, base.transform.position) > 20f)
			{
				hit = true;
			}
			base.transform.Translate(heading * (speed * Time.deltaTime));
			return false;
		}

		public void Destroy()
		{
			Object.Destroy(base.gameObject);
		}

		public void OnCollisionEnter2D(Collision2D collision)
		{
			Collider2D collider = collision.collider;
			if (collider.name == "DamagedWall")
			{
				Object.Destroy(collider.gameObject);
				hit = true;
			}
			else if (collider.name == "Wall")
			{
				hit = true;
			}
		}

		public static TankShell Shoot(GameObject prefab, Vector2 startPosition, Vector2 heading)
		{
			GameObject gameObject = Object.Instantiate(prefab, startPosition, Quaternion.identity);
			TankShell component = gameObject.GetComponent<TankShell>();
			if (component == null)
			{
				Object.Destroy(gameObject);
				return null;
			}
			component.startPosition = startPosition;
			component.heading = heading;
			return component;
		}
	}
}
