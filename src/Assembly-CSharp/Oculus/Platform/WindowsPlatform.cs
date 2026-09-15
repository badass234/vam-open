using System;
using System.Runtime.InteropServices;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform
{
	public class WindowsPlatform
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void UnityLogDelegate(IntPtr tag, IntPtr msg);

		private void CPPLogCallback(IntPtr tag, IntPtr message)
		{
			Debug.Log($"{Marshal.PtrToStringAnsi(tag)}: {Marshal.PtrToStringAnsi(message)}");
		}

		private IntPtr getCallbackPointer()
		{
			return IntPtr.Zero;
		}

		public bool Initialize(string appId)
		{
			if (string.IsNullOrEmpty(appId))
			{
				throw new UnityException("AppID must not be null or empty");
			}
			CAPI.ovr_UnityInitWrapperWindows(appId, getCallbackPointer());
			return true;
		}

		public Request<PlatformInitialize> AsyncInitialize(string appId)
		{
			if (string.IsNullOrEmpty(appId))
			{
				throw new UnityException("AppID must not be null or empty");
			}
			return new Request<PlatformInitialize>(CAPI.ovr_UnityInitWrapperWindowsAsynchronous(appId, getCallbackPointer()));
		}
	}
}
