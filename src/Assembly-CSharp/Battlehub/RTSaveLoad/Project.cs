using System;
using System.Collections.Generic;
using System.Linq;

namespace Battlehub.RTSaveLoad
{
	public class Project : IProject
	{
		private IStorage m_storage = Dependencies.Storage;

		private ISerializer m_serializer = Dependencies.Serializer;

		private const string FileMetaExt = "rtmeta";

		private const string ProjectMetaExt = "rtpmeta";

		private const string ProjectDataExt = "rtpdata";

		public void Parallel(Action<ProjectEventHandler>[] actions, ProjectEventHandler callback)
		{
			if (actions == null || actions.Length == 0)
			{
				callback(new ProjectPayload());
			}
			int counter = actions.Length;
			bool hasError = false;
			foreach (Action<ProjectEventHandler> action in actions)
			{
				action(delegate(ProjectPayload actionCallback)
				{
					hasError |= actionCallback.HasError;
					counter--;
					if (counter == 0)
					{
						callback(new ProjectPayload
						{
							HasError = hasError
						});
					}
				});
			}
		}

		public void LoadProject(string name, ProjectEventHandler<ProjectRoot> callback, bool metaOnly = true, params int[] exceptTypes)
		{
			HashSet<int> exceptTypesHs = null;
			if (exceptTypes != null && exceptTypes.Length > 0)
			{
				exceptTypesHs = new HashSet<int>();
				for (int i = 0; i < exceptTypes.Length; i++)
				{
					if (!exceptTypesHs.Contains(exceptTypes[i]))
					{
						exceptTypesHs.Add(exceptTypes[i]);
					}
				}
			}
			m_storage.CheckFolderExists(name, delegate(StoragePayload<string, bool> checkFolderResult)
			{
				if (checkFolderResult.Data)
				{
					ProjectRoot root = new ProjectRoot();
					m_storage.LoadFile(name + ".rtpmeta", delegate(StoragePayload<string, byte[]> loadMetaCallback)
					{
						if (loadMetaCallback.Data != null)
						{
							root.Meta = m_serializer.Deserialize<ProjectMeta>(loadMetaCallback.Data);
						}
						else
						{
							root.Meta = new ProjectMeta();
						}
						m_storage.LoadFile(name + ".rtpdata", delegate(StoragePayload<string, byte[]> loadDataCallback)
						{
							if (loadDataCallback.Data != null)
							{
								root.Data = m_serializer.Deserialize<ProjectData>(loadDataCallback.Data);
							}
							else
							{
								root.Data = new ProjectData();
							}
							root.Item = ProjectItem.CreateFolder(name);
							LoadFolders(root.Item, delegate
							{
								LoadFiles(root.Item, delegate
								{
									if (callback != null)
									{
										callback(new ProjectPayload<ProjectRoot>(root));
									}
								}, metaOnly, exceptTypesHs);
							});
						});
					});
				}
				else if (callback != null)
				{
					callback(new ProjectPayload<ProjectRoot>(null));
				}
			});
		}

		private void LoadFiles(ProjectItem item, ProjectEventHandler callback, bool metaOnly = true, HashSet<int> exceptTypesHS = null)
		{
			ProjectItem[] array = ((item.Children == null) ? new ProjectItem[0] : item.Children.Where((ProjectItem c) => c.IsFolder).ToArray());
			Action<ProjectEventHandler>[] loadFilesActions = new Action<ProjectEventHandler>[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				ProjectItem childItem = array[i];
				loadFilesActions[i] = delegate(ProjectEventHandler cb)
				{
					LoadFiles(childItem, cb, metaOnly, exceptTypesHS);
				};
			}
			m_storage.GetFiles(item.ToString(), delegate(StoragePayload<string, string[]> getFilesCompleted)
			{
				string[] data = getFilesCompleted.Data;
				if (data == null && data.Length > 0)
				{
					Parallel(loadFilesActions, delegate
					{
						if (callback != null)
						{
							callback(new ProjectPayload());
						}
					});
				}
				else
				{
					int metaLength;
					if (metaOnly)
					{
						data = data.Where((string n) => n.EndsWith(".rtmeta")).ToArray();
						metaLength = data.Length;
					}
					else
					{
						data = data.Where((string n) => n.EndsWith(".rtmeta")).ToArray();
						metaLength = data.Length;
						Array.Resize(ref data, metaLength + metaLength);
						for (int j = 0; j < metaLength; j++)
						{
							data[metaLength + j] = data[j].Remove(data[j].LastIndexOf(".rtmeta"));
						}
					}
					if (metaLength > 0)
					{
						m_storage.LoadFiles(data, delegate(StoragePayload<string[], byte[][]> loadFilesCompleted)
						{
							if (item.Children == null)
							{
								item.Children = new List<ProjectItem>();
							}
							for (int k = 0; k < metaLength; k++)
							{
								ProjectItemMeta meta = null;
								ProjectItemData data2 = null;
								byte[] array2 = loadFilesCompleted.Data[k];
								if (array2 != null)
								{
									meta = m_serializer.Deserialize<ProjectItemMeta>(array2);
								}
								if (!metaOnly && meta != null && (exceptTypesHS == null || !exceptTypesHS.Contains(meta.TypeCode)))
								{
									byte[] array3 = loadFilesCompleted.Data[metaLength + k];
									if (array3 != null)
									{
										data2 = m_serializer.Deserialize<ProjectItemData>(array3);
									}
								}
								if (meta.TypeCode == 1)
								{
									ProjectItem projectItem = item.Children.Where((ProjectItem c) => c.IsFolder && c.NameExt == meta.Name).FirstOrDefault();
									if (projectItem != null)
									{
										projectItem.Internal_Meta = meta;
									}
								}
								else
								{
									ProjectItem item2 = new ProjectItem(meta, data2);
									item.AddChild(item2);
								}
							}
							Parallel(loadFilesActions, delegate
							{
								if (callback != null)
								{
									callback(new ProjectPayload());
								}
							});
						});
					}
					else
					{
						Parallel(loadFilesActions, delegate
						{
							if (callback != null)
							{
								callback(new ProjectPayload());
							}
						});
					}
				}
			});
		}

		private void LoadFolders(ProjectItem item, ProjectEventHandler callback)
		{
			m_storage.GetFolders(item.ToString(), delegate(StoragePayload<string, string[]> getFoldersCompleted)
			{
				string[] data = getFoldersCompleted.Data;
				if (data != null && data.Length > 0)
				{
					Action<ProjectEventHandler>[] array = new Action<ProjectEventHandler>[data.Length];
					if (item.Children == null)
					{
						item.Children = new List<ProjectItem>(data.Length);
					}
					for (int i = 0; i < data.Length; i++)
					{
						string name = data[i];
						ProjectItem childItem = ProjectItem.CreateFolder(name);
						item.AddChild(childItem);
						array[i] = delegate(ProjectEventHandler cb)
						{
							LoadFolders(childItem, cb);
						};
					}
					Parallel(array, delegate
					{
						if (callback != null)
						{
							callback(new ProjectPayload());
						}
					});
				}
				else if (callback != null)
				{
					callback(new ProjectPayload());
				}
			}, false);
		}

		public void SaveProjectMeta(string name, ProjectMeta meta, ProjectEventHandler callback)
		{
			m_storage.SaveFile(name + ".rtpmeta", m_serializer.Serialize(meta), delegate
			{
				if (callback != null)
				{
					callback(new ProjectPayload());
				}
			});
		}

		public void Load(string[] path, ProjectEventHandler<ProjectItem[]> callback, params int[] exceptTypes)
		{
			int pathLength = path.Length;
			Array.Resize(ref path, pathLength + pathLength);
			for (int i = 0; i < pathLength; i++)
			{
				path[pathLength + i] = path[i] + ".rtmeta";
			}
			HashSet<int> exceptTypesHs = null;
			if (exceptTypes != null && exceptTypes.Length > 0)
			{
				exceptTypesHs = new HashSet<int>();
				for (int j = 0; j < exceptTypes.Length; j++)
				{
					if (!exceptTypesHs.Contains(exceptTypes[j]))
					{
						exceptTypesHs.Add(exceptTypes[j]);
					}
				}
			}
			m_storage.LoadFiles(path, delegate(StoragePayload<string[], byte[][]> loadFilesResult)
			{
				List<ProjectItem> list = new List<ProjectItem>();
				for (int k = 0; k < pathLength; k++)
				{
					byte[] array = loadFilesResult.Data[k];
					byte[] array2 = loadFilesResult.Data[pathLength + k];
					if (array != null && array2 != null)
					{
						ProjectItemMeta projectItemMeta = m_serializer.Deserialize<ProjectItemMeta>(array2);
						ProjectItemData data = ((exceptTypesHs != null && exceptTypesHs.Contains(projectItemMeta.TypeCode)) ? null : m_serializer.Deserialize<ProjectItemData>(array));
						list.Add(new ProjectItem(projectItemMeta, data));
					}
				}
				callback(new ProjectPayload<ProjectItem[]>(list.ToArray()));
			});
		}

		public void LoadData(ProjectItem[] items, ProjectEventHandler<ProjectItem[]> callback, params int[] exceptTypes)
		{
			HashSet<int> hashSet = null;
			if (exceptTypes != null && exceptTypes.Length > 0)
			{
				hashSet = new HashSet<int>();
				for (int i = 0; i < exceptTypes.Length; i++)
				{
					if (!hashSet.Contains(exceptTypes[i]))
					{
						hashSet.Add(exceptTypes[i]);
					}
				}
			}
			if (exceptTypes != null)
			{
				items = items.Where((ProjectItem item) => !exceptTypes.Contains(item.TypeCode)).ToArray();
			}
			string[] path = items.Select((ProjectItem item) => item.ToString()).ToArray();
			m_storage.LoadFiles(path, delegate(StoragePayload<string[], byte[][]> loadFilesResult)
			{
				for (int j = 0; j < path.Length; j++)
				{
					byte[] array = loadFilesResult.Data[j];
					if (array != null)
					{
						ProjectItemData internal_Data = m_serializer.Deserialize<ProjectItemData>(array);
						items[j].Internal_Data = internal_Data;
					}
				}
				callback(new ProjectPayload<ProjectItem[]>(items));
			});
		}

		public void Save(ProjectItem item, bool metaOnly, ProjectEventHandler callback)
		{
			Save(item, item.ToString(), metaOnly, callback);
		}

		private void Save(ProjectItem item, string path, bool metaOnly, ProjectEventHandler callback)
		{
			if (item.IsFolder)
			{
				ProjectItem[] items = item.FlattenHierarchy(true);
				Save(items, metaOnly, callback);
				return;
			}
			m_storage.SaveFile(path + ".rtmeta", m_serializer.Serialize(item.Internal_Meta), delegate
			{
				if (item.Internal_Data != null)
				{
					if (metaOnly)
					{
						if (callback != null)
						{
							callback(new ProjectPayload());
						}
					}
					else
					{
						m_storage.SaveFile(path, m_serializer.Serialize(item.Internal_Data), delegate
						{
							if (callback != null)
							{
								callback(new ProjectPayload());
							}
						});
					}
				}
				else if (callback != null)
				{
					callback(new ProjectPayload());
				}
			});
		}

		public void Save(ProjectItem[] items, bool metaOnly, ProjectEventHandler callback)
		{
			ProjectItem[] source = items.Where((ProjectItem item) => item.IsFolder).ToArray();
			string[] path = source.Select((ProjectItem item) => item.ToString()).ToArray();
			byte[][] fileData = ((!metaOnly) ? (from item in items
				where item.Internal_Data != null
				select m_serializer.Serialize(item.Internal_Data)).Union(items.Select((ProjectItem item) => m_serializer.Serialize(item.Internal_Meta))).ToArray() : items.Select((ProjectItem item) => m_serializer.Serialize(item.Internal_Meta)).ToArray());
			string[] filePath = ((!metaOnly) ? (from item in items
				where item.Internal_Data != null
				select item.ToString()).Union(items.Select((ProjectItem item) => item.ToString() + ".rtmeta")).ToArray() : items.Select((ProjectItem item) => item.ToString() + ".rtmeta").ToArray());
			m_storage.CreateFolders(path, delegate
			{
				m_storage.SaveFiles(filePath, fileData, delegate
				{
					if (callback != null)
					{
						callback(new ProjectPayload());
					}
				});
			});
		}

		public void Delete(ProjectItem item, ProjectEventHandler callback)
		{
			if (item == null)
			{
				callback(new ProjectPayload());
				return;
			}
			string text = item.ToString();
			if (item.IsFolder)
			{
				m_storage.DeleteFolder(text, delegate
				{
					if (callback != null)
					{
						callback(new ProjectPayload());
					}
				});
				return;
			}
			string[] path = new string[2]
			{
				text,
				text + ".rtmeta"
			};
			m_storage.DeleteFiles(path, delegate
			{
				if (callback != null)
				{
					callback(new ProjectPayload());
				}
			});
		}

		public void Delete(ProjectItem[] items, ProjectEventHandler callback)
		{
			items = ProjectItem.GetRootItems(items);
			string[] folderPath = (from item in items
				where item.IsFolder
				select item.ToString()).ToArray();
			ProjectItem[] source = items.Where((ProjectItem item) => !item.IsFolder).ToArray();
			string[] filePath = source.Select((ProjectItem item) => item.ToString()).Union(source.Select((ProjectItem item) => item.ToString() + ".rtmeta")).ToArray();
			GroupOperation(folderPath, filePath, m_storage.DeleteFolders, m_storage.DeleteFiles, callback);
		}

		public void Move(ProjectItem item, ProjectItem parent, ProjectEventHandler callback)
		{
			string srcPath = item.ToString();
			parent.AddChild(item);
			item.Name = ProjectItem.GetUniqueName(item.Name, item, item.Parent);
			string dstPath = item.ToString();
			Move(item, callback, srcPath, dstPath);
		}

		public void Move(ProjectItem[] items, ProjectItem parent, ProjectEventHandler callback)
		{
			items = ProjectItem.GetRootItems(items);
			string[] folderPath = (from item in items
				where item.IsFolder
				select item.ToString()).ToArray();
			ProjectItem[] source = items.Where((ProjectItem item) => !item.IsFolder).ToArray();
			string[] filePath = source.Select((ProjectItem item) => item.ToString()).Union(source.Select((ProjectItem item) => item.ToString() + ".rtmeta")).ToArray();
			ProjectItem[] array = items;
			foreach (ProjectItem projectItem in array)
			{
				parent.AddChild(projectItem);
				projectItem.Name = ProjectItem.GetUniqueName(projectItem.Name, projectItem, projectItem.Parent);
			}
			string[] folderDstPath = (from item in items
				where item.IsFolder
				select item.ToString()).ToArray();
			string[] fileDstPath = source.Select((ProjectItem item) => item.ToString()).Union(source.Select((ProjectItem item) => item.ToString() + ".rtmeta")).ToArray();
			GroupOperation(folderPath, filePath, folderDstPath, fileDstPath, m_storage.MoveFolders, m_storage.MoveFiles, callback);
		}

		public void Rename(ProjectItem item, string name, ProjectEventHandler callback)
		{
			string srcPath = item.ToString();
			string name2 = item.Name;
			item.Name = ProjectItem.GetUniqueName(name, item, item.Parent);
			string dstPath = item.ToString();
			if (!item.IsFolder && !item.IsScene)
			{
				m_storage.LoadFile(srcPath, delegate(StoragePayload<string, byte[]> loadFilesResult)
				{
					byte[] data = loadFilesResult.Data;
					if (data != null)
					{
						ProjectItemData internal_Data = m_serializer.Deserialize<ProjectItemData>(data);
						item.Internal_Data = internal_Data;
						item.Rename(name);
					}
					Save(item, srcPath, false, delegate
					{
						UnloadData(item);
						Move(item, callback, srcPath, dstPath);
					});
				});
			}
			else if (!item.IsFolder)
			{
				Save(item, srcPath, true, delegate
				{
					Move(item, callback, srcPath, dstPath);
				});
			}
			else
			{
				Move(item, callback, srcPath, dstPath);
			}
		}

		private void Move(ProjectItem item, ProjectEventHandler callback, string srcPath, string dstPath)
		{
			if (item.IsFolder)
			{
				m_storage.MoveFolder(srcPath, dstPath, delegate
				{
					if (callback != null)
					{
						callback(new ProjectPayload());
					}
				});
				return;
			}
			m_storage.MoveFiles(new string[2]
			{
				srcPath,
				srcPath + ".rtmeta"
			}, new string[2]
			{
				dstPath,
				dstPath + ".rtmeta"
			}, delegate
			{
				if (callback != null)
				{
					callback(new ProjectPayload());
				}
			});
		}

		private void GroupOperation(string[] folderPath, string[] filePath, string[] folderDstPath, string[] fileDstPath, Action<string[], string[], StorageEventHandler<string[], string[]>> folderOperation, Action<string[], string[], StorageEventHandler<string[], string[]>> fileOperation, ProjectEventHandler callback)
		{
			if (folderPath.Length > 0)
			{
				folderOperation(folderPath, folderDstPath, delegate
				{
					if (filePath.Length > 0)
					{
						fileOperation(filePath, fileDstPath, delegate
						{
							if (callback != null)
							{
								callback(new ProjectPayload());
							}
						});
					}
					else if (callback != null)
					{
						callback(new ProjectPayload());
					}
				});
			}
			else if (filePath.Length > 0)
			{
				fileOperation(filePath, fileDstPath, delegate
				{
					if (callback != null)
					{
						callback(new ProjectPayload());
					}
				});
			}
			else if (callback != null)
			{
				callback(new ProjectPayload());
			}
		}

		private void GroupOperation(string[] folderPath, string[] filePath, Action<string[], StorageEventHandler<string[]>> folderOperation, Action<string[], StorageEventHandler<string[]>> fileOperation, ProjectEventHandler callback)
		{
			if (folderPath.Length > 0)
			{
				folderOperation(folderPath, delegate
				{
					if (filePath.Length > 0)
					{
						fileOperation(filePath, delegate
						{
							if (callback != null)
							{
								callback(new ProjectPayload());
							}
						});
					}
					else if (callback != null)
					{
						callback(new ProjectPayload());
					}
				});
			}
			else if (filePath.Length > 0)
			{
				fileOperation(filePath, delegate
				{
					if (callback != null)
					{
						callback(new ProjectPayload());
					}
				});
			}
			else if (callback != null)
			{
				callback(new ProjectPayload());
			}
		}

		public void UnloadData(ProjectItem projectItem)
		{
			if (projectItem == null)
			{
				return;
			}
			projectItem.Internal_Data = null;
			if (projectItem.Children != null)
			{
				for (int i = 0; i < projectItem.Children.Count; i++)
				{
					UnloadData(projectItem.Children[i]);
				}
			}
		}

		public void Exists(ProjectItem item, ProjectEventHandler<bool> callback)
		{
			if (item.IsFolder)
			{
				m_storage.CheckFolderExists(item.ToString(), delegate(StoragePayload<string, bool> result)
				{
					if (callback != null)
					{
						callback(new ProjectPayload<bool>(result.Data));
					}
				});
				return;
			}
			m_storage.CheckFileExists(item.ToString(), delegate(StoragePayload<string, bool> result)
			{
				if (callback != null)
				{
					callback(new ProjectPayload<bool>(result.Data));
				}
			});
		}
	}
}
