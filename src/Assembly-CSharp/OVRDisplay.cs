using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.XR;

public class OVRDisplay
{
	public struct EyeRenderDesc
	{
		public Vector2 resolution;

		public Vector2 fov;
	}

	public struct LatencyData
	{
		public float render;

		public float timeWarp;

		public float postPresent;

		public float renderError;

		public float timeWarpError;
	}

	private bool needsConfigureTexture;

	private EyeRenderDesc[] eyeDescs = new EyeRenderDesc[2];

	private bool recenterRequested;

	private int recenterRequestedFrameCount = int.MaxValue;

	public Vector3 acceleration
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return Vector3.zero;
			}
			return OVRPlugin.GetNodeAcceleration(OVRPlugin.Node.None, OVRPlugin.Step.Render).FromFlippedZVector3f();
		}
	}

	public Vector3 angularAcceleration
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return Vector3.zero;
			}
			return OVRPlugin.GetNodeAngularAcceleration(OVRPlugin.Node.None, OVRPlugin.Step.Render).FromFlippedZVector3f() * 57.29578f;
		}
	}

	public Vector3 velocity
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return Vector3.zero;
			}
			return OVRPlugin.GetNodeVelocity(OVRPlugin.Node.None, OVRPlugin.Step.Render).FromFlippedZVector3f();
		}
	}

	public Vector3 angularVelocity
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return Vector3.zero;
			}
			return OVRPlugin.GetNodeAngularVelocity(OVRPlugin.Node.None, OVRPlugin.Step.Render).FromFlippedZVector3f() * 57.29578f;
		}
	}

	public LatencyData latency
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return default(LatencyData);
			}
			string input = OVRPlugin.latency;
			Regex regex = new Regex("Render: ([0-9]+[.][0-9]+)ms, TimeWarp: ([0-9]+[.][0-9]+)ms, PostPresent: ([0-9]+[.][0-9]+)ms", RegexOptions.None);
			LatencyData result = default(LatencyData);
			Match match = regex.Match(input);
			if (match.Success)
			{
				result.render = float.Parse(match.Groups[1].Value);
				result.timeWarp = float.Parse(match.Groups[2].Value);
				result.postPresent = float.Parse(match.Groups[3].Value);
			}
			return result;
		}
	}

	public float appFramerate
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return 0f;
			}
			return OVRPlugin.GetAppFramerate();
		}
	}

	public int recommendedMSAALevel
	{
		get
		{
			int num = OVRPlugin.recommendedMSAALevel;
			if (num == 1)
			{
				num = 0;
			}
			return num;
		}
	}

	public float[] displayFrequenciesAvailable => OVRPlugin.systemDisplayFrequenciesAvailable;

	public float displayFrequency
	{
		get
		{
			return OVRPlugin.systemDisplayFrequency;
		}
		set
		{
			OVRPlugin.systemDisplayFrequency = value;
		}
	}

	public event Action RecenteredPose;

	public OVRDisplay()
	{
		UpdateTextures();
	}

	public void Update()
	{
		UpdateTextures();
		if (recenterRequested && Time.frameCount > recenterRequestedFrameCount)
		{
			if (this.RecenteredPose != null)
			{
				this.RecenteredPose();
			}
			recenterRequested = false;
			recenterRequestedFrameCount = int.MaxValue;
		}
	}

	public void RecenterPose()
	{
		InputTracking.Recenter();
		recenterRequested = true;
		recenterRequestedFrameCount = Time.frameCount;
		OVRMixedReality.RecenterPose();
	}

	public EyeRenderDesc GetEyeRenderDesc(XRNode eye)
	{
		return eyeDescs[(int)eye];
	}

	private void UpdateTextures()
	{
		ConfigureEyeDesc(XRNode.LeftEye);
		ConfigureEyeDesc(XRNode.RightEye);
	}

	private void ConfigureEyeDesc(XRNode eye)
	{
		if (OVRManager.isHmdPresent)
		{
			OVRPlugin.Sizei eyeTextureSize = OVRPlugin.GetEyeTextureSize((OVRPlugin.Eye)eye);
			OVRPlugin.Frustumf eyeFrustum = OVRPlugin.GetEyeFrustum((OVRPlugin.Eye)eye);
			eyeDescs[(int)eye] = new EyeRenderDesc
			{
				resolution = new Vector2(eyeTextureSize.w, eyeTextureSize.h),
				fov = 57.29578f * new Vector2(eyeFrustum.fovX, eyeFrustum.fovY)
			};
		}
	}
}
