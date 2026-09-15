using UnityEngine;
using UnityEngine.UI;

namespace Weelco.VRKeyboard
{
	public class VRKeyboardButton : VRKeyboardBase
	{
		private Button button;

		private Text label;

		private Image icon;

		private void OnDestroy()
		{
			if (Initialized)
			{
				button.onClick.RemoveListener(HandleClick);
			}
		}

		public void Init()
		{
			if (!Initialized)
			{
				Transform transform = base.transform.Find("Image");
				if (transform != null)
				{
					icon = transform.GetComponent<Image>();
				}
				label = base.transform.Find("Text").GetComponent<Text>();
				label.enabled = icon == null;
				button = base.transform.GetComponent<Button>();
				button.onClick.AddListener(HandleClick);
				Initialized = true;
			}
		}

		public void SetKeyText(string value, bool ignoreIcon = false)
		{
			label.text = value;
			if (icon != null)
			{
				label.enabled = ignoreIcon;
				icon.enabled = !ignoreIcon;
			}
		}

		private void HandleClick()
		{
			if (OnVRKeyboardBtnClick != null)
			{
				OnVRKeyboardBtnClick(label.text);
			}
		}
	}
}
