using UnityEngine;

namespace Leap.Unity.Space
{
	public interface ITransformer
	{
		LeapSpaceAnchor anchor { get; }

		Vector3 TransformPoint(Vector3 localRectPos);

		Vector3 InverseTransformPoint(Vector3 localWarpedSpace);

		Quaternion TransformRotation(Vector3 localRectPos, Quaternion localRectRot);

		Quaternion InverseTransformRotation(Vector3 localWarpedPos, Quaternion localWarpedRot);

		Vector3 TransformDirection(Vector3 localRectPos, Vector3 localRectDirection);

		Vector3 InverseTransformDirection(Vector3 localWarpedSpace, Vector3 localWarpedDirection);

		Matrix4x4 GetTransformationMatrix(Vector3 localRectPos);
	}
}
