using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects.EventSystems
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1125, typeof(PersistentStandaloneInputModule))]
	public class PersistentPointerInputModule : PersistentBaseInputModule
	{
	}
}
