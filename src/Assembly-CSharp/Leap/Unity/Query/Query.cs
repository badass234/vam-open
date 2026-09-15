using System;
using System.Collections;
using System.Collections.Generic;

namespace Leap.Unity.Query
{
	public struct Query<T>
	{
		public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
		{
			private T[] _array;

			private int _count;

			private int _nextIndex;

			object IEnumerator.Current
			{
				get
				{
					if (_nextIndex == 0)
					{
						throw new InvalidOperationException();
					}
					return Current;
				}
			}

			public T Current { get; private set; }

			public Enumerator(T[] array, int count)
			{
				_array = array;
				_count = count;
				_nextIndex = 0;
				Current = default(T);
			}

			public bool MoveNext()
			{
				if (_nextIndex >= _count)
				{
					return false;
				}
				Current = _array[_nextIndex++];
				return true;
			}

			public void Dispose()
			{
				ArrayPool<T>.Recycle(_array);
			}

			public void Reset()
			{
				throw new InvalidOperationException();
			}
		}

		public struct QuerySlice : IDisposable
		{
			public readonly T[] BackingArray;

			public readonly int Count;

			public T this[int index] => BackingArray[index];

			public QuerySlice(T[] array, int count)
			{
				BackingArray = array;
				Count = count;
			}

			public void Dispose()
			{
				ArrayPool<T>.Recycle(BackingArray);
			}
		}

		private struct Validator
		{
			private class Id
			{
				public int value;
			}

			private static int _nextId = 1;

			private Id _idRef;

			private int _idValue;

			public void Validate()
			{
				if (_idValue == 0)
				{
					throw new InvalidOperationException("This Query is not valid, you cannot construct a Query using the default constructor.");
				}
				if (_idRef == null || _idRef.value != _idValue)
				{
					throw new InvalidOperationException("This Query has already been disposed.  A Query can only be used once before it is automatically disposed.");
				}
			}

			public static Validator Spawn()
			{
				Id id = Pool<Id>.Spawn();
				id.value = _nextId++;
				Validator result = default(Validator);
				result._idRef = id;
				result._idValue = id.value;
				return result;
			}

			public static void Invalidate(Validator validator)
			{
				validator._idRef.value = -1;
				Pool<Id>.Recycle(validator._idRef);
			}
		}

		private T[] _array;

		private int _count;

		private Validator _validator;

		public Query(T[] array, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (count < 0)
			{
				throw new ArgumentException("Count must be non-negative, but was " + count);
			}
			if (count > array.Length)
			{
				throw new ArgumentException("Count was " + count + " but the provided array only had a length of " + array.Length);
			}
			_array = array;
			_count = count;
			_validator = Validator.Spawn();
		}

		public Query(ICollection<T> collection)
		{
			_array = ArrayPool<T>.Spawn(collection.Count);
			_count = collection.Count;
			collection.CopyTo(_array, 0);
			_validator = Validator.Spawn();
		}

		public Query(Query<T> other)
		{
			other._validator.Validate();
			_array = ArrayPool<T>.Spawn(other._count);
			_count = other._count;
			Array.Copy(other._array, _array, _count);
			_validator = Validator.Spawn();
		}

		public Query<K> OfType<K>() where K : T
		{
			_validator.Validate();
			K[] array = ArrayPool<K>.Spawn(_count);
			int count = 0;
			for (int i = 0; i < _count; i++)
			{
				if (_array[i] is K)
				{
					array[count++] = (K)(object)_array[i];
				}
			}
			Dispose();
			return new Query<K>(array, count);
		}

		public Query<K> Cast<K>() where K : class
		{
			return this.Select((T item) => item as K);
		}

		public void Dispose()
		{
			_validator.Validate();
			ArrayPool<T>.Recycle(_array);
			Validator.Invalidate(_validator);
			_array = null;
			_count = 0;
		}

		public void Deconstruct(out T[] array, out int count)
		{
			_validator.Validate();
			array = _array;
			count = _count;
			Validator.Invalidate(_validator);
			_array = null;
			_count = 0;
		}

		public QuerySlice Deconstruct()
		{
			T[] array;
			int count;
			Deconstruct(out array, out count);
			return new QuerySlice(array, count);
		}

		public Enumerator GetEnumerator()
		{
			_validator.Validate();
			T[] array;
			int count;
			Deconstruct(out array, out count);
			return new Enumerator(array, count);
		}
	}
}
