using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Mono.CSharp
{
	public class Attribute
	{
		public readonly string ExplicitTarget;

		public AttributeTargets Target;

		private readonly ATypeNameExpression expression;

		private Arguments pos_args;

		private Arguments named_args;

		private bool resolve_error;

		private bool arg_resolved;

		private readonly bool nameEscaped;

		private readonly Location loc;

		public TypeSpec Type;

		private Attributable[] targets;

		private IMemberContext context;

		public static readonly AttributeUsageAttribute DefaultUsageAttribute = new AttributeUsageAttribute(AttributeTargets.All);

		public static readonly object[] EmptyObject = new object[0];

		private List<KeyValuePair<MemberExpr, NamedArgument>> named_values;

		public Location Location => loc;

		public Arguments NamedArguments => named_args;

		public Arguments PositionalArguments => pos_args;

		public bool ResolveError => resolve_error;

		public ATypeNameExpression TypeExpression => expression;

		private Attributable Owner => targets[0];

		public bool HasSecurityAttribute
		{
			get
			{
				PredefinedAttribute security = context.Module.PredefinedAttributes.Security;
				if (security.IsDefined)
				{
					return TypeSpec.IsBaseClass(Type, security.TypeSpec, false);
				}
				return false;
			}
		}

		public string Name => expression.Name;

		public Report Report => context.Module.Compiler.Report;

		public Attribute(string target, ATypeNameExpression expr, Arguments[] args, Location loc, bool nameEscaped)
		{
			expression = expr;
			if (args != null)
			{
				pos_args = args[0];
				named_args = args[1];
			}
			this.loc = loc;
			ExplicitTarget = target;
			this.nameEscaped = nameEscaped;
		}

		private void AddModuleCharSet(ResolveContext rc)
		{
			if (!HasField("CharSet") && rc.Module.PredefinedTypes.CharSet.Define())
			{
				if (NamedArguments == null)
				{
					named_args = new Arguments(1);
				}
				Constant expr = Constant.CreateConstantFromValue(rc.Module.PredefinedTypes.CharSet.TypeSpec, rc.Module.DefaultCharSet, Location);
				NamedArguments.Add(new NamedArgument("CharSet", loc, expr));
			}
		}

		public Attribute Clone()
		{
			return new Attribute(ExplicitTarget, expression, null, loc, nameEscaped)
			{
				pos_args = pos_args,
				named_args = NamedArguments
			};
		}

		public void AttachTo(Attributable target, IMemberContext context)
		{
			if (targets == null)
			{
				targets = new Attributable[1] { target };
				this.context = context;
			}
			else if (context is NamespaceContainer)
			{
				targets[0] = target;
				this.context = context;
			}
			else
			{
				Attributable[] array = new Attributable[targets.Length + 1];
				targets.CopyTo(array, 0);
				array[targets.Length] = target;
				targets = array;
				target.OptAttributes = null;
			}
		}

		public ResolveContext CreateResolveContext()
		{
			return new ResolveContext(context, ResolveContext.Options.ConstantScope);
		}

		private static void Error_InvalidNamedArgument(ResolveContext rc, NamedArgument name)
		{
			rc.Report.Error(617, name.Location, "`{0}' is not a valid named attribute argument. Named attribute arguments must be fields which are not readonly, static, const or read-write properties which are public and not static", name.Name);
		}

		private static void Error_InvalidNamedArgumentType(ResolveContext rc, NamedArgument name)
		{
			rc.Report.Error(655, name.Location, "`{0}' is not a valid named attribute argument because it is not a valid attribute parameter type", name.Name);
		}

		public static void Error_AttributeArgumentIsDynamic(IMemberContext context, Location loc)
		{
			context.Module.Compiler.Report.Error(1982, loc, "An attribute argument cannot be dynamic expression");
		}

		public void Error_MissingGuidAttribute()
		{
			Report.Error(596, Location, "The Guid attribute must be specified with the ComImport attribute");
		}

		public void Error_MisusedExtensionAttribute()
		{
			Report.Error(1112, Location, "Do not use `{0}' directly. Use parameter modifier `this' instead", GetSignatureForError());
		}

		public void Error_MisusedDynamicAttribute()
		{
			Report.Error(1970, loc, "Do not use `{0}' directly. Use `dynamic' keyword instead", GetSignatureForError());
		}

		private void Error_AttributeEmitError(string inner)
		{
			Report.Error(647, Location, "Error during emitting `{0}' attribute. The reason is `{1}'", Type.GetSignatureForError(), inner);
		}

		public void Error_InvalidArgumentValue(TypeSpec attributeType)
		{
			Report.Error(591, Location, "Invalid value for argument to `{0}' attribute", attributeType.GetSignatureForError());
		}

		public void Error_InvalidSecurityParent()
		{
			Report.Error(7070, Location, "Security attribute `{0}' is not valid on this declaration type. Security attributes are only valid on assembly, type and method declarations", Type.GetSignatureForError());
		}

		private void ResolveAttributeType(bool comparisonOnly)
		{
			SessionReportPrinter sessionReportPrinter = new SessionReportPrinter();
			SessionReportPrinter sessionReportPrinter2 = null;
			ReportPrinter reportPrinter = Report.SetPrinter(sessionReportPrinter);
			bool flag = false;
			bool flag2 = false;
			ATypeNameExpression aTypeNameExpression = null;
			TypeSpec typeSpec;
			TypeSpec typeSpec2;
			try
			{
				typeSpec = expression.ResolveAsType(context);
				sessionReportPrinter.EndSession();
				if (typeSpec != null && sessionReportPrinter.ErrorsCount == 0)
				{
					flag = typeSpec.IsAttribute;
				}
				if (nameEscaped)
				{
					typeSpec2 = null;
				}
				else
				{
					aTypeNameExpression = (ATypeNameExpression)expression.Clone(null);
					aTypeNameExpression.Name += "Attribute";
					sessionReportPrinter2 = new SessionReportPrinter();
					Report.SetPrinter(sessionReportPrinter2);
					typeSpec2 = aTypeNameExpression.ResolveAsType(context);
					sessionReportPrinter2.EndSession();
					if (typeSpec2 != null && sessionReportPrinter2.ErrorsCount == 0)
					{
						flag2 = typeSpec2.IsAttribute;
					}
					sessionReportPrinter2.EndSession();
				}
			}
			finally
			{
				context.Module.Compiler.Report.SetPrinter(reportPrinter);
			}
			if ((flag & flag2) && typeSpec != typeSpec2)
			{
				if (!comparisonOnly)
				{
					Report.Error(1614, Location, "`{0}' is ambiguous between `{1}' and `{2}'. Use either `@{0}' or `{0}Attribute'", GetSignatureForError(), expression.GetSignatureForError(), aTypeNameExpression.GetSignatureForError());
					resolve_error = true;
				}
			}
			else if (flag)
			{
				Type = typeSpec;
			}
			else if (flag2)
			{
				Type = typeSpec2;
			}
			else
			{
				if (comparisonOnly)
				{
					return;
				}
				resolve_error = true;
				if (typeSpec != null)
				{
					if (sessionReportPrinter.IsEmpty)
					{
						Report.SymbolRelatedToPreviousError(typeSpec);
						Report.Error(616, Location, "`{0}': is not an attribute class", typeSpec.GetSignatureForError());
					}
					else
					{
						sessionReportPrinter.Merge(reportPrinter);
					}
				}
				else if (typeSpec2 != null)
				{
					if (sessionReportPrinter2.IsEmpty)
					{
						Report.SymbolRelatedToPreviousError(typeSpec2);
						Report.Error(616, Location, "`{0}': is not an attribute class", typeSpec2.GetSignatureForError());
					}
					else
					{
						sessionReportPrinter2.Merge(reportPrinter);
					}
				}
				else
				{
					sessionReportPrinter.Merge(reportPrinter);
				}
			}
		}

		public TypeSpec ResolveTypeForComparison()
		{
			if (Type == null && !resolve_error)
			{
				ResolveAttributeType(true);
			}
			return Type;
		}

		public string GetSignatureForError()
		{
			if (Type != null)
			{
				return Type.GetSignatureForError();
			}
			return expression.GetSignatureForError();
		}

		public bool IsValidSecurityAttribute()
		{
			if (HasSecurityAttribute)
			{
				return IsSecurityActionValid();
			}
			return false;
		}

		private static bool IsValidMethodImplOption(int value)
		{
			MethodImplOptions methodImplOptions = (MethodImplOptions)256;
			foreach (MethodImplOptions value2 in System.Enum.GetValues(typeof(MethodImplOptions)))
			{
				methodImplOptions |= value2;
			}
			return ((uint)value | (uint)methodImplOptions) == (uint)methodImplOptions;
		}

		public static bool IsValidArgumentType(TypeSpec t)
		{
			if (t.IsArray)
			{
				ArrayContainer arrayContainer = (ArrayContainer)t;
				if (arrayContainer.Rank > 1)
				{
					return false;
				}
				t = arrayContainer.Element;
			}
			switch (t.BuiltinType)
			{
			case BuiltinTypeSpec.Type.FirstPrimitive:
			case BuiltinTypeSpec.Type.Byte:
			case BuiltinTypeSpec.Type.SByte:
			case BuiltinTypeSpec.Type.Char:
			case BuiltinTypeSpec.Type.Short:
			case BuiltinTypeSpec.Type.UShort:
			case BuiltinTypeSpec.Type.Int:
			case BuiltinTypeSpec.Type.UInt:
			case BuiltinTypeSpec.Type.Long:
			case BuiltinTypeSpec.Type.ULong:
			case BuiltinTypeSpec.Type.Float:
			case BuiltinTypeSpec.Type.Double:
			case BuiltinTypeSpec.Type.Object:
			case BuiltinTypeSpec.Type.Dynamic:
			case BuiltinTypeSpec.Type.String:
			case BuiltinTypeSpec.Type.Type:
				return true;
			default:
				return t.IsEnum;
			}
		}

		public MethodSpec Resolve()
		{
			if (resolve_error)
			{
				return null;
			}
			resolve_error = true;
			arg_resolved = true;
			if (Type == null)
			{
				ResolveAttributeType(false);
				if (Type == null)
				{
					return null;
				}
			}
			if (Type.IsAbstract)
			{
				Report.Error(653, Location, "Cannot apply attribute class `{0}' because it is abstract", GetSignatureForError());
				return null;
			}
			ObsoleteAttribute attributeObsolete = Type.GetAttributeObsolete();
			if (attributeObsolete != null)
			{
				AttributeTester.Report_ObsoleteMessage(attributeObsolete, Type.GetSignatureForError(), Location, Report);
			}
			ResolveContext resolveContext = null;
			MethodSpec value;
			if (pos_args != null || !context.Module.AttributeConstructorCache.TryGetValue(Type, out value))
			{
				resolveContext = CreateResolveContext();
				value = ResolveConstructor(resolveContext);
				if (value == null)
				{
					return null;
				}
				if (pos_args == null && value.Parameters.IsEmpty)
				{
					context.Module.AttributeConstructorCache.Add(Type, value);
				}
			}
			ModuleContainer module = context.Module;
			if ((Type == module.PredefinedAttributes.DllImport || Type == module.PredefinedAttributes.UnmanagedFunctionPointer) && module.HasDefaultCharSet)
			{
				if (resolveContext == null)
				{
					resolveContext = CreateResolveContext();
				}
				AddModuleCharSet(resolveContext);
			}
			if (NamedArguments != null)
			{
				if (resolveContext == null)
				{
					resolveContext = CreateResolveContext();
				}
				if (!ResolveNamedArguments(resolveContext))
				{
					return null;
				}
			}
			resolve_error = false;
			return value;
		}

		private MethodSpec ResolveConstructor(ResolveContext ec)
		{
			if (pos_args != null)
			{
				bool dynamic;
				pos_args.Resolve(ec, out dynamic);
				if (dynamic)
				{
					Error_AttributeArgumentIsDynamic(ec.MemberContext, loc);
					return null;
				}
			}
			return Expression.ConstructorLookup(ec, Type, ref pos_args, loc);
		}

		private bool ResolveNamedArguments(ResolveContext ec)
		{
			int count = NamedArguments.Count;
			List<string> list = new List<string>(count);
			named_values = new List<KeyValuePair<MemberExpr, NamedArgument>>(count);
			foreach (NamedArgument namedArgument in NamedArguments)
			{
				string name = namedArgument.Name;
				if (list.Contains(name))
				{
					ec.Report.Error(643, namedArgument.Location, "Duplicate named attribute `{0}' argument", name);
					continue;
				}
				list.Add(name);
				namedArgument.Resolve(ec);
				Expression expression = Expression.MemberLookup(ec, false, Type, name, 0, Expression.MemberLookupRestrictions.ExactArity, loc);
				if (expression == null)
				{
					expression = Expression.MemberLookup(ec, true, Type, name, 0, Expression.MemberLookupRestrictions.ExactArity, loc);
					if (expression != null)
					{
						Expression.ErrorIsInaccesible(ec, expression.GetSignatureForError(), loc);
						return false;
					}
				}
				if (expression == null)
				{
					Expression.Error_TypeDoesNotContainDefinition(ec, Location, Type, name);
					return false;
				}
				if (!(expression is PropertyExpr) && !(expression is FieldExpr))
				{
					Error_InvalidNamedArgument(ec, namedArgument);
					return false;
				}
				ObsoleteAttribute attributeObsolete;
				if (expression is PropertyExpr)
				{
					PropertySpec propertyInfo = ((PropertyExpr)expression).PropertyInfo;
					if (!propertyInfo.HasSet || !propertyInfo.HasGet || propertyInfo.IsStatic || !propertyInfo.Get.IsPublic || !propertyInfo.Set.IsPublic)
					{
						ec.Report.SymbolRelatedToPreviousError(propertyInfo);
						Error_InvalidNamedArgument(ec, namedArgument);
						return false;
					}
					if (!IsValidArgumentType(expression.Type))
					{
						ec.Report.SymbolRelatedToPreviousError(propertyInfo);
						Error_InvalidNamedArgumentType(ec, namedArgument);
						return false;
					}
					attributeObsolete = propertyInfo.GetAttributeObsolete();
					propertyInfo.MemberDefinition.SetIsAssigned();
				}
				else
				{
					FieldSpec spec = ((FieldExpr)expression).Spec;
					if (spec.IsReadOnly || spec.IsStatic || !spec.IsPublic)
					{
						Error_InvalidNamedArgument(ec, namedArgument);
						return false;
					}
					if (!IsValidArgumentType(expression.Type))
					{
						ec.Report.SymbolRelatedToPreviousError(spec);
						Error_InvalidNamedArgumentType(ec, namedArgument);
						return false;
					}
					attributeObsolete = spec.GetAttributeObsolete();
					spec.MemberDefinition.SetIsAssigned();
				}
				if (attributeObsolete != null && !context.IsObsolete)
				{
					AttributeTester.Report_ObsoleteMessage(attributeObsolete, expression.GetSignatureForError(), expression.Location, Report);
				}
				if (namedArgument.Type != expression.Type)
				{
					namedArgument.Expr = Convert.ImplicitConversionRequired(ec, namedArgument.Expr, expression.Type, namedArgument.Expr.Location);
				}
				if (namedArgument.Expr != null)
				{
					named_values.Add(new KeyValuePair<MemberExpr, NamedArgument>((MemberExpr)expression, namedArgument));
				}
			}
			return true;
		}

		public string GetValidTargets()
		{
			StringBuilder stringBuilder = new StringBuilder();
			AttributeTargets validOn = Type.GetAttributeUsage(context.Module.PredefinedAttributes.AttributeUsage).ValidOn;
			if ((validOn & AttributeTargets.Assembly) != 0)
			{
				stringBuilder.Append("assembly, ");
			}
			if ((validOn & AttributeTargets.Module) != 0)
			{
				stringBuilder.Append("module, ");
			}
			if ((validOn & AttributeTargets.Class) != 0)
			{
				stringBuilder.Append("class, ");
			}
			if ((validOn & AttributeTargets.Struct) != 0)
			{
				stringBuilder.Append("struct, ");
			}
			if ((validOn & AttributeTargets.Enum) != 0)
			{
				stringBuilder.Append("enum, ");
			}
			if ((validOn & AttributeTargets.Constructor) != 0)
			{
				stringBuilder.Append("constructor, ");
			}
			if ((validOn & AttributeTargets.Method) != 0)
			{
				stringBuilder.Append("method, ");
			}
			if ((validOn & AttributeTargets.Property) != 0)
			{
				stringBuilder.Append("property, indexer, ");
			}
			if ((validOn & AttributeTargets.Field) != 0)
			{
				stringBuilder.Append("field, ");
			}
			if ((validOn & AttributeTargets.Event) != 0)
			{
				stringBuilder.Append("event, ");
			}
			if ((validOn & AttributeTargets.Interface) != 0)
			{
				stringBuilder.Append("interface, ");
			}
			if ((validOn & AttributeTargets.Parameter) != 0)
			{
				stringBuilder.Append("parameter, ");
			}
			if ((validOn & AttributeTargets.Delegate) != 0)
			{
				stringBuilder.Append("delegate, ");
			}
			if ((validOn & AttributeTargets.ReturnValue) != 0)
			{
				stringBuilder.Append("return, ");
			}
			if ((validOn & AttributeTargets.GenericParameter) != 0)
			{
				stringBuilder.Append("type parameter, ");
			}
			return stringBuilder.Remove(stringBuilder.Length - 2, 2).ToString();
		}

		public AttributeUsageAttribute GetAttributeUsageAttribute()
		{
			if (!arg_resolved)
			{
				Resolve();
			}
			if (resolve_error)
			{
				return DefaultUsageAttribute;
			}
			AttributeUsageAttribute attributeUsageAttribute = new AttributeUsageAttribute((AttributeTargets)((Constant)pos_args[0].Expr).GetValue());
			BoolConstant boolConstant = GetNamedValue("AllowMultiple") as BoolConstant;
			if (boolConstant != null)
			{
				attributeUsageAttribute.AllowMultiple = boolConstant.Value;
			}
			boolConstant = GetNamedValue("Inherited") as BoolConstant;
			if (boolConstant != null)
			{
				attributeUsageAttribute.Inherited = boolConstant.Value;
			}
			return attributeUsageAttribute;
		}

		public string GetIndexerAttributeValue()
		{
			if (!arg_resolved)
			{
				Resolve();
			}
			if (resolve_error || pos_args.Count != 1 || !(pos_args[0].Expr is Constant))
			{
				return null;
			}
			return ((Constant)pos_args[0].Expr).GetValue() as string;
		}

		public string GetConditionalAttributeValue()
		{
			if (!arg_resolved)
			{
				Resolve();
			}
			if (resolve_error)
			{
				return null;
			}
			return ((Constant)pos_args[0].Expr).GetValue() as string;
		}

		public ObsoleteAttribute GetObsoleteAttribute()
		{
			if (!arg_resolved)
			{
				Class obj = Type.MemberDefinition as Class;
				if (obj != null && !obj.HasMembersDefined)
				{
					obj.Define();
				}
				Resolve();
			}
			if (resolve_error)
			{
				return null;
			}
			if (pos_args == null)
			{
				return new ObsoleteAttribute();
			}
			string message = ((Constant)pos_args[0].Expr).GetValue() as string;
			if (pos_args.Count == 1)
			{
				return new ObsoleteAttribute(message);
			}
			return new ObsoleteAttribute(message, ((BoolConstant)pos_args[1].Expr).Value);
		}

		public bool GetClsCompliantAttributeValue()
		{
			if (!arg_resolved)
			{
				Resolve();
			}
			if (resolve_error)
			{
				return false;
			}
			return ((BoolConstant)pos_args[0].Expr).Value;
		}

		public TypeSpec GetCoClassAttributeValue()
		{
			if (!arg_resolved)
			{
				Resolve();
			}
			if (resolve_error)
			{
				return null;
			}
			return GetArgumentType();
		}

		public bool CheckTarget()
		{
			string[] validAttributeTargets = Owner.ValidAttributeTargets;
			if (ExplicitTarget == null || ExplicitTarget == validAttributeTargets[0])
			{
				Target = Owner.AttributeTargets;
				return true;
			}
			if (Array.Exists(validAttributeTargets, (string i) => i == ExplicitTarget))
			{
				switch (ExplicitTarget)
				{
				case "return":
					Target = AttributeTargets.ReturnValue;
					return true;
				case "param":
					Target = AttributeTargets.Parameter;
					return true;
				case "field":
					Target = AttributeTargets.Field;
					return true;
				case "method":
					Target = AttributeTargets.Method;
					return true;
				case "property":
					Target = AttributeTargets.Property;
					return true;
				case "module":
					Target = AttributeTargets.Module;
					return true;
				default:
					throw new InternalErrorException("Unknown explicit target: " + ExplicitTarget);
				}
			}
			StringBuilder stringBuilder = new StringBuilder();
			string[] array = validAttributeTargets;
			foreach (string value in array)
			{
				stringBuilder.Append(value);
				stringBuilder.Append(", ");
			}
			stringBuilder.Remove(stringBuilder.Length - 2, 2);
			Report.Warning(657, 1, Location, "`{0}' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `{1}'. All attributes in this section will be ignored", ExplicitTarget, stringBuilder.ToString());
			return false;
		}

		private bool IsSecurityActionValid()
		{
			Constant value = null;
			SecurityAction? securityActionValue = GetSecurityActionValue(ref value);
			bool flag = Target == AttributeTargets.Assembly || Target == AttributeTargets.Module;
			switch (securityActionValue)
			{
			case SecurityAction.Demand:
			case SecurityAction.Assert:
			case SecurityAction.Deny:
			case SecurityAction.PermitOnly:
			case SecurityAction.LinkDemand:
			case SecurityAction.InheritanceDemand:
				if (!flag)
				{
					return true;
				}
				break;
			case SecurityAction.RequestMinimum:
			case SecurityAction.RequestOptional:
			case SecurityAction.RequestRefuse:
				if (flag)
				{
					return true;
				}
				break;
			case null:
				Report.Error(7048, loc, "First argument of a security attribute `{0}' must be a valid SecurityAction", Type.GetSignatureForError());
				return false;
			default:
				Report.Error(7049, value.Location, "Security attribute `{0}' has an invalid SecurityAction value `{1}'", Type.GetSignatureForError(), value.GetValueAsLiteral());
				return false;
			}
			AttributeTargets target = Target;
			if (target == AttributeTargets.Assembly)
			{
				Report.Error(7050, value.Location, "SecurityAction value `{0}' is invalid for security attributes applied to an assembly", value.GetSignatureForError());
			}
			else
			{
				Report.Error(7051, value.Location, "SecurityAction value `{0}' is invalid for security attributes applied to a type or a method", value.GetSignatureForError());
			}
			return false;
		}

		private SecurityAction? GetSecurityActionValue(ref Constant value)
		{
			if (pos_args == null)
			{
				PredefinedAttributes predefinedAttributes = context.Module.PredefinedAttributes;
				if (Type == predefinedAttributes.HostProtection.TypeSpec)
				{
					value = new IntConstant(context.Module.Compiler.BuiltinTypes, 6, loc);
					return SecurityAction.LinkDemand;
				}
				return null;
			}
			value = (Constant)pos_args[0].Expr;
			return (SecurityAction)value.GetValue();
		}

		public void ExtractSecurityPermissionSet(MethodSpec ctor, ref Dictionary<SecurityAction, PermissionSet> permissions)
		{
			throw new NotSupportedException();
		}

		public Constant GetNamedValue(string name)
		{
			if (named_values == null)
			{
				return null;
			}
			for (int i = 0; i < named_values.Count; i++)
			{
				if (named_values[i].Value.Name == name)
				{
					return named_values[i].Value.Expr as Constant;
				}
			}
			return null;
		}

		public CharSet GetCharSetValue()
		{
			return (CharSet)System.Enum.Parse(typeof(CharSet), ((Constant)pos_args[0].Expr).GetValue().ToString());
		}

		public bool HasField(string fieldName)
		{
			if (named_values == null)
			{
				return false;
			}
			foreach (KeyValuePair<MemberExpr, NamedArgument> named_value in named_values)
			{
				if (named_value.Value.Name == fieldName)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsInternalCall()
		{
			return (GetMethodImplOptions() & MethodImplOptions.InternalCall) != 0;
		}

		public MethodImplOptions GetMethodImplOptions()
		{
			MethodImplOptions result = (MethodImplOptions)0;
			if (pos_args.Count == 1)
			{
				result = (MethodImplOptions)System.Enum.Parse(typeof(MethodImplOptions), ((Constant)pos_args[0].Expr).GetValue().ToString());
			}
			else if (HasField("Value"))
			{
				Constant namedValue = GetNamedValue("Value");
				result = (MethodImplOptions)System.Enum.Parse(typeof(MethodImplOptions), namedValue.GetValue().ToString());
			}
			return result;
		}

		public bool IsExplicitLayoutKind()
		{
			if (pos_args == null || pos_args.Count != 1)
			{
				return false;
			}
			return (LayoutKind)System.Enum.Parse(typeof(LayoutKind), ((Constant)pos_args[0].Expr).GetValue().ToString()) == LayoutKind.Explicit;
		}

		public Expression GetParameterDefaultValue()
		{
			if (pos_args == null)
			{
				return null;
			}
			return pos_args[0].Expr;
		}

		public override bool Equals(object obj)
		{
			Attribute attribute = obj as Attribute;
			if (attribute == null)
			{
				return false;
			}
			if (Type == attribute.Type)
			{
				return Target == attribute.Target;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode() ^ Target.GetHashCode();
		}

		public void Emit(Dictionary<Attribute, List<Attribute>> allEmitted)
		{
			MethodSpec methodSpec = Resolve();
			if (methodSpec == null)
			{
				return;
			}
			PredefinedAttributes predefinedAttributes = context.Module.PredefinedAttributes;
			AttributeUsageAttribute attributeUsage = Type.GetAttributeUsage(predefinedAttributes.AttributeUsage);
			if ((attributeUsage.ValidOn & Target) == 0)
			{
				Report.Error(592, Location, "The attribute `{0}' is not valid on this declaration type. It is valid on `{1}' declarations only", GetSignatureForError(), GetValidTargets());
				return;
			}
			byte[] cdata;
			if (pos_args == null && named_values == null)
			{
				cdata = AttributeEncoder.Empty;
			}
			else
			{
				AttributeEncoder attributeEncoder = new AttributeEncoder();
				if (pos_args != null)
				{
					TypeSpec[] types = methodSpec.Parameters.Types;
					for (int i = 0; i < pos_args.Count; i++)
					{
						TypeSpec typeSpec = types[i];
						Expression expr = pos_args[i].Expr;
						if (i == 0)
						{
							if ((Type == predefinedAttributes.IndexerName || Type == predefinedAttributes.Conditional) && expr is Constant)
							{
								string s = ((Constant)expr).GetValue() as string;
								if (!Tokenizer.IsValidIdentifier(s) || (Type == predefinedAttributes.IndexerName && Tokenizer.IsKeyword(s)))
								{
									context.Module.Compiler.Report.Error(633, expr.Location, "The argument to the `{0}' attribute must be a valid identifier", GetSignatureForError());
									return;
								}
							}
							else if (Type == predefinedAttributes.Guid)
							{
								string value = ((StringConstant)expr).Value;
								try
								{
									new Guid(value);
								}
								catch
								{
									Error_InvalidArgumentValue(Type);
									return;
								}
							}
							else if (Type == predefinedAttributes.AttributeUsage)
							{
								if (((IntConstant)((EnumConstant)expr).Child).Value == 0)
								{
									Error_InvalidArgumentValue(Type);
								}
							}
							else if (Type == predefinedAttributes.MarshalAs)
							{
								if (pos_args.Count == 1 && (UnmanagedType)System.Enum.Parse(typeof(UnmanagedType), ((Constant)pos_args[0].Expr).GetValue().ToString()) == UnmanagedType.ByValArray && !(Owner is FieldBase))
								{
									Report.Error(7055, pos_args[0].Expr.Location, "Unmanaged type `ByValArray' is only valid for fields");
								}
							}
							else if (Type == predefinedAttributes.DllImport)
							{
								if (pos_args.Count == 1 && pos_args[0].Expr is Constant && string.IsNullOrEmpty(((Constant)pos_args[0].Expr).GetValue() as string))
								{
									Error_InvalidArgumentValue(Type);
								}
							}
							else if (Type == predefinedAttributes.MethodImpl && pos_args.Count == 1 && !IsValidMethodImplOption((int)((Constant)expr).GetValueAsLong()))
							{
								Error_InvalidArgumentValue(Type);
							}
						}
						expr.EncodeAttributeValue(context, attributeEncoder, typeSpec, typeSpec);
					}
				}
				if (named_values != null)
				{
					attributeEncoder.Encode((ushort)named_values.Count);
					foreach (KeyValuePair<MemberExpr, NamedArgument> named_value in named_values)
					{
						if (named_value.Key is FieldExpr)
						{
							attributeEncoder.Encode((byte)83);
						}
						else
						{
							attributeEncoder.Encode((byte)84);
						}
						attributeEncoder.Encode(named_value.Key.Type);
						attributeEncoder.Encode(named_value.Value.Name);
						named_value.Value.Expr.EncodeAttributeValue(context, attributeEncoder, named_value.Key.Type, named_value.Key.Type);
					}
				}
				else
				{
					attributeEncoder.EncodeEmptyNamedArguments();
				}
				cdata = attributeEncoder.ToArray();
			}
			if (!methodSpec.DeclaringType.IsConditionallyExcluded(context))
			{
				try
				{
					Attributable[] array = targets;
					for (int j = 0; j < array.Length; j++)
					{
						array[j].ApplyAttributeBuilder(this, methodSpec, cdata, predefinedAttributes);
					}
				}
				catch (Exception ex)
				{
					if (!(ex is BadImageFormatException) || Report.Errors <= 0)
					{
						Error_AttributeEmitError(ex.Message);
					}
					return;
				}
			}
			if (!attributeUsage.AllowMultiple && allEmitted != null)
			{
				if (allEmitted.ContainsKey(this))
				{
					List<Attribute> list = allEmitted[this];
					if (list == null)
					{
						list = (allEmitted[this] = new List<Attribute>(2));
					}
					list.Add(this);
				}
				else
				{
					allEmitted.Add(this, null);
				}
			}
			if (context.Module.Compiler.Settings.VerifyClsCompliance && Owner.IsClsComplianceRequired())
			{
				if (pos_args != null)
				{
					pos_args.CheckArrayAsAttribute(context.Module.Compiler);
				}
				if (NamedArguments != null)
				{
					NamedArguments.CheckArrayAsAttribute(context.Module.Compiler);
				}
			}
		}

		private Expression GetValue()
		{
			if (pos_args == null || pos_args.Count < 1)
			{
				return null;
			}
			return pos_args[0].Expr;
		}

		public string GetString()
		{
			Expression value = GetValue();
			if (value is StringConstant)
			{
				return ((StringConstant)value).Value;
			}
			return null;
		}

		public bool GetBoolean()
		{
			Expression value = GetValue();
			if (value is BoolConstant)
			{
				return ((BoolConstant)value).Value;
			}
			return false;
		}

		public TypeSpec GetArgumentType()
		{
			return (GetValue() as TypeOf)?.TypeArgument;
		}
	}
}
