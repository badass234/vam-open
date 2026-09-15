public class AdjustJointSpringsControl : JSONStorable
{
	public AdjustJointSprings jointSprings;

	public JSONStorableFloat springStrengthJSON;

	public void SyncSpringStrength(float f)
	{
		if (jointSprings != null)
		{
			jointSprings.percent = f;
		}
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			AdjustJointSpringsControlUI componentInChildren = UITransform.GetComponentInChildren<AdjustJointSpringsControlUI>(true);
			if (componentInChildren != null)
			{
				springStrengthJSON.slider = componentInChildren.springStrengthSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			AdjustJointSpringsControlUI componentInChildren = UITransformAlt.GetComponentInChildren<AdjustJointSpringsControlUI>(true);
			if (componentInChildren != null)
			{
				springStrengthJSON.sliderAlt = componentInChildren.springStrengthSlider;
			}
		}
	}

	protected void Init()
	{
		springStrengthJSON = new JSONStorableFloat("springStrength", 0.1f, SyncSpringStrength, 0f, 1f);
		RegisterFloat(springStrengthJSON);
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
