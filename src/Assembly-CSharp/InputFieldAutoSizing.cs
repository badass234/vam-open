using UnityEngine;
using UnityEngine.UI;

public class InputFieldAutoSizing : InputField
{
	public ScrollRect scrollRect;

	public bool keepCursorVisible = true;

	public bool scrollFollowIfAtBottom;

	protected TextGenerator textGenerator;

	protected LayoutElement parentLayoutElement;

	protected int keepAtBottom;

	protected bool preferredHeightDirty;

	protected int lastCaretPosition;

	protected bool scrolledToBottom;

	protected void SyncPreferredHeight()
	{
		if (textGenerator != null && parentLayoutElement != null)
		{
			Vector2 size = base.textComponent.rectTransform.rect.size;
			TextGenerationSettings generationSettings = base.textComponent.GetGenerationSettings(size);
			generationSettings.generateOutOfBounds = false;
			float num = textGenerator.GetPreferredHeight(base.text, generationSettings);
			if (num > base.textComponent.rectTransform.rect.height)
			{
				parentLayoutElement.preferredHeight = num;
			}
			else if (num < base.textComponent.rectTransform.rect.height)
			{
				parentLayoutElement.preferredHeight = num;
			}
			if (keepAtBottom > 0)
			{
				scrollRect.verticalNormalizedPosition = 0f;
			}
		}
	}

	protected void OnValueChange(string s)
	{
		if (scrollFollowIfAtBottom && scrollRect.verticalScrollbar.value < 0.01f)
		{
			keepAtBottom = 2;
		}
		preferredHeightDirty = true;
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			if (preferredHeightDirty)
			{
				SyncPreferredHeight();
				preferredHeightDirty = false;
			}
			if (keepCursorVisible && base.isFocused && lastCaretPosition != base.caretPosition)
			{
				lastCaretPosition = base.caretPosition;
				Vector2 size = base.textComponent.rectTransform.rect.size;
				TextGenerationSettings generationSettings = base.textComponent.GetGenerationSettings(size);
				generationSettings.generateOutOfBounds = false;
				float num = textGenerator.GetPreferredHeight(base.text, generationSettings);
				float num2 = num * 0.5f;
				float num3 = textGenerator.characters[base.caretPosition].cursorPos.y + num2;
				scrollRect.verticalNormalizedPosition = num3 / num;
			}
			if (scrollFollowIfAtBottom && keepAtBottom > 0)
			{
				keepAtBottom--;
				scrollRect.verticalNormalizedPosition = 0f;
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying && scrollFollowIfAtBottom)
		{
			scrollRect.verticalNormalizedPosition = 0f;
			scrollRect.verticalScrollbar.value = 0f;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (Application.isPlaying)
		{
			textGenerator = new TextGenerator();
			if (base.transform.parent != null)
			{
				parentLayoutElement = base.transform.parent.GetComponent<LayoutElement>();
			}
			base.onValueChanged.AddListener(OnValueChange);
			if (scrollFollowIfAtBottom)
			{
				keepAtBottom = 2;
				scrollRect.verticalNormalizedPosition = 0f;
				scrollRect.verticalScrollbar.value = 0f;
			}
			SyncPreferredHeight();
		}
	}
}
