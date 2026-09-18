using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ConvCast : TypeCast
	{
		public enum Mode : byte
		{
			I1_U1 = 0,
			I1_U2 = 1,
			I1_U4 = 2,
			I1_U8 = 3,
			I1_CH = 4,
			U1_I1 = 5,
			U1_CH = 6,
			I2_I1 = 7,
			I2_U1 = 8,
			I2_U2 = 9,
			I2_U4 = 10,
			I2_U8 = 11,
			I2_CH = 12,
			U2_I1 = 13,
			U2_U1 = 14,
			U2_I2 = 15,
			U2_CH = 16,
			I4_I1 = 17,
			I4_U1 = 18,
			I4_I2 = 19,
			I4_U2 = 20,
			I4_U4 = 21,
			I4_U8 = 22,
			I4_CH = 23,
			U4_I1 = 24,
			U4_U1 = 25,
			U4_I2 = 26,
			U4_U2 = 27,
			U4_I4 = 28,
			U4_CH = 29,
			I8_I1 = 30,
			I8_U1 = 31,
			I8_I2 = 32,
			I8_U2 = 33,
			I8_I4 = 34,
			I8_U4 = 35,
			I8_U8 = 36,
			I8_CH = 37,
			I8_I = 38,
			U8_I1 = 39,
			U8_U1 = 40,
			U8_I2 = 41,
			U8_U2 = 42,
			U8_I4 = 43,
			U8_U4 = 44,
			U8_I8 = 45,
			U8_CH = 46,
			U8_I = 47,
			CH_I1 = 48,
			CH_U1 = 49,
			CH_I2 = 50,
			R4_I1 = 51,
			R4_U1 = 52,
			R4_I2 = 53,
			R4_U2 = 54,
			R4_I4 = 55,
			R4_U4 = 56,
			R4_I8 = 57,
			R4_U8 = 58,
			R4_CH = 59,
			R8_I1 = 60,
			R8_U1 = 61,
			R8_I2 = 62,
			R8_U2 = 63,
			R8_I4 = 64,
			R8_U4 = 65,
			R8_I8 = 66,
			R8_U8 = 67,
			R8_CH = 68,
			R8_R4 = 69,
			I_I8 = 70
		}

		private Mode mode;

		public ConvCast(Expression child, TypeSpec return_type, Mode m)
			: base(child, return_type)
		{
			mode = m;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return this;
		}

		public override string ToString()
		{
			return $"ConvCast ({mode}, {child})";
		}

		public override void Emit(EmitContext ec)
		{
			base.Emit(ec);
			Emit(ec, mode);
		}

		public static void Emit(EmitContext ec, Mode mode)
		{
			if (ec.HasSet(BuilderContext.Options.CheckedScope))
			{
				switch (mode)
				{
				case Mode.I1_U1:
					ec.Emit(OpCodes.Conv_Ovf_U1);
					break;
				case Mode.I1_U2:
					ec.Emit(OpCodes.Conv_Ovf_U2);
					break;
				case Mode.I1_U4:
					ec.Emit(OpCodes.Conv_Ovf_U4);
					break;
				case Mode.I1_U8:
					ec.Emit(OpCodes.Conv_Ovf_U8);
					break;
				case Mode.I1_CH:
					ec.Emit(OpCodes.Conv_Ovf_U2);
					break;
				case Mode.U1_I1:
					ec.Emit(OpCodes.Conv_Ovf_I1_Un);
					break;
				case Mode.I2_I1:
					ec.Emit(OpCodes.Conv_Ovf_I1);
					break;
				case Mode.I2_U1:
					ec.Emit(OpCodes.Conv_Ovf_U1);
					break;
				case Mode.I2_U2:
					ec.Emit(OpCodes.Conv_Ovf_U2);
					break;
				case Mode.I2_U4:
					ec.Emit(OpCodes.Conv_Ovf_U4);
					break;
				case Mode.I2_U8:
					ec.Emit(OpCodes.Conv_Ovf_U8);
					break;
				case Mode.I2_CH:
					ec.Emit(OpCodes.Conv_Ovf_U2);
					break;
				case Mode.U2_I1:
					ec.Emit(OpCodes.Conv_Ovf_I1_Un);
					break;
				case Mode.U2_U1:
					ec.Emit(OpCodes.Conv_Ovf_U1_Un);
					break;
				case Mode.U2_I2:
					ec.Emit(OpCodes.Conv_Ovf_I2_Un);
					break;
				case Mode.I4_I1:
					ec.Emit(OpCodes.Conv_Ovf_I1);
					break;
				case Mode.I4_U1:
					ec.Emit(OpCodes.Conv_Ovf_U1);
					break;
				case Mode.I4_I2:
					ec.Emit(OpCodes.Conv_Ovf_I2);
					break;
				case Mode.I4_U4:
					ec.Emit(OpCodes.Conv_Ovf_U4);
					break;
				case Mode.I4_U2:
					ec.Emit(OpCodes.Conv_Ovf_U2);
					break;
				case Mode.I4_U8:
					ec.Emit(OpCodes.Conv_Ovf_U8);
					break;
				case Mode.I4_CH:
					ec.Emit(OpCodes.Conv_Ovf_U2);
					break;
				case Mode.U4_I1:
					ec.Emit(OpCodes.Conv_Ovf_I1_Un);
					break;
				case Mode.U4_U1:
					ec.Emit(OpCodes.Conv_Ovf_U1_Un);
					break;
				case Mode.U4_I2:
					ec.Emit(OpCodes.Conv_Ovf_I2_Un);
					break;
				case Mode.U4_U2:
					ec.Emit(OpCodes.Conv_Ovf_U2_Un);
					break;
				case Mode.U4_I4:
					ec.Emit(OpCodes.Conv_Ovf_I4_Un);
					break;
				case Mode.U4_CH:
					ec.Emit(OpCodes.Conv_Ovf_U2_Un);
					break;
				case Mode.I8_I1:
					ec.Emit(OpCodes.Conv_Ovf_I1);
					break;
				case Mode.I8_U1:
					ec.Emit(OpCodes.Conv_Ovf_U1);
					break;
				case Mode.I8_I2:
					ec.Emit(OpCodes.Conv_Ovf_I2);
					break;
				case Mode.I8_U2:
					ec.Emit(OpCodes.Conv_Ovf_U2);
					break;
				case Mode.I8_I4:
					ec.Emit(OpCodes.Conv_Ovf_I4);
					break;
				case Mode.I8_U4:
					ec.Emit(OpCodes.Conv_Ovf_U4);
					break;
				case Mode.I8_U8:
					ec.Emit(OpCodes.Conv_Ovf_U8);
					break;
				case Mode.I8_CH:
					ec.Emit(OpCodes.Conv_Ovf_U2);
					break;
				case Mode.I8_I:
					ec.Emit(OpCodes.Conv_Ovf_U);
					break;
				case Mode.U8_I1:
					ec.Emit(OpCodes.Conv_Ovf_I1_Un);
					break;
				case Mode.U8_U1:
					ec.Emit(OpCodes.Conv_Ovf_U1_Un);
					break;
				case Mode.U8_I2:
					ec.Emit(OpCodes.Conv_Ovf_I2_Un);
					break;
				case Mode.U8_U2:
					ec.Emit(OpCodes.Conv_Ovf_U2_Un);
					break;
				case Mode.U8_I4:
					ec.Emit(OpCodes.Conv_Ovf_I4_Un);
					break;
				case Mode.U8_U4:
					ec.Emit(OpCodes.Conv_Ovf_U4_Un);
					break;
				case Mode.U8_I8:
					ec.Emit(OpCodes.Conv_Ovf_I8_Un);
					break;
				case Mode.U8_CH:
					ec.Emit(OpCodes.Conv_Ovf_U2_Un);
					break;
				case Mode.U8_I:
					ec.Emit(OpCodes.Conv_Ovf_U_Un);
					break;
				case Mode.CH_I1:
					ec.Emit(OpCodes.Conv_Ovf_I1_Un);
					break;
				case Mode.CH_U1:
					ec.Emit(OpCodes.Conv_Ovf_U1_Un);
					break;
				case Mode.CH_I2:
					ec.Emit(OpCodes.Conv_Ovf_I2_Un);
					break;
				case Mode.R4_I1:
					ec.Emit(OpCodes.Conv_Ovf_I1);
					break;
				case Mode.R4_U1:
					ec.Emit(OpCodes.Conv_Ovf_U1);
					break;
				case Mode.R4_I2:
					ec.Emit(OpCodes.Conv_Ovf_I2);
					break;
				case Mode.R4_U2:
					ec.Emit(OpCodes.Conv_Ovf_U2);
					break;
				case Mode.R4_I4:
					ec.Emit(OpCodes.Conv_Ovf_I4);
					break;
				case Mode.R4_U4:
					ec.Emit(OpCodes.Conv_Ovf_U4);
					break;
				case Mode.R4_I8:
					ec.Emit(OpCodes.Conv_Ovf_I8);
					break;
				case Mode.R4_U8:
					ec.Emit(OpCodes.Conv_Ovf_U8);
					break;
				case Mode.R4_CH:
					ec.Emit(OpCodes.Conv_Ovf_U2);
					break;
				case Mode.R8_I1:
					ec.Emit(OpCodes.Conv_Ovf_I1);
					break;
				case Mode.R8_U1:
					ec.Emit(OpCodes.Conv_Ovf_U1);
					break;
				case Mode.R8_I2:
					ec.Emit(OpCodes.Conv_Ovf_I2);
					break;
				case Mode.R8_U2:
					ec.Emit(OpCodes.Conv_Ovf_U2);
					break;
				case Mode.R8_I4:
					ec.Emit(OpCodes.Conv_Ovf_I4);
					break;
				case Mode.R8_U4:
					ec.Emit(OpCodes.Conv_Ovf_U4);
					break;
				case Mode.R8_I8:
					ec.Emit(OpCodes.Conv_Ovf_I8);
					break;
				case Mode.R8_U8:
					ec.Emit(OpCodes.Conv_Ovf_U8);
					break;
				case Mode.R8_CH:
					ec.Emit(OpCodes.Conv_Ovf_U2);
					break;
				case Mode.R8_R4:
					ec.Emit(OpCodes.Conv_R4);
					break;
				case Mode.I_I8:
					ec.Emit(OpCodes.Conv_Ovf_I8_Un);
					break;
				case Mode.U1_CH:
				case Mode.U2_CH:
					break;
				}
			}
			else
			{
				switch (mode)
				{
				case Mode.I1_U1:
					ec.Emit(OpCodes.Conv_U1);
					break;
				case Mode.I1_U2:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.I1_U4:
					ec.Emit(OpCodes.Conv_U4);
					break;
				case Mode.I1_U8:
					ec.Emit(OpCodes.Conv_I8);
					break;
				case Mode.I1_CH:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.U1_I1:
					ec.Emit(OpCodes.Conv_I1);
					break;
				case Mode.U1_CH:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.I2_I1:
					ec.Emit(OpCodes.Conv_I1);
					break;
				case Mode.I2_U1:
					ec.Emit(OpCodes.Conv_U1);
					break;
				case Mode.I2_U2:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.I2_U4:
					ec.Emit(OpCodes.Conv_U4);
					break;
				case Mode.I2_U8:
					ec.Emit(OpCodes.Conv_I8);
					break;
				case Mode.I2_CH:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.U2_I1:
					ec.Emit(OpCodes.Conv_I1);
					break;
				case Mode.U2_U1:
					ec.Emit(OpCodes.Conv_U1);
					break;
				case Mode.U2_I2:
					ec.Emit(OpCodes.Conv_I2);
					break;
				case Mode.I4_I1:
					ec.Emit(OpCodes.Conv_I1);
					break;
				case Mode.I4_U1:
					ec.Emit(OpCodes.Conv_U1);
					break;
				case Mode.I4_I2:
					ec.Emit(OpCodes.Conv_I2);
					break;
				case Mode.I4_U2:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.I4_U8:
					ec.Emit(OpCodes.Conv_I8);
					break;
				case Mode.I4_CH:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.U4_I1:
					ec.Emit(OpCodes.Conv_I1);
					break;
				case Mode.U4_U1:
					ec.Emit(OpCodes.Conv_U1);
					break;
				case Mode.U4_I2:
					ec.Emit(OpCodes.Conv_I2);
					break;
				case Mode.U4_U2:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.U4_CH:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.I8_I1:
					ec.Emit(OpCodes.Conv_I1);
					break;
				case Mode.I8_U1:
					ec.Emit(OpCodes.Conv_U1);
					break;
				case Mode.I8_I2:
					ec.Emit(OpCodes.Conv_I2);
					break;
				case Mode.I8_U2:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.I8_I4:
					ec.Emit(OpCodes.Conv_I4);
					break;
				case Mode.I8_U4:
					ec.Emit(OpCodes.Conv_U4);
					break;
				case Mode.I8_CH:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.I8_I:
					ec.Emit(OpCodes.Conv_U);
					break;
				case Mode.U8_I1:
					ec.Emit(OpCodes.Conv_I1);
					break;
				case Mode.U8_U1:
					ec.Emit(OpCodes.Conv_U1);
					break;
				case Mode.U8_I2:
					ec.Emit(OpCodes.Conv_I2);
					break;
				case Mode.U8_U2:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.U8_I4:
					ec.Emit(OpCodes.Conv_I4);
					break;
				case Mode.U8_U4:
					ec.Emit(OpCodes.Conv_U4);
					break;
				case Mode.U8_CH:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.U8_I:
					ec.Emit(OpCodes.Conv_U);
					break;
				case Mode.CH_I1:
					ec.Emit(OpCodes.Conv_I1);
					break;
				case Mode.CH_U1:
					ec.Emit(OpCodes.Conv_U1);
					break;
				case Mode.CH_I2:
					ec.Emit(OpCodes.Conv_I2);
					break;
				case Mode.R4_I1:
					ec.Emit(OpCodes.Conv_I1);
					break;
				case Mode.R4_U1:
					ec.Emit(OpCodes.Conv_U1);
					break;
				case Mode.R4_I2:
					ec.Emit(OpCodes.Conv_I2);
					break;
				case Mode.R4_U2:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.R4_I4:
					ec.Emit(OpCodes.Conv_I4);
					break;
				case Mode.R4_U4:
					ec.Emit(OpCodes.Conv_U4);
					break;
				case Mode.R4_I8:
					ec.Emit(OpCodes.Conv_I8);
					break;
				case Mode.R4_U8:
					ec.Emit(OpCodes.Conv_U8);
					break;
				case Mode.R4_CH:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.R8_I1:
					ec.Emit(OpCodes.Conv_I1);
					break;
				case Mode.R8_U1:
					ec.Emit(OpCodes.Conv_U1);
					break;
				case Mode.R8_I2:
					ec.Emit(OpCodes.Conv_I2);
					break;
				case Mode.R8_U2:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.R8_I4:
					ec.Emit(OpCodes.Conv_I4);
					break;
				case Mode.R8_U4:
					ec.Emit(OpCodes.Conv_U4);
					break;
				case Mode.R8_I8:
					ec.Emit(OpCodes.Conv_I8);
					break;
				case Mode.R8_U8:
					ec.Emit(OpCodes.Conv_U8);
					break;
				case Mode.R8_CH:
					ec.Emit(OpCodes.Conv_U2);
					break;
				case Mode.R8_R4:
					ec.Emit(OpCodes.Conv_R4);
					break;
				case Mode.I_I8:
					ec.Emit(OpCodes.Conv_U8);
					break;
				case Mode.U2_CH:
				case Mode.I4_U4:
				case Mode.U4_I4:
				case Mode.I8_U8:
				case Mode.U8_I8:
					break;
				}
			}
		}
	}
}
