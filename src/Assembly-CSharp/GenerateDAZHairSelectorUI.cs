public class GenerateDAZHairSelectorUI : GenerateDAZDynamicSelectorUI
{
	protected override DAZDynamicItem[] GetDynamicItems()
	{
		return characterSelector.hairItems;
	}
}
