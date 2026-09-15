using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public static class GameObjectExtension
	{
		public static GameObject InstantiatePrefab(this GameObject prefab, Vector3 position, Quaternion rotation)
		{
			if (prefab == null)
			{
				return null;
			}
			PersistentIgnore component = prefab.GetComponent<PersistentIgnore>();
			if (component != null)
			{
				return component.InstantiatePrefab(prefab, position, rotation);
			}
			return Object.Instantiate(prefab, position, rotation);
		}
	}
}
