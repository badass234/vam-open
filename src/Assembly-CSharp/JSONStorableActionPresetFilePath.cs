public class JSONStorableActionPresetFilePath
{
	public delegate void PresetFilePathActionCallback(string path);

	protected JSONStorableUrl url;

	public string name;

	public PresetFilePathActionCallback actionCallback;

	public JSONStorable storable;

	public JSONStorableActionPresetFilePath(string n, PresetFilePathActionCallback callback, JSONStorableUrl urlJSON)
	{
		name = n;
		actionCallback = callback;
		url = urlJSON;
	}

	public void Browse(JSONStorableString.SetStringCallback callback)
	{
		if (url != null)
		{
			url.setCallbackFunction = callback;
			url.FileBrowse();
		}
	}
}
