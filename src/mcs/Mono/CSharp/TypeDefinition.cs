using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	public abstract class TypeDefinition : TypeContainer, ITypeDefinition, IMemberDefinition
	{
		public struct BaseContext : IMemberContext, IModuleContext
		{
			private TypeContainer tc;

			public CompilerContext Compiler => tc.Compiler;

			public TypeSpec CurrentType => tc.PartialContainer.CurrentType;

			public TypeParameters CurrentTypeParameters => tc.PartialContainer.CurrentTypeParameters;

			public MemberCore CurrentMemberDefinition => tc;

			public bool IsObsolete => tc.IsObsolete;

			public bool IsUnsafe => tc.IsUnsafe;

			public bool IsStatic => tc.IsStatic;

			public ModuleContainer Module => tc.Module;

			public BaseContext(TypeContainer tc)
			{
				this.tc = tc;
			}

			public string GetSignatureForError()
			{
				return tc.GetSignatureForError();
			}

			public ExtensionMethodCandidates LookupExtensionMethod(string name, int arity)
			{
				return null;
			}

			public FullNamedExpression LookupNamespaceAlias(string name)
			{
				return tc.Parent.LookupNamespaceAlias(name);
			}

			public FullNamedExpression LookupNamespaceOrType(string name, int arity, LookupMode mode, Location loc)
			{
				if (arity == 0)
				{
					TypeParameters currentTypeParameters = CurrentTypeParameters;
					if (currentTypeParameters != null)
					{
						TypeParameter typeParameter = currentTypeParameters.Find(name);
						if (typeParameter != null)
						{
							return new TypeParameterExpr(typeParameter, loc);
						}
					}
				}
				return tc.Parent.LookupNamespaceOrType(name, arity, mode, loc);
			}
		}

		[Flags]
		private enum CachedMethods
		{
			Equals = 1,
			GetHashCode = 2,
			HasStaticFieldInitializer = 4
		}

		private readonly List<MemberCore> members;

		protected List<FieldInitializer> initialized_fields;

		protected List<FieldInitializer> initialized_static_fields;

		private Dictionary<MethodSpec, Method> hoisted_base_call_proxies;

		private Dictionary<string, FullNamedExpression> Cache = new Dictionary<string, FullNamedExpression>();

		protected FieldBase first_nonstatic_field;

		protected TypeSpec base_type;

		private FullNamedExpression base_type_expr;

		protected TypeSpec[] iface_exprs;

		protected List<FullNamedExpression> type_bases;

		private List<TypeDefinition> class_partial_parts;

		private TypeDefinition InTransit;

		public TypeBuilder TypeBuilder;

		private GenericTypeParameterBuilder[] all_tp_builders;

		private TypeParameters all_type_parameters;

		public const string DefaultIndexerName = "Item";

		private bool has_normal_indexers;

		private string indexer_name;

		protected bool requires_delayed_unmanagedtype_check;

		private bool error;

		private bool members_defined;

		private bool members_defined_ok;

		protected bool has_static_constructor;

		private CachedMethods cached_method;

		protected TypeSpec spec;

		private TypeSpec current_type;

		public int DynamicSitesCounter;

		public int AnonymousMethodsCounter;

		public int MethodGroupsCounter;

		private static readonly string[] attribute_targets = new string[1] { "type" };

		private static readonly string[] attribute_targets_primary = new string[2] { "type", "method" };

		private PendingImplementation pending;

		public List<FullNamedExpression> BaseTypeExpressions => type_bases;

		public override TypeSpec CurrentType
		{
			get
			{
				if (current_type == null)
				{
					if (IsGenericOrParentIsGeneric)
					{
						TypeSpec[] targs = ((CurrentTypeParameters == null) ? TypeSpec.EmptyTypes : CurrentTypeParameters.Types);
						current_type = spec.MakeGenericType(this, targs);
					}
					else
					{
						current_type = spec;
					}
				}
				return current_type;
			}
		}

		public override TypeParameters CurrentTypeParameters => base.PartialContainer.MemberName.TypeParameters;

		private int CurrentTypeParametersStartIndex
		{
			get
			{
				int num = all_tp_builders.Length;
				if (CurrentTypeParameters != null)
				{
					return num - CurrentTypeParameters.Count;
				}
				return num;
			}
		}

		public virtual AssemblyDefinition DeclaringAssembly => Module.DeclaringAssembly;

		IAssemblyDefinition ITypeDefinition.DeclaringAssembly => Module.DeclaringAssembly;

		public TypeSpec Definition => spec;

		public bool HasMembersDefined => members_defined;

		public bool HasInstanceConstructor
		{
			get
			{
				return (caching_flags & Flags.HasInstanceConstructor) != 0;
			}
			set
			{
				caching_flags |= Flags.HasInstanceConstructor;
			}
		}

		public bool HasExplicitLayout
		{
			get
			{
				return (caching_flags & Flags.HasExplicitLayout) != 0;
			}
			set
			{
				caching_flags |= Flags.HasExplicitLayout;
			}
		}

		public bool HasOperators
		{
			get
			{
				return (caching_flags & Flags.HasUserOperators) != 0;
			}
			set
			{
				caching_flags |= Flags.HasUserOperators;
			}
		}

		public bool HasStructLayout
		{
			get
			{
				return (caching_flags & Flags.HasStructLayout) != 0;
			}
			set
			{
				caching_flags |= Flags.HasStructLayout;
			}
		}

		public TypeSpec[] Interfaces => iface_exprs;

		public bool IsGenericOrParentIsGeneric => all_type_parameters != null;

		public bool IsTopLevel => !(Parent is TypeDefinition);

		public bool IsPartial => (base.ModFlags & Modifiers.PARTIAL) != 0;

		bool ITypeDefinition.IsTypeForwarder => false;

		bool ITypeDefinition.IsCyclicTypeForwarder => false;

		private bool IsPartialPart => base.PartialContainer != this;

		public MemberCache MemberCache => spec.MemberCache;

		public List<MemberCore> Members => members;

		string ITypeDefinition.Namespace
		{
			get
			{
				TypeContainer parent = Parent;
				while (parent.Kind != MemberKind.Namespace)
				{
					parent = parent.Parent;
				}
				if (parent.MemberName != null)
				{
					return parent.GetSignatureForError();
				}
				return null;
			}
		}

		public ParametersCompiled PrimaryConstructorParameters { get; set; }

		public Arguments PrimaryConstructorBaseArguments { get; set; }

		public Location PrimaryConstructorBaseArgumentsStart { get; set; }

		public TypeParameters TypeParametersAll => all_type_parameters;

		public override string[] ValidAttributeTargets
		{
			get
			{
				if (PrimaryConstructorParameters == null)
				{
					return attribute_targets;
				}
				return attribute_targets_primary;
			}
		}

		public override AttributeTargets AttributeTargets
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public TypeSpec BaseType => spec.BaseType;

		protected virtual TypeAttributes TypeAttr => ModifiersExtensions.TypeAttr(base.ModFlags, IsTopLevel);

		public int TypeParametersCount => base.MemberName.Arity;

		TypeParameterSpec[] ITypeDefinition.TypeParameters => base.PartialContainer.CurrentTypeParameters.Types;

		public bool IsComImport
		{
			get
			{
				if (base.OptAttributes == null)
				{
					return false;
				}
				return base.OptAttributes.Contains(Module.PredefinedAttributes.ComImport);
			}
		}

		public override string DocComment
		{
			get
			{
				return comment;
			}
			set
			{
				if (value != null)
				{
					comment += value;
				}
			}
		}

		public PendingImplementation PendingImplementations => pending;

		public bool HasEquals => (cached_method & CachedMethods.Equals) != 0;

		public bool HasGetHashCode => (cached_method & CachedMethods.GetHashCode) != 0;

		public bool HasStaticFieldInitializer
		{
			get
			{
				return (cached_method & CachedMethods.HasStaticFieldInitializer) != 0;
			}
			set
			{
				if (value)
				{
					cached_method |= CachedMethods.HasStaticFieldInitializer;
				}
				else
				{
					cached_method &= ~CachedMethods.HasStaticFieldInitializer;
				}
			}
		}

		public override string DocCommentHeader => "T:";

		protected TypeDefinition(TypeContainer parent, MemberName name, Attributes attrs, MemberKind kind)
			: base(parent, name, attrs, kind)
		{
			base.PartialContainer = this;
			members = new List<MemberCore>();
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public void AddMember(MemberCore symbol)
		{
			if (symbol.MemberName.ExplicitInterface != null && Kind != MemberKind.Class && Kind != MemberKind.Struct)
			{
				base.Report.Error(541, symbol.Location, "`{0}': explicit interface declaration can only be declared in a class or struct", symbol.GetSignatureForError());
			}
			AddNameToContainer(symbol, symbol.MemberName.Name);
			members.Add(symbol);
		}

		public override void AddTypeContainer(TypeContainer tc)
		{
			AddNameToContainer(tc, tc.MemberName.Basename);
			base.AddTypeContainer(tc);
		}

		protected override void AddTypeContainerMember(TypeContainer tc)
		{
			members.Add(tc);
			if (containers == null)
			{
				containers = new List<TypeContainer>();
			}
			base.AddTypeContainerMember(tc);
		}

		public virtual void AddNameToContainer(MemberCore symbol, string name)
		{
			if (((base.ModFlags | symbol.ModFlags) & Modifiers.COMPILER_GENERATED) != 0)
			{
				return;
			}
			MemberCore value;
			if (!base.PartialContainer.defined_names.TryGetValue(name, out value))
			{
				base.PartialContainer.defined_names.Add(name, symbol);
			}
			else
			{
				if (symbol.EnableOverloadChecks(value))
				{
					return;
				}
				InterfaceMemberBase interfaceMemberBase = value as InterfaceMemberBase;
				if (interfaceMemberBase == null || !interfaceMemberBase.IsExplicitImpl)
				{
					base.Report.SymbolRelatedToPreviousError(value);
					if ((value.ModFlags & Modifiers.PARTIAL) != 0 && (symbol is ClassOrStruct || symbol is Interface))
					{
						Error_MissingPartialModifier(symbol);
					}
					else if (symbol is TypeParameter)
					{
						base.Report.Error(692, symbol.Location, "Duplicate type parameter `{0}'", symbol.GetSignatureForError());
					}
					else
					{
						base.Report.Error(102, symbol.Location, "The type `{0}' already contains a definition for `{1}'", GetSignatureForError(), name);
					}
				}
			}
		}

		public void AddConstructor(Constructor c)
		{
			AddConstructor(c, false);
		}

		public void AddConstructor(Constructor c, bool isDefault)
		{
			bool flag = (c.ModFlags & Modifiers.STATIC) != 0;
			if (!isDefault)
			{
				AddNameToContainer(c, flag ? Constructor.TypeConstructorName : Constructor.ConstructorName);
			}
			if (flag && c.ParameterInfo.IsEmpty)
			{
				base.PartialContainer.has_static_constructor = true;
			}
			else
			{
				base.PartialContainer.HasInstanceConstructor = true;
			}
			members.Add(c);
		}

		public bool AddField(FieldBase field)
		{
			AddMember(field);
			if ((field.ModFlags & Modifiers.STATIC) != 0)
			{
				return true;
			}
			FieldBase fieldBase = base.PartialContainer.first_nonstatic_field;
			if (fieldBase == null)
			{
				base.PartialContainer.first_nonstatic_field = field;
				return true;
			}
			if (Kind == MemberKind.Struct && fieldBase.Parent != field.Parent)
			{
				base.Report.SymbolRelatedToPreviousError(fieldBase.Parent);
				base.Report.Warning(282, 3, field.Location, "struct instance field `{0}' found in different declaration from instance field `{1}'", field.GetSignatureForError(), fieldBase.GetSignatureForError());
			}
			return true;
		}

		public void AddIndexer(Indexer i)
		{
			members.Add(i);
		}

		public void AddOperator(Operator op)
		{
			base.PartialContainer.HasOperators = true;
			AddMember(op);
		}

		public void AddPartialPart(TypeDefinition part)
		{
			if (Kind == MemberKind.Class)
			{
				if (class_partial_parts == null)
				{
					class_partial_parts = new List<TypeDefinition>();
				}
				class_partial_parts.Add(part);
			}
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.Method)
			{
				foreach (MemberCore member in members)
				{
					Constructor constructor = member as Constructor;
					if (constructor != null && constructor.IsPrimaryConstructor)
					{
						constructor.ApplyAttributeBuilder(a, ctor, cdata, pa);
						return;
					}
				}
				throw new InternalErrorException();
			}
			if (has_normal_indexers && a.Type == pa.DefaultMember)
			{
				base.Report.Error(646, a.Location, "Cannot specify the `DefaultMember' attribute on type containing an indexer");
			}
			else if (a.Type == pa.Required)
			{
				base.Report.Error(1608, a.Location, "The RequiredAttribute attribute is not permitted on C# types");
			}
			else
			{
				TypeBuilder.SetCustomAttribute((ConstructorInfo)ctor.GetMetaInfo(), cdata);
			}
		}

		public string GetAttributeDefaultMember()
		{
			return indexer_name ?? "Item";
		}

		public void RegisterFieldForInitialization(MemberCore field, FieldInitializer expression)
		{
			if (IsPartialPart)
			{
				base.PartialContainer.RegisterFieldForInitialization(field, expression);
			}
			if ((field.ModFlags & Modifiers.STATIC) != 0)
			{
				if (initialized_static_fields == null)
				{
					HasStaticFieldInitializer = true;
					initialized_static_fields = new List<FieldInitializer>(4);
				}
				initialized_static_fields.Add(expression);
				return;
			}
			if (Kind == MemberKind.Struct && Compiler.Settings.Version != LanguageVersion.Experimental)
			{
				base.Report.Error(573, expression.Location, "'{0}': Structs cannot have instance property or field initializers", GetSignatureForError());
			}
			if (initialized_fields == null)
			{
				initialized_fields = new List<FieldInitializer>(4);
			}
			initialized_fields.Add(expression);
		}

		public void ResolveFieldInitializers(BlockContext ec)
		{
			if (ec.IsStatic)
			{
				if (initialized_static_fields == null)
				{
					return;
				}
				bool flag = !ec.Module.Compiler.Settings.Optimize;
				ExpressionStatement[] array = new ExpressionStatement[initialized_static_fields.Count];
				for (int i = 0; i < initialized_static_fields.Count; i++)
				{
					FieldInitializer fieldInitializer = initialized_static_fields[i];
					ExpressionStatement expressionStatement = fieldInitializer.ResolveStatement(ec);
					if (expressionStatement == null)
					{
						expressionStatement = EmptyExpressionStatement.Instance;
					}
					else if (!fieldInitializer.IsSideEffectFree)
					{
						flag = true;
					}
					array[i] = expressionStatement;
				}
				for (int i = 0; i < initialized_static_fields.Count; i++)
				{
					FieldInitializer fieldInitializer2 = initialized_static_fields[i];
					if (flag || !fieldInitializer2.IsDefaultInitializer)
					{
						ec.AssignmentInfoOffset += fieldInitializer2.AssignmentOffset;
						ec.CurrentBlock.AddScopeStatement(new StatementExpression(array[i]));
					}
				}
			}
			else
			{
				if (initialized_fields == null)
				{
					return;
				}
				for (int j = 0; j < initialized_fields.Count; j++)
				{
					FieldInitializer fieldInitializer3 = initialized_fields[j];
					Expression expression = fieldInitializer3.Clone(new CloneContext());
					ExpressionStatement expressionStatement2 = fieldInitializer3.ResolveStatement(ec);
					if (expressionStatement2 == null)
					{
						initialized_fields[j] = new FieldInitializer(fieldInitializer3.Field, ErrorExpression.Instance, Location.Null);
					}
					else if (!fieldInitializer3.IsDefaultInitializer || Kind == MemberKind.Struct || !ec.Module.Compiler.Settings.Optimize)
					{
						ec.AssignmentInfoOffset += fieldInitializer3.AssignmentOffset;
						ec.CurrentBlock.AddScopeStatement(new StatementExpression(expressionStatement2));
						initialized_fields[j] = (FieldInitializer)expression;
					}
				}
			}
		}

		public override void GenerateDocComment(DocumentationBuilder builder)
		{
			if (IsPartialPart)
			{
				return;
			}
			base.GenerateDocComment(builder);
			foreach (MemberCore member in members)
			{
				member.GenerateDocComment(builder);
			}
		}

		public TypeSpec GetAttributeCoClass()
		{
			if (base.OptAttributes == null)
			{
				return null;
			}
			return base.OptAttributes.Search(Module.PredefinedAttributes.CoClass)?.GetCoClassAttributeValue();
		}

		public AttributeUsageAttribute GetAttributeUsage(PredefinedAttribute pa)
		{
			Attribute attribute = null;
			if (base.OptAttributes != null)
			{
				attribute = base.OptAttributes.Search(pa);
			}
			return attribute?.GetAttributeUsageAttribute();
		}

		public virtual CompilationSourceFile GetCompilationSourceFile()
		{
			TypeContainer parent = Parent;
			CompilationSourceFile compilationSourceFile;
			while (true)
			{
				compilationSourceFile = parent as CompilationSourceFile;
				if (compilationSourceFile != null)
				{
					break;
				}
				parent = parent.Parent;
			}
			return compilationSourceFile;
		}

		public override string GetSignatureForMetadata()
		{
			if (Parent is TypeDefinition)
			{
				return Parent.GetSignatureForMetadata() + "+" + TypeNameParser.Escape(FilterNestedName(base.MemberName.Basename));
			}
			return base.GetSignatureForMetadata();
		}

		public virtual void SetBaseTypes(List<FullNamedExpression> baseTypes)
		{
			type_bases = baseTypes;
		}

		protected virtual TypeSpec[] ResolveBaseTypes(out FullNamedExpression base_class)
		{
			base_class = null;
			if (type_bases == null)
			{
				return null;
			}
			int count = type_bases.Count;
			TypeSpec[] array = null;
			BaseContext baseContext = new BaseContext(this);
			int i = 0;
			int num = 0;
			for (; i < count; i++)
			{
				FullNamedExpression fullNamedExpression = type_bases[i];
				TypeSpec typeSpec = fullNamedExpression.ResolveAsType(baseContext);
				if (typeSpec == null)
				{
					continue;
				}
				if (i == 0 && Kind == MemberKind.Class && !typeSpec.IsInterface)
				{
					if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
					{
						base.Report.Error(1965, base.Location, "Class `{0}' cannot derive from the dynamic type", GetSignatureForError());
						continue;
					}
					base_type = typeSpec;
					base_class = fullNamedExpression;
					continue;
				}
				if (array == null)
				{
					array = new TypeSpec[count - i];
				}
				if (typeSpec.IsInterface)
				{
					for (int j = 0; j < num; j++)
					{
						if (typeSpec == array[j])
						{
							base.Report.Error(528, base.Location, "`{0}' is already listed in interface list", typeSpec.GetSignatureForError());
							break;
						}
					}
					if (Kind == MemberKind.Interface && !IsAccessibleAs(typeSpec))
					{
						base.Report.Error(61, fullNamedExpression.Location, "Inconsistent accessibility: base interface `{0}' is less accessible than interface `{1}'", typeSpec.GetSignatureForError(), GetSignatureForError());
					}
				}
				else
				{
					base.Report.SymbolRelatedToPreviousError(typeSpec);
					if (Kind != MemberKind.Class)
					{
						base.Report.Error(527, fullNamedExpression.Location, "Type `{0}' in interface list is not an interface", typeSpec.GetSignatureForError());
					}
					else if (base_class != null)
					{
						base.Report.Error(1721, fullNamedExpression.Location, "`{0}': Classes cannot have multiple base classes (`{1}' and `{2}')", GetSignatureForError(), base_class.GetSignatureForError(), typeSpec.GetSignatureForError());
					}
					else
					{
						base.Report.Error(1722, fullNamedExpression.Location, "`{0}': Base class `{1}' must be specified as first", GetSignatureForError(), typeSpec.GetSignatureForError());
					}
				}
				array[num++] = typeSpec;
			}
			return array;
		}

		private void CheckPairedOperators()
		{
			bool flag = false;
			List<Operator.OpType> list = new List<Operator.OpType>();
			for (int i = 0; i < members.Count; i++)
			{
				Operator obj = members[i] as Operator;
				if (obj == null)
				{
					continue;
				}
				Operator.OpType operatorType = obj.OperatorType;
				if (operatorType == Operator.OpType.Equality || operatorType == Operator.OpType.Inequality)
				{
					flag = true;
				}
				if (list.Contains(operatorType))
				{
					continue;
				}
				Operator.OpType matchingOperator = obj.GetMatchingOperator();
				if (matchingOperator == Operator.OpType.TOP)
				{
					continue;
				}
				bool flag2 = false;
				for (int j = 0; j < members.Count; j++)
				{
					Operator obj2 = members[j] as Operator;
					if (obj2 != null && obj2.OperatorType == matchingOperator && TypeSpecComparer.IsEqual(obj.ReturnType, obj2.ReturnType) && TypeSpecComparer.Equals(obj.ParameterTypes, obj2.ParameterTypes))
					{
						list.Add(matchingOperator);
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					base.Report.Error(216, obj.Location, "The operator `{0}' requires a matching operator `{1}' to also be defined", obj.GetSignatureForError(), Operator.GetName(matchingOperator));
				}
			}
			if (flag)
			{
				if (!HasEquals)
				{
					base.Report.Warning(660, 2, base.Location, "`{0}' defines operator == or operator != but does not override Object.Equals(object o)", GetSignatureForError());
				}
				if (!HasGetHashCode)
				{
					base.Report.Warning(661, 2, base.Location, "`{0}' defines operator == or operator != but does not override Object.GetHashCode()", GetSignatureForError());
				}
			}
		}

		public override void CreateMetadataName(StringBuilder sb)
		{
			if (Parent.MemberName != null)
			{
				Parent.CreateMetadataName(sb);
				if (sb.Length != 0)
				{
					sb.Append(".");
				}
			}
			sb.Append(base.MemberName.Basename);
		}

		private bool CreateTypeBuilder()
		{
			int typeSize = ((Kind == MemberKind.Struct && first_nonstatic_field == null && !(this is StateMachine)) ? 1 : 0);
			TypeDefinition typeDefinition = Parent as TypeDefinition;
			if (typeDefinition == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				CreateMetadataName(stringBuilder);
				TypeBuilder = Module.CreateBuilder(stringBuilder.ToString(), TypeAttr, typeSize);
			}
			else
			{
				TypeBuilder = typeDefinition.TypeBuilder.DefineNestedType(FilterNestedName(base.MemberName.Basename), TypeAttr, null, typeSize);
			}
			if (DeclaringAssembly.Importer != null)
			{
				DeclaringAssembly.Importer.AddCompiledType(TypeBuilder, spec);
			}
			spec.SetMetaInfo(TypeBuilder);
			spec.MemberCache = new MemberCache(this);
			TypeParameters typeParameters = null;
			if (typeDefinition != null)
			{
				spec.DeclaringType = Parent.CurrentType;
				typeDefinition.MemberCache.AddMember(spec);
				typeParameters = typeDefinition.all_type_parameters;
			}
			if (base.MemberName.TypeParameters != null || typeParameters != null)
			{
				string[] names = CreateTypeParameters(typeParameters);
				all_tp_builders = TypeBuilder.DefineGenericParameters(names);
				if (CurrentTypeParameters != null)
				{
					CurrentTypeParameters.Create(spec, CurrentTypeParametersStartIndex, this);
					CurrentTypeParameters.Define(all_tp_builders);
				}
			}
			return true;
		}

		public static string FilterNestedName(string name)
		{
			return name.Replace('.', '_');
		}

		private string[] CreateTypeParameters(TypeParameters parentAllTypeParameters)
		{
			int num = 0;
			string[] array;
			if (parentAllTypeParameters != null)
			{
				if (CurrentTypeParameters == null)
				{
					all_type_parameters = parentAllTypeParameters;
					return parentAllTypeParameters.GetAllNames();
				}
				array = new string[parentAllTypeParameters.Count + CurrentTypeParameters.Count];
				all_type_parameters = new TypeParameters(array.Length);
				all_type_parameters.Add(parentAllTypeParameters);
				num = all_type_parameters.Count;
				for (int i = 0; i < num; i++)
				{
					array[i] = all_type_parameters[i].MemberName.Name;
				}
			}
			else
			{
				array = new string[CurrentTypeParameters.Count];
			}
			for (int j = 0; j < CurrentTypeParameters.Count; j++)
			{
				if (all_type_parameters != null)
				{
					all_type_parameters.Add(base.MemberName.TypeParameters[j]);
				}
				string text = (array[num + j] = CurrentTypeParameters[j].MemberName.Name);
				for (int k = 0; k < num + j; k++)
				{
					if (!(array[k] != text))
					{
						TypeParameter typeParameter = CurrentTypeParameters[j];
						TypeParameter conflict = all_type_parameters[k];
						typeParameter.WarningParentNameConflict(conflict);
					}
				}
			}
			if (all_type_parameters == null)
			{
				all_type_parameters = CurrentTypeParameters;
			}
			return array;
		}

		public SourceMethodBuilder CreateMethodSymbolEntry()
		{
			if (Module.DeclaringAssembly.SymbolWriter == null || (base.ModFlags & Modifiers.DEBUGGER_HIDDEN) != 0)
			{
				return null;
			}
			CompilationSourceFile compilationSourceFile = GetCompilationSourceFile();
			if (compilationSourceFile == null)
			{
				return null;
			}
			return new SourceMethodBuilder(compilationSourceFile.SymbolUnitEntry);
		}

		public MethodSpec CreateHoistedBaseCallProxy(ResolveContext rc, MethodSpec method)
		{
			Method value;
			if (hoisted_base_call_proxies == null)
			{
				hoisted_base_call_proxies = new Dictionary<MethodSpec, Method>();
				value = null;
			}
			else
			{
				hoisted_base_call_proxies.TryGetValue(method, out value);
			}
			if (value == null)
			{
				string name = CompilerGeneratedContainer.MakeName(method.Name, null, "BaseCallProxy", hoisted_base_call_proxies.Count);
				TypeArguments typeArguments = null;
				TypeSpec typeSpec = method.ReturnType;
				TypeSpec[] array = method.Parameters.Types;
				MemberName name2;
				if (method.IsGeneric)
				{
					TypeParameterSpec[] typeParameters = method.GenericDefinition.TypeParameters;
					TypeParameters typeParameters2 = new TypeParameters();
					typeArguments = new TypeArguments();
					typeArguments.Arguments = new TypeSpec[typeParameters.Length];
					for (int i = 0; i < typeParameters.Length; i++)
					{
						TypeParameterSpec typeParameterSpec = typeParameters[i];
						TypeParameter typeParameter = new TypeParameter(typeParameterSpec, null, new MemberName(typeParameterSpec.Name, base.Location), null);
						typeParameters2.Add(typeParameter);
						typeArguments.Add(new SimpleName(typeParameterSpec.Name, base.Location));
						typeArguments.Arguments[i] = typeParameter.Type;
					}
					name2 = new MemberName(name, typeParameters2, base.Location);
					TypeParameterMutator typeParameterMutator = new TypeParameterMutator(typeParameters, typeParameters2);
					typeSpec = typeParameterMutator.Mutate(typeSpec);
					array = typeParameterMutator.Mutate(array);
				}
				else
				{
					name2 = new MemberName(name);
				}
				Parameter[] array2 = new Parameter[method.Parameters.Count];
				for (int j = 0; j < array2.Length; j++)
				{
					IParameterData parameterData = method.Parameters.FixedParameters[j];
					array2[j] = new Parameter(new TypeExpression(array[j], base.Location), parameterData.Name, parameterData.ModFlags, null, base.Location);
					array2[j].Resolve(this, j);
				}
				ParametersCompiled parametersCompiled = ParametersCompiled.CreateFullyResolved(array2, method.Parameters.Types);
				if (method.Parameters.HasArglist)
				{
					parametersCompiled.FixedParameters[0] = new Parameter(null, "__arglist", Parameter.Modifier.NONE, null, base.Location);
					parametersCompiled.Types[0] = Module.PredefinedTypes.RuntimeArgumentHandle.Resolve();
				}
				value = new Method(this, new TypeExpression(typeSpec, base.Location), Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED | Modifiers.DEBUGGER_HIDDEN, name2, parametersCompiled, null);
				ToplevelBlock toplevelBlock = new ToplevelBlock(Compiler, value.ParameterInfo, base.Location)
				{
					IsCompilerGenerated = true
				};
				MethodGroupExpr methodGroupExpr = MethodGroupExpr.CreatePredefined(method, method.DeclaringType, base.Location);
				methodGroupExpr.InstanceExpression = new BaseThis(method.DeclaringType, base.Location);
				if (typeArguments != null)
				{
					methodGroupExpr.SetTypeArguments(rc, typeArguments);
				}
				Invocation expr = new Invocation(methodGroupExpr, toplevelBlock.GetAllParametersArguments());
				Statement s = ((method.ReturnType.Kind != MemberKind.Void) ? ((Statement)new Return(expr, base.Location)) : ((Statement)new StatementExpression(expr)));
				toplevelBlock.AddStatement(s);
				value.Block = toplevelBlock;
				members.Add(value);
				value.Define();
				value.PrepareEmit();
				hoisted_base_call_proxies.Add(method, value);
			}
			return value.Spec;
		}

		protected bool DefineBaseTypes()
		{
			if (IsPartialPart && Kind == MemberKind.Class)
			{
				return true;
			}
			return DoDefineBaseType();
		}

		private bool DoDefineBaseType()
		{
			iface_exprs = ResolveBaseTypes(out base_type_expr);
			bool flag;
			if (IsPartialPart)
			{
				flag = false;
				if (base_type_expr != null)
				{
					if (base.PartialContainer.base_type_expr != null && base.PartialContainer.base_type != base_type)
					{
						base.Report.SymbolRelatedToPreviousError(base_type_expr.Location, "");
						base.Report.Error(263, base.Location, "Partial declarations of `{0}' must not specify different base classes", GetSignatureForError());
					}
					else
					{
						base.PartialContainer.base_type_expr = base_type_expr;
						base.PartialContainer.base_type = base_type;
						flag = true;
					}
				}
				if (iface_exprs != null)
				{
					if (base.PartialContainer.iface_exprs == null)
					{
						base.PartialContainer.iface_exprs = iface_exprs;
					}
					else
					{
						List<TypeSpec> list = new List<TypeSpec>(base.PartialContainer.iface_exprs);
						TypeSpec[] array = iface_exprs;
						foreach (TypeSpec item in array)
						{
							if (!list.Contains(item))
							{
								list.Add(item);
							}
						}
						base.PartialContainer.iface_exprs = list.ToArray();
					}
				}
				base.PartialContainer.members.AddRange(members);
				if (containers != null)
				{
					if (base.PartialContainer.containers == null)
					{
						base.PartialContainer.containers = new List<TypeContainer>();
					}
					base.PartialContainer.containers.AddRange(containers);
				}
				if (PrimaryConstructorParameters != null)
				{
					if (base.PartialContainer.PrimaryConstructorParameters != null)
					{
						base.Report.Error(8036, base.Location, "Only one part of a partial type can declare primary constructor parameters");
					}
					else
					{
						base.PartialContainer.PrimaryConstructorParameters = PrimaryConstructorParameters;
					}
				}
				members_defined = (members_defined_ok = true);
				caching_flags |= Flags.CloseTypeCreated;
			}
			else
			{
				flag = true;
			}
			TypeSpec typeSpec = CheckRecursiveDefinition(this);
			if (typeSpec != null)
			{
				base.Report.SymbolRelatedToPreviousError(typeSpec);
				if (this is Interface)
				{
					base.Report.Error(529, base.Location, "Inherited interface `{0}' causes a cycle in the interface hierarchy of `{1}'", GetSignatureForError(), typeSpec.GetSignatureForError());
					iface_exprs = null;
					base.PartialContainer.iface_exprs = null;
				}
				else
				{
					base.Report.Error(146, base.Location, "Circular base class dependency involving `{0}' and `{1}'", GetSignatureForError(), typeSpec.GetSignatureForError());
					base_type = null;
					base.PartialContainer.base_type = null;
				}
			}
			if (iface_exprs != null)
			{
				if (!PrimaryConstructorBaseArgumentsStart.IsNull)
				{
					base.Report.Error(8049, PrimaryConstructorBaseArgumentsStart, "Implemented interfaces cannot have arguments");
				}
				TypeSpec[] array = iface_exprs;
				foreach (TypeSpec typeSpec2 in array)
				{
					if (typeSpec2 != null && spec.AddInterfaceDefined(typeSpec2))
					{
						TypeBuilder.AddInterfaceImplementation(typeSpec2.GetMetaInfo());
					}
				}
			}
			if (Kind == MemberKind.Interface)
			{
				spec.BaseType = Compiler.BuiltinTypes.Object;
				return true;
			}
			if (flag)
			{
				SetBaseType();
			}
			if (class_partial_parts != null)
			{
				foreach (TypeDefinition class_partial_part in class_partial_parts)
				{
					if (class_partial_part.PrimaryConstructorBaseArguments != null)
					{
						PrimaryConstructorBaseArguments = class_partial_part.PrimaryConstructorBaseArguments;
					}
					class_partial_part.DoDefineBaseType();
				}
			}
			return true;
		}

		private void SetBaseType()
		{
			if (base_type == null)
			{
				TypeBuilder.SetParent(null);
			}
			else if (spec.BaseType != base_type)
			{
				spec.BaseType = base_type;
				if (IsPartialPart)
				{
					spec.UpdateInflatedInstancesBaseType();
				}
				TypeBuilder.SetParent(base_type.GetMetaInfo());
			}
		}

		public override void ExpandBaseInterfaces()
		{
			if (!IsPartialPart)
			{
				DoExpandBaseInterfaces();
			}
			base.ExpandBaseInterfaces();
		}

		public void DoExpandBaseInterfaces()
		{
			if ((caching_flags & Flags.InterfacesExpanded) != 0)
			{
				return;
			}
			caching_flags |= Flags.InterfacesExpanded;
			if (iface_exprs != null)
			{
				TypeSpec[] array = iface_exprs;
				foreach (TypeSpec typeSpec in array)
				{
					if (typeSpec == null)
					{
						continue;
					}
					(typeSpec.MemberDefinition as TypeDefinition)?.DoExpandBaseInterfaces();
					if (typeSpec.Interfaces == null)
					{
						continue;
					}
					foreach (TypeSpec @interface in typeSpec.Interfaces)
					{
						if (spec.AddInterfaceDefined(@interface))
						{
							TypeBuilder.AddInterfaceImplementation(@interface.GetMetaInfo());
						}
					}
				}
			}
			if (base_type == null)
			{
				return;
			}
			(base_type.MemberDefinition as TypeDefinition)?.DoExpandBaseInterfaces();
			if (base_type.Interfaces == null)
			{
				return;
			}
			foreach (TypeSpec interface2 in base_type.Interfaces)
			{
				spec.AddInterfaceDefined(interface2);
			}
		}

		public override void PrepareEmit()
		{
			if ((caching_flags & Flags.CloseTypeCreated) != 0)
			{
				return;
			}
			foreach (MemberCore member in members)
			{
				PropertyBasedMember propertyBasedMember = member as PropertyBasedMember;
				if (propertyBasedMember != null)
				{
					propertyBasedMember.PrepareEmit();
					continue;
				}
				MethodCore methodCore = member as MethodCore;
				if (methodCore != null)
				{
					methodCore.PrepareEmit();
				}
				else
				{
					(member as Const)?.DefineValue();
				}
			}
			base.PrepareEmit();
		}

		public override bool CreateContainer()
		{
			if (TypeBuilder != null)
			{
				return !error;
			}
			if (error)
			{
				return false;
			}
			if (IsPartialPart)
			{
				spec = base.PartialContainer.spec;
				TypeBuilder = base.PartialContainer.TypeBuilder;
				all_tp_builders = base.PartialContainer.all_tp_builders;
				all_type_parameters = base.PartialContainer.all_type_parameters;
			}
			else if (!CreateTypeBuilder())
			{
				error = true;
				return false;
			}
			return base.CreateContainer();
		}

		protected override void DoDefineContainer()
		{
			DefineBaseTypes();
			DoResolveTypeParameters();
		}

		public void SetPredefinedSpec(BuiltinTypeSpec spec)
		{
			spec.SetMetaInfo(TypeBuilder);
			spec.MemberCache = this.spec.MemberCache;
			spec.DeclaringType = this.spec.DeclaringType;
			this.spec = spec;
			current_type = null;
			if (class_partial_parts == null)
			{
				return;
			}
			foreach (TypeDefinition class_partial_part in class_partial_parts)
			{
				class_partial_part.spec = spec;
			}
		}

		public override void RemoveContainer(TypeContainer cont)
		{
			base.RemoveContainer(cont);
			Members.Remove(cont);
			Cache.Remove(cont.MemberName.Basename);
		}

		protected virtual bool DoResolveTypeParameters()
		{
			TypeParameters typeParameters = base.MemberName.TypeParameters;
			if (typeParameters == null)
			{
				return true;
			}
			BaseContext baseContext = new BaseContext(this);
			for (int i = 0; i < typeParameters.Count; i++)
			{
				TypeParameter typeParameter = typeParameters[i];
				if (!typeParameter.ResolveConstraints(baseContext))
				{
					error = true;
					return false;
				}
				if (!IsPartialPart)
				{
					continue;
				}
				TypeParameter typeParameter2 = base.PartialContainer.CurrentTypeParameters[i];
				typeParameter.Create(spec, this);
				typeParameter.Define(typeParameter2);
				if (typeParameter.OptAttributes != null)
				{
					if (typeParameter2.OptAttributes == null)
					{
						typeParameter2.OptAttributes = typeParameter.OptAttributes;
					}
					else
					{
						typeParameter2.OptAttributes.Attrs.AddRange(typeParameter.OptAttributes.Attrs);
					}
				}
			}
			if (IsPartialPart)
			{
				base.PartialContainer.CurrentTypeParameters.UpdateConstraints(this);
			}
			return true;
		}

		private TypeSpec CheckRecursiveDefinition(TypeDefinition tc)
		{
			if (InTransit != null)
			{
				return spec;
			}
			InTransit = tc;
			if (base_type != null)
			{
				TypeDefinition typeDefinition = base_type.MemberDefinition as TypeDefinition;
				if (typeDefinition != null && typeDefinition.CheckRecursiveDefinition(this) != null)
				{
					return base_type;
				}
			}
			if (iface_exprs != null)
			{
				TypeSpec[] array = iface_exprs;
				foreach (TypeSpec typeSpec in array)
				{
					if (typeSpec != null)
					{
						Interface obj = typeSpec.MemberDefinition as Interface;
						if (obj != null && obj.CheckRecursiveDefinition(this) != null)
						{
							return typeSpec;
						}
					}
				}
			}
			if (!IsTopLevel && Parent.PartialContainer.CheckRecursiveDefinition(this) != null)
			{
				return spec;
			}
			InTransit = null;
			return null;
		}

		public sealed override bool Define()
		{
			if (members_defined)
			{
				return members_defined_ok;
			}
			members_defined_ok = DoDefineMembers();
			members_defined = true;
			base.Define();
			return members_defined_ok;
		}

		protected virtual bool DoDefineMembers()
		{
			if (iface_exprs != null)
			{
				TypeSpec[] array = iface_exprs;
				foreach (TypeSpec typeSpec in array)
				{
					if (typeSpec == null)
					{
						continue;
					}
					(typeSpec.MemberDefinition as Interface)?.Define();
					ObsoleteAttribute attributeObsolete = typeSpec.GetAttributeObsolete();
					if (attributeObsolete != null && !base.IsObsolete)
					{
						AttributeTester.Report_ObsoleteMessage(attributeObsolete, typeSpec.GetSignatureForError(), base.Location, base.Report);
					}
					if (typeSpec.Arity > 0)
					{
						VarianceDecl.CheckTypeVariance(typeSpec, Variance.Covariant, this);
						if (((InflatedTypeSpec)typeSpec).HasDynamicArgument() && !base.IsCompilerGenerated)
						{
							base.Report.Error(1966, base.Location, "`{0}': cannot implement a dynamic interface `{1}'", GetSignatureForError(), typeSpec.GetSignatureForError());
							return false;
						}
					}
					if (!typeSpec.IsGenericOrParentIsGeneric)
					{
						continue;
					}
					TypeSpec[] array2 = iface_exprs;
					foreach (TypeSpec typeSpec2 in array2)
					{
						if (typeSpec2 == typeSpec || typeSpec2 == null)
						{
							break;
						}
						if (TypeSpecComparer.Unify.IsEqual(typeSpec, typeSpec2))
						{
							base.Report.Error(695, base.Location, "`{0}' cannot implement both `{1}' and `{2}' because they may unify for some type parameter substitutions", GetSignatureForError(), typeSpec2.GetSignatureForError(), typeSpec.GetSignatureForError());
						}
					}
				}
				if (Kind == MemberKind.Interface)
				{
					foreach (TypeSpec @interface in spec.Interfaces)
					{
						MemberCache.AddInterface(@interface);
					}
				}
			}
			if (base_type != null)
			{
				if (base_type_expr != null)
				{
					ObsoleteAttribute attributeObsolete2 = base_type.GetAttributeObsolete();
					if (attributeObsolete2 != null && !base.IsObsolete)
					{
						AttributeTester.Report_ObsoleteMessage(attributeObsolete2, base_type.GetSignatureForError(), base_type_expr.Location, base.Report);
					}
					if (IsGenericOrParentIsGeneric && base_type.IsAttribute)
					{
						base.Report.Error(698, base_type_expr.Location, "A generic type cannot derive from `{0}' because it is an attribute class", base_type.GetSignatureForError());
					}
				}
				ClassOrStruct classOrStruct = base_type.MemberDefinition as ClassOrStruct;
				if (classOrStruct != null)
				{
					classOrStruct.Define();
					if (HasMembersDefined)
					{
						return true;
					}
				}
			}
			if (Kind == MemberKind.Struct || Kind == MemberKind.Class)
			{
				pending = PendingImplementation.GetPendingImplementations(this);
			}
			int count = members.Count;
			for (int k = 0; k < count; k++)
			{
				InterfaceMemberBase interfaceMemberBase = members[k] as InterfaceMemberBase;
				if (interfaceMemberBase != null && interfaceMemberBase.IsExplicitImpl)
				{
					try
					{
						interfaceMemberBase.Define();
					}
					catch (Exception e)
					{
						throw new InternalErrorException(interfaceMemberBase, e);
					}
				}
			}
			for (int l = 0; l < count; l++)
			{
				InterfaceMemberBase interfaceMemberBase2 = members[l] as InterfaceMemberBase;
				if ((interfaceMemberBase2 == null || !interfaceMemberBase2.IsExplicitImpl) && !(members[l] is TypeContainer))
				{
					try
					{
						members[l].Define();
					}
					catch (Exception e2)
					{
						throw new InternalErrorException(members[l], e2);
					}
				}
			}
			if (HasOperators)
			{
				CheckPairedOperators();
			}
			if (requires_delayed_unmanagedtype_check)
			{
				requires_delayed_unmanagedtype_check = false;
				foreach (MemberCore member in members)
				{
					Field field = member as Field;
					if (field != null && field.MemberType != null && field.MemberType.IsPointer)
					{
						TypeManager.VerifyUnmanaged(Module, field.MemberType, field.Location);
					}
				}
			}
			ComputeIndexerName();
			if (HasEquals && !HasGetHashCode)
			{
				base.Report.Warning(659, 3, base.Location, "`{0}' overrides Object.Equals(object) but does not override Object.GetHashCode()", GetSignatureForError());
			}
			if (Kind == MemberKind.Interface && iface_exprs != null)
			{
				MemberCache.RemoveHiddenMembers(spec);
			}
			return true;
		}

		private void ComputeIndexerName()
		{
			IList<MemberSpec> list = MemberCache.FindMembers(spec, MemberCache.IndexerNameAlias, true);
			if (list == null)
			{
				return;
			}
			string text = null;
			foreach (MemberSpec item in list)
			{
				if (item.DeclaringType == spec)
				{
					has_normal_indexers = true;
					if (text == null)
					{
						text = (indexer_name = item.Name);
					}
					else if (item.Name != text)
					{
						base.Report.Error(668, ((Indexer)item.MemberDefinition).Location, "Two indexers have different names; the IndexerName attribute must be used with the same name on every indexer within a type");
					}
				}
			}
		}

		private void EmitIndexerName()
		{
			if (has_normal_indexers)
			{
				MethodSpec methodSpec = Module.PredefinedMembers.DefaultMemberAttributeCtor.Get();
				if (methodSpec != null)
				{
					AttributeEncoder attributeEncoder = new AttributeEncoder();
					attributeEncoder.Encode(GetAttributeDefaultMember());
					attributeEncoder.EncodeEmptyNamedArguments();
					TypeBuilder.SetCustomAttribute((ConstructorInfo)methodSpec.GetMetaInfo(), attributeEncoder.ToArray());
				}
			}
		}

		public override void VerifyMembers()
		{
			if (!base.IsCompilerGenerated && Compiler.Settings.WarningLevel >= 3 && this == base.PartialContainer)
			{
				bool flag = Kind == MemberKind.Struct || IsExposedFromAssembly();
				foreach (MemberCore member in members)
				{
					if (member is Event)
					{
						if (!member.IsUsed && !base.PartialContainer.HasStructLayout)
						{
							base.Report.Warning(67, 3, member.Location, "The event `{0}' is never used", member.GetSignatureForError());
						}
						continue;
					}
					if ((member.ModFlags & Modifiers.AccessibilityMask) != Modifiers.PRIVATE)
					{
						if (flag)
						{
							continue;
						}
						member.SetIsUsed();
					}
					Field field = member as Field;
					if (field == null)
					{
						continue;
					}
					if (!member.IsUsed)
					{
						if (!base.PartialContainer.HasStructLayout)
						{
							if ((member.caching_flags & Flags.IsAssigned) == 0)
							{
								base.Report.Warning(169, 3, member.Location, "The private field `{0}' is never used", member.GetSignatureForError());
							}
							else
							{
								base.Report.Warning(414, 3, member.Location, "The private field `{0}' is assigned but its value is never used", member.GetSignatureForError());
							}
						}
					}
					else if ((field.caching_flags & Flags.IsAssigned) == 0 && Compiler.Settings.WarningLevel >= 4 && field.OptAttributes == null && !base.PartialContainer.HasStructLayout)
					{
						Constant constant = New.Constantify(field.MemberType, field.Location);
						string text = ((constant != null) ? constant.GetValueAsLiteral() : ((!TypeSpec.IsReferenceType(field.MemberType)) ? null : "null"));
						if (text != null)
						{
							text = " `" + text + "'";
						}
						base.Report.Warning(649, 4, field.Location, "Field `{0}' is never assigned to, and will always have its default value{1}", field.GetSignatureForError(), text);
					}
				}
			}
			base.VerifyMembers();
		}

		public override void Emit()
		{
			if (base.OptAttributes != null)
			{
				base.OptAttributes.Emit();
			}
			if (!base.IsCompilerGenerated)
			{
				if (!IsTopLevel)
				{
					bool overrides = false;
					MemberSpec bestCandidate;
					MemberSpec memberSpec = MemberCache.FindBaseMember(this, out bestCandidate, ref overrides);
					if (memberSpec == null && bestCandidate == null)
					{
						if ((base.ModFlags & Modifiers.NEW) != 0)
						{
							base.Report.Warning(109, 4, base.Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError());
						}
					}
					else if ((base.ModFlags & Modifiers.NEW) == 0)
					{
						if (bestCandidate == null)
						{
							bestCandidate = memberSpec;
						}
						base.Report.SymbolRelatedToPreviousError(bestCandidate);
						base.Report.Warning(108, 2, base.Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended", GetSignatureForError(), bestCandidate.GetSignatureForError());
					}
				}
				if (base_type != null && base_type_expr != null)
				{
					ConstraintChecker.Check(this, base_type, base_type_expr.Location);
				}
				if (iface_exprs != null)
				{
					TypeSpec[] array = iface_exprs;
					foreach (TypeSpec typeSpec in array)
					{
						if (typeSpec != null)
						{
							ConstraintChecker.Check(this, typeSpec, base.Location);
						}
					}
				}
			}
			if (all_tp_builders != null)
			{
				int currentTypeParametersStartIndex = CurrentTypeParametersStartIndex;
				for (int j = 0; j < all_tp_builders.Length; j++)
				{
					if (j < currentTypeParametersStartIndex)
					{
						all_type_parameters[j].EmitConstraints(all_tp_builders[j]);
						continue;
					}
					TypeParameter typeParameter = CurrentTypeParameters[j - currentTypeParametersStartIndex];
					typeParameter.CheckGenericConstraints(!base.IsObsolete);
					typeParameter.Emit();
				}
			}
			if ((base.ModFlags & Modifiers.COMPILER_GENERATED) != 0 && !Parent.IsCompilerGenerated)
			{
				Module.PredefinedAttributes.CompilerGenerated.EmitAttribute(TypeBuilder);
			}
			base.Emit();
			for (int k = 0; k < members.Count; k++)
			{
				MemberCore memberCore = members[k];
				if ((memberCore.caching_flags & Flags.CloseTypeCreated) == 0)
				{
					memberCore.Emit();
				}
			}
			EmitIndexerName();
			CheckAttributeClsCompliance();
			if (pending != null)
			{
				pending.VerifyPendingMethods();
			}
		}

		private void CheckAttributeClsCompliance()
		{
			if (!spec.IsAttribute || !IsExposedFromAssembly() || !Compiler.Settings.VerifyClsCompliance || !IsClsComplianceRequired())
			{
				return;
			}
			foreach (MemberCore member in members)
			{
				Constructor constructor = member as Constructor;
				if (constructor != null && constructor.HasCompliantArgs)
				{
					return;
				}
			}
			base.Report.Warning(3015, 1, base.Location, "`{0}' has no accessible constructors which use only CLS-compliant types", GetSignatureForError());
		}

		public sealed override void EmitContainer()
		{
			if ((caching_flags & Flags.CloseTypeCreated) == 0)
			{
				Emit();
			}
		}

		public override void CloseContainer()
		{
			if ((caching_flags & Flags.CloseTypeCreated) != 0)
			{
				return;
			}
			if (spec.BaseType != null)
			{
				TypeContainer typeContainer = spec.BaseType.MemberDefinition as TypeContainer;
				if (typeContainer != null)
				{
					typeContainer.CloseContainer();
					if ((caching_flags & Flags.CloseTypeCreated) != 0)
					{
						return;
					}
				}
			}
			try
			{
				caching_flags |= Flags.CloseTypeCreated;
				TypeBuilder.CreateType();
			}
			catch (TypeLoadException)
			{
			}
			catch (Exception e)
			{
				throw new InternalErrorException(this, e);
			}
			base.CloseContainer();
			containers = null;
			initialized_fields = null;
			initialized_static_fields = null;
			type_bases = null;
			base.OptAttributes = null;
		}

		public bool MethodModifiersValid(MemberCore mc)
		{
			bool result = true;
			Modifiers modFlags = mc.ModFlags;
			if ((modFlags & Modifiers.STATIC) != 0 && (modFlags & (Modifiers.ABSTRACT | Modifiers.VIRTUAL | Modifiers.OVERRIDE)) != 0)
			{
				base.Report.Error(112, mc.Location, "A static member `{0}' cannot be marked as override, virtual or abstract", mc.GetSignatureForError());
				result = false;
			}
			if ((modFlags & Modifiers.OVERRIDE) != 0 && (modFlags & (Modifiers.NEW | Modifiers.VIRTUAL)) != 0)
			{
				base.Report.Error(113, mc.Location, "A member `{0}' marked as override cannot be marked as new or virtual", mc.GetSignatureForError());
				result = false;
			}
			if ((modFlags & Modifiers.ABSTRACT) != 0)
			{
				if ((modFlags & Modifiers.EXTERN) != 0)
				{
					base.Report.Error(180, mc.Location, "`{0}' cannot be both extern and abstract", mc.GetSignatureForError());
					result = false;
				}
				if ((modFlags & Modifiers.SEALED) != 0)
				{
					base.Report.Error(502, mc.Location, "`{0}' cannot be both abstract and sealed", mc.GetSignatureForError());
					result = false;
				}
				if ((modFlags & Modifiers.VIRTUAL) != 0)
				{
					base.Report.Error(503, mc.Location, "The abstract method `{0}' cannot be marked virtual", mc.GetSignatureForError());
					result = false;
				}
				if ((base.ModFlags & Modifiers.ABSTRACT) == 0)
				{
					base.Report.SymbolRelatedToPreviousError(this);
					base.Report.Error(513, mc.Location, "`{0}' is abstract but it is declared in the non-abstract class `{1}'", mc.GetSignatureForError(), GetSignatureForError());
					result = false;
				}
			}
			if ((modFlags & Modifiers.PRIVATE) != 0 && (modFlags & (Modifiers.ABSTRACT | Modifiers.VIRTUAL | Modifiers.OVERRIDE)) != 0)
			{
				base.Report.Error(621, mc.Location, "`{0}': virtual or abstract members cannot be private", mc.GetSignatureForError());
				result = false;
			}
			if ((modFlags & Modifiers.SEALED) != 0 && (modFlags & Modifiers.OVERRIDE) == 0)
			{
				base.Report.Error(238, mc.Location, "`{0}' cannot be sealed because it is not an override", mc.GetSignatureForError());
				result = false;
			}
			return result;
		}

		protected override bool VerifyClsCompliance()
		{
			if (!base.VerifyClsCompliance())
			{
				return false;
			}
			if (Kind != MemberKind.Delegate)
			{
				MemberCache.VerifyClsCompliance(Definition, base.Report);
			}
			if (BaseType != null && !BaseType.IsCLSCompliant())
			{
				base.Report.Warning(3009, 1, base.Location, "`{0}': base type `{1}' is not CLS-compliant", GetSignatureForError(), BaseType.GetSignatureForError());
			}
			return true;
		}

		public bool VerifyImplements(InterfaceMemberBase mb)
		{
			TypeSpec[] interfaces = base.PartialContainer.Interfaces;
			if (interfaces != null)
			{
				TypeSpec[] array = interfaces;
				foreach (TypeSpec typeSpec in array)
				{
					if (typeSpec == mb.InterfaceType)
					{
						return true;
					}
					IList<TypeSpec> interfaces2 = typeSpec.Interfaces;
					if (interfaces2 == null)
					{
						continue;
					}
					foreach (TypeSpec item in interfaces2)
					{
						if (item == mb.InterfaceType)
						{
							return true;
						}
					}
				}
			}
			base.Report.SymbolRelatedToPreviousError(mb.InterfaceType);
			base.Report.Error(540, mb.Location, "`{0}': containing type does not implement interface `{1}'", mb.GetSignatureForError(), mb.InterfaceType.GetSignatureForError());
			return false;
		}

		public bool IsBaseTypeDefinition(TypeSpec baseType)
		{
			if (TypeBuilder == null)
			{
				return false;
			}
			TypeSpec baseType2 = spec;
			do
			{
				if (baseType2.MemberDefinition == baseType.MemberDefinition)
				{
					return true;
				}
				baseType2 = baseType2.BaseType;
			}
			while (baseType2 != null);
			return false;
		}

		public override bool IsClsComplianceRequired()
		{
			if (IsPartialPart)
			{
				return base.PartialContainer.IsClsComplianceRequired();
			}
			return base.IsClsComplianceRequired();
		}

		bool ITypeDefinition.IsInternalAsPublic(IAssemblyDefinition assembly)
		{
			return Module.DeclaringAssembly == assembly;
		}

		public virtual bool IsUnmanagedType()
		{
			return false;
		}

		public void LoadMembers(TypeSpec declaringType, bool onlyTypes, ref MemberCache cache)
		{
			throw new NotSupportedException("Not supported for compiled definition " + GetSignatureForError());
		}

		public override FullNamedExpression LookupNamespaceOrType(string name, int arity, LookupMode mode, Location loc)
		{
			FullNamedExpression value;
			if (arity == 0 && Cache.TryGetValue(name, out value) && mode != LookupMode.IgnoreAccessibility)
			{
				return value;
			}
			value = null;
			if (arity == 0)
			{
				TypeParameters currentTypeParameters = CurrentTypeParameters;
				if (currentTypeParameters != null)
				{
					TypeParameter typeParameter = currentTypeParameters.Find(name);
					if (typeParameter != null)
					{
						value = new TypeParameterExpr(typeParameter, Location.Null);
					}
				}
			}
			if (value == null)
			{
				TypeSpec typeSpec = LookupNestedTypeInHierarchy(name, arity);
				if (typeSpec != null && (typeSpec.IsAccessible(this) || mode == LookupMode.IgnoreAccessibility))
				{
					value = new TypeExpression(typeSpec, Location.Null);
				}
				else
				{
					int errors = Compiler.Report.Errors;
					value = Parent.LookupNamespaceOrType(name, arity, mode, loc);
					if (errors != Compiler.Report.Errors)
					{
						return value;
					}
				}
			}
			if (arity == 0 && mode == LookupMode.Normal)
			{
				Cache[name] = value;
			}
			return value;
		}

		private TypeSpec LookupNestedTypeInHierarchy(string name, int arity)
		{
			return MemberCache.FindNestedType(base.PartialContainer.CurrentType, name, arity);
		}

		public void Mark_HasEquals()
		{
			cached_method |= CachedMethods.Equals;
		}

		public void Mark_HasGetHashCode()
		{
			cached_method |= CachedMethods.GetHashCode;
		}

		public override void WriteDebugSymbol(MonoSymbolFile file)
		{
			if (IsPartialPart)
			{
				return;
			}
			foreach (MemberCore member in members)
			{
				member.WriteDebugSymbol(file);
			}
		}
	}
}
