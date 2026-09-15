using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Batch-mode entry point of the rebuild pipeline:
///
///     Unity.exe -batchmode -nographics -quit -projectPath VaM_Rebuild \
///               -logFile artifacts\compile-gate.log -executeMethod RebuildGate.Report
///
/// The value of this class is not the report, it is the fact that Unity has to compile the whole
/// project before it can find a method to invoke. If the code does not compile, the method is never
/// called, Unity prints the compiler output and the process exits non-zero - which is exactly the
/// gate Stage 4 needs. Without an execute target a batch run can finish with "Nothing changed" and
/// report nothing at all.
/// </summary>
[InitializeOnLoad]
public static class RebuildGate
{
    // Entering play mode reloads the editor assemblies, which throws away every static field of this
    // class and every event subscription it made. The play run therefore keeps its settings in
    // EditorPrefs - the one store that survives a reload - and re-arms itself from this constructor,
    // which Unity runs after every reload, including the one that precedes play mode.
    private const string ArmedKey = "RebuildGate.Play.Armed";
    private const string SecondsKey = "RebuildGate.Play.Seconds";
    private const string ErrorLimitKey = "RebuildGate.Play.ErrorLimit";
    private const string SceneKey = "RebuildGate.Play.Scene";
    private const string WarmupKey = "RebuildGate.Play.Warmup";
    private const string SceneDoneKey = "RebuildGate.Play.SceneDone";

    static RebuildGate()
    {
        if (!EditorPrefs.GetBool(ArmedKey, false))
        {
            return;
        }

        Application.logMessageReceived += OnLogMessage;
        EditorApplication.update += OnPlayUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        double.TryParse(EditorPrefs.GetString(SecondsKey, "30"), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out playSeconds);
        playErrorLimit = EditorPrefs.GetInt(ErrorLimitKey, 40);
        playScene = EditorPrefs.GetString(SceneKey, string.Empty);
        playWarmup = EditorPrefs.GetFloat(WarmupKey, 15f);
        playSceneRequested = EditorPrefs.GetBool(SceneDoneKey, false);
        Debug.Log("----- RebuildGate play: armed after reload -----");
    }

    /// <summary>Counts the runtime asset bundles reachable through the StreamingAssets junction.</summary>
    private static void ReportBundles(string projectRoot, StringBuilder report)
    {
        string bundles = Path.Combine(projectRoot, "StreamingAssets");
        string inside = Path.Combine(Application.dataPath, "StreamingAssets");

        if (Directory.Exists(inside))
        {
            report.AppendLine("WARNING: Assets/StreamingAssets exists - Unity would import the whole bundle set");
        }

        if (!Directory.Exists(bundles))
        {
            report.AppendLine("bundles: MISSING - run scripts\\New-StreamingAssetsLink.ps1");
            return;
        }

        string[] files = Directory.GetFiles(bundles);
        long bytes = 0;
        foreach (string file in files)
        {
            bytes += new FileInfo(file).Length;
        }

        report.AppendLine(string.Format("bundles: {0} files, {1:N2} GB", files.Length, bytes / 1073741824.0));
        report.AppendLine("bundle manifest StandaloneWindows64: " +
                          (File.Exists(Path.Combine(bundles, "StandaloneWindows64")) ? "present" : "MISSING"));
    }

    private static void ReportAssemblies(string projectRoot, StringBuilder report)
    {
        string dir = Path.Combine(Path.Combine(projectRoot, "Library"), "ScriptAssemblies");
        if (!Directory.Exists(dir))
        {
            report.AppendLine("script assemblies: none built");
            return;
        }

        List<string> lines = new List<string>();
        foreach (string dll in Directory.GetFiles(dir, "*.dll"))
        {
            lines.Add(string.Format("  {0} ({1:N0} B)", Path.GetFileName(dll), new FileInfo(dll).Length));
        }

        report.AppendLine(string.Format("script assemblies: {0}", lines.Count));
        foreach (string line in lines)
        {
            report.AppendLine(line);
        }
    }

    /// <summary>
    /// Unity is free to reuse a script assembly it built earlier, so a probe can be compiled into
    /// the project, print nothing, and look like a negative result. Every diagnostic run therefore
    /// starts by proving which probes are present in the assembly the player actually loaded.
    /// </summary>
    private static void ReportScriptMarkers(StringBuilder report)
    {
        Assembly scriptAssembly = null;
        foreach (Assembly candidate in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (candidate.GetName().Name == "Assembly-CSharp")
            {
                scriptAssembly = candidate;
                break;
            }
        }

        if (scriptAssembly == null)
        {
            report.AppendLine("script assembly: Assembly-CSharp is not loaded");
            return;
        }

        report.AppendLine(string.Format("script assembly: {0}", scriptAssembly.Location));
        report.AppendLine(string.Format("  written: {0:yyyy-MM-dd HH:mm:ss}", File.GetLastWriteTime(scriptAssembly.Location)));

        string[][] probes =
        {
            new string[] { "MeshVR.PresetManager", "SetNamesFromPath" },
            new string[] { "DAZHairGroup", "InitInstance" },
            new string[] { "DAZCharacterSelector", "SyncCustomItems" }
        };

        foreach (string[] probe in probes)
        {
            Type type = scriptAssembly.GetType(probe[0]);
            MethodInfo method = type == null ? null : type.GetMethod(probe[1], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                report.AppendLine(string.Format("  {0}.{1}: not found", probe[0], probe[1]));
                continue;
            }

            report.AppendLine(string.Format("  {0}.{1}: {2}", probe[0], probe[1], HasProbe(method) ? "probe present" : "probe MISSING"));
        }
    }

    /// <summary>True when the method body still contains a "[DIAG" string literal.</summary>
    private static bool HasProbe(MethodInfo method)
    {
        MethodBody body = method.GetMethodBody();
        if (body == null)
        {
            return false;
        }

        byte[] il = body.GetILAsByteArray();
        for (int i = 0; i + 4 < il.Length; i++)
        {
            if (il[i] != 0x72)
            {
                continue;
            }

            string literal = null;
            try
            {
                literal = method.Module.ResolveString(BitConverter.ToInt32(il, i + 1));
            }
            catch (Exception)
            {
            }

            if (literal != null && literal.StartsWith("[DIAG"))
            {
                return true;
            }

            i += 4;
        }

        return false;
    }

    public static void Report()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        StringBuilder report = new StringBuilder();
        report.AppendLine("----- RebuildGate " + Application.unityVersion + " -----");
        report.AppendLine("project: " + projectRoot);
        report.AppendLine("streamingAssetsPath: " + Application.streamingAssetsPath);
        report.AppendLine("dataPath: " + Application.dataPath);

        ReportAssemblies(projectRoot, report);
        ReportScriptMarkers(report);
        ReportBundles(projectRoot, report);

        report.AppendLine(string.Format("build scenes: {0}", EditorBuildSettings.scenes.Length));
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            report.AppendLine("  " + scene.path);
        }

        Debug.Log(report.ToString());
        Debug.Log("----- RebuildGate OK -----");
    }

    // ---------------------------------------------------------------- scene inspection

    /// <summary>The scene the player boots into: what VaM.exe itself would load first.</summary>
    private static string BootScene()
    {
        if (EditorBuildSettings.scenes.Length == 0)
        {
            return null;
        }

        return EditorBuildSettings.scenes[0].path;
    }

    /// <summary>
    /// Static analysis of the boot scene, and the cheapest possible answer to the question the whole
    /// rebuild is about: does the scene still serialize the components the original assembly
    /// declared? A decompiled-and-recompiled assembly keeps the class and field names, so every
    /// [Serializable] reference should still bind - and every component that does not bind shows up
    /// as a null entry in a GameObject's component list.
    /// </summary>
    public static void InspectScene()
    {
        string scenePath = BootScene();
        if (scenePath == null)
        {
            Debug.LogError("----- RebuildGate FAILED ----- no scene in build settings");
            EditorApplication.Exit(1);
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        StringBuilder report = new StringBuilder();
        report.AppendLine("----- RebuildGate scene inspection -----");
        report.AppendLine("scene: " + scenePath);

        int objects = 0;
        int components = 0;
        List<string> missing = new List<string>();
        Dictionary<string, int> types = new Dictionary<string, int>();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                objects++;
                foreach (Component component in transform.GetComponents<Component>())
                {
                    components++;
                    if (component == null)
                    {
                        // Unity keeps the slot and reports a null component when the script asset for
                        // a serialized MonoBehaviour cannot be resolved.
                        missing.Add(transform.gameObject.name);
                        continue;
                    }

                    string type = component.GetType().Name;
                    int count;
                    types.TryGetValue(type, out count);
                    types[type] = count + 1;
                }
            }
        }

        report.AppendLine(string.Format("game objects: {0}, components: {1}", objects, components));
        report.AppendLine(string.Format("unresolved components (missing scripts): {0}", missing.Count));
        for (int i = 0; i < missing.Count && i < 20; i++)
        {
            report.AppendLine("  " + missing[i]);
        }

        List<string> names = new List<string>(types.Keys);
        names.Sort(delegate (string a, string b) { return types[b].CompareTo(types[a]); });
        report.AppendLine("component types:");
        for (int i = 0; i < names.Count && i < 25; i++)
        {
            report.AppendLine(string.Format("  {0} x{1}", names[i], types[names[i]]));
        }

        Debug.Log(report.ToString());
        Debug.Log(missing.Count == 0 ? "----- RebuildGate OK -----" : "----- RebuildGate FAILED -----");
        EditorApplication.Exit(missing.Count == 0 ? 0 : 1);
    }

    // ---------------------------------------------------------------- play mode

    private static readonly List<string> PlayErrors = new List<string>();
    private static readonly HashSet<string> PlaySeen = new HashSet<string>();
    private static double playStartedAt;
    private static double playSeconds = 30.0;
    private static int playErrorLimit = 40;
    private static bool playReported;
    private static bool playVerdict;
    private static string playScene = string.Empty;
    private static double playWarmup = 15.0;
    private static bool playSceneRequested;
    private static int playErrorsAtLoad = -1;
    private static string playContentAtFailure;

    /// <summary>
    /// Runs the boot scene in play mode for a while and reports what the game printed.
    ///
    /// This is deliberately not "the game works" but "the game got off the ground": the report ends
    /// with the state of SuperController, the singleton the whole of VaM hangs off, so a run that
    /// reaches it has loaded its scenes, its asset bundles and its player, and a run that does not
    /// stops at the first exception it printed.
    ///
    ///     Unity.exe -batchmode -projectPath VaM_Rebuild -logFile <log> \
    ///               -executeMethod RebuildGate.Play -smokeSeconds 30
    ///
    /// A scene can be loaded on top of the boot with -smokeScene, once the boot has settled:
    ///
    ///     ... -executeMethod RebuildGate.Play -smokeSeconds 120 \
    ///         -smokeWarmup 20 -smokeScene "MeshedVR.BonusScenes.9:/Saves/scene/....json"
    ///
    /// That is the same call the in-game file browser makes, so it turns "click through the UI and
    /// watch the console" into a repeatable run: the report then covers the boot plus the scene.
    /// 
    /// Note the missing -nographics. VaM's cloth, hair and collider systems are compute-shader
    /// systems, and a headless editor has no graphics device at all: every shader reports "All
    /// passes removed", ComputeShader.FindKernel returns -1 and GPUCollidersManager drowns the log
    /// in NullReferenceExceptions before the game has loaded anything. The graphics device has to be
    /// real for this gate to say anything about the rebuild.
    ///
    /// No -quit: the editor has to stay alive for the play mode to run, so the method exits itself.
    /// </summary>
    public static void Play()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        for (int i = 0; i < arguments.Length - 1; i++)
        {
            if (arguments[i] == "-smokeSeconds")
            {
                double.TryParse(arguments[i + 1], out playSeconds);
            }

            if (arguments[i] == "-smokeErrorLimit")
            {
                int.TryParse(arguments[i + 1], out playErrorLimit);
            }

            if (arguments[i] == "-smokeScene")
            {
                playScene = arguments[i + 1];
            }

            if (arguments[i] == "-smokeWarmup")
            {
                double.TryParse(arguments[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture,
                                out playWarmup);
            }
        }

        string scenePath = BootScene();
        if (scenePath == null)
        {
            Debug.LogError("----- RebuildGate FAILED ----- no scene in build settings");
            EditorApplication.Exit(1);
            return;
        }

        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        ShowGameView();
        MaximizeEditorWindow();
        EditorPrefs.SetString(SecondsKey, playSeconds.ToString("0.###", CultureInfo.InvariantCulture));
        EditorPrefs.SetInt(ErrorLimitKey, playErrorLimit);
        EditorPrefs.SetString(SceneKey, playScene ?? string.Empty);
        EditorPrefs.SetFloat(WarmupKey, (float)playWarmup);
        EditorPrefs.SetBool(SceneDoneKey, false);
        EditorPrefs.SetBool(ArmedKey, true);
        Debug.Log(string.Format("----- RebuildGate play: {0} for {1} s -----", scenePath, playSeconds));
        if (playScene != null && playScene != string.Empty)
        {
            Debug.Log(string.Format("----- RebuildGate play: will load {0} after {1} s -----",
                                    playScene, playWarmup));
        }

        EditorApplication.isPlaying = true;
    }

    /// <summary>
    /// Brings the Game view forward and makes it fill the editor before play mode starts.
    ///
    /// A batch-mode run has no window at all, but a visible run should show the game rather than
    /// whatever tab happened to be in front - the Game view only draws while it is the one being
    /// shown, so a hidden one makes a working boot look like a frozen editor. Focus alone is not
    /// enough: the Game view keeps the size of whatever dock slot the saved layout gave it, which is
    /// often a few hundred pixels, so the play run is only watchable once the view is maximized too.
    /// </summary>
    private static void ShowGameView()
    {
        Type gameView = Type.GetType("UnityEditor.GameView,UnityEditor");
        if (gameView == null)
        {
            return;
        }

        EditorWindow window = EditorWindow.GetWindow(gameView);
        if (window == null)
        {
            return;
        }

        window.Focus();

        // maximized is an implementation detail that has moved between editor versions - it was
        // static, then per-instance - so set whichever one this editor has instead of pinning the
        // gate to one of them.
        System.Reflection.PropertyInfo maximized = typeof(EditorWindow).GetProperty("maximized",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
        System.Reflection.MethodInfo setter = maximized != null ? maximized.GetSetMethod() : null;
        if (setter == null)
        {
            return;
        }

        try
        {
            setter.Invoke(setter.IsStatic ? null : (object)window, new object[] { true });
        }
        catch (Exception e)
        {
            Debug.LogWarning("----- RebuildGate play: could not maximize the Game view: " + e.Message + " -----");
        }
    }

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr window, int command);

    /// <summary>
    /// Maximizes the editor's own window so the Game view has the whole screen to grow into.
    ///
    /// Maximizing the docked view only makes it fill the editor window; if that window is the small
    /// one Unity restores from its saved layout, the game is still a postage stamp. There is no editor
    /// API for the host window's frame, so ask Windows directly - and skip it entirely in batch mode,
    /// where there is no window to maximize.
    /// </summary>
    private static void MaximizeEditorWindow()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-batchmode") >= 0)
        {
            return;
        }

        try
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
            process.Refresh();
            if (process.MainWindowHandle != IntPtr.Zero)
            {
                ShowWindow(process.MainWindowHandle, 3);    // SW_MAXIMIZE
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("----- RebuildGate play: could not maximize the editor window: " + e.Message + " -----");
        }
    }

    private static void OnLogMessage(string message, string stackTrace, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
        {
            return;
        }

        string entry = string.Format("[{0}] {1}", type, message.Replace("\n", " ").Trim());

        // A defect that repeats every frame buries everything else: the first run of this gate
        // produced 11 MB of the same NullReferenceException and the report never got written.
        if (PlaySeen.Count < 4000 && !PlaySeen.Add(entry))
        {
            return;
        }

        if (PlayErrors.Count >= playErrorLimit)
        {
            return;
        }

        PlayErrors.Add(entry);

        // "Not ready for load" says a content item refused to load but names neither the item nor the
        // path, and the state that refused it can change while the rest of the scene finishes loading.
        // Snapshot the content at the moment it happens, not only at the end of the run.
        if (playContentAtFailure == null && message.IndexOf("Not ready for load", StringComparison.Ordinal) >= 0)
        {
            playContentAtFailure = ContentReport("at the moment of the error");

            // Written out now rather than at the end of the run: the state that refused the load is
            // the whole point of the probe, and a run that ends early - a crash, a closed editor -
            // would otherwise take it with it.
            WriteReportFile(playContentAtFailure);
        }
    }

    private static void OnPlayUpdate()
    {
        if (playReported || !Application.isPlaying)
        {
            return;
        }

        if (playStartedAt == 0.0)
        {
            playStartedAt = EditorApplication.timeSinceStartup;
            Debug.Log("----- RebuildGate play: first frame -----");
            return;
        }

        double elapsed = EditorApplication.timeSinceStartup - playStartedAt;

        if (!playSceneRequested && playScene != null && playScene != string.Empty && elapsed >= playWarmup)
        {
            playSceneRequested = true;
            playErrorsAtLoad = PlayErrors.Count;
            EditorPrefs.SetBool(SceneDoneKey, true);
            LoadPlayScene();
        }

        if (elapsed < playSeconds)
        {
            return;
        }

        // Report before trying to stop: a play mode that is being torn down is exactly when the
        // interesting state disappears, and the editor is not guaranteed to reach the exit at all.
        playReported = true;
        playVerdict = PlayVerdict();
        string report = PlayReport();
        WriteReportFile(report);
        Debug.Log(report);
        EditorPrefs.SetBool(ArmedKey, false);
        EditorApplication.isPlaying = false;
        EditorApplication.Exit(playVerdict ? 0 : 1);
    }

    /// <summary>
    /// Writes the play report beside the run's log file.
    ///
    /// The log is normally where the report reaches the pipeline, but one run already ended without it:
    /// the editor exits from inside play-mode teardown, and a log still being written when the process
    /// dies loses its tail. A file written before the exit survives that.
    /// </summary>
    private static void WriteReportFile(string report)
    {
        string[] arguments = Environment.GetCommandLineArgs();
        string path = null;
        for (int i = 0; i < arguments.Length - 1; i++)
        {
            if (arguments[i] == "-logFile")
            {
                path = arguments[i + 1];
            }
        }

        if (path == null || path == string.Empty)
        {
            return;
        }

        try
        {
            File.WriteAllText(Path.ChangeExtension(path, null) + ".report.txt", report);
        }
        catch (Exception e)
        {
            Debug.LogWarning("----- RebuildGate play: could not write the report file: " + e.Message + " -----");
        }
    }

    /// <summary>
    /// Loads the scene named by -smokeScene through the game's own entry point.
    ///
    /// SuperController.Load is what the file browser ends up calling, so a scene that fails here
    /// fails the same way it fails for a player. The scene name is the game's own addressing, not a
    /// filesystem path - "MeshedVR.BonusScenes.9:/Saves/scene/..." for content inside a .var package,
    /// "Saves/scene/..." for content in the install.
    /// </summary>
    private static void LoadPlayScene()
    {
        SuperController controller = SuperController.singleton;
        if (controller == null)
        {
            Debug.LogError("----- RebuildGate play: no SuperController, cannot load " + playScene + " -----");
            return;
        }

        Debug.Log(string.Format("----- RebuildGate play: loading {0} -----", playScene));
        try
        {
            controller.Load(playScene);
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("----- RebuildGate play: loading {0} threw {1} -----", playScene, e));
        }
    }

    private static void OnPlayModeChanged(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredEditMode || !playReported)
        {
            return;
        }

        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.Exit(playVerdict ? 0 : 1);
    }

    private static bool PlayVerdict()
    {
        SuperController controller = SuperController.singleton;
        if (controller == null || controller.GetAtomUIDs().Count == 0)
        {
            return false;
        }

        // Two messages mean a scene item could not be resolved. "X is missing" is FileManager's way of
        // saying it cannot see the file at all, and "Not ready for load" is what DAZDynamic says when
        // its store path, its .vam or its .vaj did not resolve. Both are failures that hide behind a
        // scene which still loads and still renders - the person keeps its body and quietly loses its
        // hair - so neither may pass the gate. Everything else, including the Scene view's own copy of
        // MKGlow throwing because the glow shader is not in the export, is reported without changing
        // the verdict.
        foreach (string error in PlayErrors)
        {
            if (error.IndexOf(" is missing", StringComparison.Ordinal) >= 0 ||
                error.IndexOf("Not ready for load", StringComparison.Ordinal) >= 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Lists the dynamic content items - hair, clothing, and everything else a scene stores as a
    /// reference rather than a mesh - that cannot resolve their own files right now.
    ///
    /// This exists because such an item is refused with the single message "Not ready for load.
    /// Invalid load path or params", which names neither the item nor the path, and because
    /// DAZDynamic.CheckReadyForLoad() needs both the .vam and the .vaj beside the named store. Asking
    /// the same question the game asks - and asking it both the moment the error appears and again when
    /// the run ends - separates an item whose stored path is wrong from an item FileManager could not
    /// see yet, which is the difference between a broken link and a load that raced the asset scan.
    /// </summary>
    private static string ContentReport(string when)
    {
        List<MeshVR.DAZDynamic> items = new List<MeshVR.DAZDynamic>();
        foreach (UnityEngine.Object candidate in Resources.FindObjectsOfTypeAll(typeof(MeshVR.DAZDynamic)))
        {
            // Prefabs and package assets are not scene content, and their serialized defaults would
            // drown the report: only what a loaded scene actually instanced matters here.
            MeshVR.DAZDynamic item = candidate as MeshVR.DAZDynamic;
            if (item != null && !EditorUtility.IsPersistent(item))
            {
                items.Add(item);
            }
        }

        List<MeshVR.DAZDynamic> unloadable = new List<MeshVR.DAZDynamic>();
        List<MeshVR.DAZDynamic> active = new List<MeshVR.DAZDynamic>();
        foreach (MeshVR.DAZDynamic item in items)
        {
            if (!item.CheckReadyForLoad())
            {
                unloadable.Add(item);
            }
            else if (item.gameObject.activeInHierarchy)
            {
                // Readiness is not the only way a load can fail: an item the scene switched on also
                // exercises the .vam/.vaj reads and the JSON behind them, so it is worth its detail
                // lines even when the file checks pass.
                active.Add(item);
            }
        }

        StringBuilder report = new StringBuilder();
        report.AppendLine(string.Format("dynamic content {0}: {1} item(s), {2} cannot load, {3} active",
                                        when, items.Count, unloadable.Count, active.Count));

        List<MeshVR.DAZDynamic> suspects = new List<MeshVR.DAZDynamic>(unloadable);
        suspects.AddRange(active);
        if (suspects.Count == 0)
        {
            return report.ToString();
        }

        report.AppendLine("working directory: " + Directory.GetCurrentDirectory());

        // Ten is enough to see the pattern; a run with hundreds of broken items has a bigger problem
        // than this report can carry anyway.
        for (int i = 0; i < suspects.Count && i < 10; i++)
        {
            MeshVR.DAZDynamic item = suspects[i];
            string folder = item.GetStoreFolderPath();
            string vam = folder + item.storeName + ".vam";
            string vaj = folder + item.storeName + ".vaj";
            report.AppendLine(string.Format("  {0}: itemType={1}, storeName=\"{2}\", creatorName=\"{3}\", storeFolderName=\"{4}\", package=\"{5}\"",
                                            TransformPath(item.transform), item.itemType, item.storeName,
                                            item.creatorName, item.storeFolderName, item.package));
            report.AppendLine(string.Format("    folder path: {0} (directory exists: {1})",
                                            folder ?? "<null>", folder != null && Directory.Exists(folder)));
            report.AppendLine(string.Format("    {0}: File.Exists={1}, FileManager.FileExists={2}",
                                            vam, File.Exists(vam), MVR.FileManagement.FileManager.FileExists(vam)));
            report.AppendLine(string.Format("    {0}: File.Exists={1}, FileManager.FileExists={2}",
                                            vaj, File.Exists(vaj), MVR.FileManagement.FileManager.FileExists(vaj)));
        }

        return report.ToString();
    }

    private static string TransformPath(Transform transform)
    {
        string path = transform.name;
        for (Transform parent = transform.parent; parent != null; parent = parent.parent)
        {
            path = parent.name + "/" + path;
        }

        return path;
    }

    private static string PlayReport()
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("----- RebuildGate play report -----");
        report.AppendLine(string.Format("played: {0:N1} s", playSeconds));
        if (playScene != null && playScene != string.Empty)
        {
            report.AppendLine(string.Format("scene: {0} (after {1:N0} s)", playScene, playWarmup));
            report.AppendLine(string.Format("scene loaded: {0}{1}",
                playSceneRequested ? "yes" : "no - the run ended before the load",
                playErrorsAtLoad >= 0 ? string.Format(", errors before the load: {0}", playErrorsAtLoad) : ""));
        }

        report.AppendLine(string.Format("errors and exceptions: {0}{1}", PlayErrors.Count,
            PlayErrors.Count >= playErrorLimit ? " (capped, distinct messages only)" : ""));
        foreach (string error in PlayErrors)
        {
            report.AppendLine("  " + error);
        }

        if (playContentAtFailure != null)
        {
            report.Append(playContentAtFailure);
        }

        SuperController controller = SuperController.singleton;
        if (controller == null)
        {
            report.AppendLine("SuperController.singleton: null - the game did not boot");
        }
        else
        {
            report.AppendLine("SuperController.singleton: " + controller.name);

            // A boot that never leaves the loading state still looks healthy from the outside - the
            // singleton is alive and errors are zero - but nothing downstream ever runs (PerfMon, for
            // instance, only starts counting frames once GlobalSceneOptions finishes loading).
            report.AppendLine(string.Format("loading: SuperController={0}, GlobalSceneOptions={1}, simulation resetting={2}",
                controller.isLoading, MeshVR.GlobalSceneOptions.IsLoading, controller.IsSimulationResetting()));

            string[] uids = controller.GetAtomUIDs().ToArray();
            report.AppendLine(string.Format("atoms: {0} ({1})", uids.Length, string.Join(", ", uids)));
            report.Append(ContentReport("when the run ended"));
            report.AppendLine(string.Format("PerfMon components: {0}", UnityEngine.Object.FindObjectsOfType<MeshVR.PerfMon>().Length));
            foreach (MeshVR.PerfMon perf in UnityEngine.Object.FindObjectsOfType<MeshVR.PerfMon>())
            {
                // The original prints "Benchmark complete" after 900 counted frames. _totFrames only
                // advances from PerfMon's WaitForEndOfFrame coroutine, so a zero here means the coroutine
                // never resumed - which is what batchmode does, not a defect in the rebuild.
                System.Reflection.FieldInfo frames = typeof(MeshVR.PerfMon)
                    .GetField("_totFrames", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                report.AppendLine(string.Format("  PerfMon frames={0}, window starts at {1}, window size {2}, on={3}",
                    frames != null ? frames.GetValue(perf) : "?", perf.avgCalcStartFrame, perf.avgCalcNumFrames, perf.on));
            }
        }

        // The editor prints "Unloading broken assembly Assets/Plugins/RTTypeModel.dll" at start-up, but
        // it prints the same thing about its own UnityEditor.UI.dll, so the message cannot be taken at
        // face value. Ask the runtime whether the assembly is actually usable.
        System.Reflection.Assembly rtTypeModel = null;
        try { rtTypeModel = System.Reflection.Assembly.Load("RTTypeModel"); }
        catch (Exception e) { report.AppendLine("RTTypeModel: " + e.GetType().Name + " - " + e.Message); }
        if (rtTypeModel != null)
        {
            report.AppendLine(string.Format("RTTypeModel: loaded, {0} types, image runtime {1}",
                rtTypeModel.GetTypes().Length, rtTypeModel.ImageRuntimeVersion));
        }

        // ProtobufSerializer builds its type model in a static constructor, so this is the save/load
        // path's first real step - and it is the only consumer of RTTypeModel.
        try
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
                typeof(Battlehub.RTSaveLoad.ProtobufSerializer).TypeHandle);
            report.AppendLine("ProtobufSerializer: static constructor ran");
        }
        catch (Exception e)
        {
            report.AppendLine("ProtobufSerializer: " + e.GetType().Name + " - " + e.Message);
        }

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            report.AppendLine(string.Format("scene: {0}", SceneManager.GetSceneAt(i).name));
        }

        report.AppendLine(string.Format("loaded scenes: {0}", SceneManager.sceneCount));
        report.AppendLine(string.Format("mono behaviours in the loaded scenes: {0}", UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().Length));
        report.AppendLine(playVerdict ? "----- RebuildGate OK -----" : "----- RebuildGate FAILED -----");
        return report.ToString();
    }
}
