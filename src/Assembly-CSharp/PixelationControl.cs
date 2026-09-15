using UnityEngine;
using UnityEngine.UI;

public class PixelationControl : JSONStorable
{
	[SerializeField]
	protected float _pixelation = 0.02f;

	public JSONStorableFloat pixelationJSON;

	protected void SyncMaterials()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Material[] materials = renderer.materials;
			Material[] array2 = materials;
			foreach (Material material in array2)
			{
				if (material != null && material.HasProperty("_Pixelation"))
				{
					material.SetFloat("_Pixelation", _pixelation);
				}
			}
		}
		RawImage[] componentsInChildren2 = GetComponentsInChildren<RawImage>();
		RawImage[] array3 = componentsInChildren2;
		foreach (RawImage rawImage in array3)
		{
			Material materialForRendering = rawImage.materialForRendering;
			if (materialForRendering != null && materialForRendering.HasProperty("_Pixelation"))
			{
				materialForRendering.SetFloat("_Pixelation", _pixelation);
			}
		}
	}

	protected void SyncPixelation(float f)
	{
		_pixelation = f;
		SyncMaterials();
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			PixelationControlUI componentInChildren = t.GetComponentInChildren<PixelationControlUI>(true);
			if (componentInChildren != null)
			{
				pixelationJSON.RegisterSlider(componentInChildren.pixelationSlider, isAlt);
			}
		}
	}

	protected void Init()
	{
		pixelationJSON = new JSONStorableFloat("pixelation", 0.02f, SyncPixelation, 0.001f, 0.1f);
		RegisterFloat(pixelationJSON);
		SyncMaterials();
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
