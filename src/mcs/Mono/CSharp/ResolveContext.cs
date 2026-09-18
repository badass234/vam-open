using System;

namespace Mono.CSharp
{
	public class ResolveContext : IMemberContext, IModuleContext
	{
		[Flags]
		public enum Options
		{
			CheckedScope = 1,
			ConstantCheckState = 2,
			AllCheckStateFlags = CheckedScope | ConstantCheckState,
			UnsafeScope = 4,
			CatchScope = 8,
			FinallyScope = 0x10,
			FieldInitializerScope = 0x20,
			CompoundAssignmentScope = 0x40,
			FixedInitializerScope = 0x80,
			BaseInitializer = 0x100,
			EnumScope = 0x200,
			ConstantScope = 0x400,
			ConstructorScope = 0x800,
			UsingInitializerScope = 0x1000,
			LockScope = 0x2000,
			TryScope = 0x4000,
			TryWithCatchScope = 0x8000,
			ConditionalAccessReceiver = 0x10000,
			ProbingMode = 0x400000,
			InferReturnType = 0x800000,
			OmitDebuggingInfo = 0x1000000,
			ExpressionTreeConversion = 0x2000000,
			InvokeSpecialName = 0x4000000
		}

		public struct FlagsHandle : IDisposable
		{
			private readonly ResolveContext ec;

			private readonly Options invmask;

			private readonly Options oldval;

			public FlagsHandle(ResolveContext ec, Options flagsToSet)
				: this(ec, flagsToSet, flagsToSet)
			{
			}

			public FlagsHandle(ResolveContext ec, Options mask, Options val)
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

		public AnonymousExpression CurrentAnonymousMethod;

		public Expression CurrentInitializerVariable;

		public Block CurrentBlock;

		public readonly IMemberContext MemberContext;

		public BuiltinTypes BuiltinTypes => MemberContext.Module.Compiler.BuiltinTypes;

		public virtual ExplicitBlock ConstructorBlock => CurrentBlock.Explicit;

		public Iterator CurrentIterator => CurrentAnonymousMethod as Iterator;

		public TypeSpec CurrentType => MemberContext.CurrentType;

		public TypeParameters CurrentTypeParameters => MemberContext.CurrentTypeParameters;

		public MemberCore CurrentMemberDefinition => MemberContext.CurrentMemberDefinition;

		public bool ConstantCheckState => (flags & Options.ConstantCheckState) != 0;

		public bool IsInProbingMode => (flags & Options.ProbingMode) != 0;

		public bool IsObsolete => MemberContext.IsObsolete;

		public bool IsStatic => MemberContext.IsStatic;

		public bool IsUnsafe
		{
			get
			{
				if (!HasSet(Options.UnsafeScope))
				{
					return MemberContext.IsUnsafe;
				}
				return true;
			}
		}

		public bool IsRuntimeBinder => Module.Compiler.IsRuntimeBinder;

		public bool IsVariableCapturingRequired => !IsInProbingMode;

		public ModuleContainer Module => MemberContext.Module;

		public Report Report => Module.Compiler.Report;

		public ResolveContext(IMemberContext mc)
		{
			if (mc == null)
			{
				throw new ArgumentNullException();
			}
			MemberContext = mc;
			if (mc.Module.Compiler.Settings.Checked)
			{
				flags |= Options.CheckedScope;
			}
			flags |= Options.ConstantCheckState;
		}

		public ResolveContext(IMemberContext mc, Options options)
			: this(mc)
		{
			flags |= options;
		}

		public bool MustCaptureVariable(INamedBlockVariable local)
		{
			if (CurrentAnonymousMethod == null)
			{
				return false;
			}
			if (CurrentAnonymousMethod.IsIterator)
			{
				if (!local.IsParameter)
				{
					return local.Block.Explicit.HasYield;
				}
				return true;
			}
			if (CurrentAnonymousMethod is AsyncInitializer)
			{
				if (!local.IsParameter && !local.Block.Explicit.HasAwait && !CurrentBlock.Explicit.HasAwait)
				{
					return local.Block.ParametersBlock != CurrentBlock.ParametersBlock.Original;
				}
				return true;
			}
			return local.Block.ParametersBlock != CurrentBlock.ParametersBlock.Original;
		}

		public bool HasSet(Options options)
		{
			return (flags & options) == options;
		}

		public bool HasAny(Options options)
		{
			return (flags & options) != 0;
		}

		public FlagsHandle Set(Options options)
		{
			return new FlagsHandle(this, options);
		}

		public FlagsHandle With(Options options, bool enable)
		{
			return new FlagsHandle(this, options, enable ? options : ((Options)0));
		}

		public string GetSignatureForError()
		{
			return MemberContext.GetSignatureForError();
		}

		public ExtensionMethodCandidates LookupExtensionMethod(string name, int arity)
		{
			return MemberContext.LookupExtensionMethod(name, arity);
		}

		public FullNamedExpression LookupNamespaceOrType(string name, int arity, LookupMode mode, Location loc)
		{
			return MemberContext.LookupNamespaceOrType(name, arity, mode, loc);
		}

		public FullNamedExpression LookupNamespaceAlias(string name)
		{
			return MemberContext.LookupNamespaceAlias(name);
		}
	}
}
