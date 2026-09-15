using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class NamedAudioClip
{
	public AudioClipManager manager;

	public AudioClip sourceClip;

	public bool destroyed;

	public Text uidText;

	[SerializeField]
	protected string _uid;

	public InputField displayNameField;

	[SerializeField]
	protected string _displayName;

	public InputField categoryField;

	[SerializeField]
	protected string _category;

	public Button testButton;

	public Text testButtonText;

	public AudioClip clipToPlay => sourceClip;

	public string uid
	{
		get
		{
			return _uid;
		}
		set
		{
			if (_uid != value)
			{
				_uid = value;
				if (uidText != null)
				{
					uidText.text = _uid;
				}
			}
		}
	}

	public string displayName
	{
		get
		{
			return _displayName;
		}
		set
		{
			if (_displayName != value)
			{
				_displayName = value;
				if (displayNameField != null)
				{
					displayNameField.text = _displayName;
				}
			}
		}
	}

	public string category
	{
		get
		{
			return _category;
		}
		set
		{
			if (_category != value)
			{
				_category = value;
				if (categoryField != null)
				{
					categoryField.text = _category;
				}
			}
		}
	}

	protected void SetDisplayName(string v)
	{
		displayName = v;
	}

	protected void SetCategory(string c)
	{
		category = c;
	}

	public void Test()
	{
		if (manager != null)
		{
			manager.TestClip(this);
		}
	}

	public virtual void InitUI()
	{
		if (uidText != null)
		{
			uidText.text = _uid;
		}
		if (displayNameField != null)
		{
			displayNameField.text = _displayName;
			displayNameField.onEndEdit.AddListener(SetDisplayName);
		}
		if (categoryField != null)
		{
			categoryField.text = _category;
			categoryField.onEndEdit.AddListener(SetCategory);
		}
		if (testButton != null)
		{
			testButton.onClick.AddListener(Test);
		}
	}
}
