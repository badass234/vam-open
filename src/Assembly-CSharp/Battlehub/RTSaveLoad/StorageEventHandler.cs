namespace Battlehub.RTSaveLoad
{
	public delegate void StorageEventHandler<T>(StoragePayload<T> payload);
	public delegate void StorageEventHandler<T1, T2>(StoragePayload<T1, T2> payload);
}
