using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using HelloMeow;
using ICSharpCode.SharpZipLib.Core;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class URLAudioClipManager : AudioClipManager
{
	public static URLAudioClipManager singleton;

	protected string[] customParamNames = new string[1] { "clips" };

	public ScrollRectContentManager clipContentManager;

	public RectTransform clipPrefab;

	protected Queue<URLAudioClip> urlqueue;

	public Button fileBrowseButton;

	public InputField urlInputField;

	public InputFieldAction urlInputFieldAction;

	public Button addClipButton;

	public Button copyToClipboardButton;

	public Button copyFromClipboardButton;

	public override string[] GetCustomParamNames()
	{
		return customParamNames;
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if ((includePhysical || forceStore) && clips.Count > 0)
		{
			needsStore = true;
			JSONArray jSONArray = new JSONArray();
			foreach (URLAudioClip clip in clips)
			{
				JSONClass jSONClass = new JSONClass();
				if (Regex.IsMatch(clip.url, "^http"))
				{
					jSONClass["url"] = clip.url;
				}
				else if (SuperController.singleton != null)
				{
					jSONClass["url"] = SuperController.singleton.NormalizeSavePath(clip.url);
				}
				else
				{
					jSONClass["url"] = clip.url;
				}
				jSONClass["displayName"] = clip.displayName;
				jSONArray.Add(jSONClass);
			}
			jSON["clips"] = jSONArray;
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (base.physicalLocked || !restorePhysical || IsCustomPhysicalParamLocked("clips"))
		{
			return;
		}
		if (jc["clips"] != null)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			JSONArray asArray = jc["clips"].AsArray;
			foreach (JSONNode item in asArray)
			{
				JSONClass asObject = item.AsObject;
				if (!(asObject != null) || !(asObject["url"] != null))
				{
					continue;
				}
				string text = asObject["url"];
				if (asObject["displayName"] != null)
				{
					string value = asObject["displayName"];
					if (SuperController.singleton != null)
					{
						text = SuperController.singleton.NormalizeLoadPath(text);
					}
					if (!dictionary.ContainsKey(text))
					{
						dictionary.Add(text, value);
					}
				}
			}
			List<NamedAudioClip> list = new List<NamedAudioClip>();
			foreach (NamedAudioClip clip in clips)
			{
				URLAudioClip uRLAudioClip = clip as URLAudioClip;
				if (dictionary.ContainsKey(uRLAudioClip.url))
				{
					dictionary.Remove(uRLAudioClip.url);
				}
				else
				{
					list.Add(clip);
				}
			}
			if (!base.mergeRestore)
			{
				foreach (NamedAudioClip item2 in list)
				{
					RemoveClip(item2, false);
				}
			}
			{
				foreach (KeyValuePair<string, string> item3 in dictionary)
				{
					QueueClip(item3.Key, item3.Value, true);
				}
				return;
			}
		}
		if (setMissingToDefault)
		{
			RemoveAllClips();
		}
	}

	protected void LoadFileIntoByteArray(FileEntry fe, out IntPtr byteArray)
	{
		byte[] buffer = new byte[32768];
		using (FileEntryStream fileEntryStream = FileManager.OpenStream(fe))
		{
			byte[] array = new byte[fe.Size];
			using (MemoryStream destination = new MemoryStream(array))
			{
				StreamUtils.Copy(fileEntryStream.Stream, destination, buffer);
				byteArray = Marshal.AllocHGlobal(array.Length);
				Marshal.Copy(array, 0, byteArray, array.Length);
			}
		}
	}

	private IEnumerator ProcessQueue()
	{
		while (true)
		{
			if (urlqueue.Count > 0)
			{
				URLAudioClip uac = urlqueue.Dequeue();
				AsyncFlag af = new AsyncFlag("URL Audio: " + uac.displayName);
				if (uac.fromRestore)
				{
					SuperController.singleton.ResetSimulation(af);
				}
				AsyncFlag iconFlag = new AsyncFlag("URL Audio");
				SuperController.singleton.SetLoadingIconFlag(iconFlag);
				string url = uac.url;
				FileEntry fe = FileManager.GetFileEntry(url);
				bool isPackageFile = false;
				if (fe != null && fe is VarFileEntry)
				{
					isPackageFile = true;
				}
				if (url.EndsWith(".mp3") || url.EndsWith(".wav") || url.EndsWith(".ogg"))
				{
					BassImporter bi = GetComponent<BassImporter>();
					if (bi != null)
					{
						if (isPackageFile)
						{
							bi.Prep();
							IntPtr byteArray = default(IntPtr);
							Thread loadThread = new Thread((ThreadStart)delegate
							{
								LoadFileIntoByteArray(fe, out byteArray);
							});
							loadThread.Start();
							while (loadThread.IsAlive)
							{
								yield return null;
							}
							yield return bi.SetData(byteArray, fe.Size);
						}
						else
						{
							if (Regex.IsMatch(url, "^file:///"))
							{
								url = url.Replace("file:///", "file://");
							}
							if (!Regex.IsMatch(url, "^http") && !Regex.IsMatch(url, "^file"))
							{
								url = ((!url.Contains(":/")) ? ("file://./" + url) : ("file://" + url));
							}
							bi.Import(url);
							while (!bi.isLoaded && !bi.isError && !uac.removed)
							{
								if (uac.loadProgressSlider != null)
								{
									uac.loadProgressSlider.value = bi.progress;
								}
								if (uac.sizeText != null)
								{
									uac.sizeText.text = string.Empty;
								}
								yield return null;
							}
						}
						if (!uac.removed)
						{
							if (!bi.isError)
							{
								uac.sourceClip = bi.audioClip;
								if (uac.loadProgressSlider != null)
								{
									uac.loadProgressSlider.value = 1f;
								}
								if (uac.sourceClip != null)
								{
									uac.ready = true;
								}
								else if (!uac.error)
								{
									uac.errorMsg = "Unable to extract audio from url";
									uac.error = true;
									SuperController.LogError("Unable to extract audio from url " + url);
								}
							}
							else
							{
								uac.errorMsg = bi.error;
								uac.error = true;
								SuperController.LogError("Error during mp3/wav import " + bi.error);
							}
						}
					}
					else
					{
						uac.errorMsg = "MP3/WAV importer not defined";
						uac.error = true;
						SuperController.LogError("MP3/WAV importer not defined. Cannot import mp3/wav");
					}
				}
				else if (!isPackageFile)
				{
					if (!Regex.IsMatch(url, "^http") && !Regex.IsMatch(url, "^file"))
					{
						url = ((!url.Contains(":/")) ? ("file:///.\\" + url) : ("file:///" + url));
					}
					WWW www = new WWW(url);
					while (!www.isDone)
					{
						if (uac.removed)
						{
							www.Dispose();
							break;
						}
						if (uac.loadProgressSlider != null)
						{
							uac.loadProgressSlider.value = www.progress;
						}
						if (uac.sizeText != null)
						{
							float num = (float)www.bytesDownloaded / 1000000f;
							uac.sizeText.text = $"{num:F1}MB";
						}
						yield return null;
					}
					if (!uac.removed)
					{
						if (www.error == null || www.error == string.Empty)
						{
							bool isMp3 = false;
							if (www.responseHeaders.Count > 0)
							{
								foreach (KeyValuePair<string, string> responseHeader in www.responseHeaders)
								{
									if (responseHeader.Key == "Content-Type")
									{
										string[] array = responseHeader.Value.Split(';');
										string[] array2 = array;
										foreach (string text in array2)
										{
											if (text.EndsWith(".mp3\""))
											{
												isMp3 = true;
											}
										}
									}
									else
									{
										if (!(responseHeader.Key == "Content-Disposition"))
										{
											continue;
										}
										string[] array3 = responseHeader.Value.Split(';');
										string[] array4 = array3;
										foreach (string text2 in array4)
										{
											if (text2.EndsWith(".mp3"))
											{
												isMp3 = true;
											}
										}
									}
								}
							}
							float fsize = (float)www.bytesDownloaded / 1000000f;
							if (uac.sizeText != null)
							{
								uac.sizeText.text = $"{fsize:F1}MB";
							}
							if (url.EndsWith(".mp3") || isMp3)
							{
								BassImporter bi2 = GetComponent<BassImporter>();
								if (bi2 != null)
								{
									yield return bi2.SetData(www.bytes);
									if (!uac.removed)
									{
										if (bi2.isError)
										{
											uac.errorMsg = bi2.error;
											uac.error = true;
											SuperController.LogError("Error during mp3 import " + bi2.error);
										}
										else
										{
											uac.sourceClip = bi2.audioClip;
										}
									}
								}
								else
								{
									uac.errorMsg = "MP3 importer not defined";
									uac.error = true;
									SuperController.LogError("MP3 importer not defined. Cannot import mp3");
								}
							}
							else
							{
								uac.sourceClip = www.GetAudioClip();
								if (uac.sourceClip == null)
								{
									try
									{
										uac.sourceClip = NAudioPlayer.AudioClipFromMp3Data(www.bytes);
									}
									catch (Exception ex)
									{
										uac.error = true;
										uac.errorMsg = "Could not extract audio data";
										SuperController.LogError("Could not extract audio data: " + ex.Message);
									}
								}
							}
							if (uac.loadProgressSlider != null)
							{
								uac.loadProgressSlider.value = 1f;
							}
							if (uac.sourceClip != null)
							{
								uac.ready = true;
							}
							else if (!uac.error)
							{
								uac.errorMsg = "Unable to extract audio from url";
								uac.error = true;
								SuperController.LogError("Unable to extract audio from url " + url);
							}
						}
						else
						{
							uac.error = true;
							uac.errorMsg = www.error;
							SuperController.LogError("Could not load audio source at " + url + " Error: " + www.error);
						}
					}
				}
				else
				{
					uac.error = true;
					uac.errorMsg = "Packages only support mp3, ogg, and wav files";
					SuperController.LogError("Could not load audio source at " + url + " Error: " + uac.error);
				}
				af.Raise();
				iconFlag.Raise();
			}
			yield return null;
		}
	}

	public override bool RemoveClip(NamedAudioClip nac)
	{
		return RemoveClip(nac, true);
	}

	protected bool RemoveClip(NamedAudioClip nac, bool validate)
	{
		if (base.RemoveClip(nac))
		{
			URLAudioClip uRLAudioClip = nac as URLAudioClip;
			if (nac.sourceClip != null)
			{
				UnityEngine.Object.Destroy(nac.sourceClip);
			}
			if (uRLAudioClip.UIpanel != null)
			{
				if (clipContentManager != null)
				{
					clipContentManager.RemoveItem(uRLAudioClip.UIpanel);
				}
				UnityEngine.Object.Destroy(uRLAudioClip.UIpanel.gameObject);
			}
			if (validate && SuperController.singleton != null)
			{
				SuperController.singleton.ValidateAllAtoms();
			}
			return true;
		}
		return false;
	}

	public override void RemoveAllClips()
	{
		foreach (NamedAudioClip clip in clips)
		{
			URLAudioClip uRLAudioClip = clip as URLAudioClip;
			if (uRLAudioClip.UIpanel != null)
			{
				if (clipContentManager != null)
				{
					clipContentManager.RemoveItem(uRLAudioClip.UIpanel);
				}
				UnityEngine.Object.Destroy(uRLAudioClip.UIpanel.gameObject);
			}
		}
		base.RemoveAllClips();
		if (SuperController.singleton != null)
		{
			SuperController.singleton.ValidateAllAtoms();
		}
	}

	protected string URLToUid(string url)
	{
		FileEntry fileEntry = FileManager.GetFileEntry(url);
		if (fileEntry != null && fileEntry is VarFileEntry)
		{
			VarFileEntry varFileEntry = fileEntry as VarFileEntry;
			string internalSlashPath = varFileEntry.InternalSlashPath;
			return Regex.Replace(internalSlashPath, ".*/", string.Empty);
		}
		if (Regex.IsMatch(url, "^http"))
		{
			return url;
		}
		string text = Regex.Replace(url, "\\\\", "/");
		return Regex.Replace(url, ".*/", string.Empty);
	}

	public URLAudioClip QueueClip(string url, string displayName = null, bool fromRestore = false)
	{
		URLAudioClip uRLAudioClip = null;
		if (url != null && url != string.Empty)
		{
			bool flag = false;
			if (Regex.IsMatch(url, "^http"))
			{
				if (UserPreferences.singleton == null)
				{
					flag = true;
				}
				else if (UserPreferences.singleton.enableWebMisc)
				{
					if (UserPreferences.singleton.CheckWhitelistDomain(url))
					{
						flag = true;
					}
					else
					{
						SuperController.LogError("Attempted to load audio from URL " + url + " which is not on whitelist", true, !UserPreferences.singleton.hideDisabledWebMessages);
					}
				}
				else if (!UserPreferences.singleton.hideDisabledWebMessages)
				{
					SuperController.LogError("Attempted to load http URL audio when web load option is disabled. To enable, see User Preferences -> Web Security tab");
					SuperController.singleton.ShowMainHUDAuto();
					SuperController.singleton.SetActiveUI("MainMenu");
					SuperController.singleton.SetMainMenuTab("TabUserPrefs");
					SuperController.singleton.SetUserPrefsTab("TabSecurity");
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				string text = URLToUid(url);
				if (!uidToClip.ContainsKey(text))
				{
					uRLAudioClip = new URLAudioClip();
					uRLAudioClip.fromRestore = fromRestore;
					uRLAudioClip.uid = text;
					uRLAudioClip.url = url;
					uRLAudioClip.category = "web";
					if (displayName == null)
					{
						uRLAudioClip.displayName = Regex.Replace(uRLAudioClip.uid, ".*/", string.Empty);
					}
					else
					{
						uRLAudioClip.displayName = displayName;
					}
					base.AddClip(uRLAudioClip);
					urlqueue.Enqueue(uRLAudioClip);
					if (clipContentManager != null && clipPrefab != null)
					{
						RectTransform rectTransform = UnityEngine.Object.Instantiate(clipPrefab);
						clipContentManager.AddItem(rectTransform);
						uRLAudioClip.UIpanel = rectTransform;
						URLAudioClipUI component = rectTransform.GetComponent<URLAudioClipUI>();
						if (component != null)
						{
							uRLAudioClip.removeButton = component.removeButton;
							uRLAudioClip.testButton = component.testButton;
							uRLAudioClip.testButtonText = component.testButtonText;
							uRLAudioClip.uidText = component.urlText;
							uRLAudioClip.sizeText = component.sizeText;
							uRLAudioClip.displayNameField = component.displayNameField;
							uRLAudioClip.readyToggle = component.readyToggle;
							uRLAudioClip.loadProgressSlider = component.loadProgressSlider;
							uRLAudioClip.InitUI();
						}
					}
				}
			}
		}
		return uRLAudioClip;
	}

	public void CopyURLToClipboard()
	{
		if (urlInputField != null)
		{
			GUIUtility.systemCopyBuffer = urlInputField.text;
		}
	}

	public void CopyURLFromClipboard()
	{
		if (urlInputField != null)
		{
			urlInputField.text = GUIUtility.systemCopyBuffer;
			QueueClip(urlInputField.text);
		}
	}

	public void QueueFilePath(string path)
	{
		if (path != null && path != string.Empty)
		{
			path = SuperController.singleton.NormalizeMediaPath(path);
			if (urlInputField != null)
			{
				urlInputField.text = path;
				QueueClip(path);
			}
		}
	}

	public void FileBrowse()
	{
		if (SuperController.singleton != null)
		{
			List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory("Custom/Sounds", true, true, true, true);
			ShortCut shortCut = new ShortCut();
			shortCut.displayName = "Root";
			shortCut.path = Path.GetFullPath(".");
			shortCutsForDirectory.Insert(0, shortCut);
			SuperController.singleton.GetMediaPathDialog(QueueFilePath, "mp3|ogg|wav", "Custom/Sounds", true, true, false, null, false, shortCutsForDirectory);
		}
	}

	public override void InitUI()
	{
		if (urlInputField != null)
		{
			if (urlInputFieldAction != null)
			{
				InputFieldAction inputFieldAction = urlInputFieldAction;
				inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(inputFieldAction.onSubmitHandlers, (InputFieldAction.OnSubmit)delegate
				{
					QueueClip(urlInputField.text);
				});
			}
			if (addClipButton != null)
			{
				addClipButton.onClick.AddListener(delegate
				{
					QueueClip(urlInputField.text);
				});
			}
		}
		if (copyToClipboardButton != null)
		{
			copyToClipboardButton.onClick.AddListener(CopyURLToClipboard);
		}
		if (copyFromClipboardButton != null)
		{
			copyFromClipboardButton.onClick.AddListener(CopyURLFromClipboard);
		}
		if (fileBrowseButton != null)
		{
			fileBrowseButton.onClick.AddListener(FileBrowse);
		}
	}

	protected override void Init()
	{
		base.Init();
		singleton = this;
		if (!FileManager.DirectoryExists("Custom/Sounds"))
		{
			FileManager.CreateDirectory("Custom/Sounds");
		}
		InitUI();
		urlqueue = new Queue<URLAudioClip>();
		StartCoroutine(ProcessQueue());
	}
}
