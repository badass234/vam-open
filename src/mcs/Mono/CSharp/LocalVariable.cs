using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public sealed class LocalVariable : INamedBlockVariable, ILocalVariable
	{
		[Flags]
		public enum Flags
		{
			Used = 1,
			IsThis = 2,
			AddressTaken = 4,
			CompilerGenerated = 8,
			Constant = 0x10,
			ForeachVariable = 0x20,
			FixedVariable = 0x40,
			UsingVariable = 0x80,
			IsLocked = 0x100,
			ReadonlyMask = ForeachVariable | FixedVariable | UsingVariable
		}

		private TypeSpec type;

		private readonly string name;

		private readonly Location loc;

		private readonly Block block;

		private Flags flags;

		private Constant const_value;

		public VariableInfo VariableInfo;

		private HoistedVariable hoisted_variant;

		private LocalBuilder builder;

		public bool AddressTaken => (flags & Flags.AddressTaken) != 0;

		public Block Block => block;

		public Constant ConstantValue
		{
			get
			{
				return const_value;
			}
			set
			{
				const_value = value;
			}
		}

		public HoistedVariable HoistedVariant
		{
			get
			{
				return hoisted_variant;
			}
			set
			{
				hoisted_variant = value;
			}
		}

		public bool IsDeclared => type != null;

		public bool IsCompilerGenerated => (flags & Flags.CompilerGenerated) != 0;

		public bool IsConstant => (flags & Flags.Constant) != 0;

		public bool IsLocked
		{
			get
			{
				return (flags & Flags.IsLocked) != 0;
			}
			set
			{
				flags = (value ? (flags | Flags.IsLocked) : (flags & ~Flags.IsLocked));
			}
		}

		public bool IsThis => (flags & Flags.IsThis) != 0;

		public bool IsFixed => (flags & Flags.FixedVariable) != 0;

		bool INamedBlockVariable.IsParameter => false;

		public bool IsReadonly => (flags & Flags.ReadonlyMask) != 0;

		public Location Location => loc;

		public string Name => name;

		public TypeSpec Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
			}
		}

		public LocalVariable(Block block, string name, Location loc)
		{
			this.block = block;
			this.name = name;
			this.loc = loc;
		}

		public LocalVariable(Block block, string name, Flags flags, Location loc)
			: this(block, name, loc)
		{
			this.flags = flags;
		}

		public LocalVariable(LocalVariable li, string name, Location loc)
			: this(li.block, name, li.flags, loc)
		{
		}

		public void CreateBuilder(EmitContext ec)
		{
			if ((flags & Flags.Used) == 0)
			{
				if (VariableInfo == null)
				{
					throw new InternalErrorException("VariableInfo is null and the variable `{0}' is not used", name);
				}
				if (VariableInfo.IsEverAssigned)
				{
					ec.Report.Warning(219, 3, Location, "The variable `{0}' is assigned but its value is never used", Name);
				}
				else
				{
					ec.Report.Warning(168, 3, Location, "The variable `{0}' is declared but never used", Name);
				}
			}
			if (HoistedVariant != null)
			{
				return;
			}
			if (builder != null)
			{
				if ((flags & Flags.CompilerGenerated) == 0)
				{
					throw new InternalErrorException("Already created variable `{0}'", name);
				}
			}
			else
			{
				builder = ec.DeclareLocal(Type, IsFixed);
				if (!ec.HasSet(BuilderContext.Options.OmitDebugInfo) && (flags & Flags.CompilerGenerated) == 0)
				{
					ec.DefineLocalVariable(name, builder);
				}
			}
		}

		public static LocalVariable CreateCompilerGenerated(TypeSpec type, Block block, Location loc)
		{
			return new LocalVariable(block, GetCompilerGeneratedName(block), Flags.Used | Flags.CompilerGenerated, loc)
			{
				Type = type
			};
		}

		public Expression CreateReferenceExpression(ResolveContext rc, Location loc)
		{
			if (IsConstant && const_value != null)
			{
				return Constant.CreateConstantFromValue(Type, const_value.GetValue(), loc);
			}
			return new LocalVariableReference(this, loc);
		}

		public void Emit(EmitContext ec)
		{
			if ((flags & Flags.CompilerGenerated) != 0)
			{
				CreateBuilder(ec);
			}
			ec.Emit(OpCodes.Ldloc, builder);
		}

		public void EmitAssign(EmitContext ec)
		{
			if ((flags & Flags.CompilerGenerated) != 0)
			{
				CreateBuilder(ec);
			}
			ec.Emit(OpCodes.Stloc, builder);
		}

		public void EmitAddressOf(EmitContext ec)
		{
			if ((flags & Flags.CompilerGenerated) != 0)
			{
				CreateBuilder(ec);
			}
			ec.Emit(OpCodes.Ldloca, builder);
		}

		public static string GetCompilerGeneratedName(Block block)
		{
			return "$locvar" + block.ParametersBlock.TemporaryLocalsCount++.ToString("X");
		}

		public string GetReadOnlyContext()
		{
			switch (flags & Flags.ReadonlyMask)
			{
			case Flags.FixedVariable:
				return "fixed variable";
			case Flags.ForeachVariable:
				return "foreach iteration variable";
			case Flags.UsingVariable:
				return "using variable";
			default:
				throw new InternalErrorException("Variable is not readonly");
			}
		}

		public bool IsThisAssigned(FlowAnalysisContext fc, Block block)
		{
			if (VariableInfo == null)
			{
				throw new Exception();
			}
			if (IsAssigned(fc))
			{
				return true;
			}
			return VariableInfo.IsFullyInitialized(fc, block.StartLocation);
		}

		public bool IsAssigned(FlowAnalysisContext fc)
		{
			return fc.IsDefinitelyAssigned(VariableInfo);
		}

		public void PrepareAssignmentAnalysis(BlockContext bc)
		{
			if ((flags & (Flags.ReadonlyMask | Flags.CompilerGenerated | Flags.Constant)) == 0)
			{
				VariableInfo = VariableInfo.Create(bc, this);
			}
		}

		public void SetIsUsed()
		{
			flags |= Flags.Used;
		}

		public void SetHasAddressTaken()
		{
			flags |= Flags.Used | Flags.AddressTaken;
		}

		public override string ToString()
		{
			return $"LocalInfo ({name},{type},{VariableInfo},{Location})";
		}
	}
}
