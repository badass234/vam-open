using System;
using System.Collections.Generic;

namespace IKVM.Reflection.Reader
{
	internal sealed class UnboundGenericMethodParameter : TypeParameterType
	{
		private sealed class DummyModule : NonPEModule
		{
			public override int MDStreamVersion
			{
				get
				{
					throw new InvalidOperationException();
				}
			}

			public override Assembly Assembly
			{
				get
				{
					throw new InvalidOperationException();
				}
			}

			public override string FullyQualifiedName
			{
				get
				{
					throw new InvalidOperationException();
				}
			}

			public override string Name
			{
				get
				{
					throw new InvalidOperationException();
				}
			}

			public override Guid ModuleVersionId
			{
				get
				{
					throw new InvalidOperationException();
				}
			}

			public override string ScopeName
			{
				get
				{
					throw new InvalidOperationException();
				}
			}

			internal DummyModule()
				: base(new Universe())
			{
			}

			protected override Exception NotSupportedException()
			{
				return new InvalidOperationException();
			}

			protected override Exception ArgumentOutOfRangeException()
			{
				return new InvalidOperationException();
			}

			public override bool Equals(object obj)
			{
				throw new InvalidOperationException();
			}

			public override int GetHashCode()
			{
				throw new InvalidOperationException();
			}

			public override string ToString()
			{
				throw new InvalidOperationException();
			}

			internal override Type FindType(TypeName typeName)
			{
				throw new InvalidOperationException();
			}

			internal override Type FindTypeIgnoreCase(TypeName lowerCaseName)
			{
				throw new InvalidOperationException();
			}

			internal override void GetTypesImpl(List<Type> list)
			{
				throw new InvalidOperationException();
			}
		}

		private static readonly DummyModule module = new DummyModule();

		private readonly int position;

		public override string Namespace
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public override string Name
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public override int MetadataToken
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public override Module Module => module;

		public override int GenericParameterPosition => position;

		public override Type DeclaringType => null;

		public override MethodBase DeclaringMethod
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public override GenericParameterAttributes GenericParameterAttributes
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		internal override bool IsBaked
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		internal static Type Make(int position)
		{
			return module.universe.CanonicalizeType(new UnboundGenericMethodParameter(position));
		}

		private UnboundGenericMethodParameter(int position)
			: base(30)
		{
			this.position = position;
		}

		public override bool Equals(object obj)
		{
			UnboundGenericMethodParameter unboundGenericMethodParameter = obj as UnboundGenericMethodParameter;
			if (unboundGenericMethodParameter != null)
			{
				return unboundGenericMethodParameter.position == position;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return position;
		}

		public override Type[] GetGenericParameterConstraints()
		{
			throw new InvalidOperationException();
		}

		public override CustomModifiers[] __GetGenericParameterConstraintCustomModifiers()
		{
			throw new InvalidOperationException();
		}

		internal override Type BindTypeParameters(IGenericBinder binder)
		{
			return binder.BindMethodParameter(this);
		}
	}
}
