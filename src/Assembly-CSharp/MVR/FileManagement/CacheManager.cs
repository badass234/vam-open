using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace MVR.FileManagement
{
	public class CacheManager : MonoBehaviour
	{
		public delegate void CacheSizeCallback(float f);

		public static CacheManager singleton;

		protected static string textureFolderName = "Textures";

		protected static string packageJSONFolderName = "PackageJSON";

		protected static string videoFolderName = "Videos";

		public string defaultCacheDir = "Cache";

		protected string _cacheDir;

		public CacheSizeUI cacheSizeUI;

		protected List<CacheSizeCallback> callbacks;

		protected float cacheSize;

		protected float cacheSizeThreaded;

		protected Thread cacheSizeRecalcThread;

		protected Coroutine cacheSizeCoroutine;

		public static bool CachingEnabled { get; set; }

		public string CacheDir
		{
			get
			{
				return _cacheDir;
			}
			set
			{
				if (_cacheDir != value)
				{
					ClearCache();
					_cacheDir = value;
					InitCache();
				}
			}
		}

		public static string GetCacheDir()
		{
			if (singleton != null)
			{
				return singleton.CacheDir;
			}
			return null;
		}

		public static string GetTextureCacheDir()
		{
			if (singleton != null)
			{
				return singleton.CacheDir + "/" + textureFolderName;
			}
			return null;
		}

		public static string GetPackageJSONCacheDir()
		{
			if (singleton != null)
			{
				return singleton.CacheDir + "/" + packageJSONFolderName;
			}
			return null;
		}

		public static string GetVideoCacheDir()
		{
			if (singleton != null)
			{
				return singleton.CacheDir + "/" + videoFolderName;
			}
			return null;
		}

		public static void SetCacheDir(string dir)
		{
			if (singleton != null)
			{
				singleton.CacheDir = dir;
			}
		}

		public static void ResetCacheDir()
		{
			if (singleton != null)
			{
				singleton.CacheDir = singleton.defaultCacheDir;
			}
		}

		protected void GetCacheSizeThreaded()
		{
			long num = 0L;
			DirectoryInfo directoryInfo = new DirectoryInfo(CacheDir);
			FileInfo[] files = directoryInfo.GetFiles("*.vamcache", SearchOption.AllDirectories);
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				num += fileInfo.Length;
			}
			files = directoryInfo.GetFiles("*.vamcachemeta", SearchOption.AllDirectories);
			FileInfo[] array2 = files;
			foreach (FileInfo fileInfo2 in array2)
			{
				num += fileInfo2.Length;
			}
			files = directoryInfo.GetFiles("*.vamcachejson", SearchOption.AllDirectories);
			FileInfo[] array3 = files;
			foreach (FileInfo fileInfo3 in array3)
			{
				num += fileInfo3.Length;
			}
			cacheSizeThreaded = (float)num / 1E+09f;
		}

		private IEnumerator GetCacheSizeCo()
		{
			if (cacheSizeRecalcThread == null)
			{
				cacheSizeRecalcThread = new Thread((ThreadStart)delegate
				{
					GetCacheSizeThreaded();
				});
				cacheSizeRecalcThread.Start();
			}
			while (cacheSizeRecalcThread.IsAlive)
			{
				yield return null;
			}
			cacheSize = cacheSizeThreaded;
			cacheSizeRecalcThread = null;
			cacheSizeCoroutine = null;
			foreach (CacheSizeCallback callback in callbacks)
			{
				callback?.Invoke(cacheSize);
			}
			callbacks.Clear();
		}

		public void GetCacheSize(CacheSizeCallback callback)
		{
			callbacks.Add(callback);
			if (cacheSizeCoroutine == null)
			{
				cacheSizeCoroutine = StartCoroutine(GetCacheSizeCo());
			}
		}

		protected void InitCache()
		{
			try
			{
				if (!Directory.Exists(CacheDir))
				{
					Directory.CreateDirectory(CacheDir);
				}
				string path = CacheDir + "/" + textureFolderName;
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				string path2 = CacheDir + "/" + packageJSONFolderName;
				if (!Directory.Exists(path2))
				{
					Directory.CreateDirectory(path2);
				}
				string path3 = CacheDir + "/" + videoFolderName;
				if (!Directory.Exists(path3))
				{
					Directory.CreateDirectory(path3);
				}
			}
			catch (Exception ex)
			{
				SuperController.LogError("Failed to init cache " + ex);
			}
		}

		public void ClearCache()
		{
			if (!Directory.Exists(CacheDir))
			{
				return;
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(CacheDir);
			FileInfo[] files = directoryInfo.GetFiles("*.vamcache", SearchOption.AllDirectories);
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				try
				{
					fileInfo.Delete();
				}
				catch (Exception)
				{
				}
			}
			files = directoryInfo.GetFiles("*.vamcachemeta", SearchOption.AllDirectories);
			FileInfo[] array2 = files;
			foreach (FileInfo fileInfo2 in array2)
			{
				try
				{
					fileInfo2.Delete();
				}
				catch (Exception)
				{
				}
			}
			files = directoryInfo.GetFiles("*.vamcachejson", SearchOption.AllDirectories);
			FileInfo[] array3 = files;
			foreach (FileInfo fileInfo3 in array3)
			{
				try
				{
					fileInfo3.Delete();
				}
				catch (Exception)
				{
				}
			}
			files = directoryInfo.GetFiles("*.avi", SearchOption.AllDirectories);
			FileInfo[] array4 = files;
			foreach (FileInfo fileInfo4 in array4)
			{
				try
				{
					fileInfo4.Delete();
				}
				catch (Exception)
				{
				}
			}
			files = directoryInfo.GetFiles("*.mp4", SearchOption.AllDirectories);
			FileInfo[] array5 = files;
			foreach (FileInfo fileInfo5 in array5)
			{
				try
				{
					fileInfo5.Delete();
				}
				catch (Exception)
				{
				}
			}
			if (cacheSizeUI != null)
			{
				cacheSizeUI.UpdateUsed();
			}
		}

		private void Awake()
		{
			singleton = this;
			callbacks = new List<CacheSizeCallback>();
			if (CacheDir == null)
			{
				CacheDir = defaultCacheDir;
				InitCache();
			}
		}

		private void OnDisable()
		{
			if (cacheSizeCoroutine != null)
			{
				StopCoroutine(cacheSizeCoroutine);
				cacheSizeCoroutine = null;
				cacheSizeRecalcThread = null;
			}
		}

		private void OnDestroy()
		{
			CachingEnabled = false;
			_cacheDir = null;
			singleton = null;
		}
	}
}
