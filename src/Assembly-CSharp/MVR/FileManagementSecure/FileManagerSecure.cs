using System;
using System.Collections.Generic;
using System.IO;
using MVR.FileManagement;

namespace MVR.FileManagementSecure
{
	public class FileManagerSecure
	{
		public static string GetFullPath(string path)
		{
			return FileManager.GetFullPath(path);
		}

		public static string NormalizeLoadPath(string path)
		{
			return FileManager.NormalizeLoadPath(path);
		}

		public static string NormalizePath(string path)
		{
			return FileManager.NormalizePath(path);
		}

		public static string GetDirectoryName(string path, bool returnSlashPath = false)
		{
			return FileManager.GetDirectoryName(path, returnSlashPath);
		}

		public static string GetFileName(string path)
		{
			return Path.GetFileName(path);
		}

		public static bool FileExists(string path, bool onlySystemFiles = false)
		{
			return FileManager.FileExists(path, onlySystemFiles, true);
		}

		public static bool IsFileInPackage(string path)
		{
			return FileManager.IsFileInPackage(path);
		}

		public static DateTime FileLastWriteTime(string path, bool onlySystemFiles = false)
		{
			return FileManager.FileLastWriteTime(path, onlySystemFiles, true);
		}

		public static DateTime FileCreationTime(string path, bool onlySystemFiles = false)
		{
			return FileManager.FileCreationTime(path, onlySystemFiles, true);
		}

		public static void RegisterRefreshHandler(OnRefresh refreshHandler)
		{
			FileManager.RegisterRefreshHandler(refreshHandler);
		}

		public static void UnregisterRefreshHandler(OnRefresh refreshHandler)
		{
			FileManager.UnregisterRefreshHandler(refreshHandler);
		}

		public static bool PackageExists(string packageUid)
		{
			VarPackage package = FileManager.GetPackage(packageUid);
			return package != null;
		}

		public static int GetPackageVersion(string packageUid)
		{
			return FileManager.GetPackage(packageUid)?.Version ?? (-1);
		}

		public static List<ShortCut> GetShortCutsForDirectory(string dir, bool allowNavigationAboveRegularDirectories = false, bool useFullPaths = false, bool generateAllFlattenedShortcut = false, bool includeRegularDirsInFlattenedShortcut = false)
		{
			return FileManager.GetShortCutsForDirectory(dir, allowNavigationAboveRegularDirectories, useFullPaths, generateAllFlattenedShortcut, includeRegularDirsInFlattenedShortcut);
		}

		public static bool DirectoryExists(string path, bool onlySystemDirectories = false)
		{
			return FileManager.DirectoryExists(path, onlySystemDirectories, true);
		}

		public static bool IsDirectoryInPackage(string path)
		{
			return FileManager.IsDirectoryInPackage(path);
		}

		public static DateTime DirectoryLastWriteTime(string path, bool onlySystemDirectories = false)
		{
			return FileManager.DirectoryLastWriteTime(path, onlySystemDirectories, true);
		}

		public static DateTime DirectoryCreationTime(string path, bool onlySystemDirectories = false)
		{
			return FileManager.DirectoryCreationTime(path, onlySystemDirectories, true);
		}

		public static string[] GetDirectories(string dir, string pattern = null)
		{
			return FileManager.GetDirectories(dir, pattern, true);
		}

		public static string[] GetFiles(string dir, string pattern = null)
		{
			return FileManager.GetFiles(dir, pattern, true);
		}

		public static void CreateDirectory(string path)
		{
			FileManager.CreateDirectoryFromPlugin(path, null, null, null);
		}

		public static void CreateDirectory(string path, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			FileManager.CreateDirectoryFromPlugin(path, confirmCallback, denyCallback, exceptionCallback);
		}

		public static byte[] ReadAllBytes(string path)
		{
			return FileManager.ReadAllBytes(path, true);
		}

		public static string ReadAllText(string path)
		{
			return FileManager.ReadAllText(path, true);
		}

		public static void WriteAllText(string path, string text)
		{
			FileManager.WriteAllTextFromPlugin(path, text, null, null, null);
		}

		public static void WriteAllText(string path, string text, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			FileManager.WriteAllTextFromPlugin(path, text, confirmCallback, denyCallback, exceptionCallback);
		}

		public static void WriteAllBytes(string path, byte[] bytes)
		{
			FileManager.WriteAllBytesFromPlugin(path, bytes, null, null, null);
		}

		public static void WriteAllBytes(string path, byte[] bytes, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			FileManager.WriteAllBytesFromPlugin(path, bytes, confirmCallback, denyCallback, exceptionCallback);
		}

		public static void DeleteFile(string path)
		{
			FileManager.DeleteFileFromPlugin(path, null, null, null);
		}

		public static void DeleteFile(string path, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			FileManager.DeleteFileFromPlugin(path, confirmCallback, denyCallback, exceptionCallback);
		}

		public static void CopyFile(string oldPath, string newPath)
		{
			FileManager.CopyFileFromPlugin(oldPath, newPath, null, null, null);
		}

		public static void CopyFile(string oldPath, string newPath, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			FileManager.CopyFileFromPlugin(oldPath, newPath, confirmCallback, denyCallback, exceptionCallback);
		}

		public static void MoveFile(string oldPath, string newPath, bool overwrite = true)
		{
			FileManager.MoveFileFromPlugin(oldPath, newPath, overwrite, null, null, null);
		}

		public static void MoveFile(string oldPath, string newPath, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback, bool overwrite = true)
		{
			FileManager.MoveFileFromPlugin(oldPath, newPath, overwrite, confirmCallback, denyCallback, exceptionCallback);
		}
	}
}
