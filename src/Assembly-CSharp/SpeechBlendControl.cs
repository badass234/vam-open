using System;
using System.Collections.Generic;
using SpeechBlendEngine;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBlendControl : JSONStorable
{
	public enum MorphSetType
	{
		Normal,
		Physics,
		Custom
	}

	public SpeechBlend speechBlend;

	public AudioSourceControl audioSourceControl;

	protected JSONStorableBool enabledJSON;

	protected JSONStorableFloat volumeJSON;

	protected JSONStorableFloat maxVolumeJSON;

	protected MorphSetType _morphSetType;

	public JSONStorableStringChooser morphSetJSON;

	protected GameObject advancedPanel;

	public JSONStorableAction openAdvancedPanelAction;

	protected InputField mouthOpenMorphUidInputField;

	protected JSONStorableString[] normalVisemeNamesJSON;

	protected JSONStorableString[] physicsVisemeNamesJSON;

	public JSONStorableString[] customVisemeNamesJSON;

	protected JSONStorableString[] normalMorphUidsJSON;

	protected JSONStorableString[] physicsMorphUidsJSON;

	public JSONStorableString[] customMorphUidsJSON;

	public JSONStorableAction[] pasteCustomMorphUidAction;

	protected JSONStorableString normalMouthOpenMorphUidJSON;

	protected JSONStorableString physicsMouthOpenMorphUidJSON;

	public JSONStorableString customMouthOpenMorphUidJSON;

	public JSONStorableAction pasteCustomMouthOpenMorphUidAction;

	public JSONStorableFloat[] normalVisemeWeightsJSON;

	public JSONStorableFloat[] physicsVisemeWeightsJSON;

	public JSONStorableFloat[] customVisemeWeightsJSON;

	public JSONStorableBool[] visemeFoundJSON;

	public JSONStorableFloat[] visemeRawValuesJSON;

	public JSONStorableFloat[] visemeValuesJSON;

	public JSONStorableBool mouthOpenVisemeFoundJSON;

	public JSONStorableFloat mouthOpenVisemeValueJSON;

	protected JSONStorableString[] currentVisemeNamesJSON;

	protected JSONStorableString[] currentMorphUidsJSON;

	protected JSONStorableString currentMouthOpenMorphUidJSON;

	protected JSONStorableFloat[] currentWeightsJSON;

	public JSONStorableAction resetAllAdvancedSettingsAction;

	protected bool visemeValuesJSONUIConnected;

	protected SpeechBlendVisemeUI[] visemeUIs;

	public float volume => volumeJSON.val;

	public float maxVolume => maxVolumeJSON.val;

	public JSONStorableFloat volumeMultiplierJSON { get; protected set; }

	public JSONStorableFloat volumeClampJSON { get; protected set; }

	public JSONStorableFloat volumeThresholdJSON { get; protected set; }

	public JSONStorableFloat mouthOpenFactorJSON { get; protected set; }

	public JSONStorableFloat mouthOpenChangeRateJSON { get; protected set; }

	public JSONStorableFloat visemeDetectionFactorJSON { get; protected set; }

	public JSONStorableFloat visemeThresholdJSON { get; protected set; }

	public JSONStorableFloat timeBetweenSamplingJSON { get; protected set; }

	public JSONStorableFloat sampleTimeAdjustJSON { get; protected set; }

	public JSONStorableFloat visemeMorphChangeRateJSON { get; protected set; }

	public JSONStorableFloat visemeMorphClampJSON { get; protected set; }

	public JSONStorableStringChooser voiceTypeJSON { get; protected set; }

	protected void SyncEnabled(bool b)
	{
		if (speechBlend != null)
		{
			speechBlend.enabled = b;
		}
	}

	protected void SyncVolumeMultiplier(float f)
	{
		if (speechBlend != null)
		{
			speechBlend.volumeMultiplier = f;
		}
	}

	protected void SyncVolumeClamp(float f)
	{
		if (speechBlend != null)
		{
			speechBlend.volumeClamp = f;
		}
	}

	protected void SyncVolumeThreshold(float f)
	{
		if (speechBlend != null)
		{
			speechBlend.volumeThreshold = f;
		}
	}

	protected void SyncMouthOpenFactorAmount(float f)
	{
		if (speechBlend != null)
		{
			speechBlend.jawMovementAmount = f;
		}
	}

	protected void SyncMouthOpenChangeRate(float f)
	{
		if (speechBlend != null)
		{
			speechBlend.jawMovementSpeed = f;
		}
	}

	protected void SyncVisemeDetectionFactor(float f)
	{
		if (speechBlend != null)
		{
			speechBlend.lipsBlendshapeMovementAmount = f;
		}
	}

	protected void SyncVisemeThreshold(float f)
	{
		if (speechBlend != null)
		{
			speechBlend.visemeThreshold = f;
		}
	}

	protected void SyncTimeBetweenSampling(float f)
	{
		if (speechBlend != null)
		{
			speechBlend.timeBetweenSampling = f;
		}
	}

	protected void SyncSampleTimeAdjust(float f)
	{
		if (speechBlend != null)
		{
			speechBlend.lookaheadAdjust = f;
		}
	}

	protected void SyncVisemeMorphChangeRate(float f)
	{
		if (speechBlend != null)
		{
			speechBlend.lipsBlendshapeChangeSpeed = f;
		}
	}

	protected void SyncVisemeMorphClamp(float f)
	{
		if (speechBlend != null)
		{
			speechBlend.blendshapeCutoff = f;
		}
	}

	protected void SyncVoiceType(string s)
	{
		if (speechBlend != null)
		{
			try
			{
				VoiceProfile.VoiceType voiceType = (VoiceProfile.VoiceType)Enum.Parse(typeof(VoiceProfile.VoiceType), s);
				speechBlend.voiceType = voiceType;
			}
			catch (ArgumentException)
			{
				Debug.LogError("Attempted to set voice type to " + s + " which is not a valid type");
			}
		}
	}

	protected void SyncMorphSetParams()
	{
		bool isStorable = _morphSetType == MorphSetType.Normal;
		bool isStorable2 = _morphSetType == MorphSetType.Physics;
		bool isStorable3 = _morphSetType == MorphSetType.Custom;
		for (int i = 0; i < normalVisemeWeightsJSON.Length; i++)
		{
			normalVisemeWeightsJSON[i].isStorable = isStorable;
		}
		for (int j = 0; j < physicsVisemeWeightsJSON.Length; j++)
		{
			physicsVisemeWeightsJSON[j].isStorable = isStorable2;
		}
		for (int k = 0; k < customVisemeWeightsJSON.Length; k++)
		{
			customVisemeWeightsJSON[k].isStorable = isStorable3;
		}
		for (int l = 0; l < customMorphUidsJSON.Length; l++)
		{
			customMorphUidsJSON[l].isStorable = isStorable3;
		}
		customMouthOpenMorphUidJSON.isStorable = isStorable3;
	}

	protected void SyncMorphSet(string s)
	{
		if (speechBlend != null)
		{
			try
			{
				MorphSetType morphSetType = (MorphSetType)Enum.Parse(typeof(MorphSetType), s);
				speechBlend.useBuiltInMorphs = _morphSetType != MorphSetType.Custom;
				speechBlend.setChoice = (int)morphSetType;
				_morphSetType = morphSetType;
				SyncMorphSetParams();
				SyncVisemeUIs();
			}
			catch (ArgumentException)
			{
				Debug.LogError("Attempted to set morph set to " + s + " which is not a valid type");
			}
		}
	}

	public void OpenAdvancedPanel()
	{
		if (advancedPanel != null)
		{
			advancedPanel.SetActive(true);
		}
	}

	public void ResetAllAdvancedSettings()
	{
		for (int i = 0; i < normalVisemeWeightsJSON.Length; i++)
		{
			normalVisemeWeightsJSON[i].SetValToDefault();
		}
		for (int j = 0; j < physicsVisemeWeightsJSON.Length; j++)
		{
			physicsVisemeWeightsJSON[j].SetValToDefault();
		}
		for (int k = 0; k < customVisemeWeightsJSON.Length; k++)
		{
			customVisemeWeightsJSON[k].SetValToDefault();
		}
		for (int l = 0; l < customMorphUidsJSON.Length; l++)
		{
			customMorphUidsJSON[l].SetValToDefault();
		}
		customMouthOpenMorphUidJSON.SetValToDefault();
	}

	protected void SyncVisemeUIs()
	{
		if (visemeUIs == null)
		{
			return;
		}
		if (visemeUIs.Length == normalVisemeWeightsJSON.Length)
		{
			if (currentVisemeNamesJSON != null)
			{
				for (int i = 0; i < currentVisemeNamesJSON.Length; i++)
				{
					currentVisemeNamesJSON[i].text = null;
				}
			}
			if (currentMorphUidsJSON != null)
			{
				for (int j = 0; j < currentMorphUidsJSON.Length; j++)
				{
					currentMorphUidsJSON[j].inputField = null;
				}
			}
			if (currentMouthOpenMorphUidJSON != null)
			{
				currentMouthOpenMorphUidJSON.inputField = null;
			}
			if (currentWeightsJSON != null)
			{
				for (int k = 0; k < currentWeightsJSON.Length; k++)
				{
					currentWeightsJSON[k].slider = null;
				}
			}
			switch (_morphSetType)
			{
			case MorphSetType.Normal:
				currentVisemeNamesJSON = normalVisemeNamesJSON;
				currentMorphUidsJSON = normalMorphUidsJSON;
				currentMouthOpenMorphUidJSON = normalMouthOpenMorphUidJSON;
				currentWeightsJSON = normalVisemeWeightsJSON;
				break;
			case MorphSetType.Physics:
				currentVisemeNamesJSON = physicsVisemeNamesJSON;
				currentMorphUidsJSON = physicsMorphUidsJSON;
				currentMouthOpenMorphUidJSON = physicsMouthOpenMorphUidJSON;
				currentWeightsJSON = physicsVisemeWeightsJSON;
				break;
			case MorphSetType.Custom:
				currentVisemeNamesJSON = customVisemeNamesJSON;
				currentMorphUidsJSON = customMorphUidsJSON;
				currentMouthOpenMorphUidJSON = customMouthOpenMorphUidJSON;
				currentWeightsJSON = customVisemeWeightsJSON;
				break;
			}
			for (int l = 0; l < speechBlend.VisemeMorphs.Length; l++)
			{
				visemeFoundJSON[l].val = speechBlend.VisemeMorphs[l] != null;
			}
			mouthOpenVisemeFoundJSON.val = speechBlend.MouthOpenMorph != null;
			for (int m = 0; m < currentWeightsJSON.Length; m++)
			{
				currentVisemeNamesJSON[m].text = visemeUIs[m].visemeNameText;
				currentMorphUidsJSON[m].inputField = visemeUIs[m].visemeMorphUidInputField;
				currentWeightsJSON[m].slider = visemeUIs[m].visemeWeightSlider;
			}
			for (int n = 0; n < pasteCustomMorphUidAction.Length; n++)
			{
				pasteCustomMorphUidAction[n].button = visemeUIs[n].pasteMorphUidButton;
				if (pasteCustomMorphUidAction[n].button != null)
				{
					pasteCustomMorphUidAction[n].button.gameObject.SetActive(_morphSetType == MorphSetType.Custom);
				}
			}
			currentMouthOpenMorphUidJSON.inputField = mouthOpenMorphUidInputField;
			if (pasteCustomMouthOpenMorphUidAction.button != null)
			{
				pasteCustomMouthOpenMorphUidAction.button.gameObject.SetActive(_morphSetType == MorphSetType.Custom);
			}
			if (visemeValuesJSONUIConnected)
			{
				return;
			}
			if (visemeValuesJSON != null)
			{
				for (int num = 0; num < visemeValuesJSON.Length; num++)
				{
					visemeValuesJSON[num].slider = visemeUIs[num].visemeValueSlider;
				}
			}
			if (visemeRawValuesJSON != null)
			{
				for (int num2 = 0; num2 < visemeRawValuesJSON.Length; num2++)
				{
					visemeRawValuesJSON[num2].slider = visemeUIs[num2].visemeRawValueSlider;
				}
			}
			if (visemeFoundJSON != null)
			{
				for (int num3 = 0; num3 < visemeFoundJSON.Length; num3++)
				{
					visemeFoundJSON[num3].indicator = visemeUIs[num3].visemeFoundIndicator;
					visemeFoundJSON[num3].negativeIndicator = visemeUIs[num3].visemeFoundNegativeIndicator;
				}
			}
			visemeValuesJSONUIConnected = true;
		}
		else
		{
			Debug.LogError("Length of visemeUIs " + visemeUIs.Length + " does not match length of weights " + normalVisemeWeightsJSON.Length);
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		base.InitUI(t, isAlt);
		if (!(t != null))
		{
			return;
		}
		SpeechBlendControlUI componentInChildren = t.GetComponentInChildren<SpeechBlendControlUI>(true);
		if (componentInChildren != null)
		{
			enabledJSON.RegisterToggle(componentInChildren.enabledToggle, isAlt);
			volumeJSON.RegisterSlider(componentInChildren.volumeSlider, isAlt);
			maxVolumeJSON.RegisterSlider(componentInChildren.maxVolumeSlider, isAlt);
			volumeMultiplierJSON.RegisterSlider(componentInChildren.volumeMultiplierSlider, isAlt);
			volumeClampJSON.RegisterSlider(componentInChildren.volumeClampSlider, isAlt);
			volumeThresholdJSON.RegisterSlider(componentInChildren.volumeThresholdSlider, isAlt);
			mouthOpenFactorJSON.RegisterSlider(componentInChildren.mouthOpenFactorSlider, isAlt);
			mouthOpenChangeRateJSON.RegisterSlider(componentInChildren.mouthOpenChangeRateSlider, isAlt);
			visemeDetectionFactorJSON.RegisterSlider(componentInChildren.visemeDetectionFactorSlider, isAlt);
			visemeThresholdJSON.RegisterSlider(componentInChildren.visemeThresholdSlider, isAlt);
			timeBetweenSamplingJSON.RegisterSlider(componentInChildren.timeBetweenSamplingSlider, isAlt);
			sampleTimeAdjustJSON.RegisterSlider(componentInChildren.sampleTimeAdjustSlider, isAlt);
			visemeMorphChangeRateJSON.RegisterSlider(componentInChildren.visemeMorphChangeRateSlider, isAlt);
			visemeMorphClampJSON.RegisterSlider(componentInChildren.visemeMorphClampSlider, isAlt);
			voiceTypeJSON.RegisterPopup(componentInChildren.voiceTypePopup, isAlt);
			morphSetJSON.RegisterPopup(componentInChildren.morphSetPopup, isAlt);
			if (!isAlt)
			{
				advancedPanel = componentInChildren.advancedPanel;
				mouthOpenMorphUidInputField = componentInChildren.mouthOpenMorphUidInputField;
			}
			mouthOpenVisemeFoundJSON.RegisterIndicator(componentInChildren.mouthOpenVisemeFoundIndicator, isAlt);
			mouthOpenVisemeFoundJSON.RegisterNegativeIndicator(componentInChildren.mouthOpenVisemeFoundNegativeIndicator, isAlt);
			mouthOpenVisemeValueJSON.RegisterSlider(componentInChildren.mouthOpenVisemeValueSlider, isAlt);
			openAdvancedPanelAction.RegisterButton(componentInChildren.openAdvancedPanelButton, isAlt);
			if (advancedPanel != null)
			{
				visemeUIs = advancedPanel.GetComponentsInChildren<SpeechBlendVisemeUI>(true);
				SyncVisemeUIs();
			}
			pasteCustomMouthOpenMorphUidAction.RegisterButton(componentInChildren.pasteMouthOpenMorphUidButton, isAlt);
			resetAllAdvancedSettingsAction.RegisterButton(componentInChildren.resetAllAdvancedSettingsButton, isAlt);
		}
	}

	protected void Init()
	{
		if (!(speechBlend != null))
		{
			return;
		}
		enabledJSON = new JSONStorableBool("enabled", speechBlend.enabled, SyncEnabled);
		enabledJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(enabledJSON);
		volumeJSON = new JSONStorableFloat("volume", 0f, 0f, 1f, true, false);
		maxVolumeJSON = new JSONStorableFloat("maxVolume", 0f, 0f, 1f, true, false);
		volumeMultiplierJSON = new JSONStorableFloat("volumeMultiplier", speechBlend.volumeMultiplier, SyncVolumeMultiplier, 0f, 10f, false);
		volumeMultiplierJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(volumeMultiplierJSON);
		volumeClampJSON = new JSONStorableFloat("volumeClamp", speechBlend.volumeClamp, SyncVolumeClamp, 0f, 1f);
		volumeClampJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(volumeClampJSON);
		volumeThresholdJSON = new JSONStorableFloat("volumeThreshold", speechBlend.volumeThreshold, SyncVolumeThreshold, 0f, 1f);
		volumeThresholdJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(volumeThresholdJSON);
		mouthOpenFactorJSON = new JSONStorableFloat("mouthOpenFactor", speechBlend.jawMovementAmount, SyncMouthOpenFactorAmount, 0f, 1f);
		mouthOpenFactorJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(mouthOpenFactorJSON);
		mouthOpenChangeRateJSON = new JSONStorableFloat("mouthOpenChangeRate", speechBlend.jawMovementSpeed, SyncMouthOpenChangeRate, 0f, 1f);
		mouthOpenChangeRateJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(mouthOpenChangeRateJSON);
		visemeDetectionFactorJSON = new JSONStorableFloat("visemeDetectionFactor", speechBlend.lipsBlendshapeMovementAmount, SyncVisemeDetectionFactor, -0.5f, 1f);
		visemeDetectionFactorJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(visemeDetectionFactorJSON);
		visemeThresholdJSON = new JSONStorableFloat("visemeThreshold", speechBlend.visemeThreshold, SyncVisemeThreshold, 0f, 1f);
		visemeThresholdJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(visemeThresholdJSON);
		timeBetweenSamplingJSON = new JSONStorableFloat("timeBetweenSampling", speechBlend.timeBetweenSampling, SyncTimeBetweenSampling, 0f, 0.15f);
		timeBetweenSamplingJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(timeBetweenSamplingJSON);
		sampleTimeAdjustJSON = new JSONStorableFloat("sampleTimeAdjust", speechBlend.lookaheadAdjust, SyncSampleTimeAdjust, -0.2f, 0.2f);
		sampleTimeAdjustJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(sampleTimeAdjustJSON);
		visemeMorphChangeRateJSON = new JSONStorableFloat("visemeMorphChangeRate", speechBlend.lipsBlendshapeChangeSpeed, SyncVisemeMorphChangeRate, 0f, 1f);
		visemeMorphChangeRateJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(visemeMorphChangeRateJSON);
		visemeMorphClampJSON = new JSONStorableFloat("visemeMorphClamp", speechBlend.blendshapeCutoff, SyncVisemeMorphClamp, 0f, 1f);
		visemeMorphClampJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(visemeMorphClampJSON);
		List<string> choicesList = new List<string>(Enum.GetNames(typeof(VoiceProfile.VoiceType)));
		voiceTypeJSON = new JSONStorableStringChooser("voiceType", choicesList, speechBlend.voiceType.ToString(), "Voice Type", SyncVoiceType);
		voiceTypeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(voiceTypeJSON);
		List<string> choicesList2 = new List<string>(Enum.GetNames(typeof(MorphSetType)));
		morphSetJSON = new JSONStorableStringChooser("morphSet", choicesList2, ((MorphSetType)speechBlend.setChoice/*cast due to .constrained prefix*/).ToString(), "Morph Set", SyncMorphSet);
		morphSetJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(morphSetJSON);
		openAdvancedPanelAction = new JSONStorableAction("Open Advanced", OpenAdvancedPanel);
		RegisterAction(openAdvancedPanelAction);
		if (speechBlend.faceBlendshapeNamesArray.Length == 3)
		{
			SpeechUtil.VisemeBlendshapeNames visemeBlendshapeNames = speechBlend.faceBlendshapeNamesArray[0];
			SpeechUtil.VisemeWeight visemeWeight = speechBlend.visemeWeightTuningArray[0];
			normalVisemeNamesJSON = new JSONStorableString[visemeBlendshapeNames.template.Nvis];
			normalMorphUidsJSON = new JSONStorableString[visemeBlendshapeNames.template.Nvis];
			normalVisemeWeightsJSON = new JSONStorableFloat[visemeBlendshapeNames.template.Nvis];
			for (int i = 0; i < normalVisemeNamesJSON.Length; i++)
			{
				string text = visemeBlendshapeNames.template.visemeNames[i];
				normalVisemeNamesJSON[i] = new JSONStorableString("normalVisemeName" + i, text);
				normalMorphUidsJSON[i] = new JSONStorableString("normalMorphUid" + text, visemeBlendshapeNames.visemeNames[i]);
				normalMorphUidsJSON[i].interactable = false;
				int index = i;
				normalVisemeWeightsJSON[i] = new JSONStorableFloat("normalVisemeWeight" + text, visemeWeight.weights[i], delegate(float f)
				{
					speechBlend.visemeWeightTuningArray[0].weights[index] = f;
				}, 0f, 2f);
				normalVisemeWeightsJSON[i].storeType = JSONStorableParam.StoreType.Physical;
				RegisterFloat(normalVisemeWeightsJSON[i]);
			}
			normalMouthOpenMorphUidJSON = new JSONStorableString("normalMouthOpenMorphUid", visemeBlendshapeNames.mouthOpenName);
			normalMouthOpenMorphUidJSON.interactable = false;
			visemeBlendshapeNames = speechBlend.faceBlendshapeNamesArray[1];
			visemeWeight = speechBlend.visemeWeightTuningArray[1];
			physicsVisemeNamesJSON = new JSONStorableString[visemeBlendshapeNames.template.Nvis];
			physicsMorphUidsJSON = new JSONStorableString[visemeBlendshapeNames.template.Nvis];
			physicsVisemeWeightsJSON = new JSONStorableFloat[visemeBlendshapeNames.template.Nvis];
			for (int j = 0; j < physicsVisemeNamesJSON.Length; j++)
			{
				string text2 = visemeBlendshapeNames.template.visemeNames[j];
				physicsVisemeNamesJSON[j] = new JSONStorableString("physicsVisemeName" + j, text2);
				physicsMorphUidsJSON[j] = new JSONStorableString("physicsMorphUid" + text2, visemeBlendshapeNames.visemeNames[j]);
				physicsMorphUidsJSON[j].interactable = false;
				int index2 = j;
				physicsVisemeWeightsJSON[j] = new JSONStorableFloat("physicsVisemeWeight" + text2, visemeWeight.weights[j], delegate(float f)
				{
					speechBlend.visemeWeightTuningArray[1].weights[index2] = f;
				}, 0f, 2f);
				physicsVisemeWeightsJSON[j].storeType = JSONStorableParam.StoreType.Physical;
				RegisterFloat(physicsVisemeWeightsJSON[j]);
			}
			physicsMouthOpenMorphUidJSON = new JSONStorableString("physicsMouthOpenMorphUid", visemeBlendshapeNames.mouthOpenName);
			physicsMouthOpenMorphUidJSON.interactable = false;
			visemeBlendshapeNames = speechBlend.faceBlendshapeNamesArray[2];
			visemeWeight = speechBlend.visemeWeightTuningArray[2];
			customVisemeNamesJSON = new JSONStorableString[visemeBlendshapeNames.template.Nvis];
			customMorphUidsJSON = new JSONStorableString[visemeBlendshapeNames.template.Nvis];
			pasteCustomMorphUidAction = new JSONStorableAction[visemeBlendshapeNames.template.Nvis];
			customVisemeWeightsJSON = new JSONStorableFloat[visemeBlendshapeNames.template.Nvis];
			for (int k = 0; k < customVisemeNamesJSON.Length; k++)
			{
				int index3 = k;
				string text3 = visemeBlendshapeNames.template.visemeNames[k];
				customVisemeNamesJSON[k] = new JSONStorableString("customVisemeName" + k, text3);
				customMorphUidsJSON[k] = new JSONStorableString("customMorphUid" + text3, visemeBlendshapeNames.visemeNames[k], delegate(string s)
				{
					speechBlend.faceBlendshapeNamesArray[2].visemeNames[index3] = s;
					speechBlend.InitMorphs();
				});
				RegisterString(customMorphUidsJSON[k]);
				pasteCustomMorphUidAction[k] = new JSONStorableAction("pasteCustomMorphUid" + text3, delegate
				{
					customMorphUidsJSON[index3].val = DAZMorph.uidCopy;
				});
				RegisterAction(pasteCustomMorphUidAction[k]);
				customVisemeWeightsJSON[k] = new JSONStorableFloat("customVisemeWeight" + text3, visemeWeight.weights[k], delegate(float f)
				{
					speechBlend.visemeWeightTuningArray[2].weights[index3] = f;
				}, 0f, 2f);
				customVisemeWeightsJSON[k].storeType = JSONStorableParam.StoreType.Physical;
				RegisterFloat(customVisemeWeightsJSON[k]);
			}
			customMouthOpenMorphUidJSON = new JSONStorableString("customMouthOpenMorphUid", visemeBlendshapeNames.mouthOpenName);
			customMouthOpenMorphUidJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterString(customMouthOpenMorphUidJSON);
			pasteCustomMouthOpenMorphUidAction = new JSONStorableAction("pasteCustomMouthMorphUid", delegate
			{
				customMouthOpenMorphUidJSON.val = DAZMorph.uidCopy;
			});
			RegisterAction(pasteCustomMouthOpenMorphUidAction);
			visemeFoundJSON = new JSONStorableBool[visemeBlendshapeNames.template.Nvis];
			visemeValuesJSON = new JSONStorableFloat[visemeBlendshapeNames.template.Nvis];
			visemeRawValuesJSON = new JSONStorableFloat[visemeBlendshapeNames.template.Nvis];
			for (int l = 0; l < visemeValuesJSON.Length; l++)
			{
				string text4 = visemeBlendshapeNames.template.visemeNames[l];
				visemeFoundJSON[l] = new JSONStorableBool("visemeFound" + text4, false);
				visemeFoundJSON[l].isStorable = false;
				visemeFoundJSON[l].isRestorable = false;
				RegisterBool(visemeFoundJSON[l]);
				visemeRawValuesJSON[l] = new JSONStorableFloat("visemeRawValue" + text4, 0f, 0f, 1f, true, false);
				visemeRawValuesJSON[l].isStorable = false;
				visemeRawValuesJSON[l].isRestorable = false;
				RegisterFloat(visemeRawValuesJSON[l]);
				visemeValuesJSON[l] = new JSONStorableFloat("visemeValue" + text4, 0f, 0f, 1f, true, false);
				visemeValuesJSON[l].isStorable = false;
				visemeValuesJSON[l].isRestorable = false;
				RegisterFloat(visemeValuesJSON[l]);
			}
			mouthOpenVisemeFoundJSON = new JSONStorableBool("mouthOpenVisemeFound", false);
			mouthOpenVisemeFoundJSON.isStorable = false;
			mouthOpenVisemeFoundJSON.isRestorable = false;
			RegisterBool(mouthOpenVisemeFoundJSON);
			mouthOpenVisemeValueJSON = new JSONStorableFloat("mouthOpenVisemeValue", 0f, 0f, 1f, true, false);
			mouthOpenVisemeValueJSON.isStorable = false;
			mouthOpenVisemeValueJSON.isRestorable = false;
			RegisterFloat(mouthOpenVisemeValueJSON);
			SyncMorphSetParams();
			resetAllAdvancedSettingsAction = new JSONStorableAction("ResetAllAdvancedSettings", ResetAllAdvancedSettings);
			RegisterAction(resetAllAdvancedSettingsAction);
		}
		else
		{
			Debug.LogError("Speech blend is not using exactly 3 sets");
		}
		if (audioSourceControl != null)
		{
			AudioSourceControl obj = audioSourceControl;
			obj.onMicStartHandlers = (AudioSourceControl.OnMicStart)Delegate.Combine(obj.onMicStartHandlers, (AudioSourceControl.OnMicStart)delegate
			{
				speechBlend.liveMode = true;
			});
			AudioSourceControl obj2 = audioSourceControl;
			obj2.onMicStopHandlers = (AudioSourceControl.OnMicStop)Delegate.Combine(obj2.onMicStopHandlers, (AudioSourceControl.OnMicStop)delegate
			{
				speechBlend.liveMode = false;
			});
		}
	}

	protected void Update()
	{
		if (!(speechBlend != null) || volumeJSON == null)
		{
			return;
		}
		volumeJSON.val = speechBlend.current_volume;
		maxVolumeJSON.val = speechBlend.recent_max_volume;
		if (speechBlend.VisemeValues != null)
		{
			for (int i = 0; i < visemeValuesJSON.Length; i++)
			{
				visemeValuesJSON[i].val = speechBlend.VisemeValues[i];
				visemeRawValuesJSON[i].val = speechBlend.VisemeRawValues[i];
			}
		}
		mouthOpenVisemeValueJSON.val = speechBlend.MouthOpenValue;
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
