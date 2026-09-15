using GPUTools.Common.Scripts.Tools.Commands;

namespace GPUTools.Hair.Scripts.Runtime.Commands.Physics
{
	public class BuildLineSpheres : BuildChainCommand
	{
		private readonly HairSettings settings;

		public BuildLineSpheres(HairSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			settings.RuntimeData.ProcessedLineSpheres = GPUCollidersManager.processedLineSpheresBuffer;
		}

		protected override void OnFixedDispatch()
		{
			settings.RuntimeData.ProcessedLineSpheres = GPUCollidersManager.processedLineSpheresBuffer;
		}
	}
}
