using UnityEngine;

public class AnimatedDAZBoneDirectDrives : JSONStorable
{
	[SerializeField]
	protected bool _on = true;

	protected JSONStorableBool onJSON;

	[HideInInspector]
	public AnimatedDAZBoneDirectDrive[] drives;

	protected void SyncOn(bool b)
	{
		_on = b;
		if (!_on)
		{
			AnimatedDAZBoneDirectDrive[] array = drives;
			foreach (AnimatedDAZBoneDirectDrive animatedDAZBoneDirectDrive in array)
			{
				animatedDAZBoneDirectDrive.RestoreBoneControl();
			}
		}
	}

	public void AutoSetDrives()
	{
		drives = GetComponentsInChildren<AnimatedDAZBoneDirectDrive>(true);
	}

	protected void Init()
	{
		onJSON = new JSONStorableBool("on", _on, SyncOn);
		onJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(onJSON);
		AnimatedDAZBoneDirectDrive[] array = drives;
		foreach (AnimatedDAZBoneDirectDrive animatedDAZBoneDirectDrive in array)
		{
			animatedDAZBoneDirectDrive.Init();
		}
	}

	protected void OnDisable()
	{
		AnimatedDAZBoneDirectDrive[] array = drives;
		foreach (AnimatedDAZBoneDirectDrive animatedDAZBoneDirectDrive in array)
		{
			animatedDAZBoneDirectDrive.RestoreBoneControl();
		}
	}

	protected void Start()
	{
		AnimatedDAZBoneDirectDrive[] array = drives;
		foreach (AnimatedDAZBoneDirectDrive animatedDAZBoneDirectDrive in array)
		{
			animatedDAZBoneDirectDrive.InitParent();
		}
	}

	protected void Update()
	{
		if (_on)
		{
			AnimatedDAZBoneDirectDrive[] array = drives;
			foreach (AnimatedDAZBoneDirectDrive animatedDAZBoneDirectDrive in array)
			{
				animatedDAZBoneDirectDrive.Prep();
				animatedDAZBoneDirectDrive.ThreadedUpdate();
				animatedDAZBoneDirectDrive.Finish();
			}
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		Awake();
		if (t != null)
		{
			AnimatedDAZBoneDirectDrivesUI componentInChildren = t.GetComponentInChildren<AnimatedDAZBoneDirectDrivesUI>(true);
			if (componentInChildren != null)
			{
				onJSON.RegisterToggle(componentInChildren.onToggle, isAlt);
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}
}
