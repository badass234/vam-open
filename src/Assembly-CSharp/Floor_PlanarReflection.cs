using System;
using UnityEngine;

[Serializable]
public class Floor_PlanarReflection : MonoBehaviour
{
	public int renderTextureSize;

	public float clipPlaneOffset;

	public bool disablePixelLights;

	private RenderTexture renderTexture;

	private int restorePixelLightCount;

	private Camera sourceCamera;

	public Floor_PlanarReflection()
	{
		renderTextureSize = 256;
		clipPlaneOffset = 0.01f;
		disablePixelLights = true;
	}

	public virtual void Start()
	{
		renderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 16);
		renderTexture.isPowerOfTwo = true;
		base.gameObject.AddComponent<Camera>();
		Camera component = GetComponent<Camera>();
		Camera main = Camera.main;
		component.targetTexture = renderTexture;
		component.clearFlags = main.clearFlags;
		component.backgroundColor = main.backgroundColor;
		component.nearClipPlane = main.nearClipPlane;
		component.farClipPlane = main.farClipPlane;
		component.fieldOfView = main.fieldOfView;
		GetComponent<Renderer>().material.SetTexture("_ReflectionTex", renderTexture);
	}

	public virtual void Update()
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));
		GetComponent<Renderer>().material.SetMatrix("_ProjMatrix", matrix4x * Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix * base.transform.localToWorldMatrix);
	}

	public virtual void OnDisable()
	{
		UnityEngine.Object.Destroy(renderTexture);
	}

	public virtual void LateUpdate()
	{
		sourceCamera = Camera.main;
		if (!sourceCamera)
		{
			Debug.Log("Reflection rendering requires that a Camera that is tagged \"MainCamera\"! Disabling reflection.");
			GetComponent<Camera>().enabled = false;
		}
		else
		{
			GetComponent<Camera>().enabled = true;
		}
	}

	public virtual void OnPreCull()
	{
		sourceCamera = Camera.main;
		if ((bool)sourceCamera)
		{
			Vector3 position = base.transform.position;
			Vector3 up = base.transform.up;
			float w = 0f - Vector3.Dot(up, position) - clipPlaneOffset;
			Vector4 plane = new Vector4(up.x, up.y, up.z, w);
			Matrix4x4 matrix4x = CalculateReflectionMatrix(plane);
			GetComponent<Camera>().worldToCameraMatrix = sourceCamera.worldToCameraMatrix * matrix4x;
			Vector4 clipPlane = CameraSpacePlane(position, up);
			GetComponent<Camera>().projectionMatrix = CalculateObliqueMatrix(sourceCamera.projectionMatrix, clipPlane);
		}
		else
		{
			GetComponent<Camera>().ResetWorldToCameraMatrix();
		}
	}

	public virtual void OnPreRender()
	{
		GL.invertCulling = true;
		if (disablePixelLights)
		{
			restorePixelLightCount = QualitySettings.pixelLightCount;
		}
	}

	public virtual void OnPostRender()
	{
		GL.invertCulling = false;
		if (disablePixelLights)
		{
			QualitySettings.pixelLightCount = restorePixelLightCount;
		}
	}

	public virtual Vector4 CameraSpacePlane(Vector3 pos, Vector3 normal)
	{
		Vector3 point = pos + normal * clipPlaneOffset;
		Matrix4x4 worldToCameraMatrix = GetComponent<Camera>().worldToCameraMatrix;
		Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
		Vector3 normalized = worldToCameraMatrix.MultiplyVector(normal).normalized;
		return new Vector4(normalized.x, normalized.y, normalized.z, 0f - Vector3.Dot(lhs, normalized));
	}

	public static float sgn(float a)
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

	public static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 b = default(Vector4);
		b.x = (sgn(clipPlane.x) + projection[8]) / projection[0];
		b.y = (sgn(clipPlane.y) + projection[9]) / projection[5];
		b.z = -1f;
		b.w = (1f + projection[10]) / projection[14];
		Vector4 vector = clipPlane * (2f / Vector4.Dot(clipPlane, b));
		projection[2] = vector.x;
		projection[6] = vector.y;
		projection[10] = vector.z + 1f;
		projection[14] = vector.w;
		return projection;
	}

	public static Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
	{
		Matrix4x4 result = default(Matrix4x4);
		result.m00 = 1f - 2f * plane[0] * plane[0];
		result.m01 = -2f * plane[0] * plane[1];
		result.m02 = -2f * plane[0] * plane[2];
		result.m03 = -2f * plane[3] * plane[0];
		result.m10 = -2f * plane[1] * plane[0];
		result.m11 = 1f - 2f * plane[1] * plane[1];
		result.m12 = -2f * plane[1] * plane[2];
		result.m13 = -2f * plane[3] * plane[1];
		result.m20 = -2f * plane[2] * plane[0];
		result.m21 = -2f * plane[2] * plane[1];
		result.m22 = 1f - 2f * plane[2] * plane[2];
		result.m23 = -2f * plane[3] * plane[2];
		result.m30 = 0f;
		result.m31 = 0f;
		result.m32 = 0f;
		result.m33 = 1f;
		return result;
	}
}
