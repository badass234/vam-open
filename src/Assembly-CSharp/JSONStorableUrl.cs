using System.Collections.Generic;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class JSONStorableUrl : JSONStorableString
{
	public delegate void BeginBrowseCallback();

	public delegate void EndBrowseCallback();

	public delegate void BeginBrowseWithObjectCallback(JSONStorableUrl jsurl);

	public delegate void EndBrowseWithObjectCallback(JSONStorableUrl jsurl);

	protected string _filter;

	protected string _activeSuggestedPath;

	protected static Dictionary<string, string> suggestedPathGroups;

	protected string _suggestedPathGroup;

	protected string _suggestedPath;

	protected string _normalizedSuggestedPath;

	protected bool _allowBrowseAboveSuggestPath = true;

	public bool allowFullComputerBrowse = true;

	public bool showDirs = true;

	public List<ShortCut> shortCuts;

	protected bool _forceCallbackOnSet;

	public bool hideExtension;

	public string fileRemovePrefix;

	protected bool _valueSetFromBrowse;

	public BeginBrowseCallback beginBrowseCallback;

	public EndBrowseCallback endBrowseCallback;

	public BeginBrowseWithObjectCallback beginBrowseWithObjectCallback;

	public EndBrowseWithObjectCallback endBrowseWithObjectCallback;

	protected Button _fileBrowseButton;

	protected Button _fileBrowseButtonAlt;

	protected Button _copyToClipboardButton;

	protected Button _copyToClipboardButtonAlt;

	protected Button _copyFromClipboardButton;

	protected Button _copyFromClipboardButtonAlt;

	protected Button _clearButton;

	protected Button _clearButtonAlt;

	protected Button _reloadButton;

	protected Button _reloadButtonAlt;

	public string suggestedPathGroup
	{
		get
		{
			return _suggestedPathGroup;
		}
		set
		{
			if (_suggestedPathGroup != value)
			{
				_suggestedPathGroup = value;
				if (suggestedPathGroups == null)
				{
					suggestedPathGroups = new Dictionary<string, string>();
				}
				if (!suggestedPathGroups.ContainsKey(_suggestedPathGroup))
				{
					suggestedPathGroups.Add(_suggestedPathGroup, _suggestedPath);
				}
			}
		}
	}

	public string suggestedPath
	{
		get
		{
			return _suggestedPath;
		}
		set
		{
			if (_suggestedPath != value)
			{
				_suggestedPath = value;
				if (_suggestedPath == null || _suggestedPath == string.Empty)
				{
					_normalizedSuggestedPath = null;
					_activeSuggestedPath = null;
				}
				else
				{
					_normalizedSuggestedPath = _suggestedPath.Replace('\\', '/');
					_normalizedSuggestedPath = Regex.Replace(_normalizedSuggestedPath, "/$", string.Empty);
					ResetActiveSuggestedPath();
				}
			}
		}
	}

	public bool allowBrowseAboveSuggestedPath
	{
		get
		{
			return _allowBrowseAboveSuggestPath;
		}
		set
		{
			if (_allowBrowseAboveSuggestPath != value)
			{
				_allowBrowseAboveSuggestPath = value;
				ResetActiveSuggestedPath();
			}
		}
	}

	public bool valueSetFromBrowse => _valueSetFromBrowse;

	public override string val
	{
		get
		{
			return _val;
		}
		set
		{
			string text = ((value == null) ? value : value.TrimEnd().TrimStart());
			if (_val != text)
			{
				base.val = text;
			}
		}
	}

	public Button fileBrowseButton
	{
		get
		{
			return _fileBrowseButton;
		}
		set
		{
			if (_fileBrowseButton != value)
			{
				if (_fileBrowseButton != null)
				{
					_fileBrowseButton.onClick.RemoveListener(FileBrowse);
				}
				_fileBrowseButton = value;
				if (_fileBrowseButton != null)
				{
					_fileBrowseButton.onClick.AddListener(FileBrowse);
				}
			}
		}
	}

	public Button fileBrowseButtonAlt
	{
		get
		{
			return _fileBrowseButtonAlt;
		}
		set
		{
			if (_fileBrowseButtonAlt != value)
			{
				if (_fileBrowseButtonAlt != null)
				{
					_fileBrowseButtonAlt.onClick.RemoveListener(FileBrowse);
				}
				_fileBrowseButtonAlt = value;
				if (_fileBrowseButtonAlt != null)
				{
					_fileBrowseButtonAlt.onClick.AddListener(FileBrowse);
				}
			}
		}
	}

	public Button copyToClipboardButton
	{
		get
		{
			return _copyToClipboardButton;
		}
		set
		{
			if (_copyToClipboardButton != value)
			{
				if (_copyToClipboardButton != null)
				{
					_copyToClipboardButton.onClick.RemoveListener(CopyToClipboard);
				}
				_copyToClipboardButton = value;
				if (_copyToClipboardButton != null)
				{
					_copyToClipboardButton.onClick.AddListener(CopyToClipboard);
				}
			}
		}
	}

	public Button copyToClipboardButtonAlt
	{
		get
		{
			return _copyToClipboardButtonAlt;
		}
		set
		{
			if (_copyToClipboardButtonAlt != value)
			{
				if (_copyToClipboardButtonAlt != null)
				{
					_copyToClipboardButtonAlt.onClick.RemoveListener(CopyToClipboard);
				}
				_copyToClipboardButtonAlt = value;
				if (_copyToClipboardButtonAlt != null)
				{
					_copyToClipboardButtonAlt.onClick.AddListener(CopyToClipboard);
				}
			}
		}
	}

	public Button copyFromClipboardButton
	{
		get
		{
			return _copyFromClipboardButton;
		}
		set
		{
			if (_copyFromClipboardButton != value)
			{
				if (_copyFromClipboardButton != null)
				{
					_copyFromClipboardButton.onClick.RemoveListener(CopyFromClipboard);
				}
				_copyFromClipboardButton = value;
				if (_copyFromClipboardButton != null)
				{
					_copyFromClipboardButton.onClick.AddListener(CopyFromClipboard);
				}
			}
		}
	}

	public Button copyFromClipboardButtonAlt
	{
		get
		{
			return _copyFromClipboardButtonAlt;
		}
		set
		{
			if (_copyFromClipboardButtonAlt != value)
			{
				if (_copyFromClipboardButtonAlt != null)
				{
					_copyFromClipboardButtonAlt.onClick.RemoveListener(CopyFromClipboard);
				}
				_copyFromClipboardButtonAlt = value;
				if (_copyFromClipboardButtonAlt != null)
				{
					_copyFromClipboardButtonAlt.onClick.AddListener(CopyFromClipboard);
				}
			}
		}
	}

	public Button clearButton
	{
		get
		{
			return _clearButton;
		}
		set
		{
			if (_clearButton != value)
			{
				if (_clearButton != null)
				{
					_clearButton.onClick.RemoveListener(Clear);
				}
				_clearButton = value;
				if (_clearButton != null)
				{
					_clearButton.onClick.AddListener(Clear);
				}
			}
		}
	}

	public Button clearButtonAlt
	{
		get
		{
			return _clearButtonAlt;
		}
		set
		{
			if (_clearButtonAlt != value)
			{
				if (_clearButtonAlt != null)
				{
					_clearButtonAlt.onClick.RemoveListener(Clear);
				}
				_clearButtonAlt = value;
				if (_clearButtonAlt != null)
				{
					_clearButtonAlt.onClick.AddListener(Clear);
				}
			}
		}
	}

	public Button reloadButton
	{
		get
		{
			return _reloadButton;
		}
		set
		{
			if (_reloadButton != value)
			{
				if (_reloadButton != null)
				{
					_reloadButton.onClick.RemoveListener(Reload);
				}
				_reloadButton = value;
				if (_reloadButton != null)
				{
					_reloadButton.onClick.AddListener(Reload);
				}
			}
		}
	}

	public Button reloadButtonAlt
	{
		get
		{
			return _reloadButtonAlt;
		}
		set
		{
			if (_reloadButtonAlt != value)
			{
				if (_reloadButtonAlt != null)
				{
					_reloadButtonAlt.onClick.RemoveListener(Reload);
				}
				_reloadButtonAlt = value;
				if (_reloadButtonAlt != null)
				{
					_reloadButtonAlt.onClick.AddListener(Reload);
				}
			}
		}
	}

	public JSONStorableUrl(string paramName, string startingValue)
		: base(paramName, startingValue)
	{
		type = JSONStorable.Type.Url;
		_filter = null;
		suggestedPath = null;
		_forceCallbackOnSet = false;
	}

	public JSONStorableUrl(string paramName, string startingValue, SetStringCallback callback)
		: base(paramName, startingValue, callback)
	{
		type = JSONStorable.Type.Url;
		_filter = null;
		suggestedPath = null;
		_forceCallbackOnSet = false;
	}

	public JSONStorableUrl(string paramName, string startingValue, SetJSONStringCallback callback)
		: base(paramName, startingValue, callback)
	{
		type = JSONStorable.Type.Url;
		_filter = null;
		suggestedPath = null;
		_forceCallbackOnSet = false;
	}

	public JSONStorableUrl(string paramName, string startingValue, string filter)
		: base(paramName, startingValue)
	{
		type = JSONStorable.Type.Url;
		_filter = filter;
		suggestedPath = null;
		_forceCallbackOnSet = false;
	}

	public JSONStorableUrl(string paramName, string startingValue, SetStringCallback callback, string filter)
		: base(paramName, startingValue, callback)
	{
		type = JSONStorable.Type.Url;
		_filter = filter;
		suggestedPath = null;
		_forceCallbackOnSet = false;
	}

	public JSONStorableUrl(string paramName, string startingValue, SetJSONStringCallback callback, string filter)
		: base(paramName, startingValue, callback)
	{
		type = JSONStorable.Type.Url;
		_filter = filter;
		suggestedPath = null;
		_forceCallbackOnSet = false;
	}

	public JSONStorableUrl(string paramName, string startingValue, string filter, string suggestPath)
		: base(paramName, startingValue)
	{
		type = JSONStorable.Type.Url;
		_filter = filter;
		suggestedPath = suggestPath;
		_forceCallbackOnSet = false;
	}

	public JSONStorableUrl(string paramName, string startingValue, SetStringCallback callback, string filter, string suggestPath)
		: base(paramName, startingValue, callback)
	{
		type = JSONStorable.Type.Url;
		_filter = filter;
		suggestedPath = suggestPath;
		_forceCallbackOnSet = false;
	}

	public JSONStorableUrl(string paramName, string startingValue, SetJSONStringCallback callback, string filter, string suggestPath)
		: base(paramName, startingValue, callback)
	{
		type = JSONStorable.Type.Url;
		_filter = filter;
		suggestedPath = suggestPath;
		_forceCallbackOnSet = false;
	}

	public JSONStorableUrl(string paramName, string startingValue, SetStringCallback callback, string filter, bool forceCallbackOnSet)
		: base(paramName, startingValue, callback)
	{
		type = JSONStorable.Type.Url;
		_filter = filter;
		suggestedPath = null;
		_forceCallbackOnSet = forceCallbackOnSet;
	}

	public JSONStorableUrl(string paramName, string startingValue, SetJSONStringCallback callback, string filter, bool forceCallbackOnSet)
		: base(paramName, startingValue, callback)
	{
		type = JSONStorable.Type.Url;
		_filter = filter;
		suggestedPath = null;
		_forceCallbackOnSet = forceCallbackOnSet;
	}

	public JSONStorableUrl(string paramName, string startingValue, SetStringCallback callback, string filter, string suggestPath, bool forceCallbackOnSet)
		: base(paramName, startingValue, callback)
	{
		type = JSONStorable.Type.Url;
		_filter = filter;
		suggestedPath = suggestPath;
		_forceCallbackOnSet = forceCallbackOnSet;
	}

	public JSONStorableUrl(string paramName, string startingValue, SetJSONStringCallback callback, string filter, string suggestPath, bool forceCallbackOnSet)
		: base(paramName, startingValue, callback)
	{
		type = JSONStorable.Type.Url;
		_filter = filter;
		suggestedPath = suggestPath;
		_forceCallbackOnSet = forceCallbackOnSet;
	}

	protected void ResetActiveSuggestedPath()
	{
		_activeSuggestedPath = _normalizedSuggestedPath;
		if (_activeSuggestedPath != null && allowBrowseAboveSuggestedPath)
		{
			_activeSuggestedPath = _activeSuggestedPath.Replace('/', '\\');
		}
	}

	public static void SetSuggestedPathGroupPath(string group, string path)
	{
		if (suggestedPathGroups == null)
		{
			suggestedPathGroups = new Dictionary<string, string>();
		}
		if (group != null && path != null)
		{
			suggestedPathGroups.Remove(group);
			suggestedPathGroups.Add(group, path);
		}
	}

	public override bool StoreJSON(JSONClass jc, bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		bool flag = NeedsStore(jc, includePhysical, includeAppearance) && (forceStore || _val != defaultVal);
		if (flag)
		{
			if (Regex.IsMatch(val, "^http"))
			{
				jc[name] = val;
			}
			else if (SuperController.singleton != null)
			{
				jc[name] = SuperController.singleton.NormalizeSavePath(val);
			}
			else
			{
				jc[name] = val;
			}
		}
		return flag;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		if (!NeedsRestore(jc, restorePhysical, restoreAppearance))
		{
			return;
		}
		if (jc[name] != null)
		{
			string path = jc[name];
			if (SuperController.singleton != null)
			{
				path = SuperController.singleton.NormalizeLoadPath(path);
			}
			val = path;
		}
		else if (setMissingToDefault)
		{
			val = defaultVal;
		}
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		if (!NeedsLateRestore(jc, restorePhysical, restoreAppearance))
		{
			return;
		}
		if (jc[name] != null)
		{
			string path = jc[name];
			if (SuperController.singleton != null)
			{
				path = SuperController.singleton.NormalizeLoadPath(path);
			}
			val = path;
		}
		else if (setMissingToDefault)
		{
			val = defaultVal;
		}
	}

	protected void ReplaceSuggestedPathGroupPathWithActivePath()
	{
		if (suggestedPathGroup != null)
		{
			suggestedPathGroups.Remove(suggestedPathGroup);
			suggestedPathGroups.Add(suggestedPathGroup, _activeSuggestedPath);
		}
	}

	public void SetFilePath(string path)
	{
		if (path == null || !(path != string.Empty))
		{
			return;
		}
		path = SuperController.singleton.NormalizeMediaPath(path);
		_activeSuggestedPath = FileManager.GetDirectoryName(path, true);
		if (allowBrowseAboveSuggestedPath || _normalizedSuggestedPath == null)
		{
			_activeSuggestedPath = _activeSuggestedPath.Replace('/', '\\');
			ReplaceSuggestedPathGroupPathWithActivePath();
		}
		else
		{
			string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(_normalizedSuggestedPath, _activeSuggestedPath);
			if (suggestedBrowserDirectoryFromDirectoryPath != null)
			{
				_activeSuggestedPath = suggestedBrowserDirectoryFromDirectoryPath;
				ReplaceSuggestedPathGroupPathWithActivePath();
			}
		}
		if (_val != path)
		{
			val = path;
		}
		else if (_forceCallbackOnSet)
		{
			if (setCallbackFunction != null)
			{
				setCallbackFunction(_val);
			}
			if (setJSONCallbackFunction != null)
			{
				setJSONCallbackFunction(this);
			}
		}
	}

	protected void SetFilePathBrowseCallback(string path, bool didClose)
	{
		_valueSetFromBrowse = true;
		SetFilePath(path);
		_valueSetFromBrowse = false;
		if (didClose)
		{
			if (endBrowseCallback != null)
			{
				endBrowseCallback();
			}
			if (endBrowseWithObjectCallback != null)
			{
				endBrowseWithObjectCallback(this);
			}
		}
	}

	public void FileBrowse()
	{
		if (beginBrowseCallback != null)
		{
			beginBrowseCallback();
		}
		if (beginBrowseWithObjectCallback != null)
		{
			beginBrowseWithObjectCallback(this);
		}
		if (SuperController.singleton != null)
		{
			if (suggestedPathGroup != null)
			{
				suggestedPathGroups.TryGetValue(suggestedPathGroup, out _activeSuggestedPath);
			}
			if (_activeSuggestedPath == null || _activeSuggestedPath == string.Empty)
			{
				_activeSuggestedPath = _suggestedPath;
				ReplaceSuggestedPathGroupPathWithActivePath();
			}
			else if (!FileManager.DirectoryExists(_activeSuggestedPath))
			{
				_activeSuggestedPath = _suggestedPath;
				ReplaceSuggestedPathGroupPathWithActivePath();
			}
			SuperController.singleton.GetMediaPathDialog(SetFilePathBrowseCallback, _filter, _activeSuggestedPath, allowFullComputerBrowse, showDirs, false, fileRemovePrefix, hideExtension, shortCuts, true, allowBrowseAboveSuggestedPath);
		}
	}

	public void RegisterFileBrowseButton(Button b, bool isAlt = false)
	{
		if (isAlt)
		{
			fileBrowseButtonAlt = b;
		}
		else
		{
			fileBrowseButton = b;
		}
	}

	public void CopyToClipboard()
	{
		GUIUtility.systemCopyBuffer = _val;
	}

	public void RegisterCopyToClipboardButton(Button b, bool isAlt = false)
	{
		if (isAlt)
		{
			copyToClipboardButtonAlt = b;
		}
		else
		{
			copyToClipboardButton = b;
		}
	}

	public void CopyFromClipboard()
	{
		val = GUIUtility.systemCopyBuffer;
	}

	public void RegisterCopyFromClipboardButton(Button b, bool isAlt = false)
	{
		if (isAlt)
		{
			copyFromClipboardButtonAlt = b;
		}
		else
		{
			copyFromClipboardButton = b;
		}
	}

	public void Clear()
	{
		if (_val != string.Empty)
		{
			val = string.Empty;
		}
		else if (_forceCallbackOnSet)
		{
			if (setCallbackFunction != null)
			{
				setCallbackFunction(string.Empty);
			}
			if (setJSONCallbackFunction != null)
			{
				setJSONCallbackFunction(this);
			}
		}
	}

	public void RegisterClearButton(Button b, bool isAlt = false)
	{
		if (isAlt)
		{
			clearButtonAlt = b;
		}
		else
		{
			clearButton = b;
		}
	}

	public void Reload()
	{
		_valueSetFromBrowse = true;
		if (setCallbackFunction != null)
		{
			setCallbackFunction(_val);
		}
		if (setJSONCallbackFunction != null)
		{
			setJSONCallbackFunction(this);
		}
		_valueSetFromBrowse = false;
	}

	public void RegisterReloadButton(Button b, bool isAlt = false)
	{
		if (isAlt)
		{
			reloadButtonAlt = b;
		}
		else
		{
			reloadButton = b;
		}
	}
}
