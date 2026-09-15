using GPUTools.Common.Scripts.Tools.Commands;

namespace GPUTools.Hair.Scripts.Runtime.Commands.Physics
{
	public class BuildSpheres : BuildChainCommand
	{
		private readonly HairSettings settings;

		public BuildSpheres(HairSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			settings.RuntimeData.ProcessedSpheres = GPUCollidersManager.processedSpheresBuffer;
		}

		protected override void OnFixedDispatch()
		{
			settings.RuntimeData.ProcessedSpheres = GPUCollidersManager.processedSpheresBuffer;
		}
	}
}
