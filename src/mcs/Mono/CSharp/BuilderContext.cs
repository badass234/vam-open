using System;

namespace Mono.CSharp
{
	public class BuilderContext
	{
		[Flags]
		public enum Options
		{
			CheckedScope = 1,
			AccurateDebugInfo = 2,
			OmitDebugInfo = 4,
			ConstructorScope = 8,
			AsyncBody = 0x10
		}

		public struct FlagsHandle : IDisposable
		{
			private readonly BuilderContext ec;

			private readonly Options invmask;

			private readonly Options oldval;

			public FlagsHandle(BuilderContext ec, Options flagsToSet)
				: this(ec, flagsToSet, flagsToSet)
			{
			}

			public FlagsHandle(BuilderContext ec, Options mask, Options val)
			{
				this.ec = ec;
				invmask = ~mask;
				oldval = ec.flags & mask;
				ec.flags = (ec.flags & invmask) | (val & mask);
			}

			public void Dispose()
			{
				ec.flags = (ec.flags & invmask) | oldval;
			}
		}

		protected Options flags;

		public bool HasSet(Options options)
		{
			return (flags & options) == options;
		}

		public FlagsHandle With(Options options, bool enable)
		{
			return new FlagsHandle(this, options, enable ? options : ((Options)0));
		}
	}
}
