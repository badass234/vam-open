using System.Reflection.Emit;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	public class StateMachineMethod : Method
	{
		private readonly StateMachineInitializer expr;

		public StateMachineMethod(StateMachine host, StateMachineInitializer expr, FullNamedExpression returnType, Modifiers mod, MemberName name, Block.Flags blockFlags)
			: base(host, returnType, mod | Modifiers.COMPILER_GENERATED, name, ParametersCompiled.EmptyReadOnlyParameters, null)
		{
			this.expr = expr;
			base.Block = new ToplevelBlock(host.Compiler, ParametersCompiled.EmptyReadOnlyParameters, Location.Null, blockFlags);
		}

		public override EmitContext CreateEmitContext(ILGenerator ig, SourceMethodBuilder sourceMethod)
		{
			EmitContext emitContext = new EmitContext(this, ig, base.MemberType, sourceMethod);
			emitContext.CurrentAnonymousMethod = expr;
			if (expr is AsyncInitializer)
			{
				emitContext.With(BuilderContext.Options.AsyncBody, true);
			}
			return emitContext;
		}
	}
}
