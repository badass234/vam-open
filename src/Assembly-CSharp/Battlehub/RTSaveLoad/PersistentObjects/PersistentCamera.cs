using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentCamera : PersistentBehaviour
	{
		public float fieldOfView;

		public float nearClipPlane;

		public float farClipPlane;

		public uint renderingPath;

		public bool allowHDR;

		public bool forceIntoRenderTexture;

		public bool allowMSAA;

		public float orthographicSize;

		public bool orthographic;

		public uint opaqueSortMode;

		public uint transparencySortMode;

		public Vector3 transparencySortAxis;

		public float depth;

		public int cullingMask;

		public int eventMask;

		public Color backgroundColor;

		public Rect rect;

		public Rect pixelRect;

		public long targetTexture;

		public bool useJitteredProjectionMatrixForTransparentRendering;

		public uint clearFlags;

		public float stereoSeparation;

		public float stereoConvergence;

		public uint cameraType;

		public bool stereoMirrorMode;

		public uint stereoTargetEye;

		public int targetDisplay;

		public bool useOcclusionCulling;

		public float[] layerCullDistances;

		public bool layerCullSpherical;

		public uint depthTextureMode;

		public bool clearStencilAfterLightingPass;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Camera camera = (Camera)obj;
			camera.fieldOfView = fieldOfView;
			camera.nearClipPlane = nearClipPlane;
			camera.farClipPlane = farClipPlane;
			camera.renderingPath = (RenderingPath)renderingPath;
			camera.allowHDR = allowHDR;
			camera.forceIntoRenderTexture = forceIntoRenderTexture;
			camera.allowMSAA = allowMSAA;
			camera.orthographicSize = orthographicSize;
			camera.orthographic = orthographic;
			camera.opaqueSortMode = (OpaqueSortMode)opaqueSortMode;
			camera.transparencySortMode = (TransparencySortMode)transparencySortMode;
			camera.transparencySortAxis = transparencySortAxis;
			camera.depth = depth;
			camera.cullingMask = cullingMask;
			camera.eventMask = eventMask;
			camera.backgroundColor = backgroundColor;
			camera.rect = rect;
			camera.pixelRect = pixelRect;
			camera.targetTexture = (RenderTexture)objects.Get(targetTexture);
			camera.useJitteredProjectionMatrixForTransparentRendering = useJitteredProjectionMatrixForTransparentRendering;
			camera.clearFlags = (CameraClearFlags)clearFlags;
			camera.stereoSeparation = stereoSeparation;
			camera.stereoConvergence = stereoConvergence;
			camera.cameraType = (CameraType)cameraType;
			camera.stereoTargetEye = (StereoTargetEyeMask)stereoTargetEye;
			camera.targetDisplay = targetDisplay;
			camera.useOcclusionCulling = useOcclusionCulling;
			camera.layerCullDistances = layerCullDistances;
			camera.layerCullSpherical = layerCullSpherical;
			camera.depthTextureMode = (DepthTextureMode)depthTextureMode;
			camera.clearStencilAfterLightingPass = clearStencilAfterLightingPass;
			return camera;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Camera camera = (Camera)obj;
				fieldOfView = camera.fieldOfView;
				nearClipPlane = camera.nearClipPlane;
				farClipPlane = camera.farClipPlane;
				renderingPath = (uint)camera.renderingPath;
				allowHDR = camera.allowHDR;
				forceIntoRenderTexture = camera.forceIntoRenderTexture;
				allowMSAA = camera.allowMSAA;
				orthographicSize = camera.orthographicSize;
				orthographic = camera.orthographic;
				opaqueSortMode = (uint)camera.opaqueSortMode;
				transparencySortMode = (uint)camera.transparencySortMode;
				transparencySortAxis = camera.transparencySortAxis;
				depth = camera.depth;
				cullingMask = camera.cullingMask;
				eventMask = camera.eventMask;
				backgroundColor = camera.backgroundColor;
				rect = camera.rect;
				pixelRect = camera.pixelRect;
				targetTexture = camera.targetTexture.GetMappedInstanceID();
				useJitteredProjectionMatrixForTransparentRendering = camera.useJitteredProjectionMatrixForTransparentRendering;
				clearFlags = (uint)camera.clearFlags;
				stereoSeparation = camera.stereoSeparation;
				stereoConvergence = camera.stereoConvergence;
				cameraType = (uint)camera.cameraType;
				stereoTargetEye = (uint)camera.stereoTargetEye;
				targetDisplay = camera.targetDisplay;
				useOcclusionCulling = camera.useOcclusionCulling;
				layerCullDistances = camera.layerCullDistances;
				layerCullSpherical = camera.layerCullSpherical;
				depthTextureMode = (uint)camera.depthTextureMode;
				clearStencilAfterLightingPass = camera.clearStencilAfterLightingPass;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(targetTexture, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Camera camera = (Camera)obj;
				AddDependency(camera.targetTexture, dependencies);
			}
		}
	}
}
