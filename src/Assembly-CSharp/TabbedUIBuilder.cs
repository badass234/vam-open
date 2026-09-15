using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class TabbedUIBuilder : MonoBehaviour
{
	[Serializable]
	public class Tab
	{
		public string name;

		public Transform prefab;

		public Color color;
	}

	public Transform tabPrefab;

	public Transform column2TabPrefab;

	public float column2Offset = 200f;

	public UITabSelector selector;

	protected float currentYPos;

	protected float tabWidth;

	protected float maxHeight;

	public string startingTab;

	public Tab[] tabs;

	public Tab[] column2Tabs;

	protected bool wasBuilt;

	public string alternateStartingTab { get; set; }

	public string activeTabName
	{
		get
		{
			if (selector != null)
			{
				return selector.activeTabName;
			}
			return null;
		}
	}

	protected void CopyFields(Type t, object source, object dest)
	{
		if (source.GetType() != dest.GetType())
		{
			return;
		}
		FieldInfo[] fields = t.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			try
			{
				object value = fieldInfo.GetValue(source);
				if (object.ReferenceEquals(value, null))
				{
					continue;
				}
				if (value.GetType().IsSubclassOf(typeof(Transform)))
				{
					Transform transform = value as Transform;
					if (transform != null)
					{
						fieldInfo.SetValue(dest, value);
					}
				}
				else
				{
					fieldInfo.SetValue(dest, value);
				}
			}
			catch (Exception ex)
			{
				Debug.Log("Caught exception " + ex);
			}
		}
	}

	public void AddTab(Tab tab, bool addTab = true, bool column2 = false)
	{
		if (selector != null && tab.prefab != null)
		{
			if (tabPrefab != null && addTab)
			{
				Transform transform = ((!column2 || !(column2TabPrefab != null)) ? UnityEngine.Object.Instantiate(tabPrefab) : UnityEngine.Object.Instantiate(column2TabPrefab));
				transform.SetParent(selector.toggleContainer, false);
				transform.name = tab.name;
				RectTransform component = transform.GetComponent<RectTransform>();
				if (component != null)
				{
					Vector2 zero = Vector2.zero;
					zero.y = currentYPos;
					if (column2)
					{
						zero.x += column2Offset;
					}
					component.anchoredPosition = zero;
					Vector2 sizeDelta = component.sizeDelta;
					currentYPos += sizeDelta.y;
					tabWidth = sizeDelta.x;
					if (currentYPos > maxHeight)
					{
						maxHeight = currentYPos;
					}
				}
				UISideAlign component2 = transform.GetComponent<UISideAlign>();
				if (component2 != null)
				{
					component2.Sync();
				}
				Image[] componentsInChildren = transform.GetComponentsInChildren<Image>();
				foreach (Image image in componentsInChildren)
				{
					image.color = tab.color;
				}
				Toggle component3 = transform.GetComponent<Toggle>();
				if (component3 != null)
				{
					component3.onValueChanged.AddListener(delegate
					{
						selector.ActiveTabChanged();
					});
					ToggleGroup component4 = selector.GetComponent<ToggleGroup>();
					if (component4 != null)
					{
						component3.group = component4;
					}
				}
				Text componentInChildren = transform.GetComponentInChildren<Text>();
				if (componentInChildren != null)
				{
					componentInChildren.text = tab.name;
				}
			}
			Transform transform2;
			if (tab.prefab.gameObject.scene.rootCount == 0)
			{
				transform2 = UnityEngine.Object.Instantiate(tab.prefab);
				transform2.SetParent(selector.transform, false);
			}
			else
			{
				transform2 = tab.prefab;
			}
			transform2.name = tab.name;
			Image component5 = transform2.GetComponent<Image>();
			if (component5 != null)
			{
				component5.color = tab.color;
			}
			UIProvider[] componentsInChildren2 = transform2.GetComponentsInChildren<UIProvider>();
			foreach (UIProvider uIProvider in componentsInChildren2)
			{
				Type type = uIProvider.GetType();
				UIProvider uIProvider2 = null;
				if (!uIProvider.completeProvider)
				{
					uIProvider2 = (UIProvider)GetComponent(type);
					if (uIProvider2 == null)
					{
						uIProvider2 = (UIProvider)base.gameObject.AddComponent(type);
					}
					if (uIProvider2 != null)
					{
						CopyFields(type, uIProvider, uIProvider2);
						uIProvider2.completeProvider = true;
					}
				}
			}
		}
		else
		{
			Debug.LogError("Tried to AddTab when UITabSelector is null");
		}
	}

	public void Build()
	{
		if (wasBuilt || tabs == null || tabs.Length <= 0)
		{
			return;
		}
		wasBuilt = true;
		bool addTab = true;
		if (tabs.Length == 1)
		{
			addTab = false;
		}
		if (!(selector != null))
		{
			return;
		}
		GameObject gameObject = new GameObject("ToggleContainer");
		gameObject.transform.SetParent(selector.transform);
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
		selector.toggleContainer = gameObject.transform;
		Vector2 vector = default(Vector2);
		vector.x = 1f;
		vector.y = 0f;
		rectTransform.anchorMin = vector;
		rectTransform.anchorMax = vector;
		Vector2 pivot = (rectTransform.pivot = Vector2.zero);
		Vector2 anchoredPosition = (rectTransform.anchoredPosition = Vector2.zero);
		Vector2 sizeDelta = default(Vector2);
		if (column2Tabs != null && column2Tabs.Length > 0)
		{
			sizeDelta.x = tabWidth * 2f;
		}
		else
		{
			sizeDelta.x = tabWidth;
		}
		sizeDelta.y = maxHeight;
		rectTransform.sizeDelta = sizeDelta;
		UISideAlign uISideAlign = gameObject.AddComponent<UISideAlign>();
		uISideAlign.Sync();
		Tab[] array = tabs;
		foreach (Tab tab in array)
		{
			AddTab(tab, addTab);
		}
		if (column2Tabs != null && column2Tabs.Length > 0)
		{
			currentYPos = 0f;
			Tab[] array2 = column2Tabs;
			foreach (Tab tab2 in array2)
			{
				AddTab(tab2, addTab, true);
			}
		}
		selector.startingTabName = startingTab;
		selector.alternateStartingTabName = alternateStartingTab;
		if (selector.transform.parent != null && selector.transform.parent.parent != null)
		{
			gameObject = new GameObject("TabsBackPanel");
			gameObject.transform.SetParent(selector.transform.parent.parent);
			gameObject.transform.SetAsFirstSibling();
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			rectTransform = gameObject.AddComponent<RectTransform>();
			vector.x = 1f;
			vector.y = 0f;
			rectTransform.anchorMin = vector;
			rectTransform.anchorMax = vector;
			rectTransform.pivot = pivot;
			anchoredPosition.x = -25f;
			anchoredPosition.y = -10f;
			rectTransform.anchoredPosition = anchoredPosition;
			if (column2Tabs != null && column2Tabs.Length > 0)
			{
				sizeDelta.x = tabWidth * 2f + 50f;
			}
			else
			{
				sizeDelta.x = tabWidth + 50f;
			}
			sizeDelta.y = maxHeight + 35f;
			rectTransform.sizeDelta = sizeDelta;
			uISideAlign = gameObject.AddComponent<UISideAlign>();
			uISideAlign.leftSideOffsetX = 25f;
			uISideAlign.rightSideOffsetX = -25f;
			uISideAlign.Sync();
			Image image = gameObject.AddComponent<Image>();
			Color color = image.color;
			color.a = 0.02f;
			image.color = color;
			gameObject = new GameObject("BackPanel");
			gameObject.transform.SetParent(selector.transform.parent.parent);
			gameObject.transform.SetAsFirstSibling();
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			rectTransform = gameObject.AddComponent<RectTransform>();
			vector.x = 0f;
			vector.y = 0f;
			rectTransform.anchorMin = vector;
			Vector2 anchorMax = default(Vector2);
			anchorMax.x = 1f;
			anchorMax.y = 1f;
			rectTransform.anchorMax = anchorMax;
			pivot.x = 0.5f;
			pivot.y = 0.5f;
			rectTransform.pivot = pivot;
			anchoredPosition.x = 0f;
			anchoredPosition.y = 7.5f;
			rectTransform.anchoredPosition = anchoredPosition;
			sizeDelta.x = 50f;
			sizeDelta.y = 35f;
			rectTransform.sizeDelta = sizeDelta;
			image = gameObject.AddComponent<Image>();
			image.color = color;
		}
	}

	private void Awake()
	{
		Build();
	}
}
