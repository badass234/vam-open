using UnityEngine;

public class AdjustJoints : ScaleChangeReceiverJSONStorable
{
	public bool isAppearance;

	public ConfigurableJoint joint1;

	public ConfigurableJoint joint2;

	protected JSONStorableFloat massJSON;

	[SerializeField]
	protected float _mass;

	public bool useSetCenterOfGravity;

	public Vector3 lowCenterOfGravity;

	public Vector3 highCenterOfGravity;

	public bool useJoint1COGForJoint2 = true;

	public Vector3 lowCenterOfGravityJoint2;

	public Vector3 highCenterOfGravityJoint2;

	protected Vector3 currentCenterOfGravity;

	protected Vector3 currentCenterOfGravityJoint2;

	protected JSONStorableFloat centerOfGravityPercentJSON;

	[SerializeField]
	protected float _centerOfGravityPercent;

	protected float scalePow = 1f;

	[SerializeField]
	protected float _limitSpringMultiplier;

	[SerializeField]
	protected float _limitDamperMultiplier;

	protected JSONStorableFloat springJSON;

	[SerializeField]
	protected float _spring;

	protected JSONStorableFloat damperJSON;

	[SerializeField]
	protected float _damper;

	[SerializeField]
	protected float _springDamperMultiplier = 3f;

	[SerializeField]
	protected bool _springDamperMultiplierOn;

	protected float _defaultJoint1SlerpMaxForce;

	protected float _defaultJoint1XMaxForce;

	protected float _defaultJoint1YZMaxForce;

	protected float _defaultJoint2SlerpMaxForce;

	protected float _defaultJoint2XMaxForce;

	protected float _defaultJoint2YZMaxForce;

	protected JSONStorableFloat positionSpringXJSON;

	[SerializeField]
	protected float _positionSpringX;

	protected JSONStorableFloat positionDamperXJSON;

	[SerializeField]
	protected float _positionDamperX;

	protected JSONStorableFloat positionSpringYJSON;

	[SerializeField]
	protected float _positionSpringY;

	protected JSONStorableFloat positionDamperYJSON;

	[SerializeField]
	protected float _positionDamperY;

	protected JSONStorableFloat positionSpringZJSON;

	[SerializeField]
	protected float _positionSpringZ;

	protected JSONStorableFloat positionDamperZJSON;

	[SerializeField]
	protected float _positionDamperZ;

	public bool useSmoothChanges;

	public Vector3 setJoint1TargetRotation;

	public Vector3 setJoint2TargetRotation;

	public Vector3 smoothedJoint1TargetRotation;

	public Vector3 smoothedJoint1TargetRotationVelocity;

	public Vector3 smoothedJoint2TargetRotation;

	public Vector3 smoothedJoint2TargetRotationVelocity;

	protected JSONStorableFloat smoothTargetRotationSpringJSON;

	[SerializeField]
	protected float _smoothTargetRotationSpring = 1f;

	protected JSONStorableFloat smoothTargetRotationDamperJSON;

	[SerializeField]
	protected float _smoothTargetRotationDamper = 1f;

	[SerializeField]
	protected bool _useDAZMorphForRotation;

	public float DAZMorphAngleMultiplier = 0.05f;

	protected JSONStorableFloat targetRotationXJSON;

	[SerializeField]
	protected float _targetRotationX;

	public bool invertJoint2RotationX;

	protected JSONStorableFloat targetRotationYJSON;

	[SerializeField]
	protected float _targetRotationY;

	public bool invertJoint2RotationY;

	protected JSONStorableFloat targetRotationZJSON;

	[SerializeField]
	protected float _targetRotationZ;

	public bool invertJoint2RotationZ;

	public float additionalJoint1RotationX;

	public float additionalJoint1RotationY;

	public float additionalJoint1RotationZ;

	public float additionalJoint2RotationX;

	public float additionalJoint2RotationY;

	public float additionalJoint2RotationZ;

	public AudioSourceControl audioSourceControl;

	protected JSONStorableBool driveXRotationFromAudioSourceJSON;

	[SerializeField]
	protected bool _driveXRotationFromAudioSource;

	protected JSONStorableFloat driveXRotationFromAudioSourceMultiplierJSON;

	[SerializeField]
	protected float _driveXRotationFromAudioSourceMultiplier = 4000f;

	protected JSONStorableFloat driveXRotationFromAudioSourceAdditionalAngleJSON;

	[SerializeField]
	protected float _driveXRotationFromAudioSourceAdditionalAngle;

	protected JSONStorableFloat driveXRotationFromAudioSourceMaxAngleJSON;

	[SerializeField]
	protected float _driveXRotationFromAudioSourceMaxAngle = 30f;

	public float mass
	{
		get
		{
			return _mass;
		}
		set
		{
			if (massJSON != null)
			{
				massJSON.val = value;
			}
			else if (_mass != value)
			{
				SyncMass(value);
			}
		}
	}

	public float centerOfGravityPercent
	{
		get
		{
			return _centerOfGravityPercent;
		}
		set
		{
			if (centerOfGravityPercentJSON != null)
			{
				centerOfGravityPercentJSON.val = value;
			}
			else if (_centerOfGravityPercent != value)
			{
				SyncCenterOfGravity(value);
			}
		}
	}

	public float limitSpringMultiplier
	{
		get
		{
			return _limitSpringMultiplier;
		}
		set
		{
			if (_limitSpringMultiplier != value)
			{
				_limitSpringMultiplier = value;
				SyncJoint();
			}
		}
	}

	public float limitDamperMultiplier
	{
		get
		{
			return _limitDamperMultiplier;
		}
		set
		{
			if (_limitDamperMultiplier != value)
			{
				_limitDamperMultiplier = value;
				SyncJoint();
			}
		}
	}

	public float spring
	{
		get
		{
			return _spring;
		}
		set
		{
			if (springJSON != null)
			{
				springJSON.val = value;
			}
			else if (_spring != value)
			{
				SyncSpring(value);
			}
		}
	}

	public float damper
	{
		get
		{
			return _damper;
		}
		set
		{
			if (damperJSON != null)
			{
				damperJSON.val = value;
			}
			else if (_damper != value)
			{
				SyncDamper(value);
			}
		}
	}

	public float springDamperMultiplier
	{
		get
		{
			return _springDamperMultiplier;
		}
		set
		{
			if (_springDamperMultiplier != value)
			{
				_springDamperMultiplier = value;
				SyncJoint();
			}
		}
	}

	public bool springDamperMultiplierOn
	{
		get
		{
			return _springDamperMultiplierOn;
		}
		set
		{
			if (_springDamperMultiplierOn != value)
			{
				_springDamperMultiplierOn = value;
				SyncJoint();
			}
		}
	}

	public float positionSpringX
	{
		get
		{
			return _positionSpringX;
		}
		set
		{
			if (positionSpringXJSON != null)
			{
				positionSpringXJSON.val = value;
			}
			else if (_positionSpringX != value)
			{
				SyncPositionSpringX(value);
			}
		}
	}

	public float positionDamperX
	{
		get
		{
			return _positionDamperX;
		}
		set
		{
			if (positionSpringXJSON != null)
			{
				positionSpringXJSON.val = value;
			}
			else if (_positionDamperX != value)
			{
				SyncPositionDamperX(value);
			}
		}
	}

	public float positionSpringY
	{
		get
		{
			return _positionSpringY;
		}
		set
		{
			if (positionSpringYJSON != null)
			{
				positionSpringYJSON.val = value;
			}
			else if (_positionSpringY != value)
			{
				SyncPositionSpringY(value);
			}
		}
	}

	public float positionDamperY
	{
		get
		{
			return _positionDamperY;
		}
		set
		{
			if (positionDamperYJSON != null)
			{
				positionDamperYJSON.val = value;
			}
			else if (_positionDamperY != value)
			{
				SyncPositionDamperY(value);
			}
		}
	}

	public float positionSpringZ
	{
		get
		{
			return _positionSpringZ;
		}
		set
		{
			if (positionSpringZJSON != null)
			{
				positionSpringZJSON.val = value;
			}
			else if (_positionSpringZ != value)
			{
				SyncPositionSpringZ(value);
			}
		}
	}

	public float positionDamperZ
	{
		get
		{
			return _positionDamperZ;
		}
		set
		{
			if (positionDamperZJSON != null)
			{
				positionDamperZJSON.val = value;
			}
			else if (_positionDamperZ != value)
			{
				SyncPositionDamperZ(value);
			}
		}
	}

	public float smoothTargetRotationSpring
	{
		get
		{
			return _smoothTargetRotationSpring;
		}
		set
		{
			if (smoothTargetRotationSpringJSON != null)
			{
				smoothTargetRotationSpringJSON.val = value;
			}
			else if (_smoothTargetRotationSpring != value)
			{
				SyncSmoothTargetRotationSpring(value);
			}
		}
	}

	public float smoothTargetRotationDamper
	{
		get
		{
			return _smoothTargetRotationDamper;
		}
		set
		{
			if (smoothTargetRotationDamperJSON != null)
			{
				smoothTargetRotationDamperJSON.val = value;
			}
			else if (_smoothTargetRotationDamper != value)
			{
				SyncSmoothTargetRotationDamper(value);
			}
		}
	}

	public bool useDAZMorphForRotation
	{
		get
		{
			return _useDAZMorphForRotation;
		}
		set
		{
			if (_useDAZMorphForRotation != value)
			{
				_useDAZMorphForRotation = value;
				SyncTargetRotation();
			}
		}
	}

	public float targetRotationX
	{
		get
		{
			return _targetRotationX;
		}
		set
		{
			if (targetRotationXJSON != null)
			{
				targetRotationXJSON.val = value;
			}
			else if (_targetRotationX != value)
			{
				SyncTargetRotationX(value);
			}
		}
	}

	public float targetRotationY
	{
		get
		{
			return _targetRotationY;
		}
		set
		{
			if (targetRotationYJSON != null)
			{
				targetRotationYJSON.val = value;
			}
			else if (_targetRotationY != value)
			{
				SyncTargetRotationY(value);
			}
		}
	}

	public float targetRotationZ
	{
		get
		{
			return _targetRotationZ;
		}
		set
		{
			if (targetRotationZJSON != null)
			{
				targetRotationZJSON.val = value;
			}
			else if (_targetRotationZ != value)
			{
				SyncTargetRotationZ(value);
			}
		}
	}

	public bool driveXRotationFromAudioSource
	{
		get
		{
			return _driveXRotationFromAudioSource;
		}
		set
		{
			if (driveXRotationFromAudioSourceJSON != null)
			{
				driveXRotationFromAudioSourceJSON.val = value;
			}
			else if (_driveXRotationFromAudioSource != value)
			{
				SyncDriveXRotationFromAudioSource(value);
			}
		}
	}

	public float driveXRotationFromAudioSourceMultiplier
	{
		get
		{
			return _driveXRotationFromAudioSourceMultiplier;
		}
		set
		{
			if (driveXRotationFromAudioSourceMultiplierJSON != null)
			{
				driveXRotationFromAudioSourceMultiplierJSON.val = value;
			}
			else if (_driveXRotationFromAudioSourceMultiplier != value)
			{
				SyncDriveXRotationFromAudioSourceMultiplier(value);
			}
		}
	}

	public float driveXRotationFromAudioSourceAdditionalAngle
	{
		get
		{
			return _driveXRotationFromAudioSourceAdditionalAngle;
		}
		set
		{
			if (driveXRotationFromAudioSourceAdditionalAngleJSON != null)
			{
				driveXRotationFromAudioSourceAdditionalAngleJSON.val = value;
			}
			else if (_driveXRotationFromAudioSourceAdditionalAngle != value)
			{
				SyncDriveXRotationFromAudioSourceAdditionalAngle(value);
			}
		}
	}

	public float driveXRotationFromAudioSourceMaxAngle
	{
		get
		{
			return _driveXRotationFromAudioSourceMaxAngle;
		}
		set
		{
			if (driveXRotationFromAudioSourceMaxAngleJSON != null)
			{
				driveXRotationFromAudioSourceMaxAngleJSON.val = value;
			}
			else if (_driveXRotationFromAudioSourceMaxAngle != value)
			{
				SyncDriveXRotationFromAudioSourceMaxAngle(value);
			}
		}
	}

	protected void SyncMass(float f)
	{
		_mass = f;
		if (joint1 != null)
		{
			Rigidbody component = joint1.GetComponent<Rigidbody>();
			if (component != null && component.mass != _mass)
			{
				component.mass = _mass;
				component.WakeUp();
			}
		}
		if (joint2 != null)
		{
			Rigidbody component2 = joint2.GetComponent<Rigidbody>();
			if (component2 != null && component2.mass != _mass)
			{
				component2.mass = _mass;
				component2.WakeUp();
			}
		}
	}

	protected void SyncCenterOfGravity(float f)
	{
		_centerOfGravityPercent = f;
		if (!useSetCenterOfGravity)
		{
			return;
		}
		currentCenterOfGravity = Vector3.Lerp(lowCenterOfGravity, highCenterOfGravity, _centerOfGravityPercent);
		if (joint1 != null)
		{
			Rigidbody component = joint1.GetComponent<Rigidbody>();
			if (component != null && component.centerOfMass != currentCenterOfGravity)
			{
				component.centerOfMass = currentCenterOfGravity;
				component.WakeUp();
			}
		}
		if (joint2 != null)
		{
			Rigidbody component2 = joint2.GetComponent<Rigidbody>();
			if (useJoint1COGForJoint2)
			{
				currentCenterOfGravityJoint2 = currentCenterOfGravity;
			}
			else
			{
				currentCenterOfGravityJoint2 = Vector3.Lerp(lowCenterOfGravityJoint2, highCenterOfGravityJoint2, _centerOfGravityPercent);
			}
			if (component2 != null && component2.centerOfMass != currentCenterOfGravityJoint2)
			{
				component2.centerOfMass = currentCenterOfGravityJoint2;
				component2.WakeUp();
			}
		}
	}

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		scalePow = Mathf.Pow(1.7f, _scale - 1f);
		if (base.gameObject.activeInHierarchy)
		{
			SyncJoint();
		}
	}

	protected void SyncJoint()
	{
		float num = scalePow;
		float num2 = scalePow;
		float num3 = num;
		float num4 = _spring * num;
		float num5 = _damper * num2;
		if (joint1 != null)
		{
			JointDrive slerpDrive = joint1.slerpDrive;
			if (_springDamperMultiplierOn)
			{
				slerpDrive.positionSpring = num4 * _springDamperMultiplier;
				slerpDrive.positionDamper = num5 * _springDamperMultiplier;
			}
			else
			{
				slerpDrive.positionSpring = num4;
				slerpDrive.positionDamper = num5;
			}
			slerpDrive.maximumForce = _defaultJoint1SlerpMaxForce * num3;
			joint1.slerpDrive = slerpDrive;
			slerpDrive = joint1.angularXDrive;
			slerpDrive.positionSpring = num4;
			slerpDrive.positionDamper = num5;
			slerpDrive.maximumForce = _defaultJoint1XMaxForce * num3;
			joint1.angularXDrive = slerpDrive;
			slerpDrive = joint1.angularYZDrive;
			slerpDrive.positionSpring = num4;
			slerpDrive.positionDamper = num5;
			slerpDrive.maximumForce = _defaultJoint1YZMaxForce * num3;
			joint1.angularYZDrive = slerpDrive;
			SoftJointLimitSpring angularXLimitSpring = joint1.angularXLimitSpring;
			angularXLimitSpring.spring = num4 * _limitSpringMultiplier;
			angularXLimitSpring.damper = num5 * _limitDamperMultiplier;
			joint1.angularXLimitSpring = angularXLimitSpring;
			angularXLimitSpring = joint1.angularYZLimitSpring;
			angularXLimitSpring.spring = num4 * _limitSpringMultiplier;
			angularXLimitSpring.damper = num5 * _limitDamperMultiplier;
			joint1.angularYZLimitSpring = angularXLimitSpring;
			Rigidbody component = joint1.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.WakeUp();
			}
		}
		if (joint2 != null)
		{
			JointDrive slerpDrive2 = joint2.slerpDrive;
			if (_springDamperMultiplierOn)
			{
				slerpDrive2.positionSpring = num4 * _springDamperMultiplier;
				slerpDrive2.positionDamper = num5 * _springDamperMultiplier;
			}
			else
			{
				slerpDrive2.positionSpring = num4;
				slerpDrive2.positionDamper = num5;
			}
			slerpDrive2.maximumForce = _defaultJoint2SlerpMaxForce * num3;
			joint2.slerpDrive = slerpDrive2;
			slerpDrive2 = joint2.angularXDrive;
			slerpDrive2.positionSpring = num4;
			slerpDrive2.positionDamper = num5;
			slerpDrive2.maximumForce = _defaultJoint2XMaxForce * num3;
			joint2.angularXDrive = slerpDrive2;
			slerpDrive2 = joint2.angularYZDrive;
			slerpDrive2.positionSpring = num4;
			slerpDrive2.positionDamper = num5;
			slerpDrive2.maximumForce = _defaultJoint2YZMaxForce * num3;
			joint2.angularYZDrive = slerpDrive2;
			SoftJointLimitSpring angularXLimitSpring2 = joint2.angularXLimitSpring;
			angularXLimitSpring2.spring = num4 * _limitSpringMultiplier;
			angularXLimitSpring2.damper = num5 * _limitDamperMultiplier;
			joint2.angularXLimitSpring = angularXLimitSpring2;
			angularXLimitSpring2 = joint2.angularYZLimitSpring;
			angularXLimitSpring2.spring = num4 * _limitSpringMultiplier;
			angularXLimitSpring2.damper = num5 * _limitDamperMultiplier;
			joint2.angularYZLimitSpring = angularXLimitSpring2;
			Rigidbody component2 = joint2.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				component2.WakeUp();
			}
		}
	}

	protected void SyncSpring(float f)
	{
		_spring = f;
		SyncJoint();
	}

	protected void SyncDamper(float f)
	{
		_damper = f;
		SyncJoint();
	}

	protected void SyncJointPositionXDrive()
	{
		if (joint1 != null)
		{
			JointDrive xDrive = joint1.xDrive;
			xDrive.positionSpring = _positionSpringX;
			xDrive.positionDamper = _positionDamperX;
			joint1.xDrive = xDrive;
			Rigidbody component = joint1.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.WakeUp();
			}
		}
		if (joint2 != null)
		{
			JointDrive xDrive2 = joint2.xDrive;
			xDrive2.positionSpring = _positionSpringX;
			xDrive2.positionDamper = _positionDamperX;
			joint2.xDrive = xDrive2;
			Rigidbody component2 = joint2.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				component2.WakeUp();
			}
		}
	}

	protected void SyncPositionSpringX(float f)
	{
		_positionSpringX = f;
		SyncJointPositionXDrive();
	}

	protected void SyncPositionDamperX(float f)
	{
		_positionDamperX = f;
		SyncJointPositionXDrive();
	}

	protected void SyncJointPositionYDrive()
	{
		if (joint1 != null)
		{
			JointDrive yDrive = joint1.yDrive;
			yDrive.positionSpring = _positionSpringY;
			yDrive.positionDamper = _positionDamperY;
			joint1.yDrive = yDrive;
			Rigidbody component = joint1.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.WakeUp();
			}
		}
		if (joint2 != null)
		{
			JointDrive yDrive2 = joint2.yDrive;
			yDrive2.positionSpring = _positionSpringY;
			yDrive2.positionDamper = _positionDamperY;
			joint2.yDrive = yDrive2;
			Rigidbody component2 = joint2.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				component2.WakeUp();
			}
		}
	}

	protected void SyncPositionSpringY(float f)
	{
		_positionSpringY = f;
		SyncJointPositionYDrive();
	}

	protected void SyncPositionDamperY(float f)
	{
		_positionDamperY = f;
		SyncJointPositionYDrive();
	}

	protected void SyncJointPositionZDrive()
	{
		if (joint1 != null)
		{
			JointDrive zDrive = joint1.zDrive;
			zDrive.positionSpring = _positionSpringZ;
			zDrive.positionDamper = _positionDamperZ;
			joint1.zDrive = zDrive;
			Rigidbody component = joint1.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.WakeUp();
			}
		}
		if (joint2 != null)
		{
			JointDrive zDrive2 = joint2.zDrive;
			zDrive2.positionSpring = _positionSpringZ;
			zDrive2.positionDamper = _positionDamperZ;
			joint2.zDrive = zDrive2;
			Rigidbody component2 = joint2.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				component2.WakeUp();
			}
		}
	}

	protected void SyncPositionSpringZ(float f)
	{
		_positionSpringZ = f;
		SyncJointPositionZDrive();
	}

	protected void SyncPositionDamperZ(float f)
	{
		_positionDamperZ = f;
		SyncJointPositionZDrive();
	}

	protected void SetTargetRotation()
	{
		if (joint1 != null)
		{
			DAZBone component = joint1.GetComponent<DAZBone>();
			if (component != null)
			{
				Vector3 baseJointRotation = smoothedJoint1TargetRotation;
				baseJointRotation.x = 0f - baseJointRotation.x;
				component.baseJointRotation = baseJointRotation;
			}
			else
			{
				Quaternion targetRotation = Quaternion.Euler(smoothedJoint1TargetRotation);
				joint1.targetRotation = targetRotation;
			}
		}
		if (joint2 != null)
		{
			DAZBone component2 = joint2.GetComponent<DAZBone>();
			if (component2 != null)
			{
				Vector3 baseJointRotation2 = smoothedJoint2TargetRotation;
				baseJointRotation2.x = 0f - baseJointRotation2.x;
				component2.baseJointRotation = baseJointRotation2;
			}
			else
			{
				Quaternion targetRotation2 = Quaternion.Euler(smoothedJoint2TargetRotation);
				joint2.targetRotation = targetRotation2;
			}
		}
	}

	protected void SmoothTargetRotation()
	{
		if (joint1 != null)
		{
			smoothedJoint1TargetRotation += (smoothedJoint1TargetRotationVelocity = (1f - 0.5f * smoothTargetRotationDamper) * smoothedJoint1TargetRotationVelocity + 0.5f * smoothTargetRotationSpring * (setJoint1TargetRotation - smoothedJoint1TargetRotation));
		}
		if (joint2 != null)
		{
			smoothedJoint2TargetRotation += (smoothedJoint2TargetRotationVelocity = (1f - 0.5f * smoothTargetRotationDamper) * smoothedJoint2TargetRotationVelocity + 0.5f * smoothTargetRotationSpring * (setJoint2TargetRotation - smoothedJoint2TargetRotation));
		}
		SetTargetRotation();
	}

	public void SyncTargetRotation()
	{
		if (_useDAZMorphForRotation)
		{
			SetDAZMorph component = GetComponent<SetDAZMorph>();
			if (component != null && component.enabled)
			{
				component.morphRawValue = (targetRotationX + additionalJoint1RotationX) * DAZMorphAngleMultiplier;
			}
			return;
		}
		if (joint1 != null)
		{
			setJoint1TargetRotation.x = targetRotationX + additionalJoint1RotationX;
			setJoint1TargetRotation.y = targetRotationY + additionalJoint1RotationY;
			setJoint1TargetRotation.z = targetRotationZ + additionalJoint1RotationZ;
		}
		if (joint2 != null)
		{
			if (invertJoint2RotationX)
			{
				setJoint2TargetRotation.x = 0f - targetRotationX + additionalJoint2RotationX;
			}
			else
			{
				setJoint2TargetRotation.x = targetRotationX + additionalJoint2RotationX;
			}
			if (invertJoint2RotationY)
			{
				setJoint2TargetRotation.y = 0f - targetRotationY + additionalJoint2RotationY;
			}
			else
			{
				setJoint2TargetRotation.y = targetRotationY + additionalJoint2RotationY;
			}
			if (invertJoint2RotationZ)
			{
				setJoint2TargetRotation.z = 0f - targetRotationZ + additionalJoint2RotationZ;
			}
			else
			{
				setJoint2TargetRotation.z = targetRotationZ + additionalJoint2RotationZ;
			}
		}
		if (!useSmoothChanges)
		{
			smoothedJoint1TargetRotation = setJoint1TargetRotation;
			smoothedJoint2TargetRotation = setJoint2TargetRotation;
			SetTargetRotation();
		}
	}

	protected void SyncSmoothTargetRotationSpring(float f)
	{
		_smoothTargetRotationSpring = f;
	}

	protected void SyncSmoothTargetRotationDamper(float f)
	{
		_smoothTargetRotationDamper = f;
	}

	protected void SyncTargetRotationX(float f)
	{
		_targetRotationX = f;
		SyncTargetRotation();
	}

	protected void SyncTargetRotationY(float f)
	{
		_targetRotationY = f;
		SyncTargetRotation();
	}

	protected void SyncTargetRotationZ(float f)
	{
		_targetRotationZ = f;
		SyncTargetRotation();
	}

	protected void SyncDriveXRotationFromAudioSource(bool b)
	{
		_driveXRotationFromAudioSource = b;
	}

	protected void SyncDriveXRotationFromAudioSourceMultiplier(float f)
	{
		_driveXRotationFromAudioSourceMultiplier = f;
	}

	protected void SyncDriveXRotationFromAudioSourceAdditionalAngle(float f)
	{
		_driveXRotationFromAudioSourceAdditionalAngle = f;
	}

	protected void SyncDriveXRotationFromAudioSourceMaxAngle(float f)
	{
		_driveXRotationFromAudioSourceMaxAngle = f;
	}

	public override void InitUI()
	{
		if (!(UITransform != null) || !base.isActiveAndEnabled)
		{
			return;
		}
		AdjustJointsUI componentInChildren = UITransform.GetComponentInChildren<AdjustJointsUI>(true);
		if (componentInChildren != null)
		{
			massJSON.slider = componentInChildren.massSlider;
			centerOfGravityPercentJSON.slider = componentInChildren.centerOfGravityPercentSlider;
			springJSON.slider = componentInChildren.springSlider;
			damperJSON.slider = componentInChildren.damperSlider;
			positionSpringXJSON.slider = componentInChildren.positionSpringXSlider;
			positionSpringYJSON.slider = componentInChildren.positionSpringYSlider;
			positionSpringZJSON.slider = componentInChildren.positionSpringZSlider;
			positionDamperXJSON.slider = componentInChildren.positionDamperXSlider;
			positionDamperYJSON.slider = componentInChildren.positionDamperYSlider;
			positionDamperZJSON.slider = componentInChildren.positionDamperZSlider;
			if (smoothTargetRotationSpringJSON != null)
			{
				smoothTargetRotationSpringJSON.slider = componentInChildren.smoothTargetRotationSpringSlider;
			}
			if (smoothTargetRotationDamperJSON != null)
			{
				smoothTargetRotationDamperJSON.slider = componentInChildren.smoothTargetRotationDamperSlider;
			}
			targetRotationXJSON.slider = componentInChildren.targetRotationXSlider;
			targetRotationYJSON.slider = componentInChildren.targetRotationYSlider;
			targetRotationZJSON.slider = componentInChildren.targetRotationZSlider;
			if (audioSourceControl != null)
			{
				driveXRotationFromAudioSourceJSON.toggle = componentInChildren.driveXRotationFromAudioSourceToggle;
				driveXRotationFromAudioSourceMultiplierJSON.slider = componentInChildren.driveXRotationFromAudioSourceMultiplierSlider;
				driveXRotationFromAudioSourceAdditionalAngleJSON.slider = componentInChildren.driveXRotationFromAudioSourceAdditionalAngleSlider;
				driveXRotationFromAudioSourceMaxAngleJSON.slider = componentInChildren.driveXRotationFromAudioSourceMaxAngleSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null) || !base.isActiveAndEnabled)
		{
			return;
		}
		AdjustJointsUI componentInChildren = UITransformAlt.GetComponentInChildren<AdjustJointsUI>(true);
		if (componentInChildren != null)
		{
			massJSON.sliderAlt = componentInChildren.massSlider;
			centerOfGravityPercentJSON.sliderAlt = componentInChildren.centerOfGravityPercentSlider;
			springJSON.sliderAlt = componentInChildren.springSlider;
			damperJSON.sliderAlt = componentInChildren.damperSlider;
			positionSpringXJSON.sliderAlt = componentInChildren.positionSpringXSlider;
			positionSpringYJSON.sliderAlt = componentInChildren.positionSpringYSlider;
			positionSpringZJSON.sliderAlt = componentInChildren.positionSpringZSlider;
			positionDamperXJSON.sliderAlt = componentInChildren.positionDamperXSlider;
			positionDamperYJSON.sliderAlt = componentInChildren.positionDamperYSlider;
			positionDamperZJSON.sliderAlt = componentInChildren.positionDamperZSlider;
			if (smoothTargetRotationSpringJSON != null)
			{
				smoothTargetRotationSpringJSON.sliderAlt = componentInChildren.smoothTargetRotationSpringSlider;
			}
			if (smoothTargetRotationDamperJSON != null)
			{
				smoothTargetRotationDamperJSON.sliderAlt = componentInChildren.smoothTargetRotationDamperSlider;
			}
			targetRotationXJSON.sliderAlt = componentInChildren.targetRotationXSlider;
			targetRotationYJSON.sliderAlt = componentInChildren.targetRotationYSlider;
			targetRotationZJSON.sliderAlt = componentInChildren.targetRotationZSlider;
			if (audioSourceControl != null)
			{
				driveXRotationFromAudioSourceJSON.toggleAlt = componentInChildren.driveXRotationFromAudioSourceToggle;
				driveXRotationFromAudioSourceMultiplierJSON.sliderAlt = componentInChildren.driveXRotationFromAudioSourceMultiplierSlider;
				driveXRotationFromAudioSourceAdditionalAngleJSON.sliderAlt = componentInChildren.driveXRotationFromAudioSourceAdditionalAngleSlider;
				driveXRotationFromAudioSourceMaxAngleJSON.sliderAlt = componentInChildren.driveXRotationFromAudioSourceMaxAngleSlider;
			}
		}
	}

	protected void DeregisterUI()
	{
		massJSON.slider = null;
		centerOfGravityPercentJSON.slider = null;
		springJSON.slider = null;
		damperJSON.slider = null;
		positionSpringXJSON.slider = null;
		positionSpringYJSON.slider = null;
		positionSpringZJSON.slider = null;
		positionDamperXJSON.slider = null;
		positionDamperYJSON.slider = null;
		positionDamperZJSON.slider = null;
		if (smoothTargetRotationSpringJSON != null)
		{
			smoothTargetRotationSpringJSON.slider = null;
		}
		if (smoothTargetRotationDamperJSON != null)
		{
			smoothTargetRotationDamperJSON.slider = null;
		}
		targetRotationXJSON.slider = null;
		targetRotationYJSON.slider = null;
		targetRotationZJSON.slider = null;
		if (audioSourceControl != null)
		{
			driveXRotationFromAudioSourceJSON.toggle = null;
			driveXRotationFromAudioSourceMultiplierJSON.slider = null;
			driveXRotationFromAudioSourceAdditionalAngleJSON.slider = null;
			driveXRotationFromAudioSourceMaxAngleJSON.slider = null;
		}
	}

	protected void SyncAll()
	{
		SyncMass(_mass);
		SyncCenterOfGravity(_centerOfGravityPercent);
		SyncJoint();
		SyncTargetRotation();
		SyncJointPositionXDrive();
		SyncJointPositionYDrive();
		SyncJointPositionZDrive();
	}

	protected void Init()
	{
		massJSON = new JSONStorableFloat("mass", _mass, SyncMass, 0.1f, 2f);
		RegisterFloat(massJSON);
		centerOfGravityPercentJSON = new JSONStorableFloat("centerOfGravityPercent", _centerOfGravityPercent, SyncCenterOfGravity, 0f, 1f);
		RegisterFloat(centerOfGravityPercentJSON);
		springJSON = new JSONStorableFloat("spring", _spring, SyncSpring, 0f, 100f);
		RegisterFloat(springJSON);
		damperJSON = new JSONStorableFloat("damper", _damper, SyncDamper, 0f, 5f);
		RegisterFloat(damperJSON);
		positionSpringXJSON = new JSONStorableFloat("positionSpringX", _positionSpringX, SyncPositionSpringX, 0f, 1000f);
		RegisterFloat(positionSpringXJSON);
		positionSpringYJSON = new JSONStorableFloat("positionSpringY", _positionSpringY, SyncPositionSpringY, 0f, 1000f);
		RegisterFloat(positionSpringYJSON);
		positionSpringZJSON = new JSONStorableFloat("positionSpringZ", _positionSpringZ, SyncPositionSpringZ, 0f, 1000f);
		RegisterFloat(positionSpringZJSON);
		positionDamperXJSON = new JSONStorableFloat("positionDamperX", _positionDamperX, SyncPositionDamperX, 0f, 1000f);
		RegisterFloat(positionDamperXJSON);
		positionDamperYJSON = new JSONStorableFloat("positionDamperY", _positionDamperY, SyncPositionDamperY, 0f, 1000f);
		RegisterFloat(positionDamperYJSON);
		positionDamperZJSON = new JSONStorableFloat("positionDamperZ", _positionDamperZ, SyncPositionDamperZ, 0f, 1000f);
		RegisterFloat(positionDamperZJSON);
		if (useSmoothChanges)
		{
			smoothTargetRotationSpringJSON = new JSONStorableFloat("smoothTargetRotationSpring", _smoothTargetRotationSpring, SyncSmoothTargetRotationSpring, 0.1f, 1f, false);
			RegisterFloat(smoothTargetRotationSpringJSON);
			smoothTargetRotationDamperJSON = new JSONStorableFloat("smoothTargetRotationDamper", _smoothTargetRotationDamper, SyncSmoothTargetRotationDamper, 0.1f, 1f, false);
			RegisterFloat(smoothTargetRotationDamperJSON);
		}
		targetRotationXJSON = new JSONStorableFloat("targetRotationX", _targetRotationX, SyncTargetRotationX, -20f, 20f);
		RegisterFloat(targetRotationXJSON);
		targetRotationYJSON = new JSONStorableFloat("targetRotationY", _targetRotationY, SyncTargetRotationY, -20f, 20f);
		RegisterFloat(targetRotationYJSON);
		targetRotationZJSON = new JSONStorableFloat("targetRotationZ", _targetRotationZ, SyncTargetRotationZ, -20f, 20f);
		RegisterFloat(targetRotationZJSON);
		if (audioSourceControl != null)
		{
			driveXRotationFromAudioSourceJSON = new JSONStorableBool("driveXRotationFromAudioSource", _driveXRotationFromAudioSource, SyncDriveXRotationFromAudioSource);
			RegisterBool(driveXRotationFromAudioSourceJSON);
			driveXRotationFromAudioSourceMultiplierJSON = new JSONStorableFloat("driveXRotationFromAudioSourceMultiplier", _driveXRotationFromAudioSourceMultiplier, SyncDriveXRotationFromAudioSourceMultiplier, 0f, 1000f, false);
			RegisterFloat(driveXRotationFromAudioSourceMultiplierJSON);
			driveXRotationFromAudioSourceAdditionalAngleJSON = new JSONStorableFloat("driveXRotationFromAudioSourceAdditionalAngle", _driveXRotationFromAudioSourceAdditionalAngle, SyncDriveXRotationFromAudioSourceAdditionalAngle, -35f, 0f);
			RegisterFloat(driveXRotationFromAudioSourceAdditionalAngleJSON);
			driveXRotationFromAudioSourceMaxAngleJSON = new JSONStorableFloat("driveXRotationFromAudioSourceMaxAngle", _driveXRotationFromAudioSourceMaxAngle, SyncDriveXRotationFromAudioSourceMaxAngle, -35f, 0f);
			RegisterFloat(driveXRotationFromAudioSourceMaxAngleJSON);
		}
		if (!isAppearance)
		{
			massJSON.storeType = JSONStorableParam.StoreType.Physical;
			centerOfGravityPercentJSON.storeType = JSONStorableParam.StoreType.Physical;
			springJSON.storeType = JSONStorableParam.StoreType.Physical;
			damperJSON.storeType = JSONStorableParam.StoreType.Physical;
			positionSpringXJSON.storeType = JSONStorableParam.StoreType.Physical;
			positionSpringYJSON.storeType = JSONStorableParam.StoreType.Physical;
			positionSpringZJSON.storeType = JSONStorableParam.StoreType.Physical;
			positionDamperXJSON.storeType = JSONStorableParam.StoreType.Physical;
			positionDamperYJSON.storeType = JSONStorableParam.StoreType.Physical;
			positionDamperZJSON.storeType = JSONStorableParam.StoreType.Physical;
			if (useSmoothChanges)
			{
				smoothTargetRotationSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
				smoothTargetRotationDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
			}
			targetRotationXJSON.storeType = JSONStorableParam.StoreType.Physical;
			targetRotationYJSON.storeType = JSONStorableParam.StoreType.Physical;
			targetRotationZJSON.storeType = JSONStorableParam.StoreType.Physical;
			if (audioSourceControl != null)
			{
				driveXRotationFromAudioSourceJSON.storeType = JSONStorableParam.StoreType.Physical;
				driveXRotationFromAudioSourceMultiplierJSON.storeType = JSONStorableParam.StoreType.Physical;
				driveXRotationFromAudioSourceAdditionalAngleJSON.storeType = JSONStorableParam.StoreType.Physical;
				driveXRotationFromAudioSourceMaxAngleJSON.storeType = JSONStorableParam.StoreType.Physical;
			}
		}
		if (joint1 != null)
		{
			_defaultJoint1SlerpMaxForce = joint1.slerpDrive.maximumForce;
			_defaultJoint1XMaxForce = joint1.angularXDrive.maximumForce;
			_defaultJoint1YZMaxForce = joint1.angularYZDrive.maximumForce;
		}
		if (joint2 != null)
		{
			_defaultJoint2SlerpMaxForce = joint2.slerpDrive.maximumForce;
			_defaultJoint2XMaxForce = joint2.angularXDrive.maximumForce;
			_defaultJoint2YZMaxForce = joint2.angularYZDrive.maximumForce;
		}
	}

	private void Update()
	{
		if (driveXRotationFromAudioSource && audioSourceControl != null)
		{
			if (_driveXRotationFromAudioSourceMaxAngle < 0f)
			{
				targetRotationX = Mathf.Clamp(audioSourceControl.smoothedClipLoudness * (0f - driveXRotationFromAudioSourceMultiplier) + _driveXRotationFromAudioSourceAdditionalAngle, _driveXRotationFromAudioSourceMaxAngle, 0f);
			}
			else
			{
				targetRotationX = Mathf.Clamp(audioSourceControl.smoothedClipLoudness * (0f - driveXRotationFromAudioSourceMultiplier) + _driveXRotationFromAudioSourceAdditionalAngle, 0f, _driveXRotationFromAudioSourceMaxAngle);
			}
		}
		if (useSmoothChanges)
		{
			SmoothTargetRotation();
		}
	}

	private void OnEnable()
	{
		InitUI();
		InitUIAlt();
		SyncAll();
	}

	private void OnDisable()
	{
		DeregisterUI();
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
		}
	}
}
