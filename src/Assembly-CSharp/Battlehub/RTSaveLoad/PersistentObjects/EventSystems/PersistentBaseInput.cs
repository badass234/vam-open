using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTSaveLoad.PersistentObjects.EventSystems
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentBaseInput : PersistentUIBehaviour
	{
		public uint imeCompositionMode;

		public Vector2 compositionCursorPos;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			BaseInput baseInput = (BaseInput)obj;
			baseInput.imeCompositionMode = (IMECompositionMode)imeCompositionMode;
			baseInput.compositionCursorPos = compositionCursorPos;
			return baseInput;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				BaseInput baseInput = (BaseInput)obj;
				imeCompositionMode = (uint)baseInput.imeCompositionMode;
				compositionCursorPos = baseInput.compositionCursorPos;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
