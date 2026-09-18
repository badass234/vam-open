using System;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ArrayAccess : Expression, IDynamicAssign, IAssignMethod, IMemoryLocation
	{
		private ElementAccess ea;

		private LocalTemporary temp;

		private bool prepared;

		private bool? has_await_args;

		public bool ConditionalAccess { get; set; }

		public bool ConditionalAccessReceiver { get; set; }

		public ArrayAccess(ElementAccess ea_data, Location l)
		{
			ea = ea_data;
			loc = l;
		}

		public void AddressOf(EmitContext ec, AddressOp mode)
		{
			ArrayContainer arrayContainer = (ArrayContainer)ea.Expr.Type;
			if (!has_await_args.HasValue && ec.HasSet(BuilderContext.Options.AsyncBody) && ea.Arguments.ContainsEmitWithAwait())
			{
				LoadInstanceAndArguments(ec, false, true);
			}
			LoadInstanceAndArguments(ec, false, false);
			if (arrayContainer.Element.IsGenericParameter && mode == AddressOp.Load)
			{
				ec.Emit(OpCodes.Readonly);
			}
			ec.EmitArrayAddress(arrayContainer);
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			if (ConditionalAccess)
			{
				Error_NullShortCircuitInsideExpressionTree(ec);
			}
			return ea.CreateExpressionTree(ec);
		}

		public override bool ContainsEmitWithAwait()
		{
			return ea.ContainsEmitWithAwait();
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
		{
			if (HasConditionalAccess())
			{
				Error_NullPropagatingLValue(ec);
			}
			return DoResolve(ec);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			bool dynamic;
			ea.Arguments.Resolve(ec, out dynamic);
			ArrayContainer arrayContainer = ea.Expr.Type as ArrayContainer;
			int count = ea.Arguments.Count;
			if (arrayContainer.Rank != count)
			{
				ec.Report.Error(22, ea.Location, "Wrong number of indexes `{0}' inside [], expected `{1}'", count.ToString(), arrayContainer.Rank.ToString());
				return null;
			}
			type = arrayContainer.Element;
			if (type.IsPointer && !ec.IsUnsafe)
			{
				Expression.UnsafeError(ec, ea.Location);
			}
			if (ConditionalAccessReceiver)
			{
				type = Expression.LiftMemberType(ec, type);
			}
			foreach (Argument argument in ea.Arguments)
			{
				NamedArgument namedArgument = argument as NamedArgument;
				if (namedArgument != null)
				{
					Expression.Error_NamedArgument(namedArgument, ec.Report);
				}
				argument.Expr = ConvertExpressionToArrayIndex(ec, argument.Expr);
			}
			eclass = ExprClass.Variable;
			return this;
		}

		protected override void Error_NegativeArrayIndex(ResolveContext ec, Location loc)
		{
			ec.Report.Warning(251, 2, loc, "Indexing an array with a negative index (array indices always start at zero)");
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			ea.FlowAnalysis(fc);
		}

		public override bool HasConditionalAccess()
		{
			if (!ConditionalAccess)
			{
				return ea.Expr.HasConditionalAccess();
			}
			return true;
		}

		private void LoadInstanceAndArguments(EmitContext ec, bool duplicateArguments, bool prepareAwait)
		{
			if (prepareAwait)
			{
				ea.Expr = ea.Expr.EmitToField(ec);
			}
			else
			{
				new InstanceEmitter(ea.Expr, false).Emit(ec, ConditionalAccess);
				if (duplicateArguments)
				{
					ec.Emit(OpCodes.Dup);
					LocalTemporary localTemporary = new LocalTemporary(ea.Expr.Type);
					localTemporary.Store(ec);
					ea.Expr = localTemporary;
				}
			}
			Arguments arguments = ea.Arguments.Emit(ec, duplicateArguments, prepareAwait);
			if (arguments != null)
			{
				ea.Arguments = arguments;
			}
		}

		public void Emit(EmitContext ec, bool leave_copy)
		{
			if (prepared)
			{
				ec.EmitLoadFromPtr(type);
			}
			else
			{
				if (!has_await_args.HasValue && ec.HasSet(BuilderContext.Options.AsyncBody) && ea.Arguments.ContainsEmitWithAwait())
				{
					LoadInstanceAndArguments(ec, false, true);
				}
				if (ConditionalAccessReceiver)
				{
					ec.ConditionalAccess = new ConditionalAccessContext(type, ec.DefineLabel());
				}
				ArrayContainer arrayContainer = (ArrayContainer)ea.Expr.Type;
				LoadInstanceAndArguments(ec, false, false);
				ec.EmitArrayLoad(arrayContainer);
				if (ConditionalAccessReceiver)
				{
					ec.CloseConditionalAccess((type.IsNullableType && type != arrayContainer.Element) ? type : null);
				}
			}
			if (leave_copy)
			{
				ec.Emit(OpCodes.Dup);
				temp = new LocalTemporary(type);
				temp.Store(ec);
			}
		}

		public override void Emit(EmitContext ec)
		{
			Emit(ec, false);
		}

		public void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			ArrayContainer ac = (ArrayContainer)ea.Expr.Type;
			TypeSpec typeSpec = source.Type;
			has_await_args = ec.HasSet(BuilderContext.Options.AsyncBody) && (ea.Arguments.ContainsEmitWithAwait() || source.ContainsEmitWithAwait());
			if (typeSpec.IsStruct && ((isCompound && !(source is DynamicExpressionStatement)) || !BuiltinTypeSpec.IsPrimitiveType(typeSpec)))
			{
				LoadInstanceAndArguments(ec, false, has_await_args.Value);
				if (has_await_args.Value)
				{
					if (source.ContainsEmitWithAwait())
					{
						source = source.EmitToField(ec);
						isCompound = false;
						prepared = true;
					}
					LoadInstanceAndArguments(ec, isCompound, false);
				}
				else
				{
					prepared = true;
				}
				ec.EmitArrayAddress(ac);
				if (isCompound)
				{
					ec.Emit(OpCodes.Dup);
					prepared = true;
				}
			}
			else
			{
				LoadInstanceAndArguments(ec, isCompound, has_await_args.Value);
				if (has_await_args.Value)
				{
					if (source.ContainsEmitWithAwait())
					{
						source = source.EmitToField(ec);
					}
					LoadInstanceAndArguments(ec, false, false);
				}
			}
			source.Emit(ec);
			if (isCompound)
			{
				(ea.Expr as LocalTemporary)?.Release(ec);
			}
			if (leave_copy)
			{
				ec.Emit(OpCodes.Dup);
				temp = new LocalTemporary(type);
				temp.Store(ec);
			}
			if (prepared)
			{
				ec.EmitStoreFromPtr(typeSpec);
			}
			else
			{
				ec.EmitArrayStore(ac);
			}
			if (temp != null)
			{
				temp.Emit(ec);
				temp.Release(ec);
			}
		}

		public override Expression EmitToField(EmitContext ec)
		{
			ea.Expr = ea.Expr.EmitToField(ec);
			ea.Arguments = ea.Arguments.Emit(ec, false, true);
			return this;
		}

		public System.Linq.Expressions.Expression MakeAssignExpression(BuilderContext ctx, Expression source)
		{
			throw new NotImplementedException();
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return System.Linq.Expressions.Expression.ArrayIndex(ea.Expr.MakeExpression(ctx), MakeExpressionArguments(ctx));
		}

		private System.Linq.Expressions.Expression[] MakeExpressionArguments(BuilderContext ctx)
		{
			using (ctx.With(BuilderContext.Options.CheckedScope, true))
			{
				return Arguments.MakeExpression(ea.Arguments, ctx);
			}
		}
	}
}
