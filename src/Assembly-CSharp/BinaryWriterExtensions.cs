using System.IO;
using UnityEngine;

internal static class BinaryWriterExtensions
{
	public static void Write(this BinaryWriter writer, OvrAvatarDriver.PoseFrame frame)
	{
		writer.Write(frame.headPosition);
		writer.Write(frame.headRotation);
		writer.Write(frame.handLeftPosition);
		writer.Write(frame.handLeftRotation);
		writer.Write(frame.handRightPosition);
		writer.Write(frame.handRightRotation);
		writer.Write(frame.voiceAmplitude);
		writer.Write(frame.controllerLeftPose);
		writer.Write(frame.controllerRightPose);
	}

	public static void Write(this BinaryWriter writer, Vector3 vec3)
	{
		writer.Write(vec3.x);
		writer.Write(vec3.y);
		writer.Write(vec3.z);
	}

	public static void Write(this BinaryWriter writer, Vector2 vec2)
	{
		writer.Write(vec2.x);
		writer.Write(vec2.y);
	}

	public static void Write(this BinaryWriter writer, Quaternion quat)
	{
		writer.Write(quat.x);
		writer.Write(quat.y);
		writer.Write(quat.z);
		writer.Write(quat.w);
	}

	public static void Write(this BinaryWriter writer, OvrAvatarDriver.ControllerPose pose)
	{
		writer.Write((uint)pose.buttons);
		writer.Write((uint)pose.touches);
		writer.Write(pose.joystickPosition);
		writer.Write(pose.indexTrigger);
		writer.Write(pose.handTrigger);
		writer.Write(pose.isActive);
	}
}
