using System;
using System.Threading;

namespace Leap
{
	public static class CSharpExtensions
	{
		public static bool NearlyEquals(this float a, float b, float epsilon = 1.1920929E-07f)
		{
			float num = Math.Abs(a);
			float num2 = Math.Abs(b);
			float num3 = Math.Abs(a - b);
			if (a == b)
			{
				return true;
			}
			if (a == 0f || b == 0f || num3 < float.MinValue)
			{
				return num3 < epsilon * float.MinValue;
			}
			return num3 / (num + num2) < epsilon;
		}

		public static bool HasMethod(this object objectToCheck, string methodName)
		{
			Type type = objectToCheck.GetType();
			return type.GetMethod(methodName) != null;
		}

		public static int indexOf(this Enum enumItem)
		{
			return Array.IndexOf(Enum.GetValues(enumItem.GetType()), enumItem);
		}

		public static T itemFor<T>(this int ordinal)
		{
			T[] array = (T[])Enum.GetValues(typeof(T));
			return array[ordinal];
		}

		public static void Dispatch<T>(this EventHandler<T> handler, object sender, T eventArgs) where T : EventArgs
		{
			handler?.Invoke(sender, eventArgs);
		}

		public static void DispatchOnContext<T>(this EventHandler<T> handler, object sender, SynchronizationContext context, T eventArgs) where T : EventArgs
		{
			if (handler == null)
			{
				return;
			}
			if (context != null)
			{
				SendOrPostCallback d = delegate(object spc_args)
				{
					handler(sender, spc_args as T);
				};
				context.Post(d, eventArgs);
			}
			else
			{
				handler(sender, eventArgs);
			}
		}
	}
}
