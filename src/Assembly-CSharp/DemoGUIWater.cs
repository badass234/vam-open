using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoGUIWater : MonoBehaviour
{
	public float UpdateInterval = 0.5f;

	public int MaxScenes = 2;

	public bool IsMobileScene;

	public Light Sun;

	public GameObject SunTransform;

	public GameObject Boat;

	public GameObject water1;

	public GameObject water2;

	public float angle = 130f;

	private bool canUpdateTestMaterial;

	private GameObject cam;

	private GUIStyle guiStyleHeader = new GUIStyle();

	private Material currentWaterMaterial;

	private Material causticMaterial;

	private GameObject currentWater;

	private float transparent;

	private float fadeBlend;

	private float refl;

	private float refraction;

	private float waterWaveScaleXZ = 1f;

	private Vector4 waterDirection;

	private Vector4 causticDirection;

	private Vector4 foamDirection;

	private Vector4 ABDirection;

	private Vector4 CDDirection;

	private float direction = 1f;

	private Color reflectionColor;

	private Vector3 oldCausticScale;

	private float oldTextureScale;

	private float oldWaveScale;

	private GameObject caustic;

	private float startSunIntencity;

	private void Start()
	{
		guiStyleHeader.fontSize = 18;
		guiStyleHeader.normal.textColor = new Color(1f, 0f, 0f);
		UpdateCurrentWater();
	}

	private void UpdateCurrentWater()
	{
		if (Boat != null)
		{
			Boat.SetActive(false);
			Boat.SetActive(true);
		}
		startSunIntencity = Sun.intensity;
		currentWater = GameObject.Find("Water");
		currentWaterMaterial = currentWater.GetComponent<Renderer>().material;
		refl = currentWaterMaterial.GetColor("_ReflectionColor").r;
		if (!IsMobileScene)
		{
			transparent = currentWaterMaterial.GetFloat("_DepthTransperent");
		}
		if (!IsMobileScene)
		{
			fadeBlend = currentWaterMaterial.GetFloat("_FadeDepth");
		}
		refraction = currentWaterMaterial.GetFloat("_Distortion");
		oldTextureScale = currentWaterMaterial.GetFloat("_TexturesScale");
		oldWaveScale = currentWaterMaterial.GetFloat("_WaveScale");
		GameObject gameObject = GameObject.Find("InfiniteWaterMesh");
		if (gameObject != null)
		{
			gameObject.GetComponent<Renderer>().material = currentWaterMaterial;
		}
		GameObject gameObject2 = GameObject.Find("ProjectorCausticScale");
		if (gameObject2 != null)
		{
			oldCausticScale = gameObject2.transform.localScale;
		}
		caustic = GameObject.Find("Caustic");
		if (IsMobileScene)
		{
			caustic.SetActive(true);
		}
		if (!IsMobileScene)
		{
			causticMaterial = caustic.GetComponent<Projector>().material;
		}
		waterDirection = currentWaterMaterial.GetVector("_Direction");
		if (!IsMobileScene)
		{
			foamDirection = currentWaterMaterial.GetVector("_FoamDirection");
		}
		if (!IsMobileScene)
		{
			causticDirection = causticMaterial.GetVector("_CausticDirection");
		}
		ABDirection = currentWaterMaterial.GetVector("_GDirectionAB");
		CDDirection = currentWaterMaterial.GetVector("_GDirectionCD");
	}

	private void OnGUI()
	{
		if (IsMobileScene)
		{
			GUIMobile();
		}
		else
		{
			GUIPC();
		}
	}

	private void GUIMobile()
	{
		if (currentWaterMaterial == null)
		{
			return;
		}
		if (GUI.Button(new Rect(10f, 35f, 150f, 40f), "On/Off Ripples"))
		{
			caustic.SetActive(true);
			water1.SetActive(!water1.activeSelf);
			water2.SetActive(!water2.activeSelf);
			caustic = GameObject.Find("Caustic");
			if (IsMobileScene)
			{
				caustic.SetActive(true);
			}
		}
		if (GUI.Button(new Rect(10f, 190f, 150f, 40f), "On/Off caustic"))
		{
			caustic.SetActive(!caustic.activeSelf);
		}
		GUIStyle gUIStyle = new GUIStyle();
		gUIStyle.normal.textColor = new Color(1f, 1f, 1f);
		angle = GUI.HorizontalSlider(new Rect(10f, 102f, 120f, 15f), angle, 30f, 240f);
		GUI.Label(new Rect(140f, 100f, 30f, 30f), "Day Time", gUIStyle);
		float value = Mathf.Sin((angle - 60f) / 50f);
		Sun.intensity = Mathf.Clamp01(value) * startSunIntencity + 0.05f;
		SunTransform.transform.rotation = Quaternion.Euler(0f, 0f, angle);
		refl = GUI.HorizontalSlider(new Rect(10f, 122f, 120f, 15f), refl, 0f, 1f);
		reflectionColor = new Color(refl, refl, refl, 1f);
		GUI.Label(new Rect(140f, 120f, 30f, 30f), "Reflection", gUIStyle);
		currentWaterMaterial.SetColor("_ReflectionColor", reflectionColor);
		refraction = GUI.HorizontalSlider(new Rect(10f, 142f, 120f, 15f), refraction, 0f, 700f);
		GUI.Label(new Rect(140f, 140f, 30f, 30f), "Distortion", gUIStyle);
		currentWaterMaterial.SetFloat("_Distortion", refraction);
		waterWaveScaleXZ = GUI.HorizontalSlider(new Rect(10f, 162f, 120f, 15f), waterWaveScaleXZ, 0.3f, 3f);
		GUI.Label(new Rect(140f, 160f, 30f, 30f), "Scale", gUIStyle);
		currentWaterMaterial.SetFloat("_WaveScale", oldWaveScale * waterWaveScaleXZ);
		currentWaterMaterial.SetFloat("_TexturesScale", oldTextureScale * waterWaveScaleXZ);
	}

	private void GUIPC()
	{
		if (currentWaterMaterial == null)
		{
			return;
		}
		if (GUI.Button(new Rect(10f, 35f, 150f, 40f), "Change Scene "))
		{
			int buildIndex = SceneManager.GetActiveScene().buildIndex;
			if (buildIndex == MaxScenes - 1)
			{
				SceneManager.LoadScene(0);
			}
			else
			{
				SceneManager.LoadScene(buildIndex + 1);
			}
			UpdateCurrentWater();
		}
		GUIStyle gUIStyle = new GUIStyle();
		gUIStyle.normal.textColor = new Color(1f, 1f, 1f);
		angle = GUI.HorizontalSlider(new Rect(10f, 102f, 120f, 15f), angle, 30f, 240f);
		GUI.Label(new Rect(140f, 100f, 30f, 30f), "Day Time", gUIStyle);
		float value = Mathf.Sin((angle - 60f) / 50f);
		Sun.intensity = Mathf.Clamp01(value) * startSunIntencity + 0.05f;
		SunTransform.transform.rotation = Quaternion.Euler(0f, 0f, angle);
		transparent = GUI.HorizontalSlider(new Rect(10f, 122f, 120f, 15f), transparent, 0f, 1f);
		GUI.Label(new Rect(140f, 120f, 30f, 30f), "Depth Transperent", gUIStyle);
		currentWaterMaterial.SetFloat("_DepthTransperent", transparent);
		fadeBlend = GUI.HorizontalSlider(new Rect(10f, 142f, 120f, 15f), fadeBlend, 0f, 1f);
		GUI.Label(new Rect(140f, 140f, 30f, 30f), "Fade Depth", gUIStyle);
		currentWaterMaterial.SetFloat("_FadeDepth", fadeBlend);
		refl = GUI.HorizontalSlider(new Rect(10f, 162f, 120f, 15f), refl, 0f, 1f);
		reflectionColor = new Color(refl, refl, refl, 1f);
		GUI.Label(new Rect(140f, 160f, 30f, 30f), "Reflection", gUIStyle);
		currentWaterMaterial.SetColor("_ReflectionColor", reflectionColor);
		refraction = GUI.HorizontalSlider(new Rect(10f, 182f, 120f, 15f), refraction, 0f, 700f);
		GUI.Label(new Rect(140f, 180f, 30f, 30f), "Distortion", gUIStyle);
		currentWaterMaterial.SetFloat("_Distortion", refraction);
		waterWaveScaleXZ = GUI.HorizontalSlider(new Rect(10f, 202f, 120f, 15f), waterWaveScaleXZ, 0.3f, 3f);
		GUI.Label(new Rect(140f, 200f, 30f, 30f), "Scale", gUIStyle);
		currentWaterMaterial.SetFloat("_WaveScale", oldWaveScale * waterWaveScaleXZ);
		currentWaterMaterial.SetFloat("_TexturesScale", oldTextureScale * waterWaveScaleXZ);
		GameObject gameObject = GameObject.Find("ProjectorCausticScale");
		Vector3 vector = oldCausticScale * waterWaveScaleXZ;
		if ((double)(gameObject.transform.localScale - vector).magnitude > 0.01)
		{
			gameObject.transform.localScale = vector;
			caustic.GetComponent<ProjectorMatrix>().UpdateMatrix();
		}
		direction = GUI.HorizontalSlider(new Rect(10f, 222f, 120f, 15f), direction, 1f, -1f);
		GUI.Label(new Rect(140f, 220f, 30f, 30f), "Direction", gUIStyle);
		currentWaterMaterial.SetVector("_Direction", waterDirection * direction);
		currentWaterMaterial.SetVector("_FoamDirection", foamDirection * direction);
		causticMaterial.SetVector("_CausticDirection", causticDirection * direction);
		currentWaterMaterial.SetVector("_GDirectionAB", ABDirection * direction);
		currentWaterMaterial.SetVector("_GDirectionCD", CDDirection * direction);
	}

	private void OnDestroy()
	{
		if (!IsMobileScene)
		{
			causticMaterial.SetVector("_CausticDirection", causticDirection);
		}
	}

	private void OnSetColorMain(Color color)
	{
		currentWaterMaterial.SetColor("_Color", color);
	}
}
