using System;
using LeapInternal;

namespace Leap
{
	public class Device : IEquatable<Device>
	{
		public enum DeviceType
		{
			TYPE_INVALID = -1,
			TYPE_PERIPHERAL = 3,
			TYPE_DRAGONFLY = 4354,
			TYPE_NIGHTCRAWLER = 4609,
			TYPE_RIGEL = 4610,
			[Obsolete]
			TYPE_LAPTOP = 4611,
			[Obsolete]
			TYPE_KEYBOARD = 4612
		}

		public IntPtr Handle { get; private set; }

		public float HorizontalViewAngle { get; private set; }

		public float VerticalViewAngle { get; private set; }

		public float Range { get; private set; }

		public float Baseline { get; private set; }

		public bool IsStreaming { get; private set; }

		public DeviceType Type { get; private set; }

		public string SerialNumber { get; private set; }

		public bool IsSmudged
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsLightingBad
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public Device()
		{
		}

		public Device(IntPtr deviceHandle, float horizontalViewAngle, float verticalViewAngle, float range, float baseline, DeviceType type, bool isStreaming, string serialNumber)
		{
			Handle = deviceHandle;
			HorizontalViewAngle = horizontalViewAngle;
			VerticalViewAngle = verticalViewAngle;
			Range = range;
			Baseline = baseline;
			Type = type;
			IsStreaming = isStreaming;
			SerialNumber = serialNumber;
		}

		public void Update(float horizontalViewAngle, float verticalViewAngle, float range, float baseline, bool isStreaming, string serialNumber)
		{
			HorizontalViewAngle = horizontalViewAngle;
			VerticalViewAngle = verticalViewAngle;
			Range = range;
			Baseline = baseline;
			IsStreaming = isStreaming;
			SerialNumber = serialNumber;
		}

		public void Update(Device updatedDevice)
		{
			HorizontalViewAngle = updatedDevice.HorizontalViewAngle;
			VerticalViewAngle = updatedDevice.VerticalViewAngle;
			Range = updatedDevice.Range;
			Baseline = updatedDevice.Baseline;
			IsStreaming = updatedDevice.IsStreaming;
			SerialNumber = updatedDevice.SerialNumber;
		}

		public bool SetPaused(bool pause)
		{
			ulong prior = 0uL;
			ulong set = 0uL;
			ulong clear = 0uL;
			if (pause)
			{
				set = 1uL;
			}
			else
			{
				clear = 1uL;
			}
			eLeapRS eLeapRS = LeapC.SetDeviceFlags(Handle, set, clear, out prior);
			return eLeapRS == eLeapRS.eLeapRS_Success;
		}

		public bool Equals(Device other)
		{
			return SerialNumber == other.SerialNumber;
		}

		public override string ToString()
		{
			return "Device serial# " + SerialNumber;
		}
	}
}
