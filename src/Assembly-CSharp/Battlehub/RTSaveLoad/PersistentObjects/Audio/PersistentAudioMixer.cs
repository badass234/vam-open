using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Audio;

namespace Battlehub.RTSaveLoad.PersistentObjects.Audio
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAudioMixer : PersistentObject
	{
		public long outputAudioMixerGroup;

		public uint updateMode;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AudioMixer audioMixer = (AudioMixer)obj;
			audioMixer.outputAudioMixerGroup = (AudioMixerGroup)objects.Get(outputAudioMixerGroup);
			audioMixer.updateMode = (AudioMixerUpdateMode)updateMode;
			return audioMixer;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AudioMixer audioMixer = (AudioMixer)obj;
				outputAudioMixerGroup = audioMixer.outputAudioMixerGroup.GetMappedInstanceID();
				updateMode = (uint)audioMixer.updateMode;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(outputAudioMixerGroup, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				AudioMixer audioMixer = (AudioMixer)obj;
				AddDependency(audioMixer.outputAudioMixerGroup, dependencies);
			}
		}
	}
}
