using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using MVR.FileManagement;
using MeshVR;
using SimpleJSON;
using UnityEngine;

[ExecuteInEditMode]
public class DAZMorphBank : MonoBehaviour
{
	public delegate void MorphFavoriteChanged(DAZMorph morph);

	[SerializeField]
	protected DAZMesh _connectedMesh;

	public string autoImportFolder = string.Empty;

	public string autoImportFolderLegacy = string.Empty;

	public string conflictingFolder = string.Empty;

	public string geometryId;

	public DAZBones morphBones;

	public DAZBones morphBones2;

	public bool enableMCMMorphs = true;

	public bool bonesDirty;

	public Dictionary<DAZBone, bool> boneRotationsDirty;

	public MorphFavoriteChanged onMorphFavoriteChangedHandlers;

	protected Dictionary<string, DAZMorphSubBank> _morphSubBanksByRegion;

	protected List<DAZMorph> _morphs;

	protected List<DAZMorph> _unactivatedMorphs;

	protected Dictionary<string, DAZMorph> _builtInMorphsByName;

	protected Dictionary<string, DAZMorph> _builtInMorphsByUid;

	protected Dictionary<string, DAZMorph> _morphsByName;

	protected Dictionary<string, DAZMorph> _morphsByUid;

	protected Dictionary<string, DAZMorph> _morphsByDisplayName;

	protected bool demandActivatedMorphsDirty;

	protected Dictionary<string, string> _morphToRegionName;

	protected HashSet<string> badMorphNames;

	public bool updateEnabled = true;

	public bool useThreadedMorphing;

	protected DAZMorphTaskInfo applyMorphsTask;

	protected bool _threadsRunning;

	protected bool triggerThreadResetMorphs;

	protected bool threadedVerticesChanged;

	protected int[] threadedChangedVertices;

	protected int numThreadedChangedVertices;

	protected int numMaxThreadedChangedVertices;

	protected bool checkAllThreadedVertices;

	protected bool visibleNonPoseThreadedVerticesChanged;

	public bool visibleNonPoseVerticesChanged;

	protected Vector3[] _threadedMorphedUVVertices;

	protected Vector3[] _threadedVisibleMorphedUVVertices;

	public bool _threadedNormalsDirtyThisFrame;

	public bool _threadedTangentsDirtyThisFrame;

	protected List<DAZMorph> dirtyMorphs;

	protected int totalFrameCount;

	protected int missedFrameCount;

	protected bool wasInit;

	protected HashSet<string> currentLoadedMorphPackageUids;

	protected HashSet<string> currentPreloadMorphPackageUids;

	public DAZMesh connectedMesh
	{
		get
		{
			return _connectedMesh;
		}
		set
		{
			if (_connectedMesh != value)
			{
				_connectedMesh = value;
				ResetMorphs();
			}
		}
	}

	public List<DAZMorph> morphs => _morphs;

	public int numMorphs
	{
		get
		{
			if (_morphs == null)
			{
				return 0;
			}
			return _morphs.Count;
		}
	}

	public void NotifyMorphFavoriteChanged(DAZMorph morph)
	{
		if (onMorphFavoriteChangedHandlers != null)
		{
			onMorphFavoriteChangedHandlers(morph);
		}
	}

	protected void BuildMorphSubBanksByRegion()
	{
		if (_morphSubBanksByRegion == null)
		{
			_morphSubBanksByRegion = new Dictionary<string, DAZMorphSubBank>();
		}
		else
		{
			_morphSubBanksByRegion.Clear();
		}
		DAZMorphSubBank[] componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
		DAZMorphSubBank[] array = componentsInChildren;
		foreach (DAZMorphSubBank dAZMorphSubBank in array)
		{
			_morphSubBanksByRegion.Add(dAZMorphSubBank.name, dAZMorphSubBank);
		}
	}

	protected void BuildMorphsList()
	{
		if (_morphs == null)
		{
			_morphs = new List<DAZMorph>();
		}
		else
		{
			_morphs.Clear();
		}
		if (_unactivatedMorphs == null)
		{
			_unactivatedMorphs = new List<DAZMorph>();
		}
		else
		{
			_unactivatedMorphs.Clear();
		}
		foreach (DAZMorphSubBank value in _morphSubBanksByRegion.Values)
		{
			foreach (DAZMorph combinedMorph in value.combinedMorphs)
			{
				if (combinedMorph.isDemandLoaded && !combinedMorph.isDemandActivated)
				{
					_unactivatedMorphs.Add(combinedMorph);
				}
				else
				{
					_morphs.Add(combinedMorph);
				}
			}
		}
	}

	protected void BuildMorphsByNameAndUid()
	{
		if (_builtInMorphsByName == null)
		{
			_builtInMorphsByName = new Dictionary<string, DAZMorph>();
		}
		else
		{
			_builtInMorphsByName.Clear();
		}
		if (_builtInMorphsByUid == null)
		{
			_builtInMorphsByUid = new Dictionary<string, DAZMorph>();
		}
		else
		{
			_builtInMorphsByUid.Clear();
		}
		if (_morphsByName == null)
		{
			_morphsByName = new Dictionary<string, DAZMorph>();
		}
		else
		{
			_morphsByName.Clear();
		}
		if (_morphsByUid == null)
		{
			_morphsByUid = new Dictionary<string, DAZMorph>();
		}
		else
		{
			_morphsByUid.Clear();
		}
		if (_morphsByDisplayName == null)
		{
			_morphsByDisplayName = new Dictionary<string, DAZMorph>();
		}
		else
		{
			_morphsByDisplayName.Clear();
		}
		foreach (DAZMorph morph in _morphs)
		{
			if (morph.disable)
			{
				continue;
			}
			DAZMorph value;
			if (!_morphsByUid.ContainsKey(morph.uid))
			{
				_morphsByUid.Add(morph.uid, morph);
			}
			else if (_morphsByUid.TryGetValue(morph.uid, out value))
			{
				Debug.LogError("Duplicate morph by uid " + morph.uid + " Region 1: " + value.region + " Region 2: " + morph.region);
			}
			if (!morph.isRuntime)
			{
				_builtInMorphsByName.Add(morph.morphName, morph);
				_builtInMorphsByUid.Add(morph.uid, morph);
			}
			if (!_morphsByName.ContainsKey(morph.morphName))
			{
				_morphsByName.Add(morph.morphName, morph);
			}
			if (morph.resolvedDisplayName != null)
			{
				if (!_morphsByDisplayName.ContainsKey(morph.resolvedDisplayName))
				{
					_morphsByDisplayName.Add(morph.resolvedDisplayName, morph);
				}
			}
			else
			{
				Debug.LogError("Morph " + morph.morphName + " has null display name");
			}
		}
		foreach (DAZMorph unactivatedMorph in _unactivatedMorphs)
		{
			if (unactivatedMorph.disable)
			{
				continue;
			}
			DAZMorph value2;
			if (!_morphsByUid.ContainsKey(unactivatedMorph.uid))
			{
				_morphsByUid.Add(unactivatedMorph.uid, unactivatedMorph);
			}
			else if (_morphsByUid.TryGetValue(unactivatedMorph.uid, out value2))
			{
				Debug.LogError("Duplicate morph by uid " + unactivatedMorph.uid + " Region 1: " + value2.region + " Region 2: " + unactivatedMorph.region);
			}
			if (!_morphsByName.ContainsKey(unactivatedMorph.morphName))
			{
				_morphsByName.Add(unactivatedMorph.morphName, unactivatedMorph);
			}
			if (unactivatedMorph.resolvedDisplayName != null)
			{
				if (!_morphsByDisplayName.ContainsKey(unactivatedMorph.resolvedDisplayName))
				{
					_morphsByDisplayName.Add(unactivatedMorph.resolvedDisplayName, unactivatedMorph);
				}
			}
			else
			{
				Debug.LogError("Morph " + unactivatedMorph.morphName + " has null display name");
			}
		}
	}

	protected void DemandActivateMorph(DAZMorph dm)
	{
		if (dm.isDemandLoaded && !dm.isDemandActivated)
		{
			dm.morphBank = this;
			dm.Init();
			dm.isDemandActivated = true;
			demandActivatedMorphsDirty = true;
		}
	}

	public bool CleanDemandActivatedMorphs()
	{
		if (demandActivatedMorphsDirty)
		{
			BuildMorphsList();
			demandActivatedMorphsDirty = false;
			return true;
		}
		return false;
	}

	public bool UnloadDemandActivatedMorphs()
	{
		bool result = false;
		for (int i = 0; i < _morphs.Count; i++)
		{
			DAZMorph dAZMorph = _morphs[i];
			if (dAZMorph.isDemandActivated && dAZMorph.isDemandLoaded && !dAZMorph.active)
			{
				Debug.Log("Unload demand activated morph " + dAZMorph.uid);
				if (dAZMorph.deltasLoaded)
				{
					dAZMorph.UnloadDeltas();
				}
				dAZMorph.isDemandActivated = false;
				result = true;
			}
		}
		demandActivatedMorphsDirty = result;
		return result;
	}

	public int GetRuntimeMorphDeltasLoadedCount()
	{
		int num = 0;
		for (int i = 0; i < _morphs.Count; i++)
		{
			DAZMorph dAZMorph = _morphs[i];
			if (dAZMorph.isRuntime && dAZMorph.deltasLoaded)
			{
				num++;
			}
		}
		return num;
	}

	public void UnloadRuntimeMorphDeltas()
	{
		for (int i = 0; i < _morphs.Count; i++)
		{
			DAZMorph dAZMorph = _morphs[i];
			if (dAZMorph.isRuntime && dAZMorph.deltasLoaded && !dAZMorph.active)
			{
				Debug.Log("Unload morph deltas for " + dAZMorph.uid);
				dAZMorph.UnloadDeltas();
			}
		}
	}

	public DAZMorph GetMorph(string morphName)
	{
		Init();
		DAZMorph value;
		if (_morphsByName.TryGetValue(morphName, out value))
		{
			DemandActivateMorph(value);
			return value;
		}
		return null;
	}

	public DAZMorph GetBuiltInMorph(string morphName)
	{
		Init();
		DAZMorph value;
		if (_builtInMorphsByName.TryGetValue(morphName, out value))
		{
			return value;
		}
		return null;
	}

	public DAZMorph GetBuiltInMorphByUid(string morphName)
	{
		Init();
		DAZMorph value;
		if (_builtInMorphsByUid.TryGetValue(morphName, out value))
		{
			return value;
		}
		return null;
	}

	public DAZMorph GetMorphByUid(string uid)
	{
		Init();
		DAZMorph value;
		if (_morphsByUid.TryGetValue(uid, out value))
		{
			DemandActivateMorph(value);
			return value;
		}
		return null;
	}

	public DAZMorph GetMorphByDisplayName(string morphDisplayName)
	{
		Init();
		DAZMorph value;
		if (_morphsByDisplayName.TryGetValue(morphDisplayName, out value))
		{
			DemandActivateMorph(value);
			return value;
		}
		return null;
	}

	protected void BuildMorphToRegionName()
	{
		if (_morphToRegionName == null)
		{
			_morphToRegionName = new Dictionary<string, string>();
		}
		else
		{
			_morphToRegionName.Clear();
		}
		foreach (DAZMorphSubBank value2 in _morphSubBanksByRegion.Values)
		{
			foreach (DAZMorph combinedMorph in value2.combinedMorphs)
			{
				string value = ((combinedMorph.overrideRegion != null && combinedMorph.overrideRegion != string.Empty) ? combinedMorph.overrideRegion : ((!value2.useOverrideRegionName) ? combinedMorph.region : value2.overrideRegionName));
				if (!_morphToRegionName.ContainsKey(combinedMorph.morphName))
				{
					_morphToRegionName.Add(combinedMorph.morphName, value);
				}
			}
		}
	}

	public string GetMorphRegionName(string morphName)
	{
		Init();
		string value;
		if (_morphToRegionName.TryGetValue(morphName, out value))
		{
			return value;
		}
		return null;
	}

	public bool IsBadMorph(string name)
	{
		if (name == null)
		{
			return true;
		}
		if (badMorphNames != null && badMorphNames.Contains(name))
		{
			return true;
		}
		return false;
	}

	public bool AddMorphUsingSubBanks(DAZMorph dm)
	{
		if (badMorphNames == null)
		{
			badMorphNames = new HashSet<string>();
		}
		if (dm.numDeltas == 0 && dm.formulas.Length == 0)
		{
			if (dm.resolvedDisplayName != null && dm.resolvedDisplayName != string.Empty)
			{
				badMorphNames.Add(dm.resolvedDisplayName);
			}
			return false;
		}
		if (dm.min == dm.max)
		{
			if (dm.resolvedDisplayName != null && dm.resolvedDisplayName != string.Empty)
			{
				badMorphNames.Add(dm.resolvedDisplayName);
			}
			return false;
		}
		if (dm.resolvedDisplayName == null || dm.resolvedDisplayName == string.Empty)
		{
			return false;
		}
		if (_morphSubBanksByRegion == null)
		{
			BuildMorphSubBanksByRegion();
		}
		if (dm.region == null || dm.region == string.Empty)
		{
			dm.region = "NoRegion";
		}
		DAZMorphSubBank value;
		if (!_morphSubBanksByRegion.TryGetValue(dm.region, out value))
		{
			GameObject gameObject = new GameObject(dm.region);
			gameObject.transform.parent = base.transform;
			value = gameObject.AddComponent<DAZMorphSubBank>();
			_morphSubBanksByRegion.Add(dm.region, value);
		}
		if (value != null)
		{
			value.AddMorph(dm);
			if (wasInit && Application.isPlaying)
			{
				dm.Init();
			}
		}
		return true;
	}

	protected bool RuntimeImportMorph(FileEntry morphInfFile, bool isTransient = false, bool isDemandLoaded = false, bool forceReload = false)
	{
		bool result = false;
		string path = morphInfFile.Path;
		string text = path.Replace(".vmi", ".vmb");
		FileEntry fileEntry = FileManager.GetFileEntry(text);
		if (fileEntry != null)
		{
			try
			{
				JSONNode jSONNode = null;
				if (morphInfFile is VarFileEntry)
				{
					VarFileEntry varFileEntry = morphInfFile as VarFileEntry;
					jSONNode = varFileEntry.Package.GetJSONCache(varFileEntry.InternalSlashPath);
				}
				if (jSONNode == null)
				{
					string aJSON = FileManager.ReadAllText(morphInfFile);
					jSONNode = JSON.Parse(aJSON);
				}
				if (jSONNode != null)
				{
					DAZMorph dAZMorph = new DAZMorph();
					dAZMorph.morphBank = this;
					dAZMorph.LoadMetaFromJSON(jSONNode);
					dAZMorph.visible = true;
					dAZMorph.isRuntime = true;
					dAZMorph.isDemandLoaded = isDemandLoaded;
					if (morphInfFile is VarFileEntry)
					{
						VarFileEntry varFileEntry2 = morphInfFile as VarFileEntry;
						dAZMorph.isInPackage = true;
						dAZMorph.packageUid = varFileEntry2.Package.Uid;
						dAZMorph.packageLicense = varFileEntry2.Package.LicenseType;
						dAZMorph.isLatestVersion = varFileEntry2.Package.isNewestEnabledVersion;
						dAZMorph.version = "v" + varFileEntry2.Package.Version;
					}
					else
					{
						dAZMorph.isInPackage = false;
						dAZMorph.packageUid = null;
						dAZMorph.packageLicense = null;
						dAZMorph.isLatestVersion = true;
						dAZMorph.version = null;
					}
					dAZMorph.metaLoadPath = morphInfFile.Path;
					dAZMorph.isTransient = isTransient;
					dAZMorph.deltasLoadPath = fileEntry.Path;
					dAZMorph.uid = morphInfFile.Uid;
					if (isTransient)
					{
						DAZMorph morphByDisplayName = GetMorphByDisplayName(dAZMorph.resolvedDisplayName);
						if (morphByDisplayName == null)
						{
							result = true;
							AddMorphUsingSubBanks(dAZMorph);
						}
					}
					else
					{
						DAZMorph morphByUid = GetMorphByUid(dAZMorph.uid);
						if (morphByUid == null)
						{
							result = true;
							AddMorphUsingSubBanks(dAZMorph);
						}
						else if (forceReload && morphByUid.isRuntime && !morphByUid.isInPackage)
						{
							Debug.Log("Force reload morph " + morphByUid.uid);
							morphByUid.UnloadDeltas();
							morphByUid.CopyParameters(dAZMorph, false);
							morphByUid.formulas = dAZMorph.formulas;
							morphByUid.appliedValue = 0f;
							morphByUid.active = false;
							result = true;
						}
					}
				}
				else if (SuperController.singleton != null)
				{
					SuperController.singleton.Error("Parse error while loading morph " + path);
				}
				else
				{
					Debug.LogError("Parse error while loading morph " + path);
				}
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during read of morph binary file " + text + " " + ex);
			}
		}
		else if (SuperController.singleton != null)
		{
			SuperController.singleton.Error("Missing morph deltas file " + text + ". Cannot import");
		}
		else
		{
			Debug.LogError("Missing morph deltas file " + text + ". Cannot import");
		}
		return result;
	}

	protected bool RuntimeImportFromDir(string path, bool isTransient = false, bool isDemandLoaded = false, bool forceReload = false)
	{
		DirectoryEntry directoryEntry = FileManager.GetDirectoryEntry(path);
		return RuntimeImportFromDir(directoryEntry, isTransient, isDemandLoaded, forceReload);
	}

	protected bool RuntimeImportFromDir(DirectoryEntry de, bool isTransient = false, bool isDemandLoaded = false, bool forceReload = false)
	{
		bool result = false;
		if (de != null)
		{
			List<DirectoryEntry> subDirectories = de.SubDirectories;
			foreach (DirectoryEntry item in subDirectories)
			{
				if (RuntimeImportFromDir(item, isTransient, isDemandLoaded, forceReload))
				{
					result = true;
				}
			}
			bool flag = true;
			if (de is SystemDirectoryEntry)
			{
				List<FileEntry> files = de.GetFiles("*.dsf");
				foreach (FileEntry item2 in files)
				{
					string path = item2.Path;
					string text = path.Replace(".dsf", string.Empty);
					string text2 = text + ".vmi";
					string path2 = text + ".vmb";
					if (FileManager.FileExists(text2, true, true) && FileManager.FileExists(path2, true, true))
					{
						continue;
					}
					Debug.Log("Compiling custom morphs from " + item2);
					JSONNode jSONNode = DAZImport.ReadJSON(path);
					if (!(jSONNode != null))
					{
						continue;
					}
					JSONNode jSONNode2 = jSONNode["modifier_library"];
					if (!(jSONNode2 != null))
					{
						continue;
					}
					int num = 0;
					foreach (JSONNode item3 in jSONNode2.AsArray)
					{
						num++;
						DAZMorph dAZMorph = new DAZMorph();
						dAZMorph.Import(item3);
						string text3 = ((num != 1) ? (text + "_" + num + ".vmi") : text2);
						string path3 = text3.Replace(".vmi", ".vmb");
						bool flag2 = false;
						try
						{
							using (StreamWriter streamWriter = FileManager.OpenStreamWriter(text3))
							{
								JSONClass metaJSON = dAZMorph.GetMetaJSON();
								if (metaJSON != null)
								{
									flag2 = true;
									StringBuilder stringBuilder = new StringBuilder(10000);
									metaJSON.ToString(string.Empty, stringBuilder);
									streamWriter.Write(stringBuilder.ToString());
									dAZMorph.SaveDeltasToBinaryFile(path3);
								}
							}
							if (!flag2)
							{
								FileManager.DeleteFile(text3);
							}
						}
						catch (Exception ex)
						{
							Debug.LogError(string.Concat("Error during compile of morph ", item2, " ", ex));
						}
					}
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				List<FileEntry> files2 = de.GetFiles("*.vmi");
				foreach (FileEntry item4 in files2)
				{
					if (RuntimeImportMorph(item4, isTransient, isDemandLoaded, forceReload))
					{
						result = true;
					}
				}
			}
		}
		return result;
	}

	public bool LoadTransientMorphs(string dir)
	{
		DAZMorphSubBank[] componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
		bool flag = false;
		DAZMorphSubBank[] array = componentsInChildren;
		foreach (DAZMorphSubBank dAZMorphSubBank in array)
		{
			if (dAZMorphSubBank.ClearTransientMorphs())
			{
				flag = true;
			}
		}
		if (flag)
		{
			RebuildAllLookups();
		}
		bool flag2 = false;
		if (autoImportFolder != null && autoImportFolder != string.Empty)
		{
			string path = dir + "/" + autoImportFolder;
			if (RuntimeImportFromDir(path, true))
			{
				componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
				DAZMorphSubBank[] array2 = componentsInChildren;
				foreach (DAZMorphSubBank dAZMorphSubBank2 in array2)
				{
					dAZMorphSubBank2.CompleteTransientMorphAdd();
				}
				flag2 = true;
			}
		}
		if (autoImportFolderLegacy != null && autoImportFolderLegacy != string.Empty)
		{
			string path2 = dir + "/" + autoImportFolderLegacy;
			if (RuntimeImportFromDir(path2, true))
			{
				componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
				DAZMorphSubBank[] array3 = componentsInChildren;
				foreach (DAZMorphSubBank dAZMorphSubBank3 in array3)
				{
					dAZMorphSubBank3.CompleteTransientMorphAdd();
				}
				flag2 = true;
			}
		}
		if (flag2)
		{
			RebuildAllLookups();
		}
		if (flag || flag2)
		{
			return true;
		}
		return false;
	}

	public void ClearPackageMorphs()
	{
		DAZMorphSubBank[] componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
		bool flag = false;
		DAZMorphSubBank[] array = componentsInChildren;
		foreach (DAZMorphSubBank dAZMorphSubBank in array)
		{
			if (dAZMorphSubBank.ClearPackageMorphs())
			{
				flag = true;
			}
		}
		if (flag)
		{
			RebuildAllLookups();
		}
	}

	public void ClearTransientMorphs()
	{
		DAZMorphSubBank[] componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
		bool flag = false;
		DAZMorphSubBank[] array = componentsInChildren;
		foreach (DAZMorphSubBank dAZMorphSubBank in array)
		{
			if (dAZMorphSubBank.ClearTransientMorphs())
			{
				flag = true;
			}
		}
		if (flag)
		{
			RebuildAllLookups();
		}
	}

	public void RebuildAllLookups()
	{
		BuildMorphSubBanksByRegion();
		BuildMorphsList();
		BuildMorphsByNameAndUid();
		BuildMorphToRegionName();
	}

	protected void InitMorphs()
	{
		if (_morphs != null)
		{
			for (int i = 0; i < _morphs.Count; i++)
			{
				_morphs[i].morphBank = this;
				_morphs[i].Init();
			}
		}
	}

	public void ResetMorphs()
	{
		Init();
		if (useThreadedMorphing && Application.isPlaying)
		{
			triggerThreadResetMorphs = true;
			return;
		}
		ApplyMorphsInternal();
		if (connectedMesh != null)
		{
			connectedMesh.ResetMorphedVertices();
		}
		if (_morphs != null)
		{
			for (int i = 0; i < _morphs.Count; i++)
			{
				_morphs[i].appliedValue = 0f;
				_morphs[i].active = false;
				if (_morphs[i].isDriven)
				{
					_morphs[i].morphValue = 0f;
					_morphs[i].SetDriven(false, string.Empty, true);
				}
			}
		}
		ZeroAllBoneMorphs();
	}

	public void ResetMorphsFast(bool resetBones = true)
	{
		if (_morphs != null)
		{
			for (int i = 0; i < _morphs.Count; i++)
			{
				DAZMorph dAZMorph = _morphs[i];
				if (dAZMorph.appliedValue != 0f)
				{
					dAZMorph.appliedValue = 0f;
					dAZMorph.active = false;
				}
				if (dAZMorph.isDriven)
				{
					dAZMorph.SetValueThreadSafe(0f);
					dAZMorph.SetDriven(false, string.Empty);
					if (dirtyMorphs != null)
					{
						dirtyMorphs.Add(dAZMorph);
					}
				}
			}
		}
		if (resetBones)
		{
			ZeroAllBoneMorphs();
		}
	}

	protected void MTTask(object info)
	{
		DAZMorphTaskInfo dAZMorphTaskInfo = (DAZMorphTaskInfo)info;
		while (_threadsRunning)
		{
			dAZMorphTaskInfo.resetEvent.WaitOne(-1, true);
			if (dAZMorphTaskInfo.kill)
			{
				break;
			}
			if (dAZMorphTaskInfo.taskType == DAZMorphTaskType.ApplyMorphs)
			{
				Thread.Sleep(0);
				ApplyMorphsThreaded();
			}
			dAZMorphTaskInfo.working = false;
		}
	}

	protected void StopThreads()
	{
		_threadsRunning = false;
		if (applyMorphsTask != null)
		{
			applyMorphsTask.kill = true;
			applyMorphsTask.resetEvent.Set();
			while (applyMorphsTask.thread.IsAlive)
			{
			}
			applyMorphsTask = null;
		}
	}

	protected void StartThreads()
	{
		if (!_threadsRunning)
		{
			_threadsRunning = true;
			applyMorphsTask = new DAZMorphTaskInfo();
			applyMorphsTask.threadIndex = 0;
			applyMorphsTask.name = "ApplyMorphsTask";
			applyMorphsTask.resetEvent = new AutoResetEvent(false);
			applyMorphsTask.thread = new Thread(MTTask);
			applyMorphsTask.thread.Priority = System.Threading.ThreadPriority.Lowest;
			applyMorphsTask.taskType = DAZMorphTaskType.ApplyMorphs;
			applyMorphsTask.thread.Start(applyMorphsTask);
		}
	}

	protected void ClearDirtyMorphs()
	{
		if (dirtyMorphs == null)
		{
			dirtyMorphs = new List<DAZMorph>();
		}
		else
		{
			dirtyMorphs.Clear();
		}
	}

	protected void CleanDirtyMorphs()
	{
		foreach (DAZMorph dirtyMorph in dirtyMorphs)
		{
			dirtyMorph.SyncJSON();
			dirtyMorph.SyncDrivenUI();
		}
	}

	public void ApplyMorphsThreadedStart()
	{
		if (_morphs == null)
		{
			_morphs = new List<DAZMorph>();
		}
		ClearDirtyMorphs();
		if (threadedChangedVertices == null)
		{
			numMaxThreadedChangedVertices = _threadedMorphedUVVertices.Length;
			threadedChangedVertices = new int[numMaxThreadedChangedVertices];
		}
		checkAllThreadedVertices = false;
		numThreadedChangedVertices = 0;
		threadedVerticesChanged = false;
		visibleNonPoseThreadedVerticesChanged = false;
		bonesDirty = false;
		if (boneRotationsDirty == null)
		{
			boneRotationsDirty = new Dictionary<DAZBone, bool>();
		}
	}

	public void ApplyMorphsThreaded()
	{
		bool flag = true;
		int num = 5;
		while (flag)
		{
			num--;
			if (num == 0)
			{
				break;
			}
			flag = false;
			for (int i = 0; i < _morphs.Count; i++)
			{
				DAZMorph dAZMorph = _morphs[i];
				if (dAZMorph.disable)
				{
					continue;
				}
				float num2 = dAZMorph.morphValue;
				float appliedValue = dAZMorph.appliedValue;
				bool flag2 = appliedValue != num2;
				bool flag3 = num2 != 0f;
				if (dAZMorph.hasMorphValueFormulas && (flag2 || flag3))
				{
					DAZMorphFormula[] formulas = dAZMorph.formulas;
					foreach (DAZMorphFormula dAZMorphFormula in formulas)
					{
						if (dAZMorphFormula.targetType != 0)
						{
							continue;
						}
						DAZMorph morph = GetMorph(dAZMorphFormula.target);
						if (morph == null)
						{
							continue;
						}
						if (dAZMorph.wasZeroedKeepChildValues)
						{
							if (morph.SetDriven(false, string.Empty))
							{
								dirtyMorphs.Add(morph);
							}
						}
						else if (morph.SetValueThreadSafe(dAZMorphFormula.multiplier * num2))
						{
							morph.SetDriven(flag3, dAZMorph.displayName);
							dirtyMorphs.Add(morph);
						}
					}
				}
				dAZMorph.wasZeroedKeepChildValues = false;
				if (enableMCMMorphs && dAZMorph.hasMCMFormulas)
				{
					DAZMorphFormula[] formulas2 = dAZMorph.formulas;
					foreach (DAZMorphFormula dAZMorphFormula2 in formulas2)
					{
						if (dAZMorphFormula2.targetType == DAZMorphFormulaTargetType.MCM)
						{
							DAZMorph morph2 = GetMorph(dAZMorphFormula2.target);
							if (morph2 != null)
							{
								num2 = morph2.morphValue * dAZMorphFormula2.multiplier;
								if (dAZMorph.SetValueThreadSafe(num2))
								{
									dAZMorph.SetDriven(true, morph2.displayName);
									dirtyMorphs.Add(dAZMorph);
								}
								continue;
							}
							num2 = 0f;
							if (dAZMorph.SetValueThreadSafe(num2))
							{
								dAZMorph.SetDriven(true, "Missing source morph");
								dirtyMorphs.Add(dAZMorph);
							}
							num2 = dAZMorph.morphValue;
						}
						else if (dAZMorphFormula2.targetType == DAZMorphFormulaTargetType.MCMMult)
						{
							DAZMorph morph3 = GetMorph(dAZMorphFormula2.target);
							if (morph3 != null)
							{
								num2 = dAZMorph.morphValue * morph3.morphValue;
							}
						}
					}
					flag2 = appliedValue != num2;
					flag3 = num2 != 0f;
				}
				if (!flag2)
				{
					continue;
				}
				flag = true;
				if (dAZMorph.isRuntime && !dAZMorph.deltasLoaded)
				{
					dAZMorph.LoadDeltas();
				}
				ApplyBoneMorphs(morphBones, dAZMorph);
				ApplyBoneMorphs(morphBones2, dAZMorph);
				if (dAZMorph.deltas.Length > 0)
				{
					threadedVerticesChanged = true;
					if (dAZMorph.visible && !dAZMorph.isPoseControl)
					{
						visibleNonPoseThreadedVerticesChanged = true;
						float num3 = num2 - appliedValue;
						DAZMorphVertex[] deltas = dAZMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex in deltas)
						{
							if (dAZMorphVertex.vertex < _threadedMorphedUVVertices.Length)
							{
								if (numThreadedChangedVertices < numMaxThreadedChangedVertices)
								{
									threadedChangedVertices[numThreadedChangedVertices] = dAZMorphVertex.vertex;
									numThreadedChangedVertices++;
								}
								else
								{
									checkAllThreadedVertices = true;
								}
								_threadedMorphedUVVertices[dAZMorphVertex.vertex].x += dAZMorphVertex.delta.x * num3;
								_threadedMorphedUVVertices[dAZMorphVertex.vertex].y += dAZMorphVertex.delta.y * num3;
								_threadedMorphedUVVertices[dAZMorphVertex.vertex].z += dAZMorphVertex.delta.z * num3;
								_threadedVisibleMorphedUVVertices[dAZMorphVertex.vertex].x += dAZMorphVertex.delta.x * num3;
								_threadedVisibleMorphedUVVertices[dAZMorphVertex.vertex].y += dAZMorphVertex.delta.y * num3;
								_threadedVisibleMorphedUVVertices[dAZMorphVertex.vertex].z += dAZMorphVertex.delta.z * num3;
							}
						}
					}
					else
					{
						float num4 = num2 - appliedValue;
						DAZMorphVertex[] deltas2 = dAZMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex2 in deltas2)
						{
							if (dAZMorphVertex2.vertex < _threadedMorphedUVVertices.Length)
							{
								if (numThreadedChangedVertices < numMaxThreadedChangedVertices)
								{
									threadedChangedVertices[numThreadedChangedVertices] = dAZMorphVertex2.vertex;
									numThreadedChangedVertices++;
								}
								else
								{
									checkAllThreadedVertices = true;
								}
								_threadedMorphedUVVertices[dAZMorphVertex2.vertex].x += dAZMorphVertex2.delta.x * num4;
								_threadedMorphedUVVertices[dAZMorphVertex2.vertex].y += dAZMorphVertex2.delta.y * num4;
								_threadedMorphedUVVertices[dAZMorphVertex2.vertex].z += dAZMorphVertex2.delta.z * num4;
							}
						}
					}
				}
				dAZMorph.appliedValue = num2;
				dAZMorph.active = flag3;
			}
		}
	}

	public void PrepMorphsThreadedFast()
	{
		ClearDirtyMorphs();
	}

	public bool ApplyMorphsThreadedFast(Vector3[] verts, Vector3[] visibleNonPoseVerts, DAZBones bones)
	{
		bool flag = true;
		int num = 5;
		bool result = false;
		int num2 = verts.Length;
		while (flag)
		{
			num--;
			if (num == 0)
			{
				break;
			}
			flag = false;
			for (int i = 0; i < _morphs.Count; i++)
			{
				DAZMorph dAZMorph = _morphs[i];
				if (dAZMorph.disable)
				{
					continue;
				}
				float num3 = dAZMorph.morphValue;
				if (float.IsNaN(num3))
				{
					Debug.LogError("Detected NaN value for morph " + dAZMorph.displayName);
					continue;
				}
				float appliedValue = dAZMorph.appliedValue;
				bool flag2 = appliedValue != num3;
				bool flag3 = num3 != 0f;
				if (dAZMorph.hasMorphValueFormulas && (flag2 || flag3))
				{
					DAZMorphFormula[] formulas = dAZMorph.formulas;
					foreach (DAZMorphFormula dAZMorphFormula in formulas)
					{
						if (dAZMorphFormula.targetType != 0)
						{
							continue;
						}
						DAZMorph morph = GetMorph(dAZMorphFormula.target);
						if (morph == null)
						{
							continue;
						}
						if (dAZMorph.wasZeroedKeepChildValues)
						{
							if (morph.SetDriven(false, string.Empty))
							{
								dirtyMorphs.Add(morph);
							}
						}
						else if (morph.SetValueThreadSafe(dAZMorphFormula.multiplier * num3))
						{
							morph.SetDriven(flag3, dAZMorph.displayName);
							dirtyMorphs.Add(morph);
						}
					}
				}
				dAZMorph.wasZeroedKeepChildValues = false;
				if (enableMCMMorphs && dAZMorph.hasMCMFormulas)
				{
					DAZMorphFormula[] formulas2 = dAZMorph.formulas;
					foreach (DAZMorphFormula dAZMorphFormula2 in formulas2)
					{
						if (dAZMorphFormula2.targetType == DAZMorphFormulaTargetType.MCM)
						{
							DAZMorph morph2 = GetMorph(dAZMorphFormula2.target);
							if (morph2 != null)
							{
								num3 = morph2.morphValue * dAZMorphFormula2.multiplier;
								if (dAZMorph.SetValueThreadSafe(num3))
								{
									dAZMorph.SetDriven(true, morph2.displayName);
									dirtyMorphs.Add(dAZMorph);
								}
							}
							else
							{
								num3 = 0f;
								if (dAZMorph.SetValueThreadSafe(num3))
								{
									dAZMorph.SetDriven(true, "Missing source morph");
									dirtyMorphs.Add(dAZMorph);
								}
							}
						}
						else if (dAZMorphFormula2.targetType == DAZMorphFormulaTargetType.MCMMult)
						{
							DAZMorph morph3 = GetMorph(dAZMorphFormula2.target);
							if (morph3 != null)
							{
								num3 = dAZMorph.morphValue * morph3.morphValue;
							}
						}
					}
					flag2 = appliedValue != num3;
					flag3 = num3 != 0f;
				}
				if (!flag2)
				{
					continue;
				}
				flag = true;
				if (dAZMorph.isRuntime && !dAZMorph.deltasLoaded)
				{
					dAZMorph.LoadDeltas();
				}
				ApplyBoneMorphs(bones, dAZMorph);
				if (dAZMorph.deltas.Length > 0)
				{
					threadedVerticesChanged = true;
					float num4 = num3 - appliedValue;
					if (dAZMorph.visible && !dAZMorph.isPoseControl)
					{
						result = true;
						visibleNonPoseThreadedVerticesChanged = true;
						DAZMorphVertex[] deltas = dAZMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex in deltas)
						{
							if (dAZMorphVertex.vertex < num2)
							{
								verts[dAZMorphVertex.vertex].x += dAZMorphVertex.delta.x * num4;
								verts[dAZMorphVertex.vertex].y += dAZMorphVertex.delta.y * num4;
								verts[dAZMorphVertex.vertex].z += dAZMorphVertex.delta.z * num4;
								visibleNonPoseVerts[dAZMorphVertex.vertex].x += dAZMorphVertex.delta.x * num4;
								visibleNonPoseVerts[dAZMorphVertex.vertex].y += dAZMorphVertex.delta.y * num4;
								visibleNonPoseVerts[dAZMorphVertex.vertex].z += dAZMorphVertex.delta.z * num4;
							}
						}
					}
					else
					{
						DAZMorphVertex[] deltas2 = dAZMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex2 in deltas2)
						{
							if (dAZMorphVertex2.vertex < num2)
							{
								verts[dAZMorphVertex2.vertex].x += dAZMorphVertex2.delta.x * num4;
								verts[dAZMorphVertex2.vertex].y += dAZMorphVertex2.delta.y * num4;
								verts[dAZMorphVertex2.vertex].z += dAZMorphVertex2.delta.z * num4;
							}
						}
					}
				}
				dAZMorph.appliedValue = num3;
				dAZMorph.active = flag3;
			}
		}
		return result;
	}

	public void ApplyMorphsThreadedFastFinish()
	{
		CleanDirtyMorphs();
	}

	public void ApplyMorphsThreadedFinish()
	{
		if (triggerThreadResetMorphs)
		{
			ApplyMorphsInternal();
			if (connectedMesh != null)
			{
				connectedMesh.ResetMorphedVertices();
				_threadedMorphedUVVertices = (Vector3[])connectedMesh.UVVertices.Clone();
				_threadedVisibleMorphedUVVertices = (Vector3[])connectedMesh.UVVertices.Clone();
			}
			else
			{
				Debug.LogWarning("ResetMorphs called when connected mesh was not set. Vertices were not reset.");
			}
			if (_morphs != null)
			{
				for (int i = 0; i < _morphs.Count; i++)
				{
					_morphs[i].appliedValue = 0f;
					_morphs[i].active = false;
				}
			}
			ZeroAllBoneMorphs();
			triggerThreadResetMorphs = false;
			visibleNonPoseVerticesChanged = true;
			return;
		}
		CleanDirtyMorphs();
		if (bonesDirty)
		{
			if (morphBones != null)
			{
				morphBones.SetMorphedTransform();
			}
			if (morphBones2 != null)
			{
				morphBones2.SetMorphedTransform();
			}
			bonesDirty = false;
		}
		foreach (DAZBone key in boneRotationsDirty.Keys)
		{
			key.SyncMorphBoneRotations();
		}
		boneRotationsDirty.Clear();
		visibleNonPoseVerticesChanged = visibleNonPoseThreadedVerticesChanged;
		if (!threadedVerticesChanged || !(connectedMesh != null))
		{
			return;
		}
		Vector3[] rawMorphedUVVertices = connectedMesh.rawMorphedUVVertices;
		Vector3[] visibleMorphedUVVertices = connectedMesh.visibleMorphedUVVertices;
		if (checkAllThreadedVertices)
		{
			int numBaseVertices = connectedMesh.numBaseVertices;
			for (int j = 0; j < numBaseVertices; j++)
			{
				rawMorphedUVVertices[j] = _threadedMorphedUVVertices[j];
				visibleMorphedUVVertices[j] = _threadedVisibleMorphedUVVertices[j];
			}
		}
		else
		{
			for (int k = 0; k < numThreadedChangedVertices; k++)
			{
				int num = threadedChangedVertices[k];
				rawMorphedUVVertices[num] = _threadedMorphedUVVertices[num];
				visibleMorphedUVVertices[num] = _threadedVisibleMorphedUVVertices[num];
			}
		}
		connectedMesh.ApplyMorphVertices(visibleNonPoseVerticesChanged);
	}

	protected void ApplyBoneMorphs(DAZBones bones, DAZMorph morph, bool zero = false)
	{
		if (!(bones != null))
		{
			return;
		}
		DAZMorphFormula[] formulas = morph.formulas;
		foreach (DAZMorphFormula dAZMorphFormula in formulas)
		{
			switch (dAZMorphFormula.targetType)
			{
			case DAZMorphFormulaTargetType.GeneralScale:
				if (zero)
				{
					bones.SetGeneralScale(geometryId + ":" + morph.morphName, 0f);
				}
				else
				{
					bones.SetGeneralScale(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
				}
				break;
			case DAZMorphFormulaTargetType.BoneCenterX:
			{
				bonesDirty = true;
				DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
				if (dAZBone != null)
				{
					if (zero)
					{
						dAZBone.SetBoneXOffset(geometryId + ":" + morph.morphName, 0f);
					}
					else
					{
						dAZBone.SetBoneXOffset(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
					}
				}
				break;
			}
			case DAZMorphFormulaTargetType.BoneCenterY:
			{
				bonesDirty = true;
				DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
				if (dAZBone != null)
				{
					if (zero)
					{
						dAZBone.SetBoneYOffset(geometryId + ":" + morph.morphName, 0f);
					}
					else
					{
						dAZBone.SetBoneYOffset(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
					}
				}
				break;
			}
			case DAZMorphFormulaTargetType.BoneCenterZ:
			{
				bonesDirty = true;
				DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
				if (dAZBone != null)
				{
					if (zero)
					{
						dAZBone.SetBoneZOffset(geometryId + ":" + morph.morphName, 0f);
					}
					else
					{
						dAZBone.SetBoneZOffset(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
					}
				}
				break;
			}
			case DAZMorphFormulaTargetType.OrientationX:
			{
				bonesDirty = true;
				DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
				if (dAZBone != null)
				{
					if (zero)
					{
						dAZBone.SetBoneOrientationXOffset(geometryId + ":" + morph.morphName, 0f);
					}
					else
					{
						dAZBone.SetBoneOrientationXOffset(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
					}
				}
				break;
			}
			case DAZMorphFormulaTargetType.OrientationY:
			{
				bonesDirty = true;
				DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
				if (dAZBone != null)
				{
					if (zero)
					{
						dAZBone.SetBoneOrientationYOffset(geometryId + ":" + morph.morphName, 0f);
					}
					else
					{
						dAZBone.SetBoneOrientationYOffset(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
					}
				}
				break;
			}
			case DAZMorphFormulaTargetType.OrientationZ:
			{
				bonesDirty = true;
				DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
				if (dAZBone != null)
				{
					if (zero)
					{
						dAZBone.SetBoneOrientationZOffset(geometryId + ":" + morph.morphName, 0f);
					}
					else
					{
						dAZBone.SetBoneOrientationZOffset(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
					}
				}
				break;
			}
			case DAZMorphFormulaTargetType.RotationX:
			{
				DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
				if (dAZBone != null)
				{
					if (!boneRotationsDirty.ContainsKey(dAZBone))
					{
						boneRotationsDirty.Add(dAZBone, true);
					}
					if (zero)
					{
						dAZBone.SetBoneXRotation(geometryId + ":" + morph.morphName, 0f);
					}
					else
					{
						dAZBone.SetBoneXRotation(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
					}
				}
				break;
			}
			case DAZMorphFormulaTargetType.RotationY:
			{
				DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
				if (dAZBone != null)
				{
					if (!boneRotationsDirty.ContainsKey(dAZBone))
					{
						boneRotationsDirty.Add(dAZBone, true);
					}
					if (zero)
					{
						dAZBone.SetBoneYRotation(geometryId + ":" + morph.morphName, 0f);
					}
					else
					{
						dAZBone.SetBoneYRotation(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
					}
				}
				break;
			}
			case DAZMorphFormulaTargetType.RotationZ:
			{
				DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
				if (dAZBone != null)
				{
					if (!boneRotationsDirty.ContainsKey(dAZBone))
					{
						boneRotationsDirty.Add(dAZBone, true);
					}
					if (zero)
					{
						dAZBone.SetBoneZRotation(geometryId + ":" + morph.morphName, 0f);
					}
					else
					{
						dAZBone.SetBoneZRotation(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
					}
				}
				break;
			}
			}
		}
	}

	protected void ApplyAllBoneMorphs()
	{
		for (int i = 0; i < _morphs.Count; i++)
		{
			ApplyBoneMorphs(morphBones, _morphs[i]);
			ApplyBoneMorphs(morphBones2, _morphs[i]);
		}
		if (morphBones != null)
		{
			morphBones.SetMorphedTransform(true);
		}
		if (morphBones2 != null)
		{
			morphBones2.SetMorphedTransform(true);
		}
		bonesDirty = false;
	}

	protected void ZeroAllBoneMorphs()
	{
		if (_morphs != null)
		{
			for (int i = 0; i < _morphs.Count; i++)
			{
				ApplyBoneMorphs(morphBones, _morphs[i], true);
				ApplyBoneMorphs(morphBones2, _morphs[i], true);
			}
		}
		if (morphBones != null)
		{
			DAZBone[] dazBones = morphBones.dazBones;
			foreach (DAZBone dAZBone in dazBones)
			{
				dAZBone.ForceClearMorphs();
			}
			morphBones.SetMorphedTransform(true);
		}
		if (morphBones2 != null)
		{
			DAZBone[] dazBones2 = morphBones2.dazBones;
			foreach (DAZBone dAZBone2 in dazBones2)
			{
				dAZBone2.ForceClearMorphs();
			}
			morphBones2.SetMorphedTransform(true);
		}
		bonesDirty = false;
	}

	protected void ApplyMorphsInternal(bool force = false)
	{
		if (_morphs == null)
		{
			_morphs = new List<DAZMorph>();
		}
		visibleNonPoseVerticesChanged = false;
		bool flag = false;
		bool flag2 = true;
		int num = 5;
		bonesDirty = false;
		Vector3[] array = null;
		Vector3[] array2 = null;
		Vector3[] array3 = null;
		if (connectedMesh != null)
		{
			array = connectedMesh.morphedBaseVertices;
			array2 = connectedMesh.rawMorphedUVVertices;
			array3 = connectedMesh.visibleMorphedUVVertices;
		}
		while (flag2)
		{
			num--;
			if (num == 0)
			{
				break;
			}
			flag2 = false;
			for (int i = 0; i < _morphs.Count; i++)
			{
				DAZMorph dAZMorph = _morphs[i];
				if (dAZMorph.disable)
				{
					continue;
				}
				float num2 = dAZMorph.morphValue;
				float appliedValue = dAZMorph.appliedValue;
				bool flag3 = appliedValue != num2;
				bool flag4 = num2 != 0f;
				if (dAZMorph.hasMorphValueFormulas && (flag3 || flag4))
				{
					DAZMorphFormula[] formulas = dAZMorph.formulas;
					foreach (DAZMorphFormula dAZMorphFormula in formulas)
					{
						if (dAZMorphFormula.targetType != 0)
						{
							continue;
						}
						DAZMorph morph = GetMorph(dAZMorphFormula.target);
						if (morph != null)
						{
							if (dAZMorph.wasZeroedKeepChildValues)
							{
								morph.SetDriven(false, string.Empty, true);
								continue;
							}
							morph.morphValue = dAZMorphFormula.multiplier * num2;
							morph.SetDriven(flag4, dAZMorph.displayName, true);
						}
					}
				}
				dAZMorph.wasZeroedKeepChildValues = false;
				if (enableMCMMorphs && dAZMorph.hasMCMFormulas)
				{
					DAZMorphFormula[] formulas2 = dAZMorph.formulas;
					foreach (DAZMorphFormula dAZMorphFormula2 in formulas2)
					{
						if (dAZMorphFormula2.targetType == DAZMorphFormulaTargetType.MCM)
						{
							DAZMorph morph2 = GetMorph(dAZMorphFormula2.target);
							if (morph2 != null)
							{
								num2 = (dAZMorph.morphValue = morph2.morphValue * dAZMorphFormula2.multiplier);
								dAZMorph.SetDriven(true, morph2.displayName);
							}
							else
							{
								num2 = 0f;
								dAZMorph.morphValue = 0f;
							}
						}
						else if (dAZMorphFormula2.targetType == DAZMorphFormulaTargetType.MCMMult)
						{
							DAZMorph morph3 = GetMorph(dAZMorphFormula2.target);
							if (morph3 != null)
							{
								num2 = dAZMorph.morphValue * morph3.morphValue;
							}
						}
					}
					flag3 = appliedValue != num2;
					flag4 = num2 != 0f;
				}
				if (!flag3)
				{
					continue;
				}
				flag2 = true;
				if (dAZMorph.isRuntime && !dAZMorph.deltasLoaded)
				{
					dAZMorph.LoadDeltas();
				}
				ApplyBoneMorphs(morphBones, dAZMorph);
				ApplyBoneMorphs(morphBones2, dAZMorph);
				if (dAZMorph.deltas.Length > 0 && connectedMesh != null && array2 != null)
				{
					flag = true;
					if (dAZMorph.visible && !dAZMorph.isPoseControl)
					{
						visibleNonPoseVerticesChanged = true;
						float num4 = num2 - appliedValue;
						DAZMorphVertex[] deltas = dAZMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex in deltas)
						{
							if (dAZMorphVertex.vertex < array2.Length)
							{
								array2[dAZMorphVertex.vertex].x += dAZMorphVertex.delta.x * num4;
								array[dAZMorphVertex.vertex].x = array2[dAZMorphVertex.vertex].x;
								array3[dAZMorphVertex.vertex].x = array2[dAZMorphVertex.vertex].x;
								array2[dAZMorphVertex.vertex].y += dAZMorphVertex.delta.y * num4;
								array[dAZMorphVertex.vertex].y = array2[dAZMorphVertex.vertex].y;
								array3[dAZMorphVertex.vertex].y = array2[dAZMorphVertex.vertex].y;
								array2[dAZMorphVertex.vertex].z += dAZMorphVertex.delta.z * num4;
								array[dAZMorphVertex.vertex].z = array2[dAZMorphVertex.vertex].z;
								array3[dAZMorphVertex.vertex].z = array2[dAZMorphVertex.vertex].z;
								if (dAZMorph.triggerNormalRecalc)
								{
									connectedMesh.morphedBaseDirtyVertices[dAZMorphVertex.vertex] = true;
									connectedMesh.morphedNormalsDirty = true;
								}
								if (dAZMorph.triggerTangentRecalc)
								{
									connectedMesh.morphedUVDirtyVertices[dAZMorphVertex.vertex] = true;
									connectedMesh.morphedTangentsDirty = true;
								}
							}
						}
					}
					else
					{
						float num5 = num2 - appliedValue;
						DAZMorphVertex[] deltas2 = dAZMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex2 in deltas2)
						{
							if (dAZMorphVertex2.vertex < array2.Length)
							{
								array2[dAZMorphVertex2.vertex].x += dAZMorphVertex2.delta.x * num5;
								array[dAZMorphVertex2.vertex].x = array2[dAZMorphVertex2.vertex].x;
								array2[dAZMorphVertex2.vertex].y += dAZMorphVertex2.delta.y * num5;
								array[dAZMorphVertex2.vertex].y = array2[dAZMorphVertex2.vertex].y;
								array2[dAZMorphVertex2.vertex].z += dAZMorphVertex2.delta.z * num5;
								array[dAZMorphVertex2.vertex].z = array2[dAZMorphVertex2.vertex].z;
								if (dAZMorph.triggerNormalRecalc)
								{
									connectedMesh.morphedBaseDirtyVertices[dAZMorphVertex2.vertex] = true;
									connectedMesh.morphedNormalsDirty = true;
								}
								if (dAZMorph.triggerTangentRecalc)
								{
									connectedMesh.morphedUVDirtyVertices[dAZMorphVertex2.vertex] = true;
									connectedMesh.morphedTangentsDirty = true;
								}
							}
						}
					}
				}
				dAZMorph.appliedValue = num2;
				dAZMorph.active = flag4;
			}
		}
		if (bonesDirty)
		{
			if (morphBones != null)
			{
				morphBones.SetMorphedTransform(true);
			}
			if (morphBones2 != null)
			{
				morphBones2.SetMorphedTransform(true);
			}
			bonesDirty = false;
		}
		if ((flag || force) && connectedMesh != null)
		{
			connectedMesh.ApplyMorphVertices(visibleNonPoseVerticesChanged);
		}
	}

	public void ApplyMorphs(bool force = false)
	{
		if ((bool)connectedMesh && connectedMesh.wasInit)
		{
			connectedMesh.StartMorph();
			if (!Application.isPlaying || !useThreadedMorphing)
			{
				ApplyMorphsInternal(force);
			}
		}
	}

	public void ApplyMorphsImmediate()
	{
		if (Application.isPlaying && useThreadedMorphing && _threadsRunning)
		{
			while (applyMorphsTask.working)
			{
				Thread.Sleep(0);
			}
			ApplyMorphsThreadedFinish();
			ApplyMorphsThreadedStart();
			applyMorphsTask.working = true;
			applyMorphsTask.resetEvent.Set();
			while (applyMorphsTask.working)
			{
				Thread.Sleep(0);
			}
			ApplyMorphsThreadedFinish();
			ApplyMorphsThreadedStart();
			applyMorphsTask.working = true;
			applyMorphsTask.resetEvent.Set();
			visibleNonPoseVerticesChanged = true;
			if (connectedMesh != null)
			{
				connectedMesh.ApplyMorphVertices(visibleNonPoseVerticesChanged);
			}
		}
		else
		{
			ApplyMorphsInternal();
		}
	}

	private void Update()
	{
		if (!connectedMesh || !connectedMesh.wasInit || !updateEnabled)
		{
			return;
		}
		connectedMesh.StartMorph();
		if (Application.isPlaying && useThreadedMorphing)
		{
			StartThreads();
			totalFrameCount++;
			if (!applyMorphsTask.working)
			{
				ApplyMorphsThreadedFinish();
				ApplyMorphsThreadedStart();
				applyMorphsTask.working = true;
				applyMorphsTask.resetEvent.Set();
			}
			else if (OVRManager.isHmdPresent)
			{
				missedFrameCount++;
				Debug.LogWarning("ApplyMorphsTask did not complete in 1 frame. Missed " + missedFrameCount + " out of total " + totalFrameCount);
				DebugHUD.Msg("ApplyMorphsTask miss " + missedFrameCount + " out of total " + totalFrameCount);
				DebugHUD.Alert2();
			}
		}
		else
		{
			ApplyMorphsInternal();
		}
	}

	public void ReInit()
	{
		RebuildAllLookups();
		ResetMorphs();
		ApplyMorphs(true);
	}

	public void Init()
	{
		if (!wasInit)
		{
			wasInit = true;
			if (boneRotationsDirty == null)
			{
				boneRotationsDirty = new Dictionary<DAZBone, bool>();
			}
			if (morphBones != null)
			{
				morphBones.Init();
			}
			if (morphBones2 != null)
			{
				morphBones2.Init();
			}
			RebuildAllLookups();
			if (Application.isPlaying)
			{
				RefreshRuntimeMorphs();
				RefreshPackageMorphs();
				InitMorphs();
			}
		}
	}

	public bool RefreshRuntimeMorphs()
	{
		bool result = false;
		if (wasInit && autoImportFolder != null && autoImportFolder != string.Empty && RuntimeImportFromDir(autoImportFolder, false, false, true))
		{
			DAZMorphSubBank[] componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
			DAZMorphSubBank[] array = componentsInChildren;
			foreach (DAZMorphSubBank dAZMorphSubBank in array)
			{
				dAZMorphSubBank.CompleteRuntimeMorphAdd();
			}
			RebuildAllLookups();
			result = true;
		}
		return result;
	}

	public bool RefreshPackageMorphs()
	{
		if (wasInit && autoImportFolder != null && autoImportFolder != string.Empty)
		{
			List<VarDirectoryEntry> list = FileManager.FindVarDirectories(autoImportFolder);
			HashSet<string> hashSet = new HashSet<string>();
			HashSet<string> hashSet2 = new HashSet<string>();
			foreach (VarDirectoryEntry item in list)
			{
				hashSet.Add(item.Package.Uid);
				if (item.Package.Group.GetCustomOption("preloadMorphs"))
				{
					hashSet2.Add(item.Package.Uid);
				}
			}
			if (currentLoadedMorphPackageUids != null && currentPreloadMorphPackageUids != null && currentLoadedMorphPackageUids.SetEquals(hashSet) && currentPreloadMorphPackageUids.SetEquals(hashSet2))
			{
				return false;
			}
			currentLoadedMorphPackageUids = hashSet;
			currentPreloadMorphPackageUids = hashSet2;
			Dictionary<string, float> dictionary = new Dictionary<string, float>();
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			Dictionary<string, float> dictionary3 = new Dictionary<string, float>();
			foreach (DAZMorph morph in _morphs)
			{
				if (morph.isInPackage && morph.morphValue != morph.startValue)
				{
					dictionary.Add(morph.uid, morph.morphValue);
					dictionary2.Add(morph.uid, morph.resolvedDisplayName);
					if (!dictionary3.ContainsKey(morph.resolvedDisplayName))
					{
						dictionary3.Add(morph.resolvedDisplayName, morph.morphValue);
					}
				}
			}
			DAZMorphSubBank[] componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
			bool flag = false;
			DAZMorphSubBank[] array = componentsInChildren;
			foreach (DAZMorphSubBank dAZMorphSubBank in array)
			{
				if (dAZMorphSubBank.ClearPackageMorphs())
				{
					flag = true;
				}
			}
			if (flag)
			{
				RebuildAllLookups();
			}
			bool flag2 = false;
			foreach (VarDirectoryEntry item2 in list)
			{
				if (item2.Package.Group.GetCustomOption("preloadMorphs"))
				{
					if (RuntimeImportFromDir(item2))
					{
						flag2 = true;
					}
				}
				else if (RuntimeImportFromDir(item2, false, true))
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
				DAZMorphSubBank[] array2 = componentsInChildren;
				foreach (DAZMorphSubBank dAZMorphSubBank2 in array2)
				{
					dAZMorphSubBank2.CompletePackageMorphAdd();
				}
				RebuildAllLookups();
				foreach (string key in dictionary.Keys)
				{
					DAZMorph dAZMorph = GetMorphByUid(key);
					string value;
					if (dAZMorph == null && dictionary2.TryGetValue(key, out value))
					{
						dAZMorph = GetMorphByDisplayName(value);
					}
					float value2;
					if (dAZMorph != null && dictionary.TryGetValue(key, out value2))
					{
						dAZMorph.morphValue = value2;
					}
				}
				CleanDemandActivatedMorphs();
			}
			if (flag || flag2)
			{
				return true;
			}
		}
		return false;
	}

	protected void EnableInit()
	{
		if (connectedMesh != null)
		{
			connectedMesh.Init();
			if (Application.isPlaying)
			{
				if (_threadedMorphedUVVertices == null)
				{
					_threadedMorphedUVVertices = (Vector3[])connectedMesh.UVVertices.Clone();
				}
				if (_threadedVisibleMorphedUVVertices == null)
				{
					_threadedVisibleMorphedUVVertices = (Vector3[])connectedMesh.UVVertices.Clone();
				}
			}
		}
		ResetMorphs();
		ApplyMorphs(true);
	}

	private void OnDisable()
	{
		if (updateEnabled)
		{
			if (!Application.isPlaying)
			{
				wasInit = false;
			}
			ZeroAllBoneMorphs();
		}
	}

	private void OnEnable()
	{
		if (updateEnabled)
		{
			if (!Application.isPlaying)
			{
				Init();
			}
			EnableInit();
		}
	}

	private void Awake()
	{
		if (Application.isPlaying)
		{
			Init();
		}
	}

	protected void OnDestroy()
	{
		if (Application.isPlaying)
		{
			StopThreads();
		}
	}

	protected void OnApplicationQuit()
	{
		if (Application.isPlaying)
		{
			StopThreads();
		}
	}
}
