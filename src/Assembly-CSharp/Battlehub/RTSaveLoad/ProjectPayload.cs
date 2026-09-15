namespace Battlehub.RTSaveLoad
{
	public class ProjectPayload
	{
		public bool HasError;
	}
	public class ProjectPayload<T> : ProjectPayload
	{
		public T Data { get; private set; }

		public ProjectPayload(T data)
		{
			Data = data;
		}
	}
}
