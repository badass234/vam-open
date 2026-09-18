using System.Reflection.Emit;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	public abstract class AnonymousExpression : ExpressionStatement
	{
		protected class AnonymousMethodMethod : Method
		{
			public readonly AnonymousExpression AnonymousMethod;

			public readonly AnonymousMethodStorey Storey;

			public AnonymousMethodMethod(TypeDefinition parent, AnonymousExpression am, AnonymousMethodStorey storey, TypeExpr return_type, Modifiers mod, MemberName name, ParametersCompiled parameters)
				: base(parent, return_type, mod | Modifiers.COMPILER_GENERATED, name, parameters, null)
			{
				AnonymousMethod = am;
				Storey = storey;
				Parent.PartialContainer.Members.Add(this);
				base.Block = new ToplevelBlock(am.block, parameters);
			}

			public override EmitContext CreateEmitContext(ILGenerator ig, SourceMethodBuilder sourceMethod)
			{
				return new EmitContext(this, ig, base.ReturnType, sourceMethod)
				{
					CurrentAnonymousMethod = AnonymousMethod
				};
			}

			protected override void DefineTypeParameters()
			{
			}

			protected override bool ResolveMemberType()
			{
				if (!base.ResolveMemberType())
				{
					return false;
				}
				if (Storey != null && Storey.Mutator != null)
				{
					if (!parameters.IsEmpty)
					{
						TypeSpec[] array = Storey.Mutator.Mutate(parameters.Types);
						if (array != parameters.Types)
						{
							parameters = ParametersCompiled.CreateFullyResolved((Parameter[])parameters.FixedParameters, array);
						}
					}
					member_type = Storey.Mutator.Mutate(member_type);
				}
				return true;
			}

			public override void Emit()
			{
				if (base.MethodBuilder == null)
				{
					Define();
				}
				base.Emit();
			}
		}

		protected readonly ParametersBlock block;

		public TypeSpec ReturnType;

		public abstract string ContainerType { get; }

		public abstract bool IsIterator { get; }

		public abstract AnonymousMethodStorey Storey { get; }

		public ParametersBlock Block => block;

		protected AnonymousExpression(ParametersBlock block, TypeSpec return_type, Location loc)
		{
			ReturnType = return_type;
			this.block = block;
			base.loc = loc;
		}

		public AnonymousExpression Compatible(ResolveContext ec)
		{
			return Compatible(ec, this);
		}

		public AnonymousExpression Compatible(ResolveContext ec, AnonymousExpression ae)
		{
			if (block.Resolved)
			{
				return this;
			}
			BlockContext blockContext = new BlockContext(ec, block, ReturnType);
			blockContext.CurrentAnonymousMethod = ae;
			AnonymousMethodBody anonymousMethodBody = this as AnonymousMethodBody;
			if (ec.HasSet(ResolveContext.Options.InferReturnType) && anonymousMethodBody != null)
			{
				anonymousMethodBody.ReturnTypeInference = new TypeInferenceContext();
			}
			BlockContext blockContext2 = ec as BlockContext;
			if (blockContext2 != null)
			{
				blockContext.AssignmentInfoOffset = blockContext2.AssignmentInfoOffset;
				blockContext.EnclosingLoop = blockContext2.EnclosingLoop;
				blockContext.EnclosingLoopOrSwitch = blockContext2.EnclosingLoopOrSwitch;
				blockContext.Switch = blockContext2.Switch;
			}
			int errors = ec.Report.Errors;
			bool flag = Block.Resolve(blockContext);
			if (flag && errors == ec.Report.Errors)
			{
				MarkReachable(default(Reachability));
				if (!CheckReachableExit(ec.Report))
				{
					return null;
				}
				if (blockContext2 != null)
				{
					blockContext2.AssignmentInfoOffset = blockContext.AssignmentInfoOffset;
				}
			}
			if (anonymousMethodBody != null && anonymousMethodBody.ReturnTypeInference != null)
			{
				anonymousMethodBody.ReturnTypeInference.FixAllTypes(ec);
				ReturnType = anonymousMethodBody.ReturnTypeInference.InferredTypeArguments[0];
				anonymousMethodBody.ReturnTypeInference = null;
				if (block.IsAsync && ReturnType != null)
				{
					ReturnType = ((ReturnType.Kind == MemberKind.Void) ? ec.Module.PredefinedTypes.Task.TypeSpec : ec.Module.PredefinedTypes.TaskGeneric.TypeSpec.MakeGenericType(ec, new TypeSpec[1] { ReturnType }));
				}
			}
			if (flag && errors != ec.Report.Errors)
			{
				return null;
			}
			if (!flag)
			{
				return null;
			}
			return this;
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		private bool CheckReachableExit(Report report)
		{
			if (block.HasReachableClosingBrace && ReturnType.Kind != MemberKind.Void && !IsIterator)
			{
				report.Error(1643, StartLocation, "Not all code paths return a value in anonymous method of type `{0}'", GetSignatureForError());
				return false;
			}
			return true;
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			MarkReachable(default(Reachability));
			CheckReachableExit(fc.Report);
			DefiniteAssignmentBitSet definiteAssignment = fc.BranchDefiniteAssignment();
			ParametersBlock parametersBlock = fc.ParametersBlock;
			fc.ParametersBlock = Block;
			DefiniteAssignmentBitSet definiteAssignmentOnTrue = fc.DefiniteAssignmentOnTrue;
			DefiniteAssignmentBitSet definiteAssignmentOnFalse = fc.DefiniteAssignmentOnFalse;
			TryFinally tryFinally = fc.TryFinally;
			DefiniteAssignmentBitSet definiteAssignmentOnTrue2 = (fc.DefiniteAssignmentOnFalse = null);
			fc.DefiniteAssignmentOnTrue = definiteAssignmentOnTrue2;
			fc.TryFinally = null;
			block.FlowAnalysis(fc);
			fc.ParametersBlock = parametersBlock;
			fc.DefiniteAssignment = definiteAssignment;
			fc.DefiniteAssignmentOnTrue = definiteAssignmentOnTrue;
			fc.DefiniteAssignmentOnFalse = definiteAssignmentOnFalse;
			fc.TryFinally = tryFinally;
		}

		public override void MarkReachable(Reachability rc)
		{
			block.MarkReachable(rc);
		}

		public void SetHasThisAccess()
		{
			ExplicitBlock explicitBlock = block;
			while (!explicitBlock.HasCapturedThis)
			{
				explicitBlock.HasCapturedThis = true;
				explicitBlock = ((explicitBlock.Parent == null) ? null : explicitBlock.Parent.Explicit);
				if (explicitBlock == null)
				{
					break;
				}
			}
		}
	}
}
