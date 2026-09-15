using UnityEngine;
using UnityEngine.Events;

namespace Leap.Unity
{
	public class Detector : MonoBehaviour
	{
		private bool _isActive;

		[Tooltip("Dispatched when condition is detected.")]
		public UnityEvent OnActivate;

		[Tooltip("Dispatched when condition is no longer detected.")]
		public UnityEvent OnDeactivate;

		protected Color OnColor = Color.green;

		protected Color OffColor = Color.red;

		protected Color LimitColor = Color.blue;

		protected Color DirectionColor = Color.white;

		protected Color NormalColor = Color.gray;

		public bool IsActive => _isActive;

		public virtual void Activate()
		{
			if (!IsActive)
			{
				_isActive = true;
				OnActivate.Invoke();
			}
		}

		public virtual void Deactivate()
		{
			if (IsActive)
			{
				_isActive = false;
				OnDeactivate.Invoke();
			}
		}
	}
}
