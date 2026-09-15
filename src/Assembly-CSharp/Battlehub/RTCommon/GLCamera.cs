using UnityEngine;

namespace Battlehub.RTCommon
{
	[ExecuteInEditMode]
	public class GLCamera : MonoBehaviour
	{
		public int CullingMask = -1;

		private void OnPostRender()
		{
			if (GLRenderer.Instance != null)
			{
				GLRenderer.Instance.Draw(CullingMask);
			}
		}
	}
}
