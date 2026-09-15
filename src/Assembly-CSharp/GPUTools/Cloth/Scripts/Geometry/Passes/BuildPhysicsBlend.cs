using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Painter.Scripts;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Geometry.Passes
{
	public class BuildPhysicsBlend : BuildChainCommand
	{
		private readonly ClothSettings settings;

		public BuildPhysicsBlend(ClothSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnUpdateSettings()
		{
			OnBuild();
		}

		protected override void OnBuild()
		{
			if (settings.EditorType == ClothEditorType.Texture)
			{
				BlendFromTexture();
			}
			if (settings.EditorType == ClothEditorType.Painter)
			{
				BlendFromPainter();
			}
			if (settings.EditorType == ClothEditorType.Provider)
			{
				BlendFromProvider();
			}
		}

		private void BlendFromPainter()
		{
			if (settings.EditorPainter == null)
			{
				Debug.LogError("Painter field is uninitialized");
				return;
			}
			for (int i = 0; i < settings.GeometryData.ParticlesBlend.Length; i++)
			{
				Color color = settings.EditorPainter.Colors[i];
				SetBlend(i, color);
			}
		}

		private void BlendFromTexture()
		{
			Texture2D editorTexture = settings.EditorTexture;
			Vector2[] uv = settings.MeshProvider.Mesh.uv;
			for (int i = 0; i < uv.Length; i++)
			{
				Vector2 vector = uv[i];
				Color pixelBilinear = editorTexture.GetPixelBilinear(vector.x, vector.y);
				SetBlend(i, pixelBilinear);
			}
			if (uv.Length == 0)
			{
				Debug.LogWarning("Add uv to mesh to use vertices blend");
			}
		}

		private void BlendFromProvider()
		{
			Color[] simColors = settings.MeshProvider.SimColors;
			if (simColors != null)
			{
				for (int i = 0; i < settings.GeometryData.ParticlesBlend.Length; i++)
				{
					Color color = simColors[i];
					SetBlend(i, color);
				}
			}
			else
			{
				for (int j = 0; j < settings.GeometryData.ParticlesBlend.Length; j++)
				{
					SetZeroBlend(j);
				}
			}
		}

		private void SetBlend(int i, Color color)
		{
			if (settings.SimulateVsKinematicChannel == ColorChannel.R)
			{
				settings.GeometryData.ParticlesBlend[i] = color.r;
			}
			if (settings.SimulateVsKinematicChannel == ColorChannel.G)
			{
				settings.GeometryData.ParticlesBlend[i] = color.g;
			}
			if (settings.SimulateVsKinematicChannel == ColorChannel.B)
			{
				settings.GeometryData.ParticlesBlend[i] = color.b;
			}
			if (settings.SimulateVsKinematicChannel == ColorChannel.A)
			{
				settings.GeometryData.ParticlesBlend[i] = color.a;
			}
		}

		private void SetZeroBlend(int i)
		{
			settings.GeometryData.ParticlesBlend[i] = 0f;
		}
	}
}
