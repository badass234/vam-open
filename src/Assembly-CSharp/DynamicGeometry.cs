using System;
using UnityEngine;

public class DynamicGeometry : JSONStorable
{
	public ObjectChooser[] choosers;

	public RectTransform chooserPopupPrefab;

	public RectTransform materialUIPrefab;

	public RectTransform imageControlUIPrefab;

	public RectTransform audioSourceControlUIPrefab;

	public Color materialUITabColor;

	public Color imageControlUITabColor;

	public Color audioSourceControlUITabColor;

	public UIConnectorMaster materialUIConnectorMaster;

	protected UIMultiConnector materialUIConnector;

	protected UIMultiConnector imageControlUIConnector;

	protected UIMultiConnector audioSourceControlUIConnector;

	protected bool isSyncingUITabs;

	protected void SyncUITabs()
	{
		if (!(materialUIConnector != null) || !(materialUIConnectorMaster != null))
		{
			return;
		}
		materialUIConnector.ClearConnectors();
		materialUIConnectorMaster.ClearRuntimeTabs(true);
		MaterialOptions[] componentsInChildren = GetComponentsInChildren<MaterialOptions>(false);
		MaterialOptions[] array = componentsInChildren;
		foreach (MaterialOptions materialOptions in array)
		{
			materialUIConnector.AddConnector(materialOptions);
			TabbedUIBuilder.Tab tab = new TabbedUIBuilder.Tab();
			tab.name = materialOptions.storeId;
			tab.prefab = materialUIPrefab;
			tab.color = materialUITabColor;
			materialUIConnectorMaster.AddTab(tab);
		}
		if (imageControlUIConnector != null)
		{
			imageControlUIConnector.ClearConnectors();
			ImageControl[] componentsInChildren2 = GetComponentsInChildren<ImageControl>(false);
			ImageControl[] array2 = componentsInChildren2;
			foreach (ImageControl imageControl in array2)
			{
				imageControlUIConnector.AddConnector(imageControl);
				TabbedUIBuilder.Tab tab2 = new TabbedUIBuilder.Tab();
				tab2.name = imageControl.storeId;
				tab2.prefab = imageControlUIPrefab;
				tab2.color = imageControlUITabColor;
				materialUIConnectorMaster.AddTab(tab2);
			}
		}
		if (audioSourceControlUIConnector != null)
		{
			audioSourceControlUIConnector.ClearConnectors();
			AudioSourceControl[] componentsInChildren3 = GetComponentsInChildren<AudioSourceControl>(false);
			AudioSourceControl[] array3 = componentsInChildren3;
			foreach (AudioSourceControl audioSourceControl in array3)
			{
				audioSourceControlUIConnector.AddConnector(audioSourceControl);
				TabbedUIBuilder.Tab tab3 = new TabbedUIBuilder.Tab();
				tab3.name = audioSourceControl.storeId;
				tab3.prefab = audioSourceControlUIPrefab;
				tab3.color = audioSourceControlUITabColor;
				materialUIConnectorMaster.AddTab(tab3);
			}
		}
	}

	protected void GeometryChanged(string newChoice)
	{
		SyncUITabs();
		materialUIConnectorMaster.Rebuild();
	}

	protected virtual void Init()
	{
		if (choosers != null)
		{
			ObjectChooser[] array = choosers;
			foreach (ObjectChooser objectChooser in array)
			{
				if (objectChooser != null)
				{
					objectChooser.ForceAwake();
					objectChooser.onChoiceChangedHandlers = (ObjectChooser.ChoiceChanged)Delegate.Combine(objectChooser.onChoiceChangedHandlers, new ObjectChooser.ChoiceChanged(GeometryChanged));
				}
			}
		}
		if (!(materialUIConnectorMaster != null))
		{
			return;
		}
		UIMultiConnector[] components = materialUIConnectorMaster.GetComponents<UIMultiConnector>();
		UIMultiConnector[] array2 = components;
		foreach (UIMultiConnector uIMultiConnector in array2)
		{
			if (uIMultiConnector.typeToConnect.Type == typeof(MaterialOptions))
			{
				materialUIConnector = uIMultiConnector;
			}
			else if (uIMultiConnector.typeToConnect.Type == typeof(ImageControl))
			{
				imageControlUIConnector = uIMultiConnector;
			}
			else if (uIMultiConnector.typeToConnect.Type == typeof(AudioSourceControl))
			{
				audioSourceControlUIConnector = uIMultiConnector;
			}
		}
		SyncUITabs();
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (!(t != null) || isSyncingUITabs)
		{
			return;
		}
		DynamicGeometryUI componentInChildren = t.GetComponentInChildren<DynamicGeometryUI>(true);
		if (!(componentInChildren != null))
		{
			return;
		}
		RectTransform chooserPopupContainer = componentInChildren.chooserPopupContainer;
		if (!(chooserPopupContainer != null))
		{
			return;
		}
		ObjectChooser[] array = choosers;
		foreach (ObjectChooser objectChooser in array)
		{
			if (objectChooser != null)
			{
				RectTransform rectTransform = UnityEngine.Object.Instantiate(chooserPopupPrefab);
				rectTransform.SetParent(chooserPopupContainer, false);
				UIPopup componentInChildren2 = rectTransform.GetComponentInChildren<UIPopup>();
				objectChooser.chooserJSON.popup = componentInChildren2;
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
