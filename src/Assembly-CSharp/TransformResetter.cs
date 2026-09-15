using UnityEngine;

public class TransformResetter : PhysicsSimulator
{
	public Vector3 startingLocalPosition;

	public Quaternion startingLocalRotation;

	protected bool wasInit;

	protected override void SyncResetSimulation()
	{
		base.SyncResetSimulation();
		Init();
		ResetTransform();
	}

	protected void Init()
	{
		if (!wasInit)
		{
			wasInit = true;
			startingLocalPosition = base.transform.localPosition;
			startingLocalRotation = base.transform.localRotation;
		}
	}

	private void Awake()
	{
		Init();
	}

	protected void ResetTransform()
	{
		base.transform.localPosition = startingLocalPosition;
		base.transform.localRotation = startingLocalRotation;
	}
}
