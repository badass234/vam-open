using System;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public interface IProjectManager : ISceneManager
	{
		ProjectItem Project { get; }

		event EventHandler ProjectLoading;

		event EventHandler<ProjectManagerEventArgs> ProjectLoaded;

		event EventHandler<ProjectManagerEventArgs> BundledResourcesAdded;

		event EventHandler<ProjectManagerEventArgs> DynamicResourcesAdded;

		bool IsResource(UnityEngine.Object obj);

		ID GetID(UnityEngine.Object obj);

		void LoadProject(string name, ProjectManagerCallback<ProjectItem> callback);

		void AddBundledResources(ProjectItem folder, string bundleName, Func<UnityEngine.Object, string, bool> filter, ProjectManagerCallback<ProjectItem[]> callback);

		void AddBundledResource(ProjectItem folder, string bundleName, string assetName, ProjectManagerCallback<ProjectItem[]> callback);

		void AddBundledResource<T>(ProjectItem folder, string bundleName, string assetName, ProjectManagerCallback<ProjectItem[]> callback);

		void AddBundledResource(ProjectItem folder, string bundleName, string assetName, Type assetType, ProjectManagerCallback<ProjectItem[]> callback);

		void AddBundledResources(ProjectItem folder, string bundleName, string[] assetNames, ProjectManagerCallback<ProjectItem[]> callback);

		void AddBundledResources(ProjectItem folder, string bundleName, string[] assetNames, Type[] assetTypes, Func<UnityEngine.Object, string, bool> filter, ProjectManagerCallback<ProjectItem[]> callback);

		void AddDynamicResource(ProjectItem folder, UnityEngine.Object obj, ProjectManagerCallback<ProjectItem[]> callback);

		void AddDynamicResources(ProjectItem folder, UnityEngine.Object[] objects, ProjectManagerCallback<ProjectItem[]> callback);

		void AddDynamicResource(ProjectItem folder, UnityEngine.Object obj, bool includingDependencies, Func<UnityEngine.Object, bool> filter, ProjectManagerCallback<ProjectItem[]> callback);

		void AddDynamicResources(ProjectItem folder, UnityEngine.Object[] objects, bool includingDependencies, Func<UnityEngine.Object, bool> filter, ProjectManagerCallback<ProjectItem[]> callback);

		void CreateFolder(string name, ProjectItem parent, ProjectManagerCallback<ProjectItem> callback);

		void SaveObjects(ProjectItemObjectPair[] itemObjectPairs, ProjectManagerCallback callback);

		void GetOrCreateObjects(ProjectItem folder, ProjectManagerCallback<ProjectItemObjectPair[]> callback);

		void GetOrCreateObjects(ProjectItem[] projectItems, ProjectManagerCallback<ProjectItemObjectPair[]> callback);

		void Duplicate(ProjectItem[] projectItems, ProjectManagerCallback<ProjectItem[]> callback);

		void Rename(ProjectItem projectItem, string newName, ProjectManagerCallback callback);

		void Move(ProjectItem[] projectItems, ProjectItem folder, ProjectManagerCallback callback);

		void Delete(ProjectItem[] projectItems, ProjectManagerCallback callback);

		void IgnoreTypes(params Type[] type);
	}
}
