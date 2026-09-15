using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MaterialOptionTextureGroup
{
	[NonSerialized]
	public MaterialOptions materialOptions;

	public string name = "Texture Group";

	public string textureName = "_MainTex";

	public string secondaryTextureName = "_SpecTex";

	public string thirdTextureName = "_GlossTex";

	public string fourthTextureName = "_AlphaTex";

	public string fifthTextureName = "_BumpMap";

	public string sixthTextureName = "_DecalTex";

	public bool mapTexturesToTextureNames = true;

	public Texture2D autoCreateDefaultTexture;

	public string autoCreateDefaultSetName;

	public string autoCreateTextureFilePrefix;

	public string autoCreateSetNamePrefix;

	public int[] materialSlots;

	public int[] materialSlots2;

	public MaterialOptionTextureSet[] sets;

	protected Dictionary<string, MaterialOptionTextureSet> setNameToSet;

	public MaterialOptionTextureSet GetSetByName(string setName)
	{
		MaterialOptionTextureSet value = null;
		if (setNameToSet == null)
		{
			setNameToSet = new Dictionary<string, MaterialOptionTextureSet>();
			for (int i = 0; i < sets.Length; i++)
			{
				if (setNameToSet.ContainsKey(sets[i].name))
				{
					Debug.LogError("Duplicate material options texture set " + sets[i].name + " found. Must be unique by name");
				}
				else
				{
					setNameToSet.Add(sets[i].name, sets[i]);
				}
			}
		}
		setNameToSet.TryGetValue(setName, out value);
		return value;
	}
}
