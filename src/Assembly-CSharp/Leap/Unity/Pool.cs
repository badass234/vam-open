using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity
{
	public static class Pool<T> where T : new()
	{
		[ThreadStatic]
		private static Stack<T> _pool = new Stack<T>();

		public static T Spawn()
		{
			if (_pool == null)
			{
				_pool = new Stack<T>();
			}
			T val = ((_pool.Count <= 0) ? new T() : _pool.Pop());
			if (val is IPoolable)
			{
				(val as IPoolable).OnSpawn();
			}
			return val;
		}

		public static void Recycle(T t)
		{
			if (t == null)
			{
				Debug.LogError("Cannot recycle a null object.");
				return;
			}
			if (t is IPoolable)
			{
				(t as IPoolable).OnRecycle();
			}
			_pool.Push(t);
		}
	}
}
