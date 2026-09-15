using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAudioChorusFilter : PersistentBehaviour
	{
		public float dryMix;

		public float wetMix1;

		public float wetMix2;

		public float wetMix3;

		public float delay;

		public float rate;

		public float depth;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AudioChorusFilter audioChorusFilter = (AudioChorusFilter)obj;
			audioChorusFilter.dryMix = dryMix;
			audioChorusFilter.wetMix1 = wetMix1;
			audioChorusFilter.wetMix2 = wetMix2;
			audioChorusFilter.wetMix3 = wetMix3;
			audioChorusFilter.delay = delay;
			audioChorusFilter.rate = rate;
			audioChorusFilter.depth = depth;
			return audioChorusFilter;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AudioChorusFilter audioChorusFilter = (AudioChorusFilter)obj;
				dryMix = audioChorusFilter.dryMix;
				wetMix1 = audioChorusFilter.wetMix1;
				wetMix2 = audioChorusFilter.wetMix2;
				wetMix3 = audioChorusFilter.wetMix3;
				delay = audioChorusFilter.delay;
				rate = audioChorusFilter.rate;
				depth = audioChorusFilter.depth;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
