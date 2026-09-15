using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Leap.Unity
{
	public class SerializableDictionary<TKey, TValue> : SerializableDictionaryBase, IEnumerable<KeyValuePair<TKey, TValue>>, ICanReportDuplicateInformation, ISerializationCallbackReceiver, ISerializableDictionary, IEnumerable
	{
		[SerializeField]
		private List<TKey> _keys = new List<TKey>();

		[SerializeField]
		private List<TValue> _values = new List<TValue>();

		[NonSerialized]
		private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

		public TValue this[TKey key]
		{
			get
			{
				return _dictionary[key];
			}
			set
			{
				_dictionary[key] = value;
			}
		}

		public Dictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;

		public Dictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;

		public int Count => _dictionary.Count;

		public void Add(TKey key, TValue value)
		{
			_dictionary.Add(key, value);
		}

		public void Clear()
		{
			_dictionary.Clear();
		}

		public bool ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}

		public bool ContainsValue(TValue value)
		{
			return _dictionary.ContainsValue(value);
		}

		public bool Remove(TKey key)
		{
			return _dictionary.Remove(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		public static implicit operator Dictionary<TKey, TValue>(SerializableDictionary<TKey, TValue> serializableDictionary)
		{
			return serializableDictionary._dictionary;
		}

		public virtual float KeyDisplayRatio()
		{
			return 0.5f;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<TKey> list = _dictionary.Keys.ToList();
			List<TValue> list2 = _dictionary.Values.ToList();
			stringBuilder.Append("[");
			for (int i = 0; i < list.Count; i++)
			{
				stringBuilder.Append("{");
				stringBuilder.Append(list[i].ToString());
				stringBuilder.Append(" : ");
				stringBuilder.Append(list2[i].ToString());
				stringBuilder.Append("}, \n");
			}
			stringBuilder.Remove(stringBuilder.Length - 3, 3);
			stringBuilder.Append("]");
			return stringBuilder.ToString();
		}

		public void OnAfterDeserialize()
		{
			_dictionary.Clear();
			if (_keys != null && _values != null)
			{
				int num = Mathf.Min(_keys.Count, _values.Count);
				for (int i = 0; i < num; i++)
				{
					TKey val = _keys[i];
					TValue value = _values[i];
					if (val != null)
					{
						_dictionary[val] = value;
					}
				}
			}
			_keys.Clear();
			_values.Clear();
		}

		public void OnBeforeSerialize()
		{
			if (_keys == null)
			{
				_keys = new List<TKey>();
			}
			if (_values == null)
			{
				_values = new List<TValue>();
			}
			Dictionary<TKey, TValue>.Enumerator enumerator = _dictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, TValue> current = enumerator.Current;
				_keys.Add(current.Key);
				_values.Add(current.Value);
			}
		}
	}
}
