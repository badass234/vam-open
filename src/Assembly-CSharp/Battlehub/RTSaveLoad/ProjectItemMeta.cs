using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class ProjectItemMeta
	{
		public int TypeCode;

		public string Name;

		public bool IsExposedFromEditor;

		public PersistentDescriptor Descriptor;

		public AssetBundleDescriptor BundleDescriptor;

		public string TypeName
		{
			get
			{
				if (Descriptor == null)
				{
					return null;
				}
				return Descriptor.TypeName;
			}
		}

		public string BundleName
		{
			get
			{
				if (BundleDescriptor == null)
				{
					return null;
				}
				return BundleDescriptor.BundleName;
			}
		}
	}
}
