using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTHandles
{
	public class EditorHint : MonoBehaviour
	{
		[SerializeField]
		private EditorDemo EditorDemo;

		private void Start()
		{
			string empty = string.Empty;
			Text component = GetComponent<Text>();
			component.text = string.Concat("Right / Mid Mouse Button or Arrows - scene navigation\nMouse Wheel - zoom\n", EditorDemo.FocusKey, " - focus \n", empty, EditorDemo.ModifierKey, " + ", EditorDemo.SnapToGridKey, " - snap to grid \n", EditorDemo.ModifierKey, " + ", EditorDemo.DuplicateKey, " - duplicate object", EditorDemo.DeleteKey, " - delete object \n", EditorDemo.ModifierKey, " + ", EditorDemo.EnterPlayModeKey, " - toggle playmode \nQ W E R - select  handle \nX - toggle coordinate system \nTo create prefab click corresponding button");
		}
	}
}
