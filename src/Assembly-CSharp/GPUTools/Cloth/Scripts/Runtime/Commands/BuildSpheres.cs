using GPUTools.Common.Scripts.Tools.Commands;

namespace GPUTools.Cloth.Scripts.Runtime.Commands
{
	public class BuildSpheres : BuildChainCommand
	{
		private readonly ClothSettings settings;

		public BuildSpheres(ClothSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			settings.Runtime.ProcessedSpheres = GPUCollidersManager.processedSpheresBuffer;
		}

		protected override void OnFixedDispatch()
		{
			settings.Runtime.ProcessedSpheres = GPUCollidersManager.processedSpheresBuffer;
		}
	}
}
