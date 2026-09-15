using System;
using Battlehub.RTSaveLoad.PersistentObjects.EventSystems;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentRectMask2D : PersistentUIBehaviour
	{
	}
}
