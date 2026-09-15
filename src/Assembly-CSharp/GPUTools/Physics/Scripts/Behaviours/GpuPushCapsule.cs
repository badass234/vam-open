using UnityEngine;

namespace GPUTools.Physics.Scripts.Behaviours
{
	[ExecuteInEditMode]
	public class GpuPushCapsule : GpuEditCapsule
	{
		private void OnEnable()
		{
			if (capsuleCollider == null)
			{
				capsuleCollider = GetComponent<CapsuleCollider>();
			}
			if (Application.isPlaying)
			{
				GPUCollidersManager.RegisterPushCapsule(this);
			}
			UpdateData();
		}

		private void OnDisable()
		{
			if (Application.isPlaying)
			{
				GPUCollidersManager.DeregisterPushCapsule(this);
			}
		}
	}
}
