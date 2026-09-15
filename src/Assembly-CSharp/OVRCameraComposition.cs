using System;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class OVRCameraComposition : OVRComposition
{
	public class OVRCameraFrameCompositionManager : MonoBehaviour
	{
		public GameObject cameraFrameGameObj;

		public OVRCameraComposition composition;

		public RenderTexture boundaryMeshMaskTexture;

		private Material cameraFrameMaterial;

		private Material whiteMaterial;

		private void Start()
		{
			Shader shader = Shader.Find("Oculus/Unlit");
			if (!shader)
			{
				Debug.LogError("Oculus/Unlit shader does not exist");
				return;
			}
			whiteMaterial = new Material(shader);
			whiteMaterial.color = Color.white;
		}

		private void OnPreRender()
		{
			if (OVRManager.instance.virtualGreenScreenType != 0 && boundaryMeshMaskTexture != null && composition.boundaryMesh != null)
			{
				RenderTexture active = RenderTexture.active;
				RenderTexture.active = boundaryMeshMaskTexture;
				GL.PushMatrix();
				GL.LoadProjectionMatrix(GetComponent<Camera>().projectionMatrix);
				GL.Clear(false, true, Color.black);
				for (int i = 0; i < whiteMaterial.passCount; i++)
				{
					if (whiteMaterial.SetPass(i))
					{
						Graphics.DrawMeshNow(composition.boundaryMesh, composition.cameraRig.ComputeTrackReferenceMatrix());
					}
				}
				GL.PopMatrix();
				RenderTexture.active = active;
			}
			if ((bool)cameraFrameGameObj)
			{
				if (cameraFrameMaterial == null)
				{
					cameraFrameMaterial = cameraFrameGameObj.GetComponent<MeshRenderer>().material;
				}
				cameraFrameMaterial.SetFloat("_Visible", 1f);
			}
		}

		private void OnPostRender()
		{
			if ((bool)cameraFrameGameObj)
			{
				cameraFrameMaterial.SetFloat("_Visible", 0f);
			}
		}
	}

	protected GameObject cameraFramePlaneObject;

	protected float cameraFramePlaneDistance;

	protected readonly bool hasCameraDeviceOpened;

	protected readonly bool useDynamicLighting;

	internal readonly OVRPlugin.CameraDevice cameraDevice = OVRPlugin.CameraDevice.WebCamera0;

	private OVRCameraRig cameraRig;

	private Mesh boundaryMesh;

	private float boundaryMeshTopY;

	private float boundaryMeshBottomY;

	private OVRManager.VirtualGreenScreenType boundaryMeshType;

	private bool nullcameraRigWarningDisplayed;

	protected OVRCameraComposition(OVRManager.CameraDevice inCameraDevice, bool inUseDynamicLighting, OVRManager.DepthQuality depthQuality)
	{
		cameraDevice = OVRCompositionUtil.ConvertCameraDevice(inCameraDevice);
		hasCameraDeviceOpened = false;
		useDynamicLighting = inUseDynamicLighting;
		bool flag = OVRPlugin.DoesCameraDeviceSupportDepth(cameraDevice);
		if (useDynamicLighting && !flag)
		{
			Debug.LogWarning("The camera device doesn't support depth. The result of dynamic lighting might not be correct");
		}
		if (!OVRPlugin.IsCameraDeviceAvailable(cameraDevice))
		{
			return;
		}
		OVRPlugin.CameraExtrinsics cameraExtrinsics;
		OVRPlugin.CameraIntrinsics cameraIntrinsics;
		if (OVRPlugin.GetExternalCameraCount() > 0 && OVRPlugin.GetMixedRealityCameraInfo(0, out cameraExtrinsics, out cameraIntrinsics))
		{
			OVRPlugin.SetCameraDevicePreferredColorFrameSize(cameraDevice, cameraIntrinsics.ImageSensorPixelResolution.w, cameraIntrinsics.ImageSensorPixelResolution.h);
		}
		if (useDynamicLighting)
		{
			OVRPlugin.SetCameraDeviceDepthSensingMode(cameraDevice, OVRPlugin.CameraDeviceDepthSensingMode.Fill);
			OVRPlugin.CameraDeviceDepthQuality depthQuality2 = OVRPlugin.CameraDeviceDepthQuality.Medium;
			switch (depthQuality)
			{
			case OVRManager.DepthQuality.Low:
				depthQuality2 = OVRPlugin.CameraDeviceDepthQuality.Low;
				break;
			case OVRManager.DepthQuality.Medium:
				depthQuality2 = OVRPlugin.CameraDeviceDepthQuality.Medium;
				break;
			case OVRManager.DepthQuality.High:
				depthQuality2 = OVRPlugin.CameraDeviceDepthQuality.High;
				break;
			default:
				Debug.LogWarning("Unknown depth quality");
				break;
			}
			OVRPlugin.SetCameraDevicePreferredDepthQuality(cameraDevice, depthQuality2);
		}
		OVRPlugin.OpenCameraDevice(cameraDevice);
		if (OVRPlugin.HasCameraDeviceOpened(cameraDevice))
		{
			hasCameraDeviceOpened = true;
		}
	}

	public override void Cleanup()
	{
		OVRCompositionUtil.SafeDestroy(ref cameraFramePlaneObject);
		if (hasCameraDeviceOpened)
		{
			OVRPlugin.CloseCameraDevice(cameraDevice);
		}
	}

	public override void RecenterPose()
	{
		boundaryMesh = null;
	}

	protected void CreateCameraFramePlaneObject(GameObject parentObject, Camera mixedRealityCamera, bool useDynamicLighting)
	{
		cameraFramePlaneObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
		cameraFramePlaneObject.name = "MRCameraFrame";
		cameraFramePlaneObject.transform.parent = parentObject.transform;
		cameraFramePlaneObject.GetComponent<Collider>().enabled = false;
		cameraFramePlaneObject.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
		Material material = new Material(Shader.Find((!useDynamicLighting) ? "Oculus/OVRMRCameraFrame" : "Oculus/OVRMRCameraFrameLit"));
		cameraFramePlaneObject.GetComponent<MeshRenderer>().material = material;
		material.SetColor("_Color", Color.white);
		material.SetFloat("_Visible", 0f);
		cameraFramePlaneObject.transform.localScale = new Vector3(4f, 4f, 4f);
		cameraFramePlaneObject.SetActive(true);
		OVRCameraFrameCompositionManager oVRCameraFrameCompositionManager = mixedRealityCamera.gameObject.AddComponent<OVRCameraFrameCompositionManager>();
		oVRCameraFrameCompositionManager.cameraFrameGameObj = cameraFramePlaneObject;
		oVRCameraFrameCompositionManager.composition = this;
	}

	protected void UpdateCameraFramePlaneObject(Camera mainCamera, Camera mixedRealityCamera, RenderTexture boundaryMeshMaskTexture)
	{
		bool flag = false;
		Material material = cameraFramePlaneObject.GetComponent<MeshRenderer>().material;
		Texture2D texture2D = Texture2D.blackTexture;
		Texture2D value = Texture2D.whiteTexture;
		if (OVRPlugin.IsCameraDeviceColorFrameAvailable(cameraDevice))
		{
			texture2D = OVRPlugin.GetCameraDeviceColorFrameTexture(cameraDevice);
		}
		else
		{
			Debug.LogWarning("Camera: color frame not ready");
			flag = true;
		}
		bool flag2 = OVRPlugin.DoesCameraDeviceSupportDepth(cameraDevice);
		if (useDynamicLighting && flag2)
		{
			if (OVRPlugin.IsCameraDeviceDepthFrameAvailable(cameraDevice))
			{
				value = OVRPlugin.GetCameraDeviceDepthFrameTexture(cameraDevice);
			}
			else
			{
				Debug.LogWarning("Camera: depth frame not ready");
				flag = true;
			}
		}
		if (flag)
		{
			return;
		}
		Vector3 rhs = mainCamera.transform.position - mixedRealityCamera.transform.position;
		float num = (cameraFramePlaneDistance = Vector3.Dot(mixedRealityCamera.transform.forward, rhs));
		cameraFramePlaneObject.transform.position = mixedRealityCamera.transform.position + mixedRealityCamera.transform.forward * num;
		cameraFramePlaneObject.transform.rotation = mixedRealityCamera.transform.rotation;
		float num2 = Mathf.Tan(mixedRealityCamera.fieldOfView * ((float)Math.PI / 180f) * 0.5f);
		cameraFramePlaneObject.transform.localScale = new Vector3(num * mixedRealityCamera.aspect * num2 * 2f, num * num2 * 2f, 1f);
		float num3 = num * num2 * 2f;
		float x = num3 * mixedRealityCamera.aspect;
		float cullingDistance = float.MaxValue;
		cameraRig = null;
		if (OVRManager.instance.virtualGreenScreenType != 0)
		{
			cameraRig = mainCamera.GetComponentInParent<OVRCameraRig>();
			if (cameraRig != null && cameraRig.centerEyeAnchor == null)
			{
				cameraRig = null;
			}
			RefreshBoundaryMesh(mixedRealityCamera, out cullingDistance);
		}
		material.mainTexture = texture2D;
		material.SetTexture("_DepthTex", value);
		material.SetVector("_FlipParams", new Vector4((!OVRManager.instance.flipCameraFrameHorizontally) ? 0f : 1f, (!OVRManager.instance.flipCameraFrameVertically) ? 0f : 1f, 0f, 0f));
		material.SetColor("_ChromaKeyColor", OVRManager.instance.chromaKeyColor);
		material.SetFloat("_ChromaKeySimilarity", OVRManager.instance.chromaKeySimilarity);
		material.SetFloat("_ChromaKeySmoothRange", OVRManager.instance.chromaKeySmoothRange);
		material.SetFloat("_ChromaKeySpillRange", OVRManager.instance.chromaKeySpillRange);
		material.SetVector("_TextureDimension", new Vector4(texture2D.width, texture2D.height, 1f / (float)texture2D.width, 1f / (float)texture2D.height));
		material.SetVector("_TextureWorldSize", new Vector4(x, num3, 0f, 0f));
		material.SetFloat("_SmoothFactor", OVRManager.instance.dynamicLightingSmoothFactor);
		material.SetFloat("_DepthVariationClamp", OVRManager.instance.dynamicLightingDepthVariationClampingValue);
		material.SetFloat("_CullingDistance", cullingDistance);
		if (OVRManager.instance.virtualGreenScreenType == OVRManager.VirtualGreenScreenType.Off || boundaryMesh == null || boundaryMeshMaskTexture == null)
		{
			material.SetTexture("_MaskTex", Texture2D.whiteTexture);
		}
		else if (cameraRig == null)
		{
			if (!nullcameraRigWarningDisplayed)
			{
				Debug.LogWarning("Could not find the OVRCameraRig/CenterEyeAnchor object. Please check if the OVRCameraRig has been setup properly. The virtual green screen has been temporarily disabled");
				nullcameraRigWarningDisplayed = true;
			}
			material.SetTexture("_MaskTex", Texture2D.whiteTexture);
		}
		else
		{
			if (nullcameraRigWarningDisplayed)
			{
				Debug.Log("OVRCameraRig/CenterEyeAnchor object found. Virtual green screen is activated");
				nullcameraRigWarningDisplayed = false;
			}
			material.SetTexture("_MaskTex", boundaryMeshMaskTexture);
		}
	}

	protected void RefreshBoundaryMesh(Camera camera, out float cullingDistance)
	{
		float num = ((!OVRManager.instance.virtualGreenScreenApplyDepthCulling) ? float.PositiveInfinity : OVRManager.instance.virtualGreenScreenDepthTolerance);
		cullingDistance = OVRCompositionUtil.GetMaximumBoundaryDistance(camera, OVRCompositionUtil.ToBoundaryType(OVRManager.instance.virtualGreenScreenType)) + num;
		if (boundaryMesh == null || boundaryMeshType != OVRManager.instance.virtualGreenScreenType || boundaryMeshTopY != OVRManager.instance.virtualGreenScreenTopY || boundaryMeshBottomY != OVRManager.instance.virtualGreenScreenBottomY)
		{
			boundaryMeshTopY = OVRManager.instance.virtualGreenScreenTopY;
			boundaryMeshBottomY = OVRManager.instance.virtualGreenScreenBottomY;
			boundaryMesh = OVRCompositionUtil.BuildBoundaryMesh(OVRCompositionUtil.ToBoundaryType(OVRManager.instance.virtualGreenScreenType), boundaryMeshTopY, boundaryMeshBottomY);
			boundaryMeshType = OVRManager.instance.virtualGreenScreenType;
		}
	}
}
