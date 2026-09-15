using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models
{
	public class MatchmakingEnqueuedUser
	{
		public readonly Dictionary<string, string> CustomData;

		public readonly User UserOptional;

		[Obsolete("Deprecated in favor of UserOptional")]
		public readonly User User;

		public MatchmakingEnqueuedUser(IntPtr o)
		{
			CustomData = CAPI.DataStoreFromNative(CAPI.ovr_MatchmakingEnqueuedUser_GetCustomData(o));
			IntPtr intPtr = CAPI.ovr_MatchmakingEnqueuedUser_GetUser(o);
			User = new User(intPtr);
			if (intPtr == IntPtr.Zero)
			{
				UserOptional = null;
			}
			else
			{
				UserOptional = User;
			}
		}
	}
}
