using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class GenerateDAZMorphsControlJSONStorable : JSONStorable
{
	public GenerateDAZMorphsControlUI morphsControlUI;

	public DAZMorphBank morphBank;

	protected string[] customParamNames = new string[1] { "morphs" };

	private bool wasInit;

	public override string[] GetCustomParamNames()
	{
		return customParamNames;
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (includeAppearance || forceStore)
		{
			JSONArray jSONArray = (JSONArray)(jSON["morphs"] = new JSONArray());
			if (morphsControlUI != null)
			{
				List<DAZMorph> morphs = morphsControlUI.GetMorphs();
				if (morphs != null)
				{
					foreach (DAZMorph item in morphs)
					{
						JSONClass jSONClass = new JSONClass();
						if (item.StoreJSON(jSONClass, forceStore))
						{
							jSONArray.Add(jSONClass);
							needsStore = true;
						}
					}
				}
				else
				{
					Debug.LogWarning("morphDisplayNames not set for " + base.name);
				}
			}
			else
			{
				Debug.LogWarning("morphsControl UI not set for " + base.name);
			}
		}
		return jSON;
	}

	public void ResetMorphs()
	{
		if (morphsControlUI != null)
		{
			List<DAZMorph> morphs = morphsControlUI.GetMorphs();
			if (morphs != null)
			{
				foreach (DAZMorph item in morphs)
				{
					item.Reset();
				}
			}
		}
		if (morphBank != null)
		{
			morphBank.ResetMorphs();
		}
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (base.appearanceLocked || !restoreAppearance)
		{
			return;
		}
		if (!base.mergeRestore)
		{
			ResetMorphs();
		}
		if (IsCustomAppearanceParamLocked("morphs"))
		{
			return;
		}
		if (jc["morphs"] != null && morphsControlUI != null)
		{
			foreach (JSONClass item in jc["morphs"].AsArray)
			{
				string text = item["name"];
				if (text != null)
				{
					DAZMorph morphByDisplayName = morphsControlUI.GetMorphByDisplayName(text);
					if (morphByDisplayName != null)
					{
						morphByDisplayName.RestoreFromJSON(item);
					}
					else
					{
						SuperController.LogError("Could not find morph " + text + " referenced in save file");
					}
				}
			}
		}
		if (morphBank != null)
		{
			morphBank.ApplyMorphsImmediate();
		}
	}

	public void Init(bool force = false)
	{
		if (wasInit && !force)
		{
			return;
		}
		wasInit = true;
		List<DAZMorph> morphs = morphsControlUI.GetMorphs();
		if (morphs == null)
		{
			return;
		}
		foreach (DAZMorph item in morphs)
		{
			RegisterFloat(item.jsonFloat);
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
		}
	}
}
