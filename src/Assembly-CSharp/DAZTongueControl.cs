using UnityEngine;

public class DAZTongueControl : PhysicsSimulatorJSONStorable
{
	public Transform[] tongueCollisionAutoGroups;

	public Rigidbody[] tongueCollisionRigidbodies;

	protected JSONStorableBool tongueCollisionJSON;

	[SerializeField]
	public bool _tongueCollision = true;

	public bool tongueCollision
	{
		get
		{
			return _tongueCollision;
		}
		set
		{
			if (tongueCollisionJSON != null)
			{
				tongueCollisionJSON.val = value;
			}
			else if (_tongueCollision != value)
			{
				SyncTongueCollision(value);
			}
		}
	}

	protected override void SyncCollisionEnabled()
	{
		base.SyncCollisionEnabled();
		SyncTongueCollision();
	}

	protected override void SyncUseInterpolation()
	{
		base.SyncUseInterpolation();
		if (tongueCollisionRigidbodies != null)
		{
			Rigidbody[] array = tongueCollisionRigidbodies;
			foreach (Rigidbody rigidbody in array)
			{
				if (_useInterpolation)
				{
					rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				}
				else
				{
					rigidbody.interpolation = RigidbodyInterpolation.None;
				}
			}
		}
		if (tongueCollisionAutoGroups == null)
		{
			return;
		}
		Transform[] array2 = tongueCollisionAutoGroups;
		foreach (Transform transform in array2)
		{
			Rigidbody[] componentsInChildren = transform.GetComponentsInChildren<Rigidbody>();
			Rigidbody[] array3 = componentsInChildren;
			foreach (Rigidbody rigidbody2 in array3)
			{
				if (_useInterpolation)
				{
					rigidbody2.interpolation = RigidbodyInterpolation.Interpolate;
				}
				else
				{
					rigidbody2.interpolation = RigidbodyInterpolation.None;
				}
			}
		}
	}

	protected void SyncTongueCollision()
	{
		if (tongueCollisionRigidbodies != null)
		{
			Rigidbody[] array = tongueCollisionRigidbodies;
			foreach (Rigidbody rigidbody in array)
			{
				rigidbody.detectCollisions = _tongueCollision && !_resetSimulation;
			}
		}
		if (tongueCollisionAutoGroups == null)
		{
			return;
		}
		Transform[] array2 = tongueCollisionAutoGroups;
		foreach (Transform transform in array2)
		{
			Rigidbody[] componentsInChildren = transform.GetComponentsInChildren<Rigidbody>();
			Rigidbody[] array3 = componentsInChildren;
			foreach (Rigidbody rigidbody2 in array3)
			{
				rigidbody2.detectCollisions = _tongueCollision && !_resetSimulation;
			}
		}
	}

	protected void SyncTongueCollision(bool b)
	{
		_tongueCollision = b;
		SyncTongueCollision();
	}

	protected void Init()
	{
		tongueCollisionJSON = new JSONStorableBool("tongueCollision", _tongueCollision, SyncTongueCollision);
		tongueCollisionJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(tongueCollisionJSON);
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			DAZTongueControlUI componentInChildren = t.GetComponentInChildren<DAZTongueControlUI>(true);
			if (componentInChildren != null)
			{
				tongueCollisionJSON.RegisterToggle(componentInChildren.tongueCollisionToggle, isAlt);
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
