using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

namespace MeshVR
{
	public class PresetManager : MonoBehaviour
	{
		[Serializable]
		public class SpecificStorable
		{
			public Transform specificStorableBucket;

			public string storeId;

			public string specificKey;

			public bool isSpecificKeyAnObject;

			public bool includeChildren;
		}

		public class Storable
		{
			public JSONStorable storable;

			public string specificKey;

			public bool isSpecificKeyAnObject;
		}

		public enum ItemType
		{
			None,
			Custom,
			ClothingFemale,
			ClothingMale,
			ClothingNeutral,
			Atom,
			HairFemale,
			HairMale,
			HairNeutral
		}

		[Serializable]
		public class ConditionalLoadEvent
		{
			public string flag;

			public UnityEvent ifEvent;

			public UnityEvent elseEvent;
		}

		public UnityEvent postLoadEvent;

		public UnityEvent postLoadOptimizeEvent;

		public string[] conditionalFlagsToStore;

		public bool setConditionalFlagsOnLoadDefaults = true;

		public bool storeConditionalFlagsAlways = true;

		public bool storeConditionalFlagsWhenStoreOptional;

		public bool storeConditionalFlagsWhenStoreOptional2;

		public bool storeConditionalFlagsWhenStoreOptional3;

		public ConditionalLoadEvent[] conditionalLoadEvents;

		public bool onlyCallPostLoadEventIfIncludeOptional;

		public ItemType itemType;

		public string storedCreatorName;

		public string creatorName;

		public string storeFolderName;

		public string storeName;

		protected string _presetName;

		protected string presetPackageName = string.Empty;

		protected string presetPackagePath = string.Empty;

		protected string presetSubPath;

		protected string presetSubName;

		public string package = string.Empty;

		public string customPath = string.Empty;

		public bool storeOptionalStorables;

		public bool storeOptionalStorables2;

		public bool storeOptionalStorables3;

		public bool storePresetBinary;

		public bool setOptionalToDefaultOnRestore;

		public bool setOptional2ToDefaultOnRestore;

		public bool setOptional3ToDefaultOnRestore;

		public bool useTransformAndChildren = true;

		public bool includeOptional = true;

		public bool includeOptional2 = true;

		public bool includeOptional3 = true;

		public bool includePhysical = true;

		public bool includeAppearance = true;

		public bool optionalFirst;

		public SpecificStorable[] optionalSpecificStorables;

		public SpecificStorable[] optionalSpecificStorables2;

		public SpecificStorable[] optionalSpecificStorables3;

		public SpecificStorable[] specificStorables;

		public Transform[] dynamicStorablesBuckets;

		public bool ignoreExclude;

		protected string storeRoot = "Custom/";

		protected List<Storable> storables;

		protected List<Storable> optionalStorables;

		protected List<Storable> optionalStorables2;

		protected List<Storable> optionalStorables3;

		protected List<Storable> dynamicStorables;

		protected Dictionary<JSONStorable, bool> regularStorables;

		protected Dictionary<string, bool> specificKeyStorables;

		protected bool _tempUnlockParams;

		protected bool _paramsLocked;

		public JSONClass lastLoadedJSON;

		public JSONClass filteredJSON;

		protected bool isMergeRestore;

		public bool neverSetUnlistedParamsToDefault;

		protected bool setUnlistedParamsToDefault = true;

		protected bool setUnlistedDynamicStorableParamsToDefault = true;

		public bool includeChildrenMaterialOptions;

		public string presetName
		{
			get
			{
				return _presetName;
			}
			set
			{
				if (!(_presetName != value))
				{
					return;
				}
				_presetName = value;
				if (_presetName == null)
				{
					presetPackageName = string.Empty;
					presetPackagePath = string.Empty;
					presetSubName = string.Empty;
					presetSubPath = string.Empty;
					return;
				}
				string text = _presetName;
				if (_presetName.Contains(":"))
				{
					string[] array = _presetName.Split(':');
					presetPackageName = array[0];
					presetPackagePath = array[0] + ":/";
					text = array[1];
				}
				else
				{
					presetPackageName = string.Empty;
					presetPackagePath = string.Empty;
				}
				if (text.Contains("/"))
				{
					presetSubPath = Path.GetDirectoryName(text) + "/";
					presetSubName = Path.GetFileName(text);
				}
				else
				{
					presetSubPath = string.Empty;
					presetSubName = text;
				}
			}
		}

		protected bool tempUnlockParams
		{
			get
			{
				return _tempUnlockParams;
			}
			set
			{
				if (_tempUnlockParams != value)
				{
					_tempUnlockParams = value;
					SyncParamsLocked();
				}
			}
		}

		public bool paramsLocked
		{
			get
			{
				return _paramsLocked;
			}
			set
			{
				if (_paramsLocked != value)
				{
					_paramsLocked = value;
					SyncParamsLocked();
				}
			}
		}

		public bool IsInPackage()
		{
			return package != string.Empty;
		}

		public string GetStoreRootPath(bool includePackage = true)
		{
			string result = null;
			string text = string.Empty;
			if (package != string.Empty && includePackage)
			{
				text = package + ":/";
			}
			switch (itemType)
			{
			case ItemType.None:
				result = text + storeRoot;
				break;
			case ItemType.Custom:
				result = text + storeRoot + customPath;
				break;
			case ItemType.ClothingFemale:
				result = text + storeRoot + "Clothing/Female/";
				break;
			case ItemType.ClothingMale:
				result = text + storeRoot + "Clothing/Male/";
				break;
			case ItemType.ClothingNeutral:
				result = text + storeRoot + "Clothing/Neutral/";
				break;
			case ItemType.Atom:
				result = text + storeRoot + "Atom/" + customPath;
				break;
			case ItemType.HairFemale:
				result = text + storeRoot + "Hair/Female/";
				break;
			case ItemType.HairMale:
				result = text + storeRoot + "Hair/Male/";
				break;
			case ItemType.HairNeutral:
				result = text + storeRoot + "Hair/Neutral/";
				break;
			}
			return result;
		}

		public List<string> FindFavoriteNames()
		{
			List<string> list = new List<string>();
			if (itemType != 0)
			{
				string storeFolderPath = GetStoreFolderPath(false);
				if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty && Directory.Exists(storeFolderPath))
				{
					string[] files = Directory.GetFiles(storeFolderPath, storeName + "_*.vap.fav", SearchOption.AllDirectories);
					string[] array = files;
					foreach (string input in array)
					{
						string text = Regex.Replace(input, "\\.fav$", string.Empty);
						text = text.Replace("\\", "/");
						string presetNameFromFilePath = GetPresetNameFromFilePath(text);
						if (presetNameFromFilePath != null)
						{
							list.Add(presetNameFromFilePath);
						}
					}
				}
			}
			return list;
		}

		public string[] PathToNames(string inpath)
		{
			string text = string.Empty;
			string text2 = string.Empty;
			string text3 = string.Empty;
			string text4 = string.Empty;
			string text5 = ":/";
			string text6;
			if (inpath.Contains(text5))
			{
				string[] array = inpath.Split(new string[1] { text5 }, StringSplitOptions.None);
				text4 = array[0];
				text6 = array[1];
			}
			else
			{
				text6 = inpath;
			}
			if (text6 != null && text6 != string.Empty)
			{
				text = Path.GetFileName(text6);
				if (text != null && text != string.Empty)
				{
					text = Regex.Replace(text, "\\.(vap|vam|vaj|vab)$", string.Empty);
					string directoryName = Path.GetDirectoryName(text6);
					if (directoryName != null && directoryName != string.Empty)
					{
						// Path.GetDirectoryName returns Windows separators while the store root is written
						// with forward slashes; unnormalised the strip never matched, the fully rooted
						// directory was kept as the folder name, and GetStoreFolderPath doubled the root
						// into a path that does not exist.
						string storeRootPath = GetStoreRootPath(false);
						string text7 = Regex.Replace(directoryName.Replace('\\', '/'), "^" + (storeRootPath == null ? string.Empty : Regex.Escape(storeRootPath)), string.Empty);
						if (text7.Contains("/"))
						{
							text3 = Regex.Replace(text7, "/.*", string.Empty);
							text2 = Regex.Replace(text7, text3 + "/", string.Empty);
						}
						else
						{
							text2 = text7;
						}
					}
				}
			}
			return new string[4] { text2, text3, text, text4 };
		}

		public void SetNamesFromPath(string path)
		{
			string[] array = PathToNames(path);
			storeFolderName = array[0];
			creatorName = array[1];
			storeName = array[2];
			package = array[3];
		}

		public string GetStoreFolderPath(bool includePackage = true)
		{
			string text = null;
			string storeRootPath = GetStoreRootPath(includePackage);
			if (storeRootPath != null)
			{
				text = storeRootPath;
				if (creatorName != null && creatorName != string.Empty)
				{
					text = text + creatorName + "/";
				}
				if (storeFolderName != null && storeFolderName != string.Empty)
				{
					string text2 = Regex.Replace(storeFolderName, "/$", string.Empty);
					text = text + text2 + "/";
				}
			}
			return text;
		}

		public string GetStorePathBase()
		{
			string storeFolderPath = GetStoreFolderPath();
			return storeFolderPath + storeName;
		}

		public string GetPresetNameFromFilePath(string fpath)
		{
			VarFileEntry varFileEntry = FileManager.GetVarFileEntry(fpath);
			string text = string.Empty;
			string text2 = fpath;
			if (varFileEntry != null)
			{
				text = varFileEntry.Package.Uid + ":";
				text2 = varFileEntry.InternalSlashPath;
			}
			string storeFolderPath = GetStoreFolderPath(false);
			string text3 = text2.Replace(storeFolderPath, string.Empty);
			string result = null;
			if (text3 == text2)
			{
				SuperController.LogError("Preset path " + fpath + " is not compatible with store folder path " + storeFolderPath);
			}
			else
			{
				string empty = string.Empty;
				string empty2 = string.Empty;
				if (text3.Contains("/"))
				{
					empty = Path.GetDirectoryName(text3) + "/";
					empty2 = Path.GetFileName(text3);
				}
				else
				{
					empty = string.Empty;
					empty2 = text3;
				}
				string text4 = empty2.Replace(storeName + "_", string.Empty);
				if (text4 == empty2)
				{
					SuperController.LogError("Preset " + empty2 + " is not a preset for current store " + storeName);
				}
				else
				{
					if (text4.Contains("__"))
					{
						text = Regex.Replace(text4, "__.*", string.Empty);
						if (FileManager.IsPackage(text))
						{
							text += ":";
							text4 = Regex.Replace(text4, ".*__", string.Empty);
						}
					}
					result = text4.Replace(".vap", string.Empty);
					result = text + empty + result;
				}
			}
			return result;
		}

		protected void ClearLockStorablesInList(List<Storable> sts)
		{
			if (sts == null)
			{
				return;
			}
			string storeFolderPath = GetStoreFolderPath(false);
			foreach (Storable st in sts)
			{
				JSONStorable storable = st.storable;
				if (!(storable != null) || (!ignoreExclude && storable.exclude))
				{
					continue;
				}
				if (st.specificKey != null && st.specificKey != string.Empty)
				{
					JSONStorableParam param = storable.GetParam(st.specificKey);
					if (param != null)
					{
						param.ClearLock(storeFolderPath);
						continue;
					}
					storable.ClearCustomAppearanceParamLock(st.specificKey, storeFolderPath);
					storable.ClearCustomPhysicalParamLock(st.specificKey, storeFolderPath);
				}
				else
				{
					storable.ClearAppearanceLock(storeFolderPath);
					storable.ClearPhysicalLock(storeFolderPath);
				}
			}
		}

		protected void LockStorablesInList(List<Storable> sts)
		{
			if (sts == null || !_paramsLocked || _tempUnlockParams)
			{
				return;
			}
			string storeFolderPath = GetStoreFolderPath(false);
			foreach (Storable st in sts)
			{
				JSONStorable storable = st.storable;
				if (!(storable != null) || (!ignoreExclude && storable.exclude) || !storable.gameObject.activeInHierarchy)
				{
					continue;
				}
				if (st.specificKey != null && st.specificKey != string.Empty)
				{
					JSONStorableParam param = storable.GetParam(st.specificKey);
					if (param != null)
					{
						param.SetLock(storeFolderPath);
						continue;
					}
					if (includeAppearance)
					{
						storable.SetCustomAppearanceParamLock(st.specificKey, storeFolderPath);
					}
					if (includePhysical)
					{
						storable.SetCustomPhysicalParamLock(st.specificKey, storeFolderPath);
					}
				}
				else
				{
					if (includeAppearance)
					{
						storable.SetAppearanceLock(storeFolderPath);
					}
					if (includePhysical)
					{
						storable.SetPhysicalLock(storeFolderPath);
					}
				}
			}
		}

		public void SyncParamsLocked()
		{
			ClearLockStorablesInList(storables);
			ClearLockStorablesInList(optionalStorables);
			ClearLockStorablesInList(optionalStorables2);
			ClearLockStorablesInList(optionalStorables3);
			LockStorablesInList(storables);
			if (includeOptional && storeOptionalStorables)
			{
				LockStorablesInList(optionalStorables);
			}
			if (includeOptional2 && storeOptionalStorables2)
			{
				LockStorablesInList(optionalStorables2);
			}
			if (includeOptional3 && storeOptionalStorables3)
			{
				LockStorablesInList(optionalStorables3);
			}
			RefreshDynamicStorables();
		}

		protected void StoreStorablesInList(JSONClass jc, List<Storable> sts, bool storeAll)
		{
			if (sts == null)
			{
				return;
			}
			foreach (Storable st in sts)
			{
				JSONStorable storable = st.storable;
				if (!(storable != null) || (!ignoreExclude && storable.exclude) || !storable.gameObject.activeInHierarchy)
				{
					continue;
				}
				try
				{
					storable.isPresetStore = true;
					JSONClass jSON = storable.GetJSON(includePhysical, includeAppearance, storeAll);
					storable.isPresetStore = false;
					if (!storable.needsStore)
					{
						continue;
					}
					if (st.specificKey != null && st.specificKey != string.Empty)
					{
						JSONClass jSONClass = new JSONClass();
						foreach (string key in jSON.Keys)
						{
							if (key == st.specificKey || key == "id")
							{
								jSONClass[key] = jSON[key];
							}
						}
						jc["storables"].Add(jSONClass);
					}
					else
					{
						jc["storables"].Add(jSON);
					}
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception during Preset Store of " + storable.storeId + ": " + ex);
				}
			}
		}

		protected virtual void StorePresetBinary()
		{
		}

		protected void StoreStorables(JSONClass jc, bool storeAll)
		{
			if (!(jc != null))
			{
				return;
			}
			RefreshDynamicStorables();
			jc["storables"] = new JSONArray();
			if (optionalFirst)
			{
				if (includeOptional && storeOptionalStorables)
				{
					StoreStorablesInList(jc, optionalStorables, storeAll);
				}
				if (includeOptional2 && storeOptionalStorables2)
				{
					StoreStorablesInList(jc, optionalStorables2, storeAll);
				}
				if (includeOptional3 && storeOptionalStorables3)
				{
					StoreStorablesInList(jc, optionalStorables3, storeAll);
				}
			}
			StoreStorablesInList(jc, storables, storeAll);
			StoreStorablesInList(jc, dynamicStorables, storeAll);
			if (!optionalFirst)
			{
				if (includeOptional && storeOptionalStorables)
				{
					StoreStorablesInList(jc, optionalStorables, storeAll);
				}
				if (includeOptional2 && storeOptionalStorables2)
				{
					StoreStorablesInList(jc, optionalStorables2, storeAll);
				}
				if (includeOptional3 && storeOptionalStorables3)
				{
					StoreStorablesInList(jc, optionalStorables3, storeAll);
				}
			}
		}

		protected void PreRestoreStorable(JSONStorable js)
		{
			if (js != null && (ignoreExclude || !js.exclude))
			{
				js.isPresetRestore = true;
				js.mergeRestore = isMergeRestore;
				try
				{
					js.PreRestore();
					js.PreRestore(includePhysical, includeAppearance);
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception during PreRestore of " + js.storeId + ": " + ex);
				}
				js.mergeRestore = false;
				js.isPresetRestore = false;
			}
		}

		protected void PreRestore()
		{
			if (optionalFirst)
			{
				if (includeOptional && optionalStorables != null)
				{
					foreach (Storable optionalStorable in optionalStorables)
					{
						JSONStorable storable = optionalStorable.storable;
						PreRestoreStorable(storable);
					}
				}
				if (includeOptional2 && optionalStorables2 != null)
				{
					foreach (Storable item in optionalStorables2)
					{
						JSONStorable storable2 = item.storable;
						PreRestoreStorable(storable2);
					}
				}
				if (includeOptional3 && optionalStorables3 != null)
				{
					foreach (Storable item2 in optionalStorables3)
					{
						JSONStorable storable3 = item2.storable;
						PreRestoreStorable(storable3);
					}
				}
			}
			if (storables != null)
			{
				foreach (Storable storable9 in storables)
				{
					JSONStorable storable4 = storable9.storable;
					PreRestoreStorable(storable4);
				}
			}
			if (dynamicStorables != null)
			{
				foreach (Storable dynamicStorable in dynamicStorables)
				{
					JSONStorable storable5 = dynamicStorable.storable;
					PreRestoreStorable(storable5);
				}
			}
			if (optionalFirst)
			{
				return;
			}
			if (includeOptional && optionalStorables != null)
			{
				foreach (Storable optionalStorable2 in optionalStorables)
				{
					JSONStorable storable6 = optionalStorable2.storable;
					PreRestoreStorable(storable6);
				}
			}
			if (includeOptional2 && optionalStorables2 != null)
			{
				foreach (Storable item3 in optionalStorables2)
				{
					JSONStorable storable7 = item3.storable;
					PreRestoreStorable(storable7);
				}
			}
			if (!includeOptional3 || optionalStorables3 == null)
			{
				return;
			}
			foreach (Storable item4 in optionalStorables3)
			{
				JSONStorable storable8 = item4.storable;
				PreRestoreStorable(storable8);
			}
		}

		protected void Restore(JSONClass jc)
		{
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			Dictionary<string, JSONStorable> dictionary2 = new Dictionary<string, JSONStorable>();
			if (includeOptional && optionalStorables != null)
			{
				foreach (Storable optionalStorable in optionalStorables)
				{
					JSONStorable storable = optionalStorable.storable;
					if (storable != null && (ignoreExclude || !storable.exclude) && !dictionary2.ContainsKey(storable.storeId))
					{
						dictionary2.Add(storable.storeId, storable);
					}
				}
			}
			if (includeOptional2 && optionalStorables2 != null)
			{
				foreach (Storable item in optionalStorables2)
				{
					JSONStorable storable2 = item.storable;
					if (storable2 != null && (ignoreExclude || !storable2.exclude) && !dictionary2.ContainsKey(storable2.storeId))
					{
						dictionary2.Add(storable2.storeId, storable2);
					}
				}
			}
			if (includeOptional3 && optionalStorables3 != null)
			{
				foreach (Storable item2 in optionalStorables3)
				{
					JSONStorable storable3 = item2.storable;
					if (storable3 != null && (ignoreExclude || !storable3.exclude) && !dictionary2.ContainsKey(storable3.storeId))
					{
						dictionary2.Add(storable3.storeId, storable3);
					}
				}
			}
			if (storables != null)
			{
				foreach (Storable storable10 in storables)
				{
					JSONStorable storable4 = storable10.storable;
					if (storable4 != null && (ignoreExclude || !storable4.exclude) && !dictionary2.ContainsKey(storable4.storeId))
					{
						dictionary2.Add(storable4.storeId, storable4);
					}
				}
			}
			foreach (JSONClass item3 in jc["storables"].AsArray)
			{
				string text = item3["id"];
				JSONStorable value;
				if (dictionary2.TryGetValue(text, out value))
				{
					bool value2;
					if (!specificKeyStorables.TryGetValue(text, out value2))
					{
						value2 = false;
					}
					value.isPresetRestore = true;
					value.mergeRestore = isMergeRestore;
					try
					{
						value.RestoreFromJSON(item3, includePhysical, includeAppearance, null, !value2 && setUnlistedParamsToDefault);
					}
					catch (Exception ex)
					{
						SuperController.LogError("Exception during Restore of " + value.storeId + ": " + ex);
					}
					value.mergeRestore = false;
					value.isPresetRestore = false;
					if (!dictionary.ContainsKey(item3["id"]))
					{
						dictionary.Add(item3["id"], true);
					}
					continue;
				}
				foreach (Storable dynamicStorable in dynamicStorables)
				{
					value = dynamicStorable.storable;
					if (value != null && value.storeId == text && (ignoreExclude || !value.exclude) && (!value.onlyStoreIfActive || value.gameObject.activeInHierarchy))
					{
						bool value3;
						if (!specificKeyStorables.TryGetValue(text, out value3))
						{
							value3 = false;
						}
						value.isPresetRestore = true;
						value.mergeRestore = isMergeRestore;
						try
						{
							value.RestoreFromJSON(item3, includePhysical, includeAppearance, null, !value3 && setUnlistedParamsToDefault);
						}
						catch (Exception ex2)
						{
							SuperController.LogError("Exception during Restore of " + value.storeId + ": " + ex2);
						}
						value.mergeRestore = false;
						value.isPresetRestore = false;
						if (!dictionary.ContainsKey(item3["id"]))
						{
							dictionary.Add(item3["id"], true);
						}
						break;
					}
				}
			}
			JSONClass jc2 = new JSONClass();
			if (!setUnlistedParamsToDefault)
			{
				return;
			}
			foreach (Storable storable11 in storables)
			{
				JSONStorable storable5 = storable11.storable;
				if (storable5 != null && (ignoreExclude || !storable5.exclude) && !dictionary.ContainsKey(storable5.storeId) && !specificKeyStorables.ContainsKey(storable5.storeId))
				{
					storable5.isPresetRestore = true;
					try
					{
						storable5.RestoreFromJSON(jc2, includePhysical, includeAppearance);
					}
					catch (Exception ex3)
					{
						SuperController.LogError("Exception during Restore of " + storable5.storeId + ": " + ex3);
					}
					storable5.isPresetRestore = false;
				}
			}
			foreach (Storable dynamicStorable2 in dynamicStorables)
			{
				JSONStorable storable6 = dynamicStorable2.storable;
				if (storable6 != null && (ignoreExclude || !storable6.exclude) && (!storable6.onlyStoreIfActive || storable6.gameObject.activeInHierarchy) && !dictionary.ContainsKey(storable6.storeId))
				{
					storable6.isPresetRestore = true;
					try
					{
						storable6.RestoreFromJSON(jc2, includePhysical, includeAppearance);
					}
					catch (Exception ex4)
					{
						SuperController.LogError("Exception during Restore of " + storable6.storeId + ": " + ex4);
					}
					storable6.isPresetRestore = false;
				}
			}
			if (includeOptional && setOptionalToDefaultOnRestore)
			{
				foreach (Storable optionalStorable2 in optionalStorables)
				{
					JSONStorable storable7 = optionalStorable2.storable;
					if (storable7 != null && (ignoreExclude || !storable7.exclude) && !dictionary.ContainsKey(storable7.storeId) && !specificKeyStorables.ContainsKey(storable7.storeId))
					{
						storable7.isPresetRestore = true;
						try
						{
							storable7.RestoreFromJSON(jc2, includePhysical, includeAppearance);
						}
						catch (Exception ex5)
						{
							SuperController.LogError("Exception during Restore of " + storable7.storeId + ": " + ex5);
						}
						storable7.isPresetRestore = false;
					}
				}
			}
			if (includeOptional2 && setOptional2ToDefaultOnRestore)
			{
				foreach (Storable item4 in optionalStorables2)
				{
					JSONStorable storable8 = item4.storable;
					if (storable8 != null && (ignoreExclude || !storable8.exclude) && !dictionary.ContainsKey(storable8.storeId) && !specificKeyStorables.ContainsKey(storable8.storeId))
					{
						storable8.isPresetRestore = true;
						try
						{
							storable8.RestoreFromJSON(jc2, includePhysical, includeAppearance);
						}
						catch (Exception ex6)
						{
							SuperController.LogError("Exception during Restore of " + storable8.storeId + ": " + ex6);
						}
						storable8.isPresetRestore = false;
					}
				}
			}
			if (!includeOptional3 || !setOptional3ToDefaultOnRestore)
			{
				return;
			}
			foreach (Storable item5 in optionalStorables3)
			{
				JSONStorable storable9 = item5.storable;
				if (storable9 != null && (ignoreExclude || !storable9.exclude) && !dictionary.ContainsKey(storable9.storeId) && !specificKeyStorables.ContainsKey(storable9.storeId))
				{
					storable9.isPresetRestore = true;
					try
					{
						storable9.RestoreFromJSON(jc2, includePhysical, includeAppearance);
					}
					catch (Exception ex7)
					{
						SuperController.LogError("Exception during Restore of " + storable9.storeId + ": " + ex7);
					}
					storable9.isPresetRestore = false;
				}
			}
		}

		protected void LateRestore(JSONClass jc)
		{
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			Dictionary<string, JSONStorable> dictionary2 = new Dictionary<string, JSONStorable>();
			if (includeOptional && optionalStorables != null)
			{
				foreach (Storable optionalStorable in optionalStorables)
				{
					JSONStorable storable = optionalStorable.storable;
					if (storable != null && (ignoreExclude || !storable.exclude) && !dictionary2.ContainsKey(storable.storeId))
					{
						dictionary2.Add(storable.storeId, storable);
					}
				}
			}
			if (includeOptional2 && optionalStorables2 != null)
			{
				foreach (Storable item in optionalStorables2)
				{
					JSONStorable storable2 = item.storable;
					if (storable2 != null && (ignoreExclude || !storable2.exclude) && !dictionary2.ContainsKey(storable2.storeId))
					{
						dictionary2.Add(storable2.storeId, storable2);
					}
				}
			}
			if (includeOptional3 && optionalStorables3 != null)
			{
				foreach (Storable item2 in optionalStorables3)
				{
					JSONStorable storable3 = item2.storable;
					if (storable3 != null && (ignoreExclude || !storable3.exclude) && !dictionary2.ContainsKey(storable3.storeId))
					{
						dictionary2.Add(storable3.storeId, storable3);
					}
				}
			}
			if (storables != null)
			{
				foreach (Storable storable10 in storables)
				{
					JSONStorable storable4 = storable10.storable;
					if (storable4 != null && (ignoreExclude || !storable4.exclude) && !dictionary2.ContainsKey(storable4.storeId))
					{
						dictionary2.Add(storable4.storeId, storable4);
					}
				}
			}
			foreach (JSONClass item3 in jc["storables"].AsArray)
			{
				string text = item3["id"];
				JSONStorable value;
				if (dictionary2.TryGetValue(text, out value))
				{
					bool value2;
					if (!specificKeyStorables.TryGetValue(text, out value2))
					{
						value2 = false;
					}
					value.isPresetRestore = true;
					value.mergeRestore = isMergeRestore;
					try
					{
						value.LateRestoreFromJSON(item3, includePhysical, includeAppearance, !value2 && setUnlistedParamsToDefault);
					}
					catch (Exception ex)
					{
						SuperController.LogError("Exception during LateRestore of " + value.storeId + ": " + ex);
					}
					value.mergeRestore = false;
					value.isPresetRestore = false;
					if (!dictionary.ContainsKey(item3["id"]))
					{
						dictionary.Add(item3["id"], true);
					}
					continue;
				}
				foreach (Storable dynamicStorable in dynamicStorables)
				{
					value = dynamicStorable.storable;
					if (value != null && value.storeId == text && (ignoreExclude || !value.exclude) && (!value.onlyStoreIfActive || value.gameObject.activeInHierarchy))
					{
						bool value3;
						if (!specificKeyStorables.TryGetValue(text, out value3))
						{
							value3 = false;
						}
						value.isPresetRestore = true;
						value.mergeRestore = isMergeRestore;
						try
						{
							value.LateRestoreFromJSON(item3, includePhysical, includeAppearance, !value3 && setUnlistedParamsToDefault);
						}
						catch (Exception ex2)
						{
							SuperController.LogError("Exception during LateRestore of " + value.storeId + ": " + ex2);
						}
						value.mergeRestore = false;
						value.isPresetRestore = false;
						if (!dictionary.ContainsKey(item3["id"]))
						{
							dictionary.Add(item3["id"], true);
						}
						break;
					}
				}
			}
			JSONClass jc2 = new JSONClass();
			if (!setUnlistedParamsToDefault)
			{
				return;
			}
			foreach (Storable storable11 in storables)
			{
				JSONStorable storable5 = storable11.storable;
				if (storable5 != null && (ignoreExclude || !storable5.exclude) && !dictionary.ContainsKey(storable5.storeId) && !specificKeyStorables.ContainsKey(storable5.storeId))
				{
					storable5.isPresetRestore = true;
					try
					{
						storable5.LateRestoreFromJSON(jc2, includePhysical, includeAppearance);
					}
					catch (Exception ex3)
					{
						SuperController.LogError("Exception during LateRestore of " + storable5.storeId + ": " + ex3);
					}
					storable5.isPresetRestore = false;
				}
			}
			foreach (Storable dynamicStorable2 in dynamicStorables)
			{
				JSONStorable storable6 = dynamicStorable2.storable;
				if (storable6 != null && (ignoreExclude || !storable6.exclude) && (!storable6.onlyStoreIfActive || storable6.gameObject.activeInHierarchy) && !dictionary.ContainsKey(storable6.storeId))
				{
					storable6.isPresetRestore = true;
					try
					{
						storable6.LateRestoreFromJSON(jc2, includePhysical, includeAppearance);
					}
					catch (Exception ex4)
					{
						SuperController.LogError("Exception during LateRestore of " + storable6.storeId + ": " + ex4);
					}
					storable6.isPresetRestore = false;
				}
			}
			if (includeOptional && setOptionalToDefaultOnRestore)
			{
				foreach (Storable optionalStorable2 in optionalStorables)
				{
					JSONStorable storable7 = optionalStorable2.storable;
					if (storable7 != null && (ignoreExclude || !storable7.exclude) && !dictionary.ContainsKey(storable7.storeId) && !specificKeyStorables.ContainsKey(storable7.storeId))
					{
						storable7.isPresetRestore = true;
						try
						{
							storable7.LateRestoreFromJSON(jc2, includePhysical, includeAppearance);
						}
						catch (Exception ex5)
						{
							SuperController.LogError("Exception during LateRestore of " + storable7.storeId + ": " + ex5);
						}
						storable7.isPresetRestore = false;
					}
				}
			}
			if (includeOptional2 && setOptional2ToDefaultOnRestore)
			{
				foreach (Storable item4 in optionalStorables2)
				{
					JSONStorable storable8 = item4.storable;
					if (storable8 != null && (ignoreExclude || !storable8.exclude) && !dictionary.ContainsKey(storable8.storeId) && !specificKeyStorables.ContainsKey(storable8.storeId))
					{
						storable8.isPresetRestore = true;
						try
						{
							storable8.LateRestoreFromJSON(jc2, includePhysical, includeAppearance);
						}
						catch (Exception ex6)
						{
							SuperController.LogError("Exception during LateRestore of " + storable8.storeId + ": " + ex6);
						}
						storable8.isPresetRestore = false;
					}
				}
			}
			if (!includeOptional3 || !setOptional3ToDefaultOnRestore)
			{
				return;
			}
			foreach (Storable item5 in optionalStorables3)
			{
				JSONStorable storable9 = item5.storable;
				if (storable9 != null && (ignoreExclude || !storable9.exclude) && !dictionary.ContainsKey(storable9.storeId) && !specificKeyStorables.ContainsKey(storable9.storeId))
				{
					storable9.isPresetRestore = true;
					try
					{
						storable9.LateRestoreFromJSON(jc2, includePhysical, includeAppearance);
					}
					catch (Exception ex7)
					{
						SuperController.LogError("Exception during LateRestore of " + storable9.storeId + ": " + ex7);
					}
					storable9.isPresetRestore = false;
				}
			}
		}

		protected void PostRestoreStorable(JSONStorable js)
		{
			if (js != null && (ignoreExclude || !js.exclude))
			{
				js.isPresetRestore = true;
				js.mergeRestore = isMergeRestore;
				try
				{
					js.PostRestore();
					js.PostRestore(includePhysical, includeAppearance);
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception during PostRestore of " + js.storeId + ": " + ex);
				}
				js.mergeRestore = false;
				js.isPresetRestore = false;
			}
		}

		protected void PostRestore()
		{
			if (optionalFirst)
			{
				if (includeOptional && optionalStorables != null)
				{
					foreach (Storable optionalStorable in optionalStorables)
					{
						JSONStorable storable = optionalStorable.storable;
						PostRestoreStorable(storable);
					}
				}
				if (includeOptional2 && optionalStorables2 != null)
				{
					foreach (Storable item in optionalStorables2)
					{
						JSONStorable storable2 = item.storable;
						PostRestoreStorable(storable2);
					}
				}
				if (includeOptional3 && optionalStorables3 != null)
				{
					foreach (Storable item2 in optionalStorables3)
					{
						JSONStorable storable3 = item2.storable;
						PostRestoreStorable(storable3);
					}
				}
			}
			if (storables != null)
			{
				foreach (Storable storable9 in storables)
				{
					JSONStorable storable4 = storable9.storable;
					PostRestoreStorable(storable4);
				}
			}
			if (dynamicStorables != null)
			{
				foreach (Storable dynamicStorable in dynamicStorables)
				{
					JSONStorable storable5 = dynamicStorable.storable;
					PostRestoreStorable(storable5);
				}
			}
			if (optionalFirst)
			{
				return;
			}
			if (includeOptional && optionalStorables != null)
			{
				foreach (Storable optionalStorable2 in optionalStorables)
				{
					JSONStorable storable6 = optionalStorable2.storable;
					PostRestoreStorable(storable6);
				}
			}
			if (includeOptional2 && optionalStorables2 != null)
			{
				foreach (Storable item3 in optionalStorables2)
				{
					JSONStorable storable7 = item3.storable;
					PostRestoreStorable(storable7);
				}
			}
			if (!includeOptional3 || optionalStorables3 == null)
			{
				return;
			}
			foreach (Storable item4 in optionalStorables3)
			{
				JSONStorable storable8 = item4.storable;
				PostRestoreStorable(storable8);
			}
		}

		protected void FilterStorables(JSONClass inputjc, JSONClass outputjc, List<Storable> sts)
		{
			Dictionary<string, Storable> dictionary = new Dictionary<string, Storable>();
			JSONArray asArray = inputjc["storables"].AsArray;
			JSONArray asArray2 = outputjc["storables"].AsArray;
			foreach (Storable st in sts)
			{
				JSONStorable storable = st.storable;
				if (!(storable != null) || (!ignoreExclude && storable.exclude))
				{
					continue;
				}
				if (st.specificKey != null && st.specificKey != string.Empty)
				{
					string key = st.storable.storeId + ":" + st.specificKey;
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, st);
					}
				}
				else if (!dictionary.ContainsKey(st.storable.storeId))
				{
					dictionary.Add(st.storable.storeId, st);
				}
			}
			Dictionary<string, JSONClass> dictionary2 = new Dictionary<string, JSONClass>();
			foreach (JSONClass item in asArray2)
			{
				if (item["id"] != null)
				{
					dictionary2.Add(item["id"], item);
				}
			}
			foreach (JSONClass item2 in asArray)
			{
				if (!(item2["id"] != null))
				{
					continue;
				}
				Storable value;
				if (dictionary.TryGetValue(item2["id"], out value))
				{
					JSONClass value2;
					if (value.storable != null && !dictionary2.TryGetValue(value.storable.storeId, out value2))
					{
						asArray2.Add(item2);
						dictionary2.Add(value.storable.storeId, item2);
					}
					continue;
				}
				foreach (string key2 in item2.Keys)
				{
					if (dictionary.TryGetValue(string.Concat(item2["id"], ":", key2), out value) && value.storable != null)
					{
						JSONClass value3;
						if (!dictionary2.TryGetValue(value.storable.storeId, out value3))
						{
							value3 = new JSONClass();
							value3["id"] = value.storable.storeId;
							asArray2.Add(value3);
							dictionary2.Add(value.storable.storeId, value3);
						}
						value3[value.specificKey] = item2[value.specificKey];
					}
				}
			}
		}

		protected void RestoreStorables(JSONClass jc)
		{
			if (!(jc != null))
			{
				return;
			}
			tempUnlockParams = true;
			RefreshDynamicStorables();
			PreRestore();
			Restore(jc);
			LateRestore(jc);
			PostRestore();
			if (postLoadEvent != null && (!onlyCallPostLoadEventIfIncludeOptional || includeOptional))
			{
				postLoadEvent.Invoke();
			}
			if (postLoadOptimizeEvent != null && UserPreferences.singleton != null && UserPreferences.singleton.optimizeMemoryOnPresetLoad)
			{
				postLoadOptimizeEvent.Invoke();
			}
			if (conditionalLoadEvents != null)
			{
				ConditionalLoadEvent[] array = conditionalLoadEvents;
				foreach (ConditionalLoadEvent conditionalLoadEvent in array)
				{
					string flag = conditionalLoadEvent.flag;
					if (flag == null || !(flag != string.Empty))
					{
						continue;
					}
					if (jc[flag] != null && jc[flag].AsBool)
					{
						if (conditionalLoadEvent.ifEvent != null)
						{
							conditionalLoadEvent.ifEvent.Invoke();
						}
					}
					else if (conditionalLoadEvent.elseEvent != null)
					{
						conditionalLoadEvent.elseEvent.Invoke();
					}
				}
			}
			tempUnlockParams = false;
		}

		public virtual void RestorePresetBinary()
		{
		}

		public void CreateStoreFolderPath()
		{
			string storeFolderPath = GetStoreFolderPath(false);
			if (storeFolderPath != null && Application.isPlaying)
			{
				FileManager.CreateDirectory(storeFolderPath);
			}
		}

		public bool CheckPresetReadyForStore()
		{
			bool result = false;
			if (itemType != 0)
			{
				string storeFolderPath = GetStoreFolderPath(false);
				if (IsPresetInPackage())
				{
					VarPackage varPackage = FileManager.GetPackage(presetPackageName);
					if (varPackage == null || !varPackage.IsSimulated)
					{
						return false;
					}
				}
				if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty && _presetName != null && _presetName != string.Empty)
				{
					result = true;
				}
			}
			return result;
		}

		public void DeletePreset()
		{
			if (itemType == ItemType.None)
			{
				return;
			}
			string storeFolderPath = GetStoreFolderPath(false);
			if (IsPresetInPackage())
			{
				VarPackage varPackage = FileManager.GetPackage(presetPackageName);
				if (varPackage == null || !varPackage.IsSimulated)
				{
					return;
				}
			}
			if (storeFolderPath == null || !(storeFolderPath != string.Empty) || storeName == null || !(storeName != string.Empty) || _presetName == null || !(_presetName != string.Empty))
			{
				return;
			}
			string text = storeFolderPath + presetSubPath + storeName + "_" + presetSubName + ".vap";
			try
			{
				if (FileManager.FileExists(text))
				{
					FileManager.DeleteFile(text);
				}
				if (FileManager.FileExists(text + ".jpg"))
				{
					FileManager.DeleteFile(text + ".jpg");
				}
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception while trying to delete " + text + " " + ex);
			}
		}

		public bool CheckPresetExistance()
		{
			bool result = false;
			if (itemType != 0)
			{
				string storeFolderPath = GetStoreFolderPath(false);
				if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty && _presetName != null && _presetName != string.Empty)
				{
					string path = presetPackagePath + storeFolderPath + presetSubPath + storeName + "_" + presetSubName + ".vap";
					result = FileManager.FileExists(path);
				}
			}
			return result;
		}

		public bool IsPresetInPackage()
		{
			bool result = false;
			if (presetPackageName != null && presetPackageName != string.Empty)
			{
				result = true;
			}
			return result;
		}

		public string GetFavoriteStorePath()
		{
			string result = null;
			string storeFolderPath = GetStoreFolderPath(false);
			if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty && _presetName != null && _presetName != string.Empty)
			{
				result = ((!IsPresetInPackage()) ? (storeFolderPath + presetSubPath + storeName + "_" + presetSubName + ".vap.fav") : (storeFolderPath + presetSubPath + storeName + "_" + presetPackageName + "__" + presetSubName + ".vap.fav"));
			}
			return result;
		}

		public bool IsFavorite()
		{
			string favoriteStorePath = GetFavoriteStorePath();
			if (favoriteStorePath != null && FileManager.FileExists(favoriteStorePath))
			{
				return true;
			}
			return false;
		}

		public void SetFavorite(bool b)
		{
			string favoriteStorePath = GetFavoriteStorePath();
			if (favoriteStorePath == null)
			{
				return;
			}
			if (FileManager.FileExists(favoriteStorePath))
			{
				if (!b)
				{
					FileManager.DeleteFile(favoriteStorePath);
				}
			}
			else if (b)
			{
				string directoryName = FileManager.GetDirectoryName(favoriteStorePath);
				if (!FileManager.DirectoryExists(directoryName))
				{
					FileManager.CreateDirectory(directoryName);
				}
				FileManager.WriteAllText(favoriteStorePath, string.Empty);
			}
		}

		public bool StorePreset(bool doScreenshot = false)
		{
			return StorePreset(true, doScreenshot);
		}

		public bool StorePreset(bool storeAll, bool doScreenshot)
		{
			bool result = false;
			if (itemType != 0)
			{
				string text = GetStoreFolderPath(false);
				if (text != null && text != string.Empty && storeName != null && storeName != string.Empty && _presetName != null && _presetName != string.Empty)
				{
					CreateStoreFolderPath();
					if (presetPackageName != null)
					{
						VarPackage varPackage = FileManager.GetPackage(presetPackageName);
						if (varPackage != null && varPackage.IsSimulated)
						{
							text = varPackage.SlashPath + ":/" + text;
							FileManager.CreateDirectory(text);
						}
					}
					if (presetSubPath != null && presetSubPath != string.Empty)
					{
						FileManager.CreateDirectory(text + presetSubPath);
					}
					string text2 = text + presetSubPath + storeName + "_" + presetSubName + ".vap";
					JSONClass jSONClass = new JSONClass();
					jSONClass["setUnlistedParamsToDefault"].AsBool = storeAll;
					if (conditionalFlagsToStore != null && (storeConditionalFlagsAlways || (storeConditionalFlagsWhenStoreOptional && storeOptionalStorables) || (storeConditionalFlagsWhenStoreOptional2 && storeOptionalStorables2) || (storeConditionalFlagsWhenStoreOptional3 && storeOptionalStorables3)))
					{
						string[] array = conditionalFlagsToStore;
						foreach (string aKey in array)
						{
							jSONClass[aKey].AsBool = true;
						}
					}
					FileManager.SetSaveDirFromFilePath(text2);
					StoreStorables(jSONClass, storeAll);
					StringBuilder stringBuilder = new StringBuilder(100000);
					jSONClass.ToString(string.Empty, stringBuilder);
					string value = stringBuilder.ToString();
					try
					{
						StreamWriter streamWriter = FileManager.OpenStreamWriter(text2);
						streamWriter.Write(value);
						streamWriter.Close();
						if (doScreenshot)
						{
							string text3 = text + presetSubPath + storeName + "_" + presetSubName + ".jpg";
							text3 = text3.Replace('/', '\\');
							SuperController.singleton.DoSaveScreenshot(text3);
						}
						result = true;
					}
					catch (Exception ex)
					{
						SuperController.LogError("Exception while storing to " + text2 + " " + ex);
					}
					if (storePresetBinary)
					{
						StorePresetBinary();
					}
				}
				else
				{
					SuperController.LogError("Not all preset parameters set. Cannot store");
				}
			}
			else
			{
				SuperController.LogError("Item type set to None. Cannot store");
			}
			return result;
		}

		public bool LoadPreset()
		{
			if (LoadPresetPre())
			{
				bool result = LoadPresetPost();
				FileManager.PopLoadDir();
				return result;
			}
			return false;
		}

		public bool MergeLoadPreset()
		{
			if (LoadPresetPre(true))
			{
				bool result = LoadPresetPost();
				FileManager.PopLoadDir();
				return result;
			}
			return false;
		}

		public void LoadPresetFromJSON(JSONClass inputJSON, bool isMerge = false)
		{
			string storeFolderPath = GetStoreFolderPath(false);
			bool flag = false;
			if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty && _presetName != null && _presetName != string.Empty)
			{
				string path = presetPackagePath + storeFolderPath + presetSubPath + storeName + "_" + presetSubName + ".vap";
				FileManager.PushLoadDirFromFilePath(path);
				flag = true;
			}
			LoadPresetPreFromJSON(inputJSON, isMerge);
			LoadPresetPost();
			if (flag)
			{
				FileManager.PopLoadDir();
			}
		}

		protected void LoadPresetPreFromJSON(JSONClass inputJSON, bool isMerge = false)
		{
			lastLoadedJSON = inputJSON;
			if (isMerge)
			{
				isMergeRestore = true;
				setUnlistedParamsToDefault = false;
			}
			else
			{
				isMergeRestore = false;
				if (neverSetUnlistedParamsToDefault)
				{
					setUnlistedParamsToDefault = false;
				}
				else
				{
					setUnlistedParamsToDefault = true;
					if (inputJSON["setUnlistedParamsToDefault"] != null)
					{
						setUnlistedParamsToDefault = inputJSON["setUnlistedParamsToDefault"].AsBool;
						isMergeRestore = !setUnlistedParamsToDefault;
					}
				}
			}
			filteredJSON = new JSONClass();
			if (conditionalLoadEvents != null)
			{
				ConditionalLoadEvent[] array = conditionalLoadEvents;
				foreach (ConditionalLoadEvent conditionalLoadEvent in array)
				{
					if (conditionalLoadEvent.flag != null && conditionalLoadEvent.flag != string.Empty && inputJSON[conditionalLoadEvent.flag] != null)
					{
						filteredJSON[conditionalLoadEvent.flag] = inputJSON[conditionalLoadEvent.flag];
					}
				}
			}
			if (setUnlistedParamsToDefault)
			{
				LoadDefaultsPreInternal();
			}
			else
			{
				filteredJSON["storables"] = new JSONArray();
			}
			if (optionalFirst)
			{
				if (includeOptional)
				{
					FilterStorables(inputJSON, filteredJSON, optionalStorables);
				}
				if (includeOptional2)
				{
					FilterStorables(inputJSON, filteredJSON, optionalStorables2);
				}
				if (includeOptional3)
				{
					FilterStorables(inputJSON, filteredJSON, optionalStorables3);
				}
			}
			FilterStorables(inputJSON, filteredJSON, storables);
			RefreshDynamicStorables();
			FilterStorables(inputJSON, filteredJSON, dynamicStorables);
			if (!optionalFirst)
			{
				if (includeOptional)
				{
					FilterStorables(inputJSON, filteredJSON, optionalStorables);
				}
				if (includeOptional2)
				{
					FilterStorables(inputJSON, filteredJSON, optionalStorables2);
				}
				if (includeOptional3)
				{
					FilterStorables(inputJSON, filteredJSON, optionalStorables3);
				}
			}
		}

		public bool LoadPresetPre(bool isMerge = false)
		{
			bool result = false;
			if (itemType != 0)
			{
				string storeFolderPath = GetStoreFolderPath(false);
				if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty && _presetName != null && _presetName != string.Empty)
				{
					string text = presetPackagePath + storeFolderPath + presetSubPath + storeName + "_" + presetSubName + ".vap";
					if (FileManager.FileExists(text))
					{
						string empty = string.Empty;
						try
						{
							FileManager.PushLoadDirFromFilePath(text);
							using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(text, true))
							{
								empty = fileEntryStreamReader.ReadToEnd();
								JSONNode jSONNode = JSON.Parse(empty);
								JSONClass asObject = jSONNode.AsObject;
								LoadPresetPreFromJSON(asObject, isMerge);
								result = true;
							}
						}
						catch (Exception ex)
						{
							SuperController.LogError("Exception while loading " + text + " " + ex);
						}
					}
					else
					{
						SuperController.LogError("Could not load json " + text);
					}
					RestorePresetBinary();
				}
				else
				{
					SuperController.LogError("Not all preset parameters set. Cannot load");
				}
			}
			else
			{
				SuperController.LogError("Item type set to None. Cannot load");
			}
			return result;
		}

		protected void LoadDefaultsPreProcessStorables(List<Storable> sts, JSONArray outputStorables)
		{
			if (sts == null)
			{
				return;
			}
			Dictionary<string, JSONClass> dictionary = new Dictionary<string, JSONClass>();
			foreach (JSONClass outputStorable in outputStorables)
			{
				if (outputStorable["id"] != null)
				{
					dictionary.Add(outputStorable["id"], outputStorable);
				}
			}
			foreach (Storable st in sts)
			{
				JSONStorable storable = st.storable;
				if ((ignoreExclude || !storable.exclude) && st.specificKey != null && st.specificKey != string.Empty)
				{
					JSONClass value;
					if (!dictionary.TryGetValue(storable.storeId, out value))
					{
						value = new JSONClass();
						value["id"] = storable.storeId;
						outputStorables.Add(value);
						dictionary.Add(storable.storeId, value);
					}
					if (st.isSpecificKeyAnObject)
					{
						JSONClass value2 = new JSONClass();
						value[st.specificKey] = value2;
					}
					else
					{
						value[st.specificKey] = string.Empty;
					}
				}
			}
		}

		protected void LoadDefaultsPreInternal()
		{
			JSONArray jSONArray = new JSONArray();
			filteredJSON["storables"] = jSONArray;
			if (optionalFirst)
			{
				if (includeOptional && setOptionalToDefaultOnRestore)
				{
					LoadDefaultsPreProcessStorables(optionalStorables, jSONArray);
				}
				if (includeOptional2 && setOptional2ToDefaultOnRestore)
				{
					LoadDefaultsPreProcessStorables(optionalStorables2, jSONArray);
				}
				if (includeOptional3 && setOptional3ToDefaultOnRestore)
				{
					LoadDefaultsPreProcessStorables(optionalStorables3, jSONArray);
				}
			}
			LoadDefaultsPreProcessStorables(storables, jSONArray);
			if (!optionalFirst)
			{
				if (includeOptional && setOptionalToDefaultOnRestore)
				{
					LoadDefaultsPreProcessStorables(optionalStorables, jSONArray);
				}
				if (includeOptional2 && setOptional2ToDefaultOnRestore)
				{
					LoadDefaultsPreProcessStorables(optionalStorables2, jSONArray);
				}
				if (includeOptional3 && setOptional3ToDefaultOnRestore)
				{
					LoadDefaultsPreProcessStorables(optionalStorables3, jSONArray);
				}
			}
		}

		public bool LoadDefaultsPre()
		{
			bool result = false;
			if (itemType != 0)
			{
				string storeFolderPath = GetStoreFolderPath(false);
				if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty)
				{
					lastLoadedJSON = new JSONClass();
					if (conditionalFlagsToStore != null && setConditionalFlagsOnLoadDefaults)
					{
						string[] array = conditionalFlagsToStore;
						foreach (string aKey in array)
						{
							lastLoadedJSON[aKey].AsBool = true;
						}
					}
					filteredJSON = lastLoadedJSON;
					isMergeRestore = false;
					setUnlistedParamsToDefault = true;
					LoadDefaultsPreInternal();
					result = true;
				}
			}
			return result;
		}

		public bool LoadPresetPost()
		{
			if (filteredJSON != null)
			{
				RestoreStorables(filteredJSON);
				return true;
			}
			return false;
		}

		protected void RefreshSpecificStorables(SpecificStorable[] spStorables, List<Storable> outputStorables)
		{
			foreach (SpecificStorable specificStorable in spStorables)
			{
				if (!(specificStorable.specificStorableBucket != null))
				{
					continue;
				}
				JSONStorable[] array = ((!specificStorable.includeChildren) ? specificStorable.specificStorableBucket.GetComponents<JSONStorable>() : specificStorable.specificStorableBucket.GetComponentsInChildren<JSONStorable>(true));
				JSONStorable[] array2 = array;
				foreach (JSONStorable jSONStorable in array2)
				{
					if (!(jSONStorable is PresetManagerControl) && (specificStorable.storeId == string.Empty || specificStorable.storeId == jSONStorable.storeId))
					{
						Storable storable = new Storable();
						storable.storable = jSONStorable;
						if (specificStorable.specificKey != null && specificStorable.specificKey != string.Empty && !specificKeyStorables.ContainsKey(specificStorable.storeId))
						{
							specificKeyStorables.Add(specificStorable.storeId, true);
						}
						storable.specificKey = specificStorable.specificKey;
						storable.isSpecificKeyAnObject = specificStorable.isSpecificKeyAnObject;
						outputStorables.Add(storable);
						if (!regularStorables.ContainsKey(jSONStorable))
						{
							regularStorables.Add(jSONStorable, true);
						}
					}
				}
			}
		}

		public void RefreshStorables()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			storables = new List<Storable>();
			optionalStorables = new List<Storable>();
			optionalStorables2 = new List<Storable>();
			optionalStorables3 = new List<Storable>();
			regularStorables = new Dictionary<JSONStorable, bool>();
			specificKeyStorables = new Dictionary<string, bool>();
			if (specificStorables != null && specificStorables.Length > 0)
			{
				RefreshSpecificStorables(specificStorables, storables);
			}
			if (optionalSpecificStorables != null && optionalSpecificStorables.Length > 0)
			{
				RefreshSpecificStorables(optionalSpecificStorables, optionalStorables);
			}
			if (optionalSpecificStorables2 != null && optionalSpecificStorables2.Length > 0)
			{
				RefreshSpecificStorables(optionalSpecificStorables2, optionalStorables2);
			}
			if (optionalSpecificStorables3 != null && optionalSpecificStorables3.Length > 0)
			{
				RefreshSpecificStorables(optionalSpecificStorables3, optionalStorables3);
			}
			if (!useTransformAndChildren)
			{
				return;
			}
			JSONStorable[] componentsInChildren = GetComponentsInChildren<JSONStorable>(true);
			JSONStorable[] array = componentsInChildren;
			foreach (JSONStorable jSONStorable in array)
			{
				if (!regularStorables.ContainsKey(jSONStorable) && !(jSONStorable is PresetManagerControl))
				{
					Storable storable = new Storable();
					storable.storable = jSONStorable;
					storables.Add(storable);
					regularStorables.Add(jSONStorable, true);
				}
			}
		}

		protected void RefreshDynamicStorables()
		{
			if (dynamicStorables != null)
			{
				ClearLockStorablesInList(dynamicStorables);
			}
			dynamicStorables = new List<Storable>();
			Transform[] array = dynamicStorablesBuckets;
			foreach (Transform transform in array)
			{
				JSONStorable[] componentsInChildren = transform.GetComponentsInChildren<JSONStorable>(true);
				JSONStorable[] array2 = componentsInChildren;
				foreach (JSONStorable jSONStorable in array2)
				{
					if (!regularStorables.ContainsKey(jSONStorable))
					{
						Storable storable = new Storable();
						storable.storable = jSONStorable;
						dynamicStorables.Add(storable);
					}
				}
			}
			LockStorablesInList(dynamicStorables);
		}

		public void SyncMaterialOptions()
		{
			MaterialOptions[] array = ((!includeChildrenMaterialOptions) ? GetComponents<MaterialOptions>() : GetComponentsInChildren<MaterialOptions>(true));
			string storeFolderPath = GetStoreFolderPath();
			MaterialOptions[] array2 = array;
			foreach (MaterialOptions materialOptions in array2)
			{
				materialOptions.SetCustomTextureFolder(storeFolderPath);
			}
		}

		protected virtual void Awake()
		{
			RefreshStorables();
			RefreshDynamicStorables();
			SyncMaterialOptions();
		}
	}
}
