using System.Collections.Generic;
using System.Text.RegularExpressions;
using GPUTools.Cloth.Scripts.Runtime.Physics;
using MVR.FileManagement;
using MeshVR;
using UnityEngine;

[ExecuteInEditMode]
public class DAZClothingItem : DAZDynamicItem
{
	public enum ExclusiveRegion
	{
		None,
		UnderHip,
		UnderChest,
		Hip,
		Chest,
		Shoes,
		Glasses,
		Gloves,
		Hat,
		Legs
	}

	public enum ColliderType
	{
		None,
		Shoe
	}

	public enum ControllerType
	{
		None,
		LeftFoot,
		RightFoot,
		LeftToe,
		RightToe
	}

	[SerializeField]
	protected bool _jointAdjustEnabled = true;

	public bool adjustFemaleBreastJointSpringAndDamper;

	public float breastJointSpringAndDamperMultiplier = 3f;

	public bool adjustFemaleGluteJointSpringAndDamper;

	public float gluteJointSpringAndDamperMultiplier = 3f;

	public ExclusiveRegion exclusiveRegion;

	public ColliderType colliderTypeRight;

	public ColliderType colliderTypeLeft;

	public BoxCollider colliderRight;

	public BoxCollider colliderLeft;

	public Vector3 colliderDimensions;

	public Vector3 colliderRightCenter;

	public Vector3 colliderLeftCenter;

	public Vector3 colliderRightRotation;

	public Vector3 colliderLeftRotation;

	public ControllerType driveXAngleTargetController1Type;

	public ControllerType driveXAngleTargetController2Type;

	public FreeControllerV3 driveXAngleTargetController1;

	public FreeControllerV3 driveXAngleTargetController2;

	public float driveXAngleTarget;

	public ControllerType drive2XAngleTargetController1Type;

	public ControllerType drive2XAngleTargetController2Type;

	public FreeControllerV3 drive2XAngleTargetController1;

	public FreeControllerV3 drive2XAngleTargetController2;

	public float drive2XAngleTarget;

	protected DAZClothingItemControl[] clothingItemControls;

	public bool jointAdjustEnabled
	{
		get
		{
			return _jointAdjustEnabled;
		}
		set
		{
			if (_jointAdjustEnabled != value)
			{
				_jointAdjustEnabled = value;
				SyncClothingAdjustments();
			}
		}
	}

	public void SyncClothingAdjustments()
	{
		if (characterSelector != null)
		{
			characterSelector.SyncClothingAdjustments();
		}
	}

	public void RefreshClothingItems()
	{
		if (characterSelector != null)
		{
			characterSelector.RefreshDynamicClothes();
		}
	}

	public void RefreshClothingItemThumbnail(string thumbPath)
	{
		if (characterSelector != null)
		{
			characterSelector.InvalidateDynamicClothingItemThumbnail(thumbPath);
			characterSelector.RefreshDynamicClothingThumbnails();
		}
	}

	public bool IsClothingUIDAvailable(string uid)
	{
		if (characterSelector != null)
		{
			return characterSelector.IsClothingUIDAvailable(uid);
		}
		return false;
	}

	protected override void SyncOtherTags()
	{
		otherTags = GetAllClothingOtherTags();
		base.SyncOtherTags();
	}

	public HashSet<string> GetAllClothingOtherTags()
	{
		if (characterSelector != null)
		{
			return characterSelector.GetClothingOtherTags();
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
				hidePath = "Custom/Clothing/Female/Builtin/" + assetName + "/" + assetName + ".hide";
			}
			else if (gender == Gender.Male)
			{
				hidePath = "Custom/Clothing/Male/Builtin/" + assetName + "/" + assetName + ".hide";
			}
			else
			{
				hidePath = "Custom/Clothing/Neutral/Builtin/" + assetName + "/" + assetName + ".hide";
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
				userPrefsPath = "Custom/Clothing/Female/Builtin/" + assetName + "/" + assetName + ".prefs";
			}
			else if (gender == Gender.Male)
			{
				userPrefsPath = "Custom/Clothing/Male/Builtin/" + assetName + "/" + assetName + ".prefs";
			}
			else
			{
				userPrefsPath = "Custom/Clothing/Neutral/Builtin/" + assetName + "/" + assetName + ".prefs";
			}
		}
	}

	public void SetLocked(bool b)
	{
		locked = b;
		if (clothingItemControls == null)
		{
			return;
		}
		DAZClothingItemControl[] array = clothingItemControls;
		foreach (DAZClothingItemControl dAZClothingItemControl in array)
		{
			if (dAZClothingItemControl.lockedJSON != null)
			{
				dAZClothingItemControl.lockedJSON.val = b;
			}
		}
	}

	protected override void InitInstance()
	{
		base.InitInstance();
		clothingItemControls = GetComponentsInChildren<DAZClothingItemControl>(true);
		if (clothingItemControls != null)
		{
			DAZClothingItemControl[] array = clothingItemControls;
			foreach (DAZClothingItemControl dAZClothingItemControl in array)
			{
				dAZClothingItemControl.clothingItem = this;
			}
		}
		DAZDynamic componentInChildren = GetComponentInChildren<DAZDynamic>(true);
		if (componentInChildren != null)
		{
			if (gender == Gender.Female)
			{
				componentInChildren.itemType = PresetManager.ItemType.ClothingFemale;
			}
			else if (gender == Gender.Male)
			{
				componentInChildren.itemType = PresetManager.ItemType.ClothingMale;
			}
			else
			{
				componentInChildren.itemType = PresetManager.ItemType.ClothingNeutral;
			}
			if (dynamicRuntimeLoadPath != null && dynamicRuntimeLoadPath != string.Empty)
			{
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

	public override void PartialResetPhysics()
	{
		if (base.enabled)
		{
			ClothPhysics[] componentsInChildren = GetComponentsInChildren<ClothPhysics>(true);
			ClothPhysics[] array = componentsInChildren;
			foreach (ClothPhysics clothPhysics in array)
			{
				clothPhysics.PartialResetPhysics();
			}
		}
	}

	public override void ResetPhysics()
	{
		if (base.enabled)
		{
			ClothPhysics[] componentsInChildren = GetComponentsInChildren<ClothPhysics>(true);
			ClothPhysics[] array = componentsInChildren;
			foreach (ClothPhysics clothPhysics in array)
			{
				clothPhysics.ResetPhysics();
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (colliderRight != null)
		{
			colliderRight.gameObject.SetActive(true);
			colliderRight.transform.localEulerAngles = colliderRightRotation;
			colliderRight.size = colliderDimensions;
			colliderRight.center = colliderRightCenter;
		}
		if (colliderLeft != null)
		{
			colliderLeft.gameObject.SetActive(true);
			colliderLeft.transform.localEulerAngles = colliderLeftRotation;
			colliderLeft.size = colliderDimensions;
			colliderLeft.center = colliderLeftCenter;
		}
		if (driveXAngleTargetController1 != null)
		{
			driveXAngleTargetController1.jointRotationDriveXTarget = driveXAngleTarget;
		}
		if (driveXAngleTargetController2 != null)
		{
			driveXAngleTargetController2.jointRotationDriveXTarget = driveXAngleTarget;
		}
		if (drive2XAngleTargetController1 != null)
		{
			drive2XAngleTargetController1.jointRotationDriveXTarget = drive2XAngleTarget;
		}
		if (drive2XAngleTargetController2 != null)
		{
			drive2XAngleTargetController2.jointRotationDriveXTarget = drive2XAngleTarget;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (colliderRight != null)
		{
			colliderRight.gameObject.SetActive(false);
		}
		if (colliderLeft != null)
		{
			colliderLeft.gameObject.SetActive(false);
		}
		if (driveXAngleTargetController1 != null)
		{
			driveXAngleTargetController1.jointRotationDriveXTarget = 0f;
		}
		if (driveXAngleTargetController2 != null)
		{
			driveXAngleTargetController2.jointRotationDriveXTarget = 0f;
		}
		if (drive2XAngleTargetController1 != null)
		{
			drive2XAngleTargetController1.jointRotationDriveXTarget = 0f;
		}
		if (drive2XAngleTargetController2 != null)
		{
			drive2XAngleTargetController2.jointRotationDriveXTarget = 0f;
		}
	}
}
