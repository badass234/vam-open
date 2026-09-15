using System.Collections.Generic;
using UnityEngine;

namespace MeshVR
{
	public class GlobalLightingManager : MonoBehaviour
	{
		public class LightProbesHolder
		{
			public LightProbes lp;
		}

		public static GlobalLightingManager singleton;

		protected LightmapData[] _startingLightmapData;

		protected List<LightmapData[]> lightmapDataStack;

		protected LightProbesHolder _startingLightProbesHolder;

		protected List<LightProbesHolder> lightProbesStack;

		protected LightmapData[] currentLightmapData => lightmapDataStack[lightmapDataStack.Count - 1];

		protected LightProbesHolder currentLightProbesHolder => lightProbesStack[lightProbesStack.Count - 1];

		protected void ResyncLightmapData()
		{
			LightmapData[] lightmaps = lightmapDataStack[lightmapDataStack.Count - 1];
			LightmapSettings.lightmaps = lightmaps;
		}

		public bool PushLightmapData(LightmapData[] lmd)
		{
			LightmapData[] array = lightmapDataStack[lightmapDataStack.Count - 1];
			if (array == lmd)
			{
				return false;
			}
			lightmapDataStack.Add(lmd);
			ResyncLightmapData();
			return true;
		}

		public void RemoveLightmapData(LightmapData[] lmd)
		{
			if (lmd != null)
			{
				lightmapDataStack.Remove(lmd);
			}
			ResyncLightmapData();
		}

		protected void ResyncLightProbes()
		{
			LightProbesHolder lightProbesHolder = lightProbesStack[lightProbesStack.Count - 1];
			LightmapSettings.lightProbes = lightProbesHolder.lp;
		}

		public LightProbesHolder PushLightProbes(LightProbes lp)
		{
			LightProbesHolder lightProbesHolder = lightProbesStack[lightProbesStack.Count - 1];
			if (lightProbesHolder.lp == lp)
			{
				return null;
			}
			LightProbesHolder lightProbesHolder2 = new LightProbesHolder();
			lightProbesHolder2.lp = lp;
			lightProbesStack.Add(lightProbesHolder2);
			ResyncLightProbes();
			return lightProbesHolder2;
		}

		public void PushLightProbesHolder(LightProbesHolder lph)
		{
			lightProbesStack.Add(lph);
			ResyncLightProbes();
		}

		public void RemoveLightProbesHolder(LightProbesHolder lph)
		{
			if (lph != null)
			{
				lightProbesStack.Remove(lph);
			}
			ResyncLightProbes();
		}

		private void Awake()
		{
			singleton = this;
			_startingLightmapData = LightmapSettings.lightmaps;
			_startingLightProbesHolder = new LightProbesHolder();
			_startingLightProbesHolder.lp = LightmapSettings.lightProbes;
			lightmapDataStack = new List<LightmapData[]>();
			lightmapDataStack.Add(_startingLightmapData);
			lightProbesStack = new List<LightProbesHolder>();
			lightProbesStack.Add(_startingLightProbesHolder);
		}
	}
}
