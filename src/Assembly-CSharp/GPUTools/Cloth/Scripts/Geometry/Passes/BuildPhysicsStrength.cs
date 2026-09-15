using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Painter.Scripts;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Geometry.Passes
{
	public class BuildPhysicsStrength : BuildChainCommand
	{
		private readonly ClothSettings settings;

		public BuildPhysicsStrength(ClothSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			if (settings.EditorStrengthType == ClothEditorType.Texture)
			{
				BlendFromTexture();
			}
			if (settings.EditorStrengthType == ClothEditorType.Painter)
			{
				BlendFromPainter();
			}
		}

		private void BlendFromPainter()
		{
			if (settings.EditorStrengthPainter == null)
			{
				Debug.LogError("Painter field is uninitialized");
				return;
			}
			for (int i = 0; i < settings.GeometryData.ParticlesStrength.Length; i++)
			{
				Color color = settings.EditorStrengthPainter.Colors[i];
				SetBlend(i, color);
			}
		}

		private void BlendFromTexture()
		{
			Texture2D editorStrengthTexture = settings.EditorStrengthTexture;
			if (editorStrengthTexture != null)
			{
				Vector2[] uv = settings.MeshProvider.Mesh.uv;
				for (int i = 0; i < uv.Length; i++)
				{
					Vector2 vector = uv[i];
					Color pixelBilinear = editorStrengthTexture.GetPixelBilinear(vector.x, vector.y);
					SetBlend(i, pixelBilinear);
				}
				if (uv.Length == 0)
				{
					Debug.LogWarning("Add uv to mesh to use vertices blend");
				}
			}
		}

		private void SetBlend(int i, Color color)
		{
			if (settings.StrengthChannel == ColorChannel.R)
			{
				settings.GeometryData.ParticlesStrength[i] = color.r;
			}
			if (settings.StrengthChannel == ColorChannel.G)
			{
				settings.GeometryData.ParticlesStrength[i] = color.g;
			}
			if (settings.StrengthChannel == ColorChannel.B)
			{
				settings.GeometryData.ParticlesStrength[i] = color.b;
			}
			if (settings.StrengthChannel == ColorChannel.A)
			{
				settings.GeometryData.ParticlesStrength[i] = color.a;
			}
		}
	}
}
