using UnityEngine;

namespace Battlehub.RTCommon
{
	public class SpriteGizmo : MonoBehaviour, IGL
	{
		public Material Material;

		[SerializeField]
		[HideInInspector]
		private SphereCollider m_collider;

		private void OnEnable()
		{
			GLRenderer instance = GLRenderer.Instance;
			if ((bool)instance)
			{
				instance.Add(this);
			}
			m_collider = GetComponent<SphereCollider>();
			if (m_collider == null)
			{
				m_collider = base.gameObject.AddComponent<SphereCollider>();
				m_collider.radius = 0.25f;
			}
			if (m_collider != null && m_collider.hideFlags == HideFlags.None)
			{
				m_collider.hideFlags = HideFlags.HideInInspector;
			}
		}

		private void OnDisable()
		{
			GLRenderer instance = GLRenderer.Instance;
			if ((bool)instance)
			{
				instance.Remove(this);
			}
			if (m_collider != null)
			{
				Object.Destroy(m_collider);
				m_collider = null;
			}
		}

		void IGL.Draw(int cullingMask)
		{
			RTLayer rTLayer = RTLayer.SceneView;
			if (((uint)cullingMask & (uint)rTLayer) != 0)
			{
				Material.SetPass(0);
				RuntimeGraphics.DrawQuad(base.transform.localToWorldMatrix);
			}
		}
	}
}
