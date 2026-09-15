using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class GravityControl : JSONStorable
{
	protected string[] customParamNames = new string[3] { "gravityX", "gravityY", "gravityZ" };

	public Slider gravityXSlider;

	[SerializeField]
	private float _gravityX;

	public Slider gravityYSlider;

	[SerializeField]
	private float _gravityY = -9.81f;

	public Slider gravityZSlider;

	[SerializeField]
	private float _gravityZ;

	private Vector3 _setGravity;

	public float gravityX
	{
		get
		{
			return _gravityX;
		}
		set
		{
			if (_gravityX != value)
			{
				_gravityX = value;
				if (gravityXSlider != null)
				{
					gravityXSlider.value = _gravityX;
				}
				SetGravity();
			}
		}
	}

	public float gravityY
	{
		get
		{
			return _gravityY;
		}
		set
		{
			if (_gravityY != value)
			{
				_gravityY = value;
				if (gravityYSlider != null)
				{
					gravityYSlider.value = _gravityY;
				}
				SetGravity();
			}
		}
	}

	public float gravityZ
	{
		get
		{
			return _gravityZ;
		}
		set
		{
			if (_gravityZ != value)
			{
				_gravityZ = value;
				if (gravityZSlider != null)
				{
					gravityZSlider.value = _gravityZ;
				}
				SetGravity();
			}
		}
	}

	public Vector3 gravity
	{
		get
		{
			return _setGravity;
		}
		set
		{
			if (_gravityX != value.x || _gravityY != value.y || _gravityZ != value.z)
			{
				_gravityX = value.x;
				_gravityY = value.y;
				_gravityZ = value.z;
				SetGravity();
			}
		}
	}

	public override string[] GetCustomParamNames()
	{
		return customParamNames;
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (includePhysical || forceStore)
		{
			if (gravityXSlider != null)
			{
				SliderControl component = gravityXSlider.GetComponent<SliderControl>();
				if (component == null || component.defaultValue != gravityX || forceStore)
				{
					needsStore = true;
					jSON["gravityX"].AsFloat = gravityX;
				}
			}
			if (gravityYSlider != null)
			{
				SliderControl component2 = gravityYSlider.GetComponent<SliderControl>();
				if (component2 == null || component2.defaultValue != gravityY || forceStore)
				{
					needsStore = true;
					jSON["gravityY"].AsFloat = gravityY;
				}
			}
			if (gravityZSlider != null)
			{
				SliderControl component3 = gravityZSlider.GetComponent<SliderControl>();
				if (component3 == null || component3.defaultValue != gravityZ || forceStore)
				{
					needsStore = true;
					jSON["gravityZ"].AsFloat = gravityZ;
				}
			}
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (base.physicalLocked || !restorePhysical)
		{
			return;
		}
		if (!IsCustomPhysicalParamLocked("gravityX"))
		{
			if (jc["gravityX"] != null)
			{
				gravityX = jc["gravityX"].AsFloat;
			}
			else if (setMissingToDefault && gravityXSlider != null)
			{
				SliderControl component = gravityXSlider.GetComponent<SliderControl>();
				if (component != null)
				{
					gravityX = component.defaultValue;
				}
			}
		}
		if (!IsCustomPhysicalParamLocked("gravityY"))
		{
			if (jc["gravityY"] != null)
			{
				gravityY = jc["gravityY"].AsFloat;
			}
			else if (setMissingToDefault && gravityYSlider != null)
			{
				SliderControl component2 = gravityYSlider.GetComponent<SliderControl>();
				if (component2 != null)
				{
					gravityY = component2.defaultValue;
				}
			}
		}
		if (IsCustomPhysicalParamLocked("gravityZ"))
		{
			return;
		}
		if (jc["gravityZ"] != null)
		{
			gravityZ = jc["gravityZ"].AsFloat;
		}
		else if (setMissingToDefault && gravityZSlider != null)
		{
			SliderControl component3 = gravityZSlider.GetComponent<SliderControl>();
			if (component3 != null)
			{
				gravityZ = component3.defaultValue;
			}
		}
	}

	private void SetGravity()
	{
		_setGravity.x = _gravityX;
		_setGravity.y = _gravityY;
		_setGravity.z = _gravityZ;
		Physics.gravity = _setGravity;
	}

	public override void InitUI()
	{
		if (gravityXSlider != null)
		{
			gravityXSlider.value = _gravityX;
			gravityXSlider.onValueChanged.AddListener(delegate
			{
				gravityX = gravityXSlider.value;
			});
			SliderControl component = gravityXSlider.GetComponent<SliderControl>();
			if (component != null)
			{
				component.defaultValue = _gravityX;
			}
		}
		if (gravityYSlider != null)
		{
			gravityYSlider.value = _gravityY;
			gravityYSlider.onValueChanged.AddListener(delegate
			{
				gravityY = gravityYSlider.value;
			});
			SliderControl component2 = gravityYSlider.GetComponent<SliderControl>();
			if (component2 != null)
			{
				component2.defaultValue = _gravityY;
			}
		}
		if (gravityZSlider != null)
		{
			gravityZSlider.value = _gravityZ;
			gravityZSlider.onValueChanged.AddListener(delegate
			{
				gravityZ = gravityZSlider.value;
			});
			SliderControl component3 = gravityZSlider.GetComponent<SliderControl>();
			if (component3 != null)
			{
				component3.defaultValue = _gravityZ;
			}
		}
	}

	private void Start()
	{
		InitUI();
		SetGravity();
	}
}
