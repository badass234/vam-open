using UnityEngine;

public class ScaleChangeReceiverJoint : ScaleChangeReceiver
{
	protected ConfigurableJoint CJ;

	protected float scalePow = 1f;

	protected bool _defaultsSet;

	protected float _defaultSpring;

	protected float _defaultDamper;

	protected float _defaultMaxForce;

	protected float _defaultXSpring;

	protected float _defaultXDamper;

	protected float _defaultXMaxForce;

	protected float _defaultYZSpring;

	protected float _defaultYZDamper;

	protected float _defaultYZMaxForce;

	protected float _defaultLinearLimit;

	protected Vector3 _defaultTargetPosition;

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		scalePow = Mathf.Pow(1.7f, _scale - 1f);
		Adjust();
	}

	protected void Adjust()
	{
		if (CJ == null)
		{
			CJ = GetComponent<ConfigurableJoint>();
		}
		if (CJ != null)
		{
			float num = scalePow;
			float num2 = scalePow;
			float num3 = num;
			JointDrive slerpDrive = CJ.slerpDrive;
			if (!_defaultsSet)
			{
				_defaultSpring = slerpDrive.positionSpring;
				_defaultDamper = slerpDrive.positionDamper;
				_defaultMaxForce = slerpDrive.maximumForce;
			}
			slerpDrive.positionSpring = _defaultSpring * num;
			slerpDrive.positionDamper = _defaultDamper * num2;
			slerpDrive.maximumForce = _defaultMaxForce * num3;
			CJ.slerpDrive = slerpDrive;
			slerpDrive = CJ.angularXDrive;
			if (!_defaultsSet)
			{
				_defaultXSpring = slerpDrive.positionSpring;
				_defaultXDamper = slerpDrive.positionDamper;
				_defaultXMaxForce = slerpDrive.maximumForce;
			}
			slerpDrive.positionSpring = _defaultXSpring * num;
			slerpDrive.positionDamper = _defaultXDamper * num2;
			slerpDrive.maximumForce = _defaultXMaxForce * num3;
			CJ.angularXDrive = slerpDrive;
			slerpDrive = CJ.angularYZDrive;
			if (!_defaultsSet)
			{
				_defaultYZSpring = slerpDrive.positionSpring;
				_defaultYZDamper = slerpDrive.positionDamper;
				_defaultYZMaxForce = slerpDrive.maximumForce;
			}
			slerpDrive.positionSpring = _defaultYZSpring * num;
			slerpDrive.positionDamper = _defaultYZDamper * num2;
			slerpDrive.maximumForce = _defaultYZMaxForce * num3;
			CJ.angularYZDrive = slerpDrive;
			SoftJointLimit linearLimit = CJ.linearLimit;
			if (!_defaultsSet)
			{
				_defaultLinearLimit = linearLimit.limit;
			}
			linearLimit.limit = _defaultLinearLimit * _scale;
			CJ.linearLimit = linearLimit;
			Vector3 targetPosition = CJ.targetPosition;
			if (!_defaultsSet)
			{
				_defaultTargetPosition = targetPosition;
			}
			CJ.targetPosition = _defaultTargetPosition * _scale;
			_defaultsSet = true;
			Rigidbody component = GetComponent<Rigidbody>();
			component.WakeUp();
		}
	}
}
