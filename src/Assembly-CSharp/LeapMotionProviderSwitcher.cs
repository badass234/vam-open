using Leap.Unity;
using UnityEngine;

public class LeapMotionProviderSwitcher : MonoBehaviour
{
	public HandModelManager handModelManager;

	private void OnEnable()
	{
		LeapXRServiceProvider component = GetComponent<LeapXRServiceProvider>();
		if (component != null && handModelManager != null)
		{
			handModelManager.leapProvider = component;
		}
	}
}
