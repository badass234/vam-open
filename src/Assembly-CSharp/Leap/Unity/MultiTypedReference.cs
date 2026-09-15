using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity
{
	public abstract class MultiTypedReference<BaseType> where BaseType : class
	{
		public abstract BaseType Value { get; set; }

		public abstract void Clear();
	}
	public class MultiTypedReference<BaseType, A, B> : MultiTypedReference<BaseType> where BaseType : class where A : BaseType where B : BaseType
	{
		[SerializeField]
		protected int _index = -1;

		[SerializeField]
		private List<A> _a = new List<A>();

		[SerializeField]
		private List<B> _b = new List<B>();

		[NonSerialized]
		protected BaseType _cachedValue;

		public sealed override BaseType Value
		{
			get
			{
				if (_cachedValue != null)
				{
					return _cachedValue;
				}
				_cachedValue = internalGet();
				return _cachedValue;
			}
			set
			{
				Clear();
				internalSetAfterClear(value);
				_cachedValue = value;
			}
		}

		public override void Clear()
		{
			_cachedValue = (BaseType)null;
			if (_index == 0)
			{
				_a.Clear();
			}
			else if (_index == 1)
			{
				_b.Clear();
			}
			_index = -1;
		}

		protected virtual BaseType internalGet()
		{
			if (_index == -1)
			{
				return (BaseType)null;
			}
			if (_index == 0)
			{
				return _cachedValue = (BaseType)(object)_a[0];
			}
			if (_index == 1)
			{
				return _cachedValue = (BaseType)(object)_b[0];
			}
			throw new Exception("Invalid index " + _index);
		}

		protected virtual void internalSetAfterClear(BaseType obj)
		{
			if (obj == null)
			{
				_index = -1;
				return;
			}
			if (obj is A)
			{
				_a.Add((A)obj);
				_index = 0;
				return;
			}
			if (obj is B)
			{
				_b.Add((B)obj);
				_index = 1;
				return;
			}
			throw new ArgumentException("The type " + obj.GetType().Name + " is not supported by this reference.");
		}
	}
	public class MultiTypedReference<BaseType, A, B, C> : MultiTypedReference<BaseType, A, B> where BaseType : class where A : BaseType where B : BaseType where C : BaseType
	{
		[SerializeField]
		private List<C> _c = new List<C>();

		public override void Clear()
		{
			if (_index == 2)
			{
				_c.Clear();
			}
			base.Clear();
		}

		protected override BaseType internalGet()
		{
			if (_index == 2)
			{
				return (BaseType)(object)_c[0];
			}
			return base.internalGet();
		}

		protected override void internalSetAfterClear(BaseType obj)
		{
			if (obj is C)
			{
				_c.Add((C)obj);
				_index = 2;
			}
			else
			{
				base.internalSetAfterClear(obj);
			}
		}
	}
	public class MultiTypedReference<BaseType, A, B, C, D> : MultiTypedReference<BaseType, A, B, C> where BaseType : class where A : BaseType where B : BaseType where C : BaseType where D : BaseType
	{
		[SerializeField]
		private List<D> _d = new List<D>();

		public override void Clear()
		{
			if (_index == 3)
			{
				_d.Clear();
			}
			base.Clear();
		}

		protected override BaseType internalGet()
		{
			if (_index == 3)
			{
				return (BaseType)(object)_d[0];
			}
			return base.internalGet();
		}

		protected override void internalSetAfterClear(BaseType obj)
		{
			if (obj is D)
			{
				_d.Add((D)obj);
				_index = 3;
			}
			else
			{
				base.internalSetAfterClear(obj);
			}
		}
	}
}
