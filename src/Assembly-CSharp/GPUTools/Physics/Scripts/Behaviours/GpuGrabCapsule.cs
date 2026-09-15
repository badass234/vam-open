using UnityEngine;

namespace GPUTools.Physics.Scripts.Behaviours
{
	[ExecuteInEditMode]
	public class GpuGrabCapsule : GpuEditCapsule
	{
		protected Matrix4x4 _lastWorldToLocalMatrix;

		protected Matrix4x4 _changeMatrix;

		public Matrix4x4 changeMatrix => _changeMatrix;

		private void OnEnable()
		{
			if (capsuleCollider == null)
			{
				capsuleCollider = GetComponent<CapsuleCollider>();
			}
			if (Application.isPlaying)
			{
				GPUCollidersManager.RegisterGrabCapsule(this);
			}
			UpdateData();
		}

		private void OnDisable()
		{
			if (Application.isPlaying)
			{
				GPUCollidersManager.DeregisterGrabCapsule(this);
			}
		}

		public override void UpdateData()
		{
			base.UpdateData();
			_changeMatrix = base.transform.localToWorldMatrix * _lastWorldToLocalMatrix;
			_lastWorldToLocalMatrix = base.transform.worldToLocalMatrix;
		}
	}
}
