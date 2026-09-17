using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DynamicCSharp;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using Mono.CSharp;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class MVRPluginManager : JSONStorable
{
	public Transform pluginPanelPrefab;

	public Transform scriptControllerPanelPrefab;

	public Transform scriptUIPrefab;

	public Transform configurableSliderPrefab;

	public Transform configurableTogglePrefab;

	public Transform configurableColorPickerPrefab;

	public Transform configurableButtonPrefab;

	public Transform configurablePopupPrefab;

	public Transform configurableScrollablePopupPrefab;

	public Transform configurableFilterablePopupPrefab;

	public Transform configurableTextFieldPrefab;

	public Transform configurableSpacerPrefab;

	public Transform pluginListPanel;

	public RectTransform scriptUIParent;

	public Transform pluginContainer;

	protected List<MVRPlugin> plugins;

	protected Dictionary<string, bool> pluginUIDs;

	protected JSONStorableAction CreatePluginAction;

	protected Color pluginPanelErrorColor = new Color(1f, 0.5f, 0.5f);

	protected Color pluginPanelWarningColor = new Color(1f, 0.95f, 0.5f);

	protected Color pluginPanelValidColor = Color.gray;

	protected ScriptDomain domain;

	protected MD5CryptoServiceProvider md5;

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (((includeAppearance && includePhysical) || forceStore) && (plugins.Count > 0 || forceStore))
		{
			needsStore = true;
			JSONClass jc = (JSONClass)(jSON["plugins"] = new JSONClass());
			foreach (MVRPlugin plugin in plugins)
			{
				plugin.pluginURLJSON.StoreJSON(jc, includePhysical, includeAppearance, forceStore);
			}
		}
		return jSON;
	}

	public override void PreRestore(bool restorePhysical, bool restoreAppearance)
	{
		if (restorePhysical && restoreAppearance && !base.mergeRestore && !base.physicalLocked && !base.appearanceLocked && !IsCustomPhysicalParamLocked("plugins") && !IsCustomAppearanceParamLocked("plugins"))
		{
			RemoveAllPlugins();
		}
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		base.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
		insideRestore = true;
		if (!base.physicalLocked && !base.appearanceLocked && restoreAppearance && restorePhysical && !IsCustomPhysicalParamLocked("plugins") && !IsCustomAppearanceParamLocked("plugins"))
		{
			if (jc["plugins"] != null)
			{
				if (base.mergeRestore)
				{
					GivePluginsNewUIDs(plugins);
				}
				else
				{
					RemoveAllPlugins();
				}
				JSONClass asObject = jc["plugins"].AsObject;
				List<MVRPlugin> list = new List<MVRPlugin>();
				if (asObject != null)
				{
					foreach (string key in asObject.Keys)
					{
						MVRPlugin mVRPlugin = CreatePluginWithId(key);
						pluginUIDs.Add(key, true);
						list.Add(mVRPlugin);
						mVRPlugin.pluginURLJSON.RestoreFromJSON(asObject);
					}
				}
				if (base.mergeRestore)
				{
					GivePluginsNewUIDs(list);
					GivePluginsNewUIDs(plugins, false);
				}
			}
			else if (setMissingToDefault && !base.mergeRestore)
			{
				RemoveAllPlugins();
			}
		}
		insideRestore = false;
	}

	protected string CreatePluginUID(bool isTempName = false)
	{
		string text = ((!isTempName) ? "plugin#" : "pluginTemp#");
		for (int i = 0; i < 1000; i++)
		{
			string text2 = text + i;
			if (!pluginUIDs.ContainsKey(text2))
			{
				text = text2;
				pluginUIDs.Add(text, true);
				break;
			}
		}
		return text;
	}

	protected void BeginBrowse(JSONStorableUrl jsurl)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory("Custom/Scripts", true, true, true, true);
		List<ShortCut> list = new List<ShortCut>();
		ShortCut shortCut = new ShortCut();
		shortCut.displayName = "Root";
		shortCut.path = Path.GetFullPath(".");
		list.Add(shortCut);
		foreach (ShortCut item in shortCutsForDirectory)
		{
			VarPackage package = FileManager.GetPackage(item.package);
			if (package != null)
			{
				if (!package.PluginsAlwaysDisabled)
				{
					list.Add(item);
				}
			}
			else
			{
				list.Add(item);
			}
		}
		jsurl.shortCuts = list;
	}

	protected MVRPlugin CreatePluginWithId(string pluginUID)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		MVRPlugin mvrp = new MVRPlugin();
		mvrp.uid = pluginUID;
		plugins.Add(mvrp);
		JSONStorableUrl jSONStorableUrl = new JSONStorableUrl(pluginUID, string.Empty, (JSONStorableString.SetStringCallback)delegate
		{
			SyncPluginUrl(mvrp);
		}, "cs|cslist|dll", "Custom/Scripts");
		mvrp.pluginURLJSON = jSONStorableUrl;
		jSONStorableUrl.beginBrowseWithObjectCallback = BeginBrowse;
		if (pluginPanelPrefab != null)
		{
			Transform transform = UnityEngine.Object.Instantiate(pluginPanelPrefab);
			if (pluginListPanel != null)
			{
				transform.SetParent(pluginListPanel, false);
			}
			else
			{
				transform.gameObject.SetActive(false);
			}
			mvrp.configUI = transform;
			MVRPluginUI component = transform.GetComponent<MVRPluginUI>();
			if (component != null)
			{
				mvrp.scriptControllerContent = component.scriptControllerContent;
				jSONStorableUrl.fileBrowseButton = component.fileBrowseButton;
				jSONStorableUrl.clearButton = component.clearButton;
				jSONStorableUrl.reloadButton = component.reloadButton;
				jSONStorableUrl.text = component.urlText;
				if (component.uidText != null)
				{
					component.uidText.text = pluginUID;
				}
				if (component.removeButton != null)
				{
					component.removeButton.onClick.AddListener(delegate
					{
						RemovePlugin(mvrp);
					});
				}
			}
		}
		return mvrp;
	}

	protected void GivePluginNewUID(MVRPlugin mvrp, bool isTempName = true)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		string uid = mvrp.uid;
		string text = (mvrp.uid = CreatePluginUID(isTempName));
		if (mvrp.pluginURLJSON != null)
		{
			mvrp.pluginURLJSON.name = text;
		}
		if (mvrp.configUI != null)
		{
			MVRPluginUI component = mvrp.configUI.GetComponent<MVRPluginUI>();
			if (component != null && component.uidText != null)
			{
				component.uidText.text = text;
			}
		}
		foreach (MVRScriptController scriptController in mvrp.scriptControllers)
		{
			if (scriptController.script != null)
			{
				containingAtom.UnregisterAdditionalStorable(scriptController.script);
			}
			if (scriptController.gameObject != null)
			{
				scriptController.gameObject.name = scriptController.gameObject.name.Replace(uid, text);
				if (scriptController.configUI != null)
				{
					MVRScriptControllerUI component2 = scriptController.configUI.GetComponent<MVRScriptControllerUI>();
					if (component2 != null && component2.label != null)
					{
						component2.label.text = scriptController.gameObject.name;
					}
				}
			}
			if (scriptController.script != null)
			{
				containingAtom.RegisterAdditionalStorable(scriptController.script);
			}
		}
		pluginUIDs.Remove(uid);
	}

	protected void GivePluginsNewUIDs(List<MVRPlugin> pluginsList, bool isTemp = true)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		foreach (MVRPlugin plugins in pluginsList)
		{
			GivePluginNewUID(plugins, isTemp);
		}
	}

	public MVRPlugin CreatePlugin()
	{
		string pluginUID = CreatePluginUID();
		return CreatePluginWithId(pluginUID);
	}

	public void ReloadPluginWithUID(string uid)
	{
		if (plugins == null)
		{
			return;
		}
		foreach (MVRPlugin plugin in plugins)
		{
			if (plugin.uid == uid)
			{
				plugin.Reload();
			}
		}
	}

	protected void CreatePluginCallback()
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		CreatePlugin();
	}

	protected void DestroyScriptController(MVRScriptController mvrsc)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		Exception ex = null;
		if (mvrsc.script != null)
		{
			if (mvrsc.script.enabledJSON != null)
			{
				mvrsc.script.enabledJSON.toggle = null;
			}
			if (mvrsc.script.pluginLabelJSON != null)
			{
				mvrsc.script.pluginLabelJSON.inputField = null;
				mvrsc.script.pluginLabelJSON.inputFieldAction = null;
			}
			try
			{
				UnityEngine.Object.DestroyImmediate(mvrsc.script);
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
			mvrsc.script = null;
		}
		if (mvrsc.configUI != null)
		{
			UnityEngine.Object.Destroy(mvrsc.configUI.gameObject);
			mvrsc.configUI = null;
		}
		if (mvrsc.customUI != null)
		{
			Canvas componentInChildren = mvrsc.customUI.GetComponentInChildren<Canvas>();
			if (componentInChildren != null)
			{
				SuperController.singleton.RemoveCanvas(componentInChildren);
			}
			UnityEngine.Object.Destroy(mvrsc.customUI.gameObject);
			mvrsc.customUI = null;
		}
		if (mvrsc.gameObject != null)
		{
			UnityEngine.Object.Destroy(mvrsc.gameObject);
			mvrsc.gameObject = null;
		}
		if (ex != null)
		{
			throw ex;
		}
	}

	protected void RemovePluginScriptControllers(MVRPlugin mvrp)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		Exception ex = null;
		if (mvrp.scriptControllers != null)
		{
			foreach (MVRScriptController scriptController in mvrp.scriptControllers)
			{
				if (scriptController.script != null)
				{
					containingAtom.UnregisterAdditionalStorable(scriptController.script);
				}
				try
				{
					DestroyScriptController(scriptController);
				}
				catch (Exception ex2)
				{
					ex = ex2;
				}
			}
		}
		mvrp.scriptControllers = new List<MVRScriptController>();
		if (ex != null)
		{
			throw ex;
		}
	}

	protected void RemovePlugin(MVRPlugin mvrp)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		try
		{
			RemovePluginScriptControllers(mvrp);
			if (mvrp.configUI != null)
			{
				UnityEngine.Object.Destroy(mvrp.configUI.gameObject);
				mvrp.configUI = null;
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception while trying to remove plugin " + mvrp.pluginURLJSON.val + ": " + ex);
		}
		pluginUIDs.Remove(mvrp.uid);
		plugins.Remove(mvrp);
	}

	public void RemovePluginWithUID(string uid)
	{
		if (plugins == null)
		{
			return;
		}
		MVRPlugin mVRPlugin = null;
		foreach (MVRPlugin plugin in plugins)
		{
			if (plugin.uid == uid)
			{
				mVRPlugin = plugin;
				break;
			}
		}
		if (mVRPlugin != null)
		{
			RemovePlugin(mVRPlugin);
		}
	}

	public void RemoveAllPlugins()
	{
		List<MVRPlugin> list = new List<MVRPlugin>(plugins);
		foreach (MVRPlugin item in list)
		{
			RemovePlugin(item);
		}
	}

	protected MVRScriptController CreateScriptController(MVRPlugin mvrp, ScriptType type)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		MVRScriptController mVRScriptController = new MVRScriptController();
		GameObject gameObject = new GameObject(mvrp.uid + "temp");
		gameObject.transform.SetParent(pluginContainer);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		mVRScriptController.gameObject = gameObject;
		ScriptProxy scriptProxy = null;
		try
		{
			scriptProxy = type.CreateInstance(gameObject);
		}
		catch (Exception createException)
		{
			// The component the constructor was building is on the object already, and Unity goes on
			// calling Update on it, so a plugin that throws here reports its own exception on every
			// frame afterwards. Destroying the object stops it at the point it failed.
			ReportPluginFailure(mvrp, "Exception while creating plugin " + mvrp.pluginURLJSON.val, createException);
			UnityEngine.Object.Destroy(gameObject);
			return null;
		}
		if (scriptProxy == null)
		{
			SuperController.LogError("Failed to create instance of " + mvrp.pluginURLJSON.val);
			UnityEngine.Object.Destroy(gameObject);
			return null;
		}
		string text2 = (gameObject.name = mvrp.uid + "_" + scriptProxy.GetInstanceType().ToString());
		if (scriptUIPrefab != null)
		{
			Transform transform = UnityEngine.Object.Instantiate(scriptUIPrefab);
			if (scriptUIParent != null)
			{
				transform.SetParent(scriptUIParent, false);
			}
			transform.gameObject.SetActive(false);
			mVRScriptController.customUI = transform;
		}
		Toggle toggle = null;
		InputField inputField = null;
		InputFieldAction inputFieldAction = null;
		if (scriptControllerPanelPrefab != null)
		{
			Transform transform2 = UnityEngine.Object.Instantiate(scriptControllerPanelPrefab);
			if (mvrp.scriptControllerContent != null)
			{
				transform2.SetParent(mvrp.scriptControllerContent, false);
			}
			MVRScriptControllerUI component = transform2.GetComponent<MVRScriptControllerUI>();
			if (component != null)
			{
				if (component.label != null)
				{
					component.label.text = text2;
				}
				if (component.openUIButton != null)
				{
					component.openUIButton.onClick.AddListener(mVRScriptController.OpenUI);
				}
				toggle = component.enabledToggle;
				inputField = component.userLabelInputField;
				inputFieldAction = component.userLabelInputFieldAction;
			}
			mVRScriptController.configUI = transform2;
		}
		MVRScript component2 = gameObject.GetComponent<MVRScript>();
		component2.exclude = exclude;
		mVRScriptController.script = component2;
		if (component2.ShouldIgnore())
		{
			try
			{
				DestroyScriptController(mVRScriptController);
			}
			catch
			{
			}
			mVRScriptController = null;
		}
		else
		{
			try
			{
				component2.ForceAwake();
				component2.containingAtom = containingAtom;
				component2.manager = this;
				try
				{
					component2.Init();
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception during plugin script Init: " + ex);
					// A plugin that throws its way out of Init leaves its own state half built, and
					// Unity goes on calling Update on the component - the breathing plugin then
					// reports a NullReferenceException for every frame the atom is loaded. Turning
					// the component off stops the loop; the script panel and its toggle stay where
					// they are, so a plugin that is worth retrying can still be switched back on.
					component2.enabled = false;
					ReportPluginFailure(mvrp, "Plugin " + mvrp.pluginURLJSON.val + " failed to initialize", ex);
				}
				if (component2.enabledJSON != null)
				{
					component2.enabledJSON.toggle = toggle;
				}
				if (component2.pluginLabelJSON != null)
				{
					component2.pluginLabelJSON.inputField = inputField;
					component2.pluginLabelJSON.inputFieldAction = inputFieldAction;
				}
				if (mVRScriptController.customUI != null)
				{
					component2.UITransform = mVRScriptController.customUI;
					component2.InitUI();
					MVRScriptUI componentInChildren = mVRScriptController.customUI.GetComponentInChildren<MVRScriptUI>();
					if (componentInChildren != null && componentInChildren.closeButton != null)
					{
						componentInChildren.closeButton.onClick.AddListener(mVRScriptController.CloseUI);
					}
					Canvas componentInChildren2 = mVRScriptController.customUI.GetComponentInChildren<Canvas>();
					if (componentInChildren2 != null)
					{
						SuperController.singleton.AddCanvas(componentInChildren2);
					}
				}
				containingAtom.RegisterAdditionalStorable(component2);
				if (insideRestore)
				{
					containingAtom.RestoreFromLast(component2);
				}
			}
			catch (Exception innerException)
			{
				DestroyScriptController(mVRScriptController);
				mVRScriptController = null;
				throw new Exception("Exception during script init ", innerException);
			}
		}
		return mVRScriptController;
	}

	protected void UserConfirmDenyComplete(MVRPlugin mvrp, bool didConfirm)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		insideRestore = true;
		SyncPluginUrlInternal(mvrp, true);
		insideRestore = false;
	}

	protected void SyncPluginUrl(MVRPlugin mvrp)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		mvrp.SetupUserConfirmDeny(UserConfirmDenyComplete);
		SyncPluginUrlInternal(mvrp, false);
	}

	protected void SetPluginPanelColor(MVRPlugin mvrp, Color c)
	{
		if (mvrp.configUI != null)
		{
			Image component = mvrp.configUI.GetComponent<Image>();
			if (component != null)
			{
				component.color = c;
			}
		}
	}

	protected void SetPluginPanelErrorColor(MVRPlugin mvrp)
	{
		SetPluginPanelColor(mvrp, pluginPanelErrorColor);
	}

	protected void SetPluginPanelWarningColor(MVRPlugin mvrp)
	{
		SetPluginPanelColor(mvrp, pluginPanelWarningColor);
	}

	protected void SetPluginPanelValidColor(MVRPlugin mvrp)
	{
		SetPluginPanelColor(mvrp, pluginPanelValidColor);
	}

	protected void AlertUserToNetworkRestriction(string url)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		SuperController.AlertUser("Load of plugin\n\n" + url + "\n\nfailed due to this plugin requiring networking and 'Allow Plugins Network Access' User Preference being set to off.\n\nClick OK to open the User Preference panel if you want to change this setting to be on. You will need to reload the plugin and/or the scene after changing this setting.\n\nClick Cancel to continue without making changes to your User Preferences.", delegate
		{
			SuperController.singleton.DeactivateWorldUI();
			SuperController.singleton.activeUI = SuperController.ActiveUI.MainMenu;
			SuperController.singleton.SetMainMenuTab("TabUserPrefs");
			SuperController.singleton.SetUserPrefsTab("TabSecurity");
		}, null);
	}

	private static readonly HashSet<string> reportedPluginFailures = new HashSet<string>();

	/// <summary>
	/// Puts a plugin failure in front of the user, once per plugin.
	///
	/// A scene that asks for a plugin which is not installed, or one whose script throws while it is
	/// being created, leaves something on screen that does not work. The log says so and the plugin
	/// panel turns red, but neither is where the user is looking, and a plugin whose constructor
	/// throws keeps throwing - one line per frame for as long as the object lives. The first failure
	/// of a plugin is therefore shown in a dialog, and the log keeps the rest.
	/// </summary>
	protected static void ReportPluginFailure(MVRPlugin mvrp, string message, Exception failure)
	{
		string label = ((mvrp != null && mvrp.pluginURLJSON != null) ? mvrp.pluginURLJSON.val : "unknown");
		string text = message + ((failure != null) ? (" [" + failure.GetType().Name + ": " + failure.Message + "]") : string.Empty);
		SuperController.LogError(text);
		if (!reportedPluginFailures.Add(label))
		{
			return;
		}
		SuperController.AlertUser(text, new UnityEngine.Events.UnityAction(delegate
		{
		}));
	}

	protected void SyncPluginUrlInternal(MVRPlugin mvrp, bool isFromConfirmDenyResponse)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		if (UserPreferences.singleton == null || UserPreferences.singleton.enablePlugins)
		{
			if (domain == null)
			{
				domain = ScriptDomain.CreateDomain("MVRPlugins", true);
				IEnumerable<string> resolvedVersionDefines = SuperController.singleton.GetResolvedVersionDefines();
				if (resolvedVersionDefines != null)
				{
					foreach (string item in resolvedVersionDefines)
					{
						domain.CompilerService.AddConditionalSymbol(item);
					}
				}
			}
			RemovePluginScriptControllers(mvrp);
			bool flag = false;
			string text = null;
			string pluginHash = FileManager.GetPluginHash();
			if (pluginHash != null)
			{
				text = FileManager.GetPluginHashDisplayName();
				flag = FileManager.IsFileInPackage(text);
			}
			string val = mvrp.pluginURLJSON.val;
			if (domain == null || !(pluginContainer != null) || val == null || !(val != string.Empty))
			{
				return;
			}
			if (FileManager.FileExists(val))
			{
				try
				{
					Location.Reset();
					string text2 = val.Replace("/", "_");
					text2 = text2.Replace("\\", "_");
					text2 = text2.Replace(".", "_");
					text2 = text2.Replace(":", "_");
					if (val.EndsWith(".cslist") || val.EndsWith(".dll"))
					{
						SetPluginPanelValidColor(mvrp);
						ScriptAssembly scriptAssembly = null;
						if (val.EndsWith(".cslist"))
						{
							string directoryName = FileManager.GetDirectoryName(val);
							List<string> list = new List<string>();
							bool flag2 = false;
							HashSet<string> hashSet = new HashSet<string>();
							using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(val, true))
							{
								StreamReader streamReader = fileEntryStreamReader.StreamReader;
								string text3;
								while ((text3 = streamReader.ReadLine()) != null)
								{
									string text4 = text3.Trim();
									if (directoryName != string.Empty && !text4.Contains(":/"))
									{
										text4 = directoryName + "/" + text4;
									}
									if (text4 != string.Empty)
									{
										text4 = text4.Replace('/', '\\');
										if (FileManager.IsFileInPackage(text4))
										{
											VarFileEntry varFileEntry = FileManager.GetVarFileEntry(text4);
											if (varFileEntry != null)
											{
												flag2 = true;
												VarPackage package = varFileEntry.Package;
												if (mvrp.IsVarPackageConfirmed(package))
												{
													list.Add(text4);
												}
												else if (package.PluginsAlwaysDisabled)
												{
													SetPluginPanelWarningColor(mvrp);
													hashSet.Add(package.Uid);
												}
												else if (!isFromConfirmDenyResponse)
												{
													mvrp.AddRequestedPackage(package);
												}
												else
												{
													SetPluginPanelWarningColor(mvrp);
												}
											}
										}
										else
										{
											if (flag)
											{
												SuperController.LogError("Plugin inside a package " + text + " tried to load another plugin from outside of a package at path " + text4 + " which is no longer allowed");
												RemovePlugin(mvrp);
												return;
											}
											list.Add(text4);
										}
									}
								}
							}
							if (!isFromConfirmDenyResponse && mvrp.HasRequestedPackages)
							{
								mvrp.UserConfirm();
								return;
							}
							if (hashSet.Count > 0)
							{
								string[] array = new string[hashSet.Count];
								hashSet.CopyTo(array);
								string text5 = string.Join(" ", array);
								SuperController.LogMessage("Plugin scripts from packages " + text5 + " were not loaded because they are in packages that have always been denied plugin loading. Go to Package Manager, open the package, and click User Prefs tab to change", true, true);
							}
							if (list.Count <= 0)
							{
								return;
							}
							try
							{
								bool flag3 = false;
								List<string> list2 = new List<string>();
								StringBuilder stringBuilder = new StringBuilder();
								foreach (string item2 in list)
								{
									if (!item2.EndsWith(".cs"))
									{
										SuperController.LogError("File " + val + " in csfile " + item2 + " must be a .cs file");
										SetPluginPanelErrorColor(mvrp);
										return;
									}
									string text6 = FileManager.ReadAllText(item2, true);
									stringBuilder.Append(text6);
									if (text6.Contains("UnityEngine.Application.OpenURL") || text6.Contains("Application.OpenURL"))
									{
										text6 = text6.Replace("UnityEngine.Application.OpenURL", "SuperController.OpenURL");
										text6 = text6.Replace("Application.OpenURL", "SuperController.OpenURL");
										flag3 = true;
									}
									list2.Add(text6);
								}
								string text7 = "MVRPlugin_" + text2 + "_" + GetMD5Hash(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
								FileManager.RegisterPluginHashToPluginPath(text7, val);
								domain.CompilerService.SetSuggestedAssemblyNamePrefix(text7);
								scriptAssembly = ((!flag2 && !flag3) ? domain.CompileAndLoadScriptFiles(list.ToArray()) : domain.CompileAndLoadScriptSources(list2.ToArray()));
								if (scriptAssembly == null)
								{
									SuperController.LogError("Compile of " + val + " failed. Errors:");
									string[] errors = domain.CompilerService.Errors;
									foreach (string text8 in errors)
									{
										if (!text8.StartsWith("[CS]"))
										{
											SuperController.LogError(text8 + "\n");
										}
									}
									SetPluginPanelErrorColor(mvrp);
									return;
								}
							}
							catch (Exception ex)
							{
								if (ex.Message.Contains("RuntimeNamespaceRestriction"))
								{
									AlertUserToNetworkRestriction(val);
								}
								SuperController.LogError("Compile of " + val + " failed. Exception: " + ex);
								if (domain.CompilerService.Errors.Length > 0)
								{
									SuperController.LogError("Compile of " + val + " failed. Errors:");
									string[] errors2 = domain.CompilerService.Errors;
									foreach (string err in errors2)
									{
										SuperController.LogError(err);
									}
								}
								SetPluginPanelErrorColor(mvrp);
								return;
							}
						}
						else if (FileManager.IsFileInPackage(val))
						{
							VarFileEntry varFileEntry2 = FileManager.GetVarFileEntry(val);
							if (varFileEntry2 != null)
							{
								VarPackage package2 = varFileEntry2.Package;
								if (!mvrp.IsVarPackageConfirmed(package2))
								{
									if (package2.PluginsAlwaysDisabled)
									{
										SetPluginPanelWarningColor(mvrp);
										SuperController.LogMessage("Plugin " + val + " was not loaded because it is in a package that has always been denied plugin loading. Go to Package Manager, open the package, and click User Prefs tab to change", true, true);
									}
									else if (!isFromConfirmDenyResponse)
									{
										mvrp.AddRequestedPackage(package2);
										mvrp.UserConfirm();
									}
									else
									{
										SetPluginPanelWarningColor(mvrp);
									}
									return;
								}
								byte[] array2 = FileManager.ReadAllBytes(val, true);
								string hash = text2 + "_" + GetMD5Hash(array2);
								FileManager.RegisterPluginHashToPluginPath(hash, val);
								scriptAssembly = domain.LoadAssembly(array2);
								FileManager.RegisterAssemblyToPluginHash(scriptAssembly.RawAssembly, hash);
							}
						}
						else
						{
							if (flag)
							{
								SuperController.LogError("Plugin inside a package " + text + " tried to load another plugin from outside of a package at path " + val + " which is no longer allowed");
								RemovePlugin(mvrp);
								return;
							}
							byte[] bytes = FileManager.ReadAllBytes(val, true);
							string hash2 = text2 + "_" + GetMD5Hash(bytes);
							FileManager.RegisterPluginHashToPluginPath(hash2, val);
							scriptAssembly = domain.LoadAssembly(val);
							FileManager.RegisterAssemblyToPluginHash(scriptAssembly.RawAssembly, hash2);
						}
						if (scriptAssembly != null)
						{
							ScriptType[] array3 = scriptAssembly.FindAllSubtypesOf<MVRScript>();
							if (array3.Length > 0)
							{
								ScriptType[] array4 = array3;
								foreach (ScriptType type in array4)
								{
									MVRScriptController mVRScriptController = CreateScriptController(mvrp, type);
									if (mVRScriptController != null)
									{
										mvrp.scriptControllers.Add(mVRScriptController);
									}
								}
							}
							else
							{
								Debug.LogError("No MVRScript types found");
								SetPluginPanelErrorColor(mvrp);
							}
						}
						else
						{
							SuperController.LogError("Unable to load assembly from " + val);
							SetPluginPanelErrorColor(mvrp);
						}
					}
					else if (val.EndsWith(".cs"))
					{
						ScriptType scriptType = null;
						try
						{
							if (FileManager.IsFileInPackage(val))
							{
								VarFileEntry varFileEntry3 = FileManager.GetVarFileEntry(val);
								if (varFileEntry3 != null)
								{
									VarPackage package3 = varFileEntry3.Package;
									if (!mvrp.IsVarPackageConfirmed(package3))
									{
										if (package3.PluginsAlwaysDisabled)
										{
											SuperController.LogMessage("Plugin " + val + " was not loaded because it is in a package that has always been denied plugin loading. Go to Package Manager, open the package, and click User Prefs tab to change", true, true);
											SetPluginPanelWarningColor(mvrp);
										}
										else if (!isFromConfirmDenyResponse)
										{
											mvrp.AddRequestedPackage(package3);
											mvrp.UserConfirm();
										}
										else
										{
											SetPluginPanelWarningColor(mvrp);
										}
										return;
									}
									byte[] bytes2 = FileManager.ReadAllBytes(val, true);
									string text9 = "MVRPlugin_" + text2 + "_" + GetMD5Hash(bytes2);
									FileManager.RegisterPluginHashToPluginPath(text9, val);
									domain.CompilerService.SetSuggestedAssemblyNamePrefix(text9);
									string text10 = FileManager.ReadAllText(val);
									text10 = text10.Replace("UnityEngine.Application.OpenURL", "SuperController.OpenURL");
									text10 = text10.Replace("Application.OpenURL", "SuperController.OpenURL");
									scriptType = domain.CompileAndLoadScriptSource(text10);
								}
							}
							else
							{
								if (flag)
								{
									SuperController.LogError("Plugin inside a package " + text + " tried to load another plugin from outside of a package at path " + val + " which is no longer allowed");
									RemovePlugin(mvrp);
									return;
								}
								byte[] bytes3 = FileManager.ReadAllBytes(val, true);
								string text11 = "MVRPlugin_" + text2 + "_" + GetMD5Hash(bytes3);
								FileManager.RegisterPluginHashToPluginPath(text11, val);
								domain.CompilerService.SetSuggestedAssemblyNamePrefix(text11);
								string text12 = FileManager.ReadAllText(val);
								if (text12.Contains("UnityEngine.Application.OpenURL") || text12.Contains("Application.OpenURL"))
								{
									text12 = text12.Replace("UnityEngine.Application.OpenURL", "SuperController.OpenURL");
									text12 = text12.Replace("Application.OpenURL", "SuperController.OpenURL");
									scriptType = domain.CompileAndLoadScriptSource(text12);
								}
								else
								{
									scriptType = domain.CompileAndLoadScriptFile(val);
								}
							}
							if (scriptType == null)
							{
								SuperController.LogError("Compile of " + val + " failed. Errors:");
								string[] errors3 = domain.CompilerService.Errors;
								foreach (string err2 in errors3)
								{
									SuperController.LogError(err2);
								}
								SetPluginPanelErrorColor(mvrp);
								return;
							}
						}
						catch (Exception ex2)
						{
							if (ex2.Message.Contains("RuntimeNamespaceRestriction"))
							{
								AlertUserToNetworkRestriction(val);
							}
							SuperController.LogError("Compile of " + val + " failed. Exception: " + ex2);
							if (domain.CompilerService.Errors.Length > 0)
							{
								SuperController.LogError("Compile of " + val + " failed. Errors:");
								string[] errors4 = domain.CompilerService.Errors;
								foreach (string err3 in errors4)
								{
									SuperController.LogError(err3);
								}
							}
							SetPluginPanelErrorColor(mvrp);
							return;
						}
						if (scriptType.IsSubtypeOf<MVRScript>())
						{
							MVRScriptController mVRScriptController2 = CreateScriptController(mvrp, scriptType);
							if (mVRScriptController2 != null)
							{
								mvrp.scriptControllers.Add(mVRScriptController2);
							}
							SetPluginPanelValidColor(mvrp);
						}
						else
						{
							SetPluginPanelErrorColor(mvrp);
							SuperController.LogError("Script loaded at " + val + " must inherit from MVRScript");
						}
					}
					else
					{
						SetPluginPanelErrorColor(mvrp);
						SuperController.LogError("Plugin file " + val + " must be a .cs, .cslist, or .dll file");
					}
					return;
				}
				catch (Exception ex3)
				{
					SetPluginPanelErrorColor(mvrp);
					SuperController.LogError("Exception during compile of " + val + ": " + ex3);
					return;
				}
			}
			SetPluginPanelErrorColor(mvrp);
			ReportPluginFailure(mvrp, "Plugin file " + val + " does not exist", null);
		}
		else
		{
			SuperController.LogError("Attempted to load plugin when plugins option is disabled. To enable, see User Preferences -> Security tab");
			SuperController.singleton.ShowMainHUDAuto();
			SuperController.singleton.SetActiveUI("MainMenu");
			SuperController.singleton.SetMainMenuTab("TabUserPrefs");
			SuperController.singleton.SetUserPrefsTab("TabSecurity");
		}
	}

	public string GetMD5Hash(byte[] bytes)
	{
		if (md5 == null)
		{
			md5 = new MD5CryptoServiceProvider();
		}
		byte[] array = md5.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	protected void Init()
	{
		if (pluginContainer == null)
		{
			pluginContainer = base.transform;
		}
		plugins = new List<MVRPlugin>();
		pluginUIDs = new Dictionary<string, bool>();
		CreatePluginAction = new JSONStorableAction("CreatePlugin", CreatePluginCallback);
		RegisterAction(CreatePluginAction);
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		MVRPluginManagerUI componentInChildren = UITransform.GetComponentInChildren<MVRPluginManagerUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		scriptUIParent = componentInChildren.scriptUIParent;
		pluginListPanel = componentInChildren.pluginListPanel;
		if (pluginListPanel != null)
		{
			foreach (MVRPlugin plugin in plugins)
			{
				if (plugin.configUI != null)
				{
					plugin.configUI.SetParent(pluginListPanel, false);
					plugin.configUI.gameObject.SetActive(true);
				}
				foreach (MVRScriptController scriptController in plugin.scriptControllers)
				{
					if (scriptController.customUI != null && scriptUIParent != null)
					{
						scriptController.customUI.SetParent(scriptUIParent, false);
					}
				}
			}
		}
		CreatePluginAction.button = componentInChildren.addPluginButton;
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}

	protected void OnDestroy()
	{
		foreach (MVRPlugin plugin in plugins)
		{
			foreach (MVRScriptController scriptController in plugin.scriptControllers)
			{
				if (scriptController.customUI != null)
				{
					UnityEngine.Object.Destroy(scriptController.customUI.gameObject);
				}
			}
			if (plugin.configUI != null)
			{
				UnityEngine.Object.Destroy(plugin.configUI.gameObject);
			}
		}
	}
}
