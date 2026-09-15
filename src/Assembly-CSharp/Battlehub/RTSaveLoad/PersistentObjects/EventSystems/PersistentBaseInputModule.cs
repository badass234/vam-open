using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects.EventSystems
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1124, typeof(PersistentPointerInputModule))]
	public class PersistentBaseInputModule : PersistentUIBehaviour
	{
	}
}
