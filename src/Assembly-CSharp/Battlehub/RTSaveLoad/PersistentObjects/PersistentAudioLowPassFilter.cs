using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAudioLowPassFilter : PersistentBehaviour
	{
		public float cutoffFrequency;

		public PersistentAnimationCurve customCutoffCurve;

		public float lowpassResonanceQ;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AudioLowPassFilter audioLowPassFilter = (AudioLowPassFilter)obj;
			audioLowPassFilter.cutoffFrequency = cutoffFrequency;
			audioLowPassFilter.customCutoffCurve = Write(audioLowPassFilter.customCutoffCurve, customCutoffCurve, objects);
			audioLowPassFilter.lowpassResonanceQ = lowpassResonanceQ;
			return audioLowPassFilter;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AudioLowPassFilter audioLowPassFilter = (AudioLowPassFilter)obj;
				cutoffFrequency = audioLowPassFilter.cutoffFrequency;
				customCutoffCurve = Read(customCutoffCurve, audioLowPassFilter.customCutoffCurve);
				lowpassResonanceQ = audioLowPassFilter.lowpassResonanceQ;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(customCutoffCurve, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				AudioLowPassFilter audioLowPassFilter = (AudioLowPassFilter)obj;
				GetDependencies(customCutoffCurve, audioLowPassFilter.customCutoffCurve, dependencies);
			}
		}
	}
}
