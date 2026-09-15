using System;

namespace Leap.Unity.Attachments
{
	[Flags]
	public enum AttachmentPointFlags
	{
		None = 0,
		Wrist = 2,
		Palm = 4,
		ThumbProximalJoint = 8,
		ThumbDistalJoint = 0x10,
		ThumbTip = 0x20,
		IndexKnuckle = 0x40,
		IndexMiddleJoint = 0x80,
		IndexDistalJoint = 0x100,
		IndexTip = 0x200,
		MiddleKnuckle = 0x400,
		MiddleMiddleJoint = 0x800,
		MiddleDistalJoint = 0x1000,
		MiddleTip = 0x2000,
		RingKnuckle = 0x4000,
		RingMiddleJoint = 0x8000,
		RingDistalJoint = 0x10000,
		RingTip = 0x20000,
		PinkyKnuckle = 0x40000,
		PinkyMiddleJoint = 0x80000,
		PinkyDistalJoint = 0x100000,
		PinkyTip = 0x200000
	}
}
