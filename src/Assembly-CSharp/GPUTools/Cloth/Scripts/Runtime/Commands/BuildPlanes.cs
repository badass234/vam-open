using GPUTools.Common.Scripts.Tools.Commands;

namespace GPUTools.Cloth.Scripts.Runtime.Commands
{
	public class BuildPlanes : BuildChainCommand
	{
		private readonly ClothSettings settings;

		public BuildPlanes(ClothSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			settings.Runtime.Planes = GPUCollidersManager.planesBuffer;
		}

		protected override void OnFixedDispatch()
		{
			settings.Runtime.Planes = GPUCollidersManager.planesBuffer;
		}
	}
}
