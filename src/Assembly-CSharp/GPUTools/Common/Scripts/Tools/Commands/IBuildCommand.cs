namespace GPUTools.Common.Scripts.Tools.Commands
{
	public interface IBuildCommand
	{
		void Build();

		void Dispatch();

		void FixedDispatch();

		void UpdateSettings();

		void Dispose();
	}
}
