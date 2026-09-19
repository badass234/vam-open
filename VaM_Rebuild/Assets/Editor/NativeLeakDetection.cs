using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Makes Unity's native-container check print the allocation stack, for the runs that need to know
/// where a leaked container came from:
///
///     $env:VAMOPEN_LEAK_TRACES = 1
///
/// The editor's default mode reports the leak as one line -
/// `A Native Collection has not been disposed, resulting in a memory leak. Enable Full StackTraces to
/// get more details.` - which names neither the container nor its owner. The mode that records the
/// stack is reached through `Unity.Collections.NativeLeakDetection.Mode`, a static property rather
/// than a project setting, so it has to be set from an editor script.
///
/// Off unless the variable is set: the mode costs an allocation stack per container, which is not
/// something a normal run should pay for or be changed by.
///
/// The type is reached by reflection, not by a `using Unity.Collections`, because this file has to
/// compile on whatever editor opens the project - `EnabledWithStackTrace` is absent from the enum
/// before 2019.4 - and a missing symbol would otherwise be a compile error in the gate rather than a
/// line in the log.
/// </summary>
[InitializeOnLoad]
internal static class NativeLeakDetection
{
    private const string EnableVariable = "VAMOPEN_LEAK_TRACES";

    private static PropertyInfo mode;
    private static bool probed;
    private static bool reported;

    static NativeLeakDetection()
    {
        if (!Enabled())
        {
            return;
        }

        Apply();
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static bool Enabled()
    {
        return Environment.GetEnvironmentVariable(EnableVariable) == "1";
    }

    // Entering play mode runs a domain reload, which throws the mode away along with the static
    // fields - and the containers the check reports on are allocated by the game and by the editor's
    // views while play mode comes up, so the mode has to be back before the first frame. Both ends of
    // the transition are covered: the play mode state enum has no "entering" member, so the moment
    // before it is ExitingEditMode, and covering it as well survives -EnterPlayModeOptions suppressing
    // the reload.
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredPlayMode)
        {
            Apply();
        }
    }

    private static void Apply()
    {
        Probe();

        if (mode == null)
        {
            Unavailable("this editor has no Unity.Collections.NativeLeakDetection");
            return;
        }

        // The enum member is the other half of the surface: 2018.x has the type and only the two
        // older names, so both the type and the name are checked before either is used.
        const string Name = "EnabledWithStackTrace";
        if (!Enum.IsDefined(mode.PropertyType, Name))
        {
            Unavailable("Unity.Collections.NativeLeakDetectionMode has no " + Name);
            return;
        }

        object traces = Enum.Parse(mode.PropertyType, Name);

        // The property is set from a constructor that runs after every reload, so it is set, and
        // logged, only when it actually changes - otherwise the mode would be announced a dozen
        // times in a log whose point is one warning.
        if (Equals(mode.GetValue(null, null), traces))
        {
            return;
        }

        mode.SetValue(null, traces, null);
        Debug.Log("----- NativeLeakDetection: " + EnableVariable + "=1, Mode = " + traces +
                  " on " + Application.unityVersion + " -----");
    }

    private static void Probe()
    {
        if (probed)
        {
            return;
        }

        probed = true;
        Type type = Type.GetType("Unity.Collections.NativeLeakDetection, UnityEngine.CoreModule");
        if (type != null)
        {
            mode = type.GetProperty("Mode", BindingFlags.Public | BindingFlags.Static);
        }
    }

    private static void Unavailable(string why)
    {
        if (reported)
        {
            return;
        }

        reported = true;
        Debug.LogWarning("----- NativeLeakDetection: " + why + ", so " + EnableVariable +
                         " does nothing here -----");
    }
}
