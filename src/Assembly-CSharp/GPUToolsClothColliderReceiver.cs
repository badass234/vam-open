using GPUTools.Cloth.Scripts;

public class GPUToolsClothColliderReceiver : GPUToolsColliderReceiver
{
	public void SyncClothSettings()
	{
		ClothSettings[] componentsInChildren = GetComponentsInChildren<ClothSettings>(true);
		ClothSettings[] array = componentsInChildren;
		foreach (ClothSettings clothSettings in array)
		{
			clothSettings.ColliderProviders = _providerGameObjects;
			if (clothSettings.builder != null)
			{
				if (clothSettings.builder.spheres != null)
				{
					clothSettings.builder.spheres.UpdateSettings();
				}
				if (clothSettings.builder.lineSpheres != null)
				{
					clothSettings.builder.lineSpheres.UpdateSettings();
				}
			}
		}
	}

	public override void SyncProviders()
	{
	}
}
