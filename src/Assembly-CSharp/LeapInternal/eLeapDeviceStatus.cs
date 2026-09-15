namespace LeapInternal
{
	public enum eLeapDeviceStatus : uint
	{
		eLeapDeviceStatus_Streaming = 1u,
		eLeapDeviceStatus_Paused = 2u,
		eLeapDeviceStatus_Robust = 4u,
		eLeapDeviceStatus_Smudged = 8u,
		eLeapDeviceStatus_LowResource = 16u,
		eLeapDeviceStatus_UnknownFailure = 3892379648u,
		eLeapDeviceStatus_BadCalibration = 3892379649u,
		eLeapDeviceStatus_BadFirmware = 3892379650u,
		eLeapDeviceStatus_BadTransport = 3892379651u,
		eLeapDeviceStatus_BadControl = 3892379652u
	}
}
