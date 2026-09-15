using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithPartyID : Message<PartyID>
	{
		public MessageWithPartyID(IntPtr c_message)
			: base(c_message)
		{
		}

		public override PartyID GetPartyID()
		{
			return base.Data;
		}

		protected override PartyID GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetPartyID(obj);
			return new PartyID(o);
		}
	}
}
