using System;
using UnityEngine;
using UnityEngine.UI;
using Weelco.VRKeyboard;

namespace Weelco
{
	public class VRKeyboardDemo : MonoBehaviour
	{
		public int maxOutputChars = 30;

		public Text inputFieldLabel;

		public VRKeyboardFull keyboard;

		private void Start()
		{
			if ((bool)keyboard)
			{
				VRKeyboardFull vRKeyboardFull = keyboard;
				vRKeyboardFull.OnVRKeyboardBtnClick = (VRKeyboardBase.VRKeyboardBtnClick)Delegate.Combine(vRKeyboardFull.OnVRKeyboardBtnClick, new VRKeyboardBase.VRKeyboardBtnClick(HandleClick));
				keyboard.Init();
			}
		}

		private void OnDestroy()
		{
			if ((bool)keyboard)
			{
				VRKeyboardFull vRKeyboardFull = keyboard;
				vRKeyboardFull.OnVRKeyboardBtnClick = (VRKeyboardBase.VRKeyboardBtnClick)Delegate.Remove(vRKeyboardFull.OnVRKeyboardBtnClick, new VRKeyboardBase.VRKeyboardBtnClick(HandleClick));
			}
		}

		private void HandleClick(string value)
		{
			if (value.Equals("BACK"))
			{
				BackspaceKey();
			}
			else if (value.Equals("ENTER"))
			{
				EnterKey();
			}
			else
			{
				TypeKey(value);
			}
		}

		private void BackspaceKey()
		{
			if (inputFieldLabel.text.Length >= 1)
			{
				inputFieldLabel.text = inputFieldLabel.text.Remove(inputFieldLabel.text.Length - 1, 1);
			}
		}

		private void EnterKey()
		{
		}

		private void TypeKey(string value)
		{
			char[] array = value.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				TypeKey(array[i]);
			}
		}

		private void TypeKey(char key)
		{
			if (inputFieldLabel.text.Length < maxOutputChars)
			{
				inputFieldLabel.text += key;
			}
		}
	}
}
