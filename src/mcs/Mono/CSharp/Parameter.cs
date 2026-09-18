using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class Parameter : ParameterBase, IParameterData, ILocalVariable
	{
		[Flags]
		public enum Modifier : byte
		{
			NONE = 0,
			PARAMS = 1,
			REF = 2,
			OUT = 4,
			This = 8,
			CallerMemberName = 0x10,
			CallerLineNumber = 0x20,
			CallerFilePath = 0x40,
			RefOutMask = REF | OUT,
			ModifierMask = RefOutMask | PARAMS | This,
			CallerMask = CallerMemberName | CallerLineNumber | CallerFilePath
		}

		private static readonly string[] attribute_targets = new string[1] { "param" };

		private FullNamedExpression texpr;

		private Modifier modFlags;

		private string name;

		private Expression default_expr;

		protected TypeSpec parameter_type;

		private readonly Location loc;

		protected int idx;

		public bool HasAddressTaken;

		private TemporaryVariableReference expr_tree_variable;

		private HoistedParameter hoisted_variant;

		public Expression DefaultExpression => default_expr;

		public DefaultParameterValueExpression DefaultValue
		{
			get
			{
				return default_expr as DefaultParameterValueExpression;
			}
			set
			{
				default_expr = value;
			}
		}

		Expression IParameterData.DefaultValue
		{
			get
			{
				DefaultParameterValueExpression defaultParameterValueExpression = default_expr as DefaultParameterValueExpression;
				if (defaultParameterValueExpression != null)
				{
					return defaultParameterValueExpression.Child;
				}
				return default_expr;
			}
		}

		private bool HasOptionalExpression => default_expr is DefaultParameterValueExpression;

		public Location Location => loc;

		public Modifier ParameterModifier => modFlags;

		public TypeSpec Type
		{
			get
			{
				return parameter_type;
			}
			set
			{
				parameter_type = value;
			}
		}

		public FullNamedExpression TypeExpression => texpr;

		public override string[] ValidAttributeTargets => attribute_targets;

		public bool HasDefaultValue => default_expr != null;

		public bool HasExtensionMethodModifier => (modFlags & Modifier.This) != 0;

		public HoistedParameter HoistedVariant
		{
			get
			{
				return hoisted_variant;
			}
			set
			{
				hoisted_variant = value;
			}
		}

		public Modifier ModFlags => modFlags & ~Modifier.This;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public override AttributeTargets AttributeTargets => AttributeTargets.Parameter;

		public Parameter(FullNamedExpression type, string name, Modifier mod, Attributes attrs, Location loc)
		{
			this.name = name;
			modFlags = mod;
			this.loc = loc;
			texpr = type;
			attributes = attrs;
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.In && ModFlags == Modifier.OUT)
			{
				a.Report.Error(36, a.Location, "An out parameter cannot have the `In' attribute");
				return;
			}
			if (a.Type == pa.ParamArray)
			{
				a.Report.Error(674, a.Location, "Do not use `System.ParamArrayAttribute'. Use the `params' keyword instead");
				return;
			}
			if (a.Type == pa.Out && (ModFlags & Modifier.REF) != Modifier.NONE && !base.OptAttributes.Contains(pa.In))
			{
				a.Report.Error(662, a.Location, "Cannot specify only `Out' attribute on a ref parameter. Use both `In' and `Out' attributes or neither");
				return;
			}
			if (a.Type == pa.CLSCompliant)
			{
				a.Report.Warning(3022, 1, a.Location, "CLSCompliant attribute has no meaning when applied to parameters. Try putting it on the method instead");
			}
			else if (a.Type == pa.DefaultParameterValue || a.Type == pa.OptionalParameter)
			{
				if (HasOptionalExpression)
				{
					a.Report.Error(1745, a.Location, "Cannot specify `{0}' attribute on optional parameter `{1}'", a.Type.GetSignatureForError().Replace("Attribute", ""), Name);
				}
				if (a.Type == pa.DefaultParameterValue)
				{
					return;
				}
			}
			else if (a.Type == pa.CallerMemberNameAttribute)
			{
				if ((modFlags & Modifier.CallerMemberName) == 0)
				{
					a.Report.Error(4022, a.Location, "The CallerMemberName attribute can only be applied to parameters with default value");
				}
			}
			else if (a.Type == pa.CallerLineNumberAttribute)
			{
				if ((modFlags & Modifier.CallerLineNumber) == 0)
				{
					a.Report.Error(4020, a.Location, "The CallerLineNumber attribute can only be applied to parameters with default value");
				}
			}
			else if (a.Type == pa.CallerFilePathAttribute && (modFlags & Modifier.CallerFilePath) == 0)
			{
				a.Report.Error(4021, a.Location, "The CallerFilePath attribute can only be applied to parameters with default value");
			}
			base.ApplyAttributeBuilder(a, ctor, cdata, pa);
		}

		public virtual bool CheckAccessibility(InterfaceMemberBase member)
		{
			if (parameter_type == null)
			{
				return true;
			}
			return member.IsAccessibleAs(parameter_type);
		}

		private bool IsValidCallerContext(MemberCore memberContext)
		{
			Method method = memberContext as Method;
			if (method != null)
			{
				return !method.IsPartialImplementation;
			}
			return true;
		}

		public virtual TypeSpec Resolve(IMemberContext rc, int index)
		{
			if (parameter_type != null)
			{
				return parameter_type;
			}
			if (attributes != null)
			{
				attributes.AttachTo(this, rc);
			}
			parameter_type = texpr.ResolveAsType(rc);
			if (parameter_type == null)
			{
				return null;
			}
			idx = index;
			if ((modFlags & Modifier.RefOutMask) != Modifier.NONE && parameter_type.IsSpecialRuntimeType)
			{
				rc.Module.Compiler.Report.Error(1601, Location, "Method or delegate parameter cannot be of type `{0}'", GetSignatureForError());
				return null;
			}
			VarianceDecl.CheckTypeVariance(parameter_type, ((modFlags & Modifier.RefOutMask) == 0) ? Variance.Contravariant : Variance.None, rc);
			if (parameter_type.IsStatic)
			{
				rc.Module.Compiler.Report.Error(721, Location, "`{0}': static types cannot be used as parameters", texpr.GetSignatureForError());
				return parameter_type;
			}
			if ((modFlags & Modifier.This) != Modifier.NONE && (parameter_type.IsPointer || parameter_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic))
			{
				rc.Module.Compiler.Report.Error(1103, Location, "The extension method cannot be of type `{0}'", parameter_type.GetSignatureForError());
			}
			return parameter_type;
		}

		private void ResolveCallerAttributes(ResolveContext rc)
		{
			PredefinedAttributes predefinedAttributes = rc.Module.PredefinedAttributes;
			Attribute attribute = null;
			Attribute attribute2 = null;
			foreach (Attribute attr in attributes.Attrs)
			{
				TypeSpec typeSpec = attr.ResolveTypeForComparison();
				if (typeSpec == null)
				{
					continue;
				}
				if (typeSpec == predefinedAttributes.CallerMemberNameAttribute)
				{
					TypeSpec typeSpec2 = rc.BuiltinTypes.String;
					if (typeSpec2 != parameter_type && !Convert.ImplicitReferenceConversionExists(typeSpec2, parameter_type))
					{
						rc.Report.Error(4019, attr.Location, "The CallerMemberName attribute cannot be applied because there is no standard conversion from `{0}' to `{1}'", typeSpec2.GetSignatureForError(), parameter_type.GetSignatureForError());
					}
					if (!IsValidCallerContext(rc.CurrentMemberDefinition))
					{
						rc.Report.Warning(4026, 1, attr.Location, "The CallerMemberName applied to parameter `{0}' will have no effect because it applies to a member that is used in context that do not allow optional arguments", name);
					}
					modFlags |= Modifier.CallerMemberName;
					attribute = attr;
				}
				else if (typeSpec == predefinedAttributes.CallerLineNumberAttribute)
				{
					TypeSpec typeSpec2 = rc.BuiltinTypes.Int;
					if (typeSpec2 != parameter_type && !Convert.ImplicitStandardConversionExists(new IntConstant(typeSpec2, int.MaxValue, Location.Null), parameter_type))
					{
						rc.Report.Error(4017, attr.Location, "The CallerLineNumberAttribute attribute cannot be applied because there is no standard conversion from `{0}' to `{1}'", typeSpec2.GetSignatureForError(), parameter_type.GetSignatureForError());
					}
					if (!IsValidCallerContext(rc.CurrentMemberDefinition))
					{
						rc.Report.Warning(4024, 1, attr.Location, "The CallerLineNumberAttribute applied to parameter `{0}' will have no effect because it applies to a member that is used in context that do not allow optional arguments", name);
					}
					modFlags |= Modifier.CallerLineNumber;
				}
				else if (typeSpec == predefinedAttributes.CallerFilePathAttribute)
				{
					TypeSpec typeSpec2 = rc.BuiltinTypes.String;
					if (typeSpec2 != parameter_type && !Convert.ImplicitReferenceConversionExists(typeSpec2, parameter_type))
					{
						rc.Report.Error(4018, attr.Location, "The CallerFilePath attribute cannot be applied because there is no standard conversion from `{0}' to `{1}'", typeSpec2.GetSignatureForError(), parameter_type.GetSignatureForError());
					}
					if (!IsValidCallerContext(rc.CurrentMemberDefinition))
					{
						rc.Report.Warning(4025, 1, attr.Location, "The CallerFilePath applied to parameter `{0}' will have no effect because it applies to a member that is used in context that do not allow optional arguments", name);
					}
					modFlags |= Modifier.CallerFilePath;
					attribute2 = attr;
				}
			}
			if ((modFlags & Modifier.CallerLineNumber) != Modifier.NONE)
			{
				if (attribute != null)
				{
					rc.Report.Warning(7081, 1, attribute.Location, "The CallerMemberNameAttribute applied to parameter `{0}' will have no effect. It is overridden by the CallerLineNumberAttribute", Name);
				}
				if (attribute2 != null)
				{
					rc.Report.Warning(7082, 1, attribute2.Location, "The CallerFilePathAttribute applied to parameter `{0}' will have no effect. It is overridden by the CallerLineNumberAttribute", name);
				}
			}
			if ((modFlags & Modifier.CallerMemberName) != Modifier.NONE && attribute2 != null)
			{
				rc.Report.Warning(7080, 1, attribute2.Location, "The CallerMemberNameAttribute applied to parameter `{0}' will have no effect. It is overridden by the CallerFilePathAttribute", name);
			}
		}

		public void ResolveDefaultValue(ResolveContext rc)
		{
			if (default_expr != null)
			{
				((DefaultParameterValueExpression)default_expr).Resolve(rc, this);
				if (attributes != null)
				{
					ResolveCallerAttributes(rc);
				}
			}
			else
			{
				if (attributes == null)
				{
					return;
				}
				PredefinedAttributes predefinedAttributes = rc.Module.PredefinedAttributes;
				Attribute attribute = attributes.Search(predefinedAttributes.DefaultParameterValue);
				if (attribute != null)
				{
					if (attribute.Resolve() == null)
					{
						return;
					}
					Expression parameterDefaultValue = attribute.GetParameterDefaultValue();
					if (parameterDefaultValue == null)
					{
						return;
					}
					ResolveContext resolveContext = attribute.CreateResolveContext();
					default_expr = parameterDefaultValue.Resolve(resolveContext);
					if (default_expr is BoxedCast)
					{
						default_expr = ((BoxedCast)default_expr).Child;
					}
					if (!(default_expr is Constant))
					{
						if (parameter_type.BuiltinType == BuiltinTypeSpec.Type.Object)
						{
							rc.Report.Error(1910, default_expr.Location, "Argument of type `{0}' is not applicable for the DefaultParameterValue attribute", default_expr.Type.GetSignatureForError());
						}
						else
						{
							rc.Report.Error(1909, default_expr.Location, "The DefaultParameterValue attribute is not applicable on parameters of type `{0}'", default_expr.Type.GetSignatureForError());
						}
						default_expr = null;
					}
					else if (!TypeSpecComparer.IsEqual(default_expr.Type, parameter_type) && (!(default_expr is NullConstant) || !TypeSpec.IsReferenceType(parameter_type) || parameter_type.IsGenericParameter) && parameter_type.BuiltinType != BuiltinTypeSpec.Type.Object && parameter_type.BuiltinType != BuiltinTypeSpec.Type.Dynamic)
					{
						Expression expression = Convert.ImplicitUserConversion(resolveContext, default_expr, parameter_type, loc);
						if (expression == null || !TypeSpecComparer.IsEqual(expression.Type, parameter_type))
						{
							rc.Report.Error(1908, default_expr.Location, "The type of the default value should match the type of the parameter");
						}
					}
				}
				else if (attributes.Search(predefinedAttributes.OptionalParameter) != null)
				{
					default_expr = EmptyExpression.MissingValue;
				}
			}
		}

		public void Error_DuplicateName(Report r)
		{
			r.Error(100, Location, "The parameter name `{0}' is a duplicate", Name);
		}

		public virtual string GetSignatureForError()
		{
			string text = ((parameter_type == null) ? texpr.GetSignatureForError() : parameter_type.GetSignatureForError());
			string modifierSignature = GetModifierSignature(modFlags);
			if (modifierSignature.Length > 0)
			{
				return modifierSignature + " " + text;
			}
			return text;
		}

		public static string GetModifierSignature(Modifier mod)
		{
			switch (mod)
			{
			case Modifier.OUT:
				return "out";
			case Modifier.PARAMS:
				return "params";
			case Modifier.REF:
				return "ref";
			case Modifier.This:
				return "this";
			default:
				return "";
			}
		}

		public void IsClsCompliant(IMemberContext ctx)
		{
			if (!parameter_type.IsCLSCompliant())
			{
				ctx.Module.Compiler.Report.Warning(3001, 1, Location, "Argument type `{0}' is not CLS-compliant", parameter_type.GetSignatureForError());
			}
		}

		public virtual void ApplyAttributes(MethodBuilder mb, ConstructorBuilder cb, int index, PredefinedAttributes pa)
		{
			if (builder != null)
			{
				throw new InternalErrorException("builder already exists");
			}
			ParameterAttributes parameterAttributes = AParametersCollection.GetParameterAttribute(modFlags);
			if (HasOptionalExpression)
			{
				parameterAttributes |= ParameterAttributes.Optional;
			}
			if (mb == null)
			{
				builder = cb.DefineParameter(index, parameterAttributes, Name);
			}
			else
			{
				builder = mb.DefineParameter(index, parameterAttributes, Name);
			}
			if (base.OptAttributes != null)
			{
				base.OptAttributes.Emit();
			}
			if (HasDefaultValue && default_expr.Type != null)
			{
				DefaultParameterValueExpression defaultValue = DefaultValue;
				Constant constant = ((defaultValue != null) ? (defaultValue.Child as Constant) : (default_expr as Constant));
				if (constant != null)
				{
					if (constant.Type.BuiltinType == BuiltinTypeSpec.Type.Decimal)
					{
						pa.DecimalConstant.EmitAttribute(builder, (decimal)constant.GetValue(), constant.Location);
					}
					else
					{
						SetConstant(builder, constant.GetValue());
					}
				}
				else if (default_expr.Type.IsStruct || default_expr.Type.IsGenericParameter)
				{
					SetConstant(builder, null);
				}
			}
			if (parameter_type != null)
			{
				if (parameter_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					pa.Dynamic.EmitAttribute(builder);
				}
				else if (parameter_type.HasDynamicElement)
				{
					pa.Dynamic.EmitAttribute(builder, parameter_type, Location);
				}
			}
		}

		// A default value the run-time cannot describe as a constant must not fail the whole
		// compilation. mono's mscorlib up to 2.0 stored a Nullable<T> default silently; the 4.x
		// one throws ArgumentException instead, which used to abort the compile of every plugin
		// that has an optional Nullable<T> parameter.
		private static void SetConstant(ParameterBuilder builder, object value)
		{
			try
			{
				builder.SetConstant(value);
			}
			catch (ArgumentException)
			{
			}
			catch (NotSupportedException)
			{
			}
		}

		public Parameter Clone()
		{
			Parameter parameter = (Parameter)MemberwiseClone();
			if (attributes != null)
			{
				parameter.attributes = attributes.Clone();
			}
			return parameter;
		}

		public ExpressionStatement CreateExpressionTreeVariable(BlockContext ec)
		{
			if ((modFlags & Modifier.RefOutMask) != Modifier.NONE)
			{
				ec.Report.Error(1951, Location, "An expression tree parameter cannot use `ref' or `out' modifier");
			}
			expr_tree_variable = TemporaryVariableReference.Create(ResolveParameterExpressionType(ec, Location).Type, ec.CurrentBlock.ParametersBlock, Location);
			expr_tree_variable = (TemporaryVariableReference)expr_tree_variable.Resolve(ec);
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(new TypeOf(parameter_type, Location)));
			arguments.Add(new Argument(new StringConstant(ec.BuiltinTypes, Name, Location)));
			return new SimpleAssign(ExpressionTreeVariableReference(), Expression.CreateExpressionFactoryCall(ec, "Parameter", null, arguments, Location));
		}

		public void Emit(EmitContext ec)
		{
			ec.EmitArgumentLoad(idx);
		}

		public void EmitAssign(EmitContext ec)
		{
			ec.EmitArgumentStore(idx);
		}

		public void EmitAddressOf(EmitContext ec)
		{
			if ((ModFlags & Modifier.RefOutMask) != Modifier.NONE)
			{
				ec.EmitArgumentLoad(idx);
			}
			else
			{
				ec.EmitArgumentAddress(idx);
			}
		}

		public TemporaryVariableReference ExpressionTreeVariableReference()
		{
			return expr_tree_variable;
		}

		public static TypeExpr ResolveParameterExpressionType(IMemberContext ec, Location location)
		{
			return new TypeExpression(ec.Module.PredefinedTypes.ParameterExpression.Resolve(), location);
		}

		public void SetIndex(int index)
		{
			idx = index;
		}

		public void Warning_UselessOptionalParameter(Report Report)
		{
			Report.Warning(1066, 1, Location, "The default value specified for optional parameter `{0}' will never be used", Name);
		}
	}
}
