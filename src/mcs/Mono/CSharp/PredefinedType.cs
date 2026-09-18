using System.Collections.Generic;
using System.IO;

namespace Mono.CSharp
{
	public class PredefinedType
	{
		private readonly string name;

		private readonly string ns;

		private readonly int arity;

		private readonly MemberKind kind;

		protected readonly ModuleContainer module;

		protected TypeSpec type;

		private bool defined;

		public int Arity => arity;

		public bool IsDefined => type != null;

		public string Name => name;

		public string Namespace => ns;

		public TypeSpec TypeSpec => type;

		public PredefinedType(ModuleContainer module, MemberKind kind, string ns, string name, int arity)
			: this(module, kind, ns, name)
		{
			this.arity = arity;
		}

		public PredefinedType(ModuleContainer module, MemberKind kind, string ns, string name)
		{
			this.module = module;
			this.kind = kind;
			this.name = name;
			this.ns = ns;
		}

		public PredefinedType(BuiltinTypeSpec type)
		{
			kind = type.Kind;
			name = type.Name;
			ns = type.Namespace;
			this.type = type;
		}

		public bool Define()
		{
			if (type != null)
			{
				return true;
			}
			if (!defined)
			{
				defined = true;
				type = Resolve(module, kind, ns, name, arity, false, false);
			}
			return type != null;
		}

		public string GetSignatureForError()
		{
			return ns + "." + name;
		}

		public static TypeSpec Resolve(ModuleContainer module, MemberKind kind, string ns, string name, int arity, bool required, bool reportErrors)
		{
			Namespace obj = module.GlobalRootNamespace.GetNamespace(ns, required);
			IList<TypeSpec> list = null;
			if (obj != null)
			{
				list = obj.GetAllTypes(name);
			}
			if (list == null)
			{
				if (reportErrors)
				{
					module.Compiler.Report.Error(518, "The predefined type `{0}.{1}' is not defined or imported", ns, name);
				}
				return null;
			}
			TypeSpec typeSpec = null;
			foreach (TypeSpec item in list)
			{
				if ((item.Kind != kind && (item.Kind != MemberKind.Struct || kind != MemberKind.Void || !(item.MemberDefinition is TypeContainer))) || item.Arity != arity || ((item.Modifiers & Modifiers.INTERNAL) != 0 && !item.MemberDefinition.IsInternalAsPublic(module.DeclaringAssembly)))
				{
					continue;
				}
				if (typeSpec == null)
				{
					typeSpec = item;
					continue;
				}
				TypeSpec ms = typeSpec;
				if (!typeSpec.MemberDefinition.IsImported && module.Compiler.BuiltinTypes.Object.MemberDefinition.DeclaringAssembly == item.MemberDefinition.DeclaringAssembly)
				{
					typeSpec = item;
				}
				string text = ((!(typeSpec.MemberDefinition is MemberCore)) ? Path.GetFileName(((ImportedAssemblyDefinition)typeSpec.MemberDefinition.DeclaringAssembly).Location) : ((MemberCore)typeSpec.MemberDefinition).Location.Name);
				module.Compiler.Report.SymbolRelatedToPreviousError(ms);
				module.Compiler.Report.SymbolRelatedToPreviousError(item);
				module.Compiler.Report.Warning(1685, 1, "The predefined type `{0}.{1}' is defined multiple times. Using definition from `{2}'", ns, name, text);
				break;
			}
			if ((typeSpec == null) & reportErrors)
			{
				TypeSpec typeSpec2 = list[0];
				if (typeSpec2.Kind == MemberKind.MissingType)
				{
					module.Compiler.Report.Error(518, "The predefined type `{0}.{1}' is defined in an assembly that is not referenced.", ns, name);
				}
				else
				{
					Location loc;
					if (typeSpec2.MemberDefinition is MemberCore)
					{
						loc = ((MemberCore)typeSpec2.MemberDefinition).Location;
					}
					else
					{
						loc = Location.Null;
						module.Compiler.Report.SymbolRelatedToPreviousError(typeSpec2);
					}
					module.Compiler.Report.Error(520, loc, "The predefined type `{0}.{1}' is not declared correctly", ns, name);
				}
			}
			return typeSpec;
		}

		public TypeSpec Resolve()
		{
			if (type == null)
			{
				type = Resolve(module, kind, ns, name, arity, false, true);
			}
			return type;
		}
	}
}
