using System.Collections.Generic;
using UnityEngine;

public class ObjectChooser : JSONStorable
{
	public delegate void ChoiceChanged(string s);

	public Transform choiceContainer;

	public string startingChoiceName;

	public bool addNoneChoice;

	protected ObjectChoice[] choices;

	protected List<string> choiceNames;

	protected Dictionary<string, ObjectChoice> choiceNameToObjectChoice;

	public ChoiceChanged onChoiceChangedHandlers;

	public bool useStoreIdForChooserDisplayName;

	public JSONStorableStringChooser chooserJSON;

	public ObjectChoice CurrentChoice { get; private set; }

	protected void SyncChoice(string s)
	{
		ObjectChoice value;
		if (addNoneChoice && s == "None")
		{
			if (CurrentChoice != null)
			{
				CurrentChoice.gameObject.SetActive(false);
			}
			CurrentChoice = null;
			if (onChoiceChangedHandlers != null)
			{
				onChoiceChangedHandlers("None");
			}
		}
		else if (choiceNameToObjectChoice.TryGetValue(s, out value))
		{
			if (CurrentChoice != null)
			{
				CurrentChoice.gameObject.SetActive(false);
			}
			CurrentChoice = value;
			value.gameObject.SetActive(true);
			if (onChoiceChangedHandlers != null)
			{
				onChoiceChangedHandlers(s);
			}
		}
	}

	protected virtual void Init()
	{
		choiceNames = new List<string>();
		if (addNoneChoice)
		{
			choiceNames.Add("None");
		}
		if (choiceContainer != null)
		{
			choices = choiceContainer.GetComponentsInChildren<ObjectChoice>(true);
		}
		else
		{
			choices = GetComponentsInChildren<ObjectChoice>(true);
		}
		choiceNameToObjectChoice = new Dictionary<string, ObjectChoice>();
		ObjectChoice[] array = choices;
		foreach (ObjectChoice objectChoice in array)
		{
			choiceNames.Add(objectChoice.displayName);
			choiceNameToObjectChoice.Add(objectChoice.displayName, objectChoice);
			if (startingChoiceName == null || startingChoiceName == string.Empty)
			{
				if (objectChoice.gameObject.activeSelf)
				{
					CurrentChoice = objectChoice;
					startingChoiceName = CurrentChoice.displayName;
				}
			}
			else if (objectChoice.displayName == startingChoiceName)
			{
				CurrentChoice = objectChoice;
				objectChoice.gameObject.SetActive(true);
			}
			else
			{
				objectChoice.gameObject.SetActive(false);
			}
		}
		if (useStoreIdForChooserDisplayName)
		{
			chooserJSON = new JSONStorableStringChooser("choiceName", choiceNames, startingChoiceName, base.storeId, SyncChoice);
		}
		else
		{
			chooserJSON = new JSONStorableStringChooser("choiceName", choiceNames, startingChoiceName, null, SyncChoice);
		}
		RegisterStringChooser(chooserJSON);
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		ObjectChoice[] array = choices;
		foreach (ObjectChoice objectChoice in array)
		{
			JSONStorable[] componentsInChildren = objectChoice.GetComponentsInChildren<JSONStorable>(true);
			JSONStorable[] array2 = componentsInChildren;
			foreach (JSONStorable jSONStorable in array2)
			{
				jSONStorable.SetUI(UITransform);
			}
		}
		ObjectChooserUI componentInChildren = UITransform.GetComponentInChildren<ObjectChooserUI>();
		if (componentInChildren != null)
		{
			chooserJSON.popup = componentInChildren.chooserPopup;
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		ObjectChoice[] array = choices;
		foreach (ObjectChoice objectChoice in array)
		{
			JSONStorable[] componentsInChildren = objectChoice.GetComponentsInChildren<JSONStorable>(true);
			JSONStorable[] array2 = componentsInChildren;
			foreach (JSONStorable jSONStorable in array2)
			{
				jSONStorable.SetUIAlt(UITransformAlt);
			}
		}
		ObjectChooserUI componentInChildren = UITransformAlt.GetComponentInChildren<ObjectChooserUI>(true);
		if (componentInChildren != null)
		{
			chooserJSON.popupAlt = componentInChildren.chooserPopup;
		}
	}

	public void ForceAwake()
	{
		Awake();
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
