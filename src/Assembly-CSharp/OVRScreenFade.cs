using System.Collections;
using UnityEngine;

public class OVRScreenFade : MonoBehaviour
{
	[Tooltip("Fade duration")]
	public float fadeTime = 2f;

	[Tooltip("Screen color at maximum fade")]
	public Color fadeColor = new Color(0.01f, 0.01f, 0.01f, 1f);

	public bool fadeOnStart = true;

	public int renderQueue = 5000;

	private float uiFadeAlpha;

	private MeshRenderer fadeRenderer;

	private MeshFilter fadeMesh;

	private Material fadeMaterial;

	private bool isFading;

	public float currentAlpha { get; private set; }

	private void Awake()
	{
		fadeMaterial = new Material(Shader.Find("Oculus/Unlit Transparent Color"));
		fadeMesh = base.gameObject.AddComponent<MeshFilter>();
		fadeRenderer = base.gameObject.AddComponent<MeshRenderer>();
		Mesh mesh = new Mesh();
		fadeMesh.mesh = mesh;
		Vector3[] array = new Vector3[4];
		float num = 2f;
		float num2 = 2f;
		float z = 1f;
		array[0] = new Vector3(0f - num, 0f - num2, z);
		array[1] = new Vector3(num, 0f - num2, z);
		array[2] = new Vector3(0f - num, num2, z);
		array[3] = new Vector3(num, num2, z);
		mesh.vertices = array;
		mesh.triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
		mesh.normals = new Vector3[4]
		{
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		SetFadeLevel(0f);
	}

	public void FadeOut()
	{
		StartCoroutine(Fade(0f, 1f));
	}

	private void OnLevelFinishedLoading(int level)
	{
		StartCoroutine(Fade(1f, 0f));
	}

	private void Start()
	{
		if (fadeOnStart)
		{
			StartCoroutine(Fade(1f, 0f));
		}
	}

	private void OnEnable()
	{
		if (!fadeOnStart)
		{
			SetFadeLevel(0f);
		}
	}

	private void OnDestroy()
	{
		if (fadeRenderer != null)
		{
			Object.Destroy(fadeRenderer);
		}
		if (fadeMaterial != null)
		{
			Object.Destroy(fadeMaterial);
		}
		if (fadeMesh != null)
		{
			Object.Destroy(fadeMesh);
		}
	}

	public void SetUIFade(float level)
	{
		uiFadeAlpha = Mathf.Clamp01(level);
		SetMaterialAlpha();
	}

	public void SetFadeLevel(float level)
	{
		currentAlpha = level;
		SetMaterialAlpha();
	}

	private IEnumerator Fade(float startAlpha, float endAlpha)
	{
		float elapsedTime = 0f;
		while (elapsedTime < fadeTime)
		{
			elapsedTime += Time.deltaTime;
			currentAlpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsedTime / fadeTime));
			SetMaterialAlpha();
			yield return new WaitForEndOfFrame();
		}
	}

	private void SetMaterialAlpha()
	{
		Color color = fadeColor;
		color.a = Mathf.Max(currentAlpha, uiFadeAlpha);
		isFading = color.a > 0f;
		if (fadeMaterial != null)
		{
			fadeMaterial.color = color;
			fadeMaterial.renderQueue = renderQueue;
			fadeRenderer.material = fadeMaterial;
			fadeRenderer.enabled = isFading;
		}
	}
}
