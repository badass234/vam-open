using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MVRScriptUI : UIProvider
{
	public RectTransform rightUIContent;

	public RectTransform leftUIContent;

	public RectTransform fullWidthUIContent;

	public Button closeButton;

	private IEnumerator FixScrollRect(ScrollRect sr)
	{
		yield return null;
		sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
	}

	private void OnEnable()
	{
		ScrollRect componentInChildren = GetComponentInChildren<ScrollRect>();
		if (componentInChildren != null)
		{
			componentInChildren.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
			StartCoroutine(FixScrollRect(componentInChildren));
		}
	}
}
