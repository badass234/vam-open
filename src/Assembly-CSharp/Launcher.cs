using System;
using System.Diagnostics;
using System.IO;
using MHLab.PATCH;
using MHLab.PATCH.Debugging;
using MHLab.PATCH.Install;
using MHLab.PATCH.Settings;
using MHLab.PATCH.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityThreading;

public class Launcher : MonoBehaviour
{
	private enum LauncherStatus
	{
		IDLE,
		IS_BUSY
	}

	private LauncherManager m_launcher;

	private InstallManager m_installer;

	[Header("Patcher & Patches")]
	[Tooltip("If enabled your launcher will provide to check and apply patches to your current build.")]
	public bool ActivatePatcher = true;

	[Tooltip("Your versions.txt file remote URL")]
	public string VersionsFileDownloadURL = "http://your/url/to/versions.txt";

	[Tooltip("Your patches directory remote URL")]
	public string PatchesDirectoryURL = "http://your/url/to/patches/directory/";

	[Tooltip("Your game name! This string will be attached to app root path to launch your game when patching process will end!")]
	public string AppToLaunch = "Build.exe";

	[Tooltip("Determines if your launcher will be closed after your game starts!")]
	public bool CloseLauncherOnStart = true;

	[Tooltip("This argument will be attached to your game running command!")]
	public string Argument = "default";

	[Tooltip("If enabled your argument will be sent as raw text, if not your argument will be sent as \"YourGame.exe --LaunchArgs=YourArgument\"")]
	public bool UseRawArgument;

	[Tooltip("If enabled your patcher can be included in your Unity game build")]
	public bool IsIntegrated;

	[Tooltip("If IsIntegrated is true, your patcher will load this scene after patch process")]
	public int SceneToLoad = 1;

	[Space(10f)]
	[Header("Installer & Repairer")]
	[Tooltip("If enabled your launcher will install your build files before patches checking.")]
	public bool ActivateInstaller = true;

	[Tooltip("If enabled your launcher will start to check files integrity before patches checking. It is useful to fix files corruption of your users' builds!")]
	public bool ActivateRepairer = true;

	[Tooltip("Your builds directory remote URL")]
	public string BuildsDirectoryURL = "http://your/url/to/builds/directory/";

	[Tooltip("Your launcher name!")]
	public string LauncherName = "PATCH.exe";

	[Tooltip("If enabled your installer will install locally your game, if not your installer will install your game in Program Files/ProgramFilesDirectoryToInstall directory")]
	public bool InstallInLocalPath;

	[Tooltip("If your installer have to install your game under Program Files folder, this will be the name of your game directory!")]
	public string ProgramFilesDirectoryToInstall = "MHLab";

	[Tooltip("If enabled your installer will create a shortcut to your patcher on desktop")]
	public bool CreateDesktopShortcut = true;

	[Space(10f)]
	[Header("Common settings")]
	[Tooltip("How many times downloader can retry to download a file, if an error occurs?")]
	public ushort DownloadRetryAttempts;

	[Tooltip("Enables WebRequests or FTPRequests with credentials. Generally, you need this when your remote directories are proteted by login or your remote URLs are FTP ones!")]
	public bool EnableCredentials;

	[Tooltip("Username for your requests")]
	public string Username = "YourUsernameHere";

	[Tooltip("Password for your requests")]
	public string Password = "YourPasswordHere";

	[Space(10f)]
	[Header("GUI Components")]
	public ProgressBar MainBar;

	public ProgressBar DetailBar;

	public Text MainLog;

	public Text DetailLog;

	public Button LaunchButton;

	public RectTransform Overlay;

	public RectTransform MainMenu;

	public RectTransform RestartMenu;

	public RectTransform OptionsMenu;

	private ActionThread m_updateCheckingThread;

	private DateTime _lastTime = DateTime.UtcNow;

	private long _lastSize;

	private int _downloadSpeed;

	private void Start()
	{
		Singleton<Localizatron>.Instance.SetLanguage("en_EN");
		LocalizeGUI();
		OverrideSettings();
		m_launcher = new LauncherManager();
		m_launcher.SetOnSetMainProgressBarAction(OnSetMainProgressBar);
		m_launcher.SetOnSetDetailProgressBarAction(OnSetDetailProgressBar);
		m_launcher.SetOnIncreaseMainProgressBarAction(OnIncreaseMainProgressBar);
		m_launcher.SetOnIncreaseDetailProgressBarAction(OnIncreaseDetailProgressBar);
		m_launcher.SetOnLogAction(OnLog);
		m_launcher.SetOnErrorAction(OnError);
		m_launcher.SetOnFatalErrorAction(OnFatalError);
		m_launcher.SetOnTaskStartedAction(OnTaskStarted);
		m_launcher.SetOnTaskCompletedAction(OnTaskCompleted);
		m_launcher.SetOnDownloadProgressAction(OnDownloadProgress);
		m_launcher.SetOnDownloadCompletedAction(OnDownloadCompleted);
		m_installer = new InstallManager();
		m_installer.SetOnSetMainProgressBarAction(OnSetMainProgressBar);
		m_installer.SetOnSetDetailProgressBarAction(OnSetDetailProgressBar);
		m_installer.SetOnIncreaseMainProgressBarAction(OnIncreaseMainProgressBar);
		m_installer.SetOnIncreaseDetailProgressBarAction(OnIncreaseDetailProgressBar);
		m_installer.SetOnLogAction(OnLog);
		m_installer.SetOnErrorAction(OnError);
		m_installer.SetOnFatalErrorAction(OnFatalError);
		m_installer.SetOnTaskStartedAction(OnTaskStarted);
		m_installer.SetOnTaskCompletedAction(OnTaskCompleted);
		m_installer.SetOnDownloadProgressAction(OnDownloadProgress);
		m_installer.SetOnDownloadCompletedAction(OnDownloadCompleted);
	}

	private void OnSetMainProgressBar(int min, int max)
	{
		UnityThreadHelper.Dispatcher.Dispatch(delegate
		{
			MainBar.Clear();
			MainBar.Maximum = max;
			MainBar.Minimum = min;
			MainBar.Step = 1f;
		});
	}

	private void OnSetDetailProgressBar(int min, int max)
	{
		UnityThreadHelper.Dispatcher.Dispatch(delegate
		{
			DetailBar.Clear();
			DetailBar.Maximum = max;
			DetailBar.Minimum = min;
			DetailBar.Step = 1f;
		});
	}

	private void OnIncreaseMainProgressBar()
	{
		UnityThreadHelper.Dispatcher.Dispatch(delegate
		{
			MainBar.PerformStep();
		});
	}

	private void OnIncreaseDetailProgressBar()
	{
		UnityThreadHelper.Dispatcher.Dispatch(delegate
		{
			DetailBar.PerformStep();
		});
	}

	private void OnLog(string main, string detail)
	{
		UnityThreadHelper.Dispatcher.Dispatch(delegate
		{
			MainLog.text = Singleton<Localizatron>.Instance.Translate(main);
			DetailLog.text = Singleton<Localizatron>.Instance.Translate(detail);
		});
		MHLab.PATCH.Debugging.Debugger.Log(main + " - " + detail);
	}

	private void OnError(string main, string detail, Exception e)
	{
		UnityThreadHelper.Dispatcher.Dispatch(delegate
		{
			MainLog.text = Singleton<Localizatron>.Instance.Translate(main);
			DetailLog.text = Singleton<Localizatron>.Instance.Translate(detail);
		});
		MHLab.PATCH.Debugging.Debugger.Log(e.Message);
		m_updateCheckingThread.Abort();
	}

	private void OnFatalError(string main, string detail, Exception e)
	{
		UnityThreadHelper.Dispatcher.Dispatch(delegate
		{
			MainLog.text = Singleton<Localizatron>.Instance.Translate(main);
			DetailLog.text = Singleton<Localizatron>.Instance.Translate(detail);
		});
		MHLab.PATCH.Debugging.Debugger.Log(e.Message);
		m_updateCheckingThread.Abort();
	}

	private void OnTaskStarted(string message)
	{
		UnityThreadHelper.Dispatcher.Dispatch(delegate
		{
			LaunchButton.interactable = false;
			MainLog.text = Singleton<Localizatron>.Instance.Translate(message);
			DetailLog.text = string.Empty;
		});
		MHLab.PATCH.Debugging.Debugger.Log(message);
	}

	private void OnTaskCompleted(string message)
	{
		UnityThreadHelper.Dispatcher.Dispatch(delegate
		{
			MainLog.text = Singleton<Localizatron>.Instance.Translate(message);
			DetailLog.text = string.Empty;
			LaunchButton.interactable = true;
			if (IsIntegrated)
			{
				if (Overlay != null)
				{
					Overlay.gameObject.SetActive(true);
				}
				if (m_launcher.IsDirty())
				{
					RestartMenu.gameObject.SetActive(true);
				}
				else
				{
					MainMenu.gameObject.SetActive(true);
				}
			}
		});
		MHLab.PATCH.Debugging.Debugger.Log(message);
	}

	private void OnDownloadProgress(long currentFileSize, long totalFileSize, int percentageCompleted)
	{
		if (_lastTime.AddSeconds(1.0) <= DateTime.UtcNow)
		{
			_downloadSpeed = (int)((double)(currentFileSize - _lastSize) / (DateTime.UtcNow - _lastTime).TotalSeconds);
			_lastSize = currentFileSize;
			_lastTime = DateTime.UtcNow;
		}
		UnityThreadHelper.Dispatcher.Dispatch(delegate
		{
			DetailBar.Progress = percentageCompleted;
			DetailBar.SetProgressText(percentageCompleted + "% - (" + Utility.FormatSizeBinary(currentFileSize, 2) + "/" + Utility.FormatSizeBinary(totalFileSize, 2) + ") @ " + Utility.FormatSizeBinary(_downloadSpeed, 2) + "/s");
		});
	}

	private void OnDownloadCompleted()
	{
		UnityThreadHelper.Dispatcher.Dispatch(delegate
		{
			DetailBar.SetProgressText(string.Empty);
		});
	}

	private void CheckForPatches()
	{
		m_launcher.CheckForUpdates();
	}

	public void CloseButton_click()
	{
		Application.Quit();
	}

	public void StartGame_click()
	{
		SceneManager.LoadScene(SceneToLoad);
	}

	public void OptionButton_click()
	{
		if (MainMenu != null)
		{
			MainMenu.gameObject.SetActive(false);
		}
		if (OptionsMenu != null)
		{
			OptionsMenu.gameObject.SetActive(true);
		}
	}

	public void BackButton_click()
	{
		if (OptionsMenu != null)
		{
			OptionsMenu.gameObject.SetActive(false);
		}
		if (MainMenu != null)
		{
			MainMenu.gameObject.SetActive(true);
		}
	}

	public void EnglishButton_click()
	{
		Singleton<Localizatron>.Instance.SetLanguage("en_EN");
		LocalizeGUI();
	}

	public void ItalianButton_click()
	{
		Singleton<Localizatron>.Instance.SetLanguage("it_IT");
		LocalizeGUI();
	}

	public void LaunchButton_click()
	{
		try
		{
			if (!InstallInLocalPath)
			{
				SettingsManager.APP_PATH = m_installer.GetInstallationPath();
				SettingsManager.RegeneratePaths();
				OverrideSettings(false);
			}
			Process process = new Process();
			process.StartInfo.FileName = SettingsManager.LAUNCH_APP;
			process.StartInfo.Arguments = ((!SettingsManager.USE_RAW_LAUNCH_ARG) ? SettingsManager.LAUNCH_COMMAND : SettingsManager.LAUNCH_ARG);
			process.StartInfo.UseShellExecute = false;
			process.Start();
			if (CloseLauncherOnStart)
			{
				Application.Quit();
			}
			if (!InstallInLocalPath)
			{
				SettingsManager.APP_PATH = Directory.GetParent(Application.dataPath).FullName;
				SettingsManager.RegeneratePaths();
			}
		}
		catch
		{
			if (CloseLauncherOnStart)
			{
				Application.Quit();
			}
		}
	}

	public void RestartButton_click()
	{
		Process process = new Process();
		process.StartInfo.FileName = SettingsManager.LAUNCH_APP;
		process.StartInfo.Arguments = ((!SettingsManager.USE_RAW_LAUNCH_ARG) ? SettingsManager.LAUNCH_COMMAND : SettingsManager.LAUNCH_ARG);
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.Verb = "runas";
		process.Start();
		Application.Quit();
	}

	protected void OverrideSettings(bool overrideAppPath = true)
	{
		if (overrideAppPath)
		{
			SettingsManager.APP_PATH = Directory.GetParent(Application.dataPath).FullName;
			SettingsManager.RegeneratePaths();
		}
		SettingsManager.VERSIONS_FILE_DOWNLOAD_URL = VersionsFileDownloadURL;
		SettingsManager.PATCHES_DOWNLOAD_URL = PatchesDirectoryURL;
		SettingsManager.BUILDS_DOWNLOAD_URL = BuildsDirectoryURL;
		SettingsManager.PATCH_DOWNLOAD_RETRY_ATTEMPTS = DownloadRetryAttempts;
		SettingsManager.LAUNCH_APP = SettingsManager.APP_PATH + Path.DirectorySeparatorChar + AppToLaunch;
		SettingsManager.LAUNCHER_NAME = LauncherName;
		SettingsManager.LAUNCH_ARG = Argument;
		SettingsManager.USE_RAW_LAUNCH_ARG = UseRawArgument;
		SettingsManager.ENABLE_FTP = EnableCredentials;
		SettingsManager.FTP_USERNAME = Username;
		SettingsManager.FTP_PASSWORD = Password;
		SettingsManager.INSTALL_IN_LOCAL_PATH = InstallInLocalPath;
		SettingsManager.PROGRAM_FILES_DIRECTORY_TO_INSTALL = ProgramFilesDirectoryToInstall;
		SettingsManager.PROGRAM_FILES_DIRECTORY_TO_INSTALL_ABS_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), ProgramFilesDirectoryToInstall);
		SettingsManager.PATCH_VERSION_PATH = SettingsManager.APP_PATH + Path.DirectorySeparatorChar + "version";
		SettingsManager.VERSION_FILE_LOCAL_PATH = SettingsManager.APP_PATH + Path.DirectorySeparatorChar + "version";
	}

	protected void LocalizeGUI()
	{
		LaunchButton.GetComponentInChildren<Text>().text = Singleton<Localizatron>.Instance.Translate("LAUNCH");
	}

	protected void EnsureExecutePrivileges()
	{
	}
}
