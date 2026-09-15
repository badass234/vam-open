using UnityEngine;
using UnityEngine.UI;

namespace MVR.FileManagement
{
	public class PackageBuilderPackageItem : MonoBehaviour
	{
		public Button button;

		public Text text;

		protected string _package;

		public Image image;

		public string Package
		{
			get
			{
				return _package;
			}
			set
			{
				if (_package != value)
				{
					_package = value;
					if (button != null)
					{
						text.text = _package;
					}
				}
			}
		}

		public void SetColor(Color c)
		{
			if (image != null)
			{
				image.color = c;
			}
		}
	}
}
