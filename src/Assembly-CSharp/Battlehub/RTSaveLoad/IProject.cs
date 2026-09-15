namespace Battlehub.RTSaveLoad
{
	public interface IProject
	{
		void LoadProject(string name, ProjectEventHandler<ProjectRoot> callback, bool metaOnly = false, params int[] exceptTypes);

		void SaveProjectMeta(string name, ProjectMeta meta, ProjectEventHandler callback);

		void Save(ProjectItem item, bool metaOnly, ProjectEventHandler callback);

		void Save(ProjectItem[] items, bool metaOnly, ProjectEventHandler callback);

		void Load(string[] path, ProjectEventHandler<ProjectItem[]> callback, params int[] exceptTypes);

		void LoadData(ProjectItem[] items, ProjectEventHandler<ProjectItem[]> callback, params int[] exceptTypes);

		void UnloadData(ProjectItem item);

		void Delete(ProjectItem item, ProjectEventHandler callback);

		void Delete(ProjectItem[] items, ProjectEventHandler callback);

		void Move(ProjectItem item, ProjectItem parent, ProjectEventHandler callback);

		void Move(ProjectItem[] items, ProjectItem parent, ProjectEventHandler callback);

		void Rename(ProjectItem item, string name, ProjectEventHandler callback);

		void Exists(ProjectItem item, ProjectEventHandler<bool> callback);
	}
}
