using UnityEngine;

namespace Leap.Unity.Space
{
	public class IdentityTransformer : ITransformer
	{
		public static readonly IdentityTransformer single = new IdentityTransformer();

		public LeapSpaceAnchor anchor => null;

		public Vector3 TransformPoint(Vector3 localRectPos)
		{
			return localRectPos;
		}

		public Vector3 InverseTransformPoint(Vector3 localWarpedSpace)
		{
			return localWarpedSpace;
		}

		public Quaternion TransformRotation(Vector3 localRectPos, Quaternion localRectRot)
		{
			return localRectRot;
		}

		public Quaternion InverseTransformRotation(Vector3 localWarpedPos, Quaternion localWarpedRot)
		{
			return localWarpedRot;
		}

		public Vector3 TransformDirection(Vector3 localRectPos, Vector3 localRectDirection)
		{
			return localRectDirection;
		}

		public Vector3 InverseTransformDirection(Vector3 localWarpedSpace, Vector3 localWarpedDirection)
		{
			return localWarpedDirection;
		}

		public Matrix4x4 GetTransformationMatrix(Vector3 localRectPos)
		{
			return Matrix4x4.TRS(localRectPos, Quaternion.identity, Vector3.one);
		}
	}
}
