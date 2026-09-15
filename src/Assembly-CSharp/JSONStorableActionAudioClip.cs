public class JSONStorableActionAudioClip
{
	public delegate void AudioClipActionCallback(NamedAudioClip nac);

	public string name;

	public AudioClipActionCallback actionCallback;

	public JSONStorable storable;

	public JSONStorableActionAudioClip(string n, AudioClipActionCallback callback)
	{
		name = n;
		actionCallback = callback;
	}
}
