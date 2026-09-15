public class JSONStorableActionSceneFilePath
{
	public delegate void SceneFilePathActionCallback(string path);

	public string name;

	public SceneFilePathActionCallback actionCallback;

	public JSONStorable storable;

	public JSONStorableActionSceneFilePath(string n, SceneFilePathActionCallback callback)
	{
		name = n;
		actionCallback = callback;
	}
}
