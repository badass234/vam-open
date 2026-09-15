using System;
using System.Collections.Generic;

namespace Leap
{
	public class DeviceList : List<Device>
	{
		public Device ActiveDevice
		{
			get
			{
				if (base.Count == 1)
				{
					return base[0];
				}
				for (int i = 0; i < base.Count; i++)
				{
					if (base[i].IsStreaming)
					{
						return base[i];
					}
				}
				return null;
			}
		}

		public bool IsEmpty => base.Count == 0;

		public Device FindDeviceByHandle(IntPtr deviceHandle)
		{
			for (int i = 0; i < base.Count; i++)
			{
				if (base[i].Handle == deviceHandle)
				{
					return base[i];
				}
			}
			return null;
		}

		public void AddOrUpdate(Device device)
		{
			Device device2 = FindDeviceByHandle(device.Handle);
			if (device2 != null)
			{
				device2.Update(device);
			}
			else
			{
				Add(device);
			}
		}
	}
}
