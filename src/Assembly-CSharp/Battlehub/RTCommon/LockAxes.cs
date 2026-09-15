using System.Linq;
using UnityEngine;

namespace Battlehub.RTCommon
{
	public class LockAxes : MonoBehaviour
	{
		public bool PositionX;

		public bool PositionY;

		public bool PositionZ;

		public bool RotationX;

		public bool RotationY;

		public bool RotationZ;

		public bool RotationScreen;

		public bool ScaleX;

		public bool ScaleY;

		public bool ScaleZ;

		public static LockObject Eval(LockAxes[] lockAxes)
		{
			LockObject lockObject = new LockObject();
			if (lockAxes != null)
			{
				lockObject.PositionX = lockAxes.Any((LockAxes la) => la.PositionX);
				lockObject.PositionY = lockAxes.Any((LockAxes la) => la.PositionY);
				lockObject.PositionZ = lockAxes.Any((LockAxes la) => la.PositionZ);
				lockObject.RotationX = lockAxes.Any((LockAxes la) => la.RotationX);
				lockObject.RotationY = lockAxes.Any((LockAxes la) => la.RotationY);
				lockObject.RotationZ = lockAxes.Any((LockAxes la) => la.RotationZ);
				lockObject.RotationScreen = lockAxes.Any((LockAxes la) => la.RotationScreen);
				lockObject.ScaleX = lockAxes.Any((LockAxes la) => la.ScaleX);
				lockObject.ScaleY = lockAxes.Any((LockAxes la) => la.ScaleY);
				lockObject.ScaleZ = lockAxes.Any((LockAxes la) => la.ScaleZ);
			}
			return lockObject;
		}
	}
}
