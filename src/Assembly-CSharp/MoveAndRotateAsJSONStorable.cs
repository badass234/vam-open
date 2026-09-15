using UnityEngine;

public class MoveAndRotateAsJSONStorable : JSONStorable
{
	public enum UpdateTime
	{
		Normal,
		Late,
		Fixed,
		Batch
	}

	public Transform moveAndRotateAsTransform;

	protected JSONStorableFloat xOffsetJSON;

	protected JSONStorableFloat yOffsetJSON;

	protected JSONStorableFloat zOffsetJSON;

	protected Vector3 offset;

	public bool moveAsEnabled = true;

	public bool rotateAsEnabled = true;

	public bool useRigidbody;

	public UpdateTime updateTime;

	protected void SyncXOffset(float f)
	{
		offset.x = f;
	}

	protected void SyncYOffset(float f)
	{
		offset.y = f;
	}

	protected void SyncZOffset(float f)
	{
		offset.z = f;
	}

	public void DoUpdate()
	{
		if (!moveAndRotateAsTransform)
		{
			return;
		}
		if (moveAsEnabled)
		{
			if (useRigidbody)
			{
				Rigidbody component = base.transform.GetComponent<Rigidbody>();
				if (component != null)
				{
					Vector3 vector = moveAndRotateAsTransform.right * offset.x + moveAndRotateAsTransform.up * offset.y + moveAndRotateAsTransform.forward * offset.z;
					component.MovePosition(moveAndRotateAsTransform.position + vector);
				}
			}
			else
			{
				base.transform.position = moveAndRotateAsTransform.position;
				base.transform.localPosition += offset;
			}
		}
		if (!rotateAsEnabled)
		{
			return;
		}
		if (useRigidbody)
		{
			Rigidbody component2 = base.transform.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				component2.MoveRotation(moveAndRotateAsTransform.rotation);
			}
		}
		else
		{
			base.transform.rotation = moveAndRotateAsTransform.rotation;
		}
	}

	protected void Init()
	{
		xOffsetJSON = new JSONStorableFloat("xOffset", 0f, SyncXOffset, -0.1f, 0.1f, false);
		RegisterFloat(xOffsetJSON);
		yOffsetJSON = new JSONStorableFloat("yOffset", 0f, SyncYOffset, -0.1f, 0.1f, false);
		RegisterFloat(yOffsetJSON);
		zOffsetJSON = new JSONStorableFloat("zOffset", 0f, SyncZOffset, -0.1f, 0.1f, false);
		RegisterFloat(zOffsetJSON);
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			MoveAndRotateAsJSONStorableUI componentInChildren = UITransform.GetComponentInChildren<MoveAndRotateAsJSONStorableUI>();
			if (componentInChildren != null)
			{
				xOffsetJSON.slider = componentInChildren.xOffsetSlider;
				yOffsetJSON.slider = componentInChildren.yOffsetSlider;
				zOffsetJSON.slider = componentInChildren.zOffsetSlider;
			}
		}
	}

	private void OnEnable()
	{
		DoUpdate();
	}

	private void Start()
	{
		DoUpdate();
	}

	private void FixedUpdate()
	{
		if (updateTime == UpdateTime.Fixed)
		{
			DoUpdate();
		}
	}

	private void Update()
	{
		if (updateTime == UpdateTime.Normal)
		{
			DoUpdate();
		}
	}

	private void LateUpdate()
	{
		if (updateTime == UpdateTime.Late)
		{
			DoUpdate();
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
		}
	}
}
