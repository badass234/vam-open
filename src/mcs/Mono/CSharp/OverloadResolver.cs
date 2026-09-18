using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public struct OverloadResolver
	{
		[Flags]
		public enum Restrictions
		{
			None = 0,
			DelegateInvoke = 1,
			ProbingOnly = 2,
			CovariantDelegate = 4,
			NoBaseMembers = 8,
			BaseMembersIncluded = 0x10,
			GetEnumeratorLookup = 0x20
		}

		public interface IBaseMembersProvider
		{
			IList<MemberSpec> GetBaseMembers(TypeSpec baseType);

			IParametersMember GetOverrideMemberParameters(MemberSpec member);

			MethodGroupExpr LookupExtensionMethod(ResolveContext rc);
		}

		public interface IErrorHandler
		{
			bool AmbiguousCandidates(ResolveContext rc, MemberSpec best, MemberSpec ambiguous);

			bool ArgumentMismatch(ResolveContext rc, MemberSpec best, Argument a, int index);

			bool NoArgumentMatch(ResolveContext rc, MemberSpec best);

			bool TypeInferenceFailed(ResolveContext rc, MemberSpec best);
		}

		public interface IInstanceQualifier
		{
			TypeSpec InstanceType { get; }

			bool CheckProtectedMemberAccess(ResolveContext rc, MemberSpec member);
		}

		private sealed class NoBaseMembers : IBaseMembersProvider
		{
			public static readonly IBaseMembersProvider Instance = new NoBaseMembers();

			public IList<MemberSpec> GetBaseMembers(TypeSpec baseType)
			{
				return null;
			}

			public IParametersMember GetOverrideMemberParameters(MemberSpec member)
			{
				return null;
			}

			public MethodGroupExpr LookupExtensionMethod(ResolveContext rc)
			{
				return null;
			}
		}

		private struct AmbiguousCandidate
		{
			public readonly MemberSpec Member;

			public readonly bool Expanded;

			public readonly AParametersCollection Parameters;

			public AmbiguousCandidate(MemberSpec member, AParametersCollection parameters, bool expanded)
			{
				Member = member;
				Parameters = parameters;
				Expanded = expanded;
			}
		}

		private Location loc;

		private IList<MemberSpec> members;

		private TypeArguments type_arguments;

		private IBaseMembersProvider base_provider;

		private IErrorHandler custom_errors;

		private IInstanceQualifier instance_qualifier;

		private Restrictions restrictions;

		private MethodGroupExpr best_candidate_extension_group;

		private TypeSpec best_candidate_return_type;

		private SessionReportPrinter lambda_conv_msgs;

		public IBaseMembersProvider BaseMembersProvider
		{
			get
			{
				return base_provider;
			}
			set
			{
				base_provider = value;
			}
		}

		public bool BestCandidateIsDynamic { get; set; }

		public MethodGroupExpr BestCandidateNewMethodGroup => best_candidate_extension_group;

		public TypeSpec BestCandidateReturnType => best_candidate_return_type;

		public IErrorHandler CustomErrors
		{
			get
			{
				return custom_errors;
			}
			set
			{
				custom_errors = value;
			}
		}

		private TypeSpec DelegateType
		{
			get
			{
				if ((restrictions & Restrictions.DelegateInvoke) == 0)
				{
					throw new InternalErrorException("Not running in delegate mode", loc);
				}
				return members[0].DeclaringType;
			}
		}

		public IInstanceQualifier InstanceQualifier
		{
			get
			{
				return instance_qualifier;
			}
			set
			{
				instance_qualifier = value;
			}
		}

		private bool IsProbingOnly => (restrictions & Restrictions.ProbingOnly) != 0;

		private bool IsDelegateInvoke => (restrictions & Restrictions.DelegateInvoke) != 0;

		public OverloadResolver(IList<MemberSpec> members, Restrictions restrictions, Location loc)
			: this(members, null, restrictions, loc)
		{
		}

		public OverloadResolver(IList<MemberSpec> members, TypeArguments targs, Restrictions restrictions, Location loc)
		{
			this = default(OverloadResolver);
			if (members == null || members.Count == 0)
			{
				throw new ArgumentException("empty members set");
			}
			this.members = members;
			this.loc = loc;
			type_arguments = targs;
			this.restrictions = restrictions;
			if (IsDelegateInvoke)
			{
				this.restrictions |= Restrictions.NoBaseMembers;
			}
			base_provider = NoBaseMembers.Instance;
		}

		private static int BetterExpressionConversion(ResolveContext ec, Argument a, TypeSpec p, TypeSpec q)
		{
			TypeSpec typeSpec = a.Type;
			if (typeSpec == InternalType.AnonymousMethod && ec.Module.Compiler.Settings.Version > LanguageVersion.ISO_2)
			{
				if (p.IsExpressionTreeType || q.IsExpressionTreeType)
				{
					if (q.MemberDefinition != p.MemberDefinition)
					{
						return 0;
					}
					q = TypeManager.GetTypeArguments(q)[0];
					p = TypeManager.GetTypeArguments(p)[0];
				}
				MethodSpec invokeMethod = Delegate.GetInvokeMethod(p);
				MethodSpec invokeMethod2 = Delegate.GetInvokeMethod(q);
				if (!TypeSpecComparer.Equals(invokeMethod.Parameters.Types, invokeMethod2.Parameters.Types))
				{
					return 0;
				}
				p = invokeMethod.ReturnType;
				TypeSpec delegate_type = q;
				q = invokeMethod2.ReturnType;
				if (p.Kind == MemberKind.Void)
				{
					if (q.Kind == MemberKind.Void)
					{
						return 0;
					}
					return 2;
				}
				if (q.Kind == MemberKind.Void)
				{
					if (p.Kind == MemberKind.Void)
					{
						return 0;
					}
					return 1;
				}
				AnonymousMethodExpression anonymousMethodExpression = (AnonymousMethodExpression)a.Expr;
				if ((p.IsGenericTask || q.IsGenericTask) && anonymousMethodExpression.Block.IsAsync && p.IsGenericTask && q.IsGenericTask)
				{
					q = q.TypeArguments[0];
					p = p.TypeArguments[0];
				}
				if (q != p)
				{
					typeSpec = anonymousMethodExpression.InferReturnType(ec, null, delegate_type);
					if (typeSpec == null)
					{
						return 1;
					}
					if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
					{
						typeSpec = ec.BuiltinTypes.Object;
					}
				}
			}
			if (typeSpec == p)
			{
				return 1;
			}
			if (typeSpec == q)
			{
				return 2;
			}
			return BetterTypeConversion(ec, p, q);
		}

		public static int BetterTypeConversion(ResolveContext ec, TypeSpec p, TypeSpec q)
		{
			if (p == null || q == null)
			{
				throw new InternalErrorException("BetterTypeConversion got a null conversion");
			}
			switch (p.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Int:
				if (q.BuiltinType == BuiltinTypeSpec.Type.UInt || q.BuiltinType == BuiltinTypeSpec.Type.ULong)
				{
					return 1;
				}
				break;
			case BuiltinTypeSpec.Type.Long:
				if (q.BuiltinType == BuiltinTypeSpec.Type.ULong)
				{
					return 1;
				}
				break;
			case BuiltinTypeSpec.Type.SByte:
				switch (q.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Byte:
				case BuiltinTypeSpec.Type.UShort:
				case BuiltinTypeSpec.Type.UInt:
				case BuiltinTypeSpec.Type.ULong:
					return 1;
				}
				break;
			case BuiltinTypeSpec.Type.Short:
				switch (q.BuiltinType)
				{
				case BuiltinTypeSpec.Type.UShort:
				case BuiltinTypeSpec.Type.UInt:
				case BuiltinTypeSpec.Type.ULong:
					return 1;
				}
				break;
			case BuiltinTypeSpec.Type.Dynamic:
				return 2;
			}
			switch (q.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Int:
				if (p.BuiltinType == BuiltinTypeSpec.Type.UInt || p.BuiltinType == BuiltinTypeSpec.Type.ULong)
				{
					return 2;
				}
				break;
			case BuiltinTypeSpec.Type.Long:
				if (p.BuiltinType == BuiltinTypeSpec.Type.ULong)
				{
					return 2;
				}
				break;
			case BuiltinTypeSpec.Type.SByte:
				switch (p.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Byte:
				case BuiltinTypeSpec.Type.UShort:
				case BuiltinTypeSpec.Type.UInt:
				case BuiltinTypeSpec.Type.ULong:
					return 2;
				}
				break;
			case BuiltinTypeSpec.Type.Short:
				switch (p.BuiltinType)
				{
				case BuiltinTypeSpec.Type.UShort:
				case BuiltinTypeSpec.Type.UInt:
				case BuiltinTypeSpec.Type.ULong:
					return 2;
				}
				break;
			case BuiltinTypeSpec.Type.Dynamic:
				return 1;
			}
			Expression expr = new EmptyExpression(p);
			Expression expr2 = new EmptyExpression(q);
			bool flag = Convert.ImplicitConversionExists(ec, expr, q);
			bool flag2 = Convert.ImplicitConversionExists(ec, expr2, p);
			if (flag && !flag2)
			{
				return 1;
			}
			if (flag2 && !flag)
			{
				return 2;
			}
			return 0;
		}

		private static bool BetterFunction(ResolveContext ec, Arguments args, MemberSpec candidate, AParametersCollection cparam, bool candidate_params, MemberSpec best, AParametersCollection bparam, bool best_params)
		{
			AParametersCollection parameters = ((IParametersMember)candidate).Parameters;
			AParametersCollection parameters2 = ((IParametersMember)best).Parameters;
			bool flag = false;
			bool flag2 = true;
			int num = args?.Count ?? 0;
			int i = 0;
			Argument argument = null;
			int num2 = 0;
			int num3 = 0;
			while (i < num)
			{
				argument = args[i];
				if (argument.IsDefaultArgument)
				{
					break;
				}
				NamedArgument namedArgument = argument as NamedArgument;
				TypeSpec typeSpec;
				TypeSpec typeSpec2;
				if (namedArgument != null)
				{
					int parameterIndexByName = cparam.GetParameterIndexByName(namedArgument.Name);
					typeSpec = parameters.Types[parameterIndexByName];
					if (candidate_params && parameters.FixedParameters[parameterIndexByName].ModFlags == Parameter.Modifier.PARAMS)
					{
						typeSpec = TypeManager.GetElementType(typeSpec);
					}
					parameterIndexByName = bparam.GetParameterIndexByName(namedArgument.Name);
					typeSpec2 = parameters2.Types[parameterIndexByName];
					if (best_params && parameters2.FixedParameters[parameterIndexByName].ModFlags == Parameter.Modifier.PARAMS)
					{
						typeSpec2 = TypeManager.GetElementType(typeSpec2);
					}
				}
				else
				{
					typeSpec = parameters.Types[num2];
					typeSpec2 = parameters2.Types[num3];
					if (candidate_params && parameters.FixedParameters[num2].ModFlags == Parameter.Modifier.PARAMS)
					{
						typeSpec = TypeManager.GetElementType(typeSpec);
						num2--;
					}
					if (best_params && parameters2.FixedParameters[num3].ModFlags == Parameter.Modifier.PARAMS)
					{
						typeSpec2 = TypeManager.GetElementType(typeSpec2);
						num3--;
					}
				}
				if (!TypeSpecComparer.IsEqual(typeSpec, typeSpec2))
				{
					flag2 = false;
					switch (BetterExpressionConversion(ec, argument, typeSpec, typeSpec2))
					{
					case 2:
						return false;
					default:
						flag = true;
						break;
					case 0:
						break;
					}
				}
				i++;
				num2++;
				num3++;
			}
			if (flag)
			{
				return true;
			}
			if (!flag2)
			{
				if (parameters.Count < parameters2.Count && !candidate_params && parameters2.FixedParameters[i].HasDefaultValue)
				{
					return true;
				}
				return false;
			}
			if (candidate_params != best_params)
			{
				return !candidate_params;
			}
			for (; i < parameters.Count && i < parameters2.Count; i++)
			{
				IParameterData parameterData = parameters.FixedParameters[i];
				IParameterData parameterData2 = parameters2.FixedParameters[i];
				if (parameterData.HasDefaultValue != parameterData2.HasDefaultValue)
				{
					return parameterData.HasDefaultValue;
				}
				if (parameters.Count == parameters2.Count)
				{
					if (!parameterData.HasDefaultValue)
					{
						break;
					}
					continue;
				}
				return false;
			}
			if (parameters.Count != parameters2.Count)
			{
				return parameters.Count < parameters2.Count;
			}
			if (best.IsGeneric != candidate.IsGeneric)
			{
				return best.IsGeneric;
			}
			AParametersCollection parameters3 = ((IParametersMember)candidate.MemberDefinition).Parameters;
			AParametersCollection parameters4 = ((IParametersMember)best.MemberDefinition).Parameters;
			bool flag3 = false;
			for (i = 0; i < num; i++)
			{
				NamedArgument namedArgument2 = ((num == 0) ? null : (args[i] as NamedArgument));
				TypeSpec typeSpec;
				TypeSpec typeSpec2;
				if (namedArgument2 != null)
				{
					typeSpec = parameters3.Types[cparam.GetParameterIndexByName(namedArgument2.Name)];
					typeSpec2 = parameters4.Types[bparam.GetParameterIndexByName(namedArgument2.Name)];
				}
				else
				{
					typeSpec = parameters3.Types[i];
					typeSpec2 = parameters4.Types[i];
				}
				if (typeSpec != typeSpec2)
				{
					TypeSpec typeSpec3 = MoreSpecific(typeSpec, typeSpec2);
					if (typeSpec3 == typeSpec2)
					{
						return false;
					}
					if (typeSpec3 == typeSpec)
					{
						flag3 = true;
					}
				}
			}
			if (flag3)
			{
				return true;
			}
			return false;
		}

		private static bool CheckInflatedArguments(MethodSpec ms)
		{
			if (!TypeParameterSpec.HasAnyTypeParameterTypeConstrained(ms.GenericDefinition))
			{
				return true;
			}
			ConstraintChecker constraintChecker = new ConstraintChecker(null);
			TypeSpec[] types = ms.Parameters.Types;
			for (int i = 0; i < types.Length; i++)
			{
				InflatedTypeSpec inflatedTypeSpec = types[i] as InflatedTypeSpec;
				if (inflatedTypeSpec != null)
				{
					TypeSpec[] typeArguments = inflatedTypeSpec.TypeArguments;
					if (typeArguments.Length != 0 && !constraintChecker.CheckAll(inflatedTypeSpec.GetDefinition(), typeArguments, inflatedTypeSpec.Constraints, Location.Null))
					{
						return false;
					}
				}
			}
			return true;
		}

		public static void Error_ConstructorMismatch(ResolveContext rc, TypeSpec type, int argCount, Location loc)
		{
			rc.Report.Error(1729, loc, "The type `{0}' does not contain a constructor that takes `{1}' arguments", type.GetSignatureForError(), argCount.ToString());
		}

		private int IsApplicable(ResolveContext ec, ref Arguments arguments, int arg_count, ref MemberSpec candidate, IParametersMember pm, ref bool params_expanded_form, ref bool dynamicArgument, ref TypeSpec returnType, bool errorMode)
		{
			AParametersCollection parameters = pm.Parameters;
			AParametersCollection parameters2 = ((IParametersMember)candidate).Parameters;
			int num = parameters.Count;
			int num2 = 0;
			Arguments arguments2 = arguments;
			if (arg_count != num)
			{
				if ((restrictions & Restrictions.CovariantDelegate) == 0)
				{
					for (int i = 0; i < parameters.Count; i++)
					{
						if (parameters.FixedParameters[i].HasDefaultValue)
						{
							num2 = parameters.Count - i;
							break;
						}
					}
				}
				if (num2 != 0)
				{
					if (parameters2.HasParams)
					{
						num2--;
						if (arg_count < num)
						{
							num--;
						}
					}
					else
					{
						if (arg_count > num)
						{
							int num3 = System.Math.Abs(arg_count - num);
							return 1000000000 + num3;
						}
						if (arg_count < num - num2)
						{
							int num4 = System.Math.Abs(num - num2 - arg_count);
							return 1000000000 + num4;
						}
					}
				}
				else if (arg_count != num)
				{
					int num5 = System.Math.Abs(arg_count - num);
					if (!parameters2.HasParams)
					{
						return 1000000000 + num5;
					}
					if (arg_count < num - 1)
					{
						return 1000000000 + num5;
					}
				}
				if (num2 != 0)
				{
					if (arguments == null)
					{
						arguments = new Arguments(num2);
					}
					else
					{
						Arguments arguments3 = new Arguments(num);
						arguments3.AddRange(arguments);
						arguments = arguments3;
					}
					for (int j = arg_count; j < num; j++)
					{
						arguments.Add(null);
					}
				}
			}
			if (arg_count > 0)
			{
				if (arguments[arg_count - 1] is NamedArgument)
				{
					arg_count = arguments.Count;
					for (int k = 0; k < arg_count; k++)
					{
						bool flag = false;
						Argument argument;
						do
						{
							NamedArgument namedArgument = arguments[k] as NamedArgument;
							if (namedArgument == null)
							{
								break;
							}
							int parameterIndexByName = parameters.GetParameterIndexByName(namedArgument.Name);
							if (parameterIndexByName < 0)
							{
								return 100000000 - k;
							}
							if (parameterIndexByName == k)
							{
								break;
							}
							if (parameterIndexByName >= num)
							{
								if ((parameters2.FixedParameters[parameterIndexByName].ModFlags & Parameter.Modifier.PARAMS) == 0)
								{
									break;
								}
								arguments.Add(null);
								arg_count++;
								argument = null;
							}
							else
							{
								if (parameterIndexByName == arg_count)
								{
									return 100000000 - k - 1;
								}
								argument = arguments[parameterIndexByName];
								if (argument != null && !(argument is NamedArgument))
								{
									break;
								}
							}
							if (!flag)
							{
								arguments = arguments.MarkOrderedArgument(namedArgument);
								flag = true;
							}
							if (arguments == arguments2)
							{
								arguments = new Arguments(arguments2.Count);
								arguments.AddRange(arguments2);
							}
							arguments[parameterIndexByName] = arguments[k];
							arguments[k] = argument;
						}
						while (argument != null);
					}
				}
				else
				{
					arg_count = arguments.Count;
				}
			}
			else if (arguments != null)
			{
				arg_count = arguments.Count;
			}
			if (arg_count != num && !parameters2.HasParams)
			{
				return 10000000 - System.Math.Abs(num - arg_count);
			}
			List<MissingTypeSpecReference> missingDependencies = candidate.GetMissingDependencies();
			if (missingDependencies != null)
			{
				ImportedTypeDefinition.Error_MissingDependency(ec, missingDependencies, loc);
				return -1;
			}
			MethodSpec methodSpec = candidate as MethodSpec;
			TypeSpec[] types;
			if (methodSpec != null && methodSpec.IsGeneric)
			{
				if (type_arguments != null)
				{
					int arity = methodSpec.Arity;
					if (arity != type_arguments.Count)
					{
						return 100000 - System.Math.Abs(type_arguments.Count - arity);
					}
					if (type_arguments.Arguments != null)
					{
						methodSpec = methodSpec.MakeGenericMethod(ec, type_arguments.Arguments);
					}
				}
				else
				{
					if (lambda_conv_msgs == null)
					{
						for (int l = 0; l < arg_count; l++)
						{
							Argument argument2 = arguments[l];
							if (argument2 == null)
							{
								continue;
							}
							AnonymousMethodExpression anonymousMethodExpression = argument2.Expr as AnonymousMethodExpression;
							if (anonymousMethodExpression != null)
							{
								if (lambda_conv_msgs == null)
								{
									lambda_conv_msgs = new SessionReportPrinter();
								}
								anonymousMethodExpression.TypeInferenceReportPrinter = lambda_conv_msgs;
							}
						}
					}
					TypeInference typeInference = new TypeInference(arguments);
					TypeSpec[] array = typeInference.InferMethodArguments(ec, methodSpec);
					if (array == null)
					{
						return 100000 - typeInference.InferenceScore;
					}
					if (lambda_conv_msgs != null)
					{
						lambda_conv_msgs.ClearSession();
					}
					if (array.Length != 0)
					{
						if (!errorMode)
						{
							for (int m = 0; m < array.Length; m++)
							{
								if (!array[m].IsAccessible(ec))
								{
									return 100000 - m;
								}
							}
						}
						methodSpec = methodSpec.MakeGenericMethod(ec, array);
					}
				}
				if (!CheckInflatedArguments(methodSpec))
				{
					candidate = methodSpec;
					return 10000;
				}
				if (candidate != pm)
				{
					MethodSpec methodSpec2 = (MethodSpec)pm;
					returnType = new TypeParameterInflator(ec, methodSpec.DeclaringType, methodSpec2.GenericDefinition.TypeParameters, methodSpec.TypeArguments).Inflate(returnType);
				}
				else
				{
					returnType = methodSpec.ReturnType;
				}
				candidate = methodSpec;
				parameters = methodSpec.Parameters;
				types = parameters.Types;
			}
			else
			{
				if (type_arguments != null)
				{
					return 1000000;
				}
				types = parameters2.Types;
			}
			Parameter.Modifier modifier = Parameter.Modifier.NONE;
			TypeSpec typeSpec = null;
			for (int n = 0; n < arg_count; n++)
			{
				Argument argument3 = arguments[n];
				if (argument3 == null)
				{
					IParameterData parameterData = parameters.FixedParameters[n];
					if (!parameterData.HasDefaultValue)
					{
						arguments = arguments2;
						return arg_count * 2 + 2;
					}
					Expression expression = parameterData.DefaultValue;
					if (expression != null)
					{
						expression = ResolveDefaultValueArgument(ec, types[n], expression, loc);
						if (expression == null)
						{
							for (int num6 = n; num6 < arg_count; num6++)
							{
								arguments.RemoveAt(n);
							}
							return (arg_count - n) * 2 + 1;
						}
					}
					if ((parameterData.ModFlags & Parameter.Modifier.CallerMask) != Parameter.Modifier.NONE)
					{
						if ((parameterData.ModFlags & Parameter.Modifier.CallerLineNumber) != Parameter.Modifier.NONE)
						{
							expression = new IntLiteral(ec.BuiltinTypes, loc.Row, loc);
						}
						else if ((parameterData.ModFlags & Parameter.Modifier.CallerFilePath) != Parameter.Modifier.NONE)
						{
							expression = new StringLiteral(ec.BuiltinTypes, loc.NameFullPath, loc);
						}
						else if (ec.MemberContext.CurrentMemberDefinition != null)
						{
							expression = new StringLiteral(ec.BuiltinTypes, ec.MemberContext.CurrentMemberDefinition.GetCallerMemberName(), loc);
						}
					}
					arguments[n] = new Argument(expression, Argument.AType.Default);
					continue;
				}
				if (modifier != Parameter.Modifier.PARAMS)
				{
					modifier = (parameters.FixedParameters[n].ModFlags & ~Parameter.Modifier.PARAMS) | (parameters2.FixedParameters[n].ModFlags & Parameter.Modifier.PARAMS);
					typeSpec = types[n];
				}
				else if (!params_expanded_form)
				{
					params_expanded_form = true;
					typeSpec = ((ElementTypeSpec)typeSpec).Element;
					n -= 2;
					continue;
				}
				int num7 = 1;
				if (!params_expanded_form)
				{
					if (argument3.IsExtensionType)
					{
						if (ExtensionMethodGroupExpr.IsExtensionTypeCompatible(argument3.Type, typeSpec))
						{
							num7 = 0;
							continue;
						}
					}
					else
					{
						num7 = IsArgumentCompatible(ec, argument3, modifier, typeSpec);
						if (num7 < 0)
						{
							dynamicArgument = true;
						}
					}
				}
				if (num7 != 0 && (modifier & Parameter.Modifier.PARAMS) != Parameter.Modifier.NONE && (restrictions & Restrictions.CovariantDelegate) == 0)
				{
					if (!params_expanded_form)
					{
						typeSpec = ((ElementTypeSpec)typeSpec).Element;
					}
					if (num7 > 0)
					{
						num7 = IsArgumentCompatible(ec, argument3, Parameter.Modifier.NONE, typeSpec);
					}
					if (num7 < 0)
					{
						params_expanded_form = true;
						dynamicArgument = true;
					}
					else if (num7 == 0 || arg_count > parameters.Count)
					{
						params_expanded_form = true;
					}
				}
				if (num7 <= 0)
				{
					continue;
				}
				if (params_expanded_form)
				{
					num7++;
				}
				return (arg_count - n) * 2 + num7;
			}
			if (dynamicArgument)
			{
				arguments = arguments2;
			}
			return 0;
		}

		public static Expression ResolveDefaultValueArgument(ResolveContext ec, TypeSpec ptype, Expression e, Location loc)
		{
			if (e is Constant && e.Type == ptype)
			{
				return e;
			}
			if (e == EmptyExpression.MissingValue && (ptype.BuiltinType == BuiltinTypeSpec.Type.Object || ptype.BuiltinType == BuiltinTypeSpec.Type.Dynamic))
			{
				e = new MemberAccess(new MemberAccess(new MemberAccess(new QualifiedAliasMember(QualifiedAliasMember.GlobalAlias, "System", loc), "Reflection", loc), "Missing", loc), "Value", loc);
			}
			else if (e is Constant)
			{
				e = Convert.ImplicitConversionStandard(ec, e, ptype, loc);
				if (e == null)
				{
					return null;
				}
			}
			else
			{
				e = new DefaultValueExpression(new TypeExpression(ptype, loc), loc);
			}
			return e.Resolve(ec);
		}

		private int IsArgumentCompatible(ResolveContext ec, Argument argument, Parameter.Modifier param_mod, TypeSpec parameter)
		{
			if (((argument.Modifier | param_mod) & Parameter.Modifier.RefOutMask) != Parameter.Modifier.NONE)
			{
				TypeSpec type = argument.Type;
				if ((argument.Modifier & Parameter.Modifier.RefOutMask) != (param_mod & Parameter.Modifier.RefOutMask))
				{
					if (type.BuiltinType == BuiltinTypeSpec.Type.Dynamic && (argument.Modifier & Parameter.Modifier.RefOutMask) == 0 && (restrictions & Restrictions.CovariantDelegate) == 0)
					{
						return -1;
					}
					return 1;
				}
				if (type != parameter)
				{
					if (type == InternalType.VarOutType)
					{
						return 0;
					}
					if (!TypeSpecComparer.IsEqual(type, parameter))
					{
						if (type.BuiltinType == BuiltinTypeSpec.Type.Dynamic && (argument.Modifier & Parameter.Modifier.RefOutMask) == 0 && (restrictions & Restrictions.CovariantDelegate) == 0)
						{
							return -1;
						}
						return 2;
					}
				}
			}
			else
			{
				if (argument.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic && (restrictions & Restrictions.CovariantDelegate) == 0)
				{
					return -1;
				}
				if (!Convert.ImplicitConversionExists(ec, argument.Expr, parameter))
				{
					if (!parameter.IsDelegate || !(argument.Expr is AnonymousMethodExpression))
					{
						return 3;
					}
					return 2;
				}
			}
			return 0;
		}

		private static TypeSpec MoreSpecific(TypeSpec p, TypeSpec q)
		{
			if (TypeManager.IsGenericParameter(p) && !TypeManager.IsGenericParameter(q))
			{
				return q;
			}
			if (!TypeManager.IsGenericParameter(p) && TypeManager.IsGenericParameter(q))
			{
				return p;
			}
			ArrayContainer arrayContainer = p as ArrayContainer;
			if (arrayContainer != null)
			{
				ArrayContainer arrayContainer2 = q as ArrayContainer;
				if (arrayContainer2 == null)
				{
					return null;
				}
				TypeSpec typeSpec = MoreSpecific(arrayContainer.Element, arrayContainer2.Element);
				if (typeSpec == arrayContainer.Element)
				{
					return p;
				}
				if (typeSpec == arrayContainer2.Element)
				{
					return q;
				}
			}
			else if (p.IsGeneric && q.IsGeneric)
			{
				TypeSpec[] typeArguments = TypeManager.GetTypeArguments(p);
				TypeSpec[] typeArguments2 = TypeManager.GetTypeArguments(q);
				bool flag = false;
				bool flag2 = false;
				for (int i = 0; i < typeArguments.Length; i++)
				{
					TypeSpec typeSpec2 = MoreSpecific(typeArguments[i], typeArguments2[i]);
					if (typeSpec2 == typeArguments[i])
					{
						flag = true;
					}
					if (typeSpec2 == typeArguments2[i])
					{
						flag2 = true;
					}
				}
				if (flag && !flag2)
				{
					return p;
				}
				if (!flag & flag2)
				{
					return q;
				}
			}
			return null;
		}

		public T ResolveMember<T>(ResolveContext rc, ref Arguments args) where T : MemberSpec, IParametersMember
		{
			List<AmbiguousCandidate> list = null;
			Arguments args2 = null;
			bool flag = false;
			bool flag2 = false;
			IParametersMember parametersMember = null;
			int arg_count = ((args != null) ? args.Count : 0);
			Arguments arguments = args;
			bool flag3 = false;
			MemberSpec memberSpec = null;
			int num = 0;
			MemberSpec memberSpec2;
			int num2;
			while (true)
			{
				memberSpec2 = null;
				num2 = int.MaxValue;
				IList<MemberSpec> baseMembers = members;
				do
				{
					for (int i = 0; i < baseMembers.Count; i++)
					{
						MemberSpec candidate = baseMembers[i];
						if ((candidate.Modifiers & Modifiers.OVERRIDE) != 0 || (!flag3 && (!candidate.IsAccessible(rc) || (rc.IsRuntimeBinder && !candidate.DeclaringType.IsAccessible(rc)) || ((candidate.Modifiers & (Modifiers.PROTECTED | Modifiers.STATIC)) == Modifiers.PROTECTED && instance_qualifier != null && !instance_qualifier.CheckProtectedMemberAccess(rc, candidate)))))
						{
							continue;
						}
						IParametersMember parametersMember2 = candidate as IParametersMember;
						if (parametersMember2 == null)
						{
							if (Invocation.IsMemberInvocable(candidate))
							{
								memberSpec = candidate;
							}
							continue;
						}
						if ((candidate.Modifiers & (Modifiers.ABSTRACT | Modifiers.VIRTUAL)) != 0)
						{
							IParametersMember overrideMemberParameters = base_provider.GetOverrideMemberParameters(candidate);
							if (overrideMemberParameters != null)
							{
								parametersMember2 = overrideMemberParameters;
							}
						}
						bool params_expanded_form = false;
						bool dynamicArgument = false;
						TypeSpec returnType = parametersMember2.MemberType;
						int num3 = IsApplicable(rc, ref arguments, arg_count, ref candidate, parametersMember2, ref params_expanded_form, ref dynamicArgument, ref returnType, flag3);
						if (lambda_conv_msgs != null)
						{
							lambda_conv_msgs.EndSession();
						}
						if (num3 < num2)
						{
							if (num3 < 0)
							{
								return null;
							}
							num = 1;
							if ((restrictions & Restrictions.GetEnumeratorLookup) == 0 || arguments.Count == 0)
							{
								num2 = num3;
								memberSpec2 = candidate;
								args2 = arguments;
								flag = params_expanded_form;
								flag2 = dynamicArgument;
								parametersMember = parametersMember2;
								best_candidate_return_type = returnType;
							}
						}
						else if (num3 == 0)
						{
							if ((restrictions & Restrictions.BaseMembersIncluded) != Restrictions.None && TypeSpec.IsBaseClass(memberSpec2.DeclaringType, candidate.DeclaringType, true))
							{
								continue;
							}
							num++;
							bool flag4;
							if (memberSpec2.DeclaringType.IsInterface && candidate.DeclaringType.ImplementsInterface(memberSpec2.DeclaringType, false))
							{
								flag4 = true;
								if (list != null)
								{
									foreach (AmbiguousCandidate item in list)
									{
										AmbiguousCandidate ambiguousCandidate2 = item;
										if (!candidate.DeclaringType.ImplementsInterface(memberSpec2.DeclaringType, false))
										{
											flag4 = false;
											break;
										}
									}
									if (flag4)
									{
										list = null;
									}
								}
							}
							else
							{
								flag4 = BetterFunction(rc, arguments, candidate, parametersMember2.Parameters, params_expanded_form, memberSpec2, parametersMember.Parameters, flag);
							}
							if (flag4)
							{
								memberSpec2 = candidate;
								args2 = arguments;
								flag = params_expanded_form;
								flag2 = dynamicArgument;
								parametersMember = parametersMember2;
								best_candidate_return_type = returnType;
							}
							else
							{
								if (list == null)
								{
									list = new List<AmbiguousCandidate>();
								}
								list.Add(new AmbiguousCandidate(candidate, parametersMember2.Parameters, params_expanded_form));
							}
						}
						arguments = args;
					}
				}
				while (num2 != 0 && (baseMembers = base_provider.GetBaseMembers(baseMembers[0].DeclaringType.BaseType)) != null);
				if (num2 == 0)
				{
					break;
				}
				if (!flag3)
				{
					MethodGroupExpr methodGroupExpr = base_provider.LookupExtensionMethod(rc);
					if (methodGroupExpr != null)
					{
						methodGroupExpr = methodGroupExpr.OverloadResolve(rc, ref args, null, restrictions);
						if (methodGroupExpr != null)
						{
							best_candidate_extension_group = methodGroupExpr;
							return (T)(MemberSpec)methodGroupExpr.BestCandidate;
						}
					}
				}
				if (IsProbingOnly)
				{
					return null;
				}
				if (flag3 || (lambda_conv_msgs != null && !lambda_conv_msgs.IsEmpty))
				{
					break;
				}
				lambda_conv_msgs = null;
				flag3 = true;
			}
			if ((num2 != 0) | flag3)
			{
				ReportOverloadError(rc, memberSpec2, parametersMember, args2, flag);
				return null;
			}
			if (flag2)
			{
				if (args[0].IsExtensionType)
				{
					rc.Report.Error(1973, loc, "Type `{0}' does not contain a member `{1}' and the best extension method overload `{2}' cannot be dynamically dispatched. Consider calling the method without the extension method syntax", args[0].Type.GetSignatureForError(), memberSpec2.Name, memberSpec2.GetSignatureForError());
				}
				if (num == 1 && memberSpec2.IsGeneric && type_arguments != null)
				{
					MethodSpec methodSpec = memberSpec2 as MethodSpec;
					if (methodSpec != null && TypeParameterSpec.HasAnyTypeParameterConstrained(methodSpec.GenericDefinition))
					{
						new ConstraintChecker(rc).CheckAll(methodSpec.GetGenericMethodDefinition(), methodSpec.TypeArguments, methodSpec.Constraints, loc);
					}
				}
				BestCandidateIsDynamic = true;
				return null;
			}
			if ((restrictions & (Restrictions.ProbingOnly | Restrictions.CovariantDelegate)) == (Restrictions.ProbingOnly | Restrictions.CovariantDelegate))
			{
				return (T)memberSpec2;
			}
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					AmbiguousCandidate ambiguousCandidate = list[j];
					if (!BetterFunction(rc, args2, memberSpec2, parametersMember.Parameters, flag, ambiguousCandidate.Member, ambiguousCandidate.Parameters, ambiguousCandidate.Expanded))
					{
						MemberSpec member = ambiguousCandidate.Member;
						if (custom_errors == null || !custom_errors.AmbiguousCandidates(rc, memberSpec2, member))
						{
							rc.Report.SymbolRelatedToPreviousError(memberSpec2);
							rc.Report.SymbolRelatedToPreviousError(member);
							rc.Report.Error(121, loc, "The call is ambiguous between the following methods or properties: `{0}' and `{1}'", memberSpec2.GetSignatureForError(), member.GetSignatureForError());
						}
						return (T)memberSpec2;
					}
				}
			}
			if (memberSpec != null && !IsProbingOnly)
			{
				rc.Report.SymbolRelatedToPreviousError(memberSpec2);
				rc.Report.SymbolRelatedToPreviousError(memberSpec);
				rc.Report.Warning(467, 2, loc, "Ambiguity between method `{0}' and invocable non-method `{1}'. Using method group", memberSpec2.GetSignatureForError(), memberSpec.GetSignatureForError());
			}
			if (!VerifyArguments(rc, ref args2, memberSpec2, parametersMember, flag))
			{
				return null;
			}
			if (memberSpec2 == null)
			{
				return null;
			}
			if (!IsProbingOnly && !rc.IsInProbingMode)
			{
				ObsoleteAttribute attributeObsolete = memberSpec2.GetAttributeObsolete();
				if (attributeObsolete != null && !rc.IsObsolete)
				{
					AttributeTester.Report_ObsoleteMessage(attributeObsolete, memberSpec2.GetSignatureForError(), loc, rc.Report);
				}
				memberSpec2.MemberDefinition.SetIsUsed();
			}
			args = args2;
			return (T)memberSpec2;
		}

		public MethodSpec ResolveOperator(ResolveContext rc, ref Arguments args)
		{
			return ResolveMember<MethodSpec>(rc, ref args);
		}

		private void ReportArgumentMismatch(ResolveContext ec, int idx, MemberSpec method, Argument a, AParametersCollection expected_par, TypeSpec paramType)
		{
			if ((custom_errors != null && custom_errors.ArgumentMismatch(ec, method, a, idx)) || a.Type == InternalType.ErrorType)
			{
				return;
			}
			if (a is CollectionElementInitializer.ElementInitializerArgument)
			{
				ec.Report.SymbolRelatedToPreviousError(method);
				if ((expected_par.FixedParameters[idx].ModFlags & Parameter.Modifier.RefOutMask) != Parameter.Modifier.NONE)
				{
					ec.Report.Error(1954, loc, "The best overloaded collection initalizer method `{0}' cannot have `ref' or `out' modifier", TypeManager.CSharpSignature(method));
					return;
				}
				ec.Report.Error(1950, loc, "The best overloaded collection initalizer method `{0}' has some invalid arguments", TypeManager.CSharpSignature(method));
			}
			else if (IsDelegateInvoke)
			{
				ec.Report.Error(1594, loc, "Delegate `{0}' has some invalid arguments", DelegateType.GetSignatureForError());
			}
			else
			{
				ec.Report.SymbolRelatedToPreviousError(method);
				ec.Report.Error(1502, loc, "The best overloaded method match for `{0}' has some invalid arguments", method.GetSignatureForError());
			}
			Parameter.Modifier modifier = ((idx < expected_par.Count) ? expected_par.FixedParameters[idx].ModFlags : Parameter.Modifier.NONE);
			string text = (idx + 1).ToString();
			if (((modifier & Parameter.Modifier.RefOutMask) ^ (a.Modifier & Parameter.Modifier.RefOutMask)) != Parameter.Modifier.NONE)
			{
				if ((modifier & Parameter.Modifier.RefOutMask) == 0)
				{
					ec.Report.Error(1615, a.Expr.Location, "Argument `#{0}' does not require `{1}' modifier. Consider removing `{1}' modifier", text, Parameter.GetModifierSignature(a.Modifier));
				}
				else
				{
					ec.Report.Error(1620, a.Expr.Location, "Argument `#{0}' is missing `{1}' modifier", text, Parameter.GetModifierSignature(modifier));
				}
				return;
			}
			string text2 = a.GetSignatureForError();
			string text3 = paramType.GetSignatureForError();
			if (text2 == text3)
			{
				text2 = a.Type.GetSignatureForErrorIncludingAssemblyName();
				text3 = paramType.GetSignatureForErrorIncludingAssemblyName();
			}
			if ((modifier & Parameter.Modifier.RefOutMask) != Parameter.Modifier.NONE)
			{
				text2 = Parameter.GetModifierSignature(a.Modifier) + " " + text2;
				text3 = Parameter.GetModifierSignature(a.Modifier) + " " + text3;
			}
			ec.Report.Error(1503, a.Expr.Location, "Argument `#{0}' cannot convert `{1}' expression to type `{2}'", text, text2, text3);
		}

		private void ReportOverloadError(ResolveContext rc, MemberSpec best_candidate, IParametersMember pm, Arguments args, bool params_expanded)
		{
			int num = ((type_arguments != null) ? type_arguments.Count : 0);
			int num2 = args?.Count ?? 0;
			if (num != best_candidate.Arity && (num > 0 || ((IParametersMember)best_candidate).Parameters.IsEmpty))
			{
				new MethodGroupExpr(new MemberSpec[1] { best_candidate }, best_candidate.DeclaringType, loc).Error_TypeArgumentsCannotBeUsed(rc, best_candidate, loc);
			}
			else
			{
				if (lambda_conv_msgs != null && lambda_conv_msgs.Merge(rc.Report.Printer))
				{
					return;
				}
				if ((best_candidate.Modifiers & (Modifiers.PROTECTED | Modifiers.STATIC)) == Modifiers.PROTECTED && InstanceQualifier != null && !InstanceQualifier.CheckProtectedMemberAccess(rc, best_candidate))
				{
					MemberExpr.Error_ProtectedMemberAccess(rc, best_candidate, InstanceQualifier.InstanceType, loc);
				}
				if (pm != null && (((pm.Parameters.Count == num2) | params_expanded) || HasUnfilledParams(best_candidate, pm, args)))
				{
					if (!best_candidate.IsAccessible(rc) || !best_candidate.DeclaringType.IsAccessible(rc))
					{
						rc.Report.SymbolRelatedToPreviousError(best_candidate);
						Expression.ErrorIsInaccesible(rc, best_candidate.GetSignatureForError(), loc);
						return;
					}
					MethodSpec methodSpec = best_candidate as MethodSpec;
					if (methodSpec != null && methodSpec.IsGeneric)
					{
						bool flag = true;
						if (methodSpec.TypeArguments != null)
						{
							flag = new ConstraintChecker(rc.MemberContext).CheckAll(methodSpec.GetGenericMethodDefinition(), methodSpec.TypeArguments, methodSpec.Constraints, loc);
						}
						if (num == 0 && methodSpec.TypeArguments == null)
						{
							if ((custom_errors == null || !custom_errors.TypeInferenceFailed(rc, best_candidate)) && flag)
							{
								rc.Report.Error(411, loc, "The type arguments for method `{0}' cannot be inferred from the usage. Try specifying the type arguments explicitly", methodSpec.GetGenericMethodDefinition().GetSignatureForError());
							}
							return;
						}
					}
					VerifyArguments(rc, ref args, best_candidate, pm, params_expanded);
				}
				else if (custom_errors == null || !custom_errors.NoArgumentMatch(rc, best_candidate))
				{
					if (best_candidate.Kind == MemberKind.Constructor)
					{
						rc.Report.SymbolRelatedToPreviousError(best_candidate);
						Error_ConstructorMismatch(rc, best_candidate.DeclaringType, num2, loc);
					}
					else if (IsDelegateInvoke)
					{
						rc.Report.SymbolRelatedToPreviousError(DelegateType);
						rc.Report.Error(1593, loc, "Delegate `{0}' does not take `{1}' arguments", DelegateType.GetSignatureForError(), num2.ToString());
					}
					else
					{
						string arg = ((best_candidate.Kind == MemberKind.Indexer) ? "this" : best_candidate.Name);
						rc.Report.SymbolRelatedToPreviousError(best_candidate);
						rc.Report.Error(1501, loc, "No overload for method `{0}' takes `{1}' arguments", arg, num2.ToString());
					}
				}
			}
		}

		private static bool HasUnfilledParams(MemberSpec best_candidate, IParametersMember pm, Arguments args)
		{
			AParametersCollection parameters = ((IParametersMember)best_candidate).Parameters;
			if (!parameters.HasParams)
			{
				return false;
			}
			string text = null;
			for (int num = parameters.Count - 1; num != 0; num--)
			{
				IParameterData parameterData = parameters.FixedParameters[num];
				if ((parameterData.ModFlags & Parameter.Modifier.PARAMS) != Parameter.Modifier.NONE)
				{
					text = parameterData.Name;
					break;
				}
			}
			if (args == null)
			{
				return false;
			}
			foreach (Argument arg in args)
			{
				NamedArgument namedArgument = arg as NamedArgument;
				if (namedArgument != null && namedArgument.Name == text)
				{
					text = null;
					break;
				}
			}
			if (text == null)
			{
				return false;
			}
			return args.Count + 1 == pm.Parameters.Count;
		}

		private bool VerifyArguments(ResolveContext ec, ref Arguments args, MemberSpec member, IParametersMember pm, bool chose_params_expanded)
		{
			AParametersCollection parameters = pm.Parameters;
			AParametersCollection parameters2 = ((IParametersMember)member).Parameters;
			TypeSpec[] types = parameters2.Types;
			Parameter.Modifier modifier = Parameter.Modifier.NONE;
			TypeSpec typeSpec = null;
			int i = 0;
			int j = 0;
			Argument argument = null;
			ArrayInitializer arrayInitializer = null;
			bool flag = pm.MemberType.IsPointer;
			int num;
			for (num = ((args != null) ? args.Count : 0); i < num; i++, j++)
			{
				argument = args[i];
				if (argument == null)
				{
					continue;
				}
				if (modifier != Parameter.Modifier.PARAMS)
				{
					modifier = parameters2.FixedParameters[i].ModFlags;
					typeSpec = types[i];
					flag |= typeSpec.IsPointer;
					if (modifier == Parameter.Modifier.PARAMS && chose_params_expanded)
					{
						arrayInitializer = new ArrayInitializer(num - i, argument.Expr.Location);
						typeSpec = TypeManager.GetElementType(typeSpec);
					}
				}
				if (((argument.Modifier | modifier) & Parameter.Modifier.RefOutMask) != Parameter.Modifier.NONE)
				{
					if ((argument.Modifier & Parameter.Modifier.RefOutMask) != (modifier & Parameter.Modifier.RefOutMask))
					{
						break;
					}
					TypeSpec type = argument.Type;
					if (type == typeSpec)
					{
						continue;
					}
					if (type == InternalType.VarOutType)
					{
						((DeclarationExpression)argument.Expr).Variable.Type = typeSpec;
						continue;
					}
					if (!TypeSpecComparer.IsEqual(type, typeSpec))
					{
						break;
					}
				}
				NamedArgument namedArgument = argument as NamedArgument;
				if (namedArgument != null)
				{
					int parameterIndexByName = parameters.GetParameterIndexByName(namedArgument.Name);
					if (parameterIndexByName < 0 || parameterIndexByName >= parameters.Count)
					{
						if (IsDelegateInvoke)
						{
							ec.Report.SymbolRelatedToPreviousError(DelegateType);
							ec.Report.Error(1746, namedArgument.Location, "The delegate `{0}' does not contain a parameter named `{1}'", DelegateType.GetSignatureForError(), namedArgument.Name);
						}
						else
						{
							ec.Report.SymbolRelatedToPreviousError(member);
							ec.Report.Error(1739, namedArgument.Location, "The best overloaded method match for `{0}' does not contain a parameter named `{1}'", TypeManager.CSharpSignature(member), namedArgument.Name);
						}
					}
					else if (args[parameterIndexByName] != argument && args[parameterIndexByName] != null)
					{
						if (IsDelegateInvoke)
						{
							ec.Report.SymbolRelatedToPreviousError(DelegateType);
						}
						else
						{
							ec.Report.SymbolRelatedToPreviousError(member);
						}
						ec.Report.Error(1744, namedArgument.Location, "Named argument `{0}' cannot be used for a parameter which has positional argument specified", namedArgument.Name);
					}
				}
				if (argument.Expr.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					continue;
				}
				if ((restrictions & Restrictions.CovariantDelegate) != Restrictions.None && !Delegate.IsTypeCovariant(ec, argument.Expr.Type, typeSpec))
				{
					if (argument.IsExtensionType)
					{
						MemberAccess memberAccess = new MemberAccess(argument.Expr, member.Name, loc);
						memberAccess.Error_TypeDoesNotContainDefinition(ec, argument.Expr.Type, memberAccess.Name);
					}
					else
					{
						custom_errors.NoArgumentMatch(ec, member);
					}
					return false;
				}
				Expression expression;
				if (argument.IsExtensionType)
				{
					if (argument.Expr.Type == typeSpec || TypeSpecComparer.IsEqual(argument.Expr.Type, typeSpec))
					{
						expression = argument.Expr;
					}
					else
					{
						expression = Convert.ImplicitReferenceConversion(argument.Expr, typeSpec, false);
						if (expression == null)
						{
							expression = Convert.ImplicitBoxingConversion(argument.Expr, argument.Expr.Type, typeSpec);
						}
					}
				}
				else
				{
					expression = Convert.ImplicitConversion(ec, argument.Expr, typeSpec, loc);
				}
				if (expression == null)
				{
					break;
				}
				if (arrayInitializer != null)
				{
					arrayInitializer.Add(argument.Expr);
					args.RemoveAt(i--);
					num--;
					argument.Expr = expression;
				}
				else
				{
					argument.Expr = expression;
				}
			}
			if (i != num)
			{
				for (; i < num; i++)
				{
					Argument argument2 = args[i];
					if (argument2 != null && argument2.Type == InternalType.VarOutType)
					{
						((DeclarationExpression)argument2.Expr).Variable.Type = InternalType.ErrorType;
					}
				}
				ReportArgumentMismatch(ec, j, member, argument, parameters, typeSpec);
				return false;
			}
			if (arrayInitializer == null && num + 1 == parameters.Count)
			{
				if (args == null)
				{
					args = new Arguments(1);
				}
				typeSpec = types[parameters.Count - 1];
				typeSpec = TypeManager.GetElementType(typeSpec);
				flag |= typeSpec.IsPointer;
				arrayInitializer = new ArrayInitializer(0, loc);
			}
			if (arrayInitializer != null)
			{
				args.Add(new Argument(new ArrayCreation(new TypeExpression(typeSpec, loc), arrayInitializer, loc).Resolve(ec)));
				num++;
			}
			if (flag && !ec.IsUnsafe)
			{
				Expression.UnsafeError(ec, loc);
			}
			if (type_arguments == null && member.IsGeneric)
			{
				TypeSpec[] typeArguments = ((MethodSpec)member).TypeArguments;
				foreach (TypeSpec typeSpec2 in typeArguments)
				{
					if (!typeSpec2.IsAccessible(ec))
					{
						ec.Report.SymbolRelatedToPreviousError(typeSpec2);
						Expression.ErrorIsInaccesible(ec, member.GetSignatureForError(), loc);
						break;
					}
				}
			}
			return true;
		}
	}
}
