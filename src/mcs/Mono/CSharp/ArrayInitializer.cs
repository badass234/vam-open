using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public class ArrayInitializer : Expression
	{
		private List<Expression> elements;

		private BlockVariable variable;

		public int Count => elements.Count;

		public List<Expression> Elements => elements;

		public Expression this[int index] => elements[index];

		public BlockVariable VariableDeclaration
		{
			get
			{
				return variable;
			}
			set
			{
				variable = value;
			}
		}

		public ArrayInitializer(List<Expression> init, Location loc)
		{
			elements = init;
			base.loc = loc;
		}

		public ArrayInitializer(int count, Location loc)
			: this(new List<Expression>(count), loc)
		{
		}

		public ArrayInitializer(Location loc)
			: this(4, loc)
		{
		}

		public void Add(Expression expr)
		{
			elements.Add(expr);
		}

		public override bool ContainsEmitWithAwait()
		{
			throw new NotSupportedException();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotSupportedException("ET");
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			ArrayInitializer arrayInitializer = (ArrayInitializer)t;
			arrayInitializer.elements = new List<Expression>(elements.Count);
			foreach (Expression element in elements)
			{
				arrayInitializer.elements.Add(element.Clone(clonectx));
			}
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			FieldBase fieldBase = rc.CurrentMemberDefinition as FieldBase;
			TypeExpression requested_base_type;
			if (fieldBase != null && rc.CurrentAnonymousMethod == null)
			{
				requested_base_type = new TypeExpression(fieldBase.MemberType, fieldBase.Location);
			}
			else
			{
				if (variable == null)
				{
					throw new NotImplementedException("Unexpected array initializer context");
				}
				if (variable.TypeExpression is VarExpr)
				{
					rc.Report.Error(820, loc, "An implicitly typed local variable declarator cannot use an array initializer");
					return EmptyExpression.Null;
				}
				requested_base_type = new TypeExpression(variable.Variable.Type, variable.Variable.Location);
			}
			return new ArrayCreation(requested_base_type, this).Resolve(rc);
		}

		public override void Emit(EmitContext ec)
		{
			throw new InternalErrorException("Missing Resolve call");
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			throw new InternalErrorException("Missing Resolve call");
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
