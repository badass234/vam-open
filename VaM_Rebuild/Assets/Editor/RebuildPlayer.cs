using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Batch-mode entry point that builds the standalone player:
///
///     Unity.exe -batchmode -nographics -quit -projectPath VaM_Rebuild \
///               -logFile artifacts\player-build.log -executeMethod RebuildPlayer.Build
///
/// The player goes to <c>artifacts\player</c>, outside the project, because anything built inside the
/// asset tree would be imported by the editor and taken apart on the next start. What is written there
/// is the player alone: the data it loads at runtime - <c>Saves</c>, <c>Custom</c>,
/// <c>AddonPackages</c>, <c>Keys</c> and the <c>StreamingAssets</c> the bundles live in - is not copied
/// into the build, it is linked in afterwards by <c>scripts\New-PlayerRuntimeLinks.ps1</c>, which is
/// also what keeps a build from writing into the VaM installation.
///
/// The verdict is logged the way <see cref="RebuildGate"/> logs its own, because Unity's batch-mode
/// exit code does not distinguish "the build failed" from "the editor never got that far".
/// </summary>
public static class RebuildPlayer
{
    private const string ExeName = "VAMOpen.exe";
    private const string DataDirectoryName = "VAMOpen_Data";

    /// <summary>
    /// What a player build owns in the output directory. Cleared before a build so that a failed one
    /// cannot leave the previous build's exe behind and be read as a success.
    /// </summary>
    private static readonly string[] OwnedOutputs =
    {
        ExeName,
        "VAMOpen.pdb",
        DataDirectoryName,
        "UnityPlayer.dll",
        "WinPixEventRuntime.dll",
        "MonoBleedingEdge"
    };

    /// <summary>The runtime data the built player reads next to its own exe.</summary>
    /// <remarks>
    /// <c>Keys</c> is in the list because of what its absence costs, not what it weighs: without
    /// <c>Keys/1.21/key.json</c> the game falls back to its restricted package set and registers nine
    /// of the eighteen packages the editor scans for the same project.
    /// </remarks>
    private static readonly string[] RuntimeData = { "AddonPackages", "Custom", "Saves", "AddonPackagesUserPrefs", "Keys" };

    public static void Build()
    {
        string playerRoot = Path.Combine(RepositoryRoot(), "artifacts", "player");
        string exe = Path.Combine(playerRoot, ExeName);
        string[] scenes = EnabledScenes();

        Debug.Log("----- RebuildPlayer: build started -----");
        Debug.Log("  project: " + Application.dataPath);
        Debug.Log("  output:  " + exe);
        Debug.Log(string.Format("  scenes:  {0} enabled, boot {1}", scenes.Length,
                                scenes.Length == 0 ? "none" : scenes[0]));

        if (scenes.Length == 0)
        {
            // An empty scene list builds a player that starts with nothing at all, which would look
            // like a build failure much later and in a place with no log. Refused here instead.
            Debug.LogError("----- RebuildPlayer FAILED: the build settings list no enabled scene -----");
            return;
        }

        Directory.CreateDirectory(playerRoot);
        DeletePreviousOutputs(playerRoot);

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = exe;
        options.target = BuildTarget.StandaloneWindows64;
        options.targetGroup = BuildTargetGroup.Standalone;
        options.options = BuildOptions.None;

        // The return value is deliberately unused: 2018.1 hands back a BuildReport, older editors a
        // string, and this file has to compile under either. The output on disk is the verdict.
        BuildPipeline.BuildPlayer(options);

        Report(playerRoot, exe);
    }

    private static void Report(string playerRoot, string exe)
    {
        if (!File.Exists(exe))
        {
            Debug.LogError("----- RebuildPlayer FAILED: no player written to " + exe + " -----");
            return;
        }

        Debug.Log(string.Format("  player:  {0:N0} B in {1}", SizeOf(playerRoot), playerRoot));

        // The build never contains the game's data, so the report says which of it is already there.
        // The player resolves all of it relative to its own directory - the game computes its root as
        // the parent of Application.dataPath, and the bundles come from Application.streamingAssetsPath,
        // which is <exe>_Data\StreamingAssets - so the links belong exactly here.
        List<string> missing = new List<string>();
        foreach (string name in RuntimeData)
        {
            if (!Directory.Exists(Path.Combine(playerRoot, name))) { missing.Add(name); }
        }
        string streamingAssets = Path.Combine(playerRoot, DataDirectoryName, "StreamingAssets");
        if (!Directory.Exists(streamingAssets)) { missing.Add(DataDirectoryName + "\\StreamingAssets"); }

        if (missing.Count > 0)
        {
            Debug.Log("  runtime data missing: " + string.Join(", ", missing.ToArray()));
            Debug.Log("  run scripts\\New-PlayerRuntimeLinks.ps1 before starting the player");
        }
        else
        {
            Debug.Log("  runtime data: linked");
        }

        Debug.Log("----- RebuildPlayer OK -----");
    }

    private static string[] EnabledScenes()
    {
        List<string> paths = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled) { paths.Add(scene.path); }
        }
        return paths.ToArray();
    }

    /// <summary>
    /// The repository root, which is the parent of the project folder the editor was pointed at.
    /// </summary>
    private static string RepositoryRoot()
    {
        DirectoryInfo project = Directory.GetParent(Application.dataPath);
        return Directory.GetParent(project.FullName).FullName;
    }

    private static void DeletePreviousOutputs(string playerRoot)
    {
        foreach (string name in OwnedOutputs)
        {
            string path = Path.Combine(playerRoot, name);
            if (Directory.Exists(path)) { DeleteTree(path); }
            else if (File.Exists(path)) { File.Delete(path); }
        }
    }

    /// <summary>
    /// Deletes a directory tree, unlinking junctions rather than walking through them.
    /// </summary>
    /// <remarks>
    /// A player that has been through <c>New-PlayerRuntimeLinks.ps1</c> holds junctions:
    /// <c>&lt;exe&gt;_Data\StreamingAssets</c> is the installation's 16.47 GB of bundles. Mono's
    /// <c>Directory.Delete(path, true)</c> sees a junction as an ordinary subdirectory, so it either
    /// throws <c>IOException: Directory ... is not empty</c> - which is what a second player build
    /// used to do, taking the build with it - or removes the files behind the link, which are the
    /// user's game and not ours.
    /// </remarks>
    private static void DeleteTree(string path)
    {
        if (IsJunction(path)) { UnlinkJunction(path); return; }

        foreach (string child in Directory.GetDirectories(path))
        {
            if (IsJunction(child)) { UnlinkJunction(child); }
            else { DeleteTree(child); }
        }
        Directory.Delete(path, true);
    }

    /// <summary>
    /// Removes a junction, leaving whatever it points at alone.
    /// </summary>
    /// <remarks>
    /// Neither shape of the BCL call can be used here. Mono's <c>Directory.Delete(path, false)</c>
    /// answers a reparse point with <c>UnauthorizedAccessException</c> - measured, not assumed: the
    /// first version of this helper did exactly that and the build died on it - and with
    /// <c>recursive: true</c> it descends through the link, which is the one thing that must not
    /// happen, because the link is the installation's 16.47 GB of bundles. <c>RemoveDirectory</c> is
    /// the call that takes the name away and nothing else. (.NET Framework's <c>Directory.Delete</c>
    /// does the same thing, which is why the PowerShell script that makes these links can use it.)
    /// </remarks>
    private static void UnlinkJunction(string path)
    {
        if (!RemoveDirectory(path))
        {
            throw new IOException(string.Format("could not unlink {0}: Win32 error {1}",
                                                path, Marshal.GetLastWin32Error()));
        }
    }

    private static bool IsJunction(string path)
    {
        return (new DirectoryInfo(path).Attributes & FileAttributes.ReparsePoint) != 0;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool RemoveDirectory(string path);

    private static long SizeOf(string directory)
    {
        long total = 0;
        foreach (string file in Directory.GetFiles(directory))
        {
            total += new FileInfo(file).Length;
        }
        // Junctions are skipped for the same reason they are unlinked: the player's own size does
        // not include the gigabytes of game data that happen to be linked next to it.
        foreach (string child in Directory.GetDirectories(directory))
        {
            if (!IsJunction(child)) { total += SizeOf(child); }
        }
        return total;
    }
}
