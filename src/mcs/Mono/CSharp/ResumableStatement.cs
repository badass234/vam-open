using System.Reflection.Emit;

namespace Mono.CSharp
{
	public abstract class ResumableStatement : Statement
	{
		private bool prepared;

		protected Label resume_point;

		public Label PrepareForEmit(EmitContext ec)
		{
			if (!prepared)
			{
				prepared = true;
				resume_point = ec.DefineLabel();
			}
			return resume_point;
		}

		public virtual Label PrepareForDispose(EmitContext ec, Label end)
		{
			return end;
		}

		public virtual void EmitForDispose(EmitContext ec, LocalBuilder pc, Label end, bool have_dispatcher)
		{
		}
	}
}
