using System.Reflection;
using UnityEngine;
using UnityEngine.PostProcessing;

public class AQUAS_LensEffects : MonoBehaviour
{
	public AQUAS_Parameters.UnderWaterParameters underWaterParameters = new AQUAS_Parameters.UnderWaterParameters();

	public AQUAS_Parameters.GameObjects gameObjects = new AQUAS_Parameters.GameObjects();

	public AQUAS_Parameters.BubbleSpawnCriteria bubbleSpawnCriteria = new AQUAS_Parameters.BubbleSpawnCriteria();

	public AQUAS_Parameters.WetLens wetLens = new AQUAS_Parameters.WetLens();

	public AQUAS_Parameters.CausticSettings causticSettings = new AQUAS_Parameters.CausticSettings();

	public AQUAS_Parameters.Audio soundEffects = new AQUAS_Parameters.Audio();

	private int sprayFrameIndex;

	private GameObject tenkokuObj;

	private Material airLensMaterial;

	private Material waterPlaneMaterial;

	[HideInInspector]
	public float t;

	private float t2;

	private float bubbleSpawnTimer;

	private float defaultFogDensity;

	private Color defaultFogColor;

	private float defaultFoamContrast;

	private float defaultBloomIntensity;

	private float defaultSpecularity;

	private float defaultRefraction;

	private bool defaultFog;

	private bool defaultSunShaftsEnabled;

	private bool defaultBloomEnabled;

	private bool defaultBlurEnabled;

	private bool defaultVignetteEnabled;

	private bool defaultNoiseEnabled;

	[HideInInspector]
	public bool setAfloatFog = true;

	[HideInInspector]
	public bool rundown;

	private bool playSurfaceSplash;

	private bool playDiveSplash;

	private bool playUnderwater;

	private int bubbleCount;

	private int maxBubbleCount;

	private int activePlane;

	private int lastActivePlane = 100;

	private FieldInfo fi;

	private PostProcessingBehaviour postProcessing;

	private AudioSource waterLensAudio;

	private AudioSource airLensAudio;

	private AudioSource audioComp;

	private AudioSource cameraAudio;

	private Projector primaryCausticsProjector;

	private Projector secondaryCausticsProjector;

	private AQUAS_Caustics primaryAquasCaustics;

	private AQUAS_Caustics secondaryAquasCaustics;

	private AQUAS_BubbleBehaviour bubbleBehaviour;

	public bool underWater { get; private set; }

	private void Start()
	{
		if (gameObjects.mainCamera.GetComponent<PostProcessingBehaviour>() == null)
		{
			gameObjects.mainCamera.AddComponent<PostProcessingBehaviour>();
		}
		postProcessing = gameObjects.mainCamera.GetComponent<PostProcessingBehaviour>();
		waterLensAudio = gameObjects.waterLens.GetComponent<AudioSource>();
		airLensAudio = gameObjects.airLens.GetComponent<AudioSource>();
		audioComp = GetComponent<AudioSource>();
		cameraAudio = gameObjects.mainCamera.GetComponent<AudioSource>();
		bubbleBehaviour = gameObjects.bubble.GetComponent<AQUAS_BubbleBehaviour>();
		gameObjects.airLens.SetActive(true);
		gameObjects.waterLens.SetActive(false);
		airLensMaterial = gameObjects.airLens.GetComponent<Renderer>().material;
		waterPlaneMaterial = gameObjects.waterPlanes[0].GetComponent<Renderer>().material;
		t = wetLens.wetTime + wetLens.dryingTime;
		t2 = 0f;
		bubbleSpawnTimer = 0f;
		defaultFog = RenderSettings.fog;
		defaultFogDensity = RenderSettings.fogDensity;
		defaultFogColor = RenderSettings.fogColor;
		defaultFoamContrast = waterPlaneMaterial.GetFloat("_FoamContrast");
		defaultSpecularity = waterPlaneMaterial.GetFloat("_Specular");
		if (waterPlaneMaterial.HasProperty("_Refraction"))
		{
			defaultRefraction = waterPlaneMaterial.GetFloat("_Refraction");
		}
		postProcessing.profile = underWaterParameters.defaultProfile;
		audioComp.clip = soundEffects.sounds[0];
		audioComp.loop = true;
		audioComp.Stop();
		airLensAudio.clip = soundEffects.sounds[1];
		airLensAudio.loop = false;
		airLensAudio.Stop();
		waterLensAudio.clip = soundEffects.sounds[2];
		waterLensAudio.loop = false;
		waterLensAudio.Stop();
		if (GameObject.Find("Tenkoku DynamicSky") != null)
		{
			tenkokuObj = GameObject.Find("Tenkoku DynamicSky");
		}
	}

	private void Update()
	{
		CheckIfStillUnderWater();
		if (underWater)
		{
			t = 0f;
			t2 += Time.deltaTime;
			gameObjects.airLens.SetActive(false);
			gameObjects.waterLens.SetActive(true);
			sprayFrameIndex = 0;
			rundown = true;
			BubbleSpawner();
			if (playUnderwater)
			{
				audioComp.Play();
				playUnderwater = false;
			}
			if (playDiveSplash)
			{
				waterLensAudio.Play();
				playDiveSplash = false;
			}
			playSurfaceSplash = true;
			airLensAudio.Stop();
			cameraAudio.enabled = false;
			airLensAudio.volume = soundEffects.surfacingVolume;
			audioComp.volume = soundEffects.diveVolume;
			waterLensAudio.volume = soundEffects.underwaterVolume;
			if (primaryCausticsProjector != null)
			{
				primaryCausticsProjector.material.SetTextureScale("_Texture", new Vector2(causticSettings.causticTiling.y, causticSettings.causticTiling.y));
				primaryCausticsProjector.material.SetFloat("_Intensity", causticSettings.causticIntensity.y);
				primaryAquasCaustics.maxCausticDepth = causticSettings.maxCausticDepth;
			}
			if (secondaryCausticsProjector != null)
			{
				secondaryCausticsProjector.material.SetTextureScale("_Texture", new Vector2(causticSettings.causticTiling.y, causticSettings.causticTiling.y));
				secondaryCausticsProjector.material.SetFloat("_Intensity", causticSettings.causticIntensity.y);
				secondaryAquasCaustics.maxCausticDepth = causticSettings.maxCausticDepth;
			}
			waterPlaneMaterial.SetFloat("_UnderwaterMode", 1f);
			waterPlaneMaterial.SetFloat("_FoamContrast", 0f);
			waterPlaneMaterial.SetFloat("_Specular", defaultSpecularity * 5f);
			waterPlaneMaterial.SetFloat("_Refraction", 0.7f);
			if (postProcessing.profile != underWaterParameters.underwaterProfile)
			{
				postProcessing.profile = underWaterParameters.underwaterProfile;
			}
			if (tenkokuObj != null)
			{
				Component component = tenkokuObj.GetComponent("TenkokuModule");
				component.GetType().GetField("enableFog", BindingFlags.Instance | BindingFlags.Public)?.SetValue(component, false);
			}
			RenderSettings.fog = true;
			RenderSettings.fogDensity = underWaterParameters.fogDensity;
			RenderSettings.fogColor = underWaterParameters.fogColor;
			return;
		}
		t2 = 0f;
		t += Time.deltaTime;
		gameObjects.airLens.SetActive(true);
		gameObjects.waterLens.SetActive(false);
		if (rundown)
		{
			sprayFrameIndex = 0;
			NextFrame();
			InvokeRepeating("NextFrame", 1f / wetLens.rundownSpeed, 1f / wetLens.rundownSpeed);
			rundown = false;
		}
		bubbleCount = 0;
		maxBubbleCount = Random.Range(bubbleSpawnCriteria.minBubbleCount, bubbleSpawnCriteria.maxBubbleCount);
		bubbleSpawnTimer = 0f;
		if (playSurfaceSplash)
		{
			airLensAudio.Play();
			playSurfaceSplash = false;
		}
		playUnderwater = true;
		playDiveSplash = true;
		audioComp.Stop();
		waterLensAudio.Stop();
		cameraAudio.enabled = true;
		if (primaryCausticsProjector != null)
		{
			primaryCausticsProjector.material.SetTextureScale("_Texture", new Vector2(causticSettings.causticTiling.x, causticSettings.causticTiling.x));
			primaryCausticsProjector.material.SetFloat("_Intensity", causticSettings.causticIntensity.x);
		}
		if (secondaryCausticsProjector != null)
		{
			secondaryCausticsProjector.material.SetTextureScale("_Texture", new Vector2(causticSettings.causticTiling.x, causticSettings.causticTiling.x));
			secondaryCausticsProjector.material.SetFloat("_Intensity", causticSettings.causticIntensity.x);
		}
		if (t <= wetLens.wetTime)
		{
			airLensMaterial.SetFloat("_Refraction", 1f);
			airLensMaterial.SetFloat("_Transparency", 0.01f);
		}
		else
		{
			airLensMaterial.SetFloat("_Refraction", Mathf.Lerp(1f, 0f, (t - wetLens.wetTime) / wetLens.dryingTime));
			airLensMaterial.SetFloat("_Transparency", Mathf.Lerp(0.01f, 0f, (t - wetLens.wetTime) / wetLens.dryingTime));
		}
		waterPlaneMaterial.SetFloat("_FoamContrast", defaultFoamContrast);
		waterPlaneMaterial.SetFloat("_UnderwaterMode", 0f);
		waterPlaneMaterial.SetFloat("_Specular", defaultSpecularity);
		waterPlaneMaterial.SetFloat("_Refraction", defaultRefraction);
		if (postProcessing.profile != underWaterParameters.defaultProfile)
		{
			postProcessing.profile = underWaterParameters.defaultProfile;
		}
		if (tenkokuObj != null)
		{
			Component component2 = tenkokuObj.GetComponent("TenkokuModule");
			component2.GetType().GetField("enableFog", BindingFlags.Instance | BindingFlags.Public)?.SetValue(component2, true);
		}
		RenderSettings.fog = defaultFog;
		if (setAfloatFog)
		{
			RenderSettings.fogColor = defaultFogColor;
			RenderSettings.fogDensity = defaultFogDensity;
		}
	}

	private bool CheckIfUnderWater(int waterPlanesCount)
	{
		if (!gameObjects.useSquaredPlanes)
		{
			for (int i = 0; i < waterPlanesCount; i++)
			{
				if (!(Mathf.Pow(base.transform.position.x - gameObjects.waterPlanes[i].transform.position.x, 2f) + Mathf.Pow(base.transform.position.z - gameObjects.waterPlanes[i].transform.position.z, 2f) < Mathf.Pow(gameObjects.waterPlanes[i].GetComponent<Renderer>().bounds.extents.x, 2f)))
				{
					continue;
				}
				if (activePlane != lastActivePlane)
				{
					if (gameObjects.waterPlanes[activePlane].transform.Find("PrimaryCausticsProjector") != null)
					{
						primaryCausticsProjector = gameObjects.waterPlanes[activePlane].transform.Find("PrimaryCausticsProjector").GetComponent<Projector>();
						primaryAquasCaustics = gameObjects.waterPlanes[activePlane].transform.Find("PrimaryCausticsProjector").GetComponent<AQUAS_Caustics>();
					}
					if (gameObjects.waterPlanes[activePlane].transform.Find("SecondaryCausticsProjector") != null)
					{
						secondaryCausticsProjector = gameObjects.waterPlanes[activePlane].transform.Find("SecondaryCausticsProjector").GetComponent<Projector>();
						secondaryAquasCaustics = gameObjects.waterPlanes[activePlane].transform.Find("SecondaryCausticsProjector").GetComponent<AQUAS_Caustics>();
					}
					lastActivePlane = activePlane;
				}
				activePlane = i;
				if (base.transform.position.y < gameObjects.waterPlanes[i].transform.position.y)
				{
					waterPlaneMaterial = gameObjects.waterPlanes[i].GetComponent<Renderer>().material;
					activePlane = i;
					return true;
				}
			}
		}
		else
		{
			for (int j = 0; j < waterPlanesCount; j++)
			{
				if (!(Mathf.Abs(base.transform.position.x - gameObjects.waterPlanes[j].transform.position.x) < gameObjects.waterPlanes[j].GetComponent<Renderer>().bounds.extents.x) || !(Mathf.Abs(base.transform.position.z - gameObjects.waterPlanes[j].transform.position.z) < gameObjects.waterPlanes[j].GetComponent<Renderer>().bounds.extents.z))
				{
					continue;
				}
				if (activePlane != lastActivePlane)
				{
					if (gameObjects.waterPlanes[activePlane].transform.Find("PrimaryCausticsProjector") != null)
					{
						primaryCausticsProjector = gameObjects.waterPlanes[activePlane].transform.Find("PrimaryCausticsProjector").GetComponent<Projector>();
						primaryAquasCaustics = gameObjects.waterPlanes[activePlane].transform.Find("PrimaryCausticsProjector").GetComponent<AQUAS_Caustics>();
					}
					if (gameObjects.waterPlanes[activePlane].transform.Find("SecondaryCausticsProjector") != null)
					{
						secondaryCausticsProjector = gameObjects.waterPlanes[activePlane].transform.Find("SecondaryCausticsProjector").GetComponent<Projector>();
						secondaryAquasCaustics = gameObjects.waterPlanes[activePlane].transform.Find("SecondaryCausticsProjector").GetComponent<AQUAS_Caustics>();
					}
					lastActivePlane = activePlane;
				}
				activePlane = j;
				if (base.transform.position.y < gameObjects.waterPlanes[j].transform.position.y)
				{
					waterPlaneMaterial = gameObjects.waterPlanes[0].GetComponent<Renderer>().material;
					activePlane = j;
					return true;
				}
			}
		}
		return false;
	}

	private void CheckIfStillUnderWater()
	{
		if (!gameObjects.useSquaredPlanes)
		{
			if (underWater && Mathf.Pow(base.transform.position.x - gameObjects.waterPlanes[activePlane].transform.position.x, 2f) + Mathf.Pow(base.transform.position.z - gameObjects.waterPlanes[activePlane].transform.position.z, 2f) > Mathf.Pow(gameObjects.waterPlanes[activePlane].GetComponent<Renderer>().bounds.extents.x, 2f))
			{
				underWater = false;
			}
			else if (underWater && base.transform.position.y > gameObjects.waterPlanes[activePlane].transform.position.y)
			{
				underWater = false;
			}
			else if (!underWater)
			{
				underWater = CheckIfUnderWater(gameObjects.waterPlanes.Count);
			}
		}
		else if ((underWater && Mathf.Abs(base.transform.position.x - gameObjects.waterPlanes[activePlane].transform.position.x) > gameObjects.waterPlanes[activePlane].GetComponent<Renderer>().bounds.extents.x) || (underWater && Mathf.Abs(base.transform.position.z - gameObjects.waterPlanes[activePlane].transform.position.z) > gameObjects.waterPlanes[activePlane].GetComponent<Renderer>().bounds.extents.z))
		{
			underWater = false;
		}
		else if (underWater && base.transform.position.y > gameObjects.waterPlanes[activePlane].transform.position.y)
		{
			underWater = false;
		}
		else if (!underWater)
		{
			underWater = CheckIfUnderWater(gameObjects.waterPlanes.Count);
		}
	}

	private void NextFrame()
	{
		if (sprayFrameIndex >= wetLens.sprayFrames.Length - 1)
		{
			sprayFrameIndex = 0;
			CancelInvoke("NextFrame");
		}
		airLensMaterial.SetTexture("_CutoutReferenceTexture", wetLens.sprayFramesCutout[sprayFrameIndex]);
		airLensMaterial.SetTexture("_Normal", wetLens.sprayFrames[sprayFrameIndex]);
		sprayFrameIndex++;
	}

	private void BubbleSpawner()
	{
		if (t2 > bubbleSpawnTimer && maxBubbleCount > bubbleCount)
		{
			float num = Random.Range(0f, bubbleSpawnCriteria.avgScaleSummand * 2f);
			bubbleBehaviour.mainCamera = gameObjects.mainCamera;
			bubbleBehaviour.waterLevel = gameObjects.waterPlanes[activePlane].transform.position.y;
			bubbleBehaviour.averageUpdrift = bubbleSpawnCriteria.averageUpdrift + Random.Range((0f - bubbleSpawnCriteria.averageUpdrift) * 0.75f, bubbleSpawnCriteria.averageUpdrift * 0.75f);
			gameObjects.bubble.transform.localScale += new Vector3(num, num, num);
			Object.Instantiate(gameObjects.bubble, new Vector3(base.transform.position.x + Random.Range(0f - bubbleSpawnCriteria.maxSpawnDistance, bubbleSpawnCriteria.maxSpawnDistance), base.transform.position.y - 0.4f, base.transform.position.z + Random.Range(0f - bubbleSpawnCriteria.maxSpawnDistance, bubbleSpawnCriteria.maxSpawnDistance)), Quaternion.identity);
			bubbleSpawnTimer += Random.Range(bubbleSpawnCriteria.minSpawnTimer, bubbleSpawnCriteria.maxSpawnTimer);
			bubbleCount++;
			gameObjects.bubble.transform.localScale = new Vector3(bubbleSpawnCriteria.baseScale, bubbleSpawnCriteria.baseScale, bubbleSpawnCriteria.baseScale);
		}
		else if (t2 > bubbleSpawnTimer && maxBubbleCount == bubbleCount)
		{
			float num2 = Random.Range(0f, bubbleSpawnCriteria.avgScaleSummand * 2f);
			bubbleBehaviour.mainCamera = gameObjects.mainCamera;
			bubbleBehaviour.waterLevel = gameObjects.waterPlanes[activePlane].transform.position.y;
			bubbleBehaviour.averageUpdrift = bubbleSpawnCriteria.averageUpdrift + Random.Range((0f - bubbleSpawnCriteria.averageUpdrift) * 0.75f, bubbleSpawnCriteria.averageUpdrift * 0.75f);
			gameObjects.bubble.transform.localScale += new Vector3(num2, num2, num2);
			Object.Instantiate(gameObjects.bubble, new Vector3(base.transform.position.x + Random.Range(0f - bubbleSpawnCriteria.maxSpawnDistance, bubbleSpawnCriteria.maxSpawnDistance), base.transform.position.y - 0.4f, base.transform.position.z + Random.Range(0f - bubbleSpawnCriteria.maxSpawnDistance, bubbleSpawnCriteria.maxSpawnDistance)), Quaternion.identity);
			bubbleSpawnTimer += Random.Range(bubbleSpawnCriteria.minSpawnTimerL, bubbleSpawnCriteria.maxSpawnTimerL);
			gameObjects.bubble.transform.localScale = new Vector3(bubbleSpawnCriteria.baseScale, bubbleSpawnCriteria.baseScale, bubbleSpawnCriteria.baseScale);
		}
	}
}
