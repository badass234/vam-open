using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class GenerateDAZDynamicSelectorUI : GenerateTabbedUI
{
	public enum SortBy
	{
		AtoZ,
		ZtoA,
		NewToOld,
		OldToNew,
		Creator
	}

	public enum ShowType
	{
		All,
		Active,
		BuiltInAll,
		BuiltInSim,
		BuiltInWrap,
		CustomAll,
		CustomPackage,
		CustomLocal,
		Locked,
		MissingTags,
		Hidden,
		NotLatest,
		Real,
		NotReal
	}

	public DAZCharacterSelector characterSelector;

	public RectTransform userPrefsPanel;

	public Text userPrefsLabel;

	public RectTransform userPrefsRegionTagsContent;

	public RectTransform userPrefsTypeTagsContent;

	public RectTransform userPrefsOtherTagsContent;

	public Button closeUserPrefsPanelButton;

	public Toggle userPrefsHideToggle;

	public Text userPrefsBuiltInTagsText;

	public Toggle userPrefsReplaceTagsToggle;

	public InputField userPrefsTagsInputField;

	public Button resyncUIButton;

	protected DAZDynamicItem currentUserPrefItem;

	protected Color packageColor = new Color(1f, 0.85f, 0.9f);

	public UIPopup sortByPopup;

	protected SortBy _sortBy = SortBy.NewToOld;

	public UIPopup showTypePopup;

	protected ShowType _showType;

	public InputField nameFilterField;

	protected string _nameFilter;

	public Toggle latestOnlyToggle;

	protected bool _latestOnly = true;

	public Toggle showLegacyToggle;

	protected bool _showLegacy;

	public UIPopup creatorNameFilterPopup;

	protected string _creatorNameFilter = "All";

	public RectTransform tagsPanel;

	public Transform tagTogglePrefab;

	public RectTransform regionTagsContent;

	public RectTransform typeTagsContent;

	public RectTransform otherTagsContent;

	public Toggle singleTagFilterToggle;

	protected bool _singleTagFilter;

	public string[] regionTags;

	public string[] typeTags;

	protected HashSet<string> allTags = new HashSet<string>();

	protected HashSet<string> otherTags = new HashSet<string>();

	protected HashSet<string> tagsFilterSet = new HashSet<string>();

	protected bool skipFieldSetCallback;

	public InputField tagsFilterField;

	protected string _tagsFilter;

	protected Dictionary<string, Toggle> tagToToggle = new Dictionary<string, Toggle>();

	protected List<DAZDynamicItem> dynamicItems;

	protected Dictionary<DAZDynamicItem, Toggle> dynamicItemToToggle;

	protected List<DAZDynamicItem> filteredDynamicItems;

	protected List<string> creatorChoices;

	public SortBy sortBy
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
				ResyncUI();
			}
		}
	}

	public ShowType showType
	{
		get
		{
			return _showType;
		}
		set
		{
			if (_showType != value)
			{
				_showType = value;
				ResyncUI();
			}
		}
	}

	public string nameFilter
	{
		get
		{
			return _nameFilter;
		}
		set
		{
			if (_nameFilter != value)
			{
				_nameFilter = value;
				ResyncUI();
			}
		}
	}

	public bool latestOnly
	{
		get
		{
			return _latestOnly;
		}
		set
		{
			if (_latestOnly != value)
			{
				_latestOnly = value;
				ResyncUI();
			}
		}
	}

	public bool showLegacy
	{
		get
		{
			return _showLegacy;
		}
		set
		{
			if (_showLegacy != value)
			{
				_showLegacy = value;
				ResyncUI();
			}
		}
	}

	public string creatorNameFilter
	{
		get
		{
			return _creatorNameFilter;
		}
		set
		{
			if (_creatorNameFilter != value)
			{
				_creatorNameFilter = value;
				ResyncUI();
			}
		}
	}

	protected bool singleTagFilter
	{
		get
		{
			return _singleTagFilter;
		}
		set
		{
			if (_singleTagFilter != value)
			{
				_singleTagFilter = value;
				if (singleTagFilterToggle != null)
				{
					singleTagFilterToggle.isOn = _singleTagFilter;
				}
				if (_singleTagFilter && tagsFilterSet.Count > 1)
				{
					List<string> list = new List<string>(tagsFilterSet);
					SetTagsFilter(list[0]);
				}
			}
		}
	}

	protected string tagsFilter
	{
		get
		{
			return _tagsFilter;
		}
		set
		{
			if (_tagsFilter != value)
			{
				_tagsFilter = value;
				if (tagsFilterField != null)
				{
					skipFieldSetCallback = true;
					tagsFilterField.text = _tagsFilter;
					skipFieldSetCallback = false;
				}
				ResyncUI();
			}
		}
	}

	public override void TabChange(string name, bool on)
	{
		dynamicItemToToggle = new Dictionary<DAZDynamicItem, Toggle>();
		base.TabChange(name, on);
	}

	protected void CloseUserPrefsPanel()
	{
		if (currentUserPrefItem != null)
		{
			currentUserPrefItem.DeactivateUserPrefs();
		}
		currentUserPrefItem = null;
		if (userPrefsPanel != null)
		{
			userPrefsPanel.gameObject.SetActive(false);
		}
	}

	protected void UserPrefSync(DAZDynamicItem dynamicItem)
	{
		if (currentUserPrefItem != null)
		{
			if (currentUserPrefItem == dynamicItem)
			{
				if (userPrefsPanel.gameObject.activeSelf)
				{
					CloseUserPrefsPanel();
					return;
				}
				userPrefsPanel.gameObject.SetActive(true);
				currentUserPrefItem.ActivateUserPrefs();
			}
			else
			{
				userPrefsPanel.gameObject.SetActive(true);
				currentUserPrefItem.DeactivateUserPrefs();
				currentUserPrefItem = dynamicItem;
				currentUserPrefItem.ActivateUserPrefs();
			}
		}
		else
		{
			userPrefsPanel.gameObject.SetActive(true);
			currentUserPrefItem = dynamicItem;
			currentUserPrefItem.ActivateUserPrefs();
		}
	}

	protected override Transform InstantiateControl(Transform parent, int index)
	{
		Transform transform = base.InstantiateControl(parent, index);
		DAZDynamicItem dynamicItem = filteredDynamicItems[index];
		Toggle component = transform.GetComponent<Toggle>();
		if (dynamicItem != null && component != null)
		{
			dynamicItemToToggle.Add(dynamicItem, component);
			component.onValueChanged.AddListener(delegate(bool arg0)
			{
				if (characterSelector != null)
				{
					characterSelector.SetActiveDynamicItem(dynamicItem, arg0);
				}
			});
			component.isOn = dynamicItem.active;
			bool isInPackage = dynamicItem.IsInPackage;
			DAZDynamicItemUI ddiui = transform.GetComponent<DAZDynamicItemUI>();
			if (ddiui != null)
			{
				if (isInPackage)
				{
					if (ddiui.backgroundImage != null)
					{
						ddiui.backgroundImage.color = packageColor;
					}
					if (ddiui.openInPackageButton != null)
					{
						ddiui.openInPackageButton.gameObject.SetActive(true);
						ddiui.openInPackageButton.onClick.AddListener(delegate
						{
							SuperController.singleton.OpenPackageInManager(dynamicItem.uid);
						});
					}
					if (ddiui.packageVersionText != null)
					{
						ddiui.packageVersionText.gameObject.SetActive(true);
						ddiui.packageVersionText.text = dynamicItem.version;
					}
					if (ddiui.packageUidText != null)
					{
						ddiui.packageUidText.text = dynamicItem.packageUid;
					}
					if (ddiui.packageLicenseText != null)
					{
						ddiui.packageLicenseText.text = dynamicItem.packageLicense;
					}
				}
				else
				{
					if (ddiui.openInPackageButton != null)
					{
						ddiui.openInPackageButton.gameObject.SetActive(false);
					}
					if (ddiui.packageVersionText != null)
					{
						ddiui.packageVersionText.gameObject.SetActive(false);
					}
				}
				if (ddiui.displayNameText != null)
				{
					ddiui.displayNameText.text = dynamicItem.displayName;
				}
				if (ddiui.creatorNameText != null)
				{
					ddiui.creatorNameText.text = dynamicItem.creatorName;
				}
				if (ddiui.customizationButton != null)
				{
					if (dynamicItem.hasCustomizationUI)
					{
						ddiui.customizationButton.gameObject.SetActive(true);
						ddiui.customizationButton.onClick.AddListener(delegate
						{
							dynamicItem.OpenUI();
						});
					}
					else
					{
						ddiui.customizationButton.gameObject.SetActive(false);
					}
				}
				if (ddiui.rawImage != null)
				{
					dynamicItem.GetThumbnail(delegate(Texture2D tex)
					{
						if (ddiui.rawImage != null)
						{
							ddiui.rawImage.texture = tex;
						}
					});
				}
				if (ddiui.toggleUserPrefsPanelButton != null)
				{
					if (dynamicItem.hasUserPrefs)
					{
						ddiui.toggleUserPrefsPanelButton.gameObject.SetActive(true);
						ddiui.toggleUserPrefsPanelButton.onClick.AddListener(delegate
						{
							UserPrefSync(dynamicItem);
						});
					}
					else
					{
						ddiui.toggleUserPrefsPanelButton.gameObject.SetActive(false);
					}
				}
				if (ddiui.hiddenIndicator != null)
				{
					ddiui.hiddenIndicator.gameObject.SetActive(dynamicItem.isHidden);
				}
				dynamicItem.dynamicSelectorUI = this;
			}
			else
			{
				Debug.LogError("Could not find DAZDynamicUI component");
			}
		}
		return transform;
	}

	public void SetDynamicItemToggle(DAZDynamicItem dynamicItem, bool on)
	{
		Toggle value;
		if (dynamicItemToToggle != null && dynamicItemToToggle.TryGetValue(dynamicItem, out value))
		{
			value.isOn = on;
		}
	}

	public void RefreshThumbnails()
	{
		if (dynamicItemToToggle == null)
		{
			return;
		}
		foreach (DAZDynamicItem key in dynamicItemToToggle.Keys)
		{
			Toggle value;
			if (!dynamicItemToToggle.TryGetValue(key, out value))
			{
				continue;
			}
			DAZDynamicItemUI ddiui = value.GetComponent<DAZDynamicItemUI>();
			if (ddiui != null && ddiui.rawImage != null)
			{
				key.GetThumbnail(delegate(Texture2D tex)
				{
					ddiui.rawImage.texture = tex;
				});
			}
		}
	}

	public void SetSortBy(string sortByString)
	{
		try
		{
			SortBy sortBy = (SortBy)Enum.Parse(typeof(SortBy), sortByString);
			this.sortBy = sortBy;
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set sort by to " + sortByString + " which is not a valid type");
		}
	}

	public void SetShowType(string showTypeString)
	{
		try
		{
			ShowType showType = (ShowType)Enum.Parse(typeof(ShowType), showTypeString);
			this.showType = showType;
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set show type to " + showTypeString + " which is not a valid type");
		}
	}

	public void SetNameFilter(string f)
	{
		nameFilter = f;
	}

	public void SetLatestOnly(bool b)
	{
		latestOnly = b;
	}

	public void SetShowLegacy(bool b)
	{
		showLegacy = b;
	}

	public void SetCreatorNameFilter(string f)
	{
		creatorNameFilter = f;
	}

	protected void SetSingleTagFilter(bool b)
	{
		singleTagFilter = b;
	}

	public void OpenTagsPanel()
	{
		CloseUserPrefsPanel();
		if (tagsPanel != null)
		{
			tagsPanel.gameObject.SetActive(true);
		}
	}

	public void CloseTagsPanel()
	{
		if (tagsPanel != null)
		{
			tagsPanel.gameObject.SetActive(false);
		}
	}

	public HashSet<string> GetOtherTags()
	{
		return otherTags;
	}

	protected void SyncFilterSetToFilter()
	{
		if (_tagsFilter != null && _tagsFilter != string.Empty)
		{
			string[] collection = _tagsFilter.Split(',');
			tagsFilterSet = new HashSet<string>(collection);
		}
		else
		{
			tagsFilterSet = new HashSet<string>();
		}
	}

	protected void SyncFilterToFilterSet()
	{
		string[] array = new string[tagsFilterSet.Count];
		tagsFilterSet.CopyTo(array);
		tagsFilter = string.Join(",", array);
	}

	protected void SyncFilterTagFromToggle(string tag, bool isEnabled)
	{
		if (isEnabled)
		{
			if (_singleTagFilter)
			{
				tagsFilterSet.Clear();
				tagsFilterSet.Add(tag);
			}
			else
			{
				tagsFilterSet.Add(tag);
			}
		}
		else
		{
			tagsFilterSet.Remove(tag);
		}
		SyncFilterToFilterSet();
		if (isEnabled && _singleTagFilter)
		{
			SyncFilterTogglesToFilter();
		}
	}

	protected void SyncFilterTogglesToFilter()
	{
		foreach (KeyValuePair<string, Toggle> item in tagToToggle)
		{
			if (tagsFilterSet.Contains(item.Key))
			{
				if (item.Value != null)
				{
					item.Value.isOn = true;
				}
			}
			else if (item.Value != null)
			{
				item.Value.isOn = false;
			}
		}
	}

	public void SetTagsFilter(string f)
	{
		if (!skipFieldSetCallback)
		{
			string input = f.Trim();
			input = Regex.Replace(input, ",\\s+", ",");
			input = Regex.Replace(input, "\\s+,", ",");
			input = input.ToLower();
			if (input != f && tagsFilterField != null)
			{
				skipFieldSetCallback = true;
				tagsFilterField.text = input;
				skipFieldSetCallback = false;
			}
			tagsFilter = input;
			SyncFilterSetToFilter();
			SyncFilterTogglesToFilter();
		}
	}

	protected void CreateTagToggle(string tag, Transform parent)
	{
		Transform transform = UnityEngine.Object.Instantiate(tagTogglePrefab);
		Text componentInChildren = transform.GetComponentInChildren<Text>();
		componentInChildren.text = tag;
		ToggleGroup component = parent.GetComponent<ToggleGroup>();
		Toggle componentInChildren2 = transform.GetComponentInChildren<Toggle>();
		if (component != null)
		{
			componentInChildren2.group = component;
		}
		componentInChildren2.onValueChanged.AddListener(delegate(bool b)
		{
			SyncFilterTagFromToggle(tag, b);
		});
		tagToToggle.Remove(tag);
		tagToToggle.Add(tag, componentInChildren2);
		transform.SetParent(parent, false);
	}

	protected void SyncOtherTagsUI()
	{
		if (!(tagTogglePrefab != null) || !(otherTagsContent != null))
		{
			return;
		}
		foreach (Transform item in otherTagsContent)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		List<string> list = otherTags.ToList();
		list.Sort();
		foreach (string item2 in list)
		{
			CreateTagToggle(item2, otherTagsContent);
		}
	}

	protected void InitTagsUI()
	{
		tagToToggle = new Dictionary<string, Toggle>();
		if (tagTogglePrefab != null)
		{
			if (regionTags != null && regionTagsContent != null)
			{
				List<string> list = new List<string>();
				for (int i = 0; i < regionTags.Length; i++)
				{
					list.Add(regionTags[i].ToLower());
				}
				list.Sort();
				foreach (string item in list)
				{
					CreateTagToggle(item, regionTagsContent);
				}
			}
			if (typeTags != null && typeTagsContent != null)
			{
				List<string> list2 = new List<string>();
				for (int j = 0; j < typeTags.Length; j++)
				{
					list2.Add(typeTags[j].ToLower());
				}
				list2.Sort();
				foreach (string item2 in list2)
				{
					CreateTagToggle(item2, typeTagsContent);
				}
			}
		}
		SyncOtherTagsUI();
	}

	protected virtual DAZDynamicItem[] GetDynamicItems()
	{
		return new DAZDynamicItem[0];
	}

	public void ResyncTags()
	{
		if (allTags == null)
		{
			allTags = new HashSet<string>();
		}
		else
		{
			allTags.Clear();
		}
		string[] array = regionTags;
		foreach (string item in array)
		{
			allTags.Add(item);
		}
		string[] array2 = typeTags;
		foreach (string item2 in array2)
		{
			allTags.Add(item2);
		}
		if (otherTags == null)
		{
			otherTags = new HashSet<string>();
		}
		else
		{
			otherTags.Clear();
		}
		if (dynamicItems == null)
		{
			dynamicItems = new List<DAZDynamicItem>();
		}
		else
		{
			dynamicItems.Clear();
		}
		if (characterSelector != null)
		{
			DAZDynamicItem[] array3 = GetDynamicItems();
			foreach (DAZDynamicItem dAZDynamicItem in array3)
			{
				dAZDynamicItem.Init();
				string[] tagsArray = dAZDynamicItem.tagsArray;
				foreach (string item3 in tagsArray)
				{
					if (!allTags.Contains(item3))
					{
						allTags.Add(item3);
						otherTags.Add(item3);
					}
				}
				dynamicItems.Add(dAZDynamicItem);
			}
		}
		SyncOtherTagsUI();
	}

	public void ResyncItems()
	{
		ResyncTags();
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		foreach (DAZDynamicItem dynamicItem in dynamicItems)
		{
			string text = dynamicItem.creatorName;
			if (text == null || text == string.Empty)
			{
				text = "None";
			}
			if (!dictionary.ContainsKey(text))
			{
				dictionary.Add(text, true);
			}
		}
		creatorChoices = dictionary.Keys.ToList();
		creatorChoices.Sort();
		creatorChoices.Insert(0, "All");
		if (creatorNameFilterPopup != null)
		{
			creatorNameFilterPopup.numPopupValues = creatorChoices.Count;
			for (int i = 0; i < creatorChoices.Count; i++)
			{
				creatorNameFilterPopup.setPopupValue(i, creatorChoices[i]);
			}
		}
	}

	protected bool CaseInsensitiveStringContains(string s1, string s2)
	{
		return s1.IndexOf(s2, StringComparison.CurrentCultureIgnoreCase) >= 0;
	}

	protected void ResyncUIInternal()
	{
		dynamicItemToToggle = new Dictionary<DAZDynamicItem, Toggle>();
		if (!(controlUIPrefab != null) || !(tabUIPrefab != null) || !(tabButtonUIPrefab != null) || dynamicItems == null)
		{
			return;
		}
		string[] array = null;
		if (_tagsFilter != null && _tagsFilter != string.Empty)
		{
			array = _tagsFilter.Split(',');
		}
		List<DAZDynamicItem> list = new List<DAZDynamicItem>();
		filteredDynamicItems = new List<DAZDynamicItem>();
		foreach (DAZDynamicItem dynamicItem in dynamicItems)
		{
			if (dynamicItem.showFirstInUI)
			{
				list.Add(dynamicItem);
				continue;
			}
			switch (_showType)
			{
			case ShowType.Active:
				if (!dynamicItem.active)
				{
					continue;
				}
				break;
			case ShowType.BuiltInAll:
				if (dynamicItem.type == DAZDynamicItem.Type.Custom)
				{
					continue;
				}
				break;
			case ShowType.BuiltInSim:
				if (dynamicItem.type != DAZDynamicItem.Type.Sim)
				{
					continue;
				}
				break;
			case ShowType.BuiltInWrap:
				if (dynamicItem.type != 0)
				{
					continue;
				}
				break;
			case ShowType.CustomAll:
				if (dynamicItem.type != DAZDynamicItem.Type.Custom)
				{
					continue;
				}
				break;
			case ShowType.CustomPackage:
				if (dynamicItem.type != DAZDynamicItem.Type.Custom || !dynamicItem.IsInPackage)
				{
					continue;
				}
				break;
			case ShowType.CustomLocal:
				if (dynamicItem.type != DAZDynamicItem.Type.Custom || dynamicItem.IsInPackage)
				{
					continue;
				}
				break;
			case ShowType.Locked:
				if (!dynamicItem.locked)
				{
					continue;
				}
				break;
			case ShowType.MissingTags:
				if (dynamicItem.tagsArray != null && dynamicItem.tagsArray.Length != 0)
				{
					continue;
				}
				break;
			case ShowType.Hidden:
				if (!dynamicItem.isHidden)
				{
					continue;
				}
				break;
			case ShowType.NotLatest:
				if (dynamicItem.isLatestVersion)
				{
					continue;
				}
				break;
			case ShowType.Real:
				if (!dynamicItem.isRealItem)
				{
					continue;
				}
				break;
			case ShowType.NotReal:
				if (dynamicItem.isRealItem)
				{
					continue;
				}
				break;
			}
			if ((_showType != ShowType.Hidden && dynamicItem.isHidden && !dynamicItem.active) || (_showType != ShowType.NotLatest && _latestOnly && !dynamicItem.active && !dynamicItem.isLatestVersion) || (dynamicItem.isLegacy && !dynamicItem.active && !_showLegacy) || (_nameFilter != null && _nameFilter != string.Empty && !CaseInsensitiveStringContains(dynamicItem.displayName, _nameFilter)) || (_creatorNameFilter != null && _creatorNameFilter != string.Empty && _creatorNameFilter != "All" && dynamicItem.creatorName != _creatorNameFilter))
			{
				continue;
			}
			if (array != null)
			{
				bool flag = true;
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (text != string.Empty && !dynamicItem.CheckMatchTag(text))
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			filteredDynamicItems.Add(dynamicItem);
		}
		switch (sortBy)
		{
		case SortBy.AtoZ:
			filteredDynamicItems.Sort((DAZDynamicItem i1, DAZDynamicItem i2) => i1.displayName.CompareTo(i2.displayName));
			break;
		case SortBy.ZtoA:
			filteredDynamicItems.Sort((DAZDynamicItem i1, DAZDynamicItem i2) => i2.displayName.CompareTo(i1.displayName));
			break;
		case SortBy.NewToOld:
			filteredDynamicItems.Reverse();
			break;
		case SortBy.Creator:
			filteredDynamicItems.Sort((DAZDynamicItem i1, DAZDynamicItem i2) => (i1.creatorName == i2.creatorName) ? i1.displayName.CompareTo(i2.displayName) : i1.creatorName.CompareTo(i2.creatorName));
			break;
		}
		filteredDynamicItems.InsertRange(0, list);
		for (int k = 0; k < filteredDynamicItems.Count; k++)
		{
			AllocateControl();
		}
	}

	public void ResyncUIIfActiveFilterOn()
	{
		if (_showType == ShowType.Active)
		{
			ResyncUI();
		}
	}

	protected void GotoTabWithItem(DAZDynamicItem di)
	{
		for (int i = 0; i < filteredDynamicItems.Count; i++)
		{
			if (filteredDynamicItems[i] == di)
			{
				int tabNum = i / numElementsPerTab + 1;
				GotoTab(tabNum);
				break;
			}
		}
	}

	public void ResyncUI()
	{
		GenerateStart();
		ResyncUIInternal();
		GenerateFinish();
		if (currentUserPrefItem != null)
		{
			GotoTabWithItem(currentUserPrefItem);
		}
	}

	public void Resync()
	{
		ResyncItems();
		ResyncUI();
	}

	protected override void Generate()
	{
		base.Generate();
		ResyncItems();
		ResyncUIInternal();
	}

	protected void Awake()
	{
		if (sortByPopup != null)
		{
			UIPopup uIPopup = sortByPopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetSortBy));
		}
		if (showTypePopup != null)
		{
			UIPopup uIPopup2 = showTypePopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetShowType));
		}
		if (latestOnlyToggle != null)
		{
			latestOnlyToggle.isOn = _latestOnly;
			latestOnlyToggle.onValueChanged.AddListener(SetLatestOnly);
		}
		if (showLegacyToggle != null)
		{
			showLegacyToggle.isOn = _showLegacy;
			showLegacyToggle.onValueChanged.AddListener(SetShowLegacy);
		}
		if (nameFilterField != null)
		{
			nameFilterField.text = _nameFilter;
			nameFilterField.onValueChanged.AddListener(SetNameFilter);
		}
		if (creatorNameFilterPopup != null)
		{
			creatorNameFilterPopup.currentValue = _creatorNameFilter;
			UIPopup uIPopup3 = creatorNameFilterPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetCreatorNameFilter));
		}
		if (tagsFilterField != null)
		{
			tagsFilterField.text = _tagsFilter;
			tagsFilterField.onValueChanged.AddListener(SetTagsFilter);
		}
		if (singleTagFilterToggle != null)
		{
			singleTagFilterToggle.isOn = _singleTagFilter;
			singleTagFilterToggle.onValueChanged.AddListener(SetSingleTagFilter);
		}
		if (closeUserPrefsPanelButton != null)
		{
			closeUserPrefsPanelButton.onClick.AddListener(CloseUserPrefsPanel);
		}
		if (resyncUIButton != null)
		{
			resyncUIButton.onClick.AddListener(ResyncUI);
		}
		InitTagsUI();
	}
}
