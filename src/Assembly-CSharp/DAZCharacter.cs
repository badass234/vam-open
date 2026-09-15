using UnityEngine;

[ExecuteInEditMode]
public class DAZCharacter : JSONStorableDynamic
{
	public string displayName;

	public string displayNameAlt;

	public string UVname;

	public Texture2D thumbnail;

	public DAZBones rootBonesForSkinning;

	public bool isMale;

	[SerializeField]
	protected DAZSkinV2 _skin;

	public bool useBaseSkinForClothes;

	[SerializeField]
	protected DAZSkinV2 _skinForClothes;

	public DAZSkinV2 skin => _skin;

	public DAZSkinV2 skinForClothes => _skinForClothes;

	protected override void Connect()
	{
		DAZSkinV2 dAZSkinV = GetComponentInChildren<DAZMergedSkinV2>(true);
		DAZSkinV2 componentInChildren = GetComponentInChildren<DAZSkinV2>(true);
		if (dAZSkinV == null)
		{
			dAZSkinV = componentInChildren;
		}
		if (useBaseSkinForClothes)
		{
			_skinForClothes = componentInChildren;
		}
		else
		{
			_skinForClothes = dAZSkinV;
		}
		_skin = dAZSkinV;
		if (rootBonesForSkinning != null)
		{
			DAZSkinV2[] componentsInChildren = GetComponentsInChildren<DAZSkinV2>(true);
			DAZSkinV2[] array = componentsInChildren;
			foreach (DAZSkinV2 dAZSkinV2 in array)
			{
				dAZSkinV2.root = rootBonesForSkinning;
			}
		}
		DAZCharacterMaterialOptions[] componentsInChildren2 = GetComponentsInChildren<DAZCharacterMaterialOptions>();
		DAZCharacterMaterialOptions[] array2 = componentsInChildren2;
		foreach (DAZCharacterMaterialOptions dAZCharacterMaterialOptions in array2)
		{
			dAZCharacterMaterialOptions.skin = _skin;
		}
		DAZSkinControl componentInChildren2 = GetComponentInChildren<DAZSkinControl>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.skin = _skin;
		}
	}
}
