namespace Mono.CSharp
{
	public abstract class StateMachine : AnonymousMethodStorey
	{
		public enum State
		{
			Running = -3,
			Uninitialized = -2,
			After = -1,
			Start = 0
		}

		private Field pc_field;

		private StateMachineMethod method;

		private int local_name_idx;

		public TypeParameters OriginalTypeParameters { get; private set; }

		public StateMachineMethod StateMachineMethod => method;

		public Field PC => pc_field;

		protected StateMachine(ParametersBlock block, TypeDefinition parent, MemberBase host, TypeParameters tparams, string name, MemberKind kind)
			: base(block, parent, host, tparams, name, kind)
		{
			OriginalTypeParameters = tparams;
		}

		public void AddEntryMethod(StateMachineMethod method)
		{
			if (this.method != null)
			{
				throw new InternalErrorException();
			}
			this.method = method;
			base.Members.Add(method);
		}

		protected override bool DoDefineMembers()
		{
			pc_field = AddCompilerGeneratedField("$PC", new TypeExpression(Compiler.BuiltinTypes.Int, base.Location));
			return base.DoDefineMembers();
		}

		protected override string GetVariableMangledName(LocalVariable local_info)
		{
			if (local_info.IsCompilerGenerated)
			{
				return base.GetVariableMangledName(local_info);
			}
			return "<" + local_info.Name + ">__" + local_name_idx++.ToString("X");
		}
	}
}
