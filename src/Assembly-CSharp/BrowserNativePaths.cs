#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

/// <summary>
/// Points the shipped ZFBrowser assembly at the browser runtime where the editor keeps it.
/// </summary>
/// <remarks>
/// VaM ships <c>ZFBrowser.dll</c> built for the player, and there the folders line up:
/// <c>FileLocations.GetCEFDirs</c> derives every path from <c>Application.dataPath + "/Plugins"</c>,
/// which in a player is <c>&lt;exe&gt;_Data\Plugins</c> -- exactly where the installation keeps the
/// whole CEF set, flat.
///
/// In the editor <c>Application.dataPath</c> is <c>&lt;project&gt;\Assets</c>, so the same code looks
/// for the runtime in <c>Assets\Plugins</c>, and this project keeps it one level down, in
/// <c>Assets\Plugins\x86_64</c>, because that is the folder Unity reads native plugins from and the
/// one the player build stages. The shipped assembly cannot be asked to look there: the editor branch
/// that would name the architecture is not in VaM's player build at all. Three things fail, and all
/// three are answered here.
///
/// <list type="number">
/// <item><c>BrowserNative.HandLoadSymbols</c> loads <c>&lt;binariesPath&gt;/ZFProxyWeb.dll</c>, which
/// needs the rest of the CEF set beside it: <c>zf_cef.dll</c>, <c>chrome_elf.dll</c>, the <c>.pak</c>
/// and <c>.bin</c> files, <c>locales\</c> and <c>ZFGameBrowser.exe</c>. Redirecting the four paths
/// reproduces the original installation's own layout rather than inventing one, so nothing else about
/// the load changes -- in particular <c>PATH</c>, which <c>LoadNative</c> extends with the game root,
/// is left alone.</item>
/// <item><c>StandaloneWebResources.LoadIndex</c> reads <c>&lt;project&gt;\Assets\Resources\browser_assets</c>
/// and throws when it is absent, so the empty index is written here for the editor. It is the same 14
/// bytes <c>RebuildPlayer.WriteWebResourceIndex</c> writes into a player -- a length-prefixed
/// <c>zfbRes_v1</c> header and a count of zero, meaning the resources live on disk -- and it is the file
/// ZFBrowser's own package layout expects to find there.</item>
/// <item><c>logFile</c> is forwarded to CEF, and the shipped default would put it in the asset tree. It
/// is redirected into <c>Library</c>, which is generated state already.</item>
/// </list>
///
/// <c>FileLocations.Dirs</c> caches into a private static field on its first read, so this has to run
/// before any browser exists. <c>BeforeSceneLoad</c> is earlier than the <c>Awake</c> that creates one,
/// and earlier than any scene, which is why the hook sits here rather than in <c>VRWebBrowser.Awake</c>.
///
/// Editor only, and deliberately so: a player build is already the layout the assembly expects, and
/// <c>RebuildPlayer</c> is what reproduces it.
/// </remarks>
internal static class BrowserNativePaths
{
	private const string RuntimeFolder = @"Plugins\x86_64";
	private const string LocalesFolder = "locales";
	private const string SubprocessExecutable = "ZFGameBrowser.exe";
	private const string LoaderLibrary = "ZFProxyWeb.dll";
	private const string WebResourceIndex = @"Resources\browser_assets";
	private const string WebResourceHeader = "zfbRes_v1";

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Configure()
	{
		string runtime = Path.Combine(Application.dataPath, RuntimeFolder);
		if (!File.Exists(Path.Combine(runtime, LoaderLibrary)))
		{
			Debug.LogWarning("The browser runtime is not staged at " + runtime + ", so the built-in browser cannot start. Run scripts\\Setup-RebuildProject.ps1.");
			return;
		}

		FileLocations.CEFDirs dirs = FileLocations.Dirs;
		dirs.resourcesPath = runtime;
		dirs.binariesPath = runtime;
		dirs.localesPath = Path.Combine(runtime, LocalesFolder);
		dirs.subprocessFile = Path.Combine(runtime, SubprocessExecutable);
		dirs.logFile = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Library", "browser.log");

		WriteWebResourceIndex();
	}

	private static void WriteWebResourceIndex()
	{
		string path = Path.Combine(Application.dataPath, WebResourceIndex);
		if (File.Exists(path))
		{
			return;
		}

		Directory.CreateDirectory(Path.GetDirectoryName(path));
		using (FileStream stream = File.Create(path))
		using (BinaryWriter writer = new BinaryWriter(stream))
		{
			writer.Write(WebResourceHeader);
			writer.Write(0);
		}
	}
}
#endif