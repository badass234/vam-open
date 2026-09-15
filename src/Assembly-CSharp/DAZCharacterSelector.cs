using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GPUTools.Physics.Scripts.Behaviours;
using MVR.FileManagement;
using MeshVR;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class DAZCharacterSelector : JSONStorable
{
	public enum Gender
	{
		None,
		Male,
		Female,
		Both
	}

	protected enum ResyncMorphsOption
	{
		All,
		CurrentGender,
		OtherGender
	}

	protected string[] customParamNames = new string[5] { "character", "clothing", "hair", "morphs", "morphsOtherGender" };

	protected DAZSkinV2.SkinMethod saveSkinMethod;

	protected DAZSkinV2 exportSkin;

	public DAZBones rootBones;

	public string rootBonesName = "Genesis2";

	public string rootBonesNameMale = "Genesis2Male";

	public string rootBonesNameFemale = "Genesis2Female";

	[HideInInspector]
	[SerializeField]
	protected Gender _gender;

	public Transform[] maleTransforms;

	public Transform[] femaleTransforms;

	public AdjustJoints femaleBreastAdjustJoints;

	public AdjustJoints femaleGluteAdjustJoints;

	protected bool _disableAnatomy;

	protected JSONStorableBool disableAnatomyJSON;

	public int[] maleAnatomyOnMaterialSlots;

	public int[] maleAnatomyOffMaterialSlots;

	public int[] femaleAnatomyOnMaterialSlots;

	public int[] femaleAnatomyOffMaterialSlots;

	private DAZMaleAnatomy[] maleAnatomyComponents;

	public Transform morphBankContainer;

	public Transform femaleMorphBank1Prefab;

	public Transform femaleMorphBank2Prefab;

	public Transform femaleMorphBank3Prefab;

	public Transform maleMorphBank1Prefab;

	public Transform maleMorphBank2Prefab;

	public Transform maleMorphBank3Prefab;

	public DAZMorphBank femaleMorphBank1;

	public DAZMorphBank femaleMorphBank2;

	public DAZMorphBank femaleMorphBank3;

	public DAZMorphBank maleMorphBank1;

	public DAZMorphBank maleMorphBank2;

	public DAZMorphBank maleMorphBank3;

	protected HashSet<string> registeredMorphNames;

	protected bool _onlyShowFavoriteMorphsInParameterLists = true;

	protected JSONStorableBool onlyShowFavoriteMorphsInParametersListsJSON;

	protected bool _useMaleMorphsOnFemale;

	protected JSONStorableBool useMaleMorphsOnFemaleJSON;

	protected bool _useFemaleMorphsOnMale;

	protected JSONStorableBool useFemaleMorphsOnMaleJSON;

	public Transform maleCharactersContainer;

	public Transform femaleCharactersContainer;

	public Transform femaleCharactersPrefab;

	public Transform maleCharactersPrefab;

	protected JSONStorableStringChooser characterChooserJSON;

	protected bool _unloadCharactersWhenSwitching;

	protected JSONStorableBool unloadCharactersWhenSwitchingJSON;

	public string startingCharacterName;

	private Dictionary<string, DAZCharacter> _characterByName;

	private DAZCharacter[] _femaleCharacters;

	private DAZCharacter[] _maleCharacters;

	private DAZCharacter[] _characters;

	private DAZCharacter _selectedCharacter;

	protected AsyncFlag delayResumeFlag;

	protected AsyncFlag onCharacterLoadedFlag;

	protected bool _loadedGenderChange;

	public DAZCharacter femalePlaceholderCharacter;

	public DAZCharacter malePlaceholderCharacter;

	protected DAZCharacter _loadedCharacter;

	public Transform UIBucketForDynamicItems;

	public DAZBone hipBone;

	public DAZBone pelvisBone;

	public DAZBone chestBone;

	public DAZBone headBone;

	public DAZBone leftHandBone;

	public DAZBone rightHandBone;

	public DAZBone leftFootBone;

	public DAZBone rightFootBone;

	protected HashSet<string> alreadyReportedDuplicates;

	protected Coroutine refreshCoroutine;

	public Transform maleClothingContainer;

	public Transform femaleClothingContainer;

	public Transform maleClothingPrefab;

	public Transform femaleClothingPrefab;

	public Transform dynamicClothingItemPrefab;

	public DAZClothingItem maleClothingCreatorItem;

	public DAZClothingItem femaleClothingCreatorItem;

	public BoxCollider leftShoeCollider;

	public BoxCollider rightShoeCollider;

	public FreeControllerV3 leftFootController;

	public FreeControllerV3 rightFootController;

	public FreeControllerV3 leftToeController;

	public FreeControllerV3 rightToeController;

	protected Dictionary<string, JSONStorableBool> clothingItemJSONs;

	protected List<JSONStorableAction> clothingItemToggleJSONs;

	protected Dictionary<string, DAZClothingItem> _clothingItemById;

	protected Dictionary<string, DAZClothingItem> _clothingItemByBackupId;

	protected DAZClothingItem[] _maleClothingItems;

	protected DAZClothingItem[] _femaleClothingItems;

	protected JSONStorableAction undressAllClothingAction;

	protected JSONStorableAction removeAllClothingAction;

	protected JSONStorableAction removeAllRealClothingAction;

	public Transform maleHairContainer;

	public Transform femaleHairContainer;

	public Transform maleHairPrefab;

	public Transform femaleHairPrefab;

	public Transform dynamicHairItemPrefab;

	public DAZHairGroup maleHairCreatorItem;

	public DAZHairGroup femaleHairCreatorItem;

	protected Dictionary<string, JSONStorableBool> hairItemJSONs;

	protected List<JSONStorableAction> hairItemToggleJSONs;

	protected Dictionary<string, DAZHairGroup> _hairItemById;

	protected Dictionary<string, DAZHairGroup> _hairItemByBackupId;

	protected DAZHairGroup[] _maleHairItems;

	protected DAZHairGroup[] _femaleHairItems;

	protected JSONStorableAction unloadInactiveObjectsJSONAction;

	public Collider[] auxBreastColliders;

	protected JSONStorableBool useAuxBreastCollidersJSON;

	[SerializeField]
	protected bool _useAuxBreastColliders = true;

	public Transform[] regularColliders;

	public Transform[] regularCollidersFemale;

	public Transform[] regularCollidersMale;

	public Transform[] advancedCollidersFemale;

	public Transform[] advancedCollidersMale;

	protected JSONStorableBool useAdvancedCollidersJSON;

	[SerializeField]
	protected bool _useAdvancedColliders;

	public Transform customUIBucket;

	public GenerateDAZCharacterSelectorUI characterSelectorUI;

	public GenerateDAZCharacterSelectorUI characterSelectorUIAlt;

	public GenerateDAZMorphsControlUI morphsControlFemaleUI;

	public GenerateDAZMorphsControlUI morphsControlFemaleUIAlt;

	public GenerateDAZMorphsControlUI morphsControlMaleUI;

	public GenerateDAZMorphsControlUI morphsControlMaleUIAlt;

	public GenerateDAZClothingSelectorUI clothingSelectorFemaleUI;

	public GenerateDAZClothingSelectorUI clothingSelectorMaleUI;

	public GenerateDAZHairSelectorUI hairSelectorFemaleUI;

	public GenerateDAZHairSelectorUI hairSelectorFemaleUIAlt;

	public GenerateDAZHairSelectorUI hairSelectorMaleUI;

	public GenerateDAZHairSelectorUI hairSelectorMaleUIAlt;

	public DAZCharacterTextureControlUI characterTextureUI;

	public Transform characterTextureUITab;

	public DAZCharacterTextureControlUI characterTextureUIAlt;

	public Transform characterTextureUITabAlt;

	public DAZCharacterMaterialOptions copyUIFrom;

	public Text color1DisplayNameText;

	public HSVColorPicker color1Picker;

	public RectTransform color1Container;

	public Text color2DisplayNameText;

	public HSVColorPicker color2Picker;

	public RectTransform color2Container;

	public Text color3DisplayNameText;

	public HSVColorPicker color3Picker;

	public RectTransform color3Container;

	public Text param1DisplayNameText;

	public Slider param1Slider;

	public Text param1DisplayNameTextAlt;

	public Slider param1SliderAlt;

	public Text param2DisplayNameText;

	public Slider param2Slider;

	public Text param2DisplayNameTextAlt;

	public Slider param2SliderAlt;

	public Text param3DisplayNameText;

	public Slider param3Slider;

	public Text param3DisplayNameTextAlt;

	public Slider param3SliderAlt;

	public Text param4DisplayNameText;

	public Slider param4Slider;

	public Text param4DisplayNameTextAlt;

	public Slider param4SliderAlt;

	public Text param5DisplayNameText;

	public Slider param5Slider;

	public Text param5DisplayNameTextAlt;

	public Slider param5SliderAlt;

	public Text param6DisplayNameText;

	public Slider param6Slider;

	public Text param6DisplayNameTextAlt;

	public Slider param6SliderAlt;

	public Text param7DisplayNameText;

	public Slider param7Slider;

	public Text param7DisplayNameTextAlt;

	public Slider param7SliderAlt;

	public Text param8DisplayNameText;

	public Slider param8Slider;

	public Text param8DisplayNameTextAlt;

	public Slider param8SliderAlt;

	public Text param9DisplayNameText;

	public Slider param9Slider;

	public Text param9DisplayNameTextAlt;

	public Slider param9SliderAlt;

	public Text param10DisplayNameText;

	public Slider param10Slider;

	public Text param10DisplayNameTextAlt;

	public Slider param10SliderAlt;

	public UIPopup textureGroup1Popup;

	public Text textureGroup1Text;

	public UIPopup textureGroup1PopupAlt;

	public Text textureGroup1TextAlt;

	public UIPopup textureGroup2Popup;

	public Text textureGroup2Text;

	public UIPopup textureGroup2PopupAlt;

	public Text textureGroup2TextAlt;

	public UIPopup textureGroup3Popup;

	public Text textureGroup3Text;

	public UIPopup textureGroup3PopupAlt;

	public Text textureGroup3TextAlt;

	public UIPopup textureGroup4Popup;

	public Text textureGroup4Text;

	public UIPopup textureGroup4PopupAlt;

	public Text textureGroup4TextAlt;

	public UIPopup textureGroup5Popup;

	public Text textureGroup5Text;

	public UIPopup textureGroup5PopupAlt;

	public Text textureGroup5TextAlt;

	public Button restoreMaterialFromDefaultsButton;

	public Button saveMaterialToStore1Button;

	public Button restoreMaterialFromStore1Button;

	public Button saveMaterialToStore2Button;

	public Button restoreMaterialFromStore2Button;

	public Button saveMaterialToStore3Button;

	public Button restoreMaterialFromStore3Button;

	public Button restoreMaterialFromDefaultsButtonAlt;

	public Button saveMaterialToStore1ButtonAlt;

	public Button restoreMaterialFromStore1ButtonAlt;

	public Button saveMaterialToStore2ButtonAlt;

	public Button restoreMaterialFromStore2ButtonAlt;

	public Button saveMaterialToStore3ButtonAlt;

	public Button restoreMaterialFromStore3ButtonAlt;

	private DAZMeshEyelidControl _eyelidControl;

	private DAZCharacterRun _characterRun;

	private DAZPhysicsMesh[] _physicsMeshes;

	private SetDAZMorph[] _setDAZMorphs;

	private SpeechBlend lipSync;

	private AutoColliderBatchUpdater[] _autoColliderBatchUpdaters;

	private AutoColliderGroup[] _autoColliderGroups;

	private AutoCollider[] _autoColliders;

	private SetAnchorFromVertex[] _setAnchorFromVertexComps;

	private IgnoreChildColliders[] _ignoreChildColliders;

	private DAZCharacterMaterialOptions[] _materialOptions;

	public DAZCharacterMaterialOptions femaleEyelashMaterialOptions;

	public DAZCharacterMaterialOptions maleEyelashMaterialOptions;

	private bool wasInit;

	public Gender gender
	{
		get
		{
			return _gender;
		}
		set
		{
			if (_gender != value)
			{
				_gender = value;
				SyncGender();
			}
		}
	}

	public DAZMorphBank morphBank1
	{
		get
		{
			switch (gender)
			{
			case Gender.Female:
				return femaleMorphBank1;
			case Gender.Male:
				return maleMorphBank1;
			default:
				return null;
			}
		}
	}

	public DAZMorphBank morphBank1OtherGender
	{
		get
		{
			switch (gender)
			{
			case Gender.Female:
				return maleMorphBank1;
			case Gender.Male:
				return femaleMorphBank1;
			default:
				return null;
			}
		}
	}

	public DAZMorphBank morphBank2
	{
		get
		{
			switch (gender)
			{
			case Gender.Female:
				return femaleMorphBank2;
			case Gender.Male:
				return maleMorphBank2;
			default:
				return null;
			}
		}
	}

	public DAZMorphBank morphBank3
	{
		get
		{
			switch (gender)
			{
			case Gender.Female:
				return femaleMorphBank3;
			case Gender.Male:
				return maleMorphBank3;
			default:
				return null;
			}
		}
	}

	public DAZCharacter[] femaleCharacters => _femaleCharacters;

	public DAZCharacter[] maleCharacters => _maleCharacters;

	public DAZCharacter[] characters => _characters;

	public DAZCharacter selectedCharacter
	{
		get
		{
			return _selectedCharacter;
		}
		set
		{
			if (_selectedCharacter != value)
			{
				if (_selectedCharacter != null)
				{
					DAZCharacter dAZCharacter = _selectedCharacter;
					dAZCharacter.onPreloadedHandlers = (JSONStorableDynamic.OnLoaded)Delegate.Remove(dAZCharacter.onPreloadedHandlers, new JSONStorableDynamic.OnLoaded(OnCharacterPreloaded));
					DAZCharacter dAZCharacter2 = _selectedCharacter;
					dAZCharacter2.onLoadedHandlers = (JSONStorableDynamic.OnLoaded)Delegate.Remove(dAZCharacter2.onLoadedHandlers, new JSONStorableDynamic.OnLoaded(OnCharacterLoaded));
					if (_loadedCharacter != _selectedCharacter)
					{
						_selectedCharacter.needsPostLoadJSONRestore = false;
						_selectedCharacter.gameObject.SetActive(false);
					}
					DisconnectCharacterOptionsUI();
				}
				_selectedCharacter = value;
				if (!(_selectedCharacter != null))
				{
					return;
				}
				bool flag = false;
				if (_selectedCharacter.isMale)
				{
					if (_gender != Gender.Male)
					{
						gender = Gender.Male;
						flag = true;
					}
				}
				else if (_gender != Gender.Female)
				{
					gender = Gender.Female;
					flag = true;
				}
				if (characterSelectorUI != null)
				{
					characterSelectorUI.SetActiveCharacterToggleNoCallback(_selectedCharacter.displayName);
				}
				if (characterSelectorUIAlt != null)
				{
					characterSelectorUIAlt.SetActiveCharacterToggleNoCallback(_selectedCharacter.displayName);
				}
				if (Application.isPlaying)
				{
					if (flag && _characterRun != null)
					{
						_characterRun.Disconnect();
					}
					if (onCharacterLoadedFlag != null && !onCharacterLoadedFlag.Raised)
					{
						onCharacterLoadedFlag.Raise();
					}
					if (!_selectedCharacter.gameObject.activeInHierarchy)
					{
						onCharacterLoadedFlag = new AsyncFlag("Character Load: " + _selectedCharacter.displayName);
						if (SuperController.singleton != null && (containingAtom == null || containingAtom.on) && selectedCharacter.needsPostLoadJSONRestore && !base.isPresetRestore)
						{
							SuperController.singleton.ResetSimulation(onCharacterLoadedFlag);
						}
					}
				}
				DAZCharacter dAZCharacter3 = _selectedCharacter;
				dAZCharacter3.onPreloadedHandlers = (JSONStorableDynamic.OnLoaded)Delegate.Combine(dAZCharacter3.onPreloadedHandlers, new JSONStorableDynamic.OnLoaded(OnCharacterPreloaded));
				DAZCharacter dAZCharacter4 = _selectedCharacter;
				dAZCharacter4.onLoadedHandlers = (JSONStorableDynamic.OnLoaded)Delegate.Combine(dAZCharacter4.onLoadedHandlers, new JSONStorableDynamic.OnLoaded(OnCharacterLoaded));
				_selectedCharacter.gameObject.SetActive(true);
			}
			else if (_selectedCharacter.needsPostLoadJSONRestore)
			{
				_selectedCharacter.PostLoadJSONRestore();
			}
		}
	}

	public DAZClothingItem[] maleClothingItems
	{
		get
		{
			Init();
			return _maleClothingItems;
		}
	}

	public DAZClothingItem[] femaleClothingItems
	{
		get
		{
			Init();
			return _femaleClothingItems;
		}
	}

	public DAZClothingItem[] clothingItems
	{
		get
		{
			Init();
			if (gender == Gender.Male)
			{
				return _maleClothingItems;
			}
			if (gender == Gender.Female)
			{
				return _femaleClothingItems;
			}
			return null;
		}
	}

	public DAZHairGroup[] maleHairItems
	{
		get
		{
			Init();
			return _maleHairItems;
		}
	}

	public DAZHairGroup[] femaleHairItems
	{
		get
		{
			Init();
			return _femaleHairItems;
		}
	}

	public DAZHairGroup[] hairItems
	{
		get
		{
			Init();
			if (gender == Gender.Male)
			{
				return _maleHairItems;
			}
			if (gender == Gender.Female)
			{
				return _femaleHairItems;
			}
			return null;
		}
	}

	public bool useAuxBreastColliders
	{
		get
		{
			return _useAuxBreastColliders;
		}
		set
		{
			if (useAuxBreastCollidersJSON != null)
			{
				useAuxBreastCollidersJSON.val = value;
			}
			else if (_useAuxBreastColliders != value)
			{
				SyncUseAuxBreastColliders(value);
			}
		}
	}

	public bool useAdvancedColliders
	{
		get
		{
			return _useAdvancedColliders;
		}
		set
		{
			if (useAdvancedCollidersJSON != null)
			{
				useAdvancedCollidersJSON.val = value;
			}
			else if (_useAdvancedColliders != value)
			{
				SyncUseAdvancedColliders(value);
			}
		}
	}

	public GenerateDAZMorphsControlUI morphsControlUI
	{
		get
		{
			if (gender == Gender.Male)
			{
				return morphsControlMaleUI;
			}
			return morphsControlFemaleUI;
		}
	}

	public GenerateDAZMorphsControlUI morphsControlUIOtherGender
	{
		get
		{
			if (gender == Gender.Male)
			{
				return morphsControlFemaleUI;
			}
			return morphsControlMaleUI;
		}
	}

	public GenerateDAZMorphsControlUI morphsControlUIAlt
	{
		get
		{
			if (gender == Gender.Male)
			{
				return morphsControlMaleUIAlt;
			}
			return morphsControlFemaleUIAlt;
		}
	}

	public GenerateDAZClothingSelectorUI clothingSelectorUI
	{
		get
		{
			if (gender == Gender.Male)
			{
				return clothingSelectorMaleUI;
			}
			return clothingSelectorFemaleUI;
		}
	}

	public GenerateDAZHairSelectorUI hairSelectorUI
	{
		get
		{
			if (gender == Gender.Male)
			{
				return hairSelectorMaleUI;
			}
			return hairSelectorFemaleUI;
		}
	}

	public GenerateDAZHairSelectorUI hairSelectorUIAlt
	{
		get
		{
			if (gender == Gender.Male)
			{
				return hairSelectorMaleUIAlt;
			}
			return hairSelectorFemaleUIAlt;
		}
	}

	public override string[] GetCustomParamNames()
	{
		return customParamNames;
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		JSONArray jSONArray = new JSONArray();
		if (includeAppearance || forceStore)
		{
			if (base.isPresetStore)
			{
				if (forceStore)
				{
					jSON["character"] = selectedCharacter.displayName;
					needsStore = true;
				}
			}
			else
			{
				jSON["character"] = selectedCharacter.displayName;
				needsStore = true;
			}
			bool flag = !base.isPresetStore;
			for (int i = 0; i < clothingItems.Length; i++)
			{
				DAZClothingItem dAZClothingItem = clothingItems[i];
				if (((!base.isPresetStore || forceStore) && dAZClothingItem.active) || (base.isPresetStore && dAZClothingItem.active != dAZClothingItem.startActive))
				{
					if (dAZClothingItem.packageUid != null && dAZClothingItem.packageUid != string.Empty && SuperController.singleton != null && SuperController.singleton.packageMode)
					{
						SuperController.singleton.AddVarPackageRefToVacPackage(dAZClothingItem.packageUid);
					}
					JSONClass jSONClass = new JSONClass();
					jSONArray.Add(jSONClass);
					jSONClass["id"] = dAZClothingItem.uid;
					if (dAZClothingItem.internalUid != null && dAZClothingItem.internalUid != string.Empty)
					{
						jSONClass["internalId"] = dAZClothingItem.internalUid;
					}
					jSONClass["enabled"].AsBool = dAZClothingItem.active;
					flag = true;
				}
			}
			if (flag || forceStore)
			{
				jSON["clothing"] = jSONArray;
				needsStore = true;
			}
			jSONArray = new JSONArray();
			bool flag2 = !base.isPresetStore;
			for (int j = 0; j < hairItems.Length; j++)
			{
				DAZHairGroup dAZHairGroup = hairItems[j];
				if (((!base.isPresetStore || forceStore) && dAZHairGroup.active) || (base.isPresetStore && dAZHairGroup.active != dAZHairGroup.startActive))
				{
					if (dAZHairGroup.packageUid != null && dAZHairGroup.packageUid != string.Empty && SuperController.singleton != null && SuperController.singleton.packageMode)
					{
						SuperController.singleton.AddVarPackageRefToVacPackage(dAZHairGroup.packageUid);
					}
					JSONClass jSONClass2 = new JSONClass();
					jSONArray.Add(jSONClass2);
					jSONClass2["id"] = dAZHairGroup.uid;
					if (dAZHairGroup.internalUid != null && dAZHairGroup.internalUid != string.Empty)
					{
						jSONClass2["internalId"] = dAZHairGroup.internalUid;
					}
					jSONClass2["enabled"].AsBool = dAZHairGroup.active;
					flag2 = true;
				}
			}
			if (flag2 || forceStore)
			{
				jSON["hair"] = jSONArray;
				needsStore = true;
			}
		}
		jSONArray = new JSONArray();
		bool flag3 = !base.isPresetStore;
		if (morphsControlUI != null)
		{
			List<DAZMorph> morphs = morphsControlUI.GetMorphs();
			if (morphs != null)
			{
				foreach (DAZMorph item in morphs)
				{
					bool isPoseControl = item.isPoseControl;
					if ((!includePhysical || !isPoseControl) && (!includeAppearance || isPoseControl))
					{
						continue;
					}
					JSONClass jSONClass3 = new JSONClass();
					if (item.StoreJSON(jSONClass3))
					{
						if (item.isRuntime && SuperController.singleton != null && SuperController.singleton.packageMode)
						{
							string metaLoadPath = item.metaLoadPath;
							metaLoadPath = Regex.Replace(metaLoadPath, ".*/Import/", "Import/");
							SuperController.singleton.AddFileToPackage(item.metaLoadPath, metaLoadPath);
							metaLoadPath = item.deltasLoadPath;
							metaLoadPath = Regex.Replace(metaLoadPath, ".*/Import/", "Import/");
							SuperController.singleton.AddFileToPackage(item.deltasLoadPath, metaLoadPath);
						}
						flag3 = true;
						jSONArray.Add(jSONClass3);
					}
				}
			}
		}
		else
		{
			Debug.LogWarning("morphsControl UI not set for " + base.name + " character " + selectedCharacter.displayName);
		}
		if (flag3 || forceStore)
		{
			jSON["morphs"] = jSONArray;
			needsStore = true;
		}
		if (((_gender == Gender.Female && _useMaleMorphsOnFemale) || (_gender == Gender.Male && _useFemaleMorphsOnMale)) && morphsControlUIOtherGender != null)
		{
			jSONArray = new JSONArray();
			bool flag4 = false;
			List<DAZMorph> morphs2 = morphsControlUIOtherGender.GetMorphs();
			if (morphs2 != null)
			{
				foreach (DAZMorph item2 in morphs2)
				{
					bool isPoseControl2 = item2.isPoseControl;
					if ((!includePhysical || !isPoseControl2) && (!includeAppearance || isPoseControl2))
					{
						continue;
					}
					JSONClass jSONClass4 = new JSONClass();
					if (item2.StoreJSON(jSONClass4))
					{
						if (item2.isRuntime && SuperController.singleton != null && SuperController.singleton.packageMode)
						{
							string metaLoadPath2 = item2.metaLoadPath;
							metaLoadPath2 = Regex.Replace(metaLoadPath2, ".*/Import/", "Import/");
							SuperController.singleton.AddFileToPackage(item2.metaLoadPath, metaLoadPath2);
							metaLoadPath2 = item2.deltasLoadPath;
							metaLoadPath2 = Regex.Replace(metaLoadPath2, ".*/Import/", "Import/");
							SuperController.singleton.AddFileToPackage(item2.deltasLoadPath, metaLoadPath2);
						}
						flag4 = true;
						jSONArray.Add(jSONClass4);
					}
				}
			}
			if (flag4 || forceStore)
			{
				jSON["morphsOtherGender"] = jSONArray;
				needsStore = true;
			}
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		Init();
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		insideRestore = true;
		if (!base.appearanceLocked && restoreAppearance)
		{
			if (!IsCustomAppearanceParamLocked("character"))
			{
				if (jc["character"] != null)
				{
					string text = jc["character"];
					if (text == string.Empty)
					{
						SelectCharacterByName(startingCharacterName, true);
					}
					else
					{
						SelectCharacterByName(text, true);
					}
				}
				else if (setMissingToDefault)
				{
					SelectCharacterByName(startingCharacterName, true);
				}
			}
			if (!IsCustomAppearanceParamLocked("hair"))
			{
				if (jc["hair"] != null)
				{
					JSONArray asArray = jc["hair"].AsArray;
					if (asArray != null)
					{
						if (!base.mergeRestore)
						{
							ResetHair(true);
						}
						foreach (JSONClass item in asArray)
						{
							string id = item["id"];
							string text2 = item["internalId"];
							string itemId = FileManager.NormalizeID(id);
							bool asBool = item["enabled"].AsBool;
							DAZHairGroup hairItem = GetHairItem(itemId);
							if (hairItem == null && text2 != null)
							{
								hairItem = GetHairItem(text2);
								if (hairItem != null)
								{
									itemId = text2;
								}
							}
							SetActiveHairItem(itemId, asBool, true);
						}
					}
					else
					{
						string text3 = jc["hair"];
						if (text3 == string.Empty)
						{
							if (!base.mergeRestore)
							{
								ResetHair();
							}
						}
						else
						{
							if (!base.mergeRestore)
							{
								ResetHair(true);
							}
							SetActiveHairItem(text3, true, true);
						}
					}
				}
				else if (setMissingToDefault && !base.mergeRestore)
				{
					ResetHair();
				}
			}
			if (!IsCustomAppearanceParamLocked("clothing"))
			{
				if (jc["clothing"] != null)
				{
					JSONArray asArray2 = jc["clothing"].AsArray;
					if (asArray2 != null)
					{
						if (!base.mergeRestore)
						{
							ResetClothing(true);
						}
						foreach (JSONClass item2 in asArray2)
						{
							string text4 = item2["id"];
							string itemId2 = ((text4 != null) ? FileManager.NormalizeID(text4) : ((string)item2["name"]));
							string text5 = item2["internalId"];
							bool asBool2 = item2["enabled"].AsBool;
							DAZClothingItem clothingItem = GetClothingItem(itemId2);
							if (clothingItem == null && text5 != null)
							{
								clothingItem = GetClothingItem(text5);
								if (clothingItem != null)
								{
									itemId2 = text5;
								}
							}
							SetActiveClothingItem(itemId2, asBool2, true);
						}
					}
					else if (!base.mergeRestore)
					{
						ResetClothing();
					}
				}
				else if (setMissingToDefault && !base.mergeRestore)
				{
					ResetClothing();
				}
			}
			if (containingAtom.isPreparingToPutBackInPool)
			{
				DAZCharacter[] array = _femaleCharacters;
				foreach (DAZCharacter dAZCharacter in array)
				{
					dAZCharacter.ResetUnregisteredInstance();
				}
				DAZCharacter[] array2 = _maleCharacters;
				foreach (DAZCharacter dAZCharacter2 in array2)
				{
					dAZCharacter2.ResetUnregisteredInstance();
				}
				DAZClothingItem[] array3 = _femaleClothingItems;
				foreach (DAZClothingItem dAZClothingItem in array3)
				{
					dAZClothingItem.ResetUnregisteredInstance();
				}
				DAZClothingItem[] array4 = _maleClothingItems;
				foreach (DAZClothingItem dAZClothingItem2 in array4)
				{
					dAZClothingItem2.ResetUnregisteredInstance();
				}
				DAZHairGroup[] array5 = _femaleHairItems;
				foreach (DAZHairGroup dAZHairGroup in array5)
				{
					dAZHairGroup.ResetUnregisteredInstance();
				}
				DAZHairGroup[] array6 = _maleHairItems;
				foreach (DAZHairGroup dAZHairGroup2 in array6)
				{
					dAZHairGroup2.ResetUnregisteredInstance();
				}
			}
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = base.appearanceLocked || IsCustomAppearanceParamLocked("morphs");
		bool flag4 = base.physicalLocked || IsCustomPhysicalParamLocked("morphs");
		bool flag5 = base.appearanceLocked || IsCustomAppearanceParamLocked("morphsOtherGender");
		bool flag6 = base.physicalLocked || IsCustomPhysicalParamLocked("morphsOtherGender");
		if (!flag3 || !flag4)
		{
			if (jc["morphs"] != null && morphsControlUI != null && !base.mergeRestore)
			{
				ResetMorphsToDefault(restorePhysical && !flag4, restoreAppearance && !flag3);
				flag = true;
			}
			else if (setMissingToDefault && !base.mergeRestore)
			{
				ResetMorphsToDefault(restorePhysical && !flag4, restoreAppearance && !flag3);
				flag = true;
			}
		}
		if (!flag5 || !flag6)
		{
			if (jc["morphsOtherGender"] != null && morphsControlUIOtherGender != null && !base.mergeRestore)
			{
				ResetMorphsOtherGender(restorePhysical && !flag6, restoreAppearance && !flag5);
				flag = true;
			}
			else if (setMissingToDefault && !base.mergeRestore)
			{
				ResetMorphsOtherGender(restorePhysical && !flag6, restoreAppearance && !flag5);
				flag = true;
			}
		}
		if (flag)
		{
			if (_characterRun != null)
			{
				_characterRun.SmoothApplyMorphsLite();
			}
			else
			{
				if (morphBank1 != null)
				{
					morphBank1.ApplyMorphsImmediate();
				}
				if (morphBank1OtherGender != null)
				{
					morphBank1OtherGender.ApplyMorphsImmediate();
				}
				if (morphBank2 != null)
				{
					morphBank2.ApplyMorphsImmediate();
				}
				if (morphBank3 != null)
				{
					morphBank3.ApplyMorphsImmediate();
				}
			}
		}
		if (containingAtom.isPreparingToPutBackInPool)
		{
			UnloadDemandActivatedMorphs();
		}
		if ((!flag3 || !flag4) && jc["morphs"] != null && morphsControlUI != null)
		{
			if (!flag3 && restoreAppearance && SuperController.singleton != null)
			{
				bool flag7 = false;
				if (morphBank1 != null && morphBank1.LoadTransientMorphs(SuperController.singleton.currentLoadDir))
				{
					flag7 = true;
				}
				if (morphBank2 != null && morphBank2.LoadTransientMorphs(SuperController.singleton.currentLoadDir))
				{
					flag7 = true;
				}
				if (morphBank3 != null && morphBank3.LoadTransientMorphs(SuperController.singleton.currentLoadDir))
				{
					flag7 = true;
				}
				if (flag7)
				{
					flag2 = true;
					ResyncMorphs(ResyncMorphsOption.CurrentGender);
				}
			}
			JSONArray asArray3 = jc["morphs"].AsArray;
			if (asArray3 != null)
			{
				foreach (JSONClass item3 in asArray3)
				{
					string text6 = item3["uid"];
					string text7 = item3["name"];
					DAZMorph dAZMorph = null;
					if (text6 != null)
					{
						text6 = FileManager.NormalizeID(text6);
						dAZMorph = morphsControlUI.GetMorphByUid(text6);
					}
					if (dAZMorph == null && text7 != null)
					{
						dAZMorph = morphsControlUI.GetMorphByDisplayName(text7);
					}
					if (dAZMorph != null)
					{
						bool isPoseControl = dAZMorph.isPoseControl;
						if ((!flag4 && restorePhysical && isPoseControl) || (!flag3 && restoreAppearance && !isPoseControl))
						{
							dAZMorph.RestoreFromJSON(item3);
						}
					}
					else if (!morphsControlUI.IsBadMorph(text7))
					{
						if (text6 != null)
						{
							SuperController.LogError("Could not find morph by uid " + text6 + " or name " + text7 + " referenced in save file");
						}
						else if (text7 != null)
						{
							SuperController.LogError("Could not find morph by name " + text7 + " referenced in save file");
						}
					}
				}
				if (morphsControlUI.CleanDemandActivatedMorphs())
				{
					flag2 = true;
				}
			}
		}
		if ((!flag5 || !flag6) && jc["morphsOtherGender"] != null && morphsControlUIOtherGender != null)
		{
			if (!flag5 && restoreAppearance && SuperController.singleton != null)
			{
				bool flag8 = false;
				if (morphBank1OtherGender != null && morphBank1OtherGender.LoadTransientMorphs(SuperController.singleton.currentLoadDir))
				{
					flag8 = true;
				}
				if (flag8)
				{
					flag2 = true;
					ResyncMorphs(ResyncMorphsOption.OtherGender);
				}
			}
			JSONArray asArray4 = jc["morphsOtherGender"].AsArray;
			if (asArray4 != null)
			{
				foreach (JSONClass item4 in asArray4)
				{
					string text8 = item4["uid"];
					string text9 = item4["name"];
					DAZMorph dAZMorph2 = null;
					if (text8 != null)
					{
						text8 = FileManager.NormalizeID(text8);
						dAZMorph2 = morphsControlUIOtherGender.GetMorphByUid(text8);
					}
					if (dAZMorph2 == null && text9 != null)
					{
						dAZMorph2 = morphsControlUIOtherGender.GetMorphByDisplayName(text9);
					}
					if (dAZMorph2 != null)
					{
						bool isPoseControl2 = dAZMorph2.isPoseControl;
						if ((!flag6 && restorePhysical && isPoseControl2) || (!flag5 && restoreAppearance && !isPoseControl2))
						{
							dAZMorph2.RestoreFromJSON(item4);
						}
					}
					else if (!morphsControlUIOtherGender.IsBadMorph(text9))
					{
						if (text8 != null)
						{
							SuperController.LogError("Could not find morph by uid " + text8 + " or name " + text9 + " referenced in save file");
						}
						else if (text9 != null)
						{
							SuperController.LogError("Could not find morph by name " + text9 + " referenced in save file");
						}
					}
				}
				if (morphsControlUIOtherGender.CleanDemandActivatedMorphs())
				{
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			ResyncMorphRegistry();
		}
		if (flag)
		{
			if (_characterRun != null)
			{
				_characterRun.SmoothApplyMorphs();
			}
			else
			{
				if (morphBank1 != null)
				{
					morphBank1.ApplyMorphsImmediate();
				}
				if (morphBank1OtherGender != null)
				{
					morphBank1OtherGender.ApplyMorphsImmediate();
				}
				if (morphBank2 != null)
				{
					morphBank2.ApplyMorphsImmediate();
				}
				if (morphBank3 != null)
				{
					morphBank3.ApplyMorphsImmediate();
				}
			}
		}
		insideRestore = false;
	}

	private IEnumerator ExportOBJHelper()
	{
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		_characterRun.doSnap = false;
		OBJExporter oe = GetComponent<OBJExporter>();
		Dictionary<int, bool> enabledMats = new Dictionary<int, bool>();
		for (int i = 0; i < exportSkin.materialsEnabled.Length; i++)
		{
			if (exportSkin.materialsEnabled[i])
			{
				enabledMats.Add(i, true);
			}
		}
		oe.Export(selectedCharacter.name + ".obj", exportSkin.GetMesh(), _characterRun.snappedMorphedUVVertices, _characterRun.snappedMorphedUVNormals, exportSkin.dazMesh.materials, enabledMats);
		oe.Export(selectedCharacter.name + "_skinned.obj", exportSkin.GetMesh(), _characterRun.snappedSkinnedVertices, _characterRun.snappedSkinnedNormals, exportSkin.dazMesh.materials, enabledMats);
	}

	public void ExportCurrentCharacterOBJ()
	{
		OBJExporter component = GetComponent<OBJExporter>();
		DAZMergedSkinV2 componentInChildren = selectedCharacter.GetComponentInChildren<DAZMergedSkinV2>();
		if (componentInChildren != null && component != null && _characterRun != null)
		{
			exportSkin = componentInChildren;
			_characterRun.doSnap = true;
			StartCoroutine(ExportOBJHelper());
		}
	}

	public void InitBones()
	{
		if (rootBones != null)
		{
			rootBones.Init();
			maleAnatomyComponents = rootBones.GetComponentsInChildren<DAZMaleAnatomy>(true);
		}
	}

	protected void SyncGender()
	{
		Transform[] array = maleTransforms;
		foreach (Transform transform in array)
		{
			if (transform != null)
			{
				if (_gender == Gender.Both || _gender == Gender.Male)
				{
					transform.gameObject.SetActive(true);
				}
				else
				{
					transform.gameObject.SetActive(false);
				}
			}
		}
		if (maleMorphBank1 != null)
		{
			if (_gender == Gender.Both || _gender == Gender.Male)
			{
				maleMorphBank1.gameObject.SetActive(true);
			}
			else
			{
				maleMorphBank1.gameObject.SetActive(false);
			}
		}
		if (maleMorphBank2 != null)
		{
			if (_gender == Gender.Both || _gender == Gender.Male)
			{
				maleMorphBank2.gameObject.SetActive(true);
			}
			else
			{
				maleMorphBank2.gameObject.SetActive(false);
			}
		}
		if (maleMorphBank3 != null)
		{
			if (_gender == Gender.Both || _gender == Gender.Male)
			{
				maleMorphBank3.gameObject.SetActive(true);
			}
			else
			{
				maleMorphBank3.gameObject.SetActive(false);
			}
		}
		Transform[] array2 = femaleTransforms;
		foreach (Transform transform2 in array2)
		{
			if (transform2 != null)
			{
				if (_gender == Gender.Both || _gender == Gender.Female)
				{
					transform2.gameObject.SetActive(true);
				}
				else
				{
					transform2.gameObject.SetActive(false);
				}
			}
		}
		if (femaleMorphBank1 != null)
		{
			if (_gender == Gender.Both || _gender == Gender.Female)
			{
				femaleMorphBank1.gameObject.SetActive(true);
			}
			else
			{
				femaleMorphBank1.gameObject.SetActive(false);
			}
		}
		if (femaleMorphBank2 != null)
		{
			if (_gender == Gender.Both || _gender == Gender.Female)
			{
				femaleMorphBank2.gameObject.SetActive(true);
			}
			else
			{
				femaleMorphBank2.gameObject.SetActive(false);
			}
		}
		if (femaleMorphBank3 != null)
		{
			if (_gender == Gender.Both || _gender == Gender.Female)
			{
				femaleMorphBank3.gameObject.SetActive(true);
			}
			else
			{
				femaleMorphBank3.gameObject.SetActive(false);
			}
		}
		ResyncMorphRegistry();
		if (rootBones != null)
		{
			if (_gender == Gender.Male)
			{
				rootBones.name = rootBonesNameMale;
				rootBones.isMale = true;
			}
			else if (_gender == Gender.Female)
			{
				rootBones.name = rootBonesNameFemale;
				rootBones.isMale = false;
			}
			else
			{
				rootBones.name = rootBonesName;
				rootBones.isMale = false;
			}
		}
		SyncColliders();
		Init(true);
	}

	private void SyncAnatomy()
	{
		if (!(_selectedCharacter != null))
		{
			return;
		}
		bool flag = !_disableAnatomy;
		if (clothingItems != null)
		{
			DAZClothingItem[] array = clothingItems;
			foreach (DAZClothingItem dAZClothingItem in array)
			{
				if (dAZClothingItem != null && dAZClothingItem.active && dAZClothingItem.disableAnatomy)
				{
					flag = false;
					break;
				}
			}
		}
		if (hairItems != null)
		{
			DAZHairGroup[] array2 = hairItems;
			foreach (DAZHairGroup dAZHairGroup in array2)
			{
				if (dAZHairGroup != null && dAZHairGroup.active && dAZHairGroup.disableAnatomy)
				{
					flag = false;
					break;
				}
			}
		}
		DAZSkinV2 skin = _selectedCharacter.skin;
		if (gender == Gender.Male)
		{
			if (maleAnatomyComponents != null && maleAnatomyComponents.Length > 0)
			{
				DAZMaleAnatomy[] array3 = maleAnatomyComponents;
				foreach (DAZMaleAnatomy dAZMaleAnatomy in array3)
				{
					Rigidbody[] componentsInChildren = dAZMaleAnatomy.GetComponentsInChildren<Rigidbody>();
					Rigidbody[] array4 = componentsInChildren;
					foreach (Rigidbody rigidbody in array4)
					{
						rigidbody.detectCollisions = flag;
					}
				}
			}
			if (skin != null)
			{
				int[] array5 = maleAnatomyOnMaterialSlots;
				foreach (int num in array5)
				{
					if (skin.materialsEnabled.Length > num)
					{
						skin.materialsEnabled[num] = flag;
					}
				}
				int[] array6 = maleAnatomyOffMaterialSlots;
				foreach (int num2 in array6)
				{
					if (skin.materialsEnabled.Length > num2)
					{
						skin.materialsEnabled[num2] = !flag;
					}
				}
			}
			if (!(skin != null) || !(skin.dazMesh != null))
			{
				return;
			}
			int[] array7 = maleAnatomyOnMaterialSlots;
			foreach (int num4 in array7)
			{
				if (skin.dazMesh.materialsEnabled.Length > num4)
				{
					skin.dazMesh.materialsEnabled[num4] = flag;
				}
			}
			int[] array8 = maleAnatomyOffMaterialSlots;
			foreach (int num6 in array8)
			{
				if (skin.dazMesh.materialsEnabled.Length > num6)
				{
					skin.dazMesh.materialsEnabled[num6] = !flag;
				}
			}
			return;
		}
		if (skin != null)
		{
			int[] array9 = femaleAnatomyOnMaterialSlots;
			foreach (int num8 in array9)
			{
				if (skin.materialsEnabled.Length > num8)
				{
					skin.materialsEnabled[num8] = flag;
				}
			}
			int[] array10 = femaleAnatomyOffMaterialSlots;
			foreach (int num10 in array10)
			{
				if (skin.materialsEnabled.Length > num10)
				{
					skin.materialsEnabled[num10] = !flag;
				}
			}
		}
		if (skin != null && skin.dazMesh != null)
		{
			int[] array11 = femaleAnatomyOnMaterialSlots;
			foreach (int num12 in array11)
			{
				if (skin.dazMesh.materialsEnabled.Length > num12)
				{
					skin.dazMesh.materialsEnabled[num12] = flag;
				}
			}
			int[] array12 = femaleAnatomyOffMaterialSlots;
			foreach (int num14 in array12)
			{
				if (skin.dazMesh.materialsEnabled.Length > num14)
				{
					skin.dazMesh.materialsEnabled[num14] = !flag;
				}
			}
		}
		if (femaleBreastAdjustJoints != null)
		{
			float num15 = 1f;
			bool springDamperMultiplierOn = false;
			DAZClothingItem[] array13 = clothingItems;
			foreach (DAZClothingItem dAZClothingItem2 in array13)
			{
				if (dAZClothingItem2 != null && dAZClothingItem2.active && dAZClothingItem2.adjustFemaleBreastJointSpringAndDamper && dAZClothingItem2.jointAdjustEnabled)
				{
					springDamperMultiplierOn = true;
					if (dAZClothingItem2.breastJointSpringAndDamperMultiplier > num15)
					{
						num15 = dAZClothingItem2.breastJointSpringAndDamperMultiplier;
					}
				}
			}
			femaleBreastAdjustJoints.springDamperMultiplierOn = springDamperMultiplierOn;
			femaleBreastAdjustJoints.springDamperMultiplier = num15;
		}
		if (!(femaleGluteAdjustJoints != null))
		{
			return;
		}
		float num17 = 1f;
		bool springDamperMultiplierOn2 = false;
		DAZClothingItem[] array14 = clothingItems;
		foreach (DAZClothingItem dAZClothingItem3 in array14)
		{
			if (dAZClothingItem3 != null && dAZClothingItem3.active && dAZClothingItem3.adjustFemaleGluteJointSpringAndDamper && dAZClothingItem3.jointAdjustEnabled)
			{
				springDamperMultiplierOn2 = true;
				if (dAZClothingItem3.breastJointSpringAndDamperMultiplier > num17)
				{
					num17 = dAZClothingItem3.gluteJointSpringAndDamperMultiplier;
				}
			}
		}
		femaleGluteAdjustJoints.springDamperMultiplierOn = springDamperMultiplierOn2;
		femaleGluteAdjustJoints.springDamperMultiplier = num17;
	}

	protected void SyncDisableAnatomy(bool b)
	{
		_disableAnatomy = b;
		SyncAnatomy();
	}

	public void SetMorphAnimatable(DAZMorph dm)
	{
		SuperController.LogError("SetMorphAnimatable is deprecated");
	}

	protected void InitMorphBanks()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (morphBankContainer == null)
		{
			morphBankContainer = base.transform;
		}
		if (femaleMorphBank1 == null && femaleMorphBank1Prefab != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(femaleMorphBank1Prefab.gameObject);
			gameObject.transform.SetParent(morphBankContainer);
			femaleMorphBank1 = gameObject.GetComponent<DAZMorphBank>();
			femaleMorphBank1.morphBones = rootBones;
			femaleMorphBank1.Init();
			DAZMorphBank dAZMorphBank = femaleMorphBank1;
			dAZMorphBank.onMorphFavoriteChangedHandlers = (DAZMorphBank.MorphFavoriteChanged)Delegate.Combine(dAZMorphBank.onMorphFavoriteChangedHandlers, new DAZMorphBank.MorphFavoriteChanged(MorphFavoriteChanged));
			if (morphsControlFemaleUI != null)
			{
				morphsControlFemaleUI.morphBank1 = femaleMorphBank1;
			}
			if (morphsControlFemaleUIAlt != null)
			{
				morphsControlFemaleUIAlt.morphBank1 = femaleMorphBank1;
			}
		}
		if (femaleMorphBank2 == null && femaleMorphBank2Prefab != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(femaleMorphBank2Prefab.gameObject);
			gameObject2.transform.SetParent(morphBankContainer);
			femaleMorphBank2 = gameObject2.GetComponent<DAZMorphBank>();
			femaleMorphBank2.morphBones = rootBones;
			femaleMorphBank2.Init();
			DAZMorphBank dAZMorphBank2 = femaleMorphBank2;
			dAZMorphBank2.onMorphFavoriteChangedHandlers = (DAZMorphBank.MorphFavoriteChanged)Delegate.Combine(dAZMorphBank2.onMorphFavoriteChangedHandlers, new DAZMorphBank.MorphFavoriteChanged(MorphFavoriteChanged));
			if (morphsControlFemaleUI != null)
			{
				morphsControlFemaleUI.morphBank2 = femaleMorphBank2;
			}
			if (morphsControlFemaleUIAlt != null)
			{
				morphsControlFemaleUIAlt.morphBank2 = femaleMorphBank2;
			}
		}
		if (femaleMorphBank3 == null && femaleMorphBank3Prefab != null)
		{
			GameObject gameObject3 = UnityEngine.Object.Instantiate(femaleMorphBank3Prefab.gameObject);
			gameObject3.transform.SetParent(morphBankContainer);
			femaleMorphBank3 = gameObject3.GetComponent<DAZMorphBank>();
			femaleMorphBank3.morphBones = rootBones;
			femaleMorphBank3.Init();
			DAZMorphBank dAZMorphBank3 = femaleMorphBank3;
			dAZMorphBank3.onMorphFavoriteChangedHandlers = (DAZMorphBank.MorphFavoriteChanged)Delegate.Combine(dAZMorphBank3.onMorphFavoriteChangedHandlers, new DAZMorphBank.MorphFavoriteChanged(MorphFavoriteChanged));
			if (morphsControlFemaleUI != null)
			{
				morphsControlFemaleUI.morphBank3 = femaleMorphBank3;
			}
			if (morphsControlFemaleUIAlt != null)
			{
				morphsControlFemaleUIAlt.morphBank3 = femaleMorphBank3;
			}
		}
		if (maleMorphBank1 == null && maleMorphBank1Prefab != null)
		{
			GameObject gameObject4 = UnityEngine.Object.Instantiate(maleMorphBank1Prefab.gameObject);
			gameObject4.transform.SetParent(morphBankContainer);
			maleMorphBank1 = gameObject4.GetComponent<DAZMorphBank>();
			maleMorphBank1.morphBones = rootBones;
			maleMorphBank1.Init();
			DAZMorphBank dAZMorphBank4 = maleMorphBank1;
			dAZMorphBank4.onMorphFavoriteChangedHandlers = (DAZMorphBank.MorphFavoriteChanged)Delegate.Combine(dAZMorphBank4.onMorphFavoriteChangedHandlers, new DAZMorphBank.MorphFavoriteChanged(MorphFavoriteChanged));
			if (morphsControlMaleUI != null)
			{
				morphsControlMaleUI.morphBank1 = maleMorphBank1;
			}
			if (morphsControlMaleUIAlt != null)
			{
				morphsControlMaleUIAlt.morphBank1 = maleMorphBank1;
			}
		}
		if (maleMorphBank2 == null && maleMorphBank2Prefab != null)
		{
			GameObject gameObject5 = UnityEngine.Object.Instantiate(maleMorphBank2Prefab.gameObject);
			gameObject5.transform.SetParent(morphBankContainer);
			maleMorphBank2 = gameObject5.GetComponent<DAZMorphBank>();
			maleMorphBank2.morphBones = rootBones;
			maleMorphBank2.Init();
			DAZMorphBank dAZMorphBank5 = maleMorphBank2;
			dAZMorphBank5.onMorphFavoriteChangedHandlers = (DAZMorphBank.MorphFavoriteChanged)Delegate.Combine(dAZMorphBank5.onMorphFavoriteChangedHandlers, new DAZMorphBank.MorphFavoriteChanged(MorphFavoriteChanged));
			if (morphsControlMaleUI != null)
			{
				morphsControlMaleUI.morphBank2 = maleMorphBank2;
			}
			if (morphsControlMaleUIAlt != null)
			{
				morphsControlMaleUIAlt.morphBank2 = maleMorphBank2;
			}
		}
		if (maleMorphBank3 == null && maleMorphBank3Prefab != null)
		{
			GameObject gameObject6 = UnityEngine.Object.Instantiate(maleMorphBank3Prefab.gameObject);
			gameObject6.transform.SetParent(morphBankContainer);
			maleMorphBank3 = gameObject6.GetComponent<DAZMorphBank>();
			maleMorphBank3.morphBones = rootBones;
			maleMorphBank3.Init();
			DAZMorphBank dAZMorphBank6 = maleMorphBank3;
			dAZMorphBank6.onMorphFavoriteChangedHandlers = (DAZMorphBank.MorphFavoriteChanged)Delegate.Combine(dAZMorphBank6.onMorphFavoriteChangedHandlers, new DAZMorphBank.MorphFavoriteChanged(MorphFavoriteChanged));
			if (morphsControlMaleUI != null)
			{
				morphsControlMaleUI.morphBank3 = maleMorphBank3;
			}
			if (morphsControlMaleUIAlt != null)
			{
				morphsControlMaleUIAlt.morphBank3 = maleMorphBank3;
			}
		}
		ResyncMorphRegistry();
	}

	protected void SyncOnlyShowFavoriteMorphsInParameterLists(bool b)
	{
		_onlyShowFavoriteMorphsInParameterLists = b;
		ResyncMorphRegistry();
	}

	protected void MorphFavoriteChanged(DAZMorph dm)
	{
		if (_onlyShowFavoriteMorphsInParameterLists)
		{
			ResyncMorphRegistry();
		}
	}

	protected void ResyncMorphRegistry()
	{
		if (registeredMorphNames == null)
		{
			registeredMorphNames = new HashSet<string>();
		}
		MassDeregister(registeredMorphNames);
		registeredMorphNames.Clear();
		if (_gender == Gender.Female)
		{
			List<DAZMorph> morphs = morphsControlFemaleUI.GetMorphs();
			if (morphs != null)
			{
				foreach (DAZMorph item9 in morphs)
				{
					if (item9.activeImmediate)
					{
						string resolvedDisplayName = item9.resolvedDisplayName;
						string item = "morph: " + resolvedDisplayName;
						if (!registeredMorphNames.Contains(item))
						{
							item9.jsonFloat.name = item;
							item9.jsonFloat.altName = resolvedDisplayName;
							item9.jsonFloat.hidden = _onlyShowFavoriteMorphsInParameterLists && !item9.favorite;
							RegisterFloat(item9.jsonFloat);
							registeredMorphNames.Add(item);
						}
					}
				}
				foreach (DAZMorph item10 in morphs)
				{
					if (!item10.activeImmediate)
					{
						string resolvedDisplayName2 = item10.resolvedDisplayName;
						string item2 = "morph: " + resolvedDisplayName2;
						if (!registeredMorphNames.Contains(item2))
						{
							item10.jsonFloat.name = item2;
							item10.jsonFloat.altName = resolvedDisplayName2;
							item10.jsonFloat.hidden = _onlyShowFavoriteMorphsInParameterLists && !item10.favorite;
							RegisterFloat(item10.jsonFloat);
							registeredMorphNames.Add(item2);
						}
					}
				}
			}
			List<DAZMorph> morphs2 = morphsControlMaleUI.GetMorphs();
			if (morphs2 == null)
			{
				return;
			}
			foreach (DAZMorph item11 in morphs2)
			{
				if (item11.activeImmediate)
				{
					string resolvedDisplayName3 = item11.resolvedDisplayName;
					string item3 = "morphOtherGender: " + resolvedDisplayName3;
					if (!registeredMorphNames.Contains(item3))
					{
						item11.jsonFloat.name = item3;
						item11.jsonFloat.altName = resolvedDisplayName3;
						item11.jsonFloat.hidden = _onlyShowFavoriteMorphsInParameterLists && !item11.favorite;
						RegisterFloat(item11.jsonFloat);
						registeredMorphNames.Add(item3);
					}
				}
			}
			{
				foreach (DAZMorph item12 in morphs2)
				{
					if (!item12.activeImmediate)
					{
						string resolvedDisplayName4 = item12.resolvedDisplayName;
						string item4 = "morphOtherGender: " + resolvedDisplayName4;
						if (!registeredMorphNames.Contains(item4))
						{
							item12.jsonFloat.name = item4;
							item12.jsonFloat.altName = resolvedDisplayName4;
							item12.jsonFloat.hidden = _onlyShowFavoriteMorphsInParameterLists && !item12.favorite;
							RegisterFloat(item12.jsonFloat);
							registeredMorphNames.Add(item4);
						}
					}
				}
				return;
			}
		}
		if (_gender != Gender.Male)
		{
			return;
		}
		List<DAZMorph> morphs3 = morphsControlMaleUI.GetMorphs();
		if (morphs3 != null)
		{
			foreach (DAZMorph item13 in morphs3)
			{
				if (item13.activeImmediate)
				{
					string resolvedDisplayName5 = item13.resolvedDisplayName;
					string item5 = "morph: " + resolvedDisplayName5;
					if (!registeredMorphNames.Contains(item5))
					{
						item13.jsonFloat.name = item5;
						item13.jsonFloat.altName = resolvedDisplayName5;
						item13.jsonFloat.hidden = _onlyShowFavoriteMorphsInParameterLists && !item13.favorite;
						RegisterFloat(item13.jsonFloat);
						registeredMorphNames.Add(item5);
					}
				}
			}
			foreach (DAZMorph item14 in morphs3)
			{
				if (!item14.activeImmediate)
				{
					string resolvedDisplayName6 = item14.resolvedDisplayName;
					string item6 = "morph: " + resolvedDisplayName6;
					if (!registeredMorphNames.Contains(item6))
					{
						item14.jsonFloat.name = item6;
						item14.jsonFloat.altName = resolvedDisplayName6;
						item14.jsonFloat.hidden = _onlyShowFavoriteMorphsInParameterLists && !item14.favorite;
						RegisterFloat(item14.jsonFloat);
						registeredMorphNames.Add(item6);
					}
				}
			}
		}
		List<DAZMorph> morphs4 = morphsControlFemaleUI.GetMorphs();
		if (morphs4 == null)
		{
			return;
		}
		foreach (DAZMorph item15 in morphs4)
		{
			if (item15.activeImmediate)
			{
				string resolvedDisplayName7 = item15.resolvedDisplayName;
				string item7 = "morphOtherGender: " + resolvedDisplayName7;
				if (!registeredMorphNames.Contains(item7))
				{
					item15.jsonFloat.name = item7;
					item15.jsonFloat.altName = resolvedDisplayName7;
					item15.jsonFloat.hidden = _onlyShowFavoriteMorphsInParameterLists && !item15.favorite;
					RegisterFloat(item15.jsonFloat);
					registeredMorphNames.Add(item7);
				}
			}
		}
		foreach (DAZMorph item16 in morphs4)
		{
			if (!item16.activeImmediate)
			{
				string resolvedDisplayName8 = item16.resolvedDisplayName;
				string item8 = "morphOtherGender: " + resolvedDisplayName8;
				if (!registeredMorphNames.Contains(item8))
				{
					item16.jsonFloat.name = item8;
					item16.jsonFloat.altName = resolvedDisplayName8;
					item16.jsonFloat.hidden = _onlyShowFavoriteMorphsInParameterLists && !item16.favorite;
					RegisterFloat(item16.jsonFloat);
					registeredMorphNames.Add(item8);
				}
			}
		}
	}

	protected void ResetMorphBanks()
	{
		if (_characterRun == null)
		{
			if (morphBank1 != null)
			{
				morphBank1.ResetMorphs();
			}
			if (morphBank2 != null)
			{
				morphBank2.ResetMorphs();
			}
			if (morphBank3 != null)
			{
				morphBank3.ResetMorphs();
			}
		}
	}

	protected void ResetMorphsToDefault(bool resetPhysical, bool resetAppearance)
	{
		Init();
		if (_characterRun != null)
		{
			_characterRun.WaitForRunTask();
		}
		if (morphsControlUI != null)
		{
			List<DAZMorph> morphs = morphsControlUI.GetMorphs();
			if (morphs != null)
			{
				foreach (DAZMorph item in morphs)
				{
					bool isPoseControl = item.isPoseControl;
					if ((resetPhysical && isPoseControl) || (resetAppearance && !isPoseControl))
					{
						item.Reset();
					}
				}
			}
		}
		ResetMorphBanks();
	}

	public void ResetMorphsOtherGender(bool resetPhysical, bool resetAppearance)
	{
		if (((_gender != Gender.Female || !_useMaleMorphsOnFemale) && (_gender != Gender.Male || !_useFemaleMorphsOnMale)) || !(morphsControlUIOtherGender != null))
		{
			return;
		}
		List<DAZMorph> morphs = morphsControlUIOtherGender.GetMorphs();
		if (morphs != null)
		{
			foreach (DAZMorph item in morphs)
			{
				bool isPoseControl = item.isPoseControl;
				if ((resetPhysical && isPoseControl) || (resetAppearance && !isPoseControl))
				{
					item.Reset();
				}
			}
		}
		if (_characterRun == null && morphBank1OtherGender != null)
		{
			morphBank1OtherGender.ResetMorphs();
		}
	}

	protected void SyncUseOtherGenderMorphs()
	{
		if (_characterRun != null)
		{
			_characterRun.useOtherGenderMorphs = (_gender == Gender.Female && _useMaleMorphsOnFemale) || (_gender == Gender.Male && _useFemaleMorphsOnMale);
			_characterRun.ResetMorphs();
		}
	}

	protected void SyncUseMaleMorphsOnFemale(bool b)
	{
		_useMaleMorphsOnFemale = b;
		if (_gender == Gender.Female && morphBank1OtherGender != null)
		{
			morphBank1OtherGender.ResetMorphs();
		}
		SyncUseOtherGenderMorphs();
	}

	protected void SyncUseFemaleMorphsOnMale(bool b)
	{
		_useFemaleMorphsOnMale = b;
		if (_gender == Gender.Male && morphBank1OtherGender != null)
		{
			morphBank1OtherGender.ResetMorphs();
		}
		SyncUseOtherGenderMorphs();
	}

	protected void ResyncMorphs(ResyncMorphsOption resyncOption = ResyncMorphsOption.All)
	{
		if (resyncOption == ResyncMorphsOption.All || resyncOption == ResyncMorphsOption.CurrentGender)
		{
			if (morphsControlUI != null)
			{
				morphsControlUI.ResyncMorphs();
				morphsControlUI.ForceCategoryRefresh();
			}
			if (morphsControlUIAlt != null)
			{
				morphsControlUIAlt.ResyncMorphs();
				morphsControlUIAlt.ForceCategoryRefresh();
			}
		}
		if ((resyncOption == ResyncMorphsOption.All || resyncOption == ResyncMorphsOption.OtherGender) && morphsControlUIOtherGender != null)
		{
			morphsControlUIOtherGender.ResyncMorphs();
			morphsControlUIOtherGender.ForceCategoryRefresh();
		}
	}

	public bool RefreshPackageMorphs()
	{
		bool flag = false;
		if (femaleMorphBank1 != null && femaleMorphBank1.RefreshPackageMorphs())
		{
			flag = true;
		}
		if (femaleMorphBank2 != null && femaleMorphBank2.RefreshPackageMorphs())
		{
			flag = true;
		}
		if (femaleMorphBank3 != null && femaleMorphBank3.RefreshPackageMorphs())
		{
			flag = true;
		}
		if (maleMorphBank1 != null && maleMorphBank1.RefreshPackageMorphs())
		{
			flag = true;
		}
		if (maleMorphBank2 != null && maleMorphBank2.RefreshPackageMorphs())
		{
			flag = true;
		}
		if (maleMorphBank3 != null && maleMorphBank3.RefreshPackageMorphs())
		{
			flag = true;
		}
		if (flag)
		{
			ResyncMorphs();
			ResyncMorphRegistry();
		}
		return flag;
	}

	public bool RefreshRuntimeMorphs()
	{
		bool flag = false;
		if (femaleMorphBank1 != null && femaleMorphBank1.RefreshRuntimeMorphs())
		{
			flag = true;
		}
		if (femaleMorphBank2 != null && femaleMorphBank2.RefreshRuntimeMorphs())
		{
			flag = true;
		}
		if (femaleMorphBank3 != null && femaleMorphBank3.RefreshRuntimeMorphs())
		{
			flag = true;
		}
		if (maleMorphBank1 != null && maleMorphBank1.RefreshRuntimeMorphs())
		{
			flag = true;
		}
		if (maleMorphBank2 != null && maleMorphBank2.RefreshRuntimeMorphs())
		{
			flag = true;
		}
		if (maleMorphBank3 != null && maleMorphBank3.RefreshRuntimeMorphs())
		{
			flag = true;
		}
		if (flag)
		{
			ResyncMorphs();
			ResyncMorphRegistry();
		}
		return flag;
	}

	public bool CleanDemandActivatedMorphs()
	{
		bool flag = false;
		if (morphsControlUI != null && morphsControlUI.CleanDemandActivatedMorphs())
		{
			flag = true;
			morphsControlUI.ResyncMorphs();
			morphsControlUI.ForceCategoryRefresh();
			if (morphsControlUIAlt != null)
			{
				morphsControlUIAlt.ResyncMorphs();
				morphsControlUIAlt.ForceCategoryRefresh();
			}
		}
		if (morphsControlUIOtherGender != null && morphsControlUIOtherGender.CleanDemandActivatedMorphs())
		{
			flag = true;
			morphsControlUIOtherGender.ResyncMorphs();
			morphsControlUIOtherGender.ForceCategoryRefresh();
		}
		if (flag)
		{
			ResyncMorphRegistry();
		}
		return flag;
	}

	public int GetRuntimeMorphDeltasLoadedCount()
	{
		int num = 0;
		if (femaleMorphBank1 != null)
		{
			num += femaleMorphBank1.GetRuntimeMorphDeltasLoadedCount();
		}
		if (femaleMorphBank2 != null)
		{
			num += femaleMorphBank2.GetRuntimeMorphDeltasLoadedCount();
		}
		if (femaleMorphBank3 != null)
		{
			num += femaleMorphBank3.GetRuntimeMorphDeltasLoadedCount();
		}
		if (maleMorphBank1 != null)
		{
			num += maleMorphBank1.GetRuntimeMorphDeltasLoadedCount();
		}
		if (maleMorphBank2 != null)
		{
			num += maleMorphBank2.GetRuntimeMorphDeltasLoadedCount();
		}
		if (maleMorphBank3 != null)
		{
			num += maleMorphBank3.GetRuntimeMorphDeltasLoadedCount();
		}
		return num;
	}

	public void UnloadRuntimeMorphDeltas()
	{
		if (femaleMorphBank1 != null)
		{
			femaleMorphBank1.UnloadRuntimeMorphDeltas();
		}
		if (femaleMorphBank2 != null)
		{
			femaleMorphBank2.UnloadRuntimeMorphDeltas();
		}
		if (femaleMorphBank3 != null)
		{
			femaleMorphBank3.UnloadRuntimeMorphDeltas();
		}
		if (maleMorphBank1 != null)
		{
			maleMorphBank1.UnloadRuntimeMorphDeltas();
		}
		if (maleMorphBank2 != null)
		{
			maleMorphBank2.UnloadRuntimeMorphDeltas();
		}
		if (maleMorphBank3 != null)
		{
			maleMorphBank3.UnloadRuntimeMorphDeltas();
		}
	}

	public void UnloadDemandActivatedMorphs()
	{
		bool flag = false;
		if (femaleMorphBank1 != null && femaleMorphBank1.UnloadDemandActivatedMorphs())
		{
			flag = true;
		}
		if (femaleMorphBank2 != null && femaleMorphBank2.UnloadDemandActivatedMorphs())
		{
			flag = true;
		}
		if (femaleMorphBank3 != null && femaleMorphBank3.UnloadDemandActivatedMorphs())
		{
			flag = true;
		}
		if (maleMorphBank1 != null && maleMorphBank1.UnloadDemandActivatedMorphs())
		{
			flag = true;
		}
		if (maleMorphBank2 != null && maleMorphBank2.UnloadDemandActivatedMorphs())
		{
			flag = true;
		}
		if (maleMorphBank3 != null && maleMorphBank3.UnloadDemandActivatedMorphs())
		{
			flag = true;
		}
		if (flag)
		{
			CleanDemandActivatedMorphs();
			if (_characterRun != null)
			{
				_characterRun.ResetMorphs();
			}
		}
	}

	protected void SyncCharacterChoice(string choice)
	{
		SelectCharacterByName(choice);
	}

	public void UnloadInactiveCharacters()
	{
		if (_femaleCharacters != null)
		{
			DAZCharacter[] array = _femaleCharacters;
			foreach (DAZCharacter dAZCharacter in array)
			{
				dAZCharacter.UnloadIfInactive();
			}
		}
		if (_maleCharacters != null)
		{
			DAZCharacter[] array2 = _maleCharacters;
			foreach (DAZCharacter dAZCharacter2 in array2)
			{
				dAZCharacter2.UnloadIfInactive();
			}
		}
		ValidateDynamicClothesSkin();
		ValidateDynamicHairSkin();
	}

	public void UnloadDisabledCharacters()
	{
		if (_femaleCharacters != null)
		{
			DAZCharacter[] array = _femaleCharacters;
			foreach (DAZCharacter dAZCharacter in array)
			{
				dAZCharacter.UnloadIfNotEnabled();
			}
		}
		if (_maleCharacters != null)
		{
			DAZCharacter[] array2 = _maleCharacters;
			foreach (DAZCharacter dAZCharacter2 in array2)
			{
				dAZCharacter2.UnloadIfNotEnabled();
			}
		}
		ValidateDynamicClothesSkin();
		ValidateDynamicHairSkin();
	}

	protected void SyncUnloadCharactersWhenSwitching()
	{
		DAZCharacter[] array = _femaleCharacters;
		foreach (DAZCharacter dAZCharacter in array)
		{
			dAZCharacter.unloadOnDisable = _unloadCharactersWhenSwitching;
		}
		DAZCharacter[] array2 = _maleCharacters;
		foreach (DAZCharacter dAZCharacter2 in array2)
		{
			dAZCharacter2.unloadOnDisable = _unloadCharactersWhenSwitching;
		}
	}

	protected void SyncUnloadCharactersWhenSwitching(bool b)
	{
		_unloadCharactersWhenSwitching = b;
		SyncUnloadCharactersWhenSwitching();
	}

	protected void EarlyInitCharacters()
	{
		if (Application.isPlaying)
		{
			if (femaleCharactersPrefab != null && femaleCharactersContainer != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(femaleCharactersPrefab.gameObject);
				gameObject.transform.SetParent(femaleCharactersContainer);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.one;
			}
			if (maleCharactersPrefab != null && maleCharactersContainer != null)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(maleCharactersPrefab.gameObject);
				gameObject2.transform.SetParent(maleCharactersContainer);
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject2.transform.localRotation = Quaternion.identity;
				gameObject2.transform.localScale = Vector3.one;
			}
			if (femalePlaceholderCharacter != null && !femalePlaceholderCharacter.gameObject.activeSelf)
			{
				femalePlaceholderCharacter.gameObject.SetActive(true);
				femalePlaceholderCharacter.gameObject.SetActive(false);
			}
			if (malePlaceholderCharacter != null && !malePlaceholderCharacter.gameObject.activeSelf)
			{
				malePlaceholderCharacter.gameObject.SetActive(true);
				malePlaceholderCharacter.gameObject.SetActive(false);
			}
		}
	}

	public void InitCharacters()
	{
		if (characterChooserJSON != null)
		{
			DeregisterStringChooser(characterChooserJSON);
		}
		if (femaleCharactersContainer != null)
		{
			_femaleCharacters = femaleCharactersContainer.GetComponentsInChildren<DAZCharacter>(true);
		}
		else
		{
			_femaleCharacters = new DAZCharacter[0];
		}
		if (maleCharactersContainer != null)
		{
			_maleCharacters = maleCharactersContainer.GetComponentsInChildren<DAZCharacter>(true);
		}
		else
		{
			_maleCharacters = new DAZCharacter[0];
		}
		SyncUnloadCharactersWhenSwitching();
		_characterByName = new Dictionary<string, DAZCharacter>();
		_characters = new DAZCharacter[_femaleCharacters.Length + _maleCharacters.Length];
		int num = 0;
		for (int i = 0; i < _femaleCharacters.Length; i++)
		{
			_femaleCharacters[i].containingAtom = containingAtom;
			_femaleCharacters[i].rootBonesForSkinning = rootBones;
			_characters[num] = _femaleCharacters[i];
			num++;
		}
		for (int j = 0; j < _maleCharacters.Length; j++)
		{
			_maleCharacters[j].containingAtom = containingAtom;
			_maleCharacters[j].rootBonesForSkinning = rootBones;
			_characters[num] = _maleCharacters[j];
			num++;
		}
		List<string> list = new List<string>();
		string startingValue = string.Empty;
		DAZCharacter[] array = _characters;
		foreach (DAZCharacter dAZCharacter in array)
		{
			if (!(dAZCharacter != null))
			{
				continue;
			}
			if (_characterByName.ContainsKey(dAZCharacter.displayName))
			{
				Debug.LogError("Character " + dAZCharacter.displayName + " is a duplicate. Cannot add");
				continue;
			}
			list.Add(dAZCharacter.displayName);
			_characterByName.Add(dAZCharacter.displayName, dAZCharacter);
			if (dAZCharacter.gameObject.activeSelf)
			{
				startingValue = dAZCharacter.displayName;
				_selectedCharacter = dAZCharacter;
			}
		}
		if (Application.isPlaying)
		{
			characterChooserJSON = new JSONStorableStringChooser("characterSelection", list, startingValue, "Character Selection", SyncCharacterChoice);
			characterChooserJSON.isRestorable = false;
			characterChooserJSON.isStorable = false;
			RegisterStringChooser(characterChooserJSON);
		}
	}

	public void SelectCharacterByName(string characterName, bool fromRestore = false)
	{
		if (_characterByName == null)
		{
			Init();
		}
		DAZCharacter value;
		if (_characterByName.TryGetValue(characterName, out value))
		{
			if (fromRestore)
			{
				value.needsPostLoadJSONRestore = true;
			}
			selectedCharacter = value;
		}
		if (characterChooserJSON != null)
		{
			characterChooserJSON.valNoCallback = characterName;
		}
	}

	protected void ConnectSkin()
	{
		DAZSkinV2 skin = _selectedCharacter.skin;
		DAZSkinV2 skinForClothes = _selectedCharacter.skinForClothes;
		if (!(skin != null))
		{
			return;
		}
		skin.Init();
		skin.ResetPostSkinMorphs();
		if (_setDAZMorphs != null)
		{
			SetDAZMorph[] setDAZMorphs = _setDAZMorphs;
			foreach (SetDAZMorph setDAZMorph in setDAZMorphs)
			{
				if (morphBank1 != null)
				{
					setDAZMorph.morphBank = morphBank1;
				}
				if (morphBank2 != null)
				{
					setDAZMorph.morphBankAlt = morphBank2;
				}
				if (morphBank3 != null)
				{
					setDAZMorph.morphBankAlt2 = morphBank3;
				}
			}
		}
		if (lipSync != null && morphBank1 != null)
		{
			lipSync.morphBank = morphBank1;
		}
		if (_physicsMeshes != null)
		{
			DAZPhysicsMesh[] physicsMeshes = _physicsMeshes;
			foreach (DAZPhysicsMesh dAZPhysicsMesh in physicsMeshes)
			{
				if (dAZPhysicsMesh != null && dAZPhysicsMesh.isEnabled)
				{
					dAZPhysicsMesh.Init();
					dAZPhysicsMesh.skinTransform = skin.transform;
					dAZPhysicsMesh.skin = skin;
				}
			}
		}
		if (_characterRun != null)
		{
			_characterRun.morphBank1 = morphBank1;
			_characterRun.morphBank1OtherGender = morphBank1OtherGender;
			if (_loadedGenderChange)
			{
				SyncUseOtherGenderMorphs();
			}
			_characterRun.morphBank2 = morphBank2;
			_characterRun.morphBank3 = morphBank3;
			_characterRun.skin = (DAZMergedSkinV2)skin;
			_characterRun.bones = rootBones;
			_characterRun.autoColliderUpdaters = _autoColliderBatchUpdaters;
			_characterRun.Connect(_loadedGenderChange);
		}
		if (_eyelidControl != null)
		{
			_eyelidControl.morphBank = morphBank1;
		}
		if (_autoColliderBatchUpdaters != null)
		{
			AutoColliderBatchUpdater[] autoColliderBatchUpdaters = _autoColliderBatchUpdaters;
			foreach (AutoColliderBatchUpdater autoColliderBatchUpdater in autoColliderBatchUpdaters)
			{
				autoColliderBatchUpdater.skin = skin;
			}
		}
		if (_autoColliders != null)
		{
			AutoCollider[] autoColliders = _autoColliders;
			foreach (AutoCollider autoCollider in autoColliders)
			{
				if (autoCollider != null)
				{
					autoCollider.skinTransform = skin.transform;
					autoCollider.skin = skin;
				}
			}
		}
		if (_setAnchorFromVertexComps != null)
		{
			SetAnchorFromVertex[] setAnchorFromVertexComps = _setAnchorFromVertexComps;
			foreach (SetAnchorFromVertex setAnchorFromVertex in setAnchorFromVertexComps)
			{
				if (setAnchorFromVertex != null)
				{
					setAnchorFromVertex.skinTransform = skin.transform;
					setAnchorFromVertex.skin = skin;
				}
			}
		}
		if (gender == Gender.Male)
		{
			if (maleEyelashMaterialOptions != null)
			{
				maleEyelashMaterialOptions.skin = skin;
			}
		}
		else if (femaleEyelashMaterialOptions != null)
		{
			femaleEyelashMaterialOptions.skin = skin;
		}
		DAZCharacterMaterialOptions[] materialOptions = _materialOptions;
		foreach (DAZCharacterMaterialOptions dAZCharacterMaterialOptions in materialOptions)
		{
			if (dAZCharacterMaterialOptions != null)
			{
				dAZCharacterMaterialOptions.skin = skin;
				if (dAZCharacterMaterialOptions.isPassthrough)
				{
					dAZCharacterMaterialOptions.ConnectPassthroughBuckets();
				}
			}
		}
		ConnectCharacterMaterialOptionsUI();
		if (clothingItems != null)
		{
			DAZClothingItem[] array = clothingItems;
			foreach (DAZClothingItem dAZClothingItem in array)
			{
				if (dAZClothingItem != null)
				{
					dAZClothingItem.skin = skinForClothes;
				}
			}
		}
		if (hairItems == null)
		{
			return;
		}
		DAZHairGroup[] array2 = hairItems;
		foreach (DAZHairGroup dAZHairGroup in array2)
		{
			if (dAZHairGroup != null)
			{
				dAZHairGroup.skin = skin;
			}
		}
	}

	private IEnumerator DelayResume(AsyncFlag af, int count)
	{
		delayResumeFlag = af;
		for (int i = 0; i < count; i++)
		{
			yield return null;
		}
		af.Raise();
	}

	protected void OnCharacterPreloaded()
	{
		_loadedGenderChange = false;
		if (_loadedCharacter != null && _loadedCharacter != _selectedCharacter)
		{
			_loadedCharacter.gameObject.SetActive(false);
			if (_loadedCharacter.isMale != _selectedCharacter.isMale)
			{
				_loadedGenderChange = true;
			}
		}
		_loadedCharacter = _selectedCharacter;
	}

	protected void OnCharacterLoaded()
	{
		DAZMesh[] componentsInChildren = _selectedCharacter.GetComponentsInChildren<DAZMesh>(true);
		DAZMesh[] array = componentsInChildren;
		foreach (DAZMesh dAZMesh in array)
		{
			if (morphBank1 != null && morphBank1.geometryId == dAZMesh.geometryId)
			{
				morphBank1.connectedMesh = dAZMesh;
				dAZMesh.morphBank = morphBank1;
				if (morphBank1OtherGender != null)
				{
					morphBank1OtherGender.connectedMesh = dAZMesh;
				}
			}
			if (morphBank2 != null && morphBank2.geometryId == dAZMesh.geometryId)
			{
				morphBank2.connectedMesh = dAZMesh;
				dAZMesh.morphBank = morphBank2;
			}
			if (morphBank3 != null && morphBank3.geometryId == dAZMesh.geometryId)
			{
				morphBank3.connectedMesh = dAZMesh;
				dAZMesh.morphBank = morphBank3;
			}
		}
		ConnectSkin();
		SyncAnatomy();
		if (onCharacterLoadedFlag != null)
		{
			StartCoroutine(DelayResume(onCharacterLoadedFlag, 5));
			onCharacterLoadedFlag = null;
		}
		if (_unloadCharactersWhenSwitching)
		{
			StartCoroutine(UnloadUnusedAssetsDelayed());
		}
	}

	protected void ConnectDynamicItem(DAZDynamicItem di)
	{
		di.containingAtom = containingAtom;
		di.UIbucket = UIBucketForDynamicItems;
		switch (di.drawRigidOnBoneType)
		{
		case DAZDynamicItem.BoneType.None:
			di.drawRigidOnBone = null;
			break;
		case DAZDynamicItem.BoneType.Hip:
			di.drawRigidOnBone = hipBone;
			break;
		case DAZDynamicItem.BoneType.Pelvis:
			di.drawRigidOnBone = pelvisBone;
			break;
		case DAZDynamicItem.BoneType.Chest:
			di.drawRigidOnBone = chestBone;
			break;
		case DAZDynamicItem.BoneType.Head:
			di.drawRigidOnBone = headBone;
			break;
		case DAZDynamicItem.BoneType.LeftHand:
			di.drawRigidOnBone = leftHandBone;
			break;
		case DAZDynamicItem.BoneType.RightHand:
			di.drawRigidOnBone = rightHandBone;
			break;
		case DAZDynamicItem.BoneType.LeftFoot:
			di.drawRigidOnBone = leftFootBone;
			break;
		case DAZDynamicItem.BoneType.RightFoot:
			di.drawRigidOnBone = rightFootBone;
			break;
		}
		switch (di.drawRigidOnBoneTypeLeft)
		{
		case DAZDynamicItem.BoneType.None:
			di.drawRigidOnBoneLeft = null;
			break;
		case DAZDynamicItem.BoneType.Hip:
			di.drawRigidOnBoneLeft = hipBone;
			break;
		case DAZDynamicItem.BoneType.Pelvis:
			di.drawRigidOnBoneLeft = pelvisBone;
			break;
		case DAZDynamicItem.BoneType.Chest:
			di.drawRigidOnBoneLeft = chestBone;
			break;
		case DAZDynamicItem.BoneType.Head:
			di.drawRigidOnBoneLeft = headBone;
			break;
		case DAZDynamicItem.BoneType.LeftHand:
			di.drawRigidOnBoneLeft = leftHandBone;
			break;
		case DAZDynamicItem.BoneType.RightHand:
			di.drawRigidOnBoneLeft = rightHandBone;
			break;
		case DAZDynamicItem.BoneType.LeftFoot:
			di.drawRigidOnBoneLeft = leftFootBone;
			break;
		case DAZDynamicItem.BoneType.RightFoot:
			di.drawRigidOnBoneLeft = rightFootBone;
			break;
		}
		switch (di.drawRigidOnBoneTypeRight)
		{
		case DAZDynamicItem.BoneType.None:
			di.drawRigidOnBoneRight = null;
			break;
		case DAZDynamicItem.BoneType.Hip:
			di.drawRigidOnBoneRight = hipBone;
			break;
		case DAZDynamicItem.BoneType.Pelvis:
			di.drawRigidOnBoneRight = pelvisBone;
			break;
		case DAZDynamicItem.BoneType.Chest:
			di.drawRigidOnBoneRight = chestBone;
			break;
		case DAZDynamicItem.BoneType.Head:
			di.drawRigidOnBoneRight = headBone;
			break;
		case DAZDynamicItem.BoneType.LeftHand:
			di.drawRigidOnBoneRight = leftHandBone;
			break;
		case DAZDynamicItem.BoneType.RightHand:
			di.drawRigidOnBoneRight = rightHandBone;
			break;
		case DAZDynamicItem.BoneType.LeftFoot:
			di.drawRigidOnBoneRight = leftFootBone;
			break;
		case DAZDynamicItem.BoneType.RightFoot:
			di.drawRigidOnBoneRight = rightFootBone;
			break;
		}
		switch (di.autoColliderReferenceBoneType)
		{
		case DAZDynamicItem.BoneType.None:
			di.autoColliderReference = null;
			break;
		case DAZDynamicItem.BoneType.Hip:
			if (hipBone == null)
			{
				di.autoColliderReference = null;
			}
			else
			{
				di.autoColliderReference = hipBone.transform;
			}
			break;
		case DAZDynamicItem.BoneType.Pelvis:
			if (pelvisBone == null)
			{
				di.autoColliderReference = null;
			}
			else
			{
				di.autoColliderReference = pelvisBone.transform;
			}
			break;
		case DAZDynamicItem.BoneType.Chest:
			if (chestBone == null)
			{
				di.autoColliderReference = null;
			}
			else
			{
				di.autoColliderReference = chestBone.transform;
			}
			break;
		case DAZDynamicItem.BoneType.Head:
			if (headBone == null)
			{
				di.autoColliderReference = null;
			}
			else
			{
				di.autoColliderReference = headBone.transform;
			}
			break;
		case DAZDynamicItem.BoneType.LeftHand:
			if (leftHandBone == null)
			{
				di.autoColliderReference = null;
			}
			else
			{
				di.autoColliderReference = leftHandBone.transform;
			}
			break;
		case DAZDynamicItem.BoneType.RightHand:
			if (rightHandBone == null)
			{
				di.autoColliderReference = null;
			}
			else
			{
				di.autoColliderReference = rightHandBone.transform;
			}
			break;
		case DAZDynamicItem.BoneType.LeftFoot:
			if (leftFootBone == null)
			{
				di.autoColliderReference = null;
			}
			else
			{
				di.autoColliderReference = leftFootBone.transform;
			}
			break;
		case DAZDynamicItem.BoneType.RightFoot:
			if (rightFootBone == null)
			{
				di.autoColliderReference = null;
			}
			else
			{
				di.autoColliderReference = rightFootBone.transform;
			}
			break;
		}
	}

	protected void SyncCustomItems(DAZDynamicItem.Gender gender, DAZDynamicItem[] existingItems, string searchPath, Transform dynamicItemPrefab, Transform container)
	{
		if (alreadyReportedDuplicates == null)
		{
			alreadyReportedDuplicates = new HashSet<string>();
		}
		Dictionary<string, DAZDynamicItem> dictionary = new Dictionary<string, DAZDynamicItem>();
		foreach (DAZDynamicItem dAZDynamicItem in existingItems)
		{
			if (dAZDynamicItem.type == DAZDynamicItem.Type.Custom && dAZDynamicItem.isDynamicRuntimeLoaded && !dictionary.ContainsKey(dAZDynamicItem.uid))
			{
				dictionary.Add(dAZDynamicItem.uid, dAZDynamicItem);
			}
		}
		List<FileEntry> list = new List<FileEntry>();
		try
		{
			FileManager.FindAllFiles(searchPath, "*.vam", list);
			FileManager.SortFileEntriesByLastWriteTime(list);
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception during refresh of dynamic items " + ex);
		}
		Dictionary<string, bool> dictionary2 = new Dictionary<string, bool>();
		Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
		foreach (FileEntry item in list)
		{
			try
			{
				JSONNode jSONNode = null;
				if (item is VarFileEntry)
				{
					VarFileEntry varFileEntry = item as VarFileEntry;
					jSONNode = varFileEntry.Package.GetJSONCache(varFileEntry.InternalSlashPath);
				}
				if (jSONNode == null)
				{
					string aJSON = FileManager.ReadAllText(item);
					jSONNode = JSON.Parse(aJSON);
				}
				if (!(jSONNode != null))
				{
					continue;
				}
				JSONClass asObject = jSONNode.AsObject;
				string text = string.Empty;
				if (asObject["uid"] != null)
				{
					text = asObject["uid"];
				}
				string empty = string.Empty;
				string empty2 = string.Empty;
				string packageUid = null;
				string packageLicense = null;
				int num = -1;
				bool isLatestVersion = true;
				if (item is VarFileEntry)
				{
					empty = item.Uid;
					VarFileEntry varFileEntry2 = item as VarFileEntry;
					num = varFileEntry2.Package.Version;
					empty2 = varFileEntry2.InternalSlashPath;
					packageUid = varFileEntry2.Package.Uid;
					packageLicense = varFileEntry2.Package.LicenseType;
					isLatestVersion = varFileEntry2.Package.isNewestEnabledVersion;
				}
				else
				{
					empty = item.Uid;
					empty2 = item.SlashPath;
				}
				string value;
				if (dictionary3.TryGetValue(empty, out value))
				{
					if (!alreadyReportedDuplicates.Contains(string.Concat(item, ":", empty, ":", value)))
					{
						SuperController.LogError(string.Concat("Custom item ", item, " uses same UID ", empty, " as item ", value, ". Cannot add"));
						alreadyReportedDuplicates.Add(string.Concat(item, ":", empty, ":", value));
					}
					continue;
				}
				dictionary3.Add(empty, item.Uid);
				string text2 = asObject["displayName"];
				if (text2 == null || text2 == string.Empty)
				{
					text2 = Regex.Replace(text, ".*:", string.Empty);
				}
				string creatorName = "None";
				if (asObject["creatorName"] != null)
				{
					creatorName = asObject["creatorName"];
				}
				if (!(empty != string.Empty) || !(text2 != string.Empty))
				{
					continue;
				}
				DAZDynamicItem value2;
				if (dictionary.TryGetValue(empty, out value2))
				{
					if (!dictionary2.ContainsKey(empty))
					{
						dictionary2.Add(empty, true);
					}
					value2.displayName = text2;
					value2.creatorName = creatorName;
					value2.isLatestVersion = isLatestVersion;
					value2.packageUid = packageUid;
					value2.packageLicense = packageLicense;
					if (num != -1)
					{
						value2.version = "v" + num;
					}
					else
					{
						value2.version = string.Empty;
					}
					if (asObject["tags"] != null)
					{
						value2.tags = asObject["tags"];
					}
					if (asObject["isRealItem"] != null)
					{
						value2.isRealItem = asObject["isRealItem"].AsBool;
					}
					continue;
				}
				Transform transform = UnityEngine.Object.Instantiate(dynamicItemPrefab);
				transform.name = transform.name.Replace("(Clone)", string.Empty);
				transform.gameObject.SetActive(false);
				transform.parent = container;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;
				value2 = transform.GetComponent<DAZDynamicItem>();
				if (!(value2 != null))
				{
					continue;
				}
				value2.containingAtom = containingAtom;
				value2.UIbucket = customUIBucket;
				value2.gender = gender;
				value2.uid = empty;
				value2.backupId = empty2;
				value2.internalUid = text;
				value2.displayName = text2;
				value2.creatorName = creatorName;
				value2.packageUid = packageUid;
				value2.packageLicense = packageLicense;
				value2.isLatestVersion = isLatestVersion;
				if (num != -1)
				{
					value2.version = "v" + num;
				}
				else
				{
					value2.version = string.Empty;
				}
				if (asObject["tags"] != null)
				{
					value2.tags = asObject["tags"];
				}
				if (asObject["isRealItem"] != null)
				{
					value2.isRealItem = asObject["isRealItem"].AsBool;
				}
				if (_selectedCharacter != null)
				{
					if (value2.gender == DAZDynamicItem.Gender.Male && !_selectedCharacter.isMale)
					{
						if (malePlaceholderCharacter != null)
						{
							value2.skin = malePlaceholderCharacter.skin;
						}
					}
					else if (value2.gender == DAZDynamicItem.Gender.Female && _selectedCharacter.isMale)
					{
						if (femalePlaceholderCharacter != null)
						{
							value2.skin = femalePlaceholderCharacter.skin;
						}
					}
					else
					{
						value2.skin = _selectedCharacter.skin;
					}
				}
				value2.isDynamicRuntimeLoaded = true;
				value2.dynamicRuntimeLoadPath = item.Uid;
			}
			catch (Exception ex2)
			{
				SuperController.LogError(string.Concat("Exception while reading dynamic item metafile ", item, " ", ex2));
			}
		}
		foreach (DAZDynamicItem value3 in dictionary.Values)
		{
			if (!dictionary2.ContainsKey(value3.uid))
			{
				value3.transform.SetParent(null);
				UnityEngine.Object.Destroy(value3.gameObject);
			}
		}
	}

	public void SetActiveDynamicItem(DAZDynamicItem item, bool active, bool fromRestore = false)
	{
		if (item is DAZClothingItem)
		{
			SetActiveClothingItem(item as DAZClothingItem, active, fromRestore);
		}
		else if (item is DAZHairGroup)
		{
			SetActiveHairItem(item as DAZHairGroup, active, fromRestore);
		}
	}

	public void LoadDynamicCreatorItem(DAZDynamicItem item, DAZDynamic dd)
	{
		if (item.type != DAZDynamicItem.Type.Custom)
		{
			return;
		}
		SetActiveDynamicItem(item, false);
		if (item.gender == DAZDynamicItem.Gender.Female)
		{
			if (item is DAZClothingItem)
			{
				LoadFemaleClothingCreatorItem(dd);
			}
			else if (item is DAZHairGroup)
			{
				LoadFemaleHairCreatorItem(dd);
			}
		}
		else if (item.gender == DAZDynamicItem.Gender.Male)
		{
			if (item is DAZClothingItem)
			{
				LoadMaleClothingCreatorItem(dd);
			}
			else if (item is DAZHairGroup)
			{
				LoadMaleHairCreatorItem(dd);
			}
		}
	}

	protected IEnumerator RefreshWhenHubClosed()
	{
		while (SuperController.singleton.HubOpen || SuperController.singleton.activeUI == SuperController.ActiveUI.PackageDownloader)
		{
			yield return null;
		}
		AsyncFlag af = new AsyncFlag("Clothing and Hair Refresh");
		SuperController.singleton.SetLoadingIconFlag(af);
		yield return null;
		float startt = GlobalStopwatch.GetElapsedMilliseconds();
		RefreshDynamicClothes();
		RefreshDynamicHair();
		float stopt = GlobalStopwatch.GetElapsedMilliseconds();
		Debug.Log("Deferred Person refresh clothing and hair took " + (stopt - startt).ToString("F1") + " ms");
		refreshCoroutine = null;
		af.Raise();
	}

	public void RefreshDynamicItems()
	{
		if (SuperController.singleton.HubOpen || SuperController.singleton.activeUI == SuperController.ActiveUI.PackageDownloader)
		{
			if (refreshCoroutine == null)
			{
				refreshCoroutine = SuperController.singleton.StartCoroutine(RefreshWhenHubClosed());
			}
		}
		else
		{
			float elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
			RefreshDynamicClothes();
			RefreshDynamicHair();
			float elapsedMilliseconds2 = GlobalStopwatch.GetElapsedMilliseconds();
			Debug.Log("Person refresh clothing and hair took " + (elapsedMilliseconds2 - elapsedMilliseconds).ToString("F1") + " ms");
		}
	}

	protected void ResetClothing(bool clearAll = false)
	{
		Init();
		DAZClothingItem[] array = _femaleClothingItems;
		foreach (DAZClothingItem dAZClothingItem in array)
		{
			if (clearAll)
			{
				SetActiveClothingItem(dAZClothingItem, false);
			}
			else
			{
				SetActiveClothingItem(dAZClothingItem, dAZClothingItem.startActive);
			}
		}
		DAZClothingItem[] array2 = _maleClothingItems;
		foreach (DAZClothingItem dAZClothingItem2 in array2)
		{
			if (clearAll)
			{
				SetActiveClothingItem(dAZClothingItem2, false);
			}
			else
			{
				SetActiveClothingItem(dAZClothingItem2, dAZClothingItem2.startActive);
			}
		}
	}

	protected void SyncClothingItem(JSONStorableBool clothingItemJSON)
	{
		string altName = clothingItemJSON.altName;
		SetActiveClothingItem(altName, clothingItemJSON.val);
	}

	public bool IsClothingUIDAvailable(string uid)
	{
		bool result = true;
		if (_clothingItemById != null && _clothingItemById.ContainsKey(uid))
		{
			return false;
		}
		return result;
	}

	protected void ConnectClothingItem(DAZClothingItem dci)
	{
		switch (dci.colliderTypeLeft)
		{
		case DAZClothingItem.ColliderType.None:
			dci.colliderLeft = null;
			break;
		case DAZClothingItem.ColliderType.Shoe:
			dci.colliderLeft = leftShoeCollider;
			break;
		}
		switch (dci.colliderTypeRight)
		{
		case DAZClothingItem.ColliderType.None:
			dci.colliderRight = null;
			break;
		case DAZClothingItem.ColliderType.Shoe:
			dci.colliderRight = rightShoeCollider;
			break;
		}
		switch (dci.driveXAngleTargetController1Type)
		{
		case DAZClothingItem.ControllerType.None:
			dci.driveXAngleTargetController1 = null;
			break;
		case DAZClothingItem.ControllerType.LeftFoot:
			dci.driveXAngleTargetController1 = leftFootController;
			break;
		case DAZClothingItem.ControllerType.RightFoot:
			dci.driveXAngleTargetController1 = rightFootController;
			break;
		case DAZClothingItem.ControllerType.LeftToe:
			dci.driveXAngleTargetController1 = leftToeController;
			break;
		case DAZClothingItem.ControllerType.RightToe:
			dci.driveXAngleTargetController1 = rightToeController;
			break;
		}
		switch (dci.driveXAngleTargetController2Type)
		{
		case DAZClothingItem.ControllerType.None:
			dci.driveXAngleTargetController2 = null;
			break;
		case DAZClothingItem.ControllerType.LeftFoot:
			dci.driveXAngleTargetController2 = leftFootController;
			break;
		case DAZClothingItem.ControllerType.RightFoot:
			dci.driveXAngleTargetController2 = rightFootController;
			break;
		case DAZClothingItem.ControllerType.LeftToe:
			dci.driveXAngleTargetController2 = leftToeController;
			break;
		case DAZClothingItem.ControllerType.RightToe:
			dci.driveXAngleTargetController2 = rightToeController;
			break;
		}
		switch (dci.drive2XAngleTargetController1Type)
		{
		case DAZClothingItem.ControllerType.None:
			dci.drive2XAngleTargetController1 = null;
			break;
		case DAZClothingItem.ControllerType.LeftFoot:
			dci.drive2XAngleTargetController1 = leftFootController;
			break;
		case DAZClothingItem.ControllerType.RightFoot:
			dci.drive2XAngleTargetController1 = rightFootController;
			break;
		case DAZClothingItem.ControllerType.LeftToe:
			dci.drive2XAngleTargetController1 = leftToeController;
			break;
		case DAZClothingItem.ControllerType.RightToe:
			dci.drive2XAngleTargetController1 = rightToeController;
			break;
		}
		switch (dci.drive2XAngleTargetController2Type)
		{
		case DAZClothingItem.ControllerType.None:
			dci.drive2XAngleTargetController2 = null;
			break;
		case DAZClothingItem.ControllerType.LeftFoot:
			dci.drive2XAngleTargetController2 = leftFootController;
			break;
		case DAZClothingItem.ControllerType.RightFoot:
			dci.drive2XAngleTargetController2 = rightFootController;
			break;
		case DAZClothingItem.ControllerType.LeftToe:
			dci.drive2XAngleTargetController2 = leftToeController;
			break;
		case DAZClothingItem.ControllerType.RightToe:
			dci.drive2XAngleTargetController2 = rightToeController;
			break;
		}
	}

	protected void EarlyInitClothingItems()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (femaleClothingPrefab != null && femaleClothingContainer != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(femaleClothingPrefab.gameObject);
			gameObject.transform.SetParent(femaleClothingContainer);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			DAZClothingItem[] componentsInChildren = gameObject.GetComponentsInChildren<DAZClothingItem>(true);
			DAZClothingItem[] array = componentsInChildren;
			foreach (DAZClothingItem dAZClothingItem in array)
			{
				ConnectDynamicItem(dAZClothingItem);
				ConnectClothingItem(dAZClothingItem);
			}
		}
		if (maleClothingPrefab != null && maleClothingContainer != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(maleClothingPrefab.gameObject);
			gameObject2.transform.SetParent(maleClothingContainer);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = Vector3.one;
			DAZClothingItem[] componentsInChildren2 = gameObject2.GetComponentsInChildren<DAZClothingItem>(true);
			DAZClothingItem[] array2 = componentsInChildren2;
			foreach (DAZClothingItem dAZClothingItem2 in array2)
			{
				ConnectDynamicItem(dAZClothingItem2);
				ConnectClothingItem(dAZClothingItem2);
			}
		}
	}

	public void InvalidateDynamicClothingItemThumbnail(string thumbPath)
	{
		string text = thumbPath.Replace(".jpg", ".vam");
		DAZClothingItem[] array = clothingItems;
		foreach (DAZClothingItem dAZClothingItem in array)
		{
			if (dAZClothingItem.dynamicRuntimeLoadPath == text)
			{
				dAZClothingItem.thumbnail = null;
			}
		}
	}

	public void RefreshDynamicClothingThumbnails()
	{
		if (gender == Gender.Female)
		{
			if (clothingSelectorFemaleUI != null)
			{
				clothingSelectorFemaleUI.RefreshThumbnails();
			}
		}
		else if (gender == Gender.Male && clothingSelectorMaleUI != null)
		{
			clothingSelectorMaleUI.RefreshThumbnails();
		}
	}

	protected void ValidateDynamicClothesSkin()
	{
		if (!(_selectedCharacter != null))
		{
			return;
		}
		DAZClothingItem[] componentsInChildren = femaleClothingContainer.GetComponentsInChildren<DAZClothingItem>(true);
		DAZClothingItem[] array = componentsInChildren;
		foreach (DAZClothingItem dAZClothingItem in array)
		{
			if (dAZClothingItem.gender == DAZDynamicItem.Gender.Male && !_selectedCharacter.isMale)
			{
				if (malePlaceholderCharacter != null)
				{
					dAZClothingItem.skin = malePlaceholderCharacter.skin;
				}
			}
			else if (dAZClothingItem.gender == DAZDynamicItem.Gender.Female && _selectedCharacter.isMale)
			{
				if (femalePlaceholderCharacter != null)
				{
					dAZClothingItem.skin = femalePlaceholderCharacter.skin;
				}
			}
			else
			{
				dAZClothingItem.skin = _selectedCharacter.skin;
			}
		}
		DAZClothingItem[] componentsInChildren2 = maleClothingContainer.GetComponentsInChildren<DAZClothingItem>(true);
		DAZClothingItem[] array2 = componentsInChildren2;
		foreach (DAZClothingItem dAZClothingItem2 in array2)
		{
			if (dAZClothingItem2.gender == DAZDynamicItem.Gender.Male && !_selectedCharacter.isMale)
			{
				if (malePlaceholderCharacter != null)
				{
					dAZClothingItem2.skin = malePlaceholderCharacter.skin;
				}
			}
			else if (dAZClothingItem2.gender == DAZDynamicItem.Gender.Female && _selectedCharacter.isMale)
			{
				if (femalePlaceholderCharacter != null)
				{
					dAZClothingItem2.skin = femalePlaceholderCharacter.skin;
				}
			}
			else
			{
				dAZClothingItem2.skin = _selectedCharacter.skin;
			}
		}
	}

	public void RefreshDynamicClothes()
	{
		if (Application.isPlaying && dynamicClothingItemPrefab != null)
		{
			DAZClothingItem[] componentsInChildren = femaleClothingContainer.GetComponentsInChildren<DAZClothingItem>(true);
			SyncCustomItems(DAZDynamicItem.Gender.Female, componentsInChildren, "Custom/Clothing/Female/", dynamicClothingItemPrefab, femaleClothingContainer);
			DAZClothingItem[] componentsInChildren2 = maleClothingContainer.GetComponentsInChildren<DAZClothingItem>(true);
			SyncCustomItems(DAZDynamicItem.Gender.Male, componentsInChildren2, "Custom/Clothing/Male/", dynamicClothingItemPrefab, maleClothingContainer);
			InitClothingItems();
			if (clothingSelectorFemaleUI != null)
			{
				clothingSelectorFemaleUI.Resync();
			}
			if (clothingSelectorMaleUI != null)
			{
				clothingSelectorMaleUI.Resync();
			}
		}
	}

	public HashSet<string> GetClothingOtherTags()
	{
		if (gender == Gender.Female)
		{
			if (clothingSelectorFemaleUI != null)
			{
				return clothingSelectorFemaleUI.GetOtherTags();
			}
		}
		else if (gender == Gender.Male && clothingSelectorMaleUI != null)
		{
			return clothingSelectorMaleUI.GetOtherTags();
		}
		return null;
	}

	public void InitClothingItems()
	{
		if (maleClothingContainer != null)
		{
			_maleClothingItems = maleClothingContainer.GetComponentsInChildren<DAZClothingItem>(true);
		}
		else
		{
			_maleClothingItems = new DAZClothingItem[0];
		}
		if (femaleClothingContainer != null)
		{
			_femaleClothingItems = femaleClothingContainer.GetComponentsInChildren<DAZClothingItem>(true);
		}
		else
		{
			_femaleClothingItems = new DAZClothingItem[0];
		}
		_clothingItemById = new Dictionary<string, DAZClothingItem>();
		_clothingItemByBackupId = new Dictionary<string, DAZClothingItem>();
		if (Application.isPlaying)
		{
			if (clothingItemJSONs == null)
			{
				clothingItemJSONs = new Dictionary<string, JSONStorableBool>();
			}
			if (clothingItemToggleJSONs == null)
			{
				clothingItemToggleJSONs = new List<JSONStorableAction>();
			}
		}
		DAZClothingItem[] array = clothingItems;
		foreach (DAZClothingItem dc in array)
		{
			if (Application.isPlaying && !clothingItemJSONs.ContainsKey(dc.uid))
			{
				JSONStorableBool jSONStorableBool = new JSONStorableBool("clothing:" + dc.uid, dc.gameObject.activeSelf, SyncClothingItem);
				jSONStorableBool.altName = dc.uid;
				jSONStorableBool.isRestorable = false;
				jSONStorableBool.isStorable = false;
				RegisterBool(jSONStorableBool);
				clothingItemJSONs.Add(dc.uid, jSONStorableBool);
				JSONStorableAction jSONStorableAction = new JSONStorableAction("toggle:" + dc.uid, delegate
				{
					ToggleClothingItem(dc);
				});
				RegisterAction(jSONStorableAction);
				clothingItemToggleJSONs.Add(jSONStorableAction);
			}
			dc.characterSelector = this;
			if (_clothingItemById.ContainsKey(dc.uid))
			{
				Debug.LogError("Duplicate uid found for clothing item " + dc.uid);
			}
			else
			{
				_clothingItemById.Add(dc.uid, dc);
			}
			if (dc.internalUid != null && dc.internalUid != string.Empty && !_clothingItemById.ContainsKey(dc.internalUid))
			{
				_clothingItemById.Add(dc.internalUid, dc);
			}
			if (dc.backupId != null && dc.backupId != string.Empty && !_clothingItemByBackupId.ContainsKey(dc.backupId))
			{
				_clothingItemByBackupId.Add(dc.backupId, dc);
			}
			if (dc.gameObject.activeSelf)
			{
				dc.active = true;
			}
		}
		if (!Application.isPlaying)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (JSONStorableBool value in clothingItemJSONs.Values)
		{
			string altName = value.altName;
			if (!_clothingItemById.ContainsKey(altName))
			{
				DeregisterBool(value);
				list.Add(altName);
			}
		}
		foreach (string item in list)
		{
			clothingItemJSONs.Remove(item);
		}
		List<JSONStorableAction> list2 = new List<JSONStorableAction>();
		foreach (JSONStorableAction clothingItemToggleJSON in clothingItemToggleJSONs)
		{
			string key = clothingItemToggleJSON.name.Replace("toggle:", string.Empty);
			if (!_clothingItemById.ContainsKey(key))
			{
				DeregisterAction(clothingItemToggleJSON);
			}
			else
			{
				list2.Add(clothingItemToggleJSON);
			}
		}
		clothingItemToggleJSONs = list2;
	}

	public void UnloadInactiveClothingItems()
	{
		if (_femaleClothingItems != null)
		{
			DAZClothingItem[] array = _femaleClothingItems;
			foreach (DAZClothingItem dAZClothingItem in array)
			{
				dAZClothingItem.UnloadIfInactive();
			}
		}
		if (_maleClothingItems != null)
		{
			DAZClothingItem[] array2 = _maleClothingItems;
			foreach (DAZClothingItem dAZClothingItem2 in array2)
			{
				dAZClothingItem2.UnloadIfInactive();
			}
		}
	}

	public void UnloadDisabledClothingItems()
	{
		if (_femaleClothingItems != null)
		{
			DAZClothingItem[] array = _femaleClothingItems;
			foreach (DAZClothingItem dAZClothingItem in array)
			{
				dAZClothingItem.UnloadIfNotEnabled();
			}
		}
		if (_maleClothingItems != null)
		{
			DAZClothingItem[] array2 = _maleClothingItems;
			foreach (DAZClothingItem dAZClothingItem2 in array2)
			{
				dAZClothingItem2.UnloadIfNotEnabled();
			}
		}
	}

	public void SyncClothingAdjustments()
	{
		SyncAnatomy();
	}

	public void SyncHairAdjustments()
	{
		SyncAnatomy();
	}

	public DAZClothingItem GetClothingItem(string itemId)
	{
		if (_clothingItemById == null || _clothingItemByBackupId == null)
		{
			Init();
		}
		DAZClothingItem value;
		if (_clothingItemById.TryGetValue(itemId, out value))
		{
			return value;
		}
		if (_clothingItemByBackupId.TryGetValue(itemId, out value))
		{
			return value;
		}
		return null;
	}

	public void EnableUndressOnClothingItem(DAZClothingItem clothingItem)
	{
		if (clothingItem != null)
		{
			ClothSimControl[] componentsInChildren = clothingItem.GetComponentsInChildren<ClothSimControl>();
			ClothSimControl[] array = componentsInChildren;
			foreach (ClothSimControl clothSimControl in array)
			{
				clothSimControl.AllowDetach();
			}
		}
	}

	public void EnableUndressAllClothingItems()
	{
		for (int i = 0; i < clothingItems.Length; i++)
		{
			DAZClothingItem dAZClothingItem = clothingItems[i];
			ClothSimControl[] componentsInChildren = dAZClothingItem.GetComponentsInChildren<ClothSimControl>();
			ClothSimControl[] array = componentsInChildren;
			foreach (ClothSimControl clothSimControl in array)
			{
				clothSimControl.AllowDetach();
			}
		}
	}

	public void RemoveAllClothing()
	{
		DAZClothingItem[] array = clothingItems;
		foreach (DAZClothingItem dAZClothingItem in array)
		{
			if (!dAZClothingItem.locked)
			{
				SetActiveClothingItem(dAZClothingItem, false);
			}
		}
		if (clothingSelectorUI != null)
		{
			clothingSelectorUI.ResyncUIIfActiveFilterOn();
		}
	}

	public void RemoveAllRealClothing()
	{
		DAZClothingItem[] array = clothingItems;
		foreach (DAZClothingItem dAZClothingItem in array)
		{
			if (dAZClothingItem.isRealItem && !dAZClothingItem.locked)
			{
				SetActiveClothingItem(dAZClothingItem, false);
			}
		}
		if (clothingSelectorUI != null)
		{
			clothingSelectorUI.ResyncUIIfActiveFilterOn();
		}
	}

	public void LockActiveClothing()
	{
		DAZClothingItem[] array = clothingItems;
		foreach (DAZClothingItem dAZClothingItem in array)
		{
			if (dAZClothingItem.active)
			{
				dAZClothingItem.SetLocked(true);
			}
		}
	}

	public void UnlockAllClothing()
	{
		DAZClothingItem[] array = clothingItems;
		foreach (DAZClothingItem dAZClothingItem in array)
		{
			if (dAZClothingItem.locked)
			{
				dAZClothingItem.SetLocked(false);
			}
		}
	}

	public void ToggleClothingItem(DAZClothingItem item)
	{
		SetActiveClothingItem(item, !item.active);
	}

	public void SetActiveClothingItem(string itemId, bool active, bool fromRestore = false)
	{
		DAZClothingItem value;
		if (_clothingItemById != null && _clothingItemById.TryGetValue(itemId, out value))
		{
			SetActiveClothingItem(value, active, fromRestore);
		}
		else if (_clothingItemByBackupId != null && _clothingItemByBackupId.TryGetValue(itemId, out value))
		{
			SetActiveClothingItem(value, active, fromRestore);
		}
		else
		{
			SuperController.LogError("Clothing item " + itemId + " is missing");
		}
	}

	private IEnumerator DelayLoadClothingCreatorItem(DAZClothingItem item, DAZDynamic source)
	{
		while (!item.ready)
		{
			yield return null;
		}
		yield return null;
		DAZRuntimeCreator drc = item.GetComponentInChildren<DAZRuntimeCreator>();
		if (drc != null)
		{
			drc.LoadFromPath(source.GetMetaStorePath());
			item.OpenUI();
		}
	}

	public void LoadMaleClothingCreatorItem(DAZDynamic source)
	{
		if (maleClothingCreatorItem != null)
		{
			SetActiveClothingItem(maleClothingCreatorItem, true);
			StartCoroutine(DelayLoadClothingCreatorItem(maleClothingCreatorItem, source));
		}
	}

	public void LoadFemaleClothingCreatorItem(DAZDynamic source)
	{
		if (femaleClothingCreatorItem != null)
		{
			SetActiveClothingItem(femaleClothingCreatorItem, true);
			StartCoroutine(DelayLoadClothingCreatorItem(femaleClothingCreatorItem, source));
		}
	}

	public void SetActiveClothingItem(DAZClothingItem item, bool active, bool fromRestore = false)
	{
		if (!(item != null))
		{
			return;
		}
		if (item.locked)
		{
			if (containingAtom.isPreparingToPutBackInPool)
			{
				item.SetLocked(false);
			}
			else if (insideRestore)
			{
				return;
			}
		}
		if (active && fromRestore)
		{
			item.needsPostLoadJSONRestore = true;
		}
		if (item.locked && !active)
		{
			item.SetLocked(false);
		}
		item.active = active;
		DAZClothingItem.ExclusiveRegion exclusiveRegion = item.exclusiveRegion;
		if (active && exclusiveRegion != 0)
		{
			for (int i = 0; i < clothingItems.Length; i++)
			{
				if (clothingItems[i] != item && item.gender == clothingItems[i].gender && clothingItems[i].exclusiveRegion == exclusiveRegion && clothingItems[i].active)
				{
					SetActiveClothingItem(clothingItems[i], false);
				}
			}
		}
		item.gameObject.SetActive(active);
		if (clothingSelectorUI != null)
		{
			clothingSelectorUI.SetDynamicItemToggle(item, active);
		}
		JSONStorableBool value;
		if (clothingItemJSONs.TryGetValue(item.uid, out value))
		{
			value.val = active;
		}
		SyncAnatomy();
	}

	public void SetClothingItemLock(DAZClothingItem item, bool locked)
	{
		if (!(item != null))
		{
		}
	}

	protected void ResetHair(bool clearAll = false)
	{
		Init();
		DAZHairGroup[] array = _femaleHairItems;
		foreach (DAZHairGroup dAZHairGroup in array)
		{
			if (clearAll)
			{
				SetActiveHairItem(dAZHairGroup, false);
			}
			else
			{
				SetActiveHairItem(dAZHairGroup, dAZHairGroup.startActive);
			}
		}
		DAZHairGroup[] array2 = _maleHairItems;
		foreach (DAZHairGroup dAZHairGroup2 in array2)
		{
			if (clearAll)
			{
				SetActiveHairItem(dAZHairGroup2, false);
			}
			else
			{
				SetActiveHairItem(dAZHairGroup2, dAZHairGroup2.startActive);
			}
		}
	}

	protected void SyncHairItem(JSONStorableBool hairItemJSON)
	{
		string altName = hairItemJSON.altName;
		SetActiveHairItem(altName, hairItemJSON.val);
	}

	public bool IsHairUIDAvailable(string uid)
	{
		bool result = true;
		if (_hairItemById != null && _hairItemById.ContainsKey(uid))
		{
			return false;
		}
		return result;
	}

	protected void EarlyInitHairItems()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (femaleHairPrefab != null && femaleHairContainer != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(femaleHairPrefab.gameObject);
			gameObject.transform.SetParent(femaleHairContainer);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			DAZDynamicItem[] componentsInChildren = gameObject.GetComponentsInChildren<DAZDynamicItem>(true);
			DAZDynamicItem[] array = componentsInChildren;
			foreach (DAZDynamicItem di in array)
			{
				ConnectDynamicItem(di);
			}
		}
		if (maleHairPrefab != null && maleHairContainer != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(maleHairPrefab.gameObject);
			gameObject2.transform.SetParent(maleHairContainer);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = Vector3.one;
			DAZDynamicItem[] componentsInChildren2 = gameObject2.GetComponentsInChildren<DAZDynamicItem>(true);
			DAZDynamicItem[] array2 = componentsInChildren2;
			foreach (DAZDynamicItem di2 in array2)
			{
				ConnectDynamicItem(di2);
			}
		}
	}

	public void InvalidateDynamicHairItemThumbnail(string thumbPath)
	{
		string text = Regex.Replace(thumbPath, "\\.jpg$", ".vam");
		DAZHairGroup[] array = hairItems;
		foreach (DAZHairGroup dAZHairGroup in array)
		{
			if (dAZHairGroup.dynamicRuntimeLoadPath == text)
			{
				dAZHairGroup.thumbnail = null;
			}
		}
	}

	public void RefreshDynamicHairThumbnails()
	{
		if (gender == Gender.Female)
		{
			if (hairSelectorFemaleUI != null)
			{
				hairSelectorFemaleUI.RefreshThumbnails();
			}
		}
		else if (gender == Gender.Male && hairSelectorMaleUI != null)
		{
			hairSelectorMaleUI.RefreshThumbnails();
		}
	}

	protected void ValidateDynamicHairSkin()
	{
		if (!(_selectedCharacter != null))
		{
			return;
		}
		DAZHairGroup[] componentsInChildren = femaleHairContainer.GetComponentsInChildren<DAZHairGroup>(true);
		DAZHairGroup[] array = componentsInChildren;
		foreach (DAZHairGroup dAZHairGroup in array)
		{
			if (dAZHairGroup.gender == DAZDynamicItem.Gender.Male && !_selectedCharacter.isMale)
			{
				if (malePlaceholderCharacter != null)
				{
					dAZHairGroup.skin = malePlaceholderCharacter.skin;
				}
			}
			else if (dAZHairGroup.gender == DAZDynamicItem.Gender.Female && _selectedCharacter.isMale)
			{
				if (femalePlaceholderCharacter != null)
				{
					dAZHairGroup.skin = femalePlaceholderCharacter.skin;
				}
			}
			else
			{
				dAZHairGroup.skin = _selectedCharacter.skin;
			}
		}
		DAZHairGroup[] componentsInChildren2 = maleHairContainer.GetComponentsInChildren<DAZHairGroup>(true);
		DAZHairGroup[] array2 = componentsInChildren2;
		foreach (DAZHairGroup dAZHairGroup2 in array2)
		{
			if (dAZHairGroup2.gender == DAZDynamicItem.Gender.Male && !_selectedCharacter.isMale)
			{
				if (malePlaceholderCharacter != null)
				{
					dAZHairGroup2.skin = malePlaceholderCharacter.skin;
				}
			}
			else if (dAZHairGroup2.gender == DAZDynamicItem.Gender.Female && _selectedCharacter.isMale)
			{
				if (femalePlaceholderCharacter != null)
				{
					dAZHairGroup2.skin = femalePlaceholderCharacter.skin;
				}
			}
			else
			{
				dAZHairGroup2.skin = _selectedCharacter.skin;
			}
		}
	}

	public void RefreshDynamicHair()
	{
		if (Application.isPlaying && dynamicHairItemPrefab != null)
		{
			DAZHairGroup[] componentsInChildren = femaleHairContainer.GetComponentsInChildren<DAZHairGroup>(true);
			SyncCustomItems(DAZDynamicItem.Gender.Female, componentsInChildren, "Custom/Hair/Female/", dynamicHairItemPrefab, femaleHairContainer);
			DAZHairGroup[] componentsInChildren2 = maleHairContainer.GetComponentsInChildren<DAZHairGroup>(true);
			SyncCustomItems(DAZDynamicItem.Gender.Male, componentsInChildren2, "Custom/Hair/Male/", dynamicHairItemPrefab, maleHairContainer);
			InitHairItems();
			if (hairSelectorFemaleUI != null)
			{
				hairSelectorFemaleUI.Resync();
			}
			if (hairSelectorMaleUI != null)
			{
				hairSelectorMaleUI.Resync();
			}
		}
	}

	public HashSet<string> GetHairOtherTags()
	{
		if (gender == Gender.Female)
		{
			if (hairSelectorFemaleUI != null)
			{
				return hairSelectorFemaleUI.GetOtherTags();
			}
		}
		else if (gender == Gender.Male && hairSelectorMaleUI != null)
		{
			return hairSelectorMaleUI.GetOtherTags();
		}
		return null;
	}

	public void InitHairItems()
	{
		if (maleHairContainer != null)
		{
			_maleHairItems = maleHairContainer.GetComponentsInChildren<DAZHairGroup>(true);
		}
		else
		{
			_maleHairItems = new DAZHairGroup[0];
		}
		if (femaleHairContainer != null)
		{
			_femaleHairItems = femaleHairContainer.GetComponentsInChildren<DAZHairGroup>(true);
		}
		else
		{
			_femaleHairItems = new DAZHairGroup[0];
		}
		_hairItemById = new Dictionary<string, DAZHairGroup>();
		_hairItemByBackupId = new Dictionary<string, DAZHairGroup>();
		if (Application.isPlaying)
		{
			if (hairItemJSONs == null)
			{
				hairItemJSONs = new Dictionary<string, JSONStorableBool>();
			}
			if (hairItemToggleJSONs == null)
			{
				hairItemToggleJSONs = new List<JSONStorableAction>();
			}
		}
		DAZHairGroup[] array = hairItems;
		foreach (DAZHairGroup dc in array)
		{
			if (Application.isPlaying && !hairItemJSONs.ContainsKey(dc.uid))
			{
				JSONStorableBool jSONStorableBool = new JSONStorableBool("hair:" + dc.uid, dc.gameObject.activeSelf, SyncHairItem);
				jSONStorableBool.altName = dc.uid;
				jSONStorableBool.isRestorable = false;
				jSONStorableBool.isStorable = false;
				RegisterBool(jSONStorableBool);
				hairItemJSONs.Add(dc.uid, jSONStorableBool);
				JSONStorableAction jSONStorableAction = new JSONStorableAction("toggle:" + dc.uid, delegate
				{
					ToggleHairItem(dc);
				});
				RegisterAction(jSONStorableAction);
				hairItemToggleJSONs.Add(jSONStorableAction);
			}
			dc.characterSelector = this;
			if (_hairItemById.ContainsKey(dc.uid))
			{
				Debug.LogError("Duplicate uid found for hair item " + dc.uid);
			}
			else
			{
				_hairItemById.Add(dc.uid, dc);
			}
			if (dc.internalUid != null && dc.internalUid != string.Empty && !_hairItemById.ContainsKey(dc.internalUid))
			{
				_hairItemById.Add(dc.internalUid, dc);
			}
			if (!_hairItemByBackupId.ContainsKey(dc.backupId))
			{
				_hairItemByBackupId.Add(dc.backupId, dc);
			}
			if (dc.gameObject.activeSelf)
			{
				dc.active = true;
			}
		}
		if (!Application.isPlaying)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (JSONStorableBool value in hairItemJSONs.Values)
		{
			string altName = value.altName;
			if (!_hairItemById.ContainsKey(altName))
			{
				DeregisterBool(value);
				list.Add(altName);
			}
		}
		foreach (string item in list)
		{
			hairItemJSONs.Remove(item);
		}
		List<JSONStorableAction> list2 = new List<JSONStorableAction>();
		foreach (JSONStorableAction hairItemToggleJSON in hairItemToggleJSONs)
		{
			string key = hairItemToggleJSON.name.Replace("toggle:", string.Empty);
			if (!_hairItemById.ContainsKey(key))
			{
				DeregisterAction(hairItemToggleJSON);
			}
			else
			{
				list2.Add(hairItemToggleJSON);
			}
		}
		hairItemToggleJSONs = list2;
	}

	public void UnloadInactiveHairItems()
	{
		if (_femaleHairItems != null)
		{
			DAZHairGroup[] array = _femaleHairItems;
			foreach (DAZHairGroup dAZHairGroup in array)
			{
				dAZHairGroup.UnloadIfInactive();
			}
		}
		if (_maleHairItems != null)
		{
			DAZHairGroup[] array2 = _maleHairItems;
			foreach (DAZHairGroup dAZHairGroup2 in array2)
			{
				dAZHairGroup2.UnloadIfInactive();
			}
		}
	}

	public void UnloadDisabledHairItems()
	{
		if (_femaleHairItems != null)
		{
			DAZHairGroup[] array = _femaleHairItems;
			foreach (DAZHairGroup dAZHairGroup in array)
			{
				dAZHairGroup.UnloadIfNotEnabled();
			}
		}
		if (_maleHairItems != null)
		{
			DAZHairGroup[] array2 = _maleHairItems;
			foreach (DAZHairGroup dAZHairGroup2 in array2)
			{
				dAZHairGroup2.UnloadIfNotEnabled();
			}
		}
	}

	public DAZHairGroup GetHairItem(string itemId)
	{
		if (_hairItemById == null)
		{
			Init();
		}
		DAZHairGroup value;
		if (_hairItemById.TryGetValue(itemId, out value))
		{
			return value;
		}
		if (_hairItemByBackupId.TryGetValue(itemId, out value))
		{
			return value;
		}
		return null;
	}

	public void RemoveAllHair()
	{
		DAZHairGroup[] array = hairItems;
		foreach (DAZHairGroup dAZHairGroup in array)
		{
			if (!dAZHairGroup.locked)
			{
				SetActiveHairItem(dAZHairGroup, false);
			}
		}
		if (hairSelectorUI != null)
		{
			hairSelectorUI.ResyncUIIfActiveFilterOn();
		}
	}

	public void LockActiveHair()
	{
		DAZHairGroup[] array = hairItems;
		foreach (DAZHairGroup dAZHairGroup in array)
		{
			if (dAZHairGroup.active)
			{
				dAZHairGroup.SetLocked(true);
			}
		}
	}

	public void UnlockAllHair()
	{
		DAZHairGroup[] array = hairItems;
		foreach (DAZHairGroup dAZHairGroup in array)
		{
			if (dAZHairGroup.locked)
			{
				dAZHairGroup.SetLocked(false);
			}
		}
	}

	public void ToggleHairItem(DAZHairGroup item)
	{
		SetActiveHairItem(item, !item.active);
	}

	public void SetActiveHairItem(string itemId, bool active, bool fromRestore = false)
	{
		DAZHairGroup value;
		if (_hairItemById != null && _hairItemById.TryGetValue(itemId, out value))
		{
			SetActiveHairItem(value, active, fromRestore);
		}
		else if (_hairItemByBackupId != null && _hairItemByBackupId.TryGetValue(itemId, out value))
		{
			SetActiveHairItem(value, active, fromRestore);
		}
		else if (itemId != "Sim Hair")
		{
			SuperController.LogError("Hair item " + itemId + " is missing");
		}
	}

	private IEnumerator DelayLoadHairCreatorItem(DAZHairGroup item, DAZDynamic source)
	{
		while (!item.ready)
		{
			yield return null;
		}
		yield return null;
		DAZRuntimeCreator drc = item.GetComponentInChildren<DAZRuntimeCreator>();
		if (drc != null)
		{
			drc.LoadFromPath(source.GetMetaStorePath());
			item.OpenUI();
		}
	}

	public void LoadMaleHairCreatorItem(DAZDynamic source)
	{
		if (maleHairCreatorItem != null)
		{
			SetActiveHairItem(maleHairCreatorItem, true);
			StartCoroutine(DelayLoadHairCreatorItem(maleHairCreatorItem, source));
		}
	}

	public void LoadFemaleHairCreatorItem(DAZDynamic source)
	{
		if (femaleHairCreatorItem != null)
		{
			SetActiveHairItem(femaleHairCreatorItem, true);
			StartCoroutine(DelayLoadHairCreatorItem(femaleHairCreatorItem, source));
		}
	}

	public void SetActiveHairItem(DAZHairGroup item, bool active, bool fromRestore = false)
	{
		if (!(item != null))
		{
			return;
		}
		if (item.locked)
		{
			if (containingAtom.isPreparingToPutBackInPool)
			{
				item.SetLocked(false);
			}
			else if (insideRestore)
			{
				return;
			}
		}
		if (active && fromRestore)
		{
			item.needsPostLoadJSONRestore = true;
		}
		if (item.locked && !active)
		{
			item.SetLocked(false);
		}
		item.active = active;
		item.gameObject.SetActive(active);
		if (hairSelectorUI != null)
		{
			hairSelectorUI.SetDynamicItemToggle(item, active);
		}
		JSONStorableBool value;
		if (hairItemJSONs.TryGetValue(item.uid, out value))
		{
			value.val = active;
		}
		SyncAnatomy();
	}

	protected IEnumerator UnloadUnusedAssetsDelayed()
	{
		yield return null;
		yield return null;
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	public void UnloadInactiveObjects()
	{
		UnloadInactiveCharacters();
		UnloadInactiveClothingItems();
		UnloadInactiveHairItems();
		UnloadRuntimeMorphDeltas();
		UnloadDemandActivatedMorphs();
		StartCoroutine(UnloadUnusedAssetsDelayed());
	}

	protected void SyncUseAuxBreastColliders(bool b)
	{
		_useAuxBreastColliders = b;
		SyncColliders();
	}

	protected void SyncUseAdvancedColliders(bool b)
	{
		_useAdvancedColliders = b;
		SyncColliders();
	}

	protected void SyncColliders()
	{
		if (_useAdvancedColliders)
		{
			Transform[] array = regularCollidersFemale;
			foreach (Transform transform in array)
			{
				transform.gameObject.SetActive(false);
			}
			Transform[] array2 = regularCollidersMale;
			foreach (Transform transform2 in array2)
			{
				transform2.gameObject.SetActive(false);
			}
			Transform[] array3 = regularColliders;
			foreach (Transform transform3 in array3)
			{
				transform3.gameObject.SetActive(false);
			}
			if (_gender == Gender.Male)
			{
				Transform[] array4 = advancedCollidersFemale;
				foreach (Transform transform4 in array4)
				{
					transform4.gameObject.SetActive(false);
				}
				Transform[] array5 = advancedCollidersMale;
				foreach (Transform transform5 in array5)
				{
					transform5.gameObject.SetActive(true);
				}
			}
			else
			{
				Transform[] array6 = advancedCollidersMale;
				foreach (Transform transform6 in array6)
				{
					transform6.gameObject.SetActive(false);
				}
				Transform[] array7 = advancedCollidersFemale;
				foreach (Transform transform7 in array7)
				{
					transform7.gameObject.SetActive(true);
				}
			}
		}
		else
		{
			Transform[] array8 = advancedCollidersFemale;
			foreach (Transform transform8 in array8)
			{
				transform8.gameObject.SetActive(false);
			}
			Transform[] array9 = advancedCollidersMale;
			foreach (Transform transform9 in array9)
			{
				transform9.gameObject.SetActive(false);
			}
			if (_gender == Gender.Male)
			{
				Transform[] array10 = regularCollidersFemale;
				foreach (Transform transform10 in array10)
				{
					transform10.gameObject.SetActive(false);
				}
				Transform[] array11 = regularCollidersMale;
				foreach (Transform transform11 in array11)
				{
					transform11.gameObject.SetActive(true);
				}
			}
			else
			{
				Transform[] array12 = regularCollidersMale;
				foreach (Transform transform12 in array12)
				{
					transform12.gameObject.SetActive(false);
				}
				Transform[] array13 = regularCollidersFemale;
				foreach (Transform transform13 in array13)
				{
					transform13.gameObject.SetActive(true);
				}
			}
			Transform[] array14 = regularColliders;
			foreach (Transform transform14 in array14)
			{
				transform14.gameObject.SetActive(true);
			}
		}
		Collider[] array15 = auxBreastColliders;
		foreach (Collider collider in array15)
		{
			collider.enabled = useAuxBreastColliders;
			CapsuleLineSphereCollider component = collider.GetComponent<CapsuleLineSphereCollider>();
			if (component != null)
			{
				component.enabled = useAuxBreastColliders;
			}
			GpuSphereCollider component2 = collider.GetComponent<GpuSphereCollider>();
			if (component2 != null)
			{
				component2.enabled = useAuxBreastColliders;
			}
		}
		if (!Application.isPlaying)
		{
			return;
		}
		IgnoreChildColliders[] ignoreChildColliders = _ignoreChildColliders;
		foreach (IgnoreChildColliders ignoreChildColliders2 in ignoreChildColliders)
		{
			ignoreChildColliders2.SyncColliders();
		}
		DAZPhysicsMesh[] physicsMeshes = _physicsMeshes;
		foreach (DAZPhysicsMesh dAZPhysicsMesh in physicsMeshes)
		{
			dAZPhysicsMesh.InitColliders();
		}
		AutoColliderBatchUpdater[] autoColliderBatchUpdaters = _autoColliderBatchUpdaters;
		foreach (AutoColliderBatchUpdater autoColliderBatchUpdater in autoColliderBatchUpdaters)
		{
			if (autoColliderBatchUpdater != null)
			{
				autoColliderBatchUpdater.UpdateAutoColliders();
			}
		}
		AutoColliderGroup[] autoColliderGroups = _autoColliderGroups;
		foreach (AutoColliderGroup autoColliderGroup in autoColliderGroups)
		{
			if (autoColliderGroup != null && autoColliderGroup.isActiveAndEnabled)
			{
				autoColliderGroup.InitColliders();
			}
		}
		if (_useAdvancedColliders)
		{
			containingAtom.ResetPhysics(false);
		}
	}

	public void CopyUI()
	{
		if (copyUIFrom != null)
		{
			color1DisplayNameText = copyUIFrom.color1DisplayNameText;
			color1Picker = copyUIFrom.color1Picker;
			color1Container = copyUIFrom.color1Container;
			color2DisplayNameText = copyUIFrom.color2DisplayNameText;
			color2Picker = copyUIFrom.color2Picker;
			color2Container = copyUIFrom.color2Container;
			color3DisplayNameText = copyUIFrom.color3DisplayNameText;
			color3Picker = copyUIFrom.color3Picker;
			color3Container = copyUIFrom.color3Container;
			param1DisplayNameText = copyUIFrom.param1DisplayNameText;
			param1Slider = copyUIFrom.param1Slider;
			param1DisplayNameTextAlt = copyUIFrom.param1DisplayNameTextAlt;
			param1SliderAlt = copyUIFrom.param1SliderAlt;
			param2DisplayNameText = copyUIFrom.param2DisplayNameText;
			param2Slider = copyUIFrom.param2Slider;
			param2DisplayNameTextAlt = copyUIFrom.param2DisplayNameTextAlt;
			param2SliderAlt = copyUIFrom.param2SliderAlt;
			param3DisplayNameText = copyUIFrom.param3DisplayNameText;
			param3Slider = copyUIFrom.param3Slider;
			param3DisplayNameTextAlt = copyUIFrom.param3DisplayNameTextAlt;
			param3SliderAlt = copyUIFrom.param3SliderAlt;
			param4DisplayNameText = copyUIFrom.param4DisplayNameText;
			param4Slider = copyUIFrom.param4Slider;
			param4DisplayNameTextAlt = copyUIFrom.param4DisplayNameTextAlt;
			param4SliderAlt = copyUIFrom.param4SliderAlt;
			param5DisplayNameText = copyUIFrom.param5DisplayNameText;
			param5Slider = copyUIFrom.param5Slider;
			param5DisplayNameTextAlt = copyUIFrom.param5DisplayNameTextAlt;
			param5SliderAlt = copyUIFrom.param5SliderAlt;
			param6DisplayNameText = copyUIFrom.param6DisplayNameText;
			param6Slider = copyUIFrom.param6Slider;
			param6DisplayNameTextAlt = copyUIFrom.param6DisplayNameTextAlt;
			param6SliderAlt = copyUIFrom.param6SliderAlt;
			param7DisplayNameText = copyUIFrom.param7DisplayNameText;
			param7Slider = copyUIFrom.param7Slider;
			param7DisplayNameTextAlt = copyUIFrom.param7DisplayNameTextAlt;
			param7SliderAlt = copyUIFrom.param7SliderAlt;
			param8DisplayNameText = copyUIFrom.param8DisplayNameText;
			param8Slider = copyUIFrom.param8Slider;
			param8DisplayNameTextAlt = copyUIFrom.param8DisplayNameTextAlt;
			param8SliderAlt = copyUIFrom.param8SliderAlt;
			param9DisplayNameText = copyUIFrom.param9DisplayNameText;
			param9Slider = copyUIFrom.param9Slider;
			param9DisplayNameTextAlt = copyUIFrom.param9DisplayNameTextAlt;
			param9SliderAlt = copyUIFrom.param9SliderAlt;
			param10DisplayNameText = copyUIFrom.param10DisplayNameText;
			param10Slider = copyUIFrom.param10Slider;
			param10DisplayNameTextAlt = copyUIFrom.param10DisplayNameTextAlt;
			param10SliderAlt = copyUIFrom.param10SliderAlt;
			textureGroup1Popup = copyUIFrom.textureGroup1Popup;
			textureGroup1PopupAlt = copyUIFrom.textureGroup1PopupAlt;
			textureGroup2Popup = copyUIFrom.textureGroup2Popup;
			textureGroup2PopupAlt = copyUIFrom.textureGroup2PopupAlt;
			textureGroup3Popup = copyUIFrom.textureGroup3Popup;
			textureGroup3PopupAlt = copyUIFrom.textureGroup3PopupAlt;
			textureGroup4Popup = copyUIFrom.textureGroup4Popup;
			textureGroup4PopupAlt = copyUIFrom.textureGroup4PopupAlt;
			textureGroup5Popup = copyUIFrom.textureGroup5Popup;
			textureGroup5PopupAlt = copyUIFrom.textureGroup5PopupAlt;
		}
	}

	private void DisconnectCharacterOptionsUI()
	{
		if (_selectedCharacter != null)
		{
			DAZCharacterMaterialOptions componentInChildren = _selectedCharacter.GetComponentInChildren<DAZCharacterMaterialOptions>();
			if (componentInChildren != null)
			{
				componentInChildren.DeregisterUI();
			}
			DAZCharacterTextureControl componentInChildren2 = _selectedCharacter.GetComponentInChildren<DAZCharacterTextureControl>();
			if (componentInChildren2 != null)
			{
				componentInChildren2.DeregisterUI();
				componentInChildren2.DeregisterUIAlt();
			}
		}
	}

	private void ConnectCharacterMaterialOptionsUI()
	{
		if (!Application.isPlaying || !(_selectedCharacter != null))
		{
			return;
		}
		DAZCharacterMaterialOptions componentInChildren = _selectedCharacter.GetComponentInChildren<DAZCharacterMaterialOptions>();
		if (componentInChildren != null)
		{
			componentInChildren.CheckAwake();
			componentInChildren.color1DisplayNameText = color1DisplayNameText;
			componentInChildren.color1Picker = color1Picker;
			componentInChildren.color1Container = color1Container;
			componentInChildren.color2DisplayNameText = color2DisplayNameText;
			componentInChildren.color2Picker = color2Picker;
			componentInChildren.color2Container = color2Container;
			componentInChildren.color3DisplayNameText = color3DisplayNameText;
			componentInChildren.color3Picker = color3Picker;
			componentInChildren.color3Container = color3Container;
			componentInChildren.param1DisplayNameText = param1DisplayNameText;
			componentInChildren.param1Slider = param1Slider;
			componentInChildren.param1DisplayNameTextAlt = param1DisplayNameTextAlt;
			componentInChildren.param1SliderAlt = param1SliderAlt;
			componentInChildren.param2DisplayNameText = param2DisplayNameText;
			componentInChildren.param2Slider = param2Slider;
			componentInChildren.param2DisplayNameTextAlt = param2DisplayNameTextAlt;
			componentInChildren.param2SliderAlt = param2SliderAlt;
			componentInChildren.param3DisplayNameText = param3DisplayNameText;
			componentInChildren.param3Slider = param3Slider;
			componentInChildren.param3DisplayNameTextAlt = param3DisplayNameTextAlt;
			componentInChildren.param3SliderAlt = param3SliderAlt;
			componentInChildren.param4DisplayNameText = param4DisplayNameText;
			componentInChildren.param4Slider = param4Slider;
			componentInChildren.param4DisplayNameTextAlt = param4DisplayNameTextAlt;
			componentInChildren.param4SliderAlt = param4SliderAlt;
			componentInChildren.param5DisplayNameText = param5DisplayNameText;
			componentInChildren.param5Slider = param5Slider;
			componentInChildren.param5DisplayNameTextAlt = param5DisplayNameTextAlt;
			componentInChildren.param5SliderAlt = param5SliderAlt;
			componentInChildren.param6DisplayNameText = param6DisplayNameText;
			componentInChildren.param6Slider = param6Slider;
			componentInChildren.param6DisplayNameTextAlt = param6DisplayNameTextAlt;
			componentInChildren.param6SliderAlt = param6SliderAlt;
			componentInChildren.param7DisplayNameText = param7DisplayNameText;
			componentInChildren.param7Slider = param7Slider;
			componentInChildren.param7DisplayNameTextAlt = param7DisplayNameTextAlt;
			componentInChildren.param7SliderAlt = param7SliderAlt;
			componentInChildren.param8DisplayNameText = param8DisplayNameText;
			componentInChildren.param8Slider = param8Slider;
			componentInChildren.param8DisplayNameTextAlt = param8DisplayNameTextAlt;
			componentInChildren.param8SliderAlt = param8SliderAlt;
			componentInChildren.param9DisplayNameText = param9DisplayNameText;
			componentInChildren.param9Slider = param9Slider;
			componentInChildren.param9DisplayNameTextAlt = param9DisplayNameTextAlt;
			componentInChildren.param9SliderAlt = param9SliderAlt;
			componentInChildren.param10DisplayNameText = param10DisplayNameText;
			componentInChildren.param10Slider = param10Slider;
			componentInChildren.param10DisplayNameTextAlt = param10DisplayNameTextAlt;
			componentInChildren.param10SliderAlt = param10SliderAlt;
			componentInChildren.textureGroup1Popup = textureGroup1Popup;
			componentInChildren.textureGroup1PopupAlt = textureGroup1PopupAlt;
			componentInChildren.textureGroup2Popup = textureGroup2Popup;
			componentInChildren.textureGroup2PopupAlt = textureGroup2PopupAlt;
			componentInChildren.textureGroup3Popup = textureGroup3Popup;
			componentInChildren.textureGroup3PopupAlt = textureGroup3PopupAlt;
			componentInChildren.textureGroup4Popup = textureGroup4Popup;
			componentInChildren.textureGroup4PopupAlt = textureGroup4PopupAlt;
			componentInChildren.textureGroup5Popup = textureGroup5Popup;
			componentInChildren.textureGroup5PopupAlt = textureGroup5PopupAlt;
			componentInChildren.restoreAllFromDefaultsAction.button = restoreMaterialFromDefaultsButton;
			componentInChildren.restoreAllFromDefaultsAction.buttonAlt = restoreMaterialFromDefaultsButtonAlt;
			componentInChildren.restoreAllFromStore1Action.button = restoreMaterialFromStore1Button;
			componentInChildren.restoreAllFromStore1Action.buttonAlt = restoreMaterialFromStore1ButtonAlt;
			componentInChildren.restoreAllFromStore2Action.button = restoreMaterialFromStore2Button;
			componentInChildren.restoreAllFromStore2Action.buttonAlt = restoreMaterialFromStore2ButtonAlt;
			componentInChildren.restoreAllFromStore3Action.button = restoreMaterialFromStore3Button;
			componentInChildren.restoreAllFromStore3Action.buttonAlt = restoreMaterialFromStore3ButtonAlt;
			componentInChildren.saveToStore1Action.button = saveMaterialToStore1Button;
			componentInChildren.saveToStore1Action.buttonAlt = saveMaterialToStore1ButtonAlt;
			componentInChildren.saveToStore2Action.button = saveMaterialToStore2Button;
			componentInChildren.saveToStore2Action.buttonAlt = saveMaterialToStore2ButtonAlt;
			componentInChildren.saveToStore3Action.button = saveMaterialToStore3Button;
			componentInChildren.saveToStore3Action.buttonAlt = saveMaterialToStore3ButtonAlt;
			componentInChildren.InitUI();
			componentInChildren.InitUIAlt();
		}
		DAZCharacterTextureControl componentInChildren2 = _selectedCharacter.GetComponentInChildren<DAZCharacterTextureControl>();
		if (componentInChildren2 != null)
		{
			if (characterTextureUITab != null)
			{
				characterTextureUITab.gameObject.SetActive(true);
			}
			if (characterTextureUI != null)
			{
				componentInChildren2.SetUI(characterTextureUI.transform);
			}
			if (characterTextureUITabAlt != null)
			{
				characterTextureUITabAlt.gameObject.SetActive(true);
			}
			if (characterTextureUIAlt != null)
			{
				componentInChildren2.SetUIAlt(characterTextureUIAlt.transform);
			}
		}
		else
		{
			if (characterTextureUITab != null)
			{
				characterTextureUITab.gameObject.SetActive(false);
			}
			if (characterTextureUITabAlt != null)
			{
				characterTextureUITabAlt.gameObject.SetActive(false);
			}
		}
	}

	public void InitComponents()
	{
		_eyelidControl = GetComponentInChildren<DAZMeshEyelidControl>();
		_characterRun = GetComponentInChildren<DAZCharacterRun>();
		if (_characterRun != null)
		{
			_characterRun.characterSelector = this;
		}
		_physicsMeshes = GetComponentsInChildren<DAZPhysicsMesh>(true);
		_setDAZMorphs = GetComponentsInChildren<SetDAZMorph>(true);
		lipSync = GetComponentInChildren<SpeechBlend>(true);
		_autoColliderBatchUpdaters = rootBones.GetComponentsInChildren<AutoColliderBatchUpdater>(true);
		_autoColliderGroups = rootBones.GetComponentsInChildren<AutoColliderGroup>(true);
		_autoColliders = rootBones.GetComponentsInChildren<AutoCollider>(true);
		_setAnchorFromVertexComps = rootBones.GetComponentsInChildren<SetAnchorFromVertex>(true);
		_ignoreChildColliders = rootBones.GetComponentsInChildren<IgnoreChildColliders>(true);
		List<DAZCharacterMaterialOptions> list = new List<DAZCharacterMaterialOptions>();
		DAZCharacterMaterialOptions[] componentsInChildren = GetComponentsInChildren<DAZCharacterMaterialOptions>();
		DAZCharacterMaterialOptions[] array = componentsInChildren;
		foreach (DAZCharacterMaterialOptions dAZCharacterMaterialOptions in array)
		{
			DAZCharacter component = dAZCharacterMaterialOptions.GetComponent<DAZCharacter>();
			DAZSkinV2 component2 = dAZCharacterMaterialOptions.GetComponent<DAZSkinV2>();
			if (component == null && component2 == null && dAZCharacterMaterialOptions != femaleEyelashMaterialOptions && dAZCharacterMaterialOptions != maleEyelashMaterialOptions)
			{
				list.Add(dAZCharacterMaterialOptions);
			}
		}
		_materialOptions = list.ToArray();
	}

	protected void InitJSONParams()
	{
		if (Application.isPlaying)
		{
			useAdvancedCollidersJSON = new JSONStorableBool("useAdvancedColliders", _useAdvancedColliders, SyncUseAdvancedColliders);
			useAdvancedCollidersJSON.storeType = JSONStorableParam.StoreType.Any;
			RegisterBool(useAdvancedCollidersJSON);
			useAuxBreastCollidersJSON = new JSONStorableBool("useAuxBreastColliders", _useAuxBreastColliders, SyncUseAuxBreastColliders);
			useAuxBreastCollidersJSON.storeType = JSONStorableParam.StoreType.Any;
			RegisterBool(useAuxBreastCollidersJSON);
			disableAnatomyJSON = new JSONStorableBool("disableAnatomy", _disableAnatomy, SyncDisableAnatomy);
			RegisterBool(disableAnatomyJSON);
			onlyShowFavoriteMorphsInParametersListsJSON = new JSONStorableBool("onlyShowFavoriteMorphsInParametersLists", _onlyShowFavoriteMorphsInParameterLists, SyncOnlyShowFavoriteMorphsInParameterLists);
			useMaleMorphsOnFemaleJSON = new JSONStorableBool("useMaleMorphsOnFemale", _useMaleMorphsOnFemale, SyncUseMaleMorphsOnFemale);
			RegisterBool(useMaleMorphsOnFemaleJSON);
			useFemaleMorphsOnMaleJSON = new JSONStorableBool("useFemaleMorphsOnMale", _useFemaleMorphsOnMale, SyncUseFemaleMorphsOnMale);
			RegisterBool(useFemaleMorphsOnMaleJSON);
			unloadCharactersWhenSwitchingJSON = new JSONStorableBool("unloadCharactersWhenSwitching", _unloadCharactersWhenSwitching, SyncUnloadCharactersWhenSwitching);
			unloadCharactersWhenSwitchingJSON.isStorable = false;
			unloadCharactersWhenSwitchingJSON.isRestorable = false;
			RegisterBool(unloadCharactersWhenSwitchingJSON);
			unloadInactiveObjectsJSONAction = new JSONStorableAction("UnloadInactiveObjects", UnloadInactiveObjects);
			RegisterAction(unloadInactiveObjectsJSONAction);
			removeAllClothingAction = new JSONStorableAction("RemoveAllClothing", RemoveAllClothing);
			RegisterAction(removeAllClothingAction);
			removeAllRealClothingAction = new JSONStorableAction("RemoveAllRealClothing", RemoveAllRealClothing);
			RegisterAction(removeAllRealClothingAction);
			undressAllClothingAction = new JSONStorableAction("UndressAllClothing", EnableUndressAllClothingItems);
			RegisterAction(undressAllClothingAction);
		}
	}

	protected void EarlyInit()
	{
		EarlyInitClothingItems();
		EarlyInitHairItems();
		EarlyInitCharacters();
	}

	public void Init(bool genderChange = false)
	{
		if (!wasInit || genderChange)
		{
			wasInit = true;
			if (!genderChange)
			{
				InitMorphBanks();
				InitBones();
				InitComponents();
				InitCharacters();
				RefreshDynamicClothes();
				RefreshDynamicHair();
			}
			InitClothingItems();
			InitHairItems();
			SyncAnatomy();
		}
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			DAZCharacterSelectorUI componentInChildren = UITransform.GetComponentInChildren<DAZCharacterSelectorUI>();
			if (componentInChildren != null)
			{
				useAdvancedCollidersJSON.toggle = componentInChildren.useAdvancedCollidersToggle;
				useAuxBreastCollidersJSON.toggle = componentInChildren.useAuxBreastCollidersToggle;
				disableAnatomyJSON.toggle = componentInChildren.disableAnatomyToggle;
				onlyShowFavoriteMorphsInParametersListsJSON.toggle = componentInChildren.onlyShowFavoriteMorphsInParameterListsToggle;
				useMaleMorphsOnFemaleJSON.toggle = componentInChildren.useMaleMorphsOnFemaleToggle;
				useFemaleMorphsOnMaleJSON.toggle = componentInChildren.useFemaleMorphsOnMaleToggle;
				unloadCharactersWhenSwitchingJSON.toggle = componentInChildren.unloadCharactersWhenSwitchingToggle;
				unloadInactiveObjectsJSONAction.button = componentInChildren.unloadInactiveObjectsButton;
				removeAllClothingAction.button = componentInChildren.removeAllClothingButton;
				removeAllClothingAction.buttonAlt = componentInChildren.removeAllClothingButtonAlt;
				removeAllRealClothingAction.button = componentInChildren.removeAllRealClothingButton;
				removeAllRealClothingAction.buttonAlt = componentInChildren.removeAllRealClothingButtonAlt;
				undressAllClothingAction.button = componentInChildren.undressAllClothingButton;
				undressAllClothingAction.buttonAlt = componentInChildren.undressAllClothingButtonAlt;
			}
		}
	}

	protected void ReportMemory(List<string> reports)
	{
		int num = 0;
		if (_femaleCharacters != null)
		{
			DAZCharacter[] array = _femaleCharacters;
			foreach (DAZCharacter dAZCharacter in array)
			{
				if (dAZCharacter.ready)
				{
					num++;
				}
			}
		}
		if (_maleCharacters != null)
		{
			DAZCharacter[] array2 = _maleCharacters;
			foreach (DAZCharacter dAZCharacter2 in array2)
			{
				if (dAZCharacter2.ready)
				{
					num++;
				}
			}
		}
		reports.Add(containingAtom.uid + " loaded characters count: " + num);
		int num2 = 0;
		if (_femaleClothingItems != null)
		{
			DAZClothingItem[] array3 = _femaleClothingItems;
			foreach (DAZClothingItem dAZClothingItem in array3)
			{
				if (dAZClothingItem.ready)
				{
					num2++;
				}
			}
		}
		if (_maleClothingItems != null)
		{
			DAZClothingItem[] array4 = _maleClothingItems;
			foreach (DAZClothingItem dAZClothingItem2 in array4)
			{
				if (dAZClothingItem2.ready)
				{
					num2++;
				}
			}
		}
		reports.Add(containingAtom.uid + " loaded clothing items count: " + num2);
		int num3 = 0;
		if (_femaleHairItems != null)
		{
			DAZHairGroup[] array5 = _femaleHairItems;
			foreach (DAZHairGroup dAZHairGroup in array5)
			{
				if (dAZHairGroup.ready)
				{
					num3++;
				}
			}
		}
		if (_maleHairItems != null)
		{
			DAZHairGroup[] array6 = _maleHairItems;
			foreach (DAZHairGroup dAZHairGroup2 in array6)
			{
				if (dAZHairGroup2.ready)
				{
					num3++;
				}
			}
		}
		reports.Add(containingAtom.uid + " loaded hair items count: " + num3);
		reports.Add(containingAtom.uid + " loaded runtime morph count: " + GetRuntimeMorphDeltasLoadedCount());
	}

	protected void OptimizeMemory()
	{
		if (containingAtom.keepParamLocksWhenPuttingBackInPool && containingAtom.inPool)
		{
			if (!base.appearanceLocked && !IsCustomAppearanceParamLocked("character"))
			{
				UnloadInactiveCharacters();
			}
			else
			{
				UnloadDisabledCharacters();
			}
			if (!base.appearanceLocked && !IsCustomAppearanceParamLocked("clothing"))
			{
				UnloadInactiveClothingItems();
			}
			else
			{
				UnloadDisabledClothingItems();
			}
			if (base.appearanceLocked || IsCustomAppearanceParamLocked("hair"))
			{
				UnloadDisabledHairItems();
			}
		}
		else
		{
			UnloadInactiveCharacters();
			UnloadInactiveClothingItems();
			UnloadInactiveHairItems();
		}
		UnloadRuntimeMorphDeltas();
		UnloadDemandActivatedMorphs();
	}

	private void OnDisable()
	{
		if (_selectedCharacter != null)
		{
			DAZCharacter dAZCharacter = _selectedCharacter;
			dAZCharacter.onLoadedHandlers = (JSONStorableDynamic.OnLoaded)Delegate.Remove(dAZCharacter.onLoadedHandlers, new JSONStorableDynamic.OnLoaded(OnCharacterLoaded));
			DAZCharacter dAZCharacter2 = _selectedCharacter;
			dAZCharacter2.onPreloadedHandlers = (JSONStorableDynamic.OnLoaded)Delegate.Remove(dAZCharacter2.onPreloadedHandlers, new JSONStorableDynamic.OnLoaded(OnCharacterPreloaded));
		}
		if (onCharacterLoadedFlag != null)
		{
			onCharacterLoadedFlag.Raise();
			onCharacterLoadedFlag = null;
		}
		if (delayResumeFlag != null)
		{
			delayResumeFlag.Raise();
		}
	}

	private void OnEnable()
	{
		Init();
		if (_selectedCharacter != null && (_selectedCharacter != _loadedCharacter || (_characterRun != null && _characterRun.skin == null)))
		{
			DAZCharacter dAZCharacter = _selectedCharacter;
			dAZCharacter.onLoadedHandlers = (JSONStorableDynamic.OnLoaded)Delegate.Combine(dAZCharacter.onLoadedHandlers, new JSONStorableDynamic.OnLoaded(OnCharacterLoaded));
			DAZCharacter dAZCharacter2 = _selectedCharacter;
			dAZCharacter2.onPreloadedHandlers = (JSONStorableDynamic.OnLoaded)Delegate.Combine(dAZCharacter2.onPreloadedHandlers, new JSONStorableDynamic.OnLoaded(OnCharacterPreloaded));
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			EarlyInit();
			Init();
			InitJSONParams();
			InitUI();
		}
	}

	private void Start()
	{
		SyncGender();
		if (Application.isPlaying)
		{
			SelectCharacterByName(startingCharacterName);
			ResetClothing();
			ResetHair();
			if (MemoryOptimizer.singleton != null)
			{
				MemoryOptimizer.singleton.RegisterMemoryOptimizerListener(OptimizeMemory);
				MemoryOptimizer.singleton.RegisterMemoryOptimizerReporter(ReportMemory);
			}
			FileManager.RegisterRefreshHandler(RefreshDynamicItems);
		}
	}

	private void OnDestroy()
	{
		if (Application.isPlaying)
		{
			if (MemoryOptimizer.singleton != null)
			{
				MemoryOptimizer.singleton.DeregisterMemoryOptimizerListener(OptimizeMemory);
				MemoryOptimizer.singleton.DeregisterMemoryOptimizerReporter(ReportMemory);
			}
			FileManager.UnregisterRefreshHandler(RefreshDynamicItems);
		}
	}
}
