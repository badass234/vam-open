using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class AsyncInitializer : StateMachineInitializer
	{
		private TypeInferenceContext return_inference;

		public override string ContainerType => "async state machine block";

		public TypeSpec DelegateType { get; set; }

		public StackFieldExpr HoistedReturnState { get; set; }

		public override bool IsIterator => false;

		public TypeInferenceContext ReturnTypeInference => return_inference;

		public AsyncInitializer(ParametersBlock block, TypeDefinition host, TypeSpec returnType)
			: base(block, host, returnType)
		{
		}

		protected override BlockContext CreateBlockContext(BlockContext bc)
		{
			BlockContext blockContext = base.CreateBlockContext(bc);
			AnonymousMethodBody anonymousMethodBody = bc.CurrentAnonymousMethod as AnonymousMethodBody;
			if (anonymousMethodBody != null)
			{
				return_inference = anonymousMethodBody.ReturnTypeInference;
			}
			blockContext.Set(ResolveContext.Options.TryScope);
			return blockContext;
		}

		public override void Emit(EmitContext ec)
		{
			throw new NotImplementedException();
		}

		public void EmitCatchBlock(EmitContext ec)
		{
			LocalVariable localVariable = LocalVariable.CreateCompilerGenerated(ec.Module.Compiler.BuiltinTypes.Exception, block, base.Location);
			ec.BeginCatchBlock(localVariable.Type);
			localVariable.EmitAssign(ec);
			ec.EmitThis();
			ec.EmitInt(-1);
			ec.Emit(OpCodes.Stfld, storey.PC.Spec);
			((AsyncTaskStorey)Storey).EmitSetException(ec, new LocalVariableReference(localVariable, base.Location));
			ec.Emit(OpCodes.Leave, move_next_ok);
			ec.EndExceptionBlock();
		}

		protected override void EmitMoveNextEpilogue(EmitContext ec)
		{
			((AsyncTaskStorey)Storey).EmitSetResult(ec);
		}

		public override void EmitStatement(EmitContext ec)
		{
			((AsyncTaskStorey)Storey).EmitInitializer(ec);
			ec.Emit(OpCodes.Ret);
		}

		public override void MarkReachable(Reachability rc)
		{
		}
	}
}
