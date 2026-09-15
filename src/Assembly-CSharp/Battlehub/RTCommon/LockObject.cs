namespace Battlehub.RTCommon
{
	public class LockObject
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

		public bool IsPositionLocked => PositionX && PositionY && PositionZ;

		public bool IsRotationLocked => RotationX && RotationY && RotationZ && RotationScreen;

		public bool IsScaleLocked => ScaleX && ScaleY && ScaleZ;
	}
}
