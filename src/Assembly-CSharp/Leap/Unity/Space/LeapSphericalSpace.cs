using UnityEngine;

namespace Leap.Unity.Space
{
	public class LeapSphericalSpace : LeapRadialSpace
	{
		public class Transformer : IRadialTransformer, ITransformer
		{
			public float angleXOffset;

			public float angleYOffset;

			public float radiusOffset;

			public float radiansPerMeter;

			public LeapSphericalSpace space { get; set; }

			public LeapSpaceAnchor anchor { get; set; }

			public Vector3 TransformPoint(Vector3 localRectPos)
			{
				Vector3 vector = space.transform.InverseTransformPoint(anchor.transform.position);
				Vector3 vector2 = localRectPos - vector;
				float f = angleXOffset + vector2.x / radiusOffset;
				float f2 = angleYOffset + vector2.y / radiusOffset;
				float num = radiusOffset + vector2.z;
				Vector3 vector3 = default(Vector3);
				vector3.x = 0f;
				vector3.y = Mathf.Sin(f2) * num;
				vector3.z = Mathf.Cos(f2) * num;
				Vector3 result = default(Vector3);
				result.x = Mathf.Sin(f) * vector3.z;
				result.y = vector3.y;
				result.z = Mathf.Cos(f) * vector3.z - space.radius;
				return result;
			}

			public Vector3 InverseTransformPoint(Vector3 localWarpedPos)
			{
				localWarpedPos.z += space.radius;
				Vector3 vector = default(Vector3);
				vector.x = 0f;
				vector.y = localWarpedPos.y;
				vector.z = new Vector2(localWarpedPos.x, localWarpedPos.z).magnitude;
				float num = Mathf.Atan2(localWarpedPos.x, localWarpedPos.z);
				float num2 = Mathf.Atan2(vector.y, vector.z);
				float magnitude = new Vector2(vector.z, vector.y).magnitude;
				Vector3 vector2 = default(Vector3);
				vector2.x = (num - angleXOffset) * radiusOffset;
				vector2.y = (num2 - angleYOffset) * radiusOffset;
				vector2.z = magnitude - radiusOffset;
				Vector3 vector3 = space.transform.InverseTransformPoint(anchor.transform.position);
				return vector3 + vector2;
			}

			public Quaternion TransformRotation(Vector3 localRectPos, Quaternion localRectRot)
			{
				Vector3 vector = space.transform.InverseTransformPoint(anchor.transform.position);
				Vector3 vector2 = localRectPos - vector;
				float num = angleXOffset + vector2.x / radiusOffset;
				float num2 = angleYOffset + vector2.y / radiusOffset;
				Quaternion quaternion = Quaternion.Euler((0f - num2) * 57.29578f, num * 57.29578f, 0f);
				return quaternion * localRectRot;
			}

			public Quaternion InverseTransformRotation(Vector3 localWarpedPos, Quaternion localWarpedRot)
			{
				localWarpedPos.z += space.radius;
				Vector3 vector = default(Vector3);
				vector.x = 0f;
				vector.y = localWarpedPos.y;
				vector.z = new Vector2(localWarpedPos.x, localWarpedPos.z).magnitude;
				float num = Mathf.Atan2(localWarpedPos.x, localWarpedPos.z);
				float num2 = Mathf.Atan2(vector.y, vector.z);
				Quaternion rotation = Quaternion.Euler((0f - num2) * 57.29578f, num * 57.29578f, 0f);
				Quaternion quaternion = Quaternion.Inverse(rotation);
				return quaternion * localWarpedRot;
			}

			public Vector3 TransformDirection(Vector3 localRectPos, Vector3 localRectDirection)
			{
				Vector3 vector = space.transform.InverseTransformPoint(anchor.transform.position);
				Vector3 vector2 = localRectPos - vector;
				float num = angleXOffset + vector2.x / radiusOffset;
				float num2 = angleYOffset + vector2.y / radiusOffset;
				Quaternion quaternion = Quaternion.Euler((0f - num2) * 57.29578f, num * 57.29578f, 0f);
				return quaternion * localRectDirection;
			}

			public Vector3 InverseTransformDirection(Vector3 localWarpedPos, Vector3 localWarpedDirection)
			{
				localWarpedPos.z += space.radius;
				Vector3 vector = default(Vector3);
				vector.x = 0f;
				vector.y = localWarpedPos.y;
				vector.z = new Vector2(localWarpedPos.x, localWarpedPos.z).magnitude;
				float num = Mathf.Atan2(localWarpedPos.x, localWarpedPos.z);
				float num2 = Mathf.Atan2(vector.y, vector.z);
				Quaternion rotation = Quaternion.Euler((0f - num2) * 57.29578f, num * 57.29578f, 0f);
				Quaternion quaternion = Quaternion.Inverse(rotation);
				return quaternion * localWarpedDirection;
			}

			public Matrix4x4 GetTransformationMatrix(Vector3 localRectPos)
			{
				Vector3 vector = space.transform.InverseTransformPoint(anchor.transform.position);
				Vector3 vector2 = localRectPos - vector;
				float num = angleXOffset + vector2.x / radiusOffset;
				float num2 = angleYOffset + vector2.y / radiusOffset;
				float num3 = radiusOffset + vector2.z;
				Vector3 vector3 = default(Vector3);
				vector3.x = 0f;
				vector3.y = Mathf.Sin(num2) * num3;
				vector3.z = Mathf.Cos(num2) * num3;
				Vector3 pos = default(Vector3);
				pos.x = Mathf.Sin(num) * vector3.z;
				pos.y = vector3.y;
				pos.z = Mathf.Cos(num) * vector3.z - space.radius;
				Quaternion q = Quaternion.Euler((0f - num2) * 57.29578f, num * 57.29578f, 0f);
				return Matrix4x4.TRS(pos, q, Vector3.one);
			}

			public Vector4 GetVectorRepresentation(Transform element)
			{
				Vector3 vector = space.transform.InverseTransformPoint(element.position);
				Vector3 vector2 = space.transform.InverseTransformPoint(anchor.transform.position);
				Vector3 vector3 = vector - vector2;
				Vector4 result = default(Vector4);
				result.x = angleXOffset + vector3.x / radiusOffset;
				result.y = angleYOffset + vector3.y / radiusOffset;
				result.z = radiusOffset + vector3.z;
				result.w = 1f / radiusOffset;
				return result;
			}
		}

		protected override ITransformer CosntructBaseTransformer()
		{
			Transformer transformer = new Transformer();
			transformer.space = this;
			transformer.anchor = this;
			transformer.angleXOffset = 0f;
			transformer.angleYOffset = 0f;
			transformer.radiusOffset = base.radius;
			transformer.radiansPerMeter = 1f / base.radius;
			return transformer;
		}

		protected override ITransformer ConstructTransformer(LeapSpaceAnchor anchor)
		{
			Transformer transformer = new Transformer();
			transformer.space = this;
			transformer.anchor = anchor;
			return transformer;
		}

		protected override void UpdateRadialTransformer(ITransformer transformer, ITransformer parent, Vector3 rectSpaceDelta)
		{
			Transformer transformer2 = transformer as Transformer;
			Transformer transformer3 = parent as Transformer;
			transformer2.angleXOffset = transformer3.angleXOffset + rectSpaceDelta.x / transformer3.radiusOffset;
			transformer2.angleYOffset = transformer3.angleYOffset + rectSpaceDelta.y / transformer3.radiusOffset;
			transformer2.radiusOffset = transformer3.radiusOffset + rectSpaceDelta.z;
			transformer2.radiansPerMeter = 1f / transformer2.radiusOffset;
		}
	}
}
