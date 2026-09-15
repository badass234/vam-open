using System;

[Flags]
public enum ovrAvatarTouch
{
	One = 1,
	Two = 2,
	Joystick = 4,
	ThumbRest = 8,
	Index = 0x10,
	Pointing = 0x40,
	ThumbUp = 0x80
}
