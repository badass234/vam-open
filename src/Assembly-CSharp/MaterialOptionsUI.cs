using System;
using UnityEngine;
using UnityEngine.UI;

public class MaterialOptionsUI : UIProvider
{
	public InputField customNameField;

	public Slider renderQueueSlider;

	public Toggle hideMaterialToggle;

	public Toggle linkToOtherMaterialsToggle;

	public RectTransform onHoverTextureUrlPanel;

	public Text onHoverTextureUrlText;

	public Text color1DisplayNameText;

	public HSVColorPicker color1Picker;

	public RectTransform color1Container;

	public Text color2DisplayNameText;

	public HSVColorPicker color2Picker;

	public RectTransform color2Container;

	public Text color3DisplayNameText;

	public HSVColorPicker color3Picker;

	public RectTransform color3Container;

	public Text param1DisplayNameText;

	public Slider param1Slider;

	public Text param2DisplayNameText;

	public Slider param2Slider;

	public Text param3DisplayNameText;

	public Slider param3Slider;

	public Text param4DisplayNameText;

	public Slider param4Slider;

	public Text param5DisplayNameText;

	public Slider param5Slider;

	public Text param6DisplayNameText;

	public Slider param6Slider;

	public Text param7DisplayNameText;

	public Slider param7Slider;

	public Text param8DisplayNameText;

	public Slider param8Slider;

	public Text param9DisplayNameText;

	public Slider param9Slider;

	public Text param10DisplayNameText;

	public Slider param10Slider;

	public UIPopup textureGroup1Popup;

	public Text textureGroup1Text;

	public UIPopup textureGroup2Popup;

	public Text textureGroup2Text;

	public UIPopup textureGroup3Popup;

	public Text textureGroup3Text;

	public UIPopup textureGroup4Popup;

	public Text textureGroup4Text;

	public UIPopup textureGroup5Popup;

	public Text textureGroup5Text;

	public Button restoreFromDefaultsButton;

	public Button saveToStore1Button;

	public Button restoreFromStore1Button;

	public Button saveToStore2Button;

	public Button restoreFromStore2Button;

	public Button saveToStore3Button;

	public Button restoreFromStore3Button;

	public Button createUVTemplateTextureButton;

	public Button createSimTemplateTextureButton;

	public Button openTextureFolderInExplorerButton;

	public Button customTexture1FileBrowseButton;

	public Button customTexture1ReloadButton;

	public Button customTexture1ClearButton;

	public Button customTexture1NullButton;

	public Button customTexture1DefaultButton;

	public Text customTexture1UrlText;

	public Text customTexture1Label;

	public Slider customTexture1TileXSlider;

	public Slider customTexture1TileYSlider;

	public Slider customTexture1OffsetXSlider;

	public Slider customTexture1OffsetYSlider;

	public Button customTexture2FileBrowseButton;

	public Button customTexture2ReloadButton;

	public Button customTexture2ClearButton;

	public Button customTexture2NullButton;

	public Button customTexture2DefaultButton;

	public Text customTexture2UrlText;

	public Text customTexture2Label;

	public Slider customTexture2TileXSlider;

	public Slider customTexture2TileYSlider;

	public Slider customTexture2OffsetXSlider;

	public Slider customTexture2OffsetYSlider;

	public Button customTexture3FileBrowseButton;

	public Button customTexture3ReloadButton;

	public Button customTexture3ClearButton;

	public Button customTexture3NullButton;

	public Button customTexture3DefaultButton;

	public Text customTexture3UrlText;

	public Text customTexture3Label;

	public Slider customTexture3TileXSlider;

	public Slider customTexture3TileYSlider;

	public Slider customTexture3OffsetXSlider;

	public Slider customTexture3OffsetYSlider;

	public Button customTexture4FileBrowseButton;

	public Button customTexture4ReloadButton;

	public Button customTexture4ClearButton;

	public Button customTexture4NullButton;

	public Button customTexture4DefaultButton;

	public Text customTexture4UrlText;

	public Text customTexture4Label;

	public Slider customTexture4TileXSlider;

	public Slider customTexture4TileYSlider;

	public Slider customTexture4OffsetXSlider;

	public Slider customTexture4OffsetYSlider;

	public Button customTexture5FileBrowseButton;

	public Button customTexture5ReloadButton;

	public Button customTexture5ClearButton;

	public Button customTexture5NullButton;

	public Button customTexture5DefaultButton;

	public Text customTexture5UrlText;

	public Text customTexture5Label;

	public Slider customTexture5TileXSlider;

	public Slider customTexture5TileYSlider;

	public Slider customTexture5OffsetXSlider;

	public Slider customTexture5OffsetYSlider;

	public Button customTexture6FileBrowseButton;

	public Button customTexture6ReloadButton;

	public Button customTexture6ClearButton;

	public Button customTexture6NullButton;

	public Button customTexture6DefaultButton;

	public Text customTexture6UrlText;

	public Text customTexture6Label;

	public Slider customTexture6TileXSlider;

	public Slider customTexture6TileYSlider;

	public Slider customTexture6OffsetXSlider;

	public Slider customTexture6OffsetYSlider;

	protected void OpenHoverTextureUrlPanel(string text)
	{
		if (onHoverTextureUrlPanel != null)
		{
			onHoverTextureUrlPanel.gameObject.SetActive(true);
		}
		if (onHoverTextureUrlText != null)
		{
			onHoverTextureUrlText.text = text;
		}
	}

	protected void CloseHoverTextureUrlPanel()
	{
		if (onHoverTextureUrlPanel != null)
		{
			onHoverTextureUrlPanel.gameObject.SetActive(false);
		}
	}

	protected virtual void Awake()
	{
		if (!(onHoverTextureUrlPanel != null) || !(onHoverTextureUrlText != null))
		{
			return;
		}
		if (customTexture1UrlText != null)
		{
			UIHoverTextNotifier component = customTexture1UrlText.GetComponent<UIHoverTextNotifier>();
			if (component != null)
			{
				component.onEnterNotifier = (UIHoverTextNotifier.TextNotifier)Delegate.Combine(component.onEnterNotifier, (UIHoverTextNotifier.TextNotifier)delegate(Text text)
				{
					OpenHoverTextureUrlPanel(text.text);
				});
				component.onExitNotifier = (UIHoverTextNotifier.TextNotifier)Delegate.Combine(component.onExitNotifier, (UIHoverTextNotifier.TextNotifier)delegate
				{
					CloseHoverTextureUrlPanel();
				});
			}
		}
		if (customTexture2UrlText != null)
		{
			UIHoverTextNotifier component2 = customTexture2UrlText.GetComponent<UIHoverTextNotifier>();
			if (component2 != null)
			{
				component2.onEnterNotifier = (UIHoverTextNotifier.TextNotifier)Delegate.Combine(component2.onEnterNotifier, (UIHoverTextNotifier.TextNotifier)delegate(Text text)
				{
					OpenHoverTextureUrlPanel(text.text);
				});
				component2.onExitNotifier = (UIHoverTextNotifier.TextNotifier)Delegate.Combine(component2.onExitNotifier, (UIHoverTextNotifier.TextNotifier)delegate
				{
					CloseHoverTextureUrlPanel();
				});
			}
		}
		if (customTexture3UrlText != null)
		{
			UIHoverTextNotifier component3 = customTexture3UrlText.GetComponent<UIHoverTextNotifier>();
			if (component3 != null)
			{
				component3.onEnterNotifier = (UIHoverTextNotifier.TextNotifier)Delegate.Combine(component3.onEnterNotifier, (UIHoverTextNotifier.TextNotifier)delegate(Text text)
				{
					OpenHoverTextureUrlPanel(text.text);
				});
				component3.onExitNotifier = (UIHoverTextNotifier.TextNotifier)Delegate.Combine(component3.onExitNotifier, (UIHoverTextNotifier.TextNotifier)delegate
				{
					CloseHoverTextureUrlPanel();
				});
			}
		}
		if (customTexture4UrlText != null)
		{
			UIHoverTextNotifier component4 = customTexture4UrlText.GetComponent<UIHoverTextNotifier>();
			if (component4 != null)
			{
				component4.onEnterNotifier = (UIHoverTextNotifier.TextNotifier)Delegate.Combine(component4.onEnterNotifier, (UIHoverTextNotifier.TextNotifier)delegate(Text text)
				{
					OpenHoverTextureUrlPanel(text.text);
				});
				component4.onExitNotifier = (UIHoverTextNotifier.TextNotifier)Delegate.Combine(component4.onExitNotifier, (UIHoverTextNotifier.TextNotifier)delegate
				{
					CloseHoverTextureUrlPanel();
				});
			}
		}
		if (customTexture5UrlText != null)
		{
			UIHoverTextNotifier component5 = customTexture5UrlText.GetComponent<UIHoverTextNotifier>();
			if (component5 != null)
			{
				component5.onEnterNotifier = (UIHoverTextNotifier.TextNotifier)Delegate.Combine(component5.onEnterNotifier, (UIHoverTextNotifier.TextNotifier)delegate(Text text)
				{
					OpenHoverTextureUrlPanel(text.text);
				});
				component5.onExitNotifier = (UIHoverTextNotifier.TextNotifier)Delegate.Combine(component5.onExitNotifier, (UIHoverTextNotifier.TextNotifier)delegate
				{
					CloseHoverTextureUrlPanel();
				});
			}
		}
		if (!(customTexture6UrlText != null))
		{
			return;
		}
		UIHoverTextNotifier component6 = customTexture6UrlText.GetComponent<UIHoverTextNotifier>();
		if (component6 != null)
		{
			component6.onEnterNotifier = (UIHoverTextNotifier.TextNotifier)Delegate.Combine(component6.onEnterNotifier, (UIHoverTextNotifier.TextNotifier)delegate(Text text)
			{
				OpenHoverTextureUrlPanel(text.text);
			});
			component6.onExitNotifier = (UIHoverTextNotifier.TextNotifier)Delegate.Combine(component6.onExitNotifier, (UIHoverTextNotifier.TextNotifier)delegate
			{
				CloseHoverTextureUrlPanel();
			});
		}
	}
}
