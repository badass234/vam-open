using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LeapInternal
{
	public static class StructMarshal<T> where T : struct
	{
		[StructLayout(LayoutKind.Sequential)]
		private class StructContainer
		{
			public T value;
		}

		[ThreadStatic]
		private static StructContainer _container;

		private static int _sizeofT;

		public static int Size => _sizeofT;

		static StructMarshal()
		{
			_sizeofT = Marshal.SizeOf(typeof(T));
		}

		public static void PtrToStruct(IntPtr ptr, out T t)
		{
			if (_container == null)
			{
				_container = new StructContainer();
			}
			try
			{
				Marshal.PtrToStructure(ptr, _container);
				t = _container.value;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				t = default(T);
			}
		}

		public static void ArrayElementToStruct(IntPtr ptr, int arrayIndex, out T t)
		{
			PtrToStruct(new IntPtr(ptr.ToInt64() + _sizeofT * arrayIndex), out t);
		}
	}
}
