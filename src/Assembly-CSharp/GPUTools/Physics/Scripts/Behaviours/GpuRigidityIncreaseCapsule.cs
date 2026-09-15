using UnityEngine;

namespace GPUTools.Physics.Scripts.Behaviours
{
	[ExecuteInEditMode]
	public class GpuRigidityIncreaseCapsule : GpuEditCapsule
	{
		private void OnEnable()
		{
			if (capsuleCollider == null)
			{
				capsuleCollider = GetComponent<CapsuleCollider>();
			}
			if (Application.isPlaying)
			{
				GPUCollidersManager.RegisterRigidityIncreaseCapsule(this);
			}
			UpdateData();
		}

		private void OnDisable()
		{
			if (Application.isPlaying)
			{
				GPUCollidersManager.DeregisterRigidityIncreaseCapsule(this);
			}
		}
	}
}
