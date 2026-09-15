using System;

namespace Leap.Unity
{
	public struct Either<A, B> : IEquatable<Either<A, B>>, IComparable, IComparable<Either<A, B>>
	{
		public readonly bool isA;

		private readonly A _a;

		private readonly B _b;

		public bool isB => !isA;

		public Maybe<A> a
		{
			get
			{
				if (isA)
				{
					return Maybe<A>.Some(_a);
				}
				return Maybe<A>.None;
			}
		}

		public Maybe<B> b
		{
			get
			{
				if (isA)
				{
					return Maybe<B>.None;
				}
				return Maybe<B>.Some(_b);
			}
		}

		public Either(A a)
		{
			if (a == null)
			{
				throw new ArgumentNullException("Cannot initialize an Either with a null value.");
			}
			isA = true;
			_a = a;
			_b = default(B);
		}

		public Either(B b)
		{
			if (b == null)
			{
				throw new ArgumentNullException("Cannot initialize an Either with a null value.");
			}
			isA = false;
			_b = b;
			_a = default(A);
		}

		public void Match(Action<A> ifA, Action<B> ifB)
		{
			if (isA)
			{
				ifA?.Invoke(_a);
			}
			else
			{
				ifB?.Invoke(_b);
			}
		}

		public bool TryGetA(out A a)
		{
			a = _a;
			return isA;
		}

		public bool TryGetB(out B b)
		{
			b = _b;
			return !isA;
		}

		public override int GetHashCode()
		{
			if (isA)
			{
				return _a.GetHashCode();
			}
			return _b.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is Either<A, B>)
			{
				return Equals((Either<A, B>)obj);
			}
			return false;
		}

		public bool Equals(Either<A, B> other)
		{
			if (isA != other.isA)
			{
				return false;
			}
			if (isA)
			{
				return _a.Equals(other._a);
			}
			return _b.Equals(other._b);
		}

		public int CompareTo(object obj)
		{
			if (!(obj is Either<A, B>))
			{
				throw new ArgumentException();
			}
			return CompareTo((Either<A, B>)obj);
		}

		public int CompareTo(Either<A, B> other)
		{
			if (isA != other.isA)
			{
				return (!isA) ? 1 : (-1);
			}
			if (isA)
			{
				return (_a as IComparable<A>)?.CompareTo(other._a) ?? (_a as IComparable)?.CompareTo(other._b) ?? 0;
			}
			return (_b as IComparable<B>)?.CompareTo(other._b) ?? (_b as IComparable)?.CompareTo(other._b) ?? 0;
		}

		public static bool operator ==(Either<A, B> either0, Either<A, B> either1)
		{
			return either0.Equals(either1);
		}

		public static bool operator !=(Either<A, B> either0, Either<A, B> either1)
		{
			return !either0.Equals(either1);
		}

		public static bool operator >(Either<A, B> either0, Either<A, B> either1)
		{
			return either0.CompareTo(either1) > 0;
		}

		public static bool operator >=(Either<A, B> either0, Either<A, B> either1)
		{
			return either0.CompareTo(either1) >= 0;
		}

		public static bool operator <(Either<A, B> either0, Either<A, B> either1)
		{
			return either0.CompareTo(either1) < 0;
		}

		public static bool operator <=(Either<A, B> either0, Either<A, B> either1)
		{
			return either0.CompareTo(either1) <= 0;
		}

		public static implicit operator Either<A, B>(A a)
		{
			return new Either<A, B>(a);
		}

		public static implicit operator Either<A, B>(B b)
		{
			return new Either<A, B>(b);
		}
	}
}
