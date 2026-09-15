using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using ICSharpCode.SharpZipLib.Core;
using MVR.FileManagementSecure;
using UnityEngine;

namespace MVR.FileManagement
{
	public class FileManager : MonoBehaviour
	{
		public class PackageUIDAndPublisher
		{
			public string uid;

			public string publisher;
		}

		public static bool debug;

		public static FileManager singleton;

		protected static Dictionary<string, VarPackage> packagesByUid;

		protected static HashSet<VarPackage> enabledPackages;

		protected static Dictionary<string, VarPackage> packagesByPath;

		protected static Dictionary<string, VarPackageGroup> packageGroups;

		protected static HashSet<VarFileEntry> allVarFileEntries;

		protected static HashSet<VarDirectoryEntry> allVarDirectoryEntries;

		protected static Dictionary<string, VarFileEntry> uidToVarFileEntry;

		protected static Dictionary<string, VarFileEntry> pathToVarFileEntry;

		protected static Dictionary<string, VarDirectoryEntry> uidToVarDirectoryEntry;

		protected static Dictionary<string, VarDirectoryEntry> pathToVarDirectoryEntry;

		protected static Dictionary<string, VarDirectoryEntry> varPackagePathToRootVarDirectory;

		protected static string packageFolder = "AddonPackages";

		protected static string userPrefsFolder = "AddonPackagesUserPrefs";

		protected static OnRefresh onRefreshHandlers;

		public static string[] demoPackagePrefixes;

		public static bool packagesEnabled = true;

		protected static HashSet<string> restrictedReadPaths;

		protected static HashSet<string> secureReadPaths;

		protected static HashSet<string> secureInternalWritePaths;

		protected static HashSet<string> securePluginWritePaths;

		protected static HashSet<string> pluginWritePathsThatDoNotNeedConfirm;

		public Transform userConfirmContainer;

		public Transform userConfirmPrefab;

		public Transform userConfirmPluginActionPrefab;

		protected static Dictionary<string, string> pluginHashToPluginPath;

		protected static Dictionary<Assembly, string> assemblyToPluginHash;

		protected AsyncFlag userConfirmFlag;

		protected HashSet<UserConfirmPanel> activeUserConfirmPanels;

		protected static HashSet<string> userConfirmedPlugins;

		protected static HashSet<string> userDeniedPlugins;

		protected static LinkedList<string> loadDirStack;

		public static string PackageFolder => packageFolder;

		public static string UserPrefsFolder => userPrefsFolder;

		public static DateTime lastPackageRefreshTime { get; protected set; }

		public static string CurrentLoadDir
		{
			get
			{
				if (loadDirStack != null && loadDirStack.Count > 0)
				{
					return loadDirStack.Last.Value;
				}
				return null;
			}
		}

		public static string CurrentPackageUid
		{
			get
			{
				string currentLoadDir = CurrentLoadDir;
				if (currentLoadDir != null)
				{
					VarDirectoryEntry varDirectoryEntry = GetVarDirectoryEntry(currentLoadDir);
					if (varDirectoryEntry != null)
					{
						return varDirectoryEntry.Package.Uid;
					}
				}
				return null;
			}
		}

		public static string TopLoadDir
		{
			get
			{
				if (loadDirStack != null && loadDirStack.Count > 0)
				{
					return loadDirStack.First.Value;
				}
				return null;
			}
		}

		public static string TopPackageUid
		{
			get
			{
				string topLoadDir = TopLoadDir;
				if (topLoadDir != null)
				{
					VarDirectoryEntry varDirectoryEntry = GetVarDirectoryEntry(topLoadDir);
					if (varDirectoryEntry != null)
					{
						return varDirectoryEntry.Package.Uid;
					}
				}
				return null;
			}
		}

		public static string CurrentSaveDir { get; protected set; }

		protected static string packagePathToUid(string vpath)
		{
			string input = vpath.Replace('\\', '/');
			input = Regex.Replace(input, "\\.(var|zip)$", string.Empty);
			return Regex.Replace(input, ".*/", string.Empty);
		}

		protected static VarPackage RegisterPackage(string vpath)
		{
			if (debug)
			{
				UnityEngine.Debug.Log("RegisterPackage " + vpath);
			}
			string text = packagePathToUid(vpath);
			string[] array = text.Split('.');
			if (array.Length == 3)
			{
				string text2 = array[0];
				string text3 = array[1];
				string key = text2 + "." + text3;
				string s = array[2];
				try
				{
					int version = int.Parse(s);
					if (!packagesByUid.ContainsKey(text))
					{
						VarPackageGroup value;
						if (!packageGroups.TryGetValue(key, out value))
						{
							value = new VarPackageGroup(key);
							packageGroups.Add(key, value);
						}
						VarPackage varPackage = new VarPackage(text, vpath, value, text2, text3, version);
						packagesByUid.Add(text, varPackage);
						packagesByPath.Add(varPackage.Path, varPackage);
						packagesByPath.Add(varPackage.SlashPath, varPackage);
						packagesByPath.Add(varPackage.FullPath, varPackage);
						packagesByPath.Add(varPackage.FullSlashPath, varPackage);
						value.AddPackage(varPackage);
						if (varPackage.Enabled)
						{
							enabledPackages.Add(varPackage);
							foreach (VarFileEntry fileEntry in varPackage.FileEntries)
							{
								allVarFileEntries.Add(fileEntry);
								uidToVarFileEntry.Add(fileEntry.Uid, fileEntry);
								if (debug)
								{
									UnityEngine.Debug.Log("Add var file with UID " + fileEntry.Uid);
								}
								pathToVarFileEntry.Add(fileEntry.Path, fileEntry);
								pathToVarFileEntry.Add(fileEntry.SlashPath, fileEntry);
								pathToVarFileEntry.Add(fileEntry.FullPath, fileEntry);
								pathToVarFileEntry.Add(fileEntry.FullSlashPath, fileEntry);
							}
							foreach (VarDirectoryEntry directoryEntry in varPackage.DirectoryEntries)
							{
								allVarDirectoryEntries.Add(directoryEntry);
								if (debug)
								{
									UnityEngine.Debug.Log("Add var directory with UID " + directoryEntry.Uid);
								}
								uidToVarDirectoryEntry.Add(directoryEntry.Uid, directoryEntry);
								pathToVarDirectoryEntry.Add(directoryEntry.Path, directoryEntry);
								pathToVarDirectoryEntry.Add(directoryEntry.SlashPath, directoryEntry);
								pathToVarDirectoryEntry.Add(directoryEntry.FullPath, directoryEntry);
								pathToVarDirectoryEntry.Add(directoryEntry.FullSlashPath, directoryEntry);
							}
							varPackagePathToRootVarDirectory.Add(varPackage.Path, varPackage.RootDirectory);
							varPackagePathToRootVarDirectory.Add(varPackage.FullPath, varPackage.RootDirectory);
						}
						return varPackage;
					}
					SuperController.LogError("Duplicate package uid " + text + ". Cannot register");
				}
				catch (FormatException)
				{
					SuperController.LogError("VAR file " + vpath + " does not use integer version field in name <creator>.<name>.<version>");
				}
			}
			else
			{
				SuperController.LogError("VAR file " + vpath + " is not named with convention <creator>.<name>.<version>");
			}
			return null;
		}

		public static void UnregisterPackage(VarPackage vp)
		{
			if (vp == null)
			{
				return;
			}
			if (vp.Group != null)
			{
				vp.Group.RemovePackage(vp);
			}
			packagesByUid.Remove(vp.Uid);
			packagesByPath.Remove(vp.Path);
			packagesByPath.Remove(vp.SlashPath);
			packagesByPath.Remove(vp.FullPath);
			packagesByPath.Remove(vp.FullSlashPath);
			enabledPackages.Remove(vp);
			foreach (VarFileEntry fileEntry in vp.FileEntries)
			{
				allVarFileEntries.Remove(fileEntry);
				uidToVarFileEntry.Remove(fileEntry.Uid);
				pathToVarFileEntry.Remove(fileEntry.Path);
				pathToVarFileEntry.Remove(fileEntry.SlashPath);
				pathToVarFileEntry.Remove(fileEntry.FullPath);
				pathToVarFileEntry.Remove(fileEntry.FullSlashPath);
			}
			foreach (VarDirectoryEntry directoryEntry in vp.DirectoryEntries)
			{
				allVarDirectoryEntries.Remove(directoryEntry);
				uidToVarDirectoryEntry.Remove(directoryEntry.Uid);
				pathToVarDirectoryEntry.Remove(directoryEntry.Path);
				pathToVarDirectoryEntry.Remove(directoryEntry.SlashPath);
				pathToVarDirectoryEntry.Remove(directoryEntry.FullPath);
				pathToVarDirectoryEntry.Remove(directoryEntry.FullSlashPath);
			}
			varPackagePathToRootVarDirectory.Remove(vp.Path);
			varPackagePathToRootVarDirectory.Remove(vp.FullPath);
			vp.Dispose();
		}

		public static void SyncJSONCache()
		{
			foreach (VarPackage package in GetPackages())
			{
				package.SyncJSONCache();
			}
		}

		public static void RegisterRefreshHandler(OnRefresh refreshHandler)
		{
			onRefreshHandlers = (OnRefresh)Delegate.Combine(onRefreshHandlers, refreshHandler);
		}

		public static void UnregisterRefreshHandler(OnRefresh refreshHandler)
		{
			onRefreshHandlers = (OnRefresh)Delegate.Remove(onRefreshHandlers, refreshHandler);
		}

		protected static void ClearAll()
		{
			foreach (VarPackage value in packagesByUid.Values)
			{
				value.Dispose();
			}
			if (packagesByUid != null)
			{
				packagesByUid.Clear();
			}
			if (packagesByPath != null)
			{
				packagesByPath.Clear();
			}
			if (packageGroups != null)
			{
				packageGroups.Clear();
			}
			if (enabledPackages != null)
			{
				enabledPackages.Clear();
			}
			if (allVarFileEntries != null)
			{
				allVarFileEntries.Clear();
			}
			if (allVarDirectoryEntries != null)
			{
				allVarDirectoryEntries.Clear();
			}
			if (uidToVarFileEntry != null)
			{
				uidToVarFileEntry.Clear();
			}
			if (pathToVarFileEntry != null)
			{
				pathToVarFileEntry.Clear();
			}
			if (uidToVarDirectoryEntry != null)
			{
				uidToVarDirectoryEntry.Clear();
			}
			if (pathToVarDirectoryEntry != null)
			{
				pathToVarDirectoryEntry.Clear();
			}
			if (varPackagePathToRootVarDirectory != null)
			{
				varPackagePathToRootVarDirectory.Clear();
			}
		}

		public static void Refresh()
		{
			if (debug)
			{
				UnityEngine.Debug.Log("FileManager Refresh()");
			}
			if (packagesByUid == null)
			{
				packagesByUid = new Dictionary<string, VarPackage>();
			}
			if (packagesByPath == null)
			{
				packagesByPath = new Dictionary<string, VarPackage>();
			}
			if (packageGroups == null)
			{
				packageGroups = new Dictionary<string, VarPackageGroup>();
			}
			if (enabledPackages == null)
			{
				enabledPackages = new HashSet<VarPackage>();
			}
			if (allVarFileEntries == null)
			{
				allVarFileEntries = new HashSet<VarFileEntry>();
			}
			if (allVarDirectoryEntries == null)
			{
				allVarDirectoryEntries = new HashSet<VarDirectoryEntry>();
			}
			if (uidToVarFileEntry == null)
			{
				uidToVarFileEntry = new Dictionary<string, VarFileEntry>();
			}
			if (pathToVarFileEntry == null)
			{
				pathToVarFileEntry = new Dictionary<string, VarFileEntry>();
			}
			if (uidToVarDirectoryEntry == null)
			{
				uidToVarDirectoryEntry = new Dictionary<string, VarDirectoryEntry>();
			}
			if (pathToVarDirectoryEntry == null)
			{
				pathToVarDirectoryEntry = new Dictionary<string, VarDirectoryEntry>();
			}
			if (varPackagePathToRootVarDirectory == null)
			{
				varPackagePathToRootVarDirectory = new Dictionary<string, VarDirectoryEntry>();
			}
			bool flag = false;
			float num = GlobalStopwatch.GetElapsedMilliseconds();
			float elapsedMilliseconds;
			try
			{
				if (!Directory.Exists(packageFolder))
				{
					CreateDirectory(packageFolder);
				}
				if (!Directory.Exists(userPrefsFolder))
				{
					CreateDirectory(userPrefsFolder);
				}
				if (Directory.Exists(packageFolder))
				{
					string[] array = null;
					string[] array2 = null;
					if (packagesEnabled)
					{
						array = Directory.GetDirectories(packageFolder, "*.var", SearchOption.AllDirectories);
						array2 = Directory.GetFiles(packageFolder, "*.var", SearchOption.AllDirectories);
					}
					else if (demoPackagePrefixes != null)
					{
						List<string> list = new List<string>();
						string[] array3 = demoPackagePrefixes;
						foreach (string text in array3)
						{
							string[] files = Directory.GetFiles(packageFolder, text + "*.var", SearchOption.AllDirectories);
							foreach (string item in files)
							{
								list.Add(item);
							}
						}
						array2 = list.ToArray();
					}
					HashSet<string> hashSet = new HashSet<string>();
					HashSet<string> hashSet2 = new HashSet<string>();
					if (array != null)
					{
						string[] array4 = array;
						foreach (string text2 in array4)
						{
							hashSet.Add(text2);
							VarPackage value;
							if (packagesByPath.TryGetValue(text2, out value))
							{
								bool flag2 = enabledPackages.Contains(value);
								bool flag3 = value.Enabled;
								if ((!flag2 && flag3) || (flag2 && !flag3) || !value.IsSimulated)
								{
									UnregisterPackage(value);
									hashSet2.Add(text2);
								}
							}
							else
							{
								hashSet2.Add(text2);
							}
						}
					}
					if (array2 != null)
					{
						string[] array5 = array2;
						foreach (string text3 in array5)
						{
							hashSet.Add(text3);
							VarPackage value2;
							if (packagesByPath.TryGetValue(text3, out value2))
							{
								bool flag4 = enabledPackages.Contains(value2);
								bool flag5 = value2.Enabled;
								if ((!flag4 && flag5) || (flag4 && !flag5) || value2.IsSimulated)
								{
									UnregisterPackage(value2);
									hashSet2.Add(text3);
								}
							}
							else
							{
								hashSet2.Add(text3);
							}
						}
					}
					HashSet<VarPackage> hashSet3 = new HashSet<VarPackage>();
					foreach (VarPackage value3 in packagesByUid.Values)
					{
						if (!hashSet.Contains(value3.Path))
						{
							hashSet3.Add(value3);
						}
					}
					foreach (VarPackage item2 in hashSet3)
					{
						UnregisterPackage(item2);
						flag = true;
					}
					foreach (string item3 in hashSet2)
					{
						RegisterPackage(item3);
						flag = true;
					}
				}
				if (flag)
				{
					foreach (VarPackage value4 in packagesByUid.Values)
					{
						value4.LoadMetaData();
					}
					foreach (VarPackageGroup value5 in packageGroups.Values)
					{
						value5.Init();
					}
				}
				elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
				float num2 = elapsedMilliseconds - num;
				UnityEngine.Debug.Log("Scanned " + packagesByUid.Count + " packages in " + num2.ToString("F1") + " ms");
				num = elapsedMilliseconds;
				foreach (VarPackage value6 in packagesByUid.Values)
				{
					if (value6.forceRefresh)
					{
						UnityEngine.Debug.Log("Force refresh of package " + value6.Uid);
						flag = true;
						value6.forceRefresh = false;
					}
				}
				if (flag)
				{
					UnityEngine.Debug.Log("Package changes detected");
					if (onRefreshHandlers != null)
					{
						onRefreshHandlers();
					}
				}
				else
				{
					UnityEngine.Debug.Log("No package changes detected");
				}
				elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
				UnityEngine.Debug.Log("Refresh Handlers took " + (elapsedMilliseconds - num).ToString("F1") + " ms");
				num = elapsedMilliseconds;
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during package refresh " + ex);
			}
			elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
			UnityEngine.Debug.Log("Refresh package handlers took " + (elapsedMilliseconds - num).ToString("F1") + " ms");
			lastPackageRefreshTime = DateTime.Now;
		}

		public static void RegisterRestrictedReadPath(string path)
		{
			if (restrictedReadPaths == null)
			{
				restrictedReadPaths = new HashSet<string>();
			}
			restrictedReadPaths.Add(Path.GetFullPath(path));
		}

		public static void RegisterSecureReadPath(string path)
		{
			if (secureReadPaths == null)
			{
				secureReadPaths = new HashSet<string>();
			}
			secureReadPaths.Add(Path.GetFullPath(path));
		}

		public static void ClearSecureReadPaths()
		{
			if (secureReadPaths == null)
			{
				secureReadPaths = new HashSet<string>();
			}
			else
			{
				secureReadPaths.Clear();
			}
		}

		public static bool IsSecureReadPath(string path)
		{
			if (secureReadPaths == null)
			{
				secureReadPaths = new HashSet<string>();
			}
			string fullPath = GetFullPath(path);
			bool flag = false;
			foreach (string restrictedReadPath in restrictedReadPaths)
			{
				if (fullPath.StartsWith(restrictedReadPath))
				{
					flag = true;
					break;
				}
			}
			bool result = false;
			if (!flag)
			{
				foreach (string secureReadPath in secureReadPaths)
				{
					if (fullPath.StartsWith(secureReadPath))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		public static void ClearSecureWritePaths()
		{
			if (secureInternalWritePaths == null)
			{
				secureInternalWritePaths = new HashSet<string>();
			}
			else
			{
				secureInternalWritePaths.Clear();
			}
			if (securePluginWritePaths == null)
			{
				securePluginWritePaths = new HashSet<string>();
			}
			else
			{
				securePluginWritePaths.Clear();
			}
			if (pluginWritePathsThatDoNotNeedConfirm == null)
			{
				pluginWritePathsThatDoNotNeedConfirm = new HashSet<string>();
			}
			else
			{
				pluginWritePathsThatDoNotNeedConfirm.Clear();
			}
		}

		public static void RegisterInternalSecureWritePath(string path)
		{
			if (secureInternalWritePaths == null)
			{
				secureInternalWritePaths = new HashSet<string>();
			}
			secureInternalWritePaths.Add(Path.GetFullPath(path));
		}

		public static void RegisterPluginSecureWritePath(string path, bool doesNotNeedConfirm)
		{
			if (securePluginWritePaths == null)
			{
				securePluginWritePaths = new HashSet<string>();
			}
			if (pluginWritePathsThatDoNotNeedConfirm == null)
			{
				pluginWritePathsThatDoNotNeedConfirm = new HashSet<string>();
			}
			string fullPath = Path.GetFullPath(path);
			securePluginWritePaths.Add(fullPath);
			if (doesNotNeedConfirm)
			{
				pluginWritePathsThatDoNotNeedConfirm.Add(fullPath);
			}
		}

		public static bool IsSecureWritePath(string path)
		{
			if (secureInternalWritePaths == null)
			{
				secureInternalWritePaths = new HashSet<string>();
			}
			string fullPath = GetFullPath(path);
			bool result = false;
			foreach (string secureInternalWritePath in secureInternalWritePaths)
			{
				if (fullPath.StartsWith(secureInternalWritePath))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public static bool IsSecurePluginWritePath(string path)
		{
			if (securePluginWritePaths == null)
			{
				securePluginWritePaths = new HashSet<string>();
			}
			string fullPath = GetFullPath(path);
			bool result = false;
			foreach (string securePluginWritePath in securePluginWritePaths)
			{
				if (fullPath.StartsWith(securePluginWritePath))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public static bool IsPluginWritePathThatNeedsConfirm(string path)
		{
			if (pluginWritePathsThatDoNotNeedConfirm == null)
			{
				pluginWritePathsThatDoNotNeedConfirm = new HashSet<string>();
			}
			string fullPath = GetFullPath(path);
			bool result = true;
			foreach (string item in pluginWritePathsThatDoNotNeedConfirm)
			{
				if (fullPath.StartsWith(item))
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public static void RegisterPluginHashToPluginPath(string hash, string path)
		{
			if (pluginHashToPluginPath == null)
			{
				pluginHashToPluginPath = new Dictionary<string, string>();
			}
			pluginHashToPluginPath.Remove(hash);
			pluginHashToPluginPath.Add(hash, path);
		}

		public static void RegisterAssemblyToPluginHash(Assembly assembly, string hash)
		{
			if (assemblyToPluginHash == null)
			{
				assemblyToPluginHash = new Dictionary<Assembly, string>();
			}
			if (assembly != null)
			{
				if (assemblyToPluginHash.ContainsKey(assembly))
				{
					assemblyToPluginHash.Remove(assembly);
				}
				assemblyToPluginHash.Add(assembly, hash);
			}
		}

		public static string GetPluginHash()
		{
			StackTrace stackTrace = new StackTrace();
			string value = null;
			for (int i = 0; i < stackTrace.FrameCount; i++)
			{
				StackFrame frame = stackTrace.GetFrame(i);
				MethodBase method = frame.GetMethod();
				Assembly assembly = method.DeclaringType.Assembly;
				if (assemblyToPluginHash != null && assemblyToPluginHash.TryGetValue(assembly, out value))
				{
					break;
				}
				AssemblyName assemblyName = method.DeclaringType.Assembly.GetName();
				string text = assemblyName.Name;
				if (text.StartsWith("MVRPlugin_"))
				{
					value = Regex.Replace(text, "_[0-9]+$", string.Empty);
					break;
				}
			}
			return value;
		}

		public static string GetPluginHashDisplayName()
		{
			string pluginHash = GetPluginHash();
			string value;
			if (pluginHashToPluginPath == null || !pluginHashToPluginPath.TryGetValue(pluginHash, out value))
			{
				return pluginHash;
			}
			return value;
		}

		public static void AssertNotCalledFromPlugin()
		{
			string pluginHash = GetPluginHash();
			if (pluginHash != null)
			{
				StackTrace stackTrace = new StackTrace();
				StackFrame frame = stackTrace.GetFrame(1);
				MethodBase method = frame.GetMethod();
				string value;
				if (pluginHashToPluginPath == null || !pluginHashToPluginPath.TryGetValue(pluginHash, out value))
				{
					value = pluginHash;
				}
				SuperController.LogError(string.Concat("Plugin with signature ", value, " tried to execute operation ", method.DeclaringType, ":", method.Name, " that is blocked from plugin access"));
				throw new Exception(string.Concat("Plugin with signature ", value, " tried to execute operation ", method.DeclaringType, ":", method.Name, " that is blocked from plugin access"));
			}
		}

		public static void AssertNotCalledFromUnityMessaging()
		{
			StackTrace stackTrace = new StackTrace();
			if (stackTrace.FrameCount >= 2)
			{
				StackFrame frame = stackTrace.GetFrame(2);
				MethodBase method = frame.GetMethod();
				if ((method.DeclaringType == typeof(Component) || method.DeclaringType == typeof(GameObject)) && (method.Name == "SendMessage" || method.Name == "SendMessageUpwards" || method.Name == "BroadcastMessage"))
				{
					StackFrame frame2 = stackTrace.GetFrame(1);
					MethodBase method2 = frame2.GetMethod();
					string pluginHashDisplayName = GetPluginHashDisplayName();
					SuperController.LogError(string.Concat("Plugin ", pluginHashDisplayName, " used SendMessage on an operation ", method2.DeclaringType, ":", method2.Name, " that is blocked from plugin access"));
					throw new Exception(string.Concat("Plugin ", pluginHashDisplayName, " used SendMessage on an operation ", method2.DeclaringType, ":", method2.Name, " that is blocked from plugin access"));
				}
			}
		}

		protected void DestroyUserConfirmPanel(UserConfirmPanel ucp)
		{
			AssertNotCalledFromUnityMessaging();
			UnityEngine.Object.Destroy(ucp.gameObject);
			activeUserConfirmPanels.Remove(ucp);
			if (activeUserConfirmPanels.Count == 0 && userConfirmFlag != null)
			{
				userConfirmFlag.Raise();
				userConfirmFlag = null;
			}
		}

		protected void CreateUserConfirmFlag()
		{
			AssertNotCalledFromUnityMessaging();
			if (userConfirmFlag == null && SuperController.singleton != null)
			{
				userConfirmFlag = new AsyncFlag("UserConfirm");
				SuperController.singleton.SetLoadingIconFlag(userConfirmFlag);
				SuperController.singleton.PauseAutoSimulation(userConfirmFlag);
			}
		}

		protected void DestroyAllUserConfirmPanels()
		{
			AssertNotCalledFromUnityMessaging();
			List<UserConfirmPanel> list = new List<UserConfirmPanel>(activeUserConfirmPanels);
			foreach (UserConfirmPanel item in list)
			{
				DestroyUserConfirmPanel(item);
			}
		}

		protected void UserConfirm(string prompt, UserActionCallback confirmCallback, UserActionCallback autoConfirmCallback, UserActionCallback confirmStickyCallback, UserActionCallback denyCallback, UserActionCallback autoDenyCallback, UserActionCallback denyStickyCallback)
		{
			AssertNotCalledFromUnityMessaging();
			if (userConfirmContainer != null && userConfirmPrefab != null)
			{
				if (activeUserConfirmPanels == null)
				{
					activeUserConfirmPanels = new HashSet<UserConfirmPanel>();
				}
				CreateUserConfirmFlag();
				Transform transform = UnityEngine.Object.Instantiate(userConfirmPrefab);
				transform.SetParent(userConfirmContainer, false);
				transform.SetAsFirstSibling();
				UserConfirmPanel ucp = transform.GetComponent<UserConfirmPanel>();
				if (ucp != null)
				{
					ucp.signature = prompt;
					ucp.SetPrompt(prompt);
					activeUserConfirmPanels.Add(ucp);
					ucp.SetConfirmCallback(delegate
					{
						DestroyUserConfirmPanel(ucp);
						if (confirmCallback != null)
						{
							confirmCallback();
						}
					});
					ucp.SetAutoConfirmCallback(delegate
					{
						DestroyUserConfirmPanel(ucp);
						if (autoConfirmCallback != null)
						{
							autoConfirmCallback();
						}
					});
					ucp.SetConfirmStickyCallback(delegate
					{
						DestroyUserConfirmPanel(ucp);
						if (confirmStickyCallback != null)
						{
							confirmStickyCallback();
						}
					});
					ucp.SetDenyCallback(delegate
					{
						DestroyUserConfirmPanel(ucp);
						if (denyCallback != null)
						{
							denyCallback();
						}
					});
					ucp.SetAutoDenyCallback(delegate
					{
						DestroyUserConfirmPanel(ucp);
						if (autoDenyCallback != null)
						{
							autoDenyCallback();
						}
					});
					ucp.SetDenyStickyCallback(delegate
					{
						DestroyUserConfirmPanel(ucp);
						if (denyStickyCallback != null)
						{
							denyStickyCallback();
						}
					});
				}
				else
				{
					UnityEngine.Object.Destroy(transform.gameObject);
					if (denyCallback != null)
					{
						denyCallback();
					}
				}
			}
			else if (denyCallback != null)
			{
				denyCallback();
			}
		}

		public static void ConfirmWithUser(string prompt, UserActionCallback confirmCallback, UserActionCallback autoConfirmCallback, UserActionCallback confirmStickyCallback, UserActionCallback denyCallback, UserActionCallback autoDenyCallback, UserActionCallback denyStickyCallback)
		{
			if (singleton != null)
			{
				singleton.UserConfirm(prompt, confirmCallback, autoConfirmCallback, confirmStickyCallback, denyCallback, autoDenyCallback, denyStickyCallback);
			}
			else
			{
				denyCallback();
			}
		}

		protected void AutoConfirmAllPanelsWithSignature(string signature)
		{
			AssertNotCalledFromUnityMessaging();
			List<UserConfirmPanel> list = new List<UserConfirmPanel>();
			foreach (UserConfirmPanel activeUserConfirmPanel in activeUserConfirmPanels)
			{
				if (activeUserConfirmPanel.signature == signature)
				{
					list.Add(activeUserConfirmPanel);
				}
			}
			foreach (UserConfirmPanel item in list)
			{
				item.AutoConfirm();
			}
		}

		protected void ConfirmAllPanelsWithSignature(string signature, bool isPlugin)
		{
			AssertNotCalledFromUnityMessaging();
			List<UserConfirmPanel> list = new List<UserConfirmPanel>();
			foreach (UserConfirmPanel activeUserConfirmPanel in activeUserConfirmPanels)
			{
				if (activeUserConfirmPanel.signature == signature)
				{
					list.Add(activeUserConfirmPanel);
				}
			}
			foreach (UserConfirmPanel item in list)
			{
				item.Confirm();
			}
			if (isPlugin)
			{
				userConfirmedPlugins.Add(signature);
			}
		}

		public static void AutoConfirmAllWithSignature(string signature)
		{
			if (singleton != null)
			{
				singleton.AutoConfirmAllPanelsWithSignature(signature);
			}
		}

		protected void AutoDenyAllPanelsWithSignature(string signature)
		{
			AssertNotCalledFromUnityMessaging();
			List<UserConfirmPanel> list = new List<UserConfirmPanel>();
			foreach (UserConfirmPanel activeUserConfirmPanel in activeUserConfirmPanels)
			{
				if (activeUserConfirmPanel.signature == signature)
				{
					list.Add(activeUserConfirmPanel);
				}
			}
			foreach (UserConfirmPanel item in list)
			{
				item.AutoDeny();
			}
		}

		protected void DenyAllPanelsWithSignature(string signature, bool isPlugin)
		{
			AssertNotCalledFromUnityMessaging();
			List<UserConfirmPanel> list = new List<UserConfirmPanel>();
			foreach (UserConfirmPanel activeUserConfirmPanel in activeUserConfirmPanels)
			{
				if (activeUserConfirmPanel.signature == signature)
				{
					list.Add(activeUserConfirmPanel);
				}
			}
			foreach (UserConfirmPanel item in list)
			{
				item.Deny();
			}
			if (isPlugin)
			{
				userDeniedPlugins.Add(signature);
			}
		}

		public static void AutoDenyAllWithSignature(string signature)
		{
			if (singleton != null)
			{
				singleton.AutoDenyAllPanelsWithSignature(signature);
			}
		}

		protected void UserConfirmPluginAction(string prompt, UserActionCallback confirmCallback, UserActionCallback denyCallback)
		{
			AssertNotCalledFromUnityMessaging();
			if (userConfirmContainer != null && userConfirmPluginActionPrefab != null)
			{
				if (userConfirmedPlugins == null)
				{
					userConfirmedPlugins = new HashSet<string>();
				}
				if (userDeniedPlugins == null)
				{
					userDeniedPlugins = new HashSet<string>();
				}
				if (activeUserConfirmPanels == null)
				{
					activeUserConfirmPanels = new HashSet<UserConfirmPanel>();
				}
				string pluginHash = GetPluginHash();
				if (pluginHash == null)
				{
					UnityEngine.Debug.LogError("Plugin did not have signature!");
				}
				if (pluginHash != null)
				{
					if (userDeniedPlugins.Contains(pluginHash))
					{
						if (denyCallback != null)
						{
							denyCallback();
						}
						return;
					}
					if (userConfirmedPlugins.Contains(pluginHash))
					{
						if (confirmCallback != null)
						{
							confirmCallback();
						}
						return;
					}
				}
				Transform transform = UnityEngine.Object.Instantiate(userConfirmPluginActionPrefab);
				transform.SetParent(userConfirmContainer, false);
				transform.SetAsFirstSibling();
				UserConfirmPanel ucp = transform.GetComponent<UserConfirmPanel>();
				if (ucp != null && pluginHash != null)
				{
					string value;
					if (pluginHashToPluginPath == null || !pluginHashToPluginPath.TryGetValue(pluginHash, out value))
					{
						value = pluginHash;
					}
					ucp.signature = pluginHash;
					ucp.SetPrompt("Plugin " + value + "\nwants to " + prompt);
					activeUserConfirmPanels.Add(ucp);
					ucp.SetConfirmCallback(delegate
					{
						DestroyUserConfirmPanel(ucp);
						if (confirmCallback != null)
						{
							confirmCallback();
						}
					});
					ucp.SetConfirmStickyCallback(delegate
					{
						ConfirmAllPanelsWithSignature(pluginHash, true);
					});
					ucp.SetDenyCallback(delegate
					{
						DestroyUserConfirmPanel(ucp);
						if (denyCallback != null)
						{
							denyCallback();
						}
					});
					ucp.SetDenyStickyCallback(delegate
					{
						DenyAllPanelsWithSignature(pluginHash, true);
					});
				}
				else
				{
					UnityEngine.Object.Destroy(transform.gameObject);
					if (denyCallback != null)
					{
						denyCallback();
					}
				}
			}
			else if (denyCallback != null)
			{
				denyCallback();
			}
		}

		public static void ConfirmPluginActionWithUser(string prompt, UserActionCallback confirmCallback, UserActionCallback denyCallback)
		{
			if (singleton != null)
			{
				singleton.UserConfirmPluginAction(prompt, confirmCallback, denyCallback);
			}
			else
			{
				denyCallback();
			}
		}

		public static string GetFullPath(string path)
		{
			string path2 = Regex.Replace(path, "^file:///", string.Empty);
			return Path.GetFullPath(path2);
		}

		public static bool IsPackagePath(string path)
		{
			string input = path.Replace('\\', '/');
			string packageUidOrPath = Regex.Replace(input, ":/.*", string.Empty);
			VarPackage package = GetPackage(packageUidOrPath);
			return package != null;
		}

		public static bool IsSimulatedPackagePath(string path)
		{
			string input = path.Replace('\\', '/');
			string packageUidOrPath = Regex.Replace(input, ":/.*", string.Empty);
			return GetPackage(packageUidOrPath)?.IsSimulated ?? false;
		}

		public static string ConvertSimulatedPackagePathToNormalPath(string path)
		{
			string text = path.Replace('\\', '/');
			if (text.Contains(":/"))
			{
				string packageUidOrPath = Regex.Replace(text, ":/.*", string.Empty);
				VarPackage package = GetPackage(packageUidOrPath);
				if (package != null && package.IsSimulated)
				{
					string text2 = Regex.Replace(text, ".*:/", string.Empty);
					path = package.SlashPath + "/" + text2;
				}
			}
			return path;
		}

		public static string RemovePackageFromPath(string path)
		{
			string input = Regex.Replace(path, ".*:/", string.Empty);
			return Regex.Replace(input, ".*:\\\\", string.Empty);
		}

		public static string NormalizePath(string path)
		{
			string text = path;
			VarFileEntry varFileEntry = GetVarFileEntry(path);
			if (varFileEntry == null)
			{
				string fullPath = GetFullPath(path);
				string oldValue = Path.GetFullPath(".") + "\\";
				string text2 = fullPath.Replace(oldValue, string.Empty);
				if (text2 != fullPath)
				{
					text = text2;
				}
				return text.Replace('\\', '/');
			}
			return varFileEntry.Uid;
		}

		public static string GetDirectoryName(string path, bool returnSlashPath = false)
		{
			VarFileEntry value;
			string path2 = ((uidToVarFileEntry != null && uidToVarFileEntry.TryGetValue(path, out value)) ? ((!returnSlashPath) ? value.Path : value.SlashPath) : ((!returnSlashPath) ? path.Replace('/', '\\') : path.Replace('\\', '/')));
			return Path.GetDirectoryName(path2);
		}

		public static string GetSuggestedBrowserDirectoryFromDirectoryPath(string suggestedDir, string currentDir, bool allowPackagePath = true)
		{
			if (currentDir == null || currentDir == string.Empty)
			{
				return suggestedDir;
			}
			string input = suggestedDir.Replace('\\', '/');
			input = Regex.Replace(input, "/$", string.Empty);
			string text = currentDir.Replace('\\', '/');
			VarDirectoryEntry varDirectoryEntry = GetVarDirectoryEntry(text);
			if (varDirectoryEntry != null)
			{
				if (!allowPackagePath)
				{
					return null;
				}
				string text2 = varDirectoryEntry.InternalSlashPath.Replace(input, string.Empty);
				if (varDirectoryEntry.InternalSlashPath != text2)
				{
					text2 = text2.Replace('/', '\\');
					return varDirectoryEntry.Package.SlashPath + ":/" + input + text2;
				}
			}
			else
			{
				string text3 = text.Replace(input, string.Empty);
				if (text != text3)
				{
					text3 = text3.Replace('/', '\\');
					return suggestedDir + text3;
				}
			}
			return null;
		}

		public static void SetLoadDir(string dir, bool restrictPath = false)
		{
			if (loadDirStack != null)
			{
				loadDirStack.Clear();
			}
			PushLoadDir(dir, restrictPath);
		}

		public static void PushLoadDir(string dir, bool restrictPath = false)
		{
			string text = dir.Replace('\\', '/');
			if (text != "/")
			{
				text = Regex.Replace(text, "/$", string.Empty);
			}
			if (restrictPath && !IsSecureReadPath(text))
			{
				throw new Exception("Attempted to push load dir for non-secure dir " + text);
			}
			if (loadDirStack == null)
			{
				loadDirStack = new LinkedList<string>();
			}
			loadDirStack.AddLast(text);
		}

		public static string PopLoadDir()
		{
			string result = null;
			if (loadDirStack != null && loadDirStack.Count > 0)
			{
				result = loadDirStack.Last.Value;
				loadDirStack.RemoveLast();
			}
			return result;
		}

		public static void SetLoadDirFromFilePath(string path, bool restrictPath = false)
		{
			if (loadDirStack != null)
			{
				loadDirStack.Clear();
			}
			PushLoadDirFromFilePath(path, restrictPath);
		}

		public static void PushLoadDirFromFilePath(string path, bool restrictPath = false)
		{
			if (restrictPath && !IsSecureReadPath(path))
			{
				throw new Exception("Attempted to set load dir from non-secure path " + path);
			}
			FileEntry fileEntry = GetFileEntry(path);
			string dir;
			if (fileEntry != null)
			{
				if (fileEntry is VarFileEntry)
				{
					dir = Path.GetDirectoryName(fileEntry.Uid);
				}
				else
				{
					dir = Path.GetDirectoryName(fileEntry.FullPath);
					string oldValue = Path.GetFullPath(".") + "\\";
					dir = dir.Replace(oldValue, string.Empty);
				}
			}
			else
			{
				dir = Path.GetDirectoryName(GetFullPath(path));
				string oldValue2 = Path.GetFullPath(".") + "\\";
				dir = dir.Replace(oldValue2, string.Empty);
			}
			PushLoadDir(dir, restrictPath);
		}

		public static string PackageIDToPackageGroupID(string packageId)
		{
			string input = Regex.Replace(packageId, "\\.[0-9]+$", string.Empty);
			input = Regex.Replace(input, "\\.latest$", string.Empty);
			return Regex.Replace(input, "\\.min[0-9]+$", string.Empty);
		}

		public static string PackageIDToPackageVersion(string packageId)
		{
			Match match = Regex.Match(packageId, "[0-9]+$");
			if (match.Success)
			{
				return match.Value;
			}
			return null;
		}

		public static string NormalizeID(string id)
		{
			string text = id;
			if (text.StartsWith("SELF:"))
			{
				string currentPackageUid = CurrentPackageUid;
				if (currentPackageUid != null)
				{
					return text.Replace("SELF:", currentPackageUid + ":");
				}
				return text.Replace("SELF:", string.Empty);
			}
			return NormalizeCommon(text);
		}

		protected static string NormalizeCommon(string path)
		{
			string text = path;
			Match match;
			if ((match = Regex.Match(text, "^(([^\\.]+\\.[^\\.]+)\\.latest):")).Success)
			{
				string value = match.Groups[1].Value;
				string value2 = match.Groups[2].Value;
				VarPackageGroup packageGroup = GetPackageGroup(value2);
				if (packageGroup != null)
				{
					VarPackage newestEnabledPackage = packageGroup.NewestEnabledPackage;
					if (newestEnabledPackage != null)
					{
						text = text.Replace(value, newestEnabledPackage.Uid);
					}
				}
			}
			else if ((match = Regex.Match(text, "^(([^\\.]+\\.[^\\.]+)\\.min([0-9]+)):")).Success)
			{
				string value3 = match.Groups[1].Value;
				string value4 = match.Groups[2].Value;
				int requestVersion = int.Parse(match.Groups[3].Value);
				VarPackageGroup packageGroup2 = GetPackageGroup(value4);
				if (packageGroup2 != null)
				{
					VarPackage closestMatchingPackageVersion = packageGroup2.GetClosestMatchingPackageVersion(requestVersion, true, false);
					if (closestMatchingPackageVersion != null)
					{
						text = text.Replace(value3, closestMatchingPackageVersion.Uid);
					}
				}
			}
			else if ((match = Regex.Match(text, "^([^\\.]+\\.[^\\.]+\\.[0-9]+):")).Success)
			{
				string value5 = match.Groups[1].Value;
				VarPackage package = GetPackage(value5);
				if (package == null || !package.Enabled)
				{
					string packageGroupUid = PackageIDToPackageGroupID(value5);
					VarPackageGroup packageGroup3 = GetPackageGroup(packageGroupUid);
					if (packageGroup3 != null)
					{
						package = packageGroup3.NewestEnabledPackage;
						if (package != null)
						{
							text = text.Replace(value5, package.Uid);
						}
					}
				}
			}
			return text;
		}

		public static string NormalizeLoadPath(string path)
		{
			string result = path;
			if (path != null && path != string.Empty && path != "/" && path != "NULL")
			{
				result = path.Replace('\\', '/');
				string currentLoadDir = CurrentLoadDir;
				if (currentLoadDir != null && currentLoadDir != string.Empty)
				{
					if (!result.Contains("/"))
					{
						result = currentLoadDir + "/" + result;
					}
					else if (Regex.IsMatch(result, "^\\./"))
					{
						result = Regex.Replace(result, "^\\./", currentLoadDir + "/");
					}
				}
				if (result.StartsWith("SELF:/"))
				{
					string currentPackageUid = CurrentPackageUid;
					result = ((currentPackageUid == null) ? result.Replace("SELF:/", string.Empty) : result.Replace("SELF:/", currentPackageUid + ":/"));
				}
				else
				{
					result = NormalizeCommon(result);
				}
			}
			return result;
		}

		public static void SetSaveDir(string path, bool restrictPath = true)
		{
			if (path == null || path == string.Empty)
			{
				CurrentSaveDir = string.Empty;
				return;
			}
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsPackagePath(path))
			{
				if (restrictPath && !IsSecureWritePath(path))
				{
					throw new Exception("Attempted to set save dir from non-secure path " + path);
				}
				string fullPath = GetFullPath(path);
				string oldValue = Path.GetFullPath(".") + "\\";
				fullPath = fullPath.Replace(oldValue, string.Empty);
				CurrentSaveDir = fullPath.Replace('\\', '/');
			}
		}

		public static void SetSaveDirFromFilePath(string path, bool restrictPath = true)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsPackagePath(path))
			{
				if (restrictPath && !IsSecureWritePath(path))
				{
					throw new Exception("Attempted to set save dir from non-secure path " + path);
				}
				string directoryName = Path.GetDirectoryName(GetFullPath(path));
				string oldValue = Path.GetFullPath(".") + "\\";
				directoryName = directoryName.Replace(oldValue, string.Empty);
				CurrentSaveDir = directoryName.Replace('\\', '/');
			}
		}

		public static void SetNullSaveDir()
		{
			CurrentSaveDir = null;
		}

		public static string NormalizeSavePath(string path)
		{
			string text = path;
			if (path != null && path != string.Empty && path != "/" && path != "NULL")
			{
				string path2 = Regex.Replace(path, "^file:///", string.Empty);
				string fullPath = Path.GetFullPath(path2);
				string oldValue = Path.GetFullPath(".") + "\\";
				string text2 = fullPath.Replace(oldValue, string.Empty);
				if (text2 != fullPath)
				{
					text = text2;
				}
				text = text.Replace('\\', '/');
				string fileName = Path.GetFileName(text2);
				string text3 = Path.GetDirectoryName(text2);
				if (text3 != null)
				{
					text3 = text3.Replace('\\', '/');
				}
				if (CurrentSaveDir == text3)
				{
					text = fileName;
				}
				else if (CurrentSaveDir != null && CurrentSaveDir != string.Empty && Regex.IsMatch(text3, "^" + CurrentSaveDir + "/"))
				{
					text = text3.Replace(CurrentSaveDir, ".") + "/" + fileName;
				}
			}
			return text;
		}

		public static List<VarPackage> GetPackages()
		{
			List<VarPackage> list = null;
			if (packagesByUid != null)
			{
				return packagesByUid.Values.ToList();
			}
			return new List<VarPackage>();
		}

		public static List<string> GetPackageUids()
		{
			List<string> list = null;
			if (packagesByUid != null)
			{
				list = packagesByUid.Keys.ToList();
				list.Sort();
			}
			else
			{
				list = new List<string>();
			}
			return list;
		}

		public static bool IsPackage(string packageUidOrPath)
		{
			if (packagesByUid != null && packagesByUid.ContainsKey(packageUidOrPath))
			{
				return true;
			}
			if (packagesByPath != null && packagesByPath.ContainsKey(packageUidOrPath))
			{
				return true;
			}
			return false;
		}

		public static VarPackage GetPackage(string packageUidOrPath)
		{
			VarPackage value = null;
			Match match;
			if ((match = Regex.Match(packageUidOrPath, "^([^\\.]+\\.[^\\.]+)\\.latest$")).Success)
			{
				string value2 = match.Groups[1].Value;
				VarPackageGroup packageGroup = GetPackageGroup(value2);
				if (packageGroup != null)
				{
					value = packageGroup.NewestPackage;
				}
			}
			else if ((match = Regex.Match(packageUidOrPath, "^([^\\.]+\\.[^\\.]+)\\.min([0-9]+)$")).Success)
			{
				string value3 = match.Groups[1].Value;
				int requestVersion = int.Parse(match.Groups[2].Value);
				VarPackageGroup packageGroup2 = GetPackageGroup(value3);
				if (packageGroup2 != null)
				{
					value = packageGroup2.GetClosestMatchingPackageVersion(requestVersion, false, false);
				}
			}
			else if (packagesByUid != null && packagesByUid.ContainsKey(packageUidOrPath))
			{
				packagesByUid.TryGetValue(packageUidOrPath, out value);
			}
			else if (packagesByPath != null && packagesByPath.ContainsKey(packageUidOrPath))
			{
				packagesByPath.TryGetValue(packageUidOrPath, out value);
			}
			return value;
		}

		public static List<VarPackageGroup> GetPackageGroups()
		{
			List<VarPackageGroup> list = null;
			if (packageGroups != null)
			{
				return packageGroups.Values.ToList();
			}
			return new List<VarPackageGroup>();
		}

		public static VarPackageGroup GetPackageGroup(string packageGroupUid)
		{
			VarPackageGroup value = null;
			if (packageGroups != null)
			{
				packageGroups.TryGetValue(packageGroupUid, out value);
			}
			return value;
		}

		public static void SyncAutoAlwaysAllowPlugins(HashSet<PackageUIDAndPublisher> packageUids)
		{
			foreach (PackageUIDAndPublisher packageUid in packageUids)
			{
				VarPackage vp = GetPackage(packageUid.uid);
				if (vp == null || !vp.HasMatchingDirectories("Custom/Scripts"))
				{
					continue;
				}
				foreach (VarPackage package in vp.Group.Packages)
				{
					if (package != vp && package.PluginsAlwaysEnabled)
					{
						vp.PluginsAlwaysEnabled = true;
						SuperController.AlertUser(vp.Uid + "\nuploaded by Hub user: " + packageUid.publisher + "\n\nwas just downloaded and contains plugins. This package has been automatically set to always enable plugins due to previous version of this same package having that preference set.\n\nClick OK if you accept.\n\nClick Cancel if you wish to reject this automatic setting.", null, delegate
						{
							vp.PluginsAlwaysEnabled = false;
						});
						break;
					}
				}
				if (UserPreferences.singleton.alwaysAllowPluginsDownloadedFromHub && !vp.PluginsAlwaysEnabled)
				{
					vp.PluginsAlwaysEnabled = true;
					SuperController.AlertUser(vp.Uid + "\nuploaded by Hub user: " + packageUid.publisher + "\n\nwas just downloaded and contains plugins. This package has been automatically set to always enable plugins due to your user preference setting.\n\nClick OK if you accept.\n\nClick Cancel if you wish to reject this automatic setting for this package.", null, delegate
					{
						vp.PluginsAlwaysEnabled = false;
					});
				}
			}
			packageUids.Clear();
		}

		public static string CleanFilePath(string path)
		{
			return path?.Replace('\\', '/');
		}

		public static void FindAllFiles(string dir, string pattern, List<FileEntry> foundFiles, bool restrictPath = false)
		{
			FindRegularFiles(dir, pattern, foundFiles, restrictPath);
			FindVarFiles(dir, pattern, foundFiles);
		}

		public static void FindAllFilesRegex(string dir, string regex, List<FileEntry> foundFiles, bool restrictPath = false)
		{
			FindRegularFilesRegex(dir, regex, foundFiles, restrictPath);
			FindVarFilesRegex(dir, regex, foundFiles);
		}

		public static void FindRegularFiles(string dir, string pattern, List<FileEntry> foundFiles, bool restrictPath = false)
		{
			if (Directory.Exists(dir))
			{
				if (restrictPath && !IsSecureReadPath(dir))
				{
					throw new Exception("Attempted to find files for non-secure path " + dir);
				}
				string regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
				FindRegularFilesRegex(dir, regex, foundFiles, restrictPath);
			}
		}

		public static bool CheckIfDirectoryChanged(string dir, DateTime previousCheckTime, bool recurse = true)
		{
			if (Directory.Exists(dir))
			{
				DateTime lastWriteTime = Directory.GetLastWriteTime(dir);
				if (lastWriteTime > previousCheckTime)
				{
					return true;
				}
				if (recurse)
				{
					string[] directories = Directory.GetDirectories(dir);
					foreach (string dir2 in directories)
					{
						if (CheckIfDirectoryChanged(dir2, previousCheckTime, recurse))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public static void FindRegularFilesRegex(string dir, string regex, List<FileEntry> foundFiles, bool restrictPath = false)
		{
			dir = CleanDirectoryPath(dir);
			if (!Directory.Exists(dir))
			{
				return;
			}
			if (restrictPath && !IsSecureReadPath(dir))
			{
				throw new Exception("Attempted to find files for non-secure path " + dir);
			}
			string[] files = Directory.GetFiles(dir);
			foreach (string text in files)
			{
				if (Regex.IsMatch(text, regex, RegexOptions.IgnoreCase))
				{
					SystemFileEntry systemFileEntry = new SystemFileEntry(text);
					if (systemFileEntry.Exists)
					{
						foundFiles.Add(systemFileEntry);
					}
					else
					{
						UnityEngine.Debug.LogError("Error in lookup SystemFileEntry for " + text);
					}
				}
			}
			string[] directories = Directory.GetDirectories(dir);
			foreach (string dir2 in directories)
			{
				FindRegularFilesRegex(dir2, regex, foundFiles);
			}
		}

		public static void FindVarFiles(string dir, string pattern, List<FileEntry> foundFiles)
		{
			if (allVarFileEntries != null)
			{
				string regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
				FindVarFilesRegex(dir, regex, foundFiles);
			}
		}

		public static void FindVarFilesRegex(string dir, string regex, List<FileEntry> foundFiles)
		{
			dir = CleanDirectoryPath(dir);
			if (allVarFileEntries == null)
			{
				return;
			}
			foreach (VarFileEntry allVarFileEntry in allVarFileEntries)
			{
				if (allVarFileEntry.InternalSlashPath.StartsWith(dir) && Regex.IsMatch(allVarFileEntry.Name, regex, RegexOptions.IgnoreCase))
				{
					foundFiles.Add(allVarFileEntry);
				}
			}
		}

		public static bool FileExists(string path, bool onlySystemFiles = false, bool restrictPath = false)
		{
			if (path != null && path != string.Empty)
			{
				if (!onlySystemFiles)
				{
					string key = CleanFilePath(path);
					if (uidToVarFileEntry != null && uidToVarFileEntry.ContainsKey(path))
					{
						return true;
					}
					if (pathToVarFileEntry != null && pathToVarFileEntry.ContainsKey(key))
					{
						return true;
					}
				}
				if (File.Exists(path))
				{
					if (restrictPath && !IsSecureReadPath(path))
					{
						throw new Exception("Attempted to check file existence for non-secure path " + path);
					}
					return true;
				}
			}
			return false;
		}

		public static DateTime FileLastWriteTime(string path, bool onlySystemFiles = false, bool restrictPath = false)
		{
			if (path != null && path != string.Empty)
			{
				if (!onlySystemFiles)
				{
					string key = CleanFilePath(path);
					VarFileEntry value;
					if (uidToVarFileEntry != null && uidToVarFileEntry.TryGetValue(path, out value))
					{
						return value.LastWriteTime;
					}
					if (pathToVarFileEntry != null && pathToVarFileEntry.TryGetValue(key, out value))
					{
						return value.LastWriteTime;
					}
				}
				if (File.Exists(path))
				{
					if (restrictPath && !IsSecureReadPath(path))
					{
						throw new Exception("Attempted to check file existence for non-secure path " + path);
					}
					FileInfo fileInfo = new FileInfo(path);
					return fileInfo.LastWriteTime;
				}
			}
			return DateTime.MinValue;
		}

		public static DateTime FileCreationTime(string path, bool onlySystemFiles = false, bool restrictPath = false)
		{
			if (path != null && path != string.Empty)
			{
				if (!onlySystemFiles)
				{
					string key = CleanFilePath(path);
					VarFileEntry value;
					if (uidToVarFileEntry != null && uidToVarFileEntry.TryGetValue(path, out value))
					{
						return value.LastWriteTime;
					}
					if (pathToVarFileEntry != null && pathToVarFileEntry.TryGetValue(key, out value))
					{
						return value.LastWriteTime;
					}
				}
				if (File.Exists(path))
				{
					if (restrictPath && !IsSecureReadPath(path))
					{
						throw new Exception("Attempted to check file existence for non-secure path " + path);
					}
					FileInfo fileInfo = new FileInfo(path);
					return fileInfo.CreationTime;
				}
			}
			return DateTime.MinValue;
		}

		public static bool IsFileInPackage(string path)
		{
			string key = CleanFilePath(path);
			if (uidToVarFileEntry != null && uidToVarFileEntry.ContainsKey(key))
			{
				return true;
			}
			if (pathToVarFileEntry != null && pathToVarFileEntry.ContainsKey(key))
			{
				return true;
			}
			return false;
		}

		public static bool IsFavorite(string path, bool restrictPath = false)
		{
			FileEntry fileEntry = GetVarFileEntry(path);
			if (fileEntry == null)
			{
				fileEntry = GetSystemFileEntry(path, restrictPath);
			}
			return fileEntry?.IsFavorite() ?? false;
		}

		public static void SetFavorite(string path, bool fav, bool restrictPath = false)
		{
			FileEntry fileEntry = GetVarFileEntry(path);
			if (fileEntry == null)
			{
				fileEntry = GetSystemFileEntry(path, restrictPath);
			}
			fileEntry?.SetFavorite(fav);
		}

		public static bool IsHidden(string path, bool restrictPath = false)
		{
			FileEntry fileEntry = GetVarFileEntry(path);
			if (fileEntry == null)
			{
				fileEntry = GetSystemFileEntry(path, restrictPath);
			}
			return fileEntry?.IsHidden() ?? false;
		}

		public static void SetHidden(string path, bool hide, bool restrictPath = false)
		{
			FileEntry fileEntry = GetVarFileEntry(path);
			if (fileEntry == null)
			{
				fileEntry = GetSystemFileEntry(path, restrictPath);
			}
			fileEntry?.SetHidden(hide);
		}

		public static FileEntry GetFileEntry(string path, bool restrictPath = false)
		{
			FileEntry fileEntry = null;
			fileEntry = GetVarFileEntry(path);
			if (fileEntry == null)
			{
				fileEntry = GetSystemFileEntry(path, restrictPath);
			}
			return fileEntry;
		}

		public static SystemFileEntry GetSystemFileEntry(string path, bool restrictPath = false)
		{
			SystemFileEntry result = null;
			if (File.Exists(path))
			{
				if (restrictPath && !IsSecureReadPath(path))
				{
					throw new Exception("Attempted to get file entry for non-secure path " + path);
				}
				result = new SystemFileEntry(path);
			}
			return result;
		}

		public static VarFileEntry GetVarFileEntry(string path)
		{
			VarFileEntry value = null;
			string key = CleanFilePath(path);
			if ((uidToVarFileEntry != null && uidToVarFileEntry.TryGetValue(key, out value)) || pathToVarFileEntry == null || pathToVarFileEntry.TryGetValue(key, out value))
			{
			}
			return value;
		}

		public static void SortFileEntriesByLastWriteTime(List<FileEntry> fileEntries)
		{
			fileEntries.Sort((FileEntry e1, FileEntry e2) => e1.LastWriteTime.CompareTo(e2.LastWriteTime));
		}

		public static string CleanDirectoryPath(string path)
		{
			if (path != null)
			{
				string input = path.Replace('\\', '/');
				return Regex.Replace(input, "/$", string.Empty);
			}
			return null;
		}

		public static int FolderContentsCount(string path)
		{
			int num = Directory.GetFiles(path).Length;
			string[] directories = Directory.GetDirectories(path);
			string[] array = directories;
			foreach (string path2 in array)
			{
				num += FolderContentsCount(path2);
			}
			return num;
		}

		public static List<VarDirectoryEntry> FindVarDirectories(string dir, bool exactMatch = true)
		{
			dir = CleanDirectoryPath(dir);
			List<VarDirectoryEntry> list = new List<VarDirectoryEntry>();
			if (allVarDirectoryEntries != null)
			{
				foreach (VarDirectoryEntry allVarDirectoryEntry in allVarDirectoryEntries)
				{
					if (exactMatch)
					{
						if (allVarDirectoryEntry.InternalSlashPath == dir)
						{
							list.Add(allVarDirectoryEntry);
						}
					}
					else if (allVarDirectoryEntry.InternalSlashPath.StartsWith(dir))
					{
						list.Add(allVarDirectoryEntry);
					}
				}
			}
			return list;
		}

		public static List<ShortCut> GetShortCutsForDirectory(string dir, bool allowNavigationAboveRegularDirectories = false, bool useFullPaths = false, bool generateAllFlattenedShortcut = false, bool includeRegularDirsInFlattenedShortcut = false)
		{
			dir = Regex.Replace(dir, ".*:\\\\", string.Empty);
			string text = dir.TrimEnd('/', '\\');
			text = text.Replace('\\', '/');
			List<VarDirectoryEntry> list = FindVarDirectories(text);
			List<ShortCut> list2 = new List<ShortCut>();
			if (DirectoryExists(text))
			{
				ShortCut shortCut = new ShortCut();
				shortCut.package = string.Empty;
				if (allowNavigationAboveRegularDirectories)
				{
					text = text.Replace('/', '\\');
					if (useFullPaths)
					{
						shortCut.path = Path.GetFullPath(text);
					}
					else
					{
						shortCut.path = text;
					}
				}
				else
				{
					shortCut.path = text;
				}
				shortCut.displayName = text;
				list2.Add(shortCut);
			}
			if (list.Count > 0)
			{
				if (generateAllFlattenedShortcut)
				{
					if (includeRegularDirsInFlattenedShortcut)
					{
						ShortCut shortCut2 = new ShortCut();
						shortCut2.path = text;
						shortCut2.displayName = "From: " + text;
						shortCut2.flatten = true;
						shortCut2.package = "All Flattened";
						shortCut2.includeRegularDirsInFlatten = true;
						list2.Add(shortCut2);
					}
					ShortCut shortCut3 = new ShortCut();
					shortCut3.path = text;
					shortCut3.displayName = "From: " + text;
					shortCut3.flatten = true;
					shortCut3.package = "AddonPackages Flattened";
					list2.Add(shortCut3);
				}
				ShortCut shortCut4 = new ShortCut();
				shortCut4.package = "AddonPackages Filtered";
				shortCut4.path = "AddonPackages";
				shortCut4.displayName = "Filter: " + text;
				shortCut4.packageFilter = text;
				list2.Add(shortCut4);
			}
			foreach (VarDirectoryEntry item in list)
			{
				ShortCut shortCut5 = new ShortCut();
				shortCut5.directoryEntry = item;
				shortCut5.isLatest = item.Package.isNewestEnabledVersion;
				shortCut5.package = item.Package.Uid;
				shortCut5.creator = item.Package.Creator;
				shortCut5.displayName = item.InternalSlashPath;
				shortCut5.path = item.SlashPath;
				list2.Add(shortCut5);
			}
			return list2;
		}

		public static bool DirectoryExists(string path, bool onlySystemDirectories = false, bool restrictPath = false)
		{
			if (path != null && path != string.Empty)
			{
				if (!onlySystemDirectories)
				{
					string key = CleanDirectoryPath(path);
					if (uidToVarDirectoryEntry != null && uidToVarDirectoryEntry.ContainsKey(key))
					{
						return true;
					}
					if (pathToVarDirectoryEntry != null && pathToVarDirectoryEntry.ContainsKey(key))
					{
						return true;
					}
				}
				if (Directory.Exists(path))
				{
					if (restrictPath && !IsSecureReadPath(path))
					{
						throw new Exception("Attempted to check file existence for non-secure path " + path);
					}
					return true;
				}
			}
			return false;
		}

		public static DateTime DirectoryLastWriteTime(string path, bool onlySystemDirectories = false, bool restrictPath = false)
		{
			if (path != null && path != string.Empty)
			{
				if (!onlySystemDirectories)
				{
					string key = CleanFilePath(path);
					VarDirectoryEntry value;
					if (uidToVarDirectoryEntry != null && uidToVarDirectoryEntry.TryGetValue(path, out value))
					{
						return value.LastWriteTime;
					}
					if (pathToVarDirectoryEntry != null && pathToVarDirectoryEntry.TryGetValue(key, out value))
					{
						return value.LastWriteTime;
					}
				}
				if (Directory.Exists(path))
				{
					if (restrictPath && !IsSecureReadPath(path))
					{
						throw new Exception("Attempted to check directory last write time for non-secure path " + path);
					}
					DirectoryInfo directoryInfo = new DirectoryInfo(path);
					return directoryInfo.LastWriteTime;
				}
			}
			return DateTime.MinValue;
		}

		public static DateTime DirectoryCreationTime(string path, bool onlySystemDirectories = false, bool restrictPath = false)
		{
			if (path != null && path != string.Empty)
			{
				if (!onlySystemDirectories)
				{
					string key = CleanFilePath(path);
					VarDirectoryEntry value;
					if (uidToVarDirectoryEntry != null && uidToVarDirectoryEntry.TryGetValue(path, out value))
					{
						return value.LastWriteTime;
					}
					if (pathToVarDirectoryEntry != null && pathToVarDirectoryEntry.TryGetValue(key, out value))
					{
						return value.LastWriteTime;
					}
				}
				if (Directory.Exists(path))
				{
					if (restrictPath && !IsSecureReadPath(path))
					{
						throw new Exception("Attempted to check directory creation time for non-secure path " + path);
					}
					DirectoryInfo directoryInfo = new DirectoryInfo(path);
					return directoryInfo.CreationTime;
				}
			}
			return DateTime.MinValue;
		}

		public static bool IsDirectoryInPackage(string path)
		{
			string key = CleanDirectoryPath(path);
			if (uidToVarDirectoryEntry != null && uidToVarDirectoryEntry.ContainsKey(key))
			{
				return true;
			}
			if (pathToVarDirectoryEntry != null && pathToVarDirectoryEntry.ContainsKey(key))
			{
				return true;
			}
			return false;
		}

		public static DirectoryEntry GetDirectoryEntry(string path, bool restrictPath = false)
		{
			string path2 = Regex.Replace(path, "(/|\\\\)$", string.Empty);
			DirectoryEntry directoryEntry = null;
			directoryEntry = GetVarDirectoryEntry(path2);
			if (directoryEntry == null)
			{
				directoryEntry = GetSystemDirectoryEntry(path2, restrictPath);
			}
			return directoryEntry;
		}

		public static SystemDirectoryEntry GetSystemDirectoryEntry(string path, bool restrictPath = false)
		{
			SystemDirectoryEntry result = null;
			if (Directory.Exists(path))
			{
				if (restrictPath && !IsSecureReadPath(path))
				{
					throw new Exception("Attempted to get directory entry for non-secure path " + path);
				}
				result = new SystemDirectoryEntry(path);
			}
			return result;
		}

		public static VarDirectoryEntry GetVarDirectoryEntry(string path)
		{
			VarDirectoryEntry value = null;
			string key = CleanDirectoryPath(path);
			if ((uidToVarDirectoryEntry != null && uidToVarDirectoryEntry.TryGetValue(key, out value)) || pathToVarDirectoryEntry == null || pathToVarDirectoryEntry.TryGetValue(key, out value))
			{
			}
			return value;
		}

		public static VarDirectoryEntry GetVarRootDirectoryEntryFromPath(string path)
		{
			VarDirectoryEntry value = null;
			if (varPackagePathToRootVarDirectory != null)
			{
				varPackagePathToRootVarDirectory.TryGetValue(path, out value);
			}
			return value;
		}

		public static string[] GetDirectories(string dir, string pattern = null, bool restrictPath = false)
		{
			if (restrictPath && !IsSecureReadPath(dir))
			{
				throw new Exception("Attempted to get directories at non-secure path " + dir);
			}
			List<string> list = new List<string>();
			DirectoryEntry directoryEntry = GetDirectoryEntry(dir, restrictPath);
			if (directoryEntry == null)
			{
				throw new Exception("Attempted to get directories at non-existent path " + dir);
			}
			string text = null;
			if (pattern != null)
			{
				text = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
			}
			foreach (DirectoryEntry subDirectory in directoryEntry.SubDirectories)
			{
				if (text == null || Regex.IsMatch(subDirectory.Name, text))
				{
					list.Add(dir + "\\" + subDirectory.Name);
				}
			}
			return list.ToArray();
		}

		public static string[] GetFiles(string dir, string pattern = null, bool restrictPath = false)
		{
			if (restrictPath && !IsSecureReadPath(dir))
			{
				throw new Exception("Attempted to get files at non-secure path " + dir);
			}
			List<string> list = new List<string>();
			DirectoryEntry directoryEntry = GetDirectoryEntry(dir, restrictPath);
			if (directoryEntry == null)
			{
				throw new Exception("Attempted to get files at non-existent path " + dir);
			}
			foreach (FileEntry file in directoryEntry.GetFiles(pattern))
			{
				list.Add(dir + "\\" + file.Name);
			}
			return list.ToArray();
		}

		public static void CreateDirectory(string path)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!DirectoryExists(path))
			{
				if (!IsSecureWritePath(path))
				{
					throw new Exception("Attempted to create directory at non-secure path " + path);
				}
				Directory.CreateDirectory(path);
			}
		}

		public static void CreateDirectoryFromPlugin(string path, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (DirectoryExists(path))
			{
				return;
			}
			if (!IsSecurePluginWritePath(path))
			{
				Exception ex = new Exception("Plugin attempted to create directory at non-secure path " + path);
				if (exceptionCallback != null)
				{
					exceptionCallback(ex);
					return;
				}
				throw ex;
			}
			try
			{
				Directory.CreateDirectory(path);
			}
			catch (Exception ex2)
			{
				if (exceptionCallback != null)
				{
					exceptionCallback(ex2);
					return;
				}
				throw ex2;
			}
			confirmCallback?.Invoke();
		}

		public static void DeleteDirectory(string path, bool recursive = false)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (DirectoryExists(path))
			{
				if (!IsSecureWritePath(path))
				{
					throw new Exception("Attempted to delete file at non-secure path " + path);
				}
				Directory.Delete(path, recursive);
			}
		}

		public static void DeleteDirectoryFromPlugin(string path, bool recursive, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!DirectoryExists(path))
			{
				return;
			}
			if (!IsSecurePluginWritePath(path))
			{
				Exception ex = new Exception("Plugin attempted to delete directory at non-secure path " + path);
				if (exceptionCallback != null)
				{
					exceptionCallback(ex);
					return;
				}
				throw ex;
			}
			if (!IsPluginWritePathThatNeedsConfirm(path))
			{
				try
				{
					Directory.Delete(path, recursive);
				}
				catch (Exception ex2)
				{
					if (exceptionCallback != null)
					{
						exceptionCallback(ex2);
						return;
					}
					throw ex2;
				}
				if (confirmCallback != null)
				{
					confirmCallback();
				}
				return;
			}
			ConfirmPluginActionWithUser("delete directory at " + path, delegate
			{
				try
				{
					Directory.Delete(path, recursive);
				}
				catch (Exception e)
				{
					if (exceptionCallback != null)
					{
						exceptionCallback(e);
					}
					return;
				}
				if (confirmCallback != null)
				{
					confirmCallback();
				}
			}, denyCallback);
		}

		public static void MoveDirectory(string oldPath, string newPath)
		{
			oldPath = ConvertSimulatedPackagePathToNormalPath(oldPath);
			if (!IsSecureWritePath(oldPath))
			{
				throw new Exception("Attempted to move directory from non-secure path " + oldPath);
			}
			newPath = ConvertSimulatedPackagePathToNormalPath(newPath);
			if (!IsSecureWritePath(newPath))
			{
				throw new Exception("Attempted to move directory to non-secure path " + newPath);
			}
			Directory.Move(oldPath, newPath);
		}

		public static void MoveDirectoryFromPlugin(string oldPath, string newPath, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			oldPath = ConvertSimulatedPackagePathToNormalPath(oldPath);
			if (!IsSecurePluginWritePath(oldPath))
			{
				Exception ex = new Exception("Plugin attempted to move directory from non-secure path " + oldPath);
				if (exceptionCallback != null)
				{
					exceptionCallback(ex);
					return;
				}
				throw ex;
			}
			newPath = ConvertSimulatedPackagePathToNormalPath(newPath);
			if (!IsSecurePluginWritePath(newPath))
			{
				Exception ex2 = new Exception("Plugin attempted to move directory to non-secure path " + newPath);
				if (exceptionCallback != null)
				{
					exceptionCallback(ex2);
					return;
				}
				throw ex2;
			}
			if (!IsPluginWritePathThatNeedsConfirm(oldPath) && !IsPluginWritePathThatNeedsConfirm(newPath))
			{
				try
				{
					Directory.Move(oldPath, newPath);
				}
				catch (Exception ex3)
				{
					if (exceptionCallback != null)
					{
						exceptionCallback(ex3);
						return;
					}
					throw ex3;
				}
				if (confirmCallback != null)
				{
					confirmCallback();
				}
				return;
			}
			ConfirmPluginActionWithUser("move directory from " + oldPath + " to " + newPath, delegate
			{
				try
				{
					Directory.Move(oldPath, newPath);
				}
				catch (Exception e)
				{
					if (exceptionCallback != null)
					{
						exceptionCallback(e);
					}
					return;
				}
				if (confirmCallback != null)
				{
					confirmCallback();
				}
			}, denyCallback);
		}

		public static FileEntryStream OpenStream(FileEntry fe)
		{
			if (fe == null)
			{
				throw new Exception("Null FileEntry passed to OpenStreamReader");
			}
			if (fe is VarFileEntry)
			{
				return new VarFileEntryStream(fe as VarFileEntry);
			}
			if (fe is SystemFileEntry)
			{
				return new SystemFileEntryStream(fe as SystemFileEntry);
			}
			throw new Exception("Unknown FileEntry class passed to OpenStreamReader");
		}

		public static FileEntryStream OpenStream(string path, bool restrictPath = false)
		{
			FileEntry fileEntry = GetFileEntry(path, restrictPath);
			if (fileEntry == null)
			{
				throw new Exception("Path " + path + " not found");
			}
			return OpenStream(fileEntry);
		}

		public static FileEntryStreamReader OpenStreamReader(FileEntry fe)
		{
			if (fe == null)
			{
				throw new Exception("Null FileEntry passed to OpenStreamReader");
			}
			if (fe is VarFileEntry)
			{
				return new VarFileEntryStreamReader(fe as VarFileEntry);
			}
			if (fe is SystemFileEntry)
			{
				return new SystemFileEntryStreamReader(fe as SystemFileEntry);
			}
			throw new Exception("Unknown FileEntry class passed to OpenStreamReader");
		}

		public static FileEntryStreamReader OpenStreamReader(string path, bool restrictPath = false)
		{
			FileEntry fileEntry = GetFileEntry(path, restrictPath);
			if (fileEntry == null)
			{
				throw new Exception("Path " + path + " not found");
			}
			return OpenStreamReader(fileEntry);
		}

		public static IEnumerator ReadAllBytesCoroutine(FileEntry fe, byte[] result)
		{
			Thread loadThread = new Thread((ThreadStart)delegate
			{
				byte[] buffer = new byte[32768];
				using (FileEntryStream fileEntryStream = OpenStream(fe))
				{
					using (MemoryStream destination = new MemoryStream(result))
					{
						StreamUtils.Copy(fileEntryStream.Stream, destination, buffer);
					}
				}
			});
			loadThread.Start();
			while (loadThread.IsAlive)
			{
				yield return null;
			}
		}

		public static byte[] ReadAllBytes(string path, bool restrictPath = false)
		{
			FileEntry fileEntry = GetFileEntry(path, restrictPath);
			if (fileEntry == null)
			{
				throw new Exception("Path " + path + " not found");
			}
			return ReadAllBytes(fileEntry);
		}

		public static byte[] ReadAllBytes(FileEntry fe)
		{
			if (fe is VarFileEntry)
			{
				byte[] buffer = new byte[32768];
				using (FileEntryStream fileEntryStream = OpenStream(fe))
				{
					byte[] array = new byte[fe.Size];
					using (MemoryStream destination = new MemoryStream(array))
					{
						StreamUtils.Copy(fileEntryStream.Stream, destination, buffer);
					}
					return array;
				}
			}
			return File.ReadAllBytes(fe.FullPath);
		}

		public static string ReadAllText(string path, bool restrictPath = false)
		{
			FileEntry fileEntry = GetFileEntry(path, restrictPath);
			if (fileEntry == null)
			{
				throw new Exception("Path " + path + " not found");
			}
			return ReadAllText(fileEntry);
		}

		public static string ReadAllText(FileEntry fe)
		{
			using (FileEntryStreamReader fileEntryStreamReader = OpenStreamReader(fe))
			{
				return fileEntryStreamReader.ReadToEnd();
			}
		}

		public static FileStream OpenStreamForCreate(string path)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsSecureWritePath(path))
			{
				throw new Exception("Attempted to open stream for create at non-secure path " + path);
			}
			return File.Open(path, FileMode.Create);
		}

		public static StreamWriter OpenStreamWriter(string path)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsSecureWritePath(path))
			{
				throw new Exception("Attempted to open stream writer at non-secure path " + path);
			}
			return new StreamWriter(path);
		}

		public static void WriteAllText(string path, string text)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsSecureWritePath(path))
			{
				throw new Exception("Attempted to write all text at non-secure path " + path);
			}
			File.WriteAllText(path, text);
		}

		public static void WriteAllTextFromPlugin(string path, string text, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsSecurePluginWritePath(path))
			{
				Exception ex = new Exception("Plugin attempted to write all text at non-secure path " + path);
				if (exceptionCallback != null)
				{
					exceptionCallback(ex);
					return;
				}
				throw ex;
			}
			if (File.Exists(path))
			{
				if (!IsPluginWritePathThatNeedsConfirm(path))
				{
					try
					{
						File.WriteAllText(path, text);
					}
					catch (Exception ex2)
					{
						if (exceptionCallback != null)
						{
							exceptionCallback(ex2);
							return;
						}
						throw ex2;
					}
					if (confirmCallback != null)
					{
						confirmCallback();
					}
					return;
				}
				ConfirmPluginActionWithUser("overwrite file " + path, delegate
				{
					try
					{
						File.WriteAllText(path, text);
					}
					catch (Exception e)
					{
						if (exceptionCallback != null)
						{
							exceptionCallback(e);
						}
						return;
					}
					if (confirmCallback != null)
					{
						confirmCallback();
					}
				}, denyCallback);
				return;
			}
			try
			{
				File.WriteAllText(path, text);
			}
			catch (Exception ex3)
			{
				if (exceptionCallback != null)
				{
					exceptionCallback(ex3);
					return;
				}
				throw ex3;
			}
			if (confirmCallback != null)
			{
				confirmCallback();
			}
		}

		public static void WriteAllBytes(string path, byte[] bytes)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsSecureWritePath(path))
			{
				throw new Exception("Attempted to write all bytes at non-secure path " + path);
			}
			File.WriteAllBytes(path, bytes);
		}

		public static void WriteAllBytesFromPlugin(string path, byte[] bytes, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsSecurePluginWritePath(path))
			{
				Exception ex = new Exception("Plugin attempted to write all bytes at non-secure path " + path);
				if (exceptionCallback != null)
				{
					exceptionCallback(ex);
					return;
				}
				throw ex;
			}
			if (File.Exists(path))
			{
				if (!IsPluginWritePathThatNeedsConfirm(path))
				{
					try
					{
						File.WriteAllBytes(path, bytes);
					}
					catch (Exception ex2)
					{
						if (exceptionCallback != null)
						{
							exceptionCallback(ex2);
							return;
						}
						throw ex2;
					}
					if (confirmCallback != null)
					{
						confirmCallback();
					}
					return;
				}
				ConfirmPluginActionWithUser("overwrite file " + path, delegate
				{
					try
					{
						File.WriteAllBytes(path, bytes);
					}
					catch (Exception e)
					{
						if (exceptionCallback != null)
						{
							exceptionCallback(e);
						}
						return;
					}
					if (confirmCallback != null)
					{
						confirmCallback();
					}
				}, denyCallback);
				return;
			}
			try
			{
				File.WriteAllBytes(path, bytes);
			}
			catch (Exception ex3)
			{
				if (exceptionCallback != null)
				{
					exceptionCallback(ex3);
					return;
				}
				throw ex3;
			}
			if (confirmCallback != null)
			{
				confirmCallback();
			}
		}

		public static void SetFileAttributes(string path, FileAttributes attrs)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsSecureWritePath(path))
			{
				throw new Exception("Attempted to set file attributes at non-secure path " + path);
			}
			File.SetAttributes(path, attrs);
		}

		public static void DeleteFile(string path)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (File.Exists(path))
			{
				if (!IsSecureWritePath(path))
				{
					throw new Exception("Attempted to delete file at non-secure path " + path);
				}
				File.Delete(path);
			}
		}

		public static void DeleteFileFromPlugin(string path, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!File.Exists(path))
			{
				return;
			}
			if (!IsSecurePluginWritePath(path))
			{
				Exception ex = new Exception("Plugin attempted to delete file at non-secure path " + path);
				if (exceptionCallback != null)
				{
					exceptionCallback(ex);
					return;
				}
				throw ex;
			}
			if (!IsPluginWritePathThatNeedsConfirm(path))
			{
				try
				{
					File.Delete(path);
				}
				catch (Exception ex2)
				{
					if (exceptionCallback != null)
					{
						exceptionCallback(ex2);
						return;
					}
					throw ex2;
				}
				if (confirmCallback != null)
				{
					confirmCallback();
				}
				return;
			}
			ConfirmPluginActionWithUser("delete file " + path, delegate
			{
				try
				{
					File.Delete(path);
				}
				catch (Exception e)
				{
					if (exceptionCallback != null)
					{
						exceptionCallback(e);
					}
					return;
				}
				if (confirmCallback != null)
				{
					confirmCallback();
				}
			}, denyCallback);
		}

		protected static void DoFileCopy(string oldPath, string newPath)
		{
			FileEntry fileEntry = GetFileEntry(oldPath);
			if (fileEntry != null && fileEntry is VarFileEntry)
			{
				byte[] buffer = new byte[4096];
				using (FileEntryStream fileEntryStream = OpenStream(fileEntry))
				{
					using (FileStream destination = OpenStreamForCreate(newPath))
					{
						StreamUtils.Copy(fileEntryStream.Stream, destination, buffer);
						return;
					}
				}
			}
			File.Copy(oldPath, newPath);
		}

		public static void CopyFile(string oldPath, string newPath, bool restrictPath = false)
		{
			oldPath = ConvertSimulatedPackagePathToNormalPath(oldPath);
			if (restrictPath && !IsSecureReadPath(oldPath))
			{
				throw new Exception("Attempted to copy file from non-secure path " + oldPath);
			}
			newPath = ConvertSimulatedPackagePathToNormalPath(newPath);
			if (!IsSecureWritePath(newPath))
			{
				throw new Exception("Attempted to copy file to non-secure path " + newPath);
			}
			DoFileCopy(oldPath, newPath);
		}

		public static void CopyFileFromPlugin(string oldPath, string newPath, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			oldPath = ConvertSimulatedPackagePathToNormalPath(oldPath);
			if (!IsSecureReadPath(oldPath))
			{
				Exception ex = new Exception("Attempted to copy file from non-secure path " + oldPath);
				if (exceptionCallback != null)
				{
					exceptionCallback(ex);
					return;
				}
				throw ex;
			}
			newPath = ConvertSimulatedPackagePathToNormalPath(newPath);
			if (!IsSecurePluginWritePath(newPath))
			{
				Exception ex2 = new Exception("Plugin attempted to copy file to non-secure path " + newPath);
				if (exceptionCallback != null)
				{
					exceptionCallback(ex2);
					return;
				}
				throw ex2;
			}
			if (File.Exists(newPath))
			{
				if (!IsPluginWritePathThatNeedsConfirm(newPath))
				{
					try
					{
						DoFileCopy(oldPath, newPath);
					}
					catch (Exception ex3)
					{
						if (exceptionCallback != null)
						{
							exceptionCallback(ex3);
							return;
						}
						throw ex3;
					}
					if (confirmCallback != null)
					{
						confirmCallback();
					}
					return;
				}
				ConfirmPluginActionWithUser("copy file from " + oldPath + " to existing file " + newPath, delegate
				{
					try
					{
						DoFileCopy(oldPath, newPath);
					}
					catch (Exception e)
					{
						if (exceptionCallback != null)
						{
							exceptionCallback(e);
						}
						return;
					}
					if (confirmCallback != null)
					{
						confirmCallback();
					}
				}, denyCallback);
				return;
			}
			try
			{
				DoFileCopy(oldPath, newPath);
			}
			catch (Exception ex4)
			{
				if (exceptionCallback != null)
				{
					exceptionCallback(ex4);
					return;
				}
				throw ex4;
			}
			if (confirmCallback != null)
			{
				confirmCallback();
			}
		}

		protected static void DoFileMove(string oldPath, string newPath, bool overwrite = true)
		{
			if (File.Exists(newPath))
			{
				if (!overwrite)
				{
					throw new Exception("File " + newPath + " exists. Cannot move into");
				}
				File.Delete(newPath);
			}
			File.Move(oldPath, newPath);
		}

		public static void MoveFile(string oldPath, string newPath, bool overwrite = true)
		{
			oldPath = ConvertSimulatedPackagePathToNormalPath(oldPath);
			if (!IsSecureWritePath(oldPath))
			{
				throw new Exception("Attempted to move file from non-secure path " + oldPath);
			}
			newPath = ConvertSimulatedPackagePathToNormalPath(newPath);
			if (!IsSecureWritePath(newPath))
			{
				throw new Exception("Attempted to move file to non-secure path " + newPath);
			}
			DoFileMove(oldPath, newPath, overwrite);
		}

		public static void MoveFileFromPlugin(string oldPath, string newPath, bool overwrite, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
		{
			oldPath = ConvertSimulatedPackagePathToNormalPath(oldPath);
			if (!IsSecurePluginWritePath(oldPath))
			{
				Exception ex = new Exception("Plugin attempted to move file from non-secure path " + oldPath);
				if (exceptionCallback != null)
				{
					exceptionCallback(ex);
					return;
				}
				throw ex;
			}
			newPath = ConvertSimulatedPackagePathToNormalPath(newPath);
			if (!IsSecurePluginWritePath(newPath))
			{
				Exception ex2 = new Exception("Plugin attempted to move file to non-secure path " + newPath);
				if (exceptionCallback != null)
				{
					exceptionCallback(ex2);
					return;
				}
				throw ex2;
			}
			if (!IsPluginWritePathThatNeedsConfirm(oldPath) && !IsPluginWritePathThatNeedsConfirm(newPath))
			{
				try
				{
					DoFileMove(oldPath, newPath, overwrite);
				}
				catch (Exception ex3)
				{
					if (exceptionCallback != null)
					{
						exceptionCallback(ex3);
						return;
					}
					throw ex3;
				}
				if (confirmCallback != null)
				{
					confirmCallback();
				}
				return;
			}
			ConfirmPluginActionWithUser("move file from " + oldPath + " to " + newPath, delegate
			{
				try
				{
					DoFileMove(oldPath, newPath, overwrite);
				}
				catch (Exception e)
				{
					if (exceptionCallback != null)
					{
						exceptionCallback(e);
					}
					return;
				}
				if (confirmCallback != null)
				{
					confirmCallback();
				}
			}, denyCallback);
		}

		private void Awake()
		{
			singleton = this;
		}

		private void OnDestroy()
		{
			ClearAll();
		}
	}
}
