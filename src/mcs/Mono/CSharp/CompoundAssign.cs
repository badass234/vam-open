using System;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class CompoundAssign : Assign
	{
		public sealed class TargetExpression : Expression
		{
			private readonly Expression child;

			public TargetExpression(Expression child)
			{
				this.child = child;
				loc = child.Location;
			}

			public override bool ContainsEmitWithAwait()
			{
				return child.ContainsEmitWithAwait();
			}

			public override Expression CreateExpressionTree(ResolveContext ec)
			{
				throw new NotSupportedException("ET");
			}

			protected override Expression DoResolve(ResolveContext ec)
			{
				type = child.Type;
				eclass = ExprClass.Value;
				return this;
			}

			public override void Emit(EmitContext ec)
			{
				child.Emit(ec);
			}

			public override Expression EmitToField(EmitContext ec)
			{
				return child.EmitToField(ec);
			}
		}

		private readonly Binary.Operator op;

		private Expression right;

		private Expression left;

		public Binary.Operator Operator => op;

		public CompoundAssign(Binary.Operator op, Expression target, Expression source)
			: base(target, source, target.Location)
		{
			right = source;
			this.op = op;
		}

		public CompoundAssign(Binary.Operator op, Expression target, Expression source, Expression left)
			: this(op, target, source)
		{
			this.left = left;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			right = right.Resolve(ec);
			if (right == null)
			{
				return null;
			}
			MemberAccess memberAccess = target as MemberAccess;
			using (ec.Set(ResolveContext.Options.CompoundAssignmentScope))
			{
				target = target.Resolve(ec);
			}
			if (target == null)
			{
				return null;
			}
			if (target is MethodGroupExpr)
			{
				ec.Report.Error(1656, loc, "Cannot assign to `{0}' because it is a `{1}'", ((MethodGroupExpr)target).Name, target.ExprClassName);
				return null;
			}
			EventExpr eventExpr = target as EventExpr;
			if (eventExpr != null)
			{
				source = Convert.ImplicitConversionRequired(ec, right, target.Type, loc);
				if (source == null)
				{
					return null;
				}
				Expression right_side = ((op == Binary.Operator.Addition) ? EmptyExpression.EventAddition : ((op != Binary.Operator.Subtraction) ? null : EmptyExpression.EventSubtraction));
				target = target.ResolveLValue(ec, right_side);
				if (target == null)
				{
					return null;
				}
				eclass = ExprClass.Value;
				type = eventExpr.Operator.ReturnType;
				return this;
			}
			if (left == null)
			{
				left = new TargetExpression(target);
			}
			source = new Binary(op, left, right, true);
			if (target is DynamicMemberAssignable)
			{
				Arguments arguments = ((DynamicMemberAssignable)target).Arguments;
				source = source.Resolve(ec);
				Arguments arguments2 = new Arguments(arguments.Count + 1);
				arguments2.AddRange(arguments);
				arguments2.Add(new Argument(source));
				CSharpBinderFlags cSharpBinderFlags = CSharpBinderFlags.ValueFromCompoundAssignment;
				if (ec.HasSet(ResolveContext.Options.CheckedScope))
				{
					cSharpBinderFlags |= CSharpBinderFlags.CheckedContext;
				}
				if (target is DynamicMemberBinder)
				{
					source = new DynamicMemberBinder(memberAccess.Name, cSharpBinderFlags, arguments2, loc).Resolve(ec);
					if (op == Binary.Operator.Addition || op == Binary.Operator.Subtraction)
					{
						arguments2 = new Arguments(arguments.Count + 1);
						arguments2.AddRange(arguments);
						arguments2.Add(new Argument(right));
						string text = ((op == Binary.Operator.Addition) ? "add_" : "remove_");
						Expression expression = DynamicInvocation.CreateSpecialNameInvoke(new MemberAccess(right, text + memberAccess.Name, loc), arguments2, loc).Resolve(ec);
						arguments2 = new Arguments(arguments.Count);
						arguments2.AddRange(arguments);
						source = new DynamicEventCompoundAssign(memberAccess.Name, arguments2, (ExpressionStatement)source, (ExpressionStatement)expression, loc).Resolve(ec);
					}
				}
				else
				{
					source = new DynamicIndexBinder(cSharpBinderFlags, arguments2, loc).Resolve(ec);
				}
				return source;
			}
			return base.DoResolve(ec);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			target.FlowAnalysis(fc);
			source.FlowAnalysis(fc);
		}

		protected override Expression ResolveConversions(ResolveContext ec)
		{
			if (target is RuntimeValueExpression)
			{
				return this;
			}
			TypeSpec typeSpec = target.Type;
			if (Convert.ImplicitConversionExists(ec, source, typeSpec))
			{
				source = Convert.ImplicitConversion(ec, source, typeSpec, loc);
				return this;
			}
			Binary binary = source as Binary;
			if (binary == null)
			{
				if (source is ReducedExpression)
				{
					binary = ((ReducedExpression)source).OriginalExpression as Binary;
				}
				else if (source is ReducedExpression.ReducedConstantExpression)
				{
					binary = ((ReducedExpression.ReducedConstantExpression)source).OriginalExpression as Binary;
				}
				else if (source is LiftedBinaryOperator)
				{
					LiftedBinaryOperator liftedBinaryOperator = (LiftedBinaryOperator)source;
					if (liftedBinaryOperator.UserOperator == null)
					{
						binary = liftedBinaryOperator.Binary;
					}
				}
				else if (source is TypeCast)
				{
					binary = ((TypeCast)source).Child as Binary;
				}
			}
			if (binary != null && ((binary.Oper & Binary.Operator.ShiftMask) != 0 || Convert.ImplicitConversionExists(ec, right, typeSpec)))
			{
				source = Convert.ExplicitConversion(ec, source, typeSpec, loc);
				return this;
			}
			if (source.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				Arguments arguments = new Arguments(1);
				arguments.Add(new Argument(source));
				return new SimpleAssign(target, new DynamicConversion(typeSpec, CSharpBinderFlags.ConvertExplicit, arguments, loc), loc).Resolve(ec);
			}
			right.Error_ValueCannotBeConverted(ec, typeSpec, false);
			return null;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			CompoundAssign obj = (CompoundAssign)t;
			obj.right = (obj.source = source.Clone(clonectx));
			obj.target = target.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
