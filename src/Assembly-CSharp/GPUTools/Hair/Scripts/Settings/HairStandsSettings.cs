using System;
using GPUTools.Hair.Scripts.Geometry.Abstract;
using GPUTools.Hair.Scripts.Settings.Abstract;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Settings
{
	[Serializable]
	public class HairStandsSettings : HairSettingsBase
	{
		public GeometryProviderBase Provider;

		public HairHeadCenterType HeadCenterType;

		public Transform HeadCenterTransform;

		public Vector3 HeadCenter;

		public int Segments => Provider.GetSegmentsNum();

		public Vector3 HeadCenterWorld
		{
			get
			{
				if (HeadCenterType == HairHeadCenterType.LocalPoint)
				{
					return (!(Provider != null)) ? Vector3.zero : Provider.transform.TransformPoint(HeadCenter);
				}
				return (!(HeadCenterTransform != null)) ? Vector3.one : HeadCenterTransform.position;
			}
		}

		public override bool Validate()
		{
			return Provider != null && Provider.Validate(true);
		}
	}
}
