using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad.PersistentObjects.Audio
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAudioMixerGroup : PersistentObject
	{
	}
}
