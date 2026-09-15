using UnityEngine;

namespace GPUTools.Physics.Scripts.Types.Joints
{
	public struct GPPointJoint
	{
		public int BodyId;

		public int MatrixId;

		public Vector3 Point;

		public float Rigidity;

		public GPPointJoint(int bodyId, int matrixId, Vector3 point, float rigidity)
		{
			BodyId = bodyId;
			Point = point;
			MatrixId = matrixId;
			Rigidity = rigidity;
		}

		public static int Size()
		{
			return 24;
		}
	}
}
