using System;
using System.Linq;
using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public class RuntimeSceneManager : MonoBehaviour, ISceneManager
	{
		protected IProject m_project;

		protected ISerializer m_serializer;

		[NonSerialized]
		private ProjectItem m_activeScene;

		public ProjectItem ActiveScene => m_activeScene;

		public event EventHandler<ProjectManagerEventArgs> SceneCreated;

		public event EventHandler<ProjectManagerEventArgs> SceneSaving;

		public event EventHandler<ProjectManagerEventArgs> SceneSaved;

		public event EventHandler<ProjectManagerEventArgs> SceneLoading;

		public event EventHandler<ProjectManagerEventArgs> SceneLoaded;

		private void Awake()
		{
			Dependencies.Serializer.DeepClone(1);
			m_project = Dependencies.Project;
			m_serializer = Dependencies.Serializer;
			AwakeOverride();
		}

		private void Start()
		{
			StartOverride();
		}

		private void OnDestroy()
		{
			OnDestroyOverride();
			IdentifiersMap.Instance = null;
		}

		protected virtual void AwakeOverride()
		{
			m_activeScene = ProjectItem.CreateScene("New Scene");
		}

		protected virtual void StartOverride()
		{
		}

		protected virtual void OnDestroyOverride()
		{
		}

		public void Exists(ProjectItem item, ProjectManagerCallback<bool> callback)
		{
			m_project.Exists(item, delegate(ProjectPayload<bool> result)
			{
				if (callback != null)
				{
					callback(result.Data);
				}
			});
		}

		public virtual void SaveScene(ProjectItem scene, ProjectManagerCallback callback)
		{
			if (this.SceneSaving != null)
			{
				this.SceneSaving(this, new ProjectManagerEventArgs(scene));
			}
			GameObject gameObject = new GameObject();
			ExtraSceneData extraSceneData = gameObject.AddComponent<ExtraSceneData>();
			extraSceneData.Selection = RuntimeSelection.objects;
			PersistentScene data = PersistentScene.CreatePersistentScene();
			if (scene.Internal_Data == null)
			{
				scene.Internal_Data = new ProjectItemData();
			}
			scene.Internal_Data.RawData = m_serializer.Serialize(data);
			UnityEngine.Object.Destroy(gameObject);
			m_project.Save(scene, false, delegate
			{
				m_project.UnloadData(scene);
				m_activeScene = scene;
				if (callback != null)
				{
					callback();
				}
				if (this.SceneSaved != null)
				{
					this.SceneSaved(this, new ProjectManagerEventArgs(scene));
				}
			});
		}

		public virtual void LoadScene(ProjectItem scene, ProjectManagerCallback callback)
		{
			RaiseSceneLoading(scene);
			bool isEnabled = RuntimeUndo.Enabled;
			RuntimeUndo.Enabled = false;
			RuntimeSelection.objects = null;
			RuntimeUndo.Enabled = isEnabled;
			m_project.LoadData(new ProjectItem[1] { scene }, delegate(ProjectPayload<ProjectItem[]> loadDataCompleted)
			{
				scene = loadDataCompleted.Data[0];
				PersistentScene persistentScene = m_serializer.Deserialize<PersistentScene>(scene.Internal_Data.RawData);
				CompleteSceneLoading(scene, callback, isEnabled, persistentScene);
			});
		}

		protected void RaiseSceneLoading(ProjectItem scene)
		{
			if (this.SceneLoading != null)
			{
				this.SceneLoading(this, new ProjectManagerEventArgs(scene));
			}
		}

		protected void RaiseSceneLoaded(ProjectItem scene)
		{
			if (this.SceneLoaded != null)
			{
				this.SceneLoaded(this, new ProjectManagerEventArgs(scene));
			}
		}

		protected void CompleteSceneLoading(ProjectItem scene, ProjectManagerCallback callback, bool isEnabled, PersistentScene persistentScene)
		{
			PersistentScene.InstantiateGameObjects(persistentScene);
			m_project.UnloadData(scene);
			ExtraSceneData extraSceneData = UnityEngine.Object.FindObjectOfType<ExtraSceneData>();
			RuntimeUndo.Enabled = false;
			RuntimeSelection.objects = extraSceneData.Selection;
			RuntimeUndo.Enabled = isEnabled;
			UnityEngine.Object.Destroy(extraSceneData.gameObject);
			m_activeScene = scene;
			callback?.Invoke();
			RaiseSceneLoaded(scene);
		}

		public virtual void CreateScene()
		{
			RuntimeSelection.objects = null;
			RuntimeUndo.Purge();
			ExposeToEditor[] array = (from go in ExposeToEditor.FindAll(ExposeToEditorObjectType.EditorMode, false)
				select go.GetComponent<ExposeToEditor>()).ToArray();
			foreach (ExposeToEditor exposeToEditor in array)
			{
				if (exposeToEditor != null)
				{
					UnityEngine.Object.DestroyImmediate(exposeToEditor.gameObject);
				}
			}
			GameObject gameObject = new GameObject();
			gameObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
			gameObject.transform.position = new Vector3(0f, 10f, 0f);
			Light light = gameObject.AddComponent<Light>();
			light.type = LightType.Directional;
			gameObject.name = "Directional Light";
			gameObject.AddComponent<ExposeToEditor>();
			GameObject gameObject2 = new GameObject();
			gameObject2.name = "Main Camera";
			gameObject2.transform.position = new Vector3(0f, 0f, -10f);
			gameObject2.AddComponent<Camera>();
			gameObject2.tag = "MainCamera";
			gameObject2.AddComponent<ExposeToEditor>();
			m_activeScene = ProjectItem.CreateScene("New Scene");
			if (this.SceneCreated != null)
			{
				this.SceneCreated(this, new ProjectManagerEventArgs(ActiveScene));
			}
		}
	}
}
