using UnityEngine;
using UnityEngine.UI;

namespace MVR.FileManagement
{
	public class PackageBuilderContentItem : MonoBehaviour
	{
		public Toggle toggle;

		public Text text;

		protected string _itemPath;

		public bool IsSelected
		{
			get
			{
				if (toggle != null)
				{
					return toggle.isOn;
				}
				return false;
			}
		}

		public string ItemPath
		{
			get
			{
				return _itemPath;
			}
			set
			{
				if (_itemPath != value)
				{
					_itemPath = value;
					if (text != null)
					{
						text.text = _itemPath;
					}
				}
			}
		}
	}
}
