using System;

namespace Mono.CSharp
{
	public class NewInitialize : New
	{
		private sealed class InitializerTargetExpression : Expression, IMemoryLocation
		{
			private NewInitialize new_instance;

			public InitializerTargetExpression(NewInitialize newInstance)
			{
				type = newInstance.type;
				loc = newInstance.loc;
				eclass = newInstance.eclass;
				new_instance = newInstance;
			}

			public override bool ContainsEmitWithAwait()
			{
				return false;
			}

			public override Expression CreateExpressionTree(ResolveContext ec)
			{
				throw new NotSupportedException("ET");
			}

			protected override Expression DoResolve(ResolveContext ec)
			{
				return this;
			}

			public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
			{
				return this;
			}

			public override void Emit(EmitContext ec)
			{
				((Expression)new_instance.instance).Emit(ec);
			}

			public override Expression EmitToField(EmitContext ec)
			{
				return (Expression)new_instance.instance;
			}

			public void AddressOf(EmitContext ec, AddressOp mode)
			{
				new_instance.instance.AddressOf(ec, mode);
			}
		}

		private CollectionOrObjectInitializers initializers;

		private IMemoryLocation instance;

		private DynamicExpressionStatement dynamic;

		public CollectionOrObjectInitializers Initializers => initializers;

		public NewInitialize(FullNamedExpression requested_type, Arguments arguments, CollectionOrObjectInitializers initializers, Location l)
			: base(requested_type, arguments, l)
		{
			this.initializers = initializers;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			base.CloneTo(clonectx, t);
			((NewInitialize)t).initializers = (CollectionOrObjectInitializers)initializers.Clone(clonectx);
		}

		public override bool ContainsEmitWithAwait()
		{
			if (!base.ContainsEmitWithAwait())
			{
				return initializers.ContainsEmitWithAwait();
			}
			return true;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(base.CreateExpressionTree(ec)));
			if (!initializers.IsEmpty)
			{
				arguments.Add(new Argument(initializers.CreateExpressionTree(ec, initializers.IsCollectionInitializer)));
			}
			return CreateExpressionFactoryCall(ec, initializers.IsCollectionInitializer ? "ListInit" : "MemberInit", arguments);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			Expression expression = base.DoResolve(ec);
			if (type == null)
			{
				return null;
			}
			if (type.IsDelegate)
			{
				ec.Report.Error(1958, Initializers.Location, "Object and collection initializers cannot be used to instantiate a delegate");
			}
			Expression currentInitializerVariable = ec.CurrentInitializerVariable;
			ec.CurrentInitializerVariable = new InitializerTargetExpression(this);
			initializers.Resolve(ec);
			ec.CurrentInitializerVariable = currentInitializerVariable;
			dynamic = expression as DynamicExpressionStatement;
			if (dynamic != null)
			{
				return this;
			}
			return expression;
		}

		public override void Emit(EmitContext ec)
		{
			if (method == null && TypeSpec.IsValueType(type) && initializers.Initializers.Count > 1 && ec.HasSet(BuilderContext.Options.AsyncBody) && initializers.ContainsEmitWithAwait())
			{
				StackFieldExpr temporaryField = ec.GetTemporaryField(type);
				if (!Emit(ec, temporaryField))
				{
					temporaryField.Emit(ec);
				}
			}
			else
			{
				base.Emit(ec);
			}
		}

		public override bool Emit(EmitContext ec, IMemoryLocation target)
		{
			IMemoryLocation memoryLocation = target;
			LocalTemporary localTemporary = null;
			bool flag = false;
			if (!initializers.IsEmpty)
			{
				memoryLocation = target as LocalTemporary;
				if (memoryLocation == null)
				{
					memoryLocation = target as StackFieldExpr;
				}
				if (memoryLocation == null)
				{
					VariableReference variableReference = target as VariableReference;
					if (variableReference != null && variableReference.IsRef)
					{
						variableReference.EmitLoad(ec);
						flag = true;
					}
				}
				if (memoryLocation == null)
				{
					memoryLocation = (localTemporary = new LocalTemporary(type));
				}
			}
			bool flag2;
			if (dynamic != null)
			{
				dynamic.Emit(ec);
				flag2 = true;
			}
			else
			{
				flag2 = base.Emit(ec, memoryLocation);
			}
			if (initializers.IsEmpty)
			{
				return flag2;
			}
			StackFieldExpr stackFieldExpr = null;
			if (flag2)
			{
				if (flag)
				{
					memoryLocation = (localTemporary = new LocalTemporary(type));
				}
				localTemporary?.Store(ec);
				if (ec.HasSet(BuilderContext.Options.AsyncBody) && initializers.ContainsEmitWithAwait())
				{
					if (localTemporary == null)
					{
						throw new NotImplementedException();
					}
					stackFieldExpr = ec.GetTemporaryField(type);
					stackFieldExpr.EmitAssign(ec, localTemporary, false, false);
					memoryLocation = stackFieldExpr;
					localTemporary.Release(ec);
					flag2 = false;
				}
			}
			instance = memoryLocation;
			initializers.Emit(ec);
			((Expression)memoryLocation).Emit(ec);
			localTemporary?.Release(ec);
			if (stackFieldExpr != null)
			{
				stackFieldExpr.IsAvailableForReuse = true;
			}
			return true;
		}

		protected override IMemoryLocation EmitAddressOf(EmitContext ec, AddressOp Mode)
		{
			instance = base.EmitAddressOf(ec, Mode);
			if (!initializers.IsEmpty)
			{
				initializers.Emit(ec);
			}
			return instance;
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			base.FlowAnalysis(fc);
			initializers.FlowAnalysis(fc);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
