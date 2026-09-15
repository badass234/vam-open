using UnityEngine;

namespace MVR
{
	public class TransformRotator : MonoBehaviour
	{
		public enum Axis
		{
			X,
			NegX,
			Y,
			NegY,
			Z,
			NegZ
		}

		public Axis axis;

		public float speed = 1f;

		private void Update()
		{
			float num = speed * Time.deltaTime;
			switch (axis)
			{
			case Axis.X:
				base.transform.Rotate(num, 0f, 0f);
				break;
			case Axis.NegX:
				base.transform.Rotate(0f - num, 0f, 0f);
				break;
			case Axis.Y:
				base.transform.Rotate(0f, num, 0f);
				break;
			case Axis.NegY:
				base.transform.Rotate(0f, 0f - num, 0f);
				break;
			case Axis.Z:
				base.transform.Rotate(0f, 0f, num);
				break;
			case Axis.NegZ:
				base.transform.Rotate(0f, 0f, 0f - num);
				break;
			}
		}
	}
}
