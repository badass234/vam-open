using UnityEngine;

namespace MVR
{
	public static class NaNUtils
	{
		public static bool IsMatrixValid(Matrix4x4 m)
		{
			if (float.IsNaN(m.m00) || float.IsNaN(m.m01) || float.IsNaN(m.m02) || float.IsNaN(m.m03) || float.IsNaN(m.m10) || float.IsNaN(m.m11) || float.IsNaN(m.m12) || float.IsNaN(m.m13) || float.IsNaN(m.m20) || float.IsNaN(m.m21) || float.IsNaN(m.m22) || float.IsNaN(m.m23) || float.IsNaN(m.m30) || float.IsNaN(m.m31) || float.IsNaN(m.m32) || float.IsNaN(m.m33))
			{
				return false;
			}
			return true;
		}

		public static bool IsQuaternionValid(Quaternion q)
		{
			if (float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w))
			{
				return false;
			}
			return true;
		}

		public static bool IsVector3Valid(Vector3 v)
		{
			if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z))
			{
				return false;
			}
			return true;
		}
	}
}
