using GPUTools.Hair.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class HairSimStyleControl : ObjectChooser
{
	public Button matchScalpButton;

	public Button matchScalpButtonAlt;

	public float matchScalpRatio = 0.7f;

	public Slider matchScalpRatioSlider;

	public Slider matchScalpRatioSliderAlt;

	public ObjectChooser scalpChooser;

	protected void SetMatchScalpRatio(float f)
	{
		matchScalpRatio = f;
	}

	public void MatchScalp()
	{
		if (base.CurrentChoice != null && scalpChooser != null && scalpChooser.CurrentChoice != null)
		{
			MaterialOptions componentInChildren = scalpChooser.CurrentChoice.GetComponentInChildren<MaterialOptions>();
			HairSettings componentInChildren2 = base.CurrentChoice.GetComponentInChildren<HairSettings>();
			if (componentInChildren2 != null && componentInChildren != null)
			{
				Color rootColor = componentInChildren2.RenderSettings.RootTipColorProvider.RootColor;
				Color tipColor = componentInChildren2.RenderSettings.RootTipColorProvider.TipColor;
				Color color = Color.Lerp(rootColor, tipColor, matchScalpRatio);
				componentInChildren.SetColor1(color);
				componentInChildren.SetColor2(color);
			}
		}
	}

	public override void InitUI()
	{
		base.InitUI();
		if (!(UITransform != null))
		{
			return;
		}
		HairSimStyleControlUI componentInChildren = UITransform.GetComponentInChildren<HairSimStyleControlUI>();
		if (componentInChildren != null)
		{
			if (matchScalpButton != null)
			{
				matchScalpButton.onClick.RemoveAllListeners();
			}
			matchScalpButton = componentInChildren.matchScalpButton;
			if (matchScalpButton != null)
			{
				matchScalpButton.onClick.AddListener(MatchScalp);
			}
			if (matchScalpRatioSlider != null)
			{
				matchScalpRatioSlider.onValueChanged.RemoveAllListeners();
			}
			matchScalpRatioSlider = componentInChildren.matchScalpRatioSlider;
			if (matchScalpRatioSlider != null)
			{
				matchScalpRatioSlider.value = matchScalpRatio;
				matchScalpRatioSlider.onValueChanged.AddListener(SetMatchScalpRatio);
			}
		}
	}

	public override void InitUIAlt()
	{
		base.InitUIAlt();
		if (!(UITransformAlt != null))
		{
			return;
		}
		HairSimStyleControlUI componentInChildren = UITransformAlt.GetComponentInChildren<HairSimStyleControlUI>();
		if (componentInChildren != null)
		{
			if (matchScalpButtonAlt != null)
			{
				matchScalpButtonAlt.onClick.RemoveAllListeners();
			}
			matchScalpButtonAlt = componentInChildren.matchScalpButton;
			if (matchScalpButtonAlt != null)
			{
				matchScalpButtonAlt.onClick.AddListener(MatchScalp);
			}
			if (matchScalpRatioSliderAlt != null)
			{
				matchScalpRatioSliderAlt.onValueChanged.RemoveAllListeners();
			}
			matchScalpRatioSliderAlt = componentInChildren.matchScalpRatioSlider;
			if (matchScalpRatioSliderAlt != null)
			{
				matchScalpRatioSliderAlt.value = matchScalpRatio;
				matchScalpRatioSliderAlt.onValueChanged.AddListener(SetMatchScalpRatio);
			}
		}
	}
}
