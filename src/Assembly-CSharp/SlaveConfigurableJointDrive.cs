using UnityEngine;

public class SlaveConfigurableJointDrive : MonoBehaviour
{
	public Transform slaveTo;

	protected ConfigurableJoint joint;

	protected Quaternion inverseStartingLocalRotation;

	private void Start()
	{
		joint = GetComponent<ConfigurableJoint>();
		inverseStartingLocalRotation = Quaternion.Inverse(base.transform.localRotation);
	}

	private void Update()
	{
		if (!(joint != null))
		{
			return;
		}
		Quaternion quaternion = inverseStartingLocalRotation * slaveTo.localRotation;
		Quaternion targetRotation = quaternion;
		if (joint.axis.x == 1f)
		{
			targetRotation.x = 0f - quaternion.x;
			if (joint.secondaryAxis.x == 1f)
			{
				Debug.LogError("Illegal joint setup");
			}
			else if (joint.secondaryAxis.y == 1f)
			{
				targetRotation.y = 0f - quaternion.y;
				targetRotation.z = 0f - quaternion.z;
			}
			else if (joint.secondaryAxis.z == 1f)
			{
				targetRotation.y = 0f - quaternion.z;
				targetRotation.z = 0f - quaternion.y;
			}
			else
			{
				Debug.LogError("Did not account for secondary axis case " + joint.secondaryAxis);
			}
		}
		else if (joint.axis.y == 1f)
		{
			targetRotation.x = 0f - quaternion.y;
			if (joint.secondaryAxis.x == 1f)
			{
				targetRotation.y = 0f - quaternion.x;
				targetRotation.z = 0f - quaternion.z;
			}
			else if (joint.secondaryAxis.y == 1f)
			{
				Debug.LogError("Illegal joint setup");
			}
			else if (joint.secondaryAxis.z == 1f)
			{
				targetRotation.y = 0f - quaternion.z;
				targetRotation.z = 0f - quaternion.x;
			}
			else
			{
				Debug.LogError("Did not account for secondary axis case " + joint.secondaryAxis);
			}
		}
		else if (joint.axis.z == 1f)
		{
			targetRotation.x = 0f - quaternion.z;
			if (joint.secondaryAxis.x == 1f)
			{
				targetRotation.y = 0f - quaternion.x;
				targetRotation.z = 0f - quaternion.y;
			}
			else if (joint.secondaryAxis.y == 1f)
			{
				targetRotation.y = 0f - quaternion.y;
				targetRotation.z = 0f - quaternion.x;
			}
			else if (joint.secondaryAxis.z == 1f)
			{
				Debug.LogError("Illegal joint setup");
			}
			else
			{
				Debug.LogError("Did not account for secondary axis case " + joint.secondaryAxis);
			}
		}
		else
		{
			Debug.LogError("Did not account for axis case " + joint.axis);
		}
		joint.targetRotation = targetRotation;
	}
}
