using UnityEngine.EventSystems;

namespace Battlehub.UIControls
{
	public class RectTransformChangeListener : UIBehaviour
	{
		public event RectTransformChanged RectTransformChanged;

		protected override void OnRectTransformDimensionsChange()
		{
			if (this.RectTransformChanged != null)
			{
				this.RectTransformChanged();
			}
		}
	}
}
