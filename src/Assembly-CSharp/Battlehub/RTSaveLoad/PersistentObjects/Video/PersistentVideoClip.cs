using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects.Video
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentVideoClip : PersistentObject
	{
	}
}
