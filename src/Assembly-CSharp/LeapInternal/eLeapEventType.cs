namespace LeapInternal
{
	public enum eLeapEventType
	{
		eLeapEventType_None = 0,
		eLeapEventType_Connection = 1,
		eLeapEventType_ConnectionLost = 2,
		eLeapEventType_Device = 3,
		eLeapEventType_DeviceFailure = 4,
		eLeapEventType_Policy = 5,
		eLeapEventType_Tracking = 256,
		eLeapEventType_ImageRequestError = 257,
		eLeapEventType_ImageComplete = 258,
		eLeapEventType_LogEvent = 259,
		eLeapEventType_DeviceLost = 260,
		eLeapEventType_ConfigResponse = 261,
		eLeapEventType_ConfigChange = 262,
		eLeapEventType_DeviceStatusChange = 263,
		eLeapEventType_DroppedFrame = 264,
		eLeapEventType_Image = 265,
		eLeapEventType_PointMappingChange = 266,
		eLeapEventType_LogEvents = 267,
		eLeapEventType_HeadPose = 268
	}
}
