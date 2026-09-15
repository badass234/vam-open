using UnityEngine;

namespace MeshVR.Hands
{
	public class HandInputInitializer : MonoBehaviour
	{
		protected virtual void Awake()
		{
			HandInput[] componentsInChildren = GetComponentsInChildren<HandInput>(true);
			HandInput[] array = componentsInChildren;
			foreach (HandInput handInput in array)
			{
				handInput.Init();
			}
		}
	}
}
