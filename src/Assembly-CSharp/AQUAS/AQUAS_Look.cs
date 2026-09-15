using System.Collections.Generic;
using UnityEngine;

namespace AQUAS
{
	public class AQUAS_Look : MonoBehaviour
	{
		[Header("Info")]
		private List<float> _rotArrayX = new List<float>();

		private List<float> _rotArrayY = new List<float>();

		private float rotAverageX;

		private float rotAverageY;

		private float mouseDeltaX;

		private float mouseDeltaY;

		[Header("Settings")]
		public bool _isLocked;

		public float _sensitivityX = 1.5f;

		public float _sensitivityY = 1.5f;

		[Tooltip("The more steps, the smoother it will be.")]
		public int _averageFromThisManySteps = 3;

		[Header("References")]
		[Tooltip("Object to be rotated when mouse moves left/right.")]
		public Transform _playerRootT;

		[Tooltip("Object to be rotated when mouse moves up/down.")]
		public Transform _cameraT;

		private void Update()
		{
			MouseLookAveraged();
		}

		private void MouseLookAveraged()
		{
			rotAverageX = 0f;
			rotAverageY = 0f;
			mouseDeltaX = 0f;
			mouseDeltaY = 0f;
			mouseDeltaX += Input.GetAxis("Mouse X") * _sensitivityX;
			mouseDeltaY += Input.GetAxis("Mouse Y") * _sensitivityY;
			_rotArrayX.Add(mouseDeltaX);
			_rotArrayY.Add(mouseDeltaY);
			if (_rotArrayX.Count >= _averageFromThisManySteps)
			{
				_rotArrayX.RemoveAt(0);
			}
			if (_rotArrayY.Count >= _averageFromThisManySteps)
			{
				_rotArrayY.RemoveAt(0);
			}
			for (int i = 0; i < _rotArrayX.Count; i++)
			{
				rotAverageX += _rotArrayX[i];
			}
			for (int j = 0; j < _rotArrayY.Count; j++)
			{
				rotAverageY += _rotArrayY[j];
			}
			rotAverageX /= _rotArrayX.Count;
			rotAverageY /= _rotArrayY.Count;
			_playerRootT.Rotate(0f, rotAverageX, 0f, Space.World);
			_cameraT.Rotate(0f - rotAverageY, 0f, 0f, Space.Self);
		}
	}
}
