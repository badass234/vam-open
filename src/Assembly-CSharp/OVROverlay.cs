using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;

public class OVROverlay : MonoBehaviour
{
	public enum OverlayShape
	{
		Quad = 0,
		Cylinder = 1,
		Cubemap = 2,
		OffcenterCubemap = 4,
		Equirect = 5
	}

	public enum OverlayType
	{
		None,
		Underlay,
		Overlay
	}

	private struct LayerTexture
	{
		public Texture appTexture;

		public IntPtr appTexturePtr;

		public Texture[] swapChain;

		public IntPtr[] swapChainPtr;
	}

	public OverlayType currentOverlayType = OverlayType.Overlay;

	public bool isDynamic;

	public bool isProtectedContent;

	public OverlayShape currentOverlayShape;

	private OverlayShape prevOverlayShape;

	public Texture[] textures = new Texture[2];

	protected IntPtr[] texturePtrs = new IntPtr[2]
	{
		IntPtr.Zero,
		IntPtr.Zero
	};

	protected bool isOverridePending;

	internal const int maxInstances = 15;

	internal static OVROverlay[] instances = new OVROverlay[15];

	private static Material tex2DMaterial;

	private static Material cubeMaterial;

	private LayerTexture[] layerTextures;

	private OVRPlugin.LayerDesc layerDesc;

	private int stageCount = -1;

	private int layerIndex = -1;

	private int layerId;

	private GCHandle layerIdHandle;

	private IntPtr layerIdPtr = IntPtr.Zero;

	private int frameIndex;

	private int prevFrameIndex = -1;

	private Renderer rend;

	private OVRPlugin.LayerLayout layout => OVRPlugin.LayerLayout.Mono;

	private int texturesPerStage => (layout != 0) ? 1 : 2;

	public void OverrideOverlayTextureInfo(Texture srcTexture, IntPtr nativePtr, XRNode node)
	{
		int num = ((node == XRNode.RightEye) ? 1 : 0);
		if (textures.Length > num)
		{
			textures[num] = srcTexture;
			texturePtrs[num] = nativePtr;
			isOverridePending = true;
		}
	}

	private bool CreateLayer(int mipLevels, int sampleCount, OVRPlugin.EyeTextureFormat etFormat, int flags, OVRPlugin.Sizei size, OVRPlugin.OverlayShape shape)
	{
		if (!layerIdHandle.IsAllocated || layerIdPtr == IntPtr.Zero)
		{
			layerIdHandle = GCHandle.Alloc(layerId, GCHandleType.Pinned);
			layerIdPtr = layerIdHandle.AddrOfPinnedObject();
		}
		if (layerIndex == -1)
		{
			for (int i = 0; i < 15; i++)
			{
				if (instances[i] == null || instances[i] == this)
				{
					layerIndex = i;
					instances[i] = this;
					break;
				}
			}
		}
		if (!isOverridePending && layerDesc.MipLevels == mipLevels && layerDesc.SampleCount == sampleCount && layerDesc.Format == etFormat && layerDesc.Layout == layout && layerDesc.LayerFlags == flags && layerDesc.TextureSize.Equals(size) && layerDesc.Shape == shape)
		{
			return false;
		}
		OVRPlugin.LayerDesc desc = OVRPlugin.CalculateLayerDesc(shape, layout, size, mipLevels, sampleCount, etFormat, flags);
		OVRPlugin.EnqueueSetupLayer(desc, layerIdPtr);
		layerId = (int)layerIdHandle.Target;
		if (layerId > 0)
		{
			layerDesc = desc;
			stageCount = OVRPlugin.GetLayerTextureStageCount(layerId);
		}
		isOverridePending = false;
		return true;
	}

	private bool CreateLayerTextures(bool useMipmaps, OVRPlugin.Sizei size, bool isHdr)
	{
		bool result = false;
		if (stageCount <= 0)
		{
			return false;
		}
		if (layerTextures == null)
		{
			frameIndex = 0;
			layerTextures = new LayerTexture[texturesPerStage];
		}
		for (int i = 0; i < texturesPerStage; i++)
		{
			if (layerTextures[i].swapChain == null)
			{
				layerTextures[i].swapChain = new Texture[stageCount];
			}
			if (layerTextures[i].swapChainPtr == null)
			{
				layerTextures[i].swapChainPtr = new IntPtr[stageCount];
			}
			for (int j = 0; j < stageCount; j++)
			{
				Texture texture = layerTextures[i].swapChain[j];
				IntPtr intPtr = layerTextures[i].swapChainPtr[j];
				if (!(texture != null) || !(intPtr != IntPtr.Zero))
				{
					if (intPtr == IntPtr.Zero)
					{
						intPtr = OVRPlugin.GetLayerTexture(layerId, j, (OVRPlugin.Eye)i);
					}
					if (!(intPtr == IntPtr.Zero))
					{
						TextureFormat format = ((!isHdr) ? TextureFormat.RGBA32 : TextureFormat.RGBAHalf);
						texture = ((currentOverlayShape == OverlayShape.Cubemap || currentOverlayShape == OverlayShape.OffcenterCubemap) ? ((Texture)Cubemap.CreateExternalTexture(size.w, format, useMipmaps, intPtr)) : ((Texture)Texture2D.CreateExternalTexture(size.w, size.h, format, useMipmaps, true, intPtr)));
						layerTextures[i].swapChain[j] = texture;
						layerTextures[i].swapChainPtr[j] = intPtr;
						result = true;
					}
				}
			}
		}
		return result;
	}

	private void DestroyLayerTextures()
	{
		int num = 0;
		while (layerTextures != null && num < texturesPerStage)
		{
			if (layerTextures[num].swapChain != null)
			{
				for (int i = 0; i < stageCount; i++)
				{
					UnityEngine.Object.DestroyImmediate(layerTextures[num].swapChain[i]);
				}
			}
			num++;
		}
		layerTextures = null;
	}

	private void DestroyLayer()
	{
		if (layerIndex != -1)
		{
			OVRPlugin.EnqueueSubmitLayer(true, false, IntPtr.Zero, IntPtr.Zero, -1, 0, OVRPose.identity.ToPosef(), Vector3.one.ToVector3f(), layerIndex, (OVRPlugin.OverlayShape)prevOverlayShape);
			instances[layerIndex] = null;
			layerIndex = -1;
		}
		if (layerIdPtr != IntPtr.Zero)
		{
			OVRPlugin.EnqueueDestroyLayer(layerIdPtr);
			layerIdPtr = IntPtr.Zero;
			layerIdHandle.Free();
			layerId = 0;
		}
		layerDesc = default(OVRPlugin.LayerDesc);
	}

	private bool LatchLayerTextures()
	{
		for (int i = 0; i < texturesPerStage; i++)
		{
			if ((textures[i] != layerTextures[i].appTexture || layerTextures[i].appTexturePtr == IntPtr.Zero) && textures[i] != null)
			{
				RenderTexture renderTexture = textures[i] as RenderTexture;
				if ((bool)renderTexture && !renderTexture.IsCreated())
				{
					renderTexture.Create();
				}
				layerTextures[i].appTexturePtr = ((!(texturePtrs[i] != IntPtr.Zero)) ? textures[i].GetNativeTexturePtr() : texturePtrs[i]);
				if (layerTextures[i].appTexturePtr != IntPtr.Zero)
				{
					layerTextures[i].appTexture = textures[i];
				}
			}
			if (currentOverlayShape == OverlayShape.Cubemap && textures[i] as Cubemap == null)
			{
				Debug.LogError("Need Cubemap texture for cube map overlay");
				return false;
			}
		}
		if (currentOverlayShape == OverlayShape.OffcenterCubemap)
		{
			Debug.LogWarning(string.Concat("Overlay shape ", currentOverlayShape, " is not supported on current platform"));
			return false;
		}
		if (layerTextures[0].appTexture == null || layerTextures[0].appTexturePtr == IntPtr.Zero)
		{
			return false;
		}
		return true;
	}

	private OVRPlugin.LayerDesc GetCurrentLayerDesc()
	{
		OVRPlugin.LayerDesc layerDesc = default(OVRPlugin.LayerDesc);
		layerDesc.Format = OVRPlugin.EyeTextureFormat.Default;
		layerDesc.LayerFlags = 8;
		layerDesc.Layout = layout;
		layerDesc.MipLevels = 1;
		layerDesc.SampleCount = 1;
		layerDesc.Shape = (OVRPlugin.OverlayShape)currentOverlayShape;
		layerDesc.TextureSize = new OVRPlugin.Sizei
		{
			w = textures[0].width,
			h = textures[0].height
		};
		OVRPlugin.LayerDesc result = layerDesc;
		Texture2D texture2D = textures[0] as Texture2D;
		if (texture2D != null)
		{
			if (texture2D.format == TextureFormat.RGBAHalf || texture2D.format == TextureFormat.RGBAFloat)
			{
				result.Format = OVRPlugin.EyeTextureFormat.R16G16B16A16_FP;
			}
			result.MipLevels = texture2D.mipmapCount;
		}
		Cubemap cubemap = textures[0] as Cubemap;
		if (cubemap != null)
		{
			if (cubemap.format == TextureFormat.RGBAHalf || cubemap.format == TextureFormat.RGBAFloat)
			{
				result.Format = OVRPlugin.EyeTextureFormat.R16G16B16A16_FP;
			}
			result.MipLevels = cubemap.mipmapCount;
		}
		RenderTexture renderTexture = textures[0] as RenderTexture;
		if (renderTexture != null)
		{
			result.SampleCount = renderTexture.antiAliasing;
			if (renderTexture.format == RenderTextureFormat.ARGBHalf || renderTexture.format == RenderTextureFormat.ARGBFloat || renderTexture.format == RenderTextureFormat.RGB111110Float)
			{
				result.Format = OVRPlugin.EyeTextureFormat.R16G16B16A16_FP;
			}
		}
		if (isProtectedContent)
		{
			result.LayerFlags |= 64;
		}
		return result;
	}

	private bool PopulateLayer(int mipLevels, bool isHdr, OVRPlugin.Sizei size, int sampleCount)
	{
		bool result = false;
		RenderTextureFormat colorFormat = (isHdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
		for (int i = 0; i < texturesPerStage; i++)
		{
			int num = frameIndex % stageCount;
			Texture texture = layerTextures[i].swapChain[num];
			if (texture == null)
			{
				continue;
			}
			for (int j = 0; j < mipLevels; j++)
			{
				int num2 = size.w >> j;
				if (num2 < 1)
				{
					num2 = 1;
				}
				int num3 = size.h >> j;
				if (num3 < 1)
				{
					num3 = 1;
				}
				RenderTextureDescriptor desc = new RenderTextureDescriptor(num2, num3, colorFormat, 0);
				desc.msaaSamples = sampleCount;
				desc.useMipMap = true;
				desc.autoGenerateMips = false;
				desc.sRGB = false;
				RenderTexture temporary = RenderTexture.GetTemporary(desc);
				if (!temporary.IsCreated())
				{
					temporary.Create();
				}
				temporary.DiscardContents();
				RenderTexture renderTexture = textures[i] as RenderTexture;
				bool flag = isHdr || (renderTexture != null && QualitySettings.activeColorSpace == ColorSpace.Linear);
				if (currentOverlayShape != OverlayShape.Cubemap && currentOverlayShape != OverlayShape.OffcenterCubemap)
				{
					tex2DMaterial.SetInt("_linearToSrgb", (!isHdr && flag) ? 1 : 0);
					tex2DMaterial.SetInt("_premultiply", 1);
					Graphics.Blit(textures[i], temporary, tex2DMaterial);
					Graphics.CopyTexture(temporary, 0, 0, texture, 0, j);
				}
				else
				{
					for (int k = 0; k < 6; k++)
					{
						cubeMaterial.SetInt("_linearToSrgb", (!isHdr && flag) ? 1 : 0);
						cubeMaterial.SetInt("_premultiply", 1);
						cubeMaterial.SetInt("_face", k);
						Graphics.Blit(textures[i], temporary, cubeMaterial);
						Graphics.CopyTexture(temporary, 0, 0, texture, k, j);
					}
				}
				RenderTexture.ReleaseTemporary(temporary);
				result = true;
			}
		}
		return result;
	}

	private bool SubmitLayer(bool overlay, bool headLocked, OVRPose pose, Vector3 scale)
	{
		int num = ((texturesPerStage >= 2) ? 1 : 0);
		bool result = OVRPlugin.EnqueueSubmitLayer(overlay, headLocked, layerTextures[0].appTexturePtr, layerTextures[num].appTexturePtr, layerId, frameIndex, pose.flipZ().ToPosef(), scale.ToVector3f(), layerIndex, (OVRPlugin.OverlayShape)currentOverlayShape);
		if (isDynamic)
		{
			frameIndex++;
		}
		prevOverlayShape = currentOverlayShape;
		return result;
	}

	private void Awake()
	{
		Debug.Log("Overlay Awake");
		if (tex2DMaterial == null)
		{
			tex2DMaterial = new Material(Shader.Find("Oculus/Texture2D Blit"));
		}
		if (cubeMaterial == null)
		{
			cubeMaterial = new Material(Shader.Find("Oculus/Cubemap Blit"));
		}
		rend = GetComponent<Renderer>();
		if (textures.Length == 0)
		{
			textures = new Texture[1];
		}
		if (rend != null && textures[0] == null)
		{
			textures[0] = rend.material.mainTexture;
		}
	}

	private void OnEnable()
	{
		if (!OVRManager.isHmdPresent)
		{
			base.enabled = false;
		}
	}

	private void OnDisable()
	{
		DestroyLayerTextures();
		DestroyLayer();
	}

	private void OnDestroy()
	{
		DestroyLayerTextures();
		DestroyLayer();
	}

	private bool ComputeSubmit(ref OVRPose pose, ref Vector3 scale, ref bool overlay, ref bool headLocked)
	{
		Camera main = Camera.main;
		overlay = currentOverlayType == OverlayType.Overlay;
		headLocked = false;
		Transform parent = base.transform;
		while (parent != null && !headLocked)
		{
			headLocked |= parent == main.transform;
			parent = parent.parent;
		}
		pose = ((!headLocked) ? base.transform.ToTrackingSpacePose(main) : base.transform.ToHeadSpacePose(main));
		scale = base.transform.lossyScale;
		for (int i = 0; i < 3; i++)
		{
			scale[i] /= main.transform.lossyScale[i];
		}
		if (currentOverlayShape == OverlayShape.Cubemap)
		{
			pose.position = main.transform.position;
		}
		if (currentOverlayShape == OverlayShape.OffcenterCubemap)
		{
			pose.position = base.transform.position;
			if (pose.position.magnitude > 1f)
			{
				Debug.LogWarning("Your cube map center offset's magnitude is greater than 1, which will cause some cube map pixel always invisible .");
				return false;
			}
		}
		if (currentOverlayShape == OverlayShape.Cylinder)
		{
			float num = scale.x / scale.z / (float)Math.PI * 180f;
			if (num > 180f)
			{
				Debug.LogWarning("Cylinder overlay's arc angle has to be below 180 degree, current arc angle is " + num + " degree.");
				return false;
			}
		}
		return true;
	}

	private void LateUpdate()
	{
		if (currentOverlayType == OverlayType.None || textures.Length < texturesPerStage || textures[0] == null || Time.frameCount <= prevFrameIndex)
		{
			return;
		}
		prevFrameIndex = Time.frameCount;
		OVRPose pose = OVRPose.identity;
		Vector3 scale = Vector3.one;
		bool overlay = false;
		bool headLocked = false;
		if (!ComputeSubmit(ref pose, ref scale, ref overlay, ref headLocked))
		{
			return;
		}
		OVRPlugin.LayerDesc currentLayerDesc = GetCurrentLayerDesc();
		bool isHdr = currentLayerDesc.Format == OVRPlugin.EyeTextureFormat.R16G16B16A16_FP;
		bool flag = CreateLayer(currentLayerDesc.MipLevels, currentLayerDesc.SampleCount, currentLayerDesc.Format, currentLayerDesc.LayerFlags, currentLayerDesc.TextureSize, currentLayerDesc.Shape);
		if (layerIndex == -1 || layerId <= 0)
		{
			return;
		}
		bool useMipmaps = currentLayerDesc.MipLevels > 1;
		flag |= CreateLayerTextures(useMipmaps, currentLayerDesc.TextureSize, isHdr);
		if (layerTextures[0].appTexture as RenderTexture != null)
		{
			isDynamic = true;
		}
		if (LatchLayerTextures() && PopulateLayer(currentLayerDesc.MipLevels, isHdr, currentLayerDesc.TextureSize, currentLayerDesc.SampleCount))
		{
			bool flag2 = SubmitLayer(overlay, headLocked, pose, scale);
			if ((bool)rend)
			{
				rend.enabled = !flag2;
			}
		}
	}
}
