using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class Matrix4x4Surrogate : ISerializationSurrogate
	{
		public float m00;

		public float m10;

		public float m20;

		public float m30;

		public float m01;

		public float m11;

		public float m21;

		public float m31;

		public float m02;

		public float m12;

		public float m22;

		public float m32;

		public float m03;

		public float m13;

		public float m23;

		public float m33;

		public static implicit operator Matrix4x4(Matrix4x4Surrogate v)
		{
			Matrix4x4 result = default(Matrix4x4);
			result.m00 = v.m00;
			result.m10 = v.m10;
			result.m20 = v.m20;
			result.m30 = v.m30;
			result.m01 = v.m01;
			result.m11 = v.m11;
			result.m21 = v.m21;
			result.m31 = v.m31;
			result.m02 = v.m02;
			result.m12 = v.m12;
			result.m22 = v.m22;
			result.m32 = v.m32;
			result.m03 = v.m03;
			result.m13 = v.m13;
			result.m23 = v.m23;
			result.m33 = v.m33;
			return result;
		}

		public static implicit operator Matrix4x4Surrogate(Matrix4x4 v)
		{
			Matrix4x4Surrogate matrix4x4Surrogate = new Matrix4x4Surrogate();
			matrix4x4Surrogate.m00 = v.m00;
			matrix4x4Surrogate.m10 = v.m10;
			matrix4x4Surrogate.m20 = v.m20;
			matrix4x4Surrogate.m30 = v.m30;
			matrix4x4Surrogate.m01 = v.m01;
			matrix4x4Surrogate.m11 = v.m11;
			matrix4x4Surrogate.m21 = v.m21;
			matrix4x4Surrogate.m31 = v.m31;
			matrix4x4Surrogate.m02 = v.m02;
			matrix4x4Surrogate.m12 = v.m12;
			matrix4x4Surrogate.m22 = v.m22;
			matrix4x4Surrogate.m32 = v.m32;
			matrix4x4Surrogate.m03 = v.m03;
			matrix4x4Surrogate.m13 = v.m13;
			matrix4x4Surrogate.m23 = v.m23;
			matrix4x4Surrogate.m33 = v.m33;
			return matrix4x4Surrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Matrix4x4 matrix4x = (Matrix4x4)obj;
			info.AddValue("m00", matrix4x.m00);
			info.AddValue("m10", matrix4x.m10);
			info.AddValue("m20", matrix4x.m20);
			info.AddValue("m30", matrix4x.m30);
			info.AddValue("m01", matrix4x.m01);
			info.AddValue("m11", matrix4x.m11);
			info.AddValue("m21", matrix4x.m21);
			info.AddValue("m31", matrix4x.m31);
			info.AddValue("m02", matrix4x.m02);
			info.AddValue("m12", matrix4x.m12);
			info.AddValue("m22", matrix4x.m22);
			info.AddValue("m32", matrix4x.m32);
			info.AddValue("m03", matrix4x.m03);
			info.AddValue("m13", matrix4x.m13);
			info.AddValue("m23", matrix4x.m23);
			info.AddValue("m33", matrix4x.m33);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Matrix4x4 matrix4x = (Matrix4x4)obj;
			matrix4x.m00 = (float)info.GetValue("m00", typeof(float));
			matrix4x.m10 = (float)info.GetValue("m10", typeof(float));
			matrix4x.m20 = (float)info.GetValue("m20", typeof(float));
			matrix4x.m30 = (float)info.GetValue("m30", typeof(float));
			matrix4x.m01 = (float)info.GetValue("m01", typeof(float));
			matrix4x.m11 = (float)info.GetValue("m11", typeof(float));
			matrix4x.m21 = (float)info.GetValue("m21", typeof(float));
			matrix4x.m31 = (float)info.GetValue("m31", typeof(float));
			matrix4x.m02 = (float)info.GetValue("m02", typeof(float));
			matrix4x.m12 = (float)info.GetValue("m12", typeof(float));
			matrix4x.m22 = (float)info.GetValue("m22", typeof(float));
			matrix4x.m32 = (float)info.GetValue("m32", typeof(float));
			matrix4x.m03 = (float)info.GetValue("m03", typeof(float));
			matrix4x.m13 = (float)info.GetValue("m13", typeof(float));
			matrix4x.m23 = (float)info.GetValue("m23", typeof(float));
			matrix4x.m33 = (float)info.GetValue("m33", typeof(float));
			return matrix4x;
		}
	}
}
