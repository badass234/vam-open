using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects.Networking.PlayerConnection
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentPlayerConnection : PersistentScriptableObject
	{
	}
}
