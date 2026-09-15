namespace Battlehub.RTSaveLoad
{
	public delegate void ProjectEventHandler<T>(ProjectPayload<T> payload);
	public delegate void ProjectEventHandler(ProjectPayload payload);
}
