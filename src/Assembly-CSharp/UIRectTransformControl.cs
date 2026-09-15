using UnityEngine;

public class UIRectTransformControl : JSONStorable
{
	protected RectTransform _rectTransform;

	protected JSONStorableFloat canvasXSizeJSON;

	protected JSONStorableFloat canvasYSizeJSON;

	public float minSize = 100f;

	public float maxSize = 2000f;

	public void SyncXSize(float s)
	{
		if (_rectTransform != null)
		{
			Vector2 sizeDelta = _rectTransform.sizeDelta;
			sizeDelta.x = s;
			_rectTransform.sizeDelta = sizeDelta;
		}
	}

	public void SyncYSize(float s)
	{
		if (_rectTransform != null)
		{
			Vector2 sizeDelta = _rectTransform.sizeDelta;
			sizeDelta.y = s;
			_rectTransform.sizeDelta = sizeDelta;
		}
	}

	protected void Init()
	{
		_rectTransform = GetComponent<RectTransform>();
		if (_rectTransform != null)
		{
			Vector2 sizeDelta = _rectTransform.sizeDelta;
			canvasXSizeJSON = new JSONStorableFloat("xSize", sizeDelta.x, SyncXSize, minSize, maxSize);
			RegisterFloat(canvasXSizeJSON);
			canvasYSizeJSON = new JSONStorableFloat("ySize", sizeDelta.y, SyncYSize, minSize, maxSize);
			RegisterFloat(canvasYSizeJSON);
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		UIRectTransformControlUI componentInChildren = UITransform.GetComponentInChildren<UIRectTransformControlUI>(true);
		if (componentInChildren != null)
		{
			if (canvasXSizeJSON != null)
			{
				canvasXSizeJSON.slider = componentInChildren.canvasXSizeSlider;
			}
			if (canvasYSizeJSON != null)
			{
				canvasYSizeJSON.slider = componentInChildren.canvasYSizeSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		UIRectTransformControlUI componentInChildren = UITransformAlt.GetComponentInChildren<UIRectTransformControlUI>(true);
		if (componentInChildren != null)
		{
			if (canvasXSizeJSON != null)
			{
				canvasXSizeJSON.sliderAlt = componentInChildren.canvasXSizeSlider;
			}
			if (canvasYSizeJSON != null)
			{
				canvasYSizeJSON.sliderAlt = componentInChildren.canvasYSizeSlider;
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
