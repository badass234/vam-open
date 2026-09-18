using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class Arguments
	{
		private sealed class ArgumentsOrdered : Arguments
		{
			private readonly List<MovableArgument> ordered;

			public ArgumentsOrdered(Arguments args)
				: base(args.Count)
			{
				AddRange(args);
				ordered = new List<MovableArgument>();
			}

			public void AddOrdered(MovableArgument arg)
			{
				ordered.Add(arg);
			}

			public override void FlowAnalysis(FlowAnalysisContext fc, List<MovableArgument> movable = null)
			{
				foreach (MovableArgument item in ordered)
				{
					if (item.ArgType != Argument.AType.Out)
					{
						item.FlowAnalysis(fc);
					}
				}
				base.FlowAnalysis(fc, ordered);
			}

			public override Arguments Emit(EmitContext ec, bool dup_args, bool prepareAwait)
			{
				foreach (MovableArgument item in ordered)
				{
					if (prepareAwait)
					{
						item.EmitToField(ec, false);
					}
					else
					{
						item.EmitToVariable(ec);
					}
				}
				return base.Emit(ec, dup_args, prepareAwait);
			}
		}

		private List<Argument> args;

		public int Count => args.Count;

		public bool HasDynamic
		{
			get
			{
				foreach (Argument arg in args)
				{
					if (arg.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic && !arg.IsByRef)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasNamed
		{
			get
			{
				foreach (Argument arg in args)
				{
					if (arg is NamedArgument)
					{
						return true;
					}
				}
				return false;
			}
		}

		public Argument this[int index]
		{
			get
			{
				return args[index];
			}
			set
			{
				args[index] = value;
			}
		}

		public Arguments(int capacity)
		{
			args = new List<Argument>(capacity);
		}

		private Arguments(List<Argument> args)
		{
			this.args = args;
		}

		public void Add(Argument arg)
		{
			args.Add(arg);
		}

		public void AddRange(Arguments args)
		{
			this.args.AddRange(args.args);
		}

		public bool ContainsEmitWithAwait()
		{
			foreach (Argument arg in args)
			{
				if (arg.Expr.ContainsEmitWithAwait())
				{
					return true;
				}
			}
			return false;
		}

		public ArrayInitializer CreateDynamicBinderArguments(ResolveContext rc)
		{
			Location loc = Location.Null;
			ArrayInitializer arrayInitializer = new ArrayInitializer(args.Count, loc);
			MemberAccess binderNamespace = DynamicExpressionStatement.GetBinderNamespace(loc);
			foreach (Argument arg in args)
			{
				Arguments arguments = new Arguments(2);
				Expression expression = new IntLiteral(rc.BuiltinTypes, 0, loc);
				if (arg.Expr is Constant)
				{
					expression = new Binary(Binary.Operator.BitwiseOr, expression, new MemberAccess(new MemberAccess(binderNamespace, "CSharpArgumentInfoFlags", loc), "Constant", loc));
				}
				else if (arg.ArgType == Argument.AType.Ref)
				{
					expression = new Binary(Binary.Operator.BitwiseOr, expression, new MemberAccess(new MemberAccess(binderNamespace, "CSharpArgumentInfoFlags", loc), "IsRef", loc));
					expression = new Binary(Binary.Operator.BitwiseOr, expression, new MemberAccess(new MemberAccess(binderNamespace, "CSharpArgumentInfoFlags", loc), "UseCompileTimeType", loc));
				}
				else if (arg.ArgType == Argument.AType.Out)
				{
					expression = new Binary(Binary.Operator.BitwiseOr, expression, new MemberAccess(new MemberAccess(binderNamespace, "CSharpArgumentInfoFlags", loc), "IsOut", loc));
					expression = new Binary(Binary.Operator.BitwiseOr, expression, new MemberAccess(new MemberAccess(binderNamespace, "CSharpArgumentInfoFlags", loc), "UseCompileTimeType", loc));
				}
				else if (arg.ArgType == Argument.AType.DynamicTypeName)
				{
					expression = new Binary(Binary.Operator.BitwiseOr, expression, new MemberAccess(new MemberAccess(binderNamespace, "CSharpArgumentInfoFlags", loc), "IsStaticType", loc));
				}
				TypeSpec type = arg.Expr.Type;
				if (type.BuiltinType != BuiltinTypeSpec.Type.Dynamic && type != InternalType.NullLiteral)
				{
					MethodGroupExpr methodGroupExpr = arg.Expr as MethodGroupExpr;
					if (methodGroupExpr != null)
					{
						rc.Report.Error(1976, arg.Expr.Location, "The method group `{0}' cannot be used as an argument of dynamic operation. Consider using parentheses to invoke the method", methodGroupExpr.Name);
					}
					else if (type == InternalType.AnonymousMethod)
					{
						rc.Report.Error(1977, arg.Expr.Location, "An anonymous method or lambda expression cannot be used as an argument of dynamic operation. Consider using a cast");
					}
					else if (type.Kind == MemberKind.Void || type == InternalType.Arglist || type.IsPointer)
					{
						rc.Report.Error(1978, arg.Expr.Location, "An expression of type `{0}' cannot be used as an argument of dynamic operation", type.GetSignatureForError());
					}
					expression = new Binary(Binary.Operator.BitwiseOr, expression, new MemberAccess(new MemberAccess(binderNamespace, "CSharpArgumentInfoFlags", loc), "UseCompileTimeType", loc));
				}
				NamedArgument namedArgument = arg as NamedArgument;
				string s;
				if (namedArgument != null)
				{
					expression = new Binary(Binary.Operator.BitwiseOr, expression, new MemberAccess(new MemberAccess(binderNamespace, "CSharpArgumentInfoFlags", loc), "NamedArgument", loc));
					s = namedArgument.Name;
				}
				else
				{
					s = null;
				}
				arguments.Add(new Argument(expression));
				arguments.Add(new Argument(new StringLiteral(rc.BuiltinTypes, s, loc)));
				arrayInitializer.Add(new Invocation(new MemberAccess(new MemberAccess(binderNamespace, "CSharpArgumentInfo", loc), "Create", loc), arguments));
			}
			return arrayInitializer;
		}

		public static Arguments CreateForExpressionTree(ResolveContext ec, Arguments args, params Expression[] e)
		{
			Arguments arguments = new Arguments((args?.Count ?? 0) + e.Length);
			for (int i = 0; i < e.Length; i++)
			{
				if (e[i] != null)
				{
					arguments.Add(new Argument(e[i]));
				}
			}
			if (args != null)
			{
				foreach (Argument arg in args.args)
				{
					Expression expression = arg.CreateExpressionTree(ec);
					if (expression != null)
					{
						arguments.Add(new Argument(expression));
					}
				}
			}
			return arguments;
		}

		public void CheckArrayAsAttribute(CompilerContext ctx)
		{
			foreach (Argument arg in args)
			{
				if (arg.Type != null && arg.Type.IsArray)
				{
					ctx.Report.Warning(3016, 1, arg.Expr.Location, "Arrays as attribute arguments are not CLS-compliant");
				}
			}
		}

		public Arguments Clone(CloneContext ctx)
		{
			Arguments arguments = new Arguments(args.Count);
			foreach (Argument arg in args)
			{
				arguments.Add(arg.Clone(ctx));
			}
			return arguments;
		}

		public void Emit(EmitContext ec)
		{
			Emit(ec, false, false);
		}

		public virtual Arguments Emit(EmitContext ec, bool dup_args, bool prepareAwait)
		{
			List<Argument> list = ((!((dup_args && Count != 0) | prepareAwait)) ? null : new List<Argument>(Count));
			foreach (Argument arg in args)
			{
				if (prepareAwait)
				{
					list.Add(arg.EmitToField(ec, true));
					continue;
				}
				arg.Emit(ec);
				if (dup_args)
				{
					if (arg.Expr.IsSideEffectFree)
					{
						list.Add(arg);
						continue;
					}
					ec.Emit(OpCodes.Dup);
					LocalTemporary localTemporary = new LocalTemporary(arg.Type);
					localTemporary.Store(ec);
					list.Add(new Argument(localTemporary, arg.ArgType));
				}
			}
			if (list != null)
			{
				return new Arguments(list);
			}
			return null;
		}

		public virtual void FlowAnalysis(FlowAnalysisContext fc, List<MovableArgument> movable = null)
		{
			bool flag = false;
			foreach (Argument arg in args)
			{
				if (arg.ArgType == Argument.AType.Out)
				{
					flag = true;
					continue;
				}
				if (movable == null)
				{
					arg.FlowAnalysis(fc);
					continue;
				}
				MovableArgument movableArgument = arg as MovableArgument;
				if (movableArgument != null && !movable.Contains(movableArgument))
				{
					arg.FlowAnalysis(fc);
				}
			}
			if (!flag)
			{
				return;
			}
			foreach (Argument arg2 in args)
			{
				if (arg2.ArgType == Argument.AType.Out)
				{
					arg2.FlowAnalysis(fc);
				}
			}
		}

		public List<Argument>.Enumerator GetEnumerator()
		{
			return args.GetEnumerator();
		}

		public void Insert(int index, Argument arg)
		{
			args.Insert(index, arg);
		}

		public static System.Linq.Expressions.Expression[] MakeExpression(Arguments args, BuilderContext ctx)
		{
			if (args == null || args.Count == 0)
			{
				return null;
			}
			System.Linq.Expressions.Expression[] array = new System.Linq.Expressions.Expression[args.Count];
			for (int i = 0; i < array.Length; i++)
			{
				Argument argument = args.args[i];
				array[i] = argument.Expr.MakeExpression(ctx);
			}
			return array;
		}

		public Arguments MarkOrderedArgument(NamedArgument a)
		{
			if (a.Expr.IsSideEffectFree)
			{
				return this;
			}
			ArgumentsOrdered argumentsOrdered = this as ArgumentsOrdered;
			if (argumentsOrdered == null)
			{
				argumentsOrdered = new ArgumentsOrdered(this);
				for (int i = 0; i < args.Count; i++)
				{
					Argument argument = args[i];
					if (argument == a)
					{
						break;
					}
					if (argument != null)
					{
						MovableArgument movableArgument = argument as MovableArgument;
						if (movableArgument == null)
						{
							movableArgument = new MovableArgument(argument);
							argumentsOrdered.args[i] = movableArgument;
						}
						argumentsOrdered.AddOrdered(movableArgument);
					}
				}
			}
			argumentsOrdered.AddOrdered(a);
			return argumentsOrdered;
		}

		public void Resolve(ResolveContext ec, out bool dynamic)
		{
			dynamic = false;
			foreach (Argument arg in args)
			{
				arg.Resolve(ec);
				if (arg.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic && !arg.IsByRef)
				{
					dynamic = true;
				}
			}
		}

		public void RemoveAt(int index)
		{
			args.RemoveAt(index);
		}
	}
}
