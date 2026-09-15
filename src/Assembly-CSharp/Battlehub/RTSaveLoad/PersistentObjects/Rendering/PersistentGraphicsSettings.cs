using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects.Rendering
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentGraphicsSettings : PersistentObject
	{
	}
}
