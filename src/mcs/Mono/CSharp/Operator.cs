using System.Reflection;
using System.Text;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class Operator : MethodOrOperator
	{
		public enum OpType : byte
		{
			LogicalNot = 0,
			OnesComplement = 1,
			Increment = 2,
			Decrement = 3,
			True = 4,
			False = 5,
			Addition = 6,
			Subtraction = 7,
			UnaryPlus = 8,
			UnaryNegation = 9,
			Multiply = 10,
			Division = 11,
			Modulus = 12,
			BitwiseAnd = 13,
			BitwiseOr = 14,
			ExclusiveOr = 15,
			LeftShift = 16,
			RightShift = 17,
			Equality = 18,
			Inequality = 19,
			GreaterThan = 20,
			LessThan = 21,
			GreaterThanOrEqual = 22,
			LessThanOrEqual = 23,
			Implicit = 24,
			Explicit = 25,
			Is = 26,
			TOP = 27
		}

		private const Modifiers AllowedModifiers = Modifiers.AllowedExplicitImplFlags | Modifiers.PUBLIC | Modifiers.STATIC;

		public readonly OpType OperatorType;

		private static readonly string[][] names;

		static Operator()
		{
			names = new string[27][];
			names[0] = new string[2] { "!", "op_LogicalNot" };
			names[1] = new string[2] { "~", "op_OnesComplement" };
			names[2] = new string[2] { "++", "op_Increment" };
			names[3] = new string[2] { "--", "op_Decrement" };
			names[4] = new string[2] { "true", "op_True" };
			names[5] = new string[2] { "false", "op_False" };
			names[6] = new string[2] { "+", "op_Addition" };
			names[7] = new string[2] { "-", "op_Subtraction" };
			names[8] = new string[2] { "+", "op_UnaryPlus" };
			names[9] = new string[2] { "-", "op_UnaryNegation" };
			names[10] = new string[2] { "*", "op_Multiply" };
			names[11] = new string[2] { "/", "op_Division" };
			names[12] = new string[2] { "%", "op_Modulus" };
			names[13] = new string[2] { "&", "op_BitwiseAnd" };
			names[14] = new string[2] { "|", "op_BitwiseOr" };
			names[15] = new string[2] { "^", "op_ExclusiveOr" };
			names[16] = new string[2] { "<<", "op_LeftShift" };
			names[17] = new string[2] { ">>", "op_RightShift" };
			names[18] = new string[2] { "==", "op_Equality" };
			names[19] = new string[2] { "!=", "op_Inequality" };
			names[20] = new string[2] { ">", "op_GreaterThan" };
			names[21] = new string[2] { "<", "op_LessThan" };
			names[22] = new string[2] { ">=", "op_GreaterThanOrEqual" };
			names[23] = new string[2] { "<=", "op_LessThanOrEqual" };
			names[24] = new string[2] { "implicit", "op_Implicit" };
			names[25] = new string[2] { "explicit", "op_Explicit" };
			names[26] = new string[2] { "is", "op_Is" };
		}

		public Operator(TypeDefinition parent, OpType type, FullNamedExpression ret_type, Modifiers mod_flags, ParametersCompiled parameters, ToplevelBlock block, Attributes attrs, Location loc)
			: base(parent, ret_type, mod_flags, Modifiers.AllowedExplicitImplFlags | Modifiers.PUBLIC | Modifiers.STATIC, new MemberName(GetMetadataName(type), loc), attrs, parameters)
		{
			OperatorType = type;
			base.Block = block;
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.Conditional)
			{
				Error_ConditionalAttributeIsNotValid();
			}
			else
			{
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
		}

		public override bool Define()
		{
			if ((base.ModFlags & (Modifiers.PUBLIC | Modifiers.STATIC)) != (Modifiers.PUBLIC | Modifiers.STATIC))
			{
				base.Report.Error(558, base.Location, "User-defined operator `{0}' must be declared static and public", GetSignatureForError());
			}
			if (!base.Define())
			{
				return false;
			}
			if (block != null)
			{
				if (block.IsIterator)
				{
					Iterator.CreateIterator(this, Parent.PartialContainer, base.ModFlags);
					base.ModFlags |= Modifiers.DEBUGGER_HIDDEN;
				}
				if (Compiler.Settings.WriteMetadataOnly)
				{
					block = null;
				}
			}
			if (OperatorType == OpType.Explicit)
			{
				Parent.MemberCache.CheckExistingMembersOverloads(this, GetMetadataName(OpType.Implicit), parameters);
			}
			else if (OperatorType == OpType.Implicit)
			{
				Parent.MemberCache.CheckExistingMembersOverloads(this, GetMetadataName(OpType.Explicit), parameters);
			}
			TypeSpec currentType = Parent.PartialContainer.CurrentType;
			TypeSpec memberType = base.MemberType;
			TypeSpec typeSpec = base.ParameterTypes[0];
			TypeSpec typeSpec2 = typeSpec;
			if (typeSpec.IsNullableType)
			{
				typeSpec2 = NullableInfo.GetUnderlyingType(typeSpec);
			}
			TypeSpec typeSpec3 = memberType;
			if (memberType.IsNullableType)
			{
				typeSpec3 = NullableInfo.GetUnderlyingType(memberType);
			}
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit)
			{
				if (typeSpec2 == typeSpec3 && typeSpec2 == currentType)
				{
					base.Report.Error(555, base.Location, "User-defined operator cannot take an object of the enclosing type and convert to an object of the enclosing type");
					return false;
				}
				TypeSpec typeSpec4;
				if (currentType == memberType || currentType == typeSpec3)
				{
					typeSpec4 = typeSpec;
				}
				else
				{
					if (currentType != typeSpec && currentType != typeSpec2)
					{
						base.Report.Error(556, base.Location, "User-defined conversion must convert to or from the enclosing type");
						return false;
					}
					typeSpec4 = memberType;
				}
				if (typeSpec4.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					base.Report.Error(1964, base.Location, "User-defined conversion `{0}' cannot convert to or from the dynamic type", GetSignatureForError());
					return false;
				}
				if (typeSpec4.IsInterface)
				{
					base.Report.Error(552, base.Location, "User-defined conversion `{0}' cannot convert to or from an interface type", GetSignatureForError());
					return false;
				}
				if (typeSpec4.IsClass)
				{
					if (TypeSpec.IsBaseClass(currentType, typeSpec4, true))
					{
						base.Report.Error(553, base.Location, "User-defined conversion `{0}' cannot convert to or from a base class", GetSignatureForError());
						return false;
					}
					if (TypeSpec.IsBaseClass(typeSpec4, currentType, false))
					{
						base.Report.Error(554, base.Location, "User-defined conversion `{0}' cannot convert to or from a derived class", GetSignatureForError());
						return false;
					}
				}
			}
			else if (OperatorType == OpType.LeftShift || OperatorType == OpType.RightShift)
			{
				if (typeSpec != currentType || parameters.Types[1].BuiltinType != BuiltinTypeSpec.Type.Int)
				{
					base.Report.Error(564, base.Location, "Overloaded shift operator must have the type of the first operand be the containing type, and the type of the second operand must be int");
					return false;
				}
			}
			else if (parameters.Count == 1)
			{
				if (OperatorType == OpType.Increment || OperatorType == OpType.Decrement)
				{
					if (memberType != currentType && !TypeSpec.IsBaseClass(memberType, currentType, false))
					{
						base.Report.Error(448, base.Location, "The return type for ++ or -- operator must be the containing type or derived from the containing type");
						return false;
					}
					if (typeSpec != currentType)
					{
						base.Report.Error(559, base.Location, "The parameter type for ++ or -- operator must be the containing type");
						return false;
					}
				}
				if (typeSpec2 != currentType)
				{
					base.Report.Error(562, base.Location, "The parameter type of a unary operator must be the containing type");
					return false;
				}
				if ((OperatorType == OpType.True || OperatorType == OpType.False) && memberType.BuiltinType != BuiltinTypeSpec.Type.FirstPrimitive)
				{
					base.Report.Error(215, base.Location, "The return type of operator True or False must be bool");
					return false;
				}
			}
			else if (typeSpec2 != currentType)
			{
				TypeSpec typeSpec5 = base.ParameterTypes[1];
				if (typeSpec5.IsNullableType)
				{
					typeSpec5 = NullableInfo.GetUnderlyingType(typeSpec5);
				}
				if (typeSpec5 != currentType)
				{
					base.Report.Error(563, base.Location, "One of the parameters of a binary operator must be the containing type");
					return false;
				}
			}
			return true;
		}

		protected override bool ResolveMemberType()
		{
			if (!base.ResolveMemberType())
			{
				return false;
			}
			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			return true;
		}

		protected override MemberSpec FindBaseMember(out MemberSpec bestCandidate, ref bool overrides)
		{
			bestCandidate = null;
			return null;
		}

		public static string GetName(OpType ot)
		{
			return names[(uint)ot][0];
		}

		public static string GetName(string metadata_name)
		{
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i][1] == metadata_name)
				{
					return names[i][0];
				}
			}
			return null;
		}

		public static string GetMetadataName(OpType ot)
		{
			return names[(uint)ot][1];
		}

		public static string GetMetadataName(string name)
		{
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i][0] == name)
				{
					return names[i][1];
				}
			}
			return null;
		}

		public static OpType? GetType(string metadata_name)
		{
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i][1] == metadata_name)
				{
					return (OpType)i;
				}
			}
			return null;
		}

		public OpType GetMatchingOperator()
		{
			switch (OperatorType)
			{
			case OpType.Equality:
				return OpType.Inequality;
			case OpType.Inequality:
				return OpType.Equality;
			case OpType.True:
				return OpType.False;
			case OpType.False:
				return OpType.True;
			case OpType.GreaterThan:
				return OpType.LessThan;
			case OpType.LessThan:
				return OpType.GreaterThan;
			case OpType.GreaterThanOrEqual:
				return OpType.LessThanOrEqual;
			case OpType.LessThanOrEqual:
				return OpType.GreaterThanOrEqual;
			default:
				return OpType.TOP;
			}
		}

		public override string GetSignatureForDocumentation()
		{
			string text = base.GetSignatureForDocumentation();
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit)
			{
				text = text + "~" + base.ReturnType.GetSignatureForDocumentation();
			}
			return text;
		}

		public override string GetSignatureForError()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit)
			{
				stringBuilder.AppendFormat("{0}.{1} operator {2}", Parent.GetSignatureForError(), GetName(OperatorType), (member_type == null) ? type_expr.GetSignatureForError() : member_type.GetSignatureForError());
			}
			else
			{
				stringBuilder.AppendFormat("{0}.operator {1}", Parent.GetSignatureForError(), GetName(OperatorType));
			}
			stringBuilder.Append(parameters.GetSignatureForError());
			return stringBuilder.ToString();
		}
	}
}
