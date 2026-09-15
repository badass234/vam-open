using GPUTools.Common.Scripts.Tools.Commands;

namespace GPUTools.Cloth.Scripts.Runtime.Commands
{
	public class BuildGrabSpheres : BuildChainCommand
	{
		private readonly ClothSettings settings;

		public BuildGrabSpheres(ClothSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			settings.Runtime.GrabSpheres = GPUCollidersManager.grabSpheresBuffer;
		}

		protected override void OnFixedDispatch()
		{
			settings.Runtime.GrabSpheres = GPUCollidersManager.grabSpheresBuffer;
		}
	}
}
