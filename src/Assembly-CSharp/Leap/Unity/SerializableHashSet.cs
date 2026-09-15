using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity
{
	public class SerializableHashSet<T> : SerializableHashSetBase, ICanReportDuplicateInformation, ISerializationCallbackReceiver, IEnumerable<T>, IEnumerable
	{
		[SerializeField]
		private List<T> _values = new List<T>();

		[NonSerialized]
		private HashSet<T> _set = new HashSet<T>();

		public int Count => _set.Count;

		public bool Add(T item)
		{
			return _set.Add(item);
		}

		public void Clear()
		{
			_set.Clear();
		}

		public bool Contains(T item)
		{
			return _set.Contains(item);
		}

		public bool Remove(T item)
		{
			return _set.Remove(item);
		}

		public static implicit operator HashSet<T>(SerializableHashSet<T> serializableHashSet)
		{
			return serializableHashSet._set;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _set.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _set.GetEnumerator();
		}

		public void ClearDuplicates()
		{
			HashSet<T> hashSet = new HashSet<T>();
			int count = _values.Count;
			while (count-- != 0)
			{
				T item = _values[count];
				if (hashSet.Contains(item))
				{
					_values.RemoveAt(count);
				}
				else
				{
					hashSet.Add(item);
				}
			}
		}

		public List<int> GetDuplicationInformation()
		{
			Dictionary<T, int> dictionary = new Dictionary<T, int>();
			foreach (T value in _values)
			{
				if (value != null)
				{
					if (dictionary.ContainsKey(value))
					{
						dictionary[value]++;
					}
					else
					{
						dictionary[value] = 1;
					}
				}
			}
			List<int> list = new List<int>();
			foreach (T value2 in _values)
			{
				if (value2 != null)
				{
					list.Add(dictionary[value2]);
				}
			}
			return list;
		}

		public void OnAfterDeserialize()
		{
			_set.Clear();
			if (_values != null)
			{
				foreach (T value in _values)
				{
					if (value != null)
					{
						_set.Add(value);
					}
				}
			}
			_values.Clear();
		}

		public void OnBeforeSerialize()
		{
			if (_values == null)
			{
				_values = new List<T>();
			}
			_values.Clear();
			_values.AddRange(this);
		}

		private bool isNull(object obj)
		{
			if (obj == null)
			{
				return true;
			}
			if (obj is UnityEngine.Object)
			{
				return obj as UnityEngine.Object == null;
			}
			return false;
		}
	}
}
