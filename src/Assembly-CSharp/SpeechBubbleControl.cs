using UnityEngine;
using UnityEngine.UI;

public class SpeechBubbleControl : JSONStorable
{
	public Transform bubbleTransform;

	public Image bubbleImage;

	public Text bubbleText;

	protected float _timeToLive;

	protected JSONStorableFloat bubbleLifetimeJSON;

	protected JSONStorableString bubbleTextJSON;

	protected bool isDisplaying;

	protected void SyncBubbleLifeTime(float f)
	{
	}

	protected void SyncBubbleText(string s)
	{
		UpdateText(s, bubbleLifetimeJSON.val);
	}

	public void UpdateText(string text, float newTimeToLive)
	{
		if (bubbleTransform != null)
		{
			bubbleTransform.gameObject.SetActive(true);
		}
		isDisplaying = true;
		Color color = bubbleImage.color;
		color.a = 1f;
		bubbleImage.color = color;
		color = bubbleText.color;
		color.a = 1f;
		bubbleText.color = color;
		bubbleText.text = text;
		_timeToLive = newTimeToLive;
	}

	protected void Update()
	{
		_timeToLive -= Time.unscaledDeltaTime;
		if (0f < _timeToLive && _timeToLive < 1f)
		{
			Color color = bubbleImage.color;
			color.a = _timeToLive;
			bubbleImage.color = color;
			color = bubbleText.color;
			color.a = _timeToLive;
			bubbleText.color = color;
		}
		if (isDisplaying && _timeToLive < 0f && bubbleTransform != null)
		{
			isDisplaying = false;
			bubbleText.text = string.Empty;
			bubbleTextJSON.valNoCallback = string.Empty;
			bubbleTransform.gameObject.SetActive(false);
		}
	}

	protected void Init()
	{
		bubbleLifetimeJSON = new JSONStorableFloat("bubbleLifetime", 5f, SyncBubbleLifeTime, 0f, 20f, false);
		RegisterFloat(bubbleLifetimeJSON);
		bubbleTextJSON = new JSONStorableString("bubbleText", string.Empty, SyncBubbleText);
		RegisterString(bubbleTextJSON);
		bubbleTextJSON.isStorable = false;
		bubbleTextJSON.isRestorable = false;
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			SpeechBubbleControlUI componentInChildren = UITransform.GetComponentInChildren<SpeechBubbleControlUI>();
			if (componentInChildren != null && bubbleLifetimeJSON != null)
			{
				bubbleLifetimeJSON.slider = componentInChildren.bubbleLifetimeSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			SpeechBubbleControlUI componentInChildren = UITransform.GetComponentInChildren<SpeechBubbleControlUI>();
			if (componentInChildren != null && bubbleLifetimeJSON != null)
			{
				bubbleLifetimeJSON.sliderAlt = componentInChildren.bubbleLifetimeSlider;
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}
}
