using GPUTools.Common.Scripts.Tools.Commands;

namespace GPUTools.Cloth.Scripts.Runtime.Commands
{
	public class BuildLineSpheres : BuildChainCommand
	{
		private readonly ClothSettings settings;

		public BuildLineSpheres(ClothSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			settings.Runtime.ProcessedLineSpheres = GPUCollidersManager.processedLineSpheresBuffer;
		}

		protected override void OnFixedDispatch()
		{
			settings.Runtime.ProcessedLineSpheres = GPUCollidersManager.processedLineSpheresBuffer;
		}
	}
}
