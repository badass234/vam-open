using System.Collections.Generic;
using UnityEngine;

public class GPUToolsColliderReceiver : MonoBehaviour
{
	protected List<GameObject> _providerGameObjects;

	protected List<GPUToolsColliderProvider> _providers;

	public List<GameObject> providerGameObjects => _providerGameObjects;

	public List<GPUToolsColliderProvider> providers
	{
		get
		{
			return _providers;
		}
		set
		{
			_providers = value;
			SyncProviders();
		}
	}

	public virtual void SyncProviders()
	{
		_providerGameObjects = new List<GameObject>();
		foreach (GPUToolsColliderProvider provider in _providers)
		{
			_providerGameObjects.Add(provider.gameObject);
		}
	}
}
