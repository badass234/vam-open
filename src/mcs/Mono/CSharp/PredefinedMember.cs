using System;

namespace Mono.CSharp
{
	public class PredefinedMember<T> where T : MemberSpec
	{
		private readonly ModuleContainer module;

		private T member;

		private TypeSpec declaring_type;

		private readonly PredefinedType declaring_type_predefined;

		private MemberFilter filter;

		private readonly Func<TypeSpec[]> filter_builder;

		public PredefinedMember(ModuleContainer module, PredefinedType type, MemberFilter filter)
		{
			this.module = module;
			declaring_type_predefined = type;
			this.filter = filter;
		}

		public PredefinedMember(ModuleContainer module, TypeSpec type, MemberFilter filter)
		{
			this.module = module;
			declaring_type = type;
			this.filter = filter;
		}

		public PredefinedMember(ModuleContainer module, PredefinedType type, string name, params TypeSpec[] types)
			: this(module, type, MemberFilter.Method(name, 0, ParametersCompiled.CreateFullyResolved(types), null))
		{
		}

		public PredefinedMember(ModuleContainer module, PredefinedType type, string name, MemberKind kind, params PredefinedType[] types)
			: this(module, type, new MemberFilter(name, 0, kind, null, null))
		{
			filter_builder = () =>
			{
				TypeSpec[] array = new TypeSpec[types.Length];
				for (int i = 0; i < array.Length; i++)
				{
					PredefinedType predefinedType = types[i];
					if (!predefinedType.Define())
					{
						return (TypeSpec[])null;
					}
					array[i] = predefinedType.TypeSpec;
				}
				return array;
			};
		}

		public PredefinedMember(ModuleContainer module, PredefinedType type, string name, MemberKind kind, Func<TypeSpec[]> typesBuilder, TypeSpec returnType)
			: this(module, type, new MemberFilter(name, 0, kind, null, returnType))
		{
			filter_builder = typesBuilder;
		}

		public PredefinedMember(ModuleContainer module, BuiltinTypeSpec type, string name, params TypeSpec[] types)
			: this(module, (TypeSpec)type, MemberFilter.Method(name, 0, ParametersCompiled.CreateFullyResolved(types), null))
		{
		}

		public T Get()
		{
			if (member != null)
			{
				return member;
			}
			if (declaring_type == null)
			{
				if (!declaring_type_predefined.Define())
				{
					return null;
				}
				declaring_type = declaring_type_predefined.TypeSpec;
			}
			if (filter_builder != null)
			{
				TypeSpec[] array = filter_builder();
				if (filter.Kind == MemberKind.Field)
				{
					filter = new MemberFilter(filter.Name, filter.Arity, filter.Kind, null, array[0]);
				}
				else
				{
					filter = new MemberFilter(filter.Name, filter.Arity, filter.Kind, ParametersCompiled.CreateFullyResolved(array), filter.MemberType);
				}
			}
			member = MemberCache.FindMember(declaring_type, filter, BindingRestriction.DeclaredOnly) as T;
			if (member == null)
			{
				return null;
			}
			if (!member.IsAccessible(module))
			{
				return null;
			}
			return member;
		}

		public T Resolve(Location loc)
		{
			if (member != null)
			{
				return member;
			}
			if (Get() != null)
			{
				return member;
			}
			if (declaring_type == null && declaring_type_predefined.Resolve() == null)
			{
				return null;
			}
			if (filter_builder != null)
			{
				filter = new MemberFilter(filter.Name, filter.Arity, filter.Kind, ParametersCompiled.CreateFullyResolved(filter_builder()), filter.MemberType);
			}
			string text = null;
			if (filter.Parameters != null)
			{
				text = filter.Parameters.GetSignatureForError();
			}
			module.Compiler.Report.Error(656, loc, "The compiler required member `{0}.{1}{2}' could not be found or is inaccessible", declaring_type.GetSignatureForError(), filter.Name, text);
			return null;
		}
	}
}
