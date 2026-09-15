using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MVR.Hub
{
	public class HubDownloader : JSONStorable
	{
		protected delegate void RequestSuccessCallback(JSONNode jsonNode);

		protected delegate void RequestErrorCallback(string err);

		public delegate void ErrorCallback(string err);

		public delegate void SuccessCallback();

		public static HubDownloader singleton;

		protected RectTransform panel;

		protected RectTransform packagesContainer;

		protected Text infoText;

		protected Text downloadInfoText;

		protected Button enableButton;

		protected RectTransform disabledIndicator;

		protected JSONStorableAction openPanelAction;

		protected JSONStorableAction closePanelAction;

		protected List<HubResourcePackageUI> trackedPackages;

		protected JSONStorableAction clearTrackedPackagesAction;

		protected int pendingResourceDetailCallbacks;

		protected int pendingResourceDownloads;

		protected List<string> errors;

		protected HashSet<FileManager.PackageUIDAndPublisher> packagesThatWereDownloaded;

		protected JSONStorableString packageNameString;

		protected JSONStorableAction findPackageAction;

		protected JSONStorableAction downloadAllTrackedPackagesAction;

		protected SuccessCallback massDownloadSuccessCallback;

		protected ErrorCallback massDownloadErrorCallback;

		protected bool processingMassDownload;

		protected bool hasPendingMassDownload;

		protected bool hasPendingDownloadPackages;

		protected bool hasPendingDownloadAllMissingPackages;

		protected string[] pendingPackagesList;

		protected SuccessCallback pendingSuccessCallback;

		protected ErrorCallback pendingErrorCallback;

		protected bool pendingFindMissingPackages;

		protected List<string> checkMissingPackageNames;

		public PackageBuilder packageManager;

		public HubBrowse hubBrowse;

		[SerializeField]
		protected string apiUrl;

		protected bool _hubDownloaderEnabled;

		protected JSONStorableBool hubDownloaderEnabledJSON;

		protected JSONStorableAction enableHubDownloaderAction;

		protected JSONStorableAction rejectHubDownloaderAction;

		public int PendingResourceDownloads => pendingResourceDownloads;

		public bool HubDownloaderEnabled
		{
			get
			{
				return _hubDownloaderEnabled;
			}
			set
			{
				if (hubDownloaderEnabledJSON != null)
				{
					hubDownloaderEnabledJSON.val = value;
				}
				else
				{
					_hubDownloaderEnabled = value;
				}
			}
		}

		private IEnumerator PostRequest(string uri, string postData, RequestSuccessCallback callback, RequestErrorCallback errorCallback)
		{
			using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, postData))
			{
				webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(postData));
				webRequest.SetRequestHeader("Content-Type", "application/json");
				webRequest.SetRequestHeader("Accept", "application/json");
				yield return webRequest.SendWebRequest();
				string[] pages = uri.Split('/');
				int page = pages.Length - 1;
				if (webRequest.isNetworkError)
				{
					Debug.LogError(pages[page] + ": Error: " + webRequest.error);
					errorCallback?.Invoke(webRequest.error);
					yield break;
				}
				JSONNode jSONNode = JSON.Parse(webRequest.downloadHandler.text);
				if (jSONNode == null)
				{
					string text = "Error - Invalid JSON response: " + webRequest.downloadHandler.text;
					Debug.LogError(pages[page] + ": " + text);
					errorCallback?.Invoke(text);
				}
				else
				{
					callback?.Invoke(jSONNode);
				}
			}
		}

		public void OpenPanelWithInfo(string info, bool disableCloseButton = false)
		{
			OpenPanel();
			if (disableCloseButton && closePanelAction.button != null)
			{
				closePanelAction.button.gameObject.SetActive(false);
			}
			if (packageNameString.inputField != null)
			{
				packageNameString.inputField.gameObject.SetActive(false);
			}
			if (findPackageAction.button != null)
			{
				findPackageAction.button.gameObject.SetActive(false);
			}
			if (downloadAllTrackedPackagesAction.button != null)
			{
				downloadAllTrackedPackagesAction.button.gameObject.SetActive(false);
			}
			if (clearTrackedPackagesAction.button != null)
			{
				clearTrackedPackagesAction.button.gameObject.SetActive(false);
			}
			if (infoText != null)
			{
				infoText.gameObject.SetActive(true);
				infoText.text = info;
			}
		}

		public void OpenPanel()
		{
			if (panel != null)
			{
				panel.gameObject.SetActive(true);
			}
			if (closePanelAction.button != null)
			{
				closePanelAction.button.gameObject.SetActive(true);
			}
			if (packageNameString.inputField != null)
			{
				packageNameString.inputField.gameObject.SetActive(true);
			}
			if (findPackageAction.button != null)
			{
				findPackageAction.button.gameObject.SetActive(true);
			}
			if (downloadAllTrackedPackagesAction.button != null)
			{
				downloadAllTrackedPackagesAction.button.gameObject.SetActive(true);
			}
			if (clearTrackedPackagesAction.button != null)
			{
				clearTrackedPackagesAction.button.gameObject.SetActive(true);
			}
			if (infoText != null)
			{
				infoText.gameObject.SetActive(false);
				infoText.text = string.Empty;
			}
		}

		public void ClosePanel()
		{
			if (panel != null)
			{
				panel.gameObject.SetActive(false);
			}
		}

		public void ClearTrackedPackages()
		{
			foreach (HubResourcePackageUI trackedPackage in trackedPackages)
			{
				UnityEngine.Object.Destroy(trackedPackage.gameObject);
			}
			trackedPackages.Clear();
			packageNameString.val = string.Empty;
		}

		protected void ResourceDownloadQueued(HubResourcePackage hrp)
		{
			pendingResourceDownloads++;
		}

		protected void ResourceDownloadComplete(HubResourcePackage hrp, bool alreadyHad)
		{
			if (!alreadyHad)
			{
				FileManager.PackageUIDAndPublisher packageUIDAndPublisher = new FileManager.PackageUIDAndPublisher();
				packageUIDAndPublisher.uid = hrp.Name;
				packageUIDAndPublisher.publisher = hrp.PublishingUser;
				packagesThatWereDownloaded.Add(packageUIDAndPublisher);
			}
			pendingResourceDownloads--;
		}

		protected void ResourceDownloadError(HubResourcePackage hrp, string err)
		{
			errors.Add(err);
			pendingResourceDownloads--;
		}

		protected void ResourceDetailCallback(JSONNode jn, bool download = false)
		{
			pendingResourceDetailCallbacks--;
			JSONClass asObject = jn.AsObject;
			if (!(asObject != null))
			{
				return;
			}
			JSONClass asObject2 = asObject["dependencies"].AsObject;
			JSONArray asArray = asObject["hubFiles"].AsArray;
			if (!(asArray != null))
			{
				return;
			}
			foreach (JSONClass item in asArray)
			{
				HubResourcePackage hubResourcePackage = new HubResourcePackage(item, hubBrowse, false);
				RectTransform rectTransform = hubBrowse.CreateDownloadPrefabInstance();
				if (!(rectTransform != null))
				{
					continue;
				}
				rectTransform.SetParent(packagesContainer, false);
				HubResourcePackageUI component = rectTransform.GetComponent<HubResourcePackageUI>();
				if (component != null)
				{
					hubResourcePackage.RegisterUI(component);
					trackedPackages.Add(component);
				}
				if (asObject2 != null)
				{
					JSONArray asArray2 = asObject2[hubResourcePackage.GroupName].AsArray;
					if (asArray2 != null)
					{
						foreach (JSONClass item2 in asArray2)
						{
							HubResourcePackage hubResourcePackage2 = new HubResourcePackage(item2, hubBrowse, true);
							RectTransform rectTransform2 = hubBrowse.CreateDownloadPrefabInstance();
							if (!(rectTransform2 != null))
							{
								continue;
							}
							rectTransform2.SetParent(packagesContainer, false);
							HubResourcePackageUI component2 = rectTransform2.GetComponent<HubResourcePackageUI>();
							if (component2 != null)
							{
								hubResourcePackage2.RegisterUI(component2);
								trackedPackages.Add(component2);
								hubResourcePackage2.downloadQueuedCallback = (HubResourcePackage.DownloadQueuedCallback)Delegate.Combine(hubResourcePackage2.downloadQueuedCallback, new HubResourcePackage.DownloadQueuedCallback(ResourceDownloadQueued));
								hubResourcePackage2.downloadCompleteCallback = (HubResourcePackage.DownloadCompleteCallback)Delegate.Combine(hubResourcePackage2.downloadCompleteCallback, new HubResourcePackage.DownloadCompleteCallback(ResourceDownloadComplete));
								hubResourcePackage2.downloadErrorCallback = (HubResourcePackage.DownloadErrorCallback)Delegate.Combine(hubResourcePackage2.downloadErrorCallback, new HubResourcePackage.DownloadErrorCallback(ResourceDownloadError));
								if (download)
								{
									hubResourcePackage2.Download();
								}
							}
						}
					}
				}
				hubResourcePackage.downloadQueuedCallback = (HubResourcePackage.DownloadQueuedCallback)Delegate.Combine(hubResourcePackage.downloadQueuedCallback, new HubResourcePackage.DownloadQueuedCallback(ResourceDownloadQueued));
				hubResourcePackage.downloadCompleteCallback = (HubResourcePackage.DownloadCompleteCallback)Delegate.Combine(hubResourcePackage.downloadCompleteCallback, new HubResourcePackage.DownloadCompleteCallback(ResourceDownloadComplete));
				hubResourcePackage.downloadErrorCallback = (HubResourcePackage.DownloadErrorCallback)Delegate.Combine(hubResourcePackage.downloadErrorCallback, new HubResourcePackage.DownloadErrorCallback(ResourceDownloadError));
				if (download)
				{
					hubResourcePackage.Download();
				}
			}
		}

		protected void ResourceDetailErrorCallback(string err, string resource_id, bool suppressLogError = false)
		{
			string text = "Error " + err + " during get resource detail for " + resource_id;
			if (!suppressLogError)
			{
				SuperController.LogError(text);
			}
			errors.Add(text);
			pendingResourceDetailCallbacks--;
		}

		public void FindResource(string resourceId, bool download = false)
		{
			JSONClass jSONClass = new JSONClass();
			jSONClass["source"] = "VaM";
			jSONClass["action"] = "getResourceDetail";
			jSONClass["latest_image"] = "Y";
			jSONClass["resource_id"] = resourceId;
			string postData = jSONClass.ToString();
			pendingResourceDetailCallbacks++;
			StartCoroutine(PostRequest(apiUrl, postData, delegate(JSONNode jsonNode)
			{
				ResourceDetailCallback(jsonNode, download);
			}, delegate(string err)
			{
				ResourceDetailErrorCallback(err, resourceId);
			}));
		}

		public void FindPackage(string packageName, bool download = false)
		{
			JSONClass jSONClass = new JSONClass();
			jSONClass["source"] = "VaM";
			jSONClass["action"] = "getResourceDetail";
			jSONClass["latest_image"] = "Y";
			jSONClass["package_name"] = packageName;
			string postData = jSONClass.ToString();
			pendingResourceDetailCallbacks++;
			StartCoroutine(PostRequest(apiUrl, postData, delegate(JSONNode jsonNode)
			{
				ResourceDetailCallback(jsonNode, download);
			}, delegate(string err)
			{
				ResourceDetailErrorCallback(err, packageName);
			}));
		}

		protected void FindPackage()
		{
			if (!string.IsNullOrEmpty(packageNameString.val))
			{
				if (packageNameString.val.StartsWith("https://hub.virtamate.com"))
				{
					string val = packageNameString.val;
					Debug.Log("Resource id is " + val);
					val = Regex.Replace(val, ".*\\.([0-9]+)/?", "$1");
					Debug.Log("Got resource id " + val);
					FindResource(val);
				}
				else
				{
					FindPackage(packageNameString.val);
				}
			}
		}

		protected void DownloadAllTrackedPackages()
		{
			if (trackedPackages == null)
			{
				return;
			}
			foreach (HubResourcePackageUI trackedPackage in trackedPackages)
			{
				trackedPackage.connectedItem.Download();
			}
		}

		public bool DownloadPackages(SuccessCallback successCallback, ErrorCallback errorCallback, params string[] packagesList)
		{
			bool result = false;
			if (_hubDownloaderEnabled)
			{
				if (!processingMassDownload)
				{
					errors.Clear();
					processingMassDownload = true;
					massDownloadSuccessCallback = successCallback;
					massDownloadErrorCallback = errorCallback;
					foreach (string package in packagesList)
					{
						JSONClass jSONClass = new JSONClass();
						jSONClass["source"] = "VaM";
						jSONClass["action"] = "getResourceDetail";
						jSONClass["latest_image"] = "Y";
						jSONClass["package_name"] = package;
						string postData = jSONClass.ToString();
						pendingResourceDetailCallbacks++;
						StartCoroutine(PostRequest(apiUrl, postData, delegate(JSONNode jsonNode)
						{
							ResourceDetailCallback(jsonNode, true);
						}, delegate(string err)
						{
							ResourceDetailErrorCallback(err, package, true);
						}));
						result = true;
					}
				}
				else
				{
					errorCallback?.Invoke("Cannot process DownloadPackages as mass download is already running");
				}
			}
			else
			{
				if (!hasPendingMassDownload)
				{
					hasPendingMassDownload = true;
					hasPendingDownloadPackages = true;
					pendingPackagesList = packagesList;
					pendingSuccessCallback = successCallback;
					pendingErrorCallback = errorCallback;
					return true;
				}
				errorCallback?.Invoke("Cannot process DownloadPackages as mass download is already running");
			}
			return result;
		}

		protected void FindMissingPackagesErrorCallback(string err)
		{
			string item = "Error during FindMissingPackages " + err;
			errors.Add(item);
			pendingFindMissingPackages = false;
		}

		protected void FindMissingPackagesCallback(JSONNode jsonNode)
		{
			pendingFindMissingPackages = false;
			if (!(jsonNode != null))
			{
				return;
			}
			JSONClass asObject = jsonNode.AsObject;
			if (!(asObject != null))
			{
				return;
			}
			string text = asObject["status"];
			if (text != null && text == "error")
			{
				string item = jsonNode["error"];
				errors.Add(item);
				return;
			}
			JSONClass asObject2 = jsonNode["packages"].AsObject;
			if (!(asObject2 != null))
			{
				return;
			}
			foreach (string checkMissingPackageName in checkMissingPackageNames)
			{
				JSONClass jSONClass = asObject2[checkMissingPackageName].AsObject;
				if (jSONClass == null)
				{
					jSONClass = new JSONClass();
					jSONClass["filename"] = checkMissingPackageName;
					jSONClass["downloadUrl"] = "null";
				}
				else
				{
					if (Regex.IsMatch(checkMissingPackageName, "[0-9]+$"))
					{
						string text2 = jSONClass["filename"];
						if (text2 == null || text2 == "null" || text2 != checkMissingPackageName + ".var")
						{
							Debug.LogError("Missing file name " + text2 + " does not match missing package name " + checkMissingPackageName);
							jSONClass["filename"] = checkMissingPackageName;
							jSONClass["file_size"] = "null";
							jSONClass["licenseType"] = "null";
							jSONClass["downloadUrl"] = "null";
						}
					}
					else
					{
						string text3 = jSONClass["filename"];
						if (text3 == null || text3 == "null")
						{
							jSONClass["filename"] = checkMissingPackageName;
						}
					}
					string text4 = jSONClass["resource_id"];
					if (text4 == null || text4 == "null")
					{
						jSONClass["downloadUrl"] = "null";
					}
				}
				HubResourcePackage hubResourcePackage = new HubResourcePackage(jSONClass, hubBrowse, true);
				RectTransform rectTransform = hubBrowse.CreateDownloadPrefabInstance();
				if (rectTransform != null)
				{
					rectTransform.SetParent(packagesContainer, false);
					HubResourcePackageUI component = rectTransform.GetComponent<HubResourcePackageUI>();
					if (component != null)
					{
						trackedPackages.Add(component);
						hubResourcePackage.RegisterUI(component);
						hubResourcePackage.downloadQueuedCallback = (HubResourcePackage.DownloadQueuedCallback)Delegate.Combine(hubResourcePackage.downloadQueuedCallback, new HubResourcePackage.DownloadQueuedCallback(ResourceDownloadQueued));
						hubResourcePackage.downloadCompleteCallback = (HubResourcePackage.DownloadCompleteCallback)Delegate.Combine(hubResourcePackage.downloadCompleteCallback, new HubResourcePackage.DownloadCompleteCallback(ResourceDownloadComplete));
						hubResourcePackage.downloadErrorCallback = (HubResourcePackage.DownloadErrorCallback)Delegate.Combine(hubResourcePackage.downloadErrorCallback, new HubResourcePackage.DownloadErrorCallback(ResourceDownloadError));
						hubResourcePackage.Download();
					}
				}
			}
		}

		public bool DownloadAllMissingPackages(SuccessCallback successCallback, ErrorCallback errorCallback)
		{
			if (packageManager != null)
			{
				if (_hubDownloaderEnabled)
				{
					List<string> missingPackageNames = packageManager.MissingPackageNames;
					if (missingPackageNames.Count > 0)
					{
						if (!processingMassDownload)
						{
							errors.Clear();
							processingMassDownload = true;
							massDownloadSuccessCallback = successCallback;
							massDownloadErrorCallback = errorCallback;
							JSONClass jSONClass = new JSONClass();
							jSONClass["source"] = "VaM";
							jSONClass["action"] = "findPackages";
							checkMissingPackageNames = missingPackageNames;
							jSONClass["packages"] = string.Join(",", missingPackageNames.ToArray());
							string postData = jSONClass.ToString();
							pendingFindMissingPackages = true;
							StartCoroutine(PostRequest(apiUrl, postData, FindMissingPackagesCallback, FindMissingPackagesErrorCallback));
							return true;
						}
						errorCallback?.Invoke("Cannot process DownloadAllMissingPackages as mass download is already running");
					}
					else
					{
						successCallback?.Invoke();
					}
				}
				else
				{
					if (!hasPendingMassDownload)
					{
						hasPendingMassDownload = true;
						hasPendingDownloadAllMissingPackages = true;
						pendingSuccessCallback = successCallback;
						pendingErrorCallback = errorCallback;
						return true;
					}
					errorCallback?.Invoke("Cannot process DownloadAllMissingPackages as mass download is already running");
				}
			}
			return false;
		}

		protected void OnPackageRefresh()
		{
			foreach (HubResourcePackageUI trackedPackage in trackedPackages)
			{
				if (trackedPackage != null)
				{
					trackedPackage.connectedItem.Refresh();
				}
			}
			FileManager.SyncAutoAlwaysAllowPlugins(packagesThatWereDownloaded);
		}

		protected void SyncHubDownloaderEnabled(bool b)
		{
			_hubDownloaderEnabled = b;
			if (_hubDownloaderEnabled && hasPendingMassDownload)
			{
				hasPendingMassDownload = false;
				if (hasPendingDownloadPackages)
				{
					hasPendingDownloadPackages = false;
					DownloadPackages(pendingSuccessCallback, pendingErrorCallback, pendingPackagesList);
					pendingSuccessCallback = null;
					pendingErrorCallback = null;
				}
				else if (hasPendingDownloadAllMissingPackages)
				{
					hasPendingDownloadAllMissingPackages = false;
					DownloadAllMissingPackages(pendingSuccessCallback, pendingErrorCallback);
					pendingSuccessCallback = null;
					pendingErrorCallback = null;
				}
			}
		}

		protected void EnableHubDownloader()
		{
			UserPreferences.singleton.enableHubDownloader = true;
		}

		protected void RejectHubDownloader()
		{
			if (hasPendingMassDownload)
			{
				if (pendingErrorCallback != null)
				{
					pendingErrorCallback("User rejected downloader enable");
				}
				hasPendingMassDownload = false;
				hasPendingDownloadPackages = false;
				hasPendingDownloadAllMissingPackages = false;
				pendingSuccessCallback = null;
				pendingErrorCallback = null;
			}
			ClosePanel();
		}

		protected void Init()
		{
			singleton = this;
			errors = new List<string>();
			packagesThatWereDownloaded = new HashSet<FileManager.PackageUIDAndPublisher>();
			trackedPackages = new List<HubResourcePackageUI>();
			FileManager.RegisterRefreshHandler(OnPackageRefresh);
			openPanelAction = new JSONStorableAction("OpenPanel", OpenPanel);
			RegisterAction(openPanelAction);
			closePanelAction = new JSONStorableAction("ClosePanel", ClosePanel);
			RegisterAction(closePanelAction);
			clearTrackedPackagesAction = new JSONStorableAction("ClearTrackedPackages", ClearTrackedPackages);
			RegisterAction(clearTrackedPackagesAction);
			packageNameString = new JSONStorableString("packageName", string.Empty);
			packageNameString.isStorable = false;
			packageNameString.isRestorable = false;
			RegisterString(packageNameString);
			findPackageAction = new JSONStorableAction("FindPackage", FindPackage);
			RegisterAction(findPackageAction);
			downloadAllTrackedPackagesAction = new JSONStorableAction("DownloadAllTrackedPackages", DownloadAllTrackedPackages);
			RegisterAction(downloadAllTrackedPackagesAction);
			hubDownloaderEnabledJSON = new JSONStorableBool("hubDownloaderEnabled", _hubDownloaderEnabled, SyncHubDownloaderEnabled);
			enableHubDownloaderAction = new JSONStorableAction("EnableHubDownloader", EnableHubDownloader);
			rejectHubDownloaderAction = new JSONStorableAction("RejectHubDownloader", RejectHubDownloader);
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			if (!(t != null))
			{
				return;
			}
			HubDownloaderUI componentInChildren = t.GetComponentInChildren<HubDownloaderUI>();
			if (componentInChildren != null)
			{
				if (!isAlt)
				{
					panel = componentInChildren.panel;
					packagesContainer = componentInChildren.packagesContainer;
					infoText = componentInChildren.infoText;
					downloadInfoText = componentInChildren.downloadInfoText;
				}
				openPanelAction.RegisterButton(componentInChildren.openPanelButton, isAlt);
				closePanelAction.RegisterButton(componentInChildren.closePanelButton, isAlt);
				clearTrackedPackagesAction.RegisterButton(componentInChildren.clearTrackedPackagesButton, isAlt);
				packageNameString.RegisterInputField(componentInChildren.packageNameInputField, isAlt);
				findPackageAction.RegisterButton(componentInChildren.findPackageButton, isAlt);
				downloadAllTrackedPackagesAction.RegisterButton(componentInChildren.downloadAllTrackedPackagesButton, isAlt);
				hubDownloaderEnabledJSON.RegisterNegativeIndicator(componentInChildren.disabledIndicator, isAlt);
				enableHubDownloaderAction.RegisterButton(componentInChildren.enableHubDownloaderButton, isAlt);
				rejectHubDownloaderAction.RegisterButton(componentInChildren.rejectHubDownloaderButton, isAlt);
			}
		}

		protected override void Awake()
		{
			if (!awakecalled)
			{
				base.Awake();
				Init();
				InitUI();
				InitUIAlt();
			}
		}

		protected void Update()
		{
			if (processingMassDownload && !pendingFindMissingPackages && pendingResourceDetailCallbacks == 0 && pendingResourceDownloads == 0)
			{
				Debug.Log("Mass download complete");
				processingMassDownload = false;
				if (errors.Count > 0)
				{
					Debug.Log("Mass download had errors");
					if (massDownloadErrorCallback != null)
					{
						massDownloadErrorCallback(string.Join("\n", errors.ToArray()));
					}
				}
				else if (massDownloadSuccessCallback != null)
				{
					Debug.Log("Mass download success");
					massDownloadSuccessCallback();
				}
			}
			if (downloadInfoText != null)
			{
				if (pendingResourceDownloads > 0)
				{
					downloadInfoText.text = "Queued downloads count: " + pendingResourceDownloads;
				}
				else
				{
					downloadInfoText.text = "No queued downloads";
				}
			}
		}
	}
}
