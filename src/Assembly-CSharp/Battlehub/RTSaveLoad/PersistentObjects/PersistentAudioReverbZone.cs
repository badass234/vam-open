using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAudioReverbZone : PersistentBehaviour
	{
		public float minDistance;

		public float maxDistance;

		public uint reverbPreset;

		public int room;

		public int roomHF;

		public int roomLF;

		public float decayTime;

		public float decayHFRatio;

		public int reflections;

		public float reflectionsDelay;

		public int reverb;

		public float reverbDelay;

		public float HFReference;

		public float LFReference;

		public float diffusion;

		public float density;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AudioReverbZone audioReverbZone = (AudioReverbZone)obj;
			audioReverbZone.minDistance = minDistance;
			audioReverbZone.maxDistance = maxDistance;
			audioReverbZone.reverbPreset = (AudioReverbPreset)reverbPreset;
			audioReverbZone.room = room;
			audioReverbZone.roomHF = roomHF;
			audioReverbZone.roomLF = roomLF;
			audioReverbZone.decayTime = decayTime;
			audioReverbZone.decayHFRatio = decayHFRatio;
			audioReverbZone.reflections = reflections;
			audioReverbZone.reflectionsDelay = reflectionsDelay;
			audioReverbZone.reverb = reverb;
			audioReverbZone.reverbDelay = reverbDelay;
			audioReverbZone.HFReference = HFReference;
			audioReverbZone.LFReference = LFReference;
			audioReverbZone.diffusion = diffusion;
			audioReverbZone.density = density;
			return audioReverbZone;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AudioReverbZone audioReverbZone = (AudioReverbZone)obj;
				minDistance = audioReverbZone.minDistance;
				maxDistance = audioReverbZone.maxDistance;
				reverbPreset = (uint)audioReverbZone.reverbPreset;
				room = audioReverbZone.room;
				roomHF = audioReverbZone.roomHF;
				roomLF = audioReverbZone.roomLF;
				decayTime = audioReverbZone.decayTime;
				decayHFRatio = audioReverbZone.decayHFRatio;
				reflections = audioReverbZone.reflections;
				reflectionsDelay = audioReverbZone.reflectionsDelay;
				reverb = audioReverbZone.reverb;
				reverbDelay = audioReverbZone.reverbDelay;
				HFReference = audioReverbZone.HFReference;
				LFReference = audioReverbZone.LFReference;
				diffusion = audioReverbZone.diffusion;
				density = audioReverbZone.density;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
