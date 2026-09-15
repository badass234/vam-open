using System;
using Battlehub.RTSaveLoad.PersistentObjects.Experimental.Rendering;
using Battlehub.RTSaveLoad.PersistentObjects.Networking.PlayerConnection;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1000, typeof(PersistentStateMachineBehaviour))]
	[ProtoInclude(1001, typeof(PersistentGUISkin))]
	[ProtoInclude(1002, typeof(PersistentPlayerConnection))]
	[ProtoInclude(1003, typeof(PersistentRenderPipelineAsset))]
	public class PersistentScriptableObject : PersistentObject
	{
	}
}
