using UnityEngine;

public class AnimationSequenceClip
{
	public delegate void RemoveCallback();

	public delegate void MoveBackwardCallback();

	public delegate void MoveForwardCallback();

	protected JSONStorableString nameJSON;

	protected JSONStorableBool useCrossFadeJSON;

	protected JSONStorableFloat crossFadeTimeJSON;

	protected JSONStorableBool isPlayingJSON;

	protected JSONStorableFloat playProgressJSON;

	public RemoveCallback removeCallback;

	protected JSONStorableAction removeAction;

	public MoveBackwardCallback moveBackwardCallback;

	protected JSONStorableAction moveBackwardAction;

	public MoveForwardCallback moveForwardCallback;

	protected JSONStorableAction moveForwardAction;

	protected Transform _ui;

	public string Name => nameJSON.val;

	public bool UseCrossFade => useCrossFadeJSON.val;

	public float CrossFadeTime => crossFadeTimeJSON.val;

	public bool IsPlaying
	{
		get
		{
			return isPlayingJSON.val;
		}
		set
		{
			isPlayingJSON.val = value;
		}
	}

	public float PlayProgress
	{
		get
		{
			return playProgressJSON.val;
		}
		set
		{
			playProgressJSON.val = value;
		}
	}

	public Transform UI
	{
		get
		{
			return _ui;
		}
		set
		{
			if (!(_ui != value))
			{
				return;
			}
			if (_ui != null)
			{
				nameJSON.text = null;
				useCrossFadeJSON.toggle = null;
				crossFadeTimeJSON.slider = null;
				isPlayingJSON.indicator = null;
				playProgressJSON.slider = null;
				removeAction.button = null;
				moveBackwardAction.button = null;
				moveForwardAction.button = null;
			}
			_ui = value;
			if (_ui != null)
			{
				AnimationSequenceClipUI component = _ui.GetComponent<AnimationSequenceClipUI>();
				if (component != null)
				{
					nameJSON.text = component.nameText;
					useCrossFadeJSON.indicator = component.useCrossFadeIndicator;
					crossFadeTimeJSON.slider = component.crossFadeTimeSlider;
					isPlayingJSON.indicator = component.isPlayingIndicator;
					playProgressJSON.slider = component.playProgressSlider;
					removeAction.button = component.removeButton;
					moveBackwardAction.button = component.moveBackwardButton;
					moveForwardAction.button = component.moveForwardButton;
				}
			}
		}
	}

	public AnimationSequenceClip(string name, bool useCrossFade, float crossFadeTime)
	{
		nameJSON = new JSONStorableString("name", name);
		useCrossFadeJSON = new JSONStorableBool("useCrossFade", useCrossFade);
		crossFadeTimeJSON = new JSONStorableFloat("crossFadeTime", crossFadeTime, 0f, 5f, true, false);
		isPlayingJSON = new JSONStorableBool("isPlaying", false);
		playProgressJSON = new JSONStorableFloat("playProgress", 0f, 0f, 1f, true, false);
		removeAction = new JSONStorableAction("Remove", Remove);
		moveBackwardAction = new JSONStorableAction("MoveBackward", MoveBackward);
		moveForwardAction = new JSONStorableAction("MoveForward", MoveForward);
	}

	public void Remove()
	{
		if (removeCallback != null)
		{
			removeCallback();
		}
	}

	public void MoveBackward()
	{
		if (moveBackwardCallback != null)
		{
			moveBackwardCallback();
		}
	}

	public void MoveForward()
	{
		if (moveForwardCallback != null)
		{
			moveForwardCallback();
		}
	}
}
