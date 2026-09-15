using UnityEngine;
using UnityEngine.UI;

namespace MVR.FileManagement
{
	public class UserConfirmPanel : MonoBehaviour
	{
		public delegate void ResponseCallback();

		public string signature;

		public Text promptText;

		public Button confirmButton;

		public Button confirmStickyButton;

		public Button denyButton;

		public Button denyStickyButton;

		protected ResponseCallback confirmCallback;

		protected ResponseCallback autoConfirmCallback;

		protected ResponseCallback confirmStickyCallback;

		protected ResponseCallback denyCallback;

		protected ResponseCallback autoDenyCallback;

		protected ResponseCallback denyStickyCallback;

		public void SetPrompt(string prompt)
		{
			if (promptText != null)
			{
				promptText.text = prompt;
			}
		}

		public void Confirm()
		{
			if (confirmCallback != null)
			{
				confirmCallback();
			}
		}

		public void SetConfirmCallback(ResponseCallback callback)
		{
			confirmCallback = callback;
			if (confirmButton != null)
			{
				confirmButton.onClick.RemoveAllListeners();
				if (callback != null)
				{
					confirmButton.onClick.AddListener(Confirm);
				}
			}
		}

		public void AutoConfirm()
		{
			if (autoConfirmCallback != null)
			{
				autoConfirmCallback();
			}
		}

		public void SetAutoConfirmCallback(ResponseCallback callback)
		{
			autoConfirmCallback = callback;
		}

		public void ConfirmSticky()
		{
			if (confirmStickyCallback != null)
			{
				confirmStickyCallback();
			}
		}

		public void SetConfirmStickyCallback(ResponseCallback callback)
		{
			confirmStickyCallback = callback;
			if (confirmStickyButton != null)
			{
				confirmStickyButton.onClick.RemoveAllListeners();
				if (callback != null)
				{
					confirmStickyButton.onClick.AddListener(ConfirmSticky);
				}
			}
		}

		public void Deny()
		{
			if (denyCallback != null)
			{
				denyCallback();
			}
		}

		public void SetDenyCallback(ResponseCallback callback)
		{
			denyCallback = callback;
			if (denyButton != null)
			{
				denyButton.onClick.RemoveAllListeners();
				if (callback != null)
				{
					denyButton.onClick.AddListener(Deny);
				}
			}
		}

		public void AutoDeny()
		{
			if (autoDenyCallback != null)
			{
				autoDenyCallback();
			}
		}

		public void SetAutoDenyCallback(ResponseCallback callback)
		{
			autoDenyCallback = callback;
		}

		public void DenySticky()
		{
			if (denyStickyCallback != null)
			{
				denyStickyCallback();
			}
		}

		public void SetDenyStickyCallback(ResponseCallback callback)
		{
			denyStickyCallback = callback;
			if (denyStickyButton != null)
			{
				denyStickyButton.onClick.RemoveAllListeners();
				if (callback != null)
				{
					denyStickyButton.onClick.AddListener(DenySticky);
				}
			}
		}
	}
}
