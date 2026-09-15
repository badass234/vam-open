using GPUTools.Skinner.Scripts.Providers;
using UnityEngine;

namespace GPUTools.Painter.Scripts
{
	public class PainterSettings : MonoBehaviour
	{
		[SerializeField]
		public MeshProvider MeshProvider = new MeshProvider();

		[SerializeField]
		public ColorBrush Brush;

		[SerializeField]
		public Color[] Colors;

		public Material SharedMaterial => GetComponent<Renderer>().sharedMaterial;

		private void Start()
		{
		}

		private void Update()
		{
		}
	}
}
