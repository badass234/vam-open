using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects.EventSystems
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentPhysics2DRaycaster : PersistentPhysicsRaycaster
	{
	}
}
