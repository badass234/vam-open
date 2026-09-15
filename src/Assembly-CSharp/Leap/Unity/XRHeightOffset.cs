using Leap.Unity.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Leap.Unity
{
	[ExecuteInEditMode]
	public class XRHeightOffset : MonoBehaviour
	{
		[Header("Room-scale Height Offset")]
		[SerializeField]
		[OnEditorChange("roomScaleHeightOffset")]
		[Tooltip("This height offset allows you to place your Rig's base location at the approximate head position of your player during edit-time, while still providing correct cross-platform XR rig heights. If the tracking space type is detected as RoomScale, the Rig will be shifted DOWN by this height on Start, matching the expected floor height for, e.g., SteamVR, while the rig remains unchanged for Android VR and Oculus single-camera targets. Use the magenta gizmo as a reference; the circles represent where your floor will be in a Room-scale experience.")]
		[MinValue(0f)]
		private float _roomScaleHeightOffset = 1.6f;

		private float _lastKnownHeightOffset;

		[Header("Auto Recenter")]
		[FormerlySerializedAs("autoRecenterOnUserPresence")]
		[Tooltip("If the detected XR device is present and supports userPresence, checking this option will detect when userPresence changes from false to true and call InputTracking.Recenter. Supported in 2017.2 and newer.")]
		public bool recenterOnUserPresence = true;

		[Tooltip("Calls InputTracking.Recenter on Start().")]
		public bool recenterOnStart = true;

		[Tooltip("If enabled, InputTracking.Recenter will be called when the assigned key is pressed.")]
		public bool recenterOnKey;

		[Tooltip("When this key is pressed, InputTracking.Recenter will be called.")]
		public KeyCode recenterKey = KeyCode.R;

		private bool _lastUserPresence;

		[Header("Runtime Height Adjustment")]
		[Tooltip("If enabled, then you can use the chosen keys to step the player's height up and down at runtime.")]
		public bool enableRuntimeAdjustment = true;

		[DisableIf("enableRuntimeAdjustment", false, null)]
		[Tooltip("Press this key on the keyboard to adjust the height offset up by stepSize.")]
		public KeyCode stepUpKey = KeyCode.UpArrow;

		[DisableIf("enableRuntimeAdjustment", false, null)]
		[Tooltip("Press this key on the keyboard to adjust the height offset down by stepSize.")]
		public KeyCode stepDownKey = KeyCode.DownArrow;

		[DisableIf("enableRuntimeAdjustment", false, null)]
		public float stepSize = 0.1f;

		public float roomScaleHeightOffset
		{
			get
			{
				return _roomScaleHeightOffset;
			}
			set
			{
				_roomScaleHeightOffset = value;
				base.transform.position += base.transform.up * (_roomScaleHeightOffset - _lastKnownHeightOffset);
				_lastKnownHeightOffset = value;
			}
		}

		private void Start()
		{
			_lastKnownHeightOffset = _roomScaleHeightOffset;
			if (XRSupportUtil.IsRoomScale())
			{
				base.transform.position -= base.transform.up * _roomScaleHeightOffset;
			}
			if (recenterOnStart)
			{
				XRSupportUtil.Recenter();
			}
		}

		private void Update()
		{
			if (!Application.isPlaying || !XRSupportUtil.IsXRDevicePresent())
			{
				return;
			}
			if (enableRuntimeAdjustment)
			{
				if (Input.GetKeyDown(stepUpKey))
				{
					roomScaleHeightOffset += stepSize;
				}
				if (Input.GetKeyDown(stepDownKey))
				{
					roomScaleHeightOffset -= stepSize;
				}
			}
			if (recenterOnUserPresence && !XRSupportUtil.IsRoomScale())
			{
				bool flag = XRSupportUtil.IsUserPresent();
				if (_lastUserPresence != flag)
				{
					if (flag)
					{
						XRSupportUtil.Recenter();
					}
					_lastUserPresence = flag;
				}
			}
			if (recenterOnKey && Input.GetKeyDown(recenterKey))
			{
				XRSupportUtil.Recenter();
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.Lerp(Color.magenta, Color.white, 0.3f).WithAlpha(0.5f);
			float num = roomScaleHeightOffset;
			int num2 = 32;
			float num3 = num * (float)num2;
			float num4 = num / num3;
			Vector3 position = base.transform.position;
			Vector3 vector = base.transform.rotation * Vector3.down;
			if (Application.isPlaying && XRSupportUtil.IsRoomScale())
			{
				Vector3 vector2 = Vector3.up * num;
				position += vector2;
			}
			for (int i = 0; (float)i < num3; i += 2)
			{
				Vector3 from = position + vector * num4 * i;
				Vector3 to = position + vector * num4 * (i + 1);
				Gizmos.DrawLine(from, to);
			}
			Vector3 position2 = position + vector * num;
			drawCircle(position2, vector, 0.01f);
			Gizmos.color = Gizmos.color.WithAlpha(0.3f);
			drawCircle(position2, vector, 0.1f);
			Gizmos.color = Gizmos.color.WithAlpha(0.2f);
			drawCircle(position2, vector, 0.2f);
		}

		private void drawCircle(Vector3 position, Vector3 normal, float radius)
		{
			Vector3 vector = normal.Perpendicular() * radius;
			Quaternion quaternion = Quaternion.AngleAxis(11.25f, normal);
			for (int i = 0; i < 32; i++)
			{
				Vector3 vector2 = quaternion * vector;
				Gizmos.DrawLine(position + vector, position + vector2);
				vector = vector2;
			}
		}
	}
}
