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
	/// them, so a family this project has not transcribed yet - the Marmoset IBL set - would
	/// be shaded by the wrong library instead. The originals still ship in z_sha, the bundle every
	/// material in the game resolves its shaders from, so they are read from there instead.
	/// </summary>
	public static class VamShaderProvider
	{
		private const string ShaderBundleName = "z_sha";

		private const string ComputeBuffSuffix = "ComputeBuff";

		private const float RetryInterval = 5f;

		private static readonly Dictionary<string, Shader> cache = new Dictionary<string, Shader>();

		private static readonly HashSet<string> warned = new HashSet<string>();

		private static Dictionary<string, Shader> bundleIndex;

		private static float nextAttemptTime;

		/// <summary>The "&lt;paramref name="shaderName"/&gt;ComputeBuff" variant of a shader.</summary>
		public static Shader FindComputeBuff(string shaderName)
		{
			if (string.IsNullOrEmpty(shaderName))
			{
				return null;
			}
			// A skin swaps its materials again whenever it re-initialises them, and by then the material
			// already carries the ComputeBuff name. Appending the suffix a second time names a shader
			// that exists nowhere, so an already-suffixed name is looked up as it is.
			if (shaderName.EndsWith(ComputeBuffSuffix, StringComparison.Ordinal))
			{
				return FindByName(shaderName);
			}
			return FindByName(shaderName + ComputeBuffSuffix);
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

		/// <summary>
		/// Points a material at this project's shader of the same name, and answers whether it moved.
		///
		/// A material that came out of a bundle holds its shader as a serialized reference, so it
		/// keeps the bundle's own copy of the family even once this project defines one - the census
		/// reads those slots as "bundle (project defines one)", and they are what still makes the
		/// shipped game data necessary for the character to look right. Shader.Find answers only
		/// from the project's assets, so re-pointing the material is the whole fix.
		///
		/// Only a family <see cref="VamProjectShaders"/> lists is moved. Shader.Find answers with an
		/// AssetRipper placeholder just as happily as with a reconstruction, and one of those draws
		/// far less than the original does - the hair's optimised path is on GPUTools/MeshedVR/HairOpt,
		/// which is a placeholder here - so a name that is not really reconstructed is left on the
		/// bundle's copy, which is the shipped shader and correct.
		/// </summary>
		public static bool UseProjectShader(Material material)
		{
			if (material == null)
			{
				return false;
			}
			Shader current = material.shader;
			if (current == null || !VamProjectShaders.Defines(current.name))
			{
				return false;
			}
			Shader project = Shader.Find(current.name);
			if (project == null || project == current)
			{
				return false;
			}
			material.shader = project;
			return true;
		}

		/// <summary>The same for a whole list, nulls included. Answers how many moved.</summary>
		public static int UseProjectShaders(Material[] materials)
		{
			if (materials == null)
			{
				return 0;
			}
			int redirected = 0;
			for (int i = 0; i < materials.Length; i++)
			{
				if (UseProjectShader(materials[i]))
				{
					redirected++;
				}
			}
			return redirected;
		}

		/// <summary>Every material of every renderer under one object, and how many moved.</summary>
		public static int UseProjectShaders(GameObject root)
		{
			if (root == null)
			{
				return 0;
			}
			Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
			int redirected = 0;
			for (int i = 0; i < renderers.Length; i++)
			{
				redirected += Redirect(renderers[i]);
			}
			return redirected;
		}

		/// <summary>
		/// The same for everything the loaded scenes are currently drawing.
		///
		/// The components that build their own materials hand them over from their own init, but most
		/// of the census's "bundle (project defines one)" slots are materials that came straight out
		/// of a scene or a bundle prefab and are never handed to any code at all: they simply sit on a
		/// renderer holding the bundle's shader object. No init hook can reach those, so they are swept.
		///
		/// Scene objects only. FindObjectsOfTypeAll also answers with the project's own prefab and
		/// material assets, and re-pointing one of those would edit what the editor has open rather
		/// than what this session draws.
		/// </summary>
		public static int UseProjectShadersEverywhere()
		{
			Renderer[] renderers = Resources.FindObjectsOfTypeAll<Renderer>();
			int redirected = 0;
			for (int i = 0; i < renderers.Length; i++)
			{
				Renderer renderer = renderers[i];
				if (renderer != null && renderer.gameObject.scene.IsValid())
				{
					redirected += Redirect(renderer);
				}
			}
			return redirected;
		}

		private static int Redirect(Renderer renderer)
		{
			if (renderer == null)
			{
				return 0;
			}
			// sharedMaterials lets the material instance keep its properties and its per-renderer
			// binding; the shader is the only thing that was wrong.
			return UseProjectShaders(renderer.sharedMaterials);
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
