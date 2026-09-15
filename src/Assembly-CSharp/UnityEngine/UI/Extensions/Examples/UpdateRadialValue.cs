namespace UnityEngine.UI.Extensions.Examples
{
	public class UpdateRadialValue : MonoBehaviour
	{
		public InputField input;

		public RadialSlider slider;

		private void Start()
		{
		}

		public void UpdateSliderValue()
		{
			float result;
			float.TryParse(input.text, out result);
			slider.Value = result;
		}

		public void UpdateSliderAndle()
		{
			int result;
			int.TryParse(input.text, out result);
			slider.Angle = result;
		}
	}
}
