using System;

namespace Mono.CSharp
{
	public class Arglist : Expression
	{
		private Arguments arguments;

		public Arguments Arguments => arguments;

		public Type[] ArgumentTypes
		{
			get
			{
				if (arguments == null)
				{
					return System.Type.EmptyTypes;
				}
				Type[] array = new Type[arguments.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = arguments[i].Expr.Type.GetMetaInfo();
				}
				return array;
			}
		}

		public Arglist(Location loc)
			: this(null, loc)
		{
		}

		public Arglist(Arguments args, Location l)
		{
			arguments = args;
			loc = l;
		}

		public override bool ContainsEmitWithAwait()
		{
			throw new NotImplementedException();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			ec.Report.Error(1952, loc, "An expression tree cannot contain a method with variable arguments");
			return null;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			eclass = ExprClass.Variable;
			type = InternalType.Arglist;
			if (arguments != null)
			{
				bool dynamic;
				arguments.Resolve(ec, out dynamic);
			}
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			if (arguments != null)
			{
				arguments.Emit(ec);
			}
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			Arglist arglist = (Arglist)t;
			if (arguments != null)
			{
				arglist.arguments = arguments.Clone(clonectx);
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
