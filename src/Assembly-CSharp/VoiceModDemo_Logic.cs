using UnityEngine;

public class VoiceModDemo_Logic : MonoBehaviour
{
	public OVRVoiceModContext[] contexts;

	public Material material;

	public Transform[] xfrms;

	public EnableSwitch SwitchTarget;

	private int targetSet;

	private Vector3 scale = new Vector3(3f, 3f, 3f);

	private float scaleMax = 10f;

	private int currentPreset;

	private void Start()
	{
		OVRMessenger.AddListener<OVRTouchpad.TouchEvent>("Touchpad", LocalTouchEventCallback);
		targetSet = 0;
		SwitchTarget.SetActive(0);
		if (material != null)
		{
			material.SetColor("_Color", Color.grey);
		}
	}

	private void Update()
	{
		int num = -1;
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			num = 0;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			num = 1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			num = 2;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			num = 3;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			num = 4;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			num = 5;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			num = 6;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			num = 7;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			num = 8;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			num = 9;
		}
		if (num != -1)
		{
			Color value = Color.black;
			for (int i = 0; i < contexts.Length; i++)
			{
				if (contexts[i].SetPreset(num))
				{
					value = contexts[i].GetPresetColor(num);
				}
			}
			if (material != null)
			{
				material.SetColor("_Color", value);
			}
		}
		UpdateModelScale();
		if (Input.GetKeyDown(KeyCode.Z))
		{
			targetSet = 0;
			SetCurrentTarget();
		}
		else if (Input.GetKeyDown(KeyCode.X))
		{
			targetSet = 1;
			SetCurrentTarget();
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	private void SetCurrentTarget()
	{
		switch (targetSet)
		{
		case 0:
			SwitchTarget.SetActive(0);
			OVRDebugConsole.Clear();
			OVRDebugConsole.Log("MICROPHONE INPUT");
			OVRDebugConsole.ClearTimeout(1.5f);
			break;
		case 1:
			SwitchTarget.SetActive(1);
			OVRDebugConsole.Clear();
			OVRDebugConsole.Log("SAMPLE INPUT");
			OVRDebugConsole.ClearTimeout(1.5f);
			break;
		}
	}

	private void LocalTouchEventCallback(OVRTouchpad.TouchEvent touchEvent)
	{
		switch (touchEvent)
		{
		case OVRTouchpad.TouchEvent.Left:
			targetSet--;
			if (targetSet < 0)
			{
				targetSet = 1;
			}
			SetCurrentTarget();
			break;
		case OVRTouchpad.TouchEvent.Right:
			targetSet++;
			if (targetSet > 1)
			{
				targetSet = 0;
			}
			SetCurrentTarget();
			break;
		case OVRTouchpad.TouchEvent.Up:
		{
			if (contexts.Length == 0)
			{
				break;
			}
			if (contexts[0].GetNumPresets() == 0)
			{
				OVRDebugConsole.Clear();
				OVRDebugConsole.Log("NO PRESETS!");
				OVRDebugConsole.ClearTimeout(1.5f);
				break;
			}
			currentPreset++;
			if (currentPreset >= contexts[0].GetNumPresets())
			{
				currentPreset = 0;
			}
			Color value2 = Color.black;
			for (int j = 0; j < contexts.Length; j++)
			{
				if (contexts[j].SetPreset(currentPreset))
				{
					value2 = contexts[j].GetPresetColor(currentPreset);
				}
			}
			if (material != null)
			{
				material.SetColor("_Color", value2);
			}
			break;
		}
		case OVRTouchpad.TouchEvent.Down:
		{
			if (contexts.Length == 0)
			{
				break;
			}
			if (contexts[0].GetNumPresets() == 0)
			{
				OVRDebugConsole.Clear();
				OVRDebugConsole.Log("NO PRESETS!");
				OVRDebugConsole.ClearTimeout(1.5f);
				break;
			}
			currentPreset--;
			if (currentPreset < 0)
			{
				currentPreset = contexts[0].GetNumPresets() - 1;
			}
			Color value = Color.black;
			for (int i = 0; i < contexts.Length; i++)
			{
				if (contexts[i].SetPreset(currentPreset))
				{
					value = contexts[i].GetPresetColor(currentPreset);
				}
			}
			if (material != null)
			{
				material.SetColor("_Color", value);
			}
			break;
		}
		}
	}

	private void UpdateModelScale()
	{
		for (int i = 0; i < xfrms.Length; i++)
		{
			if (i < contexts.Length)
			{
				xfrms[i].localScale = scale * (1f + contexts[i].GetAverageAbsVolume() * scaleMax);
			}
		}
	}
}
