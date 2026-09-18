using System;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	internal abstract class NonPEModule : Module
	{
		protected NonPEModule(Universe universe)
			: base(universe)
		{
		}

		protected virtual Exception InvalidOperationException()
		{
			return new InvalidOperationException();
		}

		protected virtual Exception NotSupportedException()
		{
			return new NotSupportedException();
		}

		protected virtual Exception ArgumentOutOfRangeException()
		{
			return new ArgumentOutOfRangeException();
		}

		internal sealed override Type GetModuleType()
		{
			throw InvalidOperationException();
		}

		internal sealed override ByteReader GetBlob(int blobIndex)
		{
			throw InvalidOperationException();
		}

		public sealed override AssemblyName[] __GetReferencedAssemblies()
		{
			throw NotSupportedException();
		}

		public sealed override string[] __GetReferencedModules()
		{
			throw NotSupportedException();
		}

		public override Type[] __GetReferencedTypes()
		{
			throw NotSupportedException();
		}

		public override Type[] __GetExportedTypes()
		{
			throw NotSupportedException();
		}

		protected sealed override long GetImageBaseImpl()
		{
			throw NotSupportedException();
		}

		protected sealed override long GetStackReserveImpl()
		{
			throw NotSupportedException();
		}

		protected sealed override int GetFileAlignmentImpl()
		{
			throw NotSupportedException();
		}

		protected override DllCharacteristics GetDllCharacteristicsImpl()
		{
			throw NotSupportedException();
		}

		internal sealed override Type ResolveType(int metadataToken, IGenericContext context)
		{
			throw ArgumentOutOfRangeException();
		}

		public sealed override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw ArgumentOutOfRangeException();
		}

		public sealed override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw ArgumentOutOfRangeException();
		}

		public sealed override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw ArgumentOutOfRangeException();
		}

		public sealed override string ResolveString(int metadataToken)
		{
			throw ArgumentOutOfRangeException();
		}

		public sealed override Type[] __ResolveOptionalParameterTypes(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments, out CustomModifiers[] customModifiers)
		{
			throw ArgumentOutOfRangeException();
		}
	}
}
