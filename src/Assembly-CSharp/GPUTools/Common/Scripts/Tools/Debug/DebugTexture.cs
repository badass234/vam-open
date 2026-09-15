using UnityEngine;

namespace GPUTools.Common.Scripts.Tools.Debug
{
	public class DebugTexture : MonoBehaviour
	{
		public Texture Texture { get; set; }

		public static void SetTexture(Texture texture)
		{
			GameObject gameObject = new GameObject("DebugTexture");
			DebugTexture debugTexture = gameObject.AddComponent<DebugTexture>();
			debugTexture.Texture = texture;
		}

		private void OnGUI()
		{
			GUI.DrawTexture(new Rect(0f, 0f, 400f, 400f), Texture, ScaleMode.ScaleToFit, false, 1f);
		}
	}
}
