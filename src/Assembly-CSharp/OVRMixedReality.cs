using UnityEngine;

internal static class OVRMixedReality
{
	public static Color chromaKeyColor = Color.green;

	public static bool useFakeExternalCamera = false;

	public static Vector3 fakeCameraPositon = new Vector3(3f, 0f, 3f);

	public static Quaternion fakeCameraRotation = Quaternion.LookRotation((new Vector3(0f, 1f, 0f) - fakeCameraPositon).normalized, Vector3.up);

	public static float fakeCameraFov = 60f;

	public static float fakeCameraAspect = 1.7777778f;

	public static OVRComposition currentComposition = null;

	public static void Update(GameObject parentObject, Camera mainCamera, OVRManager.CompositionMethod compositionMethod, bool useDynamicLighting, OVRManager.CameraDevice cameraDevice, OVRManager.DepthQuality depthQuality)
	{
		if (!OVRPlugin.initialized)
		{
			Debug.LogError("OVRPlugin not initialized");
			return;
		}
		if (!OVRPlugin.IsMixedRealityInitialized())
		{
			OVRPlugin.InitializeMixedReality();
		}
		if (!OVRPlugin.IsMixedRealityInitialized())
		{
			Debug.LogError("Unable to initialize MixedReality");
			return;
		}
		OVRPlugin.UpdateExternalCamera();
		OVRPlugin.UpdateCameraDevices();
		if (currentComposition != null && currentComposition.CompositionMethod() != compositionMethod)
		{
			currentComposition.Cleanup();
			currentComposition = null;
		}
		switch (compositionMethod)
		{
		case OVRManager.CompositionMethod.External:
			if (currentComposition == null)
			{
				currentComposition = new OVRExternalComposition(parentObject, mainCamera);
			}
			break;
		case OVRManager.CompositionMethod.Direct:
			if (currentComposition == null)
			{
				currentComposition = new OVRDirectComposition(parentObject, mainCamera, cameraDevice, useDynamicLighting, depthQuality);
			}
			break;
		case OVRManager.CompositionMethod.Sandwich:
			if (currentComposition == null)
			{
				currentComposition = new OVRSandwichComposition(parentObject, mainCamera, cameraDevice, useDynamicLighting, depthQuality);
			}
			break;
		default:
			Debug.LogError("Unknown CompositionMethod : " + compositionMethod);
			return;
		}
		currentComposition.Update(mainCamera);
	}

	public static void Cleanup()
	{
		if (currentComposition != null)
		{
			currentComposition.Cleanup();
			currentComposition = null;
		}
		if (OVRPlugin.IsMixedRealityInitialized())
		{
			OVRPlugin.ShutdownMixedReality();
		}
	}

	public static void RecenterPose()
	{
		if (currentComposition != null)
		{
			currentComposition.RecenterPose();
		}
	}
}
