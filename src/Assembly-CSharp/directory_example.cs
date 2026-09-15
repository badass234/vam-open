using UnityEngine;
using UnityEngine.UI;
using uFileBrowser;

public class directory_example : MonoBehaviour
{
	public FileBrowser browser;

	public Text pathLabel;

	public void ShowButtonClick()
	{
		browser.Show(BrowserClosed);
	}

	public void BrowserClosed(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			Debug.Log("No path selected.");
		}
		else
		{
			pathLabel.text = "You selected:\n" + path;
		}
	}
}
