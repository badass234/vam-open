using System.Collections;
using UnityEngine;

public class SetTransformScale : JSONStorable
{
	public Transform additionalScaleTransform;

	public Transform transformToReparentWhenScaling;

	private bool needsUnitScaleFix;

	protected JSONStorableFloat scaleJSON;

	[SerializeField]
	protected float _scale = 1f;

	protected bool wasInit;

	protected void FixForUnitScale()
	{
		if (needsUnitScaleFix)
		{
			if (additionalScaleTransform != null)
			{
				additionalScaleTransform.localScale = Vector3.one;
			}
			base.transform.localScale = Vector3.one;
			needsUnitScaleFix = false;
		}
	}

	protected IEnumerator FixForUnitScaleCo()
	{
		yield return null;
		FixForUnitScale();
	}

	protected void SyncScale(float newScale)
	{
		_scale = newScale;
		if (newScale == 1f)
		{
			needsUnitScaleFix = true;
			newScale = 1.00001f;
		}
		else
		{
			needsUnitScaleFix = false;
		}
		Vector3 localScale = default(Vector3);
		localScale.x = newScale;
		localScale.y = newScale;
		localScale.z = newScale;
		if (additionalScaleTransform != null)
		{
			if (transformToReparentWhenScaling != null)
			{
				Transform parent = transformToReparentWhenScaling.parent;
				transformToReparentWhenScaling.SetParent(base.transform, true);
				base.transform.localScale = localScale;
				additionalScaleTransform.localScale = localScale;
				transformToReparentWhenScaling.SetParent(parent, true);
			}
			else
			{
				base.transform.localScale = localScale;
				additionalScaleTransform.localScale = localScale;
			}
		}
		else
		{
			base.transform.localScale = localScale;
		}
		if (needsUnitScaleFix)
		{
			StartCoroutine(FixForUnitScaleCo());
		}
		if (containingAtom != null)
		{
			containingAtom.ScaleChanged(_scale);
		}
	}

	protected void Init()
	{
		if (!wasInit)
		{
			scaleJSON = new JSONStorableFloat("scale", _scale, SyncScale, 0.01f, 10f, false);
			RegisterFloat(scaleJSON);
			wasInit = true;
		}
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			Init();
			SetTransformScaleUI componentInChildren = UITransform.GetComponentInChildren<SetTransformScaleUI>(true);
			if (componentInChildren != null)
			{
				scaleJSON.slider = componentInChildren.scaleSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			Init();
			SetTransformScaleUI componentInChildren = UITransformAlt.GetComponentInChildren<SetTransformScaleUI>(true);
			if (componentInChildren != null)
			{
				scaleJSON.sliderAlt = componentInChildren.scaleSlider;
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

	protected void OnEnable()
	{
		FixForUnitScale();
	}

	protected void OnDisable()
	{
		FixForUnitScale();
	}
}
