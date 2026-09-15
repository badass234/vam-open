using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Hand inspection of a running scene.
///
/// The timed gate samples one moment and exits, which is enough to prove that a scene loads and that
/// its content works, but not enough to answer a question about a body that looks wrong on screen:
/// that answer changes from frame to frame, and the only way to see it is to sit in the running game
/// and watch. This window is that seat.
///
/// It watches the two things the reports cannot settle by themselves - which skeleton is actually
/// being animated, and which skeleton each skin binds its bones to - and states the pairing plainly.
/// DAZSkinV2 binds by bone NAME against a single DAZBones root (DAZSkinV2.InitBones), so a skin
/// pointed at the wrong copy of the skeleton binds silently, in the wrong pose, with no error
/// anywhere in the log: that is exactly the shape of a body frozen in a T-pose while the hair, bound
/// through its own path, follows the animation.
/// </summary>
public class VaMInspector : EditorWindow
{
    /// <summary>The scene the gate was last run with, so both tools look at the same thing.</summary>
    private const string DefaultScene = "MeshedVR.DemoScenes.2:/Saves/scene/MeshedVR/DemoScenes/Cyber/CyberDemoAlt.json";

    /// <summary>Set by the menu command and read by the loader, so it outlives a domain reload.</summary>
    public const string PendingSceneKey = "VaMInspector.PendingScene";

    /// <summary>The scene the window was last pointed at, so reopening the window keeps it.</summary>
    private const string SceneKey = "VaMInspector.Scene";

    /// <summary>How often the live sample is taken. Bones move slowly; a faster tick only burns frames.</summary>
    private const double SampleInterval = 0.5;

    /// <summary>Re-collecting the objects is by far the expensive part, so it runs less often.</summary>
    private const double CollectInterval = 2.0;

    /// <summary>A bone has to move this far to count as moving. Below it is float noise in a static pose.</summary>
    private const float MotionEpsilon = 0.0001f;

    private static readonly string[] InterestingNames = { "Genesis2", "g2female", "PhysicsModel", "reParentObject", "rescale" };

    private string scene = DefaultScene;
    private bool live = true;
    private Vector2 scroll;
    private string report = "Enter play mode, load a scene, and this fills in. Bones rather than pixels: it says which skeleton moves and which skeleton the body listens to.";
    private string status = string.Empty;

    private Dictionary<string, Motion> motions = new Dictionary<string, Motion>();
    private Dictionary<Type, object> caches = new Dictionary<Type, object>();
    private List<Component> targets = new List<Component>();
    private double nextSample;
    private double nextCollect;
    private int samples;
    private int movedOverAllSamples;

    [MenuItem("Rebuild/VaM Inspector")]
    public static void Open()
    {
        VaMInspector window = GetWindow<VaMInspector>("VaM Inspector");
        window.Show();
    }

    [MenuItem("Rebuild/Load Scene (stay open)")]
    public static void LoadSceneMenu()
    {
        VaMSceneLoader.Queue(EditorPrefs.GetString(SceneKey, DefaultScene));
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = true;
        }
    }

    private void OnEnable()
    {
        scene = EditorPrefs.GetString(SceneKey, DefaultScene);
        EditorApplication.update += Tick;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Tick;
    }

    private void Tick()
    {
        if (!live || !EditorApplication.isPlaying)
        {
            return;
        }

        double now = EditorApplication.timeSinceStartup;
        if (now < nextSample)
        {
            return;
        }

        nextSample = now + SampleInterval;
        if (now >= nextCollect)
        {
            nextCollect = now + CollectInterval;
            Collect();
        }

        Sample();
        report = BuildReport();
        Repaint();
    }

    /// <summary>The objects worth watching, and the objects worth clicking on.</summary>
    private void Collect()
    {
        motions.Clear();
        targets.Clear();

        List<DAZBones> skeletons = SceneObjects<DAZBones>();
        List<DAZSkinV2> skins = SceneObjects<DAZSkinV2>();
        List<DAZCharacterSelector> selectors = SceneObjects<DAZCharacterSelector>();
        List<DAZCharacter> characters = SceneObjects<DAZCharacter>();
        List<Animator> animators = SceneObjects<Animator>();
        List<SkinnedMeshRenderer> renderers = SceneObjects<SkinnedMeshRenderer>();
        List<Transform> transforms = SceneObjects<Transform>();

        Cache(skeletons);
        Cache(skins);
        Cache(selectors);
        Cache(characters);
        Cache(animators);
        Cache(renderers);
        Cache(transforms);

        foreach (DAZBones root in skeletons)
        {
            if (root == null)
            {
                continue;
            }

            Watch("bones:" + TransformPath(root.transform),
                  "skeleton " + TransformPath(root.transform),
                  BoneTransforms(root.dazBones));
            targets.Add(root);
        }

        foreach (DAZSkinV2 skin in skins)
        {
            if (skin == null)
            {
                continue;
            }

            Watch("skin:" + TransformPath(skin.transform),
                  "skin " + TransformPath(skin.transform),
                  BoneTransforms(SkinBones(skin)));
            targets.Add(skin);
        }

        foreach (DAZCharacterSelector selector in selectors)
        {
            targets.Add(selector);
        }

        foreach (DAZCharacter character in characters)
        {
            targets.Add(character);
        }

        foreach (Animator animator in animators)
        {
            targets.Add(animator);
        }

        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            if (renderer.sharedMesh != null && renderer.sharedMesh.vertexCount >= 3000)
            {
                targets.Add(renderer);
            }
        }
    }

    private void Watch(string key, string label, Transform[] bones)
    {
        Motion motion = new Motion();
        motion.key = key;
        motion.label = label;
        motion.SetBones(bones);
        motions[key] = motion;
    }

    /// <summary>Takes the sample every watched bone is compared against.</summary>
    private void Sample()
    {
        int moved = 0;
        foreach (Motion motion in motions.Values)
        {
            moved += motion.Take();
        }

        samples++;
        if (moved > 0)
        {
            movedOverAllSamples += moved;
        }
    }

    /// <summary>
    /// The bones a skin actually skins with - its own dazBones array, which is protected and so is
    /// read by reflection. Reading the skin's root instead would only say which skeleton it was
    /// pointed at, and the question here is whether those particular bones follow the animation.
    /// </summary>
    private static DAZBone[] SkinBones(DAZSkinV2 skin)
    {
        return Field(skin, "dazBones") as DAZBone[];
    }

    private static Transform[] BoneTransforms(DAZBone[] bones)
    {
        if (bones == null)
        {
            return new Transform[0];
        }

        List<Transform> transforms = new List<Transform>(bones.Length);
        foreach (DAZBone bone in bones)
        {
            if (bone != null && bone.transform != null)
            {
                transforms.Add(bone.transform);
            }
        }

        return transforms.ToArray();
    }

    /// <summary>How far a group of bones has moved since the previous sample.</summary>
    private sealed class Motion
    {
        public string key;
        public string label;
        public Transform[] bones;
        private Vector3[] previous;
        private bool[] everMoved;
        public int movedLast;
        public int movedEver;
        public float maxDelta;

        public void SetBones(Transform[] value)
        {
            if (bones != null && bones.Length == value.Length)
            {
                bones = value;
                return;
            }

            bones = value;
            previous = new Vector3[value.Length];
            everMoved = new bool[value.Length];
            movedLast = 0;
            movedEver = 0;
            maxDelta = 0f;
        }

        public int Take()
        {
            if (bones == null || bones.Length == 0 || previous == null)
            {
                return 0;
            }

            movedLast = 0;
            for (int i = 0; i < bones.Length; i++)
            {
                Transform bone = bones[i];
                if (bone == null)
                {
                    continue;
                }

                Vector3 position = bone.position;
                float delta = Vector3.Distance(position, previous[i]);
                previous[i] = position;
                if (delta <= MotionEpsilon)
                {
                    continue;
                }

                movedLast++;
                if (delta > maxDelta)
                {
                    maxDelta = delta;
                }

                if (!everMoved[i])
                {
                    everMoved[i] = true;
                    movedEver++;
                }
            }

            return movedLast;
        }
    }

    /// <summary>
    /// The dump the window shows and writes out. Every line answers a question that was asked of the
    /// timed reports and could not be answered from a single moment: which skeleton moves, which
    /// skeleton each skin listens to, and whether those are the same one.
    /// </summary>
    private string BuildReport()
    {
        StringBuilder text = new StringBuilder();
        SuperController controller = SuperController.singleton;

        text.AppendLine("VaM inspector dump");
        text.AppendLine("samples: " + samples + " every " + SampleInterval.ToString("0.0", CultureInfo.InvariantCulture) +
                        " s, bones that moved across all samples: " + movedOverAllSamples);
        if (controller == null)
        {
            text.AppendLine("SuperController.singleton: null - the game did not boot");
        }
        else
        {
            text.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "SuperController: {0}, atoms={1}, isLoading={2}, autoSimulation={3}",
                controller.name, controller.GetAtomUIDs().Count, controller.isLoading, controller.autoSimulation));
        }

        HashSet<string> animated = AnimatedSkeletons();

        text.AppendLine();
        text.AppendLine("----- skeletons (DAZBones) -----");
        foreach (DAZBones root in Sorted(Cached<DAZBones>()))
        {
            string path = TransformPath(root.transform);
            Motion motion = Lookup("bones:" + path);
            text.AppendLine("skeleton " + path);
            text.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "  bones={0}, gameObject active={1}, hierarchy active={2}, world={3} scale={4}",
                root.dazBones == null ? 0 : root.dazBones.Length,
                root.gameObject.activeSelf, root.gameObject.activeInHierarchy,
                Vector(root.transform.position), Vector(root.transform.lossyScale)));
            text.AppendLine("  motion: " + Describe(motion) + ", " + (animated.Contains(path) ? "ANIMATED" : "STATIC"));
        }

        text.AppendLine();
        text.AppendLine("----- skins (DAZSkinV2) -----");
        foreach (DAZSkinV2 skin in Sorted(Cached<DAZSkinV2>()))
        {
            string path = TransformPath(skin.transform);
            string rootPath = skin.root == null ? null : TransformPath(skin.root.transform);
            DAZBone[] bones = SkinBones(skin);
            int missing = 0;
            if (bones != null)
            {
                foreach (DAZBone bone in bones)
                {
                    if (bone == null)
                    {
                        missing++;
                    }
                }
            }

            Mesh mesh = skin.GetMesh();
            text.AppendLine("skin " + path);
            text.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "  enabled={0}, hierarchy active={1}, numBones={2}, bones array={3}, bones not bound={4}, mesh={5} verts={6}",
                skin.enabled, skin.gameObject.activeInHierarchy, skin.numBones,
                bones == null ? 0 : bones.Length, missing,
                mesh == null ? "NONE" : mesh.name, mesh == null ? 0 : mesh.vertexCount));
            text.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "  root={0} ({1})", rootPath == null ? "NULL" : rootPath,
                rootPath == null ? "no skeleton - skinning cannot run" : (animated.Contains(rootPath) ? "ANIMATED" : "STATIC")));
            text.AppendLine("  motion: " + Describe(Lookup("skin:" + path)));
        }

        text.AppendLine();
        text.AppendLine("----- the pairing -----");
        text.AppendLine(Pairing(animated));

        text.AppendLine();
        text.AppendLine("----- character selectors -----");
        foreach (DAZCharacterSelector selector in Sorted(Cached<DAZCharacterSelector>()))
        {
            DAZCharacter selected = null;
            try
            {
                selected = selector.selectedCharacter;
            }
            catch (Exception e)
            {
                text.AppendLine("  (selectedCharacter threw " + e.GetType().Name + ")");
            }

            text.AppendLine("selector " + TransformPath(selector.transform));
            text.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "  rootBones={0}, rootBonesNameFemale=\"{1}\", selectedCharacter={2}",
                selector.rootBones == null ? "NULL" : TransformPath(selector.rootBones.transform),
                selector.rootBonesNameFemale, selected == null ? "NULL" : selected.displayName));
        }

        text.AppendLine();
        text.AppendLine("----- characters (DAZCharacter) -----");
        foreach (DAZCharacter character in Sorted(Cached<DAZCharacter>()))
        {
            text.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "character {0}: displayName=\"{1}\", active={2}", TransformPath(character.transform),
                character.displayName, character.gameObject.activeInHierarchy));
            text.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "  rootBonesForSkinning={0}",
                character.rootBonesForSkinning == null ? "NULL" : TransformPath(character.rootBonesForSkinning.transform)));
            text.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "  skin={0}, skinForClothes={1}",
                character.skin == null ? "NULL" : TransformPath(character.skin.transform),
                character.skinForClothes == null ? "NULL" : TransformPath(character.skinForClothes.transform)));
        }

        ReportCopies(text);
        ReportAnimators(text);
        ReportRenderers(text);
        return text.ToString();
    }

    /// <summary>
    /// The line this whole window exists for: the skeleton that moves, the skeleton the body listens
    /// to, and whether they are the same object. A body skinned to a skeleton nothing animates holds
    /// the pose it was bound in, which is what a body stuck in a T-pose turns out to be.
    /// </summary>
    private string Pairing(HashSet<string> animated)
    {
        StringBuilder text = new StringBuilder();
        if (animated.Count == 0)
        {
            text.AppendLine("no skeleton has moved in " + samples + " samples - nothing is animating yet");
        }
        else
        {
            List<string> names = new List<string>(animated);
            names.Sort(StringComparer.Ordinal);
            text.AppendLine("animated skeleton(s) (" + names.Count + "): " + string.Join(", ", names.ToArray()));
        }

        foreach (DAZSkinV2 skin in Sorted(Cached<DAZSkinV2>()))
        {
            if (!skin.gameObject.activeInHierarchy)
            {
                continue;
            }

            DAZBone[] bones = SkinBones(skin);
            if (bones == null || bones.Length == 0)
            {
                continue;
            }

            string path = TransformPath(skin.transform);
            string rootPath = skin.root == null ? null : TransformPath(skin.root.transform);
            Motion own = Lookup("skin:" + path);
            bool skinMoves = own != null && own.movedEver > 0;
            bool rootMoves = rootPath != null && animated.Contains(rootPath);

            if (rootPath == null)
            {
                text.AppendLine("MISMATCH " + path + ": root is NULL, so the bones it was imported with were never bound");
            }
            else if (!rootMoves && skinMoves)
            {
                text.AppendLine("ODD " + path + ": its own bones move but its root " + rootPath +
                                " is not one of the animated skeletons - the bones it binds are not the bones it skins with");
            }
            else if (!rootMoves && !skinMoves)
            {
                text.AppendLine("STATIC " + path + ": bound to " + rootPath + ", which is not animated, and its own bones do not move either");
            }
            else
            {
                text.AppendLine("OK " + path + ": bound to the animated skeleton " + rootPath);
            }
        }

        return text.ToString();
    }

    /// <summary>Every skeleton whose bones have been seen to move, by transform path.</summary>
    private HashSet<string> AnimatedSkeletons()
    {
        HashSet<string> animated = new HashSet<string>();
        foreach (Motion motion in motions.Values)
        {
            if (motion.key.StartsWith("bones:", StringComparison.Ordinal) && motion.movedEver > 0)
            {
                animated.Add(motion.key.Substring("bones:".Length));
            }
        }

        return animated;
    }

    /// <summary>
    /// Every transform that looks like a copy of the character skeleton, which is how many copies
    /// there are of the thing a wrong reference could have picked instead.
    /// </summary>
    private void ReportCopies(StringBuilder text)
    {
        text.AppendLine();
        text.AppendLine("----- objects named like the skeleton -----");
        int shown = 0;
        int matched = 0;
        foreach (Transform transform in Sorted(Cached<Transform>()))
        {
            bool interesting = false;
            foreach (string name in InterestingNames)
            {
                if (transform.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    interesting = true;
                    break;
                }
            }

            if (!interesting || transform.parent == null && transform.name == transform.root.name)
            {
                continue;
            }

            matched++;
            if (shown >= 80)
            {
                continue;
            }

            shown++;
            text.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "copy {0}: activeSelf={1}, hierarchy active={2}, children={3}, at {4}{5}",
                TransformPath(transform), transform.gameObject.activeSelf, transform.gameObject.activeInHierarchy,
                transform.childCount, Vector(transform.position), Components(transform.gameObject)));
        }

        text.AppendLine("matched " + matched + ", shown " + shown);
    }

    private void ReportAnimators(StringBuilder text)
    {
        text.AppendLine();
        text.AppendLine("----- animators -----");
        foreach (Animator animator in Sorted(Cached<Animator>()))
        {
            AnimatorControllerInfo(animator, text);
        }
    }

    private static void AnimatorControllerInfo(Animator animator, StringBuilder text)
    {
        string clip = "none";
        float normalized = 0f;
        try
        {
            if (animator.runtimeAnimatorController == null)
            {
                clip = "no runtime controller";
            }
            else if (animator.layerCount > 0)
            {
                AnimatorClipInfo[] infos = animator.GetCurrentAnimatorClipInfo(0);
                if (infos != null && infos.Length > 0 && infos[0].clip != null)
                {
                    clip = infos[0].clip.name + " (" + infos[0].clip.length.ToString("0.00", CultureInfo.InvariantCulture) +
                           " s, weight " + infos[0].weight.ToString("0.00", CultureInfo.InvariantCulture) + ")";
                }

                normalized = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            }
        }
        catch (Exception e)
        {
            clip = "unreadable: " + e.GetType().Name;
        }

        text.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "animator {0}: enabled={1}, active={2}, speed={3}, cullingMode={4}, clip={5}, normalizedTime={6}",
            TransformPath(animator.transform), animator.enabled, animator.gameObject.activeInHierarchy,
            animator.speed, animator.cullingMode, clip,
            normalized.ToString("0.000", CultureInfo.InvariantCulture)));
        text.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "  avatar={0}, isHuman={1}, hasRootMotion={2}, applyRootMotion={3}",
            animator.avatar == null ? "NULL" : animator.avatar.name, animator.isHuman,
            animator.hasRootMotion, animator.applyRootMotion));
    }

    /// <summary>
    /// The skinned meshes big enough to be a body, with the skeleton each one renders through. This is
    /// the other half of the pairing question: the skin's bones decide the drawn pose, but a renderer
    /// whose rootBone sits under the other skeleton draws that skeleton's frame too.
    /// </summary>
    private void ReportRenderers(StringBuilder text)
    {
        text.AppendLine();
        text.AppendLine("----- skinned mesh renderers over 3000 verts -----");
        foreach (SkinnedMeshRenderer renderer in Sorted(Cached<SkinnedMeshRenderer>()))
        {
            Mesh mesh = renderer.sharedMesh;
            if (mesh == null || mesh.vertexCount < 3000)
            {
                continue;
            }

            DAZBones owner = renderer.rootBone == null ? null : renderer.rootBone.GetComponentInParent<DAZBones>();
            text.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "SMR {0}: enabled={1}, visible={2}, verts={3}, rootBone={4}, owner skeleton={5}",
                TransformPath(renderer.transform), renderer.enabled, renderer.isVisible,
                mesh.vertexCount, renderer.rootBone == null ? "NULL" : TransformPath(renderer.rootBone),
                owner == null ? "none" : TransformPath(owner.transform)));
            text.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "  bounds: world center {0} size {1}, local center {2} size {3}, bones={4}",
                Vector(renderer.bounds.center), Vector(renderer.bounds.size),
                Vector(renderer.localBounds.center), Vector(renderer.localBounds.size),
                renderer.bones == null ? 0 : renderer.bones.Length));

            Transform[] bones = renderer.bones;
            if (bones != null && bones.Length > 0)
            {
                text.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "  first bone={0}, last bone={1}",
                    bones[0] == null ? "NULL" : TransformPath(bones[0]),
                    bones[bones.Length - 1] == null ? "NULL" : TransformPath(bones[bones.Length - 1])));
                int under = 0;
                foreach (Transform bone in bones)
                {
                    if (bone != null && bone.GetComponentInParent<DAZBones>() == owner)
                    {
                        under++;
                    }
                }

                text.AppendLine("  bones under the same DAZBones as rootBone: " + under + " of " + bones.Length);
            }
        }
    }

    // ---------- helpers ----------

    private Motion Lookup(string key)
    {
        Motion motion;
        return motions.TryGetValue(key, out motion) ? motion : null;
    }

    /// <summary>
    /// The report is rebuilt on every sample, but finding the objects is the expensive half, so the
    /// lists found by Collect are kept and the report reads them. A scene holds thousands of
    /// transforms, and walking them twice a second would make the window the slowest thing running.
    /// </summary>
    private void Cache<T>(List<T> items) where T : UnityEngine.Object
    {
        caches[typeof(T)] = items;
    }

    private List<T> Cached<T>() where T : UnityEngine.Object
    {
        object value;
        if (caches.TryGetValue(typeof(T), out value))
        {
            return (List<T>)value;
        }

        return new List<T>();
    }

    private static string Describe(Motion motion)
    {
        if (motion == null)
        {
            return "not collected yet";
        }

        if (motion.bones == null || motion.bones.Length == 0)
        {
            return "no bones to sample";
        }

        return string.Format(CultureInfo.InvariantCulture,
            "{0} of {1} moved in the last sample, {2} of {1} have moved since the window opened, furthest {3:0.0000} m",
            motion.movedLast, motion.bones.Length, motion.movedEver, motion.maxDelta);
    }

    private static List<T> Sorted<T>(List<T> items) where T : Component
    {
        List<T> sorted = new List<T>(items);
        sorted.Sort(delegate(T a, T b)
        {
            return string.CompareOrdinal(TransformPath(a.transform), TransformPath(b.transform));
        });
        return sorted;
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

    private static string TransformPath(Transform transform)
    {
        string path = transform.name;
        for (Transform parent = transform.parent; parent != null; parent = parent.parent)
        {
            path = parent.name + "/" + path;
        }

        return path;
    }

    private static string Vector(Vector3 value)
    {
        return string.Format(CultureInfo.InvariantCulture, "({0:0.00}, {1:0.00}, {2:0.00})", value.x, value.y, value.z);
    }

    private static string Components(GameObject target)
    {
        List<string> names = new List<string>();
        foreach (Component component in target.GetComponents<Component>())
        {
            if (component != null)
            {
                names.Add(component.GetType().Name);
            }
        }

        return names.Count == 0 ? string.Empty : " components=" + string.Join(",", names.ToArray());
    }

    /// <summary>The field of any name on an object, public or not, or null when there is no such field.</summary>
    private static object Field(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return field == null ? null : field.GetValue(target);
    }

    // ---------- window ----------

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Scene to load (the game's own addressing: <packageUid>:/<path>)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        scene = EditorGUILayout.TextField(scene);
        if (GUILayout.Button("Load now", GUILayout.Width(90)))
        {
            Load();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        live = EditorGUILayout.ToggleLeft("Watch the scene", live, GUILayout.Width(140));
        if (GUILayout.Button("Refresh", GUILayout.Width(90)))
        {
            Collect();
            Sample();
            report = BuildReport();
        }

        if (GUILayout.Button("Save dump", GUILayout.Width(90)))
        {
            Save();
        }

        if (GUILayout.Button("Select nothing", GUILayout.Width(110)))
        {
            Selection.activeObject = null;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("samples: " + samples + "   moved in total: " + movedOverAllSamples +
                                   "   play mode: " + EditorApplication.isPlaying, EditorStyles.miniLabel);
        if (status.Length > 0)
        {
            EditorGUILayout.HelpBox(status, MessageType.Info);
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (Component target in targets)
        {
            if (target == null)
            {
                continue;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ping", GUILayout.Width(60)))
            {
                EditorGUIUtility.PingObject(target);
                Selection.activeObject = target;
            }

            EditorGUILayout.LabelField(TransformPath(target.transform) + "  (" + target.GetType().Name + ")");
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.SelectableLabel(report, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Asks for the scene. With the window open in play mode this hands it straight to the game; when
    /// the editor is not playing it queues the load and starts play mode, and VaMSceneLoader takes over
    /// once the game has booted, since a load asked for too early is silently refused.
    /// </summary>
    private void Load()
    {
        EditorPrefs.SetString(SceneKey, scene);
        SuperController controller = SuperController.singleton;
        if (EditorApplication.isPlaying && controller != null && !controller.isLoading)
        {
            status = "asking the game to load " + scene;
            controller.Load(scene);
            return;
        }

        VaMSceneLoader.Queue(scene);
        status = "queued " + scene + " - play mode will start and the scene loads once the game has booted";
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = true;
        }
    }

    private void Save()
    {
        string folder = ArtifactsFolder();
        if (folder == null)
        {
            status = "no artifacts folder next to the project; nothing was written";
            return;
        }

        string path = Path.Combine(folder, "inspector-dump.txt");
        try
        {
            File.WriteAllText(path, report, new UTF8Encoding(false));
            status = "wrote " + path;
        }
        catch (Exception e)
        {
            status = "could not write " + path + ": " + e.Message;
        }
    }

    /// <summary>&lt;repo&gt;\VAMOpen\artifacts, where the gate's own reports go.</summary>
    private static string ArtifactsFolder()
    {
        DirectoryInfo project = Directory.GetParent(Application.dataPath);
        DirectoryInfo root = project == null ? null : project.Parent;
        if (root == null)
        {
            return null;
        }

        string folder = Path.Combine(root.FullName, "artifacts");
        return Directory.Exists(folder) ? folder : null;
    }
}

/// <summary>
/// Loads a scene into the game and leaves it running.
///
/// The gate's load happens on a timer because the run has to end by itself. Here nothing ends: the run
/// exists so that a person can look at it, so the only thing this does is wait for the game to finish
/// booting and then ask for the scene. A load asked for while another is in flight is dropped by
/// SuperController.LoadInternal without a word, which is why both conditions matter - the game has a
/// SuperController, is not loading, and has its boot scene's atoms already in place.
/// </summary>
[InitializeOnLoad]
public static class VaMSceneLoader
{
    /// <summary>Grace after the boot finishes, so the request cannot land inside the boot's tail.</summary>
    private const double Settle = 3.0;

    private static double readySince = -1.0;

    static VaMSceneLoader()
    {
        EditorApplication.update += Tick;
    }

    public static void Queue(string scene)
    {
        EditorPrefs.SetString(VaMInspector.PendingSceneKey, scene);
        readySince = -1.0;
    }

    private static void Tick()
    {
        string scene = EditorPrefs.GetString(VaMInspector.PendingSceneKey, string.Empty);
        if (scene.Length == 0)
        {
            readySince = -1.0;
            return;
        }

        if (!EditorApplication.isPlaying)
        {
            readySince = -1.0;
            return;
        }

        SuperController controller = SuperController.singleton;
        if (controller == null || controller.isLoading || controller.GetAtomUIDs().Count == 0)
        {
            readySince = -1.0;
            return;
        }

        double now = EditorApplication.timeSinceStartup;
        if (readySince < 0.0)
        {
            readySince = now;
            Debug.Log("----- VaMInspector: the game booted; loading " + scene + " in " + Settle + " s -----");
            return;
        }

        if (now - readySince < Settle)
        {
            return;
        }

        readySince = -1.0;
        EditorPrefs.SetString(VaMInspector.PendingSceneKey, string.Empty);
        Debug.Log("----- VaMInspector: loading " + scene + " -----");
        try
        {
            controller.Load(scene);
        }
        catch (Exception e)
        {
            Debug.LogError("----- VaMInspector: loading " + scene + " threw " + e + " -----");
        }
    }
}
