using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	public bool on = true;

	public CameraTarget.CameraLocation lookAtCameraLocation;

	public bool lockXZ;

	public bool useTargetUp;

	public bool useCameraRotationIfMonitor;

	public bool lockXZIfMonitor;

	public bool lockZLocalPositionIfMonitor;

	private void LookAt()
	{
		CameraTarget cameraTarget = null;
		Transform transform = null;
		if (lookAtCameraLocation != 0)
		{
			switch (lookAtCameraLocation)
			{
			case CameraTarget.CameraLocation.Center:
				if (CameraTarget.centerTarget != null)
				{
					cameraTarget = CameraTarget.centerTarget;
					transform = CameraTarget.centerTarget.transform;
				}
				break;
			case CameraTarget.CameraLocation.Left:
				if (CameraTarget.leftTarget != null)
				{
					cameraTarget = CameraTarget.leftTarget;
					transform = CameraTarget.leftTarget.transform;
				}
				break;
			case CameraTarget.CameraLocation.Right:
				if (CameraTarget.rightTarget != null)
				{
					cameraTarget = CameraTarget.rightTarget;
					transform = CameraTarget.rightTarget.transform;
				}
				break;
			}
		}
		if (!(cameraTarget != null))
		{
			return;
		}
		if (lockZLocalPositionIfMonitor && cameraTarget.isMonitorCamera)
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition.z = 0f;
			base.transform.localPosition = localPosition;
		}
		if (useCameraRotationIfMonitor && cameraTarget.isMonitorCamera)
		{
			base.transform.rotation = transform.rotation;
			base.transform.Rotate(0f, 180f, 0f);
		}
		else if (lockXZ || (lockXZIfMonitor && cameraTarget.isMonitorCamera))
		{
			Vector3 position = transform.position;
			position.y = base.transform.position.y;
			if (useTargetUp)
			{
				base.transform.LookAt(position, transform.up);
			}
			else
			{
				base.transform.LookAt(position);
			}
		}
		else
		{
			base.transform.LookAt(transform);
		}
	}

	private void Update()
	{
		LookAt();
	}
}
