using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using UnityEngine;
using UnityEngine.UI;

namespace uFileBrowser
{
	public class FileBrowser : MonoBehaviour
	{
		public class FileAndDirInfo
		{
			protected bool _isWriteable;

			protected bool _isDirectory;

			public FileButton button;

			public bool isWriteable => _isWriteable;

			public bool isTemplate
			{
				get
				{
					if (FileEntry != null)
					{
						return FileEntry.HasFlagFile("template");
					}
					return false;
				}
			}

			public bool isTemplateModifiable
			{
				get
				{
					if (FileEntry != null)
					{
						return (FileEntry as VarFileEntry)?.IsFlagFileModifiable("template") ?? true;
					}
					return true;
				}
			}

			public bool isFavorite
			{
				get
				{
					if (FileEntry != null)
					{
						return FileEntry.IsFavorite();
					}
					return false;
				}
			}

			public bool isHidden
			{
				get
				{
					if (FileEntry != null)
					{
						return FileEntry.IsHidden();
					}
					if (DirectoryEntry != null)
					{
						return DirectoryEntry.IsHidden();
					}
					return false;
				}
			}

			public bool isHiddenModifiable
			{
				get
				{
					if (FileEntry != null)
					{
						return (FileEntry as VarFileEntry)?.IsHiddenModifiable() ?? true;
					}
					return true;
				}
			}

			public bool isDirectory => _isDirectory;

			public FileEntry FileEntry { get; protected set; }

			public DirectoryEntry DirectoryEntry { get; protected set; }

			public string Name { get; protected set; }

			public string FullName { get; protected set; }

			public DateTime LastWriteTime { get; protected set; }

			public DateTime LastWriteTimePackage { get; protected set; }

			public FileAndDirInfo(DirectoryEntry dirEntry, string currentPath)
			{
				DirectoryEntry = dirEntry;
				_isDirectory = true;
				_isWriteable = !(dirEntry is VarDirectoryEntry) && FileManager.IsSecureWritePath(dirEntry.FullPath);
				Name = dirEntry.Name;
				if (currentPath != string.Empty)
				{
					FullName = currentPath + "\\" + Name;
				}
				else
				{
					FullName = Name;
				}
				LastWriteTime = dirEntry.LastWriteTime;
				LastWriteTimePackage = LastWriteTime;
			}

			public FileAndDirInfo(FileEntry fEntry)
			{
				FileEntry = fEntry;
				_isDirectory = false;
				_isWriteable = !(fEntry is VarFileEntry) && FileManager.IsSecureWritePath(fEntry.FullPath);
				Name = fEntry.Name;
				FullName = fEntry.Uid;
				LastWriteTime = fEntry.LastWriteTime;
				VarFileEntry varFileEntry = fEntry as VarFileEntry;
				if (varFileEntry != null)
				{
					LastWriteTimePackage = varFileEntry.Package.LastWriteTime;
				}
				else
				{
					LastWriteTimePackage = LastWriteTime;
				}
			}

			public void SetTemplate(bool b)
			{
				if (_isWriteable && FileEntry != null)
				{
					FileEntry.SetFlagFile("template", b);
				}
			}

			public void SetFavorite(bool b)
			{
				if (_isWriteable && FileEntry != null)
				{
					FileEntry.SetFavorite(b);
				}
			}

			public void SetHidden(bool b)
			{
				if (_isWriteable && FileEntry != null)
				{
					FileEntry.SetHidden(b);
				}
			}
		}

		public delegate void ShowHandler(FileBrowserCallback callback, bool changeDirectory);

		public delegate void ShowFullHandler(FileBrowserFullCallback callback, bool changeDirectory);

		public delegate void GotoDirectoryCallback(string path);

		public delegate void GotoDirectoryHandler(string path, string pkgFilter, bool flatten, bool includeRegularDirs);

		public string defaultPath = string.Empty;

		public bool selectDirectory;

		public bool showFiles;

		public bool showDirs = true;

		public bool canCancel = true;

		public bool selectOnClick = true;

		public bool browseVarFilesAsDirectories = true;

		public bool showInstallFolderInDirectoryList;

		public bool forceOnlyShowTemplates;

		public bool allowUseFileAsTemplateSelect;

		public string fileFormat = string.Empty;

		public bool hideExtension;

		public string fileRemovePrefix;

		[SerializeField]
		[HideInInspector]
		private string currentPath;

		[SerializeField]
		[HideInInspector]
		private string search;

		[HideInInspector]
		private string searchLower;

		[SerializeField]
		[HideInInspector]
		private string slash;

		[SerializeField]
		[HideInInspector]
		private List<string> drives;

		private List<DirectoryButton> dirButtons;

		private List<ShortCutButton> shortCutButtons;

		private List<GameObject> dirSpacers;

		private FileButton selected;

		private FileBrowserCallback callback;

		private FileBrowserFullCallback fullCallback;

		public List<ShortCut> shortCuts;

		public bool manageContentTransform;

		public Vector2 cellSize;

		public Vector2 cellSpacing;

		public int columnCount;

		public UIPopup directoryOptionPopup;

		protected UserPreferences.DirectoryOption _directoryOption;

		public UIPopup sortByPopup;

		protected UserPreferences.SortBy _sortBy = UserPreferences.SortBy.NewToOld;

		public GameObject overlay;

		public GameObject window;

		public GameObject fileButtonPrefab;

		public GameObject directoryButtonPrefab;

		public GameObject directorySpacerPrefab;

		public Text titleText;

		public RectTransform fileContent;

		public ScrollRect filesScrollRect;

		public RectTransform dirContent;

		public RectTransform dirOption;

		public GameObject shortCutButtonPrefab;

		public RectTransform shortCutContent;

		public Button openInExplorerButton;

		public Button openPackageButton;

		public Button openOnHubButton;

		public Button promotionalButton;

		public Text promotionalButtonText;

		public Toggle keepOpenToggle;

		[SerializeField]
		protected bool _keepOpen;

		public Toggle onlyShowLatestToggle;

		protected bool _onlyShowLatest = true;

		protected HashSet<string> shortCutsCreators;

		protected List<string> shortCutsCreatorsList;

		public UIPopup shortCutsCreatorFilterPopup;

		public GameObject shortCutsOverflowIndicator;

		protected string _shortCutsCreatorFilter = "All";

		public InputField shortCutsSearchField;

		public Button shortCutsSearchClearButton;

		protected string _shortCutsSearch;

		protected string _shortCutsSearchLower;

		protected bool _ignoreShortCutsSearchChange;

		public Toggle showHiddenToggle;

		protected bool _showHidden;

		public Toggle onlyFavoritesToggle;

		protected bool _onlyFavorites;

		public Toggle onlyTemplatesToggle;

		protected bool _onlyTemplates;

		protected int _totalPages;

		protected int _page = 1;

		public Slider limitSlider;

		public Text limitValueText;

		[SerializeField]
		protected int _limit = 5;

		public int limitMultiple = 100;

		protected int _limitXMultiple = 500;

		public Text showingCountText;

		public InputField currentPathField;

		public InputField searchField;

		public Button searchCancelButton;

		public Button cancelButton;

		public Button selectButton;

		public Text selectButtonText;

		public Transform renameContainer;

		public InputField renameField;

		public InputFieldAction renameFieldAction;

		protected FileButton renameFileButton;

		public Transform deleteContainer;

		public InputField deleteField;

		protected FileButton deleteFileButton;

		public Text statusField;

		public Text fileHighlightField;

		public InputField fileEntryField;

		public Sprite folderIcon;

		public Sprite defaultIcon;

		public List<FileIcon> fileIcons = new List<FileIcon>();

		private HashSet<ImageLoaderThreaded.QueuedImage> queuedThumbnails;

		public ShowHandler showHandler;

		public ShowFullHandler showFullHandler;

		public bool clearCurrentPathOnHide = true;

		protected Dictionary<string, float> directoryScrollPositions;

		protected string currentPackageUid;

		protected string currentPackageFilter;

		protected bool useFlatten;

		protected bool includeRegularDirsInFlatten;

		public GotoDirectoryCallback gotoDirectoryCallback;

		public GotoDirectoryHandler gotoDirectoryHandler;

		protected bool ignoreSearchChange;

		protected List<FileAndDirInfo> sortedFilesAndDirs;

		protected HashSet<FileButton> displayedFileButtons;

		protected bool cacheDirty = true;

		protected string lastCacheDir;

		protected bool lastCacheUseFlatten;

		protected bool lastCacheIncludeRegularDirsInFlatten;

		protected bool lastCacheShowDirs;

		protected bool lastCacheForceOnlyShowTemplates;

		protected DateTime lastCacheTime;

		protected string lastCacheFileFormat;

		protected string lastCacheFileRemovePrefix;

		protected string lastCachePackageFilter;

		protected List<FileAndDirInfo> cachedFiles;

		protected List<FileAndDirInfo> cachedDirs;

		protected bool threadHadException;

		protected string threadException;

		public string CurrentPath => currentPath;

		public string SelectedPath
		{
			get
			{
				if (selected != null)
				{
					return selected.fullPath;
				}
				return null;
			}
		}

		public UserPreferences.DirectoryOption directoryOption
		{
			get
			{
				return _directoryOption;
			}
			set
			{
				if (_directoryOption != value)
				{
					_directoryOption = value;
					if (directoryOptionPopup != null)
					{
						directoryOptionPopup.currentValueNoCallback = _directoryOption.ToString();
					}
					SyncSort();
				}
			}
		}

		protected UserPreferences.DirectoryOption directoryOptionNoSync
		{
			get
			{
				return _directoryOption;
			}
			set
			{
				if (_directoryOption != value)
				{
					_directoryOption = value;
					if (directoryOptionPopup != null)
					{
						directoryOptionPopup.currentValueNoCallback = _directoryOption.ToString();
					}
				}
			}
		}

		public UserPreferences.SortBy sortBy
		{
			get
			{
				return _sortBy;
			}
			set
			{
				if (_sortBy != value)
				{
					_sortBy = value;
					if (sortByPopup != null)
					{
						sortByPopup.currentValueNoCallback = _sortBy.ToString();
					}
					SyncSort();
				}
			}
		}

		protected UserPreferences.SortBy sortByNoSync
		{
			get
			{
				return _sortBy;
			}
			set
			{
				if (_sortBy != value)
				{
					_sortBy = value;
					if (sortByPopup != null)
					{
						sortByPopup.currentValueNoCallback = _sortBy.ToString();
					}
				}
			}
		}

		public bool keepOpen
		{
			get
			{
				return _keepOpen;
			}
			set
			{
				if (_keepOpen != value)
				{
					_keepOpen = value;
					if (keepOpenToggle != null)
					{
						keepOpenToggle.isOn = _keepOpen;
					}
				}
			}
		}

		public bool onlyShowLatest
		{
			get
			{
				return _onlyShowLatest;
			}
			set
			{
				if (_onlyShowLatest != value)
				{
					_onlyShowLatest = value;
					if (onlyShowLatestToggle != null)
					{
						onlyShowLatestToggle.isOn = _onlyShowLatest;
					}
					UpdateDirectoryList();
				}
			}
		}

		public bool showHidden
		{
			get
			{
				return _showHidden;
			}
			set
			{
				if (_showHidden != value)
				{
					_showHidden = value;
					if (showHiddenToggle != null)
					{
						showHiddenToggle.isOn = _showHidden;
					}
					UpdateDirectoryList();
					ResetDisplayedPage();
				}
			}
		}

		public bool onlyFavorites
		{
			get
			{
				return _onlyFavorites;
			}
			set
			{
				if (_onlyFavorites != value)
				{
					_onlyFavorites = value;
					if (onlyFavoritesToggle != null)
					{
						onlyFavoritesToggle.isOn = _onlyFavorites;
					}
					ResetDisplayedPage();
				}
			}
		}

		public bool onlyTemplates
		{
			get
			{
				return _onlyTemplates;
			}
			set
			{
				if (_onlyTemplates != value)
				{
					_onlyTemplates = value;
					if (onlyTemplatesToggle != null)
					{
						onlyTemplatesToggle.isOn = _onlyTemplates;
					}
					ResetDisplayedPage();
				}
			}
		}

		public int page
		{
			get
			{
				return _page;
			}
			set
			{
				if (_page != value && value <= _totalPages && value > 0)
				{
					_page = value;
					StartCoroutine(DelaySetScroll(1f));
					SyncDisplayed();
				}
			}
		}

		public int limit
		{
			get
			{
				return _limit;
			}
			set
			{
				if (_limit != value)
				{
					_limit = value;
					_limitXMultiple = _limit * limitMultiple;
					if (limitValueText != null)
					{
						limitValueText.text = _limitXMultiple.ToString("F0");
					}
					if (limitSlider != null)
					{
						limitSlider.value = _limit;
					}
					ResetDisplayedPage();
				}
			}
		}

		public void SetShortCuts(List<ShortCut> newShortCuts, bool resetShortCutFilters = false)
		{
			bool flag = resetShortCutFilters;
			if (!flag)
			{
				if (shortCuts == null)
				{
					if (newShortCuts != null)
					{
						flag = true;
					}
				}
				else if (newShortCuts == null)
				{
					flag = true;
				}
				else if (shortCuts.Count == newShortCuts.Count)
				{
					for (int i = 0; i < shortCuts.Count; i++)
					{
						if (!shortCuts[i].IsSameAs(newShortCuts[i]))
						{
							flag = true;
							break;
						}
					}
				}
				else
				{
					flag = true;
				}
			}
			shortCuts = newShortCuts;
			if (flag)
			{
				ResetShortCutsCreators();
				ResetShortCutsSearch();
				UpdateDirectoryList();
			}
		}

		public void ClearCacheImage(string imgPath)
		{
			if (ImageLoaderThreaded.singleton != null)
			{
				ImageLoaderThreaded.singleton.ClearCacheThumbnail(imgPath);
			}
		}

		public void ClearImageQueue()
		{
			foreach (ImageLoaderThreaded.QueuedImage queuedThumbnail in queuedThumbnails)
			{
				queuedThumbnail.cancel = true;
			}
			queuedThumbnails.Clear();
		}

		public void MakeNewUniqueFolder()
		{
			string text = currentPath + slash + "NewFolder";
			int num = 0;
			while (num < 20)
			{
				if (FileManager.DirectoryExists(text))
				{
					num++;
					text = currentPath + slash + "NewFolder" + num;
					continue;
				}
				try
				{
					FileManager.CreateDirectory(text);
				}
				catch (Exception ex)
				{
					Debug.LogError("Could not make directory " + text + " Exception: " + ex.Message);
					if (statusField != null)
					{
						statusField.text = ex.Message;
					}
				}
				break;
			}
			UpdateDirectoryList();
			UpdateFileList();
		}

		public void SetDirectoryOption(string dirOptionString)
		{
			try
			{
				UserPreferences.DirectoryOption fileBrowserDirectoryOption = (this.directoryOption = (UserPreferences.DirectoryOption)Enum.Parse(typeof(UserPreferences.DirectoryOption), dirOptionString));
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.fileBrowserDirectoryOption = fileBrowserDirectoryOption;
				}
			}
			catch (ArgumentException)
			{
				Debug.LogError("Attempted to set directory option to " + dirOptionString + " which is not a valid type");
			}
		}

		public void SetSortBy(string sortByString)
		{
			try
			{
				UserPreferences.SortBy fileBrowserSortBy = (this.sortBy = (UserPreferences.SortBy)Enum.Parse(typeof(UserPreferences.SortBy), sortByString));
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.fileBrowserSortBy = fileBrowserSortBy;
				}
			}
			catch (ArgumentException)
			{
				Debug.LogError("Attempted to set sort by to " + sortByString + " which is not a valid type");
			}
		}

		private void SortFilesAndDirs(List<FileAndDirInfo> fdlist)
		{
			switch (sortBy)
			{
			case UserPreferences.SortBy.AtoZ:
				fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => a.Name.CompareTo(b.Name));
				break;
			case UserPreferences.SortBy.ZtoA:
				fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => b.Name.CompareTo(a.Name));
				break;
			case UserPreferences.SortBy.NewToOld:
				fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => b.LastWriteTime.CompareTo(a.LastWriteTime));
				break;
			case UserPreferences.SortBy.OldToNew:
				fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => a.LastWriteTime.CompareTo(b.LastWriteTime));
				break;
			case UserPreferences.SortBy.NewToOldPackage:
				fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => b.LastWriteTimePackage.CompareTo(a.LastWriteTimePackage));
				break;
			case UserPreferences.SortBy.OldToNewPackage:
				fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => a.LastWriteTimePackage.CompareTo(b.LastWriteTimePackage));
				break;
			}
		}

		protected void ResetShortCutsCreators()
		{
			if (shortCutsCreators == null)
			{
				shortCutsCreators = new HashSet<string>();
			}
			else
			{
				shortCutsCreators.Clear();
			}
			_shortCutsCreatorFilter = "All";
			if (shortCutsCreatorFilterPopup != null)
			{
				shortCutsCreatorFilterPopup.currentValueNoCallback = _shortCutsCreatorFilter;
			}
		}

		protected void AddShortCutsCreator(string creator)
		{
			shortCutsCreators.Add(creator);
		}

		protected void FinalizeShortCutsCreators()
		{
			if (shortCutsCreatorsList == null)
			{
				shortCutsCreatorsList = new List<string>();
			}
			else
			{
				shortCutsCreatorsList.Clear();
			}
			if (shortCutsCreators != null)
			{
				foreach (string shortCutsCreator in shortCutsCreators)
				{
					shortCutsCreatorsList.Add(shortCutsCreator);
				}
			}
			shortCutsCreatorsList.Sort();
			shortCutsCreatorsList.Reverse();
			shortCutsCreatorsList.Add("All");
			shortCutsCreatorsList.Reverse();
			if (shortCutsCreatorFilterPopup != null)
			{
				shortCutsCreatorFilterPopup.numPopupValues = shortCutsCreatorsList.Count;
				for (int i = 0; i < shortCutsCreatorsList.Count; i++)
				{
					shortCutsCreatorFilterPopup.setPopupValue(i, shortCutsCreatorsList[i]);
				}
			}
		}

		public void SetShortCutsCreatorFilter(string creatorFilter)
		{
			if (_shortCutsCreatorFilter != creatorFilter)
			{
				_shortCutsCreatorFilter = creatorFilter;
				if (shortCutsCreatorFilterPopup != null)
				{
					shortCutsCreatorFilterPopup.currentValueNoCallback = _shortCutsCreatorFilter;
				}
				UpdateDirectoryList();
			}
		}

		protected void ResetShortCutsSearch()
		{
			_shortCutsSearch = string.Empty;
			_shortCutsSearchLower = string.Empty;
			if (shortCutsSearchField != null)
			{
				_ignoreShortCutsSearchChange = true;
				shortCutsSearchField.text = string.Empty;
				_ignoreShortCutsSearchChange = false;
			}
		}

		public void ClearShortCutsSearchField()
		{
			if (shortCutsSearchField != null)
			{
				shortCutsSearchField.text = string.Empty;
			}
		}

		public void ShortCutsSearchChanged(string s)
		{
			if (!_ignoreShortCutsSearchChange)
			{
				_shortCutsSearch = s.Trim();
				_shortCutsSearchLower = _shortCutsSearch.ToLowerInvariant();
				UpdateDirectoryList();
			}
		}

		protected void SetShowHidden(bool b)
		{
			showHidden = b;
		}

		protected void SetOnlyFavorites(bool b)
		{
			onlyFavorites = b;
		}

		protected void SetOnlyTemplates(bool b)
		{
			onlyTemplates = b;
		}

		protected void ResetDisplayedPage()
		{
			_page = 1;
			_totalPages = 1;
			StartCoroutine(DelaySetScroll(1f));
			SyncDisplayed();
		}

		public void FirstPage()
		{
			page = 1;
		}

		public void NextPage()
		{
			page++;
		}

		public void PrevPage()
		{
			page--;
		}

		protected void SetLimit(float f)
		{
			limit = Mathf.FloorToInt(f);
		}

		protected FileButton CreateFileButton(string text, string path, bool dir, bool writeable, bool hidden, bool hiddenModifiable, bool favorite, bool isTemplate, bool isTemplateModifiable)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(fileButtonPrefab, Vector3.zero, Quaternion.identity);
			FileButton component = gameObject.GetComponent<FileButton>();
			string text2 = text;
			if (hideExtension)
			{
				text2 = Regex.Replace(text2, "\\.[^\\.]*$", string.Empty);
			}
			if (fileRemovePrefix != null)
			{
				string text3 = Regex.Replace(text2, "^" + fileRemovePrefix, string.Empty);
				if (text2 != text3)
				{
					component.removedPrefix = fileRemovePrefix;
					text2 = text3;
				}
			}
			component.Set(this, text2, path, dir, writeable, hidden, hiddenModifiable, favorite, allowUseFileAsTemplateSelect, allowUseFileAsTemplateSelect && isTemplate, isTemplateModifiable);
			if (ImageLoaderThreaded.singleton != null)
			{
				Transform transform = null;
				if (component.fileIcon != null)
				{
					transform = component.fileIcon.transform;
				}
				Transform transform2 = null;
				if (component.altIcon != null)
				{
					transform2 = component.altIcon.transform;
				}
				if (transform != null)
				{
					transform.gameObject.SetActive(true);
					if (transform2 != null)
					{
						transform2.gameObject.SetActive(false);
						RawImage altIcon = component.altIcon;
						if (altIcon != null)
						{
							FileEntry fileEntry = FileManager.GetFileEntry(path);
							if (fileEntry != null)
							{
								string text4 = Path.GetExtension(fileEntry.Path);
								string text5;
								switch (text4)
								{
								case ".duf":
									text5 = fileEntry.Path + ".png";
									text4 = ".png";
									break;
								case ".json":
								case ".vac":
								case ".vap":
								case ".vam":
								case ".scene":
								case ".assetbundle":
									text5 = Regex.Replace(fileEntry.Path, "\\.(json|vac|vap|vam|scene|assetbundle)$", ".jpg");
									if (!FileManager.FileExists(text5))
									{
										text5 = Regex.Replace(text5, "\\.jpg$", ".JPG");
									}
									text4 = ".jpg";
									break;
								default:
									text5 = fileEntry.Path;
									break;
								}
								string text6 = text4.ToLower();
								if (FileManager.FileExists(text5))
								{
									switch (text6)
									{
									case ".jpg":
									case ".jpeg":
									case ".png":
									case ".tif":
									{
										component.imgPath = text5;
										transform.gameObject.SetActive(false);
										transform2.gameObject.SetActive(true);
										Texture2D cachedThumbnail = ImageLoaderThreaded.singleton.GetCachedThumbnail(text5);
										if (cachedThumbnail != null)
										{
											altIcon.texture = cachedThumbnail;
										}
										break;
									}
									}
								}
							}
						}
					}
				}
			}
			return component;
		}

		private void SyncFileButtonImages()
		{
			foreach (FileButton displayedFileButton in displayedFileButtons)
			{
				SyncFileButtonImage(displayedFileButton);
			}
		}

		private void SyncFileButtonImage(FileButton fb)
		{
			if (fb.imgPath != null && fb.altIcon != null)
			{
				Texture2D cachedThumbnail = ImageLoaderThreaded.singleton.GetCachedThumbnail(fb.imgPath);
				if (cachedThumbnail != null)
				{
					fb.altIcon.texture = cachedThumbnail;
					return;
				}
				ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
				queuedImage.imgPath = fb.imgPath;
				queuedImage.width = 512;
				queuedImage.height = 512;
				queuedImage.setSize = true;
				queuedImage.fillBackground = true;
				queuedImage.rawImageToLoad = fb.altIcon;
				ImageLoaderThreaded.singleton.QueueThumbnail(queuedImage);
				queuedThumbnails.Add(queuedImage);
			}
		}

		private void CreateDirectoryButton(string package, string text, string path, int i)
		{
			if (dirContent != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(directoryButtonPrefab, Vector3.zero, Quaternion.identity);
				gameObject.GetComponent<RectTransform>().SetParent(dirContent, false);
				DirectoryButton component = gameObject.GetComponent<DirectoryButton>();
				component.Set(this, package, currentPackageFilter, text, path);
				dirButtons.Add(component);
			}
		}

		private void CreateShortCutButton(ShortCut shortCut, int i)
		{
			if (shortCutContent != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(shortCutButtonPrefab, Vector3.zero, Quaternion.identity);
				gameObject.GetComponent<RectTransform>().SetParent(shortCutContent, false);
				ShortCutButton component = gameObject.GetComponent<ShortCutButton>();
				component.Set(this, shortCut.package, shortCut.packageFilter, shortCut.flatten, shortCut.includeRegularDirsInFlatten, shortCut.displayName, shortCut.path, i);
				shortCutButtons.Add(component);
			}
		}

		private void CreateDirectorySpacer()
		{
			if (dirContent != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(directorySpacerPrefab, Vector3.zero, Quaternion.identity);
				gameObject.GetComponent<RectTransform>().SetParent(dirContent, false);
				dirSpacers.Add(gameObject);
			}
		}

		public void SetTitle(string title)
		{
			if (titleText != null)
			{
				titleText.text = title;
			}
		}

		protected void ShowInternal(bool changeDirectory = true)
		{
			if (statusField != null)
			{
				statusField.text = string.Empty;
			}
			if (fileEntryField != null)
			{
				fileEntryField.text = string.Empty;
			}
			if (UserPreferences.singleton != null)
			{
				sortBy = UserPreferences.singleton.fileBrowserSortBy;
				directoryOption = UserPreferences.singleton.fileBrowserDirectoryOption;
			}
			if (changeDirectory)
			{
				GotoDirectory(defaultPath);
			}
			UpdateUI();
			if ((bool)overlay)
			{
				overlay.SetActive(true);
			}
			window.SetActive(true);
		}

		public void Show(FileBrowserCallback callback, bool changeDirectory = true)
		{
			if (showHandler != null)
			{
				showHandler(callback, changeDirectory);
				return;
			}
			ShowInternal(changeDirectory);
			this.callback = callback;
			fullCallback = null;
		}

		public void Show(FileBrowserFullCallback fullCallback, bool changeDirectory = true)
		{
			if (showFullHandler != null)
			{
				showFullHandler(fullCallback, changeDirectory);
				return;
			}
			ShowInternal(changeDirectory);
			callback = null;
			this.fullCallback = fullCallback;
		}

		public void ClearCurrentPath()
		{
			ClearDirectoryScrollPos();
			currentPath = string.Empty;
		}

		public void Hide()
		{
			if (window.activeSelf)
			{
				if (selected != null)
				{
					selected.Unselect();
				}
				ClearImageQueue();
				SaveDirectoryScrollPos(currentPath);
				if (clearCurrentPathOnHide)
				{
					currentPath = string.Empty;
				}
				selected = null;
				if ((bool)overlay)
				{
					overlay.SetActive(false);
				}
				window.SetActive(false);
			}
		}

		public bool IsHidden()
		{
			return !window.activeSelf;
		}

		public void UpdateUI()
		{
			if ((bool)cancelButton)
			{
				cancelButton.gameObject.SetActive(canCancel);
			}
			if (currentPathField != null)
			{
				currentPathField.text = currentPath;
			}
			if (searchField != null)
			{
				searchField.text = search;
			}
		}

		public Sprite GetFileIcon(string path)
		{
			string empty = string.Empty;
			if (path.Contains("."))
			{
				empty = path.Substring(path.LastIndexOf('.') + 1);
				for (int i = 0; i < fileIcons.Count; i++)
				{
					if (fileIcons[i].extension == empty)
					{
						return fileIcons[i].icon;
					}
				}
				return defaultIcon;
			}
			return defaultIcon;
		}

		public void OnHiddenChange(FileButton fb, bool b)
		{
			string fullPath = fb.fullPath;
			FileEntry fileEntry = FileManager.GetFileEntry(fullPath, true);
			if (fileEntry != null)
			{
				fileEntry.SetHidden(b);
			}
			else
			{
				FileManager.GetDirectoryEntry(fullPath, true)?.SetHidden(b);
			}
			UpdateDirectoryList();
		}

		public void OnFavoriteChange(FileButton fb, bool b)
		{
			string fullPath = fb.fullPath;
			FileManager.GetFileEntry(fullPath, true)?.SetFavorite(b);
		}

		public void OnUseFileAsTemplateChange(FileButton fb, bool b)
		{
			string fullPath = fb.fullPath;
			FileManager.GetFileEntry(fullPath, true)?.SetFlagFile("template", b);
		}

		private IEnumerator RenameProcess()
		{
			yield return null;
			LookInputModule.SelectGameObject(renameField.gameObject);
			renameField.ActivateInputField();
		}

		public void OnRenameClick(FileButton fb)
		{
			if (statusField != null)
			{
				statusField.text = string.Empty;
			}
			renameFileButton = fb;
			OpenRenameDialog();
			if (renameField != null)
			{
				string text = fb.text;
				if (text.EndsWith(".json"))
				{
					text = text.Replace(".json", string.Empty);
				}
				else if (text.EndsWith(".vac"))
				{
					text = text.Replace(".vac", string.Empty);
				}
				else if (text.EndsWith(".vap"))
				{
					text = text.Replace(".vap", string.Empty);
				}
				renameField.text = text;
				StartCoroutine(RenameProcess());
			}
		}

		protected void OpenRenameDialog()
		{
			if (renameContainer != null)
			{
				renameContainer.gameObject.SetActive(true);
			}
		}

		public void OnRenameConfirm()
		{
			if (renameField != null && renameField.text != string.Empty && renameFileButton != null)
			{
				string text = ((renameFileButton.removedPrefix == null) ? (currentPath + slash + renameField.text) : (currentPath + slash + renameFileButton.removedPrefix + renameField.text));
				if (renameFileButton.isDir)
				{
					string fullPath = renameFileButton.fullPath;
					try
					{
						FileManager.AssertNotCalledFromPlugin();
						FileManager.MoveDirectory(fullPath, text);
					}
					catch (Exception ex)
					{
						Debug.LogError("Could not move directory " + fullPath + " to " + text + " Exception: " + ex.Message);
						if (statusField != null)
						{
							statusField.text = ex.Message;
						}
						OnRenameCancel();
						return;
					}
				}
				else
				{
					string fullPath2 = renameFileButton.fullPath;
					bool flag = false;
					string oldValue = string.Empty;
					if (fullPath2.EndsWith(".json"))
					{
						flag = true;
						oldValue = ".json";
						if (!text.EndsWith(".json"))
						{
							text += ".json";
						}
					}
					else if (fullPath2.EndsWith(".vac"))
					{
						flag = true;
						oldValue = ".vac";
						if (!text.EndsWith(".vac"))
						{
							text += ".vac";
						}
					}
					else if (fullPath2.EndsWith(".vap"))
					{
						flag = true;
						oldValue = ".vap";
						if (!text.EndsWith(".vap"))
						{
							text += ".vap";
						}
					}
					Debug.Log("Rename file " + fullPath2 + " to " + text);
					try
					{
						FileManager.AssertNotCalledFromPlugin();
						FileManager.MoveFile(fullPath2, text, false);
					}
					catch (Exception ex2)
					{
						Debug.LogError("Could not move file " + fullPath2 + " to " + text + " Exception: " + ex2.Message);
						if (statusField != null)
						{
							statusField.text = ex2.Message;
						}
						OnRenameCancel();
						return;
					}
					if (flag)
					{
						string text2 = fullPath2.Replace(oldValue, ".jpg");
						string text3 = text.Replace(oldValue, ".jpg");
						if (FileManager.FileExists(text2))
						{
							try
							{
								FileManager.MoveFile(text2, text3);
							}
							catch (Exception ex3)
							{
								Debug.LogError("Could not move file " + text2 + " to " + text3 + " Exception: " + ex3.Message);
								if (statusField != null)
								{
									statusField.text = ex3.Message;
								}
							}
						}
						string text4 = fullPath2 + ".fav";
						string text5 = text + ".fav";
						if (FileManager.FileExists(text4))
						{
							try
							{
								FileManager.MoveFile(text4, text5);
							}
							catch (Exception ex4)
							{
								Debug.LogError("Could not move file " + text4 + " to " + text5 + " Exception: " + ex4.Message);
								if (statusField != null)
								{
									statusField.text = ex4.Message;
								}
							}
						}
					}
				}
				UpdateFileList();
				UpdateDirectoryList();
			}
			OnRenameCancel();
		}

		public void OnRenameCancel()
		{
			if (renameContainer != null)
			{
				renameContainer.gameObject.SetActive(false);
			}
			renameFileButton = null;
		}

		public void OnDeleteClick(FileButton fb)
		{
			if (statusField != null)
			{
				statusField.text = string.Empty;
			}
			deleteFileButton = fb;
			OpenDeleteDialog();
			if (deleteField != null)
			{
				string text = fb.text;
				if (text.EndsWith(".json"))
				{
					text = text.Replace(".json", string.Empty);
				}
				else if (text.EndsWith(".vac"))
				{
					text = text.Replace(".vac", string.Empty);
				}
				else if (text.EndsWith(".vap"))
				{
					text = text.Replace(".vap", string.Empty);
				}
				deleteField.text = text;
			}
		}

		protected void OpenDeleteDialog()
		{
			if (deleteContainer != null)
			{
				deleteContainer.gameObject.SetActive(true);
			}
		}

		public void OnDeleteConfirm()
		{
			if (deleteFileButton != null)
			{
				string fullPath = deleteFileButton.fullPath;
				if (deleteFileButton.isDir)
				{
					if (FileManager.DirectoryExists(fullPath))
					{
						try
						{
							FileManager.AssertNotCalledFromPlugin();
							FileManager.DeleteDirectory(fullPath, true);
						}
						catch (Exception ex)
						{
							Debug.LogError("Could not delete directory " + fullPath + " Exception: " + ex.Message);
							if (statusField != null)
							{
								statusField.text = ex.Message;
							}
							OnDeleteCancel();
							return;
						}
					}
				}
				else
				{
					if (FileManager.FileExists(fullPath))
					{
						try
						{
							FileManager.AssertNotCalledFromPlugin();
							FileManager.DeleteFile(fullPath);
						}
						catch (Exception ex2)
						{
							Debug.LogError("Could not delete file " + fullPath + " Exception: " + ex2.Message);
							if (statusField != null)
							{
								statusField.text = ex2.Message;
							}
							OnDeleteCancel();
							return;
						}
					}
					string text = string.Empty;
					if (fullPath.EndsWith(".json"))
					{
						text = ".json";
					}
					else if (fullPath.EndsWith(".vac"))
					{
						text = ".vac";
					}
					else if (fullPath.EndsWith(".vap"))
					{
						text = ".vap";
					}
					if (text != string.Empty)
					{
						string text2 = fullPath.Replace(text, ".jpg");
						if (FileManager.FileExists(text2))
						{
							try
							{
								FileManager.DeleteFile(text2);
							}
							catch (Exception ex3)
							{
								Debug.LogError("Could not delete file " + text2 + " Exception: " + ex3.Message);
								if (statusField != null)
								{
									statusField.text = ex3.Message;
								}
							}
						}
					}
					string text3 = fullPath + ".fav";
					if (FileManager.FileExists(text3))
					{
						try
						{
							FileManager.DeleteFile(text3);
						}
						catch (Exception ex4)
						{
							Debug.LogError("Could not delete file " + text3 + " Exception: " + ex4.Message);
							if (statusField != null)
							{
								statusField.text = ex4.Message;
							}
						}
					}
				}
				UpdateFileList();
				UpdateDirectoryList();
			}
			OnDeleteCancel();
		}

		public void OnDeleteCancel()
		{
			if (deleteContainer != null)
			{
				deleteContainer.gameObject.SetActive(false);
			}
			deleteFileButton = null;
		}

		protected string DeterminePathToGoTo(string pathToGoTo)
		{
			DirectoryEntry directoryEntry = FileManager.GetDirectoryEntry(pathToGoTo);
			if (!selectDirectory && directoryEntry != null && directoryEntry is VarDirectoryEntry)
			{
				DirectoryEntry directoryEntry2 = directoryEntry.FindFirstDirectoryWithFiles();
				string uid = directoryEntry.Uid;
				string uid2 = directoryEntry2.Uid;
				if (uid2 != uid)
				{
					string text = uid2.Replace(uid, string.Empty);
					text = text.Replace('/', '\\');
					pathToGoTo += text;
				}
			}
			return pathToGoTo;
		}

		public void OnFileClick(FileButton fb)
		{
			if (fb.isDir)
			{
				if (!selectDirectory)
				{
					string text = fb.fullPath;
					VarDirectoryEntry varDirectoryEntry = FileManager.GetVarDirectoryEntry(text);
					if (currentPackageFilter != null && varDirectoryEntry != null && varDirectoryEntry.Package.RootDirectory == varDirectoryEntry)
					{
						text = text + "\\" + currentPackageFilter;
					}
					GotoDirectory(DeterminePathToGoTo(text), currentPackageFilter);
				}
				else
				{
					SelectFile(fb);
				}
			}
			else
			{
				SelectFile(fb);
			}
		}

		public void OnFilePointerEnter(FileButton fb)
		{
			if (fileHighlightField != null)
			{
				fileHighlightField.text = fb.fullPath;
			}
		}

		public void OnFilePointerExit(FileButton fb)
		{
			if (fileHighlightField != null)
			{
				fileHighlightField.text = string.Empty;
			}
		}

		public void OnDirectoryClick(DirectoryButton db)
		{
			GotoDirectory(db.fullPath, db.packageFilter);
		}

		public void OnShortCutClick(int i)
		{
			if (i >= shortCutButtons.Count)
			{
				Debug.LogError("uFileBrowser: Button index is bigger than array, something went wrong.");
			}
			else
			{
				GotoDirectory(DeterminePathToGoTo(shortCutButtons[i].fullPath), shortCutButtons[i].packageFilter, shortCutButtons[i].flatten, shortCutButtons[i].includeRegularDirsInFlatten);
			}
		}

		private IEnumerator DelaySetScroll(float scrollPos)
		{
			yield return null;
			filesScrollRect.verticalNormalizedPosition = scrollPos;
		}

		private void SaveDirectoryScrollPos(string path)
		{
			if (filesScrollRect != null)
			{
				if (directoryScrollPositions == null)
				{
					directoryScrollPositions = new Dictionary<string, float>();
				}
				string text = currentPath;
				if (!text.EndsWith("\\"))
				{
					text += "\\";
				}
				string key = fileFormat + ":" + text;
				float value;
				if (directoryScrollPositions.TryGetValue(key, out value))
				{
					directoryScrollPositions.Remove(key);
				}
				float verticalNormalizedPosition = filesScrollRect.verticalNormalizedPosition;
				directoryScrollPositions.Add(key, verticalNormalizedPosition);
			}
		}

		private void ClearDirectoryScrollPos()
		{
			if (currentPath != null && fileFormat != null && directoryScrollPositions != null)
			{
				string text = currentPath;
				if (!text.EndsWith("\\"))
				{
					text += "\\";
				}
				string key = fileFormat + ":" + text;
				float value;
				if (directoryScrollPositions.TryGetValue(key, out value))
				{
					directoryScrollPositions.Remove(key);
				}
			}
		}

		private void OpenInExplorer()
		{
			SuperController.singleton.OpenFolderInExplorer(currentPath);
		}

		private void GoToPromotionalLink()
		{
			if (promotionalButtonText != null)
			{
				SuperController.singleton.OpenLinkInBrowser(promotionalButtonText.text);
			}
		}

		private void OpenPackageInManager()
		{
			if (currentPackageUid != null && currentPackageUid != string.Empty)
			{
				SuperController.singleton.OpenPackageInManager(currentPackageUid);
			}
		}

		private void OpenOnHub()
		{
			if (currentPackageUid != null && currentPackageUid != string.Empty)
			{
				FileManager.GetPackage(currentPackageUid)?.OpenOnHub();
			}
		}

		public void GotoDirectory(string path, string pkgFilter = null, bool flatten = false, bool includeRegularDirs = false)
		{
			if (gotoDirectoryHandler != null)
			{
				gotoDirectoryHandler(path, pkgFilter, flatten, includeRegularDirs);
				return;
			}
			if (path == currentPath && path != string.Empty && pkgFilter == currentPackageFilter && useFlatten == flatten && includeRegularDirsInFlatten == includeRegularDirs)
			{
				SyncDisplayed();
				return;
			}
			currentPackageFilter = pkgFilter;
			useFlatten = flatten;
			includeRegularDirsInFlatten = includeRegularDirs;
			SaveDirectoryScrollPos(currentPath);
			if (string.IsNullOrEmpty(path))
			{
				currentPath = string.Empty;
			}
			else if (!FileManager.DirectoryExists(path) && !flatten)
			{
				Debug.LogError("uFileBrowser: Directory doesn't exist:\n" + path);
				currentPath = string.Empty;
			}
			else
			{
				currentPath = path;
			}
			if ((bool)currentPathField)
			{
				currentPathField.text = currentPath;
			}
			if (selectDirectory && fileEntryField != null)
			{
				fileEntryField.text = string.Empty;
			}
			selected = null;
			DirectoryEntry directoryEntry = FileManager.GetDirectoryEntry(path);
			if (openInExplorerButton != null)
			{
				if (directoryEntry is VarDirectoryEntry)
				{
					openInExplorerButton.gameObject.SetActive(false);
				}
				else
				{
					openInExplorerButton.gameObject.SetActive(true);
				}
			}
			if (openPackageButton != null && promotionalButton != null && promotionalButtonText != null)
			{
				if (directoryEntry is VarDirectoryEntry)
				{
					VarDirectoryEntry varDirectoryEntry = directoryEntry as VarDirectoryEntry;
					VarPackage package = varDirectoryEntry.Package;
					currentPackageUid = package.Uid;
					openPackageButton.gameObject.SetActive(true);
					if (openOnHubButton != null)
					{
						openOnHubButton.gameObject.SetActive(package.IsOnHub);
					}
					if (!SuperController.singleton.promotionalDisabled && package.PromotionalLink != null && package.PromotionalLink != string.Empty)
					{
						promotionalButton.gameObject.SetActive(true);
						promotionalButtonText.text = package.PromotionalLink;
					}
					else
					{
						promotionalButton.gameObject.SetActive(false);
					}
				}
				else
				{
					currentPackageUid = null;
					if (openOnHubButton != null)
					{
						openOnHubButton.gameObject.SetActive(false);
					}
					openPackageButton.gameObject.SetActive(false);
					promotionalButton.gameObject.SetActive(false);
				}
			}
			UpdateFileList();
			UpdateDirectoryList();
			if (filesScrollRect != null)
			{
				float value = 1f;
				if (directoryScrollPositions != null)
				{
					string text = currentPath;
					if (!text.EndsWith("\\"))
					{
						text += "\\";
					}
					string key = fileFormat + ":" + text;
					if (!directoryScrollPositions.TryGetValue(key, out value))
					{
						value = 1f;
					}
				}
				StartCoroutine(DelaySetScroll(value));
			}
			if (gotoDirectoryCallback != null)
			{
				gotoDirectoryCallback(currentPath);
			}
		}

		private void SelectFile(FileButton fb)
		{
			if (fb == selected && selectDirectory && fb.isDir)
			{
				GotoDirectory(fb.fullPath, currentPackageFilter);
			}
			else
			{
				if (!fb.isDir && selectDirectory)
				{
					return;
				}
				if (selected != null)
				{
					selected.Unselect();
				}
				selected = fb;
				fb.Select();
				if (fileEntryField != null)
				{
					fileEntryField.text = selected.text;
					if (fileEntryField.text.EndsWith(".json"))
					{
						fileEntryField.text = fileEntryField.text.Replace(".json", string.Empty);
					}
					else if (fileEntryField.text.EndsWith(".vac"))
					{
						fileEntryField.text = fileEntryField.text.Replace(".vac", string.Empty);
					}
				}
				if (selectOnClick)
				{
					SelectButtonClicked();
				}
			}
		}

		public void PathFieldEndEdit()
		{
			if (currentPathField != null)
			{
				if (FileManager.DirectoryExists(currentPathField.text))
				{
					GotoDirectory(currentPathField.text);
				}
				else
				{
					currentPathField.text = currentPath;
				}
			}
		}

		public void SearchChanged()
		{
			if ((bool)searchField && !ignoreSearchChange)
			{
				search = searchField.text.Trim();
				searchLower = search.ToLowerInvariant();
				ResetDisplayedPage();
			}
		}

		public void SearchCancelClick()
		{
			searchField.text = string.Empty;
		}

		protected void ClearSearch()
		{
			search = string.Empty;
			searchLower = string.Empty;
			ignoreSearchChange = true;
			searchField.text = string.Empty;
			ignoreSearchChange = false;
		}

		public void SetTextEntry(bool b)
		{
			if (b)
			{
				selectOnClick = false;
				if (selectButton != null)
				{
					selectButton.gameObject.SetActive(true);
				}
				if (fileEntryField != null)
				{
					fileEntryField.gameObject.SetActive(true);
				}
				if (keepOpenToggle != null)
				{
					keepOpenToggle.gameObject.SetActive(false);
				}
			}
			else
			{
				selectOnClick = true;
				if (selectButton != null)
				{
					selectButton.gameObject.SetActive(false);
				}
				if (fileEntryField != null)
				{
					fileEntryField.gameObject.SetActive(false);
				}
				if (keepOpenToggle != null)
				{
					keepOpenToggle.gameObject.SetActive(true);
				}
			}
		}

		private IEnumerator ActivateFileNameFieldProcess()
		{
			yield return null;
			LookInputModule.SelectGameObject(fileEntryField.gameObject);
			fileEntryField.ActivateInputField();
		}

		public void ActivateFileNameField()
		{
			if (fileEntryField != null)
			{
				StartCoroutine(ActivateFileNameFieldProcess());
			}
		}

		public void SelectButtonClicked()
		{
			if (!selectOnClick && fileEntryField != null)
			{
				if (fileEntryField.text != string.Empty)
				{
					string path = currentPath + slash + fileEntryField.text;
					if (!_keepOpen)
					{
						Hide();
					}
					if (callback != null)
					{
						callback(path);
					}
					if (fullCallback != null)
					{
						fullCallback(path, !_keepOpen);
					}
				}
				else if (selectDirectory)
				{
					string path2 = currentPath;
					Hide();
					if (callback != null)
					{
						callback(path2);
					}
					if (fullCallback != null)
					{
						fullCallback(path2, true);
					}
				}
			}
			else if (selected != null && ((selected.isDir && selectDirectory) || (!selected.isDir && !selectDirectory)))
			{
				string fullPath = selected.fullPath;
				if (!_keepOpen)
				{
					Hide();
				}
				if (callback != null)
				{
					callback(fullPath);
				}
				if (fullCallback != null)
				{
					fullCallback(fullPath, !_keepOpen);
				}
			}
		}

		public void CancelButtonClicked()
		{
			if (canCancel)
			{
				Hide();
				if (callback != null)
				{
					callback(string.Empty);
				}
				if (fullCallback != null)
				{
					fullCallback(string.Empty, true);
				}
			}
		}

		private void SyncSort()
		{
			if (cachedFiles != null && cachedDirs != null)
			{
				if (lastCacheUseFlatten)
				{
					sortedFilesAndDirs = cachedFiles;
					SortFilesAndDirs(sortedFilesAndDirs);
				}
				else
				{
					switch (directoryOption)
					{
					case UserPreferences.DirectoryOption.Hide:
						sortedFilesAndDirs = cachedFiles;
						SortFilesAndDirs(sortedFilesAndDirs);
						break;
					case UserPreferences.DirectoryOption.Intermix:
						sortedFilesAndDirs = new List<FileAndDirInfo>();
						sortedFilesAndDirs.AddRange(cachedFiles);
						sortedFilesAndDirs.AddRange(cachedDirs);
						SortFilesAndDirs(sortedFilesAndDirs);
						break;
					case UserPreferences.DirectoryOption.ShowFirst:
						sortedFilesAndDirs = new List<FileAndDirInfo>();
						SortFilesAndDirs(cachedDirs);
						sortedFilesAndDirs.AddRange(cachedDirs);
						SortFilesAndDirs(cachedFiles);
						sortedFilesAndDirs.AddRange(cachedFiles);
						break;
					case UserPreferences.DirectoryOption.ShowLast:
						sortedFilesAndDirs = new List<FileAndDirInfo>();
						SortFilesAndDirs(cachedFiles);
						sortedFilesAndDirs.AddRange(cachedFiles);
						SortFilesAndDirs(cachedDirs);
						sortedFilesAndDirs.AddRange(cachedDirs);
						break;
					}
				}
			}
			ResetDisplayedPage();
		}

		protected void HideButton(FileButton fb)
		{
			if (manageContentTransform && displayedFileButtons.Contains(fb))
			{
				fb.gameObject.SetActive(false);
				displayedFileButtons.Remove(fb);
			}
		}

		private void SyncDisplayed()
		{
			if (sortedFilesAndDirs == null)
			{
				return;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			Vector2 vector = cellSize + cellSpacing;
			if (!manageContentTransform)
			{
				foreach (FileButton displayedFileButton in displayedFileButtons)
				{
					displayedFileButton.gameObject.SetActive(false);
					displayedFileButton.transform.SetParent(null, false);
				}
				displayedFileButtons.Clear();
			}
			int num5 = (_page - 1) * _limitXMultiple + 1;
			int num6 = _page * _limitXMultiple;
			Vector2 anchoredPosition = default(Vector2);
			foreach (FileAndDirInfo sortedFilesAndDir in sortedFilesAndDirs)
			{
				FileEntry fileEntry = sortedFilesAndDir.FileEntry;
				DirectoryEntry directoryEntry = sortedFilesAndDir.DirectoryEntry;
				FileButton button = sortedFilesAndDir.button;
				if (!(button != null))
				{
					continue;
				}
				if (fileEntry != null)
				{
					if (!_showHidden && fileEntry.IsHidden())
					{
						HideButton(button);
						continue;
					}
					if (_onlyFavorites && !sortedFilesAndDir.isFavorite)
					{
						HideButton(button);
						continue;
					}
					if (_onlyTemplates && !sortedFilesAndDir.isTemplate)
					{
						HideButton(button);
						continue;
					}
					if (!string.IsNullOrEmpty(searchLower) && !fileEntry.UidLowerInvariant.Contains(searchLower))
					{
						VarFileEntry varFileEntry = fileEntry as VarFileEntry;
						if (varFileEntry == null)
						{
							HideButton(button);
							continue;
						}
						if (!varFileEntry.Package.UidLowerInvariant.Contains(searchLower))
						{
							HideButton(button);
							continue;
						}
					}
				}
				else if (directoryEntry != null)
				{
					if (!_showHidden && directoryEntry.IsHidden())
					{
						HideButton(button);
						continue;
					}
					if (!string.IsNullOrEmpty(searchLower) && !directoryEntry.UidLowerInvariant.Contains(searchLower))
					{
						VarDirectoryEntry varDirectoryEntry = directoryEntry as VarDirectoryEntry;
						if (varDirectoryEntry == null)
						{
							HideButton(button);
							continue;
						}
						if (!varDirectoryEntry.Package.UidLowerInvariant.Contains(searchLower))
						{
							HideButton(button);
							continue;
						}
					}
				}
				num++;
				if (num < num5 || num > num6)
				{
					HideButton(button);
					continue;
				}
				if (manageContentTransform)
				{
					button.gameObject.SetActive(true);
					RectTransform rectTransform = button.rectTransform;
					if (rectTransform != null)
					{
						anchoredPosition.x = (float)num4 * vector.x;
						anchoredPosition.y = (float)(-num3) * vector.y;
						rectTransform.anchoredPosition = anchoredPosition;
						num4++;
						if (num4 == columnCount)
						{
							num4 = 0;
							num3++;
						}
						num2++;
					}
				}
				else
				{
					button.gameObject.SetActive(true);
					button.transform.SetParent(fileContent, false);
					num2++;
				}
				displayedFileButtons.Add(button);
				SyncFileButtonImage(button);
			}
			if (manageContentTransform)
			{
				float y = (float)(num3 + 1) * vector.y;
				Vector2 sizeDelta = fileContent.sizeDelta;
				sizeDelta.y = y;
				fileContent.sizeDelta = sizeDelta;
			}
			if (showingCountText != null)
			{
				if (num6 > num)
				{
					num6 = num;
				}
				showingCountText.text = num5 + "-" + num6 + " of " + num;
			}
			_totalPages = (num - 1) / _limitXMultiple + 1;
		}

		protected List<FileAndDirInfo> FilterFormat(List<FileAndDirInfo> files, bool skipFileFormatCheck = false)
		{
			List<FileAndDirInfo> list = files;
			if (fileRemovePrefix != null && fileRemovePrefix != string.Empty)
			{
				List<FileAndDirInfo> list2 = new List<FileAndDirInfo>();
				for (int i = 0; i < list.Count; i++)
				{
					if (Regex.IsMatch(list[i].Name, "^" + fileRemovePrefix))
					{
						list2.Add(list[i]);
					}
				}
				list = list2;
			}
			if (!string.IsNullOrEmpty(fileFormat) && !skipFileFormatCheck)
			{
				List<FileAndDirInfo> list3 = new List<FileAndDirInfo>();
				string[] array = fileFormat.Split('|');
				for (int j = 0; j < list.Count; j++)
				{
					string text = string.Empty;
					if (list[j].Name.Contains("."))
					{
						text = list[j].Name.Substring(list[j].Name.LastIndexOf('.') + 1).ToLowerInvariant();
					}
					for (int k = 0; k < array.Length; k++)
					{
						if (text == array[k].Trim().ToLowerInvariant())
						{
							list3.Add(list[j]);
						}
					}
				}
				list = list3;
			}
			return list;
		}

		protected void UpdateFileListCacheThreadSafe()
		{
			List<FileAndDirInfo> list = new List<FileAndDirInfo>();
			List<FileAndDirInfo> list2 = new List<FileAndDirInfo>();
			if (string.IsNullOrEmpty(currentPath))
			{
				try
				{
					for (int i = 0; i < drives.Count; i++)
					{
						DirectoryEntry directoryEntry = FileManager.GetDirectoryEntry(drives[i]);
						FileAndDirInfo item = new FileAndDirInfo(directoryEntry, string.Empty);
						list2.Add(item);
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("uFileBrowser: " + ex);
					threadHadException = true;
					threadException = ex.Message;
				}
			}
			else if (useFlatten)
			{
				List<FileEntry> list3 = new List<FileEntry>();
				try
				{
					if (fileFormat != null)
					{
						string regex = "\\.(" + fileFormat + ")$";
						if (includeRegularDirsInFlatten)
						{
							FileManager.FindAllFilesRegex(currentPath, regex, list3);
						}
						else
						{
							FileManager.FindVarFilesRegex(currentPath, regex, list3);
						}
					}
					else if (includeRegularDirsInFlatten)
					{
						FileManager.FindAllFiles(currentPath, "*", list3);
					}
					else
					{
						FileManager.FindVarFiles(currentPath, "*", list3);
					}
					foreach (FileEntry item7 in list3)
					{
						if (item7 is VarFileEntry)
						{
							VarFileEntry varFileEntry = item7 as VarFileEntry;
							if (!varFileEntry.Package.isNewestEnabledVersion)
							{
								continue;
							}
						}
						if (item7.Exists || FileManager.IsPackage(item7.Path))
						{
							if (!forceOnlyShowTemplates || item7.HasFlagFile("template"))
							{
								FileAndDirInfo item2 = new FileAndDirInfo(item7);
								list.Add(item2);
							}
						}
						else
						{
							Debug.LogError("Unable to read file " + item7.FullPath);
							threadHadException = true;
							threadException = "Unable to read file " + item7.FullPath;
						}
					}
				}
				catch (Exception ex2)
				{
					Debug.LogError("uFileBrowser: " + ex2);
					threadHadException = true;
					threadException = ex2.Message;
				}
				list = FilterFormat(list, true);
			}
			else
			{
				DirectoryEntry directoryEntry2 = FileManager.GetDirectoryEntry(currentPath);
				if ((selectDirectory && showFiles) || !selectDirectory)
				{
					try
					{
						List<FileEntry> files = directoryEntry2.Files;
						foreach (FileEntry item8 in files)
						{
							if (item8.Exists || FileManager.IsPackage(item8.Path))
							{
								if (!forceOnlyShowTemplates || item8.HasFlagFile("template"))
								{
									FileAndDirInfo item3 = new FileAndDirInfo(item8);
									list.Add(item3);
								}
							}
							else
							{
								Debug.LogError("Unable to read file " + item8.FullPath);
								threadHadException = true;
								threadException = "Unable to read file " + item8.FullPath;
							}
						}
					}
					catch (Exception ex3)
					{
						Debug.LogError("uFileBrowser: " + ex3);
						threadHadException = true;
						threadException = ex3.Message;
					}
					list = FilterFormat(list);
				}
				if (showDirs)
				{
					try
					{
						string empty = currentPath;
						if (empty == ".")
						{
							empty = string.Empty;
						}
						List<DirectoryEntry> subDirectories = directoryEntry2.SubDirectories;
						foreach (DirectoryEntry item9 in subDirectories)
						{
							if (item9 is VarDirectoryEntry)
							{
								if (!browseVarFilesAsDirectories)
								{
									continue;
								}
								VarDirectoryEntry varDirectoryEntry = item9 as VarDirectoryEntry;
								if (currentPackageFilter != null && varDirectoryEntry.Package.RootDirectory == varDirectoryEntry)
								{
									if (varDirectoryEntry.Package.HasMatchingDirectories(currentPackageFilter))
									{
										FileAndDirInfo item4 = new FileAndDirInfo(item9, empty);
										list2.Add(item4);
									}
								}
								else
								{
									FileAndDirInfo item5 = new FileAndDirInfo(item9, empty);
									list2.Add(item5);
								}
							}
							else
							{
								FileAndDirInfo item6 = new FileAndDirInfo(item9, empty);
								list2.Add(item6);
							}
						}
					}
					catch (Exception ex4)
					{
						Debug.LogError("uFileBrowser: " + ex4.Message);
						threadHadException = true;
						threadException = ex4.Message;
					}
				}
			}
			cachedFiles = list;
			cachedDirs = list2;
			lastCacheFileFormat = fileFormat;
			lastCacheFileRemovePrefix = fileRemovePrefix;
			lastCacheUseFlatten = useFlatten;
			lastCacheIncludeRegularDirsInFlatten = includeRegularDirsInFlatten;
			lastCachePackageFilter = currentPackageFilter;
			lastCacheTime = DateTime.Now;
			lastCacheShowDirs = showDirs;
			lastCacheForceOnlyShowTemplates = forceOnlyShowTemplates;
			lastCacheDir = currentPath;
			cacheDirty = false;
		}

		private void UpdateFileList()
		{
			if (!cacheDirty)
			{
				if (currentPath != lastCacheDir)
				{
					cacheDirty = true;
				}
				else if (useFlatten != lastCacheUseFlatten)
				{
					cacheDirty = true;
				}
				else if (fileFormat != lastCacheFileFormat)
				{
					cacheDirty = true;
				}
				else if (fileRemovePrefix != lastCacheFileRemovePrefix)
				{
					cacheDirty = true;
				}
				else if (FileManager.lastPackageRefreshTime > lastCacheTime)
				{
					cacheDirty = true;
				}
				else if (lastCachePackageFilter != currentPackageFilter)
				{
					cacheDirty = true;
				}
				else if (useFlatten && FileManager.CheckIfDirectoryChanged(currentPath, lastCacheTime))
				{
					cacheDirty = true;
				}
				else if (useFlatten && includeRegularDirsInFlatten != lastCacheIncludeRegularDirsInFlatten)
				{
					cacheDirty = true;
				}
				else if (!useFlatten && FileManager.CheckIfDirectoryChanged(currentPath, lastCacheTime, false))
				{
					cacheDirty = true;
				}
				else if (!useFlatten && showDirs != lastCacheShowDirs)
				{
					cacheDirty = true;
				}
				else if (forceOnlyShowTemplates != lastCacheForceOnlyShowTemplates)
				{
					cacheDirty = true;
				}
			}
			if (cacheDirty)
			{
				int num = 0;
				ClearSearch();
				if (cachedFiles != null)
				{
					num += cachedFiles.Count;
					foreach (FileAndDirInfo cachedFile in cachedFiles)
					{
						if (cachedFile.button != null)
						{
							UnityEngine.Object.Destroy(cachedFile.button.gameObject);
						}
					}
				}
				if (cachedDirs != null)
				{
					num += cachedDirs.Count;
					foreach (FileAndDirInfo cachedDir in cachedDirs)
					{
						if (cachedDir.button != null)
						{
							UnityEngine.Object.Destroy(cachedDir.button.gameObject);
						}
					}
				}
				displayedFileButtons.Clear();
				threadHadException = false;
				UpdateFileListCacheThreadSafe();
				if (threadHadException && statusField != null)
				{
					statusField.text = threadException;
				}
				if (dirOption != null)
				{
					dirOption.gameObject.SetActive(showDirs);
				}
				ClearImageQueue();
				int num2 = 0;
				foreach (FileAndDirInfo cachedFile2 in cachedFiles)
				{
					cachedFile2.button = CreateFileButton(cachedFile2.Name, cachedFile2.FullName, false, cachedFile2.isWriteable, cachedFile2.isHidden, cachedFile2.isHiddenModifiable, cachedFile2.isFavorite, cachedFile2.isTemplate, cachedFile2.isTemplateModifiable);
					cachedFile2.button.gameObject.SetActive(false);
					if (manageContentTransform)
					{
						cachedFile2.button.transform.SetParent(fileContent, false);
					}
					num2++;
				}
				foreach (FileAndDirInfo cachedDir2 in cachedDirs)
				{
					cachedDir2.button = CreateFileButton(cachedDir2.Name, cachedDir2.FullName, true, cachedDir2.isWriteable, cachedDir2.isHidden, cachedDir2.isHiddenModifiable, false, false, false);
					cachedDir2.button.gameObject.SetActive(false);
					if (manageContentTransform)
					{
						cachedDir2.button.transform.SetParent(fileContent, false);
					}
					num2++;
				}
			}
			SyncSort();
		}

		private void UpdateDirectoryList()
		{
			if (!directoryButtonPrefab)
			{
				return;
			}
			if (dirButtons == null)
			{
				dirButtons = new List<DirectoryButton>();
			}
			else
			{
				for (int i = 0; i < dirButtons.Count; i++)
				{
					UnityEngine.Object.Destroy(dirButtons[i].gameObject);
				}
				dirButtons.Clear();
			}
			if (shortCutButtons == null)
			{
				shortCutButtons = new List<ShortCutButton>();
			}
			else
			{
				for (int j = 0; j < shortCutButtons.Count; j++)
				{
					UnityEngine.Object.Destroy(shortCutButtons[j].gameObject);
				}
				shortCutButtons.Clear();
			}
			if (dirSpacers == null)
			{
				dirSpacers = new List<GameObject>();
			}
			else
			{
				for (int k = 0; k < dirSpacers.Count; k++)
				{
					UnityEngine.Object.Destroy(dirSpacers[k]);
				}
				dirSpacers.Clear();
			}
			if (shortCutsOverflowIndicator != null)
			{
				shortCutsOverflowIndicator.SetActive(false);
			}
			if (shortCuts != null)
			{
				foreach (ShortCut shortCut in shortCuts)
				{
					if (shortCut.package != null && shortCut.package != string.Empty)
					{
						if (!string.IsNullOrEmpty(_shortCutsSearchLower) && !shortCut.package.ToLowerInvariant().Contains(_shortCutsSearchLower))
						{
							continue;
						}
						if (shortCut.creator != null && shortCut.creator != string.Empty)
						{
							AddShortCutsCreator(shortCut.creator);
							if (_shortCutsCreatorFilter != "All" && _shortCutsCreatorFilter != shortCut.creator)
							{
								continue;
							}
						}
					}
					if ((!_showHidden && shortCut.isHidden) || (_onlyShowLatest && !shortCut.isLatest))
					{
						continue;
					}
					CreateShortCutButton(shortCut, shortCutButtons.Count);
					if (shortCutButtons.Count >= 100)
					{
						if (shortCutsOverflowIndicator != null)
						{
							shortCutsOverflowIndicator.SetActive(true);
						}
						break;
					}
				}
			}
			FinalizeShortCutsCreators();
			if (useFlatten || string.IsNullOrEmpty(currentPath))
			{
				return;
			}
			if (Regex.IsMatch(currentPath, "^[A-Za-z]:\\\\"))
			{
				CreateDirectoryButton("My Computer", string.Empty, string.Empty, dirButtons.Count);
				CreateDirectorySpacer();
			}
			else if (showInstallFolderInDirectoryList && !FileManager.IsDirectoryInPackage(currentPath))
			{
				CreateDirectoryButton("Root", string.Empty, ".", dirButtons.Count);
			}
			string[] array = currentPath.Split(slash[0]);
			string text = string.Empty;
			for (int l = 0; l < array.Length; l++)
			{
				if (!string.IsNullOrEmpty(array[l]) && array[l] != ".")
				{
					text = ((!(text == string.Empty)) ? (text + slash + array[l]) : array[l]);
					string package = string.Empty;
					string empty = string.Empty;
					string packageFolder = FileManager.PackageFolder;
					if (array[l].Contains(packageFolder))
					{
						empty = Regex.Replace(array[l], ".*:/", string.Empty);
						package = Regex.Replace(array[l], ":/.*", string.Empty);
						package = package.Replace(packageFolder, string.Empty);
						package = package.TrimStart('/', '\\');
					}
					else
					{
						empty = array[l] + slash;
					}
					CreateDirectoryButton(package, empty, text, dirButtons.Count);
				}
			}
		}

		private void Awake()
		{
			slash = Path.DirectorySeparatorChar.ToString();
			drives = new List<string>(Directory.GetLogicalDrives());
			displayedFileButtons = new HashSet<FileButton>();
			queuedThumbnails = new HashSet<ImageLoaderThreaded.QueuedImage>();
			if (renameFieldAction != null)
			{
				InputFieldAction inputFieldAction = renameFieldAction;
				inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(OnRenameConfirm));
			}
			if (onlyShowLatestToggle != null)
			{
				onlyShowLatestToggle.isOn = _onlyShowLatest;
			}
			if (shortCutsCreatorFilterPopup != null)
			{
				shortCutsCreatorFilterPopup.currentValueNoCallback = _shortCutsCreatorFilter;
				UIPopup uIPopup = shortCutsCreatorFilterPopup;
				uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetShortCutsCreatorFilter));
			}
			if (shortCutsSearchField != null)
			{
				shortCutsSearchField.text = _shortCutsSearch;
				shortCutsSearchField.onValueChanged.AddListener(ShortCutsSearchChanged);
			}
			if (shortCutsSearchClearButton != null)
			{
				shortCutsSearchClearButton.onClick.AddListener(ClearShortCutsSearchField);
			}
			if (showHiddenToggle != null)
			{
				showHiddenToggle.isOn = _showHidden;
				showHiddenToggle.onValueChanged.AddListener(SetShowHidden);
			}
			if (onlyFavoritesToggle != null)
			{
				onlyFavoritesToggle.isOn = _onlyFavorites;
				onlyFavoritesToggle.onValueChanged.AddListener(SetOnlyFavorites);
			}
			if (onlyTemplatesToggle != null)
			{
				onlyTemplatesToggle.isOn = _onlyTemplates;
				onlyTemplatesToggle.onValueChanged.AddListener(SetOnlyTemplates);
			}
			if (limitSlider != null)
			{
				limitSlider.value = _limit;
				limitSlider.onValueChanged.AddListener(SetLimit);
			}
			_limitXMultiple = _limit * limitMultiple;
			if (limitValueText != null)
			{
				limitValueText.text = _limitXMultiple.ToString("F0");
			}
			if (openInExplorerButton != null)
			{
				openInExplorerButton.onClick.AddListener(OpenInExplorer);
			}
			if (openPackageButton != null)
			{
				openPackageButton.onClick.AddListener(OpenPackageInManager);
			}
			if (openOnHubButton != null)
			{
				openOnHubButton.onClick.AddListener(OpenOnHub);
			}
			if (promotionalButton != null)
			{
				promotionalButton.onClick.AddListener(GoToPromotionalLink);
			}
			if (sortByPopup != null)
			{
				sortByPopup.currentValueNoCallback = _sortBy.ToString();
				UIPopup uIPopup2 = sortByPopup;
				uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetSortBy));
			}
			if (directoryOptionPopup != null)
			{
				directoryOptionPopup.currentValueNoCallback = _directoryOption.ToString();
				UIPopup uIPopup3 = directoryOptionPopup;
				uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetDirectoryOption));
			}
		}
	}
}
