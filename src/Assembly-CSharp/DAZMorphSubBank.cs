using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DAZMorphSubBank : MonoBehaviour
{
	public bool alwaysSetDirtyOnEditorChange = true;

	[SerializeField]
	protected List<DAZMorph> _morphs;

	protected List<DAZMorph> _runtimeMorphs;

	protected List<DAZMorph> _transientMorphs;

	protected List<DAZMorph> _packageMorphs;

	protected List<DAZMorph> _combinedMorphs;

	public bool simpleView;

	public bool useOverrideRegionName;

	public string overrideRegionName;

	protected Dictionary<string, DAZMorph> _morphsByName;

	protected Dictionary<string, DAZMorph> _morphsByUid;

	protected bool _addedRuntime;

	protected bool _addedTransients;

	protected bool _addedPackage;

	public List<DAZMorph> morphs => _morphs;

	public List<DAZMorph> runtimeMorphs => _runtimeMorphs;

	public List<DAZMorph> transientMorphs => _transientMorphs;

	public List<DAZMorph> packageMorphs => _packageMorphs;

	public List<DAZMorph> combinedMorphs
	{
		get
		{
			if (_combinedMorphs == null)
			{
				RebuildMorphsByNameAndUid();
			}
			return _combinedMorphs;
		}
	}

	public int numMorphs => _morphs.Count;

	public bool ClearRuntimeMorphs()
	{
		bool flag = false;
		if (_runtimeMorphs != null && _runtimeMorphs.Count > 0)
		{
			flag = true;
			_runtimeMorphs.Clear();
		}
		if (flag)
		{
			RebuildMorphsByNameAndUid();
		}
		return flag;
	}

	public bool ClearTransientMorphs()
	{
		bool flag = false;
		if (_transientMorphs != null && _transientMorphs.Count > 0)
		{
			flag = true;
			_transientMorphs.Clear();
		}
		if (flag)
		{
			RebuildMorphsByNameAndUid();
		}
		return flag;
	}

	public bool ClearPackageMorphs()
	{
		bool flag = false;
		if (_packageMorphs != null && _packageMorphs.Count > 0)
		{
			flag = true;
			_packageMorphs.Clear();
		}
		if (flag)
		{
			RebuildMorphsByNameAndUid();
		}
		return flag;
	}

	public void CompleteRuntimeMorphAdd()
	{
		if (_addedRuntime)
		{
			RebuildMorphsByNameAndUid();
			_addedRuntime = false;
		}
	}

	public void CompleteTransientMorphAdd()
	{
		if (_addedTransients)
		{
			RebuildMorphsByNameAndUid();
			_addedTransients = false;
		}
	}

	public void CompletePackageMorphAdd()
	{
		if (_addedPackage)
		{
			RebuildMorphsByNameAndUid();
			_addedPackage = false;
		}
	}

	protected void RebuildMorphsByNameAndUid()
	{
		if (_morphs == null)
		{
			_morphs = new List<DAZMorph>();
		}
		if (_runtimeMorphs == null)
		{
			_runtimeMorphs = new List<DAZMorph>();
		}
		if (_transientMorphs == null)
		{
			_transientMorphs = new List<DAZMorph>();
		}
		if (_packageMorphs == null)
		{
			_packageMorphs = new List<DAZMorph>();
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
		foreach (DAZMorph morph in _morphs)
		{
			morph.morphSubBank = this;
			_morphsByName.Add(morph.morphName, morph);
			if (!morph.disable)
			{
				if (!_morphsByUid.ContainsKey(morph.uid))
				{
					_morphsByUid.Add(morph.uid, morph);
				}
				else
				{
					Debug.LogError("Found duplicate morph uid " + morph.uid);
				}
			}
		}
		foreach (DAZMorph packageMorph in _packageMorphs)
		{
			packageMorph.morphSubBank = this;
			if (!_morphsByName.ContainsKey(packageMorph.morphName))
			{
				_morphsByName.Add(packageMorph.morphName, packageMorph);
			}
			if (!_morphsByUid.ContainsKey(packageMorph.uid))
			{
				_morphsByUid.Add(packageMorph.uid, packageMorph);
			}
			else
			{
				Debug.LogError("Found duplicate morph uid " + packageMorph.uid);
			}
		}
		foreach (DAZMorph runtimeMorph in _runtimeMorphs)
		{
			runtimeMorph.morphSubBank = this;
			if (!_morphsByName.ContainsKey(runtimeMorph.morphName))
			{
				_morphsByName.Add(runtimeMorph.morphName, runtimeMorph);
			}
			if (!_morphsByUid.ContainsKey(runtimeMorph.uid))
			{
				_morphsByUid.Add(runtimeMorph.uid, runtimeMorph);
			}
			else
			{
				Debug.LogError("Found duplicate morph uid " + runtimeMorph.uid);
			}
		}
		foreach (DAZMorph transientMorph in _transientMorphs)
		{
			transientMorph.morphSubBank = this;
			if (!_morphsByName.ContainsKey(transientMorph.morphName))
			{
				_morphsByName.Add(transientMorph.morphName, transientMorph);
			}
			if (!_morphsByUid.ContainsKey(transientMorph.uid))
			{
				_morphsByUid.Add(transientMorph.uid, transientMorph);
			}
			else
			{
				Debug.LogError("Found duplicate morph uid " + transientMorph.uid);
			}
		}
		_combinedMorphs = _morphsByUid.Values.ToList();
	}

	public DAZMorph GetMorph(string morphName)
	{
		if (_morphsByName == null)
		{
			RebuildMorphsByNameAndUid();
		}
		DAZMorph value;
		if (_morphsByName.TryGetValue(morphName, out value))
		{
			return value;
		}
		return null;
	}

	public DAZMorph GetMorphByUid(string morphUid)
	{
		if (_morphsByUid == null)
		{
			RebuildMorphsByNameAndUid();
		}
		DAZMorph value;
		if (_morphsByUid.TryGetValue(morphUid, out value))
		{
			return value;
		}
		return null;
	}

	public void AddMorph(DAZMorph dm)
	{
		if (_morphs == null)
		{
			_morphs = new List<DAZMorph>();
		}
		if (_morphsByName == null || _morphsByUid == null)
		{
			RebuildMorphsByNameAndUid();
		}
		dm.morphSubBank = this;
		DAZMorph value;
		if (dm.isTransient)
		{
			_addedTransients = true;
			_transientMorphs.Add(dm);
		}
		else if (dm.isInPackage)
		{
			_addedPackage = true;
			_packageMorphs.Add(dm);
		}
		else if (dm.isRuntime)
		{
			_addedRuntime = true;
			_runtimeMorphs.Add(dm);
		}
		else if (_morphsByName.TryGetValue(dm.morphName, out value))
		{
			dm.CopyParameters(value);
			_morphsByName.Remove(dm.morphName);
			_morphsByName.Add(dm.morphName, dm);
			int num = _morphs.IndexOf(value);
			if (num != -1)
			{
				_morphs[num] = dm;
			}
			else
			{
				Debug.LogError("Should have found DAZMorph " + dm.morphName + " in morphs list during add");
			}
			num = _combinedMorphs.IndexOf(value);
			if (num != -1)
			{
				_combinedMorphs[num] = dm;
			}
			else
			{
				Debug.LogError("Should have found DAZMorph " + dm.morphName + " in morphs list during add");
			}
		}
		else
		{
			_morphsByName.Add(dm.morphName, dm);
			_morphs.Add(dm);
			_combinedMorphs.Add(dm);
		}
	}

	public void RemoveMorph(string morphName)
	{
		if (_morphsByName == null)
		{
			RebuildMorphsByNameAndUid();
		}
		DAZMorph value;
		if (_morphsByName.TryGetValue(morphName, out value))
		{
			if (value.isTransient)
			{
				Debug.LogError("RemoveMorph does not work with transient morphs. Use clear transient morphs instead.");
				return;
			}
			if (value.isRuntime)
			{
				Debug.LogError("RemoveMorph does not work with runtime morphs. Use clear runtime morphs instead.");
				return;
			}
			_morphsByName.Remove(morphName);
			_morphs.Remove(value);
		}
	}

	private void OnEnable()
	{
		RebuildMorphsByNameAndUid();
	}
}
