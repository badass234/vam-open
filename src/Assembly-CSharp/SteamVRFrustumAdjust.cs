using UnityEngine;
using Valve.VR;

[AddComponentMenu("SteamVR/SteamVRFrustumAdjust")]
public class SteamVRFrustumAdjust : MonoBehaviour
{
	private bool isCantedFov;

	private Camera m_Camera;

	private Matrix4x4 projectionMatrix;

	private void OnEnable()
	{
		m_Camera = GetComponent<Camera>();
		HmdMatrix34_t eyeToHeadTransform = SteamVR.instance.hmd.GetEyeToHeadTransform(EVREye.Eye_Left);
		if (eyeToHeadTransform.m0 < 1f)
		{
			isCantedFov = true;
			float pfLeft = 0f;
			float pfRight = 0f;
			float pfTop = 0f;
			float pfBottom = 0f;
			SteamVR.instance.hmd.GetProjectionRaw(EVREye.Eye_Left, ref pfLeft, ref pfRight, ref pfTop, ref pfBottom);
			float num = Mathf.Acos(eyeToHeadTransform.m0);
			float num2 = Mathf.Atan(SteamVR.instance.tanHalfFov.x);
			float num3 = Mathf.Tan(num + num2);
			projectionMatrix.m00 = 1f / num3;
			float num4 = Mathf.Atan(0f - pfLeft);
			float num5 = SteamVR.instance.tanHalfFov.y * Mathf.Cos(num4) / Mathf.Cos(num4 + num);
			projectionMatrix.m11 = 1f / num5;
			projectionMatrix.m22 = (0f - (m_Camera.farClipPlane + m_Camera.nearClipPlane)) / (m_Camera.farClipPlane - m_Camera.nearClipPlane);
			projectionMatrix.m23 = -2f * m_Camera.farClipPlane * m_Camera.nearClipPlane / (m_Camera.farClipPlane - m_Camera.nearClipPlane);
			projectionMatrix.m32 = -1f;
		}
		else
		{
			isCantedFov = false;
		}
	}

	private void OnDisable()
	{
		if (isCantedFov)
		{
			isCantedFov = false;
			m_Camera.ResetCullingMatrix();
		}
	}

	private void OnPreCull()
	{
		if (isCantedFov)
		{
			m_Camera.cullingMatrix = projectionMatrix * m_Camera.worldToCameraMatrix;
		}
	}
}
