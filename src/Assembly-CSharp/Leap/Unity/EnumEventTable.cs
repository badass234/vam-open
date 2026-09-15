using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Leap.Unity
{
	[Serializable]
	public class EnumEventTable : ISerializationCallbackReceiver
	{
		[Serializable]
		private class Entry
		{
			[SerializeField]
			public int enumValue;

			[SerializeField]
			public UnityEvent callback;
		}

		[SerializeField]
		private List<Entry> _entries = new List<Entry>();

		private Dictionary<int, UnityEvent> _entryMap = new Dictionary<int, UnityEvent>();

		public bool HasUnityEvent(int enumValue)
		{
			UnityEvent value;
			if (_entryMap.TryGetValue(enumValue, out value))
			{
				return value.GetPersistentEventCount() > 0;
			}
			return false;
		}

		public void Invoke(int enumValue)
		{
			UnityEvent value;
			if (_entryMap.TryGetValue(enumValue, out value))
			{
				value.Invoke();
			}
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			if (_entryMap == null)
			{
				_entryMap = new Dictionary<int, UnityEvent>();
			}
			else
			{
				_entryMap.Clear();
			}
			foreach (Entry entry in _entries)
			{
				_entryMap[entry.enumValue] = entry.callback;
			}
		}
	}
}
