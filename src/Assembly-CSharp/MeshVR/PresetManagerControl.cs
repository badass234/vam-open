using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

namespace MeshVR
{
	public class PresetManagerControl : JSONStorable
	{
		protected PresetManager pm;

		public PresetManagerControlOverview controlOverview;

		public bool setMaterialOptionsTexturePaths = true;

		protected bool forceUseRegularLoad;

		protected bool forceUseMergeLoad;

		protected bool forceLoadOnPresetBrowsePathSync;

		protected JSONStorableUrl presetBrowsePathJSON;

		protected JSONStorableAction openPresetBrowsePathInExplorerAction;

		protected JSONStorableString presetNameJSON;

		protected JSONStorableBool storePresetNameJSON;

		protected JSONStorableString loadPresetWithNameJSON;

		protected JSONStorableString mergeLoadPresetWithNameJSON;

		protected JSONStorableActionPresetFilePath loadPresetWithPathJSON;

		protected JSONStorableUrl loadPresetWithPathUrlJSON;

		protected JSONStorableActionPresetFilePath mergeLoadPresetWithPathJSON;

		protected JSONStorableUrl mergeLoadPresetWithPathUrlJSON;

		protected JSONStorableString storePresetWithNameJSON;

		protected JSONStorableBool favoriteJSON;

		protected JSONStorableAction storePresetAction;

		protected JSONStorableAction storePresetWithScreenshotAction;

		protected JSONStorableAction storeOverlayPresetAction;

		protected JSONStorableAction storeOverlayPresetWithScreenshotAction;

		protected JSONStorableBool storeOptionalJSON;

		protected JSONStorableBool storeOptional2JSON;

		protected JSONStorableBool storeOptional3JSON;

		protected JSONStorableBool storePresetBinaryJSON;

		protected bool isLoadingPreset;

		protected JSONStorableAction loadPresetAction;

		protected JSONStorableAction mergeLoadPresetAction;

		protected JSONStorableAction autoTypeLoadPresetAction;

		protected JSONStorableAction loadDefaultsAction;

		protected JSONStorableAction loadUserDefaultsAction;

		protected JSONStorableAction storeUserDefaultsAction;

		protected JSONStorableAction clearUserDefaultsAction;

		protected Text statusText;

		protected Text statusTextAlt;

		protected JSONStorableBool loadPresetOnSelectJSON;

		public bool showMergeLoad = true;

		protected JSONStorableBool useMergeLoadJSON;

		protected JSONStorableStringChooser favoriteSelectionJSON;

		protected JSONStorableBool includeOptionalJSON;

		protected JSONStorableBool includeOptional2JSON;

		protected JSONStorableBool includeOptional3JSON;

		protected JSONStorableBool includeAppearanceJSON;

		protected JSONStorableBool includePhysicalJSON;

		protected JSONStorableBool lockParamsJSON;

		public bool lockParams
		{
			get
			{
				return lockParamsJSON.val;
			}
			set
			{
				lockParamsJSON.val = value;
			}
		}

		protected void SyncPresetBrowsePath(string url)
		{
			if (!(pm != null) || url == null || !(url != string.Empty))
			{
				return;
			}
			string presetNameFromFilePath = pm.GetPresetNameFromFilePath(url);
			if (presetNameFromFilePath == null)
			{
				return;
			}
			presetNameJSON.val = presetNameFromFilePath;
			if ((forceLoadOnPresetBrowsePathSync || (loadPresetOnSelectJSON != null && loadPresetOnSelectJSON.val)) && pm.CheckPresetExistance())
			{
				if (forceUseMergeLoad)
				{
					MergeLoadPreset();
				}
				else if (forceUseRegularLoad)
				{
					LoadPreset();
				}
				else if (useMergeLoadJSON != null && useMergeLoadJSON.val)
				{
					MergeLoadPreset();
				}
				else
				{
					LoadPreset();
				}
				presetNameJSON.val = presetNameFromFilePath;
			}
		}

		public void OpenPresetBrowsePathInExplorer()
		{
			SuperController.singleton.OpenFolderInExplorer(presetBrowsePathJSON.val);
		}

		protected void SyncPresetLoadButton()
		{
			if (pm.CheckPresetExistance())
			{
				if (autoTypeLoadPresetAction != null && autoTypeLoadPresetAction.dynamicButton != null && autoTypeLoadPresetAction.dynamicButton.button != null)
				{
					autoTypeLoadPresetAction.dynamicButton.button.interactable = true;
				}
			}
			else if (autoTypeLoadPresetAction != null && autoTypeLoadPresetAction.dynamicButton != null && autoTypeLoadPresetAction.dynamicButton.button != null)
			{
				autoTypeLoadPresetAction.dynamicButton.button.interactable = false;
			}
		}

		protected void SyncPresetStoreButton()
		{
			if (pm.CheckPresetReadyForStore())
			{
				if (pm.CheckPresetExistance())
				{
					if (storePresetAction != null && storePresetAction.dynamicButton != null)
					{
						storePresetAction.dynamicButton.buttonColor = Color.red;
						if (storePresetAction.dynamicButton.button != null)
						{
							storePresetAction.dynamicButton.button.interactable = true;
						}
						if (storePresetAction.dynamicButton.buttonText != null)
						{
							storePresetAction.dynamicButton.buttonText.text = "Overwrite Preset";
						}
					}
					if (storePresetWithScreenshotAction != null && storePresetWithScreenshotAction.dynamicButton != null)
					{
						storePresetWithScreenshotAction.dynamicButton.buttonColor = Color.red;
						if (storePresetWithScreenshotAction.dynamicButton.button != null)
						{
							storePresetWithScreenshotAction.dynamicButton.button.interactable = true;
						}
						if (storePresetWithScreenshotAction.dynamicButton.buttonText != null)
						{
							storePresetWithScreenshotAction.dynamicButton.buttonText.text = "Overwrite Preset";
						}
					}
					if (storeOverlayPresetAction != null && storeOverlayPresetAction.dynamicButton != null)
					{
						storeOverlayPresetAction.dynamicButton.buttonColor = Color.red;
						if (storeOverlayPresetAction.dynamicButton.button != null)
						{
							storeOverlayPresetAction.dynamicButton.button.interactable = true;
						}
						if (storeOverlayPresetAction.dynamicButton.buttonText != null)
						{
							storeOverlayPresetAction.dynamicButton.buttonText.text = "Overwrite Overlay Preset";
						}
					}
					if (storeOverlayPresetWithScreenshotAction != null && storeOverlayPresetWithScreenshotAction.dynamicButton != null)
					{
						storeOverlayPresetWithScreenshotAction.dynamicButton.buttonColor = Color.red;
						if (storeOverlayPresetWithScreenshotAction.dynamicButton.button != null)
						{
							storeOverlayPresetWithScreenshotAction.dynamicButton.button.interactable = true;
						}
						if (storeOverlayPresetWithScreenshotAction.dynamicButton.buttonText != null)
						{
							storeOverlayPresetWithScreenshotAction.dynamicButton.buttonText.text = "Overwrite Overlay Preset";
						}
					}
					return;
				}
				if (storePresetAction != null && storePresetAction.dynamicButton != null)
				{
					storePresetAction.dynamicButton.buttonColor = Color.green;
					if (storePresetAction.dynamicButton.button != null)
					{
						storePresetAction.dynamicButton.button.interactable = true;
					}
					if (storePresetAction.dynamicButton.buttonText != null)
					{
						storePresetAction.dynamicButton.buttonText.text = "Create New Preset";
					}
				}
				if (storePresetWithScreenshotAction != null && storePresetWithScreenshotAction.dynamicButton != null)
				{
					storePresetWithScreenshotAction.dynamicButton.buttonColor = Color.green;
					if (storePresetWithScreenshotAction.dynamicButton.button != null)
					{
						storePresetWithScreenshotAction.dynamicButton.button.interactable = true;
					}
					if (storePresetWithScreenshotAction.dynamicButton.buttonText != null)
					{
						storePresetWithScreenshotAction.dynamicButton.buttonText.text = "Create New Preset";
					}
				}
				if (storeOverlayPresetAction != null && storeOverlayPresetAction.dynamicButton != null)
				{
					storeOverlayPresetAction.dynamicButton.buttonColor = Color.green;
					if (storeOverlayPresetAction.dynamicButton.button != null)
					{
						storeOverlayPresetAction.dynamicButton.button.interactable = true;
					}
					if (storeOverlayPresetAction.dynamicButton.buttonText != null)
					{
						storeOverlayPresetAction.dynamicButton.buttonText.text = "Create New Overlay Preset";
					}
				}
				if (storeOverlayPresetWithScreenshotAction != null && storeOverlayPresetWithScreenshotAction.dynamicButton != null)
				{
					storeOverlayPresetWithScreenshotAction.dynamicButton.buttonColor = Color.green;
					if (storeOverlayPresetWithScreenshotAction.dynamicButton.button != null)
					{
						storeOverlayPresetWithScreenshotAction.dynamicButton.button.interactable = true;
					}
					if (storeOverlayPresetWithScreenshotAction.dynamicButton.buttonText != null)
					{
						storeOverlayPresetWithScreenshotAction.dynamicButton.buttonText.text = "Create New Overlay Preset";
					}
				}
				return;
			}
			bool flag = pm.IsPresetInPackage();
			if (storePresetAction != null && storePresetAction.dynamicButton != null)
			{
				storePresetAction.dynamicButton.buttonColor = Color.gray;
				if (storePresetAction.dynamicButton.button != null)
				{
					storePresetAction.dynamicButton.button.interactable = false;
				}
				if (storePresetAction.dynamicButton.buttonText != null)
				{
					if (flag)
					{
						storePresetAction.dynamicButton.buttonText.text = "Unavailable...Package Preset";
					}
					else
					{
						storePresetAction.dynamicButton.buttonText.text = "Create New Preset";
					}
				}
			}
			if (storePresetWithScreenshotAction != null && storePresetWithScreenshotAction.dynamicButton != null)
			{
				storePresetWithScreenshotAction.dynamicButton.buttonColor = Color.gray;
				if (storePresetWithScreenshotAction.dynamicButton.button != null)
				{
					storePresetWithScreenshotAction.dynamicButton.button.interactable = false;
				}
				if (storePresetWithScreenshotAction.dynamicButton.buttonText != null)
				{
					if (flag)
					{
						storePresetWithScreenshotAction.dynamicButton.buttonText.text = "Unavailable...Package Preset";
					}
					else
					{
						storePresetWithScreenshotAction.dynamicButton.buttonText.text = "Create New Preset";
					}
				}
			}
			if (storeOverlayPresetAction != null && storeOverlayPresetAction.dynamicButton != null)
			{
				storeOverlayPresetAction.dynamicButton.buttonColor = Color.gray;
				if (storeOverlayPresetAction.dynamicButton.button != null)
				{
					storeOverlayPresetAction.dynamicButton.button.interactable = false;
				}
				if (storeOverlayPresetAction.dynamicButton.buttonText != null)
				{
					if (flag)
					{
						storeOverlayPresetAction.dynamicButton.buttonText.text = "Unavailable...Package Preset";
					}
					else
					{
						storeOverlayPresetAction.dynamicButton.buttonText.text = "Create New Overlay Preset";
					}
				}
			}
			if (storeOverlayPresetWithScreenshotAction == null || !(storeOverlayPresetWithScreenshotAction.dynamicButton != null))
			{
				return;
			}
			storeOverlayPresetWithScreenshotAction.dynamicButton.buttonColor = Color.gray;
			if (storeOverlayPresetWithScreenshotAction.dynamicButton.button != null)
			{
				storeOverlayPresetWithScreenshotAction.dynamicButton.button.interactable = false;
			}
			if (storeOverlayPresetWithScreenshotAction.dynamicButton.buttonText != null)
			{
				if (flag)
				{
					storeOverlayPresetWithScreenshotAction.dynamicButton.buttonText.text = "Unavailable...Package Preset";
				}
				else
				{
					storeOverlayPresetWithScreenshotAction.dynamicButton.buttonText.text = "Create New Overlay Preset";
				}
			}
		}

		protected void SyncPresetName(string s)
		{
			if (pm != null)
			{
				if (s.Contains("\\"))
				{
					presetNameJSON.val = s.Replace("\\", "/");
					return;
				}
				pm.presetName = s;
				favoriteJSON.valNoCallback = pm.IsFavorite();
				SyncPresetLoadButton();
				SyncPresetStoreButton();
			}
		}

		protected void SyncStorePresetName(bool b)
		{
			presetNameJSON.isStorable = b;
		}

		protected void SyncLoadPresetWithName(string s)
		{
			if (pm != null)
			{
				loadPresetWithNameJSON.valNoCallback = string.Empty;
				pm.presetName = s;
				favoriteJSON.valNoCallback = pm.IsFavorite();
				SyncPresetLoadButton();
				SyncPresetStoreButton();
				LoadPreset();
			}
		}

		protected void SyncMergeLoadPresetWithName(string s)
		{
			if (pm != null)
			{
				mergeLoadPresetWithNameJSON.valNoCallback = string.Empty;
				pm.presetName = s;
				favoriteJSON.valNoCallback = pm.IsFavorite();
				SyncPresetLoadButton();
				SyncPresetStoreButton();
				MergeLoadPreset();
			}
		}

		protected void LoadPresetWithPath(string p)
		{
			forceLoadOnPresetBrowsePathSync = true;
			forceUseRegularLoad = true;
			presetBrowsePathJSON.SetFilePath(p);
			forceUseRegularLoad = false;
			forceLoadOnPresetBrowsePathSync = false;
		}

		protected void MergeLoadPresetWithPath(string p)
		{
			forceLoadOnPresetBrowsePathSync = true;
			forceUseMergeLoad = true;
			presetBrowsePathJSON.SetFilePath(p);
			forceUseMergeLoad = false;
			forceLoadOnPresetBrowsePathSync = false;
		}

		protected void SyncStorePresetWithName(string s)
		{
			if (pm != null)
			{
				storePresetWithNameJSON.valNoCallback = string.Empty;
				pm.presetName = s;
				favoriteJSON.valNoCallback = pm.IsFavorite();
				SyncPresetLoadButton();
				SyncPresetStoreButton();
				StorePreset();
			}
		}

		protected void SyncFavorite(bool b)
		{
			pm.SetFavorite(b);
			RefreshFavoriteNames();
		}

		protected virtual void StorePreset()
		{
			StorePreset(false);
		}

		protected virtual void StorePreset(bool skipSync)
		{
			if (!(pm != null))
			{
				return;
			}
			if (pm.StorePreset(true, false))
			{
				if (!skipSync)
				{
					SyncFavorite(favoriteJSON.val);
					SyncPresetUI();
				}
				SetStatus("Stored preset " + pm.presetName);
			}
			else
			{
				SetStatus("Failed to store preset " + pm.presetName);
			}
		}

		protected virtual void StorePresetWithScreenshot()
		{
			StorePresetWithScreenshot(false);
		}

		protected virtual void StorePresetWithScreenshot(bool skipSync)
		{
			if (!(pm != null))
			{
				return;
			}
			if (pm.StorePreset(true, true))
			{
				if (!skipSync)
				{
					SyncFavorite(favoriteJSON.val);
					SyncPresetUI();
				}
				SetStatus("Stored preset " + pm.presetName);
			}
			else
			{
				SetStatus("Failed to store preset " + pm.presetName);
			}
		}

		protected virtual void StoreOverlayPreset()
		{
			StoreOverlayPreset(false);
		}

		protected virtual void StoreOverlayPreset(bool skipSync)
		{
			if (!(pm != null))
			{
				return;
			}
			if (pm.StorePreset(false, false))
			{
				if (!skipSync)
				{
					SyncFavorite(favoriteJSON.val);
					SyncPresetUI();
				}
				SetStatus("Stored preset " + pm.presetName);
			}
			else
			{
				SetStatus("Failed to store preset " + pm.presetName);
			}
		}

		protected virtual void StoreOverlayPresetWithScreenshot()
		{
			StoreOverlayPresetWithScreenshot(false);
		}

		protected virtual void StoreOverlayPresetWithScreenshot(bool skipSync)
		{
			if (!(pm != null))
			{
				return;
			}
			if (pm.StorePreset(false, true))
			{
				if (!skipSync)
				{
					SyncFavorite(favoriteJSON.val);
					SyncPresetUI();
				}
				SetStatus("Stored preset " + pm.presetName);
			}
			else
			{
				SetStatus("Failed to store preset " + pm.presetName);
			}
		}

		protected void SyncStoreOptional(bool b)
		{
			if (pm != null)
			{
				pm.storeOptionalStorables = b;
				pm.SyncParamsLocked();
			}
		}

		protected void SyncStoreOptional2(bool b)
		{
			if (pm != null)
			{
				pm.storeOptionalStorables2 = b;
				pm.SyncParamsLocked();
			}
		}

		protected void SyncStoreOptional3(bool b)
		{
			if (pm != null)
			{
				pm.storeOptionalStorables3 = b;
				pm.SyncParamsLocked();
			}
		}

		protected void SyncStorePresetBinary(bool b)
		{
			if (pm != null)
			{
				pm.storePresetBinary = b;
			}
		}

		protected virtual void LoadPreset()
		{
			if (!(pm != null))
			{
				return;
			}
			isLoadingPreset = true;
			if (pm.LoadPresetPre())
			{
				if ((pm.itemType == PresetManager.ItemType.Atom || pm.itemType == PresetManager.ItemType.Custom) && containingAtom != null)
				{
					containingAtom.SetLastRestoredData(pm.lastLoadedJSON, pm.includeAppearance, pm.includePhysical);
				}
				if (pm.LoadPresetPost())
				{
					SetStatus("Loaded preset " + pm.presetName);
				}
				else
				{
					SetStatus("Failed to load preset " + pm.presetName);
				}
			}
			else
			{
				SetStatus("Failed to load preset " + pm.presetName);
			}
			isLoadingPreset = false;
			RefreshFavoriteNames();
		}

		protected virtual void MergeLoadPreset()
		{
			if (!(pm != null))
			{
				return;
			}
			isLoadingPreset = true;
			if (pm.LoadPresetPre(true))
			{
				if ((pm.itemType == PresetManager.ItemType.Atom || pm.itemType == PresetManager.ItemType.Custom) && containingAtom != null)
				{
					containingAtom.SetLastRestoredData(pm.lastLoadedJSON, pm.includeAppearance, pm.includePhysical);
				}
				if (pm.LoadPresetPost())
				{
					SetStatus("Merge loaded preset " + pm.presetName);
				}
				else
				{
					SetStatus("Failed to load preset " + pm.presetName);
				}
			}
			else
			{
				SetStatus("Failed to load preset " + pm.presetName);
			}
			isLoadingPreset = false;
			RefreshFavoriteNames();
		}

		protected virtual void AutoTypeLoadPreset()
		{
			if (useMergeLoadJSON != null && useMergeLoadJSON.val)
			{
				MergeLoadPreset();
			}
			else
			{
				LoadPreset();
			}
		}

		protected virtual void LoadDefaults()
		{
			if (!(pm != null))
			{
				return;
			}
			isLoadingPreset = true;
			if (pm.LoadDefaultsPre())
			{
				if ((pm.itemType == PresetManager.ItemType.Atom || pm.itemType == PresetManager.ItemType.Custom) && containingAtom != null)
				{
					containingAtom.SetLastRestoredData(pm.lastLoadedJSON, pm.includeAppearance, pm.includePhysical);
				}
				if (pm.LoadPresetPost())
				{
					pm.RestorePresetBinary();
					SetStatus("Loaded defaults");
				}
				else
				{
					SetStatus("Failed to load defaults");
				}
			}
			else
			{
				SetStatus("Failed to load defaults");
			}
			isLoadingPreset = false;
		}

		public void LoadUserDefaults()
		{
			string presetName = pm.presetName;
			string package = pm.package;
			pm.presetName = "UserDefaults";
			if (pm.CheckPresetExistance())
			{
				LoadPreset();
			}
			pm.presetName = presetName;
			pm.package = package;
		}

		public void StoreUserDefaults()
		{
			string presetName = pm.presetName;
			string package = pm.package;
			pm.presetName = "UserDefaults";
			if (pm.CheckPresetReadyForStore())
			{
				StorePresetWithScreenshot(true);
			}
			pm.presetName = presetName;
			pm.package = package;
		}

		public void ClearUserDefaults()
		{
			string presetName = pm.presetName;
			string package = pm.package;
			pm.presetName = "UserDefaults";
			pm.DeletePreset();
			pm.presetName = presetName;
			pm.package = package;
		}

		protected virtual void SetStatus(string status)
		{
			if (statusText != null)
			{
				statusText.text = status;
			}
			if (statusTextAlt != null)
			{
				statusTextAlt.text = status;
			}
		}

		protected void SyncFavoriteSelection(string s)
		{
			favoriteSelectionJSON.valNoCallback = string.Empty;
			presetNameJSON.val = s;
			if (pm != null && loadPresetOnSelectJSON != null && loadPresetOnSelectJSON.val && pm.CheckPresetExistance())
			{
				if (useMergeLoadJSON != null && useMergeLoadJSON.val)
				{
					MergeLoadPreset();
				}
				else
				{
					LoadPreset();
				}
				presetNameJSON.val = s;
			}
		}

		protected void RefreshFavoriteNames()
		{
			if (pm != null && favoriteSelectionJSON != null)
			{
				favoriteSelectionJSON.choices = pm.FindFavoriteNames();
			}
		}

		protected void SyncIncludeOptional(bool b)
		{
			if (pm != null)
			{
				pm.includeOptional = b;
				pm.SyncParamsLocked();
			}
		}

		protected void SyncIncludeOptional2(bool b)
		{
			if (pm != null)
			{
				pm.includeOptional2 = b;
				pm.SyncParamsLocked();
			}
		}

		protected void SyncIncludeOptional3(bool b)
		{
			if (pm != null)
			{
				pm.includeOptional3 = b;
				pm.SyncParamsLocked();
			}
		}

		protected void SyncIncludeAppearance(bool b)
		{
			if (pm != null)
			{
				pm.includeAppearance = b;
				pm.SyncParamsLocked();
			}
		}

		protected void SyncIncludePhysical(bool b)
		{
			if (pm != null)
			{
				pm.includePhysical = b;
				pm.SyncParamsLocked();
			}
		}

		protected void SyncLockParams(bool b)
		{
			if (controlOverview != null)
			{
				controlOverview.SyncPresetManagerControlLockParams(this);
			}
			if (pm != null)
			{
				pm.paramsLocked = b;
			}
		}

		protected void BeginBrowse(JSONStorableUrl jsurl)
		{
			if (pm != null)
			{
				jsurl.fileRemovePrefix = pm.storeName + "_";
				pm.CreateStoreFolderPath();
				string storeFolderPath = pm.GetStoreFolderPath(false);
				string input = storeFolderPath;
				input = Regex.Replace(input, "/$", string.Empty);
				jsurl.suggestedPath = input;
				List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(storeFolderPath, false, false, true, true);
				jsurl.shortCuts = shortCutsForDirectory;
				if (useMergeLoadJSON.toggleAlt != null)
				{
					useMergeLoadJSON.toggleAlt.gameObject.SetActive(showMergeLoad);
				}
			}
		}

		protected void EndBrowse(JSONStorableUrl jsurl)
		{
			if (useMergeLoadJSON.toggleAlt != null)
			{
				useMergeLoadJSON.toggleAlt.gameObject.SetActive(false);
			}
		}

		public void SyncPresetUI()
		{
			RefreshFavoriteNames();
			SyncPresetLoadButton();
			SyncPresetStoreButton();
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			base.InitUI(t, isAlt);
			if (!(t != null))
			{
				return;
			}
			PresetManagerControlUI componentInChildren = t.GetComponentInChildren<PresetManagerControlUI>(true);
			if (!(pm != null) || !(componentInChildren != null))
			{
				return;
			}
			presetBrowsePathJSON.RegisterFileBrowseButton(componentInChildren.browsePresetsButton, isAlt);
			openPresetBrowsePathInExplorerAction.RegisterButton(componentInChildren.openPresetBrowsePathInExplorerButton, isAlt);
			presetNameJSON.RegisterInputField(componentInChildren.presetNameField, isAlt);
			storePresetNameJSON.RegisterToggle(componentInChildren.storePresetNameToggle, isAlt);
			favoriteJSON.RegisterToggle(componentInChildren.favoriteToggle, isAlt);
			storePresetAction.RegisterButton(componentInChildren.storePresetButton, isAlt);
			storePresetWithScreenshotAction.RegisterButton(componentInChildren.storePresetWithScreenshotButton, isAlt);
			storeOverlayPresetAction.RegisterButton(componentInChildren.storeOverlayPresetButton, isAlt);
			storeOverlayPresetWithScreenshotAction.RegisterButton(componentInChildren.storeOverlayPresetWithScreenshotButton, isAlt);
			storeOptionalJSON.RegisterToggle(componentInChildren.storeOptionalToggle, isAlt);
			storeOptional2JSON.RegisterToggle(componentInChildren.storeOptional2Toggle, isAlt);
			storeOptional3JSON.RegisterToggle(componentInChildren.storeOptional3Toggle, isAlt);
			storePresetBinaryJSON.RegisterToggle(componentInChildren.storePresetBinaryToggle, isAlt);
			autoTypeLoadPresetAction.RegisterButton(componentInChildren.loadPresetButton, isAlt);
			loadDefaultsAction.RegisterButton(componentInChildren.loadDefaultsButton, isAlt);
			loadUserDefaultsAction.RegisterButton(componentInChildren.loadUserDefaultsButton, isAlt);
			storeUserDefaultsAction.RegisterButton(componentInChildren.storeUserDefaultsButton, isAlt);
			clearUserDefaultsAction.RegisterButton(componentInChildren.clearUserDefaultsButton, isAlt);
			if (isAlt)
			{
				statusTextAlt = componentInChildren.statusText;
			}
			else
			{
				statusText = componentInChildren.statusText;
			}
			SyncPresetLoadButton();
			SyncPresetStoreButton();
			loadPresetOnSelectJSON.RegisterToggle(componentInChildren.loadPresetOnSelectToggle, isAlt);
			if (!isAlt)
			{
				useMergeLoadJSON.RegisterToggle(componentInChildren.useMergeLoadToggle);
				if (componentInChildren.useMergeLoadToggle != null)
				{
					componentInChildren.useMergeLoadToggle.gameObject.SetActive(showMergeLoad);
				}
				useMergeLoadJSON.RegisterToggle(componentInChildren.useMergeLoadBrowserToggle, true);
				if (componentInChildren.useMergeLoadBrowserToggle != null)
				{
					componentInChildren.useMergeLoadBrowserToggle.gameObject.SetActive(false);
				}
			}
			favoriteSelectionJSON.RegisterPopup(componentInChildren.favoriteSelectionPopup, isAlt);
			includeOptionalJSON.RegisterToggle(componentInChildren.includeOptionalToggle, isAlt);
			includeOptional2JSON.RegisterToggle(componentInChildren.includeOptional2Toggle, isAlt);
			includeOptional3JSON.RegisterToggle(componentInChildren.includeOptional3Toggle, isAlt);
			includeAppearanceJSON.RegisterToggle(componentInChildren.includeAppearanceToggle, isAlt);
			includePhysicalJSON.RegisterToggle(componentInChildren.includePhysicalToggle, isAlt);
			lockParamsJSON.RegisterToggle(componentInChildren.lockParamsToggle, isAlt);
		}

		public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
		{
			base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
			presetNameJSON.val = FileManager.NormalizeID(presetNameJSON.val);
			if (pm != null && !isLoadingPreset)
			{
				pm.RestorePresetBinary();
			}
		}

		protected virtual void Init()
		{
			pm = GetComponent<PresetManager>();
			if (!(pm != null))
			{
				return;
			}
			string text = string.Empty;
			if (pm.itemType == PresetManager.ItemType.Atom && containingAtom != null)
			{
				pm.customPath = containingAtom.type + "/";
				text = "Textures";
			}
			string storeFolderPath = pm.GetStoreFolderPath();
			if (storeFolderPath != null)
			{
				Directory.CreateDirectory(storeFolderPath);
				if (setMaterialOptionsTexturePaths)
				{
					MaterialOptions[] componentsInChildren = GetComponentsInChildren<MaterialOptions>(true);
					if (componentsInChildren != null && componentsInChildren.Length > 0)
					{
						string text2 = storeFolderPath + text;
						Directory.CreateDirectory(text2);
						MaterialOptions[] array = componentsInChildren;
						foreach (MaterialOptions materialOptions in array)
						{
							materialOptions.SetCustomTextureFolder(text2);
						}
					}
				}
			}
			presetBrowsePathJSON = new JSONStorableUrl("presetBrowsePath", string.Empty, SyncPresetBrowsePath, "vap", pm.GetStoreFolderPath(), true);
			presetBrowsePathJSON.beginBrowseWithObjectCallback = BeginBrowse;
			presetBrowsePathJSON.endBrowseWithObjectCallback = EndBrowse;
			presetBrowsePathJSON.allowFullComputerBrowse = false;
			presetBrowsePathJSON.allowBrowseAboveSuggestedPath = false;
			presetBrowsePathJSON.hideExtension = true;
			presetBrowsePathJSON.fileRemovePrefix = pm.storeName + "_";
			presetBrowsePathJSON.showDirs = true;
			presetBrowsePathJSON.isStorable = false;
			presetBrowsePathJSON.isRestorable = false;
			RegisterUrl(presetBrowsePathJSON);
			openPresetBrowsePathInExplorerAction = new JSONStorableAction("OpenPresetBrowsePathInExplorerAction", OpenPresetBrowsePathInExplorer);
			RegisterAction(openPresetBrowsePathInExplorerAction);
			presetNameJSON = new JSONStorableString("presetName", string.Empty, SyncPresetName);
			presetNameJSON.storeType = JSONStorableParam.StoreType.Full;
			presetNameJSON.isStorable = false;
			presetNameJSON.isRestorable = true;
			presetNameJSON.enableOnChange = true;
			RegisterString(presetNameJSON);
			storePresetNameJSON = new JSONStorableBool("storePresetName", false, SyncStorePresetName);
			storePresetNameJSON.storeType = JSONStorableParam.StoreType.Full;
			storePresetNameJSON.isStorable = true;
			storePresetNameJSON.isRestorable = true;
			RegisterBool(storePresetNameJSON);
			favoriteJSON = new JSONStorableBool("favorite", false, SyncFavorite);
			favoriteJSON.isStorable = false;
			favoriteJSON.isRestorable = false;
			RegisterBool(favoriteJSON);
			storePresetAction = new JSONStorableAction("StorePreset", StorePreset);
			RegisterAction(storePresetAction);
			storePresetWithScreenshotAction = new JSONStorableAction("StorePresetWithScreenshot", StorePresetWithScreenshot);
			RegisterAction(storePresetWithScreenshotAction);
			storeOverlayPresetAction = new JSONStorableAction("StoreOverlayPreset", StoreOverlayPreset);
			RegisterAction(storeOverlayPresetAction);
			storeOverlayPresetWithScreenshotAction = new JSONStorableAction("StoreOverlayPresetWithScreenshot", StoreOverlayPresetWithScreenshot);
			RegisterAction(storeOverlayPresetWithScreenshotAction);
			storePresetWithNameJSON = new JSONStorableString("StorePresetWithName", string.Empty, SyncStorePresetWithName);
			storePresetWithNameJSON.isStorable = false;
			storePresetWithNameJSON.isRestorable = false;
			RegisterString(storePresetWithNameJSON);
			storeOptionalJSON = new JSONStorableBool("storeOptional", pm.storeOptionalStorables, SyncStoreOptional);
			storeOptionalJSON.isStorable = false;
			storeOptionalJSON.isRestorable = false;
			RegisterBool(storeOptionalJSON);
			storeOptional2JSON = new JSONStorableBool("storeOptional2", pm.storeOptionalStorables2, SyncStoreOptional2);
			storeOptional2JSON.isStorable = false;
			storeOptional2JSON.isRestorable = false;
			RegisterBool(storeOptional2JSON);
			storeOptional3JSON = new JSONStorableBool("storeOptional3", pm.storeOptionalStorables3, SyncStoreOptional3);
			storeOptional3JSON.isStorable = false;
			storeOptional3JSON.isRestorable = false;
			RegisterBool(storeOptional3JSON);
			storePresetBinaryJSON = new JSONStorableBool("storeBinary", pm.storePresetBinary, SyncStorePresetBinary);
			storePresetBinaryJSON.isStorable = false;
			storePresetBinaryJSON.isRestorable = false;
			RegisterBool(storePresetBinaryJSON);
			loadPresetAction = new JSONStorableAction("LoadPreset", LoadPreset);
			RegisterAction(loadPresetAction);
			mergeLoadPresetAction = new JSONStorableAction("MergeLoadPreset", MergeLoadPreset);
			RegisterAction(mergeLoadPresetAction);
			autoTypeLoadPresetAction = new JSONStorableAction("AutoTypeLoadPreset", AutoTypeLoadPreset);
			RegisterAction(autoTypeLoadPresetAction);
			loadPresetWithPathUrlJSON = new JSONStorableUrl("loadPresetWithPathUrl", string.Empty, "vap", pm.GetStoreFolderPath());
			loadPresetWithPathUrlJSON.beginBrowseWithObjectCallback = BeginBrowse;
			loadPresetWithPathUrlJSON.allowFullComputerBrowse = false;
			loadPresetWithPathUrlJSON.allowBrowseAboveSuggestedPath = false;
			loadPresetWithPathUrlJSON.hideExtension = true;
			loadPresetWithPathUrlJSON.fileRemovePrefix = pm.storeName + "_";
			loadPresetWithPathUrlJSON.showDirs = true;
			loadPresetWithPathJSON = new JSONStorableActionPresetFilePath("LoadPresetWithPath", LoadPresetWithPath, loadPresetWithPathUrlJSON);
			RegisterPresetFilePathAction(loadPresetWithPathJSON);
			mergeLoadPresetWithPathUrlJSON = new JSONStorableUrl("loadPresetWithPathUrl", string.Empty, "vap", pm.GetStoreFolderPath());
			mergeLoadPresetWithPathUrlJSON.beginBrowseWithObjectCallback = BeginBrowse;
			mergeLoadPresetWithPathUrlJSON.allowFullComputerBrowse = false;
			mergeLoadPresetWithPathUrlJSON.allowBrowseAboveSuggestedPath = false;
			mergeLoadPresetWithPathUrlJSON.hideExtension = true;
			mergeLoadPresetWithPathUrlJSON.fileRemovePrefix = pm.storeName + "_";
			mergeLoadPresetWithPathUrlJSON.showDirs = true;
			mergeLoadPresetWithPathJSON = new JSONStorableActionPresetFilePath("MergeLoadPresetWithPath", MergeLoadPresetWithPath, mergeLoadPresetWithPathUrlJSON);
			RegisterPresetFilePathAction(mergeLoadPresetWithPathJSON);
			loadPresetWithNameJSON = new JSONStorableString("LoadPresetWithName", string.Empty, SyncLoadPresetWithName);
			loadPresetWithNameJSON.isStorable = false;
			loadPresetWithNameJSON.isRestorable = false;
			RegisterString(loadPresetWithNameJSON);
			mergeLoadPresetWithNameJSON = new JSONStorableString("MergeLoadPresetWithName", string.Empty, SyncMergeLoadPresetWithName);
			mergeLoadPresetWithNameJSON.isStorable = false;
			mergeLoadPresetWithNameJSON.isRestorable = false;
			RegisterString(mergeLoadPresetWithNameJSON);
			loadDefaultsAction = new JSONStorableAction("LoadDefaults", LoadDefaults);
			RegisterAction(loadDefaultsAction);
			storeUserDefaultsAction = new JSONStorableAction("StoreUserDefaults", StoreUserDefaults);
			RegisterAction(storeUserDefaultsAction);
			clearUserDefaultsAction = new JSONStorableAction("ClearUserDefaults", ClearUserDefaults);
			RegisterAction(clearUserDefaultsAction);
			loadUserDefaultsAction = new JSONStorableAction("LoadUserDefaults", LoadUserDefaults);
			RegisterAction(loadUserDefaultsAction);
			loadPresetOnSelectJSON = new JSONStorableBool("loadPresetOnSelect", true);
			loadPresetOnSelectJSON.isStorable = false;
			loadPresetOnSelectJSON.isRestorable = false;
			RegisterBool(loadPresetOnSelectJSON);
			useMergeLoadJSON = new JSONStorableBool("useMergeLoad", false);
			useMergeLoadJSON.isStorable = false;
			useMergeLoadJSON.isRestorable = false;
			RegisterBool(useMergeLoadJSON);
			favoriteSelectionJSON = new JSONStorableStringChooser("favoriteSelection", null, string.Empty, "Favorite Selection", SyncFavoriteSelection);
			favoriteSelectionJSON.isStorable = false;
			favoriteSelectionJSON.isRestorable = false;
			includeOptionalJSON = new JSONStorableBool("includeOptional", pm.includeOptional, SyncIncludeOptional);
			includeOptionalJSON.isStorable = false;
			includeOptionalJSON.isRestorable = false;
			RegisterBool(includeOptionalJSON);
			includeOptional2JSON = new JSONStorableBool("includeOptional2", pm.includeOptional2, SyncIncludeOptional2);
			includeOptional2JSON.isStorable = false;
			includeOptional2JSON.isRestorable = false;
			RegisterBool(includeOptional2JSON);
			includeOptional3JSON = new JSONStorableBool("includeOptional3", pm.includeOptional3, SyncIncludeOptional3);
			includeOptional3JSON.isStorable = false;
			includeOptional3JSON.isRestorable = false;
			RegisterBool(includeOptional3JSON);
			includeAppearanceJSON = new JSONStorableBool("includeAppearance", pm.includeAppearance, SyncIncludeAppearance);
			includeAppearanceJSON.isStorable = false;
			includeAppearanceJSON.isRestorable = false;
			RegisterBool(includeAppearanceJSON);
			includePhysicalJSON = new JSONStorableBool("includePhysical", pm.includePhysical, SyncIncludePhysical);
			includePhysicalJSON.isStorable = false;
			includePhysicalJSON.isRestorable = false;
			RegisterBool(includePhysicalJSON);
			lockParamsJSON = new JSONStorableBool("lockParams", false, SyncLockParams);
			RefreshFavoriteNames();
			RegisterStringChooser(favoriteSelectionJSON);
		}

		public override void PreRestore()
		{
			if (!isLoadingPreset && presetBrowsePathJSON != null)
			{
				presetBrowsePathJSON.val = string.Empty;
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
	}
}
