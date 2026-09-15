using UnityEngine;

public class CopyToClipboard : MonoBehaviour
{
	public void CopyStringToClipboard(string val)
	{
		GUIUtility.systemCopyBuffer = val;
	}
}
