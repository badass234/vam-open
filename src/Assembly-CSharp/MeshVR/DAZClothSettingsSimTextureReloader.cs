using System;
using GPUTools.Cloth.Scripts;
using UnityEngine;

namespace MeshVR
{
	public class DAZClothSettingsSimTextureReloader : MonoBehaviour
	{
		protected ClothSettings clothSettings;

		protected ClothEditorType originalEditorType;

		protected float[] originalParticlesBlend;

		protected DAZSkinWrapMaterialOptions[] dazSkinWrapMaterialOptions;

		protected void IndividualSimTextureUpdated()
		{
			ClothSettings[] components = GetComponents<ClothSettings>();
			ClothSettings clothSettings = null;
			ClothSettings[] array = components;
			foreach (ClothSettings clothSettings2 in array)
			{
				if (clothSettings2 != null && clothSettings2.enabled)
				{
					clothSettings = clothSettings2;
				}
			}
			if (clothSettings != null)
			{
				if (originalParticlesBlend == null && clothSettings.GeometryData != null)
				{
					originalEditorType = clothSettings.EditorType;
					originalParticlesBlend = (float[])clothSettings.GeometryData.ParticlesBlend.Clone();
				}
				bool flag = false;
				DAZSkinWrapMaterialOptions[] array2 = dazSkinWrapMaterialOptions;
				foreach (DAZSkinWrapMaterialOptions dAZSkinWrapMaterialOptions in array2)
				{
					if (dAZSkinWrapMaterialOptions.hasCustomSimTexture)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					clothSettings.EditorType = ClothEditorType.Provider;
					if (clothSettings.GeometryData != null)
					{
						clothSettings.GeometryData.ResetParticlesBlend();
					}
				}
				else
				{
					clothSettings.EditorType = originalEditorType;
					if (clothSettings.GeometryData != null)
					{
						for (int k = 0; k < clothSettings.GeometryData.ParticlesBlend.Length; k++)
						{
							clothSettings.GeometryData.ParticlesBlend[k] = originalParticlesBlend[k];
						}
					}
				}
				if (clothSettings.builder != null)
				{
					if (clothSettings.builder.physicsBlend != null)
					{
						clothSettings.builder.physicsBlend.UpdateSettings();
					}
					clothSettings.Reset();
				}
				else
				{
					Debug.LogError("Builder is null");
				}
			}
			else
			{
				Debug.LogError("Cloth settings is null");
			}
		}

		public void SyncSkinWrapMaterialOptions()
		{
			if (dazSkinWrapMaterialOptions != null)
			{
				DAZSkinWrapMaterialOptions[] array = dazSkinWrapMaterialOptions;
				foreach (DAZSkinWrapMaterialOptions dAZSkinWrapMaterialOptions in array)
				{
					dAZSkinWrapMaterialOptions.simTextureLoadedHandlers = (DAZSkinWrapMaterialOptions.SimTextureLoaded)Delegate.Remove(dAZSkinWrapMaterialOptions.simTextureLoadedHandlers, new DAZSkinWrapMaterialOptions.SimTextureLoaded(IndividualSimTextureUpdated));
				}
			}
			dazSkinWrapMaterialOptions = GetComponents<DAZSkinWrapMaterialOptions>();
			DAZSkinWrapMaterialOptions[] array2 = dazSkinWrapMaterialOptions;
			foreach (DAZSkinWrapMaterialOptions dAZSkinWrapMaterialOptions2 in array2)
			{
				dAZSkinWrapMaterialOptions2.simTextureLoadedHandlers = (DAZSkinWrapMaterialOptions.SimTextureLoaded)Delegate.Combine(dAZSkinWrapMaterialOptions2.simTextureLoadedHandlers, new DAZSkinWrapMaterialOptions.SimTextureLoaded(IndividualSimTextureUpdated));
			}
		}
	}
}
