using UnityEngine;

public class ScaleChangeReceiverMeshCollider : ScaleChangeReceiver
{
	private MeshCollider[] _colliders;

	private void Start()
	{
		_colliders = GetComponents<MeshCollider>();
	}

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		MeshCollider[] colliders = _colliders;
		foreach (MeshCollider meshCollider in colliders)
		{
			meshCollider.enabled = false;
			meshCollider.enabled = true;
		}
	}
}
