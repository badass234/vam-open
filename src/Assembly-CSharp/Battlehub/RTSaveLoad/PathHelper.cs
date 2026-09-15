using System;
using System.Collections.Generic;
using System.IO;

namespace Battlehub.RTSaveLoad
{
	public static class PathHelper
	{
		public static bool IsPathRooted(string path)
		{
			return Path.IsPathRooted(path);
		}

		public static string GetRelativePath(string filespec, string folder)
		{
			Uri uri = new Uri(filespec);
			if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				folder += Path.DirectorySeparatorChar;
			}
			Uri uri2 = new Uri(folder);
			return Uri.UnescapeDataString(uri2.MakeRelativeUri(uri).ToString().Replace('/', Path.DirectorySeparatorChar));
		}

		public static string RemoveInvalidFineNameCharacters(string name)
		{
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			for (int i = 0; i < invalidFileNameChars.Length; i++)
			{
				name = name.Replace(invalidFileNameChars[i].ToString(), string.Empty);
			}
			return name;
		}

		public static string GetUniqueName(string desiredName, string ext, string[] existingNames)
		{
			if (existingNames == null || existingNames.Length == 0)
			{
				return desiredName;
			}
			for (int i = 0; i < existingNames.Length; i++)
			{
				existingNames[i] = existingNames[i].ToLower();
			}
			HashSet<string> hashSet = new HashSet<string>(existingNames);
			if (string.IsNullOrEmpty(ext))
			{
				if (!hashSet.Contains(desiredName.ToLower()))
				{
					return desiredName;
				}
			}
			else if (!hashSet.Contains($"{desiredName.ToLower()}.{ext}"))
			{
				return desiredName;
			}
			string[] array = desiredName.Split(' ');
			string text = array[array.Length - 1];
			int result;
			if (!int.TryParse(text, out result))
			{
				result = 1;
			}
			else
			{
				desiredName = desiredName.Substring(0, desiredName.Length - text.Length).TrimEnd(' ');
			}
			for (int j = 0; j < 10000; j++)
			{
				string text2 = ((!string.IsNullOrEmpty(ext)) ? $"{desiredName} {result}.{ext}" : $"{desiredName} {result}");
				if (!hashSet.Contains(text2.ToLower()))
				{
					return text2;
				}
				result++;
			}
			if (string.IsNullOrEmpty(ext))
			{
				return string.Format("{0} {1}", desiredName, Guid.NewGuid().ToString("N"));
			}
			return string.Format("{0} {1}.{2}", desiredName, Guid.NewGuid().ToString("N"), ext);
		}

		public static string GetUniqueName(string desiredName, string[] existingNames)
		{
			return GetUniqueName(desiredName, null, existingNames);
		}
	}
}
