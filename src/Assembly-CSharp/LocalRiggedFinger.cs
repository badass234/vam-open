using Leap;
using Leap.Unity;
using UnityEngine;

public class LocalRiggedFinger : RiggedFinger
{
	public override void UpdateFinger()
	{
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				if (finger_ != null)
				{
					Quaternion quaternion = finger_.Bone((Bone.BoneType)i).Rotation.ToQuaternion() * Reorientation();
					Quaternion rotation = ((i != 0 && !(bones[i - 1] == null)) ? (finger_.Bone((Bone.BoneType)(i - 1)).Rotation.ToQuaternion() * Reorientation()) : base.transform.parent.rotation);
					Quaternion localRotation = Quaternion.Inverse(rotation) * quaternion;
					bones[i].localRotation = localRotation;
				}
				else
				{
					Debug.LogError("Finger not set");
				}
			}
		}
	}
}
