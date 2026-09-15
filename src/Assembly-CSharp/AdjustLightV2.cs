using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AdjustLightV2 : JSONStorable
{
	public bool controlBias = true;

	public float pointBias = 0.001f;

	public float spotBias = 0.001f;

	public float directionalBias = 0.02f;

	public bool controlNearPlane;

	public float pointNearPlane = 0.1f;

	public float spotNearPlane = 0.5f;

	public float directionalNearPlane = 0.5f;

	public MeshRenderer[] emissiveRenderers;

	public MeshRenderer[] offRenderers;

	public bool autoShadowType = true;

	protected Light _light;

	protected JSONStorableAction toggleOnJSONAction;

	protected JSONStorableBool onJSONParam;

	protected bool _on = true;

	protected JSONStorableFloat pointBiasJSON;

	protected JSONStorableFloat intensityJSONParam;

	protected float _intensity;

	protected JSONStorableFloat rangeJSONParam;

	protected float _range;

	protected JSONStorableColor colorJSONParam;

	protected Color _color;

	protected HSVColor _HSVcolor;

	protected LightShadows saveShadowType = LightShadows.Soft;

	protected JSONStorableBool shadowsOnJSONParam;

	protected bool _shadowsOn;

	protected JSONStorableFloat shadowStrengthJSONParam;

	protected float _shadowStrength;

	protected JSONStorableFloat spotAngleJSONParam;

	protected float _spotAngle;

	protected JSONStorableBool showHaloJSONParam;

	[SerializeField]
	protected bool _showHalo = true;

	protected JSONStorableBool showDustJSONParam;

	[SerializeField]
	protected bool _showDust = true;

	protected JSONStorableStringChooser lightTypeJSON;

	protected JSONStorableStringChooser shadowResolutionJSON;

	protected JSONStorableStringChooser renderModeJSON;

	protected void SyncEmissiveRenderers()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		MeshRenderer[] array = emissiveRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (!(meshRenderer != null))
			{
				continue;
			}
			meshRenderer.enabled = _on;
			Material[] materials = meshRenderer.materials;
			foreach (Material material in materials)
			{
				Color color = _light.color * _light.intensity;
				if (material.HasProperty("_MKGlowColor"))
				{
					material.SetColor("_MKGlowColor", color);
				}
				if (material.HasProperty("_Color"))
				{
					material.SetColor("_Color", color + Color.gray);
				}
			}
		}
		MeshRenderer[] array2 = offRenderers;
		foreach (MeshRenderer meshRenderer2 in array2)
		{
			if (meshRenderer2 != null)
			{
				meshRenderer2.enabled = !_on;
			}
		}
	}

	public void ToggleOn()
	{
		if (onJSONParam != null)
		{
			onJSONParam.val = !onJSONParam.val;
		}
	}

	public void SyncOn(bool val)
	{
		_on = val;
		if (_light != null)
		{
			_light.enabled = val;
		}
		SyncEmissiveRenderers();
	}

	protected void SyncPointBias(float f)
	{
		pointBias = f;
		SetAutoShadowType();
	}

	public void SyncIntensity(float val)
	{
		_intensity = val;
		if (_light != null)
		{
			_light.intensity = val;
		}
		SyncEmissiveRenderers();
	}

	public void SyncRange(float val)
	{
		_range = val;
		if (_light != null)
		{
			_light.range = val;
		}
	}

	public void SyncColor(float h, float s, float v)
	{
		_HSVcolor.H = h;
		_HSVcolor.S = s;
		_HSVcolor.V = v;
		_color = HSVColorPicker.HSVToRGB(h, s, v);
		if (_light != null)
		{
			_light.color = _color;
		}
		SyncEmissiveRenderers();
	}

	public void SyncShadowsOn(bool val)
	{
		_shadowsOn = val;
		if (_light != null)
		{
			if (_shadowsOn)
			{
				_light.shadows = saveShadowType;
				return;
			}
			saveShadowType = _light.shadows;
			_light.shadows = LightShadows.None;
		}
	}

	public void SyncShadowStrength(float val)
	{
		_shadowStrength = val;
		if (_light != null)
		{
			_light.shadowStrength = _shadowStrength;
		}
	}

	public void SyncSpotAngle(float val)
	{
		_spotAngle = val;
		if (_light != null)
		{
			_light.spotAngle = val;
		}
	}

	protected void SyncShowHalo(bool val)
	{
		_showHalo = val;
		Behaviour behaviour = (Behaviour)GetComponent("Halo");
		if (behaviour != null)
		{
			behaviour.enabled = _showHalo;
		}
	}

	protected void SyncShowDust(bool val)
	{
		_showDust = val;
		ParticleSystem component = GetComponent<ParticleSystem>();
		if (component != null)
		{
			ParticleSystem.EmissionModule emission = component.emission;
			emission.enabled = _showDust;
		}
		ParticleSystemRenderer component2 = GetComponent<ParticleSystemRenderer>();
		if (component2 != null)
		{
			component2.enabled = _showDust;
		}
	}

	protected void SetAutoShadowType()
	{
		if (controlBias)
		{
			switch (_light.type)
			{
			case LightType.Directional:
				_light.shadowBias = directionalBias;
				break;
			case LightType.Point:
				_light.shadowBias = pointBias;
				break;
			case LightType.Spot:
				_light.shadowBias = spotBias;
				break;
			}
		}
		if (controlNearPlane)
		{
			switch (_light.type)
			{
			case LightType.Directional:
				_light.shadowNearPlane = directionalNearPlane;
				break;
			case LightType.Point:
				_light.shadowNearPlane = pointNearPlane;
				break;
			case LightType.Spot:
				_light.shadowNearPlane = spotNearPlane;
				break;
			}
		}
		if (!autoShadowType)
		{
			return;
		}
		if (_light.type == LightType.Directional)
		{
			saveShadowType = LightShadows.Soft;
			if (_light.shadows != 0)
			{
				SetShadowType("Soft");
			}
		}
		else
		{
			saveShadowType = LightShadows.Hard;
			if (_light.shadows != 0)
			{
				SetShadowType("Hard");
			}
		}
	}

	public void SetLightType(string type)
	{
		if (!(_light != null))
		{
			return;
		}
		try
		{
			LightType lightType = (LightType)Enum.Parse(typeof(LightType), type);
			if (_light.type != lightType)
			{
				_light.type = lightType;
				SetAutoShadowType();
			}
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set light type to " + type + " which is not a valid light type");
		}
	}

	public void SetShadowResolution(string res)
	{
		if (!(_light != null))
		{
			return;
		}
		try
		{
			LightShadowResolution lightShadowResolution = (LightShadowResolution)Enum.Parse(typeof(LightShadowResolution), res);
			if (_light.shadowResolution != lightShadowResolution)
			{
				_light.shadowResolution = lightShadowResolution;
			}
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set light shadow resolution " + res + " which is not a valid value");
		}
	}

	public void SetRenderMode(string mode)
	{
		if (!(_light != null))
		{
			return;
		}
		try
		{
			LightRenderMode lightRenderMode = (LightRenderMode)Enum.Parse(typeof(LightRenderMode), mode);
			if (_light.renderMode != lightRenderMode)
			{
				_light.renderMode = lightRenderMode;
			}
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set light render mode to " + mode + " which is not a valid light render mode");
		}
	}

	public void SetShadowType(string type)
	{
		if (!(_light != null))
		{
			return;
		}
		try
		{
			LightShadows lightShadows = (LightShadows)Enum.Parse(typeof(LightShadows), type);
			if (_light.shadows != lightShadows)
			{
				_light.shadows = lightShadows;
			}
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set light shadow type to " + type + " which is not a valid light shadow type");
		}
	}

	protected void Init()
	{
		_light = GetComponent<Light>();
		if (_light != null)
		{
			_on = _light.enabled;
			onJSONParam = new JSONStorableBool("on", _on, SyncOn);
			RegisterBool(onJSONParam);
			toggleOnJSONAction = new JSONStorableAction("ToggleOn", ToggleOn);
			RegisterAction(toggleOnJSONAction);
			_intensity = _light.intensity;
			intensityJSONParam = new JSONStorableFloat("intensity", _intensity, SyncIntensity, 0f, 8f);
			RegisterFloat(intensityJSONParam);
			_range = _light.range;
			rangeJSONParam = new JSONStorableFloat("range", _range, SyncRange, 0f, 25f);
			RegisterFloat(rangeJSONParam);
			_color = _light.color;
			_HSVcolor = HSVColorPicker.RGBToHSV(_color.r, _color.g, _color.b);
			colorJSONParam = new JSONStorableColor("color", _HSVcolor, SyncColor);
			RegisterColor(colorJSONParam);
			_spotAngle = _light.spotAngle;
			spotAngleJSONParam = new JSONStorableFloat("spotAngle", _spotAngle, SyncSpotAngle, 1f, 180f);
			RegisterFloat(spotAngleJSONParam);
			pointBiasJSON = new JSONStorableFloat("pointBias", pointBias, SyncPointBias, 0f, 0.03f);
			RegisterFloat(pointBiasJSON);
			ParticleSystem component = GetComponent<ParticleSystem>();
			if (component != null)
			{
				showDustJSONParam = new JSONStorableBool("showDust", _showDust, SyncShowDust);
				RegisterBool(showDustJSONParam);
				SyncShowDust(_showDust);
			}
			Behaviour behaviour = (Behaviour)GetComponent("Halo");
			if (behaviour != null)
			{
				showHaloJSONParam = new JSONStorableBool("showHalo", _showHalo, SyncShowHalo);
				RegisterBool(showHaloJSONParam);
				SyncShowHalo(_showHalo);
			}
			if (_light.shadows != 0)
			{
				_shadowsOn = true;
				saveShadowType = _light.shadows;
			}
			else
			{
				_shadowsOn = false;
				saveShadowType = LightShadows.Soft;
			}
			shadowsOnJSONParam = new JSONStorableBool("shadowsOn", _shadowsOn, SyncShadowsOn);
			RegisterBool(shadowsOnJSONParam);
			_shadowStrength = _light.shadowStrength;
			shadowStrengthJSONParam = new JSONStorableFloat("shadowStrength", _shadowStrength, SyncShadowStrength, 0f, 1f);
			RegisterFloat(shadowStrengthJSONParam);
			string[] names = Enum.GetNames(typeof(LightType));
			List<string> choicesList = new List<string>(names);
			lightTypeJSON = new JSONStorableStringChooser("type", choicesList, _light.type.ToString(), "Light Type", SetLightType);
			RegisterStringChooser(lightTypeJSON);
			string[] names2 = Enum.GetNames(typeof(LightRenderMode));
			List<string> choicesList2 = new List<string>(names2);
			renderModeJSON = new JSONStorableStringChooser("renderType", choicesList2, _light.renderMode.ToString(), "Render Mode", SetRenderMode);
			RegisterStringChooser(renderModeJSON);
			string[] names3 = Enum.GetNames(typeof(LightShadowResolution));
			List<string> choicesList3 = new List<string>(names3);
			shadowResolutionJSON = new JSONStorableStringChooser("shadowResolution", choicesList3, _light.shadowResolution.ToString(), "Shadow Resolution", SetShadowResolution);
			RegisterStringChooser(shadowResolutionJSON);
			SyncEmissiveRenderers();
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		AdjustLightV2UI componentInChildren = UITransform.GetComponentInChildren<AdjustLightV2UI>(true);
		if (!(componentInChildren != null))
		{
			return;
		}
		onJSONParam.toggle = componentInChildren.onToggle;
		intensityJSONParam.slider = componentInChildren.intensitySlider;
		rangeJSONParam.slider = componentInChildren.rangeSlider;
		colorJSONParam.colorPicker = componentInChildren.colorPicker;
		spotAngleJSONParam.slider = componentInChildren.spotAngleSlider;
		if (showHaloJSONParam != null)
		{
			showHaloJSONParam.toggle = componentInChildren.showHaloToggle;
			if (componentInChildren.showHaloToggle != null)
			{
				componentInChildren.showHaloToggle.gameObject.SetActive(true);
			}
		}
		else if (componentInChildren.showHaloToggle != null)
		{
			componentInChildren.showHaloToggle.gameObject.SetActive(false);
		}
		if (showDustJSONParam != null)
		{
			showDustJSONParam.toggle = componentInChildren.showDustToggle;
			if (componentInChildren.showDustToggle != null)
			{
				componentInChildren.showDustToggle.gameObject.SetActive(true);
			}
		}
		else if (componentInChildren.showDustToggle != null)
		{
			componentInChildren.showDustToggle.gameObject.SetActive(false);
		}
		shadowsOnJSONParam.toggle = componentInChildren.shadowsToggle;
		shadowStrengthJSONParam.slider = componentInChildren.shadowStrengthSlider;
		pointBiasJSON.slider = componentInChildren.pointBiasSlider;
		lightTypeJSON.popup = componentInChildren.typeSelector;
		shadowResolutionJSON.popup = componentInChildren.shadowResolutionSelector;
		renderModeJSON.popup = componentInChildren.renderModeSelector;
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		AdjustLightV2UI componentInChildren = UITransformAlt.GetComponentInChildren<AdjustLightV2UI>(true);
		if (!(componentInChildren != null))
		{
			return;
		}
		onJSONParam.toggleAlt = componentInChildren.onToggle;
		intensityJSONParam.sliderAlt = componentInChildren.intensitySlider;
		rangeJSONParam.sliderAlt = componentInChildren.rangeSlider;
		colorJSONParam.colorPickerAlt = componentInChildren.colorPicker;
		spotAngleJSONParam.sliderAlt = componentInChildren.spotAngleSlider;
		if (showHaloJSONParam != null)
		{
			showHaloJSONParam.toggleAlt = componentInChildren.showHaloToggle;
			if (componentInChildren.showHaloToggle != null)
			{
				componentInChildren.showHaloToggle.gameObject.SetActive(true);
			}
		}
		else if (componentInChildren.showHaloToggle != null)
		{
			componentInChildren.showHaloToggle.gameObject.SetActive(false);
		}
		if (showDustJSONParam != null)
		{
			showDustJSONParam.toggleAlt = componentInChildren.showDustToggle;
			if (componentInChildren.showDustToggle != null)
			{
				componentInChildren.showDustToggle.gameObject.SetActive(true);
			}
		}
		else if (componentInChildren.showDustToggle != null)
		{
			componentInChildren.showDustToggle.gameObject.SetActive(false);
		}
		shadowsOnJSONParam.toggleAlt = componentInChildren.shadowsToggle;
		shadowStrengthJSONParam.sliderAlt = componentInChildren.shadowStrengthSlider;
		pointBiasJSON.sliderAlt = componentInChildren.pointBiasSlider;
		lightTypeJSON.popupAlt = componentInChildren.typeSelector;
		shadowResolutionJSON.popupAlt = componentInChildren.shadowResolutionSelector;
		renderModeJSON.popupAlt = componentInChildren.renderModeSelector;
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
