using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentKeyframe : PersistentData
	{
		public float time;

		public float value;

		public float inTangent;

		public float outTangent;

		public int tangentMode;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Keyframe keyframe = (Keyframe)obj;
			keyframe.time = time;
			keyframe.value = value;
			keyframe.inTangent = inTangent;
			keyframe.outTangent = outTangent;
			return keyframe;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Keyframe keyframe = (Keyframe)obj;
				time = keyframe.time;
				value = keyframe.value;
				inTangent = keyframe.inTangent;
				outTangent = keyframe.outTangent;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
