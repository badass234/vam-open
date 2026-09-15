using System.Collections.Generic;
using GPUTools.Cloth.Scripts;
using GPUTools.Painter.Scripts;
using UnityEngine;

public class DAZSkinWrapSwitcher : JSONStorable
{
	protected DAZSkinWrap _currentWrap;

	protected ClothSettings clothSettings;

	protected JSONStorableStringChooser currentWrapNameJSON;

	[SerializeField]
	protected string _currentWrapName;

	public string currentWrapName
	{
		get
		{
			return _currentWrapName;
		}
		set
		{
			if (currentWrapNameJSON != null)
			{
				currentWrapNameJSON.val = value;
			}
			else if (_currentWrapName != value)
			{
				SetCurrentWrapName(value);
			}
		}
	}

	public void SetCurrentWrapName(string wrapName)
	{
		DAZSkinWrap[] components = GetComponents<DAZSkinWrap>();
		_currentWrapName = null;
		_currentWrap = null;
		DAZSkinWrap[] array = components;
		foreach (DAZSkinWrap dAZSkinWrap in array)
		{
			if (!dAZSkinWrap.enabled)
			{
				continue;
			}
			if (clothSettings != null)
			{
				dAZSkinWrap.draw = false;
				if (!(dAZSkinWrap.wrapName == wrapName))
				{
					continue;
				}
				clothSettings.MeshProvider.PreCalcProvider = dAZSkinWrap;
				PainterSettings[] componentsInChildren = GetComponentsInChildren<PainterSettings>(true);
				PainterSettings painterSettings = null;
				PainterSettings painterSettings2 = null;
				string text = base.name + "Painter";
				string text2 = base.name + "Painter" + wrapName;
				PainterSettings[] array2 = componentsInChildren;
				foreach (PainterSettings painterSettings3 in array2)
				{
					if (painterSettings3.name == text)
					{
						painterSettings = painterSettings3;
					}
					else if (painterSettings3.name == text2)
					{
						painterSettings2 = painterSettings3;
					}
				}
				bool flag = false;
				if (painterSettings2 != null)
				{
					if (clothSettings.EditorPainter != painterSettings2)
					{
						flag = true;
						clothSettings.EditorPainter = painterSettings2;
					}
				}
				else if (painterSettings != null && clothSettings.EditorPainter != painterSettings)
				{
					flag = true;
					clothSettings.EditorPainter = painterSettings;
				}
				if (flag && clothSettings.builder != null)
				{
					if (clothSettings.builder.physicsBlend != null)
					{
						clothSettings.builder.physicsBlend.Build();
					}
					if (clothSettings.builder.pointJoints != null)
					{
						clothSettings.builder.pointJoints.UpdateSettings();
					}
				}
				_currentWrapName = wrapName;
				_currentWrap = dAZSkinWrap;
				dAZSkinWrap.draw = true;
			}
			else
			{
				dAZSkinWrap.draw = false;
				if (dAZSkinWrap.wrapName == wrapName)
				{
					_currentWrapName = wrapName;
					_currentWrap = dAZSkinWrap;
					dAZSkinWrap.draw = true;
				}
			}
		}
		SyncSkinWrap();
	}

	protected void SyncSkinWrap()
	{
		DAZSkinWrapMaterialOptions[] components = GetComponents<DAZSkinWrapMaterialOptions>();
		DAZSkinWrapMaterialOptions[] array = components;
		foreach (DAZSkinWrapMaterialOptions dAZSkinWrapMaterialOptions in array)
		{
			dAZSkinWrapMaterialOptions.skinWrap = _currentWrap;
		}
		DAZSkinWrapControl component = GetComponent<DAZSkinWrapControl>();
		if (component != null)
		{
			component.wrap = _currentWrap;
		}
	}

	public void Init()
	{
		DAZSkinWrap[] components = GetComponents<DAZSkinWrap>();
		List<string> list = new List<string>();
		clothSettings = GetComponent<ClothSettings>();
		DAZSkinWrap[] array = components;
		foreach (DAZSkinWrap dAZSkinWrap in array)
		{
			if (dAZSkinWrap.enabled)
			{
				list.Add(dAZSkinWrap.wrapName);
				if (clothSettings != null && clothSettings.MeshProvider.PreCalcProvider != null)
				{
					_currentWrap = clothSettings.MeshProvider.PreCalcProvider as DAZSkinWrap;
					_currentWrapName = _currentWrap.wrapName;
				}
				else if (dAZSkinWrap.draw)
				{
					_currentWrap = dAZSkinWrap;
					_currentWrapName = _currentWrap.wrapName;
				}
			}
		}
		if (list.Count > 1 && _currentWrap != null)
		{
			if (currentWrapNameJSON == null)
			{
				currentWrapNameJSON = new JSONStorableStringChooser("wrapName", list, _currentWrapName, null, SetCurrentWrapName);
				RegisterStringChooser(currentWrapNameJSON);
			}
			else
			{
				currentWrapNameJSON.choices = list;
				currentWrapNameJSON.valNoCallback = _currentWrapName;
			}
			SetCurrentWrapName(_currentWrapName);
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		DAZSkinWrapSwitcherUI componentInChildren = UITransform.GetComponentInChildren<DAZSkinWrapSwitcherUI>(true);
		if (componentInChildren != null && componentInChildren.currentWrapNamePopup != null)
		{
			if (currentWrapNameJSON != null)
			{
				componentInChildren.currentWrapNamePopup.gameObject.SetActive(true);
				currentWrapNameJSON.popup = componentInChildren.currentWrapNamePopup;
			}
			else
			{
				componentInChildren.currentWrapNamePopup.gameObject.SetActive(false);
			}
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		DAZSkinWrapSwitcherUI componentInChildren = UITransformAlt.GetComponentInChildren<DAZSkinWrapSwitcherUI>(true);
		if (componentInChildren != null && componentInChildren.currentWrapNamePopup != null)
		{
			if (currentWrapNameJSON != null)
			{
				componentInChildren.currentWrapNamePopup.gameObject.SetActive(true);
				currentWrapNameJSON.popupAlt = componentInChildren.currentWrapNamePopup;
			}
			else
			{
				componentInChildren.currentWrapNamePopup.gameObject.SetActive(false);
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
