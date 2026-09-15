using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Hair.Scripts.Runtime.Render;

namespace GPUTools.Hair.Scripts.Runtime.Commands.Render
{
	public class BuildTesselation : IBuildCommand
	{
		private readonly HairSettings settings;

		public BuildTesselation(HairSettings settings)
		{
			this.settings = settings;
		}

		public void Build()
		{
			int num = settings.StandsSettings.Provider.GetStandsNum() * 64;
			if (settings.RuntimeData.TessRenderParticles != null)
			{
				settings.RuntimeData.TessRenderParticles.Dispose();
			}
			if (num > 0)
			{
				settings.RuntimeData.TessRenderParticles = new GpuBuffer<TessRenderParticle>(num, TessRenderParticle.Size());
			}
			else
			{
				settings.RuntimeData.TessRenderParticles = null;
			}
		}

		public void Dispatch()
		{
		}

		public void FixedDispatch()
		{
		}

		public void UpdateSettings()
		{
		}

		public void Dispose()
		{
			if (settings.RuntimeData.TessRenderParticles != null)
			{
				settings.RuntimeData.TessRenderParticles.Dispose();
			}
		}
	}
}
