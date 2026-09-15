using System;
using ProtoBuf;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class AssetBundleDescriptor
	{
		public string BundleName;

		public string AssetName;

		public string TypeName;
	}
}
