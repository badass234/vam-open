using UnityEngine;
using UnityEngine.Rendering;

public class OVRExternalComposition : OVRComposition
{
	private GameObject foregroundCameraGameObject;

	private Camera foregroundCamera;

	private GameObject backgroundCameraGameObject;

	private Camera backgroundCamera;

	private GameObject cameraProxyPlane;

	public OVRExternalComposition(GameObject parentObject, Camera mainCamera)
	{
		backgroundCameraGameObject = new GameObject();
		backgroundCameraGameObject.name = "MRBackgroundCamera";
		backgroundCameraGameObject.transform.parent = parentObject.transform;
		backgroundCamera = backgroundCameraGameObject.AddComponent<Camera>();
		backgroundCamera.stereoTargetEye = StereoTargetEyeMask.None;
		backgroundCamera.depth = float.MaxValue;
		backgroundCamera.rect = new Rect(0f, 0f, 0.5f, 1f);
		backgroundCamera.clearFlags = mainCamera.clearFlags;
		backgroundCamera.backgroundColor = mainCamera.backgroundColor;
		backgroundCamera.cullingMask = mainCamera.cullingMask & ~(int)OVRManager.instance.extraHiddenLayers;
		backgroundCamera.nearClipPlane = mainCamera.nearClipPlane;
		backgroundCamera.farClipPlane = mainCamera.farClipPlane;
		foregroundCameraGameObject = new GameObject();
		foregroundCameraGameObject.name = "MRForgroundCamera";
		foregroundCameraGameObject.transform.parent = parentObject.transform;
		foregroundCamera = foregroundCameraGameObject.AddComponent<Camera>();
		foregroundCamera.stereoTargetEye = StereoTargetEyeMask.None;
		foregroundCamera.depth = float.MaxValue;
		foregroundCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
		foregroundCamera.clearFlags = CameraClearFlags.Color;
		foregroundCamera.backgroundColor = OVRMixedReality.chromaKeyColor;
		foregroundCamera.cullingMask = mainCamera.cullingMask & ~(int)OVRManager.instance.extraHiddenLayers;
		foregroundCamera.nearClipPlane = mainCamera.nearClipPlane;
		foregroundCamera.farClipPlane = mainCamera.farClipPlane;
		cameraProxyPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
		cameraProxyPlane.name = "MRProxyClipPlane";
		cameraProxyPlane.transform.parent = parentObject.transform;
		cameraProxyPlane.GetComponent<Collider>().enabled = false;
		cameraProxyPlane.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
		Material material = new Material(Shader.Find("Oculus/OVRMRClipPlane"));
		cameraProxyPlane.GetComponent<MeshRenderer>().material = material;
		material.SetColor("_Color", OVRMixedReality.chromaKeyColor);
		material.SetFloat("_Visible", 0f);
		cameraProxyPlane.transform.localScale = new Vector3(1000f, 1000f, 1000f);
		cameraProxyPlane.SetActive(true);
		OVRMRForegroundCameraManager oVRMRForegroundCameraManager = foregroundCameraGameObject.AddComponent<OVRMRForegroundCameraManager>();
		oVRMRForegroundCameraManager.clipPlaneGameObj = cameraProxyPlane;
	}

	public override OVRManager.CompositionMethod CompositionMethod()
	{
		return OVRManager.CompositionMethod.External;
	}

	public override void Update(Camera mainCamera)
	{
		OVRPlugin.SetHandNodePoseStateLatency(0.0);
		backgroundCamera.clearFlags = mainCamera.clearFlags;
		backgroundCamera.backgroundColor = mainCamera.backgroundColor;
		backgroundCamera.cullingMask = mainCamera.cullingMask & ~(int)OVRManager.instance.extraHiddenLayers;
		backgroundCamera.nearClipPlane = mainCamera.nearClipPlane;
		backgroundCamera.farClipPlane = mainCamera.farClipPlane;
		foregroundCamera.cullingMask = mainCamera.cullingMask & ~(int)OVRManager.instance.extraHiddenLayers;
		foregroundCamera.nearClipPlane = mainCamera.nearClipPlane;
		foregroundCamera.farClipPlane = mainCamera.farClipPlane;
		if (OVRMixedReality.useFakeExternalCamera || OVRPlugin.GetExternalCameraCount() == 0)
		{
			OVRPose oVRPose = default(OVRPose);
			OVRPose trackingSpacePose = default(OVRPose);
			trackingSpacePose.position = OVRMixedReality.fakeCameraPositon;
			trackingSpacePose.orientation = OVRMixedReality.fakeCameraRotation;
			oVRPose = OVRExtensions.ToWorldSpacePose(trackingSpacePose);
			backgroundCamera.fieldOfView = OVRMixedReality.fakeCameraFov;
			backgroundCamera.aspect = OVRMixedReality.fakeCameraAspect;
			backgroundCamera.transform.FromOVRPose(oVRPose);
			foregroundCamera.fieldOfView = OVRMixedReality.fakeCameraFov;
			foregroundCamera.aspect = OVRMixedReality.fakeCameraAspect;
			foregroundCamera.transform.FromOVRPose(oVRPose);
		}
		else
		{
			OVRPlugin.CameraExtrinsics cameraExtrinsics;
			OVRPlugin.CameraIntrinsics cameraIntrinsics;
			if (!OVRPlugin.GetMixedRealityCameraInfo(0, out cameraExtrinsics, out cameraIntrinsics))
			{
				Debug.LogError("Failed to get external camera information");
				return;
			}
			OVRPose pose = ComputeCameraWorldSpacePose(cameraExtrinsics);
			float fieldOfView = Mathf.Atan(cameraIntrinsics.FOVPort.UpTan) * 57.29578f * 2f;
			float aspect = cameraIntrinsics.FOVPort.LeftTan / cameraIntrinsics.FOVPort.UpTan;
			backgroundCamera.fieldOfView = fieldOfView;
			backgroundCamera.aspect = aspect;
			backgroundCamera.transform.FromOVRPose(pose);
			foregroundCamera.fieldOfView = fieldOfView;
			foregroundCamera.aspect = cameraIntrinsics.FOVPort.LeftTan / cameraIntrinsics.FOVPort.UpTan;
			foregroundCamera.transform.FromOVRPose(pose);
		}
		Vector3 vector = mainCamera.transform.position - foregroundCamera.transform.position;
		vector.y = 0f;
		cameraProxyPlane.transform.position = mainCamera.transform.position;
		cameraProxyPlane.transform.LookAt(cameraProxyPlane.transform.position + vector);
	}

	public override void Cleanup()
	{
		OVRCompositionUtil.SafeDestroy(ref backgroundCameraGameObject);
		backgroundCamera = null;
		OVRCompositionUtil.SafeDestroy(ref foregroundCameraGameObject);
		foregroundCamera = null;
		OVRCompositionUtil.SafeDestroy(ref cameraProxyPlane);
		Debug.Log("ExternalComposition deactivated");
	}
}
