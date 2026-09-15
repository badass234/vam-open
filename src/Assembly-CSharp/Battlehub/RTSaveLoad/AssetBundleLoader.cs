using System.IO;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public class AssetBundleLoader : IAssetBundleLoader
	{
		public void Load(string name, AssetBundleEventHandler callback)
		{
			if (!File.Exists(Application.streamingAssetsPath + "/" + name))
			{
				callback(name, null);
				return;
			}
			AssetBundle bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + name);
			callback?.Invoke(name, bundle);
		}
	}
}
