using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.CSharp
{
	public class NamespaceContainer : TypeContainer, IMemberContext, IModuleContext
	{
		private static readonly Namespace[] empty_namespaces = new Namespace[0];

		private readonly Namespace ns;

		public new readonly NamespaceContainer Parent;

		private List<UsingClause> clauses;

		public bool DeclarationFound;

		private Namespace[] namespace_using_table;

		private TypeSpec[] types_using_table;

		private Dictionary<string, UsingAliasNamespace> aliases;

		public override AttributeTargets AttributeTargets
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override string DocCommentHeader
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public Namespace NS => ns;

		public List<UsingClause> Usings => clauses;

		public override string[] ValidAttributeTargets
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public NamespaceContainer(MemberName name, NamespaceContainer parent)
			: base(parent, name, null, MemberKind.Namespace)
		{
			Parent = parent;
			ns = parent.NS.AddNamespace(name);
			containers = new List<TypeContainer>();
		}

		protected NamespaceContainer(ModuleContainer parent)
			: base(parent, null, null, MemberKind.Namespace)
		{
			ns = parent.GlobalRootNamespace;
			containers = new List<TypeContainer>(2);
		}

		public void AddUsing(UsingClause un)
		{
			if (DeclarationFound)
			{
				Compiler.Report.Error(1529, un.Location, "A using clause must precede all other namespace elements except extern alias declarations");
			}
			if (clauses == null)
			{
				clauses = new List<UsingClause>();
			}
			clauses.Add(un);
		}

		public void AddUsing(UsingAliasNamespace un)
		{
			if (DeclarationFound)
			{
				Compiler.Report.Error(1529, un.Location, "A using clause must precede all other namespace elements except extern alias declarations");
			}
			AddAlias(un);
		}

		private void AddAlias(UsingAliasNamespace un)
		{
			if (clauses == null)
			{
				clauses = new List<UsingClause>();
			}
			else
			{
				foreach (UsingClause clause in clauses)
				{
					UsingAliasNamespace usingAliasNamespace = clause as UsingAliasNamespace;
					if (usingAliasNamespace != null && usingAliasNamespace.Alias.Value == un.Alias.Value)
					{
						Compiler.Report.SymbolRelatedToPreviousError(usingAliasNamespace.Location, "");
						Compiler.Report.Error(1537, un.Location, "The using alias `{0}' appeared previously in this namespace", un.Alias.Value);
					}
				}
			}
			clauses.Add(un);
		}

		public override void AddPartial(TypeDefinition next_part)
		{
			TypeSpec typeSpec = ns.LookupType(this, next_part.MemberName.Name, next_part.MemberName.Arity, LookupMode.Probing, Location.Null);
			TypeDefinition existing = ((typeSpec != null) ? (typeSpec.MemberDefinition as TypeDefinition) : null);
			AddPartial(next_part, existing);
		}

		public override void AddTypeContainer(TypeContainer tc)
		{
			MemberName memberName = tc.MemberName;
			string text = memberName.Basename;
			while (memberName.Left != null)
			{
				memberName = memberName.Left;
				text = memberName.Name;
			}
			TypeContainer typeContainer = ((Parent == null) ? ((TypeContainer)Module) : ((TypeContainer)this));
			MemberCore value;
			if (typeContainer.DefinedNames.TryGetValue(text, out value))
			{
				if (tc is NamespaceContainer && value is NamespaceContainer)
				{
					AddTypeContainerMember(tc);
					return;
				}
				base.Report.SymbolRelatedToPreviousError(value);
				if ((value.ModFlags & Modifiers.PARTIAL) != 0 && (tc is ClassOrStruct || tc is Interface))
				{
					Error_MissingPartialModifier(tc);
				}
				else
				{
					base.Report.Error(101, tc.Location, "The namespace `{0}' already contains a definition for `{1}'", GetSignatureForError(), memberName.GetSignatureForError());
				}
			}
			else
			{
				typeContainer.DefinedNames.Add(text, tc);
				TypeDefinition partialContainer = tc.PartialContainer;
				if (partialContainer != null)
				{
					IList<TypeSpec> allTypes = ns.GetAllTypes(text);
					if (allTypes != null)
					{
						foreach (TypeSpec item in allTypes)
						{
							if (item.Arity == memberName.Arity)
							{
								value = (MemberCore)item.MemberDefinition;
								break;
							}
						}
					}
					if (value != null)
					{
						base.Report.SymbolRelatedToPreviousError(value);
						base.Report.Error(101, tc.Location, "The namespace `{0}' already contains a definition for `{1}'", GetSignatureForError(), memberName.GetSignatureForError());
					}
					else
					{
						ns.AddType(Module, partialContainer.Definition);
					}
				}
			}
			base.AddTypeContainer(tc);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			throw new NotSupportedException();
		}

		public override void EmitContainer()
		{
			VerifyClsCompliance();
			base.EmitContainer();
		}

		public ExtensionMethodCandidates LookupExtensionMethod(IMemberContext invocationContext, string name, int arity, int position)
		{
			NamespaceContainer namespaceContainer = this;
			do
			{
				ExtensionMethodCandidates extensionMethodCandidates = namespaceContainer.LookupExtensionMethodCandidates(invocationContext, name, arity, ref position);
				if (extensionMethodCandidates != null || namespaceContainer.MemberName == null)
				{
					return extensionMethodCandidates;
				}
				Namespace parent = namespaceContainer.ns.Parent;
				MemberName left = namespaceContainer.MemberName.Left;
				int num = position - 2;
				while (num-- > 0)
				{
					left = left.Left;
					parent = parent.Parent;
				}
				while (left != null)
				{
					position++;
					List<MethodSpec> list = parent.LookupExtensionMethod(invocationContext, name, arity);
					if (list != null)
					{
						return new ExtensionMethodCandidates(invocationContext, list, namespaceContainer, position);
					}
					left = left.Left;
					parent = parent.Parent;
				}
				position = 0;
				namespaceContainer = namespaceContainer.Parent;
			}
			while (namespaceContainer != null);
			return null;
		}

		private ExtensionMethodCandidates LookupExtensionMethodCandidates(IMemberContext invocationContext, string name, int arity, ref int position)
		{
			List<MethodSpec> list = null;
			if (position == 0)
			{
				position++;
				list = ns.LookupExtensionMethod(invocationContext, name, arity);
				if (list != null)
				{
					return new ExtensionMethodCandidates(invocationContext, list, this, position);
				}
			}
			if (position == 1)
			{
				position++;
				Namespace[] array = namespace_using_table;
				for (int i = 0; i < array.Length; i++)
				{
					List<MethodSpec> list2 = array[i].LookupExtensionMethod(invocationContext, name, arity);
					if (list2 != null)
					{
						if (list == null)
						{
							list = list2;
						}
						else
						{
							list.AddRange(list2);
						}
					}
				}
				if (types_using_table != null)
				{
					TypeSpec[] array2 = types_using_table;
					for (int i = 0; i < array2.Length; i++)
					{
						List<MethodSpec> list3 = array2[i].MemberCache.FindExtensionMethods(invocationContext, name, arity);
						if (list3 != null)
						{
							if (list == null)
							{
								list = list3;
							}
							else
							{
								list.AddRange(list3);
							}
						}
					}
				}
				if (list != null)
				{
					return new ExtensionMethodCandidates(invocationContext, list, this, position);
				}
			}
			return null;
		}

		public override FullNamedExpression LookupNamespaceOrType(string name, int arity, LookupMode mode, Location loc)
		{
			for (NamespaceContainer namespaceContainer = this; namespaceContainer != null; namespaceContainer = namespaceContainer.Parent)
			{
				FullNamedExpression fullNamedExpression = namespaceContainer.Lookup(name, arity, mode, loc);
				if (fullNamedExpression != null || namespaceContainer.MemberName == null)
				{
					return fullNamedExpression;
				}
				Namespace parent = namespaceContainer.ns.Parent;
				MemberName left = namespaceContainer.MemberName.Left;
				while (left != null)
				{
					fullNamedExpression = parent.LookupTypeOrNamespace(this, name, arity, mode, loc);
					if (fullNamedExpression != null)
					{
						return fullNamedExpression;
					}
					left = left.Left;
					parent = parent.Parent;
				}
			}
			return null;
		}

		public override void GetCompletionStartingWith(string prefix, List<string> results)
		{
			if (Usings == null)
			{
				return;
			}
			foreach (UsingClause @using in Usings)
			{
				if (@using.Alias == null)
				{
					string name = @using.NamespaceExpression.Name;
					if (name.StartsWith(prefix))
					{
						results.Add(name);
					}
				}
			}
			IEnumerable<string> enumerable = Enumerable.Empty<string>();
			Namespace[] array = namespace_using_table;
			foreach (Namespace obj in array)
			{
				if (prefix.StartsWith(obj.Name))
				{
					int num = prefix.LastIndexOf('.');
					if (num != -1)
					{
						string prefix2 = prefix.Substring(num + 1);
						enumerable = enumerable.Concat(obj.CompletionGetTypesStartingWith(prefix2));
					}
				}
				enumerable = enumerable.Concat(obj.CompletionGetTypesStartingWith(prefix));
			}
			results.AddRange(enumerable);
			base.GetCompletionStartingWith(prefix, results);
		}

		public FullNamedExpression LookupExternAlias(string name)
		{
			if (aliases == null)
			{
				return null;
			}
			UsingAliasNamespace value;
			if (aliases.TryGetValue(name, out value) && value is UsingExternAlias)
			{
				return value.ResolvedExpression;
			}
			return null;
		}

		public override FullNamedExpression LookupNamespaceAlias(string name)
		{
			for (NamespaceContainer namespaceContainer = this; namespaceContainer != null; namespaceContainer = namespaceContainer.Parent)
			{
				UsingAliasNamespace value;
				if (namespaceContainer.aliases != null && namespaceContainer.aliases.TryGetValue(name, out value))
				{
					if (value.ResolvedExpression == null)
					{
						value.Define(namespaceContainer);
					}
					return value.ResolvedExpression;
				}
			}
			return null;
		}

		private FullNamedExpression Lookup(string name, int arity, LookupMode mode, Location loc)
		{
			FullNamedExpression fullNamedExpression = ns.LookupTypeOrNamespace(this, name, arity, mode, loc);
			UsingAliasNamespace value;
			if (aliases != null && arity == 0 && aliases.TryGetValue(name, out value))
			{
				if (fullNamedExpression != null && mode != LookupMode.Probing)
				{
					Compiler.Report.SymbolRelatedToPreviousError(value.Location, null);
					Compiler.Report.Error(576, loc, "Namespace `{0}' contains a definition with same name as alias `{1}'", GetSignatureForError(), name);
				}
				if (value.ResolvedExpression == null)
				{
					value.Define(this);
				}
				return value.ResolvedExpression;
			}
			if (fullNamedExpression != null)
			{
				return fullNamedExpression;
			}
			if (namespace_using_table == null)
			{
				DoDefineNamespace();
			}
			FullNamedExpression fullNamedExpression2 = null;
			Namespace[] array = namespace_using_table;
			for (int i = 0; i < array.Length; i++)
			{
				TypeSpec typeSpec = array[i].LookupType(this, name, arity, mode, loc);
				if (typeSpec == null)
				{
					continue;
				}
				fullNamedExpression = new TypeExpression(typeSpec, loc);
				if (fullNamedExpression2 == null)
				{
					fullNamedExpression2 = fullNamedExpression;
					continue;
				}
				TypeExpr typeExpr = fullNamedExpression as TypeExpr;
				TypeExpr typeExpr2 = fullNamedExpression2 as TypeExpr;
				if (typeExpr != null && typeExpr2 == null)
				{
					fullNamedExpression2 = fullNamedExpression;
				}
				else
				{
					if (typeExpr == null)
					{
						continue;
					}
					TypeSpec typeSpec2 = Namespace.IsImportedTypeOverride(Module, typeExpr2.Type, typeExpr.Type);
					if (typeSpec2 == null)
					{
						if (mode == LookupMode.Normal)
						{
							Error_AmbiguousReference(name, typeExpr2, typeExpr, loc);
						}
						return fullNamedExpression2;
					}
					if (typeSpec2 == typeExpr.Type)
					{
						fullNamedExpression2 = typeExpr;
					}
				}
			}
			if (types_using_table != null)
			{
				TypeSpec[] array2 = types_using_table;
				for (int i = 0; i < array2.Length; i++)
				{
					IList<MemberSpec> list = MemberCache.FindMembers(array2[i], name, true);
					if (list == null)
					{
						continue;
					}
					foreach (MemberSpec item in list)
					{
						if (arity > 0 && item.Arity != arity)
						{
							continue;
						}
						if ((item.Kind & MemberKind.NestedMask) == 0)
						{
							if ((item.Modifiers & Modifiers.STATIC) != 0 && (item.Modifiers & Modifiers.METHOD_EXTENSION) == 0)
							{
								if (mode == LookupMode.Normal)
								{
									throw new NotImplementedException();
								}
								return null;
							}
						}
						else
						{
							fullNamedExpression = new TypeExpression((TypeSpec)item, loc);
							if (fullNamedExpression2 == null)
							{
								fullNamedExpression2 = fullNamedExpression;
							}
							else if (mode == LookupMode.Normal)
							{
								Error_AmbiguousReference(name, fullNamedExpression2, fullNamedExpression, loc);
							}
						}
					}
				}
			}
			return fullNamedExpression2;
		}

		private void Error_AmbiguousReference(string name, FullNamedExpression a, FullNamedExpression b, Location loc)
		{
			Report report = Compiler.Report;
			report.SymbolRelatedToPreviousError(a.Type);
			report.SymbolRelatedToPreviousError(b.Type);
			report.Error(104, loc, "`{0}' is an ambiguous reference between `{1}' and `{2}'", name, a.GetSignatureForError(), b.GetSignatureForError());
		}

		public static Expression LookupStaticUsings(IMemberContext mc, string name, int arity, Location loc)
		{
			for (MemberCore memberCore = mc.CurrentMemberDefinition; memberCore != null; memberCore = memberCore.Parent)
			{
				NamespaceContainer namespaceContainer = memberCore as NamespaceContainer;
				if (namespaceContainer != null)
				{
					List<MemberSpec> list = null;
					if (namespaceContainer.types_using_table != null)
					{
						TypeSpec[] array = namespaceContainer.types_using_table;
						for (int i = 0; i < array.Length; i++)
						{
							IList<MemberSpec> list2 = MemberCache.FindMembers(array[i], name, true);
							if (list2 == null)
							{
								continue;
							}
							foreach (MemberSpec item in list2)
							{
								if (((item.Kind & MemberKind.NestedMask) != 0 || ((item.Modifiers & Modifiers.STATIC) != 0 && (item.Modifiers & Modifiers.METHOD_EXTENSION) == 0)) && (arity <= 0 || item.Arity == arity))
								{
									if (list == null)
									{
										list = new List<MemberSpec>();
									}
									list.Add(item);
								}
							}
						}
					}
					if (list != null)
					{
						Expression expression = Expression.MemberLookupToExpression(mc, list, false, null, name, arity, Expression.MemberLookupRestrictions.None, loc);
						if (expression != null)
						{
							return expression;
						}
					}
				}
			}
			return null;
		}

		protected override void DefineNamespace()
		{
			if (namespace_using_table == null)
			{
				DoDefineNamespace();
			}
			base.DefineNamespace();
		}

		private void DoDefineNamespace()
		{
			namespace_using_table = empty_namespaces;
			if (clauses == null)
			{
				return;
			}
			List<Namespace> list = null;
			List<TypeSpec> list2 = null;
			bool flag = false;
			for (int i = 0; i < clauses.Count; i++)
			{
				UsingClause usingClause = clauses[i];
				if (usingClause.Alias != null)
				{
					if (aliases == null)
					{
						aliases = new Dictionary<string, UsingAliasNamespace>();
					}
					if (usingClause is UsingExternAlias)
					{
						usingClause.Define(this);
						if (usingClause.ResolvedExpression != null)
						{
							aliases.Add(usingClause.Alias.Value, (UsingExternAlias)usingClause);
						}
						clauses.RemoveAt(i--);
					}
					else
					{
						flag = true;
					}
					continue;
				}
				usingClause.Define(this);
				if (usingClause.ResolvedExpression == null)
				{
					clauses.RemoveAt(i--);
					continue;
				}
				NamespaceExpression namespaceExpression = usingClause.ResolvedExpression as NamespaceExpression;
				if (namespaceExpression == null)
				{
					TypeSpec type = usingClause.ResolvedExpression.Type;
					if (list2 == null)
					{
						list2 = new List<TypeSpec>();
					}
					if (list2.Contains(type))
					{
						Warning_DuplicateEntry(usingClause);
					}
					else
					{
						list2.Add(type);
					}
				}
				else
				{
					if (list == null)
					{
						list = new List<Namespace>();
					}
					if (list.Contains(namespaceExpression.Namespace))
					{
						clauses.RemoveAt(i--);
						Warning_DuplicateEntry(usingClause);
					}
					else
					{
						list.Add(namespaceExpression.Namespace);
					}
				}
			}
			namespace_using_table = ((list == null) ? new Namespace[0] : list.ToArray());
			if (list2 != null)
			{
				types_using_table = list2.ToArray();
			}
			if (!flag)
			{
				return;
			}
			for (int j = 0; j < clauses.Count; j++)
			{
				UsingClause usingClause2 = clauses[j];
				if (usingClause2.Alias != null)
				{
					aliases.Add(usingClause2.Alias.Value, (UsingAliasNamespace)usingClause2);
				}
			}
		}

		protected override void DoDefineContainer()
		{
			base.DoDefineContainer();
			if (clauses == null)
			{
				return;
			}
			for (int i = 0; i < clauses.Count; i++)
			{
				UsingClause usingClause = clauses[i];
				if (usingClause.Alias != null && usingClause.ResolvedExpression == null)
				{
					usingClause.Define(this);
				}
			}
		}

		public void EnableRedefinition()
		{
			is_defined = false;
			namespace_using_table = null;
		}

		public override void GenerateDocComment(DocumentationBuilder builder)
		{
			if (containers == null)
			{
				return;
			}
			foreach (TypeContainer container in containers)
			{
				container.GenerateDocComment(builder);
			}
		}

		public override string GetSignatureForError()
		{
			if (base.MemberName != null)
			{
				return base.GetSignatureForError();
			}
			return "global::";
		}

		public override void RemoveContainer(TypeContainer cont)
		{
			base.RemoveContainer(cont);
			NS.RemoveContainer(cont);
		}

		protected override bool VerifyClsCompliance()
		{
			if (Module.IsClsComplianceRequired())
			{
				if (base.MemberName != null && base.MemberName.Name[0] == '_')
				{
					Warning_IdentifierNotCompliant();
				}
				ns.VerifyClsCompliance();
				return true;
			}
			return false;
		}

		private void Warning_DuplicateEntry(UsingClause entry)
		{
			Compiler.Report.Warning(105, 3, entry.Location, "The using directive for `{0}' appeared previously in this namespace", entry.ResolvedExpression.GetSignatureForError());
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
