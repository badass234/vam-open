using System;
using System.Reflection.Emit;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class Is : Probe
	{
		private Unwrap expr_unwrap;

		private MethodSpec number_mg;

		private Arguments number_args;

		protected override string OperatorName => "is";

		public LocalVariable Variable { get; set; }

		public Is(Expression expr, Expression probe_type, Location l)
			: base(expr, probe_type, l)
		{
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			if (Variable != null)
			{
				throw new NotSupportedException();
			}
			Arguments args = Arguments.CreateForExpressionTree(ec, null, expr.CreateExpressionTree(ec), new TypeOf(probe_type_expr, loc));
			return CreateExpressionFactoryCall(ec, "TypeIs", args);
		}

		private Expression CreateConstantResult(ResolveContext rc, bool result)
		{
			if (result)
			{
				rc.Report.Warning(183, 1, loc, "The given expression is always of the provided (`{0}') type", probe_type_expr.GetSignatureForError());
			}
			else
			{
				rc.Report.Warning(184, 1, loc, "The given expression is never of the provided (`{0}') type", probe_type_expr.GetSignatureForError());
			}
			BoolConstant value = new BoolConstant(rc.BuiltinTypes, result, loc);
			if (!expr.IsSideEffectFree)
			{
				return new SideEffectConstant(value, this, loc);
			}
			return ReducedExpression.Create(value, this);
		}

		public override void Emit(EmitContext ec)
		{
			if (probe_type_expr == null)
			{
				if (ProbeType is WildcardPattern)
				{
					expr.EmitSideEffect(ec);
					ProbeType.Emit(ec);
				}
				else
				{
					EmitPatternMatch(ec);
				}
				return;
			}
			EmitLoad(ec);
			if (expr_unwrap == null)
			{
				ec.EmitNull();
				ec.Emit(OpCodes.Cgt_Un);
			}
		}

		public override void EmitBranchable(EmitContext ec, Label target, bool on_true)
		{
			if (probe_type_expr == null)
			{
				EmitPatternMatch(ec);
			}
			else
			{
				EmitLoad(ec);
			}
			ec.Emit(on_true ? OpCodes.Brtrue : OpCodes.Brfalse, target);
		}

		private void EmitPatternMatch(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			Label label2 = ec.DefineLabel();
			if (expr_unwrap != null)
			{
				expr_unwrap.EmitCheck(ec);
				if (ProbeType.IsNull)
				{
					ec.EmitInt(0);
					ec.Emit(OpCodes.Ceq);
					return;
				}
				ec.Emit(OpCodes.Brfalse_S, label);
				expr_unwrap.Emit(ec);
				ProbeType.Emit(ec);
				ec.Emit(OpCodes.Ceq);
				ec.Emit(OpCodes.Br_S, label2);
				ec.MarkLabel(label);
				ec.EmitInt(0);
				ec.MarkLabel(label2);
				return;
			}
			if (number_args != null && number_args.Count == 3)
			{
				default(CallEmitter).Emit(ec, number_mg, number_args, loc);
				return;
			}
			TypeSpec t = ProbeType.Type;
			base.Expr.Emit(ec);
			ec.Emit(OpCodes.Isinst, t);
			ec.Emit(OpCodes.Dup);
			ec.Emit(OpCodes.Brfalse, label);
			bool flag = ProbeType is ComplexPatternExpression;
			Label recursivePatternLabel = ec.RecursivePatternLabel;
			if (flag)
			{
				ec.RecursivePatternLabel = ec.DefineLabel();
			}
			if (number_mg != null)
			{
				default(CallEmitter).Emit(ec, number_mg, number_args, loc);
			}
			else
			{
				if (TypeSpec.IsValueType(t))
				{
					ec.Emit(OpCodes.Unbox_Any, t);
				}
				ProbeType.Emit(ec);
				if (flag)
				{
					ec.EmitInt(1);
				}
				else
				{
					ec.Emit(OpCodes.Ceq);
				}
			}
			ec.Emit(OpCodes.Br_S, label2);
			ec.MarkLabel(label);
			ec.Emit(OpCodes.Pop);
			if (flag)
			{
				ec.MarkLabel(ec.RecursivePatternLabel);
			}
			ec.RecursivePatternLabel = recursivePatternLabel;
			ec.EmitInt(0);
			ec.MarkLabel(label2);
		}

		private void EmitLoad(EmitContext ec)
		{
			Label label = default(Label);
			if (expr_unwrap != null)
			{
				expr_unwrap.EmitCheck(ec);
				if (Variable == null)
				{
					return;
				}
				ec.Emit(OpCodes.Dup);
				label = ec.DefineLabel();
				ec.Emit(OpCodes.Brfalse_S, label);
				expr_unwrap.Emit(ec);
			}
			else
			{
				expr.Emit(ec);
				if (probe_type_expr.IsGenericParameter && TypeSpec.IsValueType(expr.Type))
				{
					ec.Emit(OpCodes.Box, expr.Type);
				}
				ec.Emit(OpCodes.Isinst, probe_type_expr);
			}
			if (Variable != null)
			{
				bool flag;
				if (probe_type_expr.IsGenericParameter || probe_type_expr.IsNullableType)
				{
					ec.Emit(OpCodes.Dup);
					ec.Emit(OpCodes.Unbox_Any, probe_type_expr);
					flag = true;
				}
				else
				{
					flag = false;
				}
				Variable.CreateBuilder(ec);
				Variable.EmitAssign(ec);
				if (expr_unwrap != null)
				{
					ec.MarkLabel(label);
				}
				else if (!flag)
				{
					Variable.Emit(ec);
				}
			}
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			if (ResolveCommon(rc) == null)
			{
				return null;
			}
			type = rc.BuiltinTypes.Bool;
			eclass = ExprClass.Value;
			if (probe_type_expr == null)
			{
				return ResolveMatchingExpression(rc);
			}
			Expression expression = ResolveResultExpression(rc);
			if (Variable != null)
			{
				if (expression is Constant)
				{
					throw new NotImplementedException("constant in type pattern matching");
				}
				Variable.Type = probe_type_expr;
				BlockContext blockContext = rc as BlockContext;
				if (blockContext != null)
				{
					Variable.PrepareAssignmentAnalysis(blockContext);
				}
			}
			return expression;
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			base.FlowAnalysis(fc);
			if (Variable != null)
			{
				fc.SetVariableAssigned(Variable.VariableInfo, true);
			}
		}

		protected override void ResolveProbeType(ResolveContext rc)
		{
			if (!(ProbeType is TypeExpr) && rc.Module.Compiler.Settings.Version == LanguageVersion.Experimental)
			{
				if (ProbeType is PatternExpression)
				{
					ProbeType.Resolve(rc);
					return;
				}
				SessionReportPrinter sessionReportPrinter = new SessionReportPrinter();
				ReportPrinter printer = rc.Report.SetPrinter(sessionReportPrinter);
				probe_type_expr = ProbeType.ResolveAsType(rc);
				sessionReportPrinter.EndSession();
				if (probe_type_expr != null)
				{
					sessionReportPrinter.Merge(rc.Report.Printer);
					rc.Report.SetPrinter(printer);
					return;
				}
				VarExpr varExpr = ProbeType as VarExpr;
				if (varExpr != null && varExpr.InferType(rc, expr))
				{
					probe_type_expr = varExpr.Type;
					rc.Report.SetPrinter(printer);
					return;
				}
				SessionReportPrinter sessionReportPrinter2 = new SessionReportPrinter();
				rc.Report.SetPrinter(sessionReportPrinter2);
				ProbeType = ProbeType.Resolve(rc);
				sessionReportPrinter2.EndSession();
				if (ProbeType != null)
				{
					sessionReportPrinter2.Merge(rc.Report.Printer);
				}
				else
				{
					sessionReportPrinter.Merge(rc.Report.Printer);
				}
				rc.Report.SetPrinter(printer);
			}
			else
			{
				base.ResolveProbeType(rc);
			}
		}

		private Expression ResolveMatchingExpression(ResolveContext rc)
		{
			Constant constant = ProbeType as Constant;
			if (constant != null)
			{
				if (!Convert.ImplicitConversionExists(rc, ProbeType, base.Expr.Type))
				{
					ProbeType.Error_ValueCannotBeConverted(rc, base.Expr.Type, false);
					return null;
				}
				if (constant.IsNull)
				{
					return new Binary(Binary.Operator.Equality, base.Expr, constant).Resolve(rc);
				}
				Constant constant2 = base.Expr as Constant;
				if (constant2 != null)
				{
					constant2 = ConstantFold.BinaryFold(rc, Binary.Operator.Equality, constant2, constant, loc);
					if (constant2 != null)
					{
						return constant2;
					}
				}
				if (base.Expr.Type.IsNullableType)
				{
					expr_unwrap = new Unwrap(base.Expr);
					expr_unwrap.Resolve(rc);
					ProbeType = Convert.ImplicitConversion(rc, ProbeType, expr_unwrap.Type, loc);
				}
				else
				{
					if (ProbeType.Type == base.Expr.Type)
					{
						return new Binary(Binary.Operator.Equality, base.Expr, constant, loc).Resolve(rc);
					}
					if (ProbeType.Type.IsEnum || (ProbeType.Type.BuiltinType >= BuiltinTypeSpec.Type.Byte && ProbeType.Type.BuiltinType <= BuiltinTypeSpec.Type.Decimal))
					{
						ModuleContainer.PatternMatchingHelper patternMatchingHelper = rc.Module.CreatePatterMatchingHelper();
						number_mg = patternMatchingHelper.NumberMatcher.Spec;
						number_args = new Arguments(3);
						if (!ProbeType.Type.IsEnum)
						{
							number_args.Add(new Argument(base.Expr));
						}
						number_args.Add(new Argument(Convert.ImplicitConversion(rc, ProbeType, rc.BuiltinTypes.Object, loc)));
						number_args.Add(new Argument(new BoolLiteral(rc.BuiltinTypes, ProbeType.Type.IsEnum, loc)));
					}
				}
				return this;
			}
			if (ProbeType is PatternExpression)
			{
				if (!(ProbeType is WildcardPattern) && !Convert.ImplicitConversionExists(rc, ProbeType, base.Expr.Type))
				{
					ProbeType.Error_ValueCannotBeConverted(rc, base.Expr.Type, false);
				}
				return this;
			}
			rc.Report.Error(150, ProbeType.Location, "A constant value is expected");
			return this;
		}

		private Expression ResolveResultExpression(ResolveContext ec)
		{
			TypeSpec typeSpec = expr.Type;
			bool flag = false;
			if (expr.IsNull || expr.eclass == ExprClass.MethodGroup)
			{
				return CreateConstantResult(ec, false);
			}
			if (typeSpec.IsNullableType)
			{
				TypeSpec underlyingType = NullableInfo.GetUnderlyingType(typeSpec);
				if (!underlyingType.IsGenericParameter)
				{
					typeSpec = underlyingType;
					flag = true;
				}
			}
			TypeSpec typeSpec2 = probe_type_expr;
			bool flag2 = false;
			if (typeSpec2.IsNullableType)
			{
				TypeSpec underlyingType2 = NullableInfo.GetUnderlyingType(typeSpec2);
				if (!underlyingType2.IsGenericParameter)
				{
					typeSpec2 = underlyingType2;
					flag2 = true;
				}
			}
			if (typeSpec2.IsStruct)
			{
				if (typeSpec == typeSpec2)
				{
					if (flag && !flag2)
					{
						expr_unwrap = Unwrap.Create(expr, true);
						return this;
					}
					return CreateConstantResult(ec, true);
				}
				TypeParameterSpec typeParameterSpec = typeSpec as TypeParameterSpec;
				if (typeParameterSpec != null)
				{
					return ResolveGenericParameter(ec, typeSpec2, typeParameterSpec);
				}
				if (Convert.ExplicitReferenceConversionExists(typeSpec, typeSpec2))
				{
					return this;
				}
				if (typeSpec is InflatedTypeSpec && InflatedTypeSpec.ContainsTypeParameter(typeSpec))
				{
					return this;
				}
			}
			else
			{
				TypeParameterSpec typeParameterSpec2 = typeSpec2 as TypeParameterSpec;
				if (typeParameterSpec2 != null)
				{
					return ResolveGenericParameter(ec, typeSpec, typeParameterSpec2);
				}
				if (typeSpec2.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					ec.Report.Warning(1981, 3, loc, "Using `{0}' to test compatibility with `{1}' is identical to testing compatibility with `object'", OperatorName, typeSpec2.GetSignatureForError());
				}
				if (TypeManager.IsGenericParameter(typeSpec))
				{
					return ResolveGenericParameter(ec, typeSpec2, (TypeParameterSpec)typeSpec);
				}
				if (TypeSpec.IsValueType(typeSpec))
				{
					if (Convert.ImplicitBoxingConversion(null, typeSpec, typeSpec2) != null)
					{
						if (flag && !flag2)
						{
							expr_unwrap = Unwrap.Create(expr, false);
							return this;
						}
						return CreateConstantResult(ec, true);
					}
				}
				else
				{
					if (Convert.ImplicitReferenceConversionExists(typeSpec, typeSpec2))
					{
						Constant constant = expr as Constant;
						if (constant != null)
						{
							return CreateConstantResult(ec, !constant.IsNull);
						}
						if (typeSpec.MemberDefinition.IsImported && typeSpec.BuiltinType != BuiltinTypeSpec.Type.None && typeSpec.MemberDefinition.DeclaringAssembly != typeSpec2.MemberDefinition.DeclaringAssembly)
						{
							return this;
						}
						if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
						{
							return this;
						}
						return ReducedExpression.Create(new Binary(Binary.Operator.Inequality, expr, new NullLiteral(loc), Binary.State.UserOperatorsExcluded).Resolve(ec), this).Resolve(ec);
					}
					if (Convert.ExplicitReferenceConversionExists(typeSpec, typeSpec2))
					{
						return this;
					}
					if ((typeSpec is InflatedTypeSpec || typeSpec.IsArray) && InflatedTypeSpec.ContainsTypeParameter(typeSpec))
					{
						return this;
					}
				}
			}
			return CreateConstantResult(ec, false);
		}

		private Expression ResolveGenericParameter(ResolveContext ec, TypeSpec d, TypeParameterSpec t)
		{
			if (t.IsReferenceType && d.IsStruct)
			{
				return CreateConstantResult(ec, false);
			}
			if (expr.Type.IsGenericParameter)
			{
				if (expr.Type == d && TypeSpec.IsValueType(t) && TypeSpec.IsValueType(d))
				{
					return CreateConstantResult(ec, true);
				}
				expr = new BoxedCast(expr, d);
			}
			return this;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
