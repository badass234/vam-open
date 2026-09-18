using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public sealed class ModuleContainer : TypeContainer
	{
		public sealed class PatternMatchingHelper : CompilerGeneratedContainer
		{
			public Method NumberMatcher { get; private set; }

			public PatternMatchingHelper(ModuleContainer module)
				: base(module, new MemberName("<PatternMatchingHelper>", Location.Null), Modifiers.INTERNAL | Modifiers.STATIC | Modifiers.DEBUGGER_HIDDEN)
			{
			}

			protected override bool DoDefineMembers()
			{
				if (!base.DoDefineMembers())
				{
					return false;
				}
				NumberMatcher = GenerateNumberMatcher();
				return true;
			}

			private Method GenerateNumberMatcher()
			{
				Location location = base.Location;
				ParametersCompiled parametersCompiled = ParametersCompiled.CreateFullyResolved(new Parameter[3]
				{
					new Parameter(new TypeExpression(Compiler.BuiltinTypes.Object, location), "obj", Parameter.Modifier.NONE, null, location),
					new Parameter(new TypeExpression(Compiler.BuiltinTypes.Object, location), "value", Parameter.Modifier.NONE, null, location),
					new Parameter(new TypeExpression(Compiler.BuiltinTypes.Bool, location), "enumType", Parameter.Modifier.NONE, null, location)
				}, new BuiltinTypeSpec[3]
				{
					Compiler.BuiltinTypes.Object,
					Compiler.BuiltinTypes.Object,
					Compiler.BuiltinTypes.Bool
				});
				Method method = new Method(this, new TypeExpression(Compiler.BuiltinTypes.Bool, location), Modifiers.PUBLIC | Modifiers.STATIC | Modifiers.DEBUGGER_HIDDEN, new MemberName("NumberMatcher", location), parametersCompiled, null);
				parametersCompiled[0].Resolve(method, 0);
				parametersCompiled[1].Resolve(method, 1);
				parametersCompiled[2].Resolve(method, 2);
				ToplevelBlock toplevelBlock = (method.Block = new ToplevelBlock(Compiler, parametersCompiled, location));
				Arguments arguments = new Arguments(2);
				arguments.Add(new Argument(toplevelBlock.GetParameterReference(0, location)));
				arguments.Add(new Argument(toplevelBlock.GetParameterReference(1, location)));
				If s = new If(toplevelBlock.GetParameterReference(2, location), new Return(new Invocation(new SimpleName("Equals", location), arguments), location), location);
				toplevelBlock.AddStatement(s);
				If s2 = new If(new Binary(Binary.Operator.LogicalOr, new Is(toplevelBlock.GetParameterReference(0, location), new TypeExpression(Compiler.BuiltinTypes.Enum, location), location), new Binary(Binary.Operator.Equality, toplevelBlock.GetParameterReference(0, location), new NullLiteral(location))), new Return(new BoolLiteral(Compiler.BuiltinTypes, false, location), location), location);
				toplevelBlock.AddStatement(s2);
				MemberAccess expr = new MemberAccess(new QualifiedAliasMember("global", "System", location), "Convert", location);
				ExplicitBlock explicitBlock = new ExplicitBlock(toplevelBlock, location, location);
				LocalVariable li = LocalVariable.CreateCompilerGenerated(Compiler.BuiltinTypes.Object, toplevelBlock, location);
				Arguments arguments2 = new Arguments(1);
				arguments2.Add(new Argument(toplevelBlock.GetParameterReference(1, location)));
				Invocation expr2 = new Invocation(new MemberAccess(expr, "GetTypeCode", location), arguments2);
				Arguments arguments3 = new Arguments(1);
				arguments3.Add(new Argument(toplevelBlock.GetParameterReference(0, location)));
				arguments3.Add(new Argument(expr2));
				Invocation source = new Invocation(new MemberAccess(expr, "ChangeType", location), arguments3);
				explicitBlock.AddStatement(new StatementExpression(new SimpleAssign(new LocalVariableReference(li, location), source, location)));
				Arguments arguments4 = new Arguments(1);
				arguments4.Add(new Argument(toplevelBlock.GetParameterReference(1, location)));
				Invocation expr3 = new Invocation(new MemberAccess(new LocalVariableReference(li, location), "Equals"), arguments4);
				explicitBlock.AddStatement(new Return(expr3, location));
				ExplicitBlock explicitBlock2 = new ExplicitBlock(toplevelBlock, location, location);
				explicitBlock2.AddStatement(new Return(new BoolLiteral(Compiler.BuiltinTypes, false, location), location));
				toplevelBlock.AddStatement(new TryCatch(explicitBlock, new List<Catch>
				{
					new Catch(explicitBlock2, location)
				}, location, false));
				method.Define();
				method.PrepareEmit();
				AddMember(method);
				return method;
			}
		}

		private PatternMatchingHelper pmh;

		public CharSet? DefaultCharSet;

		public TypeAttributes DefaultCharSetType;

		private readonly Dictionary<int, List<AnonymousTypeClass>> anonymous_types;

		private readonly Dictionary<ArrayContainer.TypeRankPair, ArrayContainer> array_types;

		private readonly Dictionary<TypeSpec, PointerContainer> pointer_types;

		private readonly Dictionary<TypeSpec, ReferenceContainer> reference_types;

		private readonly Dictionary<TypeSpec, MethodSpec> attrs_cache;

		private readonly Dictionary<TypeSpec, AwaiterDefinition> awaiters;

		private readonly Dictionary<TypeSpec, TypeInfo> type_info_cache;

		private AssemblyDefinition assembly;

		private readonly CompilerContext context;

		private readonly RootNamespace global_ns;

		private readonly Dictionary<string, RootNamespace> alias_ns;

		private ModuleBuilder builder;

		private bool has_extenstion_method;

		private PredefinedAttributes predefined_attributes;

		private PredefinedTypes predefined_types;

		private PredefinedMembers predefined_members;

		public Binary.PredefinedOperator[] OperatorsBinaryEqualityLifted;

		public Binary.PredefinedOperator[] OperatorsBinaryLifted;

		private static readonly string[] attribute_targets = new string[2] { "assembly", "module" };

		public Dictionary<ArrayContainer.TypeRankPair, ArrayContainer> ArrayTypesCache => array_types;

		public Dictionary<TypeSpec, MethodSpec> AttributeConstructorCache => attrs_cache;

		public override AttributeTargets AttributeTargets => AttributeTargets.Assembly;

		public ModuleBuilder Builder => builder;

		public override CompilerContext Compiler => context;

		public int CounterAnonymousTypes { get; set; }

		public AssemblyDefinition DeclaringAssembly => assembly;

		public DocumentationBuilder DocumentationBuilder { get; set; }

		public override string DocCommentHeader
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public Evaluator Evaluator { get; set; }

		public bool HasDefaultCharSet => DefaultCharSet.HasValue;

		public bool HasExtensionMethod
		{
			get
			{
				return has_extenstion_method;
			}
			set
			{
				has_extenstion_method = value;
			}
		}

		public bool HasTypesFullyDefined { get; set; }

		public RootNamespace GlobalRootNamespace => global_ns;

		public override ModuleContainer Module => this;

		public Dictionary<TypeSpec, PointerContainer> PointerTypesCache => pointer_types;

		public PredefinedAttributes PredefinedAttributes => predefined_attributes;

		public PredefinedMembers PredefinedMembers => predefined_members;

		public PredefinedTypes PredefinedTypes => predefined_types;

		public Dictionary<TypeSpec, ReferenceContainer> ReferenceTypesCache => reference_types;

		public Dictionary<TypeSpec, TypeInfo> TypeInfoCache => type_info_cache;

		public override string[] ValidAttributeTargets => attribute_targets;

		public Dictionary<string, string> GetResourceStrings { get; private set; }

		public PatternMatchingHelper CreatePatterMatchingHelper()
		{
			if (pmh == null)
			{
				pmh = new PatternMatchingHelper(this);
				pmh.CreateContainer();
				pmh.DefineContainer();
				pmh.Define();
				AddCompilerGeneratedClass(pmh);
			}
			return pmh;
		}

		public ModuleContainer(CompilerContext context)
			: base(null, MemberName.Null, null, (MemberKind)0)
		{
			this.context = context;
			caching_flags &= ~(Flags.Obsolete_Undetected | Flags.Excluded_Undetected);
			containers = new List<TypeContainer>();
			anonymous_types = new Dictionary<int, List<AnonymousTypeClass>>();
			global_ns = new GlobalRootNamespace();
			alias_ns = new Dictionary<string, RootNamespace>();
			array_types = new Dictionary<ArrayContainer.TypeRankPair, ArrayContainer>();
			pointer_types = new Dictionary<TypeSpec, PointerContainer>();
			reference_types = new Dictionary<TypeSpec, ReferenceContainer>();
			attrs_cache = new Dictionary<TypeSpec, MethodSpec>();
			awaiters = new Dictionary<TypeSpec, AwaiterDefinition>();
			type_info_cache = new Dictionary<TypeSpec, TypeInfo>();
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public void AddAnonymousType(AnonymousTypeClass type)
		{
			List<AnonymousTypeClass> value;
			if (!anonymous_types.TryGetValue(type.Parameters.Count, out value) && value == null)
			{
				value = new List<AnonymousTypeClass>();
				anonymous_types.Add(type.Parameters.Count, value);
			}
			value.Add(type);
		}

		public void AddAttribute(Attribute attr, IMemberContext context)
		{
			attr.AttachTo(this, context);
			if (attributes == null)
			{
				attributes = new Attributes(attr);
			}
			else
			{
				attributes.AddAttribute(attr);
			}
		}

		public override void AddTypeContainer(TypeContainer tc)
		{
			AddTypeContainerMember(tc);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.Assembly)
			{
				assembly.ApplyAttributeBuilder(a, ctor, cdata, pa);
				return;
			}
			if (a.Type == pa.DefaultCharset)
			{
				switch (a.GetCharSetValue())
				{
				case CharSet.Auto:
					DefaultCharSet = CharSet.Auto;
					DefaultCharSetType = TypeAttributes.AutoClass;
					break;
				case CharSet.Unicode:
					DefaultCharSet = CharSet.Unicode;
					DefaultCharSetType = TypeAttributes.UnicodeClass;
					break;
				default:
					base.Report.Error(1724, a.Location, "Value specified for the argument to `{0}' is not valid", a.GetSignatureForError());
					break;
				case CharSet.None:
				case CharSet.Ansi:
					break;
				}
			}
			else if (a.Type == pa.CLSCompliant)
			{
				Attribute cLSCompliantAttribute = DeclaringAssembly.CLSCompliantAttribute;
				if (cLSCompliantAttribute == null)
				{
					base.Report.Warning(3012, 1, a.Location, "You must specify the CLSCompliant attribute on the assembly, not the module, to enable CLS compliance checking");
				}
				else if (DeclaringAssembly.IsCLSCompliant != a.GetBoolean())
				{
					base.Report.SymbolRelatedToPreviousError(cLSCompliantAttribute.Location, cLSCompliantAttribute.GetSignatureForError());
					base.Report.Warning(3017, 1, a.Location, "You cannot specify the CLSCompliant attribute on a module that differs from the CLSCompliant attribute on the assembly");
					return;
				}
			}
			builder.SetCustomAttribute((ConstructorInfo)ctor.GetMetaInfo(), cdata);
		}

		public override void CloseContainer()
		{
			if (anonymous_types != null)
			{
				foreach (KeyValuePair<int, List<AnonymousTypeClass>> anonymous_type in anonymous_types)
				{
					foreach (AnonymousTypeClass item in anonymous_type.Value)
					{
						item.CloseContainer();
					}
				}
			}
			base.CloseContainer();
		}

		public TypeBuilder CreateBuilder(string name, TypeAttributes attr, int typeSize)
		{
			return builder.DefineType(name, attr, null, typeSize);
		}

		public RootNamespace CreateRootNamespace(string alias)
		{
			if (alias == global_ns.Alias)
			{
				RootNamespace.Error_GlobalNamespaceRedefined(base.Report, Location.Null);
				return global_ns;
			}
			RootNamespace value;
			if (!alias_ns.TryGetValue(alias, out value))
			{
				value = new RootNamespace(alias);
				alias_ns.Add(alias, value);
			}
			return value;
		}

		public void Create(AssemblyDefinition assembly, ModuleBuilder moduleBuilder)
		{
			this.assembly = assembly;
			builder = moduleBuilder;
		}

		public override bool Define()
		{
			DefineContainer();
			ExpandBaseInterfaces();
			base.Define();
			HasTypesFullyDefined = true;
			return true;
		}

		public override bool DefineContainer()
		{
			DefineNamespace();
			return base.DefineContainer();
		}

		public void EnableRedefinition()
		{
			is_defined = false;
		}

		public override void EmitContainer()
		{
			if (base.OptAttributes != null)
			{
				base.OptAttributes.Emit();
			}
			if (Compiler.Settings.Unsafe && !assembly.IsSatelliteAssembly)
			{
				PredefinedAttribute unverifiableCode = PredefinedAttributes.UnverifiableCode;
				if (unverifiableCode.IsDefined)
				{
					unverifiableCode.EmitAttribute(builder);
				}
			}
			foreach (TypeContainer container in containers)
			{
				container.PrepareEmit();
			}
			base.EmitContainer();
			if (Compiler.Report.Errors == 0 && !Compiler.Settings.WriteMetadataOnly)
			{
				VerifyMembers();
			}
			if (anonymous_types == null)
			{
				return;
			}
			foreach (KeyValuePair<int, List<AnonymousTypeClass>> anonymous_type in anonymous_types)
			{
				foreach (AnonymousTypeClass item in anonymous_type.Value)
				{
					item.EmitContainer();
				}
			}
		}

		public override void GenerateDocComment(DocumentationBuilder builder)
		{
			foreach (TypeContainer container in containers)
			{
				container.GenerateDocComment(builder);
			}
		}

		public AnonymousTypeClass GetAnonymousType(IList<AnonymousTypeParameter> parameters)
		{
			List<AnonymousTypeClass> value;
			if (!anonymous_types.TryGetValue(parameters.Count, out value))
			{
				return null;
			}
			foreach (AnonymousTypeClass item in value)
			{
				int i;
				for (i = 0; i < parameters.Count && parameters[i].Equals(item.Parameters[i]); i++)
				{
				}
				if (i == parameters.Count)
				{
					return item;
				}
			}
			return null;
		}

		public AwaiterDefinition GetAwaiter(TypeSpec type)
		{
			AwaiterDefinition value;
			if (awaiters.TryGetValue(type, out value))
			{
				return value;
			}
			value = new AwaiterDefinition();
			value.IsCompleted = MemberCache.FindMember(type, MemberFilter.Property("IsCompleted", Compiler.BuiltinTypes.Bool), BindingRestriction.InstanceOnly) as PropertySpec;
			value.GetResult = MemberCache.FindMember(type, MemberFilter.Method("GetResult", 0, ParametersCompiled.EmptyReadOnlyParameters, null), BindingRestriction.InstanceOnly) as MethodSpec;
			PredefinedType iNotifyCompletion = PredefinedTypes.INotifyCompletion;
			value.INotifyCompletion = !iNotifyCompletion.Define() || type.ImplementsInterface(iNotifyCompletion.TypeSpec, false);
			awaiters.Add(type, value);
			return value;
		}

		public override void GetCompletionStartingWith(string prefix, List<string> results)
		{
			string[] varNames = Evaluator.GetVarNames();
			results.AddRange(varNames.Where((string l) => l.StartsWith(prefix)));
		}

		public RootNamespace GetRootNamespace(string name)
		{
			RootNamespace value;
			alias_ns.TryGetValue(name, out value);
			return value;
		}

		public override string GetSignatureForError()
		{
			return "<module>";
		}

		public Binary.PredefinedOperator[] GetPredefinedEnumAritmeticOperators(TypeSpec enumType, bool nullable)
		{
			Binary.Operator obj = (Binary.Operator)0;
			TypeSpec typeSpec;
			if (nullable)
			{
				typeSpec = NullableInfo.GetEnumUnderlyingType(this, enumType);
				obj = Binary.Operator.NullableMask;
			}
			else
			{
				typeSpec = EnumSpec.GetUnderlyingType(enumType);
			}
			return new Binary.PredefinedOperator[3]
			{
				new Binary.PredefinedOperator(enumType, typeSpec, obj | Binary.Operator.AdditionMask | Binary.Operator.SubtractionMask | Binary.Operator.DecomposedMask, enumType),
				new Binary.PredefinedOperator(typeSpec, enumType, obj | Binary.Operator.AdditionMask | Binary.Operator.SubtractionMask | Binary.Operator.DecomposedMask, enumType),
				new Binary.PredefinedOperator(enumType, obj | Binary.Operator.SubtractionMask, typeSpec)
			};
		}

		public void InitializePredefinedTypes()
		{
			predefined_attributes = new PredefinedAttributes(this);
			predefined_types = new PredefinedTypes(this);
			predefined_members = new PredefinedMembers(this);
			OperatorsBinaryEqualityLifted = Binary.CreateEqualityLiftedOperatorsTable(this);
			OperatorsBinaryLifted = Binary.CreateStandardLiftedOperatorsTable(this);
		}

		public override bool IsClsComplianceRequired()
		{
			return DeclaringAssembly.IsCLSCompliant;
		}

		public Attribute ResolveAssemblyAttribute(PredefinedAttribute a_type)
		{
			Attribute attribute = base.OptAttributes.Search("assembly", a_type);
			attribute?.Resolve();
			return attribute;
		}

		public void SetDeclaringAssembly(AssemblyDefinition assembly)
		{
			this.assembly = assembly;
		}

		public void LoadGetResourceStrings(List<string> fileNames)
		{
			foreach (string fileName in fileNames)
			{
				if (!File.Exists(fileName))
				{
					base.Report.Error(1566, "Error reading resource file `{0}'", fileName);
					break;
				}
				string[] array = File.ReadAllLines(fileName);
				foreach (string obj in array)
				{
					if (GetResourceStrings == null)
					{
						GetResourceStrings = new Dictionary<string, string>();
					}
					string text = obj.Trim();
					if (text.Length != 0 && text[0] != '#' && text[0] != ';')
					{
						int num = text.IndexOf('=');
						if (num >= 0)
						{
							string key = text.Substring(0, num).Trim();
							string value = text.Substring(num + 1).Trim();
							GetResourceStrings[key] = value;
						}
					}
				}
			}
		}
	}
}
