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
	/// "&lt;name&gt;ComputeBuff", and it looks that name up with Shader.Find, which answers from
	/// everything in the project - including the placeholder AssetRipper leaves for each family
	/// this project has not transcribed yet. A placeholder compiles, reports itself supported and
	/// looks like any other shader to a material, so the substitution is silent. The originals
	/// still ship in z_sha, the bundle every material in the game resolves its shaders from, so
	/// this class answers a name with the project's reconstruction where there is one and with the
	/// shipped original everywhere else, and hands out a placeholder only when the bundle has
	/// nothing of that name either.
	/// </summary>
	public static class VamShaderProvider
	{
		private const string ShaderBundleName = "z_sha";

		private const string ComputeBuffSuffix = "ComputeBuff";

		private static readonly Dictionary<string, Shader> cache = new Dictionary<string, Shader>();

		private static readonly HashSet<string> warned = new HashSet<string>();

		private static Dictionary<string, Shader> bundleIndex;

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

		/// <summary>
		/// A shader of this name: the project's own reconstruction where this project has one, and
		/// the shipped original everywhere else.
		///
		/// Shader.Find answers as readily with a placeholder as with a reconstruction, and picking
		/// the placeholder draws a substitute rather than the shader that was asked for. That is not
		/// hypothetical: each of the 14 families this project still leaves to AssetRipper is a
		/// one-pass blit, and Hidden/Post FX/* is among them, so a post-processing material built by
		/// name out of Shader.Find is built out of a blit. No drawn material on the boot scene was
		/// found in that state - the census counted 0 of them both before and after this class was
		/// wired into MaterialFactory, and the reports are otherwise identical - so this closes a
		/// path rather than changing a frame. A family <see cref="VamProjectShaders"/> lists is the
		/// project's own; a placeholder is what answers for every other name, so those are read out
		/// of the bundle instead.
		/// </summary>
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
			if (shader != null && VamProjectShaders.Defines(shaderName))
			{
				cache[shaderName] = shader;
				return shader;
			}
			Dictionary<string, Shader> shipped = BundleIndex();
			if (shipped == null)
			{
				// The bundle has not loaded yet. A miss is not cached, and neither is this: the
				// shipped original can still arrive and take the placeholder's place.
				return shader;
			}
			Shader original;
			if (shipped.TryGetValue(shaderName, out original) && original != null)
			{
				cache[shaderName] = original;
				return original;
			}
			if (shader != null)
			{
				cache[shaderName] = shader;
				return shader;
			}
			if (warned.Add(shaderName))
			{
				Debug.LogWarning(string.Format(
					"[VamShaderProvider] {0} is defined by no project shader and is not in the {1} bundle.",
					shaderName, ShaderBundleName));
			}
			return null;
		}

		/// <summary>
		/// Points a material at the shader of that name this project would use, and answers whether
		/// it moved.
		///
		/// Two different materials are wrong in the same way. One came out of a bundle, so it holds
		/// the bundle's own copy of the family as a serialized reference and keeps it even once this
		/// project reconstructs the family - the census reads those slots as "bundle (project defines
		/// one)". The other holds an AssetRipper placeholder, which compiles and reports itself
		/// supported while drawing far less than the original; that is the state a material built in
		/// code by name lands in, and no material on the boot scene was measured in it, so this
		/// redirects the path rather than a material. <see cref="FindByName"/> answers with the
		/// reconstruction where there is one and with the shipped original everywhere else, and it
		/// answers with the placeholder only when the bundle has nothing of that name either, so a
		/// shader it answers with is the one the material should hold.
		///
		/// A material already on the right shader is not touched. The hair's optimised path is one of
		/// those: GPUTools/MeshedVR/HairOpt is a placeholder here, the bundle has it, so the material
		/// out of the bundle already holds the shipped shader and stays where it is.
		/// </summary>
		public static bool UseProjectShader(Material material)
		{
			if (material == null)
			{
				return false;
			}
			Shader current = material.shader;
			if (current == null)
			{
				return false;
			}
			Shader resolved = FindByName(current.name);
			if (resolved == null || resolved == current)
			{
				return false;
			}
			material.shader = resolved;
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
