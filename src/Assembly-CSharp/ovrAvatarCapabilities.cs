using System;

[Flags]
public enum ovrAvatarCapabilities
{
	Body = 1,
	Hands = 2,
	Base = 4,
	BodyTilt = 0x10,
	All = -1
}
