using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Oculus.Avatar;
using UnityEngine;
using UnityEngine.Events;

public class OvrAvatar : MonoBehaviour
{
	public class PacketEventArgs : EventArgs
	{
		public readonly OvrAvatarPacket Packet;

		public PacketEventArgs(OvrAvatarPacket packet)
		{
			Packet = packet;
		}
	}

	public enum HandType
	{
		Right,
		Left,
		Max
	}

	public enum HandJoint
	{
		HandBase,
		IndexBase,
		IndexTip,
		ThumbBase,
		ThumbTip,
		Max
	}

	public OvrAvatarDriver Driver;

	public OvrAvatarBase Base;

	public OvrAvatarBody Body;

	public OvrAvatarTouchController ControllerLeft;

	public OvrAvatarTouchController ControllerRight;

	public OvrAvatarHand HandLeft;

	public OvrAvatarHand HandRight;

	public bool RecordPackets;

	public bool UseSDKPackets = true;

	public bool StartWithControllers;

	public AvatarLayer FirstPersonLayer;

	public AvatarLayer ThirdPersonLayer;

	public bool ShowFirstPerson = true;

	public bool ShowThirdPerson;

	public ovrAvatarCapabilities Capabilities = ovrAvatarCapabilities.All;

	public Shader SurfaceShader;

	public Shader SurfaceShaderSelfOccluding;

	public Shader SurfaceShaderPBS;

	public Shader SurfaceShaderPBSV2;

	private int renderPartCount;

	private bool showLeftController;

	private bool showRightController;

	private List<float[]> voiceUpdates = new List<float[]>();

	public ulong oculusUserID;

	public bool CombineMeshes;

	public IntPtr sdkAvatar = IntPtr.Zero;

	private HashSet<ulong> assetLoadingIds = new HashSet<ulong>();

	private Dictionary<string, OvrAvatarComponent> trackedComponents = new Dictionary<string, OvrAvatarComponent>();

	public UnityEvent AssetsDoneLoading = new UnityEvent();

	private bool assetsFinishedLoading;

	public Transform LeftHandCustomPose;

	public Transform RightHandCustomPose;

	private Transform cachedLeftHandCustomPose;

	private Transform[] cachedCustomLeftHandJoints;

	private ovrAvatarTransform[] cachedLeftHandTransforms;

	private Transform cachedRightHandCustomPose;

	private Transform[] cachedCustomRightHandJoints;

	private ovrAvatarTransform[] cachedRightHandTransforms;

	public PacketRecordSettings PacketSettings = new PacketRecordSettings();

	private OvrAvatarPacket CurrentUnityPacket;

	private static string[,] HandJoints = new string[2, 5]
	{
		{ "hands:r_hand_world", "hands:r_hand_world/hands:b_r_hand/hands:b_r_index1", "hands:r_hand_world/hands:b_r_hand/hands:b_r_index1/hands:b_r_index2/hands:b_r_index3/hands:b_r_index_ignore", "hands:r_hand_world/hands:b_r_hand/hands:b_r_thumb1/hands:b_r_thumb2", "hands:r_hand_world/hands:b_r_hand/hands:b_r_thumb1/hands:b_r_thumb2/hands:b_r_thumb3/hands:b_r_thumb_ignore" },
		{ "hands:l_hand_world", "hands:l_hand_world/hands:b_l_hand/hands:b_l_index1", "hands:l_hand_world/hands:b_l_hand/hands:b_l_index1/hands:b_l_index2/hands:b_l_index3/hands:b_l_index_ignore", "hands:l_hand_world/hands:b_l_hand/hands:b_l_thumb1/hands:b_l_thumb2", "hands:l_hand_world/hands:b_l_hand/hands:b_l_thumb1/hands:b_l_thumb2/hands:b_l_thumb3/hands:b_l_thumb_ignore" }
	};

	public EventHandler<PacketEventArgs> PacketRecorded;

	private void OnDestroy()
	{
		if (sdkAvatar != IntPtr.Zero)
		{
			CAPI.ovrAvatar_Destroy(sdkAvatar);
		}
	}

	public void AssetLoadedCallback(OvrAvatarAsset asset)
	{
		assetLoadingIds.Remove(asset.assetID);
	}

	private void AddAvatarComponent(GameObject componentObject, ovrAvatarComponent component)
	{
		OvrAvatarComponent ovrAvatarComponent2 = componentObject.AddComponent<OvrAvatarComponent>();
		trackedComponents.Add(component.name, ovrAvatarComponent2);
		if (AddRenderParts(ovrAvatarComponent2, component, componentObject.transform) && CombineMeshes && componentObject.name == "body")
		{
			ovrAvatarComponent2.StartMeshCombining(component);
		}
	}

	private OvrAvatarSkinnedMeshRenderComponent AddSkinnedMeshRenderComponent(GameObject gameObject, ovrAvatarRenderPart_SkinnedMeshRender skinnedMeshRender)
	{
		OvrAvatarSkinnedMeshRenderComponent ovrAvatarSkinnedMeshRenderComponent = gameObject.AddComponent<OvrAvatarSkinnedMeshRenderComponent>();
		ovrAvatarSkinnedMeshRenderComponent.Initialize(skinnedMeshRender, SurfaceShader, SurfaceShaderSelfOccluding, ThirdPersonLayer.layerIndex, FirstPersonLayer.layerIndex, renderPartCount++);
		return ovrAvatarSkinnedMeshRenderComponent;
	}

	private OvrAvatarSkinnedMeshRenderPBSComponent AddSkinnedMeshRenderPBSComponent(GameObject gameObject, ovrAvatarRenderPart_SkinnedMeshRenderPBS skinnedMeshRenderPBS)
	{
		OvrAvatarSkinnedMeshRenderPBSComponent ovrAvatarSkinnedMeshRenderPBSComponent = gameObject.AddComponent<OvrAvatarSkinnedMeshRenderPBSComponent>();
		ovrAvatarSkinnedMeshRenderPBSComponent.Initialize(skinnedMeshRenderPBS, SurfaceShaderPBS, ThirdPersonLayer.layerIndex, FirstPersonLayer.layerIndex, renderPartCount++);
		return ovrAvatarSkinnedMeshRenderPBSComponent;
	}

	private OvrAvatarSkinnedMeshPBSV2RenderComponent AddSkinnedMeshRenderPBSV2Component(GameObject gameObject, ovrAvatarRenderPart_SkinnedMeshRenderPBS_V2 skinnedMeshRenderPBSV2)
	{
		OvrAvatarSkinnedMeshPBSV2RenderComponent ovrAvatarSkinnedMeshPBSV2RenderComponent = gameObject.AddComponent<OvrAvatarSkinnedMeshPBSV2RenderComponent>();
		ovrAvatarSkinnedMeshPBSV2RenderComponent.Initialize(skinnedMeshRenderPBSV2, SurfaceShaderPBSV2, ThirdPersonLayer.layerIndex, FirstPersonLayer.layerIndex, renderPartCount++);
		return ovrAvatarSkinnedMeshPBSV2RenderComponent;
	}

	private OvrAvatarProjectorRenderComponent AddProjectorRenderComponent(GameObject gameObject, ovrAvatarRenderPart_ProjectorRender projectorRender)
	{
		ovrAvatarComponent ovrAvatarComponent2 = CAPI.ovrAvatarComponent_Get(sdkAvatar, projectorRender.componentIndex);
		OvrAvatarComponent value;
		if (trackedComponents.TryGetValue(ovrAvatarComponent2.name, out value) && projectorRender.renderPartIndex < value.RenderParts.Count)
		{
			OvrAvatarRenderComponent target = value.RenderParts[(int)projectorRender.renderPartIndex];
			OvrAvatarProjectorRenderComponent ovrAvatarProjectorRenderComponent = gameObject.AddComponent<OvrAvatarProjectorRenderComponent>();
			ovrAvatarProjectorRenderComponent.InitializeProjectorRender(projectorRender, SurfaceShader, target);
			return ovrAvatarProjectorRenderComponent;
		}
		return null;
	}

	public static IntPtr GetRenderPart(ovrAvatarComponent component, uint renderPartIndex)
	{
		long num = Marshal.SizeOf(typeof(IntPtr)) * renderPartIndex;
		IntPtr ptr = new IntPtr(component.renderParts.ToInt64() + num);
		return (IntPtr)Marshal.PtrToStructure(ptr, typeof(IntPtr));
	}

	private void UpdateAvatarComponent(ovrAvatarComponent component)
	{
		OvrAvatarComponent value;
		if (!trackedComponents.TryGetValue(component.name, out value))
		{
			throw new Exception($"trackedComponents didn't have {component.name}");
		}
		value.UpdateAvatar(component, this);
	}

	private static string GetRenderPartName(ovrAvatarComponent component, uint renderPartIndex)
	{
		return component.name + "_renderPart_" + (int)renderPartIndex;
	}

	internal static void ConvertTransform(ovrAvatarTransform transform, Transform target)
	{
		Vector3 position = transform.position;
		position.z = 0f - position.z;
		Quaternion orientation = transform.orientation;
		orientation.x = 0f - orientation.x;
		orientation.y = 0f - orientation.y;
		target.localPosition = position;
		target.localRotation = orientation;
		target.localScale = transform.scale;
	}

	public static ovrAvatarTransform CreateOvrAvatarTransform(Vector3 position, Quaternion orientation)
	{
		ovrAvatarTransform result = default(ovrAvatarTransform);
		result.position = new Vector3(position.x, position.y, 0f - position.z);
		result.orientation = new Quaternion(0f - orientation.x, 0f - orientation.y, orientation.z, orientation.w);
		result.scale = Vector3.one;
		return result;
	}

	private void RemoveAvatarComponent(string name)
	{
		OvrAvatarComponent value;
		trackedComponents.TryGetValue(name, out value);
		UnityEngine.Object.Destroy(value.gameObject);
		trackedComponents.Remove(name);
	}

	private void UpdateSDKAvatarUnityState()
	{
		uint num = CAPI.ovrAvatarComponent_Count(sdkAvatar);
		HashSet<string> hashSet = new HashSet<string>();
		for (uint num2 = 0u; num2 < num; num2++)
		{
			IntPtr intPtr = CAPI.ovrAvatarComponent_Get_Native(sdkAvatar, num2);
			ovrAvatarComponent component = (ovrAvatarComponent)Marshal.PtrToStructure(intPtr, typeof(ovrAvatarComponent));
			hashSet.Add(component.name);
			if (!trackedComponents.ContainsKey(component.name))
			{
				GameObject gameObject = null;
				Type type = null;
				if ((Capabilities & ovrAvatarCapabilities.Base) != 0)
				{
					ovrAvatarBaseComponent? ovrAvatarBaseComponent2 = CAPI.ovrAvatarPose_GetBaseComponent(sdkAvatar);
					if (ovrAvatarBaseComponent2.HasValue && intPtr == ovrAvatarBaseComponent2.Value.renderComponent)
					{
						type = typeof(OvrAvatarBase);
						if (Base != null)
						{
							gameObject = Base.gameObject;
						}
					}
				}
				if (type == null && (Capabilities & ovrAvatarCapabilities.Body) != 0)
				{
					ovrAvatarBodyComponent? ovrAvatarBodyComponent2 = CAPI.ovrAvatarPose_GetBodyComponent(sdkAvatar);
					if (ovrAvatarBodyComponent2.HasValue && intPtr == ovrAvatarBodyComponent2.Value.renderComponent)
					{
						type = typeof(OvrAvatarBody);
						if (Body != null)
						{
							gameObject = Body.gameObject;
						}
					}
				}
				if (type == null && (Capabilities & ovrAvatarCapabilities.Hands) != 0)
				{
					ovrAvatarControllerComponent? ovrAvatarControllerComponent2 = CAPI.ovrAvatarPose_GetLeftControllerComponent(sdkAvatar);
					if (type == null && ovrAvatarControllerComponent2.HasValue && intPtr == ovrAvatarControllerComponent2.Value.renderComponent)
					{
						type = typeof(OvrAvatarTouchController);
						if (ControllerLeft != null)
						{
							gameObject = ControllerLeft.gameObject;
						}
					}
					ovrAvatarControllerComponent2 = CAPI.ovrAvatarPose_GetRightControllerComponent(sdkAvatar);
					if (type == null && ovrAvatarControllerComponent2.HasValue && intPtr == ovrAvatarControllerComponent2.Value.renderComponent)
					{
						type = typeof(OvrAvatarTouchController);
						if (ControllerRight != null)
						{
							gameObject = ControllerRight.gameObject;
						}
					}
					ovrAvatarHandComponent? ovrAvatarHandComponent2 = CAPI.ovrAvatarPose_GetLeftHandComponent(sdkAvatar);
					if (type == null && ovrAvatarHandComponent2.HasValue && intPtr == ovrAvatarHandComponent2.Value.renderComponent)
					{
						type = typeof(OvrAvatarHand);
						if (HandLeft != null)
						{
							gameObject = HandLeft.gameObject;
						}
					}
					ovrAvatarHandComponent2 = CAPI.ovrAvatarPose_GetRightHandComponent(sdkAvatar);
					if (type == null && ovrAvatarHandComponent2.HasValue && intPtr == ovrAvatarHandComponent2.Value.renderComponent)
					{
						type = typeof(OvrAvatarHand);
						if (HandRight != null)
						{
							gameObject = HandRight.gameObject;
						}
					}
				}
				if (gameObject == null && type == null)
				{
					gameObject = new GameObject();
					gameObject.name = component.name;
					gameObject.transform.SetParent(base.transform);
				}
				if (gameObject != null)
				{
					AddAvatarComponent(gameObject, component);
				}
			}
			UpdateAvatarComponent(component);
		}
		HashSet<string> hashSet2 = new HashSet<string>(trackedComponents.Keys);
		hashSet2.ExceptWith(hashSet);
		foreach (string item in hashSet2)
		{
			RemoveAvatarComponent(item);
		}
	}

	private void UpdateCustomPoses()
	{
		if (UpdatePoseRoot(LeftHandCustomPose, ref cachedLeftHandCustomPose, ref cachedCustomLeftHandJoints, ref cachedLeftHandTransforms) && cachedLeftHandCustomPose == null && sdkAvatar != IntPtr.Zero)
		{
			CAPI.ovrAvatar_SetLeftHandGesture(sdkAvatar, ovrAvatarHandGesture.Default);
		}
		if (UpdatePoseRoot(RightHandCustomPose, ref cachedRightHandCustomPose, ref cachedCustomRightHandJoints, ref cachedRightHandTransforms) && cachedRightHandCustomPose == null && sdkAvatar != IntPtr.Zero)
		{
			CAPI.ovrAvatar_SetRightHandGesture(sdkAvatar, ovrAvatarHandGesture.Default);
		}
		if (sdkAvatar != IntPtr.Zero)
		{
			if (cachedLeftHandCustomPose != null && UpdateTransforms(cachedCustomLeftHandJoints, cachedLeftHandTransforms))
			{
				CAPI.ovrAvatar_SetLeftHandCustomGesture(sdkAvatar, (uint)cachedLeftHandTransforms.Length, cachedLeftHandTransforms);
			}
			if (cachedRightHandCustomPose != null && UpdateTransforms(cachedCustomRightHandJoints, cachedRightHandTransforms))
			{
				CAPI.ovrAvatar_SetRightHandCustomGesture(sdkAvatar, (uint)cachedRightHandTransforms.Length, cachedRightHandTransforms);
			}
		}
	}

	private static bool UpdatePoseRoot(Transform poseRoot, ref Transform cachedPoseRoot, ref Transform[] cachedPoseJoints, ref ovrAvatarTransform[] transforms)
	{
		if (poseRoot == cachedPoseRoot)
		{
			return false;
		}
		if (!poseRoot)
		{
			cachedPoseRoot = null;
			cachedPoseJoints = null;
			transforms = null;
		}
		else
		{
			List<Transform> list = new List<Transform>();
			OrderJoints(poseRoot, list);
			cachedPoseRoot = poseRoot;
			cachedPoseJoints = list.ToArray();
			transforms = new ovrAvatarTransform[list.Count];
		}
		return true;
	}

	private static bool UpdateTransforms(Transform[] joints, ovrAvatarTransform[] transforms)
	{
		bool result = false;
		for (int i = 0; i < joints.Length; i++)
		{
			Transform transform = joints[i];
			ovrAvatarTransform ovrAvatarTransform2 = CreateOvrAvatarTransform(transform.localPosition, transform.localRotation);
			if (ovrAvatarTransform2.position != transforms[i].position || ovrAvatarTransform2.orientation != transforms[i].orientation)
			{
				transforms[i] = ovrAvatarTransform2;
				result = true;
			}
		}
		return result;
	}

	private static void OrderJoints(Transform transform, List<Transform> joints)
	{
		joints.Add(transform);
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			OrderJoints(child, joints);
		}
	}

	private void AvatarSpecificationCallback(IntPtr avatarSpecification)
	{
		sdkAvatar = CAPI.ovrAvatar_Create(avatarSpecification, Capabilities);
		ShowLeftController(showLeftController);
		ShowRightController(showRightController);
		uint num = CAPI.ovrAvatar_GetReferencedAssetCount(sdkAvatar);
		for (uint num2 = 0u; num2 < num; num2++)
		{
			ulong num3 = CAPI.ovrAvatar_GetReferencedAsset(sdkAvatar, num2);
			if (OvrAvatarSDKManager.Instance.GetAsset(num3) == null)
			{
				OvrAvatarSDKManager.Instance.BeginLoadingAsset(num3, AssetLoadedCallback);
				assetLoadingIds.Add(num3);
			}
		}
	}

	private void Start()
	{
		ShowLeftController(StartWithControllers);
		ShowRightController(StartWithControllers);
		OvrAvatarSDKManager.Instance.RequestAvatarSpecification(oculusUserID, AvatarSpecificationCallback);
		Driver.Mode = ((!UseSDKPackets) ? OvrAvatarDriver.PacketMode.Unity : OvrAvatarDriver.PacketMode.SDK);
	}

	private void Update()
	{
		if (sdkAvatar == IntPtr.Zero)
		{
			return;
		}
		if (Driver != null)
		{
			Driver.UpdateTransforms(sdkAvatar);
			foreach (float[] voiceUpdate in voiceUpdates)
			{
				CAPI.ovrAvatarPose_UpdateVoiceVisualization(sdkAvatar, voiceUpdate);
			}
			voiceUpdates.Clear();
			CAPI.ovrAvatarPose_Finalize(sdkAvatar, Time.deltaTime);
		}
		if (RecordPackets)
		{
			RecordFrame();
		}
		if (assetLoadingIds.Count == 0)
		{
			UpdateSDKAvatarUnityState();
			UpdateCustomPoses();
			if (!assetsFinishedLoading)
			{
				AssetsDoneLoading.Invoke();
				assetsFinishedLoading = true;
			}
		}
	}

	public static ovrAvatarHandInputState CreateInputState(ovrAvatarTransform transform, OvrAvatarDriver.ControllerPose pose)
	{
		ovrAvatarHandInputState result = default(ovrAvatarHandInputState);
		result.transform = transform;
		result.buttonMask = pose.buttons;
		result.touchMask = pose.touches;
		result.joystickX = pose.joystickPosition.x;
		result.joystickY = pose.joystickPosition.y;
		result.indexTrigger = pose.indexTrigger;
		result.handTrigger = pose.handTrigger;
		result.isActive = pose.isActive;
		return result;
	}

	public void ShowControllers(bool show)
	{
		ShowLeftController(show);
		ShowRightController(show);
	}

	public void ShowLeftController(bool show)
	{
		if (sdkAvatar != IntPtr.Zero)
		{
			CAPI.ovrAvatar_SetLeftControllerVisibility(sdkAvatar, show);
		}
		showLeftController = show;
	}

	public void ShowRightController(bool show)
	{
		if (sdkAvatar != IntPtr.Zero)
		{
			CAPI.ovrAvatar_SetRightControllerVisibility(sdkAvatar, show);
		}
		showRightController = show;
	}

	public void UpdateVoiceVisualization(float[] voiceSamples)
	{
		voiceUpdates.Add(voiceSamples);
	}

	private void RecordFrame()
	{
		if (UseSDKPackets)
		{
			RecordSDKFrame();
		}
		else
		{
			RecordUnityFrame();
		}
	}

	private void RecordUnityFrame()
	{
		float num = Time.deltaTime;
		OvrAvatarDriver.PoseFrame currentPose = Driver.GetCurrentPose();
		if (CurrentUnityPacket == null)
		{
			CurrentUnityPacket = new OvrAvatarPacket(currentPose);
			num = 0f;
		}
		float num2 = 0f;
		while (num2 < num)
		{
			float num3 = num - num2;
			float num4 = PacketSettings.UpdateRate - CurrentUnityPacket.Duration;
			if (num3 < num4)
			{
				CurrentUnityPacket.AddFrame(currentPose, num3);
				num2 += num3;
				continue;
			}
			OvrAvatarDriver.PoseFrame finalFrame = CurrentUnityPacket.FinalFrame;
			OvrAvatarDriver.PoseFrame b = currentPose;
			float t = num4 / num3;
			OvrAvatarDriver.PoseFrame poseFrame = OvrAvatarDriver.PoseFrame.Interpolate(finalFrame, b, t);
			CurrentUnityPacket.AddFrame(poseFrame, num4);
			num2 += num4;
			if (PacketRecorded != null)
			{
				PacketRecorded(this, new PacketEventArgs(CurrentUnityPacket));
			}
			CurrentUnityPacket = new OvrAvatarPacket(poseFrame);
		}
	}

	private void RecordSDKFrame()
	{
		if (sdkAvatar == IntPtr.Zero)
		{
			return;
		}
		if (!PacketSettings.RecordingFrames)
		{
			CAPI.ovrAvatarPacket_BeginRecording(sdkAvatar);
			PacketSettings.AccumulatedTime = 0f;
			PacketSettings.RecordingFrames = true;
		}
		PacketSettings.AccumulatedTime += Time.deltaTime;
		if (PacketSettings.AccumulatedTime >= PacketSettings.UpdateRate)
		{
			PacketSettings.AccumulatedTime = 0f;
			IntPtr intPtr = CAPI.ovrAvatarPacket_EndRecording(sdkAvatar);
			CAPI.ovrAvatarPacket_BeginRecording(sdkAvatar);
			if (PacketRecorded != null)
			{
				PacketRecorded(this, new PacketEventArgs(new OvrAvatarPacket
				{
					ovrNativePacket = intPtr
				}));
			}
			CAPI.ovrAvatarPacket_Free(intPtr);
		}
	}

	private bool AddRenderParts(OvrAvatarComponent ovrComponent, ovrAvatarComponent component, Transform parent)
	{
		bool result = true;
		for (uint num = 0u; num < component.renderPartCount; num++)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = GetRenderPartName(component, num);
			gameObject.transform.SetParent(parent);
			IntPtr renderPart = GetRenderPart(component, num);
			ovrAvatarRenderPartType ovrAvatarRenderPartType2 = CAPI.ovrAvatarRenderPart_GetType(renderPart);
			OvrAvatarRenderComponent item;
			switch (ovrAvatarRenderPartType2)
			{
			case ovrAvatarRenderPartType.SkinnedMeshRender:
				item = AddSkinnedMeshRenderComponent(gameObject, CAPI.ovrAvatarRenderPart_GetSkinnedMeshRender(renderPart));
				break;
			case ovrAvatarRenderPartType.SkinnedMeshRenderPBS:
				item = AddSkinnedMeshRenderPBSComponent(gameObject, CAPI.ovrAvatarRenderPart_GetSkinnedMeshRenderPBS(renderPart));
				break;
			case ovrAvatarRenderPartType.ProjectorRender:
				result = false;
				item = AddProjectorRenderComponent(gameObject, CAPI.ovrAvatarRenderPart_GetProjectorRender(renderPart));
				break;
			case ovrAvatarRenderPartType.SkinnedMeshRenderPBS_V2:
				result = false;
				item = AddSkinnedMeshRenderPBSV2Component(gameObject, CAPI.ovrAvatarRenderPart_GetSkinnedMeshRenderPBSV2(renderPart));
				break;
			default:
				throw new NotImplementedException($"Unsupported render part type: {ovrAvatarRenderPartType2.ToString()}");
			}
			ovrComponent.RenderParts.Add(item);
		}
		return result;
	}

	public void RefreshBodyParts()
	{
		OvrAvatarComponent value;
		if (!trackedComponents.TryGetValue("body", out value) || !(Body != null))
		{
			return;
		}
		foreach (OvrAvatarRenderComponent renderPart in value.RenderParts)
		{
			UnityEngine.Object.Destroy(renderPart.gameObject);
		}
		value.RenderParts.Clear();
		ovrAvatarBodyComponent? ovrAvatarBodyComponent2 = CAPI.ovrAvatarPose_GetBodyComponent(sdkAvatar);
		if (ovrAvatarBodyComponent2.HasValue)
		{
			ovrAvatarComponent component = (ovrAvatarComponent)Marshal.PtrToStructure(ovrAvatarBodyComponent2.Value.renderComponent, typeof(ovrAvatarComponent));
			AddRenderParts(value, component, Body.gameObject.transform);
			return;
		}
		throw new Exception("Destroyed the body component, but didn't find a new one in the SDK");
	}

	public ovrAvatarBodyComponent? GetBodyComponent()
	{
		return CAPI.ovrAvatarPose_GetBodyComponent(sdkAvatar);
	}

	public Transform GetHandTransform(HandType hand, HandJoint joint)
	{
		if (hand >= HandType.Max || joint >= HandJoint.Max)
		{
			return null;
		}
		OvrAvatarHand ovrAvatarHand = ((hand != HandType.Left) ? HandRight : HandLeft);
		if (ovrAvatarHand != null)
		{
			OvrAvatarComponent component = ovrAvatarHand.GetComponent<OvrAvatarComponent>();
			if (component != null && component.RenderParts.Count > 0)
			{
				OvrAvatarRenderComponent ovrAvatarRenderComponent = component.RenderParts[0];
				return ovrAvatarRenderComponent.transform.Find(HandJoints[(int)hand, (int)joint]);
			}
		}
		return null;
	}

	public void GetPointingDirection(HandType hand, ref Vector3 forward, ref Vector3 up)
	{
		Transform handTransform = GetHandTransform(hand, HandJoint.HandBase);
		if (handTransform != null)
		{
			forward = handTransform.forward;
			up = handTransform.up;
		}
	}

	public Transform GetMouthTransform()
	{
		OvrAvatarComponent value;
		if (trackedComponents.TryGetValue("voice", out value) && value.RenderParts.Count > 0)
		{
			return value.RenderParts[0].transform;
		}
		return null;
	}
}
