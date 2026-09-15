using System;
using UnityEngine;

namespace Leap.Unity
{
	[Serializable]
	public class AssetFolder
	{
		[SerializeField]
		protected UnityEngine.Object _assetFolder;

		public virtual string Path
		{
			get
			{
				throw new InvalidOperationException("Cannot access the Path of an Asset Folder in a build.");
			}
			set
			{
				throw new InvalidOperationException("Cannot set the Path of an Asset Folder in a build.");
			}
		}

		public AssetFolder()
		{
		}

		public AssetFolder(string path)
		{
			Path = path;
		}
	}
}
