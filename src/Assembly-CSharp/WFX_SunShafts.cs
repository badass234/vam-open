using System.Collections.Generic;
using UnityEngine;

public class WFX_SunShafts : MonoBehaviour
{
	public enum SunShaftsResolution
	{
		Low,
		Normal,
		High
	}

	public enum ShaftsScreenBlendMode
	{
		Screen,
		Add
	}

	public SunShaftsResolution resolution = SunShaftsResolution.Normal;

	public ShaftsScreenBlendMode screenBlendMode;

	public Transform sunTransform;

	public int radialBlurIterations = 2;

	public Color sunColor = Color.white;

	public Color sunThreshold = new Color(0.87f, 0.74f, 0.65f);

	public float sunShaftBlurRadius = 2.5f;

	public float sunShaftIntensity = 1.15f;

	public float maxRadius = 0.75f;

	public bool useDepthTexture = true;

	public Shader sunShaftsShader;

	private Material sunShaftsMaterial;

	public Shader simpleClearShader;

	private Material simpleClearMaterial;

	private bool isSupported = true;

	private List<Material> createdMaterials = new List<Material>();

	protected void NotSupported()
	{
		base.enabled = false;
		isSupported = false;
	}

	private bool CheckSupport(bool needDepth)
	{
		isSupported = true;
		if (!SystemInfo.supportsImageEffects)
		{
			NotSupported();
			return false;
		}
		if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			NotSupported();
			return false;
		}
		if (needDepth)
		{
			GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		}
		return true;
	}

	protected Material CheckShaderAndCreateMaterial(Shader s, Material m2Create)
	{
		if (!s)
		{
			Debug.Log("Missing shader in " + ToString());
			base.enabled = false;
			return null;
		}
		if (s.isSupported && (bool)m2Create && m2Create.shader == s)
		{
			return m2Create;
		}
		if (!s.isSupported)
		{
			NotSupported();
			Debug.Log("The shader " + s.ToString() + " on effect " + ToString() + " is not supported on this platform!");
			return null;
		}
		m2Create = new Material(s);
		createdMaterials.Add(m2Create);
		m2Create.hideFlags = HideFlags.DontSave;
		return m2Create;
	}

	protected void ReportAutoDisable()
	{
		Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
	}

	public bool CheckResources()
	{
		CheckSupport(useDepthTexture);
		sunShaftsShader = Shader.Find("Hidden/SunShaftsComposite");
		simpleClearShader = Shader.Find("Hidden/SimpleClear");
		sunShaftsMaterial = CheckShaderAndCreateMaterial(sunShaftsShader, sunShaftsMaterial);
		simpleClearMaterial = CheckShaderAndCreateMaterial(simpleClearShader, simpleClearMaterial);
		if (!isSupported)
		{
			ReportAutoDisable();
		}
		return isSupported;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!CheckResources())
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (useDepthTexture)
		{
			GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		}
		int num = 4;
		if (resolution == SunShaftsResolution.Normal)
		{
			num = 2;
		}
		else if (resolution == SunShaftsResolution.High)
		{
			num = 1;
		}
		Vector3 vector = Vector3.one * 0.5f;
		vector = ((!sunTransform) ? new Vector3(0.5f, 0.5f, 0f) : GetComponent<Camera>().WorldToViewportPoint(sunTransform.position));
		int width = source.width / num;
		int height = source.height / num;
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0);
		sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(1f, 1f, 0f, 0f) * sunShaftBlurRadius);
		sunShaftsMaterial.SetVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, maxRadius));
		sunShaftsMaterial.SetVector("_SunThreshold", sunThreshold);
		if (!useDepthTexture)
		{
			RenderTextureFormat format = ((!GetComponent<Camera>().allowHDR) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
			RenderTexture renderTexture = (RenderTexture.active = RenderTexture.GetTemporary(source.width, source.height, 0, format));
			GL.ClearWithSkybox(false, GetComponent<Camera>());
			sunShaftsMaterial.SetTexture("_Skybox", renderTexture);
			Graphics.Blit(source, temporary, sunShaftsMaterial, 3);
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		else
		{
			Graphics.Blit(source, temporary, sunShaftsMaterial, 2);
		}
		radialBlurIterations = Mathf.Clamp(radialBlurIterations, 1, 4);
		float num2 = sunShaftBlurRadius * 0.0013020834f;
		sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(num2, num2, 0f, 0f));
		sunShaftsMaterial.SetVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, maxRadius));
		for (int i = 0; i < radialBlurIterations; i++)
		{
			RenderTexture temporary3 = RenderTexture.GetTemporary(width, height, 0);
			Graphics.Blit(temporary, temporary3, sunShaftsMaterial, 1);
			RenderTexture.ReleaseTemporary(temporary);
			num2 = sunShaftBlurRadius * (((float)i * 2f + 1f) * 6f) / 768f;
			sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(num2, num2, 0f, 0f));
			temporary = RenderTexture.GetTemporary(width, height, 0);
			Graphics.Blit(temporary3, temporary, sunShaftsMaterial, 1);
			RenderTexture.ReleaseTemporary(temporary3);
			num2 = sunShaftBlurRadius * (((float)i * 2f + 2f) * 6f) / 768f;
			sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(num2, num2, 0f, 0f));
		}
		if (vector.z >= 0f)
		{
			sunShaftsMaterial.SetVector("_SunColor", new Vector4(sunColor.r, sunColor.g, sunColor.b, sunColor.a) * sunShaftIntensity);
		}
		else
		{
			sunShaftsMaterial.SetVector("_SunColor", Vector4.zero);
		}
		sunShaftsMaterial.SetTexture("_ColorBuffer", temporary);
		Graphics.Blit(source, destination, sunShaftsMaterial, (screenBlendMode != 0) ? 4 : 0);
		RenderTexture.ReleaseTemporary(temporary);
	}
}
