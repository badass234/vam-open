using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace LeapInternal
{
	public static class MemoryManager
	{
		private struct PoolKey : IEquatable<PoolKey>
		{
			public eLeapAllocatorType type;

			public uint size;

			public override int GetHashCode()
			{
				return (int)type | (int)(size << 4);
			}

			public bool Equals(PoolKey other)
			{
				return type == other.type && size == other.size;
			}

			public override bool Equals(object obj)
			{
				if (obj is PoolKey)
				{
					return Equals((PoolKey)obj);
				}
				return false;
			}
		}

		private struct ActiveMemoryInfo
		{
			public GCHandle handle;

			public PoolKey key;
		}

		public static bool EnablePooling = false;

		public static uint MinPoolSize = 8u;

		private static Dictionary<IntPtr, ActiveMemoryInfo> _activeMemory = new Dictionary<IntPtr, ActiveMemoryInfo>();

		private static Dictionary<PoolKey, Queue<object>> _pooledMemory = new Dictionary<PoolKey, Queue<object>>();

		[MonoPInvokeCallback(typeof(Allocate))]
		public static IntPtr Pin(uint size, eLeapAllocatorType typeHint, IntPtr state)
		{
			try
			{
				PoolKey poolKey = default(PoolKey);
				poolKey.type = typeHint;
				poolKey.size = size;
				PoolKey key = poolKey;
				Queue<object> value;
				if (!_pooledMemory.TryGetValue(key, out value))
				{
					value = new Queue<object>();
					_pooledMemory[key] = value;
				}
				object value2 = ((EnablePooling && value.Count > MinPoolSize) ? value.Dequeue() : ((typeHint != eLeapAllocatorType.eLeapAllocatorType_Uint8 && typeHint == eLeapAllocatorType.eLeapAllocatorType_Float) ? ((object)new float[(size + 4 - 1) / 4]) : ((object)new byte[size])));
				GCHandle handle = GCHandle.Alloc(value2, GCHandleType.Pinned);
				IntPtr intPtr = handle.AddrOfPinnedObject();
				_activeMemory.Add(intPtr, new ActiveMemoryInfo
				{
					handle = handle,
					key = key
				});
				return intPtr;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return IntPtr.Zero;
		}

		[MonoPInvokeCallback(typeof(Deallocate))]
		public static void Unpin(IntPtr ptr, IntPtr state)
		{
			try
			{
				ActiveMemoryInfo activeMemoryInfo = _activeMemory[ptr];
				_pooledMemory[activeMemoryInfo.key].Enqueue(activeMemoryInfo.handle.Target);
				_activeMemory.Remove(ptr);
				activeMemoryInfo.handle.Free();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		public static object GetPinnedObject(IntPtr ptr)
		{
			try
			{
				return _activeMemory[ptr].handle.Target;
			}
			catch (Exception)
			{
			}
			return null;
		}
	}
}
