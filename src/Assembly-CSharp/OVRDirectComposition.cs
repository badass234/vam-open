using UnityEngine;

public class OVRDirectComposition : OVRCameraComposition
{
	public GameObject directCompositionCameraGameObject;

	public Camera directCompositionCamera;

	public RenderTexture boundaryMeshMaskTexture;

	public OVRDirectComposition(GameObject parentObject, Camera mainCamera, OVRManager.CameraDevice cameraDevice, bool useDynamicLighting, OVRManager.DepthQuality depthQuality)
		: base(cameraDevice, useDynamicLighting, depthQuality)
	{
		directCompositionCameraGameObject = new GameObject();
		directCompositionCameraGameObject.name = "MRDirectCompositionCamera";
		directCompositionCameraGameObject.transform.parent = parentObject.transform;
		directCompositionCamera = directCompositionCameraGameObject.AddComponent<Camera>();
		directCompositionCamera.stereoTargetEye = StereoTargetEyeMask.None;
		directCompositionCamera.depth = float.MaxValue;
		directCompositionCamera.rect = new Rect(0f, 0f, 1f, 1f);
		directCompositionCamera.clearFlags = mainCamera.clearFlags;
		directCompositionCamera.backgroundColor = mainCamera.backgroundColor;
		directCompositionCamera.cullingMask = mainCamera.cullingMask & ~(int)OVRManager.instance.extraHiddenLayers;
		directCompositionCamera.nearClipPlane = mainCamera.nearClipPlane;
		directCompositionCamera.farClipPlane = mainCamera.farClipPlane;
		if (!hasCameraDeviceOpened)
		{
			Debug.LogError("Unable to open camera device " + cameraDevice);
			return;
		}
		Debug.Log("DirectComposition activated : useDynamicLighting " + ((!useDynamicLighting) ? "OFF" : "ON"));
		CreateCameraFramePlaneObject(parentObject, directCompositionCamera, useDynamicLighting);
	}

	public override OVRManager.CompositionMethod CompositionMethod()
	{
		return OVRManager.CompositionMethod.Direct;
	}

	public override void Update(Camera mainCamera)
	{
		if (!hasCameraDeviceOpened)
		{
			return;
		}
		if (!OVRPlugin.SetHandNodePoseStateLatency(OVRManager.instance.handPoseStateLatency))
		{
			Debug.LogWarning("HandPoseStateLatency is invalid. Expect a value between 0.0 to 0.5, get " + OVRManager.instance.handPoseStateLatency);
		}
		directCompositionCamera.clearFlags = mainCamera.clearFlags;
		directCompositionCamera.backgroundColor = mainCamera.backgroundColor;
		directCompositionCamera.cullingMask = mainCamera.cullingMask & ~(int)OVRManager.instance.extraHiddenLayers;
		directCompositionCamera.nearClipPlane = mainCamera.nearClipPlane;
		directCompositionCamera.farClipPlane = mainCamera.farClipPlane;
		OVRPlugin.CameraExtrinsics cameraExtrinsics;
		OVRPlugin.CameraIntrinsics cameraIntrinsics;
		if (OVRMixedReality.useFakeExternalCamera || OVRPlugin.GetExternalCameraCount() == 0)
		{
			OVRPose oVRPose = default(OVRPose);
			OVRPose trackingSpacePose = default(OVRPose);
			trackingSpacePose.position = OVRMixedReality.fakeCameraPositon;
			trackingSpacePose.orientation = OVRMixedReality.fakeCameraRotation;
			oVRPose = OVRExtensions.ToWorldSpacePose(trackingSpacePose);
			directCompositionCamera.fieldOfView = OVRMixedReality.fakeCameraFov;
			directCompositionCamera.aspect = OVRMixedReality.fakeCameraAspect;
			directCompositionCamera.transform.FromOVRPose(oVRPose);
		}
		else if (OVRPlugin.GetMixedRealityCameraInfo(0, out cameraExtrinsics, out cameraIntrinsics))
		{
			OVRPose pose = ComputeCameraWorldSpacePose(cameraExtrinsics);
			float fieldOfView = Mathf.Atan(cameraIntrinsics.FOVPort.UpTan) * 57.29578f * 2f;
			float aspect = cameraIntrinsics.FOVPort.LeftTan / cameraIntrinsics.FOVPort.UpTan;
			directCompositionCamera.fieldOfView = fieldOfView;
			directCompositionCamera.aspect = aspect;
			directCompositionCamera.transform.FromOVRPose(pose);
		}
		else
		{
			Debug.LogWarning("Failed to get external camera information");
		}
		if (hasCameraDeviceOpened)
		{
			if (boundaryMeshMaskTexture == null || boundaryMeshMaskTexture.width != Screen.width || boundaryMeshMaskTexture.height != Screen.height)
			{
				boundaryMeshMaskTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.R8);
				boundaryMeshMaskTexture.Create();
			}
			UpdateCameraFramePlaneObject(mainCamera, directCompositionCamera, boundaryMeshMaskTexture);
			directCompositionCamera.GetComponent<OVRCameraFrameCompositionManager>().boundaryMeshMaskTexture = boundaryMeshMaskTexture;
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		OVRCompositionUtil.SafeDestroy(ref directCompositionCameraGameObject);
		directCompositionCamera = null;
		Debug.Log("DirectComposition deactivated");
	}
}
