using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects.Experimental.Rendering
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentRenderPipelineAsset : PersistentScriptableObject
	{
	}
}
