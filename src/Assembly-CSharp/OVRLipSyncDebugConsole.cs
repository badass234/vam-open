using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OVRLipSyncDebugConsole : MonoBehaviour
{
	public ArrayList messages = new ArrayList();

	public int maxMessages = 15;

	public Text textMsg;

	private static OVRLipSyncDebugConsole s_Instance;

	private bool clearTimeoutOn;

	private float clearTimeout;

	public static OVRLipSyncDebugConsole instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = Object.FindObjectOfType(typeof(OVRLipSyncDebugConsole)) as OVRLipSyncDebugConsole;
				if (s_Instance == null)
				{
					GameObject gameObject = new GameObject();
					gameObject.AddComponent<OVRLipSyncDebugConsole>();
					gameObject.name = "OVRLipSyncDebugConsole";
					s_Instance = Object.FindObjectOfType(typeof(OVRLipSyncDebugConsole)) as OVRLipSyncDebugConsole;
				}
			}
			return s_Instance;
		}
	}

	private void Awake()
	{
		s_Instance = this;
		Init();
	}

	private void Update()
	{
		if (clearTimeoutOn)
		{
			clearTimeout -= Time.deltaTime;
			if (clearTimeout < 0f)
			{
				Clear();
				clearTimeout = 0f;
				clearTimeoutOn = false;
			}
		}
	}

	public void Init()
	{
		if (textMsg == null)
		{
			Debug.LogWarning("DebugConsole Init WARNING::UI text not set. Will not be able to display anything.");
		}
		Clear();
	}

	public static void Log(string message)
	{
		instance.AddMessage(message, Color.white);
	}

	public static void Log(string message, Color color)
	{
		instance.AddMessage(message, color);
	}

	public static void Clear()
	{
		instance.ClearMessages();
	}

	public static void ClearTimeout(float timeToClear)
	{
		instance.SetClearTimeout(timeToClear);
	}

	public void AddMessage(string message, Color color)
	{
		messages.Add(message);
		if (textMsg != null)
		{
			textMsg.color = color;
		}
		Display();
	}

	public void ClearMessages()
	{
		messages.Clear();
		Display();
	}

	public void SetClearTimeout(float timeout)
	{
		clearTimeout = timeout;
		clearTimeoutOn = true;
	}

	private void Prune()
	{
		if (messages.Count > maxMessages)
		{
			int count = ((messages.Count > 0) ? (messages.Count - maxMessages) : 0);
			messages.RemoveRange(0, count);
		}
	}

	private void Display()
	{
		if (messages.Count > maxMessages)
		{
			Prune();
		}
		if (textMsg != null)
		{
			textMsg.text = string.Empty;
			for (int i = 0; i < messages.Count; i++)
			{
				textMsg.text += (string)messages[i];
				textMsg.text += '\n';
			}
		}
	}
}
