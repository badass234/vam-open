using System;
using System.IO;
using UnityEngine;

public class OVRMixedRealityCaptureSettings : ScriptableObject
{
	public bool enableMixedReality;

	public LayerMask extraHiddenLayers;

	public OVRManager.CompositionMethod compositionMethod;

	public OVRManager.CameraDevice capturingCameraDevice;

	public bool flipCameraFrameHorizontally;

	public bool flipCameraFrameVertically;

	public float handPoseStateLatency;

	public float sandwichCompositionRenderLatency;

	public int sandwichCompositionBufferedFrames = 8;

	public Color chromaKeyColor = Color.green;

	public float chromaKeySimilarity = 0.6f;

	public float chromaKeySmoothRange = 0.03f;

	public float chromaKeySpillRange = 0.04f;

	public bool useDynamicLighting;

	public OVRManager.DepthQuality depthQuality = OVRManager.DepthQuality.Medium;

	public float dynamicLightingSmoothFactor = 8f;

	public float dynamicLightingDepthVariationClampingValue = 0.001f;

	public OVRManager.VirtualGreenScreenType virtualGreenScreenType;

	public float virtualGreenScreenTopY;

	public float virtualGreenScreenBottomY;

	public bool virtualGreenScreenApplyDepthCulling;

	public float virtualGreenScreenDepthTolerance = 0.2f;

	private const string configFileName = "mrc.config";

	public void ReadFrom(OVRManager manager)
	{
		enableMixedReality = manager.enableMixedReality;
		compositionMethod = manager.compositionMethod;
		extraHiddenLayers = manager.extraHiddenLayers;
		capturingCameraDevice = manager.capturingCameraDevice;
		flipCameraFrameHorizontally = manager.flipCameraFrameHorizontally;
		flipCameraFrameVertically = manager.flipCameraFrameVertically;
		handPoseStateLatency = manager.handPoseStateLatency;
		sandwichCompositionRenderLatency = manager.sandwichCompositionRenderLatency;
		sandwichCompositionBufferedFrames = manager.sandwichCompositionBufferedFrames;
		chromaKeyColor = manager.chromaKeyColor;
		chromaKeySimilarity = manager.chromaKeySimilarity;
		chromaKeySmoothRange = manager.chromaKeySmoothRange;
		chromaKeySpillRange = manager.chromaKeySpillRange;
		useDynamicLighting = manager.useDynamicLighting;
		depthQuality = manager.depthQuality;
		dynamicLightingSmoothFactor = manager.dynamicLightingSmoothFactor;
		dynamicLightingDepthVariationClampingValue = manager.dynamicLightingDepthVariationClampingValue;
		virtualGreenScreenType = manager.virtualGreenScreenType;
		virtualGreenScreenTopY = manager.virtualGreenScreenTopY;
		virtualGreenScreenBottomY = manager.virtualGreenScreenBottomY;
		virtualGreenScreenApplyDepthCulling = manager.virtualGreenScreenApplyDepthCulling;
		virtualGreenScreenDepthTolerance = manager.virtualGreenScreenDepthTolerance;
	}

	public void ApplyTo(OVRManager manager)
	{
		manager.enableMixedReality = enableMixedReality;
		manager.compositionMethod = compositionMethod;
		manager.extraHiddenLayers = extraHiddenLayers;
		manager.capturingCameraDevice = capturingCameraDevice;
		manager.flipCameraFrameHorizontally = flipCameraFrameHorizontally;
		manager.flipCameraFrameVertically = flipCameraFrameVertically;
		manager.handPoseStateLatency = handPoseStateLatency;
		manager.sandwichCompositionRenderLatency = sandwichCompositionRenderLatency;
		manager.sandwichCompositionBufferedFrames = sandwichCompositionBufferedFrames;
		manager.chromaKeyColor = chromaKeyColor;
		manager.chromaKeySimilarity = chromaKeySimilarity;
		manager.chromaKeySmoothRange = chromaKeySmoothRange;
		manager.chromaKeySpillRange = chromaKeySpillRange;
		manager.useDynamicLighting = useDynamicLighting;
		manager.depthQuality = depthQuality;
		manager.dynamicLightingSmoothFactor = dynamicLightingSmoothFactor;
		manager.dynamicLightingDepthVariationClampingValue = dynamicLightingDepthVariationClampingValue;
		manager.virtualGreenScreenType = virtualGreenScreenType;
		manager.virtualGreenScreenTopY = virtualGreenScreenTopY;
		manager.virtualGreenScreenBottomY = virtualGreenScreenBottomY;
		manager.virtualGreenScreenApplyDepthCulling = virtualGreenScreenApplyDepthCulling;
		manager.virtualGreenScreenDepthTolerance = virtualGreenScreenDepthTolerance;
	}

	public void WriteToConfigurationFile()
	{
		string contents = JsonUtility.ToJson(this, true);
		try
		{
			string text = Path.Combine(Application.dataPath, "mrc.config");
			Debug.Log("Write OVRMixedRealityCaptureSettings to " + text);
			File.WriteAllText(text, contents);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Exception caught " + ex.Message);
		}
	}

	public void CombineWithConfigurationFile()
	{
		try
		{
			string text = Path.Combine(Application.dataPath, "mrc.config");
			if (File.Exists(text))
			{
				Debug.Log("MixedRealityCapture configuration file found at " + text);
				string json = File.ReadAllText(text);
				Debug.Log("Apply MixedRealityCapture configuration");
				JsonUtility.FromJsonOverwrite(json, this);
			}
			else
			{
				Debug.Log("MixedRealityCapture configuration file doesn't exist at " + text);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Exception caught " + ex.Message);
		}
	}
}
