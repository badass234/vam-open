using UnityEngine;

namespace MeshVR.Hands
{
	public class FingerInput : Finger
	{
		public float bendInputFactor = 1f;

		public bool debug;

		protected Quaternion inverseStartingLocalRotation;

		protected bool _wasInit;

		protected void CorrectBend()
		{
			currentBend += bendOffset;
			if (currentBend > 180f)
			{
				currentBend -= 360f;
			}
			else if (currentBend <= -180f)
			{
				currentBend += 360f;
			}
			currentBend *= bendInputFactor;
		}

		protected void CorrectSpread()
		{
			currentSpread += spreadOffset;
			if (currentSpread > 180f)
			{
				currentSpread -= 360f;
			}
			else if (currentSpread <= -180f)
			{
				currentSpread += 360f;
			}
		}

		protected void CorrectTwist()
		{
			currentTwist += twistOffset;
			if (currentTwist > 180f)
			{
				currentTwist -= 360f;
			}
			else if (currentTwist <= -180f)
			{
				currentTwist += 360f;
			}
		}

		public void UpdateInput()
		{
			Vector3 eulerAngles = (inverseStartingLocalRotation * base.transform.localRotation).eulerAngles;
			if (bendEnabled)
			{
				switch (bendAxis)
				{
				case Axis.X:
					currentBend = eulerAngles.x;
					break;
				case Axis.NegX:
					currentBend = 0f - eulerAngles.x;
					break;
				case Axis.Y:
					currentBend = eulerAngles.y;
					break;
				case Axis.NegY:
					currentBend = 0f - eulerAngles.y;
					break;
				case Axis.Z:
					currentBend = eulerAngles.z;
					break;
				case Axis.NegZ:
					currentBend = 0f - eulerAngles.z;
					break;
				}
				CorrectBend();
			}
			if (spreadEnabled)
			{
				switch (spreadAxis)
				{
				case Axis.X:
					currentSpread = eulerAngles.x;
					break;
				case Axis.NegX:
					currentSpread = 0f - eulerAngles.x;
					break;
				case Axis.Y:
					currentSpread = eulerAngles.y;
					break;
				case Axis.NegY:
					currentSpread = 0f - eulerAngles.y;
					break;
				case Axis.Z:
					currentSpread = eulerAngles.z;
					break;
				case Axis.NegZ:
					currentSpread = 0f - eulerAngles.z;
					break;
				}
				CorrectSpread();
			}
			if (twistEnabled)
			{
				switch (twistAxis)
				{
				case Axis.X:
					currentTwist = eulerAngles.x;
					break;
				case Axis.NegX:
					currentTwist = 0f - eulerAngles.x;
					break;
				case Axis.Y:
					currentTwist = eulerAngles.y;
					break;
				case Axis.NegY:
					currentTwist = 0f - eulerAngles.y;
					break;
				case Axis.Z:
					currentTwist = eulerAngles.z;
					break;
				case Axis.NegZ:
					currentTwist = 0f - eulerAngles.z;
					break;
				}
				CorrectTwist();
			}
		}

		public void Init()
		{
			if (!_wasInit)
			{
				_wasInit = true;
				inverseStartingLocalRotation = Quaternion.Inverse(base.transform.localRotation);
				if (debug)
				{
					Debug.Log("Starting local rotation for " + base.name + " is " + base.transform.localEulerAngles);
				}
			}
		}
	}
}
