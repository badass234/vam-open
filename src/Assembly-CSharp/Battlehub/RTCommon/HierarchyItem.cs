using UnityEngine;

namespace Battlehub.RTCommon
{
	public class HierarchyItem : MonoBehaviour
	{
		private ExposeToEditor m_parentExp;

		private ExposeToEditor m_exposeToEditor;

		private Transform m_parentTransform;

		private bool m_isAwaked;

		private void Awake()
		{
			m_exposeToEditor = GetComponent<ExposeToEditor>();
			if (base.transform.parent != null)
			{
				m_parentExp = CreateChainToParent(base.transform.parent);
				m_parentTransform = base.transform.parent;
			}
			m_isAwaked = true;
		}

		private ExposeToEditor CreateChainToParent(Transform parent)
		{
			ExposeToEditor exposeToEditor = null;
			if (parent != null)
			{
				exposeToEditor = parent.GetComponentInParent<ExposeToEditor>();
			}
			if (exposeToEditor == null)
			{
				return null;
			}
			while (parent != null && parent.gameObject != exposeToEditor.gameObject)
			{
				if (!parent.GetComponent<ExposeToEditor>() && !parent.GetComponent<HierarchyItem>())
				{
					parent.gameObject.AddComponent<HierarchyItem>();
				}
				parent = parent.parent;
			}
			return exposeToEditor;
		}

		private void TryDestroyChainToParent(Transform parent, ExposeToEditor parentExp)
		{
			if (parentExp == null)
			{
				return;
			}
			while (parent != null && parent.gameObject != parentExp.gameObject)
			{
				if (!parent.GetComponent<ExposeToEditor>())
				{
					HierarchyItem component = parent.GetComponent<HierarchyItem>();
					if ((bool)component && !HasExposeToEditorChildren(parent))
					{
						Object.Destroy(component);
					}
				}
				parent = parent.parent;
			}
		}

		private bool HasExposeToEditorChildren(Transform parentTransform)
		{
			int childCount = parentTransform.childCount;
			if (childCount == 0)
			{
				return false;
			}
			for (int i = 0; i < childCount; i++)
			{
				Transform child = parentTransform.GetChild(i);
				ExposeToEditor component = child.GetComponent<ExposeToEditor>();
				if (component != null)
				{
					return true;
				}
				HierarchyItem component2 = child.GetComponent<HierarchyItem>();
				if (component2 != null && HasExposeToEditorChildren(child))
				{
					return true;
				}
			}
			return false;
		}

		private void UpdateChildren(Transform parentTransform, ExposeToEditor parentExp)
		{
			int childCount = parentTransform.childCount;
			if (childCount == 0)
			{
				return;
			}
			for (int i = 0; i < childCount; i++)
			{
				Transform child = parentTransform.GetChild(i);
				ExposeToEditor component = child.GetComponent<ExposeToEditor>();
				HierarchyItem component2 = child.GetComponent<HierarchyItem>();
				if (component != null)
				{
					component.Parent = parentExp;
					component2.m_parentExp = parentExp;
				}
				else if (component2 != null)
				{
					UpdateChildren(child, parentExp);
				}
			}
		}

		private void OnTransformParentChanged()
		{
			if (!m_isAwaked || !(base.transform.parent != m_parentTransform))
			{
				return;
			}
			if (m_parentTransform != null && m_parentExp != null)
			{
				TryDestroyChainToParent(m_parentTransform, m_parentExp);
			}
			ExposeToEditor exposeToEditor = CreateChainToParent(base.transform.parent);
			if (exposeToEditor != m_parentExp)
			{
				if (m_exposeToEditor == null)
				{
					UpdateChildren(base.transform, exposeToEditor);
				}
				else
				{
					m_exposeToEditor.Parent = exposeToEditor;
				}
				m_parentExp = exposeToEditor;
			}
			m_parentTransform = base.transform.parent;
		}
	}
}
