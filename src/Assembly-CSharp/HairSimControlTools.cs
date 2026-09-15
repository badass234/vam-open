using System.Collections.Generic;
using UnityEngine;

public class HairSimControlTools : MonoBehaviour
{
	public Transform hairStyleTool1;

	public HairSimStyleToolControl StyleTool1;

	public FreeControllerV3 hairStyleTool1Controller;

	public FreeControllerV3 hairStyleTool1UIController;

	public Transform hairStyleTool2;

	public HairSimStyleToolControl StyleTool2;

	public FreeControllerV3 hairStyleTool2Controller;

	public FreeControllerV3 hairStyleTool2UIController;

	public Transform hairStyleTool3;

	public HairSimStyleToolControl StyleTool3;

	public FreeControllerV3 hairStyleTool3Controller;

	public FreeControllerV3 hairStyleTool3UIController;

	public Transform hairStyleTool4;

	public HairSimStyleToolControl StyleTool4;

	public FreeControllerV3 hairStyleTool4Controller;

	public FreeControllerV3 hairStyleTool4UIController;

	public Transform hairScalpMaskTool;

	public FreeControllerV3 hairScalpMaskToolController;

	public FreeControllerV3 hairScalpMaskToolUIController;

	protected HashSet<FreeControllerV3> controllers;

	public void SetOnlyToolsControllable(bool b)
	{
		if (SuperController.singleton != null)
		{
			if (b)
			{
				SuperController.singleton.SetOnlyShowControllers(controllers);
			}
			else
			{
				SuperController.singleton.SetOnlyShowControllers(null);
			}
		}
	}

	public void SetHairStyleToolVisibility(bool tool1Visibility, bool tool2Visibility, bool tool3Visibility, bool tool4Visibility)
	{
		SetHairStyleTool1Visible(tool1Visibility);
		SetHairStyleTool2Visible(tool2Visibility);
		SetHairStyleTool3Visible(tool3Visibility);
		SetHairStyleTool4Visible(tool4Visibility);
	}

	public void SetHairStyleTool1Visible(bool b)
	{
		if (hairStyleTool1 != null)
		{
			hairStyleTool1.gameObject.SetActive(b);
		}
	}

	public void SetHairStyleTool2Visible(bool b)
	{
		if (hairStyleTool2 != null)
		{
			hairStyleTool2.gameObject.SetActive(b);
		}
	}

	public void SetHairStyleTool3Visible(bool b)
	{
		if (hairStyleTool3 != null)
		{
			hairStyleTool3.gameObject.SetActive(b);
		}
	}

	public void SetHairStyleTool4Visible(bool b)
	{
		if (hairStyleTool4 != null)
		{
			hairStyleTool4.gameObject.SetActive(b);
		}
	}

	public void SetScalpMaskToolVisible(bool b)
	{
		if (hairScalpMaskTool != null)
		{
			hairScalpMaskTool.gameObject.SetActive(b);
		}
	}

	public void SetAllowRigidityPaint(bool b)
	{
		if (StyleTool1 != null)
		{
			StyleTool1.allowRigidityPaint = b;
		}
		if (StyleTool2 != null)
		{
			StyleTool2.allowRigidityPaint = b;
		}
		if (StyleTool3 != null)
		{
			StyleTool3.allowRigidityPaint = b;
		}
		if (StyleTool4 != null)
		{
			StyleTool4.allowRigidityPaint = b;
		}
	}

	private void Awake()
	{
		SetHairStyleToolVisibility(false, false, false, false);
		SetScalpMaskToolVisible(false);
		controllers = new HashSet<FreeControllerV3>();
		if (hairStyleTool1Controller != null)
		{
			controllers.Add(hairStyleTool1Controller);
		}
		if (hairStyleTool1UIController != null)
		{
			controllers.Add(hairStyleTool1UIController);
		}
		if (hairStyleTool2Controller != null)
		{
			controllers.Add(hairStyleTool2Controller);
		}
		if (hairStyleTool2UIController != null)
		{
			controllers.Add(hairStyleTool2UIController);
		}
		if (hairStyleTool3Controller != null)
		{
			controllers.Add(hairStyleTool3Controller);
		}
		if (hairStyleTool3UIController != null)
		{
			controllers.Add(hairStyleTool3UIController);
		}
		if (hairStyleTool4Controller != null)
		{
			controllers.Add(hairStyleTool4Controller);
		}
		if (hairStyleTool4UIController != null)
		{
			controllers.Add(hairStyleTool4UIController);
		}
		if (hairScalpMaskToolController != null)
		{
			controllers.Add(hairScalpMaskToolController);
		}
		if (hairScalpMaskToolUIController != null)
		{
			controllers.Add(hairScalpMaskToolUIController);
		}
	}
}
