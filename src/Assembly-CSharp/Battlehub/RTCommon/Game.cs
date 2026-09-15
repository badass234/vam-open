using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTCommon
{
	public class Game : MonoBehaviour
	{
		public Button BtnRestart;

		private ExposeToEditor[] m_editorObjects;

		private ExposeToEditor[] m_enabledEditorObjects;

		private Object[] m_editorSelection;

		private bool m_applicationQuit;

		private void Awake()
		{
			if (BtnRestart != null)
			{
				BtnRestart.onClick.AddListener(RestartGame);
			}
			RuntimeEditorApplication.ActiveWindowChanged += OnActiveWindowChanged;
			StartGame();
			AwakeOverride();
		}

		private void Start()
		{
			StartOverride();
		}

		private void OnDestroy()
		{
			if (!m_applicationQuit)
			{
				OnDestroyOverride();
				DestroyGame();
				if (BtnRestart != null)
				{
					BtnRestart.onClick.RemoveListener(RestartGame);
				}
				RuntimeEditorApplication.ActiveWindowChanged -= OnActiveWindowChanged;
			}
		}

		private void OnApplicationQuit()
		{
			m_applicationQuit = true;
		}

		private void RestartGame()
		{
			RuntimeEditorApplication.IsPlaying = false;
			RuntimeEditorApplication.IsPlaying = true;
		}

		private void StartGame()
		{
			DestroyGame();
			m_editorObjects = (from go in ExposeToEditor.FindAll(ExposeToEditorObjectType.EditorMode)
				select go.GetComponent<ExposeToEditor>() into exp
				orderby exp.transform.GetSiblingIndex()
				select exp).ToArray();
			m_enabledEditorObjects = m_editorObjects.Where((ExposeToEditor eo) => eo.gameObject.activeSelf).ToArray();
			m_editorSelection = RuntimeSelection.objects;
			HashSet<GameObject> hashSet = new HashSet<GameObject>((RuntimeSelection.gameObjects == null) ? new GameObject[0] : RuntimeSelection.gameObjects);
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < m_editorObjects.Length; i++)
			{
				ExposeToEditor exposeToEditor = m_editorObjects[i];
				if (exposeToEditor.Parent != null)
				{
					continue;
				}
				GameObject gameObject = Object.Instantiate(exposeToEditor.gameObject, exposeToEditor.transform.position, exposeToEditor.transform.rotation);
				ExposeToEditor component = gameObject.GetComponent<ExposeToEditor>();
				component.ObjectType = ExposeToEditorObjectType.PlayMode;
				component.SetName(exposeToEditor.name);
				component.Init();
				ExposeToEditor[] componentsInChildren = exposeToEditor.GetComponentsInChildren<ExposeToEditor>(true);
				ExposeToEditor[] componentsInChildren2 = gameObject.GetComponentsInChildren<ExposeToEditor>(true);
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					if (hashSet.Contains(componentsInChildren[j].gameObject))
					{
						list.Add(componentsInChildren2[j].gameObject);
					}
				}
				exposeToEditor.gameObject.SetActive(false);
			}
			bool flag = RuntimeUndo.Enabled;
			RuntimeUndo.Enabled = false;
			RuntimeSelection.objects = list.ToArray();
			RuntimeUndo.Enabled = flag;
			RuntimeUndo.Store();
		}

		private void DestroyGame()
		{
			if (m_editorObjects == null)
			{
				return;
			}
			OnDestoryGameOverride();
			ExposeToEditor[] array = (from go in ExposeToEditor.FindAll(ExposeToEditorObjectType.PlayMode)
				select go.GetComponent<ExposeToEditor>()).ToArray();
			foreach (ExposeToEditor exposeToEditor in array)
			{
				if (exposeToEditor != null)
				{
					Object.DestroyImmediate(exposeToEditor.gameObject);
				}
			}
			for (int j = 0; j < m_enabledEditorObjects.Length; j++)
			{
				ExposeToEditor exposeToEditor2 = m_enabledEditorObjects[j];
				if (exposeToEditor2 != null)
				{
					exposeToEditor2.gameObject.SetActive(true);
				}
			}
			bool flag = RuntimeUndo.Enabled;
			RuntimeUndo.Enabled = false;
			RuntimeSelection.objects = m_editorSelection;
			RuntimeUndo.Enabled = flag;
			RuntimeUndo.Restore();
			m_editorObjects = null;
			m_enabledEditorObjects = null;
			m_editorSelection = null;
		}

		protected virtual void OnActiveWindowChanged()
		{
		}

		protected virtual void AwakeOverride()
		{
		}

		protected virtual void StartOverride()
		{
		}

		protected virtual void OnDestroyOverride()
		{
		}

		protected virtual void OnDestoryGameOverride()
		{
		}
	}
}
