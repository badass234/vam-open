using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicCSharp.Demo
{
	public abstract class TankController : MonoBehaviour
	{
		private Queue<TankEvent> tankTasks = new Queue<TankEvent>();

		private bool crash;

		[HideInInspector]
		public GameObject bulletObject;

		[HideInInspector]
		public float moveSpeed = 1f;

		[HideInInspector]
		public float rotateSpeed = 3f;

		public abstract void TankMain();

		public void OnCollisionEnter2D(Collision2D collision)
		{
			Collider2D collider = collision.collider;
			if (collider.name == "DamagedWall" || collider.name == "Wall")
			{
				crash = true;
			}
		}

		public void RunTank()
		{
			StartCoroutine(RunTankRoutine());
		}

		public void MoveForward(float amount = 1f)
		{
			tankTasks.Enqueue(new TankEvent(TankEventType.Move, amount));
		}

		public void MoveBackward(float amount = 1f)
		{
			tankTasks.Enqueue(new TankEvent(TankEventType.Move, 0f - amount));
		}

		public void RotateClockwise(float amount = 90f)
		{
			tankTasks.Enqueue(new TankEvent(TankEventType.Rotate, amount));
		}

		public void RotateCounterClockwise(float amount = 90f)
		{
			tankTasks.Enqueue(new TankEvent(TankEventType.Rotate, 0f - amount));
		}

		public void Shoot()
		{
			tankTasks.Enqueue(new TankEvent(TankEventType.Shoot));
		}

		private IEnumerator RunTankRoutine()
		{
			TankMain();
			while (tankTasks.Count > 0)
			{
				if (crash)
				{
					Debug.Log("Crashed!");
					tankTasks.Clear();
					break;
				}
				TankEvent e = tankTasks.Dequeue();
				switch (e.eventType)
				{
				case TankEventType.Move:
					yield return StartCoroutine(MoveRoutine(e.eventValue));
					break;
				case TankEventType.Rotate:
					yield return StartCoroutine(RotateRoutine(e.eventValue));
					break;
				case TankEventType.Shoot:
					yield return StartCoroutine(ShootRoutine());
					break;
				}
			}
		}

		private IEnumerator MoveRoutine(float amount)
		{
			Vector2 destination = base.transform.position + base.transform.up * amount;
			while (Vector2.Distance(base.transform.position, destination) > 0.05f && !crash)
			{
				base.transform.position = Vector2.MoveTowards(base.transform.position, destination, Time.deltaTime * moveSpeed);
				yield return null;
			}
		}

		private IEnumerator RotateRoutine(float amount)
		{
			Quaternion target = Quaternion.Euler(0f, 0f, base.transform.eulerAngles.z - amount);
			while (base.transform.rotation != target && !crash)
			{
				float delta = Mathf.Abs(base.transform.eulerAngles.z - target.eulerAngles.z);
				if (delta < 0.2f)
				{
					break;
				}
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, target, Time.deltaTime * (rotateSpeed * 60f));
				yield return null;
			}
		}

		private IEnumerator ShootRoutine()
		{
			TankShell shell = TankShell.Shoot(startPosition: base.transform.position + base.transform.up * 0.8f, prefab: bulletObject, heading: base.transform.up);
			while (!shell.Step())
			{
				yield return null;
			}
			shell.Destroy();
		}
	}
}
