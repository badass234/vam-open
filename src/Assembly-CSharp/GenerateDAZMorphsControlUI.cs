using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateDAZMorphsControlUI : GenerateTabbedUI
{
	public enum ShowType
	{
		MorphAndPose,
		Morph,
		Pose
	}

	public enum SecondaryShowType
	{
		All,
		BuiltIn,
		CustomAll,
		CustomPackage,
		CustomLocal,
		Transient,
		Formula
	}

	public Toggle onlyShowFavoritesToggle;

	protected bool _onlyShowFavorites;

	public Toggle onlyShowActiveToggle;

	protected bool _onlyShowActive;

	public Toggle onlyShowLatestToggle;

	protected bool _onlyShowLatest = true;

	public InputField filterField;

	protected string _lowerCaseFilter;

	protected string _filter;

	public UIPopup showTypePopup;

	protected ShowType _showType;

	public UIPopup secondaryShowTypePopup;

	protected SecondaryShowType _secondaryShowType;

	protected List<DAZMorph> morphs;

	protected Dictionary<string, DAZMorph> morphDisplayNameToMorph;

	protected Dictionary<string, DAZMorph> morphUidToMorph;

	protected List<DAZMorph> filteredMorphs;

	protected Dictionary<string, List<DAZMorph>> categoryToMorphs;

	protected string[] morphCategories;

	protected List<DAZMorph> currentDisplayedMorphs;

	public UIPopup categoryPopup;

	public bool isAltUI;

	protected string currentCategory;

	[SerializeField]
	protected DAZMorphBank _morphBank1;

	[SerializeField]
	protected DAZMorphBank _morphBank2;

	[SerializeField]
	protected DAZMorphBank _morphBank3;

	public UIPopup ZeroPopup;

	public UIPopup DefaultPopup;

	protected bool _forceCategoryRefresh;

	public bool onlyShowFavorites
	{
		get
		{
			return _onlyShowFavorites;
		}
		set
		{
			if (_onlyShowFavorites != value)
			{
				_onlyShowFavorites = value;
				if (onlyShowFavoritesToggle != null)
				{
					onlyShowFavoritesToggle.isOn = _onlyShowFavorites;
				}
				if (currentCategory != null)
				{
					SetCategoryFromString(currentCategory);
				}
			}
		}
	}

	public bool onlyShowActive
	{
		get
		{
			return _onlyShowActive;
		}
		set
		{
			if (_onlyShowActive != value)
			{
				_onlyShowActive = value;
				if (onlyShowActiveToggle != null)
				{
					onlyShowActiveToggle.isOn = _onlyShowActive;
				}
				if (currentCategory != null)
				{
					SetCategoryFromString(currentCategory);
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
				if (currentCategory != null)
				{
					SetCategoryFromString(currentCategory);
				}
			}
		}
	}

	public string filter
	{
		get
		{
			return _filter;
		}
		set
		{
			if (_filter != value)
			{
				_filter = value;
				if (filterField != null)
				{
					filterField.text = _filter;
				}
				_lowerCaseFilter = _filter.ToLower();
				if (currentCategory != null)
				{
					SetCategoryFromString(currentCategory);
				}
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
				if (showTypePopup != null)
				{
					showTypePopup.currentValueNoCallback = _showType.ToString();
				}
				if (currentCategory != null)
				{
					SetCategoryFromString(currentCategory);
				}
			}
		}
	}

	public SecondaryShowType secondaryShowType
	{
		get
		{
			return _secondaryShowType;
		}
		set
		{
			if (_secondaryShowType != value)
			{
				_secondaryShowType = value;
				if (secondaryShowTypePopup != null)
				{
					secondaryShowTypePopup.currentValueNoCallback = _secondaryShowType.ToString();
				}
				if (currentCategory != null)
				{
					SetCategoryFromString(currentCategory);
				}
			}
		}
	}

	public DAZMorphBank morphBank1
	{
		get
		{
			return _morphBank1;
		}
		set
		{
			if (_morphBank1 != value)
			{
				_morphBank1 = value;
				ResyncMorphs();
				ResyncUI();
			}
		}
	}

	public DAZMorphBank morphBank2
	{
		get
		{
			return _morphBank2;
		}
		set
		{
			if (_morphBank2 != value)
			{
				_morphBank2 = value;
				ResyncMorphs();
				ResyncUI();
			}
		}
	}

	public DAZMorphBank morphBank3
	{
		get
		{
			return _morphBank3;
		}
		set
		{
			if (_morphBank3 != value)
			{
				_morphBank3 = value;
				ResyncMorphs();
				ResyncUI();
			}
		}
	}

	public override void TabChange(string name, bool on)
	{
		UnregisterCurrentDisplayedMorphsUI();
		currentDisplayedMorphs = new List<DAZMorph>();
		base.TabChange(name, on);
	}

	protected override Transform InstantiateControl(Transform parent, int index)
	{
		Transform transform = base.InstantiateControl(parent, index);
		DAZMorph dAZMorph = filteredMorphs[index];
		currentDisplayedMorphs.Add(dAZMorph);
		if (isAltUI)
		{
			dAZMorph.InitUIAlt(transform);
		}
		else
		{
			dAZMorph.InitUI(transform);
		}
		return transform;
	}

	protected void SetOnlyShowFavorites(bool b)
	{
		onlyShowFavorites = b;
	}

	protected void SetOnlyShowActive(bool b)
	{
		onlyShowActive = b;
	}

	protected void SetOnlyShowLatest(bool b)
	{
		onlyShowLatest = b;
	}

	public void SetFilter(string f)
	{
		filter = f;
	}

	protected void SetShowType(string t)
	{
		try
		{
			ShowType showType = (ShowType)Enum.Parse(typeof(ShowType), t);
			this.showType = showType;
		}
		catch (ArgumentException)
		{
			Debug.LogError("Tried to set show type to " + t + " which is not a valid show type");
		}
	}

	protected void SetSecondaryShowType(string t)
	{
		try
		{
			SecondaryShowType secondaryShowType = (SecondaryShowType)Enum.Parse(typeof(SecondaryShowType), t);
			this.secondaryShowType = secondaryShowType;
		}
		catch (ArgumentException)
		{
			Debug.LogError("Tried to set secondary show type to " + t + " which is not a valid secondary show type");
		}
	}

	public List<DAZMorph> GetMorphs()
	{
		return morphs;
	}

	public void SetCategoryFromString(string val)
	{
		_forceCategoryRefresh = false;
		currentCategory = val;
		GenerateStart();
		if (controlUIPrefab != null && tabUIPrefab != null && tabButtonUIPrefab != null)
		{
			List<DAZMorph> value = null;
			filteredMorphs = new List<DAZMorph>();
			if (currentCategory == "All")
			{
				value = morphs;
			}
			else
			{
				categoryToMorphs.TryGetValue(currentCategory, out value);
			}
			if (value != null)
			{
				foreach (DAZMorph item in value)
				{
					switch (_secondaryShowType)
					{
					case SecondaryShowType.BuiltIn:
						if (item.isRuntime)
						{
							continue;
						}
						break;
					case SecondaryShowType.CustomAll:
						if (!item.isRuntime || item.isTransient)
						{
							continue;
						}
						break;
					case SecondaryShowType.CustomPackage:
						if (!item.isRuntime || item.isTransient || !item.isInPackage)
						{
							continue;
						}
						break;
					case SecondaryShowType.CustomLocal:
						if (!item.isRuntime || item.isTransient || item.isInPackage)
						{
							continue;
						}
						break;
					case SecondaryShowType.Transient:
						if (!item.isTransient)
						{
							continue;
						}
						break;
					case SecondaryShowType.Formula:
						if (!item.hasMorphValueFormulas && !item.hasMCMFormulas)
						{
							continue;
						}
						break;
					}
					if ((_onlyShowFavorites && !item.favorite) || (_onlyShowActive && !item.active) || (_onlyShowLatest && !item.isLatestVersion && !item.active) || (_lowerCaseFilter != null && !item.lowerCaseResolvedDisplayName.Contains(_lowerCaseFilter)))
					{
						continue;
					}
					switch (_showType)
					{
					case ShowType.Morph:
						if (item.isPoseControl)
						{
							continue;
						}
						break;
					case ShowType.Pose:
						if (!item.isPoseControl)
						{
							continue;
						}
						break;
					}
					filteredMorphs.Add(item);
				}
				for (int i = 0; i < filteredMorphs.Count; i++)
				{
					AllocateControl();
				}
			}
		}
		GenerateFinish();
	}

	public static int CustomMorphSort(DAZMorph m1, DAZMorph m2)
	{
		return string.Compare(m1.resolvedRegionName + ":" + m1.resolvedDisplayName, m2.resolvedRegionName + ":" + m2.resolvedDisplayName);
	}

	public List<string> GetMorphDisplayNames()
	{
		if (morphDisplayNameToMorph == null)
		{
			ResyncMorphs();
		}
		return new List<string>(morphDisplayNameToMorph.Keys);
	}

	public DAZMorph GetMorphByDisplayName(string morphDisplayName)
	{
		if (morphDisplayNameToMorph == null)
		{
			ResyncMorphs();
		}
		DAZMorph value = null;
		if (morphDisplayNameToMorph != null && !morphDisplayNameToMorph.TryGetValue(morphDisplayName, out value))
		{
			if (_morphBank1 != null)
			{
				value = _morphBank1.GetMorphByDisplayName(morphDisplayName);
			}
			if (value == null && _morphBank2 != null)
			{
				value = _morphBank2.GetMorphByDisplayName(morphDisplayName);
			}
			if (value == null && _morphBank3 != null)
			{
				value = _morphBank3.GetMorphByDisplayName(morphDisplayName);
			}
		}
		return value;
	}

	public bool IsBadMorph(string morphDisplayName)
	{
		if (_morphBank1 != null && _morphBank1.IsBadMorph(morphDisplayName))
		{
			return true;
		}
		if (_morphBank2 != null && _morphBank2.IsBadMorph(morphDisplayName))
		{
			return true;
		}
		if (_morphBank3 != null && _morphBank3.IsBadMorph(morphDisplayName))
		{
			return true;
		}
		return false;
	}

	public List<string> GetMorphUids()
	{
		if (morphUidToMorph == null)
		{
			ResyncMorphs();
		}
		return new List<string>(morphUidToMorph.Keys);
	}

	public DAZMorph GetMorphByUid(string morphUid)
	{
		if (morphUidToMorph == null)
		{
			ResyncMorphs();
		}
		DAZMorph value = null;
		if (morphUidToMorph != null && !morphUidToMorph.TryGetValue(morphUid, out value))
		{
			if (_morphBank1 != null)
			{
				value = _morphBank1.GetMorphByUid(morphUid);
			}
			if (value == null && _morphBank2 != null)
			{
				value = _morphBank2.GetMorphByUid(morphUid);
			}
			if (value == null && _morphBank3 != null)
			{
				value = _morphBank3.GetMorphByUid(morphUid);
			}
		}
		return value;
	}

	public bool CleanDemandActivatedMorphs()
	{
		bool flag = false;
		if (_morphBank1 != null && _morphBank1.CleanDemandActivatedMorphs())
		{
			flag = true;
		}
		if (_morphBank2 != null && _morphBank2.CleanDemandActivatedMorphs())
		{
			flag = true;
		}
		if (_morphBank3 != null && _morphBank3.CleanDemandActivatedMorphs())
		{
			flag = true;
		}
		if (flag)
		{
			ResyncMorphs();
			if (currentCategory != null)
			{
				SetCategoryFromString(currentCategory);
			}
		}
		return flag;
	}

	protected void UnregisterCurrentDisplayedMorphsUI()
	{
		if (currentDisplayedMorphs == null)
		{
			return;
		}
		foreach (DAZMorph currentDisplayedMorph in currentDisplayedMorphs)
		{
			if (isAltUI)
			{
				currentDisplayedMorph.DeregisterUIAlt();
			}
			else
			{
				currentDisplayedMorph.DeregisterUI();
			}
		}
	}

	public void ResyncMorphs()
	{
		List<DAZMorph> list = new List<DAZMorph>();
		if (_morphBank1 != null)
		{
			foreach (DAZMorph morph in _morphBank1.morphs)
			{
				if (morph.visible)
				{
					list.Add(morph);
				}
			}
		}
		if (_morphBank2 != null)
		{
			foreach (DAZMorph morph2 in _morphBank2.morphs)
			{
				if (morph2.visible)
				{
					list.Add(morph2);
				}
			}
		}
		if (_morphBank3 != null)
		{
			foreach (DAZMorph morph3 in _morphBank3.morphs)
			{
				if (morph3.visible)
				{
					list.Add(morph3);
				}
			}
		}
		list.Sort(CustomMorphSort);
		if (morphDisplayNameToMorph == null)
		{
			morphDisplayNameToMorph = new Dictionary<string, DAZMorph>();
		}
		else
		{
			morphDisplayNameToMorph.Clear();
		}
		if (morphUidToMorph == null)
		{
			morphUidToMorph = new Dictionary<string, DAZMorph>();
		}
		else
		{
			morphUidToMorph.Clear();
		}
		if (morphs == null)
		{
			morphs = new List<DAZMorph>();
		}
		else
		{
			morphs.Clear();
		}
		foreach (DAZMorph item in list)
		{
			if (item.resolvedDisplayName != null)
			{
				if (!morphDisplayNameToMorph.ContainsKey(item.resolvedDisplayName))
				{
					morphDisplayNameToMorph.Add(item.resolvedDisplayName, item);
				}
			}
			else
			{
				Debug.LogError("Morph display name is null for morph with morph name " + item.morphName);
			}
			if (!morphUidToMorph.ContainsKey(item.uid))
			{
				morphs.Add(item);
				morphUidToMorph.Add(item.uid, item);
			}
			else
			{
				Debug.LogError("Found duplicate morph uid " + item.uid + "for morph " + item.morphName);
			}
		}
		if (currentDisplayedMorphs == null)
		{
			currentDisplayedMorphs = new List<DAZMorph>();
		}
		else
		{
			currentDisplayedMorphs.Clear();
		}
		if (categoryToMorphs == null)
		{
			categoryToMorphs = new Dictionary<string, List<DAZMorph>>();
		}
		else
		{
			categoryToMorphs.Clear();
		}
		foreach (DAZMorph morph4 in morphs)
		{
			string resolvedRegionName = morph4.resolvedRegionName;
			List<DAZMorph> value;
			if (categoryToMorphs.TryGetValue(resolvedRegionName, out value))
			{
				value.Add(morph4);
				continue;
			}
			value = new List<DAZMorph>();
			value.Add(morph4);
			categoryToMorphs.Add(resolvedRegionName, value);
		}
		morphCategories = new string[categoryToMorphs.Keys.Count + 1];
		morphCategories[0] = "All";
		categoryToMorphs.Keys.CopyTo(morphCategories, 1);
		Array.Sort(morphCategories, 1, morphCategories.Length - 1);
		if (categoryPopup != null)
		{
			categoryPopup.numPopupValues = morphCategories.Length;
			for (int i = 0; i < morphCategories.Length; i++)
			{
				categoryPopup.setPopupValue(i, morphCategories[i]);
			}
		}
	}

	protected IEnumerator DelayedCategoryRefresh()
	{
		yield return null;
		yield return null;
		yield return null;
		if (currentCategory != null && (_onlyShowActive || _forceCategoryRefresh))
		{
			SetCategoryFromString(currentCategory);
		}
	}

	public void DoZeroCommand(string command)
	{
		if (ZeroPopup != null)
		{
			ZeroPopup.currentValueNoCallback = "Zero...";
		}
		switch (command)
		{
		case "ZeroAll":
			ZeroAll();
			break;
		case "ZeroMorph":
			ZeroMorph();
			break;
		case "ZeroPose":
			ZeroPose();
			break;
		case "ZeroShown":
			ZeroShown();
			break;
		case "ZeroNearZero":
			ZeroNearZero();
			break;
		case "ZeroNearZeroMore":
			ZeroNearZeroMore();
			break;
		}
	}

	public void ZeroAll()
	{
		foreach (DAZMorph morph in morphs)
		{
			morph.morphValue = 0f;
		}
		StartCoroutine(DelayedCategoryRefresh());
	}

	public void ZeroMorph()
	{
		foreach (DAZMorph morph in morphs)
		{
			if (!morph.isPoseControl)
			{
				morph.morphValue = 0f;
			}
		}
		StartCoroutine(DelayedCategoryRefresh());
	}

	public void ZeroPose()
	{
		foreach (DAZMorph morph in morphs)
		{
			if (morph.isPoseControl)
			{
				morph.morphValue = 0f;
			}
		}
		StartCoroutine(DelayedCategoryRefresh());
	}

	public void ZeroShown()
	{
		foreach (DAZMorph filteredMorph in filteredMorphs)
		{
			filteredMorph.morphValue = 0f;
		}
		StartCoroutine(DelayedCategoryRefresh());
	}

	public void ZeroNearZero()
	{
		foreach (DAZMorph morph in morphs)
		{
			if (Mathf.Abs(morph.morphValue) <= 0.01f)
			{
				morph.morphValue = 0f;
			}
		}
		StartCoroutine(DelayedCategoryRefresh());
	}

	public void ZeroNearZeroMore()
	{
		foreach (DAZMorph morph in morphs)
		{
			if (Mathf.Abs(morph.morphValue) <= 0.05f)
			{
				morph.morphValue = 0f;
			}
		}
		StartCoroutine(DelayedCategoryRefresh());
	}

	public void DoDefaultCommand(string command)
	{
		if (DefaultPopup != null)
		{
			DefaultPopup.currentValueNoCallback = "Default...";
		}
		switch (command)
		{
		case "DefaultAll":
			DefaultAll();
			break;
		case "DefaultMorph":
			DefaultMorph();
			break;
		case "DefaultPose":
			DefaultPose();
			break;
		case "DefaultShown":
			DefaultShown();
			break;
		}
	}

	public void DefaultAll()
	{
		foreach (DAZMorph morph in morphs)
		{
			morph.morphValue = morph.startValue;
		}
		StartCoroutine(DelayedCategoryRefresh());
	}

	public void DefaultMorph()
	{
		foreach (DAZMorph morph in morphs)
		{
			if (!morph.isPoseControl)
			{
				morph.morphValue = morph.startValue;
			}
		}
		StartCoroutine(DelayedCategoryRefresh());
	}

	public void DefaultPose()
	{
		foreach (DAZMorph morph in morphs)
		{
			if (morph.isPoseControl)
			{
				morph.morphValue = morph.startValue;
			}
		}
		StartCoroutine(DelayedCategoryRefresh());
	}

	public void DefaultShown()
	{
		foreach (DAZMorph filteredMorph in filteredMorphs)
		{
			filteredMorph.morphValue = filteredMorph.startValue;
		}
		StartCoroutine(DelayedCategoryRefresh());
	}

	public void ForceCategoryRefresh()
	{
		_forceCategoryRefresh = true;
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(DelayedCategoryRefresh());
		}
	}

	public void ResyncUI()
	{
		TabChange(currentTab.ToString(), true);
	}

	protected override void Generate()
	{
		base.Generate();
		ResyncMorphs();
		if (categoryPopup != null)
		{
			categoryPopup.numPopupValues = morphCategories.Length;
			for (int i = 0; i < morphCategories.Length; i++)
			{
				categoryPopup.setPopupValue(i, morphCategories[i]);
			}
			UIPopup uIPopup = categoryPopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetCategoryFromString));
			categoryPopup.currentValue = morphCategories[0];
		}
		else if (controlUIPrefab != null && tabUIPrefab != null && tabButtonUIPrefab != null)
		{
			filteredMorphs = new List<DAZMorph>(morphs);
			for (int j = 0; j < filteredMorphs.Count; j++)
			{
				AllocateControl();
			}
		}
	}

	private void Awake()
	{
		if (onlyShowActiveToggle != null)
		{
			onlyShowActiveToggle.onValueChanged.AddListener(SetOnlyShowActive);
		}
		if (onlyShowLatestToggle != null)
		{
			onlyShowLatestToggle.isOn = _onlyShowLatest;
			onlyShowLatestToggle.onValueChanged.AddListener(SetOnlyShowLatest);
		}
		if (onlyShowFavoritesToggle != null)
		{
			onlyShowFavoritesToggle.onValueChanged.AddListener(SetOnlyShowFavorites);
		}
		if (filterField != null)
		{
			filterField.onValueChanged.AddListener(SetFilter);
		}
		if (showTypePopup != null)
		{
			showTypePopup.currentValue = _showType.ToString();
			UIPopup uIPopup = showTypePopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetShowType));
		}
		if (secondaryShowTypePopup != null)
		{
			secondaryShowTypePopup.currentValue = _secondaryShowType.ToString();
			UIPopup uIPopup2 = secondaryShowTypePopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetSecondaryShowType));
		}
		if (ZeroPopup != null)
		{
			UIPopup zeroPopup = ZeroPopup;
			zeroPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(zeroPopup.onValueChangeHandlers, new UIPopup.OnValueChange(DoZeroCommand));
		}
		if (DefaultPopup != null)
		{
			UIPopup defaultPopup = DefaultPopup;
			defaultPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(defaultPopup.onValueChangeHandlers, new UIPopup.OnValueChange(DoDefaultCommand));
		}
	}

	private void OnEnable()
	{
		StartCoroutine(DelayedCategoryRefresh());
	}
}
