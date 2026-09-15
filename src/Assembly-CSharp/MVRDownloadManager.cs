using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using MVR.FileManagement;
using UnityEngine;
using UnityEngine.UI;
using ZenFulcrum.EmbeddedBrowser;

public class MVRDownloadManager : MonoBehaviour
{
	public class Download
	{
		public Browser browser;

		public int downloadId;

		public string name;

		public string path;

		public int speed;

		public int percent;

		public string status;
	}

	[Tooltip("If true, this will find all the browser in the scene at startup and take control of their downloads.")]
	public bool manageAllBrowsers;

	[Tooltip("If true, a \"Save as\" style dialog will be given for all downloads.")]
	public bool promptForFileNames;

	[Tooltip("Where to save files. If null or blank, defaults to the user's downloads directory.")]
	public string saveFolder;

	[Tooltip("Automatically open scene on download of a var package if it has one")]
	public bool autoOpenPackageScene = true;

	[Tooltip("If given this text element will be updated with download status info.")]
	public Text statusBar;

	public bool extractOnDownload = true;

	public Toggle extractOnDownloadToggle;

	public bool overwrite = true;

	public List<Download> downloads = new List<Download>();

	private StringBuilder sb = new StringBuilder();

	private string _status = string.Empty;

	public string Status
	{
		get
		{
			if (downloads.Count == 0)
			{
				return _status;
			}
			sb.Length = 0;
			int num = 0;
			bool flag = false;
			for (int num2 = downloads.Count - 1; num2 >= 0; num2--)
			{
				if (downloads[num2].status != "complete")
				{
					flag = true;
					if (sb.Length > 0)
					{
						sb.Append(", ");
					}
					sb.Append(downloads[num2].name);
					if (downloads[num2].status == "working")
					{
						if (downloads[num2].percent >= 0)
						{
							sb.Append(" (").Append(downloads[num2].percent).Append("%)");
						}
						else
						{
							sb.Append(" (??%)");
						}
						num += downloads[num2].speed;
					}
					else
					{
						sb.Append(" (").Append(downloads[num2].status).Append(")");
					}
				}
			}
			if (flag)
			{
				string text = "Downloads";
				if (num > 0)
				{
					string text2 = text;
					text = text2 + " (" + Mathf.Round((float)num / 1048576f * 100f) / 100f + "MiB/s)";
				}
				_status = text + ": " + sb.ToString();
			}
			return _status;
		}
	}

	public void Awake()
	{
		if (manageAllBrowsers)
		{
			Browser[] array = UnityEngine.Object.FindObjectsOfType<Browser>();
			foreach (Browser browser in array)
			{
				ManageDownloads(browser);
			}
		}
		if ((bool)extractOnDownloadToggle)
		{
			extractOnDownloadToggle.isOn = extractOnDownload;
			extractOnDownloadToggle.onValueChanged.AddListener(delegate
			{
				extractOnDownload = extractOnDownloadToggle.isOn;
			});
		}
	}

	public void ManageDownloads(Browser browser)
	{
		browser.onDownloadStarted = delegate(int id, JSONNode info)
		{
			HandleDownloadStarted(browser, id, info);
		};
		browser.onDownloadStatus += delegate(int id, JSONNode info)
		{
			HandleDownloadStatus(browser, id, info);
		};
	}

	private void HandleDownloadStarted(Browser browser, int downloadId, JSONNode info)
	{
		Debug.Log("Download requested: " + info.AsJSON);
		Download download = new Download();
		download.browser = browser;
		download.downloadId = downloadId;
		download.name = info["suggestedName"];
		Download item = download;
		if (promptForFileNames)
		{
			browser.DownloadCommand(downloadId, BrowserNative.DownloadAction.Begin);
		}
		else
		{
			DirectoryInfo directoryInfo;
			if (string.IsNullOrEmpty(saveFolder))
			{
				directoryInfo = new DirectoryInfo(GetUserDownloadFolder());
			}
			else
			{
				directoryInfo = new DirectoryInfo(saveFolder);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
			}
			string text = directoryInfo.FullName + "/" + new FileInfo(info["suggestedName"]).Name;
			if (!overwrite)
			{
				while (File.Exists(text))
				{
					string extension = Path.GetExtension(text);
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
					string text2 = DateTime.Now.ToString("yyyy-MM-dd hh_mm_ss");
					text = directoryInfo.FullName + "/" + fileNameWithoutExtension + " " + text2 + extension;
				}
			}
			browser.DownloadCommand(downloadId, BrowserNative.DownloadAction.Begin, text);
		}
		downloads.Add(item);
	}

	private void HandleDownloadStatus(Browser browser, int downloadId, JSONNode info)
	{
		Debug.Log("Download status: " + info.AsJSON);
		for (int i = 0; i < downloads.Count; i++)
		{
			if (downloads[i].browser != browser || downloads[i].downloadId != downloadId)
			{
				continue;
			}
			Download download = downloads[i];
			download.status = info["status"];
			download.speed = info["speed"];
			download.percent = info["percentComplete"];
			if (!string.IsNullOrEmpty(info["fullPath"]))
			{
				download.name = Path.GetFileName(info["fullPath"]);
			}
			if (!(download.status == "complete") || !extractOnDownload || !(SuperController.singleton != null))
			{
				break;
			}
			string text = info["fullPath"];
			if (text.EndsWith(".zip") || text.EndsWith(".vac") || (text.EndsWith(".json") && autoOpenPackageScene))
			{
				SuperController.singleton.Load(text);
			}
			else
			{
				if (!text.EndsWith(".var"))
				{
					break;
				}
				string fileName = Path.GetFileName(text);
				FileManager.MoveFile(text, "AddonPackages/" + fileName);
				FileManager.Refresh();
				_status = "Package " + fileName + " downloaded. Content is ready to be used";
				if (!autoOpenPackageScene)
				{
					break;
				}
				string packageUidOrPath = fileName.Replace(".var", string.Empty);
				VarPackage package = FileManager.GetPackage(packageUidOrPath);
				if (package != null)
				{
					List<FileEntry> list = new List<FileEntry>();
					package.FindFiles("Saves/scene", "*.json", list);
					if (list.Count > 0)
					{
						FileEntry fileEntry = list[0];
						SuperController.singleton.Load(fileEntry.Uid);
					}
				}
			}
			break;
		}
	}

	public void Update()
	{
		if ((bool)statusBar)
		{
			statusBar.text = Status;
		}
	}

	public void PauseAll()
	{
		for (int i = 0; i < downloads.Count; i++)
		{
			if (downloads[i].status == "working")
			{
				downloads[i].browser.DownloadCommand(downloads[i].downloadId, BrowserNative.DownloadAction.Pause);
			}
		}
	}

	public void ResumeAll()
	{
		for (int i = 0; i < downloads.Count; i++)
		{
			if (downloads[i].status == "working")
			{
				downloads[i].browser.DownloadCommand(downloads[i].downloadId, BrowserNative.DownloadAction.Resume);
			}
		}
	}

	public void CancelAll()
	{
		for (int i = 0; i < downloads.Count; i++)
		{
			if (downloads[i].status == "working")
			{
				downloads[i].browser.DownloadCommand(downloads[i].downloadId, BrowserNative.DownloadAction.Cancel);
			}
		}
	}

	public void ClearAll()
	{
		CancelAll();
		downloads.Clear();
	}

	public static string GetUserDownloadFolder()
	{
		switch (Environment.OSVersion.Platform)
		{
		case PlatformID.Win32NT:
		{
			IntPtr ppszPath;
			if (SHGetKnownFolderPath(new Guid("{374DE290-123F-4565-9164-39C4925E467B}"), 32768u, IntPtr.Zero, out ppszPath) == 0)
			{
				string result = Marshal.PtrToStringUni(ppszPath);
				Marshal.FreeCoTaskMem(ppszPath);
				return result;
			}
			throw new Exception("Failed to get user download directory", new Win32Exception(Marshal.GetLastWin32Error()));
		}
		case PlatformID.Unix:
		{
			string text = Environment.GetEnvironmentVariable("HOME") + "/Downloads";
			DirectoryInfo directoryInfo = new DirectoryInfo(text);
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			return text;
		}
		case PlatformID.MacOSX:
			throw new NotImplementedException();
		default:
			throw new NotImplementedException();
		}
	}

	[DllImport("Shell32.dll")]
	private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
}
