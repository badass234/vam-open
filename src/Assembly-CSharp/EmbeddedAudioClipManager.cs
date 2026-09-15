public class EmbeddedAudioClipManager : AudioClipManager
{
	public static EmbeddedAudioClipManager singleton;

	public string clipsFile;

	public NamedAudioClip[] embeddedClips;

	protected override void Init()
	{
		base.Init();
		singleton = this;
		NamedAudioClip[] array = embeddedClips;
		foreach (NamedAudioClip nac in array)
		{
			AddClip(nac);
		}
	}
}
