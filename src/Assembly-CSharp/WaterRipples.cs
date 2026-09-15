using System;
using System.Threading;
using UnityEngine;

public class WaterRipples : MonoBehaviour
{
	[Range(20f, 200f)]
	public int UpdateFPS = 30;

	public bool Multithreading = true;

	public int DisplacementResolution = 128;

	public float Damping = 0.005f;

	[Range(0.0001f, 2f)]
	public float Speed = 1.5f;

	public bool UseSmoothWaves;

	public bool UseProjectedWaves;

	public Texture2D CutOutTexture;

	private Transform t;

	private float textureColorMultiplier = 10f;

	private Texture2D displacementTexture;

	private Vector2[,] waveAcceleration;

	private Color[] col;

	private Vector3[] wavePoints;

	private Vector3 scaleBounds;

	private float inversedDamping;

	private float[] cutOutTextureGray;

	private bool cutOutTextureInitialized;

	private Thread thread;

	private bool canUpdate = true;

	private double threadDeltaTime;

	private DateTime oldDateTime;

	private Vector2 movedObjPos;

	private Vector2 projectorPosition;

	private Vector4 _GAmplitude;

	private Vector4 _GFrequency;

	private Vector4 _GSteepness;

	private Vector4 _GSpeed;

	private Vector4 _GDirectionAB;

	private Vector4 _GDirectionCD;

	private void OnEnable()
	{
		canUpdate = true;
		Shader.EnableKeyword("ripples_on");
		Renderer component = GetComponent<Renderer>();
		_GAmplitude = component.sharedMaterial.GetVector("_GAmplitude");
		_GFrequency = component.sharedMaterial.GetVector("_GFrequency");
		_GSteepness = component.sharedMaterial.GetVector("_GSteepness");
		_GSpeed = component.sharedMaterial.GetVector("_GSpeed");
		_GDirectionAB = component.sharedMaterial.GetVector("_GDirectionAB");
		_GDirectionCD = component.sharedMaterial.GetVector("_GDirectionCD");
		t = base.transform;
		scaleBounds = GetComponent<MeshRenderer>().bounds.size;
		InitializeRipple();
		if (Multithreading)
		{
			thread = new Thread(UpdateRipples);
			thread.Start();
		}
	}

	public Vector3 GetOffsetByPosition(Vector3 position)
	{
		Vector3 result = GerstnerOffset4(new Vector2(position.x, position.z), _GSteepness, _GAmplitude, _GFrequency, _GSpeed, _GDirectionAB, _GDirectionCD);
		result.y += GetTextureHeightByPosition(position.x, position.z);
		result.y += t.position.y;
		return result;
	}

	public void CreateRippleByPosition(Vector3 position, float velocity)
	{
		position.x += scaleBounds.x / 2f - t.position.x;
		position.z += scaleBounds.z / 2f - t.position.z;
		position.x /= scaleBounds.x;
		position.z /= scaleBounds.z;
		position.x *= DisplacementResolution;
		position.z *= DisplacementResolution;
		SetRippleTexture((int)position.x, (int)position.z, velocity);
	}

	private void InitializeRipple()
	{
		inversedDamping = 1f - Damping;
		displacementTexture = new Texture2D(DisplacementResolution, DisplacementResolution, TextureFormat.RGBA32, false);
		displacementTexture.wrapMode = TextureWrapMode.Clamp;
		displacementTexture.filterMode = FilterMode.Bilinear;
		Shader.SetGlobalTexture("_WaterDisplacementTexture", displacementTexture);
		wavePoints = new Vector3[DisplacementResolution * DisplacementResolution];
		col = new Color[DisplacementResolution * DisplacementResolution];
		waveAcceleration = new Vector2[DisplacementResolution, DisplacementResolution];
		for (int i = 0; i < DisplacementResolution * DisplacementResolution; i++)
		{
			col[i] = new Color(0f, 0f, 0f);
			wavePoints[i] = new Vector3(0f, 0f);
		}
		for (int j = 0; j < DisplacementResolution; j++)
		{
			for (int k = 0; k < DisplacementResolution; k++)
			{
				waveAcceleration[j, k] = new Vector2(0f, 0f);
			}
		}
		if (CutOutTexture != null)
		{
			Texture2D texture2D = ScaleTexture(CutOutTexture, DisplacementResolution, DisplacementResolution);
			Color[] pixels = texture2D.GetPixels();
			cutOutTextureGray = new float[DisplacementResolution * DisplacementResolution];
			for (int l = 0; l < pixels.Length; l++)
			{
				cutOutTextureGray[l] = pixels[l].r * 0.299f + pixels[l].g * 0.587f + pixels[l].b * 0.114f;
			}
			cutOutTextureInitialized = true;
		}
	}

	private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
	{
		Texture2D texture2D = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);
		Color[] pixels = source.GetPixels();
		texture2D.SetPixels(pixels);
		TextureScale.Bilinear(texture2D, targetWidth, targetHeight);
		texture2D.Apply();
		return texture2D;
	}

	private void UpdateRipples()
	{
		oldDateTime = DateTime.UtcNow;
		while (canUpdate)
		{
			threadDeltaTime = (DateTime.UtcNow - oldDateTime).TotalMilliseconds / 1000.0;
			oldDateTime = DateTime.UtcNow;
			int num = (int)((double)(1000f / (float)UpdateFPS) - threadDeltaTime);
			if (num > 0)
			{
				Thread.Sleep(num);
			}
			RippleTextureRecalculate();
		}
	}

	private void FixedUpdate()
	{
		if (!Multithreading)
		{
			RippleTextureRecalculate();
		}
		displacementTexture.SetPixels(col);
		displacementTexture.Apply(false);
	}

	private void Update()
	{
		movedObjPos = new Vector2(t.position.x, t.position.z);
	}

	private void UpdateProjector()
	{
		int num = (int)((float)DisplacementResolution * movedObjPos.x / scaleBounds.x - projectorPosition.x);
		int num2 = (int)((float)DisplacementResolution * movedObjPos.y / scaleBounds.z - projectorPosition.y);
		projectorPosition.x += num;
		projectorPosition.y += num2;
		if (num == 0 && num2 == 0)
		{
			return;
		}
		if (num >= 0 && num2 >= 0)
		{
			for (int i = 1; i < DisplacementResolution; i++)
			{
				for (int j = 0; j < DisplacementResolution; j++)
				{
					if (i + num2 > 0 && i + num2 < DisplacementResolution && j + num > 0 && j + num < DisplacementResolution)
					{
						waveAcceleration[j, i] = waveAcceleration[j + num, i + num2];
						wavePoints[j + i * DisplacementResolution] = wavePoints[j + num + (i + num2) * DisplacementResolution];
					}
				}
			}
		}
		if (num >= 0 && num2 < 0)
		{
			for (int num3 = DisplacementResolution - 1; num3 >= 0; num3--)
			{
				for (int k = 0; k < DisplacementResolution; k++)
				{
					if (num3 + num2 > 0 && num3 + num2 < DisplacementResolution && k + num > 0 && k + num < DisplacementResolution)
					{
						waveAcceleration[k, num3] = waveAcceleration[k + num, num3 + num2];
						wavePoints[k + num3 * DisplacementResolution] = wavePoints[k + num + (num3 + num2) * DisplacementResolution];
					}
				}
			}
		}
		if (num < 0 && num2 >= 0)
		{
			for (int l = 0; l < DisplacementResolution; l++)
			{
				for (int num4 = DisplacementResolution - 1; num4 >= 0; num4--)
				{
					if (l + num2 > 0 && l + num2 < DisplacementResolution && num4 + num > 0 && num4 + num < DisplacementResolution)
					{
						waveAcceleration[num4, l] = waveAcceleration[num4 + num, l + num2];
						wavePoints[num4 + l * DisplacementResolution] = wavePoints[num4 + num + (l + num2) * DisplacementResolution];
					}
				}
			}
		}
		if (num < 0 && num2 < 0)
		{
			for (int num5 = DisplacementResolution - 1; num5 >= 0; num5--)
			{
				for (int num6 = DisplacementResolution - 1; num6 >= 0; num6--)
				{
					if (num5 + num2 > 0 && num5 + num2 < DisplacementResolution && num6 + num > 0 && num6 + num < DisplacementResolution)
					{
						waveAcceleration[num6, num5] = waveAcceleration[num6 + num, num5 + num2];
						wavePoints[num6 + num5 * DisplacementResolution] = wavePoints[num6 + num + (num5 + num2) * DisplacementResolution];
					}
				}
			}
		}
		Vector2 zero = Vector2.zero;
		for (int m = 0; m < DisplacementResolution; m++)
		{
			waveAcceleration[0, m] = zero;
			wavePoints[m * DisplacementResolution] = zero;
			waveAcceleration[DisplacementResolution - 1, m] = zero;
			wavePoints[DisplacementResolution - 1 + m * DisplacementResolution] = zero;
			waveAcceleration[m, 0] = zero;
			wavePoints[m] = zero;
			waveAcceleration[m, DisplacementResolution - 1] = zero;
			wavePoints[DisplacementResolution - 1 + m] = zero;
		}
	}

	private void OnDestroy()
	{
		canUpdate = false;
	}

	private void OnDisable()
	{
		Shader.DisableKeyword("ripples_on");
		canUpdate = false;
	}

	private void RippleTextureRecalculate()
	{
		if (UseProjectedWaves)
		{
			UpdateProjector();
		}
		int num = wavePoints.Length;
		int num2 = DisplacementResolution + 1;
		int num3 = DisplacementResolution - 2;
		int num4 = num - (DisplacementResolution + 1);
		for (int i = 0; i < num; i++)
		{
			if (i >= num2 && i < num4 && i % DisplacementResolution > 0)
			{
				int num5 = i % DisplacementResolution;
				int num6 = i / DisplacementResolution;
				float num7 = (wavePoints[i - 1].y + wavePoints[i + 1].y + wavePoints[i - DisplacementResolution].y + wavePoints[i + DisplacementResolution].y) / 4f;
				waveAcceleration[num5, num6].y += num7 - waveAcceleration[num5, num6].x;
			}
		}
		float num8 = Speed;
		if (!Multithreading)
		{
			num8 *= Time.fixedDeltaTime * (float)UpdateFPS;
		}
		for (int j = 0; j < DisplacementResolution; j++)
		{
			for (int k = 0; k < DisplacementResolution; k++)
			{
				waveAcceleration[k, j].x += waveAcceleration[k, j].y * num8;
				if (cutOutTextureInitialized)
				{
					waveAcceleration[k, j].x *= cutOutTextureGray[k + j * DisplacementResolution];
				}
				waveAcceleration[k, j].y *= inversedDamping;
				waveAcceleration[k, j].x *= inversedDamping;
				wavePoints[k + j * DisplacementResolution].y = waveAcceleration[k, j].x;
				if (!UseSmoothWaves)
				{
					float num9 = waveAcceleration[k, j].x * textureColorMultiplier;
					if (num9 >= 0f)
					{
						col[k + j * DisplacementResolution].r = num9;
					}
					else
					{
						col[k + j * DisplacementResolution].g = 0f - num9;
					}
				}
			}
		}
		if (!UseSmoothWaves)
		{
			return;
		}
		for (int l = 2; l < num3; l++)
		{
			for (int m = 2; m < num3; m++)
			{
				float num9 = (wavePoints[m + l * DisplacementResolution - 2].y * 0.2f + wavePoints[m + l * DisplacementResolution - 1].y * 0.4f + wavePoints[m + l * DisplacementResolution].y * 0.6f + wavePoints[m + l * DisplacementResolution + 1].y * 0.4f + wavePoints[m + l * DisplacementResolution + 2].y * 0.2f) / 1.6f * textureColorMultiplier;
				if (num9 >= 0f)
				{
					col[m + l * DisplacementResolution].r = num9;
				}
				else
				{
					col[m + l * DisplacementResolution].g = 0f - num9;
				}
			}
		}
		for (int n = 2; n < num3; n++)
		{
			for (int num10 = 2; num10 < num3; num10++)
			{
				float num9 = (wavePoints[num10 + n * DisplacementResolution - 2].y * 0.2f + wavePoints[num10 + n * DisplacementResolution - 1].y * 0.4f + wavePoints[num10 + n * DisplacementResolution].y * 0.6f + wavePoints[num10 + n * DisplacementResolution + 1].y * 0.4f + wavePoints[num10 + n * DisplacementResolution + 2].y * 0.2f) / 1.6f * textureColorMultiplier;
				if (num9 >= 0f)
				{
					col[num10 + n * DisplacementResolution].r = num9;
				}
				else
				{
					col[num10 + n * DisplacementResolution].g = 0f - num9;
				}
			}
		}
	}

	private void SetRippleTexture(int x, int y, float strength)
	{
		strength /= 100f;
		if (x >= 2 && x < DisplacementResolution - 2 && y >= 2 && y < DisplacementResolution - 2)
		{
			waveAcceleration[x, y].y -= strength;
			waveAcceleration[x + 1, y].y -= strength * 0.8f;
			waveAcceleration[x - 1, y].y -= strength * 0.8f;
			waveAcceleration[x, y + 1].y -= strength * 0.8f;
			waveAcceleration[x, y - 1].y -= strength * 0.8f;
			waveAcceleration[x + 1, y + 1].y -= strength * 0.7f;
			waveAcceleration[x + 1, y - 1].y -= strength * 0.7f;
			waveAcceleration[x - 1, y + 1].y -= strength * 0.7f;
			waveAcceleration[x - 1, y - 1].y -= strength * 0.7f;
			if (x >= 3 && x < DisplacementResolution - 3 && y >= 3 && y < DisplacementResolution - 3)
			{
				waveAcceleration[x + 2, y].y -= strength * 0.5f;
				waveAcceleration[x - 2, y].y -= strength * 0.5f;
				waveAcceleration[x, y + 2].y -= strength * 0.5f;
				waveAcceleration[x, y - 2].y -= strength * 0.5f;
			}
		}
	}

	private float GetTextureHeightByPosition(float x, float y)
	{
		x /= scaleBounds.x;
		y /= scaleBounds.y;
		x *= (float)DisplacementResolution;
		y *= (float)DisplacementResolution;
		if (x >= (float)DisplacementResolution || y >= (float)DisplacementResolution || x < 0f || y < 0f)
		{
			return 0f;
		}
		return waveAcceleration[(int)x, (int)y].x * textureColorMultiplier;
	}

	private Vector3 GerstnerOffset4(Vector2 xzVtx, Vector4 _GSteepness, Vector4 _GAmplitude, Vector4 _GFrequency, Vector4 _GSpeed, Vector4 _GDirectionAB, Vector4 _GDirectionCD)
	{
		Vector3 result = default(Vector3);
		float num = _GSteepness.x * _GAmplitude.x;
		float num2 = _GSteepness.y * _GAmplitude.y;
		Vector4 vector = new Vector4(num * _GDirectionAB.x, num * _GDirectionAB.y, num2 * _GDirectionAB.z, num2 * _GDirectionAB.w);
		Vector4 vector2 = new Vector4(_GSteepness.z * _GAmplitude.z * _GDirectionCD.x, _GSteepness.z * _GAmplitude.z * _GDirectionCD.y, _GSteepness.w * _GAmplitude.w * _GDirectionCD.z, _GSteepness.w * _GAmplitude.w * _GDirectionCD.w);
		float num3 = Vector2.Dot(new Vector2(_GDirectionAB.x, _GDirectionAB.y), xzVtx);
		float num4 = Vector2.Dot(new Vector2(_GDirectionAB.z, _GDirectionAB.w), xzVtx);
		float num5 = Vector2.Dot(new Vector2(_GDirectionCD.x, _GDirectionCD.y), xzVtx);
		float num6 = Vector2.Dot(new Vector2(_GDirectionCD.z, _GDirectionCD.w), xzVtx);
		Vector4 vector3 = new Vector4(num3 * _GFrequency.x, num4 * _GFrequency.y, num5 * _GFrequency.z, num6 * _GFrequency.w);
		Vector4 vector4 = new Vector4(Time.time * _GSpeed.x % 6.2831f, Time.time * _GSpeed.y % 6.2831f, Time.time * _GSpeed.z % 6.2831f, Time.time * _GSpeed.w % 6.2831f);
		Vector4 a = new Vector4(Mathf.Cos(vector3.x + vector4.x), Mathf.Cos(vector3.y + vector4.y), Mathf.Cos(vector3.z + vector4.z), Mathf.Cos(vector3.w + vector4.w));
		Vector4 a2 = new Vector4(Mathf.Sin(vector3.x + vector4.x), Mathf.Sin(vector3.y + vector4.y), Mathf.Sin(vector3.z + vector4.z), Mathf.Sin(vector3.w + vector4.w));
		result.x = Vector4.Dot(a, new Vector4(vector.x, vector.z, vector2.x, vector2.z));
		result.z = Vector4.Dot(a, new Vector4(vector.y, vector.w, vector2.y, vector2.w));
		result.y = Vector4.Dot(a2, _GAmplitude);
		return result;
	}
}
