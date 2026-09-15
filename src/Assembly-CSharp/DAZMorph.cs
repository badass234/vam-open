using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using MeshVR;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DAZMorph
{
	private const float geoScale = 0.01f;

	public bool visible;

	public bool preserveValueOnReimport;

	public bool disable;

	public bool isPoseControl;

	protected string _uid;

	public string morphName;

	public string displayName;

	public string overrideName;

	protected string _lowerCaseResolvedDisplayName;

	public string region;

	public string overrideRegion;

	public string group;

	public float importValue;

	public float startValue;

	[SerializeField]
	private float _morphValue;

	public bool active;

	protected RectTransform drivenIndicator;

	protected Text drivenByText;

	protected bool _isDriven;

	protected string _drivenBy;

	public float appliedValue;

	public float min;

	protected float startingMin;

	public float max;

	protected float startingMax;

	public int numDeltas;

	public bool triggerNormalRecalc = true;

	public bool triggerTangentRecalc = true;

	public DAZMorphVertex[] deltas;

	public DAZMorphFormula[] formulas;

	protected string _formulasString;

	protected bool _hasFormulas;

	protected bool _hasBoneModificationFormulas;

	protected bool _hasBoneRotationFormulas;

	protected bool _hasMorphValueFormulas;

	protected bool _hasMCMFormulas;

	protected bool _wasZeroedKeepChildValues;

	protected Button _zeroKeepChildValuesButton;

	public static string uidCopy = string.Empty;

	protected Button _copyUidButton;

	[NonSerialized]
	public JSONStorableFloat jsonFloat;

	protected RectTransform _animatableWarningIndicator;

	protected RectTransform _animatableWarningIndicatorAlt;

	protected DAZMorphBank _morphBank;

	protected DAZMorphSubBank _morphSubBank;

	protected static Color highCostColor = Color.red;

	protected static Color normalColor = Color.white;

	protected static Color transientColor = new Color(1f, 1f, 0.8f);

	protected static Color runtimeColor = new Color(0.8f, 0.8f, 1f);

	protected static Color packageColor = new Color(1f, 0.85f, 0.9f);

	protected Image _panelImage;

	protected Toggle _favoriteToggle;

	protected bool _favorite;

	public bool isTransient;

	public bool isRuntime;

	public bool isDemandLoaded;

	public bool isDemandActivated;

	public string packageUid;

	public string packageLicense;

	public bool isInPackage;

	public bool isLatestVersion = true;

	public string version;

	public string metaLoadPath = string.Empty;

	protected Button _openInPackageManagerButton;

	protected Button _openInPackageManagerButtonAlt;

	protected Text _packageUidText;

	protected Text _packageUidTextAlt;

	protected Text _packageLicenseText;

	protected Text _packageLicenseTextAlt;

	protected Text _versionText;

	protected Text _versionTextAlt;

	protected Button _increaseRangeButton;

	protected Button _increaseRangeButtonAlt;

	protected Button _resetRangeButton;

	protected Button _resetRangeButtonAlt;

	public string metaJSONFile;

	public bool deltasLoaded;

	public string deltasLoadPath = string.Empty;

	public string uid
	{
		get
		{
			if (_uid != null && _uid != string.Empty)
			{
				return _uid;
			}
			return resolvedDisplayName;
		}
		set
		{
			_uid = value;
		}
	}

	public string resolvedDisplayName
	{
		get
		{
			if (overrideName != null && overrideName != string.Empty)
			{
				return overrideName;
			}
			return displayName;
		}
	}

	public string lowerCaseResolvedDisplayName
	{
		get
		{
			if (_lowerCaseResolvedDisplayName == null)
			{
				if (resolvedDisplayName != null)
				{
					_lowerCaseResolvedDisplayName = resolvedDisplayName.ToLower();
				}
				else
				{
					_lowerCaseResolvedDisplayName = string.Empty;
				}
			}
			return _lowerCaseResolvedDisplayName;
		}
	}

	public string resolvedRegionName
	{
		get
		{
			if (overrideRegion != null && overrideRegion != string.Empty)
			{
				return overrideRegion;
			}
			if (morphSubBank != null && morphSubBank.useOverrideRegionName)
			{
				return morphSubBank.overrideRegionName;
			}
			return region;
		}
	}

	public float morphValue
	{
		get
		{
			return _morphValue;
		}
		set
		{
			if (jsonFloat != null)
			{
				jsonFloat.val = value;
				return;
			}
			_morphValue = value;
			activeImmediate = _morphValue != 0f;
		}
	}

	public float morphValueAdjustLimits
	{
		get
		{
			return _morphValue;
		}
		set
		{
			AdjustRange(value);
			if (jsonFloat != null)
			{
				jsonFloat.val = value;
				return;
			}
			_morphValue = value;
			activeImmediate = _morphValue != 0f;
		}
	}

	public bool activeImmediate { get; protected set; }

	public bool isDriven => _isDriven;

	public string drivenBy => _drivenBy;

	public string formulasString => _formulasString;

	public bool hasFormulas => _hasFormulas;

	public bool hasBoneModificationFormulas => _hasBoneModificationFormulas;

	public bool hasBoneRotationFormulas => _hasBoneRotationFormulas;

	public bool hasMorphValueFormulas => _hasMorphValueFormulas;

	public bool hasMCMFormulas => _hasMCMFormulas;

	public bool wasZeroedKeepChildValues
	{
		get
		{
			return _wasZeroedKeepChildValues;
		}
		set
		{
			_wasZeroedKeepChildValues = value;
		}
	}

	public Button zeroKeepChildValuesButton
	{
		get
		{
			return _zeroKeepChildValuesButton;
		}
		set
		{
			if (_zeroKeepChildValuesButton != value)
			{
				if (_zeroKeepChildValuesButton != null)
				{
					_zeroKeepChildValuesButton.onClick.RemoveListener(ZeroKeepChildValues);
				}
				_zeroKeepChildValuesButton = value;
				if (_zeroKeepChildValuesButton != null)
				{
					_zeroKeepChildValuesButton.gameObject.SetActive(_hasMorphValueFormulas);
					_zeroKeepChildValuesButton.onClick.AddListener(ZeroKeepChildValues);
				}
			}
		}
	}

	public Button copyUidButton
	{
		get
		{
			return _copyUidButton;
		}
		set
		{
			if (_copyUidButton != value)
			{
				if (_copyUidButton != null)
				{
					_copyUidButton.onClick.RemoveListener(CopyUid);
				}
				_copyUidButton = value;
				if (_copyUidButton != null)
				{
					_copyUidButton.onClick.AddListener(CopyUid);
				}
			}
		}
	}

	public RectTransform animatableWarningIndicator
	{
		get
		{
			return _animatableWarningIndicator;
		}
		set
		{
			if (_animatableWarningIndicator != value)
			{
				_animatableWarningIndicator = value;
				if (_animatableWarningIndicator != null)
				{
					_animatableWarningIndicator.gameObject.SetActive(_hasBoneModificationFormulas);
				}
			}
		}
	}

	public RectTransform animatableWarningIndicatorAlt
	{
		get
		{
			return _animatableWarningIndicatorAlt;
		}
		set
		{
			if (_animatableWarningIndicatorAlt != value)
			{
				_animatableWarningIndicatorAlt = value;
				if (_animatableWarningIndicatorAlt != null)
				{
					_animatableWarningIndicatorAlt.gameObject.SetActive(_hasBoneModificationFormulas);
				}
			}
		}
	}

	public bool animatable
	{
		get
		{
			return true;
		}
		set
		{
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
			_morphBank = value;
		}
	}

	public DAZMorphSubBank morphSubBank
	{
		get
		{
			return _morphSubBank;
		}
		set
		{
			_morphSubBank = value;
		}
	}

	public Image panelImage
	{
		get
		{
			return _panelImage;
		}
		set
		{
			if (!(_panelImage != value))
			{
				return;
			}
			if (_panelImage != null)
			{
				_panelImage.color = normalColor;
			}
			_panelImage = value;
			if (_panelImage != null)
			{
				if (isTransient)
				{
					_panelImage.color = transientColor;
				}
				else if (isInPackage)
				{
					_panelImage.color = packageColor;
				}
				else if (isRuntime)
				{
					_panelImage.color = runtimeColor;
				}
				else
				{
					_panelImage.color = normalColor;
				}
			}
		}
	}

	public Toggle favoriteToggle
	{
		get
		{
			return _favoriteToggle;
		}
		set
		{
			if (_favoriteToggle != value)
			{
				if (_favoriteToggle != null)
				{
					_favoriteToggle.onValueChanged.RemoveListener(SetFavorite);
				}
				_favoriteToggle = value;
				if (_favoriteToggle != null)
				{
					_favoriteToggle.isOn = _favorite;
					_favoriteToggle.onValueChanged.AddListener(SetFavorite);
				}
			}
		}
	}

	public bool favorite
	{
		get
		{
			return _favorite;
		}
		set
		{
			if (_favorite == value || !(_morphBank != null) || _morphBank.autoImportFolder == null || !(_morphBank.autoImportFolder != string.Empty))
			{
				return;
			}
			try
			{
				_favorite = value;
				string text = _morphBank.autoImportFolder + "/favorites";
				string path = text + "/" + resolvedDisplayName + ".fav";
				if (!FileManager.DirectoryExists(text))
				{
					FileManager.CreateDirectory(text);
				}
				if (FileManager.FileExists(path, true, true))
				{
					if (!value)
					{
						FileManager.DeleteFile(path);
					}
				}
				else if (value)
				{
					FileManager.WriteAllText(path, string.Empty);
				}
				if (favoriteToggle != null)
				{
					favoriteToggle.isOn = _favorite;
				}
				_morphBank.NotifyMorphFavoriteChanged(this);
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception during favorite set " + ex);
			}
		}
	}

	public Button openInPackageManagerButton
	{
		get
		{
			return _openInPackageManagerButton;
		}
		set
		{
			if (_openInPackageManagerButton != value)
			{
				if (_openInPackageManagerButton != null)
				{
					_openInPackageManagerButton.onClick.RemoveListener(OpenInPackageManager);
				}
				_openInPackageManagerButton = value;
				if (_openInPackageManagerButton != null)
				{
					_openInPackageManagerButton.gameObject.SetActive(isInPackage);
					_openInPackageManagerButton.onClick.AddListener(OpenInPackageManager);
				}
			}
		}
	}

	public Button openInPackageManagerButtonAlt
	{
		get
		{
			return _openInPackageManagerButtonAlt;
		}
		set
		{
			if (_openInPackageManagerButtonAlt != value)
			{
				if (_openInPackageManagerButtonAlt != null)
				{
					_openInPackageManagerButtonAlt.onClick.RemoveListener(OpenInPackageManager);
				}
				_openInPackageManagerButtonAlt = value;
				if (_openInPackageManagerButtonAlt != null)
				{
					_openInPackageManagerButtonAlt.gameObject.SetActive(isInPackage);
					_openInPackageManagerButtonAlt.onClick.AddListener(OpenInPackageManager);
				}
			}
		}
	}

	public Text packageUidText
	{
		get
		{
			return _packageUidText;
		}
		set
		{
			if (_packageUidText != value)
			{
				_packageUidText = value;
				if (_packageUidText != null)
				{
					_packageUidText.text = packageUid;
				}
			}
		}
	}

	public Text packageUidTextAlt
	{
		get
		{
			return _packageUidTextAlt;
		}
		set
		{
			if (_packageUidTextAlt != value)
			{
				_packageUidTextAlt = value;
				if (_packageUidTextAlt != null)
				{
					_packageUidTextAlt.text = packageUid;
				}
			}
		}
	}

	public Text packageLicenseText
	{
		get
		{
			return _packageLicenseText;
		}
		set
		{
			if (_packageLicenseText != value)
			{
				_packageLicenseText = value;
				if (_packageLicenseText != null)
				{
					_packageLicenseText.text = packageLicense;
				}
			}
		}
	}

	public Text packageLicenseTextAlt
	{
		get
		{
			return _packageLicenseTextAlt;
		}
		set
		{
			if (_packageLicenseTextAlt != value)
			{
				_packageLicenseTextAlt = value;
				if (_packageLicenseTextAlt != null)
				{
					_packageLicenseTextAlt.text = packageLicense;
				}
			}
		}
	}

	public Text versionText
	{
		get
		{
			return _versionText;
		}
		set
		{
			if (!(_versionText != value))
			{
				return;
			}
			_versionText = value;
			if (_versionText != null)
			{
				if (version != null && version != string.Empty)
				{
					_versionText.text = version;
					_versionText.gameObject.SetActive(true);
				}
				else
				{
					_versionText.gameObject.SetActive(false);
				}
			}
		}
	}

	public Text versionTextAlt
	{
		get
		{
			return _versionTextAlt;
		}
		set
		{
			if (!(_versionTextAlt != value))
			{
				return;
			}
			_versionTextAlt = value;
			if (_versionTextAlt != null)
			{
				if (version != null && version != string.Empty)
				{
					_versionTextAlt.text = version;
					_versionTextAlt.gameObject.SetActive(true);
				}
				else
				{
					_versionTextAlt.gameObject.SetActive(false);
				}
			}
		}
	}

	public Button increaseRangeButton
	{
		get
		{
			return _increaseRangeButton;
		}
		set
		{
			if (_increaseRangeButton != value)
			{
				if (_increaseRangeButton != null)
				{
					_increaseRangeButton.onClick.RemoveListener(IncreaseRange);
				}
				_increaseRangeButton = value;
				if (_increaseRangeButton != null)
				{
					_increaseRangeButton.onClick.AddListener(IncreaseRange);
				}
			}
		}
	}

	public Button increaseRangeButtonAlt
	{
		get
		{
			return _increaseRangeButtonAlt;
		}
		set
		{
			if (_increaseRangeButtonAlt != value)
			{
				if (_increaseRangeButtonAlt != null)
				{
					_increaseRangeButtonAlt.onClick.RemoveListener(IncreaseRange);
				}
				_increaseRangeButtonAlt = value;
				if (_increaseRangeButtonAlt != null)
				{
					_increaseRangeButtonAlt.onClick.AddListener(IncreaseRange);
				}
			}
		}
	}

	public Button resetRangeButton
	{
		get
		{
			return _resetRangeButton;
		}
		set
		{
			if (_resetRangeButton != value)
			{
				if (_resetRangeButton != null)
				{
					_resetRangeButton.onClick.RemoveListener(ResetRange);
				}
				_resetRangeButton = value;
				if (_resetRangeButton != null)
				{
					_resetRangeButton.onClick.AddListener(ResetRange);
				}
			}
		}
	}

	public Button resetRangeButtonAlt
	{
		get
		{
			return _resetRangeButtonAlt;
		}
		set
		{
			if (_resetRangeButtonAlt != value)
			{
				if (_resetRangeButtonAlt != null)
				{
					_resetRangeButtonAlt.onClick.RemoveListener(ResetRange);
				}
				_resetRangeButtonAlt = value;
				if (_resetRangeButtonAlt != null)
				{
					_resetRangeButtonAlt.onClick.AddListener(ResetRange);
				}
			}
		}
	}

	public DAZMorph()
	{
	}

	public DAZMorph(DAZMorph copyFrom)
	{
		CopyParameters(copyFrom);
		numDeltas = copyFrom.numDeltas;
		deltas = new DAZMorphVertex[copyFrom.deltas.Length];
		for (int i = 0; i < deltas.Length; i++)
		{
			deltas[i] = new DAZMorphVertex();
			deltas[i].vertex = copyFrom.deltas[i].vertex;
			deltas[i].delta = copyFrom.deltas[i].delta;
		}
		formulas = new DAZMorphFormula[copyFrom.formulas.Length];
		for (int j = 0; j < formulas.Length; j++)
		{
			formulas[j] = new DAZMorphFormula();
			formulas[j].targetType = copyFrom.formulas[j].targetType;
			formulas[j].target = copyFrom.formulas[j].target;
			formulas[j].multiplier = copyFrom.formulas[j].multiplier;
		}
	}

	public void SyncDrivenUI()
	{
		if (drivenIndicator != null)
		{
			drivenIndicator.gameObject.SetActive(_isDriven);
		}
		if (drivenByText != null)
		{
			drivenByText.text = _drivenBy;
		}
	}

	public bool SetDriven(bool b, string by, bool syncUI = false)
	{
		bool result = false;
		if (_isDriven != b)
		{
			_isDriven = b;
			result = true;
		}
		if (_drivenBy != by)
		{
			_drivenBy = by;
			result = true;
		}
		if (syncUI)
		{
			SyncDrivenUI();
		}
		return result;
	}

	protected void ZeroKeepChildValues()
	{
		morphValueAdjustLimits = 0f;
		_wasZeroedKeepChildValues = true;
	}

	public void CopyUid()
	{
		GUIUtility.systemCopyBuffer = uid;
		uidCopy = uid;
	}

	protected void SetFavorite(bool b)
	{
		favorite = b;
	}

	public void SyncJSON()
	{
		jsonFloat.valNoCallback = _morphValue;
	}

	public bool SetValue(float v)
	{
		if (jsonFloat.val != v)
		{
			SetValueInternal(v);
			return true;
		}
		return false;
	}

	public bool SetValueThreadSafe(float v)
	{
		float num = v;
		if (num < min)
		{
			num = min;
		}
		if (num > max)
		{
			num = max;
		}
		if (_morphValue != num)
		{
			_morphValue = num;
			activeImmediate = _morphValue != 0f;
			return true;
		}
		return false;
	}

	protected void AdjustRange(float v)
	{
		if (v < min)
		{
			float num = (min = Mathf.Floor(v));
			jsonFloat.min = num;
		}
		else if (v > max)
		{
			float num2 = (max = Mathf.Ceil(v));
			jsonFloat.max = num2;
		}
	}

	protected void SetValueInternal(float v)
	{
		AdjustRange(v);
		_morphValue = v;
		activeImmediate = _morphValue != 0f;
	}

	public void Init()
	{
		startValue = _morphValue;
		startingMin = min;
		startingMax = max;
		jsonFloat = new JSONStorableFloat(resolvedDisplayName, _morphValue, SetValueInternal, min, max, false);
		jsonFloat.isStorable = false;
		jsonFloat.isRestorable = false;
		if (_morphBank != null && FileManager.FileExists(_morphBank.autoImportFolder + "/favorites/" + resolvedDisplayName + ".fav", true, true))
		{
			_favorite = true;
		}
		else
		{
			_favorite = false;
		}
		_formulasString = string.Empty;
		HashSet<string> hashSet = new HashSet<string>();
		_hasFormulas = formulas.Length != 0;
		DAZMorphFormula[] array = formulas;
		foreach (DAZMorphFormula dAZMorphFormula in array)
		{
			switch (dAZMorphFormula.targetType)
			{
			case DAZMorphFormulaTargetType.MorphValue:
			{
				DAZMorph morph = morphBank.GetMorph(dAZMorphFormula.target);
				if (morph != null)
				{
					_hasMorphValueFormulas = true;
					_formulasString = _formulasString + "Drives: " + morph.resolvedDisplayName + "\n";
				}
				break;
			}
			case DAZMorphFormulaTargetType.MCM:
			{
				DAZMorph morph = morphBank.GetMorph(dAZMorphFormula.target);
				if (morph != null)
				{
					_hasMCMFormulas = true;
					_formulasString = _formulasString + "MCM driven by: " + morph.resolvedDisplayName + "\n";
				}
				break;
			}
			case DAZMorphFormulaTargetType.MCMMult:
			{
				DAZMorph morph = morphBank.GetMorph(dAZMorphFormula.target);
				if (morph != null)
				{
					_hasMCMFormulas = true;
					_formulasString = _formulasString + "MCMMult driven by: " + morph.resolvedDisplayName + "\n";
				}
				break;
			}
			case DAZMorphFormulaTargetType.BoneCenterX:
			case DAZMorphFormulaTargetType.BoneCenterY:
			case DAZMorphFormulaTargetType.BoneCenterZ:
			case DAZMorphFormulaTargetType.OrientationX:
			case DAZMorphFormulaTargetType.OrientationY:
			case DAZMorphFormulaTargetType.OrientationZ:
				_hasBoneModificationFormulas = true;
				break;
			case DAZMorphFormulaTargetType.RotationX:
			case DAZMorphFormulaTargetType.RotationY:
			case DAZMorphFormulaTargetType.RotationZ:
			{
				DAZMorph morph = morphBank.GetMorph(dAZMorphFormula.target);
				if (morph != null)
				{
					_hasBoneRotationFormulas = true;
					string text = "Rotates bone: " + morph.resolvedDisplayName + "\n";
					if (!hashSet.Contains(text))
					{
						_formulasString += text;
						hashSet.Add(text);
					}
				}
				break;
			}
			}
		}
		if (_hasBoneModificationFormulas)
		{
			_formulasString += "Has bone morphs.\n Animation not recommended\n";
		}
	}

	protected void OpenInPackageManager()
	{
		SuperController.singleton.OpenPackageInManager(packageUid);
	}

	public void IncreaseRange()
	{
		min = Mathf.Floor(min - 1f);
		jsonFloat.min = min;
		max = Mathf.Ceil(max + 1f);
		jsonFloat.max = max;
	}

	public void ResetRange()
	{
		min = startingMin;
		max = startingMax;
		jsonFloat.min = startingMin;
		jsonFloat.max = startingMax;
	}

	public void InitUI(Transform UITransform)
	{
		DAZMorphUI component = UITransform.GetComponent<DAZMorphUI>();
		if (component != null)
		{
			jsonFloat.slider = component.slider;
			increaseRangeButton = component.increaseRangeButton;
			openInPackageManagerButton = component.openInPackageButton;
			packageUidText = component.packageUidText;
			packageLicenseText = component.packageLicenseText;
			versionText = component.versionText;
			resetRangeButton = component.resetRangeButton;
			if (component.morphNameText != null)
			{
				component.morphNameText.text = resolvedDisplayName;
			}
			favoriteToggle = component.favoriteToggle;
			animatableWarningIndicator = component.animatableWarningIndicator;
			panelImage = component.panelImage;
			drivenIndicator = component.drivenIndicator;
			drivenByText = component.drivenByText;
			SyncDrivenUI();
			if (component.hasFormulasIndicator != null)
			{
				component.hasFormulasIndicator.gameObject.SetActive(_hasMorphValueFormulas || _hasMCMFormulas || _hasBoneModificationFormulas);
			}
			if (component.formulasText != null)
			{
				component.formulasText.text = formulasString;
			}
			zeroKeepChildValuesButton = component.zeroKeepChildValuesButton;
			copyUidButton = component.copyUidButton;
			if (component.categoryText != null)
			{
				component.categoryText.text = resolvedRegionName;
			}
		}
	}

	public void DeregisterUI()
	{
		jsonFloat.slider = null;
		openInPackageManagerButton = null;
		packageUidText = null;
		packageLicenseText = null;
		versionText = null;
		increaseRangeButton = null;
		resetRangeButton = null;
		favoriteToggle = null;
		animatableWarningIndicator = null;
		panelImage = null;
		drivenIndicator = null;
		drivenByText = null;
		zeroKeepChildValuesButton = null;
	}

	public void InitUIAlt(Transform UITransform)
	{
		DAZMorphUI component = UITransform.GetComponent<DAZMorphUI>();
		if (component != null)
		{
			jsonFloat.sliderAlt = component.slider;
			openInPackageManagerButtonAlt = component.openInPackageButton;
			packageUidTextAlt = component.packageUidText;
			packageLicenseTextAlt = component.packageLicenseText;
			versionTextAlt = component.versionText;
			increaseRangeButtonAlt = component.increaseRangeButton;
			resetRangeButtonAlt = component.resetRangeButton;
			if (component.morphNameText != null)
			{
				component.morphNameText.text = resolvedDisplayName;
			}
			animatableWarningIndicatorAlt = component.animatableWarningIndicator;
		}
	}

	public void DeregisterUIAlt()
	{
		jsonFloat.sliderAlt = null;
		openInPackageManagerButtonAlt = null;
		packageUidTextAlt = null;
		packageLicenseTextAlt = null;
		versionTextAlt = null;
		increaseRangeButtonAlt = null;
		resetRangeButtonAlt = null;
		animatableWarningIndicatorAlt = null;
	}

	public bool StoreJSON(JSONClass jc, bool forceStore = false)
	{
		bool flag = false;
		jc["uid"] = uid;
		jc["name"] = resolvedDisplayName;
		if (jsonFloat.val != jsonFloat.defaultVal || forceStore)
		{
			jc["value"].AsFloat = _morphValue;
			flag = true;
		}
		if (flag && isTransient && morphBank != null)
		{
			string text = morphBank.autoImportFolder + "/AUTO/" + Path.GetFileName(metaLoadPath);
			bool flag2 = false;
			if (!FileManager.FileExists(text, true, true))
			{
				try
				{
					string directoryName = Path.GetDirectoryName(text);
					FileManager.CreateDirectory(directoryName);
					FileManager.CopyFile(metaLoadPath, text);
				}
				catch (Exception ex)
				{
					flag2 = true;
					Debug.LogError("Could not copy transient morph to import folder " + ex);
				}
			}
			string text2 = morphBank.autoImportFolder + "/AUTO/" + Path.GetFileName(deltasLoadPath);
			if (!FileManager.FileExists(text2))
			{
				try
				{
					FileManager.CopyFile(deltasLoadPath, text2);
				}
				catch (Exception ex2)
				{
					flag2 = true;
					Debug.LogError("Could not copy transient morph to import folder " + ex2);
				}
			}
			if (!flag2 && _morphSubBank != null)
			{
				FileEntry fileEntry = FileManager.GetFileEntry(text);
				if (fileEntry != null)
				{
					jc["uid"] = fileEntry.Uid;
				}
			}
		}
		return flag;
	}

	public void RestoreFromJSON(JSONClass jc)
	{
		if (jc["value"] != null)
		{
			jsonFloat.val = jc["value"].AsFloat;
		}
	}

	public void Reset()
	{
		ResetRange();
		jsonFloat.val = jsonFloat.defaultVal;
	}

	public void SetDefaultValue()
	{
		jsonFloat.val = jsonFloat.defaultVal;
	}

	public void CopyParameters(DAZMorph copyFrom, bool setValue = true)
	{
		group = copyFrom.group;
		region = copyFrom.region;
		overrideRegion = copyFrom.overrideRegion;
		morphName = copyFrom.morphName;
		displayName = copyFrom.displayName;
		overrideName = copyFrom.overrideName;
		preserveValueOnReimport = copyFrom.preserveValueOnReimport;
		min = copyFrom.min;
		max = copyFrom.max;
		visible = copyFrom.visible;
		disable = copyFrom.disable;
		isPoseControl = copyFrom.isPoseControl;
		if (setValue)
		{
			_morphValue = copyFrom.morphValue;
			appliedValue = copyFrom.appliedValue;
		}
		triggerNormalRecalc = copyFrom.triggerNormalRecalc;
		triggerTangentRecalc = copyFrom.triggerTangentRecalc;
	}

	private bool ProcessFormula(JSONNode fn, DAZMorphFormula formula, string morphName)
	{
		JSONNode jSONNode = fn["operations"];
		string url = jSONNode[0]["url"];
		string text = DAZImport.DAZurlToId(url);
		if (text == morphName + "?value")
		{
			string text2 = jSONNode[2]["op"];
			if (text2 == "mult")
			{
				float asFloat = jSONNode[1]["val"].AsFloat;
				formula.multiplier = asFloat;
				return true;
			}
			Debug.LogWarning("Morph " + morphName + ": Found unknown formula " + text2);
		}
		else if (formula.target == morphName)
		{
			string text3 = fn["stage"];
			if (text3 != null)
			{
				if (text3 == "mult")
				{
					formula.targetType = DAZMorphFormulaTargetType.MCMMult;
					text = Regex.Replace(text, "\\?.*", string.Empty);
					formula.target = text;
					return true;
				}
				Debug.LogWarning("Morph " + morphName + ": Found unknown stage " + text3);
			}
			else
			{
				formula.targetType = DAZMorphFormulaTargetType.MCM;
				text = Regex.Replace(text, "\\?.*", string.Empty);
				formula.target = text;
				string text4 = jSONNode[2]["op"];
				if (text4 == "mult")
				{
					float asFloat2 = jSONNode[1]["val"].AsFloat;
					formula.multiplier = asFloat2;
					return true;
				}
				Debug.LogWarning("Morph " + morphName + ": Found unknown formula " + text4);
			}
		}
		else
		{
			Debug.LogWarning("Morph " + morphName + ": Found unknown operation url " + text);
		}
		return false;
	}

	public void Import(JSONNode mn)
	{
		morphName = mn["id"];
		_morphValue = 0f;
		appliedValue = 0f;
		if (mn["group"] != null)
		{
			group = Regex.Replace(mn["group"], "^/", string.Empty);
		}
		else
		{
			group = string.Empty;
		}
		displayName = mn["channel"]["label"];
		region = mn["region"];
		if (region == null)
		{
			region = group;
		}
		min = mn["channel"]["min"].AsFloat;
		if (min == -100000f)
		{
			min = -1f;
		}
		max = mn["channel"]["max"].AsFloat;
		if (max == 100000f)
		{
			max = 1f;
		}
		if (mn["formulas"].Count > 0)
		{
			List<DAZMorphFormula> list = new List<DAZMorphFormula>();
			foreach (JSONNode item in mn["formulas"].AsArray)
			{
				DAZMorphFormula dAZMorphFormula = new DAZMorphFormula();
				string text = item["output"];
				string input = Regex.Replace(text, "^.*#", string.Empty);
				input = Regex.Replace(input, "\\?.*", string.Empty);
				input = DAZImport.DAZurlFix(input);
				dAZMorphFormula.target = input;
				string text2 = Regex.Replace(text, "^.*\\?", string.Empty);
				switch (text2)
				{
				case "value":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.MorphValue;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						list.Add(dAZMorphFormula);
					}
					continue;
				case "scale/general":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.GeneralScale;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						list.Add(dAZMorphFormula);
					}
					continue;
				case "scale/x":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.ScaleX;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						list.Add(dAZMorphFormula);
					}
					continue;
				case "scale/y":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.ScaleY;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						list.Add(dAZMorphFormula);
					}
					continue;
				case "scale/z":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.ScaleZ;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						list.Add(dAZMorphFormula);
					}
					continue;
				case "center_point/x":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.BoneCenterX;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						dAZMorphFormula.multiplier = (0f - dAZMorphFormula.multiplier) * 0.01f;
						list.Add(dAZMorphFormula);
					}
					continue;
				case "center_point/y":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.BoneCenterY;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						dAZMorphFormula.multiplier *= 0.01f;
						list.Add(dAZMorphFormula);
					}
					continue;
				case "center_point/z":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.BoneCenterZ;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						dAZMorphFormula.multiplier *= 0.01f;
						list.Add(dAZMorphFormula);
					}
					continue;
				}
				if (Regex.IsMatch(text2, "^end_point"))
				{
					continue;
				}
				switch (text2)
				{
				case "orientation/x":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.OrientationX;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						list.Add(dAZMorphFormula);
					}
					break;
				case "orientation/y":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.OrientationY;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						dAZMorphFormula.multiplier *= -1f;
						list.Add(dAZMorphFormula);
					}
					break;
				case "orientation/z":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.OrientationZ;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						dAZMorphFormula.multiplier *= -1f;
						list.Add(dAZMorphFormula);
					}
					break;
				case "rotation/x":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.RotationX;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						list.Add(dAZMorphFormula);
					}
					break;
				case "rotation/y":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.RotationY;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						list.Add(dAZMorphFormula);
					}
					break;
				case "rotation/z":
					dAZMorphFormula.targetType = DAZMorphFormulaTargetType.RotationZ;
					if (ProcessFormula(item, dAZMorphFormula, morphName))
					{
						list.Add(dAZMorphFormula);
					}
					break;
				default:
					Debug.LogWarning("Morph " + morphName + " has unknown output type " + text);
					break;
				}
			}
			formulas = list.ToArray();
		}
		else
		{
			formulas = new DAZMorphFormula[0];
		}
		numDeltas = mn["morph"]["deltas"]["count"].AsInt;
		deltas = new DAZMorphVertex[numDeltas];
		int num = 0;
		Vector3 delta = default(Vector3);
		foreach (JSONNode item2 in mn["morph"]["deltas"]["values"].AsArray)
		{
			int asInt = item2[0].AsInt;
			delta.x = (0f - item2[1].AsFloat) * 0.01f;
			delta.y = item2[2].AsFloat * 0.01f;
			delta.z = item2[3].AsFloat * 0.01f;
			DAZMorphVertex dAZMorphVertex = new DAZMorphVertex();
			dAZMorphVertex.vertex = asInt;
			dAZMorphVertex.delta = delta;
			deltas[num] = dAZMorphVertex;
			num++;
		}
	}

	public void LoadMeta()
	{
		if (metaJSONFile != null)
		{
			JSONNode jSONNode = DAZImport.ReadJSON(metaJSONFile);
			if (jSONNode != null)
			{
				LoadMetaFromJSON(jSONNode);
			}
		}
	}

	public void LoadMetaFromJSON(JSONNode metan)
	{
		morphName = metan["id"];
		displayName = metan["displayName"];
		overrideName = metan["overrideName"];
		group = metan["group"];
		region = metan["region"];
		overrideRegion = metan["overrideRegion"];
		if (metan["min"] != null)
		{
			min = metan["min"].AsFloat;
		}
		if (metan["max"] != null)
		{
			max = metan["max"].AsFloat;
		}
		if (metan["numDeltas"] != null)
		{
			numDeltas = metan["numDeltas"].AsInt;
		}
		if (metan["isPoseControl"] != null)
		{
			isPoseControl = metan["isPoseControl"].AsBool;
		}
		if (!(metan["formulas"] != null))
		{
			return;
		}
		List<DAZMorphFormula> list = new List<DAZMorphFormula>();
		foreach (JSONClass item in metan["formulas"].AsArray)
		{
			DAZMorphFormula dAZMorphFormula = new DAZMorphFormula();
			try
			{
				bool flag = false;
				if (item["targetType"] != null)
				{
					dAZMorphFormula.targetType = (DAZMorphFormulaTargetType)Enum.Parse(typeof(DAZMorphFormulaTargetType), item["targetType"]);
				}
				else
				{
					flag = true;
				}
				if (item["target"] != null)
				{
					dAZMorphFormula.target = item["target"];
				}
				else
				{
					flag = true;
				}
				if (item["multiplier"] != null)
				{
					dAZMorphFormula.multiplier = item["multiplier"].AsFloat;
				}
				else
				{
					flag = true;
				}
				if (!flag)
				{
					list.Add(dAZMorphFormula);
				}
			}
			catch (ArgumentException)
			{
			}
		}
		formulas = list.ToArray();
	}

	public JSONClass GetMetaJSON()
	{
		JSONClass jSONClass = new JSONClass();
		if (morphName != null)
		{
			jSONClass["id"] = morphName;
		}
		if (displayName != null)
		{
			jSONClass["displayName"] = displayName;
		}
		if (overrideName != null)
		{
			jSONClass["overrideName"] = overrideName;
		}
		if (group != null)
		{
			jSONClass["group"] = group;
		}
		if (region != null)
		{
			jSONClass["region"] = region;
		}
		if (overrideRegion != null)
		{
			jSONClass["overrideRegion"] = region;
		}
		jSONClass["min"].AsFloat = min;
		jSONClass["max"].AsFloat = max;
		jSONClass["numDeltas"].AsInt = numDeltas;
		jSONClass["isPoseControl"].AsBool = isPoseControl;
		JSONArray jSONArray = new JSONArray();
		DAZMorphFormula[] array = formulas;
		foreach (DAZMorphFormula dAZMorphFormula in array)
		{
			JSONClass jSONClass2 = new JSONClass();
			if (dAZMorphFormula.targetType == DAZMorphFormulaTargetType.MCM || dAZMorphFormula.targetType == DAZMorphFormulaTargetType.MCMMult)
			{
				Debug.LogError("Morph " + morphName + " will not be compiled because it is an MCM morph which can cause unwanted changes to shape");
				return null;
			}
			jSONClass2["targetType"] = dAZMorphFormula.targetType.ToString();
			jSONClass2["target"] = dAZMorphFormula.target;
			jSONClass2["multiplier"].AsFloat = dAZMorphFormula.multiplier;
			jSONArray.Add(jSONClass2);
		}
		jSONClass["formulas"] = jSONArray;
		return jSONClass;
	}

	public void LoadDeltas()
	{
		if (deltasLoadPath != null && deltasLoadPath != string.Empty && !deltasLoaded)
		{
			deltasLoaded = true;
			LoadDeltasFromBinaryFile(deltasLoadPath);
		}
	}

	public void UnloadDeltas()
	{
		if (isRuntime && deltasLoaded)
		{
			deltas = null;
			deltasLoaded = false;
		}
	}

	public void LoadDeltasFromJSON(JSONNode deltan)
	{
		JSONArray asArray = deltan["deltas"].AsArray;
		if (!(asArray != null))
		{
			return;
		}
		numDeltas = asArray.Count;
		deltas = new DAZMorphVertex[numDeltas];
		int num = 0;
		Vector3 delta = default(Vector3);
		foreach (JSONNode item in asArray)
		{
			DAZMorphVertex dAZMorphVertex = new DAZMorphVertex();
			delta.x = item["x"].AsFloat;
			delta.y = item["y"].AsFloat;
			delta.z = item["z"].AsFloat;
			dAZMorphVertex.delta = delta;
			dAZMorphVertex.vertex = item["vid"].AsInt;
			deltas[num] = dAZMorphVertex;
			num++;
		}
	}

	public void LoadDeltasFromBinaryFile(string path)
	{
		try
		{
			using (FileEntryStream fileEntryStream = FileManager.OpenStream(path, true))
			{
				using (BinaryReader binaryReader = new BinaryReader(fileEntryStream.Stream))
				{
					numDeltas = binaryReader.ReadInt32();
					deltas = new DAZMorphVertex[numDeltas];
					Vector3 delta = default(Vector3);
					for (int i = 0; i < numDeltas; i++)
					{
						DAZMorphVertex dAZMorphVertex = new DAZMorphVertex();
						dAZMorphVertex.vertex = binaryReader.ReadInt32();
						delta.x = binaryReader.ReadSingle();
						delta.y = binaryReader.ReadSingle();
						delta.z = binaryReader.ReadSingle();
						dAZMorphVertex.delta = delta;
						deltas[i] = dAZMorphVertex;
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Error while loading binary delta file " + path + " " + ex);
		}
	}

	public void SaveDeltasToBinaryFile(string path)
	{
		using (FileStream output = FileManager.OpenStreamForCreate(path))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(output))
			{
				binaryWriter.Write(numDeltas);
				DAZMorphVertex[] array = deltas;
				foreach (DAZMorphVertex dAZMorphVertex in array)
				{
					binaryWriter.Write(dAZMorphVertex.vertex);
					binaryWriter.Write(dAZMorphVertex.delta.x);
					binaryWriter.Write(dAZMorphVertex.delta.y);
					binaryWriter.Write(dAZMorphVertex.delta.z);
				}
			}
		}
	}

	public JSONClass GetDeltasJSON()
	{
		JSONClass jSONClass = new JSONClass();
		JSONArray jSONArray = new JSONArray();
		DAZMorphVertex[] array = deltas;
		foreach (DAZMorphVertex dAZMorphVertex in array)
		{
			JSONClass jSONClass2 = new JSONClass();
			jSONClass2["x"].AsFloat = dAZMorphVertex.delta.x;
			jSONClass2["y"].AsFloat = dAZMorphVertex.delta.y;
			jSONClass2["z"].AsFloat = dAZMorphVertex.delta.z;
			jSONClass2["vid"].AsInt = dAZMorphVertex.vertex;
			jSONArray.Add(jSONClass2);
		}
		jSONClass["deltas"] = jSONArray;
		return jSONClass;
	}
}
