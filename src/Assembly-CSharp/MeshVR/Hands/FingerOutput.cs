using UnityEngine;

namespace MeshVR.Hands
{
	public class FingerOutput : Finger
	{
		protected ConfigurableJoint joint;

		protected void Awake()
		{
			joint = GetComponent<ConfigurableJoint>();
		}

		public void UpdateOutput()
		{
			if (!(joint != null))
			{
				return;
			}
			Vector3 euler = default(Vector3);
			euler.x = 0f;
			euler.y = 0f;
			euler.z = 0f;
			if (bendEnabled)
			{
				float num = currentBend + bendOffset;
				switch (bendAxis)
				{
				case Axis.X:
					euler.x = num;
					break;
				case Axis.NegX:
					euler.x = 0f - num;
					break;
				case Axis.Y:
					euler.y = num;
					break;
				case Axis.NegY:
					euler.y = 0f - num;
					break;
				case Axis.Z:
					euler.z = num;
					break;
				case Axis.NegZ:
					euler.z = 0f - num;
					break;
				}
			}
			if (spreadEnabled)
			{
				float num2 = currentSpread + spreadOffset;
				switch (spreadAxis)
				{
				case Axis.X:
					euler.x = num2;
					break;
				case Axis.NegX:
					euler.x = 0f - num2;
					break;
				case Axis.Y:
					euler.y = num2;
					break;
				case Axis.NegY:
					euler.y = 0f - num2;
					break;
				case Axis.Z:
					euler.z = num2;
					break;
				case Axis.NegZ:
					euler.z = 0f - num2;
					break;
				}
			}
			if (twistEnabled)
			{
				float num3 = currentTwist + twistOffset;
				switch (twistAxis)
				{
				case Axis.X:
					euler.x = num3;
					break;
				case Axis.NegX:
					euler.x = 0f - num3;
					break;
				case Axis.Y:
					euler.y = num3;
					break;
				case Axis.NegY:
					euler.y = 0f - num3;
					break;
				case Axis.Z:
					euler.z = num3;
					break;
				case Axis.NegZ:
					euler.z = 0f - num3;
					break;
				}
			}
			joint.targetRotation = Quaternion.Euler(euler);
		}
	}
}
