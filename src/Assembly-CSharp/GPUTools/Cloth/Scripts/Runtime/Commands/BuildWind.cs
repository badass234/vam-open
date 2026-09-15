using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Physics.Scripts.Wind;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Commands
{
	public class BuildWind : BuildChainCommand
	{
		private readonly ClothSettings settings;

		private readonly WindReceiver receiver;

		public BuildWind(ClothSettings settings)
		{
			this.settings = settings;
			receiver = new WindReceiver();
		}

		protected override void OnDispatch()
		{
			Vector3 wind = receiver.GetWind(settings.transform.position) * settings.WindMultiplier;
			settings.Runtime.Wind = wind;
		}
	}
}
