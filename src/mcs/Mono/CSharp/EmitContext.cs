using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Mono.CSharp.Nullable;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	public class EmitContext : BuilderContext
	{
		public readonly ILGenerator ig;

		private readonly TypeSpec return_type;

		private Dictionary<TypeSpec, object> temporary_storage;

		public LocalBuilder return_value;

		public Label LoopBegin;

		public Label LoopEnd;

		public Label DefaultTarget;

		public Switch Switch;

		public AnonymousExpression CurrentAnonymousMethod;

		private readonly IMemberContext member_context;

		private readonly SourceMethodBuilder methodSymbols;

		private DynamicSiteClass dynamic_site_container;

		private Label? return_label;

		private List<IExpressionCleanup> epilogue_expressions;

		public AsyncTaskStorey AsyncTaskStorey => CurrentAnonymousMethod.Storey as AsyncTaskStorey;

		public BuiltinTypes BuiltinTypes => MemberContext.Module.Compiler.BuiltinTypes;

		public ConditionalAccessContext ConditionalAccess { get; set; }

		public TypeSpec CurrentType => member_context.CurrentType;

		public TypeParameters CurrentTypeParameters => member_context.CurrentTypeParameters;

		public MemberCore CurrentTypeDefinition => member_context.CurrentMemberDefinition;

		public bool EmitAccurateDebugInfo => (flags & Options.AccurateDebugInfo) != 0;

		public bool HasMethodSymbolBuilder => methodSymbols != null;

		public bool HasReturnLabel => return_label.HasValue;

		public bool IsStatic => member_context.IsStatic;

		public bool IsStaticConstructor
		{
			get
			{
				if (member_context.IsStatic)
				{
					return (flags & Options.ConstructorScope) != 0;
				}
				return false;
			}
		}

		public bool IsAnonymousStoreyMutateRequired
		{
			get
			{
				if (CurrentAnonymousMethod != null && CurrentAnonymousMethod.Storey != null)
				{
					return CurrentAnonymousMethod.Storey.Mutator != null;
				}
				return false;
			}
		}

		public IMemberContext MemberContext => member_context;

		public ModuleContainer Module => member_context.Module;

		public bool NotifyEvaluatorOnStore
		{
			get
			{
				if (Module.Evaluator != null)
				{
					return Module.Evaluator.ModificationListener != null;
				}
				return false;
			}
		}

		public Report Report => member_context.Module.Compiler.Report;

		public TypeSpec ReturnType => return_type;

		public Label ReturnLabel => return_label.Value;

		public List<IExpressionCleanup> StatementEpilogue => epilogue_expressions;

		public LocalVariable AsyncThrowVariable { get; set; }

		public List<TryFinally> TryFinallyUnwind { get; set; }

		public Label RecursivePatternLabel { get; set; }

		public EmitContext(IMemberContext rc, ILGenerator ig, TypeSpec return_type, SourceMethodBuilder methodSymbols)
		{
			member_context = rc;
			this.ig = ig;
			this.return_type = return_type;
			if (rc.Module.Compiler.Settings.Checked)
			{
				flags |= Options.CheckedScope;
			}
			if (methodSymbols != null)
			{
				this.methodSymbols = methodSymbols;
				if (!rc.Module.Compiler.Settings.Optimize)
				{
					flags |= Options.AccurateDebugInfo;
				}
			}
			else
			{
				flags |= Options.OmitDebugInfo;
			}
		}

		public void AddStatementEpilog(IExpressionCleanup cleanupExpression)
		{
			if (epilogue_expressions == null)
			{
				epilogue_expressions = new List<IExpressionCleanup>();
			}
			else if (epilogue_expressions.Contains(cleanupExpression))
			{
				return;
			}
			epilogue_expressions.Add(cleanupExpression);
		}

		public void AssertEmptyStack()
		{
		}

		public bool Mark(Location loc)
		{
			if ((flags & Options.OmitDebugInfo) != 0)
			{
				return false;
			}
			if (loc.IsNull || methodSymbols == null)
			{
				return false;
			}
			if (loc.SourceFile.IsHiddenLocation(loc))
			{
				return false;
			}
			return true;
		}

		public void MarkCallEntry(Location loc)
		{
			if (EmitAccurateDebugInfo)
			{
				Mark(loc);
			}
		}

		public void DefineLocalVariable(string name, LocalBuilder builder)
		{
			if ((flags & Options.OmitDebugInfo) == 0)
			{
				methodSymbols.AddLocal(builder.LocalIndex, name);
			}
		}

		public void BeginCatchBlock(TypeSpec type)
		{
			if (IsAnonymousStoreyMutateRequired)
			{
				type = CurrentAnonymousMethod.Storey.Mutator.Mutate(type);
			}
			ig.BeginCatchBlock(type.GetMetaInfo());
		}

		public void BeginFilterHandler()
		{
			ig.BeginCatchBlock(null);
		}

		public void BeginExceptionBlock()
		{
			ig.BeginExceptionBlock();
		}

		public void BeginExceptionFilterBlock()
		{
			ig.BeginExceptFilterBlock();
		}

		public void BeginFinallyBlock()
		{
			ig.BeginFinallyBlock();
		}

		public void BeginScope()
		{
			Options option = flags & Options.OmitDebugInfo;
		}

		public void BeginCompilerScope()
		{
			Options option = flags & Options.OmitDebugInfo;
		}

		public void EndExceptionBlock()
		{
			ig.EndExceptionBlock();
		}

		public void EndScope()
		{
			Options option = flags & Options.OmitDebugInfo;
		}

		public void CloseConditionalAccess(TypeSpec type)
		{
			if (type != null)
			{
				Emit(OpCodes.Newobj, NullableInfo.GetConstructor(type));
			}
			MarkLabel(ConditionalAccess.EndLabel);
			ConditionalAccess = null;
		}

		public DynamicSiteClass CreateDynamicSite()
		{
			if (dynamic_site_container == null)
			{
				MemberBase host = member_context.CurrentMemberDefinition as MemberBase;
				dynamic_site_container = new DynamicSiteClass(CurrentTypeDefinition.Parent.PartialContainer, host, member_context.CurrentTypeParameters);
				CurrentTypeDefinition.Module.AddCompilerGeneratedClass(dynamic_site_container);
				dynamic_site_container.CreateContainer();
				dynamic_site_container.DefineContainer();
				dynamic_site_container.Define();
				TypeParameterInflator inflator = new TypeParameterInflator(Module, CurrentType, TypeParameterSpec.EmptyTypes, TypeSpec.EmptyTypes);
				MemberSpec ms = dynamic_site_container.CurrentType.InflateMember(inflator);
				CurrentType.MemberCache.AddMember(ms);
			}
			return dynamic_site_container;
		}

		public Label CreateReturnLabel()
		{
			if (!return_label.HasValue)
			{
				return_label = DefineLabel();
			}
			return return_label.Value;
		}

		public LocalBuilder DeclareLocal(TypeSpec type, bool pinned)
		{
			if (IsAnonymousStoreyMutateRequired)
			{
				type = CurrentAnonymousMethod.Storey.Mutator.Mutate(type);
			}
			return ig.DeclareLocal(type.GetMetaInfo(), pinned);
		}

		public Label DefineLabel()
		{
			return ig.DefineLabel();
		}

		public StackFieldExpr GetTemporaryField(TypeSpec type, bool initializedFieldRequired = false)
		{
			return new StackFieldExpr(AsyncTaskStorey.AddCapturedLocalVariable(type, initializedFieldRequired))
			{
				InstanceExpression = new CompilerGeneratedThis(CurrentType, Location.Null)
			};
		}

		public void MarkLabel(Label label)
		{
			ig.MarkLabel(label);
		}

		public void Emit(OpCode opcode)
		{
			ig.Emit(opcode);
		}

		public void Emit(OpCode opcode, LocalBuilder local)
		{
			ig.Emit(opcode, local);
		}

		public void Emit(OpCode opcode, string arg)
		{
			ig.Emit(opcode, arg);
		}

		public void Emit(OpCode opcode, double arg)
		{
			ig.Emit(opcode, arg);
		}

		public void Emit(OpCode opcode, float arg)
		{
			ig.Emit(opcode, arg);
		}

		public void Emit(OpCode opcode, Label label)
		{
			ig.Emit(opcode, label);
		}

		public void Emit(OpCode opcode, Label[] labels)
		{
			ig.Emit(opcode, labels);
		}

		public void Emit(OpCode opcode, TypeSpec type)
		{
			if (IsAnonymousStoreyMutateRequired)
			{
				type = CurrentAnonymousMethod.Storey.Mutator.Mutate(type);
			}
			ig.Emit(opcode, type.GetMetaInfo());
		}

		public void Emit(OpCode opcode, FieldSpec field)
		{
			if (IsAnonymousStoreyMutateRequired)
			{
				field = field.Mutate(CurrentAnonymousMethod.Storey.Mutator);
			}
			ig.Emit(opcode, field.GetMetaInfo());
		}

		public void Emit(OpCode opcode, MethodSpec method)
		{
			if (IsAnonymousStoreyMutateRequired)
			{
				method = method.Mutate(CurrentAnonymousMethod.Storey.Mutator);
			}
			if (method.IsConstructor)
			{
				ig.Emit(opcode, (ConstructorInfo)method.GetMetaInfo());
			}
			else
			{
				ig.Emit(opcode, (MethodInfo)method.GetMetaInfo());
			}
		}

		public void Emit(OpCode opcode, MethodInfo method)
		{
			ig.Emit(opcode, method);
		}

		public void Emit(OpCode opcode, MethodSpec method, Type[] vargs)
		{
			ig.EmitCall(opcode, (MethodInfo)method.GetMetaInfo(), vargs);
		}

		public void EmitArrayNew(ArrayContainer ac)
		{
			if (ac.Rank == 1)
			{
				TypeSpec typeSpec = (IsAnonymousStoreyMutateRequired ? CurrentAnonymousMethod.Storey.Mutator.Mutate(ac.Element) : ac.Element);
				ig.Emit(OpCodes.Newarr, typeSpec.GetMetaInfo());
				return;
			}
			if (IsAnonymousStoreyMutateRequired)
			{
				ac = (ArrayContainer)ac.Mutate(CurrentAnonymousMethod.Storey.Mutator);
			}
			ig.Emit(OpCodes.Newobj, ac.GetConstructor());
		}

		public void EmitArrayAddress(ArrayContainer ac)
		{
			if (ac.Rank > 1)
			{
				if (IsAnonymousStoreyMutateRequired)
				{
					ac = (ArrayContainer)ac.Mutate(CurrentAnonymousMethod.Storey.Mutator);
				}
				ig.Emit(OpCodes.Call, ac.GetAddressMethod());
			}
			else
			{
				TypeSpec typeSpec = (IsAnonymousStoreyMutateRequired ? CurrentAnonymousMethod.Storey.Mutator.Mutate(ac.Element) : ac.Element);
				ig.Emit(OpCodes.Ldelema, typeSpec.GetMetaInfo());
			}
		}

		public void EmitArrayLoad(ArrayContainer ac)
		{
			if (ac.Rank > 1)
			{
				if (IsAnonymousStoreyMutateRequired)
				{
					ac = (ArrayContainer)ac.Mutate(CurrentAnonymousMethod.Storey.Mutator);
				}
				ig.Emit(OpCodes.Call, ac.GetGetMethod());
				return;
			}
			TypeSpec typeSpec = ac.Element;
			if (typeSpec.Kind == MemberKind.Enum)
			{
				typeSpec = EnumSpec.GetUnderlyingType(typeSpec);
			}
			switch (typeSpec.BuiltinType)
			{
			case BuiltinTypeSpec.Type.FirstPrimitive:
			case BuiltinTypeSpec.Type.Byte:
				ig.Emit(OpCodes.Ldelem_U1);
				return;
			case BuiltinTypeSpec.Type.SByte:
				ig.Emit(OpCodes.Ldelem_I1);
				return;
			case BuiltinTypeSpec.Type.Short:
				ig.Emit(OpCodes.Ldelem_I2);
				return;
			case BuiltinTypeSpec.Type.Char:
			case BuiltinTypeSpec.Type.UShort:
				ig.Emit(OpCodes.Ldelem_U2);
				return;
			case BuiltinTypeSpec.Type.Int:
				ig.Emit(OpCodes.Ldelem_I4);
				return;
			case BuiltinTypeSpec.Type.UInt:
				ig.Emit(OpCodes.Ldelem_U4);
				return;
			case BuiltinTypeSpec.Type.Long:
			case BuiltinTypeSpec.Type.ULong:
				ig.Emit(OpCodes.Ldelem_I8);
				return;
			case BuiltinTypeSpec.Type.Float:
				ig.Emit(OpCodes.Ldelem_R4);
				return;
			case BuiltinTypeSpec.Type.Double:
				ig.Emit(OpCodes.Ldelem_R8);
				return;
			case BuiltinTypeSpec.Type.IntPtr:
				ig.Emit(OpCodes.Ldelem_I);
				return;
			}
			switch (typeSpec.Kind)
			{
			case MemberKind.Struct:
				if (IsAnonymousStoreyMutateRequired)
				{
					typeSpec = CurrentAnonymousMethod.Storey.Mutator.Mutate(typeSpec);
				}
				ig.Emit(OpCodes.Ldelema, typeSpec.GetMetaInfo());
				ig.Emit(OpCodes.Ldobj, typeSpec.GetMetaInfo());
				break;
			case MemberKind.TypeParameter:
				if (IsAnonymousStoreyMutateRequired)
				{
					typeSpec = CurrentAnonymousMethod.Storey.Mutator.Mutate(typeSpec);
				}
				ig.Emit(OpCodes.Ldelem, typeSpec.GetMetaInfo());
				break;
			case MemberKind.PointerType:
				ig.Emit(OpCodes.Ldelem_I);
				break;
			default:
				ig.Emit(OpCodes.Ldelem_Ref);
				break;
			}
		}

		public void EmitArrayStore(ArrayContainer ac)
		{
			if (ac.Rank > 1)
			{
				if (IsAnonymousStoreyMutateRequired)
				{
					ac = (ArrayContainer)ac.Mutate(CurrentAnonymousMethod.Storey.Mutator);
				}
				ig.Emit(OpCodes.Call, ac.GetSetMethod());
				return;
			}
			TypeSpec typeSpec = ac.Element;
			if (typeSpec.Kind == MemberKind.Enum)
			{
				typeSpec = EnumSpec.GetUnderlyingType(typeSpec);
			}
			switch (typeSpec.BuiltinType)
			{
			case BuiltinTypeSpec.Type.FirstPrimitive:
			case BuiltinTypeSpec.Type.Byte:
			case BuiltinTypeSpec.Type.SByte:
				Emit(OpCodes.Stelem_I1);
				return;
			case BuiltinTypeSpec.Type.Char:
			case BuiltinTypeSpec.Type.Short:
			case BuiltinTypeSpec.Type.UShort:
				Emit(OpCodes.Stelem_I2);
				return;
			case BuiltinTypeSpec.Type.Int:
			case BuiltinTypeSpec.Type.UInt:
				Emit(OpCodes.Stelem_I4);
				return;
			case BuiltinTypeSpec.Type.Long:
			case BuiltinTypeSpec.Type.ULong:
				Emit(OpCodes.Stelem_I8);
				return;
			case BuiltinTypeSpec.Type.Float:
				Emit(OpCodes.Stelem_R4);
				return;
			case BuiltinTypeSpec.Type.Double:
				Emit(OpCodes.Stelem_R8);
				return;
			}
			switch (typeSpec.Kind)
			{
			case MemberKind.Struct:
				Emit(OpCodes.Stobj, typeSpec);
				break;
			case MemberKind.TypeParameter:
				Emit(OpCodes.Stelem, typeSpec);
				break;
			case MemberKind.PointerType:
				Emit(OpCodes.Stelem_I);
				break;
			default:
				Emit(OpCodes.Stelem_Ref);
				break;
			}
		}

		public void EmitInt(int i)
		{
			EmitIntConstant(i);
		}

		private void EmitIntConstant(int i)
		{
			switch (i)
			{
			case -1:
				ig.Emit(OpCodes.Ldc_I4_M1);
				return;
			case 0:
				ig.Emit(OpCodes.Ldc_I4_0);
				return;
			case 1:
				ig.Emit(OpCodes.Ldc_I4_1);
				return;
			case 2:
				ig.Emit(OpCodes.Ldc_I4_2);
				return;
			case 3:
				ig.Emit(OpCodes.Ldc_I4_3);
				return;
			case 4:
				ig.Emit(OpCodes.Ldc_I4_4);
				return;
			case 5:
				ig.Emit(OpCodes.Ldc_I4_5);
				return;
			case 6:
				ig.Emit(OpCodes.Ldc_I4_6);
				return;
			case 7:
				ig.Emit(OpCodes.Ldc_I4_7);
				return;
			case 8:
				ig.Emit(OpCodes.Ldc_I4_8);
				return;
			}
			if (i >= -128 && i <= 127)
			{
				ig.Emit(OpCodes.Ldc_I4_S, (sbyte)i);
			}
			else
			{
				ig.Emit(OpCodes.Ldc_I4, i);
			}
		}

		public void EmitLong(long l)
		{
			if (l >= int.MinValue && l <= int.MaxValue)
			{
				EmitIntConstant((int)l);
				ig.Emit(OpCodes.Conv_I8);
			}
			else if (l >= 0 && l <= uint.MaxValue)
			{
				EmitIntConstant((int)l);
				ig.Emit(OpCodes.Conv_U8);
			}
			else
			{
				ig.Emit(OpCodes.Ldc_I8, l);
			}
		}

		public void EmitLoadFromPtr(TypeSpec type)
		{
			if (type.Kind == MemberKind.Enum)
			{
				type = EnumSpec.GetUnderlyingType(type);
			}
			switch (type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Int:
				ig.Emit(OpCodes.Ldind_I4);
				return;
			case BuiltinTypeSpec.Type.UInt:
				ig.Emit(OpCodes.Ldind_U4);
				return;
			case BuiltinTypeSpec.Type.Short:
				ig.Emit(OpCodes.Ldind_I2);
				return;
			case BuiltinTypeSpec.Type.Char:
			case BuiltinTypeSpec.Type.UShort:
				ig.Emit(OpCodes.Ldind_U2);
				return;
			case BuiltinTypeSpec.Type.Byte:
				ig.Emit(OpCodes.Ldind_U1);
				return;
			case BuiltinTypeSpec.Type.FirstPrimitive:
			case BuiltinTypeSpec.Type.SByte:
				ig.Emit(OpCodes.Ldind_I1);
				return;
			case BuiltinTypeSpec.Type.Long:
			case BuiltinTypeSpec.Type.ULong:
				ig.Emit(OpCodes.Ldind_I8);
				return;
			case BuiltinTypeSpec.Type.Float:
				ig.Emit(OpCodes.Ldind_R4);
				return;
			case BuiltinTypeSpec.Type.Double:
				ig.Emit(OpCodes.Ldind_R8);
				return;
			case BuiltinTypeSpec.Type.IntPtr:
				ig.Emit(OpCodes.Ldind_I);
				return;
			}
			switch (type.Kind)
			{
			case MemberKind.Struct:
			case MemberKind.TypeParameter:
				if (IsAnonymousStoreyMutateRequired)
				{
					type = CurrentAnonymousMethod.Storey.Mutator.Mutate(type);
				}
				ig.Emit(OpCodes.Ldobj, type.GetMetaInfo());
				break;
			case MemberKind.PointerType:
				ig.Emit(OpCodes.Ldind_I);
				break;
			default:
				ig.Emit(OpCodes.Ldind_Ref);
				break;
			}
		}

		public void EmitNull()
		{
			ig.Emit(OpCodes.Ldnull);
		}

		public void EmitArgumentAddress(int pos)
		{
			if (!IsStatic)
			{
				pos++;
			}
			if (pos > 255)
			{
				ig.Emit(OpCodes.Ldarga, pos);
			}
			else
			{
				ig.Emit(OpCodes.Ldarga_S, (byte)pos);
			}
		}

		public void EmitArgumentLoad(int pos)
		{
			if (!IsStatic)
			{
				pos++;
			}
			switch (pos)
			{
			case 0:
				ig.Emit(OpCodes.Ldarg_0);
				return;
			case 1:
				ig.Emit(OpCodes.Ldarg_1);
				return;
			case 2:
				ig.Emit(OpCodes.Ldarg_2);
				return;
			case 3:
				ig.Emit(OpCodes.Ldarg_3);
				return;
			}
			if (pos > 255)
			{
				ig.Emit(OpCodes.Ldarg, pos);
			}
			else
			{
				ig.Emit(OpCodes.Ldarg_S, (byte)pos);
			}
		}

		public void EmitArgumentStore(int pos)
		{
			if (!IsStatic)
			{
				pos++;
			}
			if (pos > 255)
			{
				ig.Emit(OpCodes.Starg, pos);
			}
			else
			{
				ig.Emit(OpCodes.Starg_S, (byte)pos);
			}
		}

		public void EmitStoreFromPtr(TypeSpec type)
		{
			if (type.IsEnum)
			{
				type = EnumSpec.GetUnderlyingType(type);
			}
			switch (type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Int:
			case BuiltinTypeSpec.Type.UInt:
				ig.Emit(OpCodes.Stind_I4);
				return;
			case BuiltinTypeSpec.Type.Long:
			case BuiltinTypeSpec.Type.ULong:
				ig.Emit(OpCodes.Stind_I8);
				return;
			case BuiltinTypeSpec.Type.Char:
			case BuiltinTypeSpec.Type.Short:
			case BuiltinTypeSpec.Type.UShort:
				ig.Emit(OpCodes.Stind_I2);
				return;
			case BuiltinTypeSpec.Type.Float:
				ig.Emit(OpCodes.Stind_R4);
				return;
			case BuiltinTypeSpec.Type.Double:
				ig.Emit(OpCodes.Stind_R8);
				return;
			case BuiltinTypeSpec.Type.FirstPrimitive:
			case BuiltinTypeSpec.Type.Byte:
			case BuiltinTypeSpec.Type.SByte:
				ig.Emit(OpCodes.Stind_I1);
				return;
			case BuiltinTypeSpec.Type.IntPtr:
				ig.Emit(OpCodes.Stind_I);
				return;
			}
			switch (type.Kind)
			{
			case MemberKind.Struct:
			case MemberKind.TypeParameter:
				if (IsAnonymousStoreyMutateRequired)
				{
					type = CurrentAnonymousMethod.Storey.Mutator.Mutate(type);
				}
				ig.Emit(OpCodes.Stobj, type.GetMetaInfo());
				break;
			case MemberKind.PointerType:
				ig.Emit(OpCodes.Stind_I);
				break;
			default:
				ig.Emit(OpCodes.Stind_Ref);
				break;
			}
		}

		public void EmitThis()
		{
			ig.Emit(OpCodes.Ldarg_0);
		}

		public void EmitEpilogue()
		{
			if (epilogue_expressions == null)
			{
				return;
			}
			foreach (IExpressionCleanup epilogue_expression in epilogue_expressions)
			{
				epilogue_expression.EmitCleanup(this);
			}
			epilogue_expressions = null;
		}

		public LocalBuilder GetTemporaryLocal(TypeSpec t)
		{
			if (temporary_storage != null)
			{
				object value;
				if (temporary_storage.TryGetValue(t, out value))
				{
					if (value is Stack<LocalBuilder>)
					{
						Stack<LocalBuilder> stack = (Stack<LocalBuilder>)value;
						value = ((stack.Count == 0) ? null : stack.Pop());
					}
					else
					{
						temporary_storage.Remove(t);
					}
				}
				if (value != null)
				{
					return (LocalBuilder)value;
				}
			}
			return DeclareLocal(t, false);
		}

		public void FreeTemporaryLocal(LocalBuilder b, TypeSpec t)
		{
			if (temporary_storage == null)
			{
				temporary_storage = new Dictionary<TypeSpec, object>(ReferenceEquality<TypeSpec>.Default);
				temporary_storage.Add(t, b);
				return;
			}
			object value;
			if (!temporary_storage.TryGetValue(t, out value))
			{
				temporary_storage.Add(t, b);
				return;
			}
			Stack<LocalBuilder> stack = value as Stack<LocalBuilder>;
			if (stack == null)
			{
				stack = new Stack<LocalBuilder>();
				stack.Push((LocalBuilder)value);
				temporary_storage[t] = stack;
			}
			stack.Push(b);
		}

		public LocalBuilder TemporaryReturn()
		{
			if (return_value == null)
			{
				return_value = DeclareLocal(return_type, false);
			}
			return return_value;
		}
	}
}
