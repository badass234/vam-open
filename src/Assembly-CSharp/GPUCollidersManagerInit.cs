using UnityEngine;

public class GPUCollidersManagerInit : MonoBehaviour
{
	private void Awake()
	{
		GPUCollidersManager component = GetComponent<GPUCollidersManager>();
		if (component != null)
		{
			component.Init();
		}
	}
}
