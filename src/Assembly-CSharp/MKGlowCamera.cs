using MK.Glow;
using UnityEngine;

public class MKGlowCamera : MonoBehaviour
{
	private MKGlow glow;

	private void OnEnable()
	{
		glow = GetComponent<MKGlow>();
		if (glow != null && UserPreferences.singleton != null)
		{
			UserPreferences.singleton.RegisterGlowCamera(glow);
		}
	}

	private void OnDisable()
	{
		if (glow != null && UserPreferences.singleton != null)
		{
			UserPreferences.singleton.DeregisterGlowCamera(glow);
		}
	}
}
