using System.IO;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public class FileSystemStorage : IStorage
	{
		private string m_basePath;

		public FileSystemStorage(string basePath)
		{
			Debug.Log("FileSystemStorage root: " + basePath);
			m_basePath = basePath;
		}

		private void CombineWithBasePath(ref string[] path)
		{
			for (int i = 0; i < path.Length; i++)
			{
				path[i] = Path.Combine(m_basePath, path[i].TrimStart('/'));
			}
		}

		private void CombineWithBasePath(ref string path)
		{
			path = Path.Combine(m_basePath, path.TrimStart('/'));
		}

		public void CheckFolderExists(string path, StorageEventHandler<string, bool> callback)
		{
			CombineWithBasePath(ref path);
			bool data = Directory.Exists(path);
			callback?.Invoke(new StoragePayload<string, bool>(path, data));
		}

		public void CheckFileExists(string path, StorageEventHandler<string, bool> callback)
		{
			CombineWithBasePath(ref path);
			bool data = File.Exists(path);
			callback?.Invoke(new StoragePayload<string, bool>(path, data));
		}

		public void CopyFile(string sourcePath, string destinationPath, StorageEventHandler<string, string> callback)
		{
			CombineWithBasePath(ref sourcePath);
			CombineWithBasePath(ref destinationPath);
			File.Copy(sourcePath, destinationPath);
			callback?.Invoke(new StoragePayload<string, string>(sourcePath, destinationPath));
		}

		public void CopyFiles(string[] sourcePath, string[] destinationPath, StorageEventHandler<string[], string[]> callback)
		{
			CombineWithBasePath(ref sourcePath);
			CombineWithBasePath(ref destinationPath);
			for (int i = 0; i < sourcePath.Length; i++)
			{
				File.Copy(sourcePath[i], destinationPath[i]);
			}
			callback?.Invoke(new StoragePayload<string[], string[]>(sourcePath, destinationPath));
		}

		public void CopyFolder(string sourcePath, string destinationPath, StorageEventHandler<string, string> callback)
		{
			CombineWithBasePath(ref sourcePath);
			CombineWithBasePath(ref destinationPath);
			DirectoryInfo source = new DirectoryInfo(sourcePath);
			DirectoryInfo destination = new DirectoryInfo(destinationPath);
			CopyAll(source, destination);
			callback?.Invoke(new StoragePayload<string, string>(sourcePath, destinationPath));
		}

		public void CopyFolders(string[] sourcePath, string[] destinationPath, StorageEventHandler<string[], string[]> callback)
		{
			CombineWithBasePath(ref sourcePath);
			CombineWithBasePath(ref destinationPath);
			for (int i = 0; i < sourcePath.Length; i++)
			{
				DirectoryInfo source = new DirectoryInfo(sourcePath[i]);
				DirectoryInfo destination = new DirectoryInfo(destinationPath[i]);
				CopyAll(source, destination);
			}
			callback?.Invoke(new StoragePayload<string[], string[]>(sourcePath, destinationPath));
		}

		private static void CopyAll(DirectoryInfo source, DirectoryInfo destination)
		{
			Directory.CreateDirectory(destination.FullName);
			FileInfo[] files = source.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				fileInfo.CopyTo(Path.Combine(destination.FullName, fileInfo.Name), true);
			}
			DirectoryInfo[] directories = source.GetDirectories();
			foreach (DirectoryInfo directoryInfo in directories)
			{
				DirectoryInfo destination2 = destination.CreateSubdirectory(directoryInfo.Name);
				CopyAll(directoryInfo, destination2);
			}
		}

		public void CreateFolder(string path, StorageEventHandler<string> callback)
		{
			CombineWithBasePath(ref path);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			callback?.Invoke(new StoragePayload<string>(path));
		}

		public void CreateFolders(string[] path, StorageEventHandler<string[]> callback)
		{
			CombineWithBasePath(ref path);
			for (int i = 0; i < path.Length; i++)
			{
				Directory.CreateDirectory(path[i]);
			}
			callback?.Invoke(new StoragePayload<string[]>(path));
		}

		public void DeleteFile(string path, StorageEventHandler<string> callback)
		{
			CombineWithBasePath(ref path);
			File.Delete(path);
			callback?.Invoke(new StoragePayload<string>(path));
		}

		public void DeleteFiles(string[] path, StorageEventHandler<string[]> callback)
		{
			CombineWithBasePath(ref path);
			for (int i = 0; i < path.Length; i++)
			{
				File.Delete(path[i]);
			}
			callback?.Invoke(new StoragePayload<string[]>(path));
		}

		public void DeleteFolder(string path, StorageEventHandler<string> callback)
		{
			CombineWithBasePath(ref path);
			Directory.Delete(path, true);
			callback?.Invoke(new StoragePayload<string>(path));
		}

		public void DeleteFolders(string[] path, StorageEventHandler<string[]> callback)
		{
			CombineWithBasePath(ref path);
			for (int i = 0; i < path.Length; i++)
			{
				Directory.Delete(path[i], true);
			}
			callback?.Invoke(new StoragePayload<string[]>(path));
		}

		public void GetFiles(string path, StorageEventHandler<string, string[]> callback, bool fullPath = true)
		{
			CombineWithBasePath(ref path);
			string[] data = ((!fullPath) ? Directory.GetFiles(path).Select(Path.GetFileName).ToArray() : (from p in Directory.GetFiles(path)
				select PathHelper.GetRelativePath(p, m_basePath)).ToArray());
			callback?.Invoke(new StoragePayload<string, string[]>(path, data));
		}

		public void GetFolders(string path, StorageEventHandler<string, string[]> callback, bool fullPath = true)
		{
			CombineWithBasePath(ref path);
			string[] data = ((!fullPath) ? (from d in Directory.GetDirectories(path)
				select new DirectoryInfo(d).Name).ToArray() : (from p in Directory.GetDirectories(path)
				select PathHelper.GetRelativePath(p, m_basePath)).ToArray());
			callback?.Invoke(new StoragePayload<string, string[]>(path, data));
		}

		public void LoadFile(string path, StorageEventHandler<string, byte[]> callback)
		{
			CombineWithBasePath(ref path);
			byte[] data = null;
			if (File.Exists(path))
			{
				data = File.ReadAllBytes(path);
			}
			callback?.Invoke(new StoragePayload<string, byte[]>(path, data));
		}

		public void LoadFiles(string[] path, StorageEventHandler<string[], byte[][]> callback)
		{
			CombineWithBasePath(ref path);
			byte[][] array = new byte[path.Length][];
			for (int i = 0; i < path.Length; i++)
			{
				if (File.Exists(path[i]))
				{
					array[i] = File.ReadAllBytes(path[i]);
				}
			}
			callback?.Invoke(new StoragePayload<string[], byte[][]>(path, array));
		}

		public void MoveFile(string sourcePath, string destinationPath, StorageEventHandler<string, string> callback)
		{
			CombineWithBasePath(ref sourcePath);
			CombineWithBasePath(ref destinationPath);
			File.Move(sourcePath, destinationPath);
			callback?.Invoke(new StoragePayload<string, string>(sourcePath, destinationPath));
		}

		public void MoveFiles(string[] sourcePath, string[] destinationPath, StorageEventHandler<string[], string[]> callback)
		{
			CombineWithBasePath(ref sourcePath);
			CombineWithBasePath(ref destinationPath);
			for (int i = 0; i < sourcePath.Length; i++)
			{
				File.Move(sourcePath[i], destinationPath[i]);
			}
			callback?.Invoke(new StoragePayload<string[], string[]>(sourcePath, destinationPath));
		}

		public void MoveFolder(string sourcePath, string destinationPath, StorageEventHandler<string, string> callback)
		{
			CombineWithBasePath(ref sourcePath);
			CombineWithBasePath(ref destinationPath);
			if (sourcePath != destinationPath)
			{
				Directory.Move(sourcePath, destinationPath);
			}
			callback?.Invoke(new StoragePayload<string, string>(sourcePath, destinationPath));
		}

		public void MoveFolders(string[] sourcePath, string[] destinationPath, StorageEventHandler<string[], string[]> callback)
		{
			CombineWithBasePath(ref sourcePath);
			CombineWithBasePath(ref destinationPath);
			for (int i = 0; i < sourcePath.Length; i++)
			{
				Directory.Move(sourcePath[i], destinationPath[i]);
			}
			callback?.Invoke(new StoragePayload<string[], string[]>(sourcePath, destinationPath));
		}

		public void SaveFile(string path, byte[] data, StorageEventHandler<string> callback)
		{
			CombineWithBasePath(ref path);
			File.WriteAllBytes(path, data);
			callback?.Invoke(new StoragePayload<string>(path));
		}

		public void SaveFiles(string[] path, byte[][] data, StorageEventHandler<string[]> callback)
		{
			CombineWithBasePath(ref path);
			for (int i = 0; i < path.Length; i++)
			{
				File.WriteAllBytes(path[i], data[i]);
			}
			callback?.Invoke(new StoragePayload<string[]>(path));
		}
	}
}
