using System;
using System.Collections.Generic;
using System.IO;
using AssetBundles;
using UnityEngine;

namespace MeshVR
{
	/// <summary>
	/// Recovers a shader the game shipped in the z_sha bundle.
	///
	/// DAZSkinV2 skins a mesh on the GPU by swapping every material onto
	/// "&lt;name&gt;ComputeBuff", and it looks that name up with Shader.Find, which only ever sees
	/// the shaders the rebuilt project itself defines. AssetRipper wrote a stub for every one of
	/// them, so a family this project has not transcribed yet - hair, the Marmoset IBL set - would
	/// be shaded by the wrong library instead. The originals still ship in z_sha, the bundle every
	/// material in the game resolves its shaders from, so they are read from there instead.
	/// </summary>
	public static class VamShaderProvider
	{
		private const string ShaderBundleName = "z_sha";

		private const float RetryInterval = 5f;

		private static readonly Dictionary<string, Shader> cache = new Dictionary<string, Shader>();

		private static readonly HashSet<string> warned = new HashSet<string>();

		private static Dictionary<string, Shader> bundleIndex;

		private static float nextAttemptTime;

		/// <summary>The "&lt;paramref name="shaderName"/&gt;ComputeBuff" variant of a shader.</summary>
		public static Shader FindComputeBuff(string shaderName)
		{
			return shaderName == null ? null : FindByName(shaderName + "ComputeBuff");
		}

		/// <summary>A shader of this name: the project's own first, then the shipped original.</summary>
		public static Shader FindByName(string shaderName)
		{
			if (string.IsNullOrEmpty(shaderName))
			{
				return null;
			}
			Shader shader;
			if (cache.TryGetValue(shaderName, out shader))
			{
				return shader;
			}
			shader = Shader.Find(shaderName);
			if (shader == null)
			{
				// A miss is not cached: the scene bundles may still be loading.
				if (Time.realtimeSinceStartup < nextAttemptTime)
				{
					return null;
				}
				nextAttemptTime = Time.realtimeSinceStartup + RetryInterval;
				shader = LookupBundle(shaderName);
			}
			if (shader == null)
			{
				if (warned.Add(shaderName))
				{
					Debug.LogWarning(string.Format(
						"[VamShaderProvider] {0} is defined by no project shader and is not in the {1} bundle.",
						shaderName, ShaderBundleName));
				}
				return null;
			}
			cache[shaderName] = shader;
			return shader;
		}

		private static Shader LookupBundle(string shaderName)
		{
			Dictionary<string, Shader> index = BundleIndex();
			Shader shader;
			if (index != null && index.TryGetValue(shaderName, out shader))
			{
				return shader;
			}
			return null;
		}

		/// <summary>
		/// Every shader of z_sha, keyed by its ShaderLab name, read once per session.
		///
		/// The bundle stores a shader under its asset path, not under the name Shader.Find takes,
		/// and the two do not map onto each other, so the assets are loaded and indexed by name.
		/// </summary>
		private static Dictionary<string, Shader> BundleIndex()
		{
			if (bundleIndex != null)
			{
				return bundleIndex;
			}
			AssetBundle bundle = OpenShaderBundle();
			if (bundle == null)
			{
				return null;
			}
			Dictionary<string, Shader> index = new Dictionary<string, Shader>(StringComparer.Ordinal);
			Shader[] shaders = bundle.LoadAllAssets<Shader>();
			for (int i = 0; i < shaders.Length; i++)
			{
				Shader shader = shaders[i];
				if (shader != null && !string.IsNullOrEmpty(shader.name) && !index.ContainsKey(shader.name))
				{
					index.Add(shader.name, shader);
				}
			}
			bundleIndex = index;
			return index;
		}

		private static AssetBundle OpenShaderBundle()
		{
			foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
			{
				if (bundle != null && string.Equals(bundle.name, ShaderBundleName, StringComparison.OrdinalIgnoreCase))
				{
					return bundle;
				}
			}
			string directory = AssetBundleManager.GetStreamingAssetsDirectory();
			if (string.IsNullOrEmpty(directory))
			{
				return null;
			}
			string path = Path.Combine(directory, ShaderBundleName);
			if (!File.Exists(path))
			{
				return null;
			}
			return AssetBundle.LoadFromFile(path);
		}
	}
}
