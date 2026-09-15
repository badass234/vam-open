using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class ProjectMeta
	{
		public int Counter;
	}
}
