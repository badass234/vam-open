using System;
using System.IO;
using UnityEngine;

namespace Leap.Unity
{
	[Serializable]
	public class StreamingFolder : AssetFolder, ISerializationCallbackReceiver
	{
		[SerializeField]
		private string _relativePath;

		public override string Path
		{
			get
			{
				return System.IO.Path.Combine(Application.streamingAssetsPath, _relativePath);
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public void OnAfterDeserialize()
		{
		}

		public void OnBeforeSerialize()
		{
		}
	}
}
