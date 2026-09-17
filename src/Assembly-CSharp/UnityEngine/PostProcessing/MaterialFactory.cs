using System;
using System.Collections.Generic;

namespace UnityEngine.PostProcessing
{
	public sealed class MaterialFactory : IDisposable
	{
		private Dictionary<string, Material> m_Materials;

		public MaterialFactory()
		{
			m_Materials = new Dictionary<string, Material>();
		}

		public Material Get(string shaderName)
		{
			Material value;
			if (!m_Materials.TryGetValue(shaderName, out value))
			{
				// The post-processing stack builds its materials by name, and this project holds
				// AssetRipper placeholders under exactly these names - Assets\Resources\shaders is
				// 14 one-pass blits that answer to Hidden/Post FX/*, so Shader.Find would answer with
				// one of those and the effect would draw a blit while the shipped implementation sat
				// unread in the z_sha bundle. Routing the lookup through VamShaderProvider answers with
				// the shipped shader, and with this project's own transcription of it once there is
				// one. Nothing attaches PostProcessingBehaviour yet, so this closes the path ahead of
				// the behaviour rather than changing what the game draws.
				Shader shader = MeshVR.VamShaderProvider.FindByName(shaderName);
				if (shader == null)
				{
					throw new ArgumentException($"Shader not found ({shaderName})");
				}
				Material material = new Material(shader);
				material.name = string.Format("PostFX - {0}", shaderName.Substring(shaderName.LastIndexOf("/") + 1));
				material.hideFlags = HideFlags.DontSave;
				value = material;
				m_Materials.Add(shaderName, value);
			}
			return value;
		}

		public void Dispose()
		{
			Dictionary<string, Material>.Enumerator enumerator = m_Materials.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Material value = enumerator.Current.Value;
				GraphicsUtils.Destroy(value);
			}
			m_Materials.Clear();
		}
	}
}
