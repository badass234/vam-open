using UnityEngine;

public class RenderQueueControl : JSONStorable
{
	protected int _renderQueue = -1;

	public JSONStorableFloat renderQueueJSON;

	protected void SyncMaterials()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(true);
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Material[] materials = renderer.materials;
			Material[] array2 = materials;
			foreach (Material material in array2)
			{
				if (material != null)
				{
					material.renderQueue = _renderQueue;
				}
			}
		}
	}

	protected void SyncRenderQueue(float f)
	{
		_renderQueue = Mathf.FloorToInt(f);
		SyncMaterials();
	}

	protected void Init()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(true);
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Material[] materials = renderer.materials;
			Material[] array2 = materials;
			foreach (Material material in array2)
			{
				if (material != null)
				{
					_renderQueue = material.renderQueue;
					break;
				}
			}
		}
		renderQueueJSON = new JSONStorableFloat("renderQueue", _renderQueue, SyncRenderQueue, -1f, 5000f);
		RegisterFloat(renderQueueJSON);
		SyncMaterials();
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			RenderQueueControlUI componentInChildren = t.GetComponentInChildren<RenderQueueControlUI>(true);
			if (componentInChildren != null && renderQueueJSON != null)
			{
				renderQueueJSON.RegisterSlider(componentInChildren.renderQueueSlider, isAlt);
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			if (Application.isPlaying)
			{
				Init();
				InitUI();
				InitUIAlt();
			}
		}
	}
}
