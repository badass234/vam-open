using UnityEngine;

public class ReflectionCamera : MonoBehaviour
{
	public LayerMask CullingMask = -17;

	public bool HDR;

	[Range(0.1f, 1f)]
	public float TextureScale = 1f;

	private RenderTexture reflectionTexture;

	private GameObject goCam;

	private Camera reflectionCamera;

	private Vector3 oldPos;

	private static float ClipPlaneOffset = 0.07f;

	private void UpdateCamera(Camera cam)
	{
		CheckCamera(cam);
		if (!(cam == null))
		{
			GL.invertCulling = true;
			Transform transform = base.transform;
			Vector3 eulerAngles = cam.transform.eulerAngles;
			reflectionCamera.transform.eulerAngles = new Vector3(0f - eulerAngles.x, eulerAngles.y, eulerAngles.z);
			reflectionCamera.transform.position = cam.transform.position;
			Vector3 position = transform.transform.position;
			position.y = transform.position.y;
			Vector3 up = transform.transform.up;
			float w = 0f - Vector3.Dot(up, position) - ClipPlaneOffset;
			Vector4 plane = new Vector4(up.x, up.y, up.z, w);
			Matrix4x4 zero = Matrix4x4.zero;
			zero = CalculateReflectionMatrix(zero, plane);
			oldPos = cam.transform.position;
			Vector3 position2 = zero.MultiplyPoint(oldPos);
			reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * zero;
			Vector4 clipPlane = CameraSpacePlane(reflectionCamera, position, up, 1f);
			Matrix4x4 projectionMatrix = cam.projectionMatrix;
			projectionMatrix = CalculateObliqueMatrix(projectionMatrix, clipPlane);
			reflectionCamera.projectionMatrix = projectionMatrix;
			reflectionCamera.transform.position = position2;
			Vector3 eulerAngles2 = cam.transform.eulerAngles;
			reflectionCamera.transform.eulerAngles = new Vector3(0f - eulerAngles2.x, eulerAngles2.y, eulerAngles2.z);
			reflectionCamera.Render();
			GL.invertCulling = false;
		}
	}

	private void CheckCamera(Camera cam)
	{
		if (goCam == null)
		{
			reflectionTexture = new RenderTexture((int)((float)Screen.width * TextureScale), (int)((float)Screen.height * TextureScale), 16, RenderTextureFormat.Default);
			reflectionTexture.DiscardContents();
			goCam = new GameObject("Water Refl Camera");
			goCam.hideFlags = HideFlags.DontSave;
			goCam.transform.position = base.transform.position;
			goCam.transform.rotation = base.transform.rotation;
			reflectionCamera = goCam.AddComponent<Camera>();
			reflectionCamera.depth = cam.depth - 10f;
			reflectionCamera.renderingPath = cam.renderingPath;
			reflectionCamera.depthTextureMode = DepthTextureMode.None;
			reflectionCamera.cullingMask = CullingMask;
			reflectionCamera.allowHDR = HDR;
			reflectionCamera.useOcclusionCulling = false;
			reflectionCamera.enabled = false;
			reflectionCamera.targetTexture = reflectionTexture;
			Shader.SetGlobalTexture("_ReflectionTex", reflectionTexture);
		}
	}

	private static float Sgn(float a)
	{
		if (a > 0f)
		{
			return 1f;
		}
		if (a < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	private static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 b = projection.inverse * new Vector4(Sgn(clipPlane.x), Sgn(clipPlane.y), 1f, 1f);
		Vector4 vector = clipPlane * (2f / Vector4.Dot(clipPlane, b));
		projection[2] = vector.x - projection[3];
		projection[6] = vector.y - projection[7];
		projection[10] = vector.z - projection[11];
		projection[14] = vector.w - projection[15];
		return projection;
	}

	private static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMat.m01 = -2f * plane[0] * plane[1];
		reflectionMat.m02 = -2f * plane[0] * plane[2];
		reflectionMat.m03 = -2f * plane[3] * plane[0];
		reflectionMat.m10 = -2f * plane[1] * plane[0];
		reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMat.m12 = -2f * plane[1] * plane[2];
		reflectionMat.m13 = -2f * plane[3] * plane[1];
		reflectionMat.m20 = -2f * plane[2] * plane[0];
		reflectionMat.m21 = -2f * plane[2] * plane[1];
		reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMat.m23 = -2f * plane[3] * plane[2];
		reflectionMat.m30 = 0f;
		reflectionMat.m31 = 0f;
		reflectionMat.m32 = 0f;
		reflectionMat.m33 = 1f;
		return reflectionMat;
	}

	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 point = pos + normal * ClipPlaneOffset;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
		Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
	}

	private void SafeDestroy<T>(T component) where T : Object
	{
		if (!(component == null))
		{
			if (!Application.isPlaying)
			{
				Object.DestroyImmediate(component);
			}
			else
			{
				Object.Destroy(component);
			}
		}
	}

	private void ClearCamera()
	{
		if ((bool)goCam)
		{
			SafeDestroy(goCam);
			goCam = null;
		}
		if ((bool)reflectionTexture)
		{
			SafeDestroy(reflectionTexture);
			reflectionTexture = null;
		}
	}

	public void OnWillRenderObject()
	{
		UpdateCamera(Camera.main);
	}

	private void OnEnable()
	{
		Shader.EnableKeyword("cubeMap_off");
	}

	private void OnDisable()
	{
		ClearCamera();
		Shader.DisableKeyword("cubeMap_off");
	}
}
