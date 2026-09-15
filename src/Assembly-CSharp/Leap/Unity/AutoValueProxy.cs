using UnityEngine;

namespace Leap.Unity
{
	public abstract class AutoValueProxy : MonoBehaviour, IValueProxy
	{
		[SerializeField]
		[HideInInspector]
		private bool _autoPushingEnabled;

		public bool autoPushingEnabled
		{
			get
			{
				return _autoPushingEnabled;
			}
			set
			{
				_autoPushingEnabled = value;
			}
		}

		public abstract void OnPullValue();

		public abstract void OnPushValue();

		private void LateUpdate()
		{
			if (_autoPushingEnabled)
			{
				OnPushValue();
			}
		}
	}
}
