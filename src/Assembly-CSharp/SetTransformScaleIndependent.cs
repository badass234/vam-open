using System.Collections.Generic;
using UnityEngine;

public class SetTransformScaleIndependent : JSONStorable
{
	public Transform additionalScaleTransform;

	public bool alignAdditionalScaleTransform = true;

	protected JSONStorableFloat scaleJSON;

	protected JSONStorableFloat scaleXJSON;

	protected JSONStorableFloat scaleYJSON;

	protected JSONStorableFloat scaleZJSON;

	[SerializeField]
	protected float _scaleX = 1f;

	[SerializeField]
	protected float _scaleY = 1f;

	[SerializeField]
	protected float _scaleZ = 1f;

	[SerializeField]
	protected float _scale = 1f;

	protected void SyncScale(float f)
	{
		_scale = f;
		SyncScale();
	}

	protected void SyncScaleX(float f)
	{
		_scaleX = f;
		SyncScale();
	}

	protected void SyncScaleY(float f)
	{
		_scaleY = f;
		SyncScale();
	}

	protected void SyncScaleZ(float f)
	{
		_scaleZ = f;
		SyncScale();
	}

	protected void SyncScale()
	{
		Vector3 localScale = default(Vector3);
		localScale.x = scaleXJSON.val * scaleJSON.val;
		localScale.y = scaleYJSON.val * scaleJSON.val;
		localScale.z = scaleZJSON.val * scaleJSON.val;
		if (additionalScaleTransform != null)
		{
			if (alignAdditionalScaleTransform)
			{
				List<Transform> list = new List<Transform>();
				foreach (Transform item in additionalScaleTransform)
				{
					list.Add(item);
				}
				foreach (Transform item2 in list)
				{
					item2.SetParent(null, true);
				}
				additionalScaleTransform.position = base.transform.position;
				additionalScaleTransform.rotation = base.transform.rotation;
				foreach (Transform item3 in list)
				{
					item3.SetParent(additionalScaleTransform, true);
					item3.localScale = Vector3.one;
				}
			}
			additionalScaleTransform.localScale = localScale;
		}
		base.transform.localScale = localScale;
	}

	protected void Init()
	{
		scaleXJSON = new JSONStorableFloat("scaleX", _scaleX, SyncScaleX, 0.01f, 10f);
		RegisterFloat(scaleXJSON);
		scaleYJSON = new JSONStorableFloat("scaleY", _scaleY, SyncScaleY, 0.01f, 10f);
		RegisterFloat(scaleYJSON);
		scaleZJSON = new JSONStorableFloat("scaleZ", _scaleZ, SyncScaleZ, 0.01f, 10f);
		RegisterFloat(scaleZJSON);
		scaleJSON = new JSONStorableFloat("scale", _scale, SyncScale, 0.01f, 10f);
		RegisterFloat(scaleJSON);
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			SetTransformScaleIndependentUI componentInChildren = UITransform.GetComponentInChildren<SetTransformScaleIndependentUI>(true);
			if (componentInChildren != null)
			{
				scaleJSON.slider = componentInChildren.scaleSlider;
				scaleXJSON.slider = componentInChildren.scaleXSlider;
				scaleYJSON.slider = componentInChildren.scaleYSlider;
				scaleZJSON.slider = componentInChildren.scaleZSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			SetTransformScaleIndependentUI componentInChildren = UITransformAlt.GetComponentInChildren<SetTransformScaleIndependentUI>(true);
			if (componentInChildren != null)
			{
				scaleJSON.sliderAlt = componentInChildren.scaleSlider;
				scaleXJSON.sliderAlt = componentInChildren.scaleXSlider;
				scaleYJSON.sliderAlt = componentInChildren.scaleYSlider;
				scaleZJSON.sliderAlt = componentInChildren.scaleZSlider;
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
