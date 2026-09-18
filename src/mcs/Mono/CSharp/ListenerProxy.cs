using System.Collections.Generic;

namespace Mono.CSharp
{
	internal static class ListenerProxy
	{
		private static readonly Dictionary<int, ValueModificationHandler> listeners = new Dictionary<int, ValueModificationHandler>();

		private static int counter;

		public static int Register(ValueModificationHandler listener)
		{
			lock (listeners)
			{
				int num = counter++;
				listeners.Add(num, listener);
				return num;
			}
		}

		public static void Unregister(int listenerId)
		{
			lock (listeners)
			{
				listeners.Remove(listenerId);
			}
		}

		public static void ValueChanged(object value, int row, int col, string name, int listenerId)
		{
			ValueModificationHandler value2;
			lock (listeners)
			{
				if (!listeners.TryGetValue(listenerId, out value2))
				{
					return;
				}
			}
			value2(name, row, col, value);
		}
	}
}
