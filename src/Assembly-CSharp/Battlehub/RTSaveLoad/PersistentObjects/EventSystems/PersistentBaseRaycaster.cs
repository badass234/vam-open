using System;
using Battlehub.RTSaveLoad.PersistentObjects.UI;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects.EventSystems
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1126, typeof(PersistentPhysicsRaycaster))]
	[ProtoInclude(1127, typeof(PersistentGraphicRaycaster))]
	public class PersistentBaseRaycaster : PersistentUIBehaviour
	{
	}
}
