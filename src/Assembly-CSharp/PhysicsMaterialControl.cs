using System;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsMaterialControl : JSONStorable
{
	protected PhysicMaterial pMaterial;

	protected Collider[] colliders;

	protected JSONStorableFloat dynamicFrictionJSON;

	[SerializeField]
	protected float _dynamicFriction = 0.6f;

	protected JSONStorableFloat staticFrictionJSON;

	[SerializeField]
	protected float _staticFriction = 0.6f;

	protected JSONStorableFloat bouncinessJSON;

	[SerializeField]
	protected float _bounciness;

	protected JSONStorableStringChooser frictionCombineJSON;

	protected JSONStorableStringChooser bounceCombineJSON;

	public float dynamicFriction
	{
		get
		{
			return _dynamicFriction;
		}
		set
		{
			if (dynamicFrictionJSON != null)
			{
				dynamicFrictionJSON.val = value;
			}
			else
			{
				SyncDynamicFriction(value);
			}
		}
	}

	public float staticFriction
	{
		get
		{
			return _staticFriction;
		}
		set
		{
			if (staticFrictionJSON != null)
			{
				staticFrictionJSON.val = value;
			}
			else
			{
				SyncStaticFriction(value);
			}
		}
	}

	public float bounciness
	{
		get
		{
			return _bounciness;
		}
		set
		{
			if (bouncinessJSON != null)
			{
				bouncinessJSON.val = value;
			}
			else
			{
				SyncBounciness(value);
			}
		}
	}

	public void SyncColliders()
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(true);
		List<Collider> list = new List<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			PhysicsMaterialControl component = collider.GetComponent<PhysicsMaterialControl>();
			if ((component == null || component == this) && (collider.sharedMaterial == null || collider.sharedMaterial.name == "Default"))
			{
				collider.sharedMaterial = pMaterial;
				list.Add(collider);
			}
		}
		colliders = list.ToArray();
	}

	protected void SyncDynamicFriction(float f)
	{
		_dynamicFriction = f;
		if (pMaterial != null)
		{
			pMaterial.dynamicFriction = f;
		}
	}

	protected void SyncStaticFriction(float f)
	{
		_staticFriction = f;
		if (pMaterial != null)
		{
			pMaterial.staticFriction = f;
		}
	}

	protected void SyncBounciness(float f)
	{
		_bounciness = f;
		if (pMaterial != null)
		{
			pMaterial.bounciness = f;
		}
	}

	protected void SetFrictionCombine(string s)
	{
		if (!(pMaterial != null))
		{
			return;
		}
		try
		{
			PhysicMaterialCombine physicMaterialCombine = (PhysicMaterialCombine)Enum.Parse(typeof(PhysicMaterialCombine), s);
			if (pMaterial.frictionCombine != physicMaterialCombine)
			{
				pMaterial.frictionCombine = physicMaterialCombine;
			}
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set friction combine " + s + " which is not a valid value");
		}
	}

	protected void SetBounceCombine(string s)
	{
		if (!(pMaterial != null))
		{
			return;
		}
		try
		{
			PhysicMaterialCombine physicMaterialCombine = (PhysicMaterialCombine)Enum.Parse(typeof(PhysicMaterialCombine), s);
			if (pMaterial.bounceCombine != physicMaterialCombine)
			{
				pMaterial.bounceCombine = physicMaterialCombine;
			}
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set bounce combine " + s + " which is not a valid value");
		}
	}

	protected void Init()
	{
		pMaterial = new PhysicMaterial();
		SyncColliders();
		SyncDynamicFriction(_dynamicFriction);
		dynamicFrictionJSON = new JSONStorableFloat("dynamicFriction", _dynamicFriction, SyncDynamicFriction, 0f, 1f);
		dynamicFrictionJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(dynamicFrictionJSON);
		SyncStaticFriction(_staticFriction);
		staticFrictionJSON = new JSONStorableFloat("staticFriction", _staticFriction, SyncStaticFriction, 0f, 1f);
		staticFrictionJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(staticFrictionJSON);
		SyncBounciness(_bounciness);
		bouncinessJSON = new JSONStorableFloat("bounciness", _bounciness, SyncBounciness, 0f, 1f);
		bouncinessJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(bouncinessJSON);
		string[] names = Enum.GetNames(typeof(PhysicMaterialCombine));
		List<string> choicesList = new List<string>(names);
		frictionCombineJSON = new JSONStorableStringChooser("frictionCombine", choicesList, pMaterial.frictionCombine.ToString(), "Friction Combine", SetFrictionCombine);
		frictionCombineJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(frictionCombineJSON);
		bounceCombineJSON = new JSONStorableStringChooser("bounceCombine", choicesList, pMaterial.bounceCombine.ToString(), "Bounce Combine", SetBounceCombine);
		bounceCombineJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(bounceCombineJSON);
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			PhysicsMaterialControlUI componentInChildren = UITransform.GetComponentInChildren<PhysicsMaterialControlUI>(true);
			if (componentInChildren != null)
			{
				dynamicFrictionJSON.slider = componentInChildren.dynamicFrictionSlider;
				staticFrictionJSON.slider = componentInChildren.staticFrictionSlider;
				bouncinessJSON.slider = componentInChildren.bouncinessSlider;
				frictionCombineJSON.popup = componentInChildren.frictionCombinePopup;
				bounceCombineJSON.popup = componentInChildren.bounceCombinePopup;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			PhysicsMaterialControlUI componentInChildren = UITransformAlt.GetComponentInChildren<PhysicsMaterialControlUI>(true);
			if (componentInChildren != null)
			{
				dynamicFrictionJSON.sliderAlt = componentInChildren.dynamicFrictionSlider;
				staticFrictionJSON.sliderAlt = componentInChildren.staticFrictionSlider;
				bouncinessJSON.sliderAlt = componentInChildren.bouncinessSlider;
				frictionCombineJSON.popupAlt = componentInChildren.frictionCombinePopup;
				bounceCombineJSON.popupAlt = componentInChildren.bounceCombinePopup;
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
}
