using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentNavigation : PersistentData
	{
		public Navigation.Mode mode;

		public long selectOnUp;

		public long selectOnDown;

		public long selectOnLeft;

		public long selectOnRight;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Navigation navigation = (Navigation)obj;
			navigation.mode = mode;
			navigation.selectOnUp = (Selectable)objects.Get(selectOnUp);
			navigation.selectOnDown = (Selectable)objects.Get(selectOnDown);
			navigation.selectOnLeft = (Selectable)objects.Get(selectOnLeft);
			navigation.selectOnRight = (Selectable)objects.Get(selectOnRight);
			return navigation;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Navigation navigation = (Navigation)obj;
				mode = navigation.mode;
				selectOnUp = navigation.selectOnUp.GetMappedInstanceID();
				selectOnDown = navigation.selectOnDown.GetMappedInstanceID();
				selectOnLeft = navigation.selectOnLeft.GetMappedInstanceID();
				selectOnRight = navigation.selectOnRight.GetMappedInstanceID();
			}
		}
	}
}
