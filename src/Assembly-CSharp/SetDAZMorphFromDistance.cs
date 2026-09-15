using UnityEngine;

public class SetDAZMorphFromDistance : SetDAZMorph
{
	public bool updateEnabled = true;

	public Transform transform1;

	public Transform transform2;

	public float distanceLow;

	public float distanceHigh = 0.1f;

	public float currentDistance;

	public bool lerpOverTime;

	public float lerpRate = 10f;

	public void DoUpdate()
	{
		if (!isOn || !(transform1 != null) || !(transform2 != null) || morph1 == null)
		{
			return;
		}
		float f = (transform1.position - transform2.position).magnitude / base.transform.lossyScale.x;
		if (float.IsNaN(f))
		{
			Debug.LogError("Detected NaN value during distance calculation for SetDAZMorphFromDistance " + base.name);
			return;
		}
		currentDistance = f;
		float num = Mathf.Clamp(currentDistance, distanceLow, distanceHigh) - distanceLow;
		float t = num / (distanceHigh - distanceLow);
		float b = Mathf.Lerp(morph1Low, morph1High, t);
		if (lerpOverTime)
		{
			currentMorph1Value = Mathf.Lerp(currentMorph1Value, b, lerpRate * Time.deltaTime);
		}
		else
		{
			currentMorph1Value = b;
		}
		morph1.SetValueThreadSafe(currentMorph1Value);
	}

	protected void SyncMorphJSON()
	{
		if (morph1 != null)
		{
			morph1.SyncJSON();
		}
	}

	private void Update()
	{
		if (updateEnabled)
		{
			DoUpdate();
			SyncMorphJSON();
		}
		else
		{
			SyncMorphJSON();
		}
	}
}
