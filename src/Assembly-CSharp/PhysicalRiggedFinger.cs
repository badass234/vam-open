using Leap.Unity;
using UnityEngine;

public class PhysicalRiggedFinger : RiggedFinger
{
	public ConfigurableJoint[] configurableJoints;

	protected Quaternion[] inverseStartingLocalRotations;

	private void OnEnable()
	{
		configurableJoints = new ConfigurableJoint[bones.Length];
		inverseStartingLocalRotations = new Quaternion[bones.Length];
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				ConfigurableJoint component = bones[i].GetComponent<ConfigurableJoint>();
				configurableJoints[i] = component;
				inverseStartingLocalRotations[i] = Quaternion.Inverse(bones[i].transform.localRotation);
			}
		}
	}

	public override void UpdateFinger()
	{
		for (int i = 0; i < bones.Length; i++)
		{
			if (!(bones[i] != null) || configurableJoints == null || i >= configurableJoints.Length)
			{
				continue;
			}
			ConfigurableJoint configurableJoint = configurableJoints[i];
			if (!(configurableJoint != null))
			{
				continue;
			}
			Quaternion quaternion = GetBoneRotation(i) * Reorientation();
			Quaternion rotation = ((i != 0 && !(bones[i - 1] == null)) ? (GetBoneRotation(i - 1) * Reorientation()) : configurableJoint.connectedBody.transform.rotation);
			Quaternion quaternion2 = Quaternion.Inverse(rotation) * quaternion;
			Quaternion quaternion3 = quaternion2;
			Quaternion targetRotation = quaternion3;
			if (configurableJoint.axis.x == 1f)
			{
				targetRotation.x = 0f - quaternion3.x;
				if (configurableJoint.secondaryAxis.x == 1f)
				{
					Debug.LogError("Illegal joint setup");
				}
				else if (configurableJoint.secondaryAxis.y == 1f)
				{
					targetRotation.y = 0f - quaternion3.y;
					targetRotation.z = 0f - quaternion3.z;
				}
				else if (configurableJoint.secondaryAxis.z == 1f)
				{
					targetRotation.y = 0f - quaternion3.z;
					targetRotation.z = 0f - quaternion3.y;
				}
				else
				{
					Debug.LogError("Did not account for secondary axis case " + configurableJoint.secondaryAxis);
				}
			}
			else if (configurableJoint.axis.y == 1f)
			{
				targetRotation.x = 0f - quaternion3.y;
				if (configurableJoint.secondaryAxis.x == 1f)
				{
					targetRotation.y = 0f - quaternion3.x;
					targetRotation.z = 0f - quaternion3.z;
				}
				else if (configurableJoint.secondaryAxis.y == 1f)
				{
					Debug.LogError("Illegal joint setup");
				}
				else if (configurableJoint.secondaryAxis.z == 1f)
				{
					targetRotation.y = 0f - quaternion3.z;
					targetRotation.z = 0f - quaternion3.x;
				}
				else
				{
					Debug.LogError("Did not account for secondary axis case " + configurableJoint.secondaryAxis);
				}
			}
			else if (configurableJoint.axis.z == 1f)
			{
				targetRotation.x = 0f - quaternion3.z;
				if (configurableJoint.secondaryAxis.x == 1f)
				{
					targetRotation.y = 0f - quaternion3.x;
					targetRotation.z = 0f - quaternion3.y;
				}
				else if (configurableJoint.secondaryAxis.y == 1f)
				{
					targetRotation.y = 0f - quaternion3.y;
					targetRotation.z = 0f - quaternion3.x;
				}
				else if (configurableJoint.secondaryAxis.z == 1f)
				{
					Debug.LogError("Illegal joint setup");
				}
				else
				{
					Debug.LogError("Did not account for secondary axis case " + configurableJoint.secondaryAxis);
				}
			}
			else
			{
				Debug.LogError("Did not account for axis case " + configurableJoint.axis);
			}
			configurableJoint.targetRotation = targetRotation;
		}
	}
}
