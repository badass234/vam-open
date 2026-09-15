using UnityEngine;

namespace GPUTools.Physics.Scripts.Behaviours
{
	[ExecuteInEditMode]
	public class GpuRigidityDecreaseCapsule : GpuEditCapsule
	{
		private void OnEnable()
		{
			if (capsuleCollider == null)
			{
				capsuleCollider = GetComponent<CapsuleCollider>();
			}
			if (Application.isPlaying)
			{
				GPUCollidersManager.RegisterRigidityDecreaseCapsule(this);
			}
			UpdateData();
		}

		private void OnDisable()
		{
			if (Application.isPlaying)
			{
				GPUCollidersManager.DeregisterRigidityDecreaseCapsule(this);
			}
		}
	}
}
