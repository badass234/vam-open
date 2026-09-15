using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using MVR.Hub;
using SimpleJSON;
using UnityEngine;

namespace MVR.FileManagement
{
	public class VarPackage
	{
		public enum ReferenceVersionOption
		{
			Latest,
			Minimum,
			Exact
		}

		protected bool _enabled;

		protected bool _pluginsAlwaysEnabled;

		protected bool _pluginsAlwaysDisabled;

		protected bool _ignoreMissingDependencyErrors;

		protected Thread unpackThread;

		protected Thread repackThread;

		protected int packFileProgressCount;

		protected int packFileTotalCount;

		public bool packThreadAbort;

		public string packThreadError;

		protected Dictionary<string, bool> customOptions;

		protected Dictionary<string, VarDirectoryEntry> _DirectoryEntryLookup;

		public bool isNewestVersion;

		public bool isNewestEnabledVersion;

		protected string[] cacheFilePatterns = new string[2] { "*.vmi", "*.vam" };

		protected JSONClass jsonCache;

		protected VarFileEntry metaEntry;

		public bool invalid { get; protected set; }

		public bool forceRefresh { get; set; }

		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled == value)
				{
					return;
				}
				_enabled = value;
				string path = Path + ".disabled";
				if (FileManager.FileExists(path))
				{
					if (_enabled)
					{
						FileManager.DeleteFile(path);
						FileManager.Refresh();
					}
				}
				else if (!_enabled)
				{
					FileManager.WriteAllText(path, string.Empty);
					FileManager.Refresh();
				}
			}
		}

		public bool PluginsAlwaysEnabled
		{
			get
			{
				return _pluginsAlwaysEnabled;
			}
			set
			{
				if (_pluginsAlwaysEnabled != value)
				{
					_pluginsAlwaysEnabled = value;
					SaveUserPrefs();
				}
			}
		}

		public bool PluginsAlwaysDisabled
		{
			get
			{
				return _pluginsAlwaysDisabled;
			}
			set
			{
				if (_pluginsAlwaysDisabled != value)
				{
					_pluginsAlwaysDisabled = value;
					SaveUserPrefs();
				}
			}
		}

		public bool IgnoreMissingDependencyErrors
		{
			get
			{
				return _ignoreMissingDependencyErrors;
			}
			set
			{
				if (_ignoreMissingDependencyErrors != value)
				{
					_ignoreMissingDependencyErrors = value;
					SaveUserPrefs();
				}
			}
		}

		public float packProgress { get; protected set; }

		public bool IsUnpacking
		{
			get
			{
				if (unpackThread != null && unpackThread.IsAlive)
				{
					return true;
				}
				return false;
			}
		}

		public bool IsRepacking
		{
			get
			{
				if (repackThread != null && repackThread.IsAlive)
				{
					return true;
				}
				return false;
			}
		}

		public bool HasOriginalCopy
		{
			get
			{
				string path = Path + ".orig";
				return FileManager.FileExists(path);
			}
		}

		public bool IsSimulated { get; protected set; }

		public ZipFile ZipFile { get; protected set; }

		public string Uid { get; protected set; }

		public string UidLowerInvariant { get; protected set; }

		public string Path { get; protected set; }

		public string SlashPath { get; protected set; }

		public string FullPath { get; protected set; }

		public string FullSlashPath { get; protected set; }

		public VarPackageGroup Group { get; protected set; }

		public string GroupName { get; protected set; }

		public string Creator { get; protected set; }

		public string Name { get; protected set; }

		public int Version { get; protected set; }

		public ReferenceVersionOption StandardReferenceVersionOption { get; protected set; }

		public ReferenceVersionOption ScriptReferenceVersionOption { get; protected set; }

		public DateTime LastWriteTime { get; protected set; }

		public long Size { get; protected set; }

		public List<VarFileEntry> FileEntries { get; protected set; }

		public List<VarDirectoryEntry> DirectoryEntries { get; protected set; }

		public string LicenseType { get; protected set; }

		public string SecondaryLicenseType { get; protected set; }

		public string EAEndYear { get; protected set; }

		public string EAEndMonth { get; protected set; }

		public string EAEndDay { get; protected set; }

		public string Description { get; protected set; }

		public string Credits { get; protected set; }

		public string Instructions { get; protected set; }

		public string PromotionalLink { get; protected set; }

		public string ProgramVersion { get; protected set; }

		public List<string> Contents { get; protected set; }

		public List<string> PackageDependencies { get; protected set; }

		public bool HasMissingDependencies { get; protected set; }

		public HashSet<string> PackageDependenciesMissing { get; protected set; }

		public List<VarPackage> PackageDependenciesResolved { get; protected set; }

		public bool HadReferenceIssues { get; protected set; }

		public VarDirectoryEntry RootDirectory { get; protected set; }

		public bool IsOnHub
		{
			get
			{
				if (HubBrowse.singleton != null && !SuperController.singleton.hubDisabled)
				{
					string packageHubResourceId = HubBrowse.singleton.GetPackageHubResourceId(Uid);
					if (packageHubResourceId != null)
					{
						return true;
					}
				}
				return false;
			}
		}

		public VarPackage(string uid, string path, VarPackageGroup group, string creator, string name, int version)
		{
			Uid = uid;
			UidLowerInvariant = uid.ToLowerInvariant();
			Path = path.Replace('/', '\\');
			SlashPath = path.Replace('\\', '/');
			FullPath = System.IO.Path.GetFullPath(Path);
			FullSlashPath = FullPath.Replace('\\', '/');
			Name = name;
			Group = group;
			GroupName = group.Name;
			Creator = creator;
			Version = version;
			HadReferenceIssues = false;
			PackageDependencies = new List<string>();
			PackageDependenciesMissing = new HashSet<string>();
			PackageDependenciesResolved = new List<VarPackage>();
			if (FileManager.debug)
			{
				Debug.Log("New package\n Uid: " + Uid + "\n Path: " + Path + "\n FullPath: " + FullPath + "\n SlashPath: " + SlashPath + "\n Name: " + Name + "\n GroupName: " + GroupName + "\n Creator: " + Creator + "\n Version: " + Version);
			}
			Scan();
		}

		protected void SyncEnabled()
		{
			_enabled = !FileManager.FileExists(Path + ".disabled");
		}

		public void Delete()
		{
			if (ZipFile != null)
			{
				ZipFile.Close();
				ZipFile = null;
			}
			if (File.Exists(Path))
			{
				FileManager.DeleteFile(Path);
			}
			else if (Directory.Exists(Path))
			{
				FileManager.DeleteDirectory(Path, true);
			}
			string path = Path + ".disabled";
			if (File.Exists(path))
			{
				FileManager.DeleteFile(path);
			}
			FileManager.Refresh();
		}

		protected void LoadUserPrefs()
		{
			string path = FileManager.UserPrefsFolder + "/" + Uid + ".prefs";
			if (FileManager.FileExists(path))
			{
				using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(path))
				{
					string aJSON = fileEntryStreamReader.ReadToEnd();
					JSONClass asObject = JSON.Parse(aJSON).AsObject;
					if (asObject != null)
					{
						_pluginsAlwaysEnabled = asObject["pluginsAlwaysEnabled"].AsBool;
						_pluginsAlwaysDisabled = asObject["pluginsAlwaysDisabled"].AsBool;
						_ignoreMissingDependencyErrors = asObject["ignoreMissingDependencyErrors"].AsBool;
					}
					return;
				}
			}
			_pluginsAlwaysEnabled = false;
			_pluginsAlwaysDisabled = false;
			_ignoreMissingDependencyErrors = false;
		}

		protected void SaveUserPrefs()
		{
			string text = FileManager.UserPrefsFolder + "/" + Uid + ".prefs";
			JSONClass jSONClass = new JSONClass();
			jSONClass["pluginsAlwaysEnabled"].AsBool = _pluginsAlwaysEnabled;
			jSONClass["pluginsAlwaysDisabled"].AsBool = _pluginsAlwaysDisabled;
			jSONClass["ignoreMissingDependencyErrors"].AsBool = _ignoreMissingDependencyErrors;
			string text2 = jSONClass.ToString(string.Empty);
			try
			{
				FileManager.WriteAllText(text, text2);
			}
			catch (Exception ex)
			{
				SuperController.LogError("Error during save of prefs file " + text + ": " + ex.Message);
			}
		}

		protected void ProcessFileMethod(object sender, ScanEventArgs args)
		{
			packFileProgressCount++;
			if (packFileTotalCount != 0)
			{
				packProgress = (float)packFileProgressCount / (float)packFileTotalCount;
			}
			if (packThreadAbort)
			{
				args.ContinueRunning = false;
			}
		}

		protected void UnpackThreaded()
		{
			try
			{
				string text = Path + ".orig";
				if (!FileManager.FileExists(text))
				{
					FileManager.CopyFile(Path, text);
				}
				FastZipEvents fastZipEvents = new FastZipEvents();
				fastZipEvents.ProcessFile = ProcessFileMethod;
				FastZip fastZip = new FastZip(fastZipEvents);
				string text2 = Path + ".extracted";
				if (FileManager.DirectoryExists(text2))
				{
					FileManager.DeleteDirectory(text2, true);
				}
				fastZip.ExtractZip(Path, text2, FastZip.Overwrite.Always, null, string.Empty, string.Empty, true);
				if (ZipFile != null)
				{
					ZipFile.Close();
					ZipFile = null;
				}
				FileManager.DeleteFile(Path);
				FileManager.MoveDirectory(text2, Path);
			}
			catch (Exception ex)
			{
				packThreadError = ex.Message;
			}
		}

		public void Unpack()
		{
			FileManager.AssertNotCalledFromPlugin();
			if (!IsSimulated && (unpackThread == null || !unpackThread.IsAlive))
			{
				packThreadError = null;
				packThreadAbort = false;
				packProgress = 0f;
				packFileProgressCount = 0;
				packFileTotalCount = FileEntries.Count;
				unpackThread = new Thread(UnpackThreaded);
				unpackThread.Start();
			}
		}

		protected void RepackThreaded()
		{
			try
			{
				string text = SlashPath + ".zip";
				if (FileManager.FileExists(text))
				{
					FileManager.DeleteFile(text);
				}
				using (ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(text)))
				{
					string[] files = Directory.GetFiles(SlashPath, "*", SearchOption.AllDirectories);
					byte[] buffer = new byte[32768];
					string[] array = files;
					foreach (string text2 in array)
					{
						string input = Regex.Replace(text2, "\\\\", "/");
						input = Regex.Replace(input, "^" + Regex.Escape(SlashPath) + "/", string.Empty);
						ZipEntry zipEntry = new ZipEntry(input);
						zipEntry.IsUnicodeText = true;
						string extension = System.IO.Path.GetExtension(text2);
						if (Regex.IsMatch(extension, "(mp3|ogg|wav|jpg|jpeg|png|gif|tif|tiff|assetbundle|scene|vac|zip)", RegexOptions.IgnoreCase))
						{
							zipOutputStream.SetLevel(0);
						}
						else
						{
							zipOutputStream.SetLevel(9);
						}
						zipOutputStream.PutNextEntry(zipEntry);
						using (FileEntryStream fileEntryStream = FileManager.OpenStream(text2))
						{
							StreamUtils.Copy(fileEntryStream.Stream, zipOutputStream, buffer);
						}
						zipEntry.DateTime = File.GetLastWriteTime(text2);
						zipOutputStream.CloseEntry();
						packFileProgressCount++;
						if (packFileTotalCount != 0)
						{
							packProgress = (float)packFileProgressCount / (float)packFileTotalCount;
						}
						if (packThreadAbort)
						{
							return;
						}
					}
				}
				string text3 = Path + ".todelete";
				try
				{
					FileManager.MoveDirectory(Path, text3);
				}
				catch (Exception)
				{
					packThreadError = "Error during attempt of move and delete of " + Path + ". Do you have this folder open in explorer or files in this folder open in another tool?";
					return;
				}
				FileManager.MoveFile(text, Path);
				FileStream file = File.Open(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
				ZipFile zipFile = new ZipFile(file);
				zipFile.IsStreamOwner = true;
				ZipFile = zipFile;
				FileManager.DeleteDirectory(text3, true);
			}
			catch (Exception ex2)
			{
				packThreadError = ex2.Message;
			}
		}

		public void Repack()
		{
			FileManager.AssertNotCalledFromPlugin();
			if (IsSimulated && (repackThread == null || !repackThread.IsAlive))
			{
				packThreadError = null;
				packThreadAbort = false;
				packProgress = 0f;
				packFileProgressCount = 0;
				packFileTotalCount = FileManager.FolderContentsCount(Path);
				repackThread = new Thread(RepackThreaded);
				repackThread.Start();
			}
		}

		public void RestoreFromOriginal()
		{
			FileManager.AssertNotCalledFromPlugin();
			string text = Path + ".orig";
			if (!FileManager.FileExists(text))
			{
				return;
			}
			if (FileManager.DirectoryExists(Path))
			{
				FileManager.DeleteDirectory(Path, true);
			}
			else if (FileManager.FileExists(Path))
			{
				if (ZipFile != null)
				{
					ZipFile.Close();
					ZipFile = null;
				}
				FileManager.DeleteFile(Path);
			}
			FileManager.MoveFile(text, Path);
		}

		public List<string> GetCustomOptionNames()
		{
			if (customOptions != null)
			{
				return customOptions.Keys.ToList();
			}
			return new List<string>();
		}

		public bool GetCustomOption(string optionName)
		{
			bool value = false;
			if (customOptions != null)
			{
				customOptions.TryGetValue(optionName, out value);
			}
			return value;
		}

		public VarDirectoryEntry GetDirectoryEntry(string path)
		{
			VarDirectoryEntry value = null;
			if (_DirectoryEntryLookup != null)
			{
				_DirectoryEntryLookup.TryGetValue(path, out value);
			}
			return value;
		}

		public bool HasMatchingDirectories(string dir)
		{
			string input = dir.Replace('\\', '/');
			input = Regex.Replace(input, "/$", string.Empty);
			foreach (VarDirectoryEntry directoryEntry in DirectoryEntries)
			{
				if (directoryEntry.InternalSlashPath == input)
				{
					return true;
				}
			}
			return false;
		}

		public List<VarDirectoryEntry> FindVarDirectories(string dir, bool exactMatch = true)
		{
			string input = dir.Replace('\\', '/');
			input = Regex.Replace(input, "/$", string.Empty);
			List<VarDirectoryEntry> list = new List<VarDirectoryEntry>();
			foreach (VarDirectoryEntry directoryEntry in DirectoryEntries)
			{
				if (exactMatch)
				{
					if (directoryEntry.InternalSlashPath == input)
					{
						list.Add(directoryEntry);
					}
				}
				else if (directoryEntry.InternalSlashPath.StartsWith(dir))
				{
					list.Add(directoryEntry);
				}
			}
			return list;
		}

		public bool HasMatchingFiles(string dir, string pattern)
		{
			if (HasMatchingDirectories(dir))
			{
				string pattern2 = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
				foreach (VarFileEntry fileEntry in FileEntries)
				{
					if (!fileEntry.InternalSlashPath.StartsWith(dir) || !Regex.IsMatch(fileEntry.Name, pattern2))
					{
						continue;
					}
					return true;
				}
			}
			return false;
		}

		public void FindFiles(string dir, string pattern, List<FileEntry> foundFiles)
		{
			string pattern2 = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
			foreach (VarFileEntry fileEntry in FileEntries)
			{
				if (fileEntry.InternalSlashPath.StartsWith(dir) && Regex.IsMatch(fileEntry.Name, pattern2))
				{
					foundFiles.Add(fileEntry);
				}
			}
		}

		public void OpenOnHub()
		{
			if (HubBrowse.singleton != null && !SuperController.singleton.hubDisabled)
			{
				string packageHubResourceId = HubBrowse.singleton.GetPackageHubResourceId(Uid);
				if (packageHubResourceId != null)
				{
					HubBrowse.singleton.OpenDetail(packageHubResourceId);
				}
			}
		}

		public void Dispose()
		{
			if (ZipFile != null)
			{
				ZipFile.Close();
				ZipFile = null;
			}
		}

		public JSONNode GetJSONCache(string filePath)
		{
			JSONNode result = null;
			if (jsonCache != null)
			{
				result = jsonCache[filePath];
			}
			return result;
		}

		protected void AddDirToCache(VarDirectoryEntry de, string pattern, JSONClass cache)
		{
			List<VarDirectoryEntry> varSubDirectories = de.VarSubDirectories;
			foreach (VarDirectoryEntry item in varSubDirectories)
			{
				AddDirToCache(item, pattern, cache);
			}
			List<FileEntry> files = de.GetFiles(pattern);
			foreach (FileEntry item2 in files)
			{
				VarFileEntry varFileEntry = item2 as VarFileEntry;
				if (varFileEntry != null)
				{
					string aJSON = FileManager.ReadAllText(item2);
					JSONNode jSONNode = JSON.Parse(aJSON);
					if (jSONNode != null)
					{
						cache[varFileEntry.InternalSlashPath] = jSONNode;
					}
				}
			}
		}

		public void SyncJSONCache()
		{
			if (CacheManager.CachingEnabled)
			{
				if (IsSimulated)
				{
					return;
				}
				try
				{
					bool flag = false;
					string text = CacheManager.GetPackageJSONCacheDir() + "/";
					string text2 = text + Uid + ".vamcachemeta";
					string path = text + Uid + ".vamcachejson";
					if (File.Exists(text2))
					{
						string text3 = FileManager.ReadAllText(text2);
						if (text3 != null)
						{
							try
							{
								JSONClass asObject = JSON.Parse(text3).AsObject;
								if (asObject != null)
								{
									string text4 = asObject["size"];
									long result;
									if (text4 != null && text4 != string.Empty && long.TryParse(text4, out result) && result == Size)
									{
										flag = true;
									}
								}
								else
								{
									Debug.LogError("Invalid cache meta file " + text2);
								}
							}
							catch (Exception ex)
							{
								SuperController.LogError("Exception during parse of package json cache (file will be regenerated to try to fix): " + ex);
							}
						}
					}
					if (!flag || !File.Exists(path))
					{
						JSONClass jSONClass = new JSONClass();
						jSONClass["type"] = "json";
						jSONClass["size"] = Size.ToString();
						jSONClass["date"] = LastWriteTime.ToString();
						File.WriteAllText(text2, jSONClass.ToString(string.Empty));
						VarDirectoryEntry rootDirectory = RootDirectory;
						JSONClass jSONClass2 = new JSONClass();
						if (rootDirectory != null)
						{
							string[] array = cacheFilePatterns;
							foreach (string pattern in array)
							{
								AddDirToCache(rootDirectory, pattern, jSONClass2);
							}
						}
						File.WriteAllText(path, jSONClass2.ToString(string.Empty));
						jsonCache = jSONClass2;
					}
					else if (jsonCache == null)
					{
						string aJSON = FileManager.ReadAllText(path);
						JSONClass asObject2 = JSON.Parse(aJSON).AsObject;
						jsonCache = asObject2;
					}
					return;
				}
				catch (Exception ex2)
				{
					SuperController.LogError("Exception during sync of package json cache: " + ex2);
					jsonCache = null;
					return;
				}
			}
			jsonCache = null;
		}

		protected void CreateDirectoryEntries(VarFileEntry varFileEntry)
		{
			string internalSlashPath = varFileEntry.InternalSlashPath;
			string[] array = internalSlashPath.Split('/');
			VarDirectoryEntry varDirectoryEntry = RootDirectory;
			string text = string.Empty;
			for (int i = 0; i < array.Length; i++)
			{
				if (i == array.Length - 1)
				{
					varDirectoryEntry.AddFileEntry(varFileEntry);
					continue;
				}
				text = ((!(text == string.Empty)) ? (text + "/" + array[i]) : (text + array[i]));
				VarDirectoryEntry value;
				if (!_DirectoryEntryLookup.TryGetValue(text, out value))
				{
					value = new VarDirectoryEntry(this, text, varDirectoryEntry);
					varDirectoryEntry.AddSubDirectory(value);
					DirectoryEntries.Add(value);
					_DirectoryEntryLookup.Add(text, value);
				}
				varDirectoryEntry = value;
			}
		}

		protected void Scan()
		{
			FileEntries = new List<VarFileEntry>();
			DirectoryEntries = new List<VarDirectoryEntry>();
			_DirectoryEntryLookup = new Dictionary<string, VarDirectoryEntry>();
			_DirectoryEntryLookup.Add(string.Empty, RootDirectory);
			SyncEnabled();
			bool flag = false;
			if (File.Exists(Path))
			{
				IsSimulated = false;
				float elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
				ZipFile zipFile = null;
				try
				{
					FileInfo fileInfo = new FileInfo(Path);
					LastWriteTime = fileInfo.LastWriteTime;
					Size = fileInfo.Length;
					RootDirectory = new VarDirectoryEntry(this, string.Empty);
					DirectoryEntries.Add(RootDirectory);
					FileStream file = File.Open(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
					zipFile = new ZipFile(file);
					zipFile.IsStreamOwner = true;
					metaEntry = null;
					foreach (ZipEntry item in zipFile)
					{
						if (item.Name.Contains("../") || item.Name.Contains("..\\"))
						{
							metaEntry = null;
							throw new Exception("Potentially malicious var file that contains .. paths detected");
						}
						if (item.IsFile)
						{
							VarFileEntry varFileEntry = new VarFileEntry(this, item.Name, item.DateTime, item.Size);
							FileEntries.Add(varFileEntry);
							CreateDirectoryEntries(varFileEntry);
							if (item.Name == "meta.json")
							{
								metaEntry = varFileEntry;
							}
						}
					}
					if (metaEntry != null)
					{
						flag = true;
					}
					ZipFile = zipFile;
				}
				catch (Exception ex)
				{
					zipFile?.Close();
					SuperController.LogError("Exception during zip file scan of " + Path + ": " + ex);
				}
				float elapsedMilliseconds2 = GlobalStopwatch.GetElapsedMilliseconds();
				float num = elapsedMilliseconds2 - elapsedMilliseconds;
				if (FileManager.debug)
				{
					Debug.Log("Scanned var package " + Path + " in " + num.ToString("F1") + " ms");
				}
			}
			else if (Directory.Exists(Path))
			{
				IsSimulated = true;
				float elapsedMilliseconds3 = GlobalStopwatch.GetElapsedMilliseconds();
				try
				{
					LastWriteTime = Directory.GetLastWriteTime(Path);
					Size = 0L;
					RootDirectory = new VarDirectoryEntry(this, string.Empty);
					DirectoryEntries.Add(RootDirectory);
					metaEntry = null;
					string[] files = Directory.GetFiles(Path, "*", SearchOption.AllDirectories);
					string[] array = files;
					foreach (string text in array)
					{
						string text2 = text.Replace(Path + "\\", string.Empty);
						text2 = text2.Replace('\\', '/');
						FileInfo fileInfo2 = new FileInfo(text);
						VarFileEntry varFileEntry2 = new VarFileEntry(this, text2, fileInfo2.LastWriteTime, fileInfo2.Length, true);
						FileEntries.Add(varFileEntry2);
						CreateDirectoryEntries(varFileEntry2);
						if (text2 == "meta.json")
						{
							metaEntry = varFileEntry2;
						}
					}
					if (metaEntry != null)
					{
						flag = true;
					}
				}
				catch (Exception ex2)
				{
					Debug.LogError("Exception during var directory scan of " + Path + ": " + ex2);
				}
				float elapsedMilliseconds4 = GlobalStopwatch.GetElapsedMilliseconds();
				float num2 = elapsedMilliseconds4 - elapsedMilliseconds3;
				if (FileManager.debug)
				{
					Debug.Log("Scanned var package " + Path + " in " + num2.ToString("F1") + " ms");
				}
			}
			if (!flag)
			{
				invalid = true;
			}
			else
			{
				SyncJSONCache();
			}
		}

		protected void FindMissingDependenciesRecursive(JSONClass jc)
		{
			JSONClass asObject = jc["dependencies"].AsObject;
			if (!(asObject != null))
			{
				return;
			}
			foreach (string key in asObject.Keys)
			{
				VarPackage package = FileManager.GetPackage(key);
				if (package == null)
				{
					HasMissingDependencies = true;
					PackageDependenciesMissing.Add(key);
				}
				JSONClass asObject2 = asObject[key].AsObject;
				if (asObject2 != null)
				{
					FindMissingDependenciesRecursive(asObject2);
				}
			}
		}

		public void LoadMetaData()
		{
			bool flag = false;
			List<string> list = new List<string>();
			if (metaEntry != null)
			{
				try
				{
					using (VarFileEntryStreamReader varFileEntryStreamReader = new VarFileEntryStreamReader(metaEntry))
					{
						string aJSON = varFileEntryStreamReader.ReadToEnd();
						JSONClass asObject = JSON.Parse(aJSON).AsObject;
						if (asObject != null)
						{
							if (asObject["licenseType"] != null)
							{
								LicenseType = asObject["licenseType"];
							}
							else
							{
								LicenseType = "MISSING";
							}
							SecondaryLicenseType = asObject["secondaryLicenseType"];
							EAEndYear = asObject["EAEndYear"];
							EAEndMonth = asObject["EAEndMonth"];
							EAEndDay = asObject["EAEndDay"];
							if (asObject["standardReferenceVersionOption"] != null)
							{
								try
								{
									string value = asObject["standardReferenceVersionOption"];
									ReferenceVersionOption standardReferenceVersionOption = (ReferenceVersionOption)Enum.Parse(typeof(ReferenceVersionOption), value);
									StandardReferenceVersionOption = standardReferenceVersionOption;
								}
								catch (ArgumentException)
								{
									StandardReferenceVersionOption = ReferenceVersionOption.Latest;
								}
							}
							else
							{
								StandardReferenceVersionOption = ReferenceVersionOption.Latest;
							}
							if (asObject["scriptReferenceVersionOption"] != null)
							{
								try
								{
									string value2 = asObject["scriptReferenceVersionOption"];
									ReferenceVersionOption scriptReferenceVersionOption = (ReferenceVersionOption)Enum.Parse(typeof(ReferenceVersionOption), value2);
									ScriptReferenceVersionOption = scriptReferenceVersionOption;
								}
								catch (ArgumentException)
								{
									ScriptReferenceVersionOption = ReferenceVersionOption.Exact;
								}
							}
							else
							{
								ScriptReferenceVersionOption = ReferenceVersionOption.Exact;
							}
							Description = asObject["description"];
							Credits = asObject["credits"];
							Instructions = asObject["instructions"];
							if (asObject["promotionalLink"] != null)
							{
								PromotionalLink = asObject["promotionalLink"];
							}
							else
							{
								PromotionalLink = asObject["patreonLink"];
							}
							List<string> list2 = new List<string>();
							JSONArray asArray = asObject["contentList"].AsArray;
							if (asArray != null)
							{
								foreach (JSONNode item in asArray)
								{
									string text = item;
									if (text != null)
									{
										list2.Add(text);
									}
								}
							}
							Contents = list2;
							PackageDependencies = new List<string>();
							PackageDependenciesMissing = new HashSet<string>();
							HasMissingDependencies = false;
							PackageDependenciesResolved = new List<VarPackage>();
							JSONClass asObject2 = asObject["dependencies"].AsObject;
							if (asObject2 != null)
							{
								foreach (string key in asObject2.Keys)
								{
									VarPackage package = FileManager.GetPackage(key);
									if (package == null)
									{
										string packageGroupUid = Regex.Replace(key, "\\.[0-9]+$", string.Empty);
										VarPackageGroup packageGroup = FileManager.GetPackageGroup(packageGroupUid);
										if (packageGroup != null)
										{
											VarPackage newestPackage = packageGroup.NewestPackage;
											PackageDependenciesResolved.Add(newestPackage);
										}
										else if (Enabled)
										{
											list.Add("Missing addon package " + key + " that package" + Uid + " depends on");
										}
									}
									else
									{
										PackageDependenciesResolved.Add(package);
									}
									PackageDependencies.Add(key);
								}
							}
							if (Enabled)
							{
								FindMissingDependenciesRecursive(asObject);
							}
							JSONClass asObject3 = asObject["customOptions"].AsObject;
							customOptions = new Dictionary<string, bool>();
							if (asObject3 != null)
							{
								foreach (string key2 in asObject3.Keys)
								{
									if (!customOptions.ContainsKey(key2))
									{
										customOptions.Add(key2, asObject3[key2].AsBool);
									}
								}
							}
							if (asObject["hadReferenceIssues"] != null)
							{
								HadReferenceIssues = asObject["hadReferenceIssues"].AsBool;
							}
							else
							{
								HadReferenceIssues = false;
							}
							flag = true;
						}
					}
				}
				catch (Exception ex3)
				{
					SuperController.LogError("Exception during process of meta.json from package " + Uid + ": " + ex3);
				}
			}
			if (flag)
			{
				LoadUserPrefs();
				if (IgnoreMissingDependencyErrors)
				{
					return;
				}
				{
					foreach (string item2 in list)
					{
						SuperController.LogError(item2);
					}
					return;
				}
			}
			invalid = true;
		}
	}
}
