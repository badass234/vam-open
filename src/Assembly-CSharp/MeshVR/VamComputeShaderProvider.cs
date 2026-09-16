using System;
using System.Collections.Generic;
using System.IO;
using AssetBundles;
using UnityEngine;

namespace MeshVR
{
	/// <summary>
	/// The two compute shaders the DAZ skins are skinned with never made it through the rip:
	/// AssetRipper does not extract compute shaders, so GPUSkinCompute and GPUMeshCompute are
	/// unassigned on every prefab and DAZImport silently skips the whole GPU skinning path.
	/// Both originals still ship inside the z_sha bundle, so they are recovered from there and
	/// cached for the session.
	/// </summary>
	public static class VamComputeShaderProvider
	{
		public const string SkinShaderName = "DAZGPUSkin";

		public const string MeshShaderName = "MeshGPU";

		private const string SkinShaderAsset = "assets/meshedvr_scripts_dazcore/dazgpuskin.compute";

		private const string MeshShaderAsset = "assets/meshedvr_scripts_main/geometrygpu/meshgpu.compute";

		private const string ShaderBundleName = "z_sha";

		private const float RetryInterval = 5f;

		private static readonly Dictionary<string, ComputeShader> cache = new Dictionary<string, ComputeShader>();

		private static AssetBundle shaderBundle;

		private static float nextAttemptTime;

		private static bool failureLogged;

		public static ComputeShader SkinShader
		{
			get
			{
				return GetShader(SkinShaderName, SkinShaderAsset);
			}
		}

		public static ComputeShader MeshShader
		{
			get
			{
				return GetShader(MeshShaderName, MeshShaderAsset);
			}
		}

		public static ComputeShader GetShader(string shaderName, string assetName)
		{
			ComputeShader shader;
			if (cache.TryGetValue(shaderName, out shader))
			{
				return shader;
			}
			// A miss is not cached: callers run every frame, and the bundles may still be loading.
			if (Time.realtimeSinceStartup < nextAttemptTime)
			{
				return null;
			}
			nextAttemptTime = Time.realtimeSinceStartup + RetryInterval;
			shader = SearchLoadedBundles(shaderName, assetName);
			if (shader == null)
			{
				shader = SearchBundle(OpenShaderBundle(), shaderName, assetName);
			}
			if (shader == null)
			{
				if (!failureLogged)
				{
					failureLogged = true;
					Debug.LogError(string.Format(
						"[VamComputeShaderProvider] {0} is not available; GPU skinning stays disabled. Expected it in the {1} bundle ({2}).",
						shaderName, ShaderBundleName, assetName));
				}
				return null;
			}
			cache[shaderName] = shader;
			return shader;
		}

		private static ComputeShader SearchLoadedBundles(string shaderName, string assetName)
		{
			foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
			{
				ComputeShader shader = SearchBundle(bundle, shaderName, assetName);
				if (shader != null)
				{
					return shader;
				}
			}
			return null;
		}

		private static ComputeShader SearchBundle(AssetBundle bundle, string shaderName, string assetName)
		{
			if (bundle == null)
			{
				return null;
			}
			string[] assetNames = bundle.GetAllAssetNames();
			for (int i = 0; i < assetNames.Length; i++)
			{
				string candidate = assetNames[i];
				if (!string.Equals(candidate, assetName, StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(Path.GetFileNameWithoutExtension(candidate), shaderName, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				ComputeShader shader = bundle.LoadAsset<ComputeShader>(candidate);
				if (shader != null)
				{
					return shader;
				}
			}
			return null;
		}

		private static AssetBundle OpenShaderBundle()
		{
			if (shaderBundle != null)
			{
				return shaderBundle;
			}
			shaderBundle = FindLoadedBundle(ShaderBundleName);
			if (shaderBundle != null)
			{
				return shaderBundle;
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
			shaderBundle = AssetBundle.LoadFromFile(path);
			return shaderBundle;
		}

		private static AssetBundle FindLoadedBundle(string bundleName)
		{
			foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
			{
				if (bundle != null && string.Equals(bundle.name, bundleName, StringComparison.OrdinalIgnoreCase))
				{
					return bundle;
				}
			}
			return null;
		}
	}
}
