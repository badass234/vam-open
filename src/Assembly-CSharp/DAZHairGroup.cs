using System.Collections.Generic;
using System.Text.RegularExpressions;
using GPUTools.Hair.Scripts.Runtime.Physics;
using GPUTools.Skinner.Scripts.Providers;
using MVR.FileManagement;
using MeshVR;
using UnityEngine;

[ExecuteInEditMode]
public class DAZHairGroup : DAZDynamicItem
{
	public CapsuleCollider[] hairColliders;

	public DAZBones rootBonesForSkinning;

	protected DAZHairGroupControl[] hairGroupControls;

	public void SyncHairAdjustments()
	{
		if (characterSelector != null)
		{
			characterSelector.SyncHairAdjustments();
		}
	}

	public void RefreshHairItems()
	{
		if (characterSelector != null)
		{
			characterSelector.RefreshDynamicHair();
		}
	}

	public void RefreshHairItemThumbnail(string thumbPath)
	{
		if (characterSelector != null)
		{
			characterSelector.InvalidateDynamicHairItemThumbnail(thumbPath);
			characterSelector.RefreshDynamicHairThumbnails();
		}
	}

	public bool IsHairUIDAvailable(string uid)
	{
		if (characterSelector != null)
		{
			return characterSelector.IsHairUIDAvailable(uid);
		}
		return false;
	}

	protected override void SyncOtherTags()
	{
		otherTags = GetAllHairOtherTags();
		base.SyncOtherTags();
	}

	public HashSet<string> GetAllHairOtherTags()
	{
		if (characterSelector != null)
		{
			return characterSelector.GetHairOtherTags();
		}
		return null;
	}

	protected override void SetHidePath()
	{
		if (dynamicRuntimeLoadPath != null && dynamicRuntimeLoadPath != string.Empty)
		{
			FileEntry fileEntry = FileManager.GetFileEntry(dynamicRuntimeLoadPath);
			hidePath = fileEntry.hidePath;
		}
		else if (assetName != null && assetName != string.Empty)
		{
			if (gender == Gender.Female)
			{
				hidePath = "Custom/Hair/Female/Builtin/" + assetName + "/" + assetName + ".hide";
			}
			else if (gender == Gender.Male)
			{
				hidePath = "Custom/Hair/Male/Builtin/" + assetName + "/" + assetName + ".hide";
			}
			else
			{
				hidePath = "Custom/Hair/Neutral/Builtin/" + assetName + "/" + assetName + ".hide";
			}
		}
	}

	protected override void SetUserPrefsPath()
	{
		if (dynamicRuntimeLoadPath != null && dynamicRuntimeLoadPath != string.Empty)
		{
			string input = FileManager.RemovePackageFromPath(dynamicRuntimeLoadPath);
			userPrefsPath = Regex.Replace(input, "\\.vam$", ".prefs");
		}
		else if (assetName != null && assetName != string.Empty)
		{
			if (gender == Gender.Female)
			{
				userPrefsPath = "Custom/Hair/Female/Builtin/" + assetName + "/" + assetName + ".prefs";
			}
			else if (gender == Gender.Male)
			{
				userPrefsPath = "Custom/Hair/Male/Builtin/" + assetName + "/" + assetName + ".prefs";
			}
			else
			{
				userPrefsPath = "Custom/Hair/Neutral/Builtin/" + assetName + "/" + assetName + ".prefs";
			}
		}
	}

	public void SetLocked(bool b)
	{
		locked = b;
		if (hairGroupControls == null)
		{
			return;
		}
		DAZHairGroupControl[] array = hairGroupControls;
		foreach (DAZHairGroupControl dAZHairGroupControl in array)
		{
			if (dAZHairGroupControl.lockedJSON != null)
			{
				dAZHairGroupControl.lockedJSON.val = b;
			}
		}
	}

	protected override void InitInstance()
	{
		base.InitInstance();
		hairGroupControls = GetComponentsInChildren<DAZHairGroupControl>(true);
		if (hairGroupControls != null)
		{
			DAZHairGroupControl[] array = hairGroupControls;
			foreach (DAZHairGroupControl dAZHairGroupControl in array)
			{
				dAZHairGroupControl.hairItem = this;
			}
		}
		if (base.skin != null)
		{
			PreCalcMeshProviderHolder[] componentsInChildren = GetComponentsInChildren<PreCalcMeshProviderHolder>(true);
			PreCalcMeshProviderHolder[] array2 = componentsInChildren;
			foreach (PreCalcMeshProviderHolder preCalcMeshProviderHolder in array2)
			{
				preCalcMeshProviderHolder.provider = base.skin;
			}
		}
		DAZDynamic componentInChildren = GetComponentInChildren<DAZDynamic>(true);
		if (componentInChildren != null)
		{
			if (gender == Gender.Female)
			{
				componentInChildren.itemType = PresetManager.ItemType.HairFemale;
			}
			else if (gender == Gender.Male)
			{
				componentInChildren.itemType = PresetManager.ItemType.HairMale;
			}
			else
			{
				componentInChildren.itemType = PresetManager.ItemType.HairNeutral;
			}
			if (dynamicRuntimeLoadPath != null && dynamicRuntimeLoadPath != string.Empty)
			{
				// TEMP DIAGNOSTIC - remove once the Custom hair store-path defect is fixed.
				if (dynamicRuntimeLoadPath.Contains("Simone"))
				{
					UnityEngine.Debug.LogError(string.Concat("[DIAG InitInstance] path=[", dynamicRuntimeLoadPath, "] backslash=", dynamicRuntimeLoadPath.Contains("\\"), " folder=[", componentInChildren.storeFolderName, "] creator=[", componentInChildren.creatorName, "] name=[", componentInChildren.storeName, "]"), this);
				}
				componentInChildren.SetNamesFromPath(dynamicRuntimeLoadPath);
				componentInChildren.Load();
			}
		}
		PresetManagerControl componentInChildren2 = GetComponentInChildren<PresetManagerControl>(true);
		if (componentInChildren2 != null)
		{
			componentInChildren2.SyncPresetUI();
		}
	}

	protected override void Connect()
	{
		base.Connect();
		DAZHairMesh[] componentsInChildren = GetComponentsInChildren<DAZHairMesh>(true);
		DAZHairMesh[] array = componentsInChildren;
		foreach (DAZHairMesh dAZHairMesh in array)
		{
			dAZHairMesh.capsuleColliders = hairColliders;
			DAZSkinV2MeshSelection component = dAZHairMesh.gameObject.GetComponent<DAZSkinV2MeshSelection>();
			if (component != null && base.skin != null)
			{
				component.meshTransform = base.skin.transform;
				component.skin = base.skin;
			}
			dAZHairMesh.Reset();
		}
		PreCalcMeshProviderHolder[] componentsInChildren2 = GetComponentsInChildren<PreCalcMeshProviderHolder>(true);
		PreCalcMeshProviderHolder[] array2 = componentsInChildren2;
		foreach (PreCalcMeshProviderHolder preCalcMeshProviderHolder in array2)
		{
			preCalcMeshProviderHolder.provider = base.skin;
		}
		ResetPhysics();
		if (rootBonesForSkinning != null)
		{
			DAZSkinV2[] componentsInChildren3 = GetComponentsInChildren<DAZSkinV2>(true);
			DAZSkinV2[] array3 = componentsInChildren3;
			foreach (DAZSkinV2 dAZSkinV in array3)
			{
				dAZSkinV.root = rootBonesForSkinning;
			}
		}
	}

	public override void PartialResetPhysics()
	{
		if (base.enabled)
		{
			GPHairPhysics[] componentsInChildren = GetComponentsInChildren<GPHairPhysics>(true);
			GPHairPhysics[] array = componentsInChildren;
			foreach (GPHairPhysics gPHairPhysics in array)
			{
				gPHairPhysics.PartialResetPhysics();
			}
		}
	}

	public override void ResetPhysics()
	{
		if (base.enabled)
		{
			GPHairPhysics[] componentsInChildren = GetComponentsInChildren<GPHairPhysics>(true);
			GPHairPhysics[] array = componentsInChildren;
			foreach (GPHairPhysics gPHairPhysics in array)
			{
				gPHairPhysics.ResetPhysics();
			}
		}
	}
}
