using System;

namespace Battlehub.RTSaveLoad
{
	public interface ISceneManager
	{
		ProjectItem ActiveScene { get; }

		event EventHandler<ProjectManagerEventArgs> SceneCreated;

		event EventHandler<ProjectManagerEventArgs> SceneSaving;

		event EventHandler<ProjectManagerEventArgs> SceneSaved;

		event EventHandler<ProjectManagerEventArgs> SceneLoading;

		event EventHandler<ProjectManagerEventArgs> SceneLoaded;

		void Exists(ProjectItem scene, ProjectManagerCallback<bool> callback);

		void SaveScene(ProjectItem scene, ProjectManagerCallback callback);

		void LoadScene(ProjectItem scene, ProjectManagerCallback callback);

		void CreateScene();
	}
}
