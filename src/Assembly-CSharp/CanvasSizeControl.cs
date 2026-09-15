using UnityEngine;

public class CanvasSizeControl : JSONStorable
{
	public Canvas controlledCanvas;

	protected JSONStorableFloat horSizeJSON;

	[SerializeField]
	protected float _horSize = 1200f;

	protected JSONStorableFloat vertSizeJSON;

	[SerializeField]
	protected float _vertSize = 1000f;

	public float horSize
	{
		get
		{
			return _horSize;
		}
		set
		{
			if (horSizeJSON != null)
			{
				horSizeJSON.val = value;
			}
			else if (_horSize != value)
			{
				SyncHorSize(value);
				SetCanvasSize();
			}
		}
	}

	public float vertSize
	{
		get
		{
			return _vertSize;
		}
		set
		{
			if (vertSizeJSON != null)
			{
				vertSizeJSON.val = value;
			}
			else if (_vertSize != value)
			{
				SyncVertSize(value);
				SetCanvasSize();
			}
		}
	}

	protected void SetCanvasSize()
	{
		if (controlledCanvas != null)
		{
			RectTransform component = controlledCanvas.GetComponent<RectTransform>();
			if (component != null)
			{
				Vector2 sizeDelta = default(Vector2);
				sizeDelta.x = horSize;
				sizeDelta.y = vertSize;
				component.sizeDelta = sizeDelta;
			}
		}
	}

	protected void SyncHorSize(float f)
	{
		_horSize = f;
		SetCanvasSize();
	}

	protected void SyncVertSize(float f)
	{
		_vertSize = f;
		SetCanvasSize();
	}

	protected void Init()
	{
		horSizeJSON = new JSONStorableFloat("horizontalSize", _horSize, SyncHorSize, 400f, 2000f);
		RegisterFloat(horSizeJSON);
		vertSizeJSON = new JSONStorableFloat("verticalSize", _vertSize, SyncVertSize, 400f, 2000f);
		RegisterFloat(vertSizeJSON);
		SetCanvasSize();
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			CanvasSizeControlUI componentInChildren = UITransform.GetComponentInChildren<CanvasSizeControlUI>(true);
			if (componentInChildren != null)
			{
				horSizeJSON.slider = componentInChildren.horSizeSlider;
				vertSizeJSON.slider = componentInChildren.vertSizeSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			CanvasSizeControlUI componentInChildren = UITransformAlt.GetComponentInChildren<CanvasSizeControlUI>(true);
			if (componentInChildren != null)
			{
				horSizeJSON.sliderAlt = componentInChildren.horSizeSlider;
				vertSizeJSON.sliderAlt = componentInChildren.vertSizeSlider;
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
