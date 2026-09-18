using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class PredefinedAttribute : PredefinedType
	{
		protected MethodSpec ctor;

		public MethodSpec Constructor => ctor;

		public PredefinedAttribute(ModuleContainer module, string ns, string name)
			: base(module, MemberKind.Class, ns, name)
		{
		}

		public static bool operator ==(TypeSpec type, PredefinedAttribute pa)
		{
			if (type == pa.type)
			{
				return pa.type != null;
			}
			return false;
		}

		public static bool operator !=(TypeSpec type, PredefinedAttribute pa)
		{
			return type != pa.type;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			throw new NotSupportedException();
		}

		public void EmitAttribute(ConstructorBuilder builder)
		{
			if (ResolveBuilder())
			{
				builder.SetCustomAttribute(GetCtorMetaInfo(), AttributeEncoder.Empty);
			}
		}

		public void EmitAttribute(MethodBuilder builder)
		{
			if (ResolveBuilder())
			{
				builder.SetCustomAttribute(GetCtorMetaInfo(), AttributeEncoder.Empty);
			}
		}

		public void EmitAttribute(PropertyBuilder builder)
		{
			if (ResolveBuilder())
			{
				builder.SetCustomAttribute(GetCtorMetaInfo(), AttributeEncoder.Empty);
			}
		}

		public void EmitAttribute(FieldBuilder builder)
		{
			if (ResolveBuilder())
			{
				builder.SetCustomAttribute(GetCtorMetaInfo(), AttributeEncoder.Empty);
			}
		}

		public void EmitAttribute(TypeBuilder builder)
		{
			if (ResolveBuilder())
			{
				builder.SetCustomAttribute(GetCtorMetaInfo(), AttributeEncoder.Empty);
			}
		}

		public void EmitAttribute(AssemblyBuilder builder)
		{
			if (ResolveBuilder())
			{
				builder.SetCustomAttribute(GetCtorMetaInfo(), AttributeEncoder.Empty);
			}
		}

		public void EmitAttribute(ModuleBuilder builder)
		{
			if (ResolveBuilder())
			{
				builder.SetCustomAttribute(GetCtorMetaInfo(), AttributeEncoder.Empty);
			}
		}

		public void EmitAttribute(ParameterBuilder builder)
		{
			if (ResolveBuilder())
			{
				builder.SetCustomAttribute(GetCtorMetaInfo(), AttributeEncoder.Empty);
			}
		}

		private ConstructorInfo GetCtorMetaInfo()
		{
			return (ConstructorInfo)ctor.GetMetaInfo();
		}

		public bool ResolveBuilder()
		{
			if (ctor != null)
			{
				return true;
			}
			if (!Define())
			{
				return false;
			}
			ctor = (MethodSpec)MemberCache.FindMember(type, MemberFilter.Constructor(ParametersCompiled.EmptyReadOnlyParameters), BindingRestriction.DeclaredOnly);
			return ctor != null;
		}
	}
}
