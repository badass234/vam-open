using UnityEngine;

namespace Leap.Unity.Space
{
	public class LeapCylindricalSpace : LeapRadialSpace
	{
		public class Transformer : IRadialTransformer, ITransformer
		{
			public float angleOffset;

			public float heightOffset;

			public float radiusOffset;

			public float radiansPerMeter;

			public LeapCylindricalSpace space { get; set; }

			public LeapSpaceAnchor anchor { get; set; }

			public Vector3 TransformPoint(Vector3 localRectPos)
			{
				Vector3 vector = space.transform.InverseTransformPoint(anchor.transform.position);
				Vector3 vector2 = localRectPos - vector;
				float f = angleOffset + vector2.x / radiusOffset;
				float y = heightOffset + vector2.y;
				float num = radiusOffset + vector2.z;
				Vector3 result = default(Vector3);
				result.x = Mathf.Sin(f) * num;
				result.y = y;
				result.z = Mathf.Cos(f) * num - space.radius;
				return result;
			}

			public Vector3 InverseTransformPoint(Vector3 localWarpedPos)
			{
				localWarpedPos.z += space.radius;
				float num = Mathf.Atan2(localWarpedPos.x, localWarpedPos.z);
				float y = localWarpedPos.y;
				float magnitude = new Vector2(localWarpedPos.x, localWarpedPos.z).magnitude;
				Vector3 vector = default(Vector3);
				vector.x = (num - angleOffset) * radiusOffset;
				vector.y = y - heightOffset;
				vector.z = magnitude - radiusOffset;
				Vector3 vector2 = space.transform.InverseTransformPoint(anchor.transform.position);
				return vector2 + vector;
			}

			public Quaternion TransformRotation(Vector3 localRectPos, Quaternion localRectRot)
			{
				Vector3 vector = space.transform.InverseTransformPoint(anchor.transform.position);
				Vector3 vector2 = localRectPos - vector;
				float num = angleOffset + vector2.x / radiusOffset;
				Quaternion quaternion = Quaternion.Euler(0f, num * 57.29578f, 0f);
				return quaternion * localRectRot;
			}

			public Quaternion InverseTransformRotation(Vector3 localWarpedPos, Quaternion localWarpedRot)
			{
				localWarpedPos.z += space.radius;
				float num = Mathf.Atan2(localWarpedPos.x, localWarpedPos.z);
				return Quaternion.Euler(0f, (0f - num) * 57.29578f, 0f) * localWarpedRot;
			}

			public Vector3 TransformDirection(Vector3 localRectPos, Vector3 localRectDirection)
			{
				Vector3 vector = space.transform.InverseTransformPoint(anchor.transform.position);
				Vector3 vector2 = localRectPos - vector;
				float num = angleOffset + vector2.x / radiusOffset;
				Quaternion quaternion = Quaternion.Euler(0f, num * 57.29578f, 0f);
				return quaternion * localRectDirection;
			}

			public Vector3 InverseTransformDirection(Vector3 localWarpedPos, Vector3 localWarpedDirection)
			{
				localWarpedPos.z += space.radius;
				float num = Mathf.Atan2(localWarpedPos.x, localWarpedPos.z);
				return Quaternion.Euler(0f, (0f - num) * 57.29578f, 0f) * localWarpedDirection;
			}

			public Matrix4x4 GetTransformationMatrix(Vector3 localRectPos)
			{
				Vector3 vector = space.transform.InverseTransformPoint(anchor.transform.position);
				Vector3 vector2 = localRectPos - vector;
				float num = angleOffset + vector2.x / radiusOffset;
				float y = heightOffset + vector2.y;
				float num2 = radiusOffset + vector2.z;
				Vector3 pos = default(Vector3);
				pos.x = Mathf.Sin(num) * num2;
				pos.y = y;
				pos.z = Mathf.Cos(num) * num2 - space.radius;
				Quaternion q = Quaternion.Euler(0f, num * 57.29578f, 0f);
				return Matrix4x4.TRS(pos, q, Vector3.one);
			}

			public Vector4 GetVectorRepresentation(Transform element)
			{
				Vector3 vector = space.transform.InverseTransformPoint(element.position);
				Vector3 vector2 = space.transform.InverseTransformPoint(anchor.transform.position);
				Vector3 vector3 = vector - vector2;
				Vector4 result = default(Vector4);
				result.x = angleOffset + vector3.x / radiusOffset;
				result.y = heightOffset + vector3.y;
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
			transformer.angleOffset = 0f;
			transformer.heightOffset = 0f;
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
			transformer2.angleOffset = transformer3.angleOffset + rectSpaceDelta.x / transformer3.radiusOffset;
			transformer2.heightOffset = transformer3.heightOffset + rectSpaceDelta.y;
			transformer2.radiusOffset = transformer3.radiusOffset + rectSpaceDelta.z;
			transformer2.radiansPerMeter = 1f / transformer2.radiusOffset;
		}
	}
}
