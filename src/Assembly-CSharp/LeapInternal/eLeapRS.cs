namespace LeapInternal
{
	public enum eLeapRS : uint
	{
		eLeapRS_Success = 0u,
		eLeapRS_UnknownError = 3791716352u,
		eLeapRS_InvalidArgument = 3791716353u,
		eLeapRS_InsufficientResources = 3791716354u,
		eLeapRS_InsufficientBuffer = 3791716355u,
		eLeapRS_Timeout = 3791716356u,
		eLeapRS_NotConnected = 3791716357u,
		eLeapRS_HandshakeIncomplete = 3791716358u,
		eLeapRS_BufferSizeOverflow = 3791716359u,
		eLeapRS_ProtocolError = 3791716360u,
		eLeapRS_InvalidClientID = 3791716361u,
		eLeapRS_UnexpectedClosed = 3791716362u,
		eLeapRS_UnknownImageFrameRequest = 3791716363u,
		eLeapRS_UnknownTrackingFrameID = 3791716364u,
		eLeapRS_RoutineIsNotSeer = 3791716365u,
		eLeapRS_TimestampTooEarly = 3791716366u,
		eLeapRS_ConcurrentPoll = 3791716367u,
		eLeapRS_NotAvailable = 3875602434u,
		eLeapRS_NotStreaming = 3875602436u,
		eLeapRS_CannotOpenDevice = 3875602437u
	}
}
