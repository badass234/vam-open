using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class ILGenerator
	{
		private struct LabelFixup
		{
			internal int label;

			internal int offset;
		}

		internal sealed class ExceptionBlock : IComparer<ExceptionBlock>
		{
			internal readonly int ordinal;

			internal Label labelEnd;

			internal int tryOffset;

			internal int tryLength;

			internal int handlerOffset;

			internal int handlerLength;

			internal int filterOffsetOrExceptionTypeToken;

			internal ExceptionHandlingClauseOptions kind;

			internal ExceptionBlock(int ordinal)
			{
				this.ordinal = ordinal;
			}

			internal ExceptionBlock(ExceptionHandler h)
			{
				ordinal = -1;
				tryOffset = h.TryOffset;
				tryLength = h.TryLength;
				handlerOffset = h.HandlerOffset;
				handlerLength = h.HandlerLength;
				kind = h.Kind;
				filterOffsetOrExceptionTypeToken = ((kind == ExceptionHandlingClauseOptions.Filter) ? h.FilterOffset : h.ExceptionTypeToken);
			}

			int IComparer<ExceptionBlock>.Compare(ExceptionBlock x, ExceptionBlock y)
			{
				if (x == y)
				{
					return 0;
				}
				if (x.tryOffset == y.tryOffset && x.tryLength == y.tryLength)
				{
					if (x.ordinal >= y.ordinal)
					{
						return 1;
					}
					return -1;
				}
				if (x.tryOffset >= y.tryOffset && x.handlerOffset + x.handlerLength <= y.handlerOffset + y.handlerLength)
				{
					return -1;
				}
				if (y.tryOffset >= x.tryOffset && y.handlerOffset + y.handlerLength <= x.handlerOffset + x.handlerLength)
				{
					return 1;
				}
				if (x.ordinal >= y.ordinal)
				{
					return 1;
				}
				return -1;
			}
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct SequencePoint
		{
		}

		private sealed class Scope
		{
			internal readonly Scope parent;

			internal readonly List<Scope> children = new List<Scope>();

			internal readonly List<LocalBuilder> locals = new List<LocalBuilder>();

			internal int startOffset;

			internal int endOffset;

			internal Scope(Scope parent)
			{
				this.parent = parent;
			}
		}

		private readonly ModuleBuilder moduleBuilder;

		private readonly ByteBuffer code;

		private readonly SignatureHelper locals;

		private int localsCount;

		private readonly List<int> tokenFixups = new List<int>();

		private readonly List<int> labels = new List<int>();

		private readonly List<int> labelStackHeight = new List<int>();

		private readonly List<LabelFixup> labelFixups = new List<LabelFixup>();

		private readonly List<ExceptionBlock> exceptions = new List<ExceptionBlock>();

		private readonly Stack<ExceptionBlock> exceptionStack = new Stack<ExceptionBlock>();

		private ushort maxStack;

		private bool fatHeader;

		private int stackHeight;

		private Scope scope;

		private byte exceptionBlockAssistanceMode;

		private const byte EBAM_COMPAT = 0;

		private const byte EBAM_DISABLE = 1;

		private const byte EBAM_CLEVER = 2;

		public int __MaxStackSize
		{
			get
			{
				return maxStack;
			}
			set
			{
				maxStack = (ushort)value;
				fatHeader = true;
			}
		}

		public int __StackHeight => stackHeight;

		public int ILOffset => code.Position;

		internal ILGenerator(ModuleBuilder moduleBuilder, int initialCapacity)
		{
			code = new ByteBuffer(initialCapacity);
			this.moduleBuilder = moduleBuilder;
			locals = SignatureHelper.GetLocalVarSigHelper(moduleBuilder);
			if (moduleBuilder.symbolWriter != null)
			{
				scope = new Scope(null);
			}
		}

		public void __DisableExceptionBlockAssistance()
		{
			exceptionBlockAssistanceMode = 1;
		}

		public void __CleverExceptionBlockAssistance()
		{
			exceptionBlockAssistanceMode = 2;
		}

		public void BeginCatchBlock(Type exceptionType)
		{
			if (exceptionType == null)
			{
				ExceptionBlock exceptionBlock = exceptionStack.Peek();
				if (exceptionBlock.kind != ExceptionHandlingClauseOptions.Filter || exceptionBlock.handlerOffset != 0)
				{
					throw new ArgumentNullException("exceptionType");
				}
				if (exceptionBlockAssistanceMode == 0 || (exceptionBlockAssistanceMode == 2 && stackHeight != -1))
				{
					Emit(OpCodes.Endfilter);
				}
				stackHeight = 0;
				UpdateStack(1);
				exceptionBlock.handlerOffset = code.Position;
			}
			else
			{
				ExceptionBlock exceptionBlock2 = BeginCatchOrFilterBlock();
				exceptionBlock2.kind = ExceptionHandlingClauseOptions.Clause;
				exceptionBlock2.filterOffsetOrExceptionTypeToken = moduleBuilder.GetTypeTokenForMemberRef(exceptionType);
				exceptionBlock2.handlerOffset = code.Position;
			}
		}

		private ExceptionBlock BeginCatchOrFilterBlock()
		{
			ExceptionBlock exceptionBlock = exceptionStack.Peek();
			if (exceptionBlockAssistanceMode == 0 || (exceptionBlockAssistanceMode == 2 && stackHeight != -1))
			{
				Emit(OpCodes.Leave, exceptionBlock.labelEnd);
			}
			stackHeight = 0;
			UpdateStack(1);
			if (exceptionBlock.tryLength == 0)
			{
				exceptionBlock.tryLength = code.Position - exceptionBlock.tryOffset;
			}
			else
			{
				exceptionBlock.handlerLength = code.Position - exceptionBlock.handlerOffset;
				exceptionStack.Pop();
				exceptionBlock = new ExceptionBlock(exceptions.Count)
				{
					labelEnd = exceptionBlock.labelEnd,
					tryOffset = exceptionBlock.tryOffset,
					tryLength = exceptionBlock.tryLength
				};
				exceptions.Add(exceptionBlock);
				exceptionStack.Push(exceptionBlock);
			}
			return exceptionBlock;
		}

		public Label BeginExceptionBlock()
		{
			ExceptionBlock exceptionBlock = new ExceptionBlock(exceptions.Count);
			exceptionBlock.labelEnd = DefineLabel();
			exceptionBlock.tryOffset = code.Position;
			exceptionStack.Push(exceptionBlock);
			exceptions.Add(exceptionBlock);
			stackHeight = 0;
			return exceptionBlock.labelEnd;
		}

		public void BeginExceptFilterBlock()
		{
			ExceptionBlock exceptionBlock = BeginCatchOrFilterBlock();
			exceptionBlock.kind = ExceptionHandlingClauseOptions.Filter;
			exceptionBlock.filterOffsetOrExceptionTypeToken = code.Position;
		}

		public void BeginFaultBlock()
		{
			BeginFinallyFaultBlock(ExceptionHandlingClauseOptions.Fault);
		}

		public void BeginFinallyBlock()
		{
			BeginFinallyFaultBlock(ExceptionHandlingClauseOptions.Finally);
		}

		private void BeginFinallyFaultBlock(ExceptionHandlingClauseOptions kind)
		{
			ExceptionBlock exceptionBlock = exceptionStack.Peek();
			if (exceptionBlockAssistanceMode == 0 || (exceptionBlockAssistanceMode == 2 && stackHeight != -1))
			{
				Emit(OpCodes.Leave, exceptionBlock.labelEnd);
			}
			if (exceptionBlock.handlerOffset == 0)
			{
				exceptionBlock.tryLength = code.Position - exceptionBlock.tryOffset;
			}
			else
			{
				exceptionBlock.handlerLength = code.Position - exceptionBlock.handlerOffset;
				Label label;
				if (exceptionBlockAssistanceMode != 0)
				{
					label = exceptionBlock.labelEnd;
				}
				else
				{
					MarkLabel(exceptionBlock.labelEnd);
					label = DefineLabel();
					Emit(OpCodes.Leave, label);
				}
				exceptionStack.Pop();
				exceptionBlock = new ExceptionBlock(exceptions.Count)
				{
					labelEnd = label,
					tryOffset = exceptionBlock.tryOffset,
					tryLength = code.Position - exceptionBlock.tryOffset
				};
				exceptions.Add(exceptionBlock);
				exceptionStack.Push(exceptionBlock);
			}
			exceptionBlock.handlerOffset = code.Position;
			exceptionBlock.kind = kind;
			stackHeight = 0;
		}

		public void EndExceptionBlock()
		{
			ExceptionBlock exceptionBlock = exceptionStack.Pop();
			if (exceptionBlockAssistanceMode == 0 || (exceptionBlockAssistanceMode == 2 && stackHeight != -1))
			{
				if (exceptionBlock.kind != ExceptionHandlingClauseOptions.Finally && exceptionBlock.kind != ExceptionHandlingClauseOptions.Fault)
				{
					Emit(OpCodes.Leave, exceptionBlock.labelEnd);
				}
				else
				{
					Emit(OpCodes.Endfinally);
				}
			}
			MarkLabel(exceptionBlock.labelEnd);
			exceptionBlock.handlerLength = code.Position - exceptionBlock.handlerOffset;
		}

		public void BeginScope()
		{
			Scope item = new Scope(scope);
			scope.children.Add(item);
			scope = item;
			scope.startOffset = code.Position;
		}

		public void UsingNamespace(string usingNamespace)
		{
		}

		public LocalBuilder DeclareLocal(Type localType)
		{
			return DeclareLocal(localType, false);
		}

		public LocalBuilder DeclareLocal(Type localType, bool pinned)
		{
			LocalBuilder localBuilder = new LocalBuilder(localType, localsCount++, pinned);
			locals.AddArgument(localType, pinned);
			if (scope != null)
			{
				scope.locals.Add(localBuilder);
			}
			return localBuilder;
		}

		public LocalBuilder __DeclareLocal(Type localType, bool pinned, CustomModifiers customModifiers)
		{
			LocalBuilder localBuilder = new LocalBuilder(localType, localsCount++, pinned, customModifiers);
			locals.__AddArgument(localType, pinned, customModifiers);
			if (scope != null)
			{
				scope.locals.Add(localBuilder);
			}
			return localBuilder;
		}

		public Label DefineLabel()
		{
			Label result = new Label(labels.Count);
			labels.Add(-1);
			labelStackHeight.Add(-1);
			return result;
		}

		public void Emit(OpCode opc)
		{
			if (opc.Value < 0)
			{
				code.Write((byte)(opc.Value >> 8));
			}
			code.Write((byte)opc.Value);
			switch (opc.FlowControl)
			{
			case FlowControl.Branch:
			case FlowControl.Break:
			case FlowControl.Return:
			case FlowControl.Throw:
				stackHeight = -1;
				break;
			default:
				UpdateStack(opc.StackDiff);
				break;
			}
		}

		private void UpdateStack(int stackdiff)
		{
			if (stackHeight == -1)
			{
				stackHeight = 0;
			}
			stackHeight += stackdiff;
			maxStack = Math.Max(maxStack, (ushort)stackHeight);
		}

		public void Emit(OpCode opc, byte arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, double arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, FieldInfo field)
		{
			Emit(opc);
			WriteToken(moduleBuilder.GetFieldToken(field).Token);
		}

		public void Emit(OpCode opc, short arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, int arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, long arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, Label label)
		{
			int num = stackHeight;
			Emit(opc);
			if (opc == OpCodes.Leave || opc == OpCodes.Leave_S)
			{
				num = 0;
			}
			else if (opc.FlowControl != FlowControl.Branch)
			{
				num = stackHeight;
			}
			if (labels[label.Index] != -1)
			{
				if (labelStackHeight[label.Index] != num && (labelStackHeight[label.Index] != 0 || num != -1))
				{
					throw new NotSupportedException("'Backward branch constraints' violated");
				}
				if (opc.OperandType == OperandType.ShortInlineBrTarget)
				{
					WriteByteBranchOffset(labels[label.Index] - (code.Position + 1));
				}
				else
				{
					code.Write(labels[label.Index] - (code.Position + 4));
				}
				return;
			}
			labelStackHeight[label.Index] = num;
			LabelFixup item = new LabelFixup
			{
				label = label.Index,
				offset = code.Position
			};
			labelFixups.Add(item);
			if (opc.OperandType == OperandType.ShortInlineBrTarget)
			{
				code.Write((byte)1);
			}
			else
			{
				code.Write(4);
			}
		}

		private void WriteByteBranchOffset(int offset)
		{
			if (offset < -128 || offset > 127)
			{
				throw new NotSupportedException("Branch offset of " + offset + " does not fit in one-byte branch target at position " + code.Position);
			}
			code.Write((byte)offset);
		}

		public void Emit(OpCode opc, Label[] labels)
		{
			Emit(opc);
			LabelFixup item = new LabelFixup
			{
				label = -1,
				offset = code.Position
			};
			labelFixups.Add(item);
			code.Write(labels.Length);
			for (int i = 0; i < labels.Length; i++)
			{
				Label label = labels[i];
				code.Write(label.Index);
				if (this.labels[label.Index] != -1)
				{
					if (labelStackHeight[label.Index] != stackHeight)
					{
						throw new NotSupportedException();
					}
				}
				else
				{
					labelStackHeight[label.Index] = stackHeight;
				}
			}
		}

		public void Emit(OpCode opc, LocalBuilder local)
		{
			if ((opc == OpCodes.Ldloc || opc == OpCodes.Ldloca || opc == OpCodes.Stloc) && local.LocalIndex < 256)
			{
				if (opc == OpCodes.Ldloc)
				{
					switch (local.LocalIndex)
					{
					case 0:
						Emit(OpCodes.Ldloc_0);
						break;
					case 1:
						Emit(OpCodes.Ldloc_1);
						break;
					case 2:
						Emit(OpCodes.Ldloc_2);
						break;
					case 3:
						Emit(OpCodes.Ldloc_3);
						break;
					default:
						Emit(OpCodes.Ldloc_S);
						code.Write((byte)local.LocalIndex);
						break;
					}
				}
				else if (opc == OpCodes.Ldloca)
				{
					Emit(OpCodes.Ldloca_S);
					code.Write((byte)local.LocalIndex);
				}
				else if (opc == OpCodes.Stloc)
				{
					switch (local.LocalIndex)
					{
					case 0:
						Emit(OpCodes.Stloc_0);
						break;
					case 1:
						Emit(OpCodes.Stloc_1);
						break;
					case 2:
						Emit(OpCodes.Stloc_2);
						break;
					case 3:
						Emit(OpCodes.Stloc_3);
						break;
					default:
						Emit(OpCodes.Stloc_S);
						code.Write((byte)local.LocalIndex);
						break;
					}
				}
			}
			else
			{
				Emit(opc);
				switch (opc.OperandType)
				{
				case OperandType.InlineVar:
					code.Write((ushort)local.LocalIndex);
					break;
				case OperandType.ShortInlineVar:
					code.Write((byte)local.LocalIndex);
					break;
				}
			}
		}

		private void WriteToken(int token)
		{
			if (ModuleBuilder.IsPseudoToken(token))
			{
				tokenFixups.Add(code.Position);
			}
			code.Write(token);
		}

		private void UpdateStack(OpCode opc, bool hasthis, Type returnType, int parameterCount)
		{
			if (opc == OpCodes.Jmp)
			{
				stackHeight = -1;
			}
			else if (opc.FlowControl == FlowControl.Call)
			{
				int num = 0;
				if ((hasthis && opc != OpCodes.Newobj) || opc == OpCodes.Calli)
				{
					num--;
				}
				num -= parameterCount;
				if (returnType != moduleBuilder.universe.System_Void)
				{
					num++;
				}
				UpdateStack(num);
			}
		}

		public void Emit(OpCode opc, MethodInfo method)
		{
			UpdateStack(opc, method.HasThis, method.ReturnType, method.ParameterCount);
			Emit(opc);
			WriteToken(moduleBuilder.GetMethodTokenForIL(method).Token);
		}

		public void Emit(OpCode opc, ConstructorInfo constructor)
		{
			Emit(opc, constructor.GetMethodInfo());
		}

		public void Emit(OpCode opc, sbyte arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, float arg)
		{
			Emit(opc);
			code.Write(arg);
		}

		public void Emit(OpCode opc, string str)
		{
			Emit(opc);
			code.Write(moduleBuilder.GetStringConstant(str).Token);
		}

		public void Emit(OpCode opc, Type type)
		{
			Emit(opc);
			if (opc == OpCodes.Ldtoken)
			{
				code.Write(moduleBuilder.GetTypeToken(type).Token);
			}
			else
			{
				code.Write(moduleBuilder.GetTypeTokenForMemberRef(type));
			}
		}

		public void Emit(OpCode opcode, SignatureHelper signature)
		{
			Emit(opcode);
			UpdateStack(opcode, signature.HasThis, signature.ReturnType, signature.ParameterCount);
			code.Write(moduleBuilder.GetSignatureToken(signature).Token);
		}

		public void EmitCall(OpCode opc, MethodInfo method, Type[] optionalParameterTypes)
		{
			__EmitCall(opc, method, optionalParameterTypes, null);
		}

		public void __EmitCall(OpCode opc, MethodInfo method, Type[] optionalParameterTypes, CustomModifiers[] customModifiers)
		{
			if (optionalParameterTypes == null || optionalParameterTypes.Length == 0)
			{
				Emit(opc, method);
				return;
			}
			Emit(opc);
			UpdateStack(opc, method.HasThis, method.ReturnType, method.ParameterCount + optionalParameterTypes.Length);
			code.Write(moduleBuilder.__GetMethodToken(method, optionalParameterTypes, customModifiers).Token);
		}

		public void __EmitCall(OpCode opc, ConstructorInfo constructor, Type[] optionalParameterTypes)
		{
			EmitCall(opc, constructor.GetMethodInfo(), optionalParameterTypes);
		}

		public void __EmitCall(OpCode opc, ConstructorInfo constructor, Type[] optionalParameterTypes, CustomModifiers[] customModifiers)
		{
			__EmitCall(opc, constructor.GetMethodInfo(), optionalParameterTypes, customModifiers);
		}

		public void EmitCalli(OpCode opc, CallingConvention callingConvention, Type returnType, Type[] parameterTypes)
		{
			SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(moduleBuilder, callingConvention, returnType);
			methodSigHelper.AddArguments(parameterTypes, null, null);
			Emit(opc, methodSigHelper);
		}

		public void EmitCalli(OpCode opc, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
		{
			SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(moduleBuilder, callingConvention, returnType);
			methodSigHelper.AddArguments(parameterTypes, null, null);
			if (optionalParameterTypes != null && optionalParameterTypes.Length != 0)
			{
				methodSigHelper.AddSentinel();
				methodSigHelper.AddArguments(optionalParameterTypes, null, null);
			}
			Emit(opc, methodSigHelper);
		}

		public void __EmitCalli(OpCode opc, __StandAloneMethodSig sig)
		{
			Emit(opc);
			if (sig.IsUnmanaged)
			{
				UpdateStack(opc, false, sig.ReturnType, sig.ParameterCount);
			}
			else
			{
				CallingConventions callingConvention = sig.CallingConvention;
				UpdateStack(opc, ((callingConvention & CallingConventions.HasThis) | CallingConventions.ExplicitThis) == CallingConventions.HasThis, sig.ReturnType, sig.ParameterCount);
			}
			ByteBuffer bb = new ByteBuffer(16);
			Signature.WriteStandAloneMethodSig(moduleBuilder, bb, sig);
			code.Write(0x11000000 | moduleBuilder.StandAloneSig.FindOrAddRecord(moduleBuilder.Blobs.Add(bb)));
		}

		public void EmitWriteLine(string text)
		{
			Universe universe = moduleBuilder.universe;
			Emit(OpCodes.Ldstr, text);
			Emit(OpCodes.Call, universe.Import(typeof(Console)).GetMethod("WriteLine", new Type[1] { universe.System_String }));
		}

		public void EmitWriteLine(FieldInfo field)
		{
			Universe universe = moduleBuilder.universe;
			Emit(OpCodes.Call, universe.Import(typeof(Console)).GetMethod("get_Out"));
			if (field.IsStatic)
			{
				Emit(OpCodes.Ldsfld, field);
			}
			else
			{
				Emit(OpCodes.Ldarg_0);
				Emit(OpCodes.Ldfld, field);
			}
			Emit(OpCodes.Callvirt, universe.Import(typeof(TextWriter)).GetMethod("WriteLine", new Type[1] { field.FieldType }));
		}

		public void EmitWriteLine(LocalBuilder local)
		{
			Universe universe = moduleBuilder.universe;
			Emit(OpCodes.Call, universe.Import(typeof(Console)).GetMethod("get_Out"));
			Emit(OpCodes.Ldloc, local);
			Emit(OpCodes.Callvirt, universe.Import(typeof(TextWriter)).GetMethod("WriteLine", new Type[1] { local.LocalType }));
		}

		public void EndScope()
		{
			scope.endOffset = code.Position;
			scope = scope.parent;
		}

		public void MarkLabel(Label loc)
		{
			labels[loc.Index] = code.Position;
			if (labelStackHeight[loc.Index] == -1)
			{
				if (stackHeight == -1)
				{
					labelStackHeight[loc.Index] = 0;
				}
				else
				{
					labelStackHeight[loc.Index] = stackHeight;
				}
			}
			else
			{
				stackHeight = labelStackHeight[loc.Index];
			}
		}

		public void ThrowException(Type excType)
		{
			Emit(OpCodes.Newobj, excType.GetConstructor(Type.EmptyTypes));
			Emit(OpCodes.Throw);
		}

		internal int WriteBody(bool initLocals)
		{
			if (moduleBuilder.symbolWriter != null)
			{
				scope.endOffset = code.Position;
			}
			ResolveBranches();
			ByteBuffer methodBodies = moduleBuilder.methodBodies;
			int localVarSigTok = 0;
			if (localsCount == 0 && exceptions.Count == 0 && maxStack <= 8 && code.Length < 64 && !fatHeader)
			{
				return WriteTinyHeaderAndCode(methodBodies);
			}
			if (localsCount != 0)
			{
				localVarSigTok = moduleBuilder.GetSignatureToken(locals).Token;
			}
			return WriteFatHeaderAndCode(methodBodies, localVarSigTok, initLocals);
		}

		private void ResolveBranches()
		{
			foreach (LabelFixup labelFixup in labelFixups)
			{
				if (labelFixup.label == -1)
				{
					code.Position = labelFixup.offset;
					int int32AtCurrentPosition = code.GetInt32AtCurrentPosition();
					int num = labelFixup.offset + 4 + 4 * int32AtCurrentPosition;
					code.Position += 4;
					for (int i = 0; i < int32AtCurrentPosition; i++)
					{
						int int32AtCurrentPosition2 = code.GetInt32AtCurrentPosition();
						code.Write(labels[int32AtCurrentPosition2] - num);
					}
				}
				else
				{
					code.Position = labelFixup.offset;
					byte byteAtCurrentPosition = code.GetByteAtCurrentPosition();
					int num2 = labels[labelFixup.label] - (code.Position + byteAtCurrentPosition);
					if (byteAtCurrentPosition == 1)
					{
						WriteByteBranchOffset(num2);
					}
					else
					{
						code.Write(num2);
					}
				}
			}
		}

		internal static void WriteTinyHeader(ByteBuffer bb, int length)
		{
			bb.Write((byte)(2 | (length << 2)));
		}

		private int WriteTinyHeaderAndCode(ByteBuffer bb)
		{
			int position = bb.Position;
			WriteTinyHeader(bb, code.Length);
			AddTokenFixups(bb.Position, moduleBuilder.tokenFixupOffsets, tokenFixups);
			bb.Write(code);
			return position;
		}

		internal static void WriteFatHeader(ByteBuffer bb, bool initLocals, bool exceptions, ushort maxStack, int codeLength, int localVarSigTok)
		{
			short num = 12291;
			if (initLocals)
			{
				num |= 0x10;
			}
			if (exceptions)
			{
				num |= 8;
			}
			bb.Write(num);
			bb.Write(maxStack);
			bb.Write(codeLength);
			bb.Write(localVarSigTok);
		}

		private int WriteFatHeaderAndCode(ByteBuffer bb, int localVarSigTok, bool initLocals)
		{
			bb.Align(4);
			int position = bb.Position;
			WriteFatHeader(bb, initLocals, exceptions.Count > 0, maxStack, code.Length, localVarSigTok);
			AddTokenFixups(bb.Position, moduleBuilder.tokenFixupOffsets, tokenFixups);
			bb.Write(code);
			if (exceptions.Count > 0)
			{
				exceptions.Sort(exceptions[0]);
				WriteExceptionHandlers(bb, exceptions);
			}
			return position;
		}

		internal static void WriteExceptionHandlers(ByteBuffer bb, List<ExceptionBlock> exceptions)
		{
			bb.Align(4);
			bool flag = false;
			if (exceptions.Count * 12 + 4 > 255)
			{
				flag = true;
			}
			else
			{
				foreach (ExceptionBlock exception in exceptions)
				{
					if (exception.tryOffset > 65535 || exception.tryLength > 255 || exception.handlerOffset > 65535 || exception.handlerLength > 255)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				bb.Write((byte)65);
				int num = exceptions.Count * 24 + 4;
				bb.Write((byte)num);
				bb.Write((short)(num >> 8));
				{
					foreach (ExceptionBlock exception2 in exceptions)
					{
						bb.Write((int)exception2.kind);
						bb.Write(exception2.tryOffset);
						bb.Write(exception2.tryLength);
						bb.Write(exception2.handlerOffset);
						bb.Write(exception2.handlerLength);
						bb.Write(exception2.filterOffsetOrExceptionTypeToken);
					}
					return;
				}
			}
			bb.Write((byte)1);
			bb.Write((byte)(exceptions.Count * 12 + 4));
			bb.Write((short)0);
			foreach (ExceptionBlock exception3 in exceptions)
			{
				bb.Write((short)exception3.kind);
				bb.Write((short)exception3.tryOffset);
				bb.Write((byte)exception3.tryLength);
				bb.Write((short)exception3.handlerOffset);
				bb.Write((byte)exception3.handlerLength);
				bb.Write(exception3.filterOffsetOrExceptionTypeToken);
			}
		}

		internal static void AddTokenFixups(int codeOffset, List<int> tokenFixupOffsets, IEnumerable<int> tokenFixups)
		{
			foreach (int tokenFixup in tokenFixups)
			{
				tokenFixupOffsets.Add(tokenFixup + codeOffset);
			}
		}
	}
}
