using GPUTools.Common.Scripts.Tools.Commands;

namespace GPUTools.Hair.Scripts.Runtime.Commands.Physics
{
	public class BuildEditLineSpheres : BuildChainCommand
	{
		private readonly HairSettings settings;

		public BuildEditLineSpheres(HairSettings settings)
		{
			this.settings = settings;
		}

		protected override void OnBuild()
		{
			settings.RuntimeData.CutLineSpheres = GPUCollidersManager.cutLineSpheresBuffer;
			settings.RuntimeData.GrowLineSpheres = GPUCollidersManager.growLineSpheresBuffer;
			settings.RuntimeData.HoldLineSpheres = GPUCollidersManager.holdLineSpheresBuffer;
			settings.RuntimeData.GrabLineSpheres = GPUCollidersManager.grabLineSpheresBuffer;
			settings.RuntimeData.PushLineSpheres = GPUCollidersManager.pushLineSpheresBuffer;
			settings.RuntimeData.PullLineSpheres = GPUCollidersManager.pullLineSpheresBuffer;
			settings.RuntimeData.BrushLineSpheres = GPUCollidersManager.brushLineSpheresBuffer;
			settings.RuntimeData.RigidityIncreaseLineSpheres = GPUCollidersManager.rigidityIncreaseLineSpheresBuffer;
			settings.RuntimeData.RigidityDecreaseLineSpheres = GPUCollidersManager.rigidityDecreaseLineSpheresBuffer;
			settings.RuntimeData.RigiditySetLineSpheres = GPUCollidersManager.rigiditySetLineSpheresBuffer;
		}

		protected override void OnFixedDispatch()
		{
			settings.RuntimeData.CutLineSpheres = GPUCollidersManager.cutLineSpheresBuffer;
			settings.RuntimeData.GrowLineSpheres = GPUCollidersManager.growLineSpheresBuffer;
			settings.RuntimeData.HoldLineSpheres = GPUCollidersManager.holdLineSpheresBuffer;
			settings.RuntimeData.GrabLineSpheres = GPUCollidersManager.grabLineSpheresBuffer;
			settings.RuntimeData.PushLineSpheres = GPUCollidersManager.pushLineSpheresBuffer;
			settings.RuntimeData.PullLineSpheres = GPUCollidersManager.pullLineSpheresBuffer;
			settings.RuntimeData.BrushLineSpheres = GPUCollidersManager.brushLineSpheresBuffer;
			settings.RuntimeData.RigidityIncreaseLineSpheres = GPUCollidersManager.rigidityIncreaseLineSpheresBuffer;
			settings.RuntimeData.RigidityDecreaseLineSpheres = GPUCollidersManager.rigidityDecreaseLineSpheresBuffer;
			settings.RuntimeData.RigiditySetLineSpheres = GPUCollidersManager.rigiditySetLineSpheresBuffer;
		}
	}
}
