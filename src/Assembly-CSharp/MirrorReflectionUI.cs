using UnityEngine;
using UnityEngine.UI;

public class MirrorReflectionUI : UIProvider
{
	public Toggle disablePixelLightsToggle;

	public UIPopup textureSizePopup;

	public UIPopup antiAliasingPopup;

	public RectTransform reflectionOpacityContainer;

	public Slider reflectionOpacitySlider;

	public RectTransform reflectionBlendContainer;

	public Slider reflectionBlendSlider;

	public RectTransform surfaceTexturePowerContainer;

	public Slider surfaceTexturePowerSlider;

	public RectTransform specularIntensityContainer;

	public Slider specularIntensitySlider;

	public HSVColorPicker reflectionColorPicker;

	public Slider renderQueueSlider;
}
