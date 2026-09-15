using System.Runtime.InteropServices;

public struct ovrAvatarHandInputState
{
	public ovrAvatarTransform transform;

	public ovrAvatarButton buttonMask;

	public ovrAvatarTouch touchMask;

	public float joystickX;

	public float joystickY;

	public float indexTrigger;

	public float handTrigger;

	[MarshalAs(UnmanagedType.I1)]
	public bool isActive;
}
