using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public static class Livestreaming
	{
		public static void SetStatusUpdateNotificationCallback(Message<LivestreamingStatus>.Callback callback)
		{
			Callback.SetNotificationCallback(Message.MessageType.Notification_Livestreaming_StatusChange, callback);
		}

		public static Request<LivestreamingStatus> GetStatus()
		{
			if (Core.IsInitialized())
			{
				return new Request<LivestreamingStatus>(CAPI.ovr_Livestreaming_GetStatus());
			}
			return null;
		}

		public static Request<LivestreamingStatus> PauseStream()
		{
			if (Core.IsInitialized())
			{
				return new Request<LivestreamingStatus>(CAPI.ovr_Livestreaming_PauseStream());
			}
			return null;
		}

		public static Request<LivestreamingStatus> ResumeStream()
		{
			if (Core.IsInitialized())
			{
				return new Request<LivestreamingStatus>(CAPI.ovr_Livestreaming_ResumeStream());
			}
			return null;
		}
	}
}
