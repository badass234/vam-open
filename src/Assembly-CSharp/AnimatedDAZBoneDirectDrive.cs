using UnityEngine;

public class AnimatedDAZBoneDirectDrive : MonoBehaviour
{
	public DAZBone dazBone;

	public Quaternion2Angles.RotationOrder outputRotationOrder;

	public Vector3 multiplier = Vector3.one;

	public Vector3 localRotationEuler;

	public Vector3 applyRotation;

	protected Transform parentOrientationTransform;

	protected ConfigurableJoint cj;

	protected Vector3 jointAxis;

	protected Vector3 jointSecondaryAxis;

	protected bool dazBoneJointRotationDisabled;

	protected Quaternion orientationTransformRotation = Quaternion.identity;

	protected Quaternion parentOrientationTransformRotation = Quaternion.identity;

	protected Quaternion jointTargetRotation;

	public Transform orientationTransform { get; protected set; }

	public void Init()
	{
		if (dazBone != null)
		{
			cj = dazBone.GetComponent<ConfigurableJoint>();
			if (cj != null)
			{
				jointAxis = cj.axis;
				jointSecondaryAxis = cj.secondaryAxis;
			}
			GameObject gameObject = new GameObject(base.name + "_orientation");
			orientationTransform = gameObject.transform;
			orientationTransform.SetParent(base.transform);
			orientationTransform.localPosition = Vector3.zero;
			dazBone.Init();
			Quaternion rotation = base.transform.rotation;
			base.transform.rotation = Quaternion.identity;
			orientationTransform.rotation = dazBone.startingRotationRelativeToRoot;
			base.transform.rotation = rotation;
		}
	}

	public void InitParent()
	{
		AnimatedDAZBoneDirectDrive component = base.transform.parent.GetComponent<AnimatedDAZBoneDirectDrive>();
		if (component != null)
		{
			parentOrientationTransform = component.orientationTransform;
		}
		else
		{
			AnimatedDAZBoneMoveProducer component2 = base.transform.parent.GetComponent<AnimatedDAZBoneMoveProducer>();
			if (component2 != null)
			{
				parentOrientationTransform = component2.orientationTransform;
			}
		}
		if (parentOrientationTransform == null)
		{
			Debug.LogError("Could not find parent orientation transform for " + base.name);
			parentOrientationTransform = base.transform.parent;
		}
	}

	public void RestoreBoneControl()
	{
		if (dazBoneJointRotationDisabled)
		{
			dazBoneJointRotationDisabled = false;
			dazBone.jointRotationDisabled = false;
		}
	}

	public void Prep()
	{
		if (orientationTransform != null)
		{
			orientationTransformRotation = orientationTransform.rotation;
		}
		if (parentOrientationTransform != null)
		{
			parentOrientationTransformRotation = parentOrientationTransform.rotation;
		}
	}

	public void ThreadedUpdate()
	{
		Quaternion q = dazBone.inverseStartingLocalRotation * Quaternion.Inverse(parentOrientationTransformRotation) * orientationTransformRotation;
		localRotationEuler = Quaternion2Angles.GetAngles(q, Quaternion2Angles.RotationOrder.XYZ) * 57.29578f;
		localRotationEuler.x *= multiplier.x;
		localRotationEuler.y *= multiplier.y;
		localRotationEuler.z *= multiplier.z;
		if (jointAxis.x == 1f)
		{
			applyRotation.x = localRotationEuler.x;
			if (jointSecondaryAxis.y == 1f)
			{
				applyRotation.y = localRotationEuler.y;
				applyRotation.z = localRotationEuler.z;
			}
			else
			{
				applyRotation.y = localRotationEuler.z;
				applyRotation.z = localRotationEuler.y;
			}
		}
		else if (jointAxis.y == 1f)
		{
			applyRotation.x = localRotationEuler.y;
			if (jointSecondaryAxis.x == 1f)
			{
				applyRotation.y = localRotationEuler.x;
				applyRotation.z = localRotationEuler.z;
			}
			else
			{
				applyRotation.y = localRotationEuler.z;
				applyRotation.z = 0f - localRotationEuler.x;
			}
		}
		else
		{
			applyRotation.x = localRotationEuler.z;
			if (jointSecondaryAxis.x == 1f)
			{
				applyRotation.y = localRotationEuler.x;
				applyRotation.z = localRotationEuler.y;
			}
			else
			{
				applyRotation.y = localRotationEuler.y;
				applyRotation.z = localRotationEuler.x;
			}
		}
		jointTargetRotation = Quaternion2Angles.EulerToQuaternion(applyRotation, outputRotationOrder);
	}

	public void Finish()
	{
		if (dazBone != null && cj != null)
		{
			dazBoneJointRotationDisabled = true;
			dazBone.jointRotationDisabled = true;
			cj.targetRotation = jointTargetRotation;
		}
	}
}
