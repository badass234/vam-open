using System;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{
	internal class DestroyDetector : MonoBehaviour
	{
		public event Action onDestroy = delegate
		{
		};

		public void OnDestroy()
		{
			this.onDestroy();
		}
	}
}
