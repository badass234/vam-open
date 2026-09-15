namespace Battlehub.RTSaveLoad
{
	public interface IStorage
	{
		void CheckFolderExists(string path, StorageEventHandler<string, bool> callback);

		void CheckFileExists(string path, StorageEventHandler<string, bool> callback);

		void GetFolders(string path, StorageEventHandler<string, string[]> callback, bool fullPath = true);

		void GetFiles(string path, StorageEventHandler<string, string[]> callback, bool fullPath = true);

		void SaveFile(string path, byte[] data, StorageEventHandler<string> callback);

		void SaveFiles(string[] path, byte[][] data, StorageEventHandler<string[]> callback);

		void LoadFile(string path, StorageEventHandler<string, byte[]> callback);

		void LoadFiles(string[] path, StorageEventHandler<string[], byte[][]> callback);

		void DeleteFile(string path, StorageEventHandler<string> callback);

		void DeleteFiles(string[] path, StorageEventHandler<string[]> callback);

		void CopyFile(string sourcePath, string destinationPath, StorageEventHandler<string, string> callback);

		void CopyFiles(string[] sourcePath, string[] destinationPath, StorageEventHandler<string[], string[]> callback);

		void MoveFile(string sourcePath, string destinationPath, StorageEventHandler<string, string> callback);

		void MoveFiles(string[] sourcePath, string[] destinationPath, StorageEventHandler<string[], string[]> callback);

		void CreateFolder(string path, StorageEventHandler<string> callback);

		void CreateFolders(string[] path, StorageEventHandler<string[]> callback);

		void DeleteFolder(string path, StorageEventHandler<string> callback);

		void DeleteFolders(string[] path, StorageEventHandler<string[]> callback);

		void CopyFolder(string sourcePath, string destinationPath, StorageEventHandler<string, string> callback);

		void CopyFolders(string[] sourcePath, string[] destinationPath, StorageEventHandler<string[], string[]> callback);

		void MoveFolder(string sourcePath, string destinationPath, StorageEventHandler<string, string> callback);

		void MoveFolders(string[] sourcePath, string[] destinationPath, StorageEventHandler<string[], string[]> callback);
	}
}
