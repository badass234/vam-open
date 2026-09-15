using UnityEngine;
using UnityEngine.UI;

namespace MVR.FileManagement
{
	public class CacheSizeUI : MonoBehaviour
	{
		public CacheManager cacheManager;

		public Text text;

		protected void CacheSizeCallback(float f)
		{
			if (this.text != null)
			{
				string text = f.ToString("F2") + "GB";
				this.text.text = text;
			}
		}

		public void UpdateUsed()
		{
			if (cacheManager != null && text != null)
			{
				cacheManager.GetCacheSize(CacheSizeCallback);
			}
		}

		protected void OnEnable()
		{
			UpdateUsed();
		}
	}
}
