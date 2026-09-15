using GPUTools.Common.Scripts.Tools.Commands;

namespace GPUTools.Hair.Scripts.Runtime.Commands
{
	public class BuildPlanes : BuildChainCommand
	{
		private readonly HairSettings settings;

		public BuildPlanes(HairSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			settings.RuntimeData.Planes = GPUCollidersManager.planesBuffer;
		}

		protected override void OnFixedDispatch()
		{
			settings.RuntimeData.Planes = GPUCollidersManager.planesBuffer;
		}
	}
}
