using UnityEngine;
using UnityEngine.UI;

namespace MVR.FileManagement
{
	public class PackageBuilderReferenceItem : MonoBehaviour
	{
		public Text text;

		protected string _reference;

		public Text issueText;

		protected string _issue;

		public Image image;

		public string Reference
		{
			get
			{
				return _reference;
			}
			set
			{
				if (_reference != value)
				{
					_reference = value;
					if (text != null)
					{
						text.text = _reference;
					}
				}
			}
		}

		public string Issue
		{
			get
			{
				return _issue;
			}
			set
			{
				if (!(_issue != value))
				{
					return;
				}
				_issue = value;
				if (issueText != null)
				{
					if (_issue == string.Empty)
					{
						issueText.gameObject.SetActive(false);
					}
					else
					{
						issueText.gameObject.SetActive(true);
					}
					issueText.text = _issue;
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

		public void SetIssueColor(Color c)
		{
			if (issueText != null)
			{
				issueText.color = c;
			}
		}
	}
}
