using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentLightProbeProxyVolume : PersistentBehaviour
	{
		public Vector3 sizeCustom;

		public Vector3 originCustom;

		public uint boundingBoxMode;

		public uint resolutionMode;

		public uint probePositionMode;

		public uint refreshMode;

		public float probeDensity;

		public int gridResolutionX;

		public int gridResolutionY;

		public int gridResolutionZ;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			LightProbeProxyVolume lightProbeProxyVolume = (LightProbeProxyVolume)obj;
			lightProbeProxyVolume.sizeCustom = sizeCustom;
			lightProbeProxyVolume.originCustom = originCustom;
			lightProbeProxyVolume.boundingBoxMode = (LightProbeProxyVolume.BoundingBoxMode)boundingBoxMode;
			lightProbeProxyVolume.resolutionMode = (LightProbeProxyVolume.ResolutionMode)resolutionMode;
			lightProbeProxyVolume.probePositionMode = (LightProbeProxyVolume.ProbePositionMode)probePositionMode;
			lightProbeProxyVolume.refreshMode = (LightProbeProxyVolume.RefreshMode)refreshMode;
			lightProbeProxyVolume.probeDensity = probeDensity;
			lightProbeProxyVolume.gridResolutionX = gridResolutionX;
			lightProbeProxyVolume.gridResolutionY = gridResolutionY;
			lightProbeProxyVolume.gridResolutionZ = gridResolutionZ;
			return lightProbeProxyVolume;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				LightProbeProxyVolume lightProbeProxyVolume = (LightProbeProxyVolume)obj;
				sizeCustom = lightProbeProxyVolume.sizeCustom;
				originCustom = lightProbeProxyVolume.originCustom;
				boundingBoxMode = (uint)lightProbeProxyVolume.boundingBoxMode;
				resolutionMode = (uint)lightProbeProxyVolume.resolutionMode;
				probePositionMode = (uint)lightProbeProxyVolume.probePositionMode;
				refreshMode = (uint)lightProbeProxyVolume.refreshMode;
				probeDensity = lightProbeProxyVolume.probeDensity;
				gridResolutionX = lightProbeProxyVolume.gridResolutionX;
				gridResolutionY = lightProbeProxyVolume.gridResolutionY;
				gridResolutionZ = lightProbeProxyVolume.gridResolutionZ;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
