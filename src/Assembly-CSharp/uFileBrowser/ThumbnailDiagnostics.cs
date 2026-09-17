using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using UnityEngine;

namespace uFileBrowser
{
	internal static class ThumbnailDiagnostics
	{
		private const int MaxMessages = 60;

		private const int MaxInspections = 200;

		private static readonly HashSet<string> reported = new HashSet<string>();

		private static int inspected;

		private static void Report(string message)
		{
			if (reported.Count >= MaxMessages || !reported.Add(message))
			{
				return;
			}
			Debug.LogError("VaMOpen thumbnail: " + message);
		}

		internal static void ReportLoaderFailure(string imgPath, string errorText)
		{
			Report("image loader failed for " + imgPath + ": " + errorText);
		}

		internal static void DiagnoseFileButton(FileButton fb, string path)
		{
			if (fb == null || inspected >= MaxInspections)
			{
				return;
			}
			inspected++;
			if (fb.imgPath != null)
			{
				return;
			}
			if (ImageLoaderThreaded.singleton == null)
			{
				Report("ImageLoaderThreaded.singleton is null, so no thumbnail is ever requested");
				return;
			}
			if (fb.altIcon == null)
			{
				Report("the file button prefab has no altIcon, so thumbnails are never shown");
				return;
			}
			if (fb.fileIcon == null)
			{
				Report("the file button prefab has no fileIcon, so thumbnails are never shown");
				return;
			}
			FileEntry fileEntry = FileManager.GetFileEntry(path);
			if (fileEntry == null)
			{
				Report("no file entry for " + path);
				return;
			}
			string thumbnailPath;
			switch (Path.GetExtension(fileEntry.Path))
			{
			case ".duf":
				thumbnailPath = fileEntry.Path + ".png";
				break;
			case ".json":
			case ".vac":
			case ".vap":
			case ".vam":
			case ".scene":
			case ".assetbundle":
				thumbnailPath = Regex.Replace(fileEntry.Path, "\\.(json|vac|vap|vam|scene|assetbundle)$", ".jpg");
				if (!FileManager.FileExists(thumbnailPath))
				{
					thumbnailPath = Regex.Replace(thumbnailPath, "\\.jpg$", ".JPG");
				}
				break;
			default:
				return;
			}
			if (!FileManager.FileExists(thumbnailPath))
			{
				Report("no thumbnail file for " + fileEntry.Path + " (tried " + thumbnailPath + ", inside a package: " + FileManager.IsFileInPackage(fileEntry.Path) + ")");
			}
		}

		private static int decodedCount;

		internal static void ReportLoaderSuccess(string imgPath, Texture2D tex)
		{
			decodedCount++;
			if (tex == null)
			{
				Report("the image loader produced no texture for " + imgPath);
				return;
			}
			if (decodedCount == 1 || decodedCount % 25 == 0)
			{
				Report("decoded " + decodedCount + " thumbnails; the last one is " + imgPath + " at " + tex.width + "x" + tex.height);
			}
		}

		private static string probeThumbnailPath;

		private static readonly List<string> probeBrowserThumbnails = new List<string>();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void BootProbe()
		{
			if (!HasSwitch("-vamopen-diag") || GameObject.Find("VaMOpen Diagnostics") != null)
			{
				return;
			}
			GameObject gameObject = new GameObject("VaMOpen Diagnostics");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<Probe>();
		}

		private static bool HasSwitch(string flagName)
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (commandLineArgs[i] == flagName)
				{
					return true;
				}
			}
			return false;
		}

		private sealed class Probe : MonoBehaviour
		{
			private IEnumerator Start()
			{
				float deadline = Time.realtimeSinceStartup + 60f;
				while (SuperController.singleton == null && Time.realtimeSinceStartup < deadline)
				{
					yield return null;
				}
				while (ImageLoaderThreaded.singleton == null && Time.realtimeSinceStartup < deadline)
				{
					yield return null;
				}
				if (ImageLoaderThreaded.singleton == null)
				{
					Report("boot probe: ImageLoaderThreaded.singleton never appeared, so nothing loads an image at all");
					yield break;
				}
				if (SuperController.singleton == null)
				{
					Report("boot probe: SuperController.singleton never appeared");
					yield break;
				}
				ReportBrowser("the scene browser", SuperController.singleton.fileBrowserUI);
				ReportBrowser("the world browser", SuperController.singleton.fileBrowserWorldUI);
				ReportBrowser("the world template browser", SuperController.singleton.templatesFileBrowserWorldUI);
				ReportBrowser("the media browser", SuperController.singleton.mediaFileBrowserUI);
				ReportBrowser("the directory browser", SuperController.singleton.directoryBrowserUI);
				ProbeSceneThumbnail();
				ProbeBrowserList();
				yield return new WaitForSeconds(5f);
				if (probeThumbnailPath != null)
				{
					Texture2D cached = ImageLoaderThreaded.singleton.GetCachedThumbnail(probeThumbnailPath);
					if (cached == null)
					{
						Report("boot probe: the thumbnail cache holds nothing for " + probeThumbnailPath + ", so the button has nothing to show");
					}
					else
					{
						Report("boot probe: the thumbnail cache holds " + cached.width + "x" + cached.height + " for " + probeThumbnailPath + ", so a button that asks for it gets a texture");
					}
				}
				int num = 0;
				foreach (string probeBrowserThumbnail in probeBrowserThumbnails)
				{
					if (ImageLoaderThreaded.singleton.GetCachedThumbnail(probeBrowserThumbnail) != null)
					{
						num++;
					}
					else
					{
						Report("boot probe: the browser asked for " + probeBrowserThumbnail + " and the cache still holds nothing for it");
					}
				}
				Report("boot probe: " + num + " of " + probeBrowserThumbnails.Count + " scene browser thumbnails are in the cache");
			}

			private string ResolveThumbnailPath(FileEntry fileEntry)
			{
				switch (Path.GetExtension(fileEntry.Path))
				{
				case ".duf":
					return fileEntry.Path + ".png";
				case ".json":
				case ".vac":
				case ".vap":
				case ".vam":
				case ".scene":
				case ".assetbundle":
				{
					string text = Regex.Replace(fileEntry.Path, "\\.(json|vac|vap|vam|scene|assetbundle)$", ".jpg");
					if (!FileManager.FileExists(text))
					{
						string text2 = Regex.Replace(text, "\\.jpg$", ".JPG");
						if (FileManager.FileExists(text2))
						{
							text = text2;
						}
					}
					return text;
				}
				default:
					return null;
				}
			}

			private void ProbeBrowserList()
			{
				List<FileEntry> list = new List<FileEntry>();
				FileManager.FindAllFiles("Saves/scene", "*.json", list);
				Report("boot probe: the scene list holds " + list.Count + " scenes");
				int num = 0;
				foreach (FileEntry fileEntry in list)
				{
					if (num >= 8)
					{
						break;
					}
					num++;
					string text = ResolveThumbnailPath(fileEntry);
					bool flag = text != null && FileManager.FileExists(text);
					Report("boot probe: scene " + fileEntry.Path + " (in a package: " + FileManager.IsFileInPackage(fileEntry.Path) + ") resolves to thumbnail " + ((text == null) ? "none" : text) + " which exists: " + flag);
					if (!flag)
					{
						continue;
					}
					ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
					queuedImage.imgPath = text;
					queuedImage.width = 512;
					queuedImage.height = 512;
					queuedImage.setSize = true;
					queuedImage.fillBackground = true;
					ImageLoaderThreaded.singleton.QueueThumbnail(queuedImage);
					probeBrowserThumbnails.Add(text);
				}
			}

			private void ReportBrowser(string label, FileBrowser browser)
			{
				if (browser == null)
				{
					Report(label + " is not set on the scene");
					return;
				}
				if (browser.fileButtonPrefab == null)
				{
					Report(label + " has no file button prefab, so it can never draw a thumbnail");
					return;
				}
				FileButton component = browser.fileButtonPrefab.GetComponent<FileButton>();
				if (component == null)
				{
					Report(label + "'s file button prefab carries no FileButton");
					return;
				}
				if (component.altIcon == null || component.fileIcon == null)
				{
					Report(label + "'s file button prefab has altIcon " + ((component.altIcon == null) ? "missing" : "wired") + " and fileIcon " + ((component.fileIcon == null) ? "missing" : "wired"));
					return;
				}
				Report(label + "'s file button prefab is wired for thumbnails");
			}

			private void ProbeSceneThumbnail()
			{
				FileEntry fileEntry = FileManager.GetFileEntry("Saves\\scene\\MeshedVR\\default.json");
				if (fileEntry == null)
				{
					fileEntry = FileManager.GetFileEntry("Saves/scene/MeshedVR/default.json");
				}
				if (fileEntry == null)
				{
					Report("boot probe: the default scene has no file entry in this installation");
					return;
				}
				string text = ResolveThumbnailPath(fileEntry);
				if (!FileManager.FileExists(text))
				{
					Report("boot probe: no thumbnail for " + fileEntry.Path + " (tried " + text + ", inside a package: " + FileManager.IsFileInPackage(fileEntry.Path) + ")");
					return;
				}
				ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
				queuedImage.imgPath = text;
				queuedImage.width = 512;
				queuedImage.height = 512;
				queuedImage.setSize = true;
				queuedImage.fillBackground = true;
				ImageLoaderThreaded.singleton.QueueThumbnail(queuedImage);
				probeThumbnailPath = text;
				Report("boot probe: asked the image loader for " + text);
			}
		}
	}
}
