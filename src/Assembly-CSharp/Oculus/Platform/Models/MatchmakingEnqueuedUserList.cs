using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models
{
	public class MatchmakingEnqueuedUserList : DeserializableList<MatchmakingEnqueuedUser>
	{
		public MatchmakingEnqueuedUserList(IntPtr a)
		{
			int num = (int)(uint)CAPI.ovr_MatchmakingEnqueuedUserArray_GetSize(a);
			_Data = new List<MatchmakingEnqueuedUser>(num);
			for (int i = 0; i < num; i++)
			{
				_Data.Add(new MatchmakingEnqueuedUser(CAPI.ovr_MatchmakingEnqueuedUserArray_GetElement(a, (UIntPtr)(ulong)i)));
			}
		}
	}
}
