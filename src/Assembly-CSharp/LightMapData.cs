using System;
using UnityEngine;

public class LightMapData : MonoBehaviour
{
	[Serializable]
	public struct RendererInfo
	{
		public Renderer renderer;

		public int lightmapIndex;

		public Vector4 lightmapOffsetScale;
	}

	[SerializeField]
	public RendererInfo m_RendererInfo;

	public void GetInfo()
	{
		m_RendererInfo.renderer = GetComponent<Renderer>();
		if ((bool)m_RendererInfo.renderer)
		{
			m_RendererInfo.lightmapIndex = m_RendererInfo.renderer.lightmapIndex;
			m_RendererInfo.lightmapOffsetScale = m_RendererInfo.renderer.lightmapScaleOffset;
		}
	}
}
