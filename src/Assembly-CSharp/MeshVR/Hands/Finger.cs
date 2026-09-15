using UnityEngine;

namespace MeshVR.Hands
{
	public class Finger : MonoBehaviour
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

		public Axis bendAxis = Axis.Z;

		public Axis spreadAxis = Axis.Y;

		public Axis twistAxis;

		public bool bendEnabled = true;

		public bool spreadEnabled;

		public bool twistEnabled;

		public float bendOffset;

		public float spreadOffset;

		public float twistOffset;

		public float currentBend;

		public float currentSpread;

		public float currentTwist;
	}
}
