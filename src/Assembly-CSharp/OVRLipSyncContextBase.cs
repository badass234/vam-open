using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OVRLipSyncContextBase : MonoBehaviour
{
	public AudioSource audioSource;

	public OVRLipSync.ContextProviders provider;

	private OVRLipSync.Frame frame = new OVRLipSync.Frame();

	private uint context;

	public int Smoothing
	{
		set
		{
			OVRLipSync.SendSignal(context, OVRLipSync.Signals.VisemeSmoothing, value, 0);
		}
	}

	public uint Context => context;

	protected OVRLipSync.Frame Frame => frame;

	private void Awake()
	{
		if (!audioSource)
		{
			audioSource = GetComponent<AudioSource>();
		}
		lock (this)
		{
			if (context == 0 && OVRLipSync.CreateContext(ref context, provider) != 0)
			{
				Debug.Log("OVRPhonemeContext.Start ERROR: Could not create Phoneme context.");
			}
		}
	}

	private void OnDestroy()
	{
		lock (this)
		{
			if (context != 0 && OVRLipSync.DestroyContext(context) != 0)
			{
				Debug.Log("OVRPhonemeContext.OnDestroy ERROR: Could not delete Phoneme context.");
			}
		}
	}

	public OVRLipSync.Frame GetCurrentPhonemeFrame()
	{
		return frame;
	}

	public void SetVisemeBlend(int viseme, int amount)
	{
		OVRLipSync.SendSignal(context, OVRLipSync.Signals.VisemeAmount, viseme, amount);
	}

	public OVRLipSync.Result ResetContext()
	{
		return OVRLipSync.ResetContext(context);
	}
}
