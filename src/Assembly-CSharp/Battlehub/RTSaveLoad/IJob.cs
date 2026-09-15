using System;

namespace Battlehub.RTSaveLoad
{
	public interface IJob
	{
		void Submit(Action<Action> job, Action completed);
	}
}
