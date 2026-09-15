using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class IntArray
	{
		public int[] Array;
	}
}
