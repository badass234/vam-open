using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace uFileBrowser
{
	public class FileButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		public Button button;

		public Image buttonImage;

		public Image fileIcon;

		public RawImage altIcon;

		public Text label;

		public Sprite selectedSprite;

		public Button renameButton;

		public Button deleteButton;

		public Toggle favoriteToggle;

		public Toggle hiddenToggle;

		public Toggle useFileAsTemplateToggle;

		public Text fullPathLabel;

		public RectTransform rectTransform;

		[HideInInspector]
		public string text;

		[HideInInspector]
		public string textLowerInvariant;

		[HideInInspector]
		public string fullPath;

		[HideInInspector]
		public string removedPrefix;

		[HideInInspector]
		public bool isDir;

		[HideInInspector]
		public string imgPath;

		private FileBrowser browser;

		public void OnPointerEnter(PointerEventData eventData)
		{
			if ((bool)browser)
			{
				browser.OnFilePointerEnter(this);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if ((bool)browser)
			{
				browser.OnFilePointerExit(this);
			}
		}

		public void Select()
		{
			button.transition = Selectable.Transition.None;
			buttonImage.overrideSprite = selectedSprite;
		}

		public void Unselect()
		{
			button.transition = Selectable.Transition.SpriteSwap;
			buttonImage.overrideSprite = null;
		}

		public void OnClick()
		{
			if ((bool)browser)
			{
				browser.OnFileClick(this);
			}
		}

		public void OnRenameClick()
		{
			if ((bool)browser)
			{
				browser.OnRenameClick(this);
			}
		}

		public void OnDeleteClick()
		{
			if ((bool)browser)
			{
				browser.OnDeleteClick(this);
			}
		}

		public void OnHiddenChange(bool b)
		{
			if ((bool)browser)
			{
				browser.OnHiddenChange(this, b);
			}
		}

		public void OnFavoriteChange(bool b)
		{
			if ((bool)browser)
			{
				browser.OnFavoriteChange(this, b);
			}
		}

		public void OnUseFileAsTemplateChange(bool b)
		{
			if ((bool)browser)
			{
				browser.OnUseFileAsTemplateChange(this, b);
			}
		}

		public void Set(FileBrowser b, string txt, string path, bool dir, bool writeable, bool hidden, bool hiddenModifiable, bool favorite, bool allowUseFileAsTemplateSelect, bool isTemplate, bool isTemplateModifiable)
		{
			rectTransform = GetComponent<RectTransform>();
			browser = b;
			text = txt;
			textLowerInvariant = txt.ToLowerInvariant();
			fullPath = path;
			isDir = dir;
			label.text = text;
			if (fullPathLabel != null)
			{
				fullPathLabel.text = fullPath;
			}
			if (isDir)
			{
				fileIcon.sprite = b.folderIcon;
			}
			else
			{
				fileIcon.sprite = b.GetFileIcon(txt);
			}
			if (deleteButton != null)
			{
				deleteButton.gameObject.SetActive(writeable);
			}
			if (renameButton != null)
			{
				renameButton.gameObject.SetActive(writeable);
			}
			if (hiddenToggle != null)
			{
				hiddenToggle.isOn = hidden;
				if (hiddenModifiable)
				{
					hiddenToggle.interactable = true;
					hiddenToggle.onValueChanged.AddListener(OnHiddenChange);
				}
				else
				{
					hiddenToggle.interactable = false;
				}
			}
			if (favoriteToggle != null)
			{
				favoriteToggle.gameObject.SetActive(!dir);
				if (!dir)
				{
					favoriteToggle.isOn = favorite;
					favoriteToggle.onValueChanged.AddListener(OnFavoriteChange);
				}
			}
			if (!(useFileAsTemplateToggle != null))
			{
				return;
			}
			useFileAsTemplateToggle.gameObject.SetActive(allowUseFileAsTemplateSelect && !dir);
			if (allowUseFileAsTemplateSelect)
			{
				useFileAsTemplateToggle.isOn = isTemplate;
				if (isTemplateModifiable)
				{
					useFileAsTemplateToggle.interactable = true;
					useFileAsTemplateToggle.onValueChanged.AddListener(OnUseFileAsTemplateChange);
				}
				else
				{
					useFileAsTemplateToggle.interactable = false;
				}
			}
		}
	}
}
