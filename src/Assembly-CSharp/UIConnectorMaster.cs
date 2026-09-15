using System.Collections.Generic;
using UnityEngine;

public class UIConnectorMaster : MonoBehaviour
{
	public Atom containingAtom;

	public Transform preInstantiatedUI;

	protected Transform instance;

	public Transform UIPrefab;

	public bool configureTabbedUIBuilder;

	public int tabLimit = 20;

	public string startingTab;

	protected string alternateStartingTab;

	public TabbedUIBuilder.Tab[] tabs;

	protected Dictionary<TabbedUIBuilder.Tab, bool> runtimeAddedTabs;

	protected Canvas[] canvases;

	protected bool _enabled;

	protected virtual void InitInstance()
	{
		TabbedUIBuilder componentInChildren = instance.GetComponentInChildren<TabbedUIBuilder>();
		if (componentInChildren != null)
		{
			if (configureTabbedUIBuilder)
			{
				if (tabs.Length > tabLimit)
				{
					List<TabbedUIBuilder.Tab> list = new List<TabbedUIBuilder.Tab>();
					List<TabbedUIBuilder.Tab> list2 = new List<TabbedUIBuilder.Tab>();
					for (int i = 0; i < tabs.Length; i++)
					{
						if (i >= tabLimit)
						{
							list2.Add(tabs[i]);
						}
						else
						{
							list.Add(tabs[i]);
						}
					}
					componentInChildren.tabs = list.ToArray();
					componentInChildren.column2Tabs = list2.ToArray();
				}
				else
				{
					componentInChildren.tabs = tabs;
				}
				componentInChildren.startingTab = startingTab;
				componentInChildren.alternateStartingTab = alternateStartingTab;
			}
			componentInChildren.Build();
		}
		UIConnector[] components = GetComponents<UIConnector>();
		UIConnector[] array = components;
		foreach (UIConnector uIConnector in array)
		{
			if (!uIConnector.disable)
			{
				uIConnector.Connect();
			}
		}
		UIMultiConnector[] components2 = GetComponents<UIMultiConnector>();
		UIMultiConnector[] array2 = components2;
		foreach (UIMultiConnector uIMultiConnector in array2)
		{
			if (!uIMultiConnector.disable)
			{
				uIMultiConnector.Connect();
			}
		}
	}

	public virtual void AddTab(TabbedUIBuilder.Tab newTab, string addBeforeTabName = null)
	{
		bool flag = false;
		int num = -1;
		if (tabs != null)
		{
			for (int i = 0; i < tabs.Length; i++)
			{
				if (tabs[i].name == newTab.name)
				{
					flag = true;
				}
				if (addBeforeTabName != null && tabs[i].name == addBeforeTabName)
				{
					num = i;
				}
			}
		}
		if (flag)
		{
			return;
		}
		int num2 = 0;
		num2 = ((tabs == null) ? 1 : (tabs.Length + 1));
		List<TabbedUIBuilder.Tab> list = new List<TabbedUIBuilder.Tab>();
		for (int j = 0; j < num2 - 1; j++)
		{
			if (num == j)
			{
				list.Add(newTab);
			}
			list.Add(tabs[j]);
		}
		if (num == -1)
		{
			list.Add(newTab);
		}
		if (Application.isPlaying)
		{
			if (runtimeAddedTabs == null)
			{
				runtimeAddedTabs = new Dictionary<TabbedUIBuilder.Tab, bool>();
			}
			runtimeAddedTabs.Add(newTab, true);
		}
		TabbedUIBuilder.Tab[] array = list.ToArray();
		tabs = array;
	}

	public virtual void RemoveTab(string tabName)
	{
		List<TabbedUIBuilder.Tab> list = new List<TabbedUIBuilder.Tab>();
		if (tabs == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < tabs.Length; i++)
		{
			if (tabs[i].name == tabName)
			{
				flag = true;
				if (runtimeAddedTabs != null && runtimeAddedTabs.ContainsKey(tabs[i]))
				{
					runtimeAddedTabs.Remove(tabs[i]);
				}
			}
			else
			{
				list.Add(tabs[i]);
			}
		}
		if (flag)
		{
			tabs = list.ToArray();
		}
	}

	public virtual void ClearRuntimeTabs(bool skipRebuild = false)
	{
		if (!Application.isPlaying || tabs == null || runtimeAddedTabs == null)
		{
			return;
		}
		List<TabbedUIBuilder.Tab> list = new List<TabbedUIBuilder.Tab>();
		bool flag = false;
		for (int i = 0; i < tabs.Length; i++)
		{
			if (runtimeAddedTabs.ContainsKey(tabs[i]))
			{
				flag = true;
			}
			else
			{
				list.Add(tabs[i]);
			}
		}
		if (flag)
		{
			tabs = list.ToArray();
			if (!skipRebuild)
			{
				Rebuild();
			}
		}
		runtimeAddedTabs = new Dictionary<TabbedUIBuilder.Tab, bool>();
	}

	public virtual void Rebuild()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (instance != null)
		{
			TabbedUIBuilder componentInChildren = instance.GetComponentInChildren<TabbedUIBuilder>();
			if (componentInChildren != null)
			{
				alternateStartingTab = componentInChildren.activeTabName;
			}
			DeregisterCanvases();
			UIConnector[] components = GetComponents<UIConnector>();
			UIConnector[] array = components;
			foreach (UIConnector uIConnector in array)
			{
				if (!uIConnector.disable)
				{
					uIConnector.Disconnect();
				}
			}
			UIMultiConnector[] components2 = GetComponents<UIMultiConnector>();
			UIMultiConnector[] array2 = components2;
			foreach (UIMultiConnector uIMultiConnector in array2)
			{
				if (!uIMultiConnector.disable)
				{
					uIMultiConnector.Disconnect();
				}
			}
			instance.SetParent(null);
			Object.Destroy(instance.gameObject);
			instance = null;
		}
		CreateInstance();
		RegisterCanvases();
	}

	protected void RegisterCanvases()
	{
		if (_enabled && instance != null && containingAtom != null && canvases != null)
		{
			Canvas[] array = canvases;
			foreach (Canvas c in array)
			{
				containingAtom.AddCanvas(c);
			}
		}
	}

	protected void DeregisterCanvases()
	{
		if (instance != null && containingAtom != null && canvases != null)
		{
			Canvas[] array = canvases;
			foreach (Canvas c in array)
			{
				containingAtom.RemoveCanvas(c);
			}
		}
	}

	protected virtual void CreateInstance()
	{
		if (!(instance == null))
		{
			return;
		}
		if (preInstantiatedUI != null)
		{
			instance = preInstantiatedUI;
			InitInstance();
		}
		else if (UIPrefab != null)
		{
			instance = Object.Instantiate(UIPrefab);
			if (instance != null)
			{
				instance.SetParent(base.transform);
				instance.localPosition = Vector3.zero;
				instance.localRotation = Quaternion.identity;
				instance.localScale = Vector3.one;
			}
			canvases = instance.GetComponentsInChildren<Canvas>();
			InitInstance();
		}
	}

	protected virtual void OnEnable()
	{
		_enabled = true;
		CreateInstance();
		RegisterCanvases();
	}

	protected virtual void OnDisable()
	{
		DeregisterCanvases();
		_enabled = false;
	}
}
