using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Mono.CSharp
{
	public class StringConcat : Expression
	{
		private Arguments arguments;

		private StringConcat(Location loc)
		{
			base.loc = loc;
			arguments = new Arguments(2);
		}

		public override bool ContainsEmitWithAwait()
		{
			return arguments.ContainsEmitWithAwait();
		}

		public static StringConcat Create(ResolveContext rc, Expression left, Expression right, Location loc)
		{
			if (left.eclass == ExprClass.Unresolved || right.eclass == ExprClass.Unresolved)
			{
				throw new ArgumentException();
			}
			StringConcat stringConcat = new StringConcat(loc);
			stringConcat.type = rc.BuiltinTypes.String;
			stringConcat.eclass = ExprClass.Value;
			stringConcat.Append(rc, left);
			stringConcat.Append(rc, right);
			return stringConcat;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Argument argument = arguments[0];
			return CreateExpressionAddCall(ec, argument, argument.CreateExpressionTree(ec), 1);
		}

		private Expression CreateExpressionAddCall(ResolveContext ec, Argument left, Expression left_etree, int pos)
		{
			Arguments args = new Arguments(2);
			Arguments arguments = new Arguments(3);
			args.Add(left);
			arguments.Add(new Argument(left_etree));
			args.Add(this.arguments[pos]);
			arguments.Add(new Argument(this.arguments[pos].CreateExpressionTree(ec)));
			IList<MemberSpec> concatMethodCandidates = GetConcatMethodCandidates();
			if (concatMethodCandidates == null)
			{
				return null;
			}
			MethodSpec methodSpec = new OverloadResolver(concatMethodCandidates, OverloadResolver.Restrictions.NoBaseMembers, loc).ResolveMember<MethodSpec>(ec, ref args);
			if (methodSpec == null)
			{
				return null;
			}
			arguments.Add(new Argument(new TypeOfMethod(methodSpec, loc)));
			Expression expression = CreateExpressionFactoryCall(ec, "Add", arguments);
			if (++pos == this.arguments.Count)
			{
				return expression;
			}
			left = new Argument(new EmptyExpression(methodSpec.ReturnType));
			return CreateExpressionAddCall(ec, left, expression, pos);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return this;
		}

		private void Append(ResolveContext rc, Expression operand)
		{
			StringConstant stringConstant = operand as StringConstant;
			if (stringConstant != null)
			{
				if (arguments.Count != 0)
				{
					Argument argument = arguments[arguments.Count - 1];
					StringConstant stringConstant2 = argument.Expr as StringConstant;
					if (stringConstant2 != null)
					{
						argument.Expr = new StringConstant(rc.BuiltinTypes, stringConstant2.Value + stringConstant.Value, stringConstant.Location);
						return;
					}
				}
			}
			else
			{
				StringConcat stringConcat = operand as StringConcat;
				if (stringConcat != null)
				{
					arguments.AddRange(stringConcat.arguments);
					return;
				}
			}
			arguments.Add(new Argument(operand));
		}

		private IList<MemberSpec> GetConcatMethodCandidates()
		{
			return MemberCache.FindMembers(type, "Concat", true);
		}

		public override void Emit(EmitContext ec)
		{
			for (int i = 0; i < arguments.Count; i++)
			{
				if (arguments[i].Expr is NullConstant)
				{
					arguments.RemoveAt(i--);
				}
			}
			IList<MemberSpec> concatMethodCandidates = GetConcatMethodCandidates();
			MethodSpec methodSpec = new OverloadResolver(concatMethodCandidates, OverloadResolver.Restrictions.NoBaseMembers, loc).ResolveMember<MethodSpec>(new ResolveContext(ec.MemberContext), ref arguments);
			if (methodSpec != null)
			{
				default(CallEmitter).EmitPredefined(ec, methodSpec, arguments);
			}
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			arguments.FlowAnalysis(fc);
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			if (arguments.Count != 2)
			{
				throw new NotImplementedException("arguments.Count != 2");
			}
			MethodInfo method = typeof(string).GetMethod("Concat", new Type[2]
			{
				typeof(object),
				typeof(object)
			});
			return System.Linq.Expressions.Expression.Add(arguments[0].Expr.MakeExpression(ctx), arguments[1].Expr.MakeExpression(ctx), method);
		}
	}
}
