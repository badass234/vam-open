using System;
using System.Collections.Generic;
using UnityEngine;

public class HairSimScalpMaskToolControl : CapsuleToolControl
{
	public enum ToolChoice
	{
		Mask,
		Unmask
	}

	public SelectableUnselect maskTool;

	public SelectableSelect unmaskTool;

	public ToolChoice toolChoice;

	protected JSONStorableStringChooser toolChoiceJSON;

	protected void SyncTool()
	{
		maskTool.enabled = false;
		unmaskTool.enabled = false;
		switch (toolChoice)
		{
		case ToolChoice.Mask:
			maskTool.enabled = true;
			break;
		case ToolChoice.Unmask:
			unmaskTool.enabled = true;
			break;
		}
	}

	protected void SyncToolChoice(string s)
	{
		try
		{
			toolChoice = (ToolChoice)Enum.Parse(typeof(ToolChoice), s);
			SyncTool();
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set tool to " + s + " which is not a valid ToolChoice");
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (!wasInit)
		{
			return;
		}
		base.InitUI(t, isAlt);
		if (t != null)
		{
			HairSimScalpMaskToolControlUI componentInChildren = UITransform.GetComponentInChildren<HairSimScalpMaskToolControlUI>();
			if (componentInChildren != null)
			{
				toolChoiceJSON.RegisterPopup(componentInChildren.toolChoicePopup, isAlt);
			}
		}
		SyncTool();
	}

	protected override void Init()
	{
		base.Init();
		List<string> choicesList = new List<string>(Enum.GetNames(typeof(ToolChoice)));
		toolChoiceJSON = new JSONStorableStringChooser("toolChoice", choicesList, toolChoice.ToString(), "Tool", SyncToolChoice);
		RegisterStringChooser(toolChoiceJSON);
		SyncTool();
	}
}
