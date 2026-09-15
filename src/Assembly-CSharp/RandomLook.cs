using System;
using System.Collections;
using UnityEngine;

public class RandomLook : MonoBehaviour
{
	public Transform[] eyeballJoints;

	public SkinnedMeshRenderer headMesh;

	public string blinkBlendShapeName;

	[Tooltip("Eyes look target. Leave blank for random")]
	public Transform lookTarget;

	public bool randomLook = true;

	public bool eyesTrackTarget;

	public bool randomBlink = true;

	[Header("\tLook Settings")]
	[Tooltip("Average time between changing random look direction (s)")]
	[Range(0.5f, 10f)]
	public float changeLookPeriod = 3f;

	[Tooltip("Maximum random look direction angle (degrees, < 90)")]
	[Range(0f, 70f)]
	public float randomLookMaxAngle = 20f;

	[Tooltip("Cutoff angle for eye tracking (degrees, < 90)")]
	[Range(0f, 70f)]
	public float eyeTrackMaxAngle = 10f;

	[Header("\tBlink Settings")]
	[Tooltip("Average blink time (s)")]
	[Range(0.5f, 10f)]
	public float blinkPeriod = 3f;

	[Tooltip("Blink speed")]
	[Range(0f, 1f)]
	public float blinkSpeed = 0.7f;

	[Tooltip("Blink hold closed time")]
	[Range(0f, 0.2f)]
	public float blinkHoldTime = 0.03f;

	[Tooltip("Blink closed amount")]
	[Range(0f, 1f)]
	public float blinkClosedLimit = 1f;

	[Tooltip("Blink open amount")]
	[Range(0f, 1f)]
	public float blinkOpenLimit;

	public Vector3 lookAngleOffset = Vector3.zero;

	public Vector3 eyesForwardVector = Vector3.forward;

	private int blinkBSindex = -1;

	private float lastBlinkTime;

	private float lastLookChangeTime;

	private float blinkPeriodRandom;

	private float lookChangePeriodRandom;

	public bool blinking;

	private Quaternion[] EyeballStartDirection = new Quaternion[2];

	private GameObject RandomLookTarget;

	private GameObject EyesCenterObject;

	private Transform randomLookTarget;

	private Mesh m;

	private void Start()
	{
		if (eyeballJoints.Length == 0)
		{
			randomLook = false;
			eyesTrackTarget = false;
		}
		else if ((eyeballJoints[0] == null) | (eyeballJoints[1] == null))
		{
			randomLook = false;
			eyesTrackTarget = false;
		}
		else
		{
			StartCoroutine(GetStartAngles());
			if (!randomLook & (lookTarget != null))
			{
				eyesTrackTarget = true;
			}
			else if (randomLook)
			{
				InitializeRandomLook();
			}
			else
			{
				eyesTrackTarget = false;
			}
		}
		if (!string.IsNullOrEmpty(blinkBlendShapeName))
		{
			m = headMesh.sharedMesh;
			for (int i = 0; i < m.blendShapeCount; i++)
			{
				if (m.GetBlendShapeName(i) == blinkBlendShapeName)
				{
					blinkBSindex = i;
					break;
				}
			}
			if (blinkBSindex == -1)
			{
				MonoBehaviour.print("Warning: blink blendshape " + blinkBlendShapeName + " not found");
				randomBlink = false;
			}
		}
		lastBlinkTime = 0f;
		blinkPeriodRandom = (UnityEngine.Random.Range(0f, 10f) - 5f) / 5f;
		if (blinkClosedLimit < blinkOpenLimit)
		{
			blinkClosedLimit = blinkOpenLimit;
		}
	}

	private void FixedUpdate()
	{
		if (randomBlink & (Time.time - (lastBlinkTime + blinkPeriodRandom) > blinkPeriod))
		{
			StartBlink();
			lastBlinkTime = Time.time;
			blinkPeriodRandom = (UnityEngine.Random.Range(0f, 10f) - 5f) / 5f;
		}
		if (randomLook & (Time.time - (lastLookChangeTime + lookChangePeriodRandom) > changeLookPeriod))
		{
			StartRandomLook();
			lastLookChangeTime = Time.time;
			lookChangePeriodRandom = (UnityEngine.Random.Range(0f, 10f) - 5f) / 5f;
		}
		else if (eyesTrackTarget)
		{
			eyeballJoints[0].LookAt(lookTarget);
			eyeballJoints[0].Rotate(lookAngleOffset);
			eyeballJoints[1].localRotation = eyeballJoints[0].localRotation;
			float num = Quaternion.Angle(eyeballJoints[0].localRotation * Quaternion.Euler(lookAngleOffset), EyeballStartDirection[0]);
			if (num > eyeTrackMaxAngle)
			{
				eyeballJoints[0].localRotation = Quaternion.Lerp(EyeballStartDirection[0], eyeballJoints[0].localRotation, eyeTrackMaxAngle / num);
				eyeballJoints[1].localRotation = eyeballJoints[0].localRotation;
			}
		}
	}

	public void StartRandomLook()
	{
		if (randomLookTarget == null)
		{
			InitializeRandomLook();
		}
		float num = 1f;
		float num2 = UnityEngine.Random.Range(0f - randomLookMaxAngle, randomLookMaxAngle) * (float)Math.PI / 180f;
		float num3 = UnityEngine.Random.Range(0f - randomLookMaxAngle, randomLookMaxAngle) * (float)Math.PI / 180f;
		randomLookTarget.localPosition = new Vector3(num / Mathf.Tan((float)Math.PI / 2f - num2), num / Mathf.Tan((float)Math.PI / 2f - num3), num);
		eyeballJoints[0].LookAt(randomLookTarget);
		eyeballJoints[0].Rotate(lookAngleOffset);
		eyeballJoints[1].localRotation = eyeballJoints[0].localRotation;
	}

	private void InitializeRandomLook()
	{
		eyesTrackTarget = false;
		EyesCenterObject = new GameObject("EyesCenter");
		EyesCenterObject.transform.position = (eyeballJoints[0].position + eyeballJoints[1].position) / 2f;
		EyesCenterObject.transform.forward = eyesForwardVector;
		EyesCenterObject.transform.parent = eyeballJoints[0].parent;
		RandomLookTarget = new GameObject("RandomLookTarget");
		randomLookTarget = RandomLookTarget.transform;
		randomLookTarget.SetParent(EyesCenterObject.transform, true);
	}

	public void StartBlink()
	{
		if (!blinking)
		{
			StartCoroutine(BlinkBlend());
		}
	}

	private IEnumerator BlinkBlend()
	{
		blinking = true;
		float dt = Time.deltaTime;
		int steps = Mathf.RoundToInt(0.1f / (blinkSpeed * 3f + 0.01f) / dt * (blinkClosedLimit - blinkOpenLimit));
		headMesh.SetBlendShapeWeight(blinkBSindex, 0f);
		for (int i = 0; i < steps; i++)
		{
			float amount = (float)i / (float)steps * 100f;
			headMesh.SetBlendShapeWeight(blinkBSindex, amount);
			if (i % 2 == 0)
			{
				yield return new WaitForSeconds(dt);
			}
		}
		headMesh.SetBlendShapeWeight(blinkBSindex, blinkClosedLimit * 100f);
		yield return new WaitForSeconds(blinkHoldTime);
		for (int j = 0; j < steps; j++)
		{
			float amount2 = (float)j / (float)steps * 100f;
			headMesh.SetBlendShapeWeight(blinkBSindex, blinkClosedLimit * 100f - amount2);
			if (j % 2 == 0)
			{
				yield return new WaitForSeconds(dt);
			}
		}
		headMesh.SetBlendShapeWeight(blinkBSindex, 0f);
		blinking = false;
	}

	private IEnumerator GetStartAngles()
	{
		yield return new WaitForSeconds(0.3f);
		EyeballStartDirection[0] = eyeballJoints[0].localRotation;
		EyeballStartDirection[1] = eyeballJoints[1].localRotation;
	}
}
