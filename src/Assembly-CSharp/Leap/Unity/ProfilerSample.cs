using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Leap.Unity
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ProfilerSample : IDisposable
	{
		public ProfilerSample(string sampleName)
		{
		}

		public ProfilerSample(string sampleName, UnityEngine.Object obj)
		{
		}

		public void Dispose()
		{
		}
	}
}
