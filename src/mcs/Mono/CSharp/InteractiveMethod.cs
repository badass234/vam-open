namespace Mono.CSharp
{
	internal class InteractiveMethod : Method
	{
		public InteractiveMethod(TypeDefinition parent, FullNamedExpression returnType, Modifiers mod, ParametersCompiled parameters)
			: base(parent, returnType, mod, new MemberName("Host"), parameters, null)
		{
		}

		public void ChangeToAsync()
		{
			base.ModFlags |= Modifiers.ASYNC;
			base.ModFlags &= ~Modifiers.UNSAFE;
			type_expr = new TypeExpression(Module.PredefinedTypes.Task.TypeSpec, base.Location);
			parameters = ParametersCompiled.EmptyReadOnlyParameters;
		}

		public override string GetSignatureForError()
		{
			return "InteractiveHost";
		}
	}
}
