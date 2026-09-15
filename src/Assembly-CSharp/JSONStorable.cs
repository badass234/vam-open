using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine;

public class JSONStorable : MonoBehaviour
{
	public enum Type
	{
		None,
		Bool,
		Float,
		Vector3,
		String,
		Url,
		StringChooser,
		Color,
		Action,
		AudioClipAction,
		SceneFilePathAction,
		PresetFilePathAction,
		StringChooserAction
	}

	public Atom containingAtom;

	public bool exclude;

	public bool onlyStoreIfActive;

	public bool needsStore;

	public string overrideId;

	protected string _subScenePrefix;

	public Transform UITransform;

	public Transform UITransformAlt;

	protected static JSONClass copyStore1;

	protected static JSONClass copyStore2;

	protected static JSONClass copyStore3;

	public JSONStorableAction saveToStore1Action;

	public JSONStorableAction saveToStore2Action;

	public JSONStorableAction saveToStore3Action;

	public JSONStorableAction restoreAllFromStore1Action;

	public JSONStorableAction restorePhysicalFromStore1Action;

	public JSONStorableAction restoreAppearanceFromStore1Action;

	public JSONStorableAction restoreAllFromStore2Action;

	public JSONStorableAction restorePhysicalFromStore2Action;

	public JSONStorableAction restoreAppearanceFromStore2Action;

	public JSONStorableAction restoreAllFromStore3Action;

	public JSONStorableAction restorePhysicalFromStore3Action;

	public JSONStorableAction restoreAppearanceFromStore3Action;

	public JSONStorableAction restoreAllFromDefaultsAction;

	public JSONStorableAction restorePhysicalFromDefaultsAction;

	public JSONStorableAction restoreAppearanceFromDefaultsAction;

	protected Dictionary<string, JSONStorableParam> allParams;

	protected Dictionary<string, JSONStorableParam> allParamsByAltName;

	protected List<string> allParamAndActionNames;

	protected List<string> allFloatAndColorParamNames;

	protected HashSet<string> physicalLocks;

	protected HashSet<string> appearanceLocks;

	protected Dictionary<string, HashSet<string>> customAppearanceParamLocks;

	protected Dictionary<string, HashSet<string>> customPhysicalParamLocks;

	protected Dictionary<string, JSONStorableBool> boolParams;

	protected List<string> boolParamNames;

	protected Dictionary<string, JSONStorableFloat> floatParams;

	protected Dictionary<string, JSONStorableFloat> floatParamsByAltName;

	protected List<string> floatParamNames;

	protected List<string> floatParamNamesIncludingHidden;

	protected Dictionary<string, JSONStorableVector3> vector3Params;

	protected List<string> vector3ParamNames;

	protected Dictionary<string, JSONStorableString> stringParams;

	protected List<string> stringParamNames;

	protected Dictionary<string, JSONStorableStringChooser> stringChooserParams;

	protected List<string> stringChooserParamNames;

	protected Dictionary<string, JSONStorableUrl> urlParams;

	protected List<string> urlParamNames;

	protected Dictionary<string, JSONStorableColor> colorParams;

	protected List<string> colorParamNames;

	protected HSVColor hsvc;

	protected Dictionary<string, JSONStorableAction> actions;

	protected List<string> actionNames;

	protected Dictionary<string, JSONStorableActionAudioClip> audioClipActions;

	protected List<string> audioClipActionNames;

	protected Dictionary<string, JSONStorableActionStringChooser> stringChooserActions;

	protected List<string> stringChooserActionNames;

	protected Dictionary<string, JSONStorableActionSceneFilePath> sceneFilePathActions;

	protected List<string> sceneFilePathActionNames;

	protected Dictionary<string, JSONStorableActionPresetFilePath> presetFilePathActions;

	protected List<string> presetFilePathActionNames;

	protected bool _isPresetStore;

	protected bool _isPresetRestore;

	protected bool insideRestore;

	protected bool _JSONWasInit;

	protected bool awakecalled;

	public string storeId
	{
		get
		{
			if (overrideId == null || overrideId == string.Empty)
			{
				return base.name;
			}
			if (overrideId[0] == '+')
			{
				if (overrideId.StartsWith("+parent+", StringComparison.CurrentCultureIgnoreCase))
				{
					if (base.transform.parent != null)
					{
						return base.transform.parent.name + base.name + overrideId.Substring(8);
					}
					return base.name + overrideId.Substring(8);
				}
				return base.name + overrideId.Substring(1);
			}
			return overrideId;
		}
	}

	public string subScenePrefix
	{
		get
		{
			return _subScenePrefix;
		}
		set
		{
			if (_subScenePrefix != value)
			{
				_subScenePrefix = value;
			}
		}
	}

	public bool physicalLocked { get; protected set; }

	public bool appearanceLocked { get; protected set; }

	public bool mergeRestore { get; set; }

	public bool isPresetStore
	{
		get
		{
			return _isPresetStore;
		}
		set
		{
			_isPresetStore = value;
		}
	}

	public bool isPresetRestore
	{
		get
		{
			return _isPresetRestore;
		}
		set
		{
			_isPresetRestore = value;
		}
	}

	public string AtomUidToStoreAtomUid(string atomUid)
	{
		if (atomUid != null && subScenePrefix != null)
		{
			string text = "^" + subScenePrefix;
			if (Regex.IsMatch(atomUid, text + "[^/]+$"))
			{
				return Regex.Replace(atomUid, text, string.Empty);
			}
			return null;
		}
		return atomUid;
	}

	public string StoredAtomUidToAtomUid(string storedAtomUid)
	{
		if (subScenePrefix != null)
		{
			return subScenePrefix + storedAtomUid;
		}
		return storedAtomUid;
	}

	protected virtual void InitUI(Transform t, bool isAlt)
	{
	}

	public virtual void SetUI(Transform t)
	{
		if (UITransform != t)
		{
			UITransform = t;
			InitUI();
		}
	}

	public virtual void InitUI()
	{
		InitUI(UITransform, false);
	}

	public virtual void SetUIAlt(Transform t)
	{
		if (UITransformAlt != t)
		{
			UITransformAlt = t;
			InitUIAlt();
		}
	}

	public virtual void InitUIAlt()
	{
		InitUI(UITransformAlt, false);
	}

	public void RestoreAllFromDefaults()
	{
		RestoreFromJSON(new JSONClass());
	}

	public void RestorePhysicalFromDefaults()
	{
		RestoreFromJSON(new JSONClass(), true, false);
	}

	public void RestoreAppearanceFromDefaults()
	{
		RestoreFromJSON(new JSONClass(), false);
	}

	public void RestoreFromStore1(bool restorePhysical = true, bool restoreAppearance = true)
	{
		if (copyStore1 != null)
		{
			RestoreFromJSON(copyStore1, restorePhysical, restoreAppearance);
		}
	}

	public void RestoreAllFromStore1()
	{
		RestoreFromStore1();
	}

	public void RestorePhysicalFromStore1()
	{
		RestoreFromStore1(true, false);
	}

	public void RestoreAppearanceFromStore1()
	{
		RestoreFromStore1(false);
	}

	public void SaveToStore1()
	{
		copyStore1 = GetJSON(true, true, true);
	}

	public void RestoreFromStore2(bool restorePhysical = true, bool restoreAppearance = true)
	{
		if (copyStore2 != null)
		{
			RestoreFromJSON(copyStore2, restorePhysical, restoreAppearance);
		}
	}

	public void RestoreAllFromStore2()
	{
		RestoreFromStore2();
	}

	public void RestorePhysicalFromStore2()
	{
		RestoreFromStore2(true, false);
	}

	public void RestoreAppearanceFromStore2()
	{
		RestoreFromStore2(false);
	}

	public void SaveToStore2()
	{
		copyStore2 = GetJSON(true, true, true);
	}

	public void RestoreFromStore3(bool restorePhysical = true, bool restoreAppearance = true)
	{
		if (copyStore3 != null)
		{
			RestoreFromJSON(copyStore3, restorePhysical, restoreAppearance);
		}
	}

	public void RestoreAllFromStore3()
	{
		RestoreFromStore3();
	}

	public void RestorePhysicalFromStore3()
	{
		RestoreFromStore3(true, false);
	}

	public void RestoreAppearanceFromStore3()
	{
		RestoreFromStore3(false);
	}

	public void SaveToStore3()
	{
		copyStore3 = GetJSON(true, true, true);
	}

	public bool HasParamsOrActions()
	{
		if (!awakecalled)
		{
			Awake();
		}
		if (allParams.Count > 0 || actionNames.Count > 0 || audioClipActionNames.Count > 0 || stringChooserActionNames.Count > 0 || sceneFilePathActionNames.Count > 0 || presetFilePathActionNames.Count > 0)
		{
			return true;
		}
		return false;
	}

	public List<string> GetAllParamAndActionNames()
	{
		if (!awakecalled)
		{
			Awake();
		}
		return allParamAndActionNames;
	}

	public List<string> GetAllFloatAndColorParamNames()
	{
		if (!awakecalled)
		{
			Awake();
		}
		return allFloatAndColorParamNames;
	}

	public Type GetParamOrActionType(string name)
	{
		if (!awakecalled)
		{
			Awake();
		}
		JSONStorableParam value;
		if (allParams.TryGetValue(name, out value))
		{
			return value.type;
		}
		if (allParamsByAltName.TryGetValue(name, out value))
		{
			return value.type;
		}
		if (IsAction(name))
		{
			return Type.Action;
		}
		if (IsAudioClipAction(name))
		{
			return Type.AudioClipAction;
		}
		if (IsStringChooserAction(name))
		{
			return Type.StringChooserAction;
		}
		if (IsSceneFilePathAction(name))
		{
			return Type.SceneFilePathAction;
		}
		if (IsPresetFilePathAction(name))
		{
			return Type.PresetFilePathAction;
		}
		return Type.None;
	}

	public JSONStorableParam GetParam(string name)
	{
		JSONStorableParam value = null;
		if (allParams.TryGetValue(name, out value))
		{
			return value;
		}
		if (allParamsByAltName.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual string[] GetCustomParamNames()
	{
		return null;
	}

	public void SetPhysicalLock(string physicalLockUid)
	{
		if (physicalLocks == null)
		{
			physicalLocks = new HashSet<string>();
		}
		if (!physicalLocks.Contains(physicalLockUid))
		{
			physicalLocks.Add(physicalLockUid);
		}
		physicalLocked = true;
	}

	public void ClearPhysicalLock(string physicalLockUid)
	{
		if (physicalLocks != null)
		{
			physicalLocks.Remove(physicalLockUid);
			if (physicalLocks.Count == 0)
			{
				physicalLocked = false;
			}
		}
	}

	public void SetAppearanceLock(string appearanceLockUid)
	{
		if (appearanceLocks == null)
		{
			appearanceLocks = new HashSet<string>();
		}
		if (!appearanceLocks.Contains(appearanceLockUid))
		{
			appearanceLocks.Add(appearanceLockUid);
		}
		appearanceLocked = true;
	}

	public void ClearAppearanceLock(string appearanceLockUid)
	{
		if (appearanceLocks != null)
		{
			appearanceLocks.Remove(appearanceLockUid);
			if (appearanceLocks.Count == 0)
			{
				appearanceLocked = false;
			}
		}
	}

	public void SetCustomAppearanceParamLock(string paramName, string lockUid)
	{
		if (customAppearanceParamLocks == null)
		{
			customAppearanceParamLocks = new Dictionary<string, HashSet<string>>();
		}
		HashSet<string> value;
		if (!customAppearanceParamLocks.TryGetValue(paramName, out value))
		{
			value = new HashSet<string>();
			customAppearanceParamLocks.Add(paramName, value);
		}
		if (!value.Contains(lockUid))
		{
			value.Add(lockUid);
		}
	}

	public bool IsCustomAppearanceParamLocked(string paramName)
	{
		if (customAppearanceParamLocks == null)
		{
			return false;
		}
		HashSet<string> value;
		if (customAppearanceParamLocks.TryGetValue(paramName, out value))
		{
			if (value.Count > 0)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public void ClearCustomAppearanceParamLock(string paramName, string lockUid)
	{
		HashSet<string> value;
		if (customAppearanceParamLocks != null && customAppearanceParamLocks.TryGetValue(paramName, out value))
		{
			value.Remove(lockUid);
		}
	}

	public void SetCustomPhysicalParamLock(string paramName, string lockUid)
	{
		if (customPhysicalParamLocks == null)
		{
			customPhysicalParamLocks = new Dictionary<string, HashSet<string>>();
		}
		HashSet<string> value;
		if (!customPhysicalParamLocks.TryGetValue(paramName, out value))
		{
			value = new HashSet<string>();
			customPhysicalParamLocks.Add(paramName, value);
		}
		if (!value.Contains(lockUid))
		{
			value.Add(lockUid);
		}
	}

	public bool IsCustomPhysicalParamLocked(string paramName)
	{
		if (customPhysicalParamLocks == null)
		{
			return false;
		}
		HashSet<string> value;
		if (customPhysicalParamLocks.TryGetValue(paramName, out value))
		{
			if (value.Count > 0)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public void ClearCustomPhysicalParamLock(string paramName, string lockUid)
	{
		HashSet<string> value;
		if (customPhysicalParamLocks != null && customPhysicalParamLocks.TryGetValue(paramName, out value))
		{
			value.Remove(lockUid);
		}
	}

	public void ClearAllLocks()
	{
		if (physicalLocks != null)
		{
			physicalLocks.Clear();
		}
		physicalLocked = false;
		if (appearanceLocks != null)
		{
			appearanceLocks.Clear();
		}
		appearanceLocked = false;
		if (customAppearanceParamLocks != null)
		{
			customAppearanceParamLocks.Clear();
		}
		if (customPhysicalParamLocks != null)
		{
			customPhysicalParamLocks.Clear();
		}
	}

	protected void InitBoolParams()
	{
		boolParams = new Dictionary<string, JSONStorableBool>();
		boolParamNames = new List<string>();
	}

	public void RegisterBool(JSONStorableBool param)
	{
		if (boolParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
		}
		else
		{
			boolParams.Add(param.name, param);
			if (!param.hidden)
			{
				boolParamNames.Add(param.name);
				allParamAndActionNames.Add(param.name);
			}
			param.storable = this;
		}
		if (!allParams.ContainsKey(param.name))
		{
			allParams.Add(param.name, param);
		}
	}

	public void DeregisterBool(JSONStorableBool param)
	{
		if (param.storable == this)
		{
			if (!boolParams.ContainsKey(param.name))
			{
				Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
				return;
			}
			allParams.Remove(param.name);
			boolParams.Remove(param.name);
			boolParamNames.Remove(param.name);
			allParamAndActionNames.Remove(param.name);
			param.storable = null;
		}
	}

	public List<string> GetBoolParamNames()
	{
		return boolParamNames;
	}

	public virtual bool IsBoolJSONParam(string name)
	{
		return boolParams.ContainsKey(name);
	}

	public JSONStorableBool GetBoolJSONParam(string name)
	{
		JSONStorableBool value;
		if (boolParams.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual bool GetBoolParamValue(string param)
	{
		JSONStorableBool value;
		if (boolParams.TryGetValue(param, out value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		return false;
	}

	public virtual void SetBoolParamValue(string param, bool value)
	{
		JSONStorableBool value2;
		if (boolParams.TryGetValue(param, out value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	protected void InitFloatParams()
	{
		floatParams = new Dictionary<string, JSONStorableFloat>();
		floatParamsByAltName = new Dictionary<string, JSONStorableFloat>();
		floatParamNames = new List<string>();
		floatParamNamesIncludingHidden = new List<string>();
	}

	public void RegisterFloat(JSONStorableFloat param)
	{
		if (floatParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
		}
		else
		{
			floatParams.Add(param.name, param);
			if (param.altName != null && !floatParamsByAltName.ContainsKey(param.altName))
			{
				floatParamsByAltName.Add(param.altName, param);
				param.registeredAltName = true;
				if (!allParamsByAltName.ContainsKey(param.altName))
				{
					allParamsByAltName.Add(param.altName, param);
				}
			}
			if (!param.hidden)
			{
				floatParamNames.Add(param.name);
				allParamAndActionNames.Add(param.name);
				allFloatAndColorParamNames.Add(param.name);
			}
			floatParamNamesIncludingHidden.Add(param.name);
			param.storable = this;
		}
		if (!allParams.ContainsKey(param.name))
		{
			allParams.Add(param.name, param);
		}
	}

	public void DeregisterFloat(JSONStorableFloat param)
	{
		if (!(param.storable == this))
		{
			return;
		}
		if (!floatParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
			return;
		}
		allParams.Remove(param.name);
		floatParams.Remove(param.name);
		floatParamNames.Remove(param.name);
		floatParamNamesIncludingHidden.Remove(param.name);
		allParamAndActionNames.Remove(param.name);
		allFloatAndColorParamNames.Remove(param.name);
		if (param.registeredAltName)
		{
			floatParamsByAltName.Remove(param.altName);
			allParamsByAltName.Remove(param.altName);
			param.registeredAltName = false;
		}
		param.storable = null;
	}

	public void MassDeregister(HashSet<string> paramNamesToDeregister)
	{
		List<string> list = new List<string>();
		foreach (string item in floatParamNamesIncludingHidden)
		{
			if (paramNamesToDeregister.Contains(item))
			{
				JSONStorableFloat value;
				if (floatParams.TryGetValue(item, out value))
				{
					value.storable = null;
					allParams.Remove(item);
					floatParams.Remove(item);
					if (value.registeredAltName)
					{
						floatParamsByAltName.Remove(value.altName);
						allParamsByAltName.Remove(value.altName);
						value.registeredAltName = false;
					}
				}
			}
			else
			{
				list.Add(item);
			}
		}
		floatParamNamesIncludingHidden = list;
		List<string> list2 = new List<string>();
		foreach (string floatParamName in floatParamNames)
		{
			if (!paramNamesToDeregister.Contains(floatParamName))
			{
				list2.Add(floatParamName);
			}
		}
		floatParamNames = list2;
		List<string> list3 = new List<string>();
		foreach (string allParamAndActionName in allParamAndActionNames)
		{
			if (!paramNamesToDeregister.Contains(allParamAndActionName))
			{
				list3.Add(allParamAndActionName);
			}
		}
		allParamAndActionNames = list3;
		List<string> list4 = new List<string>();
		foreach (string allFloatAndColorParamName in allFloatAndColorParamNames)
		{
			if (!paramNamesToDeregister.Contains(allFloatAndColorParamName))
			{
				list4.Add(allFloatAndColorParamName);
			}
		}
		allFloatAndColorParamNames = list4;
	}

	public List<string> GetFloatParamNames()
	{
		return floatParamNames;
	}

	public virtual bool IsFloatJSONParam(string name)
	{
		return floatParams.ContainsKey(name) || floatParamsByAltName.ContainsKey(name);
	}

	public JSONStorableFloat GetFloatJSONParam(string name)
	{
		JSONStorableFloat value;
		if (floatParams.TryGetValue(name, out value) || floatParamsByAltName.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual float GetFloatParamValue(string param)
	{
		JSONStorableFloat value;
		if (floatParams.TryGetValue(param, out value) || floatParamsByAltName.TryGetValue(param, out value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		return 0f;
	}

	public virtual void SetFloatParamValue(string param, float value)
	{
		JSONStorableFloat value2;
		if (floatParams.TryGetValue(param, out value2) || floatParamsByAltName.TryGetValue(param, out value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	public virtual float GetFloatJSONParamMinValue(string param)
	{
		JSONStorableFloat value;
		if (floatParams.TryGetValue(param, out value) || floatParamsByAltName.TryGetValue(param, out value))
		{
			return value.min;
		}
		Debug.LogError("Tried to get min param value for param " + param + " that does not exist");
		return 0f;
	}

	public virtual float GetFloatJSONParamMaxValue(string param)
	{
		JSONStorableFloat value;
		if (floatParams.TryGetValue(param, out value) || floatParamsByAltName.TryGetValue(param, out value))
		{
			return value.max;
		}
		Debug.LogError("Tried to get max param value for param " + param + " that does not exist");
		return 0f;
	}

	protected void InitVector3Params()
	{
		vector3Params = new Dictionary<string, JSONStorableVector3>();
		vector3ParamNames = new List<string>();
	}

	public void RegisterVector3(JSONStorableVector3 param)
	{
		if (vector3Params.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
		}
		else
		{
			vector3Params.Add(param.name, param);
			if (!param.hidden)
			{
				vector3ParamNames.Add(param.name);
				allParamAndActionNames.Add(param.name);
			}
			param.storable = this;
		}
		if (!allParams.ContainsKey(param.name))
		{
			allParams.Add(param.name, param);
		}
	}

	public void DeregisterVector3(JSONStorableVector3 param)
	{
		if (param.storable == this)
		{
			if (!vector3Params.ContainsKey(param.name))
			{
				Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
				return;
			}
			allParams.Remove(param.name);
			vector3Params.Remove(param.name);
			vector3ParamNames.Remove(param.name);
			allParamAndActionNames.Remove(param.name);
			param.storable = null;
		}
	}

	public List<string> GetVector3ParamNames()
	{
		return vector3ParamNames;
	}

	public virtual bool IsVector3JSONParam(string name)
	{
		return vector3Params.ContainsKey(name);
	}

	public JSONStorableVector3 GetVector3JSONParam(string name)
	{
		JSONStorableVector3 value;
		if (vector3Params.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual Vector3 GetVector3ParamValue(string param)
	{
		JSONStorableVector3 value;
		if (vector3Params.TryGetValue(param, out value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		return Vector3.zero;
	}

	public virtual void SetVector3ParamValue(string param, Vector3 value)
	{
		JSONStorableVector3 value2;
		if (vector3Params.TryGetValue(param, out value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	public virtual Vector3 GetVector3JSONParamMinValue(string param)
	{
		JSONStorableVector3 value;
		if (vector3Params.TryGetValue(param, out value))
		{
			return value.min;
		}
		Debug.LogError("Tried to get min param value for param " + param + " that does not exist");
		return Vector3.zero;
	}

	public virtual Vector3 GetVector3JSONParamMaxValue(string param)
	{
		JSONStorableVector3 value;
		if (vector3Params.TryGetValue(param, out value))
		{
			return value.max;
		}
		Debug.LogError("Tried to get max param value for param " + param + " that does not exist");
		return Vector3.zero;
	}

	protected void InitStringParams()
	{
		stringParams = new Dictionary<string, JSONStorableString>();
		stringParamNames = new List<string>();
	}

	public void RegisterString(JSONStorableString param)
	{
		if (stringParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
		}
		else
		{
			stringParams.Add(param.name, param);
			if (!param.hidden)
			{
				stringParamNames.Add(param.name);
				allParamAndActionNames.Add(param.name);
			}
			param.storable = this;
		}
		if (!allParams.ContainsKey(param.name))
		{
			allParams.Add(param.name, param);
		}
	}

	public void DeregisterString(JSONStorableString param)
	{
		if (param.storable == this)
		{
			if (!stringParams.ContainsKey(param.name))
			{
				Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
				return;
			}
			allParams.Remove(param.name);
			stringParams.Remove(param.name);
			stringParamNames.Remove(param.name);
			allParamAndActionNames.Remove(param.name);
			param.storable = null;
		}
	}

	public List<string> GetStringParamNames()
	{
		return stringParamNames;
	}

	public virtual bool IsStringJSONParam(string name)
	{
		return stringParams.ContainsKey(name);
	}

	public JSONStorableString GetStringJSONParam(string name)
	{
		JSONStorableString value;
		if (stringParams.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual string GetStringParamValue(string param)
	{
		JSONStorableString value;
		if (stringParams.TryGetValue(param, out value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		return null;
	}

	public virtual void SetStringParamValue(string param, string value)
	{
		JSONStorableString value2;
		if (stringParams.TryGetValue(param, out value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	protected void InitStringChooserParams()
	{
		stringChooserParams = new Dictionary<string, JSONStorableStringChooser>();
		stringChooserParamNames = new List<string>();
	}

	public void RegisterStringChooser(JSONStorableStringChooser param)
	{
		if (stringChooserParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
		}
		else
		{
			stringChooserParams.Add(param.name, param);
			if (!param.hidden)
			{
				stringChooserParamNames.Add(param.name);
				allParamAndActionNames.Add(param.name);
			}
			param.storable = this;
		}
		if (!allParams.ContainsKey(param.name))
		{
			allParams.Add(param.name, param);
		}
	}

	public void DeregisterStringChooser(JSONStorableStringChooser param)
	{
		if (param.storable == this)
		{
			if (!stringChooserParams.ContainsKey(param.name))
			{
				Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
				return;
			}
			allParams.Remove(param.name);
			stringChooserParams.Remove(param.name);
			stringChooserParamNames.Remove(param.name);
			allParamAndActionNames.Remove(param.name);
			param.storable = null;
		}
	}

	public List<string> GetStringChooserParamNames()
	{
		return stringChooserParamNames;
	}

	public virtual bool IsStringChooserJSONParam(string name)
	{
		return stringChooserParams.ContainsKey(name);
	}

	public JSONStorableStringChooser GetStringChooserJSONParam(string name)
	{
		JSONStorableStringChooser value;
		if (stringChooserParams.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual string GetStringChooserParamValue(string param)
	{
		JSONStorableStringChooser value;
		if (stringChooserParams.TryGetValue(param, out value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		return null;
	}

	public virtual void SetStringChooserParamValue(string param, string value)
	{
		JSONStorableStringChooser value2;
		if (stringChooserParams.TryGetValue(param, out value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	public virtual List<string> GetStringChooserJSONParamChoices(string param)
	{
		JSONStorableStringChooser value;
		if (stringChooserParams.TryGetValue(param, out value))
		{
			return value.choices;
		}
		Debug.LogError("Tried to get param choices for param " + param + " that does not exist");
		return null;
	}

	protected void InitUrlParams()
	{
		urlParams = new Dictionary<string, JSONStorableUrl>();
		urlParamNames = new List<string>();
	}

	public void RegisterUrl(JSONStorableUrl param)
	{
		if (urlParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
		}
		else
		{
			urlParams.Add(param.name, param);
			if (!param.hidden)
			{
				urlParamNames.Add(param.name);
				allParamAndActionNames.Add(param.name);
			}
			param.storable = this;
		}
		if (!allParams.ContainsKey(param.name))
		{
			allParams.Add(param.name, param);
		}
	}

	public void DeregisterUrl(JSONStorableUrl param)
	{
		if (param.storable == this)
		{
			if (!urlParams.ContainsKey(param.name))
			{
				Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
				return;
			}
			allParams.Remove(param.name);
			urlParams.Remove(param.name);
			urlParamNames.Remove(param.name);
			allParamAndActionNames.Remove(param.name);
			param.storable = null;
		}
	}

	public List<string> GetUrlParamNames()
	{
		return urlParamNames;
	}

	public virtual bool IsUrlJSONParam(string name)
	{
		return urlParams.ContainsKey(name);
	}

	public JSONStorableUrl GetUrlJSONParam(string name)
	{
		JSONStorableUrl value;
		if (urlParams.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual string GetUrlParamValue(string param)
	{
		JSONStorableUrl value;
		if (urlParams.TryGetValue(param, out value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		return null;
	}

	public virtual void SetUrlParamValue(string param, string value)
	{
		JSONStorableUrl value2;
		if (urlParams.TryGetValue(param, out value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	protected void InitColorParams()
	{
		colorParams = new Dictionary<string, JSONStorableColor>();
		colorParamNames = new List<string>();
	}

	public void RegisterColor(JSONStorableColor param)
	{
		if (colorParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
		}
		else
		{
			colorParams.Add(param.name, param);
			if (!param.hidden)
			{
				colorParamNames.Add(param.name);
				allParamAndActionNames.Add(param.name);
				allFloatAndColorParamNames.Add(param.name);
			}
			param.storable = this;
		}
		if (!allParams.ContainsKey(param.name))
		{
			allParams.Add(param.name, param);
		}
	}

	public void DeregisterColor(JSONStorableColor param)
	{
		if (param.storable == this)
		{
			if (!colorParams.ContainsKey(param.name))
			{
				Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
				return;
			}
			allParams.Remove(param.name);
			colorParams.Remove(param.name);
			colorParamNames.Remove(param.name);
			allParamAndActionNames.Remove(param.name);
			allFloatAndColorParamNames.Remove(param.name);
			param.storable = null;
		}
	}

	public List<string> GetColorParamNames()
	{
		return colorParamNames;
	}

	public virtual bool IsColorJSONParam(string name)
	{
		return colorParams.ContainsKey(name);
	}

	public JSONStorableColor GetColorJSONParam(string name)
	{
		JSONStorableColor value;
		if (colorParams.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual HSVColor GetColorParamValue(string param)
	{
		JSONStorableColor value;
		if (colorParams.TryGetValue(param, out value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		hsvc.H = 0f;
		hsvc.S = 0f;
		hsvc.V = 0f;
		return hsvc;
	}

	public virtual void SetColorParamValue(string param, HSVColor value)
	{
		JSONStorableColor value2;
		if (colorParams.TryGetValue(param, out value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	protected void InitActions()
	{
		actions = new Dictionary<string, JSONStorableAction>();
		actionNames = new List<string>();
	}

	public void RegisterAction(JSONStorableAction action)
	{
		if (actions.ContainsKey(action.name))
		{
			Debug.LogError("Tried registering action " + action.name + " that already exists");
			return;
		}
		actions.Add(action.name, action);
		actionNames.Add(action.name);
		allParamAndActionNames.Add(action.name);
		action.storable = this;
	}

	public void DeregisterAction(JSONStorableAction action)
	{
		if (action.storable == this)
		{
			if (!actions.ContainsKey(action.name))
			{
				Debug.LogError("Tried deregistering action " + action.name + " that does not exist");
				return;
			}
			actions.Remove(action.name);
			actionNames.Remove(action.name);
			allParamAndActionNames.Remove(action.name);
			action.storable = null;
		}
	}

	public List<string> GetActionNames()
	{
		return actionNames;
	}

	public virtual bool IsAction(string name)
	{
		return actions.ContainsKey(name);
	}

	public JSONStorableAction GetAction(string name)
	{
		JSONStorableAction value;
		if (actions.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual void CallAction(string actionName)
	{
		JSONStorableAction value;
		if (actions.TryGetValue(actionName, out value))
		{
			if (value.actionCallback != null)
			{
				value.actionCallback();
			}
		}
		else
		{
			Debug.LogError("Tried to call action " + actionName + " that does not exist");
		}
	}

	protected void InitAudioClipActions()
	{
		audioClipActions = new Dictionary<string, JSONStorableActionAudioClip>();
		audioClipActionNames = new List<string>();
	}

	public void RegisterAudioClipAction(JSONStorableActionAudioClip action)
	{
		if (audioClipActions.ContainsKey(action.name))
		{
			Debug.LogError("Tried registering action " + action.name + " that already exists");
			return;
		}
		audioClipActions.Add(action.name, action);
		audioClipActionNames.Add(action.name);
		allParamAndActionNames.Add(action.name);
		action.storable = this;
	}

	public void DeregisterAudioClipAction(JSONStorableActionAudioClip action)
	{
		if (action.storable == this)
		{
			if (!actions.ContainsKey(action.name))
			{
				Debug.LogError("Tried deregistering action " + action.name + " that does not exist");
				return;
			}
			audioClipActions.Remove(action.name);
			audioClipActionNames.Remove(action.name);
			allParamAndActionNames.Remove(action.name);
			action.storable = null;
		}
	}

	public List<string> GetAudioClipActionNames()
	{
		return audioClipActionNames;
	}

	public virtual bool IsAudioClipAction(string name)
	{
		return audioClipActions.ContainsKey(name);
	}

	public JSONStorableActionAudioClip GetAudioClipAction(string name)
	{
		JSONStorableActionAudioClip value;
		if (audioClipActions.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual void CallAction(string actionName, NamedAudioClip nac)
	{
		JSONStorableActionAudioClip value;
		if (audioClipActions.TryGetValue(actionName, out value))
		{
			if (value.actionCallback != null)
			{
				value.actionCallback(nac);
			}
		}
		else
		{
			Debug.LogError("Tried to call action " + actionName + " that does not exist");
		}
	}

	protected void InitStringChooserActions()
	{
		stringChooserActions = new Dictionary<string, JSONStorableActionStringChooser>();
		stringChooserActionNames = new List<string>();
	}

	public void RegisterStringChooserAction(JSONStorableActionStringChooser action)
	{
		if (stringChooserActions.ContainsKey(action.name))
		{
			Debug.LogError("Tried registering action " + action.name + " that already exists");
			return;
		}
		stringChooserActions.Add(action.name, action);
		stringChooserActionNames.Add(action.name);
		allParamAndActionNames.Add(action.name);
		action.storable = this;
	}

	public void DeregisterStringChooserAction(JSONStorableActionStringChooser action)
	{
		if (action.storable == this)
		{
			if (!actions.ContainsKey(action.name))
			{
				Debug.LogError("Tried deregistering action " + action.name + " that does not exist");
				return;
			}
			stringChooserActions.Remove(action.name);
			stringChooserActionNames.Remove(action.name);
			allParamAndActionNames.Remove(action.name);
			action.storable = null;
		}
	}

	public List<string> GetStringChooserActionNames()
	{
		return stringChooserActionNames;
	}

	public virtual bool IsStringChooserAction(string name)
	{
		return stringChooserActions.ContainsKey(name);
	}

	public JSONStorableActionStringChooser GetStringChooserAction(string name)
	{
		JSONStorableActionStringChooser value;
		if (stringChooserActions.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual void CallStringChooserAction(string actionName, string choice)
	{
		JSONStorableActionStringChooser value;
		if (stringChooserActions.TryGetValue(actionName, out value))
		{
			if (value.actionCallback != null)
			{
				value.actionCallback(choice);
			}
		}
		else
		{
			Debug.LogError("Tried to call action " + actionName + " that does not exist");
		}
	}

	protected void InitSceneFilePathActions()
	{
		sceneFilePathActions = new Dictionary<string, JSONStorableActionSceneFilePath>();
		sceneFilePathActionNames = new List<string>();
	}

	public void RegisterSceneFilePathAction(JSONStorableActionSceneFilePath action)
	{
		if (sceneFilePathActions.ContainsKey(action.name))
		{
			Debug.LogError("Tried registering action " + action.name + " that already exists");
			return;
		}
		sceneFilePathActions.Add(action.name, action);
		sceneFilePathActionNames.Add(action.name);
		allParamAndActionNames.Add(action.name);
		action.storable = this;
	}

	public void DeregisterSceneFilePathAction(JSONStorableActionSceneFilePath action)
	{
		if (action.storable == this)
		{
			if (!actions.ContainsKey(action.name))
			{
				Debug.LogError("Tried deregistering action " + action.name + " that does not exist");
				return;
			}
			sceneFilePathActions.Remove(action.name);
			sceneFilePathActionNames.Remove(action.name);
			allParamAndActionNames.Remove(action.name);
			action.storable = null;
		}
	}

	public List<string> GetSceneFilePathActionNames()
	{
		return sceneFilePathActionNames;
	}

	public virtual bool IsSceneFilePathAction(string name)
	{
		return sceneFilePathActions.ContainsKey(name);
	}

	public JSONStorableActionSceneFilePath GetSceneFilePathAction(string name)
	{
		JSONStorableActionSceneFilePath value;
		if (sceneFilePathActions.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual void CallAction(string actionName, string path)
	{
		JSONStorableActionSceneFilePath value;
		if (sceneFilePathActions.TryGetValue(actionName, out value))
		{
			if (value.actionCallback != null)
			{
				value.actionCallback(path);
			}
		}
		else
		{
			Debug.LogError("Tried to call action " + actionName + " that does not exist");
		}
	}

	protected void InitPresetFilePathActions()
	{
		presetFilePathActions = new Dictionary<string, JSONStorableActionPresetFilePath>();
		presetFilePathActionNames = new List<string>();
	}

	public void RegisterPresetFilePathAction(JSONStorableActionPresetFilePath action)
	{
		if (presetFilePathActions.ContainsKey(action.name))
		{
			Debug.LogError("Tried registering action " + action.name + " that already exists");
			return;
		}
		presetFilePathActions.Add(action.name, action);
		presetFilePathActionNames.Add(action.name);
		allParamAndActionNames.Add(action.name);
		action.storable = this;
	}

	public void DeregisterPresetFilePathAction(JSONStorableActionPresetFilePath action)
	{
		if (action.storable == this)
		{
			if (!actions.ContainsKey(action.name))
			{
				Debug.LogError("Tried deregistering action " + action.name + " that does not exist");
				return;
			}
			presetFilePathActions.Remove(action.name);
			presetFilePathActionNames.Remove(action.name);
			allParamAndActionNames.Remove(action.name);
			action.storable = null;
		}
	}

	public List<string> GetPresetFilePathActionNames()
	{
		return presetFilePathActionNames;
	}

	public virtual bool IsPresetFilePathAction(string name)
	{
		return presetFilePathActions.ContainsKey(name);
	}

	public JSONStorableActionPresetFilePath GetPresetFilePathAction(string name)
	{
		JSONStorableActionPresetFilePath value;
		if (presetFilePathActions.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public virtual void CallPresetFileAction(string actionName, string path)
	{
		JSONStorableActionPresetFilePath value;
		if (presetFilePathActions.TryGetValue(actionName, out value))
		{
			if (value.actionCallback != null)
			{
				value.actionCallback(path);
			}
		}
		else
		{
			Debug.LogError("Tried to call action " + actionName + " that does not exist");
		}
	}

	public virtual JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		if (!awakecalled)
		{
			Awake();
		}
		JSONClass jSONClass = new JSONClass();
		jSONClass["id"] = storeId;
		needsStore = false;
		foreach (string boolParamName in boolParamNames)
		{
			JSONStorableBool value;
			if (boolParams.TryGetValue(boolParamName, out value) && value.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		foreach (string floatParamName in floatParamNames)
		{
			JSONStorableFloat value2;
			if (floatParams.TryGetValue(floatParamName, out value2) && value2.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		foreach (string vector3ParamName in vector3ParamNames)
		{
			JSONStorableVector3 value3;
			if (vector3Params.TryGetValue(vector3ParamName, out value3) && value3.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		foreach (string stringParamName in stringParamNames)
		{
			JSONStorableString value4;
			if (stringParams.TryGetValue(stringParamName, out value4) && value4.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		foreach (string stringChooserParamName in stringChooserParamNames)
		{
			JSONStorableStringChooser value5;
			if (stringChooserParams.TryGetValue(stringChooserParamName, out value5) && value5.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		foreach (string urlParamName in urlParamNames)
		{
			JSONStorableUrl value6;
			if (urlParams.TryGetValue(urlParamName, out value6) && value6.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		foreach (string colorParamName in colorParamNames)
		{
			JSONStorableColor value7;
			if (colorParams.TryGetValue(colorParamName, out value7) && value7.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		return jSONClass;
	}

	public virtual void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		insideRestore = true;
		if (!awakecalled)
		{
			Awake();
		}
		bool flag = restorePhysical && !physicalLocked;
		bool flag2 = restoreAppearance && !appearanceLocked;
		if (flag || flag2)
		{
			foreach (string boolParamName in boolParamNames)
			{
				JSONStorableBool value;
				if (boolParams.TryGetValue(boolParamName, out value))
				{
					value.RestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
			foreach (string floatParamName in floatParamNames)
			{
				JSONStorableFloat value2;
				if (floatParams.TryGetValue(floatParamName, out value2))
				{
					value2.RestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
			foreach (string vector3ParamName in vector3ParamNames)
			{
				JSONStorableVector3 value3;
				if (vector3Params.TryGetValue(vector3ParamName, out value3))
				{
					value3.RestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
			foreach (string stringParamName in stringParamNames)
			{
				JSONStorableString value4;
				if (stringParams.TryGetValue(stringParamName, out value4))
				{
					value4.RestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
			foreach (string stringChooserParamName in stringChooserParamNames)
			{
				JSONStorableStringChooser value5;
				if (stringChooserParams.TryGetValue(stringChooserParamName, out value5))
				{
					value5.RestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
			foreach (string urlParamName in urlParamNames)
			{
				JSONStorableUrl value6;
				if (urlParams.TryGetValue(urlParamName, out value6))
				{
					value6.RestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
			foreach (string colorParamName in colorParamNames)
			{
				JSONStorableColor value7;
				if (colorParams.TryGetValue(colorParamName, out value7))
				{
					value7.RestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
		}
		insideRestore = false;
	}

	public virtual void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		insideRestore = true;
		if (!awakecalled)
		{
			Awake();
		}
		bool flag = restorePhysical && !physicalLocked;
		bool flag2 = restoreAppearance && !appearanceLocked;
		if (flag || flag2)
		{
			foreach (string boolParamName in boolParamNames)
			{
				JSONStorableBool value;
				if (boolParams.TryGetValue(boolParamName, out value))
				{
					value.LateRestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
			foreach (string floatParamName in floatParamNames)
			{
				JSONStorableFloat value2;
				if (floatParams.TryGetValue(floatParamName, out value2))
				{
					value2.LateRestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
			foreach (string vector3ParamName in vector3ParamNames)
			{
				JSONStorableVector3 value3;
				if (vector3Params.TryGetValue(vector3ParamName, out value3))
				{
					value3.LateRestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
			foreach (string stringParamName in stringParamNames)
			{
				JSONStorableString value4;
				if (stringParams.TryGetValue(stringParamName, out value4))
				{
					value4.LateRestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
			foreach (string stringChooserParamName in stringChooserParamNames)
			{
				JSONStorableStringChooser value5;
				if (stringChooserParams.TryGetValue(stringChooserParamName, out value5))
				{
					value5.LateRestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
			foreach (string urlParamName in urlParamNames)
			{
				JSONStorableUrl value6;
				if (urlParams.TryGetValue(urlParamName, out value6))
				{
					value6.LateRestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
			foreach (string colorParamName in colorParamNames)
			{
				JSONStorableColor value7;
				if (colorParams.TryGetValue(colorParamName, out value7))
				{
					value7.LateRestoreFromJSON(jc, flag, flag2, setMissingToDefault);
				}
			}
		}
		insideRestore = false;
	}

	public virtual void Validate()
	{
	}

	public virtual void PreRestore()
	{
	}

	public virtual void PreRestore(bool restorePhysical, bool restoreAppearance)
	{
	}

	public virtual void PostRestore()
	{
	}

	public virtual void PostRestore(bool restorePhysical, bool restoreAppearance)
	{
	}

	public virtual void SetDefaultsFromCurrent()
	{
		if (!awakecalled)
		{
			Awake();
		}
		foreach (string boolParamName in boolParamNames)
		{
			JSONStorableBool value;
			if (boolParams.TryGetValue(boolParamName, out value))
			{
				value.SetDefaultFromCurrent();
			}
		}
		foreach (string floatParamName in floatParamNames)
		{
			JSONStorableFloat value2;
			if (floatParams.TryGetValue(floatParamName, out value2))
			{
				value2.SetDefaultFromCurrent();
			}
		}
		foreach (string vector3ParamName in vector3ParamNames)
		{
			JSONStorableVector3 value3;
			if (vector3Params.TryGetValue(vector3ParamName, out value3))
			{
				value3.SetDefaultFromCurrent();
			}
		}
		foreach (string stringParamName in stringParamNames)
		{
			JSONStorableString value4;
			if (stringParams.TryGetValue(stringParamName, out value4))
			{
				value4.SetDefaultFromCurrent();
			}
		}
		foreach (string stringChooserParamName in stringChooserParamNames)
		{
			JSONStorableStringChooser value5;
			if (stringChooserParams.TryGetValue(stringChooserParamName, out value5))
			{
				value5.SetDefaultFromCurrent();
			}
		}
		foreach (string urlParamName in urlParamNames)
		{
			JSONStorableUrl value6;
			if (urlParams.TryGetValue(urlParamName, out value6))
			{
				value6.SetDefaultFromCurrent();
			}
		}
		foreach (string colorParamName in colorParamNames)
		{
			JSONStorableColor value7;
			if (colorParams.TryGetValue(colorParamName, out value7))
			{
				value7.SetDefaultFromCurrent();
			}
		}
	}

	public virtual void PreRemove()
	{
	}

	public virtual void Remove()
	{
	}

	protected virtual void InitJSONStorable()
	{
		if (!_JSONWasInit)
		{
			_JSONWasInit = true;
			allParams = new Dictionary<string, JSONStorableParam>();
			allParamsByAltName = new Dictionary<string, JSONStorableParam>();
			allParamAndActionNames = new List<string>();
			allFloatAndColorParamNames = new List<string>();
			InitBoolParams();
			InitFloatParams();
			InitVector3Params();
			InitStringParams();
			InitStringChooserParams();
			InitUrlParams();
			InitColorParams();
			InitActions();
			InitAudioClipActions();
			InitStringChooserActions();
			InitSceneFilePathActions();
			InitPresetFilePathActions();
			saveToStore1Action = new JSONStorableAction("SaveToStore1", SaveToStore1);
			RegisterAction(saveToStore1Action);
			saveToStore2Action = new JSONStorableAction("SaveToStore2", SaveToStore2);
			RegisterAction(saveToStore2Action);
			saveToStore3Action = new JSONStorableAction("SaveToStore3", SaveToStore3);
			RegisterAction(saveToStore3Action);
			restoreAllFromStore1Action = new JSONStorableAction("RestoreAllFromStore1", RestoreAllFromStore1);
			RegisterAction(restoreAllFromStore1Action);
			restorePhysicalFromStore1Action = new JSONStorableAction("RestorePhysicsFromStore1", RestorePhysicalFromStore1);
			RegisterAction(restorePhysicalFromStore1Action);
			restoreAppearanceFromStore1Action = new JSONStorableAction("RestoreAppearanceFromStore1", RestoreAppearanceFromStore1);
			RegisterAction(restoreAppearanceFromStore1Action);
			restoreAllFromStore2Action = new JSONStorableAction("RestoreAllFromStore2", RestoreAllFromStore2);
			RegisterAction(restoreAllFromStore2Action);
			restorePhysicalFromStore2Action = new JSONStorableAction("RestorePhysicsFromStore2", RestorePhysicalFromStore2);
			RegisterAction(restorePhysicalFromStore2Action);
			restoreAppearanceFromStore2Action = new JSONStorableAction("RestoreAppearanceFromStore2", RestoreAppearanceFromStore2);
			RegisterAction(restoreAppearanceFromStore2Action);
			restoreAllFromStore3Action = new JSONStorableAction("RestoreAllFromStore3", RestoreAllFromStore3);
			RegisterAction(restoreAllFromStore3Action);
			restorePhysicalFromStore3Action = new JSONStorableAction("RestorePhysicsFromStore3", RestorePhysicalFromStore3);
			RegisterAction(restorePhysicalFromStore3Action);
			restoreAppearanceFromStore3Action = new JSONStorableAction("RestoreAppearanceFromStore3", RestoreAppearanceFromStore3);
			RegisterAction(restoreAppearanceFromStore3Action);
			restoreAllFromDefaultsAction = new JSONStorableAction("RestoreAllFromDefaults", RestoreAllFromDefaults);
			RegisterAction(restoreAllFromDefaultsAction);
			restorePhysicalFromDefaultsAction = new JSONStorableAction("RestorePhysicalFromDefaults", RestorePhysicalFromDefaults);
			RegisterAction(restorePhysicalFromDefaultsAction);
			restoreAppearanceFromDefaultsAction = new JSONStorableAction("RestoreAppearanceFromDefaults", RestoreAppearanceFromDefaults);
			RegisterAction(restoreAppearanceFromDefaultsAction);
		}
	}

	protected virtual void Awake()
	{
		if (!awakecalled)
		{
			awakecalled = true;
			InitJSONStorable();
		}
	}
}
