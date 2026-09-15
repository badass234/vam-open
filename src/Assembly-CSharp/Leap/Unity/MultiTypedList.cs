using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity
{
	public abstract class MultiTypedList
	{
		[Serializable]
		public struct Key
		{
			public int id;

			public int index;
		}
	}
	public abstract class MultiTypedList<BaseType> : MultiTypedList, IList<BaseType>, ICollection<BaseType>, IEnumerable<BaseType>, IEnumerable
	{
		public struct Enumerator : IEnumerator<BaseType>, IEnumerator, IDisposable
		{
			private MultiTypedList<BaseType> _list;

			private int _index;

			private BaseType _current;

			object IEnumerator.Current
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public BaseType Current => _current;

			public Enumerator(MultiTypedList<BaseType> list)
			{
				_list = list;
				_index = 0;
				_current = default(BaseType);
			}

			public void Dispose()
			{
				_list = null;
				_current = default(BaseType);
			}

			public bool MoveNext()
			{
				if (_index >= _list.Count)
				{
					return false;
				}
				_current = _list[_index++];
				return true;
			}

			public void Reset()
			{
				_index = 0;
				_current = default(BaseType);
			}
		}

		public abstract int Count { get; }

		public bool IsReadOnly => false;

		public abstract BaseType this[int index] { get; set; }

		public abstract void Add(BaseType obj);

		public abstract void Clear();

		public bool Contains(BaseType item)
		{
			for (int i = 0; i < Count; i++)
			{
				if (this[i].Equals(item))
				{
					return true;
				}
			}
			return false;
		}

		public void CopyTo(BaseType[] array, int arrayIndex)
		{
			for (int i = 0; i < Count; i++)
			{
				array[i + arrayIndex] = this[i];
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public int IndexOf(BaseType item)
		{
			for (int i = 0; i < Count; i++)
			{
				if (this[i].Equals(item))
				{
					return i;
				}
			}
			return -1;
		}

		public abstract void Insert(int index, BaseType item);

		public bool Remove(BaseType item)
		{
			int num = IndexOf(item);
			if (num >= 0)
			{
				RemoveAt(num);
				return true;
			}
			return false;
		}

		public abstract void RemoveAt(int index);

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<BaseType> IEnumerable<BaseType>.GetEnumerator()
		{
			return new Enumerator(this);
		}
	}
	[Serializable]
	public class MultiTypedList<BaseType, A, B> : MultiTypedList<BaseType> where A : BaseType where B : BaseType
	{
		[SerializeField]
		private List<Key> _table = new List<Key>();

		[SerializeField]
		private List<A> _a = new List<A>();

		[SerializeField]
		private List<B> _b = new List<B>();

		public override int Count => _table.Count;

		public override BaseType this[int index]
		{
			get
			{
				Key key = _table[index];
				return (BaseType)getList(key.id)[key.index];
			}
			set
			{
				Key key = _table[index];
				getList(key.id).RemoveAt(key.index);
				Key value2 = addInternal(value);
				_table[index] = value2;
			}
		}

		public override void Add(BaseType obj)
		{
			_table.Add(addInternal(obj));
		}

		public override void Clear()
		{
			_table.Clear();
			_a.Clear();
			_b.Clear();
		}

		public override void Insert(int index, BaseType obj)
		{
			_table.Insert(index, addInternal(obj));
		}

		public override void RemoveAt(int index)
		{
			Key key = _table[index];
			_table.RemoveAt(index);
			getList(key.id).RemoveAt(key.index);
			for (int i = 0; i < _table.Count; i++)
			{
				Key value = _table[i];
				if (value.id == key.id && value.index > key.index)
				{
					value.index--;
					_table[i] = value;
				}
			}
		}

		protected Key addHelper(IList list, BaseType instance, int id)
		{
			Key key = default(Key);
			key.id = id;
			key.index = list.Count;
			Key result = key;
			list.Add(instance);
			return result;
		}

		protected virtual Key addInternal(BaseType obj)
		{
			if (obj is A)
			{
				return addHelper(_a, obj, 0);
			}
			if (obj is B)
			{
				return addHelper(_b, obj, 1);
			}
			throw new ArgumentException("This multi typed list does not support type " + obj.GetType().Name);
		}

		protected virtual IList getList(int id)
		{
			switch (id)
			{
			case 0:
				return _a;
			case 1:
				return _b;
			default:
				throw new Exception("This multi typed list does not have a list with id " + id);
			}
		}
	}
	public class MultiTypedList<BaseType, A, B, C> : MultiTypedList<BaseType, A, B> where A : BaseType where B : BaseType where C : BaseType
	{
		[SerializeField]
		private List<C> _c = new List<C>();

		protected override Key addInternal(BaseType obj)
		{
			return (!(obj is C)) ? base.addInternal(obj) : addHelper(_c, obj, 2);
		}

		protected override IList getList(int id)
		{
			return (id != 2) ? base.getList(id) : _c;
		}

		public override void Clear()
		{
			base.Clear();
			_c.Clear();
		}
	}
	public class MultiTypedList<BaseType, A, B, C, D> : MultiTypedList<BaseType, A, B, C> where A : BaseType where B : BaseType where C : BaseType where D : BaseType
	{
		[SerializeField]
		private List<D> _d = new List<D>();

		protected override Key addInternal(BaseType obj)
		{
			return (!(obj is D)) ? base.addInternal(obj) : addHelper(_d, obj, 3);
		}

		protected override IList getList(int id)
		{
			return (id != 3) ? base.getList(id) : _d;
		}

		public override void Clear()
		{
			base.Clear();
			_d.Clear();
		}
	}
	public class MultiTypedList<BaseType, A, B, C, D, E> : MultiTypedList<BaseType, A, B, C, D> where A : BaseType where B : BaseType where C : BaseType where D : BaseType where E : BaseType
	{
		[SerializeField]
		private List<E> _e = new List<E>();

		protected override Key addInternal(BaseType obj)
		{
			return (!(obj is E)) ? base.addInternal(obj) : addHelper(_e, obj, 4);
		}

		protected override IList getList(int id)
		{
			return (id != 4) ? base.getList(id) : _e;
		}

		public override void Clear()
		{
			base.Clear();
			_e.Clear();
		}
	}
	public class MultiTypedList<BaseType, A, B, C, D, E, F> : MultiTypedList<BaseType, A, B, C, D, E> where A : BaseType where B : BaseType where C : BaseType where D : BaseType where E : BaseType where F : BaseType
	{
		[SerializeField]
		private List<F> _f = new List<F>();

		protected override Key addInternal(BaseType obj)
		{
			return (!(obj is F)) ? base.addInternal(obj) : addHelper(_f, obj, 5);
		}

		protected override IList getList(int id)
		{
			return (id != 5) ? base.getList(id) : _f;
		}

		public override void Clear()
		{
			base.Clear();
			_f.Clear();
		}
	}
	public class MultiTypedList<BaseType, A, B, C, D, E, F, G> : MultiTypedList<BaseType, A, B, C, D, E, F> where A : BaseType where B : BaseType where C : BaseType where D : BaseType where E : BaseType where F : BaseType where G : BaseType
	{
		[SerializeField]
		private List<G> _g = new List<G>();

		protected override Key addInternal(BaseType obj)
		{
			return (!(obj is G)) ? base.addInternal(obj) : addHelper(_g, obj, 6);
		}

		protected override IList getList(int id)
		{
			return (id != 6) ? base.getList(id) : _g;
		}

		public override void Clear()
		{
			base.Clear();
			_g.Clear();
		}
	}
	public class MultiTypedList<BaseType, A, B, C, D, E, F, G, H> : MultiTypedList<BaseType, A, B, C, D, E, F, G> where A : BaseType where B : BaseType where C : BaseType where D : BaseType where E : BaseType where F : BaseType where G : BaseType where H : BaseType
	{
		[SerializeField]
		private List<H> _h = new List<H>();

		protected override Key addInternal(BaseType obj)
		{
			return (!(obj is H)) ? base.addInternal(obj) : addHelper(_h, obj, 7);
		}

		protected override IList getList(int id)
		{
			return (id != 7) ? base.getList(id) : _h;
		}

		public override void Clear()
		{
			base.Clear();
			_h.Clear();
		}
	}
}
