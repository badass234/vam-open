using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MVR.FileManagement
{
	public abstract class FileEntry
	{
		protected string flagBasePath;

		protected string favPath;

		public virtual string Uid { get; protected set; }

		public virtual string UidLowerInvariant { get; protected set; }

		public virtual string Path { get; protected set; }

		public virtual string SlashPath { get; protected set; }

		public virtual string FullPath { get; protected set; }

		public virtual string FullSlashPath { get; protected set; }

		public virtual string Name { get; protected set; }

		public virtual bool Exists { get; protected set; }

		public virtual DateTime LastWriteTime { get; protected set; }

		public virtual long Size { get; protected set; }

		public string hidePath { get; protected set; }

		public FileEntry()
		{
		}

		public FileEntry(string path)
		{
			if (path == null)
			{
				throw new Exception("Null path in FileEntry constructor");
			}
			Path = path.Replace('/', '\\');
			SlashPath = path.Replace('\\', '/');
			FullPath = Path;
			FullSlashPath = SlashPath;
			Uid = SlashPath;
			UidLowerInvariant = Uid.ToLowerInvariant();
			Name = Regex.Replace(SlashPath, ".*/", string.Empty);
		}

		public override string ToString()
		{
			return Path;
		}

		public abstract FileEntryStream OpenStream();

		public abstract FileEntryStreamReader OpenStreamReader();

		public virtual bool HasFlagFile(string flagName)
		{
			return flagBasePath != null && File.Exists(flagBasePath + flagName);
		}

		public virtual void SetFlagFile(string flagName, bool b)
		{
			if (flagBasePath == null)
			{
				return;
			}
			string path = flagBasePath + flagName;
			if (File.Exists(path))
			{
				if (!b)
				{
					FileManager.DeleteFile(path);
				}
			}
			else if (b)
			{
				string directoryName = FileManager.GetDirectoryName(path);
				if (!FileManager.DirectoryExists(directoryName))
				{
					FileManager.CreateDirectory(directoryName);
				}
				FileManager.WriteAllText(path, string.Empty);
			}
		}

		public virtual bool IsFavorite()
		{
			return favPath != null && File.Exists(favPath);
		}

		public virtual void SetFavorite(bool b)
		{
			if (favPath == null)
			{
				return;
			}
			if (File.Exists(favPath))
			{
				if (!b)
				{
					FileManager.DeleteFile(favPath);
				}
			}
			else if (b)
			{
				string directoryName = FileManager.GetDirectoryName(favPath);
				if (!FileManager.DirectoryExists(directoryName))
				{
					FileManager.CreateDirectory(directoryName);
				}
				FileManager.WriteAllText(favPath, string.Empty);
			}
		}

		public virtual bool IsHidden()
		{
			return hidePath != null && File.Exists(hidePath);
		}

		public virtual void SetHidden(bool b)
		{
			if (hidePath == null)
			{
				return;
			}
			if (File.Exists(hidePath))
			{
				if (!b)
				{
					FileManager.DeleteFile(hidePath);
				}
			}
			else if (b)
			{
				string directoryName = FileManager.GetDirectoryName(hidePath);
				if (!FileManager.DirectoryExists(directoryName))
				{
					FileManager.CreateDirectory(directoryName);
				}
				FileManager.WriteAllText(hidePath, string.Empty);
			}
		}
	}
}
