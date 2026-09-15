using OldMoatGames;
using UnityEngine;
using UnityEngine.UI;

public class CodeExample : MonoBehaviour
{
	private AnimatedGifPlayer AnimatedGifPlayer;

	public Button PlayButton;

	public Button PauseButton;

	public void Awake()
	{
		AnimatedGifPlayer = GetComponent<AnimatedGifPlayer>();
		AnimatedGifPlayer.FileName = "AnimatedGIFPlayerExampe 3.gif";
		AnimatedGifPlayer.AutoPlay = false;
		AnimatedGifPlayer.OnReady += OnGifLoaded;
		AnimatedGifPlayer.OnLoadError += OnGifLoadError;
		AnimatedGifPlayer.Init();
	}

	private void OnGifLoaded()
	{
		PlayButton.interactable = true;
		Debug.Log("GIF size: width: " + AnimatedGifPlayer.Width + "px, height: " + AnimatedGifPlayer.Height + " px");
	}

	private void OnGifLoadError()
	{
		Debug.Log("Error Loading GIF");
	}

	public void Play()
	{
		AnimatedGifPlayer.Play();
		PlayButton.interactable = false;
		PauseButton.interactable = true;
	}

	public void Pause()
	{
		AnimatedGifPlayer.Pause();
		PlayButton.interactable = true;
		PauseButton.interactable = false;
	}

	public void OnDisable()
	{
		AnimatedGifPlayer.OnReady -= Play;
	}
}
