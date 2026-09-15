using System;
using UnityEngine.UI;

public class DAZSkinWrapMaterialOptionsUI : MaterialOptionsUI
{
	public Button customSimTextureFileBrowseButton;

	public Button customSimTextureReloadButton;

	public Button customSimTextureClearButton;

	public Button customSimTextureNullButton;

	public Button customSimTextureDefaultButton;

	public Text customSimTextureUrlText;

	protected override void Awake()
	{
		base.Awake();
		if (!(onHoverTextureUrlPanel != null) || !(onHoverTextureUrlText != null) || !(customSimTextureUrlText != null))
		{
			return;
		}
		UIHoverTextNotifier component = customSimTextureUrlText.GetComponent<UIHoverTextNotifier>();
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
}
