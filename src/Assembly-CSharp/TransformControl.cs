using UnityEngine;

public class TransformControl : JSONStorable
{
	protected JSONStorableFloat xPositionJSON;

	protected JSONStorableFloat yPositionJSON;

	protected JSONStorableFloat zPositionJSON;

	protected JSONStorableFloat xRotationJSON;

	protected JSONStorableFloat yRotationJSON;

	protected JSONStorableFloat zRotationJSON;

	protected JSONStorableFloat scaleJSON;

	protected JSONStorableFloat xScaleJSON;

	protected JSONStorableFloat yScaleJSON;

	protected JSONStorableFloat zScaleJSON;

	public Transform scaleChangeReceiverContainer;

	protected void SyncXPosition(float f)
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.x = f;
		base.transform.localPosition = localPosition;
	}

	protected void SyncYPosition(float f)
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.y = f;
		base.transform.localPosition = localPosition;
	}

	protected void SyncZPosition(float f)
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.z = f;
		base.transform.localPosition = localPosition;
	}

	protected void SyncXRotation(float f)
	{
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.x = f;
		base.transform.localEulerAngles = localEulerAngles;
	}

	protected void SyncYRotation(float f)
	{
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.y = f;
		base.transform.localEulerAngles = localEulerAngles;
	}

	protected void SyncZRotation(float f)
	{
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.z = f;
		base.transform.localEulerAngles = localEulerAngles;
	}

	protected void SyncScale(float f)
	{
		Vector3 localScale = default(Vector3);
		localScale.x = scaleJSON.val * xScaleJSON.val;
		localScale.y = scaleJSON.val * yScaleJSON.val;
		localScale.z = scaleJSON.val * zScaleJSON.val;
		base.transform.localScale = localScale;
		if (scaleChangeReceiverContainer != null)
		{
			ScaleChangeReceiver[] componentsInChildren = scaleChangeReceiverContainer.GetComponentsInChildren<ScaleChangeReceiver>();
			ScaleChangeReceiver[] array = componentsInChildren;
			foreach (ScaleChangeReceiver scaleChangeReceiver in array)
			{
				scaleChangeReceiver.ScaleChanged(scaleChangeReceiver.scale);
			}
			ScaleChangeReceiverJSONStorable[] componentsInChildren2 = scaleChangeReceiverContainer.GetComponentsInChildren<ScaleChangeReceiverJSONStorable>();
			ScaleChangeReceiverJSONStorable[] array2 = componentsInChildren2;
			foreach (ScaleChangeReceiverJSONStorable scaleChangeReceiverJSONStorable in array2)
			{
				scaleChangeReceiverJSONStorable.ScaleChanged(scaleChangeReceiverJSONStorable.scale);
			}
		}
	}

	protected void Init()
	{
		xPositionJSON = new JSONStorableFloat("xPosition", 0f, SyncXPosition, -0.1f, 0.1f, false);
		RegisterFloat(xPositionJSON);
		yPositionJSON = new JSONStorableFloat("yPosition", 0f, SyncYPosition, -0.1f, 0.1f, false);
		RegisterFloat(yPositionJSON);
		zPositionJSON = new JSONStorableFloat("zPosition", 0f, SyncZPosition, -0.1f, 0.1f, false);
		RegisterFloat(zPositionJSON);
		xRotationJSON = new JSONStorableFloat("xRotation", 0f, SyncXRotation, -180f, 180f);
		RegisterFloat(xRotationJSON);
		yRotationJSON = new JSONStorableFloat("yRotation", 0f, SyncYRotation, -180f, 180f);
		RegisterFloat(yRotationJSON);
		zRotationJSON = new JSONStorableFloat("zRotation", 0f, SyncZRotation, -180f, 180f);
		RegisterFloat(zRotationJSON);
		scaleJSON = new JSONStorableFloat("scale", 1f, SyncScale, 0f, 2f, false);
		RegisterFloat(scaleJSON);
		xScaleJSON = new JSONStorableFloat("xScale", 1f, SyncScale, 0f, 2f, false);
		RegisterFloat(xScaleJSON);
		yScaleJSON = new JSONStorableFloat("yScale", 1f, SyncScale, 0f, 2f, false);
		RegisterFloat(yScaleJSON);
		zScaleJSON = new JSONStorableFloat("zScale", 1f, SyncScale, 0f, 2f, false);
		RegisterFloat(zScaleJSON);
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			TransformControlUI componentInChildren = UITransform.GetComponentInChildren<TransformControlUI>(true);
			if (componentInChildren != null)
			{
				xPositionJSON.slider = componentInChildren.xPositionSlider;
				yPositionJSON.slider = componentInChildren.yPositionSlider;
				zPositionJSON.slider = componentInChildren.zPositionSlider;
				xRotationJSON.slider = componentInChildren.xRotationSlider;
				yRotationJSON.slider = componentInChildren.yRotationSlider;
				zRotationJSON.slider = componentInChildren.zRotationSlider;
				scaleJSON.slider = componentInChildren.scaleSlider;
				xScaleJSON.slider = componentInChildren.xScaleSlider;
				yScaleJSON.slider = componentInChildren.yScaleSlider;
				zScaleJSON.slider = componentInChildren.zScaleSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			TransformControlUI componentInChildren = UITransformAlt.GetComponentInChildren<TransformControlUI>(true);
			if (componentInChildren != null)
			{
				xPositionJSON.sliderAlt = componentInChildren.xPositionSlider;
				yPositionJSON.sliderAlt = componentInChildren.yPositionSlider;
				zPositionJSON.sliderAlt = componentInChildren.zPositionSlider;
				xRotationJSON.sliderAlt = componentInChildren.xRotationSlider;
				yRotationJSON.sliderAlt = componentInChildren.yRotationSlider;
				zRotationJSON.sliderAlt = componentInChildren.zRotationSlider;
				scaleJSON.sliderAlt = componentInChildren.scaleSlider;
				xScaleJSON.sliderAlt = componentInChildren.xScaleSlider;
				yScaleJSON.sliderAlt = componentInChildren.yScaleSlider;
				zScaleJSON.sliderAlt = componentInChildren.zScaleSlider;
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
