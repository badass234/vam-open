using System.IO;
using UnityEngine;

internal static class BinaryReaderExtensions
{
	public static OvrAvatarDriver.PoseFrame ReadPoseFrame(this BinaryReader reader)
	{
		OvrAvatarDriver.PoseFrame result = default(OvrAvatarDriver.PoseFrame);
		result.headPosition = reader.ReadVector3();
		result.headRotation = reader.ReadQuaternion();
		result.handLeftPosition = reader.ReadVector3();
		result.handLeftRotation = reader.ReadQuaternion();
		result.handRightPosition = reader.ReadVector3();
		result.handRightRotation = reader.ReadQuaternion();
		result.voiceAmplitude = reader.ReadSingle();
		result.controllerLeftPose = reader.ReadControllerPose();
		result.controllerRightPose = reader.ReadControllerPose();
		return result;
	}

	public static Vector2 ReadVector2(this BinaryReader reader)
	{
		Vector2 result = default(Vector2);
		result.x = reader.ReadSingle();
		result.y = reader.ReadSingle();
		return result;
	}

	public static Vector3 ReadVector3(this BinaryReader reader)
	{
		Vector3 result = default(Vector3);
		result.x = reader.ReadSingle();
		result.y = reader.ReadSingle();
		result.z = reader.ReadSingle();
		return result;
	}

	public static Quaternion ReadQuaternion(this BinaryReader reader)
	{
		Quaternion result = default(Quaternion);
		result.x = reader.ReadSingle();
		result.y = reader.ReadSingle();
		result.z = reader.ReadSingle();
		result.w = reader.ReadSingle();
		return result;
	}

	public static OvrAvatarDriver.ControllerPose ReadControllerPose(this BinaryReader reader)
	{
		OvrAvatarDriver.ControllerPose result = default(OvrAvatarDriver.ControllerPose);
		result.buttons = (ovrAvatarButton)reader.ReadUInt32();
		result.touches = (ovrAvatarTouch)reader.ReadUInt32();
		result.joystickPosition = reader.ReadVector2();
		result.indexTrigger = reader.ReadSingle();
		result.handTrigger = reader.ReadSingle();
		result.isActive = reader.ReadBoolean();
		return result;
	}
}
