using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAudioReverbFilter : PersistentBehaviour
	{
		public uint reverbPreset;

		public float dryLevel;

		public float room;

		public float roomHF;

		public float decayTime;

		public float decayHFRatio;

		public float reflectionsLevel;

		public float reflectionsDelay;

		public float reverbLevel;

		public float reverbDelay;

		public float diffusion;

		public float density;

		public float hfReference;

		public float roomLF;

		public float lfReference;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AudioReverbFilter audioReverbFilter = (AudioReverbFilter)obj;
			audioReverbFilter.reverbPreset = (AudioReverbPreset)reverbPreset;
			audioReverbFilter.dryLevel = dryLevel;
			audioReverbFilter.room = room;
			audioReverbFilter.roomHF = roomHF;
			audioReverbFilter.decayTime = decayTime;
			audioReverbFilter.decayHFRatio = decayHFRatio;
			audioReverbFilter.reflectionsLevel = reflectionsLevel;
			audioReverbFilter.reflectionsDelay = reflectionsDelay;
			audioReverbFilter.reverbLevel = reverbLevel;
			audioReverbFilter.reverbDelay = reverbDelay;
			audioReverbFilter.diffusion = diffusion;
			audioReverbFilter.density = density;
			audioReverbFilter.hfReference = hfReference;
			audioReverbFilter.roomLF = roomLF;
			audioReverbFilter.lfReference = lfReference;
			return audioReverbFilter;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AudioReverbFilter audioReverbFilter = (AudioReverbFilter)obj;
				reverbPreset = (uint)audioReverbFilter.reverbPreset;
				dryLevel = audioReverbFilter.dryLevel;
				room = audioReverbFilter.room;
				roomHF = audioReverbFilter.roomHF;
				decayTime = audioReverbFilter.decayTime;
				decayHFRatio = audioReverbFilter.decayHFRatio;
				reflectionsLevel = audioReverbFilter.reflectionsLevel;
				reflectionsDelay = audioReverbFilter.reflectionsDelay;
				reverbLevel = audioReverbFilter.reverbLevel;
				reverbDelay = audioReverbFilter.reverbDelay;
				diffusion = audioReverbFilter.diffusion;
				density = audioReverbFilter.density;
				hfReference = audioReverbFilter.hfReference;
				roomLF = audioReverbFilter.roomLF;
				lfReference = audioReverbFilter.lfReference;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
