using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1106, typeof(PersistentAnimatorOverrideController))]
	public class PersistentRuntimeAnimatorController : PersistentObject
	{
	}
}
