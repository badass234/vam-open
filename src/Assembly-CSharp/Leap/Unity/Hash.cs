using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity
{
	[Serializable]
	public struct Hash : IEnumerable, IEquatable<Hash>
	{
		private int _hash;

		private static List<Behaviour> _behaviourCache = new List<Behaviour>();

		public Hash(int hash)
		{
			_hash = hash;
		}

		public void Add<T>(T t)
		{
			int num = t?.GetHashCode() ?? 647155961;
			_hash ^= num + 1043823033 + (_hash << 6) + (_hash >> 2);
		}

		public void AddRange<T>(List<T> sequence)
		{
			for (int i = 0; i < sequence.Count; i++)
			{
				Add(sequence[i]);
			}
		}

		public static Hash GetHierarchyHash(Transform root)
		{
			Hash dataHash = GetDataHash(root);
			int childCount = root.childCount;
			for (int i = 0; i < childCount; i++)
			{
				dataHash.Add(GetHierarchyHash(root.GetChild(i)));
			}
			root.GetComponents(_behaviourCache);
			for (int j = 0; j < _behaviourCache.Count; j++)
			{
				Behaviour behaviour = _behaviourCache[j];
				if (behaviour != null)
				{
					dataHash.Add(behaviour.enabled);
				}
			}
			return dataHash;
		}

		public static Hash GetDataHash(Transform transform)
		{
			Hash hash = default(Hash);
			hash.Add(transform);
			hash.Add(transform.gameObject.activeSelf);
			hash.Add(transform.localPosition);
			hash.Add(transform.localRotation);
			hash.Add(transform.localScale);
			Hash result = hash;
			if (transform is RectTransform)
			{
				RectTransform rectTransform = transform as RectTransform;
				result.Add(rectTransform.rect);
			}
			return result;
		}

		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public override int GetHashCode()
		{
			return _hash;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Hash))
			{
				return false;
			}
			return ((Hash)obj)._hash == _hash;
		}

		public bool Equals(Hash other)
		{
			return _hash == other._hash;
		}

		public static implicit operator Hash(int hash)
		{
			return new Hash(hash);
		}

		public static implicit operator int(Hash hash)
		{
			return hash._hash;
		}
	}
}
