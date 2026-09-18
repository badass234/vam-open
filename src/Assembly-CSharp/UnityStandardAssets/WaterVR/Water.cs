using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace UnityStandardAssets.WaterVR
{
	[ExecuteInEditMode]
	public class Water : MonoBehaviour
	{
		public enum WaterMode
		{
			Simple,
			Reflective,
			Refractive
		}

		public WaterMode waterMode = WaterMode.Refractive;

		public bool disablePixelLights = true;

		public int textureSize = 256;

		public float clipPlaneOffset = 0.07f;

		public LayerMask reflectLayers = -1;

		public LayerMask refractLayers = -1;

		private Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>();

		private Dictionary<Camera, Camera> m_RefractionCameras = new Dictionary<Camera, Camera>();

		private RenderTexture m_ReflectionTexture0;

		private RenderTexture m_ReflectionTexture1;

		private RenderTexture m_RefractionTexture;

		private WaterMode m_HardwareWaterSupport = WaterMode.Refractive;

		private int m_OldReflectionTextureSize;

		private int m_OldRefractionTextureSize;

		private static bool s_InsideWater;

		public void OnWillRenderObject()
		{
			if (!base.enabled || !GetComponent<Renderer>() || !GetComponent<Renderer>().sharedMaterial || !GetComponent<Renderer>().enabled)
			{
				return;
			}
			Camera current = Camera.current;
			if (!current || s_InsideWater)
			{
				return;
			}
			s_InsideWater = true;
			m_HardwareWaterSupport = FindHardwareWaterSupport();
			WaterMode waterMode = GetWaterMode();
			Camera reflectionCamera;
			Camera refractionCamera;
			CreateWaterObjects(current, out reflectionCamera, out refractionCamera);
			Vector3 position = base.transform.position;
			Vector3 up = base.transform.up;
			int pixelLightCount = QualitySettings.pixelLightCount;
			if (disablePixelLights)
			{
				QualitySettings.pixelLightCount = 0;
			}
			UpdateCameraModes(current, reflectionCamera);
			UpdateCameraModes(current, refractionCamera);
			if (waterMode >= WaterMode.Reflective)
			{
				if (current.stereoEnabled)
				{
					if (current.stereoTargetEye == StereoTargetEyeMask.Both || current.stereoTargetEye == StereoTargetEyeMask.Left)
					{
						Vector3 camPos = current.transform.TransformPoint(new Vector3(-0.5f * current.stereoSeparation, 0f, 0f));
						Matrix4x4 stereoProjectionMatrix = current.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
						RenderReflection(reflectionCamera, m_ReflectionTexture0, camPos, current.transform.rotation, stereoProjectionMatrix);
						GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex0", m_ReflectionTexture0);
					}
					if (current.stereoTargetEye == StereoTargetEyeMask.Both || current.stereoTargetEye == StereoTargetEyeMask.Right)
					{
						Vector3 camPos2 = current.transform.TransformPoint(new Vector3(0.5f * current.stereoSeparation, 0f, 0f));
						Matrix4x4 stereoProjectionMatrix2 = current.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
						RenderReflection(reflectionCamera, m_ReflectionTexture1, camPos2, current.transform.rotation, stereoProjectionMatrix2);
						GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex1", m_ReflectionTexture1);
					}
				}
				else
				{
					RenderReflection(reflectionCamera, m_ReflectionTexture0, current.transform.position, current.transform.rotation, current.projectionMatrix);
					GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex0", m_ReflectionTexture0);
				}
			}
			if (waterMode >= WaterMode.Refractive)
			{
				refractionCamera.worldToCameraMatrix = current.worldToCameraMatrix;
				Vector4 clipPlane = CameraSpacePlane(refractionCamera, position, up, -1f);
				refractionCamera.projectionMatrix = current.CalculateObliqueMatrix(clipPlane);
				refractionCamera.cullingMatrix = current.projectionMatrix * current.worldToCameraMatrix;
				refractionCamera.cullingMask = -17 & refractLayers.value;
				refractionCamera.targetTexture = m_RefractionTexture;
				refractionCamera.transform.position = current.transform.position;
				refractionCamera.transform.rotation = current.transform.rotation;
				refractionCamera.Render();
				GetComponent<Renderer>().sharedMaterial.SetTexture("_RefractionTex", m_RefractionTexture);
			}
			if (disablePixelLights)
			{
				QualitySettings.pixelLightCount = pixelLightCount;
			}
			switch (waterMode)
			{
			case WaterMode.Simple:
				Shader.EnableKeyword("WATER_SIMPLE");
				Shader.DisableKeyword("WATER_REFLECTIVE");
				Shader.DisableKeyword("WATER_REFRACTIVE");
				break;
			case WaterMode.Reflective:
				Shader.DisableKeyword("WATER_SIMPLE");
				Shader.EnableKeyword("WATER_REFLECTIVE");
				Shader.DisableKeyword("WATER_REFRACTIVE");
				break;
			case WaterMode.Refractive:
				Shader.DisableKeyword("WATER_SIMPLE");
				Shader.DisableKeyword("WATER_REFLECTIVE");
				Shader.EnableKeyword("WATER_REFRACTIVE");
				break;
			}
			s_InsideWater = false;
		}

		private void RenderReflection(Camera reflectionCamera, RenderTexture targetTexture, Vector3 camPos, Quaternion camRot, Matrix4x4 camProjMatrix)
		{
			reflectionCamera.ResetWorldToCameraMatrix();
			reflectionCamera.transform.position = camPos;
			reflectionCamera.transform.rotation = camRot;
			reflectionCamera.projectionMatrix = camProjMatrix;
			reflectionCamera.targetTexture = targetTexture;
			reflectionCamera.rect = new Rect(0f, 0f, 1f, 1f);
			Vector3 position = base.transform.position;
			Vector3 up = base.transform.up;
			float w = 0f - Vector3.Dot(up, position) - clipPlaneOffset;
			Vector4 plane = new Vector4(up.x, up.y, up.z, w);
			Matrix4x4 reflectionMat = Matrix4x4.zero;
			CalculateReflectionMatrix(ref reflectionMat, plane);
			reflectionCamera.worldToCameraMatrix *= reflectionMat;
			Vector4 clipPlane = CameraSpacePlane(reflectionCamera, position, up, 1f);
			reflectionCamera.projectionMatrix = reflectionCamera.CalculateObliqueMatrix(clipPlane);
			reflectionCamera.transform.position = reflectionCamera.cameraToWorldMatrix.GetColumn(3);
			reflectionCamera.transform.rotation = Quaternion.LookRotation(reflectionCamera.cameraToWorldMatrix.GetColumn(2), reflectionCamera.cameraToWorldMatrix.GetColumn(1));
			reflectionCamera.cullingMask = -17 & reflectLayers.value;
			bool invertCulling = GL.invertCulling;
			GL.invertCulling = !invertCulling;
			reflectionCamera.Render();
			GL.invertCulling = invertCulling;
		}

		private void OnDisable()
		{
			if ((bool)m_ReflectionTexture0)
			{
				UnityEngine.Object.DestroyImmediate(m_ReflectionTexture0);
				m_ReflectionTexture0 = null;
			}
			if ((bool)m_ReflectionTexture1)
			{
				UnityEngine.Object.DestroyImmediate(m_ReflectionTexture1);
				m_ReflectionTexture1 = null;
			}
			if ((bool)m_RefractionTexture)
			{
				UnityEngine.Object.DestroyImmediate(m_RefractionTexture);
				m_RefractionTexture = null;
			}
			foreach (KeyValuePair<Camera, Camera> reflectionCamera in m_ReflectionCameras)
			{
				UnityEngine.Object.DestroyImmediate(reflectionCamera.Value.gameObject);
			}
			m_ReflectionCameras.Clear();
			foreach (KeyValuePair<Camera, Camera> refractionCamera in m_RefractionCameras)
			{
				UnityEngine.Object.DestroyImmediate(refractionCamera.Value.gameObject);
			}
			m_RefractionCameras.Clear();
		}

		private void Update()
		{
			if ((bool)GetComponent<Renderer>())
			{
				Material sharedMaterial = GetComponent<Renderer>().sharedMaterial;
				if ((bool)sharedMaterial)
				{
					Vector4 vector = sharedMaterial.GetVector("WaveSpeed");
					float @float = sharedMaterial.GetFloat("_WaveScale");
					Vector4 value = new Vector4(@float, @float, @float * 0.4f, @float * 0.45f);
					double num = (double)Time.timeSinceLevelLoad / 20.0;
					Vector4 value2 = new Vector4((float)Math.IEEERemainder((double)(vector.x * value.x) * num, 1.0), (float)Math.IEEERemainder((double)(vector.y * value.y) * num, 1.0), (float)Math.IEEERemainder((double)(vector.z * value.z) * num, 1.0), (float)Math.IEEERemainder((double)(vector.w * value.w) * num, 1.0));
					sharedMaterial.SetVector("_WaveOffset", value2);
					sharedMaterial.SetVector("_WaveScale4", value);
				}
			}
		}

		private void UpdateCameraModes(Camera src, Camera dest)
		{
			if (dest == null)
			{
				return;
			}
			dest.clearFlags = src.clearFlags;
			dest.backgroundColor = src.backgroundColor;
			if (src.clearFlags == CameraClearFlags.Skybox)
			{
				Skybox component = src.GetComponent<Skybox>();
				Skybox component2 = dest.GetComponent<Skybox>();
				if (!component || !component.material)
				{
					component2.enabled = false;
				}
				else
				{
					component2.enabled = true;
					component2.material = component.material;
				}
			}
			dest.farClipPlane = src.farClipPlane;
			dest.nearClipPlane = src.nearClipPlane;
			dest.orthographic = src.orthographic;
			// XRDevice.isPresent is an obsolete-error from 2020.1; XRSettings.isDeviceActive is the surviving equivalent.
			if (!XRSettings.isDeviceActive)
			{
				dest.fieldOfView = src.fieldOfView;
			}
			dest.aspect = src.aspect;
			dest.orthographicSize = src.orthographicSize;
		}

		private void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera, out Camera refractionCamera)
		{
			WaterMode waterMode = GetWaterMode();
			reflectionCamera = null;
			refractionCamera = null;
			if (waterMode >= WaterMode.Reflective)
			{
				if (!m_ReflectionTexture0 || m_OldReflectionTextureSize != textureSize)
				{
					if ((bool)m_ReflectionTexture0)
					{
						UnityEngine.Object.DestroyImmediate(m_ReflectionTexture0);
					}
					m_ReflectionTexture0 = new RenderTexture(textureSize, textureSize, 16);
					m_ReflectionTexture0.name = "__WaterReflection" + GetInstanceID();
					m_ReflectionTexture0.isPowerOfTwo = true;
					m_ReflectionTexture0.hideFlags = HideFlags.DontSave;
				}
				if (currentCamera.stereoEnabled && (!m_ReflectionTexture1 || m_OldReflectionTextureSize != textureSize))
				{
					if ((bool)m_ReflectionTexture1)
					{
						UnityEngine.Object.DestroyImmediate(m_ReflectionTexture1);
					}
					m_ReflectionTexture1 = new RenderTexture(textureSize, textureSize, 16);
					m_ReflectionTexture1.isPowerOfTwo = true;
					m_ReflectionTexture1.hideFlags = HideFlags.DontSave;
				}
				m_OldReflectionTextureSize = textureSize;
				m_ReflectionCameras.TryGetValue(currentCamera, out reflectionCamera);
				if (!reflectionCamera)
				{
					GameObject gameObject = new GameObject("Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
					reflectionCamera = gameObject.GetComponent<Camera>();
					reflectionCamera.enabled = false;
					reflectionCamera.transform.position = base.transform.position;
					reflectionCamera.transform.rotation = base.transform.rotation;
					reflectionCamera.gameObject.AddComponent<FlareLayer>();
					gameObject.hideFlags = HideFlags.HideAndDontSave;
					m_ReflectionCameras[currentCamera] = reflectionCamera;
				}
			}
			if (waterMode < WaterMode.Refractive)
			{
				return;
			}
			if (!m_RefractionTexture || m_OldRefractionTextureSize != textureSize)
			{
				if ((bool)m_RefractionTexture)
				{
					UnityEngine.Object.DestroyImmediate(m_RefractionTexture);
				}
				m_RefractionTexture = new RenderTexture(textureSize, textureSize, 16);
				m_RefractionTexture.name = "__WaterRefraction" + GetInstanceID();
				m_RefractionTexture.isPowerOfTwo = true;
				m_RefractionTexture.hideFlags = HideFlags.DontSave;
				m_OldRefractionTextureSize = textureSize;
			}
			m_RefractionCameras.TryGetValue(currentCamera, out refractionCamera);
			if (!refractionCamera)
			{
				GameObject gameObject2 = new GameObject("Water Refr Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
				refractionCamera = gameObject2.GetComponent<Camera>();
				refractionCamera.enabled = false;
				refractionCamera.transform.position = base.transform.position;
				refractionCamera.transform.rotation = base.transform.rotation;
				refractionCamera.gameObject.AddComponent<FlareLayer>();
				gameObject2.hideFlags = HideFlags.HideAndDontSave;
				m_RefractionCameras[currentCamera] = refractionCamera;
			}
		}

		private WaterMode GetWaterMode()
		{
			if (m_HardwareWaterSupport < waterMode)
			{
				return m_HardwareWaterSupport;
			}
			return waterMode;
		}

		private WaterMode FindHardwareWaterSupport()
		{
			if (!GetComponent<Renderer>())
			{
				return WaterMode.Simple;
			}
			Material sharedMaterial = GetComponent<Renderer>().sharedMaterial;
			if (!sharedMaterial)
			{
				return WaterMode.Simple;
			}
			string text = sharedMaterial.GetTag("WATERMODE", false);
			if (text == "Refractive")
			{
				return WaterMode.Refractive;
			}
			if (text == "Reflective")
			{
				return WaterMode.Reflective;
			}
			return WaterMode.Simple;
		}

		private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
		{
			Vector3 point = pos + normal * clipPlaneOffset;
			Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
			Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
			Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
			return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
		}

		private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
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
		}
	}
}
