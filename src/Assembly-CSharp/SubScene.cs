using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;

public class SubScene : JSONStorable
{
	public MotionAnimationMaster motionAnimationMaster;

	public Material lineMaterial;

	protected LineDrawer lineDrawer;

	protected bool _drawContainedAtomsLines;

	protected JSONStorableBool drawContainedAtomsLinesJSON;

	protected Dictionary<string, string> storeRootMap = new Dictionary<string, string>
	{
		{ "Standard", "Custom/SubScene/" },
		{ "Wizard Set", "Custom/Wizard/Sets/" },
		{ "Wizard Scenario", "Custom/Wizard/Scenarios/" },
		{ "Wizard Glue", "Custom/Wizard/Glue/" }
	};

	protected string storeRoot = "Custom/SubScene/";

	protected JSONStorableUrl browsePathJSON;

	protected JSONStorableBool autoSetSubSceneUIDToSignatureOnBrowseLoadJSON;

	protected JSONStorableActionPresetFilePath loadSubSceneWithPathJSON;

	protected JSONStorableUrl loadSubSceneWithPathUrlJSON;

	protected JSONStorableUrl storePathJSON;

	protected JSONStorableString packageUidJSON;

	protected JSONStorableAction ClearPackageUidAction;

	protected JSONStorableString creatorNameJSON;

	protected JSONStorableString storedCreatorNameJSON;

	protected JSONStorableAction SetToYourCreatorNameAction;

	protected JSONStorableString signatureJSON;

	protected JSONStorableString storeNameJSON;

	protected JSONStorableAction ClearSubSceneAction;

	protected JSONStorableAction UnparentAllAtomsAction;

	protected bool _loadOnRestoreFromOtherSubScene = true;

	protected JSONStorableBool loadOnRestoreFromOtherSubSceneJSON;

	protected JSONStorableAction AddLooseAtomsToSubSceneAction;

	protected JSONStorableAction IsolateEditSubSceneAction;

	public bool isIsolateEditing;

	protected JSONStorableAction StoreSubSceneAction;

	protected JSONStorableAction LoadSubSceneAction;

	protected HashSet<Atom> _atomsInSubScene;

	public IEnumerable<Atom> atomsInSubScene => _atomsInSubScene;

	protected void SyncDrawContainedAtomsLines(bool b)
	{
		_drawContainedAtomsLines = b;
	}

	public override void PostRestore()
	{
		if (_loadOnRestoreFromOtherSubScene && containingAtom.isSubSceneRestore)
		{
			LoadSubScene();
		}
	}

	protected void SetNamesFromFilePath(string fpath, bool noCallback = false)
	{
		VarFileEntry varFileEntry = FileManager.GetVarFileEntry(fpath);
		string text = string.Empty;
		string text2 = fpath;
		if (varFileEntry != null)
		{
			text = varFileEntry.Package.Uid;
			text2 = varFileEntry.InternalSlashPath;
		}
		string text3 = text2.Replace(storeRoot, string.Empty);
		if (text3 == text2)
		{
			SuperController.LogError("SubScene path " + fpath + " is not in correct form");
		}
		else
		{
			string[] array = text3.Split('/');
			if (array.Length < 3)
			{
				SuperController.LogError("SubScene path " + fpath + " is not in correct form");
			}
			else
			{
				string text4 = array[2];
				for (int i = 3; i < array.Length; i++)
				{
					text4 = text4 + "/" + array[i];
				}
				text4 = Regex.Replace(text4, "\\.json$", string.Empty);
				if (noCallback)
				{
					packageUidJSON.valNoCallback = text;
					creatorNameJSON.valNoCallback = array[0];
					signatureJSON.valNoCallback = array[1];
					storeNameJSON.valNoCallback = text4;
					storedCreatorNameJSON.valNoCallback = array[0];
				}
				else
				{
					packageUidJSON.val = text;
					creatorNameJSON.val = array[0];
					signatureJSON.val = array[1];
					storeNameJSON.val = text4;
					storedCreatorNameJSON.val = array[0];
				}
			}
		}
		SyncStoreButton();
		SyncLoadButton();
	}

	protected void BeginBrowse(JSONStorableUrl jsurl)
	{
		FileManager.CreateDirectory(storeRoot);
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(storeRoot, false, false, true, true);
		jsurl.shortCuts = shortCutsForDirectory;
	}

	protected void SyncBrowsePath(string url)
	{
		SetNamesFromFilePath(url);
		if (autoSetSubSceneUIDToSignatureOnBrowseLoadJSON.val && browsePathJSON.valueSetFromBrowse)
		{
			SuperController.singleton.RenameAtom(containingAtom, signatureJSON.val);
		}
		LoadSubScene();
	}

	protected void LoadSubSceneWithPath(string p)
	{
		browsePathJSON.SetFilePath(p);
	}

	protected void SetStorePathFromParts()
	{
		storePathJSON.valNoCallback = GetStorePath() + ".json";
		SyncStoreButton();
		SyncLoadButton();
	}

	protected void SyncStorePath(string url)
	{
		SetNamesFromFilePath(url, true);
	}

	protected void SyncPackageUid(string s)
	{
		SetStorePathFromParts();
	}

	protected void ClearPackageUid()
	{
		packageUidJSON.val = string.Empty;
	}

	protected void SyncCreatorName(string s)
	{
		SetStorePathFromParts();
	}

	protected void SetToYourCreatorName()
	{
		if (UserPreferences.singleton != null)
		{
			creatorNameJSON.val = UserPreferences.singleton.creatorName;
		}
	}

	protected void SyncSignature(string s)
	{
		storedCreatorNameJSON.val = string.Empty;
		SetStorePathFromParts();
	}

	protected void SyncStoreName(string s)
	{
		storedCreatorNameJSON.val = string.Empty;
		SetStorePathFromParts();
	}

	protected void ClearSubScene()
	{
		PreRemove();
	}

	protected void UnparentAllAtoms()
	{
		containingAtom.RemoveAllChildAtoms();
	}

	protected void SyncLoadOnRestoreFromOtherSubScene(bool b)
	{
		_loadOnRestoreFromOtherSubScene = b;
	}

	protected void AddLooseAtomsToSubScene()
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		foreach (Atom atom in SuperController.singleton.GetAtoms())
		{
			if (atom.canBeParented && !atom.isSubSceneType && atom.type != "PlayerNavigationPanel" && atom.parentAtom == null)
			{
				atom.SetParentAtom(containingAtom);
			}
		}
	}

	protected void IsolateEditSubScene()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.StartIsolateEditSubScene(this);
		}
	}

	public bool CheckReadyForStore()
	{
		if (packageUidJSON != null && packageUidJSON.val == string.Empty && creatorNameJSON.val != null && creatorNameJSON.val != string.Empty && signatureJSON.val != null && signatureJSON.val != string.Empty && storeNameJSON.val != null && storeNameJSON.val != string.Empty)
		{
			return true;
		}
		return false;
	}

	protected string GetStorePath(bool includePackage = true)
	{
		string text = string.Empty;
		if (packageUidJSON.val != string.Empty && includePackage)
		{
			text = packageUidJSON.val + ":/";
		}
		string text2 = text;
		return text2 + storeRoot + creatorNameJSON.val + "/" + signatureJSON.val + "/" + storeNameJSON.val;
	}

	public bool CheckExistance()
	{
		string path = GetStorePath() + ".json";
		if (FileManager.FileExists(path))
		{
			return true;
		}
		return false;
	}

	protected void SyncStoreButton()
	{
		if (!(StoreSubSceneAction.dynamicButton != null))
		{
			return;
		}
		if (CheckReadyForStore())
		{
			if (StoreSubSceneAction.dynamicButton.button != null)
			{
				StoreSubSceneAction.dynamicButton.button.interactable = true;
			}
			if (CheckExistance())
			{
				StoreSubSceneAction.dynamicButton.buttonColor = Color.red;
				if (StoreSubSceneAction.dynamicButton.buttonText != null)
				{
					StoreSubSceneAction.dynamicButton.buttonText.text = "Overwrite Existing SubScene";
				}
			}
			else
			{
				StoreSubSceneAction.dynamicButton.buttonColor = Color.green;
				if (StoreSubSceneAction.dynamicButton.buttonText != null)
				{
					StoreSubSceneAction.dynamicButton.buttonText.text = "Save New SubScene";
				}
			}
		}
		else
		{
			StoreSubSceneAction.dynamicButton.buttonColor = Color.gray;
			if (StoreSubSceneAction.dynamicButton.button != null)
			{
				StoreSubSceneAction.dynamicButton.button.interactable = false;
			}
			if (StoreSubSceneAction.dynamicButton.buttonText != null)
			{
				StoreSubSceneAction.dynamicButton.buttonText.text = "Not Ready For Save";
			}
		}
	}

	public void StoreSubScene()
	{
		if (!CheckReadyForStore())
		{
			return;
		}
		try
		{
			string storePath = GetStorePath();
			string path = storePath + ".json";
			string directoryName = FileManager.GetDirectoryName(storePath);
			FileManager.CreateDirectory(directoryName);
			FileManager.SetSaveDirFromFilePath(path);
			JSONClass jSONClass = new JSONClass();
			jSONClass["setUnlistedParamsToDefault"].AsBool = true;
			drawContainedAtomsLinesJSON.StoreJSON(jSONClass, true, true, true);
			JSONClass jc = (JSONClass)(jSONClass["subScene"] = new JSONClass());
			containingAtom.StoreForSubScene(jc, true);
			JSONArray jSONArray = (JSONArray)(jSONClass["atoms"] = new JSONArray());
			foreach (Atom item in _atomsInSubScene)
			{
				FreeControllerV3[] freeControllers = item.freeControllers;
				foreach (FreeControllerV3 freeControllerV in freeControllers)
				{
					freeControllerV.forceStorePositionRotationAsLocal = true;
				}
			}
			List<Atom> list = _atomsInSubScene.ToList();
			list.Sort((Atom a1, Atom a2) => a1.uid.CompareTo(a2.uid));
			foreach (Atom item2 in list)
			{
				JSONClass jSONClass2 = new JSONClass();
				jSONArray.Add(jSONClass2);
				item2.StoreForSubScene(jSONClass2);
			}
			foreach (Atom item3 in _atomsInSubScene)
			{
				FreeControllerV3[] freeControllers2 = item3.freeControllers;
				foreach (FreeControllerV3 freeControllerV2 in freeControllers2)
				{
					freeControllerV2.forceStorePositionRotationAsLocal = false;
				}
			}
			StringBuilder stringBuilder = new StringBuilder(100000);
			jSONClass.ToString(string.Empty, stringBuilder);
			string value = stringBuilder.ToString();
			StreamWriter streamWriter = FileManager.OpenStreamWriter(path);
			streamWriter.Write(value);
			streamWriter.Close();
			if (true)
			{
				string text = storePath + ".jpg";
				text = text.Replace('/', '\\');
				SuperController.singleton.DoSaveScreenshot(text);
			}
			SyncStoreButton();
			SyncLoadButton();
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception during StoreSubScene " + ex);
		}
	}

	protected void SyncLoadButton()
	{
		bool flag = CheckExistance();
		if (!(LoadSubSceneAction.dynamicButton != null) || !(LoadSubSceneAction.dynamicButton.button != null))
		{
			return;
		}
		LoadSubSceneAction.dynamicButton.button.interactable = flag;
		if (LoadSubSceneAction.dynamicButton.buttonText != null)
		{
			if (flag)
			{
				LoadSubSceneAction.dynamicButton.buttonText.text = "Load SubScene";
			}
			else
			{
				LoadSubSceneAction.dynamicButton.buttonText.text = "Not Ready For Load";
			}
		}
	}

	protected IEnumerator LoadSubSceneCo(JSONClass inputJSON)
	{
		AsyncFlag loadFlag = new AsyncFlag("SubScene " + containingAtom.uid + " load");
		SuperController.singleton.ResetSimulation(loadFlag, true);
		bool setUnlistedParamsToDefault = true;
		if (inputJSON["setUnlistedParamsToDefault"] != null)
		{
			setUnlistedParamsToDefault = inputJSON["setUnlistedParamsToDefault"].AsBool;
		}
		drawContainedAtomsLinesJSON.RestoreFromJSON(inputJSON, true, true, setUnlistedParamsToDefault);
		containingAtom.PreRestoreForSubScene();
		foreach (Atom item in _atomsInSubScene)
		{
			item.isSubSceneRestore = true;
			item.PreRestoreForSubScene();
			item.isSubSceneRestore = false;
		}
		Dictionary<string, List<Atom>> typeToAtomPool = new Dictionary<string, List<Atom>>();
		Dictionary<string, Atom> existingAtomUidToAtom = new Dictionary<string, Atom>();
		Dictionary<string, Atom> newAtomUidToAtom = new Dictionary<string, Atom>();
		foreach (Atom item2 in _atomsInSubScene)
		{
			existingAtomUidToAtom.Add(item2.uidWithoutSubScenePath, item2);
			List<Atom> value;
			if (!typeToAtomPool.TryGetValue(item2.type, out value))
			{
				value = new List<Atom>();
				typeToAtomPool.Add(item2.type, value);
			}
			value.Add(item2);
		}
		JSONArray jatoms = inputJSON["atoms"].AsArray;
		IEnumerator enumerator3 = jatoms.GetEnumerator();
		try
		{
			while (enumerator3.MoveNext())
			{
				JSONClass jSONClass = (JSONClass)enumerator3.Current;
				string key = jSONClass["id"];
				string text = jSONClass["type"];
				Atom value2;
				if (existingAtomUidToAtom.TryGetValue(key, out value2) && value2.type == text)
				{
					newAtomUidToAtom.Add(key, value2);
					List<Atom> value3;
					if (typeToAtomPool.TryGetValue(text, out value3))
					{
						value3.Remove(value2);
					}
				}
			}
		}
		finally
		{
			IDisposable disposable;
			IDisposable disposable2 = (disposable = enumerator3 as IDisposable);
			if (disposable != null)
			{
				disposable2.Dispose();
			}
		}
		IEnumerator enumerator4 = jatoms.GetEnumerator();
		try
		{
			while (enumerator4.MoveNext())
			{
				JSONClass jSONClass2 = (JSONClass)enumerator4.Current;
				string text2 = jSONClass2["id"];
				string key2 = jSONClass2["type"];
				List<Atom> value4;
				if (!newAtomUidToAtom.ContainsKey(text2) && typeToAtomPool.TryGetValue(key2, out value4) && value4.Count > 0)
				{
					Atom atom = value4[0];
					value4.RemoveAt(0);
					newAtomUidToAtom.Add(text2, atom);
					atom.SetUID(text2);
				}
			}
		}
		finally
		{
			IDisposable disposable;
			IDisposable disposable3 = (disposable = enumerator4 as IDisposable);
			if (disposable != null)
			{
				disposable3.Dispose();
			}
		}
		foreach (string key6 in typeToAtomPool.Keys)
		{
			List<Atom> value5;
			if (!typeToAtomPool.TryGetValue(key6, out value5))
			{
				continue;
			}
			foreach (Atom item3 in value5)
			{
				item3.Remove();
			}
		}
		string subScenePath = containingAtom.uid;
		IEnumerator enumerator7 = jatoms.GetEnumerator();
		try
		{
			while (enumerator7.MoveNext())
			{
				JSONClass jatom = (JSONClass)enumerator7.Current;
				string auid = jatom["id"];
				string atype = jatom["type"];
				if (!newAtomUidToAtom.ContainsKey(auid))
				{
					string newauid = subScenePath + "/" + auid;
					SuperController.singleton.ResetSimulation(loadFlag, true);
					yield return SuperController.singleton.AddAtomByType(atype, newauid);
					Atom atom2 = SuperController.singleton.GetAtomByUid(newauid);
					if (atom2 != null)
					{
						atom2.SelectAtomParent(containingAtom);
						newAtomUidToAtom.Add(auid, atom2);
					}
					else
					{
						SuperController.LogError("Could not add subscene atom " + newauid);
					}
				}
			}
		}
		finally
		{
			IDisposable disposable;
			IDisposable disposable4 = (disposable = enumerator7 as IDisposable);
			if (disposable != null)
			{
				disposable4.Dispose();
			}
		}
		yield return null;
		JSONClass ssjc = inputJSON["subScene"].AsObject;
		IEnumerator enumerator8 = jatoms.GetEnumerator();
		try
		{
			while (enumerator8.MoveNext())
			{
				JSONClass jSONClass3 = (JSONClass)enumerator8.Current;
				string text3 = jSONClass3["id"];
				Atom value6;
				if (newAtomUidToAtom.TryGetValue(text3, out value6))
				{
					if (jSONClass3["parentAtom"] != null)
					{
						string text4 = subScenePath + "/" + jSONClass3["parentAtom"];
						Atom atomByUid = SuperController.singleton.GetAtomByUid(text4);
						if (atomByUid == null)
						{
							SuperController.LogError("Could not find subscene atom parent " + text4 + " for subscene atom " + text3);
						}
						else
						{
							value6.SelectAtomParent(atomByUid);
						}
					}
					else if (setUnlistedParamsToDefault)
					{
						value6.SelectAtomParent(containingAtom);
					}
				}
				else
				{
					SuperController.LogError("Could not find subscene atom " + text3 + " after it should have been created");
				}
			}
		}
		finally
		{
			IDisposable disposable;
			IDisposable disposable5 = (disposable = enumerator8 as IDisposable);
			if (disposable != null)
			{
				disposable5.Dispose();
			}
		}
		IEnumerator enumerator9 = jatoms.GetEnumerator();
		try
		{
			while (enumerator9.MoveNext())
			{
				JSONClass jSONClass4 = (JSONClass)enumerator9.Current;
				string key3 = jSONClass4["id"];
				Atom value7;
				if (newAtomUidToAtom.TryGetValue(key3, out value7))
				{
					value7.isSubSceneRestore = true;
					value7.RestoreTransform(jSONClass4, setUnlistedParamsToDefault);
					value7.isSubSceneRestore = false;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			IDisposable disposable6 = (disposable = enumerator9 as IDisposable);
			if (disposable != null)
			{
				disposable6.Dispose();
			}
		}
		containingAtom.Restore(ssjc, true, true, false, null, false, true, setUnlistedParamsToDefault, true);
		IEnumerator enumerator10 = jatoms.GetEnumerator();
		try
		{
			while (enumerator10.MoveNext())
			{
				JSONClass jSONClass5 = (JSONClass)enumerator10.Current;
				string key4 = jSONClass5["id"];
				Atom value8;
				if (newAtomUidToAtom.TryGetValue(key4, out value8))
				{
					value8.isSubSceneRestore = true;
					value8.Restore(jSONClass5, true, true, true, null, false, true, setUnlistedParamsToDefault);
					value8.isSubSceneRestore = false;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			IDisposable disposable7 = (disposable = enumerator10 as IDisposable);
			if (disposable != null)
			{
				disposable7.Dispose();
			}
		}
		containingAtom.LateRestore(ssjc, true, true, false, true, setUnlistedParamsToDefault, true);
		IEnumerator enumerator11 = jatoms.GetEnumerator();
		try
		{
			while (enumerator11.MoveNext())
			{
				JSONClass jSONClass6 = (JSONClass)enumerator11.Current;
				string key5 = jSONClass6["id"];
				Atom value9;
				if (newAtomUidToAtom.TryGetValue(key5, out value9))
				{
					value9.isSubSceneRestore = true;
					value9.LateRestore(jSONClass6, true, true, true, true, setUnlistedParamsToDefault);
					value9.isSubSceneRestore = false;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			IDisposable disposable8 = (disposable = enumerator11 as IDisposable);
			if (disposable != null)
			{
				disposable8.Dispose();
			}
		}
		foreach (Atom item4 in _atomsInSubScene)
		{
			item4.isSubSceneRestore = true;
			item4.PostRestore();
			item4.isSubSceneRestore = false;
		}
		yield return null;
		loadFlag.Raise();
		SuperController.singleton.NotifySubSceneLoad(this);
	}

	public void LoadSubScene()
	{
		if (!CheckExistance())
		{
			return;
		}
		JSONClass jSONClass = null;
		try
		{
			string storePath = GetStorePath();
			string path = storePath + ".json";
			FileManager.PushLoadDirFromFilePath(path);
			using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(path, true))
			{
				string aJSON = fileEntryStreamReader.ReadToEnd();
				JSONNode jSONNode = JSON.Parse(aJSON);
				jSONClass = jSONNode.AsObject;
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception during LoadSubScene " + ex);
		}
		if (jSONClass != null)
		{
			SuperController.singleton.StartCoroutine(LoadSubSceneCo(jSONClass));
		}
	}

	protected bool IsAtomInThisSubScene(Atom atom)
	{
		Atom parentAtom = atom.parentAtom;
		while (parentAtom != null)
		{
			if (parentAtom == containingAtom)
			{
				return true;
			}
			if (parentAtom.isSubSceneType)
			{
				return false;
			}
			parentAtom = parentAtom.parentAtom;
		}
		return false;
	}

	protected void OnAtomParentChanged(Atom atom, Atom newParent)
	{
		if (IsAtomInThisSubScene(atom))
		{
			AddAtomToSubScene(atom);
		}
		else
		{
			RemoveAtomFromSubScene(atom);
		}
	}

	protected void AddAtomToSubScene(Atom atom)
	{
		if (_atomsInSubScene.Contains(atom))
		{
			return;
		}
		if (!atom.isSubSceneType)
		{
			foreach (Atom child in atom.GetChildren())
			{
				AddAtomToSubScene(child);
			}
		}
		_atomsInSubScene.Add(atom);
		MotionAnimationControl[] motionAnimationControls = atom.motionAnimationControls;
		foreach (MotionAnimationControl mac in motionAnimationControls)
		{
			motionAnimationMaster.RegisterAnimationControl(mac);
		}
		if (SuperController.singleton != null)
		{
			SuperController.singleton.AtomSubSceneChanged(atom, this);
		}
	}

	protected void RemoveAtomFromSubScene(Atom atom)
	{
		if (!_atomsInSubScene.Contains(atom))
		{
			return;
		}
		if (!atom.isSubSceneType)
		{
			foreach (Atom child in atom.GetChildren())
			{
				RemoveAtomFromSubScene(child);
			}
		}
		_atomsInSubScene.Remove(atom);
		MotionAnimationControl[] motionAnimationControls = atom.motionAnimationControls;
		foreach (MotionAnimationControl mac in motionAnimationControls)
		{
			motionAnimationMaster.DeregisterAnimationControl(mac);
		}
		if (SuperController.singleton != null && atom.containingSubScene == this)
		{
			SuperController.singleton.AtomSubSceneChanged(atom, null);
		}
	}

	protected void Init()
	{
		if (lineMaterial != null)
		{
			lineDrawer = new LineDrawer(lineMaterial);
		}
		browsePathJSON = new JSONStorableUrl("browsePath", string.Empty, SyncBrowsePath, "json", storeRoot, true);
		browsePathJSON.beginBrowseWithObjectCallback = BeginBrowse;
		browsePathJSON.allowFullComputerBrowse = false;
		browsePathJSON.allowBrowseAboveSuggestedPath = false;
		browsePathJSON.hideExtension = false;
		browsePathJSON.showDirs = true;
		browsePathJSON.isRestorable = false;
		browsePathJSON.isStorable = false;
		RegisterUrl(browsePathJSON);
		autoSetSubSceneUIDToSignatureOnBrowseLoadJSON = new JSONStorableBool("autoSetSubSceneUIDToSignatureOnBrowseLoad", true);
		autoSetSubSceneUIDToSignatureOnBrowseLoadJSON.isRestorable = false;
		autoSetSubSceneUIDToSignatureOnBrowseLoadJSON.isStorable = false;
		RegisterBool(autoSetSubSceneUIDToSignatureOnBrowseLoadJSON);
		packageUidJSON = new JSONStorableString("packageUid", string.Empty, SyncPackageUid);
		packageUidJSON.isRestorable = false;
		packageUidJSON.isStorable = false;
		ClearPackageUidAction = new JSONStorableAction("ClearPackageUid", ClearPackageUid);
		RegisterAction(ClearPackageUidAction);
		creatorNameJSON = new JSONStorableString("creatorName", UserPreferences.singleton.creatorName, SyncCreatorName);
		creatorNameJSON.isRestorable = false;
		creatorNameJSON.isStorable = false;
		creatorNameJSON.enableOnChange = true;
		RegisterString(creatorNameJSON);
		SetToYourCreatorNameAction = new JSONStorableAction("SetToYourCreatorName", SetToYourCreatorName);
		RegisterAction(SetToYourCreatorNameAction);
		storedCreatorNameJSON = new JSONStorableString("storedCreatorName", string.Empty);
		signatureJSON = new JSONStorableString("signature", string.Empty, SyncSignature);
		signatureJSON.isRestorable = false;
		signatureJSON.isStorable = false;
		signatureJSON.enableOnChange = true;
		RegisterString(signatureJSON);
		storeNameJSON = new JSONStorableString("storeName", string.Empty, SyncStoreName);
		storeNameJSON.isRestorable = false;
		storeNameJSON.isStorable = false;
		storeNameJSON.enableOnChange = true;
		RegisterString(storeNameJSON);
		storePathJSON = new JSONStorableUrl("storePath", string.Empty, SyncStorePath);
		storePathJSON.storeType = JSONStorableParam.StoreType.Full;
		RegisterUrl(storePathJSON);
		StoreSubSceneAction = new JSONStorableAction("StoreSubScene", StoreSubScene);
		RegisterAction(StoreSubSceneAction);
		LoadSubSceneAction = new JSONStorableAction("LoadSubScene", LoadSubScene);
		RegisterAction(LoadSubSceneAction);
		loadSubSceneWithPathUrlJSON = new JSONStorableUrl("loadSubSceneWithPathUrl", string.Empty, "json", storeRoot);
		loadSubSceneWithPathUrlJSON.beginBrowseWithObjectCallback = BeginBrowse;
		loadSubSceneWithPathUrlJSON.allowFullComputerBrowse = false;
		loadSubSceneWithPathUrlJSON.allowBrowseAboveSuggestedPath = false;
		loadSubSceneWithPathUrlJSON.hideExtension = false;
		loadSubSceneWithPathUrlJSON.showDirs = true;
		loadSubSceneWithPathJSON = new JSONStorableActionPresetFilePath("LoadSubSceneWithPath", LoadSubSceneWithPath, loadSubSceneWithPathUrlJSON);
		RegisterPresetFilePathAction(loadSubSceneWithPathJSON);
		ClearSubSceneAction = new JSONStorableAction("ClearSubScene", ClearSubScene);
		RegisterAction(ClearSubSceneAction);
		UnparentAllAtomsAction = new JSONStorableAction("UnparentAllAtoms", UnparentAllAtoms);
		RegisterAction(UnparentAllAtomsAction);
		loadOnRestoreFromOtherSubSceneJSON = new JSONStorableBool("loadOnRestoreFromOtherSubscene", _loadOnRestoreFromOtherSubScene, SyncLoadOnRestoreFromOtherSubScene);
		RegisterBool(loadOnRestoreFromOtherSubSceneJSON);
		AddLooseAtomsToSubSceneAction = new JSONStorableAction("AddLooseAtomsToSubScene", AddLooseAtomsToSubScene);
		RegisterAction(AddLooseAtomsToSubSceneAction);
		IsolateEditSubSceneAction = new JSONStorableAction("IsolateEditSubScene", IsolateEditSubScene);
		RegisterAction(IsolateEditSubSceneAction);
		drawContainedAtomsLinesJSON = new JSONStorableBool("drawContainedAtomsLines", _drawContainedAtomsLines, SyncDrawContainedAtomsLines);
		RegisterBool(drawContainedAtomsLinesJSON);
		_atomsInSubScene = new HashSet<Atom>();
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomParentChangedHandlers = (SuperController.OnAtomParentChanged)Delegate.Combine(singleton.onAtomParentChangedHandlers, new SuperController.OnAtomParentChanged(OnAtomParentChanged));
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			SubSceneUI componentInChildren = t.GetComponentInChildren<SubSceneUI>(true);
			if (componentInChildren != null)
			{
				browsePathJSON.RegisterFileBrowseButton(componentInChildren.beginBrowseButton, isAlt);
				autoSetSubSceneUIDToSignatureOnBrowseLoadJSON.RegisterToggle(componentInChildren.autoSetSubSceneUIDToSignatureOnBrowseLoadToggle, isAlt);
				packageUidJSON.RegisterText(componentInChildren.packageUidText, isAlt);
				ClearPackageUidAction.RegisterButton(componentInChildren.clearPackageUidButton, isAlt);
				creatorNameJSON.RegisterInputField(componentInChildren.creatorNameInputField, isAlt);
				SetToYourCreatorNameAction.RegisterButton(componentInChildren.setToYourCreatorNameButton, isAlt);
				storedCreatorNameJSON.RegisterText(componentInChildren.storedCreatorNameText, isAlt);
				signatureJSON.RegisterInputField(componentInChildren.signatureInputField, isAlt);
				storeNameJSON.RegisterInputField(componentInChildren.storeNameInputField, isAlt);
				StoreSubSceneAction.RegisterButton(componentInChildren.storeSubSceneButton, isAlt);
				LoadSubSceneAction.RegisterButton(componentInChildren.loadSubSceneButton, isAlt);
				ClearSubSceneAction.RegisterButton(componentInChildren.clearSubSceneButton, isAlt);
				UnparentAllAtomsAction.RegisterButton(componentInChildren.unparentAllAtomsButton, isAlt);
				loadOnRestoreFromOtherSubSceneJSON.RegisterToggle(componentInChildren.loadOnRestoreFromOtherSubSceneToggle, isAlt);
				AddLooseAtomsToSubSceneAction.RegisterButton(componentInChildren.addLooseAtomsToSubSceneButton, isAlt);
				IsolateEditSubSceneAction.RegisterButton(componentInChildren.isolateEditSubSceneButton, isAlt);
				drawContainedAtomsLinesJSON.RegisterToggle(componentInChildren.drawContainedAtomsLinesToggle, isAlt);
				SyncLoadButton();
				SyncStoreButton();
			}
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
		if (!_drawContainedAtomsLines || !(lineMaterial != null) || containingAtom.mainController.hidden)
		{
			return;
		}
		int count = _atomsInSubScene.Count;
		if (lineDrawer == null || lineDrawer.numLines != count)
		{
			lineDrawer = new LineDrawer(count, lineMaterial);
		}
		int num = 0;
		foreach (Atom item in _atomsInSubScene)
		{
			lineDrawer.SetLinePoints(num, base.transform.position, item.mainController.transform.position);
			num++;
		}
		lineDrawer.Draw(base.gameObject.layer);
	}

	public override void PreRemove()
	{
		List<Atom> list = new List<Atom>(_atomsInSubScene);
		foreach (Atom item in list)
		{
			SuperController.singleton.RemoveAtom(item);
		}
	}

	protected void OnDestroy()
	{
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomParentChangedHandlers = (SuperController.OnAtomParentChanged)Delegate.Remove(singleton.onAtomParentChangedHandlers, new SuperController.OnAtomParentChanged(OnAtomParentChanged));
			if (isIsolateEditing)
			{
				SuperController.singleton.EndIsolateEditSubScene();
			}
		}
	}
}
