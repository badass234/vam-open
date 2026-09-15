using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MVR.FileManagement
{
	public class VarFileEntry : FileEntry
	{
		protected string packagedHidePath;

		protected string packagedFlagPath;

		public VarPackage Package { get; protected set; }

		public string InternalPath { get; protected set; }

		public string InternalSlashPath { get; protected set; }

		public bool Simulated { get; protected set; }

		public VarFileEntry(VarPackage vp, string entryName, DateTime lastWriteTime, long size, bool simulated = false)
		{
			Package = vp;
			InternalSlashPath = entryName;
			flagBasePath = "AddonPackagesFilePrefs/" + vp.Uid + "/" + InternalSlashPath + ".";
			favPath = flagBasePath + "fav";
			base.hidePath = flagBasePath + "hide";
			Uid = vp.Uid + ":/" + InternalSlashPath;
			UidLowerInvariant = Uid.ToLowerInvariant();
			InternalPath = InternalSlashPath.Replace('/', '\\');
			Path = vp.Path + ":\\" + InternalPath;
			packagedHidePath = Path + ".hide";
			packagedFlagPath = Path + ".";
			SlashPath = Path.Replace('\\', '/');
			Name = Regex.Replace(SlashPath, ".*/", string.Empty);
			Exists = true;
			FullPath = vp.FullPath + ":\\" + InternalPath;
			FullSlashPath = FullPath.Replace('\\', '/');
			LastWriteTime = lastWriteTime;
			Size = size;
			Simulated = simulated;
			if (FileManager.debug)
			{
				Debug.Log("New var file entry\n Uid: " + Uid + "\n Path: " + Path + "\n FullPath: " + FullPath + "\n SlashPath: " + SlashPath + "\n Name: " + Name + "\n InternalSlashPath: " + InternalSlashPath);
			}
		}

		public override FileEntryStream OpenStream()
		{
			return new VarFileEntryStream(this);
		}

		public override FileEntryStreamReader OpenStreamReader()
		{
			return new VarFileEntryStreamReader(this);
		}

		public override bool HasFlagFile(string flagName)
		{
			return (flagBasePath != null && File.Exists(flagBasePath + flagName)) || (packagedFlagPath != null && FileManager.FileExists(packagedFlagPath + flagName));
		}

		public bool IsFlagFileModifiable(string flagName)
		{
			return packagedFlagPath == null || !FileManager.FileExists(packagedFlagPath + flagName);
		}

		public override bool IsHidden()
		{
			return (base.hidePath != null && File.Exists(base.hidePath)) || (packagedHidePath != null && FileManager.FileExists(packagedHidePath));
		}

		public bool IsHiddenModifiable()
		{
			return packagedHidePath == null || !FileManager.FileExists(packagedHidePath);
		}
	}
}
