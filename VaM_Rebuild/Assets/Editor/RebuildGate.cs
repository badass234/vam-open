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

    /// <summary>Path of the assembly the editor compiles the game's scripts into.</summary>
    private static string ScriptAssemblyPath()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(Path.Combine(Path.Combine(projectRoot, "Library"), "ScriptAssemblies"), "Assembly-CSharp.dll");
    }

    /// <summary>
    /// The markers a diagnostic run depends on live as string literals in the assembly metadata, so
    /// the probes that are really compiled in are read straight out of the built assembly: find every
    /// "[DIAG" literal and read the surrounding text back. The literals are stored as UTF-16, which is
    /// why this searches bytes rather than text.
    ///
    /// Reflection would be the obvious way to ask the same question, but the editor loads the gameplay
    /// assembly lazily - AppDomain.GetAssemblies() does not list it yet when a batch method runs - and
    /// a probe that missed the last compile is exactly the false negative this report exists to catch.
    /// </summary>
    private static List<string> FindProbes(string assemblyPath)
    {
        List<string> probes = new List<string>();
        byte[] bytes = File.ReadAllBytes(assemblyPath);
        for (int i = 0; i + 1 < bytes.Length; i++)
        {
            if (bytes[i] != (byte)'[' || bytes[i + 1] != 0)
            {
                continue;
            }

            StringBuilder text = new StringBuilder();
            int at = i;
            while (at + 1 < bytes.Length && bytes[at + 1] == 0 && bytes[at] >= 0x20 && bytes[at] < 0x7F)
            {
                text.Append((char)bytes[at]);
                at += 2;
            }

            string literal = text.ToString();
            if (literal.StartsWith("[DIAG", StringComparison.Ordinal))
            {
                int end = literal.IndexOf(']');
                string probe = end >= 0 ? literal.Substring(0, end + 1) : literal;
                if (!probes.Contains(probe))
                {
                    probes.Add(probe);
                }
            }

            i = at;
        }

        probes.Sort(StringComparer.Ordinal);
        return probes;
    }

    /// <summary>The compiled-in probes as one line, for the reports that only need the summary.</summary>
    private static string ProbeSummary()
    {
        string assembly = ScriptAssemblyPath();
        if (!File.Exists(assembly))
        {
            return "Assembly-CSharp.dll is not built";
        }

        List<string> probes = FindProbes(assembly);
        return probes.Count == 0 ? "no TEMP DIAGNOSTIC probe is compiled in" : string.Join(", ", probes.ToArray());
    }

    /// <summary>Newest write time of any script under the given root, i.e. the moment the assembly should have been built.</summary>
    private static DateTime NewestScript(string assetsRoot)
    {
        DateTime newest = DateTime.MinValue;
        foreach (string file in Directory.GetFiles(assetsRoot, "*.cs", SearchOption.AllDirectories))
        {
            DateTime written = File.GetLastWriteTime(file);
            if (written > newest)
            {
                newest = written;
            }
        }

        return newest;
    }

    /// <summary>
    /// Unity is free to reuse a script assembly it built earlier, so a probe can be compiled into the
    /// project, print nothing, and look like a negative result. Every diagnostic run therefore starts
    /// by proving which probes the build actually contains - and whether the build is current at all.
    ///
    /// The stale build is a real trap, not a hypothetical one: after a script is edited outside the
    /// editor the asset database can still believe nothing changed, the old assembly survives, and
    /// every probe added since then silently does nothing while the project looks healthy.
    /// </summary>
    private static void ReportScriptMarkers(StringBuilder report)
    {
        string assembly = ScriptAssemblyPath();
        if (!File.Exists(assembly))
        {
            report.AppendLine("script assembly: not built - Library/ScriptAssemblies/Assembly-CSharp.dll is missing");
            return;
        }

        FileInfo info = new FileInfo(assembly);
        report.AppendLine(string.Format("script assembly: {0} ({1:N0} B, written {2:yyyy-MM-dd HH:mm:ss})",
                                        assembly, info.Length, info.LastWriteTime));

        // Only the gameplay sources matter here: a change under Assets/Editor rebuilds the editor
        // assembly, not the one the probes live in.
        DateTime newest = NewestScript(Path.Combine(Application.dataPath, "Scripts"));
        report.AppendLine(string.Format("  newest gameplay script: {0:yyyy-MM-dd HH:mm:ss}", newest));
        if (newest > info.LastWriteTime.AddSeconds(1))
        {
            report.AppendLine("  WARNING: the scripts are newer than the assembly - Unity did not recompile it");
        }

        List<string> probes = FindProbes(assembly);
        report.AppendLine(string.Format("  TEMP DIAGNOSTIC probes compiled in: {0}", probes.Count));
        foreach (string probe in probes)
        {
            report.AppendLine("    " + probe);
        }

        if (probes.Count == 0)
        {
            report.AppendLine("    (a probe missing here was not compiled - rebuild before trusting a play run)");
        }
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

        // -batchmode without -quit leaves the editor running forever once the method returns, and a
        // gate that has to be killed by hand cannot be a gate. The play runs exit themselves for the
        // same reason; the report has nothing left to wait for.
        EditorApplication.Exit(0);
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

    // A scene still loading when the run's own clock reaches -smokeSeconds is a run that ran out of
    // time, not a verdict, so the run waits this much longer for the load to finish. It is bounded so
    // that a load which hangs can never hold the editor open indefinitely.
    private const double PlaySceneGraceSeconds = 120.0;

    // The request, as opposed to the scene: whether the game was asked, whether it took the request,
    // and whether it got through it. These are tracked apart from the run's own errors because a
    // refused request is silent - the game logs a warning and returns - and a warning is not an error.
    private static double playSceneWaitedSince;
    private static bool playSceneLoadStarted;
    private static bool playSceneLoadFinished;
    private static double playSceneFinishedAt;
    private static bool playLoadRefused;
    private static int playSceneDeclared = -1;
    private static int playScenePresent;
    private static readonly List<string> playSceneMissing = new List<string>();
    private static string playSceneAuditError;
    private static string playContentAtFailure;

    // Animation cannot be judged from one look at the scene: a single sample of a posed body is a
    // still frame, and a still frame is exactly what a run looks like when the skinning wrote the
    // bind pose. The run therefore samples the posed bones, waits, and samples them again, which is
    // the only thing that separates "the clip is playing" from "the pose was applied once".
    private const double AnimationSampleSeconds = 0.6;
    private static List<Transform> playAnimationBones;
    private static List<Vector3> playAnimationBefore;
    private static string playAnimationClockBefore;
    private static double playAnimationSampledAt;
    private static string playAnimationAdvance;

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
        // SuperController answers a load it cannot start with a warning and then returns, so a scene
        // that never loaded leaves no error and no exception behind at all. The gate has to see it,
        // which means looking at this one warning before warnings are discarded.
        if (message.IndexOf("Can't load another until complete", StringComparison.Ordinal) >= 0)
        {
            playLoadRefused = true;
        }

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
            RequestPlayScene(elapsed);
        }

        TrackPlayScene();

        if (elapsed < playSeconds)
        {
            return;
        }

        // The requested scene has the run's remaining time to finish loading. Reporting in the middle of
        // a load would measure a half-built scene and call it the rebuild's fault.
        if (playSceneRequested && !playSceneLoadFinished && elapsed < playSeconds + PlaySceneGraceSeconds)
        {
            return;
        }

        // Two looks at the posed body, taken from the run's clock rather than in one pass: see the
        // note on AnimationSampleSeconds. The report waits for the second sample instead of writing
        // a still frame down as "the animation does not run".
        if (playAnimationBones == null)
        {
            SampleAnimation();
            return;
        }

        if (playAnimationAdvance == null)
        {
            if (EditorApplication.timeSinceStartup - playAnimationSampledAt < AnimationSampleSeconds)
            {
                return;
            }

            playAnimationAdvance = AnimationAdvance();
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
        string path = LogPathBase();

        if (path == null)
        {
            return;
        }

        try
        {
            File.WriteAllText(path + ".report.txt", report);
        }
        catch (Exception e)
        {
            Debug.LogWarning("----- RebuildGate play: could not write the report file: " + e.Message + " -----");
        }
    }

    /// <summary>
    /// Where the run was told to log, without an extension. Every artefact a run leaves behind goes
    /// beside its log. Null when nobody named a log file.
    /// </summary>
    private static string LogPathBase()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        for (int i = 0; i < arguments.Length - 1; i++)
        {
            if (arguments[i] == "-logFile" && arguments[i + 1] != string.Empty)
            {
                return Path.ChangeExtension(arguments[i + 1], null);
            }
        }

        return null;
    }

    /// <summary>
    /// Asks the game for the requested scene, once the game is in a state to accept it.
    ///
    /// -smokeWarmup is a lower bound, not a deadline. The game boots by loading a scene of its own
    /// (Saves/scene/MeshedVR/default.json), and SuperController.LoadInternal drops a load that arrives
    /// while another one is in flight - it logs "Already loading file ... Can't load another until
    /// complete" and returns. A warmup shorter than the boot therefore requested nothing, and the run
    /// went on to inspect the boot scene while its report claimed the request had been made.
    /// </summary>
    private static void RequestPlayScene(double elapsed)
    {
        SuperController controller = SuperController.singleton;
        if (controller == null)
        {
            ReportPlaySceneWait(elapsed, "the game has not created its SuperController yet");
            return;
        }

        if (controller.isLoading)
        {
            ReportPlaySceneWait(elapsed, "the game is still loading its own scene");
            return;
        }

        playSceneRequested = true;
        playErrorsAtLoad = PlayErrors.Count;
        EditorPrefs.SetBool(SceneDoneKey, true);
        LoadPlayScene();
    }

    /// <summary>
    /// Says once in a while why the scene has not been requested yet, so a run that waits out a long
    /// boot is distinguishable in the log from one whose -smokeScene was never reached.
    /// </summary>
    private static void ReportPlaySceneWait(double elapsed, string reason)
    {
        if (elapsed - playSceneWaitedSince < 15.0)
        {
            return;
        }

        playSceneWaitedSince = elapsed;
        Debug.Log(string.Format("----- RebuildGate play: waiting to load {0} - {1} -----", playScene, reason));
    }

    /// <summary>
    /// Follows the requested load from "the game took it" to "the game finished it", and audits the
    /// result once it is over. isLoading is set before the load's coroutine starts, so a transition to
    /// true is the game taking the request, and the transition back to false is the load being over.
    /// A request that was refused never shows the first transition, which is exactly how it is caught.
    /// </summary>
    private static void TrackPlayScene()
    {
        if (!playSceneRequested || playSceneLoadFinished)
        {
            return;
        }

        SuperController controller = SuperController.singleton;
        if (controller == null)
        {
            return;
        }

        if (controller.isLoading)
        {
            playSceneLoadStarted = true;
            return;
        }

        if (!playSceneLoadStarted)
        {
            return;
        }

        playSceneLoadFinished = true;
        playSceneFinishedAt = EditorApplication.timeSinceStartup - playStartedAt;
        AuditPlayScene(controller);
    }

    /// <summary>
    /// Compares the scene file's own atom list with the atoms the game actually created.
    ///
    /// The file the game was told to load names every atom it contains, and this reads it back through
    /// the game's own file layer, package addressing included. A scene that loaded has those atoms; the
    /// boot scene does not have them. That difference is the only thing separating "the requested scene
    /// is on screen" from "the game never left its boot scene", and reporting the second as the first is
    /// what made a T-posed, unlit default Person look like a defect in the rebuilt renderer.
    /// </summary>
    private static void AuditPlayScene(SuperController controller)
    {
        try
        {
            using (MVR.FileManagement.FileEntryStreamReader reader = MVR.FileManagement.FileManager.OpenStreamReader(playScene, true))
            {
                SimpleJSON.JSONNode root = SimpleJSON.JSON.Parse(reader.ReadToEnd());
                SimpleJSON.JSONArray atoms = root["atoms"].AsArray;
                HashSet<string> present = new HashSet<string>(controller.GetAtomUIDs());

                playSceneDeclared = 0;
                foreach (SimpleJSON.JSONNode atom in atoms)
                {
                    string id = atom["id"];
                    if (id == null || id == string.Empty)
                    {
                        continue;
                    }

                    playSceneDeclared++;
                    if (present.Contains(id))
                    {
                        playScenePresent++;
                    }
                    else
                    {
                        playSceneMissing.Add(id);
                    }
                }
            }
        }
        catch (Exception e)
        {
            playSceneAuditError = e.GetType().Name + ": " + e.Message;
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

        // A load the game refused is not a load. It is also the quietest failure this gate has: the
        // request is dropped with a warning, no error, and the scene already on screen stays there, so
        // the run looks exactly like a successful load of a scene nobody asked for.
        if (playLoadRefused)
        {
            return false;
        }

        // Being told to load a scene counts for nothing; having that scene on screen counts. The check
        // is the scene's own atom list against the atoms that exist, because the boot scene and the
        // scene that was asked for do not share their atoms, and until this check existed a run that
        // never loaded the Cyber demo passed the gate and reported its boot-scene Person as a defect.
        if (playScene != null && playScene != string.Empty)
        {
            if (!playSceneLoadStarted || !playSceneLoadFinished || playSceneAuditError != null)
            {
                return false;
            }

            if (playSceneDeclared > 0 && playSceneMissing.Count > 0)
            {
                return false;
            }
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

    /// <summary>Scene instances of a type, i.e. without the prefabs and package assets that share it.</summary>
    private static List<T> SceneObjects<T>() where T : UnityEngine.Object
    {
        List<T> found = new List<T>();
        foreach (UnityEngine.Object candidate in Resources.FindObjectsOfTypeAll(typeof(T)))
        {
            T item = candidate as T;
            if (item != null && !EditorUtility.IsPersistent(item))
            {
                found.Add(item);
            }
        }

        return found;
    }

    private static string Describe(UnityEngine.Object value)
    {
        return value == null ? "NULL" : "\"" + value.name + "\"";
    }

    /// <summary>
    /// Reads a field the report has no access to, so that a protected flag can still be printed.
    /// Arrays are printed as their length: "is the buffer there" is what a diagnostic needs from one.
    /// </summary>
    private static string Flag(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            return "<no field " + name + ">";
        }

        object value = field.GetValue(target);
        if (value == null)
        {
            return "NULL";
        }

        Array array = value as Array;
        return array != null ? array.Length.ToString() : value.ToString();
    }

    private static string MaterialSummary(Material[] materials)
    {
        if (materials == null)
        {
            return "NULL";
        }

        StringBuilder text = new StringBuilder(materials.Length.ToString());
        for (int i = 0; i < materials.Length; i++)
        {
            text.Append(i == 0 ? ": " : ", ");
            text.Append(materials[i] == null
                ? "NULL"
                : (materials[i].shader == null ? materials[i].name + " (no shader)" : materials[i].shader.name));
        }

        return text.ToString();
    }

    /// <summary>True when any of the materials draws with one of the DAZ character shaders.</summary>
    private static bool UsesCharacterShader(Material[] materials)
    {
        if (materials == null)
        {
            return false;
        }

        foreach (Material material in materials)
        {
            if (material != null && material.shader != null &&
                material.shader.name.IndexOf("Subsurface", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Reads a vertex array field by name, protected or not. Null when the field is missing.</summary>
    private static Vector3[] VectorField(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return field == null ? null : field.GetValue(target) as Vector3[];
    }

    /// <summary>How many vertices of one array are not the same as the matching vertex of the other.</summary>
    private static string Drift(Vector3[] first, Vector3[] second)
    {
        if (first == null || second == null)
        {
            return "n/a";
        }

        int count = Math.Min(first.Length, second.Length);
        int moved = 0;
        for (int i = 0; i < count; i++)
        {
            if ((first[i] - second[i]).sqrMagnitude > 1e-10f)
            {
                moved++;
            }
        }

        return string.Format("{0}/{1}", moved, count);
    }

    /// <summary>
    /// Reads a compute buffer back to the CPU. VaM creates its vertex buffers one element longer than the
    /// mesh, so the last entry of the result is not a vertex.
    /// </summary>
    private static Vector3[] ReadVertices(ComputeBuffer buffer)
    {
        if (buffer == null || buffer.count == 0)
        {
            return null;
        }

        try
        {
            Vector3[] data = new Vector3[buffer.count];
            buffer.GetData(data);
            return data;
        }
        catch (Exception e)
        {
            Debug.LogWarning("----- RebuildGate play: could not read a vertex buffer back: " + e.Message + " -----");
            return null;
        }
    }

    /// <summary>
    /// Compares the vertices the body is drawn from with the mesh those vertices belong to.
    ///
    /// The drawn array is the "verts" compute buffer that DrawMeshGPU hands to the ComputeBuff shaders,
    /// and the mesh is the unskinned geometry the skinning is supposed to be applied to. Vertices that
    /// match are vertices the skinning moved nowhere - the whole array matching is a body whose pose is
    /// on screen only in the bind pose, which is what a character in a T-pose turns out to be.
    ///
    /// This is deliberately not measured off rawSkinnedVerts or startVerts: on the merged path those hold
    /// the bind pose plus morphs whatever the state of the skinning.
    /// </summary>
    private static string DrawnVertexDrift(DAZSkinV2 skin)
    {
        Vector3[] drawn = ReadVertices(skin.rawVertsBuffer);
        Mesh mesh = skin.GetMesh();
        if (drawn == null)
        {
            return "drawn=n/a (rawVertsBuffer=NULL)";
        }

        if (mesh == null)
        {
            return string.Format("drawn={0} verts, mesh=none", drawn.Length);
        }

        Vector3[] unskinned = mesh.vertices;
        return string.Format("drawn={0} verts (buffer {1} slots), {2} of them differ from the unskinned mesh ({3})",
            drawn.Length, skin.rawVertsBuffer.count, Drift(drawn, unskinned), unskinned.Length);
    }

    /// <summary>The field of any name on an object, public or not, or null when there is no such field.</summary>
    private static object Field(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return field == null ? null : field.GetValue(target);
    }

    /// <summary>Bounds around the first <paramref name="count"/> points. False when there are none.</summary>
    private static bool PointsBounds(Vector3[] points, int count, out Bounds bounds)
    {
        bounds = new Bounds(Vector3.zero, Vector3.zero);
        if (points == null)
        {
            return false;
        }

        count = Math.Min(count, points.Length);
        if (count == 0)
        {
            return false;
        }

        bounds = new Bounds(points[0], Vector3.zero);
        for (int i = 1; i < count; i++)
        {
            bounds.Encapsulate(points[i]);
        }

        return true;
    }

    private static Vector3[] Transformed(Vector3[] points, Matrix4x4 matrix, int count)
    {
        count = Math.Min(count, points.Length);
        Vector3[] result = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = matrix.MultiplyPoint3x4(points[i]);
        }

        return result;
    }

    private static string BoxText(bool present, Bounds bounds)
    {
        return present
            ? string.Format("center {0} size {1}", bounds.center.ToString("F2"), bounds.size.ToString("F2"))
            : "n/a";
    }

    /// <summary>
    /// Where the drawn body sits, next to where its own skeleton sits.
    ///
    /// The drawn body goes to the screen through Graphics.DrawMesh with the identity matrix, so the
    /// vertices the skinning writes have to be world positions already. A body whose vertices come out in
    /// a frame of their own is drawn away from its own skeleton, and the parts of the character that are
    /// driven through another path - the hair, which has its own copy of the skeleton under FemaleHair -
    /// stay where the skeleton is and so appear to hang on their own next to a body that is not there.
    ///
    /// The bind pose box is the payload of the whole comparison: the drawn vertices expressed in the
    /// root's frame are the same box as the bind pose, in the same size, when the body on screen is the
    /// bind pose - a T-pose is exactly that, and a T-pose cannot be told from a posed body by flags.
    /// </summary>
    private static string SpatialReport(DAZSkinV2 skin)
    {
        Mesh mesh = skin.GetMesh();
        if (mesh == null)
        {
            return "  space: no mesh";
        }

        Vector3[] drawn = ReadVertices(skin.rawVertsBuffer);
        Vector3[] bind = skin.dazMesh == null ? null : skin.dazMesh.baseVertices;
        Transform root = skin.root == null ? null : skin.root.transform;
        DAZBone[] bones = Field(skin, "dazBones") as DAZBone[];

        StringBuilder report = new StringBuilder();
        report.AppendLine(string.Format("  space: root={0} at {1}, root scale {2}, drawOffset={3}, smoothing={4}",
            root == null ? "NULL" : TransformPath(root),
            root == null ? "n/a" : root.position.ToString("F2"),
            root == null ? "n/a" : root.lossyScale.ToString("F3"),
            skin.drawOffset.ToString("F2"), skin.useSmoothing));

        Bounds box;
        bool hasDrawn = PointsBounds(drawn, mesh.vertexCount, out box);
        report.AppendLine(string.Format("    drawn in world: {0}", BoxText(hasDrawn, box)));

        Bounds rootBox = new Bounds();
        bool hasDrawnInRoot = false;
        if (hasDrawn && root != null)
        {
            hasDrawnInRoot = PointsBounds(
                Transformed(drawn, root.worldToLocalMatrix, mesh.vertexCount), mesh.vertexCount, out rootBox);
        }

        Bounds bindInRoot = new Bounds();
        bool hasBind = PointsBounds(bind, bind == null ? 0 : bind.Length, out bindInRoot);
        Bounds bindInWorld = new Bounds();
        bool hasBindInWorld = hasBind && root != null;
        if (hasBindInWorld)
        {
            hasBindInWorld = PointsBounds(
                Transformed(bind, root.localToWorldMatrix, bind.Length), bind.Length, out bindInWorld);
        }

        report.AppendLine(string.Format("    bind pose in world: {0}", BoxText(hasBindInWorld, bindInWorld)));
        report.AppendLine(string.Format("    drawn in root frame: {0}", BoxText(hasDrawnInRoot, rootBox)));
        report.AppendLine(string.Format("    bind pose in root frame: {0}", BoxText(hasBind, bindInRoot)));

        if (bones != null && bones.Length > 0)
        {
            Vector3[] bonePoints = new Vector3[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                bonePoints[i] = bones[i].transform.position;
            }

            Bounds boneBox;
            PointsBounds(bonePoints, bonePoints.Length, out boneBox);
            report.AppendLine(string.Format("    skeleton in world: {0}, {1} bones, first at {2}",
                BoxText(true, boneBox), bones.Length, bonePoints[0].ToString("F2")));
        }

        report.AppendLine(AncestorChain(skin.transform));

        return report.ToString().TrimEnd('\r', '\n');
    }

    /// <summary>Renders one camera into a PNG beside the report, and returns the path.</summary>
    private static string RenderToFile(Camera camera, int width, int height, string suffix)
    {
        string path = LogPathBase();
        if (path == null)
        {
            return null;
        }

        string file = path + suffix + ".png";
        RenderTexture target = RenderTexture.GetTemporary(width, height, 24);
        RenderTexture active = RenderTexture.active;
        try
        {
            camera.targetTexture = target;
            camera.Render();

            RenderTexture.active = target;
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            image.Apply();
            File.WriteAllBytes(file, image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);
            return file;
        }
        finally
        {
            RenderTexture.active = active;
            camera.targetTexture = null;
            RenderTexture.ReleaseTemporary(target);
        }
    }

    /// <summary>
    /// Renders the body from a corner, framed on the body and on the skeleton that is supposed to be
    /// driving it, so that one frame shows whether the two are in the same place.
    /// </summary>
    private static string CaptureSkin(DAZSkinV2 skin)
    {
        if (skin == null)
        {
            return "picture: skipped, no skin in the scene owns a mesh";
        }

        Vector3[] drawn = ReadVertices(skin.rawVertsBuffer);
        Mesh mesh = skin.GetMesh();
        if (drawn == null || mesh == null)
        {
            return "picture: skipped, the body has no drawn vertices";
        }

        Bounds bounds;
        if (!PointsBounds(drawn, mesh.vertexCount, out bounds))
        {
            return "picture: skipped, the body has no drawn vertices";
        }

        DAZBone[] bones = Field(skin, "dazBones") as DAZBone[];
        if (bones != null)
        {
            foreach (DAZBone bone in bones)
            {
                bounds.Encapsulate(bone.transform.position);
            }
        }

        float radius = Mathf.Max(bounds.extents.magnitude, 0.001f);
        float distance = radius / Mathf.Tan(45f * 0.5f * Mathf.Deg2Rad) * 1.5f;

        GameObject holder = new GameObject("RebuildGateCapture");
        try
        {
            Camera camera = holder.AddComponent<Camera>();
            camera.enabled = false;
            camera.fieldOfView = 45f;
            camera.nearClipPlane = Mathf.Max(0.01f, distance - radius * 3f);
            camera.farClipPlane = distance + radius * 6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.14f, 1f);
            camera.transform.position = bounds.center + new Vector3(0.7f, 0.4f, -1f).normalized * distance;
            camera.transform.LookAt(bounds.center);

            string file = RenderToFile(camera, 1024, 1024, string.Empty);
            return file == null
                ? "picture: skipped, the run was not told where to log"
                : string.Format("picture: {0} (framed on the body at {1} and its skeleton, from {2})",
                                file, bounds.center.ToString("F2"), TransformPath(skin.transform));
        }
        catch (Exception e)
        {
            return "picture: could not be rendered - " + e.Message;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(holder);
        }
    }

    /// <summary>
    /// Renders the frame the game's own camera would put on screen, which is the only frame that shows
    /// what the defect looks like rather than what the numbers say about it.
    /// </summary>
    private static string CaptureView()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            foreach (Camera candidate in Camera.allCameras)
            {
                if (candidate.enabled && candidate.gameObject.activeInHierarchy)
                {
                    camera = candidate;
                    break;
                }
            }
        }

        if (camera == null)
        {
            return "view: skipped, the scene has no enabled camera";
        }

        try
        {
            string file = RenderToFile(camera, 1024, 576, ".view");
            return file == null
                ? "view: skipped, the run was not told where to log"
                : string.Format("view: {0} (the game's camera {1}, depth {2}, at {3})",
                                file, TransformPath(camera.transform), camera.depth,
                                camera.transform.position.ToString("F2"));
        }
        catch (Exception e)
        {
            return "view: could not be rendered - " + e.Message;
        }
    }

    /// <summary>Bones worth showing side by side when two copies of a skeleton disagree.</summary>
    private static readonly string[] BoneProbes =
    {
        "hip", "abdomen", "abdomen2", "chest", "neck", "head",
        "lCollar", "lShldr", "lForeArm", "lHand", "lThigh", "lShin", "lFoot", "Ponytail01"
    };

    /// <summary>
    /// The skin the report calls the character: the switched-on skin whose mesh has the most vertices.
    /// A merged body is built from several skins, and only one of them owns the mesh that is drawn.
    /// </summary>
    private static DAZSkinV2 CharacterSkin()
    {
        DAZSkinV2 subject = null;
        int subjectVertices = 0;
        foreach (DAZSkinV2 skin in SceneObjects<DAZSkinV2>())
        {
            Mesh mesh = skin == null ? null : skin.GetMesh();
            if (mesh == null || !skin.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (mesh.vertexCount > subjectVertices)
            {
                subject = skin;
                subjectVertices = mesh.vertexCount;
            }
        }

        return subject;
    }

    /// <summary>The posed bones of the scene, i.e. the joints of every skeleton that is switched on.</summary>
    private static List<Transform> PosedBones()
    {
        List<Transform> bones = new List<Transform>();
        foreach (DAZBone bone in SceneObjects<DAZBone>())
        {
            if (bone.gameObject.activeInHierarchy)
            {
                bones.Add(bone.transform);
            }
        }

        return bones;
    }

    // A hair joint is a DAZBone too, and the hair item's own joints are driven by physics rather than
    // by the clip, so "some bone moved" is not by itself proof that the animation runs. The joints are
    // counted apart for that reason: the body's joints are the ones only a clip can move.
    private static bool IsBodyBone(Transform bone)
    {
        return bone.GetComponentInParent<DAZHairGroup>() == null;
    }

    /// <summary>Takes the first of the two looks at the posed bones. See AnimationSampleSeconds.</summary>
    private static void SampleAnimation()
    {
        playAnimationBones = PosedBones();
        playAnimationBefore = new List<Vector3>(playAnimationBones.Count);
        foreach (Transform bone in playAnimationBones)
        {
            playAnimationBefore.Add(bone.position);
        }

        playAnimationClockBefore = AnimatorClocks();
        playAnimationSampledAt = EditorApplication.timeSinceStartup;
    }

    /// <summary>
    /// The clip each switched-on animator is in, and how far its clock is into it. This is the animator
    /// speaking for itself, where the bone positions are the result.
    /// </summary>
    private static string AnimatorClocks()
    {
        StringBuilder clocks = new StringBuilder();
        foreach (Animator animator in SceneObjects<Animator>())
        {
            if (animator == null || !animator.gameObject.activeInHierarchy ||
                animator.runtimeAnimatorController == null)
            {
                continue;
            }

            if (clocks.Length > 0)
            {
                clocks.Append(", ");
            }

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
            clocks.Append(string.Format("{0}: {1} at normalizedTime {2:F3}", animator.name,
                clips.Length == 0 || clips[0].clip == null ? "no clip" : "\"" + clips[0].clip.name + "\"",
                state.normalizedTime));
        }

        return clocks.Length == 0 ? "no animator is running a clip" : clocks.ToString();
    }

    /// <summary>Reports how far the bones moved between the two samples.</summary>
    private static string AnimationAdvance()
    {
        int bodyMoved = 0;
        int bodyBones = 0;
        int hairMoved = 0;
        int hairBones = 0;
        string furthest = "none";
        float far = 0f;
        for (int i = 0; i < playAnimationBones.Count && i < playAnimationBefore.Count; i++)
        {
            Transform bone = playAnimationBones[i];
            if (bone == null)
            {
                continue;
            }

            bool body = IsBodyBone(bone);
            float distance = Vector3.Distance(bone.position, playAnimationBefore[i]);
            bool moved = distance > 0.0001f;
            if (body)
            {
                bodyBones++;
                if (moved)
                {
                    bodyMoved++;
                }
            }
            else
            {
                hairBones++;
                if (moved)
                {
                    hairMoved++;
                }
            }

            if (distance > far)
            {
                far = distance;
                furthest = bone.name;
            }
        }

        return string.Format(
            "{0} bones sampled {1:F2} s apart: body {2}/{3} moved, hair joints {4}/{5} moved, furthest {6} by {7:F4} m",
            playAnimationBones.Count, EditorApplication.timeSinceStartup - playAnimationSampledAt,
            bodyMoved, bodyBones, hairMoved, hairBones, furthest, far);
    }

    /// <summary>
    /// The animation state, and whether it is running.
    ///
    /// A posed body can be a still frame: the scene stores a whole sequence of clips, one of them is
    /// played, and a rebuild that plays none of them draws the scene's pose once and stops. The scene's
    /// stored sequence is what plays - RestoreFromJSON plays animationSequence.First - so the clip the
    /// animator is actually in and the bone advance over time have to be read together: the clip says
    /// what the game chose, the clock and the bones say whether it is running.
    ///
    /// "animationSelection" is deliberately printed next to them: it is the entry the scene author had
    /// highlighted in the clip picker, it is stored in the scene, and it is never read by any code - it
    /// is only handed to the AddAnimationToSequence action as that action's argument. A scene whose
    /// selection and sequence name different clips is therefore not a contradiction.
    /// </summary>
    private static string AnimationReport()
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("----- animation -----");

        List<UnityAnimatorControl> controls = SceneObjects<UnityAnimatorControl>();
        report.AppendLine(string.Format("UnityAnimatorControl components in the scene: {0}", controls.Count));
        for (int i = 0; i < controls.Count && i < 3; i++)
        {
            UnityAnimatorControl control = controls[i];
            report.AppendLine(string.Format("  {0}: active={1}, animatorEnabled={2}, animatorSpeed={3}, currentAnimationName={4}",
                TransformPath(control.transform), control.gameObject.activeInHierarchy,
                Flag(control, "_animatorEnabled"), Flag(control, "_animatorSpeed"),
                Flag(control, "_currentAnimationName")));
            report.AppendLine(string.Format("    selection stored in the scene: {0}",
                Flag(control, "_animationSelection")));
        }

        int running = 0;
        foreach (Animator animator in SceneObjects<Animator>())
        {
            if (animator == null || !animator.gameObject.activeInHierarchy ||
                animator.runtimeAnimatorController == null)
            {
                continue;
            }

            running++;
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            report.AppendLine(string.Format(
                "  {0}: controller={1}, speed={2:F2}, state hash={3}, length={4:F2} s, normalizedTime={5:F3}",
                TransformPath(animator.transform), Describe(animator.runtimeAnimatorController), animator.speed,
                state.shortNameHash.ToString("X8"), state.length, state.normalizedTime));
            AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
            for (int c = 0; c < clips.Length && c < 4; c++)
            {
                report.AppendLine(string.Format("    clip: {0} ({1:F2} s), weight {2:F2}",
                    clips[c].clip == null ? "NULL" : clips[c].clip.name,
                    clips[c].clip == null ? 0f : clips[c].clip.length, clips[c].weight));
            }
        }

        report.AppendLine(string.Format("animators with a controller and switched on: {0}", running));
        report.AppendLine(string.Format("animator clock: {0}", AnimatorClocks()));
        report.AppendLine(string.Format("animator clock, the earlier sample: {0}", playAnimationClockBefore == null ? "not sampled" : playAnimationClockBefore));
        report.AppendLine(string.Format("bone advance: {0}",
            playAnimationAdvance == null ? "not sampled" : playAnimationAdvance));
        return report.ToString();
    }

    /// <summary>
    /// Groups every DAZBone in the scene into the skeleton it belongs to, by walking up to the topmost
    /// bone of the chain. A DAZ character carries one such skeleton, and an item worn by that character
    /// carries another: hair embeds a full copy of the joint hierarchy (hip, chest, head, Ponytail01...)
    /// so that one item can be fitted to any body. The copy is meant to be driven by the body's copy.
    /// Nothing in the skinning path compares the two, and the copy holds rest rotations of its own, so an
    /// undriven copy skins the hair as if the character were standing in its rest pose while the body is
    /// drawn in the pose the scene asked for. The two then line up only by coincidence, which is what a
    /// head of hair sitting away from its body looks like.
    ///
    /// So the copies are matched bone by bone by name and their local rotations compared: a driven copy
    /// agrees with the body, an undriven one keeps the rest rotations and disagrees.
    ///
    /// Only skeletons that are switched on are compared. A hair slot the scene does not use stays in the
    /// scene with its own copy of the joints, switched off and left in the rest pose it was built with,
    /// so comparing it with the live body finds a whole skeleton rotated and metres away and reads as a
    /// defect when it is a disabled slot. Every skeleton is still listed, with its state printed next to
    /// it, so the harmless case is visible rather than absent.
    /// </summary>
    private static string SkeletonReport(DAZSkinV2 subject)
    {
        Dictionary<Transform, List<DAZBone>> forest = new Dictionary<Transform, List<DAZBone>>();
        foreach (DAZBone bone in SceneObjects<DAZBone>())
        {
            Transform top = bone.transform.parent;
            while (top != null && top.GetComponent<DAZBone>() != null)
            {
                top = top.parent;
            }

            List<DAZBone> chain;
            if (!forest.TryGetValue(top, out chain))
            {
                chain = new List<DAZBone>();
                forest[top] = chain;
            }

            chain.Add(bone);
        }

        if (forest.Count == 0)
        {
            return "  skeletons: no bones in the scene";
        }

        string referenceName = subject != null && subject.root != null ? subject.root.gameObject.name : null;
        Dictionary<string, Quaternion> referenceRotations = new Dictionary<string, Quaternion>();
        Dictionary<string, Vector3> referencePositions = new Dictionary<string, Vector3>();
        Transform referenceTop = null;
        foreach (KeyValuePair<Transform, List<DAZBone>> candidate in forest)
        {
            if (referenceName != null && candidate.Key != null && candidate.Key.name == referenceName)
            {
                referenceTop = candidate.Key;
                break;
            }
        }

        if (referenceTop != null)
        {
            foreach (DAZBone bone in forest[referenceTop])
            {
                referenceRotations[bone.gameObject.name] = bone.transform.localRotation;
                referencePositions[bone.gameObject.name] = bone.transform.position;
            }
        }

        StringBuilder report = new StringBuilder();
        foreach (KeyValuePair<Transform, List<DAZBone>> entry in forest)
        {
            List<DAZBone> chain = entry.Value;
            Vector3[] points = new Vector3[chain.Count];
            Dictionary<string, Quaternion> rotations = new Dictionary<string, Quaternion>();
            Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();
            for (int i = 0; i < chain.Count; i++)
            {
                points[i] = chain[i].transform.position;
                rotations[chain[i].gameObject.name] = chain[i].transform.localRotation;
                positions[chain[i].gameObject.name] = chain[i].transform.position;
            }

            Bounds box;
            PointsBounds(points, points.Length, out box);
            bool isReference = entry.Key == referenceTop;
            bool isActive = entry.Key != null && entry.Key.gameObject.activeInHierarchy;
            report.AppendLine(string.Format("  skeleton {0}: {1} bones, world {2}{3}",
                entry.Key == null ? "(no parent)" : TransformPath(entry.Key), chain.Count,
                BoxText(true, box),
                isReference ? " <- the body" : isActive ? string.Empty : " <- inactive in the hierarchy"));

            if (isReference)
            {
                report.AppendLine(AncestorChain(entry.Key));
                continue;
            }

            if (referenceTop == null)
            {
                continue;
            }

            if (!isActive)
            {
                report.AppendLine(
                    "    not compared: the slot is switched off, so its bones keep the rest pose they were built with");
                report.AppendLine(AncestorChain(entry.Key));
                continue;
            }

            int shared = 0;
            int differing = 0;
            foreach (KeyValuePair<string, Quaternion> bone in rotations)
            {
                Quaternion reference;
                if (!referenceRotations.TryGetValue(bone.Key, out reference))
                {
                    continue;
                }

                shared++;
                if (Quaternion.Angle(bone.Value, reference) > 1f)
                {
                    differing++;
                }
            }

            report.AppendLine(string.Format(
                "    {0} of {1} bones share a name with the body, {2} of those are rotated differently",
                shared, chain.Count, differing));
            report.AppendLine(AncestorChain(entry.Key));

            foreach (string probe in BoneProbes)
            {
                Quaternion own;
                Quaternion reference;
                Vector3 ownAt;
                Vector3 referenceAt;
                if (!rotations.TryGetValue(probe, out own) || !referenceRotations.TryGetValue(probe, out reference))
                {
                    continue;
                }

                if (!positions.TryGetValue(probe, out ownAt) || !referencePositions.TryGetValue(probe, out referenceAt))
                {
                    continue;
                }

                report.AppendLine(string.Format("    {0}: ours {1} body {2} - {3:F1} deg apart, sitting {4:F2} m apart",
                    probe, own.eulerAngles.ToString("F1"), reference.eulerAngles.ToString("F1"),
                    Quaternion.Angle(own, reference), Vector3.Distance(ownAt, referenceAt)));
            }
        }

        return report.ToString().TrimEnd('\r', '\n');
    }

    /// <summary>
    /// Reports how the hair items of a scene are wired to the body.
    ///
    /// A hair item ships its own copy of the character's joint hierarchy, and DAZHairGroup is the
    /// component that points the item's skins at the skeleton that is meant to drive them
    /// (rootBonesForSkinning). A group that never received that reference leaves its skins bound to the
    /// item's own copy, which keeps the import rotations and the import position of the item, so the
    /// hair is skinned as if the character still stood where the item was imported while the body
    /// stands in the pose the scene asked for. The group and every skin under it are printed with the
    /// skeleton each one actually resolves to, together with the world positions of the two ancestor
    /// chains, so the skeleton the skins use and the skeleton the body uses can be compared by path.
    /// </summary>
    private static string HairReport()
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("----- hair -----");

        List<DAZHairGroup> groups = SceneObjects<DAZHairGroup>();
        report.AppendLine(string.Format("DAZHairGroup components in the scene: {0}", groups.Count));

        foreach (DAZHairGroup group in groups)
        {
            report.AppendLine(string.Format("  {0}: active={1}, uid={2}, type={3}, at {4}",
                TransformPath(group.transform), group.gameObject.activeInHierarchy, group.uid,
                group.type, group.transform.position.ToString("F2")));
            report.AppendLine(string.Format("    rootBonesForSkinning={0}",
                group.rootBonesForSkinning == null
                    ? "NULL"
                    : TransformPath(group.rootBonesForSkinning.transform)));
            report.AppendLine(string.Format("    skin={0}",
                group.skin == null ? "NULL" : TransformPath(group.skin.transform)));
            report.AppendLine(string.Format("    positionLink={0}, isDynamicRuntimeLoaded={1}, drawRigidOnBone={2}",
                Flag(group, "positionLink"), group.isDynamicRuntimeLoaded,
                group.drawRigidOnBone == null ? "NULL" : TransformPath(group.drawRigidOnBone.transform)));

            DAZSkinV2[] owned = group.GetComponentsInChildren<DAZSkinV2>(true);
            for (int i = 0; i < owned.Length; i++)
            {
                Mesh mesh = owned[i].GetMesh();
                report.AppendLine(string.Format("    own skin {0}: mesh={1}, skin={2}, draw={3}, root={4}",
                    TransformPath(owned[i].transform), mesh == null ? "none" : mesh.name,
                    owned[i].skin, owned[i].draw,
                    owned[i].root == null ? "NULL" : TransformPath(owned[i].root.transform)));
            }

            report.AppendLine(AncestorChain(group.transform));
        }

        return report.ToString().TrimEnd('\r', '\n');
    }

    /// <summary>
    /// Prints the world position of every ancestor of a transform, from the topmost one down. Two
    /// objects that are described as sharing a parent can sit metres apart if one of them carries an
    /// offset, and only the world position of each node tells which of the two moved.
    /// </summary>
    private static string AncestorChain(Transform transform)
    {
        StringBuilder report = new StringBuilder();
        List<Transform> chain = new List<Transform>();
        for (Transform node = transform; node != null; node = node.parent)
        {
            chain.Add(node);
        }

        chain.Reverse();
        report.AppendLine(string.Format("    chain of {0}:", TransformPath(transform)));
        for (int i = 0; i < chain.Count; i++)
        {
            report.AppendLine(string.Format("      {0} {1}: at {2}, local {3}, active={4}",
                new string(' ', i), chain[i].name, chain[i].position.ToString("F2"),
                chain[i].localPosition.ToString("F2"), chain[i].gameObject.activeInHierarchy));
        }

        return report.ToString().TrimEnd('\r', '\n');
    }

    /// <summary>
    /// Reports how far the skinning of a loaded scene's characters got.
    ///
    /// VaM skins a body through compute shaders rather than through a SkinnedMeshRenderer, and the
    /// pipeline that owns a merged body skin is not DAZSkinV2 at all: DAZCharacterRun connects to the
    /// merged skin, switches the skin's own skin/draw off, skins on its own threads and draws the result
    /// itself. A body that stops halfway through that pipeline keeps its raw bind pose geometry, which
    /// reaches the screen as a T-posed body standing in for a character whose actual pose never gets
    /// drawn. Nothing is logged when it happens, so the state has to be read off the objects.
    ///
    /// Every flag that decides whether the body is drawn is printed: the skins that own a mesh, the run
    /// that drives the merged one, the renderer each skin sits on and whether that renderer is still
    /// enabled, plus how many of the vertices the body is drawn from the skinning actually moved. The
    /// body itself is rendered into a PNG beside the report, because a body drawn in its bind pose and a
    /// body that is not drawn at all print the same flags and look nothing alike.
    /// </summary>
    /// <summary>
    /// Which renderer is putting the character on the screen, and where.
    ///
    /// The body of a VaM character is not drawn by a renderer of its own: DAZSkinV2 draws it with
    /// Graphics.DrawMesh from a compute buffer, and skips any submesh whose material is null or whose
    /// material flag is off. Whatever else is holding a character-sized mesh - a MeshRenderer left with
    /// the bind pose mesh that DrawMeshGPU would have switched off, or the DAZMesh fallback to a single
    /// unlit simpleMaterial, which is what a fullbright body without shadows is - is then what the eye
    /// sees, at the place that renderer's transform puts it.
    /// </summary>
    private static string DrawReport(DAZSkinV2 subject)
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("----- draw -----");

        List<string> lines = new List<string>();
        foreach (SkinnedMeshRenderer renderer in SceneObjects<SkinnedMeshRenderer>())
        {
            Mesh mesh = renderer.sharedMesh;
            if (mesh == null || mesh.vertexCount < 3000)
            {
                continue;
            }

            lines.Add(string.Format(
                "  SMR {0}: enabled={1}, visible={2}, verts={3}, world={4}, position={5}, materials={6}",
                TransformPath(renderer.transform), renderer.enabled, renderer.isVisible, mesh.vertexCount,
                BoxText(true, renderer.bounds), renderer.transform.position.ToString("F2"),
                MaterialSummary(renderer.sharedMaterials)));
        }

        foreach (MeshRenderer renderer in SceneObjects<MeshRenderer>())
        {
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            Mesh mesh = filter == null ? null : filter.sharedMesh;
            if (mesh == null || mesh.vertexCount < 3000)
            {
                continue;
            }

            lines.Add(string.Format(
                "  MR {0}: enabled={1}, visible={2}, verts={3}, world={4}, position={5}, materials={6}",
                TransformPath(renderer.transform), renderer.enabled, renderer.isVisible, mesh.vertexCount,
                BoxText(true, renderer.bounds), renderer.transform.position.ToString("F2"),
                MaterialSummary(renderer.sharedMaterials)));
        }

        report.AppendLine(string.Format("DAZSkinV2.staticDraw={0}, renderers holding a mesh of 3000+ vertices: {1}",
                                        DAZSkinV2.staticDraw, lines.Count));
        lines.Sort(string.CompareOrdinal);
        for (int i = 0; i < lines.Count && i < 30; i++)
        {
            report.AppendLine(lines[i]);
        }

        if (lines.Count > 30)
        {
            report.AppendLine(string.Format("  ... {0} more", lines.Count - 30));
        }

        if (subject == null)
        {
            report.AppendLine("no subject skin to inspect");
            return report.ToString();
        }

        report.AppendLine(string.Format("subject {0}", TransformPath(subject.transform)));
        report.AppendLine(string.Format(
            "    GPUuseSimpleMaterial={0}, GPUsimpleMaterial={1} ({2}), renderSuspend={3}",
            subject.GPUuseSimpleMaterial, Describe(subject.GPUsimpleMaterial),
            subject.GPUsimpleMaterial == null || subject.GPUsimpleMaterial.shader == null
                ? "NULL"
                : subject.GPUsimpleMaterial.shader.name,
            subject.renderSuspend));
        report.AppendLine(string.Format("    GPUmaterials={0}", MaterialSummary(subject.GPUmaterials)));

        if (subject.materialsEnabled != null)
        {
            StringBuilder flags = new StringBuilder();
            for (int i = 0; i < subject.materialsEnabled.Length; i++)
            {
                flags.Append(subject.materialsEnabled[i] ? "1" : "0");
            }

            report.AppendLine(string.Format("    materialsEnabled={0}", flags.ToString()));
        }

        MeshFilter ownFilter = subject.GetComponent<MeshFilter>();
        MeshRenderer ownRenderer = subject.GetComponent<MeshRenderer>();
        Mesh ownMesh = ownFilter == null ? null : ownFilter.sharedMesh;
        report.AppendLine(string.Format("    own MeshFilter={0}, own MeshRenderer={1}",
            ownFilter == null ? "none" : Describe(ownMesh),
            ownRenderer == null
                ? "none"
                : string.Format("enabled={0}, visible={1}, mesh={2}, materials={3}",
                                ownRenderer.enabled, ownRenderer.isVisible,
                                ownMesh == null ? "NULL" : ownMesh.name,
                                MaterialSummary(ownRenderer.sharedMaterials))));

        DAZMesh dazMesh = subject.dazMesh;
        if (dazMesh == null)
        {
            report.AppendLine("    dazMesh=NULL");
            return report.ToString();
        }

        report.AppendLine(string.Format(
            "    DAZMesh {0}: useSimpleMaterial={1}, simpleMaterial={2} ({3}), use2PassMaterials={4}",
            TransformPath(dazMesh.transform), dazMesh.useSimpleMaterial, Describe(dazMesh.simpleMaterial),
            dazMesh.simpleMaterial == null || dazMesh.simpleMaterial.shader == null
                ? "NULL"
                : dazMesh.simpleMaterial.shader.name,
            dazMesh.use2PassMaterials));
        report.AppendLine(string.Format("    materials={0}", MaterialSummary(dazMesh.materials)));
        report.AppendLine(string.Format("    materialsPass1={0}", MaterialSummary(dazMesh.materialsPass1)));
        report.AppendLine(string.Format("    morphedUVMappedMesh={0}, baseMesh={1}, morphedBaseMesh={2}",
            dazMesh.morphedUVMappedMesh == null
                ? "NULL"
                : dazMesh.morphedUVMappedMesh.name + " (" + dazMesh.morphedUVMappedMesh.vertexCount + " verts)",
            Describe(Field(dazMesh, "_baseMesh") as UnityEngine.Object),
            Describe(Field(dazMesh, "_morphedBaseMesh") as UnityEngine.Object)));

        MeshFilter meshFilter = dazMesh.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = dazMesh.GetComponent<MeshRenderer>();
        Mesh shownMesh = meshFilter == null ? null : meshFilter.sharedMesh;
        report.AppendLine(string.Format("    MeshFilter={0}, MeshRenderer={1}",
            meshFilter == null ? "none" : Describe(shownMesh),
            meshRenderer == null
                ? "none"
                : string.Format("enabled={0}, visible={1}, mesh={2}, materials={3}",
                                meshRenderer.enabled, meshRenderer.isVisible,
                                shownMesh == null ? "NULL" : shownMesh.name,
                                MaterialSummary(meshRenderer.sharedMaterials))));

        return report.ToString();
    }

    private static string SkinReport()
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("----- skin -----");

        try
        {
            report.AppendLine(string.Format("compute shader assets in the project: {0}",
                                            AssetDatabase.FindAssets("t:ComputeShader").Length));

            List<MeshVR.DAZImport> imports = SceneObjects<MeshVR.DAZImport>();
            report.AppendLine(string.Format("DAZImport components in the scene: {0}", imports.Count));
            for (int i = 0; i < imports.Count && i < 6; i++)
            {
                MeshVR.DAZImport import = imports[i];
                report.AppendLine(string.Format("  {0}: GPUSkinCompute={1}, GPUMeshCompute={2}",
                    TransformPath(import.transform), Describe(import.GPUSkinCompute), Describe(import.GPUMeshCompute)));
            }

            report.AppendLine(HairReport());

            List<DAZSkinV2> skins = SceneObjects<DAZSkinV2>();
            report.AppendLine(string.Format(
                "DAZSkinV2 components in the scene: {0}, staticDraw={1}",
                skins.Count, DAZSkinV2.staticDraw));

            // A skin whose mesh is null is one of the source skins a merge consumed, and it is correct
            // for those to be idle - one line each. A skin that owns a mesh is a skin that has to reach
            // the screen, so those get every flag that decides whether it does.
            List<string> idleSkins = new List<string>();
            DAZSkinV2 subject = CharacterSkin();
            foreach (DAZSkinV2 skin in skins)
            {
                Mesh mesh = skin.GetMesh();
                if (mesh == null)
                {
                    idleSkins.Add(string.Format("  {0}: mesh=none, wasInit={1}, skin={2}, draw={3}, method={4}, GPUSkinner={5}, root={6}",
                        TransformPath(skin.transform), skin.wasInit, skin.skin, skin.draw, skin.skinMethod,
                        Describe(skin.GPUSkinner),
                        skin.root == null ? "NULL" : TransformPath(skin.root.transform)));
                    continue;
                }

                MeshRenderer ownRenderer = skin.GetComponent<MeshRenderer>();
                report.AppendLine(string.Format(
                    "  {0}: enabled={1}, active={2}, wasInit={3}, skin={4}, draw={5}, method={6}, renderSuspend={7}",
                    TransformPath(skin.transform), skin.enabled, skin.gameObject.activeInHierarchy,
                    skin.wasInit, skin.skin, skin.draw, skin.skinMethod, skin.renderSuspend));
                report.AppendLine(string.Format("    renderer: {0}, materials={1}",
                    ownRenderer == null
                        ? "none"
                        : string.Format("enabled={0}, visible={1}", ownRenderer.enabled, ownRenderer.isVisible),
                    MaterialSummary(ownRenderer == null ? null : ownRenderer.sharedMaterials)));
                report.AppendLine(string.Format(
                    "    drawDisabled={0}, needsDispatch={1}, checkedComponents={2}, materialsEnabled={3}",
                    Flag(skin, "_updateDrawDisabled"), Flag(skin, "needsDispatch"),
                    Flag(skin, "alreadyCheckedForMeshComponents"),
                    skin.materialsEnabled == null ? "NULL" : skin.materialsEnabled.Length.ToString()));
                report.AppendLine(string.Format("    GPUSkinner={0}, GPUMeshCompute={1}, delayedVertsBuffer={2}, nodes={3}, root={4}",
                    Describe(skin.GPUSkinner), Describe(skin.GPUMeshCompute),
                    skin.delayedVertsBuffer == null ? "NULL" : "set",
                    skin.nodes == null ? "NULL" : skin.nodes.Length.ToString(),
                    skin.root == null ? "NULL" : skin.root.name));
                report.AppendLine(string.Format("    dazMesh={0}, mesh={1}",
                    skin.dazMesh == null
                        ? "NULL"
                        : string.Format("{0} ({1} base, {2} uv verts)", skin.dazMesh.geometryId,
                                        skin.dazMesh.numBaseVertices, skin.dazMesh.numUVVertices),
                    string.Format("{0} ({1} verts)", mesh.name, mesh.vertexCount)));
                report.AppendLine(string.Format("    generalWeights={0}, {1}",
                    Flag(skin, "_useGeneralWeights"), DrawnVertexDrift(skin)));
                report.AppendLine(string.Format("    GPUmaterials={0}", MaterialSummary(skin.GPUmaterials)));
            }

            foreach (string idle in idleSkins)
            {
                report.AppendLine(idle);
            }

            // The merged body of a VaM character is not drawn by DAZSkinV2 at all: DAZCharacterRun
            // connects to the merged skin, turns the skin's own skin/draw off, skins on its own threads
            // and draws the result with skin.DrawMeshGPU() from its LateUpdate. Every one of those steps
            // is a flag, and a run that stopped half way leaves the body drawn in its bind pose.
            List<DAZCharacterRun> runs = SceneObjects<DAZCharacterRun>();
            SuperController controller = SuperController.singleton;
            report.AppendLine(string.Format(
                "DAZCharacterRun components in the scene: {0}, SuperController.autoSimulation={1}",
                runs.Count, controller == null ? "no controller" : controller.autoSimulation.ToString()));
            for (int i = 0; i < runs.Count && i < 4; i++)
            {
                DAZCharacterRun run = runs[i];
                report.AppendLine(string.Format(
                    "  {0}: enabled={1}, active={2}, doUpdate={3}, doSkin={4}, doDraw={5}, useThreading={6}, renderSuspend={7}",
                    TransformPath(run.transform), run.enabled, run.gameObject.activeInHierarchy,
                    run.doUpdate, run.doSkin, run.doDraw, run.useThreading, run.renderSuspend));
                report.AppendLine(string.Format(
                    "    threadsRunning={0}, threadWasRun={1}, morphedUVVerts={2}, skin={3}, bones={4}, morphBank1={5}, morphBank2={6}, setDazMorphContainer={7}",
                    Flag(run, "_threadsRunning"), Flag(run, "threadWasRun"),
                    Flag(run, "mergedMeshMorphedUVVertices"), Describe(run.skin), Describe(run.bones),
                    Describe(run.morphBank1), Describe(run.morphBank2), Describe(run.setDazMorphContainer)));
                report.AppendLine(string.Format(
                    "    thisFrame: skinPrep={0} ms, skinFinish={1} ms, skinDraw={2} ms, threadSkin={3} ms, threadMerge={4} ms",
                    Flag(run, "MAIN_skinPrepTime"), Flag(run, "MAIN_skinFinishTime"),
                    Flag(run, "MAIN_skinDrawTime"),
                    Flag(run, "THREAD_skinTime"), Flag(run, "THREAD_mergeTime")));

                // RunThreaded skins mergedMeshMorphedUVVertices in place, so the copy the run takes one
                // step earlier is the same body before any bone moved it. Two identical arrays mean the
                // skinning wrote nothing, and the buffer the body is drawn from is holding the bind pose.
                Vector3[] skinnedVerts = VectorField(run, "mergedMeshMorphedUVVertices");
                Vector3[] morphOnlyVerts = VectorField(run, "mergedMeshMorphedUVVerticesCopy");
                report.AppendLine(string.Format(
                    "    mergedUVVerts={0}, prefixedMorphedUVVerts={1}, verts the skinning moved: {2}",
                    Flag(run, "mergedMeshMorphedUVVertices"), Flag(run, "mergedMeshMorphedUVVerticesCopy"),
                    Drift(skinnedVerts, morphOnlyVerts)));
            }

            report.AppendLine(DrawReport(subject));
            report.AppendLine(CaptureSkin(subject));
            report.AppendLine(CaptureView());
            if (subject != null)
            {
                report.AppendLine(SpatialReport(subject));
            }

            List<DAZBone> bones = SceneObjects<DAZBone>();
            int activeBones = 0;
            int rotated = 0;
            List<DAZBone> posed = new List<DAZBone>();
            foreach (DAZBone bone in bones)
            {
                if (!bone.gameObject.activeInHierarchy)
                {
                    continue;
                }

                activeBones++;
                if (Quaternion.Angle(bone.transform.localRotation, Quaternion.identity) > 1f)
                {
                    rotated++;
                    posed.Add(bone);
                }
            }

            report.AppendLine(string.Format(
                "DAZBone components in the scene: {0}, in active objects: {1}, of those rotated away from identity: {2}",
                bones.Count, activeBones, rotated));
            for (int i = 0; i < posed.Count && i < 5; i++)
            {
                report.AppendLine(string.Format("  {0}: localRotation {1}",
                    TransformPath(posed[i].transform), posed[i].transform.localRotation.eulerAngles.ToString("F1")));
            }

            report.AppendLine(SkeletonReport(subject));
            report.Append(AnimationReport());

            List<SkinnedMeshRenderer> skinnedRenderers = SceneObjects<SkinnedMeshRenderer>();
            List<MeshRenderer> meshRenderers = SceneObjects<MeshRenderer>();
            report.AppendLine(string.Format("renderers in the scene: {0} SkinnedMeshRenderer, {1} MeshRenderer",
                                            skinnedRenderers.Count, meshRenderers.Count));

            List<string> rendererLines = new List<string>();
            foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
            {
                rendererLines.Add(string.Format("  SMR {0}: enabled={1}, visible={2}, mesh={3}, bones={4}, materials={5}",
                    TransformPath(renderer.transform), renderer.enabled, renderer.isVisible,
                    renderer.sharedMesh == null
                        ? "NULL"
                        : string.Format("{0} ({1} verts)", renderer.sharedMesh.name, renderer.sharedMesh.vertexCount),
                    renderer.bones.Length, MaterialSummary(renderer.sharedMaterials)));
            }

            // A loaded scene holds hundreds of mesh renderers, most of them room and tool geometry, and
            // the list a diagnostic needs is the one that carries a character shader. Everything else is
            // counted instead of printed.
            List<string> characterLines = new List<string>();
            int enabledMeshRenderers = 0;
            int visibleMeshRenderers = 0;
            foreach (MeshRenderer renderer in meshRenderers)
            {
                enabledMeshRenderers += renderer.enabled ? 1 : 0;
                visibleMeshRenderers += renderer.isVisible ? 1 : 0;
                if (!UsesCharacterShader(renderer.sharedMaterials))
                {
                    continue;
                }

                MeshFilter filter = renderer.GetComponent<MeshFilter>();
                characterLines.Add(string.Format("  MR {0}: enabled={1}, visible={2}, mesh={3}, materials={4}",
                    TransformPath(renderer.transform), renderer.enabled, renderer.isVisible,
                    filter == null || filter.sharedMesh == null ? "NULL" : filter.sharedMesh.name,
                    MaterialSummary(renderer.sharedMaterials)));
            }

            characterLines.Sort(string.CompareOrdinal);

            report.AppendLine(string.Format(
                "  mesh renderers with a character shader: {0} (of {1} enabled, {2} visible in the scene)",
                characterLines.Count, enabledMeshRenderers, visibleMeshRenderers));

            // The skinned renderers go first and on their own budget: they are nine against forty
            // character mesh renderers, and a single shared cap printed only the mesh ones.
            rendererLines.Sort(string.CompareOrdinal);
            for (int i = 0; i < rendererLines.Count && i < 40; i++)
            {
                report.AppendLine(rendererLines[i]);
            }

            for (int i = 0; i < characterLines.Count && i < 40; i++)
            {
                report.AppendLine(characterLines[i]);
            }

            if (characterLines.Count > 40)
            {
                report.AppendLine(string.Format(
                    "  ... {0} more mesh renderers with a character shader", characterLines.Count - 40));
            }
        }
        catch (Exception e)
        {
            report.AppendLine("skin diagnostics stopped at " + e.GetType().Name + ": " + e.Message);
        }

        return report.ToString();
    }

    private static string PlayReport()
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("----- RebuildGate play report -----");
        report.AppendLine(string.Format("played: {0:N1} s", playSeconds));
        if (playScene != null && playScene != string.Empty)
        {
            report.AppendLine(string.Format("scene: {0} (asked for after {1:N0} s)", playScene, playWarmup));
            report.AppendLine(string.Format("scene load: requested={0}, taken={1}, finished={2}{3}, refused={4}",
                playSceneRequested, playSceneLoadStarted, playSceneLoadFinished,
                playSceneLoadFinished ? string.Format(" after {0:N1} s", playSceneFinishedAt) : "",
                playLoadRefused));
            report.AppendLine(string.Format("errors before the load: {0}",
                playErrorsAtLoad >= 0 ? playErrorsAtLoad.ToString() : "the load was never requested"));

            if (playSceneAuditError != null)
            {
                report.AppendLine("scene atoms: could not be read back - " + playSceneAuditError);
            }
            else if (playSceneDeclared >= 0)
            {
                report.AppendLine(string.Format("scene atoms: {0} declared, {1} present, {2} missing{3}",
                    playSceneDeclared, playScenePresent, playSceneMissing.Count,
                    playSceneDeclared > 0 && playSceneMissing.Count == 0
                        ? " - the scene on screen is the scene that was asked for" : ""));
                foreach (string id in playSceneMissing)
                {
                    report.AppendLine("  missing atom: " + id);
                }
            }
        }

        report.Append(SkinReport());

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

        // A probe that never made it into the build cannot print anything, and its silence is
        // indistinguishable from a negative result, so every play report states what was compiled in.
        report.AppendLine("probes: " + ProbeSummary());
        report.AppendLine(playVerdict ? "----- RebuildGate OK -----" : "----- RebuildGate FAILED -----");
        return report.ToString();
    }
}
