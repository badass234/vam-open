namespace LeapInternal
{
	public enum eLeapPolicyFlag : uint
	{
		eLeapPolicyFlag_BackgroundFrames = 1u,
		eLeapPolicyFlag_Images = 2u,
		eLeapPolicyFlag_OptimizeHMD = 4u,
		eLeapPolicyFlag_AllowPauseResume = 8u,
		eLeapPolicyFlag_MapPoints = 0x80u
	}
}
