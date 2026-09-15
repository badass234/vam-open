using UnityEngine.UI;

namespace MeshVR
{
	public class WindControlUI : UIProvider
	{
		public Toggle isGlobalToggle;

		public UIPopup atomPopup;

		public UIPopup receiverPopup;

		public UIPopup receiverTargetPopup;

		public Slider currentMagnitudeSlider;

		public Toggle autoToggle;

		public Slider periodSlider;

		public Slider quicknessSlider;

		public Slider lowerMagnitudeSlider;

		public Slider upperMagnitudeSlider;

		public Slider targetMagnitudeSlider;
	}
}
