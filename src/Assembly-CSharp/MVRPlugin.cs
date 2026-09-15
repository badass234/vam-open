using System.Collections.Generic;
using MVR.FileManagement;
using UnityEngine;

public class MVRPlugin
{
	public delegate void UserConfirmDenyComplete(MVRPlugin mvrp, bool didConfirm);

	public string uid;

	public JSONStorableUrl pluginURLJSON;

	public List<MVRScriptController> scriptControllers;

	public Transform configUI;

	public RectTransform scriptControllerContent;

	protected HashSet<VarPackage> requestedPackages;

	protected HashSet<VarPackage> confirmedOncePackages;

	protected bool userConfirmedAPackage;

	protected UserConfirmDenyComplete userConfirmDenyCompleteCallback;

	public bool HasRequestedPackages { get; protected set; }

	public MVRPlugin()
	{
		scriptControllers = new List<MVRScriptController>();
		requestedPackages = new HashSet<VarPackage>();
		confirmedOncePackages = new HashSet<VarPackage>();
		userConfirmedAPackage = false;
	}

	public void Reload()
	{
		if (pluginURLJSON != null)
		{
			pluginURLJSON.Reload();
		}
	}

	public void AddRequestedPackage(VarPackage vp)
	{
		HasRequestedPackages = true;
		if (!vp.PluginsAlwaysDisabled)
		{
			requestedPackages.Add(vp);
		}
	}

	public bool IsVarPackageConfirmed(VarPackage vp)
	{
		if (!vp.PluginsAlwaysDisabled && (vp.PluginsAlwaysEnabled || confirmedOncePackages.Contains(vp)))
		{
			return true;
		}
		return false;
	}

	protected void CheckUserConfirmDenyComplete()
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		if (requestedPackages.Count == 0 && userConfirmDenyCompleteCallback != null)
		{
			userConfirmDenyCompleteCallback(this, userConfirmedAPackage);
		}
	}

	protected void UserConfirmVarPackage(VarPackage vp)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		confirmedOncePackages.Add(vp);
		userConfirmedAPackage = true;
		requestedPackages.Remove(vp);
		CheckUserConfirmDenyComplete();
		FileManager.AutoConfirmAllWithSignature(GetPrompt(vp));
	}

	protected void AutoConfirmVarPackage(VarPackage vp)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		confirmedOncePackages.Add(vp);
		userConfirmedAPackage = true;
		requestedPackages.Remove(vp);
		CheckUserConfirmDenyComplete();
	}

	protected void UserConfirmStickyVarPackage(VarPackage vp)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		vp.PluginsAlwaysEnabled = true;
		vp.PluginsAlwaysDisabled = false;
		userConfirmedAPackage = true;
		requestedPackages.Remove(vp);
		CheckUserConfirmDenyComplete();
		FileManager.AutoConfirmAllWithSignature(GetPrompt(vp));
	}

	protected void UserDenyVarPackage(VarPackage vp)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		requestedPackages.Remove(vp);
		CheckUserConfirmDenyComplete();
		FileManager.AutoDenyAllWithSignature(GetPrompt(vp));
	}

	protected void AutoDenyVarPackage(VarPackage vp)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		requestedPackages.Remove(vp);
		CheckUserConfirmDenyComplete();
	}

	protected void UserDenyStickyVarPackage(VarPackage vp)
	{
		FileManager.AssertNotCalledFromUnityMessaging();
		vp.PluginsAlwaysEnabled = false;
		vp.PluginsAlwaysDisabled = true;
		requestedPackages.Remove(vp);
		CheckUserConfirmDenyComplete();
		FileManager.AutoDenyAllWithSignature(GetPrompt(vp));
	}

	public void SetupUserConfirmDeny(UserConfirmDenyComplete callback)
	{
		HasRequestedPackages = false;
		requestedPackages.Clear();
		confirmedOncePackages.Clear();
		userConfirmedAPackage = false;
		userConfirmDenyCompleteCallback = callback;
	}

	protected string GetPrompt(VarPackage vp)
	{
		return "Allow load of plugins from addon " + vp.Uid + "? (you should only allow if you trust the source of this plugin)";
	}

	public void UserConfirm()
	{
		foreach (VarPackage vp in requestedPackages)
		{
			FileManager.ConfirmWithUser(GetPrompt(vp), delegate
			{
				UserConfirmVarPackage(vp);
			}, delegate
			{
				AutoConfirmVarPackage(vp);
			}, delegate
			{
				UserConfirmStickyVarPackage(vp);
			}, delegate
			{
				UserDenyVarPackage(vp);
			}, delegate
			{
				AutoDenyVarPackage(vp);
			}, delegate
			{
				UserDenyStickyVarPackage(vp);
			});
		}
	}
}
