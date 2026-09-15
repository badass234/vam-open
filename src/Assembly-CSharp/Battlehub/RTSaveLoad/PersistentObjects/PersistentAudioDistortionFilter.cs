using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAudioDistortionFilter : PersistentBehaviour
	{
		public float distortionLevel;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AudioDistortionFilter audioDistortionFilter = (AudioDistortionFilter)obj;
			audioDistortionFilter.distortionLevel = distortionLevel;
			return audioDistortionFilter;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AudioDistortionFilter audioDistortionFilter = (AudioDistortionFilter)obj;
				distortionLevel = audioDistortionFilter.distortionLevel;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
