using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class Switch : LoopStatement
	{
		private sealed class LabelsRange : IComparable<LabelsRange>
		{
			public readonly long min;

			public long max;

			public readonly List<long> label_values;

			public long Range => max - min + 1;

			public LabelsRange(long value)
			{
				min = (max = value);
				label_values = new List<long>();
				label_values.Add(value);
			}

			public LabelsRange(long min, long max, ICollection<long> values)
			{
				this.min = min;
				this.max = max;
				label_values = new List<long>(values);
			}

			public bool AddValue(long value)
			{
				long num = value - min + 1;
				if (num > 2 * (label_values.Count + 1) || num <= 0)
				{
					return false;
				}
				max = value;
				label_values.Add(value);
				return true;
			}

			public int CompareTo(LabelsRange other)
			{
				int count = label_values.Count;
				int count2 = other.label_values.Count;
				if (count2 == count)
				{
					return (int)(other.min - min);
				}
				return count - count2;
			}
		}

		private sealed class DispatchStatement : Statement
		{
			private readonly Switch body;

			public DispatchStatement(Switch body)
			{
				this.body = body;
			}

			protected override void CloneTo(CloneContext clonectx, Statement target)
			{
				throw new NotImplementedException();
			}

			protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
			{
				return false;
			}

			protected override void DoEmit(EmitContext ec)
			{
				body.EmitDispatch(ec);
			}
		}

		private class MissingBreak : Statement
		{
			private readonly SwitchLabel label;

			public bool FallOut { get; set; }

			public MissingBreak(SwitchLabel sl)
			{
				label = sl;
				loc = sl.loc;
			}

			protected override void DoEmit(EmitContext ec)
			{
			}

			protected override void CloneTo(CloneContext clonectx, Statement target)
			{
			}

			protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
			{
				if (FallOut)
				{
					fc.Report.Error(8070, loc, "Control cannot fall out of switch statement through final case label `{0}'", label.GetSignatureForError());
				}
				else
				{
					fc.Report.Error(163, loc, "Control cannot fall through from one case label `{0}' to another", label.GetSignatureForError());
				}
				return true;
			}
		}

		public Expression Expr;

		private Dictionary<long, SwitchLabel> labels;

		private Dictionary<string, SwitchLabel> string_labels;

		private List<SwitchLabel> case_labels;

		private List<Tuple<GotoCase, Constant>> goto_cases;

		private List<DefiniteAssignmentBitSet> end_reachable_das;

		public TypeSpec SwitchType;

		private Expression new_expr;

		private SwitchLabel case_null;

		private SwitchLabel case_default;

		private Label defaultLabel;

		private Label nullLabel;

		private VariableReference value;

		private ExpressionStatement string_dictionary;

		private FieldExpr switch_cache_field;

		private ExplicitBlock block;

		private bool end_reachable;

		private Unwrap unwrap;

		public SwitchLabel ActiveLabel { get; set; }

		public ExplicitBlock Block => block;

		public SwitchLabel DefaultLabel => case_default;

		public bool IsNullable => unwrap != null;

		public bool IsPatternMatching
		{
			get
			{
				if (new_expr == null)
				{
					return SwitchType != null;
				}
				return false;
			}
		}

		public List<SwitchLabel> RegisteredLabels => case_labels;

		public VariableReference ExpressionValue => value;

		public Switch(Expression e, ExplicitBlock block, Location l)
			: base(block)
		{
			Expr = e;
			this.block = block;
			loc = l;
		}

		private static Expression SwitchGoverningType(ResolveContext rc, Expression expr, bool unwrapExpr)
		{
			switch (expr.Type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.FirstPrimitive:
			case BuiltinTypeSpec.Type.Byte:
			case BuiltinTypeSpec.Type.SByte:
			case BuiltinTypeSpec.Type.Char:
			case BuiltinTypeSpec.Type.Short:
			case BuiltinTypeSpec.Type.UShort:
			case BuiltinTypeSpec.Type.Int:
			case BuiltinTypeSpec.Type.UInt:
			case BuiltinTypeSpec.Type.Long:
			case BuiltinTypeSpec.Type.ULong:
			case BuiltinTypeSpec.Type.String:
				return expr;
			default:
			{
				if (expr.Type.IsEnum)
				{
					return expr;
				}
				Expression expression = null;
				TypeSpec[] switchUserTypes = rc.Module.PredefinedTypes.SwitchUserTypes;
				foreach (TypeSpec typeSpec in switchUserTypes)
				{
					if (!unwrapExpr && typeSpec.IsNullableType && expr.Type.IsNullableType)
					{
						break;
					}
					Convert.UserConversionRestriction userConversionRestriction = Convert.UserConversionRestriction.ImplicitOnly | Convert.UserConversionRestriction.ProbingOnly;
					if (unwrapExpr)
					{
						userConversionRestriction |= Convert.UserConversionRestriction.NullableSourceOnly;
					}
					Expression expression2 = Convert.UserDefinedConversion(rc, expr, typeSpec, userConversionRestriction, Location.Null);
					if (expression2 != null && expression2 is UserCast)
					{
						if (expression != null)
						{
							return null;
						}
						expression = expression2;
					}
				}
				return expression;
			}
			}
		}

		public static TypeSpec[] CreateSwitchUserTypes(ModuleContainer module, TypeSpec nullable)
		{
			BuiltinTypes builtinTypes = module.Compiler.BuiltinTypes;
			TypeSpec[] array = new BuiltinTypeSpec[10] { builtinTypes.SByte, builtinTypes.Byte, builtinTypes.Short, builtinTypes.UShort, builtinTypes.Int, builtinTypes.UInt, builtinTypes.Long, builtinTypes.ULong, builtinTypes.Char, builtinTypes.String };
			if (nullable != null)
			{
				Array.Resize(ref array, array.Length + 9);
				for (int i = 0; i < 9; i++)
				{
					array[10 + i] = nullable.MakeGenericType(module, new TypeSpec[1] { array[i] });
				}
			}
			return array;
		}

		public void RegisterLabel(BlockContext rc, SwitchLabel sl)
		{
			case_labels.Add(sl);
			if (sl.IsDefault)
			{
				if (case_default != null)
				{
					sl.Error_AlreadyOccurs(rc, case_default);
				}
				else
				{
					case_default = sl;
				}
			}
			else
			{
				if (sl.Converted == null)
				{
					return;
				}
				try
				{
					if (string_labels != null)
					{
						string text = sl.Converted.GetValue() as string;
						if (text == null)
						{
							case_null = sl;
						}
						else
						{
							string_labels.Add(text, sl);
						}
					}
					else if (sl.Converted.IsNull)
					{
						case_null = sl;
					}
					else
					{
						labels.Add(sl.Converted.GetValueAsLong(), sl);
					}
				}
				catch (ArgumentException)
				{
					if (string_labels != null)
					{
						sl.Error_AlreadyOccurs(rc, string_labels[(string)sl.Converted.GetValue()]);
					}
					else
					{
						sl.Error_AlreadyOccurs(rc, labels[sl.Converted.GetValueAsLong()]);
					}
				}
			}
		}

		private void EmitTableSwitch(EmitContext ec, Expression val)
		{
			if (labels == null || labels.Count <= 0)
			{
				return;
			}
			List<LabelsRange> list;
			if (string_labels != null)
			{
				list = new List<LabelsRange>(1);
				list.Add(new LabelsRange(0L, labels.Count - 1, labels.Keys));
			}
			else
			{
				long[] array = new long[labels.Count];
				labels.Keys.CopyTo(array, 0);
				Array.Sort(array);
				list = new List<LabelsRange>(array.Length);
				LabelsRange labelsRange = new LabelsRange(array[0]);
				list.Add(labelsRange);
				for (int i = 1; i < array.Length; i++)
				{
					long num = array[i];
					if (!labelsRange.AddValue(num))
					{
						labelsRange = new LabelsRange(num);
						list.Add(labelsRange);
					}
				}
				list.Sort();
			}
			Label label = defaultLabel;
			TypeSpec typeSpec = (SwitchType.IsEnum ? EnumSpec.GetUnderlyingType(SwitchType) : SwitchType);
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				LabelsRange labelsRange2 = list[num2];
				label = ((num2 == 0) ? defaultLabel : ec.DefineLabel());
				if (labelsRange2.Range <= 2)
				{
					foreach (long label_value in labelsRange2.label_values)
					{
						SwitchLabel switchLabel = labels[label_value];
						if (switchLabel != case_default && switchLabel != case_null)
						{
							if (switchLabel.Converted.IsZeroInteger)
							{
								val.EmitBranchable(ec, switchLabel.GetILLabel(ec), false);
								continue;
							}
							val.Emit(ec);
							switchLabel.Converted.Emit(ec);
							ec.Emit(OpCodes.Beq, switchLabel.GetILLabel(ec));
						}
					}
				}
				else
				{
					if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Long || typeSpec.BuiltinType == BuiltinTypeSpec.Type.ULong)
					{
						val.Emit(ec);
						ec.EmitLong(labelsRange2.min);
						ec.Emit(OpCodes.Blt, label);
						val.Emit(ec);
						ec.EmitLong(labelsRange2.max);
						ec.Emit(OpCodes.Bgt, label);
						val.Emit(ec);
						if (labelsRange2.min != 0L)
						{
							ec.EmitLong(labelsRange2.min);
							ec.Emit(OpCodes.Sub);
						}
						ec.Emit(OpCodes.Conv_I4);
					}
					else
					{
						val.Emit(ec);
						int num3 = (int)labelsRange2.min;
						if (num3 > 0)
						{
							ec.EmitInt(num3);
							ec.Emit(OpCodes.Sub);
						}
						else if (num3 < 0)
						{
							ec.EmitInt(-num3);
							ec.Emit(OpCodes.Add);
						}
					}
					int num4 = 0;
					long range = labelsRange2.Range;
					Label[] array2 = new Label[range];
					for (int j = 0; j < range; j++)
					{
						long num5 = labelsRange2.label_values[num4];
						if (num5 == labelsRange2.min + j)
						{
							array2[j] = labels[num5].GetILLabel(ec);
							num4++;
						}
						else
						{
							array2[j] = label;
						}
					}
					ec.Emit(OpCodes.Switch, array2);
				}
				if (num2 != 0)
				{
					ec.MarkLabel(label);
				}
			}
			if (list.Count > 0)
			{
				ec.Emit(OpCodes.Br, label);
			}
		}

		public SwitchLabel FindLabel(Constant value)
		{
			SwitchLabel switchLabel = null;
			if (string_labels != null)
			{
				string text = value.GetValue() as string;
				if (text == null)
				{
					if (case_null != null)
					{
						switchLabel = case_null;
					}
					else if (case_default != null)
					{
						switchLabel = case_default;
					}
				}
				else
				{
					string_labels.TryGetValue(text, out switchLabel);
				}
			}
			else if (value is NullLiteral)
			{
				switchLabel = case_null;
			}
			else
			{
				labels.TryGetValue(value.GetValueAsLong(), out switchLabel);
			}
			if (switchLabel == null || switchLabel.SectionStart)
			{
				return switchLabel;
			}
			int num = case_labels.IndexOf(switchLabel);
			SwitchLabel switchLabel2;
			while (true)
			{
				switchLabel2 = case_labels[num];
				if (switchLabel2.SectionStart)
				{
					break;
				}
				num--;
			}
			return switchLabel2;
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			Expr.FlowAnalysis(fc);
			DefiniteAssignmentBitSet switchInitialDefinitiveAssignment = fc.SwitchInitialDefinitiveAssignment;
			DefiniteAssignmentBitSet definiteAssignment = (fc.SwitchInitialDefinitiveAssignment = fc.DefiniteAssignment);
			block.FlowAnalysis(fc);
			fc.SwitchInitialDefinitiveAssignment = switchInitialDefinitiveAssignment;
			if (end_reachable_das != null)
			{
				DefiniteAssignmentBitSet definiteAssignmentBitSet = DefiniteAssignmentBitSet.And(end_reachable_das);
				definiteAssignment |= definiteAssignmentBitSet;
				end_reachable_das = null;
			}
			fc.DefiniteAssignment = definiteAssignment;
			if (case_default != null)
			{
				return !end_reachable;
			}
			return false;
		}

		public override bool Resolve(BlockContext ec)
		{
			Expr = Expr.Resolve(ec);
			if (Expr == null)
			{
				return false;
			}
			new_expr = SwitchGoverningType(ec, Expr, false);
			if (new_expr == null && Expr.Type.IsNullableType)
			{
				unwrap = Unwrap.Create(Expr, false);
				if (unwrap == null)
				{
					return false;
				}
				new_expr = SwitchGoverningType(ec, unwrap, true);
			}
			Expression expr;
			if (new_expr == null)
			{
				if (ec.Module.Compiler.Settings.Version != LanguageVersion.Experimental)
				{
					if (Expr.Type != InternalType.ErrorType)
					{
						ec.Report.Error(151, loc, "A switch expression of type `{0}' cannot be converted to an integral type, bool, char, string, enum or nullable type", Expr.Type.GetSignatureForError());
					}
					return false;
				}
				expr = Expr;
				SwitchType = Expr.Type;
			}
			else
			{
				expr = new_expr;
				SwitchType = new_expr.Type;
				if (SwitchType.IsNullableType)
				{
					new_expr = (unwrap = Unwrap.Create(new_expr, true));
					SwitchType = NullableInfo.GetUnderlyingType(SwitchType);
				}
				if (SwitchType.BuiltinType == BuiltinTypeSpec.Type.FirstPrimitive && ec.Module.Compiler.Settings.Version == LanguageVersion.ISO_1)
				{
					ec.Report.FeatureIsNotAvailable(ec.Module.Compiler, loc, "switch expression of boolean type");
					return false;
				}
				if (block.Statements.Count == 0)
				{
					return true;
				}
				if (SwitchType.BuiltinType == BuiltinTypeSpec.Type.String)
				{
					string_labels = new Dictionary<string, SwitchLabel>();
				}
				else
				{
					labels = new Dictionary<long, SwitchLabel>();
				}
			}
			Constant constant = expr as Constant;
			if (constant == null)
			{
				value = expr as VariableReference;
				if (value == null && !HasOnlyDefaultSection())
				{
					Block currentBlock = ec.CurrentBlock;
					ec.CurrentBlock = Block;
					value = TemporaryVariableReference.Create(SwitchType, ec.CurrentBlock, loc);
					value.Resolve(ec);
					ec.CurrentBlock = currentBlock;
				}
			}
			case_labels = new List<SwitchLabel>();
			Switch obj = ec.Switch;
			ec.Switch = this;
			LoopStatement enclosingLoopOrSwitch = ec.EnclosingLoopOrSwitch;
			ec.EnclosingLoopOrSwitch = this;
			bool flag = base.Statement.Resolve(ec);
			ec.EnclosingLoopOrSwitch = enclosingLoopOrSwitch;
			ec.Switch = obj;
			if (goto_cases != null)
			{
				foreach (Tuple<GotoCase, Constant> goto_case in goto_cases)
				{
					if (goto_case.Item1 == null)
					{
						if (DefaultLabel == null)
						{
							Goto.Error_UnknownLabel(ec, "default", loc);
						}
						continue;
					}
					SwitchLabel switchLabel = FindLabel(goto_case.Item2);
					if (switchLabel == null)
					{
						Goto.Error_UnknownLabel(ec, "case " + goto_case.Item2.GetValueAsLiteral(), loc);
					}
					else
					{
						goto_case.Item1.Label = switchLabel;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
			if (constant == null && SwitchType.BuiltinType == BuiltinTypeSpec.Type.String && string_labels.Count > 6)
			{
				ResolveStringSwitchMap(ec);
			}
			block.InsertStatement(0, new DispatchStatement(this));
			return true;
		}

		private bool HasOnlyDefaultSection()
		{
			for (int i = 0; i < block.Statements.Count; i++)
			{
				SwitchLabel switchLabel = block.Statements[i] as SwitchLabel;
				if (switchLabel != null && !switchLabel.IsDefault)
				{
					return false;
				}
			}
			return true;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			if (rc.IsUnreachable)
			{
				return rc;
			}
			base.MarkReachable(rc);
			block.MarkReachableScope(rc);
			if (block.Statements.Count == 0)
			{
				return rc;
			}
			SwitchLabel switchLabel = null;
			Constant constant = new_expr as Constant;
			if (constant != null)
			{
				switchLabel = FindLabel(constant) ?? case_default;
				if (switchLabel == null)
				{
					block.Statements.RemoveAt(0);
					return rc;
				}
			}
			Reachability rc2 = default(Reachability);
			SwitchLabel switchLabel2 = null;
			for (int i = 0; i < block.Statements.Count; i++)
			{
				Statement statement = block.Statements[i];
				SwitchLabel switchLabel3 = statement as SwitchLabel;
				if (switchLabel3 != null && switchLabel3.SectionStart)
				{
					if (!switchLabel3.IsUnreachable)
					{
						rc2 = default(Reachability);
						continue;
					}
					if (switchLabel != null && switchLabel != switchLabel3)
					{
						rc2 = Reachability.CreateUnreachable();
					}
					else if (rc2.IsUnreachable)
					{
						rc2 = default(Reachability);
					}
					else if (switchLabel2 != null)
					{
						switchLabel3.SectionStart = false;
						statement = new MissingBreak(switchLabel2);
						statement.MarkReachable(rc);
						block.Statements.Insert(i - 1, statement);
						i++;
					}
					switchLabel2 = switchLabel3;
				}
				rc2 = statement.MarkReachable(rc2);
			}
			if (!rc2.IsUnreachable && switchLabel2 != null)
			{
				switchLabel2.SectionStart = false;
				MissingBreak missingBreak = new MissingBreak(switchLabel2)
				{
					FallOut = true
				};
				missingBreak.MarkReachable(rc);
				block.Statements.Add(missingBreak);
			}
			if (case_default == null && switchLabel == null)
			{
				return rc;
			}
			if (end_reachable)
			{
				return rc;
			}
			return Reachability.CreateUnreachable();
		}

		public void RegisterGotoCase(GotoCase gotoCase, Constant value)
		{
			if (goto_cases == null)
			{
				goto_cases = new List<Tuple<GotoCase, Constant>>();
			}
			goto_cases.Add(Tuple.Create(gotoCase, value));
		}

		private void ResolveStringSwitchMap(ResolveContext ec)
		{
			FullNamedExpression fullNamedExpression;
			if (ec.Module.PredefinedTypes.Dictionary.Define())
			{
				fullNamedExpression = new TypeExpression(ec.Module.PredefinedTypes.Dictionary.TypeSpec.MakeGenericType(ec, new BuiltinTypeSpec[2]
				{
					ec.BuiltinTypes.String,
					ec.BuiltinTypes.Int
				}), loc);
			}
			else
			{
				if (!ec.Module.PredefinedTypes.Hashtable.Define())
				{
					ec.Module.PredefinedTypes.Dictionary.Resolve();
					return;
				}
				fullNamedExpression = new TypeExpression(ec.Module.PredefinedTypes.Hashtable.TypeSpec, loc);
			}
			TypeDefinition partialContainer = ec.CurrentMemberDefinition.Parent.PartialContainer;
			Field field = new Field(partialContainer, fullNamedExpression, Modifiers.PRIVATE | Modifiers.STATIC | Modifiers.COMPILER_GENERATED, new MemberName(CompilerGeneratedContainer.MakeName(null, "f", "switch$map", ec.Module.CounterSwitchTypes++), loc), null);
			if (!field.Define())
			{
				return;
			}
			partialContainer.AddField(field);
			List<Expression> list = new List<Expression>();
			int num = -1;
			labels = new Dictionary<long, SwitchLabel>(string_labels.Count);
			string text = null;
			foreach (SwitchLabel case_label in case_labels)
			{
				if (case_label.SectionStart)
				{
					labels.Add(++num, case_label);
				}
				if (case_label != case_default && case_label != case_null)
				{
					text = (string)case_label.Converted.GetValue();
					List<Expression> list2 = new List<Expression>(2);
					list2.Add(new StringLiteral(ec.BuiltinTypes, text, case_label.Location));
					case_label.Converted = new IntConstant(ec.BuiltinTypes, num, loc);
					list2.Add(case_label.Converted);
					list.Add(new CollectionElementInitializer(list2, loc));
				}
			}
			Arguments arguments = new Arguments(1);
			arguments.Add(new Argument(new IntConstant(ec.BuiltinTypes, list.Count, loc)));
			Expression expression = new NewInitialize(fullNamedExpression, arguments, new CollectionOrObjectInitializers(list, loc), loc);
			switch_cache_field = new FieldExpr(field, loc);
			string_dictionary = new SimpleAssign(switch_cache_field, expression.Resolve(ec));
		}

		private void DoEmitStringSwitch(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			value.EmitBranchable(ec, nullLabel, false);
			switch_cache_field.EmitBranchable(ec, label, true);
			using (ec.With(BuilderContext.Options.OmitDebugInfo, true))
			{
				string_dictionary.EmitStatement(ec);
			}
			ec.MarkLabel(label);
			LocalTemporary localTemporary = new LocalTemporary(ec.BuiltinTypes.Int);
			ResolveContext rc = new ResolveContext(ec.MemberContext);
			if (switch_cache_field.Type.IsGeneric)
			{
				Arguments arguments = new Arguments(2);
				arguments.Add(new Argument(value));
				arguments.Add(new Argument(localTemporary, Argument.AType.Out));
				Expression expression = new Invocation(new MemberAccess(switch_cache_field, "TryGetValue", loc), arguments).Resolve(rc);
				if (expression == null)
				{
					return;
				}
				expression.EmitBranchable(ec, defaultLabel, false);
			}
			else
			{
				Arguments arguments2 = new Arguments(1);
				arguments2.Add(new Argument(value));
				Expression expression2 = new ElementAccess(switch_cache_field, arguments2, loc).Resolve(rc);
				if (expression2 == null)
				{
					return;
				}
				LocalTemporary localTemporary2 = new LocalTemporary(ec.BuiltinTypes.Object);
				localTemporary2.EmitAssign(ec, expression2, true, false);
				ec.Emit(OpCodes.Brfalse, defaultLabel);
				((ExpressionStatement)new SimpleAssign(localTemporary, new Cast(new TypeExpression(ec.BuiltinTypes.Int, loc), localTemporary2, loc)).Resolve(rc)).EmitStatement(ec);
				localTemporary2.Release(ec);
			}
			EmitTableSwitch(ec, localTemporary);
			localTemporary.Release(ec);
		}

		private void EmitShortSwitch(EmitContext ec)
		{
			MethodSpec methodSpec = null;
			if (SwitchType.BuiltinType == BuiltinTypeSpec.Type.String)
			{
				methodSpec = ec.Module.PredefinedMembers.StringEqual.Resolve(loc);
			}
			if (methodSpec != null)
			{
				value.EmitBranchable(ec, nullLabel, false);
			}
			for (int i = 0; i < case_labels.Count; i++)
			{
				SwitchLabel switchLabel = case_labels[i];
				if (switchLabel != case_default && switchLabel != case_null)
				{
					Constant converted = switchLabel.Converted;
					if (converted == null)
					{
						switchLabel.Label.EmitBranchable(ec, switchLabel.GetILLabel(ec), true);
					}
					else if (methodSpec != null)
					{
						value.Emit(ec);
						converted.Emit(ec);
						default(CallEmitter).EmitPredefined(ec, methodSpec, new Arguments(0));
						ec.Emit(OpCodes.Brtrue, switchLabel.GetILLabel(ec));
					}
					else if (converted.IsZeroInteger && converted.Type.BuiltinType != BuiltinTypeSpec.Type.Long && converted.Type.BuiltinType != BuiltinTypeSpec.Type.ULong)
					{
						value.EmitBranchable(ec, switchLabel.GetILLabel(ec), false);
					}
					else
					{
						value.Emit(ec);
						converted.Emit(ec);
						ec.Emit(OpCodes.Beq, switchLabel.GetILLabel(ec));
					}
				}
			}
			ec.Emit(OpCodes.Br, defaultLabel);
		}

		private void EmitDispatch(EmitContext ec)
		{
			if (IsPatternMatching)
			{
				EmitShortSwitch(ec);
				return;
			}
			if (value == null)
			{
				int num = 0;
				{
					foreach (SwitchLabel case_label in case_labels)
					{
						if (!case_label.IsUnreachable && num++ > 0)
						{
							Constant constant = (Constant)new_expr;
							SwitchLabel switchLabel = FindLabel(constant) ?? case_default;
							ec.Emit(OpCodes.Br, switchLabel.GetILLabel(ec));
							break;
						}
					}
					return;
				}
			}
			if (string_dictionary != null)
			{
				DoEmitStringSwitch(ec);
			}
			else if (case_labels.Count < 4 || string_labels != null)
			{
				EmitShortSwitch(ec);
			}
			else
			{
				EmitTableSwitch(ec, value);
			}
		}

		protected override void DoEmit(EmitContext ec)
		{
			Label loopEnd = ec.LoopEnd;
			Switch obj = ec.Switch;
			ec.LoopEnd = ec.DefineLabel();
			ec.Switch = this;
			defaultLabel = ((case_default == null) ? ec.LoopEnd : case_default.GetILLabel(ec));
			nullLabel = ((case_null == null) ? defaultLabel : case_null.GetILLabel(ec));
			if (value != null)
			{
				ec.Mark(loc);
				Expression expression = new_expr ?? Expr;
				if (IsNullable)
				{
					unwrap.EmitCheck(ec);
					ec.Emit(OpCodes.Brfalse, nullLabel);
					value.EmitAssign(ec, expression, false, false);
				}
				else if (expression != value)
				{
					value.EmitAssign(ec, expression, false, false);
				}
				ec.Mark(block.StartLocation);
				block.IsCompilerGenerated = true;
			}
			else
			{
				new_expr.EmitSideEffect(ec);
			}
			block.Emit(ec);
			ec.MarkLabel(ec.LoopEnd);
			ec.LoopEnd = loopEnd;
			ec.Switch = obj;
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			Switch obj = (Switch)t;
			obj.Expr = Expr.Clone(clonectx);
			obj.Statement = (obj.block = (ExplicitBlock)block.Clone(clonectx));
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}

		public override void AddEndDefiniteAssignment(FlowAnalysisContext fc)
		{
			if (case_default != null || new_expr is Constant)
			{
				if (end_reachable_das == null)
				{
					end_reachable_das = new List<DefiniteAssignmentBitSet>();
				}
				end_reachable_das.Add(fc.DefiniteAssignment);
			}
		}

		public override void SetEndReachable()
		{
			end_reachable = true;
		}
	}
}
