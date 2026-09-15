using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using MeshVR;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class DAZDynamicItem : JSONStorableDynamic
{
	public enum Type
	{
		Wrap,
		Sim,
		Custom,
		Ignore
	}

	public delegate void ThumbnailLoadedCallback(Texture2D thumb);

	public enum Gender
	{
		Neutral,
		Female,
		Male
	}

	public enum BoneType
	{
		None,
		Hip,
		Pelvis,
		Chest,
		Head,
		LeftHand,
		RightHand,
		LeftFoot,
		RightFoot
	}

	public Type type;

	public bool isDynamicRuntimeLoaded;

	public string dynamicRuntimeLoadPath = string.Empty;

	[SerializeField]
	protected string _uid;

	public string internalUid;

	public string backupId;

	public string creatorName = "None";

	public string displayName;

	public string version;

	public string packageUid;

	public string packageLicense;

	public bool isRealItem = true;

	protected HashSet<string> _builtInTagsSet;

	protected HashSet<string> _tagsHash;

	protected string[] _builtInTagsArray;

	protected string[] _tagsArray;

	[SerializeField]
	protected string _tags = string.Empty;

	protected bool _replaceTags;

	protected JSONStorableBool replaceTagsJSON;

	protected HashSet<string> _userTagsSet = new HashSet<string>();

	protected string _userTags = string.Empty;

	protected JSONStorableString userTagsJSON;

	protected string hidePath;

	protected JSONStorableBool hideJSON;

	protected string userPrefsPath;

	protected bool isLoadingUserPrefs;

	public GenerateDAZDynamicSelectorUI dynamicSelectorUI;

	protected bool userPrefsRegisteredClose;

	protected HashSet<string> otherTags = new HashSet<string>();

	private bool ignoreTagFromToggleCallback;

	protected Dictionary<string, Toggle> tagToToggle = new Dictionary<string, Toggle>();

	public Texture2D thumbnail;

	public bool startActive;

	protected bool _active;

	public bool locked;

	public bool isLegacy;

	public bool isLatestVersion = true;

	public bool hasCustomizationUI;

	public Transform UIbucket;

	public Transform customizationUI;

	public bool showFirstInUI;

	public DAZCharacterSelector characterSelector;

	public Gender gender = Gender.Female;

	public bool disableAnatomy;

	public BoneType drawRigidOnBoneType;

	public BoneType drawRigidOnBoneTypeLeft;

	public BoneType drawRigidOnBoneTypeRight;

	public BoneType autoColliderReferenceBoneType;

	public DAZBone drawRigidOnBone;

	public DAZBone drawRigidOnBoneLeft;

	public DAZBone drawRigidOnBoneRight;

	public Transform autoColliderReference;

	[SerializeField]
	protected DAZSkinV2 _skin;

	protected bool _wasInit;

	public string uid
	{
		get
		{
			if (_uid == null || _uid == string.Empty)
			{
				return displayName;
			}
			return _uid;
		}
		set
		{
			if (_uid != value)
			{
				_uid = value;
			}
		}
	}

	public bool IsInPackage
	{
		get
		{
			if (version != null && version != string.Empty)
			{
				return true;
			}
			return false;
		}
	}

	public string[] tagsArray
	{
		get
		{
			return _tagsArray;
		}
		protected set
		{
			_tagsArray = value;
		}
	}

	public string tags
	{
		get
		{
			return _tags;
		}
		set
		{
			string text = value;
			if (text != null)
			{
				text = text.Trim();
				text = Regex.Replace(text, ",\\s+", ",");
				text = Regex.Replace(text, "\\s+,", ",");
				text = text.ToLower();
			}
			if (_tags != text)
			{
				_tags = text;
				SyncTagsLookups();
			}
		}
	}

	public bool isHidden => hidePath != null && FileManager.FileExists(hidePath);

	public bool hasUserPrefs
	{
		get
		{
			if (userPrefsPath != null && userPrefsPath != string.Empty)
			{
				return true;
			}
			return false;
		}
	}

	public bool active
	{
		get
		{
			return _active;
		}
		set
		{
			if (_active != value)
			{
				_active = value;
			}
		}
	}

	public DAZSkinV2 skin
	{
		get
		{
			return _skin;
		}
		set
		{
			if (_skin != value)
			{
				_skin = value;
				Connect();
			}
		}
	}

	protected void InitTagsLookups()
	{
		if (_tagsArray == null || _tagsHash == null)
		{
			SyncTagsLookups();
		}
	}

	protected void SyncTagsLookups()
	{
		if (_tags == null || _tags == string.Empty)
		{
			_builtInTagsArray = new string[0];
		}
		else
		{
			_builtInTagsArray = _tags.Split(',');
			for (int i = 0; i < _builtInTagsArray.Length; i++)
			{
				_builtInTagsArray[i] = _builtInTagsArray[i].ToLower();
			}
		}
		if (_builtInTagsSet == null)
		{
			_builtInTagsSet = new HashSet<string>();
		}
		else
		{
			_builtInTagsSet.Clear();
		}
		_builtInTagsSet.UnionWith(_builtInTagsArray);
		if (_replaceTags)
		{
			if (_userTags == null || _userTags == string.Empty)
			{
				_tagsArray = new string[0];
			}
			else
			{
				_tagsArray = _userTags.Split(',');
				for (int j = 0; j < _tagsArray.Length; j++)
				{
					_tagsArray[j] = _tagsArray[j].ToLower();
				}
			}
		}
		else
		{
			List<string> list = new List<string>();
			if (_tags != null && _tags != string.Empty)
			{
				string[] array = _tags.Split(',');
				for (int k = 0; k < array.Length; k++)
				{
					list.Add(array[k].ToLower());
				}
			}
			if (_userTags != null && _userTags != string.Empty)
			{
				string[] array2 = _userTags.Split(',');
				for (int l = 0; l < array2.Length; l++)
				{
					list.Add(array2[l].ToLower());
				}
			}
			_tagsArray = list.ToArray();
		}
		if (_tagsHash == null)
		{
			_tagsHash = new HashSet<string>();
		}
		else
		{
			_tagsHash.Clear();
		}
		_tagsHash.UnionWith(_tagsArray);
	}

	public bool CheckMatchTag(string tag)
	{
		return _tagsHash.Contains(tag);
	}

	protected void SyncReplaceTags(bool b)
	{
		_replaceTags = b;
		SyncTagsLookups();
		SaveUserPrefs();
	}

	protected void SyncUserTags(string tags)
	{
		string input = tags.Trim();
		input = Regex.Replace(input, ",\\s+", ",");
		input = Regex.Replace(input, "\\s+,", ",");
		input = input.ToLower();
		if (input != _userTags)
		{
			userTagsJSON.valNoCallback = input;
			_userTags = input;
			if (_userTagsSet == null)
			{
				_userTagsSet = new HashSet<string>();
			}
			else
			{
				_userTagsSet.Clear();
			}
			if (_userTags != string.Empty)
			{
				string[] other = _userTags.Split(',');
				_userTagsSet.UnionWith(other);
			}
			SyncTagsLookups();
			SyncTagTogglesToTags();
			SaveUserPrefs();
		}
	}

	protected virtual void SetHidePath()
	{
	}

	protected void SyncHide(bool b)
	{
		if (hidePath == null)
		{
			return;
		}
		if (FileManager.FileExists(hidePath))
		{
			if (!b)
			{
				FileManager.DeleteFile(hidePath);
			}
		}
		else if (b)
		{
			string directoryName = FileManager.GetDirectoryName(hidePath);
			if (!FileManager.DirectoryExists(directoryName))
			{
				FileManager.CreateDirectory(directoryName);
			}
			FileManager.WriteAllText(hidePath, string.Empty);
		}
	}

	protected virtual void SetUserPrefsPath()
	{
	}

	protected virtual void LoadUserPrefs()
	{
		if (userPrefsPath == null || !FileManager.FileExists(userPrefsPath))
		{
			return;
		}
		try
		{
			string aJSON = FileManager.ReadAllText(userPrefsPath, true);
			JSONClass asObject = JSON.Parse(aJSON).AsObject;
			if (asObject != null)
			{
				isLoadingUserPrefs = true;
				replaceTagsJSON.RestoreFromJSON(asObject);
				userTagsJSON.RestoreFromJSON(asObject);
				isLoadingUserPrefs = false;
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception during load of user prefs for item " + displayName + ": " + ex.Message);
		}
	}

	protected virtual void SaveUserPrefs()
	{
		if (isLoadingUserPrefs || userPrefsPath == null || !(userPrefsPath != string.Empty))
		{
			return;
		}
		try
		{
			string directoryName = FileManager.GetDirectoryName(userPrefsPath);
			FileManager.CreateDirectory(directoryName);
			JSONClass jSONClass = new JSONClass();
			replaceTagsJSON.StoreJSON(jSONClass, true, true, true);
			userTagsJSON.StoreJSON(jSONClass, true, true, true);
			FileManager.WriteAllText(userPrefsPath, jSONClass.ToString(string.Empty));
			if (dynamicSelectorUI != null)
			{
				dynamicSelectorUI.ResyncTags();
				dynamicSelectorUI.ResyncUI();
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception during save of user prefs for item " + displayName + ": " + ex.Message);
		}
	}

	public void ActivateUserPrefs()
	{
		InitTagsUI();
		if (dynamicSelectorUI.userPrefsLabel != null)
		{
			dynamicSelectorUI.userPrefsLabel.text = displayName + " prefs";
		}
		if (dynamicSelectorUI.userPrefsBuiltInTagsText != null)
		{
			dynamicSelectorUI.userPrefsBuiltInTagsText.text = _tags;
		}
		replaceTagsJSON.toggle = dynamicSelectorUI.userPrefsReplaceTagsToggle;
		userTagsJSON.inputField = dynamicSelectorUI.userPrefsTagsInputField;
		hideJSON.valNoCallback = isHidden;
		hideJSON.toggle = dynamicSelectorUI.userPrefsHideToggle;
	}

	public void DeactivateUserPrefs()
	{
		ClearTagsUI();
		replaceTagsJSON.toggle = null;
		userTagsJSON.inputField = null;
		hideJSON.toggle = null;
	}

	protected virtual void SyncOtherTags()
	{
		SyncOtherTagsUI();
	}

	protected void SyncTagsToTagsSet()
	{
		string[] array = new string[_userTagsSet.Count];
		_userTagsSet.CopyTo(array);
		userTagsJSON.val = string.Join(",", array);
	}

	protected void SyncTagFromToggle(string tag, bool isEnabled)
	{
		if (!ignoreTagFromToggleCallback)
		{
			if (isEnabled)
			{
				_userTagsSet.Add(tag);
			}
			else
			{
				_userTagsSet.Remove(tag);
			}
			SyncTagsToTagsSet();
		}
	}

	protected void SyncTagTogglesToTags()
	{
		ignoreTagFromToggleCallback = true;
		foreach (KeyValuePair<string, Toggle> item in tagToToggle)
		{
			if (_userTagsSet.Contains(item.Key))
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
			if (item.Value != null && _builtInTagsSet.Contains(item.Key))
			{
				ColorBlock colors = item.Value.colors;
				colors.normalColor = Color.yellow;
				colors.highlightedColor = Color.yellow;
				item.Value.colors = colors;
			}
		}
		ignoreTagFromToggleCallback = false;
	}

	protected void CreateTagToggle(string tag, Transform parent)
	{
		Transform transform = UnityEngine.Object.Instantiate(dynamicSelectorUI.tagTogglePrefab);
		Text componentInChildren = transform.GetComponentInChildren<Text>();
		componentInChildren.text = tag;
		Toggle componentInChildren2 = transform.GetComponentInChildren<Toggle>();
		componentInChildren2.onValueChanged.AddListener(delegate(bool b)
		{
			SyncTagFromToggle(tag, b);
		});
		tagToToggle.Remove(tag);
		tagToToggle.Add(tag, componentInChildren2);
		transform.SetParent(parent, false);
	}

	protected void SyncOtherTagsUI()
	{
		if (!(dynamicSelectorUI.tagTogglePrefab != null) || !(dynamicSelectorUI.userPrefsOtherTagsContent != null))
		{
			return;
		}
		foreach (Transform item in dynamicSelectorUI.userPrefsOtherTagsContent)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		List<string> list = otherTags.ToList();
		list.Sort();
		foreach (string item2 in list)
		{
			CreateTagToggle(item2, dynamicSelectorUI.userPrefsOtherTagsContent);
		}
		SyncTagTogglesToTags();
	}

	protected void ClearTagsToToggle()
	{
		if (tagToToggle == null)
		{
			tagToToggle = new Dictionary<string, Toggle>();
		}
		else
		{
			tagToToggle.Clear();
		}
	}

	protected void ClearTagsUI()
	{
		ClearTagsToToggle();
		if (dynamicSelectorUI.userPrefsRegionTagsContent != null)
		{
			foreach (Transform item in dynamicSelectorUI.userPrefsRegionTagsContent)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		if (dynamicSelectorUI.userPrefsTypeTagsContent != null)
		{
			foreach (Transform item2 in dynamicSelectorUI.userPrefsTypeTagsContent)
			{
				UnityEngine.Object.Destroy(item2.gameObject);
			}
		}
		if (!(dynamicSelectorUI.userPrefsOtherTagsContent != null))
		{
			return;
		}
		foreach (Transform item3 in dynamicSelectorUI.userPrefsOtherTagsContent)
		{
			UnityEngine.Object.Destroy(item3.gameObject);
		}
	}

	protected void InitTagsUI()
	{
		ClearTagsUI();
		if (dynamicSelectorUI.tagTogglePrefab != null)
		{
			if (dynamicSelectorUI.regionTags != null && dynamicSelectorUI.userPrefsRegionTagsContent != null)
			{
				List<string> list = new List<string>(dynamicSelectorUI.regionTags);
				list.Sort();
				foreach (string item in list)
				{
					CreateTagToggle(item, dynamicSelectorUI.userPrefsRegionTagsContent);
				}
			}
			if (dynamicSelectorUI.typeTags != null && dynamicSelectorUI.userPrefsTypeTagsContent != null)
			{
				List<string> list2 = new List<string>(dynamicSelectorUI.typeTags);
				list2.Sort();
				foreach (string item2 in list2)
				{
					CreateTagToggle(item2, dynamicSelectorUI.userPrefsTypeTagsContent);
				}
			}
			SyncOtherTags();
		}
		SyncTagTogglesToTags();
	}

	public void GetThumbnail(ThumbnailLoadedCallback callback)
	{
		if (thumbnail == null)
		{
			if (dynamicRuntimeLoadPath == null || !(dynamicRuntimeLoadPath != string.Empty))
			{
				return;
			}
			string text = Regex.Replace(dynamicRuntimeLoadPath, "\\.vam$", ".jpg");
			if (FileManager.FileExists(text))
			{
				ImageLoaderThreaded.QueuedImage qi = new ImageLoaderThreaded.QueuedImage();
				qi.imgPath = text;
				qi.width = 512;
				qi.height = 512;
				qi.setSize = true;
				qi.fillBackground = true;
				qi.callback = delegate
				{
					thumbnail = qi.tex;
					callback(thumbnail);
				};
				ImageLoaderThreaded.singleton.QueueThumbnail(qi);
			}
		}
		else
		{
			callback(thumbnail);
		}
	}

	public void OpenUI()
	{
		if (active && customizationUI != null && SuperController.singleton != null)
		{
			SuperController.singleton.SetCustomUI(customizationUI);
		}
	}

	protected override void InitInstance()
	{
		base.InitInstance();
		DAZImport componentInChildren = GetComponentInChildren<DAZImport>(true);
		if (componentInChildren != null)
		{
			if (gender == Gender.Female)
			{
				componentInChildren.importGender = DAZImport.Gender.Female;
			}
			else if (gender == Gender.Male)
			{
				componentInChildren.importGender = DAZImport.Gender.Male;
			}
			else
			{
				componentInChildren.importGender = DAZImport.Gender.Neutral;
			}
		}
	}

	protected override void Connect()
	{
		base.Connect();
		if (hasCustomizationUI && customizationUI == null)
		{
			DynamicLoadedUI componentInChildren = GetComponentInChildren<DynamicLoadedUI>(true);
			if (componentInChildren != null)
			{
				customizationUI = componentInChildren.transform;
			}
			if (Application.isPlaying && customizationUI != null)
			{
				customizationUI.gameObject.SetActive(false);
				if (UIbucket != null)
				{
					customizationUI.SetParent(UIbucket);
					customizationUI.localScale = Vector3.one;
				}
			}
		}
		DAZSkinWrap[] componentsInChildren = GetComponentsInChildren<DAZSkinWrap>(true);
		if (componentsInChildren != null && skin != null)
		{
			DAZSkinWrap[] array = componentsInChildren;
			foreach (DAZSkinWrap dAZSkinWrap in array)
			{
				dAZSkinWrap.skinTransform = skin.transform;
				dAZSkinWrap.skin = skin;
			}
		}
		if (drawRigidOnBone != null || drawRigidOnBoneLeft != null || drawRigidOnBoneRight != null)
		{
			DAZMesh[] componentsInChildren2 = GetComponentsInChildren<DAZMesh>(true);
			if (componentsInChildren2 != null)
			{
				DAZMesh[] array2 = componentsInChildren2;
				foreach (DAZMesh dAZMesh in array2)
				{
					switch (dAZMesh.boneSide)
					{
					case DAZMesh.BoneSide.Both:
						dAZMesh.drawFromBone = drawRigidOnBone;
						break;
					case DAZMesh.BoneSide.Left:
						dAZMesh.drawFromBone = drawRigidOnBoneLeft;
						break;
					case DAZMesh.BoneSide.Right:
						dAZMesh.drawFromBone = drawRigidOnBoneRight;
						break;
					}
				}
			}
			if (drawRigidOnBone != null)
			{
				AlignTransform[] componentsInChildren3 = GetComponentsInChildren<AlignTransform>(true);
				AlignTransform[] array3 = componentsInChildren3;
				foreach (AlignTransform alignTransform in array3)
				{
					alignTransform.alignTo = drawRigidOnBone.transform;
				}
			}
		}
		AutoCollider[] componentsInChildren4 = GetComponentsInChildren<AutoCollider>(true);
		if (componentsInChildren4 != null && skin != null)
		{
			AutoCollider[] array4 = componentsInChildren4;
			foreach (AutoCollider autoCollider in array4)
			{
				autoCollider.skinTransform = skin.transform;
				autoCollider.skin = skin;
				if (autoColliderReference != null)
				{
					autoCollider.reference = autoColliderReference;
				}
			}
		}
		DAZImport[] componentsInChildren5 = GetComponentsInChildren<DAZImport>(true);
		if (componentsInChildren5 != null && skin != null)
		{
			DAZImport[] array5 = componentsInChildren5;
			foreach (DAZImport dAZImport in array5)
			{
				dAZImport.skinToWrapToTransform = skin.transform;
				dAZImport.skinToWrapTo = skin;
			}
		}
	}

	public virtual void PartialResetPhysics()
	{
	}

	public virtual void ResetPhysics()
	{
	}

	public virtual void Init()
	{
		if (!_wasInit)
		{
			_wasInit = true;
			replaceTagsJSON = new JSONStorableBool("replaceBuiltInTags", false, SyncReplaceTags);
			userTagsJSON = new JSONStorableString("userTags", string.Empty, SyncUserTags);
			SetHidePath();
			hideJSON = new JSONStorableBool("hide", isHidden, SyncHide);
			SetUserPrefsPath();
			LoadUserPrefs();
			InitTagsLookups();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying && customizationUI != null && UIbucket != null)
		{
			customizationUI.SetParent(UIbucket);
			customizationUI.localScale = Vector3.one;
		}
		ResetPhysics();
	}

	protected override void OnDisable()
	{
		if (customizationUI != null && UIbucket != null && instance != null)
		{
			customizationUI.SetParent(instance);
		}
		base.OnDisable();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ClearTagsToToggle();
	}
}
