using UnityEngine;

namespace GPUTools.Physics.Scripts.Behaviours
{
	public class PlaneCollider : MonoBehaviour
	{
		public Vector4 GetWorldData()
		{
			Vector3 up = base.transform.up;
			Vector3 position = base.transform.position;
			float num = position.x * up.x + position.y * up.y + position.z * up.z;
			return new Vector4(up.x, up.y, up.z, 0f - num);
		}

		private void OnEnable()
		{
			if (Application.isPlaying)
			{
				GPUCollidersManager.RegisterPlaneCollider(this);
			}
		}

		private void OnDisable()
		{
			if (Application.isPlaying)
			{
				GPUCollidersManager.DeregisterPlaneCollider(this);
			}
		}
	}
}
