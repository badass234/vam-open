using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1100, typeof(PersistentConstantForce2D))]
	public class PersistentPhysicsUpdateBehaviour2D : PersistentBehaviour
	{
	}
}
