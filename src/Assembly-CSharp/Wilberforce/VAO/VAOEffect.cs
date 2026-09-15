using UnityEngine;

namespace Wilberforce.VAO
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[HelpURL("https://projectwilberforce.github.io/vaomanual/")]
	[AddComponentMenu("Image Effects/Rendering/Volumetric Ambient Occlusion")]
	public class VAOEffect : VAOEffectCommandBuffer
	{
		[ImageEffectOpaque]
		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			PerformOnRenderImage(source, destination);
		}
	}
}
