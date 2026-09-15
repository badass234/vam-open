using UnityEngine;

public class CapsuleToolControl : JSONStorable
{
	public CapsuleCollider toolCollider;

	public Transform cylinderMesh;

	public Transform sphere1Mesh;

	public Transform sphere2Mesh;

	public float quickSize1Radius = 0.25f;

	public float quickSize1Length = 3f;

	public float quickSize2Radius = 0.5f;

	public float quickSize2Length = 3f;

	public float quickSize3Radius = 1f;

	public float quickSize3Length = 6f;

	public float quickSize4Radius = 2f;

	public float quickSize4Length = 8f;

	protected JSONStorableFloat radiusJSON;

	protected JSONStorableFloat lengthJSON;

	protected JSONStorableAction SetQuickSize1Action;

	protected JSONStorableAction SetQuickSize2Action;

	protected JSONStorableAction SetQuickSize3Action;

	protected JSONStorableAction SetQuickSize4Action;

	protected bool wasInit;

	protected void SyncMesh()
	{
		float val = lengthJSON.val;
		float val2 = radiusJSON.val;
		float num = val2 * 2f;
		float num2 = val2 * 4f;
		float num3 = val / 2f;
		float num4 = Mathf.Max(num3 - val2, 0f);
		if (cylinderMesh != null)
		{
			Vector3 localPosition = default(Vector3);
			localPosition.x = 0f;
			localPosition.y = num4;
			localPosition.z = 0f;
			cylinderMesh.localPosition = localPosition;
			Vector3 localScale = default(Vector3);
			localScale.x = num2;
			localScale.y = num2;
			localScale.z = num4 * 4f;
			cylinderMesh.localScale = localScale;
		}
		if (sphere1Mesh != null && sphere2Mesh != null)
		{
			Vector3 vector = default(Vector3);
			vector.x = 0f;
			vector.y = num4;
			vector.z = 0f;
			sphere1Mesh.localPosition = vector;
			sphere2Mesh.localPosition = -vector;
			Vector3 localScale2 = default(Vector3);
			localScale2.x = num;
			localScale2.y = num;
			localScale2.z = num;
			sphere1Mesh.localScale = localScale2;
			sphere2Mesh.localScale = localScale2;
		}
	}

	protected void SyncRadius(float f)
	{
		if (toolCollider != null)
		{
			toolCollider.radius = f;
		}
		SyncMesh();
	}

	protected void SyncLength(float f)
	{
		if (toolCollider != null)
		{
			toolCollider.height = f;
		}
		SyncMesh();
	}

	protected void SetQuickSize1()
	{
		radiusJSON.val = quickSize1Radius;
		lengthJSON.val = quickSize1Length;
	}

	protected void SetQuickSize2()
	{
		radiusJSON.val = quickSize2Radius;
		lengthJSON.val = quickSize2Length;
	}

	protected void SetQuickSize3()
	{
		radiusJSON.val = quickSize3Radius;
		lengthJSON.val = quickSize3Length;
	}

	protected void SetQuickSize4()
	{
		radiusJSON.val = quickSize4Radius;
		lengthJSON.val = quickSize4Length;
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (!wasInit)
		{
			return;
		}
		base.InitUI(t, isAlt);
		if (t != null)
		{
			CapsuleToolControlUI componentInChildren = UITransform.GetComponentInChildren<CapsuleToolControlUI>();
			if (componentInChildren != null)
			{
				radiusJSON.RegisterSlider(componentInChildren.radiusSlider, isAlt);
				lengthJSON.RegisterSlider(componentInChildren.lengthSlider, isAlt);
				SetQuickSize1Action.RegisterButton(componentInChildren.setQuickSize1Button, isAlt);
				SetQuickSize2Action.RegisterButton(componentInChildren.setQuickSize2Button, isAlt);
				SetQuickSize3Action.RegisterButton(componentInChildren.setQuickSize3Button, isAlt);
				SetQuickSize4Action.RegisterButton(componentInChildren.setQuickSize4Button, isAlt);
			}
		}
	}

	protected virtual void Init()
	{
		wasInit = true;
		radiusJSON = new JSONStorableFloat("radius", toolCollider.radius, SyncRadius, 0.01f, 10f);
		RegisterFloat(radiusJSON);
		lengthJSON = new JSONStorableFloat("length", toolCollider.height, SyncLength, 0.01f, 20f);
		RegisterFloat(lengthJSON);
		SetQuickSize1Action = new JSONStorableAction("SetQuickSize1", SetQuickSize1);
		RegisterAction(SetQuickSize1Action);
		SetQuickSize2Action = new JSONStorableAction("SetQuickSize2", SetQuickSize2);
		RegisterAction(SetQuickSize2Action);
		SetQuickSize3Action = new JSONStorableAction("SetQuickSize3", SetQuickSize3);
		RegisterAction(SetQuickSize3Action);
		SetQuickSize4Action = new JSONStorableAction("SetQuickSize4", SetQuickSize4);
		RegisterAction(SetQuickSize4Action);
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
