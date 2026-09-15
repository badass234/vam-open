using UnityEngine;

public class AdjustJointXDrive : JSONStorable
{
	public ConfigurableJoint joint;

	public JSONStorableAngle driveAngleJSON;

	public JSONStorableFloat autoDriveSpeedJSON;

	protected void SyncDriveAngle(float f)
	{
		if (joint != null)
		{
			Vector3 euler = default(Vector3);
			euler.x = f;
			euler.y = 0f;
			euler.z = 0f;
			Quaternion targetRotation = Quaternion.Euler(euler);
			joint.targetRotation = targetRotation;
		}
	}

	protected void SyncAutoDriveSpeed(float f)
	{
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			AdjustJointXDriveUI componentInChildren = UITransform.GetComponentInChildren<AdjustJointXDriveUI>(true);
			if (componentInChildren != null)
			{
				driveAngleJSON.slider = componentInChildren.driveAngleSlider;
				autoDriveSpeedJSON.slider = componentInChildren.autoDriveSpeedSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			AdjustJointXDriveUI componentInChildren = UITransformAlt.GetComponentInChildren<AdjustJointXDriveUI>(true);
			if (componentInChildren != null)
			{
				driveAngleJSON.sliderAlt = componentInChildren.driveAngleSlider;
				autoDriveSpeedJSON.sliderAlt = componentInChildren.autoDriveSpeedSlider;
			}
		}
	}

	protected void Init()
	{
		driveAngleJSON = new JSONStorableAngle("driveAngle", 0f, SyncDriveAngle);
		RegisterFloat(driveAngleJSON);
		autoDriveSpeedJSON = new JSONStorableFloat("autoDriveSpeed", 0f, SyncAutoDriveSpeed, -10f, 10f, false);
		RegisterFloat(autoDriveSpeedJSON);
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}

	private void Update()
	{
		if (autoDriveSpeedJSON != null && autoDriveSpeedJSON.val != 0f)
		{
			float val = driveAngleJSON.val + autoDriveSpeedJSON.val * Time.deltaTime * 90f;
			driveAngleJSON.val = val;
		}
	}
}
