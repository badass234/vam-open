using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAudioHighPassFilter : PersistentBehaviour
	{
		public float cutoffFrequency;

		public float highpassResonanceQ;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AudioHighPassFilter audioHighPassFilter = (AudioHighPassFilter)obj;
			audioHighPassFilter.cutoffFrequency = cutoffFrequency;
			audioHighPassFilter.highpassResonanceQ = highpassResonanceQ;
			return audioHighPassFilter;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AudioHighPassFilter audioHighPassFilter = (AudioHighPassFilter)obj;
				cutoffFrequency = audioHighPassFilter.cutoffFrequency;
				highpassResonanceQ = audioHighPassFilter.highpassResonanceQ;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
