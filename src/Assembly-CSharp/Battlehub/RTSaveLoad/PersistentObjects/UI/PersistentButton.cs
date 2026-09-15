using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentButton : PersistentSelectable
	{
		public PersistentUnityEventBase onClick;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Button button = (Button)obj;
			Write(button.onClick, onClick, objects);
			return button;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Button button = (Button)obj;
				Read(onClick, button.onClick);
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			if (onClick != null)
			{
				onClick.FindDependencies(dependencies, objects, allowNulls);
			}
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			Button button = (Button)obj;
			if (!(button == null))
			{
				PersistentUnityEventBase persistentUnityEventBase = new PersistentUnityEventBase();
				persistentUnityEventBase.GetDependencies(button.onClick, dependencies);
			}
		}
	}
}
