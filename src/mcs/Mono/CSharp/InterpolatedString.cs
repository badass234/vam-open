using System.Collections.Generic;
using System.Text;

namespace Mono.CSharp
{
	public class InterpolatedString : Expression
	{
		private readonly StringLiteral start;

		private readonly StringLiteral end;

		private List<Expression> interpolations;

		private Arguments arguments;

		public InterpolatedString(StringLiteral start, List<Expression> interpolations, StringLiteral end)
		{
			this.start = start;
			this.end = end;
			this.interpolations = interpolations;
			loc = start.Location;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			InterpolatedString interpolatedString = (InterpolatedString)t;
			if (interpolations == null)
			{
				return;
			}
			interpolatedString.interpolations = new List<Expression>();
			foreach (Expression interpolation in interpolations)
			{
				interpolatedString.interpolations.Add(interpolation.Clone(clonectx));
			}
		}

		public Expression ConvertTo(ResolveContext rc, TypeSpec type)
		{
			TypeSpec typeSpec = rc.Module.PredefinedTypes.FormattableStringFactory.Resolve();
			if (typeSpec == null)
			{
				return null;
			}
			Expression expression = new Invocation(new MemberAccess(new TypeExpression(typeSpec, loc), "Create", loc), arguments).Resolve(rc);
			if (expression != null && expression.Type != type)
			{
				expression = Convert.ExplicitConversion(rc, expression, type, loc);
			}
			return expression;
		}

		public override bool ContainsEmitWithAwait()
		{
			if (interpolations == null)
			{
				return false;
			}
			foreach (Expression interpolation in interpolations)
			{
				if (interpolation.ContainsEmitWithAwait())
				{
					return true;
				}
			}
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext rc)
		{
			MethodSpec methodSpec = ResolveBestFormatOverload(rc);
			if (methodSpec == null)
			{
				return null;
			}
			Expression expression = new NullLiteral(loc);
			Arguments args = Arguments.CreateForExpressionTree(rc, arguments, expression, new TypeOfMethod(methodSpec, loc));
			return CreateExpressionFactoryCall(rc, "Call", args);
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			string s;
			if (interpolations == null)
			{
				s = start.Value;
				arguments = new Arguments(1);
			}
			else
			{
				for (int i = 0; i < interpolations.Count; i += 2)
				{
					((InterpolatedStringInsert)interpolations[i]).Resolve(rc);
				}
				arguments = new Arguments(interpolations.Count);
				StringBuilder stringBuilder = new StringBuilder(start.Value);
				for (int j = 0; j < interpolations.Count; j++)
				{
					if (j % 2 == 0)
					{
						stringBuilder.Append('{').Append(j / 2);
						InterpolatedStringInsert interpolatedStringInsert = (InterpolatedStringInsert)interpolations[j];
						if (interpolatedStringInsert.Alignment != null)
						{
							stringBuilder.Append(',');
							int? num = interpolatedStringInsert.ResolveAligment(rc);
							if (num.HasValue)
							{
								stringBuilder.Append(num.Value);
							}
						}
						if (interpolatedStringInsert.Format != null)
						{
							stringBuilder.Append(':');
							stringBuilder.Append(interpolatedStringInsert.Format);
						}
						stringBuilder.Append('}');
						arguments.Add(new Argument(interpolations[j]));
					}
					else
					{
						stringBuilder.Append(((StringLiteral)interpolations[j]).Value);
					}
				}
				stringBuilder.Append(end.Value);
				s = stringBuilder.ToString();
			}
			arguments.Insert(0, new Argument(new StringLiteral(rc.BuiltinTypes, s, start.Location)));
			eclass = ExprClass.Value;
			type = rc.BuiltinTypes.String;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			if (interpolations == null)
			{
				string text = start.Value.Replace("{{", "{").Replace("}}", "}");
				if (text != start.Value)
				{
					new StringConstant(ec.BuiltinTypes, text, loc).Emit(ec);
				}
				else
				{
					start.Emit(ec);
				}
			}
			else
			{
				MethodSpec methodSpec = ResolveBestFormatOverload(new ResolveContext(ec.MemberContext));
				if (methodSpec != null)
				{
					default(CallEmitter).Emit(ec, methodSpec, arguments, loc);
				}
			}
		}

		private MethodSpec ResolveBestFormatOverload(ResolveContext rc)
		{
			IList<MemberSpec> members = MemberCache.FindMembers(rc.BuiltinTypes.String, "Format", true);
			return new OverloadResolver(members, OverloadResolver.Restrictions.NoBaseMembers, loc).ResolveMember<MethodSpec>(rc, ref arguments);
		}
	}
}
