using UnityEngine;

namespace GPUTools.Physics.Scripts.Behaviours
{
	[ExecuteInEditMode]
	public class GpuHoldCapsule : GpuEditCapsule
	{
		private void OnEnable()
		{
			if (capsuleCollider == null)
			{
				capsuleCollider = GetComponent<CapsuleCollider>();
			}
			if (Application.isPlaying)
			{
				GPUCollidersManager.RegisterHoldCapsule(this);
			}
			UpdateData();
		}

		private void OnDisable()
		{
			if (Application.isPlaying)
			{
				GPUCollidersManager.DeregisterHoldCapsule(this);
			}
		}
	}
}
