using System;
using System.Collections.Generic;
using UnityEngine;

internal static class OVRMessenger
{
	public class BroadcastException : Exception
	{
		public BroadcastException(string msg)
			: base(msg)
		{
		}
	}

	public class ListenerException : Exception
	{
		public ListenerException(string msg)
			: base(msg)
		{
		}
	}

	private static MessengerHelper messengerHelper = new GameObject("MessengerHelper").AddComponent<MessengerHelper>();

	public static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

	public static List<string> permanentMessages = new List<string>();

	public static void MarkAsPermanent(string eventType)
	{
		permanentMessages.Add(eventType);
	}

	public static void Cleanup()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Delegate> item in eventTable)
		{
			bool flag = false;
			foreach (string permanentMessage in permanentMessages)
			{
				if (item.Key == permanentMessage)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(item.Key);
			}
		}
		foreach (string item2 in list)
		{
			eventTable.Remove(item2);
		}
	}

	public static void PrintEventTable()
	{
		Debug.Log("\t\t\t=== MESSENGER PrintEventTable ===");
		foreach (KeyValuePair<string, Delegate> item in eventTable)
		{
			Debug.Log("\t\t\t" + item.Key + "\t\t" + item.Value);
		}
		Debug.Log("\n");
	}

	public static void OnListenerAdding(string eventType, Delegate listenerBeingAdded)
	{
		if (!eventTable.ContainsKey(eventType))
		{
			eventTable.Add(eventType, null);
		}
		Delegate @delegate = eventTable[eventType];
		if ((object)@delegate != null && @delegate.GetType() != listenerBeingAdded.GetType())
		{
			throw new ListenerException($"Attempting to add listener with inconsistent signature for event type {eventType}. Current listeners have type {@delegate.GetType().Name} and listener being added has type {listenerBeingAdded.GetType().Name}");
		}
	}

	public static void OnListenerRemoving(string eventType, Delegate listenerBeingRemoved)
	{
		if (eventTable.ContainsKey(eventType))
		{
			Delegate @delegate = eventTable[eventType];
			if ((object)@delegate == null)
			{
				throw new ListenerException($"Attempting to remove listener with for event type \"{eventType}\" but current listener is null.");
			}
			if (@delegate.GetType() != listenerBeingRemoved.GetType())
			{
				throw new ListenerException($"Attempting to remove listener with inconsistent signature for event type {eventType}. Current listeners have type {@delegate.GetType().Name} and listener being removed has type {listenerBeingRemoved.GetType().Name}");
			}
			return;
		}
		throw new ListenerException($"Attempting to remove listener for type \"{eventType}\" but Messenger doesn't know about this event type.");
	}

	public static void OnListenerRemoved(string eventType)
	{
		if ((object)eventTable[eventType] == null)
		{
			eventTable.Remove(eventType);
		}
	}

	public static void OnBroadcasting(string eventType)
	{
	}

	public static BroadcastException CreateBroadcastSignatureException(string eventType)
	{
		return new BroadcastException($"Broadcasting message \"{eventType}\" but listeners have a different signature than the broadcaster.");
	}

	public static void AddListener(string eventType, OVRCallback handler)
	{
		OnListenerAdding(eventType, handler);
		eventTable[eventType] = (OVRCallback)Delegate.Combine((OVRCallback)eventTable[eventType], handler);
	}

	public static void AddListener<T>(string eventType, OVRCallback<T> handler)
	{
		OnListenerAdding(eventType, handler);
		eventTable[eventType] = (OVRCallback<T>)Delegate.Combine((OVRCallback<T>)eventTable[eventType], handler);
	}

	public static void AddListener<T, U>(string eventType, OVRCallback<T, U> handler)
	{
		OnListenerAdding(eventType, handler);
		eventTable[eventType] = (OVRCallback<T, U>)Delegate.Combine((OVRCallback<T, U>)eventTable[eventType], handler);
	}

	public static void AddListener<T, U, V>(string eventType, OVRCallback<T, U, V> handler)
	{
		OnListenerAdding(eventType, handler);
		eventTable[eventType] = (OVRCallback<T, U, V>)Delegate.Combine((OVRCallback<T, U, V>)eventTable[eventType], handler);
	}

	public static void RemoveListener(string eventType, OVRCallback handler)
	{
		OnListenerRemoving(eventType, handler);
		eventTable[eventType] = (OVRCallback)Delegate.Remove((OVRCallback)eventTable[eventType], handler);
		OnListenerRemoved(eventType);
	}

	public static void RemoveListener<T>(string eventType, OVRCallback<T> handler)
	{
		OnListenerRemoving(eventType, handler);
		eventTable[eventType] = (OVRCallback<T>)Delegate.Remove((OVRCallback<T>)eventTable[eventType], handler);
		OnListenerRemoved(eventType);
	}

	public static void RemoveListener<T, U>(string eventType, OVRCallback<T, U> handler)
	{
		OnListenerRemoving(eventType, handler);
		eventTable[eventType] = (OVRCallback<T, U>)Delegate.Remove((OVRCallback<T, U>)eventTable[eventType], handler);
		OnListenerRemoved(eventType);
	}

	public static void RemoveListener<T, U, V>(string eventType, OVRCallback<T, U, V> handler)
	{
		OnListenerRemoving(eventType, handler);
		eventTable[eventType] = (OVRCallback<T, U, V>)Delegate.Remove((OVRCallback<T, U, V>)eventTable[eventType], handler);
		OnListenerRemoved(eventType);
	}

	public static void Broadcast(string eventType)
	{
		OnBroadcasting(eventType);
		Delegate value;
		if (eventTable.TryGetValue(eventType, out value))
		{
			OVRCallback oVRCallback = value as OVRCallback;
			if (oVRCallback == null)
			{
				throw CreateBroadcastSignatureException(eventType);
			}
			oVRCallback();
		}
	}

	public static void Broadcast<T>(string eventType, T arg1)
	{
		OnBroadcasting(eventType);
		Delegate value;
		if (eventTable.TryGetValue(eventType, out value))
		{
			OVRCallback<T> oVRCallback = value as OVRCallback<T>;
			if (oVRCallback == null)
			{
				throw CreateBroadcastSignatureException(eventType);
			}
			oVRCallback(arg1);
		}
	}

	public static void Broadcast<T, U>(string eventType, T arg1, U arg2)
	{
		OnBroadcasting(eventType);
		Delegate value;
		if (eventTable.TryGetValue(eventType, out value))
		{
			OVRCallback<T, U> oVRCallback = value as OVRCallback<T, U>;
			if (oVRCallback == null)
			{
				throw CreateBroadcastSignatureException(eventType);
			}
			oVRCallback(arg1, arg2);
		}
	}

	public static void Broadcast<T, U, V>(string eventType, T arg1, U arg2, V arg3)
	{
		OnBroadcasting(eventType);
		Delegate value;
		if (eventTable.TryGetValue(eventType, out value))
		{
			OVRCallback<T, U, V> oVRCallback = value as OVRCallback<T, U, V>;
			if (oVRCallback == null)
			{
				throw CreateBroadcastSignatureException(eventType);
			}
			oVRCallback(arg1, arg2, arg3);
		}
	}
}
