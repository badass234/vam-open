using System.Collections.Generic;

namespace Mono.CSharp
{
	public class CollectionOrObjectInitializers : ExpressionStatement
	{
		private IList<Expression> initializers;

		private bool is_collection_initialization;

		public IList<Expression> Initializers => initializers;

		public bool IsEmpty => initializers.Count == 0;

		public bool IsCollectionInitializer => is_collection_initialization;

		public CollectionOrObjectInitializers(Location loc)
			: this(new Expression[0], loc)
		{
		}

		public CollectionOrObjectInitializers(IList<Expression> initializers, Location loc)
		{
			this.initializers = initializers;
			base.loc = loc;
		}

		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
			CollectionOrObjectInitializers collectionOrObjectInitializers = (CollectionOrObjectInitializers)target;
			collectionOrObjectInitializers.initializers = new List<Expression>(initializers.Count);
			foreach (Expression initializer in initializers)
			{
				collectionOrObjectInitializers.initializers.Add(initializer.Clone(clonectx));
			}
		}

		public override bool ContainsEmitWithAwait()
		{
			foreach (Expression initializer in initializers)
			{
				if (initializer.ContainsEmitWithAwait())
				{
					return true;
				}
			}
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			return CreateExpressionTree(ec, false);
		}

		public Expression CreateExpressionTree(ResolveContext ec, bool inferType)
		{
			ArrayInitializer arrayInitializer = new ArrayInitializer(initializers.Count, loc);
			foreach (Expression initializer in initializers)
			{
				Expression expression = initializer.CreateExpressionTree(ec);
				if (expression != null)
				{
					arrayInitializer.Add(expression);
				}
			}
			if (inferType)
			{
				return new ImplicitlyTypedArrayCreation(arrayInitializer, loc);
			}
			return new ArrayCreation(new TypeExpression(ec.Module.PredefinedTypes.MemberBinding.Resolve(), loc), arrayInitializer, loc);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			List<string> list = null;
			for (int i = 0; i < initializers.Count; i++)
			{
				Expression expression = initializers[i];
				ElementInitializer elementInitializer = expression as ElementInitializer;
				if (i == 0)
				{
					if (elementInitializer != null)
					{
						list = new List<string>(initializers.Count);
						if (!elementInitializer.IsDictionaryInitializer)
						{
							list.Add(elementInitializer.Name);
						}
					}
					else
					{
						if (expression is CompletingExpression)
						{
							expression.Resolve(ec);
							throw new InternalErrorException("This line should never be reached");
						}
						TypeSpec typeSpec = ec.CurrentInitializerVariable.Type;
						if (!typeSpec.ImplementsInterface(ec.BuiltinTypes.IEnumerable, false) && typeSpec.BuiltinType != BuiltinTypeSpec.Type.Dynamic)
						{
							ec.Report.Error(1922, loc, "A field or property `{0}' cannot be initialized with a collection object initializer because type `{1}' does not implement `{2}' interface", ec.CurrentInitializerVariable.GetSignatureForError(), ec.CurrentInitializerVariable.Type.GetSignatureForError(), ec.BuiltinTypes.IEnumerable.GetSignatureForError());
							return null;
						}
						is_collection_initialization = true;
					}
				}
				else
				{
					if (is_collection_initialization != (elementInitializer == null))
					{
						ec.Report.Error(747, expression.Location, "Inconsistent `{0}' member declaration", is_collection_initialization ? "collection initializer" : "object initializer");
						continue;
					}
					if (!is_collection_initialization && !elementInitializer.IsDictionaryInitializer)
					{
						if (list.Contains(elementInitializer.Name))
						{
							ec.Report.Error(1912, elementInitializer.Location, "An object initializer includes more than one member `{0}' initialization", elementInitializer.Name);
						}
						else
						{
							list.Add(elementInitializer.Name);
						}
					}
				}
				Expression expression2 = expression.Resolve(ec);
				if (expression2 == EmptyExpressionStatement.Instance)
				{
					initializers.RemoveAt(i--);
				}
				else
				{
					initializers[i] = expression2;
				}
			}
			type = ec.CurrentInitializerVariable.Type;
			if (is_collection_initialization && TypeManager.HasElementType(type))
			{
				ec.Report.Error(1925, loc, "Cannot initialize object of type `{0}' with a collection initializer", type.GetSignatureForError());
			}
			eclass = ExprClass.Variable;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			EmitStatement(ec);
		}

		public override void EmitStatement(EmitContext ec)
		{
			foreach (ExpressionStatement initializer in initializers)
			{
				ec.Mark(initializer.Location);
				initializer.EmitStatement(ec);
			}
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			foreach (Expression initializer in initializers)
			{
				initializer?.FlowAnalysis(fc);
			}
		}
	}
}
