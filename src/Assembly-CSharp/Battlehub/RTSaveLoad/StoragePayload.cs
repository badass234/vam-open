namespace Battlehub.RTSaveLoad
{
	public class StoragePayload<T>
	{
		public T Path { get; private set; }

		public StoragePayload(T path)
		{
			Path = path;
		}
	}
	public class StoragePayload<T1, T2> : StoragePayload<T1>
	{
		public T2 Data { get; private set; }

		public StoragePayload(T1 path, T2 data)
			: base(path)
		{
			Data = data;
		}
	}
}
