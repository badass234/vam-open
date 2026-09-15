using UnityEngine;

public class ConfigurableJointScaler : ScaleChangeReceiver
{
	protected float scalePow = 1f;

	private ConfigurableJoint joint;

	private float startingDriveSpring;

	private float startingDriveDamper;

	private float startingMaxForce;

	private float startingDriveSpringAlt;

	private float startingDriveDamperAlt;

	private float startingMaxForceAlt;

	protected bool _wasInit;

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		scalePow = Mathf.Pow(1.7f, scale - 1f);
		InitJoint();
		SyncJoint();
	}

	protected void SyncJoint()
	{
		if (joint != null)
		{
			if (joint.rotationDriveMode == RotationDriveMode.Slerp)
			{
				JointDrive slerpDrive = joint.slerpDrive;
				slerpDrive.positionSpring = startingDriveSpring * scalePow;
				slerpDrive.positionDamper = startingDriveDamper * scalePow;
				slerpDrive.maximumForce = startingMaxForce * scalePow;
				joint.slerpDrive = slerpDrive;
				return;
			}
			JointDrive angularXDrive = joint.angularXDrive;
			angularXDrive.positionSpring = startingDriveSpring * scalePow;
			angularXDrive.positionDamper = startingDriveDamper * scalePow;
			angularXDrive.maximumForce = startingMaxForce * scalePow;
			joint.angularXDrive = angularXDrive;
			JointDrive angularYZDrive = joint.angularYZDrive;
			angularYZDrive.positionSpring = startingDriveSpring * scalePow;
			angularYZDrive.positionDamper = startingDriveDamper * scalePow;
			angularYZDrive.maximumForce = startingMaxForce * scalePow;
			joint.angularYZDrive = angularYZDrive;
		}
	}

	protected void InitJoint()
	{
		if (_wasInit)
		{
			return;
		}
		_wasInit = true;
		joint = GetComponent<ConfigurableJoint>();
		if (joint != null)
		{
			if (joint.rotationDriveMode == RotationDriveMode.Slerp)
			{
				JointDrive slerpDrive = joint.slerpDrive;
				startingDriveSpring = slerpDrive.positionSpring / scalePow;
				startingDriveDamper = slerpDrive.positionDamper / scalePow;
				startingMaxForce = slerpDrive.maximumForce / scalePow;
				return;
			}
			JointDrive angularXDrive = joint.angularXDrive;
			startingDriveSpring = angularXDrive.positionSpring / scalePow;
			startingDriveDamper = angularXDrive.positionDamper / scalePow;
			startingMaxForce = angularXDrive.maximumForce / scalePow;
			JointDrive angularYZDrive = joint.angularYZDrive;
			startingDriveSpring = angularYZDrive.positionSpring / scalePow;
			startingDriveDamper = angularYZDrive.positionDamper / scalePow;
			startingMaxForce = angularYZDrive.maximumForce / scalePow;
		}
	}

	private void Awake()
	{
		InitJoint();
	}
}
