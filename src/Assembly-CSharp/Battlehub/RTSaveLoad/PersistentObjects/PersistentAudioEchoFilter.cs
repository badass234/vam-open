using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAudioEchoFilter : PersistentBehaviour
	{
		public float delay;

		public float decayRatio;

		public float dryMix;

		public float wetMix;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AudioEchoFilter audioEchoFilter = (AudioEchoFilter)obj;
			audioEchoFilter.delay = delay;
			audioEchoFilter.decayRatio = decayRatio;
			audioEchoFilter.dryMix = dryMix;
			audioEchoFilter.wetMix = wetMix;
			return audioEchoFilter;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AudioEchoFilter audioEchoFilter = (AudioEchoFilter)obj;
				delay = audioEchoFilter.delay;
				decayRatio = audioEchoFilter.decayRatio;
				dryMix = audioEchoFilter.dryMix;
				wetMix = audioEchoFilter.wetMix;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
