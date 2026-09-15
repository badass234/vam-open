using System;
using GPUTools.Common.Scripts.Tools.Ranges;
using GPUTools.Hair.Scripts.Settings.Abstract;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Settings
{
	[Serializable]
	public class HairLODSettings : HairSettingsBase
	{
		public Camera ViewCamera;

		public bool UseFixedSettings;

		public int FixedDensity = 24;

		public int FixedDetail = 24;

		public float FixedWidth = 0.0001f;

		public FloatRange Distance = new FloatRange(0f, 5f);

		public FloatRange Density = new FloatRange(4f, 8f);

		public FloatRange Detail = new FloatRange(4f, 16f);

		public FloatRange Width = new FloatRange(0.0004f, 0.002f);

		public float GetWidth(Vector3 position)
		{
			if (UseFixedSettings)
			{
				return FixedWidth;
			}
			return Width.GetLerp(GetDistanceK(position));
		}

		public int GetDencity(Vector3 position)
		{
			if (UseFixedSettings)
			{
				return FixedDensity;
			}
			return (int)Density.GetLerp(1f - GetDistanceK(position));
		}

		public int GetDetail(Vector3 position)
		{
			if (UseFixedSettings)
			{
				return FixedDetail;
			}
			return (int)Detail.GetLerp(1f - GetDistanceK(position));
		}

		public float GetDistanceK(Vector3 position)
		{
			float value = (GetDistanceToCamera(position) - Distance.Min) / (Distance.Max - Distance.Min);
			return Mathf.Clamp01(value);
		}

		public float GetDistanceToCamera(Vector3 position)
		{
			if (ViewCamera != null)
			{
				return (position - ViewCamera.transform.position).magnitude;
			}
			if (Camera.main != null)
			{
				return (position - Camera.main.transform.position).magnitude;
			}
			return 0f;
		}

		public bool IsPhysicsEnabled(Vector3 position)
		{
			return GetDistanceToCamera(position) < Distance.Max;
		}
	}
}
