using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public static class ObjectExt
	{
		public static bool HasMappedInstanceID(this Object obj)
		{
			if (IdentifiersMap.Instance == null)
			{
				Debug.LogError("Create Resource Map");
			}
			return IdentifiersMap.Instance.IsResource(obj);
		}

		public static long GetMappedInstanceID(this Object obj)
		{
			if (IdentifiersMap.Instance == null)
			{
				Debug.LogError("Create Resource Map");
			}
			return IdentifiersMap.Instance.GetMappedInstanceID(obj);
		}

		public static long[] GetMappedInstanceID(this Object[] objs)
		{
			if (IdentifiersMap.Instance == null)
			{
				Debug.LogError("Create Resource Map");
			}
			return IdentifiersMap.Instance.GetMappedInstanceID(objs);
		}
	}
}
