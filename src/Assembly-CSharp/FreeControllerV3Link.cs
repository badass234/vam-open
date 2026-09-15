using MVR;
using UnityEngine;

public class FreeControllerV3Link : MonoBehaviour
{
	public FreeControllerV3 linkedController;

	private void Update()
	{
		if ((bool)linkedController && linkedController.followWhenOff != null)
		{
			Vector3 position = linkedController.followWhenOff.position;
			if (NaNUtils.IsVector3Valid(position))
			{
				base.transform.position = position;
			}
		}
	}
}
