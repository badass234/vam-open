using System;
using Battlehub.RTSaveLoad.PersistentObjects.EventSystems;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1134, typeof(PersistentPositionAsUV1))]
	[ProtoInclude(1135, typeof(PersistentShadow))]
	public class PersistentBaseMeshEffect : PersistentUIBehaviour
	{
	}
}
