using System;
using UnityEngine;
using UnityEngine.UI;

namespace Weelco.VRInput
{
	public class UIGazePointer : IUIPointer
	{
		public Transform GazeCanvas;

		public Image GazeProgressBar;

		public float GazeClickTimer;

		public float GazeClickTimerDelay;

		private bool _isOver;

		public override Transform target
		{
			get
			{
				if (GazeCanvas == null)
				{
					throw new NullReferenceException("VRInputSettings::While Gaze Input , must contain Gaze Dot GameObject");
				}
				return GazeCanvas;
			}
		}

		public override void Initialize()
		{
		}

		public override void OnEnterControl(GameObject control)
		{
			_isOver = true;
		}

		public override void OnExitControl(GameObject control)
		{
			_isOver = false;
		}

		public bool IsOver()
		{
			return _isOver;
		}

		protected override void UpdateRaycasting(bool isHit, float distance)
		{
		}
	}
}
