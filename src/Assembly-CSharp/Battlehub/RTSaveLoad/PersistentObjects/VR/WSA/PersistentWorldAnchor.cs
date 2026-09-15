using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects.VR.WSA
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentWorldAnchor : PersistentComponent
	{
	}
}
