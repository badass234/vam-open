using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentGUIStyleState : PersistentData
	{
		public long background;

		public long[] scaledBackgrounds;

		public Color textColor;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			GUIStyleState gUIStyleState = (GUIStyleState)obj;
			gUIStyleState.background = (Texture2D)objects.Get(background);
			gUIStyleState.textColor = textColor;
			return gUIStyleState;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				GUIStyleState gUIStyleState = (GUIStyleState)obj;
				background = gUIStyleState.background.GetMappedInstanceID();
				textColor = gUIStyleState.textColor;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(background, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				GUIStyleState gUIStyleState = (GUIStyleState)obj;
				AddDependency(gUIStyleState.background, dependencies);
			}
		}
	}
}
