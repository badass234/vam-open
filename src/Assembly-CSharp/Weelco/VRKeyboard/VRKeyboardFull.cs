using System;

namespace Weelco.VRKeyboard
{
	public class VRKeyboardFull : VRKeyboardBase
	{
		private VRKeyboardButton[] keys;

		private bool areLettersActive = true;

		private bool isLowercase = true;

		private void OnDestroy()
		{
			if (Initialized)
			{
				VRKeyboardButton[] array = keys;
				foreach (VRKeyboardButton vRKeyboardButton in array)
				{
					vRKeyboardButton.OnVRKeyboardBtnClick = (VRKeyboardBtnClick)Delegate.Remove(vRKeyboardButton.OnVRKeyboardBtnClick, new VRKeyboardBtnClick(HandleClick));
				}
			}
		}

		public void Init()
		{
			if (!Initialized)
			{
				keys = base.transform.GetComponentsInChildren<VRKeyboardButton>();
				for (int i = 0; i < keys.Length; i++)
				{
					keys[i].Init();
					keys[i].SetKeyText(VRKeyboardData.allLettersLowercase[i]);
					VRKeyboardButton obj = keys[i];
					obj.OnVRKeyboardBtnClick = (VRKeyboardBtnClick)Delegate.Combine(obj.OnVRKeyboardBtnClick, new VRKeyboardBtnClick(HandleClick));
				}
				Initialized = true;
			}
		}

		private void HandleClick(string value)
		{
			if (value.Equals("sym") || value.Equals("abc"))
			{
				ChangeSpecialLetters();
			}
			else if (value.Equals("UP") || value.Equals("LOW"))
			{
				LowerUpperKeys();
			}
			else if (OnVRKeyboardBtnClick != null)
			{
				OnVRKeyboardBtnClick(value);
			}
		}

		private void ChangeSpecialLetters()
		{
			areLettersActive = !areLettersActive;
			string[] array = ((!areLettersActive) ? VRKeyboardData.allSpecials : ((!isLowercase) ? VRKeyboardData.allLettersUppercase : VRKeyboardData.allLettersLowercase));
			bool flag = false;
			for (int i = 0; i < keys.Length; i++)
			{
				flag = !areLettersActive && array[i].Equals("№");
				keys[i].SetKeyText(array[i], flag);
			}
		}

		private void LowerUpperKeys()
		{
			isLowercase = !isLowercase;
			string[] array = ((!isLowercase) ? VRKeyboardData.allLettersUppercase : VRKeyboardData.allLettersLowercase);
			for (int i = 0; i < keys.Length; i++)
			{
				keys[i].SetKeyText(array[i]);
			}
		}
	}
}
