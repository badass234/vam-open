using System;
using System.Threading;
using SpeechBlendEngine;
using UnityEngine;

public class SpeechBlend : MonoBehaviour
{
	protected class SpeechBlendTaskInfo
	{
		public string name;

		public AutoResetEvent resetEvent;

		public Thread thread;

		public volatile bool working;

		public volatile bool kill;
	}

	private struct ValueAndIndex
	{
		public int index;

		public float value;
	}

	public AudioSource voiceAudioSource;

	public SkinnedMeshRenderer headMesh;

	[HideInInspector]
	public bool showBlendShapeMenu;

	[HideInInspector]
	public SpeechUtil.VisemeBlendshapeIndexes faceBlendshapes;

	[HideInInspector]
	public SpeechUtil.VisemeWeight visemeWeightTuning;

	[Header("Settings")]
	[Space(10f)]
	[Tooltip("Toggle lipsyncing")]
	public bool lipsyncActive = true;

	[Tooltip("Select whether visemes are used")]
	public SpeechUtil.Mode trackingMode;

	[Tooltip("Volume multiplier")]
	[Range(0f, 10f)]
	public float volumeMultiplier = 1f;

	[Tooltip("Minimum volume before lip sync activates")]
	[Range(0f, 1f)]
	public float volumeThreshold;

	[Tooltip("Clamp maximum volume to prevent shape overshoot")]
	[Range(0f, 1f)]
	public float volumeClamp = 1f;

	[Tooltip("Amplitude of jaw movement")]
	[Range(0f, 1f)]
	public float jawMovementAmount = 0.5f;

	[Tooltip("Jaw motion speed")]
	[Range(0f, 1f)]
	public float jawMovementSpeed = 0.5f;

	[Tooltip("Amplitude of lip movement")]
	[Range(0f, 1f)]
	public float lipsBlendshapeMovementAmount = 0.5f;

	[Tooltip("Lip viseme movement speed")]
	[Range(0f, 1f)]
	public float lipsBlendshapeChangeSpeed = 0.5f;

	[Range(0f, 1f)]
	public float visemeThreshold;

	[Range(0.5f, 2f)]
	public float blendshapeMultiplier = 1f;

	[Range(0f, 1f)]
	public float blendshapeCutoff = 1f;

	[Range(1f, 5f)]
	public int maxSimultaneousVisemes = 5;

	public bool liveMode;

	public bool useInterpolation = true;

	protected bool hasClip;

	public bool useUnitySpectrum = true;

	public bool useHamming = true;

	public bool useLookahead = true;

	[Range(-0.2f, 0.2f)]
	public float lookaheadAdjust;

	public AnimationCurve interpolationCurve;

	[Tooltip("Number of calculations to use.")]
	public SpeechUtil.Accuracy accuracy = SpeechUtil.Accuracy.Medium;

	[Range(0f, 1f)]
	public float timeBetweenSampling = 0.1f;

	[Tooltip("Ignore distance between AudioSource and AudioListener when accounting for volume.")]
	public bool volumeEqualization;

	[Tooltip("Voice type of character")]
	public VoiceProfile.VoiceType voiceType = VoiceProfile.VoiceType.female;

	[Tooltip("Jaw joint for when not using a mouth open blendshape")]
	public Transform jawJoint;

	[Tooltip("Direction adjust for jaw opening")]
	public Vector3 jawOpenDirection = new Vector3(1f, 0f, 0f);

	[Tooltip("Angular offset for jaw joint opening")]
	public Vector3 jawJointOffset;

	[Tooltip("Blendshape template for visemes shapes. (default: DAZ)")]
	public VoiceProfile.VisemeBlendshapeTemplate shapeTemplate;

	public AudioListener activeListener;

	protected DAZMorph[] visemeMorphs;

	protected DAZMorph mouthOpenMorph;

	[SerializeField]
	protected bool _useMorphBank;

	public bool useClampChangeMethod = true;

	[SerializeField]
	protected bool _useBuiltInMorphs;

	[SerializeField]
	[HideInInspector]
	protected DAZMorphBank _morphBank;

	[SerializeField]
	[HideInInspector]
	protected int _setChoice;

	public int numberOfBlendShapeSets = 1;

	[HideInInspector]
	public SpeechUtil.VisemeBlendshapeNames[] faceBlendshapeNamesArray;

	[HideInInspector]
	public SpeechUtil.VisemeWeight[] visemeWeightTuningArray;

	private float bs_volume_scaling = 20f;

	private float jaw_volume_scaling = 20f;

	private int f_low;

	private int f_high;

	private float fres;

	private ExtractFeatures extractFeatures;

	private float[,] extractor;

	private float[,] transformer;

	private float[] modifier;

	private float[] bs_setpoint;

	private float[] bs_setpoint_last;

	private ValueAndIndex[] bs_setpoint_for_sort;

	private float[,] cmem;

	private float bs_mouthOpen_setpoint;

	private Quaternion trans_mouthOpen_setpoint;

	private Quaternion trans_mouthOpen_rest;

	public int sample_count = 256;

	private float calculated_volume;

	public float current_volume;

	public float speech_volume;

	public float recent_max_volume;

	private float normalize_mult = 1f;

	[HideInInspector]
	public VoiceProfile.VisemeBlendshapeTemplate template_saved;

	[HideInInspector]
	public SpeechUtil.VisemeBlendshapeIndexes faceBlendshapes_saved = new SpeechUtil.VisemeBlendshapeIndexes(VoiceProfile.G2_template);

	[HideInInspector]
	public SpeechUtil.VisemeWeight visemeWeightTuning_saved = new SpeechUtil.VisemeWeight(VoiceProfile.G2_template);

	private float jaw_CSF = 1f;

	private float bs_CSF = 1f;

	private int updateFrame;

	private SpeechUtil.Accuracy accuracy_last;

	private bool[] blendshapeInfluenceActive;

	protected bool taskEverRun;

	protected SpeechBlendTaskInfo speechBlendTask;

	protected bool _threadsRunning;

	private float[] audioTrace;

	private float[] influences;

	private float clipFrequency;

	private float[] soundData;

	private float[] spectrumData;

	private float[] rawData;

	private float timeToResetMaxVolume = 1f;

	private float timeSinceLastMaxVolume;

	private bool wasPlaying;

	private float timeSinceLastSample;

	private float lastFixedUpdateTime;

	protected bool useUnitySpectrumMethod => useUnitySpectrum || liveMode || !hasClip;

	public DAZMorph[] VisemeMorphs => visemeMorphs;

	public float[] VisemeRawValues { get; protected set; }

	public float[] VisemeValues { get; protected set; }

	public DAZMorph MouthOpenMorph => mouthOpenMorph;

	public float MouthOpenValue { get; protected set; }

	public bool useMorphBank
	{
		get
		{
			return _useMorphBank;
		}
		set
		{
			if (_useMorphBank != value)
			{
				_useMorphBank = value;
				if (_useMorphBank)
				{
					InitMorphs();
				}
				else
				{
					ClearMorphs();
				}
			}
		}
	}

	public bool useBuiltInMorphs
	{
		get
		{
			return _useBuiltInMorphs;
		}
		set
		{
			if (_useBuiltInMorphs != value)
			{
				_useBuiltInMorphs = value;
				InitMorphs();
			}
		}
	}

	public DAZMorphBank morphBank
	{
		get
		{
			return _morphBank;
		}
		set
		{
			if (_morphBank != value)
			{
				_morphBank = value;
				InitMorphs();
			}
		}
	}

	public int setChoice
	{
		get
		{
			return _setChoice;
		}
		set
		{
			if (_setChoice != value)
			{
				_setChoice = value;
				InitMorphs();
			}
		}
	}

	private float GetLookaheadTime()
	{
		float result = 0f;
		if (useLookahead)
		{
			int num = Mathf.FloorToInt(timeBetweenSampling / Time.fixedDeltaTime) + 1;
			float num2 = (float)(num + 2) * Time.fixedDeltaTime;
			result = num2 + lookaheadAdjust;
		}
		return result;
	}

	public void InitMorphs()
	{
		ClearMorphs();
		if (!(_morphBank != null))
		{
			return;
		}
		_morphBank.Init();
		SpeechUtil.VisemeBlendshapeNames visemeBlendshapeNames = faceBlendshapeNamesArray[setChoice];
		if (visemeBlendshapeNames != null)
		{
			if (visemeMorphs == null || visemeMorphs.Length != visemeBlendshapeNames.visemeNames.Length)
			{
				visemeMorphs = new DAZMorph[visemeBlendshapeNames.visemeNames.Length];
			}
			if (VisemeRawValues == null || VisemeRawValues.Length != visemeBlendshapeNames.visemeNames.Length)
			{
				VisemeRawValues = new float[visemeBlendshapeNames.visemeNames.Length];
			}
			if (VisemeValues == null || VisemeValues.Length != visemeBlendshapeNames.visemeNames.Length)
			{
				VisemeValues = new float[visemeBlendshapeNames.visemeNames.Length];
			}
			for (int i = 0; i < visemeBlendshapeNames.visemeNames.Length; i++)
			{
				if (string.IsNullOrEmpty(visemeBlendshapeNames.visemeNames[i]))
				{
					visemeMorphs[i] = null;
					Debug.LogError("Morph not set for viseme " + visemeBlendshapeNames.template.visemeNames[i]);
					continue;
				}
				if (_useBuiltInMorphs)
				{
					visemeMorphs[i] = _morphBank.GetBuiltInMorphByUid(visemeBlendshapeNames.visemeNames[i]);
				}
				else
				{
					visemeMorphs[i] = _morphBank.GetMorphByUid(visemeBlendshapeNames.visemeNames[i]);
				}
				if (visemeMorphs[i] == null)
				{
					Debug.LogError("Could not find morph " + visemeBlendshapeNames.visemeNames[i] + " for viseme " + visemeBlendshapeNames.template.visemeNames[i]);
				}
			}
			if (string.IsNullOrEmpty(visemeBlendshapeNames.mouthOpenName))
			{
				Debug.LogError("Morph not set for mouth open");
				return;
			}
			if (_useBuiltInMorphs)
			{
				mouthOpenMorph = _morphBank.GetBuiltInMorphByUid(visemeBlendshapeNames.mouthOpenName);
			}
			else
			{
				mouthOpenMorph = _morphBank.GetMorphByUid(visemeBlendshapeNames.mouthOpenName);
			}
			if (mouthOpenMorph == null)
			{
				Debug.LogError("Could not find mouth open morph " + visemeBlendshapeNames.mouthOpenName);
			}
		}
		else
		{
			visemeMorphs = null;
		}
	}

	protected void ClearMorphs()
	{
		if (visemeMorphs != null)
		{
			for (int i = 0; i < visemeMorphs.Length; i++)
			{
				if (visemeMorphs[i] != null)
				{
					visemeMorphs[i].morphValue = 0f;
				}
			}
			visemeMorphs = null;
		}
		if (mouthOpenMorph != null)
		{
			mouthOpenMorph.morphValue = 0f;
			mouthOpenMorph = null;
		}
	}

	protected void MTTask(object info)
	{
		SpeechBlendTaskInfo speechBlendTaskInfo = (SpeechBlendTaskInfo)info;
		while (_threadsRunning)
		{
			speechBlendTaskInfo.resetEvent.WaitOne(-1, true);
			if (speechBlendTaskInfo.kill)
			{
				break;
			}
			try
			{
				if (!useUnitySpectrumMethod && soundData != null)
				{
					current_volume = extractFeatures.GetVolume(soundData);
					current_volume *= volumeMultiplier;
					if (volumeThreshold > 0f && current_volume < volumeThreshold)
					{
						current_volume = 0f;
					}
					current_volume = Mathf.Clamp(current_volume, 0f, volumeClamp);
					spectrumData = extractFeatures.ConvertSoundDataToSpectrumData();
				}
				float num = 85f;
				float num2 = 450f;
				int num3 = Mathf.FloorToInt(num / clipFrequency * (float)spectrumData.Length);
				int num4 = Mathf.CeilToInt(num2 / clipFrequency * (float)spectrumData.Length);
				speech_volume = 0f;
				float num5 = 0f;
				int num6 = 0;
				int num7 = 0;
				for (int i = 0; i < spectrumData.Length; i++)
				{
					if (i >= num3 && i <= num4)
					{
						num6++;
						speech_volume += spectrumData[i];
					}
					else
					{
						num7++;
						num5 += spectrumData[i];
					}
				}
				speech_volume /= num6;
				num5 /= (float)num7;
				float[] mfccs = extractFeatures.ExtractSample(spectrumData, extractor, transformer, modifier, ref cmem, f_low, f_high, accuracy);
				extractFeatures.Evaluate(mfccs, voiceType, accuracy);
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception in thread " + ex);
			}
			taskEverRun = true;
			speechBlendTaskInfo.working = false;
		}
	}

	protected void StopThreads()
	{
		_threadsRunning = false;
		if (speechBlendTask != null)
		{
			speechBlendTask.kill = true;
			speechBlendTask.resetEvent.Set();
			while (speechBlendTask.thread.IsAlive)
			{
			}
			taskEverRun = false;
			speechBlendTask = null;
		}
	}

	protected void StartThreads()
	{
		if (!_threadsRunning)
		{
			_threadsRunning = true;
			speechBlendTask = new SpeechBlendTaskInfo();
			speechBlendTask.name = "SpeechBlendTask";
			speechBlendTask.resetEvent = new AutoResetEvent(false);
			speechBlendTask.thread = new Thread(MTTask);
			speechBlendTask.thread.Priority = System.Threading.ThreadPriority.Normal;
			speechBlendTask.thread.Start(speechBlendTask);
		}
	}

	private void OnEnable()
	{
		StartThreads();
		InitMorphs();
	}

	private void OnDisable()
	{
		StopThreads();
		ClearMorphs();
	}

	private void Start()
	{
		normalize_mult = 1f / (float)sample_count;
		int num = ((!_useMorphBank) ? faceBlendshapes.template.Nvis : faceBlendshapeNamesArray[_setChoice].template.Nvis);
		bs_setpoint = new float[num];
		bs_setpoint_last = new float[num];
		bs_setpoint_for_sort = new ValueAndIndex[num];
		if (jawJoint != null)
		{
			trans_mouthOpen_setpoint = jawJoint.localRotation;
		}
		trans_mouthOpen_rest = trans_mouthOpen_setpoint;
		bs_mouthOpen_setpoint = 0f;
		accuracy_last = accuracy;
		fres = ExtractFeatures.CalculateFres();
		UpdateExtractor();
		if (_useMorphBank)
		{
			if (jawJoint == null && !faceBlendshapeNamesArray[_setChoice].AnyAssigned())
			{
				MonoBehaviour.print("Warning (SpeechBlend): Neither jaw joint or face blendshapes have been assigned");
				lipsyncActive = false;
			}
			if (trackingMode.Equals(SpeechUtil.Mode.jawAndVisemes) && faceBlendshapeNamesArray[_setChoice].JawOnly())
			{
				MonoBehaviour.print("Warning (SpeechBlend): No viseme blendshapes detected, jaw-only mode enabled.");
				trackingMode = SpeechUtil.Mode.jawOnly;
			}
			else if (trackingMode.Equals(SpeechUtil.Mode.jawAndVisemes))
			{
				blendshapeInfluenceActive = new bool[num];
			}
		}
		else
		{
			if (jawJoint == null && !faceBlendshapes.AnyAssigned())
			{
				MonoBehaviour.print("Warning (SpeechBlend): Neither jaw joint or face blendshapes have been assigned");
				lipsyncActive = false;
			}
			if (trackingMode.Equals(SpeechUtil.Mode.jawAndVisemes) && faceBlendshapes.JawOnly())
			{
				MonoBehaviour.print("Warning (SpeechBlend): No viseme blendshapes detected, jaw-only mode enabled.");
				trackingMode = SpeechUtil.Mode.jawOnly;
			}
			else if (trackingMode.Equals(SpeechUtil.Mode.jawAndVisemes))
			{
				blendshapeInfluenceActive = new bool[num];
			}
		}
	}

	private void FixedUpdate()
	{
		bool flag = voiceAudioSource != null && voiceAudioSource.isPlaying;
		if (flag && lipsyncActive)
		{
			bs_volume_scaling = 10f * Mathf.Exp(2f * (lipsBlendshapeMovementAmount - 0.5f));
			jaw_volume_scaling = 10f * jawMovementAmount;
			updateFrame++;
			timeSinceLastSample += Time.fixedDeltaTime;
			if (timeSinceLastSample >= timeBetweenSampling)
			{
				lastFixedUpdateTime = Time.fixedTime;
				if (taskEverRun)
				{
					while (speechBlendTask.working)
					{
						Thread.Sleep(0);
					}
				}
				hasClip = voiceAudioSource.clip != null;
				if (useUnitySpectrumMethod || trackingMode != SpeechUtil.Mode.jawAndVisemes)
				{
					current_volume = 0f;
					if (audioTrace == null)
					{
						audioTrace = new float[sample_count];
					}
					voiceAudioSource.GetOutputData(audioTrace, 0);
					for (int i = 0; i < sample_count; i++)
					{
						current_volume += Mathf.Abs(audioTrace[i]);
					}
					current_volume *= normalize_mult;
					if (volumeEqualization)
					{
						if (SuperController.singleton != null)
						{
							current_volume = ExtractFeatures.EqualizeDistance(current_volume, voiceAudioSource, SuperController.singleton.CurrentAudioListener);
						}
						else
						{
							current_volume = ExtractFeatures.EqualizeDistance(current_volume, voiceAudioSource, activeListener);
						}
					}
					current_volume *= volumeMultiplier;
					if (volumeThreshold > 0f && current_volume < volumeThreshold)
					{
						current_volume = 0f;
					}
					current_volume = Mathf.Clamp(current_volume, 0f, volumeClamp);
				}
				if (current_volume > recent_max_volume)
				{
					recent_max_volume = current_volume;
					timeSinceLastMaxVolume = 0f;
				}
				timeSinceLastMaxVolume += timeSinceLastSample;
				if (timeSinceLastMaxVolume > timeToResetMaxVolume)
				{
					recent_max_volume = current_volume;
					timeSinceLastMaxVolume = 0f;
				}
				if (_useMorphBank)
				{
					bs_mouthOpen_setpoint = current_volume * jaw_volume_scaling * 0.1f * (1f / jaw_CSF);
				}
				else
				{
					bs_mouthOpen_setpoint = 100f * current_volume * jaw_volume_scaling * 0.1f * (1f / jaw_CSF);
				}
				if (jawJoint != null)
				{
					trans_mouthOpen_setpoint = Quaternion.Euler(jawJointOffset + trans_mouthOpen_rest.eulerAngles * (1f - jawMovementAmount * 3f) + (trans_mouthOpen_rest.eulerAngles + jawOpenDirection * current_volume * jaw_volume_scaling) * jawMovementAmount * 3f);
				}
				if (trackingMode == SpeechUtil.Mode.jawAndVisemes)
				{
					f_low = Mathf.RoundToInt((float)ExtractFeatures.getlf(accuracy) / fres);
					f_high = Mathf.RoundToInt((float)ExtractFeatures.gethf(accuracy) / fres);
					if (accuracy_last != accuracy)
					{
						UpdateExtractor();
					}
					accuracy_last = accuracy;
					if (hasClip)
					{
						clipFrequency = voiceAudioSource.clip.frequency;
					}
					if (!taskEverRun)
					{
						spectrumData = extractFeatures.GetUnitySpectrumData(voiceAudioSource, useHamming);
						float[] mfccs = extractFeatures.ExtractSample(spectrumData, extractor, transformer, modifier, ref cmem, f_low, f_high, accuracy);
						extractFeatures.Evaluate(mfccs, voiceType, accuracy);
					}
					else if (useUnitySpectrumMethod)
					{
						spectrumData = extractFeatures.GetUnitySpectrumData(voiceAudioSource, useHamming);
					}
					if (!useUnitySpectrumMethod)
					{
						soundData = extractFeatures.GetUnitySoundData(voiceAudioSource, GetLookaheadTime());
					}
					if (influences == null || influences.Length != ExtractFeatures.no_visemes)
					{
						influences = new float[ExtractFeatures.no_visemes];
					}
					else
					{
						for (int j = 0; j < influences.Length; j++)
						{
							influences[j] = 0f;
						}
					}
					for (int k = 0; k < extractFeatures.featureOutput.size; k++)
					{
						for (int l = 0; l < ExtractFeatures.no_visemes; l++)
						{
							influences[l] += VoiceProfile.Influence(voiceType, extractFeatures.featureOutput.reg[k], l, accuracy) * extractFeatures.featureOutput.w[k];
						}
					}
					float[] array = VoiceProfile.InfluenceTemplateTransform(influences, shapeTemplate);
					int num = ((!_useMorphBank) ? faceBlendshapes.template.Nvis : faceBlendshapeNamesArray[setChoice].template.Nvis);
					if (blendshapeInfluenceActive == null || blendshapeInfluenceActive.Length != num)
					{
						blendshapeInfluenceActive = new bool[num];
					}
					SpeechUtil.VisemeWeight visemeWeight = ((!_useMorphBank) ? visemeWeightTuning : visemeWeightTuningArray[setChoice]);
					for (int m = 0; m < num; m++)
					{
						float byIndex = visemeWeight.GetByIndex(m);
						array[m] *= byIndex;
						if ((double)byIndex < 0.01)
						{
							blendshapeInfluenceActive[m] = false;
						}
						else
						{
							blendshapeInfluenceActive[m] = true;
						}
					}
					if (_useMorphBank)
					{
						ValueAndIndex valueAndIndex = default(ValueAndIndex);
						for (int n = 0; n < num; n++)
						{
							bs_setpoint_last[n] = bs_setpoint[n];
							bs_setpoint[n] = array[n] * current_volume * bs_volume_scaling;
							if (bs_setpoint[n] < visemeThreshold)
							{
								bs_setpoint[n] = 0f;
							}
							valueAndIndex.value = bs_setpoint[n];
							valueAndIndex.index = n;
							bs_setpoint_for_sort[n] = valueAndIndex;
						}
					}
					else
					{
						ValueAndIndex valueAndIndex2 = default(ValueAndIndex);
						for (int num2 = 0; num2 < num; num2++)
						{
							bs_setpoint_last[num2] = bs_setpoint[num2];
							bs_setpoint[num2] = array[num2] * 100f * current_volume * bs_volume_scaling;
							if (bs_setpoint[num2] < visemeThreshold)
							{
								bs_setpoint[num2] = 0f;
							}
							valueAndIndex2.value = bs_setpoint[num2];
							valueAndIndex2.index = num2;
							bs_setpoint_for_sort[num2] = valueAndIndex2;
						}
					}
					Array.Sort(bs_setpoint_for_sort, (ValueAndIndex a, ValueAndIndex b) => b.value.CompareTo(a.value));
					for (int num3 = 0; num3 < bs_setpoint_for_sort.Length; num3++)
					{
						if (num3 >= maxSimultaneousVisemes)
						{
							bs_setpoint[bs_setpoint_for_sort[num3].index] = 0f;
						}
					}
					bs_CSF = VoiceProfile.Influence(voiceType, extractFeatures.featureOutput.reg[0], ExtractFeatures.no_visemes, accuracy);
					jaw_CSF = VoiceProfile.Influence(voiceType, extractFeatures.featureOutput.reg[0], ExtractFeatures.no_visemes, accuracy);
					bs_mouthOpen_setpoint /= VoiceProfile.Influence(voiceType, extractFeatures.featureOutput.reg[0], ExtractFeatures.no_visemes, accuracy);
					speechBlendTask.working = true;
					speechBlendTask.resetEvent.Set();
				}
				timeSinceLastSample = 0f;
				updateFrame = 0;
			}
			else if (trackingMode == SpeechUtil.Mode.jawAndVisemes)
			{
				hasClip = voiceAudioSource.clip != null;
				if (hasClip)
				{
					clipFrequency = voiceAudioSource.clip.frequency;
				}
				if (!taskEverRun)
				{
					spectrumData = extractFeatures.GetUnitySpectrumData(voiceAudioSource, useHamming);
					float[] mfccs2 = extractFeatures.ExtractSample(spectrumData, extractor, transformer, modifier, ref cmem, f_low, f_high, accuracy);
					extractFeatures.Evaluate(mfccs2, voiceType, accuracy);
				}
				else
				{
					while (speechBlendTask.working)
					{
						Thread.Sleep(0);
					}
					if (useUnitySpectrumMethod)
					{
						spectrumData = extractFeatures.GetUnitySpectrumData(voiceAudioSource, useHamming);
					}
					else
					{
						soundData = extractFeatures.GetUnitySoundData(voiceAudioSource, GetLookaheadTime());
					}
				}
				speechBlendTask.working = true;
				speechBlendTask.resetEvent.Set();
			}
		}
		else if (wasPlaying)
		{
			int num4 = ((!_useMorphBank) ? faceBlendshapes.template.Nvis : faceBlendshapeNamesArray[setChoice].template.Nvis);
			for (int num5 = 0; num5 < num4; num5++)
			{
				bs_setpoint_last[num5] = 0f;
				bs_setpoint[num5] = 0f;
			}
			bs_mouthOpen_setpoint = 0f;
			current_volume = 0f;
			recent_max_volume = 0f;
			timeSinceLastMaxVolume = 0f;
		}
		wasPlaying = flag;
	}

	private void LateUpdate()
	{
		if (_useMorphBank)
		{
			if (!faceBlendshapeNamesArray[setChoice].MouthOpenBlendshapeAssigned() && jawJoint != null)
			{
				float t = 2.5f * Mathf.Exp(3.658f * jawMovementSpeed);
				jawJoint.transform.localRotation = Quaternion.Lerp(jawJoint.transform.localRotation, trans_mouthOpen_setpoint, t);
			}
		}
		else if (!faceBlendshapes.MouthOpenBlendshapeAssigned() && jawJoint != null)
		{
			float t2 = 2.5f * Mathf.Exp(3.658f * jawMovementSpeed);
			jawJoint.transform.localRotation = Quaternion.Lerp(jawJoint.transform.localRotation, trans_mouthOpen_setpoint, t2);
		}
		UpdateBlendshapes();
	}

	private void UpdateBlendshapes()
	{
		float num = Time.deltaTime * 15f;
		float t = 1f;
		if (useInterpolation && !liveMode)
		{
			int num2 = Mathf.FloorToInt(timeBetweenSampling / Time.fixedDeltaTime) + 1;
			float num3 = (float)num2 * Time.fixedDeltaTime;
			float num4 = Time.time - lastFixedUpdateTime;
			t = Mathf.Clamp01(interpolationCurve.Evaluate(num4 / num3));
		}
		if (_useMorphBank)
		{
			if (trackingMode == SpeechUtil.Mode.jawAndVisemes && visemeMorphs != null && blendshapeInfluenceActive != null)
			{
				for (int i = 0; i < visemeMorphs.Length; i++)
				{
					if (visemeMorphs[i] == null)
					{
						continue;
					}
					if (i < blendshapeInfluenceActive.Length && blendshapeInfluenceActive[i])
					{
						float num5 = visemeMorphs[i].morphValue / blendshapeMultiplier;
						float num6 = ((!useInterpolation || liveMode) ? bs_setpoint[i] : Mathf.Lerp(bs_setpoint_last[i], bs_setpoint[i], t));
						VisemeRawValues[i] = num6;
						if (useClampChangeMethod)
						{
							float value = num6 - num5;
							float num7 = lipsBlendshapeChangeSpeed * num * bs_CSF;
							float num8 = Mathf.Clamp(value, 0f - num7, num7);
							VisemeValues[i] = Mathf.Clamp((num5 + num8) * blendshapeMultiplier, 0f, blendshapeCutoff);
						}
						else
						{
							VisemeValues[i] = Mathf.Clamp(Mathf.Lerp(num5, num6 * blendshapeMultiplier, lipsBlendshapeChangeSpeed * num * blendshapeMultiplier), 0f, blendshapeCutoff);
						}
						visemeMorphs[i].morphValue = VisemeValues[i];
					}
					else
					{
						VisemeRawValues[i] = 0f;
						VisemeValues[i] = 0f;
						visemeMorphs[i].morphValue = 0f;
					}
				}
			}
			if (mouthOpenMorph != null)
			{
				float morphValue = mouthOpenMorph.morphValue;
				float t2 = jawMovementSpeed * jaw_CSF * num * 2f;
				float value2 = Mathf.Lerp(morphValue, bs_mouthOpen_setpoint, t2);
				MouthOpenValue = Mathf.Clamp(value2, 0f, 1f);
				mouthOpenMorph.morphValue = MouthOpenValue;
			}
			return;
		}
		if (trackingMode == SpeechUtil.Mode.jawAndVisemes)
		{
			for (int j = 0; j < faceBlendshapes.template.Nvis; j++)
			{
				if (faceBlendshapes.BlendshapeAssigned(j) & blendshapeInfluenceActive[j])
				{
					float blendShapeWeight = headMesh.GetBlendShapeWeight(faceBlendshapes.GetByIndex(j));
					float num9 = bs_setpoint[j];
					float value3 = num9 - blendShapeWeight;
					float num10 = lipsBlendshapeChangeSpeed * num * 20f * bs_CSF;
					float num11 = Mathf.Clamp(value3, 0f - num10, num10);
					float value4 = Mathf.Clamp(blendShapeWeight + num11, 0f, 100f);
					headMesh.SetBlendShapeWeight(faceBlendshapes.GetByIndex(j), value4);
				}
			}
		}
		if (faceBlendshapes.MouthOpenBlendshapeAssigned())
		{
			float blendShapeWeight2 = headMesh.GetBlendShapeWeight(faceBlendshapes.mouthOpenIndex);
			float t3 = jawMovementSpeed * jaw_CSF * num * 2f;
			headMesh.SetBlendShapeWeight(faceBlendshapes.mouthOpenIndex, Mathf.Lerp(blendShapeWeight2, bs_mouthOpen_setpoint, t3));
		}
	}

	public void UpdateExtractor()
	{
		extractFeatures = new ExtractFeatures();
		extractor = ExtractFeatures.BuildExtractor(fres, ExtractFeatures.getlf(accuracy), ExtractFeatures.gethf(accuracy), accuracy);
		cmem = new float[ExtractFeatures.getC(accuracy) + 1, 2];
		modifier = ExtractFeatures.CreateCC_lifter(accuracy);
		transformer = ExtractFeatures.GenerateTransformer(accuracy);
		f_low = Mathf.RoundToInt((float)ExtractFeatures.getlf(accuracy) / fres);
		f_high = Mathf.RoundToInt((float)ExtractFeatures.gethf(accuracy) / fres);
	}
}
