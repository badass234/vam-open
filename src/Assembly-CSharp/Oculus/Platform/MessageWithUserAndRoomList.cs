using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithUserAndRoomList : Message<UserAndRoomList>
	{
		public MessageWithUserAndRoomList(IntPtr c_message)
			: base(c_message)
		{
		}

		public override UserAndRoomList GetUserAndRoomList()
		{
			return base.Data;
		}

		protected override UserAndRoomList GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr a = CAPI.ovr_Message_GetUserAndRoomArray(obj);
			return new UserAndRoomList(a);
		}
	}
}
