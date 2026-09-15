namespace Leap
{
	public struct BeginProfilingForThreadArgs
	{
		public string threadName;

		public string[] blockNames;

		public BeginProfilingForThreadArgs(string threadName, params string[] blockNames)
		{
			this.threadName = threadName;
			this.blockNames = blockNames;
		}
	}
}
