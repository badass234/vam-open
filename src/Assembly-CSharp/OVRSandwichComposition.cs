using UnityEngine;
using UnityEngine.Rendering;

public class OVRSandwichComposition : OVRCameraComposition
{
	public class HistoryRecord
	{
		public float timestamp = float.MinValue;

		public RenderTexture fgRenderTexture;

		public RenderTexture bgRenderTexture;

		public RenderTexture boundaryMeshMaskTexture;
	}

	public class OVRSandwichCompositionManager : MonoBehaviour
	{
		public RenderTexture fgTexture;

		public RenderTexture bgTexture;

		public Material alphaBlendMaterial;

		private void Start()
		{
			Shader shader = Shader.Find("Oculus/UnlitTransparent");
			if (shader == null)
			{
				Debug.LogError("Unable to create transparent shader");
			}
			else
			{
				alphaBlendMaterial = new Material(shader);
			}
		}

		private void OnPreRender()
		{
			if (fgTexture == null || bgTexture == null || alphaBlendMaterial == null)
			{
				Debug.LogError("OVRSandwichCompositionManager has not setup properly");
			}
			else
			{
				Graphics.Blit(bgTexture, RenderTexture.active);
			}
		}

		private void OnPostRender()
		{
			if (fgTexture == null || bgTexture == null || alphaBlendMaterial == null)
			{
				Debug.LogError("OVRSandwichCompositionManager has not setup properly");
			}
			else
			{
				Graphics.Blit(fgTexture, RenderTexture.active, alphaBlendMaterial);
			}
		}
	}

	public float frameRealtime;

	public Camera fgCamera;

	public Camera bgCamera;

	public readonly int historyRecordCount = 8;

	public readonly HistoryRecord[] historyRecordArray;

	public int historyRecordCursorIndex;

	public GameObject cameraProxyPlane;

	public Camera compositionCamera;

	public OVRSandwichCompositionManager compositionManager;

	private int _cameraFramePlaneLayer = -1;

	public int cameraFramePlaneLayer
	{
		get
		{
			if (_cameraFramePlaneLayer < 0)
			{
				for (int i = 24; i <= 29; i++)
				{
					string text = LayerMask.LayerToName(i);
					if (text == null || text.Length == 0)
					{
						_cameraFramePlaneLayer = i;
						break;
					}
				}
				if (_cameraFramePlaneLayer == -1)
				{
					Debug.LogWarning("Unable to find an unnamed layer between 24 and 29.");
					_cameraFramePlaneLayer = 25;
				}
				Debug.LogFormat("Set the CameraFramePlaneLayer in SandwichComposition to {0}. Please do NOT put any other gameobject in this layer.", _cameraFramePlaneLayer);
			}
			return _cameraFramePlaneLayer;
		}
	}

	public OVRSandwichComposition(GameObject parentObject, Camera mainCamera, OVRManager.CameraDevice cameraDevice, bool useDynamicLighting, OVRManager.DepthQuality depthQuality)
		: base(cameraDevice, useDynamicLighting, depthQuality)
	{
		frameRealtime = Time.realtimeSinceStartup;
		historyRecordCount = OVRManager.instance.sandwichCompositionBufferedFrames;
		if (historyRecordCount < 1)
		{
			Debug.LogWarning("Invalid sandwichCompositionBufferedFrames in OVRManager. It should be at least 1");
			historyRecordCount = 1;
		}
		if (historyRecordCount > 16)
		{
			Debug.LogWarning("The value of sandwichCompositionBufferedFrames in OVRManager is too big. It would consume a lot of memory. It has been override to 16");
			historyRecordCount = 16;
		}
		historyRecordArray = new HistoryRecord[historyRecordCount];
		for (int i = 0; i < historyRecordCount; i++)
		{
			historyRecordArray[i] = new HistoryRecord();
		}
		historyRecordCursorIndex = 0;
		GameObject gameObject = new GameObject("MRSandwichForegroundCamera");
		gameObject.transform.parent = parentObject.transform;
		fgCamera = gameObject.AddComponent<Camera>();
		fgCamera.depth = 200f;
		fgCamera.clearFlags = CameraClearFlags.Color;
		fgCamera.backgroundColor = Color.clear;
		fgCamera.cullingMask = mainCamera.cullingMask & ~(int)OVRManager.instance.extraHiddenLayers;
		fgCamera.nearClipPlane = mainCamera.nearClipPlane;
		fgCamera.farClipPlane = mainCamera.farClipPlane;
		GameObject gameObject2 = new GameObject("MRSandwichBackgroundCamera");
		gameObject2.transform.parent = parentObject.transform;
		bgCamera = gameObject2.AddComponent<Camera>();
		bgCamera.depth = 100f;
		bgCamera.clearFlags = mainCamera.clearFlags;
		bgCamera.backgroundColor = mainCamera.backgroundColor;
		bgCamera.cullingMask = mainCamera.cullingMask & ~(int)OVRManager.instance.extraHiddenLayers;
		bgCamera.nearClipPlane = mainCamera.nearClipPlane;
		bgCamera.farClipPlane = mainCamera.farClipPlane;
		cameraProxyPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
		cameraProxyPlane.name = "MRProxyClipPlane";
		cameraProxyPlane.transform.parent = parentObject.transform;
		cameraProxyPlane.GetComponent<Collider>().enabled = false;
		cameraProxyPlane.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
		Material material = new Material(Shader.Find("Oculus/OVRMRClipPlane"));
		cameraProxyPlane.GetComponent<MeshRenderer>().material = material;
		material.SetColor("_Color", Color.clear);
		material.SetFloat("_Visible", 0f);
		cameraProxyPlane.transform.localScale = new Vector3(1000f, 1000f, 1000f);
		cameraProxyPlane.SetActive(true);
		OVRMRForegroundCameraManager oVRMRForegroundCameraManager = fgCamera.gameObject.AddComponent<OVRMRForegroundCameraManager>();
		oVRMRForegroundCameraManager.clipPlaneGameObj = cameraProxyPlane;
		GameObject gameObject3 = new GameObject("MRSandwichCaptureCamera");
		gameObject3.transform.parent = parentObject.transform;
		compositionCamera = gameObject3.AddComponent<Camera>();
		compositionCamera.stereoTargetEye = StereoTargetEyeMask.None;
		compositionCamera.depth = float.MaxValue;
		compositionCamera.rect = new Rect(0f, 0f, 1f, 1f);
		compositionCamera.clearFlags = CameraClearFlags.Depth;
		compositionCamera.backgroundColor = mainCamera.backgroundColor;
		compositionCamera.cullingMask = 1 << cameraFramePlaneLayer;
		compositionCamera.nearClipPlane = mainCamera.nearClipPlane;
		compositionCamera.farClipPlane = mainCamera.farClipPlane;
		if (!hasCameraDeviceOpened)
		{
			Debug.LogError("Unable to open camera device " + cameraDevice);
			return;
		}
		Debug.Log("SandwichComposition activated : useDynamicLighting " + ((!useDynamicLighting) ? "OFF" : "ON"));
		CreateCameraFramePlaneObject(parentObject, compositionCamera, useDynamicLighting);
		cameraFramePlaneObject.layer = cameraFramePlaneLayer;
		RefreshRenderTextures(mainCamera);
		compositionManager = compositionCamera.gameObject.AddComponent<OVRSandwichCompositionManager>();
		compositionManager.fgTexture = historyRecordArray[historyRecordCursorIndex].fgRenderTexture;
		compositionManager.bgTexture = historyRecordArray[historyRecordCursorIndex].bgRenderTexture;
	}

	public override OVRManager.CompositionMethod CompositionMethod()
	{
		return OVRManager.CompositionMethod.Sandwich;
	}

	public override void Update(Camera mainCamera)
	{
		if (hasCameraDeviceOpened)
		{
			frameRealtime = Time.realtimeSinceStartup;
			historyRecordCursorIndex++;
			if (historyRecordCursorIndex >= historyRecordCount)
			{
				historyRecordCursorIndex = 0;
			}
			if (!OVRPlugin.SetHandNodePoseStateLatency(OVRManager.instance.handPoseStateLatency))
			{
				Debug.LogWarning("HandPoseStateLatency is invalid. Expect a value between 0.0 to 0.5, get " + OVRManager.instance.handPoseStateLatency);
			}
			RefreshRenderTextures(mainCamera);
			bgCamera.clearFlags = mainCamera.clearFlags;
			bgCamera.backgroundColor = mainCamera.backgroundColor;
			bgCamera.cullingMask = mainCamera.cullingMask & ~(int)OVRManager.instance.extraHiddenLayers;
			fgCamera.cullingMask = mainCamera.cullingMask & ~(int)OVRManager.instance.extraHiddenLayers;
			OVRPlugin.CameraExtrinsics cameraExtrinsics;
			OVRPlugin.CameraIntrinsics cameraIntrinsics;
			if (OVRMixedReality.useFakeExternalCamera || OVRPlugin.GetExternalCameraCount() == 0)
			{
				OVRPose oVRPose = default(OVRPose);
				OVRPose trackingSpacePose = default(OVRPose);
				trackingSpacePose.position = OVRMixedReality.fakeCameraPositon;
				trackingSpacePose.orientation = OVRMixedReality.fakeCameraRotation;
				oVRPose = OVRExtensions.ToWorldSpacePose(trackingSpacePose);
				RefreshCameraPoses(OVRMixedReality.fakeCameraFov, OVRMixedReality.fakeCameraAspect, oVRPose);
			}
			else if (OVRPlugin.GetMixedRealityCameraInfo(0, out cameraExtrinsics, out cameraIntrinsics))
			{
				OVRPose pose = ComputeCameraWorldSpacePose(cameraExtrinsics);
				float fovY = Mathf.Atan(cameraIntrinsics.FOVPort.UpTan) * 57.29578f * 2f;
				float aspect = cameraIntrinsics.FOVPort.LeftTan / cameraIntrinsics.FOVPort.UpTan;
				RefreshCameraPoses(fovY, aspect, pose);
			}
			else
			{
				Debug.LogWarning("Failed to get external camera information");
			}
			compositionCamera.GetComponent<OVRCameraFrameCompositionManager>().boundaryMeshMaskTexture = historyRecordArray[historyRecordCursorIndex].boundaryMeshMaskTexture;
			HistoryRecord historyRecordForComposition = GetHistoryRecordForComposition();
			UpdateCameraFramePlaneObject(mainCamera, compositionCamera, historyRecordForComposition.boundaryMeshMaskTexture);
			OVRSandwichCompositionManager component = compositionCamera.gameObject.GetComponent<OVRSandwichCompositionManager>();
			component.fgTexture = historyRecordForComposition.fgRenderTexture;
			component.bgTexture = historyRecordForComposition.bgRenderTexture;
			cameraProxyPlane.transform.position = fgCamera.transform.position + fgCamera.transform.forward * cameraFramePlaneDistance;
			cameraProxyPlane.transform.LookAt(cameraProxyPlane.transform.position + fgCamera.transform.forward);
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		Camera[] array = new Camera[3] { fgCamera, bgCamera, compositionCamera };
		Camera[] array2 = array;
		foreach (Camera camera in array2)
		{
			OVRCompositionUtil.SafeDestroy(camera.gameObject);
		}
		fgCamera = null;
		bgCamera = null;
		compositionCamera = null;
		Debug.Log("SandwichComposition deactivated");
	}

	private RenderTextureFormat DesiredRenderTextureFormat(RenderTextureFormat originalFormat)
	{
		switch (originalFormat)
		{
		case RenderTextureFormat.RGB565:
			return RenderTextureFormat.ARGB1555;
		case RenderTextureFormat.RGB111110Float:
			return RenderTextureFormat.ARGBHalf;
		default:
			return originalFormat;
		}
	}

	protected void RefreshRenderTextures(Camera mainCamera)
	{
		int width = Screen.width;
		int height = Screen.height;
		RenderTextureFormat renderTextureFormat = (mainCamera.targetTexture ? DesiredRenderTextureFormat(mainCamera.targetTexture.format) : RenderTextureFormat.ARGB32);
		int num = ((!mainCamera.targetTexture) ? 24 : mainCamera.targetTexture.depth);
		HistoryRecord historyRecord = historyRecordArray[historyRecordCursorIndex];
		historyRecord.timestamp = frameRealtime;
		if (historyRecord.fgRenderTexture == null || historyRecord.fgRenderTexture.width != width || historyRecord.fgRenderTexture.height != height || historyRecord.fgRenderTexture.format != renderTextureFormat || historyRecord.fgRenderTexture.depth != num)
		{
			historyRecord.fgRenderTexture = new RenderTexture(width, height, num, renderTextureFormat);
			historyRecord.fgRenderTexture.name = "Sandwich FG " + historyRecordCursorIndex;
		}
		fgCamera.targetTexture = historyRecord.fgRenderTexture;
		if (historyRecord.bgRenderTexture == null || historyRecord.bgRenderTexture.width != width || historyRecord.bgRenderTexture.height != height || historyRecord.bgRenderTexture.format != renderTextureFormat || historyRecord.bgRenderTexture.depth != num)
		{
			historyRecord.bgRenderTexture = new RenderTexture(width, height, num, renderTextureFormat);
			historyRecord.bgRenderTexture.name = "Sandwich BG " + historyRecordCursorIndex;
		}
		bgCamera.targetTexture = historyRecord.bgRenderTexture;
		if (OVRManager.instance.virtualGreenScreenType != 0)
		{
			if (historyRecord.boundaryMeshMaskTexture == null || historyRecord.boundaryMeshMaskTexture.width != width || historyRecord.boundaryMeshMaskTexture.height != height)
			{
				historyRecord.boundaryMeshMaskTexture = new RenderTexture(width, height, 0, RenderTextureFormat.R8);
				historyRecord.boundaryMeshMaskTexture.name = "Boundary Mask " + historyRecordCursorIndex;
				historyRecord.boundaryMeshMaskTexture.Create();
			}
		}
		else
		{
			historyRecord.boundaryMeshMaskTexture = null;
		}
	}

	protected HistoryRecord GetHistoryRecordForComposition()
	{
		float num = frameRealtime - OVRManager.instance.sandwichCompositionRenderLatency;
		int num2 = historyRecordCursorIndex;
		int num3 = num2 - 1;
		if (num3 < 0)
		{
			num3 = historyRecordCount - 1;
		}
		while (num3 != historyRecordCursorIndex)
		{
			if (historyRecordArray[num3].timestamp <= num)
			{
				float num4 = historyRecordArray[num2].timestamp - num;
				float num5 = num - historyRecordArray[num3].timestamp;
				return (!(num4 <= num5)) ? historyRecordArray[num3] : historyRecordArray[num2];
			}
			num2 = num3--;
			if (num3 < 0)
			{
				num3 = historyRecordCount - 1;
			}
		}
		return historyRecordArray[num2];
	}

	protected void RefreshCameraPoses(float fovY, float aspect, OVRPose pose)
	{
		Camera[] array = new Camera[3] { fgCamera, bgCamera, compositionCamera };
		Camera[] array2 = array;
		foreach (Camera camera in array2)
		{
			camera.fieldOfView = fovY;
			camera.aspect = aspect;
			camera.transform.FromOVRPose(pose);
		}
	}
}
