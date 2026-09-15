using System.Collections.Generic;

namespace GPUTools.Common.Scripts.Tools.Commands
{
	public class BuildChainCommand : IBuildCommand
	{
		private readonly List<IBuildCommand> commands = new List<IBuildCommand>();

		public void Add(IBuildCommand command)
		{
			commands.Add(command);
		}

		public void Build()
		{
			for (int i = 0; i < commands.Count; i++)
			{
				commands[i].Build();
			}
			OnBuild();
		}

		public void UpdateSettings()
		{
			for (int i = 0; i < commands.Count; i++)
			{
				commands[i].UpdateSettings();
			}
			OnUpdateSettings();
		}

		public virtual void Dispatch()
		{
			for (int i = 0; i < commands.Count; i++)
			{
				commands[i].Dispatch();
			}
			OnDispatch();
		}

		public virtual void FixedDispatch()
		{
			for (int i = 0; i < commands.Count; i++)
			{
				commands[i].FixedDispatch();
			}
			OnFixedDispatch();
		}

		public void Dispose()
		{
			for (int i = 0; i < commands.Count; i++)
			{
				commands[i].Dispose();
			}
			OnDispose();
		}

		protected virtual void OnBuild()
		{
		}

		protected virtual void OnUpdateSettings()
		{
		}

		protected virtual void OnDispatch()
		{
		}

		protected virtual void OnFixedDispatch()
		{
		}

		protected virtual void OnDispose()
		{
		}
	}
}
