using UnityEngine;

public class Torchslider2 : MonoBehaviour
{
	public GameObject TorcheObj;

	public Camera MainCamera;

	public GUISkin SkinSlider;

	private GameObject Torch;

	private float Intensity_Light;

	private bool CameraRendering;

	private void OnGUI()
	{
		GUI.Label(new Rect(25f, 25f, 150f, 30f), "Light Intensity", SkinSlider.label);
		Intensity_Light = GUI.HorizontalSlider(new Rect(25f, 50f, 150f, 30f), Intensity_Light, 0f, TorcheObj.GetComponent<Torchelight>().MaxLightIntensity, SkinSlider.horizontalSlider, SkinSlider.horizontalSliderThumb);
		CameraRendering = GUI.Toggle(new Rect(25f, 80f, 150f, 30f), CameraRendering, "Deferred lighting");
		if (CameraRendering)
		{
			MainCamera.renderingPath = RenderingPath.DeferredLighting;
		}
		else
		{
			MainCamera.renderingPath = RenderingPath.Forward;
		}
	}

	private void Update()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("TagLight");
		foreach (GameObject gameObject in array)
		{
			gameObject.GetComponent<Torchelight>().IntensityLight = Intensity_Light;
		}
	}
}
