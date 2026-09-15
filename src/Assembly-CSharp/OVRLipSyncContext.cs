using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OVRLipSyncContext : OVRLipSyncContextBase
{
	public float gain = 1f;

	public bool audioMute = true;

	public KeyCode loopback = KeyCode.L;

	public KeyCode debugVisemes = KeyCode.D;

	public bool showVisemes;

	public bool delayCompensate;

	private OVRLipSync.Frame debugFrame = new OVRLipSync.Frame();

	private float debugFrameTimer;

	private float debugFrameTimeoutValue = 0.1f;

	private void Start()
	{
		OVRMessenger.AddListener<OVRTouchpad.TouchEvent>("Touchpad", LocalTouchEventCallback);
	}

	private void Update()
	{
		if (Input.GetKeyDown(loopback))
		{
			audioMute = !audioMute;
			OVRLipSyncDebugConsole.Clear();
			OVRLipSyncDebugConsole.ClearTimeout(1.5f);
			if (audioMute)
			{
				OVRLipSyncDebugConsole.Log("LOOPBACK MODE: ENABLED");
			}
			else
			{
				OVRLipSyncDebugConsole.Log("LOOPBACK MODE: DISABLED");
			}
		}
		else if (Input.GetKeyDown(debugVisemes))
		{
			showVisemes = !showVisemes;
			if (showVisemes)
			{
				Debug.Log("DEBUG SHOW VISEMES: ENABLED");
			}
			else
			{
				OVRLipSyncDebugConsole.Clear();
				Debug.Log("DEBUG SHOW VISEMES: DISABLED");
			}
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			gain -= 1f;
			if (gain < 1f)
			{
				gain = 1f;
			}
			string text = "LINEAR GAIN: ";
			text += gain;
			OVRLipSyncDebugConsole.Clear();
			OVRLipSyncDebugConsole.Log(text);
			OVRLipSyncDebugConsole.ClearTimeout(1.5f);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			gain += 1f;
			if (gain > 15f)
			{
				gain = 15f;
			}
			string text2 = "LINEAR GAIN: ";
			text2 += gain;
			OVRLipSyncDebugConsole.Clear();
			OVRLipSyncDebugConsole.Log(text2);
			OVRLipSyncDebugConsole.ClearTimeout(1.5f);
		}
		DebugShowVisemes();
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (OVRLipSync.IsInitialized() != 0 || audioSource == null)
		{
			return;
		}
		for (int i = 0; i < data.Length; i++)
		{
			data[i] *= gain;
		}
		lock (this)
		{
			if (base.Context != 0)
			{
				OVRLipSync.Flags flags = OVRLipSync.Flags.None;
				if (delayCompensate)
				{
					flags |= OVRLipSync.Flags.DelayCompensateAudio;
				}
				OVRLipSync.Frame frame = base.Frame;
				OVRLipSync.ProcessFrameInterleaved(base.Context, data, flags, frame);
			}
		}
		if (audioMute)
		{
			for (int j = 0; j < data.Length; j++)
			{
				data[j] *= 0f;
			}
		}
	}

	private void DebugShowVisemes()
	{
		if (!showVisemes)
		{
			return;
		}
		debugFrameTimer -= Time.deltaTime;
		if (debugFrameTimer < 0f)
		{
			debugFrameTimer += debugFrameTimeoutValue;
			debugFrame.CopyInput(base.Frame);
		}
		string text = string.Empty;
		for (int i = 0; i < debugFrame.Visemes.Length; i++)
		{
			if (i < 10)
			{
				text += "0";
			}
			text += i;
			text += ":";
			int num = (int)(50f * debugFrame.Visemes[i]);
			for (int j = 0; j < num; j++)
			{
				text += "*";
			}
			text += "\n";
		}
		OVRLipSyncDebugConsole.Clear();
		OVRLipSyncDebugConsole.Log(text);
	}

	private void LocalTouchEventCallback(OVRTouchpad.TouchEvent touchEvent)
	{
		string text = "LINEAR GAIN: ";
		switch (touchEvent)
		{
		case OVRTouchpad.TouchEvent.SingleTap:
			audioMute = !audioMute;
			OVRLipSyncDebugConsole.Clear();
			OVRLipSyncDebugConsole.ClearTimeout(1.5f);
			if (audioMute)
			{
				OVRLipSyncDebugConsole.Log("LOOPBACK MODE: ENABLED");
			}
			else
			{
				OVRLipSyncDebugConsole.Log("LOOPBACK MODE: DISABLED");
			}
			break;
		case OVRTouchpad.TouchEvent.Up:
			gain += 1f;
			if (gain > 15f)
			{
				gain = 15f;
			}
			text += gain;
			OVRLipSyncDebugConsole.Clear();
			OVRLipSyncDebugConsole.Log(text);
			OVRLipSyncDebugConsole.ClearTimeout(1.5f);
			break;
		case OVRTouchpad.TouchEvent.Down:
			gain -= 1f;
			if (gain < 1f)
			{
				gain = 1f;
			}
			text += gain;
			OVRLipSyncDebugConsole.Clear();
			OVRLipSyncDebugConsole.Log(text);
			OVRLipSyncDebugConsole.ClearTimeout(1.5f);
			break;
		}
	}
}
