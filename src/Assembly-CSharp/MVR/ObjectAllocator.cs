using System.Collections.Generic;
using UnityEngine;

namespace MVR
{
	public class ObjectAllocator : MonoBehaviour
	{
		protected List<Object> allocatedObjects;

		protected void RegisterAllocatedObject(Object o)
		{
			if (Application.isPlaying)
			{
				if (allocatedObjects == null)
				{
					allocatedObjects = new List<Object>();
				}
				allocatedObjects.Add(o);
			}
		}

		protected void DestroyAllocatedObjects()
		{
			if (!Application.isPlaying || allocatedObjects == null)
			{
				return;
			}
			foreach (Object allocatedObject in allocatedObjects)
			{
				Object.Destroy(allocatedObject);
			}
		}

		protected virtual void OnDestroy()
		{
			DestroyAllocatedObjects();
		}
	}
}
