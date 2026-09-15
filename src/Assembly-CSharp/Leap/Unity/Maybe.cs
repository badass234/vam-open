using System;
using System.Runtime.InteropServices;
using Leap.Unity.Query;

namespace Leap.Unity
{
	public static class Maybe
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct NoneType
		{
		}

		public static readonly NoneType None = default(NoneType);

		public static Maybe<T> Some<T>(T value)
		{
			return new Maybe<T>(value);
		}

		public static void MatchAll<A, B>(Maybe<A> maybeA, Maybe<B> maybeB, Action<A, B> action)
		{
			maybeA.Match(delegate(A a)
			{
				maybeB.Match(delegate(B b)
				{
					action(a, b);
				});
			});
		}

		public static void MatchAll<A, B, C>(Maybe<A> maybeA, Maybe<B> maybeB, Maybe<C> maybeC, Action<A, B, C> action)
		{
			maybeA.Match(delegate(A a)
			{
				maybeB.Match(delegate(B b)
				{
					maybeC.Match(delegate(C c)
					{
						action(a, b, c);
					});
				});
			});
		}

		public static void MatchAll<A, B, C, D>(Maybe<A> maybeA, Maybe<B> maybeB, Maybe<C> maybeC, Maybe<D> maybeD, Action<A, B, C, D> action)
		{
			maybeA.Match(delegate(A a)
			{
				maybeB.Match(delegate(B b)
				{
					maybeC.Match(delegate(C c)
					{
						maybeD.Match(delegate(D d)
						{
							action(a, b, c, d);
						});
					});
				});
			});
		}
	}
	public struct Maybe<T> : IEquatable<Maybe<T>>, IComparable, IComparable<Maybe<T>>
	{
		public static readonly Maybe<T> None = default(Maybe<T>);

		public readonly bool hasValue;

		private readonly T _t;

		public T valueOrDefault
		{
			get
			{
				T t;
				if (TryGetValue(out t))
				{
					return t;
				}
				return default(T);
			}
		}

		public Maybe(T t)
		{
			if (Type<T>.isValueType)
			{
				hasValue = true;
			}
			else
			{
				hasValue = t != null;
			}
			_t = t;
		}

		public static Maybe<T> Some(T t)
		{
			if (!Type<T>.isValueType && t == null)
			{
				throw new ArgumentNullException("Cannot use Some with a null argument.");
			}
			return new Maybe<T>(t);
		}

		public bool TryGetValue(out T t)
		{
			t = _t;
			return hasValue;
		}

		public void Match(Action<T> ifValue)
		{
			if (hasValue)
			{
				ifValue(_t);
			}
		}

		public void Match(Action<T> ifValue, Action ifNot)
		{
			if (hasValue)
			{
				ifValue?.Invoke(_t);
			}
			else
			{
				ifNot();
			}
		}

		public K Match<K>(Func<T, K> ifValue, Func<K> ifNot)
		{
			if (hasValue)
			{
				if (ifValue != null)
				{
					return ifValue(_t);
				}
				return default(K);
			}
			return ifNot();
		}

		public T ValueOr(T customDefault)
		{
			if (hasValue)
			{
				return _t;
			}
			return customDefault;
		}

		public Maybe<T> ValueOr(Maybe<T> maybeCustomDefault)
		{
			if (hasValue)
			{
				return this;
			}
			return maybeCustomDefault;
		}

		public Query<T> Query()
		{
			if (hasValue)
			{
				return Values.Single(_t);
			}
			return Values.Empty<T>();
		}

		public override int GetHashCode()
		{
			return hasValue ? _t.GetHashCode() : 0;
		}

		public override bool Equals(object obj)
		{
			if (obj is Maybe<T>)
			{
				return Equals((Maybe<T>)obj);
			}
			return false;
		}

		public bool Equals(Maybe<T> other)
		{
			if (hasValue != other.hasValue)
			{
				return false;
			}
			if (hasValue)
			{
				return _t.Equals(other._t);
			}
			return true;
		}

		public int CompareTo(object obj)
		{
			if (!(obj is Maybe<T>))
			{
				throw new ArgumentException();
			}
			return CompareTo((Maybe<T>)obj);
		}

		public int CompareTo(Maybe<T> other)
		{
			if (hasValue != other.hasValue)
			{
				return hasValue ? 1 : (-1);
			}
			if (hasValue)
			{
				return (_t as IComparable<T>)?.CompareTo(other._t) ?? (_t as IComparable)?.CompareTo(other._t) ?? 0;
			}
			return 0;
		}

		public static bool operator ==(Maybe<T> maybe0, Maybe<T> maybe1)
		{
			return maybe0.Equals(maybe1);
		}

		public static bool operator !=(Maybe<T> maybe0, Maybe<T> maybe1)
		{
			return !maybe0.Equals(maybe1);
		}

		public static bool operator >(Maybe<T> maybe0, Maybe<T> maybe1)
		{
			return maybe0.CompareTo(maybe1) > 0;
		}

		public static bool operator >=(Maybe<T> maybe0, Maybe<T> maybe1)
		{
			return maybe0.CompareTo(maybe1) >= 0;
		}

		public static bool operator <(Maybe<T> maybe0, Maybe<T> maybe1)
		{
			return maybe0.CompareTo(maybe1) < 0;
		}

		public static bool operator <=(Maybe<T> maybe0, Maybe<T> maybe1)
		{
			return maybe0.CompareTo(maybe1) <= 0;
		}

		public static implicit operator Maybe<T>(T t)
		{
			return new Maybe<T>(t);
		}

		public static implicit operator Maybe<T>(Maybe.NoneType none)
		{
			return None;
		}
	}
}
