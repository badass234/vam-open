using UnityEngine;

public class GridControl : JSONStorable
{
	public static GridControl singleton;

	protected JSONStorableFloat positionGridJSON;

	protected JSONStorableFloat rotationGridJSON;

	public float positionGrid
	{
		get
		{
			if (positionGridJSON != null)
			{
				return positionGridJSON.val;
			}
			return 0.1f;
		}
	}

	public float rotationGrid
	{
		get
		{
			if (rotationGridJSON != null)
			{
				return rotationGridJSON.val;
			}
			return 15f;
		}
	}

	protected void SyncPositionGrid(float f)
	{
		float num = f;
		num *= 1000f;
		num = Mathf.Round(num);
		num /= 1000f;
		positionGridJSON.valNoCallback = num;
	}

	protected void SyncRotationGrid(float f)
	{
		float num = f;
		num *= 100f;
		num = Mathf.Round(num);
		num /= 100f;
		rotationGridJSON.valNoCallback = num;
	}

	protected void Init()
	{
		positionGridJSON = new JSONStorableFloat("positionGrid", 0.1f, SyncPositionGrid, 0.001f, 10f, false);
		RegisterFloat(positionGridJSON);
		rotationGridJSON = new JSONStorableFloat("rotationGrid", 15f, SyncRotationGrid, 0.01f, 90f);
		RegisterFloat(rotationGridJSON);
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			GridControlUI componentInChildren = UITransform.GetComponentInChildren<GridControlUI>(true);
			if (componentInChildren != null)
			{
				positionGridJSON.slider = componentInChildren.positionGridSlider;
				rotationGridJSON.slider = componentInChildren.rotationGridSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			GridControlUI componentInChildren = UITransformAlt.GetComponentInChildren<GridControlUI>(true);
			if (componentInChildren != null)
			{
				positionGridJSON.sliderAlt = componentInChildren.positionGridSlider;
				rotationGridJSON.sliderAlt = componentInChildren.rotationGridSlider;
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			singleton = this;
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}
}
