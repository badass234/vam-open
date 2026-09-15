using System;
using System.Collections.Generic;
using System.Linq;
using Battlehub.RTSaveLoad.PersistentObjects;
using ProtoBuf;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class ProjectItemData
	{
		public PersistentData[] PersistentData;

		public byte[] RawData;

		public void Rename(ProjectItemMeta meta, string name)
		{
			if (PersistentData == null || PersistentData.Length == 0)
			{
				return;
			}
			if (PersistentData.Length > 1)
			{
				Dictionary<long, PersistentData> dictionary = PersistentData.ToDictionary((PersistentData d) => d.InstanceId);
				PersistentData value;
				if (!dictionary.TryGetValue(meta.Descriptor.InstanceId, out value))
				{
					return;
				}
				PersistentObject asPersistentObject = value.AsPersistentObject;
				asPersistentObject.name = name;
				if (meta.Descriptor.Components == null)
				{
					return;
				}
				for (int i = 0; i < meta.Descriptor.Components.Length; i++)
				{
					if (dictionary.TryGetValue(meta.Descriptor.Components[i].InstanceId, out value))
					{
						asPersistentObject = value.AsPersistentObject;
						asPersistentObject.name = name;
					}
				}
			}
			else
			{
				PersistentObject asPersistentObject2 = PersistentData[0].AsPersistentObject;
				asPersistentObject2.name = name;
			}
		}
	}
}
