using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models
{
	public class UserAndRoomList : DeserializableList<UserAndRoom>
	{
		public UserAndRoomList(IntPtr a)
		{
			int num = (int)(uint)CAPI.ovr_UserAndRoomArray_GetSize(a);
			_Data = new List<UserAndRoom>(num);
			for (int i = 0; i < num; i++)
			{
				_Data.Add(new UserAndRoom(CAPI.ovr_UserAndRoomArray_GetElement(a, (UIntPtr)(ulong)i)));
			}
			_NextUrl = CAPI.ovr_UserAndRoomArray_GetNextUrl(a);
		}
	}
}
