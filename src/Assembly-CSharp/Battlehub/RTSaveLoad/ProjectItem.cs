using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class ProjectItem
	{
		public ProjectItemMeta Internal_Meta;

		public ProjectItemData Internal_Data;

		public ProjectItem Parent;

		public List<ProjectItem> Children;

		public bool IsExposedFromEditor
		{
			get
			{
				return Internal_Meta.IsExposedFromEditor;
			}
			set
			{
				Internal_Meta.IsExposedFromEditor = value;
			}
		}

		public string BundleName
		{
			get
			{
				if (Internal_Meta == null)
				{
					return null;
				}
				return Internal_Meta.BundleName;
			}
		}

		public string Name
		{
			get
			{
				return Internal_Meta.Name;
			}
			set
			{
				Internal_Meta.Name = value;
			}
		}

		public string NameExt
		{
			get
			{
				string ext = Ext;
				if (string.IsNullOrEmpty(ext))
				{
					return Internal_Meta.Name;
				}
				return $"{Internal_Meta.Name}.{Ext}";
			}
			set
			{
				if (value == null)
				{
					Internal_Meta.Name = null;
					return;
				}
				int num = value.LastIndexOf("." + Ext);
				if (num >= 0)
				{
					Internal_Meta.Name = value.Remove(num);
				}
				else
				{
					Internal_Meta.Name = value;
				}
			}
		}

		public int TypeCode
		{
			get
			{
				if (Internal_Meta == null)
				{
					return 0;
				}
				return Internal_Meta.TypeCode;
			}
		}

		public string TypeName
		{
			get
			{
				if (Internal_Meta == null)
				{
					return null;
				}
				return Internal_Meta.TypeName;
			}
		}

		public bool IsFolder => Internal_Meta.TypeCode == 1;

		public bool IsScene => Internal_Meta.TypeCode == 2;

		public bool IsResource => !IsFolder && !IsScene;

		public bool IsGameObject
		{
			get
			{
				Type type = Type.GetType(TypeName);
				return type == typeof(GameObject);
			}
		}

		public string Ext => ProjectItemTypes.Ext[Internal_Meta.TypeCode];

		public ProjectItem()
		{
		}

		public ProjectItem(ProjectItemMeta meta, ProjectItemData data)
		{
			Internal_Meta = meta;
			Internal_Data = data;
		}

		public void AddChild(ProjectItem item)
		{
			if (Children == null)
			{
				Children = new List<ProjectItem>();
			}
			if (item.Parent != null)
			{
				item.Parent.RemoveChild(item);
			}
			Children.Add(item);
			item.Parent = this;
		}

		public void RemoveChild(ProjectItem item)
		{
			if (Children != null)
			{
				Children.Remove(item);
				item.Parent = null;
			}
		}

		public int GetSiblingIndex()
		{
			return Parent.Children.IndexOf(this);
		}

		public void SetSiblingIndex(int index)
		{
			Parent.Children.Remove(this);
			Parent.Children.Insert(index, this);
		}

		public ProjectItem Get(string path)
		{
			path = path.Trim('/');
			string[] array = path.Split('/');
			ProjectItem projectItem = this;
			for (int i = 1; i < array.Length; i++)
			{
				string pathPart = array[i];
				if (projectItem.Children == null)
				{
					return projectItem;
				}
				projectItem = ((i != array.Length - 1) ? projectItem.Children.Where((ProjectItem child) => child.Name == pathPart).FirstOrDefault() : projectItem.Children.Where((ProjectItem child) => child.NameExt == pathPart).FirstOrDefault());
				if (projectItem == null)
				{
					break;
				}
			}
			return projectItem;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (ProjectItem projectItem = this; projectItem != null; projectItem = projectItem.Parent)
			{
				stringBuilder.Insert(0, projectItem.Internal_Meta.Name);
				stringBuilder.Insert(0, "/");
			}
			string ext = Ext;
			if (string.IsNullOrEmpty(ext))
			{
				return stringBuilder.ToString();
			}
			return $"{stringBuilder.ToString()}.{Ext}";
		}

		public static ProjectItem CreateFolder(string name)
		{
			ProjectItem projectItem = new ProjectItem();
			projectItem.Internal_Meta = new ProjectItemMeta
			{
				Name = name,
				TypeCode = 1
			};
			return projectItem;
		}

		public static ProjectItem CreateScene(string name)
		{
			ProjectItem projectItem = new ProjectItem();
			projectItem.Internal_Meta = new ProjectItemMeta
			{
				Name = name,
				TypeCode = 2
			};
			return projectItem;
		}

		public static string GetUniqueName(string desiredName, UnityEngine.Object obj, ProjectItem parent)
		{
			string text = ProjectItemTypes.Ext[ProjectItemTypes.GetProjectItemType(obj.GetType())];
			string[] existingNames = parent.Children.Select((ProjectItem child) => child.NameExt).ToArray();
			string uniqueName = PathHelper.GetUniqueName(desiredName, text, existingNames);
			if (uniqueName == null)
			{
				return null;
			}
			int num = uniqueName.LastIndexOf("." + text);
			if (num >= 0)
			{
				return uniqueName.Remove(num);
			}
			return uniqueName;
		}

		public static string GetUniqueName(string desiredName, ProjectItem item, ProjectItem parent, bool exceptItem = true)
		{
			return PathHelper.GetUniqueName(existingNames: (!exceptItem) ? parent.Children.Select((ProjectItem child) => child.NameExt).ToArray() : (from child in parent.Children.Except(new ProjectItem[1] { item })
				select child.NameExt).ToArray(), desiredName: desiredName, ext: item.Ext);
		}

		public static bool IsValidName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return true;
			}
			return Path.GetInvalidFileNameChars().All((char c) => !name.Contains(c));
		}

		public static ProjectItem[] GetRootItems(ProjectItem[] items)
		{
			HashSet<ProjectItem> hashSet = new HashSet<ProjectItem>();
			for (int i = 0; i < items.Length; i++)
			{
				if (!hashSet.Contains(items[i]))
				{
					hashSet.Add(items[i]);
				}
			}
			foreach (ProjectItem projectItem in items)
			{
				for (ProjectItem parent = projectItem.Parent; parent != null; parent = parent.Parent)
				{
					if (hashSet.Contains(parent))
					{
						hashSet.Remove(projectItem);
						break;
					}
				}
			}
			items = hashSet.ToArray();
			return items;
		}

		public ProjectItem[] FlattenHierarchy(bool includeSelf = false)
		{
			List<ProjectItem> list = new List<ProjectItem>();
			if (includeSelf)
			{
				list.Add(this);
			}
			GetAncestors(this, list);
			return list.ToArray();
		}

		private void GetAncestors(ProjectItem item, List<ProjectItem> list)
		{
			if (item.Children != null)
			{
				for (int i = 0; i < item.Children.Count; i++)
				{
					ProjectItem item2 = item.Children[i];
					list.Add(item2);
					GetAncestors(item2, list);
				}
			}
		}

		public void Rename(string name)
		{
			Internal_Data.Rename(Internal_Meta, name);
		}
	}
}
