using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Audio;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAudioSource : PersistentBehaviour
	{
		public float volume;

		public float pitch;

		public float time;

		public int timeSamples;

		public long clip;

		public long outputAudioMixerGroup;

		public bool loop;

		public bool ignoreListenerVolume;

		public bool playOnAwake;

		public bool ignoreListenerPause;

		public uint velocityUpdateMode;

		public float panStereo;

		public float spatialBlend;

		public bool spatialize;

		public bool spatializePostEffects;

		public float reverbZoneMix;

		public bool bypassEffects;

		public bool bypassListenerEffects;

		public bool bypassReverbZones;

		public float dopplerLevel;

		public float spread;

		public int priority;

		public bool mute;

		public float minDistance;

		public float maxDistance;

		public uint rolloffMode;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AudioSource audioSource = (AudioSource)obj;
			audioSource.volume = volume;
			audioSource.pitch = pitch;
			audioSource.time = time;
			audioSource.timeSamples = timeSamples;
			audioSource.clip = (AudioClip)objects.Get(clip);
			audioSource.outputAudioMixerGroup = (AudioMixerGroup)objects.Get(outputAudioMixerGroup);
			audioSource.loop = loop;
			audioSource.ignoreListenerVolume = ignoreListenerVolume;
			audioSource.playOnAwake = playOnAwake;
			audioSource.ignoreListenerPause = ignoreListenerPause;
			audioSource.velocityUpdateMode = (AudioVelocityUpdateMode)velocityUpdateMode;
			audioSource.panStereo = panStereo;
			audioSource.spatialBlend = spatialBlend;
			audioSource.spatialize = spatialize;
			audioSource.spatializePostEffects = spatializePostEffects;
			audioSource.reverbZoneMix = reverbZoneMix;
			audioSource.bypassEffects = bypassEffects;
			audioSource.bypassListenerEffects = bypassListenerEffects;
			audioSource.bypassReverbZones = bypassReverbZones;
			audioSource.dopplerLevel = dopplerLevel;
			audioSource.spread = spread;
			audioSource.priority = priority;
			audioSource.mute = mute;
			audioSource.minDistance = minDistance;
			audioSource.maxDistance = maxDistance;
			audioSource.rolloffMode = (AudioRolloffMode)rolloffMode;
			return audioSource;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AudioSource audioSource = (AudioSource)obj;
				volume = audioSource.volume;
				pitch = audioSource.pitch;
				time = audioSource.time;
				timeSamples = audioSource.timeSamples;
				clip = audioSource.clip.GetMappedInstanceID();
				outputAudioMixerGroup = audioSource.outputAudioMixerGroup.GetMappedInstanceID();
				loop = audioSource.loop;
				ignoreListenerVolume = audioSource.ignoreListenerVolume;
				playOnAwake = audioSource.playOnAwake;
				ignoreListenerPause = audioSource.ignoreListenerPause;
				velocityUpdateMode = (uint)audioSource.velocityUpdateMode;
				panStereo = audioSource.panStereo;
				spatialBlend = audioSource.spatialBlend;
				spatialize = audioSource.spatialize;
				spatializePostEffects = audioSource.spatializePostEffects;
				reverbZoneMix = audioSource.reverbZoneMix;
				bypassEffects = audioSource.bypassEffects;
				bypassListenerEffects = audioSource.bypassListenerEffects;
				bypassReverbZones = audioSource.bypassReverbZones;
				dopplerLevel = audioSource.dopplerLevel;
				spread = audioSource.spread;
				priority = audioSource.priority;
				mute = audioSource.mute;
				minDistance = audioSource.minDistance;
				maxDistance = audioSource.maxDistance;
				rolloffMode = (uint)audioSource.rolloffMode;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(clip, dependencies, objects, allowNulls);
			AddDependency(outputAudioMixerGroup, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				AudioSource audioSource = (AudioSource)obj;
				AddDependency(audioSource.clip, dependencies);
				AddDependency(audioSource.outputAudioMixerGroup, dependencies);
			}
		}
	}
}
