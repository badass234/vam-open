using System;

namespace Leap
{
	public struct Matrix
	{
		public static readonly Matrix Identity = new Matrix(Vector.XAxis, Vector.YAxis, Vector.ZAxis, Vector.Zero);

		public Vector xBasis { get; set; }

		public Vector yBasis { get; set; }

		public Vector zBasis { get; set; }

		public Vector origin { get; set; }

		public Matrix(Matrix other)
		{
			this = default(Matrix);
			xBasis = other.xBasis;
			yBasis = other.yBasis;
			zBasis = other.zBasis;
			origin = other.origin;
		}

		public Matrix(Vector xBasis, Vector yBasis, Vector zBasis)
		{
			this = default(Matrix);
			this.xBasis = xBasis;
			this.yBasis = yBasis;
			this.zBasis = zBasis;
			origin = Vector.Zero;
		}

		public Matrix(Vector xBasis, Vector yBasis, Vector zBasis, Vector origin)
		{
			this = default(Matrix);
			this.xBasis = xBasis;
			this.yBasis = yBasis;
			this.zBasis = zBasis;
			this.origin = origin;
		}

		public Matrix(Vector axis, float angleRadians)
		{
			this = default(Matrix);
			xBasis = Vector.XAxis;
			yBasis = Vector.YAxis;
			zBasis = Vector.ZAxis;
			origin = Vector.Zero;
			SetRotation(axis, angleRadians);
		}

		public Matrix(Vector axis, float angleRadians, Vector translation)
		{
			this = default(Matrix);
			xBasis = Vector.XAxis;
			yBasis = Vector.YAxis;
			zBasis = Vector.ZAxis;
			origin = translation;
			SetRotation(axis, angleRadians);
		}

		public Matrix(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
		{
			this = default(Matrix);
			xBasis = new Vector(m00, m01, m02);
			yBasis = new Vector(m10, m11, m12);
			zBasis = new Vector(m20, m21, m22);
			origin = Vector.Zero;
		}

		public Matrix(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22, float m30, float m31, float m32)
		{
			this = default(Matrix);
			xBasis = new Vector(m00, m01, m02);
			yBasis = new Vector(m10, m11, m12);
			zBasis = new Vector(m20, m21, m22);
			origin = new Vector(m30, m31, m32);
		}

		public static Matrix operator *(Matrix m1, Matrix m2)
		{
			return m1._operator_mul(m2);
		}

		public float[] ToArray3x3(float[] output)
		{
			output[0] = xBasis.x;
			output[1] = xBasis.y;
			output[2] = xBasis.z;
			output[3] = yBasis.x;
			output[4] = yBasis.y;
			output[5] = yBasis.z;
			output[6] = zBasis.x;
			output[7] = zBasis.y;
			output[8] = zBasis.z;
			return output;
		}

		public double[] ToArray3x3(double[] output)
		{
			output[0] = xBasis.x;
			output[1] = xBasis.y;
			output[2] = xBasis.z;
			output[3] = yBasis.x;
			output[4] = yBasis.y;
			output[5] = yBasis.z;
			output[6] = zBasis.x;
			output[7] = zBasis.y;
			output[8] = zBasis.z;
			return output;
		}

		public float[] ToArray3x3()
		{
			return ToArray3x3(new float[9]);
		}

		public float[] ToArray4x4(float[] output)
		{
			output[0] = xBasis.x;
			output[1] = xBasis.y;
			output[2] = xBasis.z;
			output[3] = 0f;
			output[4] = yBasis.x;
			output[5] = yBasis.y;
			output[6] = yBasis.z;
			output[7] = 0f;
			output[8] = zBasis.x;
			output[9] = zBasis.y;
			output[10] = zBasis.z;
			output[11] = 0f;
			output[12] = origin.x;
			output[13] = origin.y;
			output[14] = origin.z;
			output[15] = 1f;
			return output;
		}

		public double[] ToArray4x4(double[] output)
		{
			output[0] = xBasis.x;
			output[1] = xBasis.y;
			output[2] = xBasis.z;
			output[3] = 0.0;
			output[4] = yBasis.x;
			output[5] = yBasis.y;
			output[6] = yBasis.z;
			output[7] = 0.0;
			output[8] = zBasis.x;
			output[9] = zBasis.y;
			output[10] = zBasis.z;
			output[11] = 0.0;
			output[12] = origin.x;
			output[13] = origin.y;
			output[14] = origin.z;
			output[15] = 1.0;
			return output;
		}

		public float[] ToArray4x4()
		{
			return ToArray4x4(new float[16]);
		}

		public void SetRotation(Vector axis, float angleRadians)
		{
			Vector normalized = axis.Normalized;
			float num = (float)Math.Sin(angleRadians);
			float num2 = (float)Math.Cos(angleRadians);
			float num3 = 1f - num2;
			xBasis = new Vector(normalized[0u] * normalized[0u] * num3 + num2, normalized[0u] * normalized[1u] * num3 - normalized[2u] * num, normalized[0u] * normalized[2u] * num3 + normalized[1u] * num);
			yBasis = new Vector(normalized[1u] * normalized[0u] * num3 + normalized[2u] * num, normalized[1u] * normalized[1u] * num3 + num2, normalized[1u] * normalized[2u] * num3 - normalized[0u] * num);
			zBasis = new Vector(normalized[2u] * normalized[0u] * num3 - normalized[1u] * num, normalized[2u] * normalized[1u] * num3 + normalized[0u] * num, normalized[2u] * normalized[2u] * num3 + num2);
		}

		public Vector TransformPoint(Vector point)
		{
			return xBasis * point.x + yBasis * point.y + zBasis * point.z + origin;
		}

		public Vector TransformDirection(Vector direction)
		{
			return xBasis * direction.x + yBasis * direction.y + zBasis * direction.z;
		}

		public Matrix RigidInverse()
		{
			Matrix result = new Matrix(new Vector(xBasis[0u], yBasis[0u], zBasis[0u]), new Vector(xBasis[1u], yBasis[1u], zBasis[1u]), new Vector(xBasis[2u], yBasis[2u], zBasis[2u]));
			result.origin = result.TransformDirection(-origin);
			return result;
		}

		private Matrix _operator_mul(Matrix other)
		{
			return new Matrix(TransformDirection(other.xBasis), TransformDirection(other.yBasis), TransformDirection(other.zBasis), TransformPoint(other.origin));
		}

		public bool Equals(Matrix other)
		{
			return xBasis == other.xBasis && yBasis == other.yBasis && zBasis == other.zBasis && origin == other.origin;
		}

		public override string ToString()
		{
			return $"xBasis: {xBasis} yBasis: {yBasis} zBasis: {zBasis} origin: {origin}";
		}
	}
}
