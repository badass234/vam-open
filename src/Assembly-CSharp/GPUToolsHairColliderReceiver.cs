using GPUTools.Hair.Scripts;

public class GPUToolsHairColliderReceiver : GPUToolsColliderReceiver
{
	public void SyncHairSettings()
	{
		HairSettings[] componentsInChildren = GetComponentsInChildren<HairSettings>(true);
		HairSettings[] array = componentsInChildren;
		foreach (HairSettings hairSettings in array)
		{
			hairSettings.PhysicsSettings.ColliderProviders = _providerGameObjects;
			if (hairSettings.HairBuidCommand != null)
			{
				if (hairSettings.HairBuidCommand.spheres != null)
				{
					hairSettings.HairBuidCommand.spheres.UpdateSettings();
				}
				if (hairSettings.HairBuidCommand.lineSpheres != null)
				{
					hairSettings.HairBuidCommand.lineSpheres.UpdateSettings();
				}
			}
		}
	}

	public override void SyncProviders()
	{
	}
}
