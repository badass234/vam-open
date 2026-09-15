using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateDAZCharacterSelectorUI : GenerateTabbedUI
{
	public DAZCharacterSelector characterSelector;

	private bool ignoreClick;

	protected DAZCharacter[] characters;

	protected Dictionary<string, Toggle> characterNameToToggle;

	private void OnClick(string characterName)
	{
		if (characterSelector != null && !ignoreClick)
		{
			characterSelector.SelectCharacterByName(characterName);
		}
	}

	public void SetActiveCharacterToggle(string characterName)
	{
		if (characters == null || characterNameToToggle == null)
		{
			return;
		}
		for (int i = 0; i < characters.Length; i++)
		{
			string displayName = characters[i].displayName;
			Toggle value;
			if (characterNameToToggle.TryGetValue(displayName, out value))
			{
				if (displayName == characterName)
				{
					value.isOn = true;
				}
				else
				{
					value.isOn = false;
				}
			}
		}
	}

	public void SetActiveCharacterToggleNoCallback(string characterName)
	{
		ignoreClick = true;
		SetActiveCharacterToggle(characterName);
		ignoreClick = false;
	}

	public override void TabChange(string name, bool on)
	{
		characterNameToToggle = new Dictionary<string, Toggle>();
		base.TabChange(name, on);
	}

	protected override Transform InstantiateControl(Transform parent, int index)
	{
		Transform transform = base.InstantiateControl(parent, index);
		DAZCharacter dAZCharacter = characters[index];
		string displayName = dAZCharacter.displayName;
		Toggle component = transform.GetComponent<Toggle>();
		if (component != null)
		{
			string cname = displayName;
			characterNameToToggle.Add(cname, component);
			component.onValueChanged.AddListener(delegate(bool arg0)
			{
				if (arg0)
				{
					OnClick(cname);
				}
			});
			if (characterSelector.selectedCharacter != null && characterSelector.selectedCharacter.displayName == displayName)
			{
				component.isOn = true;
			}
			else
			{
				component.isOn = false;
			}
		}
		foreach (Transform item in transform)
		{
			if (item.name == "Text")
			{
				Text component2 = item.GetComponent<Text>();
				if (component2 != null)
				{
					component2.text = displayName;
				}
			}
			else if (item.name == "TextAlt")
			{
				Text component3 = item.GetComponent<Text>();
				if (component3 != null)
				{
					component3.text = dAZCharacter.displayNameAlt;
				}
			}
			else if (item.name == "UVText")
			{
				Text component4 = item.GetComponent<Text>();
				if (component4 != null)
				{
					component4.text = dAZCharacter.UVname;
				}
			}
			else if (item.name == "RawImage")
			{
				RawImage component5 = item.GetComponent<RawImage>();
				if (component5 != null)
				{
					component5.texture = characters[index].thumbnail;
				}
			}
		}
		return transform;
	}

	public void AutoGenerateItems()
	{
		if (characterSelector != null)
		{
			characters = characterSelector.characters;
		}
	}

	protected override void Generate()
	{
		AutoGenerateItems();
		base.Generate();
		characterNameToToggle = new Dictionary<string, Toggle>();
		if (controlUIPrefab != null && tabUIPrefab != null && tabButtonUIPrefab != null && characters != null)
		{
			for (int i = 0; i < characters.Length; i++)
			{
				AllocateControl();
			}
		}
	}
}
