using System.Collections.Generic;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;

public class MVRScript : JSONStorable
{
	public MVRPluginManager manager;

	protected Dictionary<UIDynamicSlider, JSONStorableFloat> sliderToJSONStorableFloat;

	protected Dictionary<UIDynamicToggle, JSONStorableBool> toggleToJSONStorableBool;

	protected Dictionary<UIDynamicColorPicker, JSONStorableColor> colorPickerToJSONStorableColor;

	protected Dictionary<UIDynamicTextField, JSONStorableString> textFieldToJSONStorableString;

	protected Dictionary<UIDynamicPopup, JSONStorableStringChooser> popupToJSONStorableStringChooser;

	protected List<Transform> rightUIElements;

	protected List<Transform> leftUIElements;

	protected RectTransform rightUIContent;

	protected RectTransform leftUIContent;

	public JSONStorableBool enabledJSON;

	public JSONStorableString pluginLabelJSON;

	public virtual bool ShouldIgnore()
	{
		return false;
	}

	public Atom GetContainingAtom()
	{
		return containingAtom;
	}

	public List<Atom> GetSceneAtoms()
	{
		List<Atom> result = null;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetAtoms();
		}
		return result;
	}

	public List<string> GetAtomUIDs()
	{
		List<string> result = null;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetAtomUIDs();
		}
		return result;
	}

	public List<string> GetVisibleAtomUIDs()
	{
		List<string> result = null;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetVisibleAtomUIDs();
		}
		return result;
	}

	public Atom GetAtomById(string uid)
	{
		if (SuperController.singleton != null)
		{
			return SuperController.singleton.GetAtomByUid(uid);
		}
		return null;
	}

	public FreeControllerV3 GetMainController()
	{
		return containingAtom.mainController;
	}

	public FreeControllerV3[] GetAllControllers()
	{
		return containingAtom.freeControllers;
	}

	public void SelectController(FreeControllerV3 fc, bool alignView = false)
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SelectController(fc, alignView);
		}
	}

	public bool GetLeftPointerShow()
	{
		bool result = false;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetLeftUIPointerShow();
		}
		return result;
	}

	public bool GetRightPointerShow()
	{
		bool result = false;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetRightUIPointerShow();
		}
		return result;
	}

	public bool GetLeftSelect()
	{
		bool result = false;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetLeftSelect();
		}
		return result;
	}

	public bool GetRightSelect()
	{
		bool result = false;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetRightSelect();
		}
		return result;
	}

	public float GetLeftTriggerVal()
	{
		float result = 0f;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetLeftGrabVal();
		}
		return result;
	}

	public float GetRightTriggerVal()
	{
		float result = 0f;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetRightGrabVal();
		}
		return result;
	}

	public bool GetLeftQuickGrab()
	{
		bool result = false;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetLeftGrab();
		}
		return result;
	}

	public bool GetRightQuickGrab()
	{
		bool result = false;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetRightGrab();
		}
		return result;
	}

	public bool GetLeftQuickRelease()
	{
		bool result = false;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetLeftGrabRelease();
		}
		return result;
	}

	public bool GetRightQuickRelease()
	{
		bool result = false;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetRightGrabRelease();
		}
		return result;
	}

	public bool GetLeftGrabToggle()
	{
		bool result = false;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetLeftHoldGrab();
		}
		return result;
	}

	public bool GetRightGrabToggle()
	{
		bool result = false;
		if (SuperController.singleton != null)
		{
			result = SuperController.singleton.GetRightHoldGrab();
		}
		return result;
	}

	public void SaveJSON(JSONClass jc, string saveName)
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SaveJSON(jc, saveName);
		}
	}

	public void SaveJSON(JSONClass jc, string saveName, UserActionCallback confirmCallback, UserActionCallback denyCallback, ExceptionCallback exceptionCallback)
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SaveJSON(jc, saveName, confirmCallback, denyCallback, exceptionCallback);
		}
	}

	public JSONNode LoadJSON(string saveName)
	{
		if (SuperController.singleton != null)
		{
			return SuperController.singleton.LoadJSON(saveName);
		}
		return null;
	}

	public UIDynamicSlider CreateSlider(JSONStorableFloat jsf, bool rightSide = false)
	{
		UIDynamicSlider uIDynamicSlider = null;
		if (manager != null && manager.configurableSliderPrefab != null && jsf.slider == null)
		{
			Transform transform = CreateUIElement(manager.configurableSliderPrefab.transform, rightSide);
			if (transform != null)
			{
				uIDynamicSlider = transform.GetComponent<UIDynamicSlider>();
				if (uIDynamicSlider != null)
				{
					uIDynamicSlider.Configure(jsf.name, jsf.min, jsf.max, jsf.val, jsf.constrained, "F2", true, !jsf.constrained);
					jsf.slider = uIDynamicSlider.slider;
					sliderToJSONStorableFloat.Add(uIDynamicSlider, jsf);
				}
			}
		}
		return uIDynamicSlider;
	}

	public void RemoveSlider(JSONStorableFloat jsf)
	{
		if (jsf != null && jsf.slider != null)
		{
			UIDynamicSlider componentInParent = jsf.slider.GetComponentInParent<UIDynamicSlider>();
			if (componentInParent != null)
			{
				sliderToJSONStorableFloat.Remove(componentInParent);
				Transform transform = componentInParent.transform;
				rightUIElements.Remove(transform);
				leftUIElements.Remove(transform);
				jsf.slider = null;
				Object.Destroy(transform.gameObject);
			}
		}
	}

	public void RemoveSlider(UIDynamicSlider dslider)
	{
		JSONStorableFloat value;
		if (sliderToJSONStorableFloat.TryGetValue(dslider, out value))
		{
			sliderToJSONStorableFloat.Remove(dslider);
			Transform transform = dslider.transform;
			rightUIElements.Remove(transform);
			leftUIElements.Remove(transform);
			value.slider = null;
			Object.Destroy(transform.gameObject);
		}
	}

	public UIDynamicToggle CreateToggle(JSONStorableBool jsb, bool rightSide = false)
	{
		UIDynamicToggle uIDynamicToggle = null;
		if (manager != null && manager.configurableTogglePrefab != null && jsb.toggle == null)
		{
			Transform transform = CreateUIElement(manager.configurableTogglePrefab.transform, rightSide);
			if (transform != null)
			{
				uIDynamicToggle = transform.GetComponent<UIDynamicToggle>();
				if (uIDynamicToggle != null)
				{
					toggleToJSONStorableBool.Add(uIDynamicToggle, jsb);
					uIDynamicToggle.label = jsb.name;
					jsb.toggle = uIDynamicToggle.toggle;
				}
			}
		}
		return uIDynamicToggle;
	}

	public void RemoveToggle(JSONStorableBool jsb)
	{
		if (jsb != null && jsb.toggle != null)
		{
			UIDynamicToggle component = jsb.toggle.GetComponent<UIDynamicToggle>();
			if (component != null)
			{
				toggleToJSONStorableBool.Remove(component);
			}
			Transform transform = jsb.toggle.transform;
			rightUIElements.Remove(transform);
			leftUIElements.Remove(transform);
			jsb.toggle = null;
			Object.Destroy(transform.gameObject);
		}
	}

	public void RemoveToggle(UIDynamicToggle dtoggle)
	{
		JSONStorableBool value;
		if (toggleToJSONStorableBool.TryGetValue(dtoggle, out value))
		{
			toggleToJSONStorableBool.Remove(dtoggle);
			Transform transform = value.toggle.transform;
			rightUIElements.Remove(transform);
			leftUIElements.Remove(transform);
			value.toggle = null;
			Object.Destroy(transform.gameObject);
		}
	}

	public UIDynamicColorPicker CreateColorPicker(JSONStorableColor jsc, bool rightSide = false)
	{
		UIDynamicColorPicker uIDynamicColorPicker = null;
		if (manager != null && manager.configurableColorPickerPrefab != null && jsc.colorPicker == null)
		{
			Transform transform = CreateUIElement(manager.configurableColorPickerPrefab.transform, rightSide);
			if (transform != null)
			{
				uIDynamicColorPicker = transform.GetComponent<UIDynamicColorPicker>();
				if (uIDynamicColorPicker != null)
				{
					colorPickerToJSONStorableColor.Add(uIDynamicColorPicker, jsc);
					uIDynamicColorPicker.label = jsc.name;
					jsc.colorPicker = uIDynamicColorPicker.colorPicker;
				}
			}
		}
		return uIDynamicColorPicker;
	}

	public void RemoveColorPicker(JSONStorableColor jsc)
	{
		if (jsc != null && jsc.colorPicker != null)
		{
			UIDynamicColorPicker componentInParent = jsc.colorPicker.GetComponentInParent<UIDynamicColorPicker>();
			if (componentInParent != null)
			{
				colorPickerToJSONStorableColor.Remove(componentInParent);
				Transform transform = componentInParent.transform;
				rightUIElements.Remove(transform);
				leftUIElements.Remove(transform);
				jsc.colorPicker = null;
				Object.Destroy(transform.gameObject);
			}
		}
	}

	public void RemoveColorPicker(UIDynamicColorPicker dcolor)
	{
		JSONStorableColor value;
		if (colorPickerToJSONStorableColor.TryGetValue(dcolor, out value))
		{
			colorPickerToJSONStorableColor.Remove(dcolor);
			Transform transform = dcolor.transform;
			rightUIElements.Remove(transform);
			leftUIElements.Remove(transform);
			value.colorPicker = null;
			Object.Destroy(transform.gameObject);
		}
	}

	public UIDynamicTextField CreateTextField(JSONStorableString jss, bool rightSide = false)
	{
		UIDynamicTextField uIDynamicTextField = null;
		if (manager != null && manager.configurableTextFieldPrefab != null && jss.text == null)
		{
			Transform transform = CreateUIElement(manager.configurableTextFieldPrefab.transform, rightSide);
			if (transform != null)
			{
				uIDynamicTextField = transform.GetComponent<UIDynamicTextField>();
				if (uIDynamicTextField != null)
				{
					textFieldToJSONStorableString.Add(uIDynamicTextField, jss);
					jss.dynamicText = uIDynamicTextField;
				}
			}
		}
		return uIDynamicTextField;
	}

	public void RemoveTextField(JSONStorableString jss)
	{
		if (jss != null && jss.text != null)
		{
			UIDynamicTextField dynamicText = jss.dynamicText;
			if (dynamicText != null)
			{
				textFieldToJSONStorableString.Remove(dynamicText);
				Transform transform = dynamicText.transform;
				rightUIElements.Remove(transform);
				leftUIElements.Remove(transform);
				jss.text = null;
				Object.Destroy(transform.gameObject);
			}
		}
	}

	public void RemoveTextField(UIDynamicTextField dtext)
	{
		JSONStorableString value;
		if (textFieldToJSONStorableString.TryGetValue(dtext, out value))
		{
			textFieldToJSONStorableString.Remove(dtext);
			Transform transform = dtext.transform;
			rightUIElements.Remove(transform);
			leftUIElements.Remove(transform);
			value.text = null;
			Object.Destroy(transform.gameObject);
		}
	}

	public UIDynamicPopup CreatePopup(JSONStorableStringChooser jsc, bool rightSide = false)
	{
		UIDynamicPopup uIDynamicPopup = null;
		if (manager != null && manager.configurablePopupPrefab != null && jsc.popup == null)
		{
			Transform transform = CreateUIElement(manager.configurablePopupPrefab.transform, rightSide);
			if (transform != null)
			{
				uIDynamicPopup = transform.GetComponent<UIDynamicPopup>();
				if (uIDynamicPopup != null)
				{
					popupToJSONStorableStringChooser.Add(uIDynamicPopup, jsc);
					uIDynamicPopup.label = jsc.name;
					jsc.popup = uIDynamicPopup.popup;
				}
			}
		}
		return uIDynamicPopup;
	}

	public UIDynamicPopup CreateScrollablePopup(JSONStorableStringChooser jsc, bool rightSide = false)
	{
		UIDynamicPopup uIDynamicPopup = null;
		if (manager != null && manager.configurableScrollablePopupPrefab != null && jsc.popup == null)
		{
			Transform transform = CreateUIElement(manager.configurableScrollablePopupPrefab.transform, rightSide);
			if (transform != null)
			{
				uIDynamicPopup = transform.GetComponent<UIDynamicPopup>();
				if (uIDynamicPopup != null)
				{
					popupToJSONStorableStringChooser.Add(uIDynamicPopup, jsc);
					uIDynamicPopup.label = jsc.name;
					jsc.popup = uIDynamicPopup.popup;
				}
			}
		}
		return uIDynamicPopup;
	}

	public UIDynamicPopup CreateFilterablePopup(JSONStorableStringChooser jsc, bool rightSide = false)
	{
		UIDynamicPopup uIDynamicPopup = null;
		if (manager != null && manager.configurableFilterablePopupPrefab != null && jsc.popup == null)
		{
			Transform transform = CreateUIElement(manager.configurableFilterablePopupPrefab.transform, rightSide);
			if (transform != null)
			{
				uIDynamicPopup = transform.GetComponent<UIDynamicPopup>();
				if (uIDynamicPopup != null)
				{
					popupToJSONStorableStringChooser.Add(uIDynamicPopup, jsc);
					uIDynamicPopup.label = jsc.name;
					jsc.popup = uIDynamicPopup.popup;
				}
			}
		}
		return uIDynamicPopup;
	}

	public void RemovePopup(JSONStorableStringChooser jsc)
	{
		if (jsc != null && jsc.popup != null)
		{
			UIDynamicPopup component = jsc.popup.GetComponent<UIDynamicPopup>();
			if (component != null)
			{
				popupToJSONStorableStringChooser.Remove(component);
			}
			Transform transform = jsc.popup.transform;
			rightUIElements.Remove(transform);
			leftUIElements.Remove(transform);
			jsc.popup = null;
			Object.Destroy(transform.gameObject);
		}
	}

	public void RemovePopup(UIDynamicPopup dpopup)
	{
		JSONStorableStringChooser value;
		if (popupToJSONStorableStringChooser.TryGetValue(dpopup, out value))
		{
			popupToJSONStorableStringChooser.Remove(dpopup);
			Transform transform = value.popup.transform;
			rightUIElements.Remove(transform);
			leftUIElements.Remove(transform);
			value.popup = null;
			Object.Destroy(transform.gameObject);
		}
	}

	public UIDynamicButton CreateButton(string label, bool rightSide = false)
	{
		UIDynamicButton uIDynamicButton = null;
		if (manager != null && manager.configurableButtonPrefab != null)
		{
			Transform transform = CreateUIElement(manager.configurableButtonPrefab.transform, rightSide);
			if (transform != null)
			{
				uIDynamicButton = transform.GetComponent<UIDynamicButton>();
				if (uIDynamicButton != null)
				{
					uIDynamicButton.label = label;
				}
			}
		}
		return uIDynamicButton;
	}

	public void RemoveButton(UIDynamicButton button)
	{
		if (button != null)
		{
			rightUIElements.Remove(button.transform);
			leftUIElements.Remove(button.transform);
			Object.Destroy(button.gameObject);
		}
	}

	public UIDynamic CreateSpacer(bool rightSide = false)
	{
		UIDynamic result = null;
		if (manager != null && manager.configurableSpacerPrefab != null)
		{
			Transform transform = CreateUIElement(manager.configurableSpacerPrefab.transform, rightSide);
			if (transform != null)
			{
				result = transform.GetComponent<UIDynamic>();
			}
		}
		return result;
	}

	public void RemoveSpacer(UIDynamic spacer)
	{
		if (spacer != null)
		{
			rightUIElements.Remove(spacer.transform);
			leftUIElements.Remove(spacer.transform);
			Object.Destroy(spacer.gameObject);
		}
	}

	public virtual void Init()
	{
	}

	protected Transform CreateUIElement(Transform prefab, bool rightSide = false)
	{
		Transform transform = null;
		if (prefab != null)
		{
			transform = Object.Instantiate(prefab);
			bool flag = false;
			if (rightSide)
			{
				if (rightUIContent != null)
				{
					flag = true;
					transform.SetParent(rightUIContent, false);
				}
			}
			else if (leftUIContent != null)
			{
				flag = true;
				transform.SetParent(leftUIContent, false);
			}
			if (flag)
			{
				transform.gameObject.SetActive(true);
			}
			else
			{
				transform.gameObject.SetActive(false);
			}
			if (rightSide)
			{
				rightUIElements.Add(transform);
			}
			else
			{
				leftUIElements.Add(transform);
			}
		}
		return transform;
	}

	protected void SyncEnabled(bool b)
	{
		base.enabled = b;
	}

	protected void SyncLabel(string s)
	{
	}

	protected void InitInternal()
	{
		rightUIElements = new List<Transform>();
		leftUIElements = new List<Transform>();
		enabledJSON = new JSONStorableBool("enabled", true, SyncEnabled);
		RegisterBool(enabledJSON);
		pluginLabelJSON = new JSONStorableString("pluginLabel", string.Empty, SyncLabel);
		RegisterString(pluginLabelJSON);
		sliderToJSONStorableFloat = new Dictionary<UIDynamicSlider, JSONStorableFloat>();
		toggleToJSONStorableBool = new Dictionary<UIDynamicToggle, JSONStorableBool>();
		colorPickerToJSONStorableColor = new Dictionary<UIDynamicColorPicker, JSONStorableColor>();
		textFieldToJSONStorableString = new Dictionary<UIDynamicTextField, JSONStorableString>();
		popupToJSONStorableStringChooser = new Dictionary<UIDynamicPopup, JSONStorableStringChooser>();
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		MVRScriptUI componentInChildren = UITransform.GetComponentInChildren<MVRScriptUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		if (componentInChildren.rightUIContent != null)
		{
			rightUIContent = componentInChildren.rightUIContent;
			foreach (Transform item in componentInChildren.rightUIContent.transform)
			{
				item.SetParent(null);
			}
			foreach (Transform rightUIElement in rightUIElements)
			{
				rightUIElement.gameObject.SetActive(true);
				rightUIElement.SetParent(componentInChildren.rightUIContent, false);
			}
		}
		if (!(componentInChildren.leftUIContent != null))
		{
			return;
		}
		leftUIContent = componentInChildren.leftUIContent;
		foreach (Transform leftUIElement in leftUIElements)
		{
			leftUIElement.gameObject.SetActive(true);
			leftUIElement.SetParent(componentInChildren.leftUIContent, false);
		}
	}

	public override void InitUIAlt()
	{
	}

	public void ForceAwake()
	{
		Awake();
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			InitInternal();
			InitUI();
			InitUIAlt();
		}
	}
}
