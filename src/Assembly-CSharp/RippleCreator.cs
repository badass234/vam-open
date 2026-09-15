using System.Collections.Generic;
using UnityEngine;

public class RippleCreator : MonoBehaviour
{
	private class ReversedRipple
	{
		public Vector3 Position;

		public float Velocity;
	}

	public bool IsReversedRipple;

	public float RippleStrenght = 0.1f;

	public float MaxVelocity = 1.5f;

	public float RandomRipplesInterval;

	public float reversedRippleDelay = 0.2f;

	public GameObject SplashEffect;

	public GameObject SplashEffectMoved;

	public AudioSource SplashAudioSource;

	private int fadeInVelocityLimit = 10;

	private int fadeInVelocity = 1;

	private WaterRipples waterRipples;

	private Vector3 oldPos;

	private float currentVelocity;

	private Transform t;

	private Queue<ReversedRipple> reversedVelocityQueue;

	private float triggeredTime;

	private bool canUpdate;

	private float randomRipplesCurrentTime;

	private bool canInstantiateRandomRipple;

	private GameObject splashMovedInstance;

	private ParticleSystem splashParticleSystem;

	public float splashSizeMultiplier = 2f;

	private void Start()
	{
		t = base.transform;
		reversedVelocityQueue = new Queue<ReversedRipple>();
	}

	private void FixedUpdate()
	{
		if (waterRipples == null)
		{
			return;
		}
		if (RandomRipplesInterval > 0.0001f && Time.time - randomRipplesCurrentTime > RandomRipplesInterval)
		{
			randomRipplesCurrentTime = Time.time;
			canInstantiateRandomRipple = true;
		}
		if (canUpdate)
		{
			currentVelocity = (t.position - oldPos).magnitude / Time.fixedDeltaTime * RippleStrenght;
			if (currentVelocity > MaxVelocity)
			{
				currentVelocity = MaxVelocity;
			}
			if (IsReversedRipple)
			{
				currentVelocity = 0f - currentVelocity;
			}
			reversedVelocityQueue.Enqueue(new ReversedRipple
			{
				Position = t.position,
				Velocity = (0f - currentVelocity) / (float)fadeInVelocity
			});
			oldPos = t.position;
			waterRipples.CreateRippleByPosition(t.position, currentVelocity / (float)fadeInVelocity);
			if (canInstantiateRandomRipple)
			{
				waterRipples.CreateRippleByPosition(t.position, Random.Range(currentVelocity / 5f, currentVelocity));
			}
			UpdateMovedSplash();
		}
		if (Time.time - triggeredTime > reversedRippleDelay)
		{
			ReversedRipple reversedRipple = reversedVelocityQueue.Dequeue();
			if (IsReversedRipple)
			{
				reversedRipple.Velocity = 0f - reversedRipple.Velocity;
			}
			waterRipples.CreateRippleByPosition(reversedRipple.Position, reversedRipple.Velocity);
			if (canInstantiateRandomRipple)
			{
				waterRipples.CreateRippleByPosition(reversedRipple.Position, Random.Range(reversedRipple.Velocity / 5f, reversedRipple.Velocity));
			}
		}
		fadeInVelocity++;
		if (fadeInVelocity > fadeInVelocityLimit)
		{
			fadeInVelocity = 1;
		}
		if (canInstantiateRandomRipple)
		{
			canInstantiateRandomRipple = false;
		}
	}

	private void OnTriggerEnter(Collider collidedObj)
	{
		WaterRipples component = collidedObj.GetComponent<WaterRipples>();
		if (component != null)
		{
			waterRipples = component;
			canUpdate = true;
			reversedVelocityQueue.Clear();
			triggeredTime = Time.time;
			fadeInVelocity = 1;
			if (SplashAudioSource != null)
			{
				SplashAudioSource.Play();
			}
			if (SplashEffect != null)
			{
				Vector3 offsetByPosition = waterRipples.GetOffsetByPosition(t.position);
				offsetByPosition.x = t.position.x;
				offsetByPosition.z = t.position.z;
				GameObject obj = Object.Instantiate(SplashEffect, offsetByPosition, default(Quaternion));
				Object.Destroy(obj, 2f);
			}
			UpdateMovedSplash();
		}
	}

	private void UpdateMovedSplash()
	{
		if (splashMovedInstance != null)
		{
			Vector3 offsetByPosition = waterRipples.GetOffsetByPosition(t.position);
			offsetByPosition.x = t.position.x;
			offsetByPosition.z = t.position.z;
			splashMovedInstance.transform.position = offsetByPosition;
			ParticleSystem.MainModule main = splashParticleSystem.main;
			main.startSize = currentVelocity * splashSizeMultiplier;
		}
		else if (SplashEffectMoved != null)
		{
			splashMovedInstance = Object.Instantiate(SplashEffectMoved, t.position, default(Quaternion));
			splashMovedInstance.transform.parent = waterRipples.transform;
			Vector3 offsetByPosition2 = waterRipples.GetOffsetByPosition(t.position);
			offsetByPosition2.x = t.position.x;
			offsetByPosition2.z = t.position.z;
			splashMovedInstance.transform.position = offsetByPosition2;
			splashParticleSystem = splashMovedInstance.GetComponentInChildren<ParticleSystem>();
			ParticleSystem.MainModule main2 = splashParticleSystem.main;
			main2.startSize = currentVelocity * splashSizeMultiplier;
		}
	}

	private void OnEnable()
	{
		waterRipples = null;
		canUpdate = false;
		if (splashMovedInstance != null)
		{
			Object.Destroy(splashMovedInstance);
		}
	}

	private void OnDisable()
	{
		if (splashMovedInstance != null)
		{
			Object.Destroy(splashMovedInstance);
		}
	}

	private void OnDestroy()
	{
		if (splashMovedInstance != null)
		{
			Object.Destroy(splashMovedInstance);
		}
	}
}
