using System.Collections.Generic;
using System.Linq;
using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class RuntimeSelectionComponent : RuntimeEditorWindow
	{
		[SerializeField]
		private PositionHandleModel m_positonHandleModel;

		[SerializeField]
		private RotationHandleModel m_rotationHandleModel;

		[SerializeField]
		private ScaleHandleModel m_scaleHandleModel;

		public KeyCode RuntimeModifierKey = KeyCode.LeftControl;

		public KeyCode EditorModifierKey = KeyCode.LeftShift;

		public KeyCode SelectAllKey = KeyCode.A;

		public KeyCode MultiselectKey = KeyCode.LeftControl;

		public KeyCode MultiselectKey2 = KeyCode.RightControl;

		public KeyCode RangeSelectKey = KeyCode.LeftShift;

		public Camera SceneCamera;

		private PositionHandle m_positionHandle;

		private RotationHandle m_rotationHandle;

		private ScaleHandle m_scaleHandle;

		public Transform HandlesParent;

		public KeyCode ModifierKey => RuntimeModifierKey;

		protected virtual LayerMask LayerMask => -1;

		protected virtual bool IPointerOverEditorArea => !RuntimeTools.IsPointerOverGameObject();

		protected PositionHandle PositionHandle => m_positionHandle;

		protected RotationHandle RotationHandle => m_rotationHandle;

		protected ScaleHandle ScaleHandle => m_scaleHandle;

		private void Start()
		{
			if (BoxSelection.Current == null)
			{
				GameObject gameObject = new GameObject();
				gameObject.name = "BoxSelection";
				gameObject.transform.SetParent(base.transform, false);
				gameObject.AddComponent<BoxSelection>();
			}
			StartOverride();
		}

		private void OnEnable()
		{
			OnEnableOverride();
		}

		private void OnDisable()
		{
			OnDisableOverride();
		}

		private void LateUpdate()
		{
			if (Input.GetMouseButtonDown(0))
			{
				if ((RuntimeTools.ActiveTool != null && RuntimeTools.ActiveTool != BoxSelection.Current) || !IPointerOverEditorArea || RuntimeTools.IsViewing)
				{
					return;
				}
				bool key = InputController.GetKey(RangeSelectKey);
				bool flag = InputController.GetKey(MultiselectKey) || InputController.GetKey(MultiselectKey2) || key;
				Ray ray = SceneCamera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, float.MaxValue))
				{
					GameObject gameObject = hitInfo.collider.gameObject;
					if (CanSelect(gameObject))
					{
						if (flag)
						{
							List<Object> list = ((RuntimeSelection.objects == null) ? new List<Object>() : RuntimeSelection.objects.ToList());
							if (list.Contains(gameObject))
							{
								list.Remove(gameObject);
								if (key)
								{
									list.Insert(0, gameObject);
								}
							}
							else
							{
								list.Insert(0, gameObject);
							}
							RuntimeSelection.Select(gameObject, list.ToArray());
						}
						else
						{
							RuntimeSelection.activeObject = gameObject;
						}
					}
					else if (!flag)
					{
						RuntimeSelection.activeObject = null;
					}
				}
				else if (!flag)
				{
					RuntimeSelection.activeObject = null;
				}
			}
			if (RuntimeEditorApplication.IsActiveWindow(this) && InputController.GetKeyDown(SelectAllKey) && InputController.GetKey(ModifierKey))
			{
				IEnumerable<GameObject> source = ((!RuntimeEditorApplication.IsPlaying) ? ExposeToEditor.FindAll(ExposeToEditorObjectType.EditorMode) : ExposeToEditor.FindAll(ExposeToEditorObjectType.PlayMode));
				RuntimeSelection.objects = source.ToArray();
			}
		}

		private void OnApplicationQuit()
		{
			BoxSelection.Filtering -= OnBoxSelectionFiltering;
			OnApplicationQuitOverride();
		}

		protected override void AwakeOverride()
		{
			base.AwakeOverride();
			if (SceneCamera == null)
			{
				SceneCamera = Camera.main;
			}
			if (HandlesParent == null)
			{
				HandlesParent = base.transform;
			}
			GameObject gameObject = new GameObject();
			gameObject.name = "PositionHandle";
			m_positionHandle = gameObject.AddComponent<PositionHandle>();
			m_positionHandle.Model = m_positonHandleModel;
			m_positionHandle.SceneCamera = SceneCamera;
			gameObject.SetActive(false);
			gameObject.transform.SetParent(HandlesParent);
			GameObject gameObject2 = new GameObject();
			gameObject2.name = "RotationHandle";
			m_rotationHandle = gameObject2.AddComponent<RotationHandle>();
			m_rotationHandle.Model = m_rotationHandleModel;
			m_rotationHandle.SceneCamera = SceneCamera;
			gameObject2.SetActive(false);
			gameObject2.transform.SetParent(HandlesParent);
			GameObject gameObject3 = new GameObject();
			gameObject3.name = "ScaleHandle";
			m_scaleHandle = gameObject3.AddComponent<ScaleHandle>();
			m_scaleHandle.Model = m_scaleHandleModel;
			m_scaleHandle.SceneCamera = SceneCamera;
			gameObject3.SetActive(false);
			gameObject3.transform.SetParent(HandlesParent);
			BoxSelection.Filtering += OnBoxSelectionFiltering;
			RuntimeSelection.SelectionChanged += OnRuntimeSelectionChanged;
			RuntimeTools.ToolChanged += OnRuntimeToolChanged;
			if (InputController.Instance == null)
			{
				base.gameObject.AddComponent<InputController>();
			}
		}

		protected virtual void StartOverride()
		{
		}

		protected virtual void OnEnableOverride()
		{
		}

		protected virtual void OnDisableOverride()
		{
		}

		private void OnApplicationQuitOverride()
		{
		}

		protected override void OnDestroyOverride()
		{
			base.OnDestroyOverride();
			BoxSelection.Filtering -= OnBoxSelectionFiltering;
			RuntimeTools.Current = RuntimeTool.None;
			RuntimeSelection.SelectionChanged -= OnRuntimeSelectionChanged;
			RuntimeTools.ToolChanged -= OnRuntimeToolChanged;
		}

		private void OnRuntimeToolChanged()
		{
			SetCursor();
			if (RuntimeSelection.activeTransform == null)
			{
				return;
			}
			if (m_positionHandle != null)
			{
				m_positionHandle.gameObject.SetActive(false);
				if (RuntimeTools.Current == RuntimeTool.Move)
				{
					m_positionHandle.transform.position = RuntimeSelection.activeTransform.position;
					m_positionHandle.Targets = GetTargets();
					m_positionHandle.gameObject.SetActive(m_positionHandle.Targets.Length > 0);
				}
			}
			if (m_rotationHandle != null)
			{
				m_rotationHandle.gameObject.SetActive(false);
				if (RuntimeTools.Current == RuntimeTool.Rotate)
				{
					m_rotationHandle.transform.position = RuntimeSelection.activeTransform.position;
					m_rotationHandle.Targets = GetTargets();
					m_rotationHandle.gameObject.SetActive(m_rotationHandle.Targets.Length > 0);
				}
			}
			if (m_scaleHandle != null)
			{
				m_scaleHandle.gameObject.SetActive(false);
				if (RuntimeTools.Current == RuntimeTool.Scale)
				{
					m_scaleHandle.transform.position = RuntimeSelection.activeTransform.position;
					m_scaleHandle.Targets = GetTargets();
					m_scaleHandle.gameObject.SetActive(m_scaleHandle.Targets.Length > 0);
				}
			}
		}

		private void OnBoxSelectionFiltering(object sender, FilteringArgs e)
		{
			if (e.Object == null)
			{
				e.Cancel = true;
			}
			ExposeToEditor component = e.Object.GetComponent<ExposeToEditor>();
			if (!component || !component.CanSelect)
			{
				e.Cancel = true;
			}
		}

		private void OnRuntimeSelectionChanged(Object[] unselected)
		{
			if (unselected != null)
			{
				for (int i = 0; i < unselected.Length; i++)
				{
					GameObject gameObject = unselected[i] as GameObject;
					if (gameObject != null)
					{
						SelectionGizmo component = gameObject.GetComponent<SelectionGizmo>();
						if (component != null)
						{
							Object.DestroyImmediate(component);
						}
						ExposeToEditor component2 = gameObject.GetComponent<ExposeToEditor>();
						if ((bool)component2 && component2.Unselected != null)
						{
							component2.Unselected.Invoke(component2);
						}
					}
				}
			}
			GameObject[] gameObjects = RuntimeSelection.gameObjects;
			if (gameObjects != null)
			{
				foreach (GameObject gameObject2 in gameObjects)
				{
					ExposeToEditor component3 = gameObject2.GetComponent<ExposeToEditor>();
					if ((bool)component3 && component3.CanSelect && !gameObject2.IsPrefab() && !gameObject2.isStatic)
					{
						SelectionGizmo selectionGizmo = gameObject2.GetComponent<SelectionGizmo>();
						if (selectionGizmo == null)
						{
							selectionGizmo = gameObject2.AddComponent<SelectionGizmo>();
						}
						selectionGizmo.SceneCamera = SceneCamera;
						if (component3.Selected != null)
						{
							component3.Selected.Invoke(component3);
						}
					}
				}
			}
			if (RuntimeSelection.activeGameObject == null || RuntimeSelection.activeGameObject.IsPrefab())
			{
				if (m_positionHandle != null)
				{
					m_positionHandle.gameObject.SetActive(false);
				}
				if (m_rotationHandle != null)
				{
					m_rotationHandle.gameObject.SetActive(false);
				}
				if (m_scaleHandle != null)
				{
					m_scaleHandle.gameObject.SetActive(false);
				}
			}
			else
			{
				OnRuntimeToolChanged();
			}
		}

		protected virtual void SetCursor()
		{
		}

		protected virtual bool CanSelect(GameObject go)
		{
			return go.GetComponent<ExposeToEditor>();
		}

		protected virtual Transform[] GetTargets()
		{
			return (from g in RuntimeSelection.gameObjects
				select g.transform into g
				orderby RuntimeSelection.activeTransform == g descending
				select g).ToArray();
		}

		public virtual void SetSceneCamera(Camera camera)
		{
			SceneCamera = camera;
			if (m_positionHandle != null)
			{
				m_positionHandle.SceneCamera = camera;
			}
			if (m_rotationHandle != null)
			{
				m_rotationHandle.SceneCamera = camera;
			}
			if (m_scaleHandle != null)
			{
				m_scaleHandle.SceneCamera = camera;
			}
			GameObject[] gameObjects = RuntimeSelection.gameObjects;
			if (gameObjects == null)
			{
				return;
			}
			foreach (GameObject gameObject in gameObjects)
			{
				ExposeToEditor component = gameObject.GetComponent<ExposeToEditor>();
				if ((bool)component && component.CanSelect && !gameObject.IsPrefab() && !gameObject.isStatic)
				{
					SelectionGizmo selectionGizmo = gameObject.GetComponent<SelectionGizmo>();
					if (selectionGizmo != null)
					{
						Object.Destroy(selectionGizmo);
						selectionGizmo = gameObject.AddComponent<SelectionGizmo>();
					}
					if (selectionGizmo != null)
					{
						selectionGizmo.SceneCamera = SceneCamera;
					}
				}
			}
		}
	}
}
