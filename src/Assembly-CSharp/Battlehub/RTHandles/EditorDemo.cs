using System;
using System.Linq;
using Battlehub.RTCommon;
using Battlehub.RTSaveLoad;
using Battlehub.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTHandles
{
	[DisallowMultipleComponent]
	public class EditorDemo : MonoBehaviour
	{
		[SerializeField]
		private string SaveFileName = "RTHandlesEditorDemo";

		private bool m_saveFileExists;

		public GameObject[] Prefabs;

		public GameObject PrefabsPanel;

		public GameObject PrefabPresenter;

		public GameObject GamePrefab;

		public RuntimeSelectionComponent SelectionController;

		public KeyCode EditorModifierKey = KeyCode.LeftShift;

		public KeyCode RuntimeModifierKey = KeyCode.LeftControl;

		public KeyCode SnapToGridKey = KeyCode.S;

		public KeyCode DuplicateKey = KeyCode.D;

		public KeyCode DeleteKey = KeyCode.Delete;

		public KeyCode EnterPlayModeKey = KeyCode.P;

		public KeyCode FocusKey = KeyCode.F;

		public KeyCode LeftKey = KeyCode.LeftArrow;

		public KeyCode RightKey = KeyCode.RightArrow;

		public KeyCode FwdKey = KeyCode.UpArrow;

		public KeyCode BwdKey = KeyCode.DownArrow;

		public KeyCode UpKey = KeyCode.PageUp;

		public KeyCode DownKey = KeyCode.PageDown;

		public float PanSpeed = 10f;

		public Texture2D PanTexture;

		private Vector3 m_panOffset;

		private bool m_pan;

		private Plane m_dragPlane;

		private Vector3 m_prevMouse;

		private Vector3 m_playerCameraPostion;

		private Quaternion m_playerCameraRotation;

		public Camera PlayerCamera;

		public Camera EditorCamera;

		public Grid Grid;

		public Button ProjectionButton;

		public Button PlayButton;

		public Button HintButton;

		public Button StopButton;

		public Button SaveButton;

		public Button LoadButton;

		public Button UndoButton;

		public Button RedoButton;

		public Button ResetButton;

		public GameObject UI;

		public GameObject TransformPanel;

		public GameObject BottomPanel;

		public Toggle TogAutoFocus;

		public Toggle TogUnitSnap;

		public Toggle TogBoundingBoxSnap;

		public Toggle TogEnableCharacters;

		public Toggle TogGrid;

		public Toggle TogShowGizmos;

		public Toggle TogPivotRotation;

		public Text TxtCurrentControl;

		public GameObject ConfirmationSave;

		public GameObject ConfirmationLoad;

		private GameObject m_game;

		private ISceneManager m_sceneManager;

		private Vector3 m_pivot;

		public float EditorCamDistance = 10f;

		private IAnimationInfo[] m_focusAnimations = new IAnimationInfo[3];

		private Transform m_autoFocusTranform;

		private bool m_enableCharacters;

		public KeyCode ModifierKey => RuntimeModifierKey;

		private bool IsInPlayMode => m_game != null;

		public Vector3 Pivot => m_pivot;

		public bool AutoFocus
		{
			get
			{
				return RuntimeTools.AutoFocus;
			}
			set
			{
				RuntimeTools.AutoFocus = value;
			}
		}

		public bool AutoUnitSnapping
		{
			get
			{
				return RuntimeTools.UnitSnapping;
			}
			set
			{
				RuntimeTools.UnitSnapping = value;
			}
		}

		public bool BoundingBoxSnapping
		{
			get
			{
				return RuntimeTools.IsSnapping;
			}
			set
			{
				RuntimeTools.IsSnapping = value;
			}
		}

		public bool EnableCharacters
		{
			get
			{
				return m_enableCharacters;
			}
			set
			{
				if (m_enableCharacters == value)
				{
					return;
				}
				m_enableCharacters = value;
				ForEachSelectedObject(delegate(GameObject go)
				{
					ExposeToEditor component = go.GetComponent<ExposeToEditor>();
					if ((bool)component)
					{
						component.Unselected.Invoke(component);
						component.Selected.Invoke(component);
					}
				});
			}
		}

		public bool ShowSelectionGizmos
		{
			get
			{
				return RuntimeTools.ShowSelectionGizmos;
			}
			set
			{
				RuntimeTools.ShowSelectionGizmos = value;
			}
		}

		public bool IsGlobalPivotRotation
		{
			get
			{
				return RuntimeTools.PivotRotation == RuntimePivotRotation.Global;
			}
			set
			{
				if (value)
				{
					RuntimeTools.PivotRotation = RuntimePivotRotation.Global;
				}
				else
				{
					RuntimeTools.PivotRotation = RuntimePivotRotation.Local;
				}
			}
		}

		public static EditorDemo Instance { get; private set; }

		private void OnAwaked(ExposeToEditor obj)
		{
			if (IsInPlayMode)
			{
				if (obj.ObjectType == ExposeToEditorObjectType.Undefined)
				{
					obj.ObjectType = ExposeToEditorObjectType.PlayMode;
				}
			}
			else if (obj.ObjectType == ExposeToEditorObjectType.Undefined)
			{
				obj.ObjectType = ExposeToEditorObjectType.EditorMode;
			}
		}

		private void OnDestroyed(ExposeToEditor obj)
		{
		}

		private void Awake()
		{
			Instance = this;
			ExposeToEditor[] array = (from go in ExposeToEditor.FindAll(ExposeToEditorObjectType.Undefined, false)
				select go.GetComponent<ExposeToEditor>()).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ObjectType = ExposeToEditorObjectType.EditorMode;
			}
			RuntimeTools.SnappingMode = SnappingMode.BoundingBox;
			RuntimeEditorApplication.IsOpened = !IsInPlayMode;
			RuntimeEditorApplication.SceneCameras = new Camera[1] { EditorCamera };
			RuntimeEditorApplication.PlaymodeStateChanged += OnPlaymodeStateChanged;
			RuntimeEditorApplication.IsOpenedChanged += OnIsOpenedChanged;
			RuntimeSelection.SelectionChanged += OnRuntimeSelectionChanged;
			RuntimeTools.ToolChanged += OnRuntimeToolChanged;
			RuntimeTools.PivotRotationChanged += OnPivotRotationChanged;
			RuntimeUndo.UndoCompleted += OnUndoCompleted;
			RuntimeUndo.RedoCompleted += OnRedoCompleted;
			RuntimeUndo.StateChanged += OnUndoRedoStateChanged;
			TransformPanel.SetActive(RuntimeSelection.activeTransform != null);
			if (Prefabs == null || !(PrefabsPanel != null) || !(PrefabPresenter != null))
			{
				return;
			}
			Prefabs = Prefabs.Where((GameObject p) => p != null).ToArray();
			for (int j = 0; j < Prefabs.Length; j++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(PrefabPresenter);
				gameObject.transform.SetParent(PrefabsPanel.transform);
				gameObject.transform.position = Vector3.zero;
				gameObject.transform.localScale = Vector3.one;
				InstantiatePrefab componentInChildren = gameObject.GetComponentInChildren<InstantiatePrefab>();
				if (componentInChildren != null)
				{
					componentInChildren.Prefab = Prefabs[j];
				}
				TakeSnapshot componentInChildren2 = gameObject.GetComponentInChildren<TakeSnapshot>();
				if (componentInChildren2 != null)
				{
					componentInChildren2.TargetPrefab = Prefabs[j];
				}
			}
		}

		private void Start()
		{
			Vector3 vector = new Vector3(1f, 1f, 1f);
			if (!(SelectionController is RuntimeSceneView))
			{
				EditorCamera.transform.position = m_pivot + vector * EditorCamDistance;
				EditorCamera.transform.LookAt(m_pivot);
				RuntimeTools.DrawSelectionGizmoRay = true;
			}
			UpdateUIState(IsInPlayMode);
			AutoFocus = TogAutoFocus.isOn;
			AutoUnitSnapping = TogUnitSnap.isOn;
			BoundingBoxSnapping = TogBoundingBoxSnap.isOn;
			ShowSelectionGizmos = TogShowGizmos.isOn;
			EnableCharacters = TogEnableCharacters.isOn;
			ExposeToEditor.Awaked += OnAwaked;
			ExposeToEditor.Destroyed += OnDestroyed;
			m_sceneManager = Dependencies.SceneManager;
			if (m_sceneManager != null)
			{
				m_sceneManager.ActiveScene.Name = SaveFileName;
				m_sceneManager.Exists(m_sceneManager.ActiveScene, delegate(bool exists)
				{
					m_saveFileExists = exists;
					LoadButton.interactable = exists;
				});
			}
		}

		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
				RuntimeEditorApplication.Reset();
			}
			RuntimeEditorApplication.PlaymodeStateChanged -= OnPlaymodeStateChanged;
			RuntimeEditorApplication.IsOpenedChanged -= OnIsOpenedChanged;
			RuntimeSelection.SelectionChanged -= OnRuntimeSelectionChanged;
			RuntimeTools.ToolChanged -= OnRuntimeToolChanged;
			RuntimeTools.PivotRotationChanged -= OnPivotRotationChanged;
			RuntimeUndo.RedoCompleted -= OnUndoCompleted;
			RuntimeUndo.RedoCompleted -= OnRedoCompleted;
			RuntimeUndo.StateChanged -= OnUndoRedoStateChanged;
			ExposeToEditor.Awaked -= OnAwaked;
			ExposeToEditor.Destroyed -= OnDestroyed;
		}

		private void Update()
		{
			if (InputController.GetKeyDown(EnterPlayModeKey) && InputController.GetKey(ModifierKey))
			{
				TogglePlayMode();
			}
			if (IsInPlayMode)
			{
				return;
			}
			if (BoundingBoxSnapping != TogBoundingBoxSnap.isOn)
			{
				TogBoundingBoxSnap.isOn = BoundingBoxSnapping;
			}
			if (InputController.GetKeyDown(DuplicateKey))
			{
				if (InputController.GetKey(ModifierKey))
				{
					Duplicate();
				}
			}
			else if (InputController.GetKeyDown(SnapToGridKey))
			{
				if (InputController.GetKey(ModifierKey))
				{
					SnapToGrid();
				}
			}
			else if (InputController.GetKeyDown(DeleteKey))
			{
				Delete();
			}
			if (!(SelectionController is RuntimeSceneView))
			{
				float axis = Input.GetAxis("Mouse ScrollWheel");
				if (axis != 0f)
				{
					EditorCamera.orthographicSize -= axis * EditorCamera.orthographicSize;
					if (EditorCamera.orthographicSize < 0.2f)
					{
						EditorCamera.orthographicSize = 0.2f;
					}
					else if (EditorCamera.orthographicSize > 15f)
					{
						EditorCamera.orthographicSize = 15f;
					}
				}
				if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
				{
					m_dragPlane = new Plane(Vector3.up, m_pivot);
					m_pan = GetPointOnDragPlane(Input.mousePosition, out m_prevMouse);
					m_prevMouse = Input.mousePosition;
					CursorHelper.SetCursor(this, PanTexture, Vector2.zero, CursorMode.Auto);
				}
				else if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
				{
					Vector3 point;
					Vector3 point2;
					if (m_pan && GetPointOnDragPlane(Input.mousePosition, out point) && GetPointOnDragPlane(m_prevMouse, out point2))
					{
						Vector3 vector = point - point2;
						m_prevMouse = Input.mousePosition;
						m_panOffset -= vector;
						EditorCamera.transform.position -= vector;
					}
				}
				else if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
				{
					m_pan = false;
					CursorHelper.ResetCursor(this);
				}
				if (InputController.GetKey(UpKey))
				{
					Vector3 position = EditorCamera.transform.position;
					position.y += PanSpeed * Time.deltaTime;
					m_panOffset.y += PanSpeed * Time.deltaTime;
					EditorCamera.transform.position = position;
				}
				if (InputController.GetKey(DownKey))
				{
					Vector3 position2 = EditorCamera.transform.position;
					position2.y -= PanSpeed * Time.deltaTime;
					m_panOffset.y -= PanSpeed * Time.deltaTime;
					EditorCamera.transform.position = position2;
				}
				if (InputController.GetKey(LeftKey))
				{
					MoveMinZ();
					MovePlusX();
				}
				if (InputController.GetKey(RightKey))
				{
					MovePlusZ();
					MoveMinX();
				}
				if (InputController.GetKey(FwdKey))
				{
					MoveMinX();
					MoveMinZ();
				}
				if (InputController.GetKey(BwdKey))
				{
					MovePlusX();
					MovePlusZ();
				}
				if (InputController.GetKeyDown(FocusKey))
				{
					Focus();
				}
				else if (AutoFocus && !(RuntimeTools.ActiveTool != null) && !(m_autoFocusTranform == null) && !(m_autoFocusTranform.position == m_pivot) && m_focusAnimations[0] != null && !m_focusAnimations[0].InProgress)
				{
					Vector3 vector2 = m_autoFocusTranform.position - m_pivot - m_panOffset;
					EditorCamera.transform.position += vector2;
					m_pivot += vector2;
					m_panOffset = Vector3.zero;
				}
			}
			if (RuntimeSelection.activeTransform != null)
			{
				Vector3 gridOffset = Grid.GridOffset;
				gridOffset.y = RuntimeSelection.activeTransform.position.y;
				Grid.GridOffset = gridOffset;
			}
		}

		private bool GetPointOnDragPlane(Vector3 mouse, out Vector3 point)
		{
			Ray ray = EditorCamera.ScreenPointToRay(mouse);
			float enter;
			if (m_dragPlane.Raycast(ray, out enter))
			{
				point = ray.GetPoint(enter);
				return true;
			}
			point = Vector3.zero;
			return false;
		}

		public void MovePlusX()
		{
			Vector3 position = EditorCamera.transform.position;
			position.x += PanSpeed * Time.deltaTime;
			m_panOffset.x += PanSpeed * Time.deltaTime;
			EditorCamera.transform.position = position;
		}

		public void MoveMinX()
		{
			Vector3 position = EditorCamera.transform.position;
			position.x -= PanSpeed * Time.deltaTime;
			m_panOffset.x -= PanSpeed * Time.deltaTime;
			EditorCamera.transform.position = position;
		}

		public void MovePlusZ()
		{
			Vector3 position = EditorCamera.transform.position;
			position.z += PanSpeed * Time.deltaTime;
			m_panOffset.z += PanSpeed * Time.deltaTime;
			EditorCamera.transform.position = position;
		}

		public void MoveMinZ()
		{
			Vector3 position = EditorCamera.transform.position;
			position.z -= PanSpeed * Time.deltaTime;
			m_panOffset.z -= PanSpeed * Time.deltaTime;
			EditorCamera.transform.position = position;
		}

		public void Duplicate()
		{
			GameObject[] gameObjects = RuntimeSelection.gameObjects;
			if (gameObjects == null)
			{
				return;
			}
			RuntimeUndo.BeginRecord();
			for (int i = 0; i < gameObjects.Length; i++)
			{
				GameObject gameObject = gameObjects[i];
				if (gameObject != null)
				{
					Transform parent = gameObject.transform.parent;
					GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation);
					gameObject2.transform.SetParent(parent, true);
					gameObjects[i] = gameObject2;
					RuntimeUndo.BeginRegisterCreateObject(gameObject2);
				}
			}
			RuntimeUndo.RecordSelection();
			RuntimeUndo.EndRecord();
			bool flag = RuntimeUndo.Enabled;
			RuntimeUndo.Enabled = false;
			RuntimeSelection.objects = gameObjects;
			RuntimeUndo.Enabled = flag;
			RuntimeUndo.BeginRecord();
			foreach (GameObject gameObject3 in gameObjects)
			{
				if (gameObject3 != null)
				{
					RuntimeUndo.RegisterCreatedObject(gameObject3);
				}
			}
			RuntimeUndo.RecordSelection();
			RuntimeUndo.EndRecord();
		}

		public void Delete()
		{
			GameObject[] gameObjects = RuntimeSelection.gameObjects;
			if (gameObjects == null)
			{
				return;
			}
			RuntimeUndo.BeginRecord();
			foreach (GameObject gameObject in gameObjects)
			{
				if (gameObject != null)
				{
					RuntimeUndo.BeginDestroyObject(gameObject);
				}
			}
			RuntimeUndo.RecordSelection();
			RuntimeUndo.EndRecord();
			bool flag = RuntimeUndo.Enabled;
			RuntimeUndo.Enabled = false;
			RuntimeSelection.objects = null;
			RuntimeUndo.Enabled = flag;
			RuntimeUndo.BeginRecord();
			foreach (GameObject gameObject2 in gameObjects)
			{
				if (gameObject2 != null)
				{
					RuntimeUndo.DestroyObject(gameObject2);
				}
			}
			RuntimeUndo.RecordSelection();
			RuntimeUndo.EndRecord();
		}

		public void TogglePlayMode()
		{
			RuntimeEditorApplication.IsPlaying = !RuntimeEditorApplication.IsPlaying;
		}

		private void OnIsOpenedChanged()
		{
			RuntimeEditorApplication.IsPlaying = !RuntimeEditorApplication.IsOpened;
		}

		private void OnPlaymodeStateChanged()
		{
			UpdateUIState(m_game == null);
			RuntimeEditorApplication.IsOpened = !RuntimeEditorApplication.IsPlaying;
			if (m_game == null)
			{
				m_game = UnityEngine.Object.Instantiate(GamePrefab);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(m_game);
				m_game = null;
			}
			RuntimeEditorApplication.IsOpened = !IsInPlayMode;
			if (BoxSelection.Current != null)
			{
				BoxSelection.Current.gameObject.SetActive(!IsInPlayMode);
			}
			if (IsInPlayMode)
			{
				RuntimeSelection.objects = null;
				RuntimeUndo.Purge();
				m_playerCameraPostion = PlayerCamera.transform.position;
				m_playerCameraRotation = PlayerCamera.transform.rotation;
			}
			else
			{
				PlayerCamera.transform.position = m_playerCameraPostion;
				PlayerCamera.transform.rotation = m_playerCameraRotation;
			}
			SaveButton.interactable = false;
		}

		public void Focus()
		{
			RuntimeSceneView runtimeSceneView = SelectionController as RuntimeSceneView;
			if (runtimeSceneView != null)
			{
				runtimeSceneView.Focus();
			}
			else if (!(RuntimeSelection.activeTransform == null))
			{
				m_autoFocusTranform = RuntimeSelection.activeTransform;
				Vector3 vector = RuntimeSelection.activeTransform.position - m_pivot - m_panOffset;
				Run.Instance.Remove(m_focusAnimations[0]);
				Run.Instance.Remove(m_focusAnimations[1]);
				Run.Instance.Remove(m_focusAnimations[2]);
				m_focusAnimations[0] = new Vector3AnimationInfo(EditorCamera.transform.position, EditorCamera.transform.position + vector, 0.1f, AnimationInfo<object, Vector3>.EaseOutCubic, delegate(object target, Vector3 value, float t, bool completed)
				{
					EditorCamera.transform.position = value;
				});
				m_focusAnimations[1] = new Vector3AnimationInfo(m_pivot, RuntimeSelection.activeTransform.position, 0.1f, AnimationInfo<object, Vector3>.EaseOutCubic, delegate(object target, Vector3 value, float t, bool completed)
				{
					m_pivot = value;
				});
				m_focusAnimations[2] = new Vector3AnimationInfo(m_panOffset, Vector3.zero, 0.1f, AnimationInfo<object, Vector3>.EaseOutCubic, delegate(object target, Vector3 value, float t, bool completed)
				{
					m_panOffset = value;
				});
				Run.Instance.Animation(m_focusAnimations[0]);
				Run.Instance.Animation(m_focusAnimations[1]);
				Run.Instance.Animation(m_focusAnimations[2]);
			}
		}

		private void OnRuntimeSelectionChanged(UnityEngine.Object[] unselectedObjects)
		{
			TransformPanel.SetActive(RuntimeSelection.activeTransform != null);
			TogPivotRotation.isOn = IsGlobalPivotRotation;
		}

		private void OnPivotRotationChanged()
		{
			TogPivotRotation.isOn = IsGlobalPivotRotation;
		}

		private void OnRuntimeToolChanged()
		{
			if (RuntimeTools.Current == RuntimeTool.None || RuntimeTools.Current == RuntimeTool.View)
			{
				TxtCurrentControl.text = "none";
				ResetButton.gameObject.SetActive(false);
			}
			else if (RuntimeTools.Current == RuntimeTool.Move)
			{
				TxtCurrentControl.text = "move";
				ResetButton.gameObject.SetActive(true);
			}
			else if (RuntimeTools.Current == RuntimeTool.Rotate)
			{
				TxtCurrentControl.text = "rotate";
				ResetButton.gameObject.SetActive(true);
			}
			else if (RuntimeTools.Current == RuntimeTool.Scale)
			{
				TxtCurrentControl.text = "scale";
				ResetButton.gameObject.SetActive(true);
			}
		}

		public void SwitchControl()
		{
			if (RuntimeTools.Current == RuntimeTool.None || RuntimeTools.Current == RuntimeTool.View)
			{
				RuntimeTools.Current = RuntimeTool.Move;
				TxtCurrentControl.text = "move";
			}
			else if (RuntimeTools.Current == RuntimeTool.Move)
			{
				RuntimeTools.Current = RuntimeTool.Rotate;
				TxtCurrentControl.text = "rotate";
			}
			else if (RuntimeTools.Current == RuntimeTool.Rotate)
			{
				RuntimeTools.Current = RuntimeTool.Scale;
				TxtCurrentControl.text = "scale";
			}
			else if (RuntimeTools.Current == RuntimeTool.Scale)
			{
				RuntimeTools.Current = RuntimeTool.View;
				TxtCurrentControl.text = "none";
			}
		}

		public void ResetPosition()
		{
			if (RuntimeTools.Current == RuntimeTool.Move)
			{
				ForEachSelectedObject(delegate(GameObject go)
				{
					go.transform.position = Vector3.zero;
				});
			}
			else if (RuntimeTools.Current == RuntimeTool.Rotate)
			{
				ForEachSelectedObject(delegate(GameObject go)
				{
					go.transform.rotation = Quaternion.identity;
				});
			}
			else if (RuntimeTools.Current == RuntimeTool.Scale)
			{
				ForEachSelectedObject(delegate(GameObject go)
				{
					go.transform.localScale = Vector3.one;
				});
			}
		}

		public void SnapToGrid()
		{
			GameObject[] gameObjects = RuntimeSelection.gameObjects;
			if (gameObjects != null && gameObjects.Length != 0)
			{
				Transform transform = gameObjects[0].transform;
				Vector3 position = transform.position;
				position.x = Mathf.Round(position.x);
				position.y = Mathf.Round(position.y);
				position.z = Mathf.Round(position.z);
				Vector3 vector = position - transform.position;
				for (int i = 0; i < gameObjects.Length; i++)
				{
					gameObjects[i].transform.position += vector;
				}
			}
		}

		private static void ForEachSelectedObject(Action<GameObject> execute)
		{
			GameObject[] gameObjects = RuntimeSelection.gameObjects;
			if (gameObjects == null)
			{
				return;
			}
			foreach (GameObject gameObject in gameObjects)
			{
				if (gameObject != null)
				{
					execute(gameObject);
				}
			}
		}

		public void Save()
		{
			if (!ConfirmationSave.activeSelf)
			{
				ConfirmationSave.SetActive(true);
				return;
			}
			RuntimeUndo.Purge();
			ConfirmationSave.SetActive(false);
			if (m_sceneManager != null)
			{
				m_sceneManager.ActiveScene.Name = SaveFileName;
				m_sceneManager.SaveScene(m_sceneManager.ActiveScene, delegate
				{
					SaveButton.interactable = false;
					m_saveFileExists = true;
					LoadButton.interactable = true;
				});
			}
		}

		public void Load()
		{
			if (!ConfirmationLoad.activeSelf)
			{
				ConfirmationLoad.SetActive(true);
				return;
			}
			RuntimeUndo.Purge();
			ExposeToEditor[] array = (from go in ExposeToEditor.FindAll(ExposeToEditorObjectType.EditorMode)
				select go.GetComponent<ExposeToEditor>()).ToArray();
			foreach (ExposeToEditor exposeToEditor in array)
			{
				if (exposeToEditor != null)
				{
					UnityEngine.Object.DestroyImmediate(exposeToEditor.gameObject);
				}
			}
			ConfirmationLoad.SetActive(false);
			if (m_sceneManager != null)
			{
				m_sceneManager.ActiveScene.Name = SaveFileName;
				m_sceneManager.LoadScene(m_sceneManager.ActiveScene, delegate
				{
					SaveButton.interactable = false;
				});
			}
		}

		public void Undo()
		{
			RuntimeUndo.Undo();
		}

		public void Redo()
		{
			RuntimeUndo.Redo();
		}

		private void OnUndoCompleted()
		{
			UndoButton.interactable = RuntimeUndo.CanUndo;
			RedoButton.interactable = RuntimeUndo.CanRedo;
			SaveButton.interactable = m_sceneManager != null;
			LoadButton.interactable = m_sceneManager != null && m_saveFileExists;
		}

		private void OnRedoCompleted()
		{
			UndoButton.interactable = RuntimeUndo.CanUndo;
			RedoButton.interactable = RuntimeUndo.CanRedo;
			SaveButton.interactable = m_sceneManager != null;
			LoadButton.interactable = m_sceneManager != null && m_saveFileExists;
		}

		private void OnUndoRedoStateChanged()
		{
			UndoButton.interactable = RuntimeUndo.CanUndo;
			RedoButton.interactable = RuntimeUndo.CanRedo;
			SaveButton.interactable = m_sceneManager != null;
			LoadButton.interactable = m_sceneManager != null && m_saveFileExists;
		}

		private void UpdateUIState(bool isInPlayMode)
		{
			if (ProjectionButton != null)
			{
				ProjectionButton.gameObject.SetActive(!isInPlayMode);
			}
			EditorCamera.gameObject.SetActive(!isInPlayMode);
			PlayerCamera.gameObject.SetActive(isInPlayMode);
			SelectionController.gameObject.SetActive(!isInPlayMode);
			PlayButton.gameObject.SetActive(!isInPlayMode);
			HintButton.gameObject.SetActive(!isInPlayMode);
			SaveButton.gameObject.SetActive(!isInPlayMode);
			LoadButton.gameObject.SetActive(!isInPlayMode);
			StopButton.gameObject.SetActive(isInPlayMode);
			UndoButton.gameObject.SetActive(!isInPlayMode);
			RedoButton.gameObject.SetActive(!isInPlayMode);
			UI.gameObject.SetActive(!isInPlayMode);
			Grid.gameObject.SetActive(TogGrid.isOn && !isInPlayMode);
			LoadButton.interactable = m_sceneManager != null && m_saveFileExists;
			if (isInPlayMode)
			{
				RuntimeEditorApplication.ActivateWindow(RuntimeWindowType.GameView);
			}
			else
			{
				RuntimeEditorApplication.ActivateWindow(RuntimeWindowType.SceneView);
			}
		}
	}
}
