using System;
using UnityEngine;
using UnityEngine.UI;

namespace DynamicCSharp.Demo
{
	public sealed class CodeUI : MonoBehaviour
	{
		public static Action<CodeUI> onNewClicked;

		public static Action<CodeUI> onLoadClicked;

		public static Action<CodeUI> onCompileClicked;

		public GameObject codeEditorObject;

		public GameObject helpObject;

		public InputField codeEditor;

		public void Start()
		{
			OnNewClicked();
		}

		public void OnNewClicked()
		{
			if (onNewClicked != null)
			{
				onNewClicked(this);
			}
		}

		public void OnExampleClicked()
		{
			if (onLoadClicked != null)
			{
				onLoadClicked(this);
			}
		}

		public void OnShowHelpClicked()
		{
			helpObject.SetActive(true);
			codeEditorObject.SetActive(false);
		}

		public void OnHideHelpClicked()
		{
			helpObject.SetActive(false);
			codeEditorObject.SetActive(true);
		}

		public void OnRunClicked()
		{
			if (onCompileClicked != null)
			{
				onCompileClicked(this);
			}
		}
	}
}
