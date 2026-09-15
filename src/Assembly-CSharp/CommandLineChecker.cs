using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CommandLineChecker : MonoBehaviour
{
	public string LaunchArgument = "default";

	public int LoadLevelId = 1;

	private List<string> CommandArgs;

	private GUIState _guiState;

	private static Vector2 windowSize = new Vector2(250f, 150f);

	private Rect windowRect0 = new Rect((float)(Screen.width / 2) - windowSize.x / 2f, (float)(Screen.height / 2) - windowSize.y / 2f, windowSize.x, windowSize.y);

	private void GetCommandLineArgs()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		CommandArgs = new List<string>();
		string[] array = commandLineArgs;
		foreach (string item in array)
		{
			CommandArgs.Add(item);
		}
	}

	private bool CheckLaunchArg()
	{
		foreach (string commandArg in CommandArgs)
		{
			if (commandArg.Contains("-LaunchArg=" + LaunchArgument))
			{
				return true;
			}
		}
		return false;
	}

	private void GUICommandLineCheckingFailed()
	{
		windowRect0 = GUILayout.Window(0, windowRect0, WindowFunction, "Launch argument fails!");
	}

	private void WindowFunction(int id)
	{
		GUILayout.Label("Launch your game with Launcher! You can't open your game without it! Application will now quit!");
		if (GUILayout.Button("OK"))
		{
			Application.Quit();
		}
		GUI.DragWindow(new Rect((float)(Screen.width / 2) - windowSize.x / 2f - 20f, (float)(Screen.height / 2) - windowSize.y / 2f - 20f, windowSize.x, windowSize.y + 20f));
	}

	private void Start()
	{
		GetCommandLineArgs();
		if (CheckLaunchArg())
		{
			SceneManager.LoadScene(LoadLevelId);
		}
		else
		{
			_guiState = GUIState.COMMAND_LINE_CHECKING_FAILED;
		}
	}

	private void OnGUI()
	{
		GUIState guiState = _guiState;
		if (guiState != 0 && guiState == GUIState.COMMAND_LINE_CHECKING_FAILED)
		{
			GUICommandLineCheckingFailed();
		}
	}
}
