using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ExplicitBlock : Block
	{
		protected AnonymousMethodStorey am_storey;

		public AnonymousMethodStorey AnonymousMethodStorey => am_storey;

		public bool HasAwait => (flags & Flags.AwaitBlock) != 0;

		public bool HasCapturedThis
		{
			get
			{
				return (flags & Flags.HasCapturedThis) != 0;
			}
			set
			{
				flags = (value ? (flags | Flags.HasCapturedThis) : (flags & ~Flags.HasCapturedThis));
			}
		}

		public bool HasCapturedVariable
		{
			get
			{
				return (flags & Flags.HasCapturedVariable) != 0;
			}
			set
			{
				flags = (value ? (flags | Flags.HasCapturedVariable) : (flags & ~Flags.HasCapturedVariable));
			}
		}

		public bool HasReachableClosingBrace
		{
			get
			{
				return (flags & Flags.ReachableEnd) != 0;
			}
			set
			{
				flags = (value ? (flags | Flags.ReachableEnd) : (flags & ~Flags.ReachableEnd));
			}
		}

		public bool HasYield => (flags & Flags.YieldBlock) != 0;

		public ExplicitBlock(Block parent, Location start, Location end)
			: this(parent, (Flags)0, start, end)
		{
		}

		public ExplicitBlock(Block parent, Flags flags, Location start, Location end)
			: base(parent, flags, start, end)
		{
			Explicit = this;
		}

		public AnonymousMethodStorey CreateAnonymousMethodStorey(ResolveContext ec)
		{
			if (ec.CurrentAnonymousMethod is StateMachineInitializer && ParametersBlock.Original == ec.CurrentAnonymousMethod.Block.Original)
			{
				return ec.CurrentAnonymousMethod.Storey;
			}
			if (am_storey == null)
			{
				MemberBase host = ec.MemberContext as MemberBase;
				am_storey = new AnonymousMethodStorey(this, ec.CurrentMemberDefinition.Parent.PartialContainer, host, ec.CurrentTypeParameters, "AnonStorey", MemberKind.Class);
			}
			return am_storey;
		}

		public void EmitScopeInitialization(EmitContext ec)
		{
			if ((flags & Flags.InitializationEmitted) == 0)
			{
				if (am_storey != null)
				{
					DefineStoreyContainer(ec, am_storey);
					am_storey.EmitStoreyInstantiation(ec, this);
				}
				if (scope_initializers != null)
				{
					EmitScopeInitializers(ec);
				}
				flags |= Flags.InitializationEmitted;
			}
		}

		public override void Emit(EmitContext ec)
		{
			if (Parent != null)
			{
				ec.BeginScope();
			}
			EmitScopeInitialization(ec);
			if (ec.EmitAccurateDebugInfo && !base.IsCompilerGenerated && ec.Mark(StartLocation))
			{
				ec.Emit(OpCodes.Nop);
			}
			DoEmit(ec);
			if (Parent != null)
			{
				ec.EndScope();
			}
			if (ec.EmitAccurateDebugInfo && HasReachableClosingBrace && !(this is ParametersBlock) && !base.IsCompilerGenerated && ec.Mark(EndLocation))
			{
				ec.Emit(OpCodes.Nop);
			}
		}

		protected void DefineStoreyContainer(EmitContext ec, AnonymousMethodStorey storey)
		{
			if (ec.CurrentAnonymousMethod != null && ec.CurrentAnonymousMethod.Storey != null)
			{
				storey.SetNestedStoryParent(ec.CurrentAnonymousMethod.Storey);
				storey.Mutator = ec.CurrentAnonymousMethod.Storey.Mutator;
			}
			storey.CreateContainer();
			storey.DefineContainer();
			if (base.Original.Explicit.HasCapturedThis && base.Original.ParametersBlock.TopBlock.ThisReferencesFromChildrenBlock != null)
			{
				for (Block block = base.Original.Explicit; block != null; block = block.Parent)
				{
					if (block.Parent != null)
					{
						AnonymousMethodStorey anonymousMethodStorey = block.Parent.Explicit.AnonymousMethodStorey;
						if (anonymousMethodStorey != null)
						{
							storey.HoistedThis = anonymousMethodStorey.HoistedThis;
							break;
						}
					}
					if (block.Explicit == block.Explicit.ParametersBlock && block.Explicit.ParametersBlock.StateMachine != null)
					{
						if (storey.HoistedThis == null)
						{
							storey.HoistedThis = block.Explicit.ParametersBlock.StateMachine.HoistedThis;
						}
						if (storey.HoistedThis != null)
						{
							break;
						}
					}
				}
				if (storey.HoistedThis == null || !(storey.Parent is HoistedStoreyClass))
				{
					foreach (ExplicitBlock item in base.Original.ParametersBlock.TopBlock.ThisReferencesFromChildrenBlock)
					{
						Block block2 = item;
						while (block2 != null && block2 != base.Original)
						{
							block2 = block2.Parent;
						}
						if (block2 == null)
						{
							continue;
						}
						if (storey.HoistedThis == null)
						{
							storey.AddCapturedThisField(ec, null);
						}
						for (ExplicitBlock explicitBlock = item; explicitBlock.AnonymousMethodStorey != storey; explicitBlock = explicitBlock.Parent.Explicit)
						{
							AnonymousMethodStorey anonymousMethodStorey2 = explicitBlock.AnonymousMethodStorey;
							ParametersBlock parametersBlock;
							if (anonymousMethodStorey2 != null)
							{
								if (explicitBlock.ParametersBlock.StateMachine == null)
								{
									AnonymousMethodStorey anonymousMethodStorey3 = null;
									for (Block parent = explicitBlock.AnonymousMethodStorey.OriginalSourceBlock.Parent; parent != null; parent = parent.Parent)
									{
										anonymousMethodStorey3 = parent.Explicit.AnonymousMethodStorey;
										if (anonymousMethodStorey3 != null)
										{
											break;
										}
									}
									if (anonymousMethodStorey3 == null)
									{
										AnonymousMethodStorey parent2 = ((storey == null || storey.Kind == MemberKind.Struct) ? null : storey);
										explicitBlock.AnonymousMethodStorey.AddCapturedThisField(ec, parent2);
										break;
									}
								}
								if (explicitBlock.ParametersBlock == ParametersBlock.Original)
								{
									anonymousMethodStorey2.AddParentStoreyReference(ec, storey);
									break;
								}
								explicitBlock = (parametersBlock = explicitBlock.ParametersBlock);
							}
							else
							{
								parametersBlock = explicitBlock as ParametersBlock;
							}
							if (parametersBlock != null && parametersBlock.StateMachine != null)
							{
								if (parametersBlock.StateMachine == storey)
								{
									break;
								}
								ExplicitBlock explicitBlock2 = parametersBlock;
								while (explicitBlock2.Parent != null)
								{
									explicitBlock2 = explicitBlock2.Parent.Explicit;
									if (explicitBlock2.AnonymousMethodStorey != null)
									{
										break;
									}
								}
								if (explicitBlock2.AnonymousMethodStorey == null)
								{
									if (parametersBlock.StateMachine.HoistedThis == null)
									{
										parametersBlock.StateMachine.AddCapturedThisField(ec, null);
										explicitBlock.HasCapturedThis = true;
									}
									continue;
								}
								ParametersBlock parametersBlock2 = parametersBlock;
								while (parametersBlock2.Parent != null)
								{
									parametersBlock2 = parametersBlock2.Parent.ParametersBlock;
									if (parametersBlock2.StateMachine != null)
									{
										break;
									}
								}
								parametersBlock.StateMachine.AddParentStoreyReference(ec, parametersBlock2.StateMachine ?? storey);
							}
							if (anonymousMethodStorey2 != null)
							{
								anonymousMethodStorey2.AddParentStoreyReference(ec, storey);
								anonymousMethodStorey2.HoistedThis = storey.HoistedThis;
							}
						}
					}
				}
			}
			IList<ExplicitBlock> referencesFromChildrenBlock = storey.ReferencesFromChildrenBlock;
			if (referencesFromChildrenBlock != null)
			{
				foreach (ExplicitBlock item2 in referencesFromChildrenBlock)
				{
					ExplicitBlock explicitBlock3 = item2;
					while (explicitBlock3.AnonymousMethodStorey != storey)
					{
						if (explicitBlock3.AnonymousMethodStorey != null)
						{
							explicitBlock3.AnonymousMethodStorey.AddParentStoreyReference(ec, storey);
							if (explicitBlock3.ParametersBlock == ParametersBlock.Original)
							{
								break;
							}
							explicitBlock3 = explicitBlock3.ParametersBlock;
						}
						ParametersBlock parametersBlock3 = explicitBlock3 as ParametersBlock;
						if (parametersBlock3 != null && parametersBlock3.StateMachine != null)
						{
							if (parametersBlock3.StateMachine == storey)
							{
								break;
							}
							parametersBlock3.StateMachine.AddParentStoreyReference(ec, storey);
						}
						explicitBlock3.HasCapturedVariable = true;
						explicitBlock3 = explicitBlock3.Parent.Explicit;
					}
				}
			}
			storey.Define();
			storey.PrepareEmit();
			storey.Parent.PartialContainer.AddCompilerGeneratedClass(storey);
		}

		public void RegisterAsyncAwait()
		{
			ExplicitBlock explicitBlock = this;
			while ((explicitBlock.flags & Flags.AwaitBlock) == 0)
			{
				explicitBlock.flags |= Flags.AwaitBlock;
				if (explicitBlock is ParametersBlock)
				{
					break;
				}
				explicitBlock = explicitBlock.Parent.Explicit;
			}
		}

		public void RegisterIteratorYield()
		{
			ParametersBlock.TopBlock.IsIterator = true;
			ExplicitBlock explicitBlock = this;
			while ((explicitBlock.flags & Flags.YieldBlock) == 0)
			{
				explicitBlock.flags |= Flags.YieldBlock;
				if (explicitBlock.Parent == null)
				{
					break;
				}
				explicitBlock = explicitBlock.Parent.Explicit;
			}
		}

		public void SetCatchBlock()
		{
			flags |= Flags.CatchBlock;
		}

		public void SetFinallyBlock()
		{
			flags |= Flags.FinallyBlock;
		}

		public void WrapIntoDestructor(TryFinally tf, ExplicitBlock tryBlock)
		{
			tryBlock.statements = statements;
			statements = new List<Statement>(1);
			statements.Add(tf);
		}
	}
}
