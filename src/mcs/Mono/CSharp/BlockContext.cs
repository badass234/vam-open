using System;

namespace Mono.CSharp
{
	public class BlockContext : ResolveContext
	{
		private readonly TypeSpec return_type;

		public int AssignmentInfoOffset;

		public ExceptionStatement CurrentTryBlock { get; set; }

		public LoopStatement EnclosingLoop { get; set; }

		public LoopStatement EnclosingLoopOrSwitch { get; set; }

		public Switch Switch { get; set; }

		public TypeSpec ReturnType => return_type;

		public BlockContext(IMemberContext mc, ExplicitBlock block, TypeSpec returnType)
			: base(mc)
		{
			if (returnType == null)
			{
				throw new ArgumentNullException("returnType");
			}
			return_type = returnType;
			CurrentBlock = block;
		}

		public BlockContext(ResolveContext rc, ExplicitBlock block, TypeSpec returnType)
			: this(rc.MemberContext, block, returnType)
		{
			if (rc.IsUnsafe)
			{
				flags |= Options.UnsafeScope;
			}
			if (rc.HasSet(Options.CheckedScope))
			{
				flags |= Options.CheckedScope;
			}
			if (!rc.ConstantCheckState)
			{
				flags &= ~Options.ConstantCheckState;
			}
			if (rc.IsInProbingMode)
			{
				flags |= Options.ProbingMode;
			}
			if (rc.HasSet(Options.FieldInitializerScope))
			{
				flags |= Options.FieldInitializerScope;
			}
			if (rc.HasSet(Options.ExpressionTreeConversion))
			{
				flags |= Options.ExpressionTreeConversion;
			}
			if (rc.HasSet(Options.BaseInitializer))
			{
				flags |= Options.BaseInitializer;
			}
		}
	}
}
