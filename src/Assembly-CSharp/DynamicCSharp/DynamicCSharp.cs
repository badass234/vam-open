using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicCSharp.Security;
using UnityEngine;

namespace DynamicCSharp
{
	public sealed class DynamicCSharp : ScriptableObject
	{
		private const string editorSettingsDirectory = "/Resources";

		private const string settingsLocation = "DynamicCSharp_Settings";

		private const BindingFlags defaultFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

		private static DynamicCSharp instance = null;

		public bool caseSensitiveNames;

		public bool discoverNonPublicTypes = true;

		public bool discoverNonPublicMembers = true;

		public bool securityCheckCode = true;

		public readonly bool debugMode;

		[HideInInspector]
		public string compilerWorkingDirectory = string.Empty;

		public string[] assemblyReferences = new string[1] { "Assembly-CSharp.dll" };

		public static readonly string[] unityAssemblyReferences = new string[7] { "UnityEngine.AudioModule.dll", "UnityEngine.CoreModule.dll", "UnityEngine.InputLegacyModule.dll", "UnityEngine.JSONSerializeModule.dll", "UnityEngine.ParticleSystemModule.dll", "UnityEngine.PhysicsModule.dll", "UnityEngine.UIModule.dll" };

		public RestrictionMode namespaceRestrictionMode = RestrictionMode.Exclusive;

		public RestrictionMode assemblyRestrictionMode = RestrictionMode.Exclusive;

		public NamespaceRestriction[] namespaceRestrictions = new NamespaceRestriction[2]
		{
			new NamespaceRestriction("System.IO"),
			new NamespaceRestriction("System.Reflection")
		};

		private Dictionary<string, RuntimeNamespaceRestriction> runtimeNamespaceRestrictions;

		public ReferenceRestriction[] referenceRestrictions = new ReferenceRestriction[2]
		{
			new ReferenceRestriction("UnityEditor.dll"),
			new ReferenceRestriction("Mono.Cecil.dll")
		};

		public TypeRestriction[] typeRestrictions = new TypeRestriction[1]
		{
			new TypeRestriction("System.AppDomain")
		};

		public MemberRestriction[] memberRestrictions = new MemberRestriction[0];

		public static DynamicCSharp Settings
		{
			get
			{
				if (instance == null)
				{
					instance = LoadSettings();
				}
				return instance;
			}
		}

		public static bool IsPlatformSupported => true;

		public IEnumerable<Restriction> Restrictions
		{
			get
			{
				NamespaceRestriction[] array = namespaceRestrictions;
				for (int i = 0; i < array.Length; i++)
				{
					yield return array[i];
				}
				if (runtimeNamespaceRestrictions != null)
				{
					foreach (RuntimeNamespaceRestriction value in runtimeNamespaceRestrictions.Values)
					{
						yield return value;
					}
				}
				ReferenceRestriction[] array2 = referenceRestrictions;
				for (int j = 0; j < array2.Length; j++)
				{
					yield return array2[j];
				}
				TypeRestriction[] array3 = typeRestrictions;
				for (int k = 0; k < array3.Length; k++)
				{
					yield return array3[k];
				}
				MemberRestriction[] array4 = memberRestrictions;
				for (int l = 0; l < array4.Length; l++)
				{
					yield return array4[l];
				}
			}
		}

		public DynamicCSharp()
		{
			int num = assemblyReferences.Length;
			Array.Resize(ref assemblyReferences, num + unityAssemblyReferences.Length);
			for (int i = num; i < assemblyReferences.Length; i++)
			{
				assemblyReferences[i] = unityAssemblyReferences[i - num];
			}
		}

		public void AddRuntimeNamespaceRestriction(string namespaceName)
		{
			if (runtimeNamespaceRestrictions == null)
			{
				runtimeNamespaceRestrictions = new Dictionary<string, RuntimeNamespaceRestriction>();
			}
			if (!runtimeNamespaceRestrictions.ContainsKey(namespaceName))
			{
				RuntimeNamespaceRestriction value = new RuntimeNamespaceRestriction(namespaceName);
				runtimeNamespaceRestrictions.Add(namespaceName, value);
			}
		}

		public void RemoveRuntimeNamespaceRestriction(string namespaceName)
		{
			if (runtimeNamespaceRestrictions == null)
			{
				runtimeNamespaceRestrictions = new Dictionary<string, RuntimeNamespaceRestriction>();
			}
			if (runtimeNamespaceRestrictions.ContainsKey(namespaceName))
			{
				runtimeNamespaceRestrictions.Remove(namespaceName);
			}
		}

		internal BindingFlags GetTypeBindings()
		{
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
			if (discoverNonPublicTypes)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			return bindingFlags;
		}

		internal BindingFlags GetMemberBindings()
		{
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
			if (discoverNonPublicMembers)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			return bindingFlags;
		}

		private static DynamicCSharp LoadSettings()
		{
			UnityEngine.Object @object = Resources.Load("DynamicCSharp_Settings");
			if (@object != null)
			{
				return @object as DynamicCSharp;
			}
			Debug.LogWarning("DynamicCSharp: Failed to load settings - Default values will be used");
			return ScriptableObject.CreateInstance<DynamicCSharp>();
		}

		public static void SaveAsset(DynamicCSharp save)
		{
		}

		public static void LoadAsset()
		{
		}
	}
}
