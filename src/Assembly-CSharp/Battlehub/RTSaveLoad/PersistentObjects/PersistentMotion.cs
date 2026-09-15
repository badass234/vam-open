using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1107, typeof(PersistentAnimationClip))]
	public class PersistentMotion : PersistentObject
	{
	}
}
