public class GenerateDAZClothingSelectorUI : GenerateDAZDynamicSelectorUI
{
	protected override DAZDynamicItem[] GetDynamicItems()
	{
		return characterSelector.clothingItems;
	}
}
