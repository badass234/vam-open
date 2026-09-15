using UnityEngine;

public class MKGlowObject : MonoBehaviour
{
	private void OnEnable()
	{
		if (UserPreferences.singleton != null)
		{
			UserPreferences.singleton.RegisterGlowObject();
		}
	}

	private void OnDisable()
	{
		if (UserPreferences.singleton != null)
		{
			UserPreferences.singleton.DeregisterGlowObject();
		}
	}
}
