using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal class RecursivePattern : ComplexPatternExpression
	{
		private MethodGroupExpr operator_mg;

		private Arguments operator_args;

		public Arguments Arguments { get; private set; }

		public RecursivePattern(ATypeNameExpression typeExpresion, Arguments arguments, Location loc)
			: base(typeExpresion, loc)
		{
			Arguments = arguments;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			type = base.TypeExpression.ResolveAsType(rc);
			if (type == null)
			{
				return null;
			}
			IList<MemberSpec> userOperator = MemberCache.GetUserOperator(type, Operator.OpType.Is, true);
			if (userOperator == null)
			{
				Error_TypeDoesNotContainDefinition(rc, type, Operator.GetName(Operator.OpType.Is) + " operator");
				return null;
			}
			List<MethodSpec> list = FindMatchingOverloads(userOperator);
			if (list == null)
			{
				Error_TypeDoesNotContainDefinition(rc, type, Operator.GetName(Operator.OpType.Is) + " operator");
				return null;
			}
			bool dynamic;
			Arguments.Resolve(rc, out dynamic);
			if (dynamic)
			{
				throw new NotImplementedException("dynamic argument");
			}
			MethodSpec methodSpec = FindBestOverload(rc, list);
			if (methodSpec == null)
			{
				Error_TypeDoesNotContainDefinition(rc, type, Operator.GetName(Operator.OpType.Is) + " operator");
				return null;
			}
			TypeSpec[] types = methodSpec.Parameters.Types;
			operator_args = new Arguments(types.Length);
			operator_args.Add(new Argument(new EmptyExpression(type)));
			for (int i = 0; i < Arguments.Count; i++)
			{
				LocalTemporary localTemporary = new LocalTemporary(types[i + 1]);
				operator_args.Add(new Argument(localTemporary, Argument.AType.Out));
				if (comparisons == null)
				{
					comparisons = new Expression[Arguments.Count];
				}
				Argument argument = Arguments[i];
				NamedArgument namedArgument = argument as NamedArgument;
				int num;
				Expression expr;
				if (namedArgument != null)
				{
					num = methodSpec.Parameters.GetParameterIndexByName(namedArgument.Name) - 1;
					expr = Arguments[num].Expr;
				}
				else
				{
					num = i;
					expr = argument.Expr;
				}
				comparisons[num] = ResolveComparison(rc, expr, localTemporary);
			}
			operator_mg = MethodGroupExpr.CreatePredefined(methodSpec, type, loc);
			eclass = ExprClass.Value;
			return this;
		}

		private List<MethodSpec> FindMatchingOverloads(IList<MemberSpec> members)
		{
			int num = Arguments.Count + 1;
			List<MethodSpec> list = null;
			foreach (MethodSpec member in members)
			{
				AParametersCollection parameters = member.Parameters;
				if (parameters.Count != num)
				{
					continue;
				}
				bool flag = true;
				for (int i = 1; i < parameters.Count; i++)
				{
					if ((parameters.FixedParameters[i].ModFlags & Parameter.Modifier.OUT) == 0)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					if (list == null)
					{
						list = new List<MethodSpec>();
					}
					list.Add(member);
				}
			}
			return list;
		}

		private MethodSpec FindBestOverload(ResolveContext rc, List<MethodSpec> methods)
		{
			for (int i = 0; i < Arguments.Count; i++)
			{
				Argument argument = Arguments[i];
				Expression expr = argument.Expr;
				if (expr is WildcardPattern)
				{
					continue;
				}
				NamedArgument namedArgument = argument as NamedArgument;
				for (int j = 0; j < methods.Count; j++)
				{
					AParametersCollection parameters = methods[j].Parameters;
					int num;
					if (namedArgument != null)
					{
						num = parameters.GetParameterIndexByName(namedArgument.Name);
						if (num < 1)
						{
							methods.RemoveAt(j--);
							continue;
						}
					}
					else
					{
						num = i + 1;
					}
					TypeSpec target_type = parameters.Types[num];
					if (!Convert.ImplicitConversionExists(rc, expr, target_type))
					{
						methods.RemoveAt(j--);
					}
				}
			}
			if (methods.Count != 1)
			{
				return null;
			}
			return methods[0];
		}

		public override void EmitBranchable(EmitContext ec, Label target, bool on_true)
		{
			operator_mg.EmitCall(ec, operator_args, false);
			ec.Emit(OpCodes.Brfalse, target);
			base.EmitBranchable(ec, target, on_true);
		}

		private static Expression ResolveComparison(ResolveContext rc, Expression expr, LocalTemporary lt)
		{
			if (expr is WildcardPattern)
			{
				return new EmptyExpression(expr.Type);
			}
			RecursivePattern recursivePattern = expr as RecursivePattern;
			expr = Convert.ImplicitConversionRequired(rc, expr, lt.Type, expr.Location);
			if (expr == null)
			{
				return null;
			}
			if (recursivePattern != null)
			{
				recursivePattern.SetParentInstance(lt);
				return expr;
			}
			Expression expression = expr;
			return new Binary(Binary.Operator.Equality, lt, expression, expression.Location).Resolve(rc);
		}

		public void SetParentInstance(Expression instance)
		{
			operator_args[0] = new Argument(instance);
		}
	}
}
