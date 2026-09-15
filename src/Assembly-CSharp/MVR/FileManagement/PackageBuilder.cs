using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using MVR.FileManagementSecure;
using MVR.Hub;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

namespace MVR.FileManagement
{
	public class PackageBuilder : JSONStorable
	{
		[Serializable]
		public class CategoryFilter
		{
			public string name;

			public string matchDirectoryPath;
		}

		protected class ReferenceReport
		{
			public string Reference { get; protected set; }

			public VarPackage Package { get; protected set; }

			public string PackageUid { get; protected set; }

			public bool HasIssue { get; protected set; }

			public string Issue { get; protected set; }

			public bool Fixable { get; protected set; }

			public string FixedReference { get; protected set; }

			public ReferenceReport(string reference, VarPackage package = null, string packageUid = null, bool hasIssue = false, string issue = "", bool fixable = false, string fixedReference = null)
			{
				Reference = reference;
				Package = package;
				PackageUid = packageUid;
				HasIssue = hasIssue;
				Issue = issue;
				Fixable = fixable;
				FixedReference = fixedReference;
			}
		}

		protected JSONStorableAction clearAllJSON;

		protected VarPackage currentPackage;

		protected string currentPackageResourceId;

		protected string currentPackageScenePath;

		protected JSONStorableBool currentPackageIsOnHubJSON;

		protected JSONStorableBool currentPackageHasSceneJSON;

		protected JSONStorableAction loadMetaFromExistingPackageJSON;

		protected char[] invalidChars;

		protected string _creatorName;

		protected JSONStorableString creatorNameJSON;

		protected string _packageName;

		protected JSONStorableString packageNameJSON;

		protected InputField versionField;

		protected int _version;

		public Color errorColor = Color.red;

		public Color warningColor = Color.yellow;

		public Color normalColor = Color.black;

		public Color disabledColor = Color.gray;

		public Color readyColor = Color.green;

		protected Text statusText;

		public bool isManager;

		protected RectTransform packagesContainer;

		public RectTransform packageItemPrefab;

		protected Dictionary<string, PackageBuilderPackageItem> packageItems;

		protected RectTransform missingPackagesContainer;

		protected Dictionary<string, PackageBuilderPackageItem> missingPackageItems;

		protected RectTransform packageReferencesContainer;

		protected List<PackageBuilderPackageItem> packageReferenceItems;

		public HubBrowse hubBrowse;

		protected JSONStorableAction scanHubForMissingPackagesAction;

		protected JSONStorableAction selectCurrentScenePackageAction;

		protected JSONStorableBool showDisabledJSON;

		protected RectTransform packageCategoryPanel;

		public RectTransform categoryTogglePrefab;

		public CategoryFilter[] categoryFilters;

		protected Dictionary<string, List<string>> categoryToPackageUids;

		protected string currentCategory;

		protected JSONStorableStringChooser categoryJSON;

		protected Button promotionalButton;

		protected Text promotionalButtonText;

		protected JSONStorableAction goToPromotionalLinkAction;

		protected JSONStorableAction copyPromotionalLinkAction;

		protected JSONStorableBool packageEnabledJSON;

		protected JSONStorableAction openOnHubAction;

		public HubDownloader hubDownloader;

		protected JSONStorableAction openInHubDownloaderAction;

		protected JSONStorableAction openSceneAction;

		protected RectTransform confirmDeletePackagePanel;

		protected Text confirmDeletePackageText;

		protected JSONStorableAction deletePackageAction;

		protected JSONStorableAction confirmDeletePackageAction;

		protected JSONStorableAction cancelDeletePackageAction;

		protected JSONStorableBool hadReferenceIssuesJSON;

		protected RectTransform packPanel;

		protected Slider packProgressSlider;

		protected AsyncFlag unpackFlag;

		protected RectTransform confirmUnpackPanel;

		protected JSONStorableAction unpackAction;

		protected JSONStorableAction confirmUnpackAction;

		protected JSONStorableAction cancelUnpackAction;

		protected AsyncFlag repackFlag;

		protected JSONStorableAction repackAction;

		protected RectTransform confirmRestoreFromOriginalPanel;

		protected JSONStorableAction restoreFromOriginalAction;

		protected JSONStorableAction confirmRestoreFromOriginalAction;

		protected JSONStorableAction cancelRestoreFromOriginalAction;

		public VarPackageCustomOption[] customOptions;

		protected string _userNotes;

		protected JSONStorableString userNotesJSON;

		protected bool _pluginsAlwaysEnabled;

		protected JSONStorableBool pluginsAlwaysEnabledJSON;

		protected bool _pluginsAlwaysDisabled;

		protected JSONStorableBool pluginsAlwaysDisabledJSON;

		protected bool _ignoreMissingDependencyErrors;

		protected JSONStorableBool ignoreMissingDependencyErrorsJSON;

		protected RectTransform contentContainer;

		public RectTransform contentItemPrefab;

		protected Dictionary<string, PackageBuilderContentItem> contentItems;

		protected string suggestedDirectory = "Custom";

		protected List<ShortCut> shortCuts;

		protected JSONStorableAction addDirectoryAction;

		protected JSONStorableAction addFileAction;

		protected JSONStorableAction removeSelectedAction;

		protected JSONStorableAction removeAllAction;

		protected string _preppedDir;

		protected bool _needsPrep;

		protected RectTransform referencesContainer;

		public RectTransform referencesItemPrefab;

		protected VarPackage.ReferenceVersionOption _standardReferenceVersionOption;

		protected JSONStorableStringChooser standardReferenceVersionOptionJSON;

		protected VarPackage.ReferenceVersionOption _scriptReferenceVersionOption = VarPackage.ReferenceVersionOption.Exact;

		protected JSONStorableStringChooser scriptReferenceVersionOptionJSON;

		protected Dictionary<string, ReferenceReport> referenceReports;

		protected List<PackageBuilderReferenceItem> referenceItems;

		protected HashSet<VarPackage> allReferencedPackages;

		protected HashSet<string> allReferencedPackageUids;

		protected JSONClass allPackagesDependencyTree;

		protected List<PackageBuilderReferenceItem> licenseReportItems;

		protected RectTransform licenseReportContainer;

		protected Text licenseReportIssueText;

		protected Text nonCommercialLicenseReportIssueText;

		protected bool licenseIssue;

		protected bool nonCommercialLicenseIssue;

		protected HashSet<string> alreadyReported;

		protected JSONStorableAction prepPackageAction;

		protected JSONStorableAction fixReferencesAction;

		protected string _description;

		protected JSONStorableString descriptionJSON;

		protected string _credits;

		protected JSONStorableString creditsJSON;

		protected string _instructions;

		protected JSONStorableString instructionsJSON;

		protected string _promotionalLink;

		protected JSONStorableString promotionalLinkJSON;

		protected string[] licenseTypes = new string[10] { "PC", "PC EA", "Questionable", "FC", "CC BY", "CC BY-SA", "CC BY-ND", "CC BY-NC", "CC BY-NC-SA", "CC BY-NC-ND" };

		protected bool[] licenseDistributableFlags = new bool[10] { false, false, false, true, true, true, true, true, true, true };

		protected bool[] licenseNonCommercialFlags = new bool[10] { true, true, true, false, false, false, false, true, true, true };

		protected string[] licenseDescriptions = new string[10] { "PC-Paid Content: cannot distribute, remix, tweak, or build upon work", "PC EA-Paid Content Early Access: cannot distribute, remix, tweak, or build upon work until the EA (Early Access) end date, at which point the secondary license is in effect.", "Questionable: should not distribute", "FC-Free Content: can distribute, remix, tweak, and build upon work, even commercially. No credit required.", "CC BY: can distribute, remix, tweak, and build upon work, even commercially. Must credit creator.", "CC BY-SA: can distribute, remix, tweak, and build upon work, even commercially, as long as license for new creations is identical. Must credit creator.", "CC BY-ND: can distribute, commercially or non-commercially, as long as passed along unchanged. Must credit creator.", "CC BY-NC: can distribute, remix, tweak, and build upon work, but only non-commercially. Must credit creator.", "CC BY-NC-SA: can distribute, remix, tweak, and build upon work, but only non-commercially, and as long as license for new creations is identical. Must credit creator.", "CC BY-NC-ND: can distribute, non-commercially, as long as passed along unchanged. Must credit creator." };

		protected Text licenseDescriptionText;

		protected string _licenseType = "CC BY";

		protected JSONStorableStringChooser licenseTypeJSON;

		protected string _secondaryLicenseType = "CC BY";

		protected JSONStorableStringChooser secondaryLicenseTypeJSON;

		protected string[] yearChoices = new string[11]
		{
			"2020", "2021", "2022", "2023", "2024", "2025", "2026", "2027", "2028", "2029",
			"2030"
		};

		protected string[] monthChoices = new string[12]
		{
			"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct",
			"Nov", "Dec"
		};

		protected JSONStorableStringChooser EAYearJSON;

		protected JSONStorableStringChooser EAMonthJSON;

		protected JSONStorableStringChooser EADayJSON;

		protected RectTransform openPrepFolderInExplorerNotice;

		protected JSONStorableAction openPrepFolderInExplorerAction;

		protected RectTransform finalizingPanel;

		protected bool isFinalizing;

		protected Thread finalizeThread;

		protected bool finalizeThreadAbort;

		protected string finalizeThreadError;

		protected int finalizeFileProgressCount;

		protected int finalizeTotalFileCount;

		protected Slider finalizeProgressSlider;

		protected float finalizeProgress;

		protected AsyncFlag finalizeFlag;

		protected JSONStorableAction finalizeCheckConfirmAction;

		protected JSONStorableAction finalizeCheckCancelAction;

		protected RectTransform finalizeCheckPanel;

		protected JSONStorableAction finalizePackageAction;

		protected JSONStorableAction cancelFinalizeAction;

		public string CreatorName
		{
			get
			{
				return _creatorName;
			}
			protected set
			{
				creatorNameJSON.val = value;
			}
		}

		public string PackageName
		{
			get
			{
				return _packageName;
			}
			set
			{
				packageNameJSON.val = value;
			}
		}

		public int Version
		{
			get
			{
				return _version;
			}
			protected set
			{
				if (_version == value)
				{
					return;
				}
				_version = value;
				if (versionField != null)
				{
					if (_version == 0)
					{
						versionField.text = string.Empty;
					}
					else
					{
						versionField.text = _version.ToString();
					}
				}
			}
		}

		public List<string> MissingPackageNames
		{
			get
			{
				if (missingPackageItems != null)
				{
					List<string> list = missingPackageItems.Keys.ToList();
					list.Sort();
					return list;
				}
				return new List<string>();
			}
		}

		public string UserNotes
		{
			get
			{
				return _userNotes;
			}
			set
			{
				userNotesJSON.val = value;
			}
		}

		public bool PluginsAlwaysEnabled
		{
			get
			{
				return _pluginsAlwaysEnabled;
			}
			set
			{
				pluginsAlwaysEnabledJSON.val = value;
			}
		}

		public bool PluginsAlwaysDisabled
		{
			get
			{
				return _pluginsAlwaysDisabled;
			}
			set
			{
				pluginsAlwaysDisabledJSON.val = value;
			}
		}

		public bool IgnoreMissingDependencyErrors
		{
			get
			{
				return _ignoreMissingDependencyErrors;
			}
			set
			{
				ignoreMissingDependencyErrorsJSON.val = value;
			}
		}

		protected bool NeedsPrep
		{
			get
			{
				return _needsPrep;
			}
			set
			{
				if (_needsPrep == value)
				{
					return;
				}
				_needsPrep = value;
				if (_needsPrep)
				{
					ClearReferenceItems();
					Version = 0;
				}
				if (prepPackageAction != null && prepPackageAction.dynamicButton != null)
				{
					if (_needsPrep)
					{
						prepPackageAction.dynamicButton.buttonColor = readyColor;
					}
					else
					{
						prepPackageAction.dynamicButton.buttonColor = disabledColor;
					}
				}
				if (openPrepFolderInExplorerNotice != null)
				{
					openPrepFolderInExplorerNotice.gameObject.SetActive(!_needsPrep);
				}
				if (openPrepFolderInExplorerAction != null && openPrepFolderInExplorerAction.button != null)
				{
					openPrepFolderInExplorerAction.button.interactable = !_needsPrep;
				}
				if (finalizePackageAction != null && finalizePackageAction.button != null)
				{
					finalizePackageAction.button.interactable = !_needsPrep;
				}
			}
		}

		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				descriptionJSON.val = value;
			}
		}

		public string Credits
		{
			get
			{
				return _credits;
			}
			set
			{
				creditsJSON.val = value;
			}
		}

		public string Instructions
		{
			get
			{
				return _instructions;
			}
			set
			{
				instructionsJSON.val = value;
			}
		}

		public string PromotionalLink
		{
			get
			{
				return _promotionalLink;
			}
			set
			{
				promotionalLinkJSON.val = value;
			}
		}

		public string LicenseType
		{
			get
			{
				return _licenseType;
			}
			set
			{
				licenseTypeJSON.val = value;
			}
		}

		public string SecondaryLicenseType
		{
			get
			{
				return _secondaryLicenseType;
			}
			set
			{
				secondaryLicenseTypeJSON.val = value;
			}
		}

		public void ClearAll()
		{
			packageNameJSON.SetValToDefault();
			ClearStatus();
			ClearContentItems();
			ClearReferenceItems();
			ClearPackageReferenceItems();
			descriptionJSON.SetValToDefault();
			creditsJSON.SetValToDefault();
			instructionsJSON.SetValToDefault();
			promotionalLinkJSON.SetValToDefault();
			licenseTypeJSON.SetValToDefault();
			secondaryLicenseTypeJSON.SetValToDefault();
			EAYearJSON.SetValToDefault();
			EAMonthJSON.SetValToDefault();
			EADayJSON.SetValToDefault();
		}

		public void LoadMetaFromPackageUid(string uid, bool includeCreatorVersionAndRefs = true)
		{
			VarPackage package = FileManager.GetPackage(uid);
			if (package != null)
			{
				LoadMetaFromPackage(package, includeCreatorVersionAndRefs);
				return;
			}
			ShowErrorStatus("Package " + uid + " not found");
			SuperController.LogError("Package " + uid + " not found");
		}

		protected void LoadMetaFromPackage(string path)
		{
			FileManager.AssertNotCalledFromUnityMessaging();
			if (path != string.Empty)
			{
				VarPackage package = FileManager.GetPackage(path);
				if (package != null)
				{
					LoadMetaFromPackage(package, false);
					return;
				}
				ShowErrorStatus("Package " + path + " not found");
				SuperController.LogError("Package " + path + " not found");
			}
		}

		protected void LoadMetaFromPackage(VarPackage vp, bool includeCreatorVersionAndRefs)
		{
			if (vp == null)
			{
				return;
			}
			if (!vp.invalid)
			{
				if (isManager)
				{
					currentPackage = vp;
					List<FileEntry> list = new List<FileEntry>();
					vp.FindFiles("Saves/scene", "*.json", list);
					if (list.Count > 0)
					{
						FileEntry fileEntry = list[0];
						currentPackageScenePath = fileEntry.Uid;
						currentPackageHasSceneJSON.val = true;
					}
					else
					{
						currentPackageScenePath = null;
						currentPackageHasSceneJSON.val = false;
					}
					if (hubBrowse != null && !SuperController.singleton.hubDisabled)
					{
						currentPackageResourceId = hubBrowse.GetPackageHubResourceId(currentPackage.Uid);
						currentPackageIsOnHubJSON.val = currentPackageResourceId != null;
					}
					else
					{
						currentPackageResourceId = null;
						currentPackageIsOnHubJSON.val = false;
					}
					packageEnabledJSON.valNoCallback = vp.Enabled;
					userNotesJSON.valNoCallback = vp.Group.UserNotes;
					_userNotes = vp.Group.UserNotes;
					pluginsAlwaysEnabledJSON.valNoCallback = vp.PluginsAlwaysEnabled;
					_pluginsAlwaysEnabled = vp.PluginsAlwaysEnabled;
					pluginsAlwaysDisabledJSON.valNoCallback = vp.PluginsAlwaysDisabled;
					_pluginsAlwaysDisabled = vp.PluginsAlwaysDisabled;
					ignoreMissingDependencyErrorsJSON.valNoCallback = vp.IgnoreMissingDependencyErrors;
					_ignoreMissingDependencyErrors = vp.IgnoreMissingDependencyErrors;
					if (repackAction.button != null)
					{
						repackAction.button.gameObject.SetActive(vp.IsSimulated);
					}
					if (unpackAction.button != null)
					{
						unpackAction.button.gameObject.SetActive(!vp.IsSimulated);
					}
					if (restoreFromOriginalAction.button != null)
					{
						restoreFromOriginalAction.button.gameObject.SetActive(vp.HasOriginalCopy);
					}
					hadReferenceIssuesJSON.val = vp.HadReferenceIssues;
				}
				ClearAll();
				packageNameJSON.val = vp.Name;
				if (includeCreatorVersionAndRefs)
				{
					creatorNameJSON.val = vp.Creator;
					Version = vp.Version;
					foreach (string packageDependency in vp.PackageDependencies)
					{
						AddPackageReferenceItem(packageDependency);
					}
				}
				standardReferenceVersionOptionJSON.val = vp.StandardReferenceVersionOption.ToString();
				scriptReferenceVersionOptionJSON.val = vp.ScriptReferenceVersionOption.ToString();
				descriptionJSON.val = vp.Description;
				creditsJSON.val = vp.Credits;
				instructionsJSON.val = vp.Instructions;
				promotionalLinkJSON.val = vp.PromotionalLink;
				licenseTypeJSON.val = vp.LicenseType;
				if (vp.SecondaryLicenseType != null)
				{
					secondaryLicenseTypeJSON.val = vp.SecondaryLicenseType;
				}
				else
				{
					secondaryLicenseTypeJSON.SetValToDefault();
				}
				if (vp.EAEndYear != null)
				{
					EAYearJSON.val = vp.EAEndYear;
				}
				else
				{
					EAYearJSON.SetValToDefault();
				}
				if (vp.EAEndMonth != null)
				{
					EAMonthJSON.val = vp.EAEndMonth;
				}
				else
				{
					EAMonthJSON.SetValToDefault();
				}
				if (vp.EAEndDay != null)
				{
					EADayJSON.val = vp.EAEndDay;
				}
				else
				{
					EADayJSON.SetValToDefault();
				}
				foreach (string content in vp.Contents)
				{
					AddContentItem(content);
				}
				if (isManager)
				{
					foreach (KeyValuePair<string, PackageBuilderPackageItem> packageItem in packageItems)
					{
						string key = packageItem.Key;
						PackageBuilderPackageItem value = packageItem.Value;
						VarPackage package = FileManager.GetPackage(key);
						if (currentPackage.Uid == key)
						{
							if (package.Enabled)
							{
								value.SetColor(readyColor);
							}
							else
							{
								value.SetColor(readyColor);
							}
						}
						else if (package.Enabled)
						{
							if (package.HasMissingDependencies)
							{
								value.SetColor(errorColor);
							}
							else
							{
								value.SetColor(Color.white);
							}
						}
						else
						{
							value.SetColor(warningColor);
						}
					}
					VarPackageCustomOption[] array = customOptions;
					foreach (VarPackageCustomOption varPackageCustomOption in array)
					{
						varPackageCustomOption.ValueNoCallback = vp.Group.GetCustomOption(varPackageCustomOption.name);
					}
				}
				else
				{
					VarPackageCustomOption[] array2 = customOptions;
					foreach (VarPackageCustomOption varPackageCustomOption2 in array2)
					{
						varPackageCustomOption2.ValueNoCallback = vp.GetCustomOption(varPackageCustomOption2.name);
					}
				}
			}
			else
			{
				ShowErrorStatus("Package " + vp.Uid + " meta file is not valid");
				SuperController.LogError("Package " + vp.Uid + " meta file is not valid");
			}
		}

		public void LoadMetaFromExistingPackage()
		{
			SuperController.singleton.GetMediaPathDialog(LoadMetaFromPackage, "var", FileManager.PackageFolder, false, true, false, Regex.Escape(CreatorName) + ".", false, null, false);
		}

		protected string FixNameToBeValidPathName(string s)
		{
			string text = s;
			if (invalidChars == null)
			{
				List<char> list = new List<char>(Path.GetInvalidFileNameChars());
				list.Add('.');
				invalidChars = list.ToArray();
			}
			bool flag = true;
			while (flag)
			{
				int num = text.IndexOfAny(invalidChars);
				if (num >= 0)
				{
					text = text.Replace(text[num], '_');
				}
				else if (text.IndexOf(' ') >= 0)
				{
					text = text.Replace(' ', '_');
				}
				else
				{
					flag = false;
				}
			}
			return text;
		}

		protected void SyncCreatorName(string s)
		{
			string text = FixNameToBeValidPathName(s);
			if (text != s)
			{
				ShowErrorStatus("Creator name was fixed to convert invalid characters to _");
				creatorNameJSON.valNoCallback = text;
			}
			else
			{
				ClearStatus();
			}
			_creatorName = text;
			NeedsPrep = true;
		}

		protected void SyncPackageName(string s)
		{
			string text = FixNameToBeValidPathName(s);
			if (text != s)
			{
				ShowErrorStatus("Package name was fixed to convert invalid characters to _");
				packageNameJSON.valNoCallback = text;
			}
			else
			{
				ClearStatus();
			}
			_packageName = text;
			NeedsPrep = true;
		}

		protected void ShowStatus(string status, Color color)
		{
			if (statusText != null)
			{
				statusText.color = color;
				statusText.text = status;
			}
		}

		protected void ShowErrorStatus(string status)
		{
			ShowStatus(status, errorColor);
		}

		protected void ShowStatus(string status)
		{
			ShowStatus(status, normalColor);
		}

		protected void ClearStatus()
		{
			ShowStatus(string.Empty);
		}

		protected void ClearPackageReferenceItems()
		{
			if (packageReferenceItems == null)
			{
				return;
			}
			foreach (PackageBuilderPackageItem packageReferenceItem in packageReferenceItems)
			{
				UnityEngine.Object.Destroy(packageReferenceItem.gameObject);
			}
			packageReferenceItems.Clear();
		}

		protected void AddPackageReferenceItem(string packageId)
		{
			if (packageReferenceItems == null || !(packageReferencesContainer != null) || !(packageItemPrefab != null))
			{
				return;
			}
			RectTransform rectTransform = UnityEngine.Object.Instantiate(packageItemPrefab);
			PackageBuilderPackageItem component = rectTransform.GetComponent<PackageBuilderPackageItem>();
			if (!(component != null))
			{
				return;
			}
			rectTransform.SetParent(packageReferencesContainer, false);
			if (component.button != null)
			{
				component.button.onClick.AddListener(delegate
				{
					LoadMetaFromPackageUid(packageId);
				});
			}
			component.Package = packageId;
			VarPackage package = FileManager.GetPackage(packageId);
			if (package == null || !package.Enabled)
			{
				string packageGroupUid = FileManager.PackageIDToPackageGroupID(packageId);
				VarPackageGroup packageGroup = FileManager.GetPackageGroup(packageGroupUid);
				if (packageGroup == null)
				{
					component.SetColor(errorColor);
				}
				else
				{
					component.SetColor(warningColor);
				}
			}
			packageReferenceItems.Add(component);
		}

		protected void SyncPackagesList()
		{
			string text = null;
			if (currentPackage != null)
			{
				text = currentPackage.Uid;
			}
			ClearAll();
			foreach (PackageBuilderPackageItem value2 in packageItems.Values)
			{
				UnityEngine.Object.Destroy(value2.gameObject);
			}
			packageItems.Clear();
			foreach (PackageBuilderPackageItem value3 in missingPackageItems.Values)
			{
				UnityEngine.Object.Destroy(value3.gameObject);
			}
			missingPackageItems.Clear();
			bool flag = false;
			List<string> value;
			if (currentCategory != null)
			{
				switch (currentCategory)
				{
				case "Preload Morphs":
					value = new List<string>();
					foreach (VarPackage package2 in FileManager.GetPackages())
					{
						if (package2.Group.GetCustomOption("preloadMorphs"))
						{
							value.Add(package2.Uid);
						}
					}
					break;
				case "Plugins Always Disabled":
					value = new List<string>();
					foreach (VarPackage package3 in FileManager.GetPackages())
					{
						if (package3.PluginsAlwaysDisabled)
						{
							value.Add(package3.Uid);
						}
					}
					break;
				case "Plugins Always Enabled":
					value = new List<string>();
					foreach (VarPackage package4 in FileManager.GetPackages())
					{
						if (package4.PluginsAlwaysEnabled)
						{
							value.Add(package4.Uid);
						}
					}
					break;
				case "Disabled":
					flag = true;
					value = new List<string>();
					foreach (VarPackage package5 in FileManager.GetPackages())
					{
						if (!package5.Enabled)
						{
							value.Add(package5.Uid);
						}
					}
					break;
				case "Missing Dependencies":
					value = new List<string>();
					foreach (VarPackage package6 in FileManager.GetPackages())
					{
						if (package6.HasMissingDependencies)
						{
							value.Add(package6.Uid);
						}
					}
					break;
				default:
					if (!categoryToPackageUids.TryGetValue(currentCategory, out value))
					{
						value = FileManager.GetPackageUids();
					}
					break;
				}
			}
			else
			{
				value = FileManager.GetPackageUids();
			}
			foreach (string vpuid in value)
			{
				VarPackage package = FileManager.GetPackage(vpuid);
				if (package == null || (!flag && !showDisabledJSON.val && !package.Enabled))
				{
					continue;
				}
				RectTransform rectTransform = UnityEngine.Object.Instantiate(packageItemPrefab);
				PackageBuilderPackageItem component = rectTransform.GetComponent<PackageBuilderPackageItem>();
				if (!(component != null))
				{
					continue;
				}
				rectTransform.SetParent(packagesContainer, false);
				if (component.button != null)
				{
					component.button.onClick.AddListener(delegate
					{
						LoadMetaFromPackageUid(vpuid);
					});
				}
				if (!package.Enabled)
				{
					component.SetColor(warningColor);
				}
				else if (package.HasMissingDependencies || package.invalid)
				{
					component.SetColor(errorColor);
				}
				component.Package = vpuid;
				packageItems.Add(vpuid, component);
			}
			HashSet<string> hashSet = new HashSet<string>();
			foreach (VarPackage package7 in FileManager.GetPackages())
			{
				if (package7.IgnoreMissingDependencyErrors)
				{
					continue;
				}
				foreach (string item in package7.PackageDependenciesMissing)
				{
					hashSet.Add(item);
				}
			}
			List<string> list = hashSet.ToList();
			list.Sort();
			foreach (string item2 in list)
			{
				RectTransform rectTransform2 = UnityEngine.Object.Instantiate(packageItemPrefab);
				PackageBuilderPackageItem component2 = rectTransform2.GetComponent<PackageBuilderPackageItem>();
				if (component2 != null)
				{
					rectTransform2.SetParent(missingPackagesContainer, false);
					string packageGroupUid = FileManager.PackageIDToPackageGroupID(item2);
					VarPackageGroup packageGroup = FileManager.GetPackageGroup(packageGroupUid);
					if (packageGroup == null)
					{
						component2.SetColor(errorColor);
					}
					else
					{
						component2.SetColor(warningColor);
					}
					component2.Package = item2;
					missingPackageItems.Add(item2, component2);
				}
			}
			if (text != null)
			{
				LoadMetaFromPackageUid(text);
			}
		}

		protected void SyncPackages()
		{
			if (packageItems != null && packagesContainer != null && packageItemPrefab != null && missingPackageItems != null && missingPackagesContainer != null)
			{
				SyncCategoryToPackageUids();
				SyncPackagesList();
			}
		}

		protected void ScanHubForMissingPackages()
		{
			if (hubBrowse != null)
			{
				hubBrowse.OpenMissingPackagesPanel();
			}
		}

		protected void SelectCurrentScenePackage()
		{
			string topPackageUid = FileManager.TopPackageUid;
			if (topPackageUid != null)
			{
				LoadMetaFromPackageUid(topPackageUid);
			}
		}

		protected void SyncShowDisabled(bool b)
		{
			SyncPackagesList();
		}

		protected void SyncCategoryToPackageUids()
		{
			if (categoryToPackageUids == null)
			{
				categoryToPackageUids = new Dictionary<string, List<string>>();
			}
			else
			{
				categoryToPackageUids.Clear();
			}
			List<string> packageUids = FileManager.GetPackageUids();
			CategoryFilter[] array = categoryFilters;
			foreach (CategoryFilter categoryFilter in array)
			{
				List<string> list = new List<string>();
				categoryToPackageUids.Add(categoryFilter.name, list);
				foreach (string item in packageUids)
				{
					if (categoryFilter.matchDirectoryPath == string.Empty)
					{
						list.Add(item);
						continue;
					}
					VarPackage package = FileManager.GetPackage(item);
					if (package.HasMatchingDirectories(categoryFilter.matchDirectoryPath))
					{
						list.Add(item);
					}
				}
			}
		}

		protected void CategoryChangeCallback(string category)
		{
			currentCategory = category;
			SyncPackagesList();
		}

		protected void GoToPromotionalLink()
		{
			if (promotionalButtonText != null)
			{
				SuperController.singleton.OpenLinkInBrowser(promotionalButtonText.text);
			}
		}

		protected void CopyPromotionalLink()
		{
			if (promotionalButtonText != null)
			{
				GUIUtility.systemCopyBuffer = promotionalButtonText.text;
			}
		}

		protected void SyncPackageEnabled(bool b)
		{
			if (currentPackage != null)
			{
				currentPackage.Enabled = b;
			}
		}

		protected void OpenOnHub()
		{
			if (hubBrowse != null && currentPackageResourceId != null && !SuperController.singleton.hubDisabled)
			{
				hubBrowse.OpenDetail(currentPackageResourceId);
			}
		}

		protected void OpenInHubDownloader()
		{
			if (hubDownloader != null && currentPackageResourceId != null && !SuperController.singleton.hubDisabled)
			{
				hubDownloader.FindResource(currentPackageResourceId);
				hubDownloader.OpenPanel();
			}
		}

		protected void OpenScene()
		{
			if (currentPackageScenePath != null)
			{
				SuperController.singleton.Load(currentPackageScenePath);
			}
		}

		protected void DeletePackage()
		{
			FileManager.AssertNotCalledFromUnityMessaging();
			if (currentPackage != null)
			{
				if (confirmDeletePackagePanel != null)
				{
					confirmDeletePackagePanel.gameObject.SetActive(true);
				}
				if (confirmDeletePackageText != null)
				{
					confirmDeletePackageText.text = "Delete " + currentPackage.Uid + "?";
				}
			}
		}

		protected void ConfirmDeletePackage()
		{
			FileManager.AssertNotCalledFromUnityMessaging();
			if (currentPackage != null)
			{
				VarPackage varPackage = currentPackage;
				currentPackage = null;
				currentPackageIsOnHubJSON.val = false;
				currentPackageResourceId = null;
				currentPackageHasSceneJSON.val = false;
				currentPackageScenePath = null;
				varPackage.Delete();
			}
			CancelDeletePackage();
		}

		protected void CancelDeletePackage()
		{
			FileManager.AssertNotCalledFromUnityMessaging();
			if (confirmDeletePackagePanel != null)
			{
				confirmDeletePackagePanel.gameObject.SetActive(false);
			}
		}

		protected void UnpackComplete()
		{
			unpackFlag.Raise();
			if (packPanel != null)
			{
				packPanel.gameObject.SetActive(false);
			}
		}

		protected IEnumerator UnpackCo()
		{
			unpackFlag = new AsyncFlag("UnpackPackage");
			SuperController.singleton.SetLoadingIconFlag(unpackFlag);
			if (packPanel != null)
			{
				packPanel.gameObject.SetActive(true);
			}
			yield return null;
			if (packProgressSlider != null)
			{
				packProgressSlider.value = 0f;
			}
			currentPackage.Unpack();
			while (currentPackage.IsUnpacking)
			{
				yield return null;
				if (packProgressSlider != null)
				{
					packProgressSlider.value = currentPackage.packProgress;
				}
			}
			if (currentPackage.packThreadError != null)
			{
				ShowErrorStatus("Exception during unpack");
				SuperController.LogError("Exception during package unpack: " + currentPackage.packThreadError);
			}
			else
			{
				FileManager.Refresh();
			}
			UnpackComplete();
		}

		protected void Unpack()
		{
			if (confirmUnpackPanel != null)
			{
				confirmUnpackPanel.gameObject.SetActive(true);
			}
		}

		protected void ConfirmUnpack()
		{
			if (confirmUnpackPanel != null)
			{
				confirmUnpackPanel.gameObject.SetActive(false);
			}
			if (currentPackage != null && !currentPackage.IsSimulated)
			{
				StartCoroutine(UnpackCo());
			}
		}

		protected void CancelUnpack()
		{
			if (confirmUnpackPanel != null)
			{
				confirmUnpackPanel.gameObject.SetActive(false);
			}
		}

		protected void RepackComplete()
		{
			repackFlag.Raise();
			if (packPanel != null)
			{
				packPanel.gameObject.SetActive(false);
			}
		}

		protected IEnumerator RepackCo()
		{
			repackFlag = new AsyncFlag("RepackPackage");
			SuperController.singleton.SetLoadingIconFlag(repackFlag);
			if (packPanel != null)
			{
				packPanel.gameObject.SetActive(true);
			}
			yield return null;
			if (packProgressSlider != null)
			{
				packProgressSlider.value = 0f;
			}
			currentPackage.Repack();
			while (currentPackage.IsRepacking)
			{
				yield return null;
				if (packProgressSlider != null)
				{
					packProgressSlider.value = currentPackage.packProgress;
				}
			}
			if (currentPackage.packThreadError != null)
			{
				ShowErrorStatus("Exception during repack");
				SuperController.LogError("Exception during package repack: " + currentPackage.packThreadError);
			}
			else
			{
				FileManager.Refresh();
			}
			RepackComplete();
		}

		protected void Repack()
		{
			if (currentPackage != null && currentPackage.IsSimulated)
			{
				StartCoroutine(RepackCo());
			}
		}

		protected void RestoreFromOriginal()
		{
			if (confirmRestoreFromOriginalPanel != null)
			{
				confirmRestoreFromOriginalPanel.gameObject.SetActive(true);
			}
		}

		protected void ConfirmRestoreFromOriginal()
		{
			if (confirmRestoreFromOriginalPanel != null)
			{
				confirmRestoreFromOriginalPanel.gameObject.SetActive(false);
			}
			if (currentPackage != null)
			{
				currentPackage.RestoreFromOriginal();
				FileManager.UnregisterPackage(currentPackage);
				FileManager.Refresh();
			}
		}

		protected void CancelRestoreFromOriginal()
		{
			if (confirmRestoreFromOriginalPanel != null)
			{
				confirmRestoreFromOriginalPanel.gameObject.SetActive(false);
			}
		}

		protected void CustomOptionChange(JSONStorableBool customBool)
		{
			if (isManager && currentPackage != null)
			{
				currentPackage.Group.SetCustomOption(customBool.name, customBool.val);
			}
		}

		protected void SyncUserNotes(string s)
		{
			_userNotes = s;
			if (currentPackage != null)
			{
				currentPackage.Group.UserNotes = _userNotes;
			}
		}

		protected void SyncPluginsAlwaysEnabled(bool b)
		{
			_pluginsAlwaysEnabled = b;
			if (currentPackage != null)
			{
				currentPackage.PluginsAlwaysEnabled = _pluginsAlwaysEnabled;
				if (_pluginsAlwaysEnabled)
				{
					pluginsAlwaysDisabledJSON.val = false;
				}
			}
		}

		protected void SyncPluginsAlwaysDisabled(bool b)
		{
			_pluginsAlwaysDisabled = b;
			if (currentPackage != null)
			{
				currentPackage.PluginsAlwaysDisabled = _pluginsAlwaysDisabled;
				if (_pluginsAlwaysDisabled)
				{
					pluginsAlwaysEnabledJSON.val = false;
				}
			}
		}

		protected void SyncIgnoreMissingDependencyErrors(bool b)
		{
			_ignoreMissingDependencyErrors = b;
			if (currentPackage != null)
			{
				currentPackage.IgnoreMissingDependencyErrors = _ignoreMissingDependencyErrors;
			}
		}

		public void ClearContentItems()
		{
			foreach (PackageBuilderContentItem value in contentItems.Values)
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
			contentItems.Clear();
			NeedsPrep = true;
			ClearStatus();
		}

		public PackageBuilderContentItem AddContentItem(string itemPath)
		{
			PackageBuilderContentItem packageBuilderContentItem = null;
			if (contentItems != null && !contentItems.ContainsKey(itemPath) && contentItemPrefab != null && contentContainer != null)
			{
				RectTransform rectTransform = UnityEngine.Object.Instantiate(contentItemPrefab);
				packageBuilderContentItem = rectTransform.GetComponent<PackageBuilderContentItem>();
				if (packageBuilderContentItem != null)
				{
					rectTransform.SetParent(contentContainer, false);
					packageBuilderContentItem.ItemPath = itemPath;
					contentItems.Add(itemPath, packageBuilderContentItem);
					NeedsPrep = true;
					ClearStatus();
				}
			}
			if (_packageName == null || _packageName == string.Empty)
			{
				packageNameJSON.val = Path.GetFileNameWithoutExtension(itemPath);
			}
			if (Regex.IsMatch(itemPath, "\\.(json|vap|vam|scene|assetbundle)$"))
			{
				string text = Regex.Replace(itemPath, "\\.(json|vac|vap|vam|scene|assetbundle)$", ".jpg");
				if (File.Exists(text))
				{
					AddContentItem(text);
				}
			}
			if (Regex.IsMatch(itemPath, "\\.vam$"))
			{
				string text2 = Regex.Replace(itemPath, "\\.vam$", ".vaj");
				if (File.Exists(text2))
				{
					AddContentItem(text2);
				}
				string text3 = Regex.Replace(itemPath, "\\.vam$", ".vab");
				if (File.Exists(text3))
				{
					AddContentItem(text3);
				}
			}
			else if (Regex.IsMatch(itemPath, "\\.vmi"))
			{
				string text4 = Regex.Replace(itemPath, "\\.vmi$", ".vmb");
				if (File.Exists(text4))
				{
					AddContentItem(text4);
				}
			}
			return packageBuilderContentItem;
		}

		public bool RemoveItem(PackageBuilderContentItem contentItem)
		{
			if (contentItems != null && contentItems.Remove(contentItem.ItemPath))
			{
				UnityEngine.Object.Destroy(contentItem.gameObject);
				NeedsPrep = true;
				ClearStatus();
				return true;
			}
			return false;
		}

		public PackageBuilderContentItem GetItem(string itemPath)
		{
			PackageBuilderContentItem value = null;
			if (contentItems != null)
			{
				contentItems.TryGetValue(itemPath, out value);
			}
			return value;
		}

		protected void AddDirectoryCallback(string dir)
		{
			if (dir != null && dir != string.Empty)
			{
				dir = FileManager.NormalizePath(dir);
				suggestedDirectory = Path.GetDirectoryName(dir);
				suggestedDirectory = suggestedDirectory.Replace('/', '\\');
				AddContentItem(dir);
			}
		}

		public void AddDirectory()
		{
			SuperController.singleton.GetDirectoryPathDialog(AddDirectoryCallback, suggestedDirectory, shortCuts, false);
		}

		protected void AddFileCallback(string file)
		{
			if (file != null && file != string.Empty)
			{
				file = FileManager.NormalizePath(file);
				suggestedDirectory = Path.GetDirectoryName(file);
				suggestedDirectory = suggestedDirectory.Replace('/', '\\');
				AddContentItem(file);
			}
		}

		public void AddFile()
		{
			SuperController.singleton.GetMediaPathDialog(AddFileCallback, string.Empty, suggestedDirectory, false, true, false, null, false, shortCuts);
		}

		public void RemoveSelected()
		{
			if (contentItems == null)
			{
				return;
			}
			List<PackageBuilderContentItem> list = new List<PackageBuilderContentItem>();
			foreach (PackageBuilderContentItem value in contentItems.Values)
			{
				if (value.IsSelected)
				{
					list.Add(value);
				}
			}
			foreach (PackageBuilderContentItem item in list)
			{
				RemoveItem(item);
			}
		}

		protected void SyncStandardReferenceVersionOption(string s)
		{
			try
			{
				VarPackage.ReferenceVersionOption standardReferenceVersionOption = (VarPackage.ReferenceVersionOption)Enum.Parse(typeof(VarPackage.ReferenceVersionOption), s);
				_standardReferenceVersionOption = standardReferenceVersionOption;
			}
			catch (ArgumentException)
			{
				SuperController.LogError("Error while setting reference version option. Bad choice " + s);
			}
		}

		protected void SyncScriptReferenceVersionOption(string s)
		{
			try
			{
				VarPackage.ReferenceVersionOption scriptReferenceVersionOption = (VarPackage.ReferenceVersionOption)Enum.Parse(typeof(VarPackage.ReferenceVersionOption), s);
				_scriptReferenceVersionOption = scriptReferenceVersionOption;
			}
			catch (ArgumentException)
			{
				SuperController.LogError("Error while setting reference version option. Bad choice " + s);
			}
		}

		protected void CopyAndFixRefs(string packageGroup, string fromFile, string toFile, HashSet<string> internalFilesHashSet, Dictionary<string, string> vamIDsToPaths)
		{
			string directoryName = Path.GetDirectoryName(fromFile);
			using (StreamReader streamReader = new StreamReader(fromFile))
			{
				using (StreamWriter streamWriter = new StreamWriter(toFile))
				{
					string text;
					while ((text = streamReader.ReadLine()) != null)
					{
						if (text.Contains("SELF:/"))
						{
							text = text.Replace("SELF:/", string.Empty);
						}
						Match match = Regex.Match(text, "\\\"([^\\\"]*)\\\"\\s*:\\s*\\\"(.*\\.)(mp3|ogg|wav|jpg|jpeg|png|gif|tif|tiff|avi|mp4|assetbundle|scene|json|vmi|vam|vap|cs|cslist|dll)\\\"", RegexOptions.IgnoreCase);
						if (match.Success)
						{
							string value = match.Groups[1].Value;
							if (value != "name" && value != "displayName" && value != "audioClip")
							{
								string text2 = match.Groups[2].Value + match.Groups[3].Value;
								bool flag = false;
								if (value == "receiverTargetName")
								{
									if (text2.StartsWith("toggle:"))
									{
										text2 = Regex.Replace(text2, "^toggle:", string.Empty);
										flag = true;
									}
									else if (text2.StartsWith("hair:"))
									{
										text2 = Regex.Replace(text2, "^hair:", string.Empty);
										flag = true;
									}
									else if (text2.StartsWith("clothing:"))
									{
										text2 = Regex.Replace(text2, "^clothing:", string.Empty);
										flag = true;
									}
								}
								if (text2.Contains(":/"))
								{
									string text3 = Regex.Replace(text2, ":/.*", string.Empty);
									string[] array = text3.Split('.');
									if (array.Length > 2)
									{
										string text4 = array[0] + "." + array[1];
										string text5 = array[2];
										if (packageGroup == text4)
										{
											string text6 = Regex.Replace(text2, ".*:/", string.Empty);
											if (internalFilesHashSet.Contains(text6))
											{
												text = text.Replace(text2, "SELF:/" + text6);
											}
											else if (!referenceReports.ContainsKey(text2))
											{
												if (File.Exists(text6))
												{
													ReferenceReport value2 = new ReferenceReport(text2, null, null, true, "FIXABLE: References older version of this package, but file not included in new one", true, text6);
													referenceReports.Add(text2, value2);
												}
												else
												{
													ReferenceReport value3 = new ReferenceReport(text2, null, null, true, "BROKEN: References older version of same package, but file not included and file is missing");
													referenceReports.Add(text2, value3);
												}
											}
										}
										else
										{
											string text7 = text2;
											string text8 = text3;
											VarPackageGroup packageGroup2 = FileManager.GetPackageGroup(text4);
											VarPackage varPackage = FileManager.GetPackage(text3);
											if (varPackage == null && packageGroup2 != null)
											{
												varPackage = packageGroup2.NewestPackage;
												text5 = varPackage.Version.ToString();
											}
											bool flag2 = varPackage?.HasMatchingDirectories("Custom/Scripts") ?? false;
											VarPackage.ReferenceVersionOption referenceVersionOption = ((!flag2) ? _standardReferenceVersionOption : _scriptReferenceVersionOption);
											if (!flag)
											{
												switch (referenceVersionOption)
												{
												case VarPackage.ReferenceVersionOption.Latest:
													text8 = text4 + ".latest";
													text7 = text2.Replace(text3, text8);
													text = text.Replace(text2, text7);
													if (packageGroup2 != null)
													{
														varPackage = packageGroup2.NewestPackage;
													}
													break;
												case VarPackage.ReferenceVersionOption.Minimum:
													text8 = text4 + ".min" + text5;
													text7 = text2.Replace(text3, text8);
													text = text.Replace(text2, text7);
													break;
												}
											}
											if (!referenceReports.ContainsKey(text7))
											{
												if (varPackage == null)
												{
													ReferenceReport value4 = new ReferenceReport(text2, null, text8, true, "BROKEN: Missing package that is referenced");
													referenceReports.Add(text7, value4);
												}
												else
												{
													ReferenceReport value5 = new ReferenceReport(text7, varPackage, text8, false, string.Empty);
													referenceReports.Add(text7, value5);
												}
											}
										}
									}
									else if (!referenceReports.ContainsKey(text2))
									{
										ReferenceReport value6 = new ReferenceReport(text2, null, null, true, "BROKEN: References file outside of install directory");
										referenceReports.Add(text2, value6);
									}
								}
								else if (!text2.StartsWith("./") && text2.Contains("/"))
								{
									if (internalFilesHashSet.Contains(text2))
									{
										text = text.Replace(text2, "SELF:/" + text2);
									}
									else if (!referenceReports.ContainsKey(text2))
									{
										if (File.Exists(text2))
										{
											ReferenceReport value7 = new ReferenceReport(text2, null, null, true, "FIXABLE: References local file not included in package", true);
											referenceReports.Add(text2, value7);
										}
										else
										{
											ReferenceReport value8 = new ReferenceReport(text2, null, null, true, "BROKEN: References local file not included in package. File is missing");
											referenceReports.Add(text2, value8);
										}
									}
								}
								else
								{
									text2 = Regex.Replace(text2, "^\\./", string.Empty);
									string text9 = directoryName + "/" + text2;
									if (!internalFilesHashSet.Contains(text9) && !referenceReports.ContainsKey(text9))
									{
										if (File.Exists(text9))
										{
											ReferenceReport value9 = new ReferenceReport(text9, null, null, true, "FIXABLE: References local relative file " + text2 + " not included in package", true);
											referenceReports.Add(text9, value9);
										}
										else
										{
											ReferenceReport value10 = new ReferenceReport(text9, null, null, true, "BROKEN: References local relative file " + text2 + " not included in package. File is missing", true);
											referenceReports.Add(text9, value10);
										}
									}
								}
							}
						}
						else
						{
							match = Regex.Match(text, "\\\"id\\\" : \\\"(.*)\\\"");
							if (match.Success)
							{
								string value11 = match.Groups[1].Value;
								string value12;
								if (vamIDsToPaths.TryGetValue(value11, out value12))
								{
									text = text.Replace(value11, "SELF:/" + value12);
								}
							}
							else
							{
								match = Regex.Match(text, "\\\"presetName\\\" : \\\"(.*)\\\"");
								if (match.Success)
								{
									string value13 = match.Groups[1].Value;
									if (value13.Contains(":"))
									{
										string text10 = Regex.Replace(value13, ":.*", string.Empty);
										string[] array2 = text10.Split('.');
										if (array2.Length > 2)
										{
											string text11 = array2[0] + "." + array2[1];
											string text12 = array2[2];
											if (packageGroup == text11)
											{
												text = text.Replace(text10, "SELF");
											}
											else
											{
												string text13 = value13;
												string text14 = text10;
												VarPackage varPackage2 = FileManager.GetPackage(text10);
												VarPackageGroup packageGroup3 = FileManager.GetPackageGroup(text11);
												if (varPackage2 == null && packageGroup3 != null)
												{
													varPackage2 = packageGroup3.NewestPackage;
													text12 = varPackage2.Version.ToString();
												}
												bool flag3 = varPackage2?.HasMatchingDirectories("Custom/Scripts") ?? false;
												switch ((!flag3) ? _standardReferenceVersionOption : _scriptReferenceVersionOption)
												{
												case VarPackage.ReferenceVersionOption.Latest:
													text14 = text11 + ".latest";
													text13 = value13.Replace(text10, text14);
													text = text.Replace(value13, text13);
													if (packageGroup3 != null)
													{
														varPackage2 = packageGroup3.NewestPackage;
													}
													break;
												case VarPackage.ReferenceVersionOption.Minimum:
													text14 = text11 + ".min" + text12;
													text13 = value13.Replace(text10, text14);
													text = text.Replace(value13, text13);
													break;
												}
												if (!referenceReports.ContainsKey(text13))
												{
													if (varPackage2 == null)
													{
														ReferenceReport value14 = new ReferenceReport(value13, null, text14, true, "BROKEN: Missing package that is referenced");
														referenceReports.Add(text13, value14);
													}
													else
													{
														ReferenceReport value15 = new ReferenceReport(text13, varPackage2, text14, false, string.Empty);
														referenceReports.Add(text13, value15);
													}
												}
											}
										}
									}
								}
							}
						}
						streamWriter.WriteLine(text);
					}
				}
			}
			DateTime lastWriteTime = File.GetLastWriteTime(fromFile);
			File.SetLastWriteTime(toFile, lastWriteTime);
		}

		protected void FindCslistRefs(string cslist, HashSet<string> internalFilesHashSet)
		{
			string directoryName = Path.GetDirectoryName(cslist);
			using (StreamReader streamReader = new StreamReader(cslist))
			{
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					string text2 = text.Trim();
					if (!(text2 != string.Empty))
					{
						continue;
					}
					if (directoryName != string.Empty)
					{
						text2 = directoryName + "/" + text2;
					}
					text2 = text2.Replace('\\', '/');
					if (!internalFilesHashSet.Contains(text2) && !referenceReports.ContainsKey(text2))
					{
						if (File.Exists(text2))
						{
							ReferenceReport value = new ReferenceReport(text2, null, null, true, "FIXABLE: References local relative file " + text2 + " not included in package", true);
							referenceReports.Add(text2, value);
						}
						else
						{
							ReferenceReport value2 = new ReferenceReport(text2, null, null, true, "BROKEN: References local relative file " + text2 + " not included in package. File is missing", true);
							referenceReports.Add(text2, value2);
						}
					}
				}
			}
		}

		protected void ClearReferenceItems()
		{
			foreach (PackageBuilderReferenceItem referenceItem in referenceItems)
			{
				UnityEngine.Object.Destroy(referenceItem.gameObject);
			}
			referenceItems.Clear();
			foreach (PackageBuilderReferenceItem licenseReportItem in licenseReportItems)
			{
				UnityEngine.Object.Destroy(licenseReportItem.gameObject);
			}
			licenseReportItems.Clear();
			allReferencedPackages.Clear();
			allReferencedPackageUids.Clear();
			allPackagesDependencyTree = null;
		}

		protected void AddReferenceItem(ReferenceReport referenceReport)
		{
			RectTransform rectTransform = UnityEngine.Object.Instantiate(referencesItemPrefab);
			PackageBuilderReferenceItem component = rectTransform.GetComponent<PackageBuilderReferenceItem>();
			if (component != null)
			{
				rectTransform.SetParent(referencesContainer, false);
				component.Reference = referenceReport.Reference;
				if (referenceReport.HasIssue)
				{
					component.SetIssueColor(errorColor);
					component.Issue = referenceReport.Issue;
				}
				else
				{
					component.Issue = string.Empty;
				}
				referenceItems.Add(component);
			}
		}

		protected void SyncReferences()
		{
			ClearReferenceItems();
			if (referenceReports == null || !(referencesItemPrefab != null) || !(referencesContainer != null))
			{
				return;
			}
			bool active = false;
			foreach (ReferenceReport value in referenceReports.Values)
			{
				if (value.HasIssue && !value.Fixable)
				{
					AddReferenceItem(value);
				}
			}
			foreach (ReferenceReport value2 in referenceReports.Values)
			{
				if (value2.HasIssue && value2.Fixable)
				{
					active = true;
					AddReferenceItem(value2);
				}
			}
			foreach (ReferenceReport value3 in referenceReports.Values)
			{
				if (!value3.HasIssue)
				{
					AddReferenceItem(value3);
				}
			}
			if (fixReferencesAction.button != null)
			{
				fixReferencesAction.button.gameObject.SetActive(active);
			}
		}

		public static void GetPackageDependenciesRecursive(VarPackage currentPackage, string currentPackageUid, HashSet<string> visited, HashSet<VarPackage> allReferencedPackages, HashSet<string> allReferencedPackageUids, JSONClass jc)
		{
			visited.Add(currentPackageUid);
			allReferencedPackageUids.Add(currentPackageUid);
			JSONClass jSONClass = (JSONClass)(jc[currentPackageUid] = new JSONClass());
			if (currentPackage == null)
			{
				jSONClass["missing"].AsBool = true;
				jSONClass["licenseType"] = "MISSING";
			}
			else
			{
				jSONClass["licenseType"] = currentPackage.LicenseType;
			}
			JSONClass jc2 = (JSONClass)(jSONClass["dependencies"] = new JSONClass());
			if (currentPackage == null)
			{
				return;
			}
			allReferencedPackages.Add(currentPackage);
			foreach (string packageDependency in currentPackage.PackageDependencies)
			{
				if (!visited.Contains(packageDependency))
				{
					VarPackage package = FileManager.GetPackage(packageDependency);
					GetPackageDependenciesRecursive(package, packageDependency, visited, allReferencedPackages, allReferencedPackageUids, jc2);
				}
			}
		}

		protected void BuildDependencyTree()
		{
			allPackagesDependencyTree = new JSONClass();
			allReferencedPackages = new HashSet<VarPackage>();
			allReferencedPackageUids = new HashSet<string>();
			HashSet<string> hashSet = new HashSet<string>();
			foreach (ReferenceReport value in referenceReports.Values)
			{
				if (value.PackageUid != null && !hashSet.Contains(value.PackageUid))
				{
					hashSet.Add(value.PackageUid);
					HashSet<string> visited = new HashSet<string>();
					GetPackageDependenciesRecursive(value.Package, value.PackageUid, visited, allReferencedPackages, allReferencedPackageUids, allPackagesDependencyTree);
				}
			}
		}

		protected void AddLicenseReportItem(string packageId, string licenseType, Color c)
		{
			if (licenseReportContainer != null && referencesItemPrefab != null)
			{
				RectTransform rectTransform = UnityEngine.Object.Instantiate(referencesItemPrefab);
				PackageBuilderReferenceItem component = rectTransform.GetComponent<PackageBuilderReferenceItem>();
				if (component != null)
				{
					rectTransform.SetParent(licenseReportContainer, false);
					component.Reference = packageId;
					component.Issue = licenseType;
					component.SetIssueColor(c);
					licenseReportItems.Add(component);
				}
			}
		}

		protected void SyncDependencyLicenseReport()
		{
			foreach (PackageBuilderReferenceItem licenseReportItem in licenseReportItems)
			{
				UnityEngine.Object.Destroy(licenseReportItem.gameObject);
			}
			licenseReportItems.Clear();
			licenseIssue = false;
			nonCommercialLicenseIssue = false;
			if (alreadyReported == null)
			{
				alreadyReported = new HashSet<string>();
			}
			else
			{
				alreadyReported.Clear();
			}
			if (_licenseType == "PC" || _licenseType == "PC EA")
			{
				foreach (VarPackage allReferencedPackage in allReferencedPackages)
				{
					for (int i = 0; i < licenseTypes.Length; i++)
					{
						if (!(allReferencedPackage.LicenseType == licenseTypes[i]) || !licenseNonCommercialFlags[i])
						{
							continue;
						}
						if (allReferencedPackage.LicenseType == "PC EA")
						{
							DateTime today = DateTime.Today;
							int year = today.Year;
							int month = today.Month;
							int day = today.Day;
							int num = MonthStringToMonthInt(allReferencedPackage.EAEndMonth);
							bool flag = false;
							int result;
							int result2;
							if (num != 0 && int.TryParse(allReferencedPackage.EAEndYear, out result) && int.TryParse(allReferencedPackage.EAEndDay, out result2))
							{
								if (year > result)
								{
									flag = true;
								}
								else if (year == result)
								{
									if (month > num)
									{
										flag = true;
									}
									else if (month == num && day > result2)
									{
										flag = true;
									}
								}
							}
							else
							{
								flag = true;
							}
							if (flag)
							{
								for (int j = 0; j < licenseTypes.Length; j++)
								{
									if (allReferencedPackage.SecondaryLicenseType == licenseTypes[j] && licenseNonCommercialFlags[j])
									{
										licenseIssue = true;
										nonCommercialLicenseIssue = true;
										alreadyReported.Add(allReferencedPackage.Uid);
										AddLicenseReportItem(allReferencedPackage.Uid, allReferencedPackage.LicenseType, errorColor);
										break;
									}
								}
								continue;
							}
							licenseIssue = true;
							nonCommercialLicenseIssue = true;
							alreadyReported.Add(allReferencedPackage.Uid);
							AddLicenseReportItem(allReferencedPackage.Uid, allReferencedPackage.LicenseType, errorColor);
							break;
						}
						licenseIssue = true;
						nonCommercialLicenseIssue = true;
						alreadyReported.Add(allReferencedPackage.Uid);
						AddLicenseReportItem(allReferencedPackage.Uid, allReferencedPackage.LicenseType, errorColor);
						break;
					}
				}
			}
			foreach (VarPackage allReferencedPackage2 in allReferencedPackages)
			{
				if (alreadyReported.Contains(allReferencedPackage2.Uid))
				{
					continue;
				}
				for (int k = 0; k < licenseTypes.Length; k++)
				{
					if (!(allReferencedPackage2.LicenseType == licenseTypes[k]) || licenseDistributableFlags[k])
					{
						continue;
					}
					if (allReferencedPackage2.LicenseType == "PC EA")
					{
						DateTime today2 = DateTime.Today;
						int year2 = today2.Year;
						int month2 = today2.Month;
						int day2 = today2.Day;
						int num2 = MonthStringToMonthInt(allReferencedPackage2.EAEndMonth);
						bool flag2 = false;
						int result3;
						int result4;
						if (num2 != 0 && int.TryParse(allReferencedPackage2.EAEndYear, out result3) && int.TryParse(allReferencedPackage2.EAEndDay, out result4))
						{
							if (year2 > result3)
							{
								flag2 = true;
							}
							else if (year2 == result3)
							{
								if (month2 > num2)
								{
									flag2 = true;
								}
								else if (month2 == num2 && day2 > result4)
								{
									flag2 = true;
								}
							}
						}
						else
						{
							flag2 = true;
						}
						if (flag2)
						{
							bool flag3 = true;
							for (int l = 0; l < licenseTypes.Length; l++)
							{
								if (allReferencedPackage2.SecondaryLicenseType == licenseTypes[l] && !licenseDistributableFlags[l])
								{
									licenseIssue = true;
									alreadyReported.Add(allReferencedPackage2.Uid);
									AddLicenseReportItem(allReferencedPackage2.Uid, allReferencedPackage2.LicenseType, errorColor);
									flag3 = false;
									break;
								}
							}
							if (flag3)
							{
								alreadyReported.Add(allReferencedPackage2.Uid);
								AddLicenseReportItem(allReferencedPackage2.Uid, allReferencedPackage2.SecondaryLicenseType, normalColor);
							}
							continue;
						}
						licenseIssue = true;
						alreadyReported.Add(allReferencedPackage2.Uid);
						AddLicenseReportItem(allReferencedPackage2.Uid, allReferencedPackage2.LicenseType, errorColor);
						break;
					}
					licenseIssue = true;
					alreadyReported.Add(allReferencedPackage2.Uid);
					AddLicenseReportItem(allReferencedPackage2.Uid, allReferencedPackage2.LicenseType, errorColor);
					break;
				}
			}
			foreach (VarPackage allReferencedPackage3 in allReferencedPackages)
			{
				if (alreadyReported.Contains(allReferencedPackage3.Uid))
				{
					continue;
				}
				bool flag4 = false;
				for (int m = 0; m < licenseTypes.Length; m++)
				{
					if (allReferencedPackage3.LicenseType == licenseTypes[m])
					{
						flag4 = true;
						break;
					}
				}
				if (!flag4)
				{
					alreadyReported.Add(allReferencedPackage3.Uid);
					AddLicenseReportItem(allReferencedPackage3.Uid, allReferencedPackage3.LicenseType, warningColor);
				}
			}
			foreach (VarPackage allReferencedPackage4 in allReferencedPackages)
			{
				if (alreadyReported.Contains(allReferencedPackage4.Uid))
				{
					continue;
				}
				for (int n = 0; n < licenseTypes.Length; n++)
				{
					if (allReferencedPackage4.LicenseType == licenseTypes[n] && licenseDistributableFlags[n])
					{
						AddLicenseReportItem(allReferencedPackage4.Uid, allReferencedPackage4.LicenseType, normalColor);
						break;
					}
				}
			}
			if (licenseReportIssueText != null)
			{
				licenseReportIssueText.gameObject.SetActive(licenseIssue && !nonCommercialLicenseIssue);
			}
			if (nonCommercialLicenseReportIssueText != null)
			{
				nonCommercialLicenseReportIssueText.gameObject.SetActive(nonCommercialLicenseIssue);
			}
		}

		public void PrepPackage()
		{
			ClearStatus();
			string creatorName = UserPreferences.singleton.creatorName;
			if (creatorName == "Anonymous")
			{
				ShowErrorStatus("Creator name cannot be Anonymous. Please set your creator name in User Preferences.");
				return;
			}
			if (_packageName == null || _packageName == string.Empty)
			{
				ShowErrorStatus("Package name is not set " + _packageName);
				return;
			}
			if (contentItems.Keys.Count == 0)
			{
				ShowErrorStatus("No files or directory contents selected");
				return;
			}
			string packageFolder = FileManager.PackageFolder;
			packageFolder = packageFolder.TrimEnd('/', '\\');
			packageFolder += "Builder/";
			int num = 1;
			string text = creatorName + "." + _packageName;
			VarPackageGroup packageGroup = FileManager.GetPackageGroup(text);
			if (packageGroup != null)
			{
				num = packageGroup.NewestVersion + 1;
			}
			Version = num;
			try
			{
				_preppedDir = packageFolder + creatorName + "." + _packageName + "." + num + ".var";
				if (File.Exists(_preppedDir))
				{
					ShowErrorStatus("Unexpected error during prep. File " + _preppedDir + " already exists ");
					return;
				}
				if (Directory.Exists(_preppedDir))
				{
					Directory.Delete(_preppedDir, true);
				}
				Directory.CreateDirectory(_preppedDir);
				HashSet<string> hashSet = new HashSet<string>();
				foreach (string key2 in contentItems.Keys)
				{
					if (Directory.Exists(key2))
					{
						string[] files = Directory.GetFiles(key2, "*", SearchOption.AllDirectories);
						string[] array = files;
						foreach (string text2 in array)
						{
							string item = text2.Replace('\\', '/');
							hashSet.Add(item);
						}
					}
					else
					{
						if (!File.Exists(key2))
						{
							ShowErrorStatus("Unexpected error during prep. Missing content item " + key2);
							return;
						}
						hashSet.Add(key2);
					}
				}
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				foreach (string item2 in hashSet)
				{
					if (!item2.EndsWith(".vam"))
					{
						continue;
					}
					using (StreamReader streamReader = new StreamReader(item2))
					{
						string aJSON = streamReader.ReadToEnd();
						JSONClass asObject = JSON.Parse(aJSON).AsObject;
						if (asObject["uid"] != null)
						{
							string key = asObject["uid"];
							if (!dictionary.ContainsKey(key))
							{
								dictionary.Add(key, item2);
							}
						}
					}
				}
				referenceReports.Clear();
				foreach (string item3 in hashSet)
				{
					if (!item3.EndsWith(".fav"))
					{
						string text3 = _preppedDir + "/" + item3;
						string directoryName = Path.GetDirectoryName(text3);
						if (!Directory.Exists(directoryName))
						{
							Directory.CreateDirectory(directoryName);
						}
						if (Regex.IsMatch(item3, "\\.(json|vap|vaj)$"))
						{
							CopyAndFixRefs(text, item3, text3, hashSet, dictionary);
						}
						else if (Regex.IsMatch(item3, "\\.cslist$"))
						{
							FindCslistRefs(item3, hashSet);
							File.Copy(item3, text3, true);
						}
						else
						{
							File.Copy(item3, text3, true);
						}
					}
				}
				SyncReferences();
				BuildDependencyTree();
				SyncDependencyLicenseReport();
				NeedsPrep = false;
				ShowStatus("Prep complete. Please complete forms and finalization.");
			}
			catch (Exception ex)
			{
				ShowErrorStatus("Exception during prep");
				SuperController.LogError("Exception during package prep: " + ex);
			}
		}

		protected void FixReferences()
		{
			if (referenceReports != null)
			{
				foreach (ReferenceReport value in referenceReports.Values)
				{
					if (value.HasIssue && value.Fixable)
					{
						if (value.FixedReference != null)
						{
							AddContentItem(value.FixedReference);
						}
						else
						{
							AddContentItem(value.Reference);
						}
					}
				}
			}
			PrepPackage();
		}

		protected void SyncDescription(string s)
		{
			_description = s;
		}

		protected void SyncCredits(string s)
		{
			_credits = s;
		}

		protected void SyncInstructions(string s)
		{
			_instructions = s;
		}

		protected void SyncPromotionalLink(string s)
		{
			_promotionalLink = s;
			if (promotionalButton != null)
			{
				if (!SuperController.singleton.promotionalDisabled && s != null && s != string.Empty)
				{
					promotionalButton.gameObject.SetActive(true);
				}
				else
				{
					promotionalButton.gameObject.SetActive(false);
				}
			}
			if (promotionalButtonText != null)
			{
				promotionalButtonText.text = s;
			}
			if (copyPromotionalLinkAction != null && copyPromotionalLinkAction.button != null)
			{
				if (!SuperController.singleton.promotionalDisabled && s != null && s != string.Empty)
				{
					copyPromotionalLinkAction.button.gameObject.SetActive(true);
				}
				else
				{
					copyPromotionalLinkAction.button.gameObject.SetActive(false);
				}
			}
		}

		protected void SyncLicenseDescriptionText()
		{
			bool flag = _licenseType == "PC EA";
			if (!(licenseDescriptionText != null))
			{
				return;
			}
			bool flag2 = false;
			for (int i = 0; i < licenseTypes.Length; i++)
			{
				if (_licenseType == licenseTypes[i])
				{
					licenseDescriptionText.text = licenseDescriptions[i];
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				licenseDescriptionText.text = "Unknown license type";
			}
			if (!flag)
			{
				return;
			}
			flag2 = false;
			for (int j = 0; j < licenseTypes.Length; j++)
			{
				if (_secondaryLicenseType == licenseTypes[j])
				{
					Text text = licenseDescriptionText;
					text.text = text.text + "\nSecondary license: " + licenseDescriptions[j];
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				licenseDescriptionText.text += "\nSecondary license: Unknown license type";
			}
		}

		protected void SyncLicenseType(string s)
		{
			_licenseType = s;
			SyncLicenseDescriptionText();
			bool active = _licenseType == "PC EA";
			if (secondaryLicenseTypeJSON != null && secondaryLicenseTypeJSON.popup != null)
			{
				secondaryLicenseTypeJSON.popup.gameObject.SetActive(active);
			}
			if (EAYearJSON != null && EAYearJSON.popup != null)
			{
				EAYearJSON.popup.gameObject.SetActive(active);
			}
			if (EAMonthJSON != null && EAMonthJSON.popup != null)
			{
				EAMonthJSON.popup.gameObject.SetActive(active);
			}
			if (EADayJSON != null && EADayJSON.popup != null)
			{
				EADayJSON.popup.gameObject.SetActive(active);
			}
			SyncDependencyLicenseReport();
		}

		protected void SyncSecondaryLicenseType(string s)
		{
			_secondaryLicenseType = s;
			SyncLicenseDescriptionText();
		}

		protected int MonthStringToMonthInt(string monthString)
		{
			switch (monthString)
			{
			case "Jan":
				return 1;
			case "Feb":
				return 2;
			case "Mar":
				return 3;
			case "Apr":
				return 4;
			case "May":
				return 5;
			case "Jun":
				return 6;
			case "Jul":
				return 7;
			case "Aug":
				return 8;
			case "Sep":
				return 9;
			case "Oct":
				return 10;
			case "Nov":
				return 11;
			case "Dec":
				return 12;
			default:
				return 0;
			}
		}

		public void OpenPrepFolderInExplorer()
		{
			SuperController.singleton.OpenFolderInExplorer(_preppedDir);
		}

		protected void ProcessFileMethod(object sender, ScanEventArgs args)
		{
			finalizeFileProgressCount++;
			if (finalizeTotalFileCount != 0)
			{
				finalizeProgress = (float)finalizeFileProgressCount / (float)finalizeTotalFileCount;
			}
			if (finalizeThreadAbort)
			{
				args.ContinueRunning = false;
			}
		}

		protected void FinalizePackageThreaded()
		{
			try
			{
				using (StreamWriter streamWriter = new StreamWriter(_preppedDir + "/meta.json"))
				{
					JSONClass jSONClass = new JSONClass();
					licenseTypeJSON.StoreJSON(jSONClass, true, true, true);
					if (licenseTypeJSON.val == "PC EA")
					{
						EAYearJSON.StoreJSON(jSONClass, true, true, true);
						EAMonthJSON.StoreJSON(jSONClass, true, true, true);
						EADayJSON.StoreJSON(jSONClass, true, true, true);
						secondaryLicenseTypeJSON.StoreJSON(jSONClass, true, true, true);
					}
					creatorNameJSON.StoreJSON(jSONClass, true, true, true);
					packageNameJSON.StoreJSON(jSONClass, true, true, true);
					standardReferenceVersionOptionJSON.StoreJSON(jSONClass, true, true, true);
					scriptReferenceVersionOptionJSON.StoreJSON(jSONClass, true, true, true);
					descriptionJSON.StoreJSON(jSONClass, true, true, true);
					creditsJSON.StoreJSON(jSONClass, true, true, true);
					instructionsJSON.StoreJSON(jSONClass, true, true, true);
					promotionalLinkJSON.StoreJSON(jSONClass, true, true, true);
					jSONClass["programVersion"] = SuperController.singleton.GetVersion();
					JSONArray jSONArray = (JSONArray)(jSONClass["contentList"] = new JSONArray());
					foreach (string key in contentItems.Keys)
					{
						jSONArray.Add(key);
					}
					jSONClass["dependencies"] = allPackagesDependencyTree;
					JSONClass jc = (JSONClass)(jSONClass["customOptions"] = new JSONClass());
					VarPackageCustomOption[] array = customOptions;
					foreach (VarPackageCustomOption varPackageCustomOption in array)
					{
						varPackageCustomOption.StoreJSON(jc);
					}
					bool asBool = false;
					JSONArray jSONArray2 = new JSONArray();
					foreach (ReferenceReport value in referenceReports.Values)
					{
						if (value.HasIssue)
						{
							asBool = true;
							JSONClass jSONClass2 = new JSONClass();
							jSONClass2["reference"] = value.Reference;
							jSONClass2["issue"] = value.Issue;
							jSONArray2.Add(jSONClass2);
						}
					}
					jSONClass["hadReferenceIssues"].AsBool = asBool;
					jSONClass["referenceIssues"] = jSONArray2;
					streamWriter.Write(jSONClass.ToString(string.Empty));
				}
				string text = FileManager.PackageFolder + "/" + Path.GetFileName(_preppedDir);
				using (ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(text)))
				{
					string[] files = Directory.GetFiles(_preppedDir, "*", SearchOption.AllDirectories);
					byte[] buffer = new byte[32768];
					string[] array2 = files;
					foreach (string text2 in array2)
					{
						string input = Regex.Replace(text2, "\\\\", "/");
						input = Regex.Replace(input, "^" + Regex.Escape(_preppedDir) + "/", string.Empty);
						ZipEntry zipEntry = new ZipEntry(input);
						zipEntry.IsUnicodeText = true;
						string extension = Path.GetExtension(text2);
						if (Regex.IsMatch(extension, "(mp3|ogg|wav|jpg|jpeg|png|gif|tif|tiff|avi|mp4|assetbundle|scene|vac|zip)", RegexOptions.IgnoreCase))
						{
							zipOutputStream.SetLevel(0);
						}
						else
						{
							zipOutputStream.SetLevel(9);
						}
						zipOutputStream.PutNextEntry(zipEntry);
						using (FileEntryStream fileEntryStream = FileManager.OpenStream(text2))
						{
							StreamUtils.Copy(fileEntryStream.Stream, zipOutputStream, buffer);
						}
						zipEntry.DateTime = File.GetLastWriteTime(text2);
						zipOutputStream.CloseEntry();
						finalizeFileProgressCount++;
						if (finalizeTotalFileCount != 0)
						{
							finalizeProgress = (float)finalizeFileProgressCount / (float)finalizeTotalFileCount;
						}
						if (finalizeThreadAbort)
						{
							return;
						}
					}
				}
				if (allReferencedPackageUids.Count <= 0)
				{
					return;
				}
				using (StreamWriter streamWriter2 = new StreamWriter(text + ".depend.txt"))
				{
					List<string> list = allReferencedPackageUids.ToList();
					list.Sort();
					foreach (string item in list)
					{
						VarPackage package = FileManager.GetPackage(item);
						if (package != null)
						{
							string text3 = item.PadRight(45);
							string text4 = " By: " + package.Creator;
							text3 += text4.PadRight(25);
							string text5 = " License: " + package.LicenseType;
							text3 += text5.PadRight(25);
							if (package.PromotionalLink != null && package.PromotionalLink != string.Empty)
							{
								text3 = text3 + " Link: " + package.PromotionalLink;
							}
							streamWriter2.WriteLine(text3);
						}
						else
						{
							Debug.LogError("Could not find referenced pacakge with id " + item);
						}
					}
				}
			}
			catch (Exception ex)
			{
				finalizeThreadError = ex.ToString();
			}
		}

		protected void AbortFinalizePackageThreaded(bool wait = true)
		{
			if (finalizeThread == null || !finalizeThread.IsAlive)
			{
				return;
			}
			finalizeThreadAbort = true;
			if (wait)
			{
				while (finalizeThread.IsAlive)
				{
					Thread.Sleep(0);
				}
			}
			finalizeThread = null;
		}

		protected void FinalizeComplete()
		{
			finalizeFlag.Raise();
			if (finalizingPanel != null)
			{
				finalizingPanel.gameObject.SetActive(false);
			}
			isFinalizing = false;
		}

		protected IEnumerator FinalizePackageCo()
		{
			finalizeFlag = new AsyncFlag("FinalizePackage");
			SuperController.singleton.SetLoadingIconFlag(finalizeFlag);
			if (finalizingPanel != null)
			{
				finalizingPanel.gameObject.SetActive(true);
			}
			yield return null;
			finalizeThreadError = null;
			finalizeThreadAbort = false;
			finalizeProgress = 0f;
			finalizeFileProgressCount = 0;
			if (finalizeProgressSlider != null)
			{
				finalizeProgressSlider.value = 0f;
			}
			finalizeTotalFileCount = FileManager.FolderContentsCount(_preppedDir);
			finalizeThread = new Thread(FinalizePackageThreaded);
			finalizeThread.Start();
			while (finalizeThread.IsAlive)
			{
				yield return null;
				if (finalizeProgressSlider != null)
				{
					finalizeProgressSlider.value = finalizeProgress;
				}
			}
			finalizeThread = null;
			if (finalizeThreadError != null)
			{
				ShowErrorStatus("Exception during finalize");
				SuperController.LogError("Exception during package finalize: " + finalizeThreadError);
			}
			else
			{
				FileManager.Refresh();
				NeedsPrep = true;
				ShowStatus("Package complete and ready for use");
			}
			FinalizeComplete();
		}

		protected void FinalizeCheckConfirm()
		{
			if (finalizeCheckPanel != null)
			{
				finalizeCheckPanel.gameObject.SetActive(false);
			}
			if (!NeedsPrep && !isFinalizing)
			{
				isFinalizing = true;
				StartCoroutine(FinalizePackageCo());
			}
		}

		protected void FinalizeCheckCancel()
		{
			if (finalizeCheckPanel != null)
			{
				finalizeCheckPanel.gameObject.SetActive(false);
			}
		}

		protected void FinalizeCheck()
		{
			if (finalizeCheckPanel != null)
			{
				finalizeCheckPanel.gameObject.SetActive(true);
			}
			else
			{
				FinalizeCheckConfirm();
			}
		}

		public void FinalizePackage()
		{
			if (NeedsPrep || isFinalizing)
			{
				return;
			}
			bool flag = false;
			foreach (ReferenceReport value in referenceReports.Values)
			{
				if (value.HasIssue)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				FinalizeCheck();
			}
			else if (licenseIssue || nonCommercialLicenseIssue)
			{
				string text = string.Empty;
				if (licenseIssue)
				{
					text += "Your package has references to other packages that are not re-distributable (PC, PC EA, or Questionable). These references may make it harder for those using your package to use it correctly and your var package may not be designated as Hub Hosted if uploaded to the Hub. Click Cancel if you want to go back and review or fix.\n\n";
				}
				if (nonCommercialLicenseIssue)
				{
					text += "Your package is marked as PC or PC EA (Paid Content) and has references to non-commercial (NC) licensed packages. You are likely in violation of the licenses unless you are the creator of the other packages or have special permission. Click Cancel to go back and review reference licenses in the Reference License Report section. Only click OK if you have explicit permission.";
				}
				SuperController.AlertUser(text, FinalizeCheckConfirm, FinalizeCheckCancel);
			}
			else
			{
				FinalizeCheckConfirm();
			}
		}

		protected void CancelFinalize()
		{
			if (isFinalizing)
			{
				StopAllCoroutines();
				AbortFinalizePackageThreaded();
				string path = FileManager.PackageFolder + "/" + Path.GetFileName(_preppedDir);
				FileManager.DeleteFile(path);
				ShowErrorStatus("Finalize aborted");
				FinalizeComplete();
			}
		}

		protected void Update()
		{
			if (!isManager)
			{
				string val = FixNameToBeValidPathName(UserPreferences.singleton.creatorName);
				creatorNameJSON.val = val;
			}
		}

		protected void Init()
		{
			shortCuts = new List<ShortCut>();
			ShortCut shortCut = new ShortCut();
			shortCut.displayName = "Saves";
			shortCut.path = "Saves";
			shortCuts.Add(shortCut);
			shortCut = new ShortCut();
			shortCut.displayName = "Saves\\scene";
			shortCut.path = "Saves\\scene";
			shortCuts.Add(shortCut);
			shortCut = new ShortCut();
			shortCut.displayName = "Custom";
			shortCut.path = "Custom";
			shortCuts.Add(shortCut);
			shortCut = new ShortCut();
			shortCut.displayName = "Custom\\Atom\\Person";
			shortCut.path = "Custom\\Atom\\Person";
			shortCuts.Add(shortCut);
			clearAllJSON = new JSONStorableAction("ClearAll", ClearAll);
			RegisterAction(clearAllJSON);
			loadMetaFromExistingPackageJSON = new JSONStorableAction("LoadMetaFromExistingPackage", LoadMetaFromExistingPackage);
			RegisterAction(loadMetaFromExistingPackageJSON);
			if (isManager)
			{
				packageItems = new Dictionary<string, PackageBuilderPackageItem>();
				missingPackageItems = new Dictionary<string, PackageBuilderPackageItem>();
				packageReferenceItems = new List<PackageBuilderPackageItem>();
				scanHubForMissingPackagesAction = new JSONStorableAction("ScanHubForMissingPackages", ScanHubForMissingPackages);
				RegisterAction(scanHubForMissingPackagesAction);
				selectCurrentScenePackageAction = new JSONStorableAction("SelectCurrentScenePackage", SelectCurrentScenePackage);
				RegisterAction(selectCurrentScenePackageAction);
				showDisabledJSON = new JSONStorableBool("showDisabled", false, SyncShowDisabled);
				RegisterBool(showDisabledJSON);
				List<string> list = new List<string>();
				CategoryFilter[] array = categoryFilters;
				foreach (CategoryFilter categoryFilter in array)
				{
					list.Add(categoryFilter.name);
				}
				categoryJSON = new JSONStorableStringChooser("category", list, "All", "Category", CategoryChangeCallback);
				RegisterStringChooser(categoryJSON);
				goToPromotionalLinkAction = new JSONStorableAction("GoToPromotionalLink", GoToPromotionalLink);
				copyPromotionalLinkAction = new JSONStorableAction("CopyPromotionalLink", CopyPromotionalLink);
				currentPackageIsOnHubJSON = new JSONStorableBool("currentPackageIsOnHub", false);
				openOnHubAction = new JSONStorableAction("OpenOnHub", OpenOnHub);
				openInHubDownloaderAction = new JSONStorableAction("OpenInHubDownloader", OpenInHubDownloader);
				currentPackageHasSceneJSON = new JSONStorableBool("currentPackageHasScene", false);
				openSceneAction = new JSONStorableAction("OpenScene", OpenScene);
				FileManager.RegisterRefreshHandler(SyncPackages);
				packageEnabledJSON = new JSONStorableBool("packageEnabled", true, SyncPackageEnabled);
				deletePackageAction = new JSONStorableAction("DeletePackage", DeletePackage);
				confirmDeletePackageAction = new JSONStorableAction("ConfirmDeletePackage", ConfirmDeletePackage);
				cancelDeletePackageAction = new JSONStorableAction("CancelDeletePackage", CancelDeletePackage);
				hadReferenceIssuesJSON = new JSONStorableBool("hadReferenceIssues", false);
				userNotesJSON = new JSONStorableString("userNotes", string.Empty, SyncUserNotes);
				pluginsAlwaysEnabledJSON = new JSONStorableBool("pluginsAlwaysEnabled", false, SyncPluginsAlwaysEnabled);
				pluginsAlwaysDisabledJSON = new JSONStorableBool("pluginsAlwaysDisabled", false, SyncPluginsAlwaysDisabled);
				ignoreMissingDependencyErrorsJSON = new JSONStorableBool("ignoreMissingDependencyErrors", false, SyncIgnoreMissingDependencyErrors);
				unpackAction = new JSONStorableAction("Unpack", Unpack);
				confirmUnpackAction = new JSONStorableAction("ConfirmUnpack", ConfirmUnpack);
				cancelUnpackAction = new JSONStorableAction("CancelUnpack", CancelUnpack);
				repackAction = new JSONStorableAction("Repack", Repack);
				restoreFromOriginalAction = new JSONStorableAction("RestoreFromOriginal", RestoreFromOriginal);
				confirmRestoreFromOriginalAction = new JSONStorableAction("ConfirmRestoreFromOriginal", ConfirmRestoreFromOriginal);
				cancelRestoreFromOriginalAction = new JSONStorableAction("CancelRestoreFromOriginal", CancelRestoreFromOriginal);
			}
			creatorNameJSON = new JSONStorableString("creatorName", string.Empty, SyncCreatorName);
			creatorNameJSON.interactable = false;
			packageNameJSON = new JSONStorableString("packageName", string.Empty, SyncPackageName);
			packageNameJSON.enableOnChange = true;
			if (isManager)
			{
				packageNameJSON.interactable = false;
			}
			RegisterString(packageNameJSON);
			VarPackageCustomOption[] array2 = customOptions;
			foreach (VarPackageCustomOption varPackageCustomOption in array2)
			{
				varPackageCustomOption.Init(CustomOptionChange);
			}
			contentItems = new Dictionary<string, PackageBuilderContentItem>();
			addDirectoryAction = new JSONStorableAction("AddDirectory", AddDirectory);
			RegisterAction(addDirectoryAction);
			addFileAction = new JSONStorableAction("AddFile", AddFile);
			RegisterAction(addFileAction);
			removeSelectedAction = new JSONStorableAction("RemoveSelected", RemoveSelected);
			RegisterAction(removeSelectedAction);
			removeAllAction = new JSONStorableAction("RemoveAll", ClearContentItems);
			RegisterAction(removeAllAction);
			referenceReports = new Dictionary<string, ReferenceReport>();
			referenceItems = new List<PackageBuilderReferenceItem>();
			licenseReportItems = new List<PackageBuilderReferenceItem>();
			allReferencedPackages = new HashSet<VarPackage>();
			allReferencedPackageUids = new HashSet<string>();
			List<string> list2 = new List<string>();
			list2.Add("Latest");
			list2.Add("Minimum");
			list2.Add("Exact");
			List<string> list3 = new List<string>();
			list3.Add("Latest (Recommended)");
			list3.Add("Minimum");
			list3.Add("Exact");
			standardReferenceVersionOptionJSON = new JSONStorableStringChooser("standardReferenceVersionOption", list2, list3, "Latest", "Standard reference version option", SyncStandardReferenceVersionOption);
			RegisterStringChooser(standardReferenceVersionOptionJSON);
			List<string> list4 = new List<string>();
			list4.Add("Latest");
			list4.Add("MinVersion");
			list4.Add("Exact (Recommended)");
			scriptReferenceVersionOptionJSON = new JSONStorableStringChooser("scriptReferenceVersionOption", list2, list4, "Exact", "Script reference version option", SyncScriptReferenceVersionOption);
			RegisterStringChooser(scriptReferenceVersionOptionJSON);
			if (!isManager)
			{
				prepPackageAction = new JSONStorableAction("PrepPackage", PrepPackage);
				RegisterAction(prepPackageAction);
				fixReferencesAction = new JSONStorableAction("FixReferences", FixReferences);
				RegisterAction(fixReferencesAction);
			}
			descriptionJSON = new JSONStorableString("description", string.Empty, SyncDescription);
			RegisterString(descriptionJSON);
			creditsJSON = new JSONStorableString("credits", string.Empty, SyncCredits);
			RegisterString(creditsJSON);
			instructionsJSON = new JSONStorableString("instructions", string.Empty, SyncInstructions);
			RegisterString(instructionsJSON);
			if (isManager)
			{
				descriptionJSON.interactable = false;
				creditsJSON.interactable = false;
				instructionsJSON.interactable = false;
			}
			promotionalLinkJSON = new JSONStorableString("promotionalLink", string.Empty, SyncPromotionalLink);
			RegisterString(promotionalLinkJSON);
			List<string> choicesList = new List<string>(licenseTypes);
			licenseTypeJSON = new JSONStorableStringChooser("licenseType", choicesList, _licenseType, "License Type", SyncLicenseType);
			RegisterStringChooser(licenseTypeJSON);
			List<string> choicesList2 = new List<string>(yearChoices);
			List<string> choicesList3 = new List<string>(monthChoices);
			List<string> list5 = new List<string>();
			for (int k = 1; k <= 31; k++)
			{
				list5.Add(k.ToString());
			}
			EAYearJSON = new JSONStorableStringChooser("EAEndYear", choicesList2, "2020", "EA Ends");
			RegisterStringChooser(EAYearJSON);
			EAMonthJSON = new JSONStorableStringChooser("EAEndMonth", choicesList3, "Jan", string.Empty);
			RegisterStringChooser(EAMonthJSON);
			EADayJSON = new JSONStorableStringChooser("EAEndDay", list5, "1", string.Empty);
			RegisterStringChooser(EADayJSON);
			List<string> list6 = new List<string>(licenseTypes);
			list6.RemoveRange(0, 3);
			secondaryLicenseTypeJSON = new JSONStorableStringChooser("secondaryLicenseType", list6, _secondaryLicenseType, "Secondary License Type", SyncSecondaryLicenseType);
			RegisterStringChooser(secondaryLicenseTypeJSON);
			if (!isManager)
			{
				openPrepFolderInExplorerAction = new JSONStorableAction("OpenPrepFolderInExplorer", OpenPrepFolderInExplorer);
				RegisterAction(openPrepFolderInExplorerAction);
				finalizeCheckConfirmAction = new JSONStorableAction("FinalizeCheckConfirm", FinalizeCheckConfirm);
				RegisterAction(finalizeCheckConfirmAction);
				finalizeCheckCancelAction = new JSONStorableAction("FinalizeCheckCancel", FinalizeCheckCancel);
				RegisterAction(finalizeCheckCancelAction);
				finalizePackageAction = new JSONStorableAction("FinalizePackage", FinalizePackage);
				RegisterAction(finalizePackageAction);
				cancelFinalizeAction = new JSONStorableAction("CancelFinalize", CancelFinalize);
				RegisterAction(cancelFinalizeAction);
			}
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			if (!(t != null))
			{
				return;
			}
			PackageBuilderUI componentInChildren = t.GetComponentInChildren<PackageBuilderUI>(true);
			if (!(componentInChildren != null))
			{
				return;
			}
			clearAllJSON.RegisterButton(componentInChildren.clearAllButton, isAlt);
			loadMetaFromExistingPackageJSON.RegisterButton(componentInChildren.loadMetaFromExistingPackageButton, isAlt);
			creatorNameJSON.RegisterInputField(componentInChildren.creatorField, isAlt);
			packageNameJSON.RegisterInputField(componentInChildren.packageNameField, isAlt);
			if (!isAlt)
			{
				versionField = componentInChildren.versionField;
			}
			if (!isAlt)
			{
				statusText = componentInChildren.statusText;
			}
			if (isManager)
			{
				scanHubForMissingPackagesAction.RegisterButton(componentInChildren.scanHubForMissingPackagesButton, isAlt);
				selectCurrentScenePackageAction.RegisterButton(componentInChildren.selectCurrentScenePackageButton, isAlt);
				showDisabledJSON.RegisterToggle(componentInChildren.showDisabledToggle, isAlt);
				goToPromotionalLinkAction.RegisterButton(componentInChildren.promotionalButton, isAlt);
				copyPromotionalLinkAction.RegisterButton(componentInChildren.copyPromotionalLinkButton, isAlt);
				openOnHubAction.RegisterButton(componentInChildren.openOnHubButton, isAlt);
				openInHubDownloaderAction.RegisterButton(componentInChildren.openInHubDownloaderButton, isAlt);
				currentPackageHasSceneJSON.RegisterIndicator(componentInChildren.currentPackageHasSceneIndicator, isAlt);
				openSceneAction.RegisterButton(componentInChildren.openSceneButton, isAlt);
				if (!isAlt)
				{
					currentPackageIsOnHubJSON.RegisterIndicator(componentInChildren.currentPackageIsOnHubIndicator);
					currentPackageIsOnHubJSON.RegisterIndicator(componentInChildren.currentPackageIsOnHubIndicator2, true);
					packageReferencesContainer = componentInChildren.packageReferencesContainer;
					packagesContainer = componentInChildren.packagesContainer;
					missingPackagesContainer = componentInChildren.missingPackagesContainer;
					packageCategoryPanel = componentInChildren.packageCategoryPanel;
					if (packageCategoryPanel != null && categoryTogglePrefab != null)
					{
						ToggleGroupValue toggleGroupValue = packageCategoryPanel.gameObject.AddComponent<ToggleGroupValue>();
						CategoryFilter[] array = categoryFilters;
						foreach (CategoryFilter categoryFilter in array)
						{
							RectTransform rectTransform = UnityEngine.Object.Instantiate(categoryTogglePrefab);
							rectTransform.SetParent(packageCategoryPanel, false);
							rectTransform.gameObject.name = categoryFilter.name;
							Text componentInChildren2 = rectTransform.GetComponentInChildren<Text>(true);
							if (componentInChildren2 != null)
							{
								componentInChildren2.text = categoryFilter.name;
							}
							Toggle component = rectTransform.GetComponent<Toggle>();
							if (component != null)
							{
								component.onValueChanged.AddListener(toggleGroupValue.ToggleChanged);
							}
						}
						toggleGroupValue.Init();
						categoryJSON.toggleGroupValue = toggleGroupValue;
					}
					promotionalButton = componentInChildren.promotionalButton;
					promotionalButtonText = componentInChildren.promotionalButtonText;
					SyncPromotionalLink(promotionalLinkJSON.val);
					confirmDeletePackagePanel = componentInChildren.confirmDeletePackagePanel;
					confirmDeletePackageText = componentInChildren.confirmDeletePackageText;
					confirmUnpackPanel = componentInChildren.confirmUnpackPanel;
					confirmRestoreFromOriginalPanel = componentInChildren.confirmRestoreFromOriginalPanel;
					packPanel = componentInChildren.packPanel;
					packProgressSlider = componentInChildren.packProgressSlider;
				}
				packageEnabledJSON.RegisterToggle(componentInChildren.packageEnabledToggle, isAlt);
				deletePackageAction.RegisterButton(componentInChildren.deletePackageButton, isAlt);
				confirmDeletePackageAction.RegisterButton(componentInChildren.confirmDeletePackageButton, isAlt);
				cancelDeletePackageAction.RegisterButton(componentInChildren.cancelDeletePackageButton, isAlt);
				userNotesJSON.RegisterInputField(componentInChildren.userNotesField, isAlt);
				pluginsAlwaysEnabledJSON.RegisterToggle(componentInChildren.pluginsAlwaysEnabledToggle, isAlt);
				pluginsAlwaysDisabledJSON.RegisterToggle(componentInChildren.pluginsAlwaysDisabledToggle, isAlt);
				ignoreMissingDependencyErrorsJSON.RegisterToggle(componentInChildren.ignoreMissingDependencyErrorsToggle, isAlt);
				unpackAction.RegisterButton(componentInChildren.unpackButton, isAlt);
				confirmUnpackAction.RegisterButton(componentInChildren.confirmUnpackButton, isAlt);
				cancelUnpackAction.RegisterButton(componentInChildren.cancelUnpackButton, isAlt);
				repackAction.RegisterButton(componentInChildren.repackButton, isAlt);
				restoreFromOriginalAction.RegisterButton(componentInChildren.restoreFromOriginalButton, isAlt);
				confirmRestoreFromOriginalAction.RegisterButton(componentInChildren.confirmRestoreFromOriginalButton, isAlt);
				cancelRestoreFromOriginalAction.RegisterButton(componentInChildren.cancelRestoreFromOriginalButton, isAlt);
				hadReferenceIssuesJSON.RegisterIndicator(componentInChildren.hadReferenceIssuesIndicator, isAlt);
			}
			if (!isAlt && customOptions != null && componentInChildren.customOptionToggles != null && customOptions.Length == componentInChildren.customOptionToggles.Length)
			{
				for (int j = 0; j < customOptions.Length; j++)
				{
					customOptions[j].SetToggle(componentInChildren.customOptionToggles[j]);
				}
			}
			if (!isAlt)
			{
				contentContainer = componentInChildren.contentContainer;
			}
			addDirectoryAction.RegisterButton(componentInChildren.addDirectoryButton, isAlt);
			addFileAction.RegisterButton(componentInChildren.addFileButton, isAlt);
			removeSelectedAction.RegisterButton(componentInChildren.removeSelectedButton, isAlt);
			removeAllAction.RegisterButton(componentInChildren.removeAllButton, isAlt);
			if (!isAlt)
			{
				referencesContainer = componentInChildren.referencesContainer;
				licenseReportContainer = componentInChildren.licenseReportContainer;
				licenseReportIssueText = componentInChildren.licenseReportIssueText;
				if (licenseReportIssueText != null)
				{
					licenseReportIssueText.gameObject.SetActive(false);
				}
				nonCommercialLicenseReportIssueText = componentInChildren.nonCommercialLicenseReportIssueText;
				if (nonCommercialLicenseReportIssueText != null)
				{
					nonCommercialLicenseReportIssueText.gameObject.SetActive(false);
				}
			}
			standardReferenceVersionOptionJSON.RegisterPopup(componentInChildren.standardReferenceVersionOptionPopup, isAlt);
			scriptReferenceVersionOptionJSON.RegisterPopup(componentInChildren.scriptReferenceVersionOptionPopup, isAlt);
			if (!isManager)
			{
				prepPackageAction.RegisterButton(componentInChildren.prepPackageButton, isAlt);
				fixReferencesAction.RegisterButton(componentInChildren.fixReferencesButton, isAlt);
			}
			if (componentInChildren.fixReferencesButton != null)
			{
				componentInChildren.fixReferencesButton.gameObject.SetActive(false);
			}
			descriptionJSON.RegisterInputField(componentInChildren.descriptionField, isAlt);
			creditsJSON.RegisterInputField(componentInChildren.creditsField, isAlt);
			instructionsJSON.RegisterInputField(componentInChildren.instructionsField, isAlt);
			promotionalLinkJSON.RegisterInputField(componentInChildren.promotionalField, isAlt);
			licenseTypeJSON.RegisterPopup(componentInChildren.licensePopup, isAlt);
			secondaryLicenseTypeJSON.RegisterPopup(componentInChildren.secondaryLicensePopup, isAlt);
			EAYearJSON.RegisterPopup(componentInChildren.EAYearPopup, isAlt);
			EAMonthJSON.RegisterPopup(componentInChildren.EAMonthPopup, isAlt);
			EADayJSON.RegisterPopup(componentInChildren.EADayPopup, isAlt);
			if (!isAlt)
			{
				licenseDescriptionText = componentInChildren.licenseDescriptionText;
				openPrepFolderInExplorerNotice = componentInChildren.openPrepFolderInExplorerNotice;
			}
			if (!isManager)
			{
				openPrepFolderInExplorerAction.RegisterButton(componentInChildren.openPrepFolderInExplorerButton, isAlt);
				finalizeCheckConfirmAction.RegisterButton(componentInChildren.finalizeCheckConfirmButton, isAlt);
				finalizeCheckCancelAction.RegisterButton(componentInChildren.finalizeCheckCancelButton, isAlt);
				finalizePackageAction.RegisterButton(componentInChildren.finalizeButton, isAlt);
				cancelFinalizeAction.RegisterButton(componentInChildren.cancelFinalizeButton, isAlt);
				if (!isAlt)
				{
					finalizeProgressSlider = componentInChildren.finalizeProgressSlider;
					finalizeCheckPanel = componentInChildren.finalizeCheckPanel;
					finalizingPanel = componentInChildren.finalizingPanel;
				}
			}
			SyncLicenseType(_licenseType);
			if (isManager)
			{
				SyncPackages();
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
				NeedsPrep = true;
			}
		}

		protected void OnDestroy()
		{
			AbortFinalizePackageThreaded(false);
		}
	}
}
