using UnityEngine;

public class TorchSlider : MonoBehaviour
{
	public GameObject TorcheObj;

	public GUISkin SkinSlider;

	private void OnGUI()
	{
		GUI.Label(new Rect(25f, 25f, 150f, 30f), "Light Intensity", SkinSlider.label);
		TorcheObj.GetComponent<Torchelight>().IntensityLight = GUI.HorizontalSlider(new Rect(25f, 50f, 150f, 30f), TorcheObj.GetComponent<Torchelight>().IntensityLight, 0f, TorcheObj.GetComponent<Torchelight>().MaxLightIntensity, SkinSlider.horizontalSlider, SkinSlider.horizontalSliderThumb);
	}
}
