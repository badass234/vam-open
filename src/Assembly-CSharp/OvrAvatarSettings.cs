using UnityEngine;

public sealed class OvrAvatarSettings : ScriptableObject
{
	private static OvrAvatarSettings instance;

	[SerializeField]
	private string ovrAppID = string.Empty;

	[SerializeField]
	private string ovrGearAppID = string.Empty;

	public static string AppID
	{
		get
		{
			return Instance.ovrAppID;
		}
		set
		{
			Instance.ovrAppID = value;
		}
	}

	public static string GearAppID
	{
		get
		{
			return Instance.ovrGearAppID;
		}
		set
		{
			Instance.ovrGearAppID = value;
		}
	}

	public static OvrAvatarSettings Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.Load<OvrAvatarSettings>("OvrAvatarSettings");
				if (instance == null)
				{
					instance = ScriptableObject.CreateInstance<OvrAvatarSettings>();
				}
			}
			return instance;
		}
		set
		{
			instance = value;
		}
	}
}
