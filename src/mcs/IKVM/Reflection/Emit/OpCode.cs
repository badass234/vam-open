namespace IKVM.Reflection.Emit
{
	public struct OpCode
	{
		private const int ValueCount = 1024;

		private const int OperandTypeCount = 19;

		private const int FlowControlCount = 9;

		private const int StackDiffCount = 5;

		private const int OpCodeTypeCount = 6;

		private const int StackBehaviourPopCount = 20;

		private const int StackBehaviourPushCount = 9;

		private static readonly StackBehaviour[] pop = new StackBehaviour[20]
		{
			StackBehaviour.Pop0,
			StackBehaviour.Pop1,
			StackBehaviour.Pop1_pop1,
			StackBehaviour.Popi,
			StackBehaviour.Popi_pop1,
			StackBehaviour.Popi_popi,
			StackBehaviour.Popi_popi8,
			StackBehaviour.Popi_popi_popi,
			StackBehaviour.Popi_popr4,
			StackBehaviour.Popi_popr8,
			StackBehaviour.Popref,
			StackBehaviour.Popref_pop1,
			StackBehaviour.Popref_popi,
			StackBehaviour.Popref_popi_popi,
			StackBehaviour.Popref_popi_popi8,
			StackBehaviour.Popref_popi_popr4,
			StackBehaviour.Popref_popi_popr8,
			StackBehaviour.Popref_popi_popref,
			StackBehaviour.Varpop,
			StackBehaviour.Popref_popi_pop1
		};

		private static readonly StackBehaviour[] push = new StackBehaviour[9]
		{
			StackBehaviour.Push0,
			StackBehaviour.Push1,
			StackBehaviour.Push1_push1,
			StackBehaviour.Pushi,
			StackBehaviour.Pushi8,
			StackBehaviour.Pushr4,
			StackBehaviour.Pushr8,
			StackBehaviour.Pushref,
			StackBehaviour.Varpush
		};

		private readonly int value;

		public short Value => (short)(value >> 22);

		public int Size
		{
			get
			{
				if (value >= 0)
				{
					return 1;
				}
				return 2;
			}
		}

		public string Name => OpCodes.GetName(Value);

		public OperandType OperandType => (OperandType)((value & 0x3FFFFF) % 19);

		public FlowControl FlowControl => (FlowControl)((value & 0x3FFFFF) / 19 % 9);

		internal int StackDiff => (value & 0x3FFFFF) / 171 % 5 - 3;

		public OpCodeType OpCodeType => (OpCodeType)((value & 0x3FFFFF) / 855 % 6);

		public StackBehaviour StackBehaviourPop => pop[(value & 0x3FFFFF) / 5130 % 20];

		public StackBehaviour StackBehaviourPush => push[(value & 0x3FFFFF) / 102600 % 9];

		internal OpCode(int value)
		{
			this.value = value;
		}

		public override bool Equals(object obj)
		{
			OpCode opCode = this;
			OpCode? opCode2 = obj as OpCode?;
			return opCode == opCode2;
		}

		public override int GetHashCode()
		{
			return value;
		}

		public bool Equals(OpCode other)
		{
			return this == other;
		}

		public static bool operator ==(OpCode a, OpCode b)
		{
			return a.value == b.value;
		}

		public static bool operator !=(OpCode a, OpCode b)
		{
			return !(a == b);
		}
	}
}
