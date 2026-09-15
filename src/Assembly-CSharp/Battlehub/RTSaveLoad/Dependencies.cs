using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public static class Dependencies
	{
		public static ISerializer Serializer => new Serializer();

		public static IStorage Storage => new FileSystemStorage(Application.persistentDataPath);

		public static IProject Project => new Project();

		public static IAssetBundleLoader BundleLoader => new AssetBundleLoader();

		public static IProjectManager ProjectManager => Object.FindObjectOfType<ProjectManager>();

		public static ISceneManager SceneManager => Object.FindObjectOfType<RuntimeSceneManager>();

		public static IRuntimeShaderUtil ShaderUtil => new RuntimeShaderUtil();

		public static IJob Job
		{
			get
			{
				Job job = Object.FindObjectOfType<Job>();
				if (job == null)
				{
					GameObject gameObject = new GameObject();
					gameObject.name = "Job";
					job = gameObject.AddComponent<Job>();
					gameObject.AddComponent<PersistentIgnore>();
				}
				return job;
			}
		}
	}
}
