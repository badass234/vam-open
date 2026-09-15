using System;
using System.Runtime.InteropServices;

namespace Oculus.Avatar
{
	public class CAPI
	{
		private const string LibFile = "libovravatar";

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_Initialize(string appID);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_Shutdown();

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovrAvatarMessage_Pop();

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarMessageType ovrAvatarMessage_GetType(IntPtr msg);

		public static ovrAvatarMessage_AvatarSpecification ovrAvatarMessage_GetAvatarSpecification(IntPtr msg)
		{
			IntPtr ptr = ovrAvatarMessage_GetAvatarSpecification_Native(msg);
			return (ovrAvatarMessage_AvatarSpecification)Marshal.PtrToStructure(ptr, typeof(ovrAvatarMessage_AvatarSpecification));
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarMessage_GetAvatarSpecification")]
		private static extern IntPtr ovrAvatarMessage_GetAvatarSpecification_Native(IntPtr msg);

		public static ovrAvatarMessage_AssetLoaded ovrAvatarMessage_GetAssetLoaded(IntPtr msg)
		{
			IntPtr ptr = ovrAvatarMessage_GetAssetLoaded_Native(msg);
			return (ovrAvatarMessage_AssetLoaded)Marshal.PtrToStructure(ptr, typeof(ovrAvatarMessage_AssetLoaded));
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarMessage_GetAssetLoaded")]
		private static extern IntPtr ovrAvatarMessage_GetAssetLoaded_Native(IntPtr msg);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatarMessage_Free(IntPtr msg);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovrAvatarSpecificationRequest_Create(ulong userID);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatarSpecificationRequest_Destroy(IntPtr specificationRequest);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_RequestAvatarSpecification(ulong userID);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_RequestAvatarSpecificationFromSpecRequest(IntPtr specificationRequest);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovrAvatar_Create(IntPtr avatarSpecification, ovrAvatarCapabilities capabilities);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_Destroy(IntPtr avatar);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatarPose_UpdateBody(IntPtr avatar, ovrAvatarTransform headPose);

		public static void ovrAvatarPose_UpdateVoiceVisualization(IntPtr avatar, float[] pcmData)
		{
			ovrAvatarPose_UpdateVoiceVisualization_Native(avatar, (uint)pcmData.Length, pcmData);
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarPose_UpdateVoiceVisualization")]
		private static extern void ovrAvatarPose_UpdateVoiceVisualization_Native(IntPtr avatar, uint pcmDataSize, [In] float[] pcmData);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatarPose_UpdateHands(IntPtr avatar, ovrAvatarHandInputState inputStateLeft, ovrAvatarHandInputState inputStateRight);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatarPose_Update3DofHands(IntPtr avatar, IntPtr inputStateLeft, IntPtr inputStateRight, ovrAvatarControllerType type);

		public static void ovrAvatarPose_UpdateSDK3DofHands(IntPtr avatar, ovrAvatarHandInputState inputStateLeft, ovrAvatarHandInputState inputStateRight, ovrAvatarControllerType type)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(inputStateLeft));
			IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(inputStateRight));
			Marshal.StructureToPtr(inputStateLeft, intPtr, false);
			Marshal.StructureToPtr(inputStateRight, intPtr2, false);
			ovrAvatar_SetLeftControllerVisibility(avatar, true);
			ovrAvatar_SetRightControllerVisibility(avatar, true);
			ovrAvatar_SetLeftHandVisibility(avatar, true);
			ovrAvatar_SetRightHandVisibility(avatar, true);
			ovrAvatarPose_Update3DofHands(avatar, intPtr, intPtr2, type);
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatarPose_Finalize(IntPtr avatar, float elapsedSeconds);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_SetLeftControllerVisibility(IntPtr avatar, bool show);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_SetRightControllerVisibility(IntPtr avatar, bool show);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_SetLeftHandVisibility(IntPtr avatar, bool show);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_SetRightHandVisibility(IntPtr avatar, bool show);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovrAvatarComponent_Count(IntPtr avatar);

		public static ovrAvatarComponent ovrAvatarComponent_Get(IntPtr avatar, uint index)
		{
			IntPtr ptr = ovrAvatarComponent_Get_Native(avatar, index);
			return (ovrAvatarComponent)Marshal.PtrToStructure(ptr, typeof(ovrAvatarComponent));
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarComponent_Get")]
		public static extern IntPtr ovrAvatarComponent_Get_Native(IntPtr avatar, uint index);

		public static ovrAvatarBaseComponent? ovrAvatarPose_GetBaseComponent(IntPtr avatar)
		{
			IntPtr intPtr = ovrAvatarPose_GetBaseComponent_Native(avatar);
			return (!(intPtr == IntPtr.Zero)) ? new ovrAvatarBaseComponent?((ovrAvatarBaseComponent)Marshal.PtrToStructure(intPtr, typeof(ovrAvatarBaseComponent))) : ((ovrAvatarBaseComponent?)null);
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarPose_GetBaseComponent")]
		private static extern IntPtr ovrAvatarPose_GetBaseComponent_Native(IntPtr avatar);

		public static ovrAvatarBodyComponent? ovrAvatarPose_GetBodyComponent(IntPtr avatar)
		{
			IntPtr intPtr = ovrAvatarPose_GetBodyComponent_Native(avatar);
			return (!(intPtr == IntPtr.Zero)) ? new ovrAvatarBodyComponent?((ovrAvatarBodyComponent)Marshal.PtrToStructure(intPtr, typeof(ovrAvatarBodyComponent))) : ((ovrAvatarBodyComponent?)null);
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarPose_GetBodyComponent")]
		private static extern IntPtr ovrAvatarPose_GetBodyComponent_Native(IntPtr avatar);

		public static ovrAvatarControllerComponent? ovrAvatarPose_GetLeftControllerComponent(IntPtr avatar)
		{
			IntPtr intPtr = ovrAvatarPose_GetLeftControllerComponent_Native(avatar);
			return (!(intPtr == IntPtr.Zero)) ? new ovrAvatarControllerComponent?((ovrAvatarControllerComponent)Marshal.PtrToStructure(intPtr, typeof(ovrAvatarControllerComponent))) : ((ovrAvatarControllerComponent?)null);
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarPose_GetLeftControllerComponent")]
		private static extern IntPtr ovrAvatarPose_GetLeftControllerComponent_Native(IntPtr avatar);

		public static ovrAvatarControllerComponent? ovrAvatarPose_GetRightControllerComponent(IntPtr avatar)
		{
			IntPtr intPtr = ovrAvatarPose_GetRightControllerComponent_Native(avatar);
			return (!(intPtr == IntPtr.Zero)) ? new ovrAvatarControllerComponent?((ovrAvatarControllerComponent)Marshal.PtrToStructure(intPtr, typeof(ovrAvatarControllerComponent))) : ((ovrAvatarControllerComponent?)null);
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarPose_GetRightControllerComponent")]
		private static extern IntPtr ovrAvatarPose_GetRightControllerComponent_Native(IntPtr avatar);

		public static ovrAvatarHandComponent? ovrAvatarPose_GetLeftHandComponent(IntPtr avatar)
		{
			IntPtr intPtr = ovrAvatarPose_GetLeftHandComponent_Native(avatar);
			return (!(intPtr == IntPtr.Zero)) ? new ovrAvatarHandComponent?((ovrAvatarHandComponent)Marshal.PtrToStructure(intPtr, typeof(ovrAvatarHandComponent))) : ((ovrAvatarHandComponent?)null);
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarPose_GetLeftHandComponent")]
		private static extern IntPtr ovrAvatarPose_GetLeftHandComponent_Native(IntPtr avatar);

		public static ovrAvatarHandComponent? ovrAvatarPose_GetRightHandComponent(IntPtr avatar)
		{
			IntPtr intPtr = ovrAvatarPose_GetRightHandComponent_Native(avatar);
			return (!(intPtr == IntPtr.Zero)) ? new ovrAvatarHandComponent?((ovrAvatarHandComponent)Marshal.PtrToStructure(intPtr, typeof(ovrAvatarHandComponent))) : ((ovrAvatarHandComponent?)null);
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarPose_GetRightHandComponent")]
		private static extern IntPtr ovrAvatarPose_GetRightHandComponent_Native(IntPtr avatar);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatarAsset_BeginLoading(ulong assetID);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarAssetType ovrAvatarAsset_GetType(IntPtr assetHandle);

		public static ovrAvatarMeshAssetData ovrAvatarAsset_GetMeshData(IntPtr assetPtr)
		{
			IntPtr ptr = ovrAvatarAsset_GetMeshData_Native(assetPtr);
			return (ovrAvatarMeshAssetData)Marshal.PtrToStructure(ptr, typeof(ovrAvatarMeshAssetData));
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarAsset_GetMeshData")]
		private static extern IntPtr ovrAvatarAsset_GetMeshData_Native(IntPtr assetPtr);

		public static ovrAvatarTextureAssetData ovrAvatarAsset_GetTextureData(IntPtr assetPtr)
		{
			IntPtr ptr = ovrAvatarAsset_GetTextureData_Native(assetPtr);
			return (ovrAvatarTextureAssetData)Marshal.PtrToStructure(ptr, typeof(ovrAvatarTextureAssetData));
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarAsset_GetTextureData")]
		private static extern IntPtr ovrAvatarAsset_GetTextureData_Native(IntPtr assetPtr);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarAsset_GetMaterialData")]
		private static extern IntPtr ovrAvatarAsset_GetMaterialData_Native(IntPtr assetPtr);

		public static ovrAvatarMaterialState ovrAvatarAsset_GetMaterialState(IntPtr assetPtr)
		{
			IntPtr ptr = ovrAvatarAsset_GetMaterialData_Native(assetPtr);
			return (ovrAvatarMaterialState)Marshal.PtrToStructure(ptr, typeof(ovrAvatarMaterialState));
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarRenderPartType ovrAvatarRenderPart_GetType(IntPtr renderPart);

		public static ovrAvatarRenderPart_SkinnedMeshRender ovrAvatarRenderPart_GetSkinnedMeshRender(IntPtr renderPart)
		{
			IntPtr ptr = ovrAvatarRenderPart_GetSkinnedMeshRender_Native(renderPart);
			return (ovrAvatarRenderPart_SkinnedMeshRender)Marshal.PtrToStructure(ptr, typeof(ovrAvatarRenderPart_SkinnedMeshRender));
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarRenderPart_GetSkinnedMeshRender")]
		private static extern IntPtr ovrAvatarRenderPart_GetSkinnedMeshRender_Native(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarTransform ovrAvatarSkinnedMeshRender_GetTransform(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarTransform ovrAvatarSkinnedMeshRenderPBS_GetTransform(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarTransform ovrAvatarSkinnedMeshRenderPBSV2_GetTransform(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarVisibilityFlags ovrAvatarSkinnedMeshRender_GetVisibilityMask(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovrAvatarSkinnedMeshRender_MaterialStateChanged(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovrAvatarSkinnedMeshRenderPBSV2_MaterialStateChanged(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarVisibilityFlags ovrAvatarSkinnedMeshRenderPBS_GetVisibilityMask(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarVisibilityFlags ovrAvatarSkinnedMeshRenderPBSV2_GetVisibilityMask(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarMaterialState ovrAvatarSkinnedMeshRender_GetMaterialState(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarPBSMaterialState ovrAvatarSkinnedMeshRenderPBSV2_GetPBSMaterialState(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovrAvatarSkinnedMeshRender_GetDirtyJoints(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovrAvatarSkinnedMeshRenderPBS_GetDirtyJoints(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovrAvatarSkinnedMeshRenderPBSV2_GetDirtyJoints(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarTransform ovrAvatarSkinnedMeshRender_GetJointTransform(IntPtr renderPart, uint jointIndex);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarTransform ovrAvatarSkinnedMeshRenderPBS_GetJointTransform(IntPtr renderPart, uint jointIndex);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ovrAvatarTransform ovrAvatarSkinnedMeshRenderPBSV2_GetJointTransform(IntPtr renderPart, uint jointIndex);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovrAvatarSkinnedMeshRenderPBS_GetAlbedoTextureAssetID(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovrAvatarSkinnedMeshRenderPBS_GetSurfaceTextureAssetID(IntPtr renderPart);

		public static ovrAvatarRenderPart_SkinnedMeshRenderPBS ovrAvatarRenderPart_GetSkinnedMeshRenderPBS(IntPtr renderPart)
		{
			IntPtr ptr = ovrAvatarRenderPart_GetSkinnedMeshRenderPBS_Native(renderPart);
			return (ovrAvatarRenderPart_SkinnedMeshRenderPBS)Marshal.PtrToStructure(ptr, typeof(ovrAvatarRenderPart_SkinnedMeshRenderPBS));
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarRenderPart_GetSkinnedMeshRenderPBS")]
		private static extern IntPtr ovrAvatarRenderPart_GetSkinnedMeshRenderPBS_Native(IntPtr renderPart);

		public static ovrAvatarRenderPart_SkinnedMeshRenderPBS_V2 ovrAvatarRenderPart_GetSkinnedMeshRenderPBSV2(IntPtr renderPart)
		{
			IntPtr ptr = ovrAvatarRenderPart_GetSkinnedMeshRenderPBSV2_Native(renderPart);
			return (ovrAvatarRenderPart_SkinnedMeshRenderPBS_V2)Marshal.PtrToStructure(ptr, typeof(ovrAvatarRenderPart_SkinnedMeshRenderPBS_V2));
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarRenderPart_GetSkinnedMeshRenderPBSV2")]
		private static extern IntPtr ovrAvatarRenderPart_GetSkinnedMeshRenderPBSV2_Native(IntPtr renderPart);

		public static ovrAvatarRenderPart_ProjectorRender ovrAvatarRenderPart_GetProjectorRender(IntPtr renderPart)
		{
			IntPtr ptr = ovrAvatarRenderPart_GetProjectorRender_Native(renderPart);
			return (ovrAvatarRenderPart_ProjectorRender)Marshal.PtrToStructure(ptr, typeof(ovrAvatarRenderPart_ProjectorRender));
		}

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrAvatarRenderPart_GetProjectorRender")]
		private static extern IntPtr ovrAvatarRenderPart_GetProjectorRender_Native(IntPtr renderPart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovrAvatar_GetReferencedAssetCount(IntPtr avatar);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ovrAvatar_GetReferencedAsset(IntPtr avatar, uint index);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_SetLeftHandGesture(IntPtr avatar, ovrAvatarHandGesture gesture);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_SetRightHandGesture(IntPtr avatar, ovrAvatarHandGesture gesture);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_SetLeftHandCustomGesture(IntPtr avatar, uint jointCount, [In] ovrAvatarTransform[] customJointTransforms);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_SetRightHandCustomGesture(IntPtr avatar, uint jointCount, [In] ovrAvatarTransform[] customJointTransforms);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatar_UpdatePoseFromPacket(IntPtr avatar, IntPtr packet, float secondsFromStart);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatarPacket_BeginRecording(IntPtr avatar);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovrAvatarPacket_EndRecording(IntPtr avatar);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ovrAvatarPacket_GetSize(IntPtr packet);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrAvatarPacket_GetDurationSeconds(IntPtr packet);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrAvatarPacket_Free(IntPtr packet);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool ovrAvatarPacket_Write(IntPtr packet, uint bufferSize, [Out] byte[] buffer);

		[DllImport("libovravatar", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovrAvatarPacket_Read(uint bufferSize, [In] byte[] buffer);
	}
}
