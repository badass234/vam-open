using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class Iterator : StateMachineInitializer
	{
		private sealed class TryFinallyBlockProxyStatement : Statement
		{
			private TryFinallyBlock block;

			private Iterator iterator;

			public TryFinallyBlockProxyStatement(Iterator iterator, TryFinallyBlock block)
			{
				this.iterator = iterator;
				this.block = block;
			}

			protected override void CloneTo(CloneContext clonectx, Statement target)
			{
				throw new NotSupportedException();
			}

			protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
			{
				throw new NotSupportedException();
			}

			protected override void DoEmit(EmitContext ec)
			{
				ec.CurrentAnonymousMethod = iterator;
				using (ec.With(BuilderContext.Options.OmitDebugInfo, !ec.HasMethodSymbolBuilder))
				{
					block.EmitFinallyBody(ec);
				}
			}
		}

		public readonly IMethodData OriginalMethod;

		public readonly bool IsEnumerable;

		public readonly TypeSpec OriginalIteratorType;

		private int finally_hosts_counter;

		public ToplevelBlock Container => OriginalMethod.Block;

		public override string ContainerType => "iterator";

		public override bool IsIterator => true;

		public Iterator(ParametersBlock block, IMethodData method, TypeDefinition host, TypeSpec iterator_type, bool is_enumerable)
			: base(block, host, host.Compiler.BuiltinTypes.Bool)
		{
			OriginalMethod = method;
			OriginalIteratorType = iterator_type;
			IsEnumerable = is_enumerable;
			type = method.ReturnType;
		}

		public Method CreateFinallyHost(TryFinallyBlock block)
		{
			Method method = new Method(storey, new TypeExpression(storey.Compiler.BuiltinTypes.Void, loc), Modifiers.COMPILER_GENERATED, new MemberName(CompilerGeneratedContainer.MakeName(null, null, "Finally", finally_hosts_counter++), loc), ParametersCompiled.EmptyReadOnlyParameters, null);
			method.Block = new ToplevelBlock(method.Compiler, method.ParameterInfo, loc, Mono.CSharp.Block.Flags.CompilerGenerated | Mono.CSharp.Block.Flags.NoFlowAnalysis);
			method.Block.AddStatement(new TryFinallyBlockProxyStatement(this, block));
			return method;
		}

		public void EmitYieldBreak(EmitContext ec, bool unwind_protect)
		{
			ec.Emit(unwind_protect ? OpCodes.Leave : OpCodes.Br, move_next_error);
		}

		public override string GetSignatureForError()
		{
			return OriginalMethod.GetSignatureForError();
		}

		public override void Emit(EmitContext ec)
		{
			storey.Instance.Emit(ec);
			if (IsEnumerable)
			{
				ec.Emit(OpCodes.Dup);
				ec.EmitInt(-2);
				FieldSpec fieldSpec = storey.PC.Spec;
				if (storey.MemberName.IsGeneric)
				{
					fieldSpec = MemberCache.GetMember(Storey.Instance.Type, fieldSpec);
				}
				ec.Emit(OpCodes.Stfld, fieldSpec);
			}
		}

		public void EmitDispose(EmitContext ec)
		{
			if (resume_points == null)
			{
				return;
			}
			Label label = ec.DefineLabel();
			Label[] array = null;
			for (int i = 0; i < resume_points.Count; i++)
			{
				Label label2 = resume_points[i].PrepareForDispose(ec, label);
				if (label2.Equals(label) && array == null)
				{
					continue;
				}
				if (array == null)
				{
					array = new Label[resume_points.Count + 1];
					for (int j = 0; j <= i; j++)
					{
						array[j] = label;
					}
				}
				array[i + 1] = label2;
			}
			if (array != null)
			{
				current_pc = ec.GetTemporaryLocal(ec.BuiltinTypes.UInt);
				ec.EmitThis();
				ec.Emit(OpCodes.Ldfld, storey.PC.Spec);
				ec.Emit(OpCodes.Stloc, current_pc);
			}
			ec.EmitThis();
			ec.EmitInt(1);
			ec.Emit(OpCodes.Stfld, ((IteratorStorey)storey).DisposingField.Spec);
			ec.EmitThis();
			ec.EmitInt(-1);
			ec.Emit(OpCodes.Stfld, storey.PC.Spec);
			if (array != null)
			{
				ec.Emit(OpCodes.Ldloc, current_pc);
				ec.Emit(OpCodes.Switch, array);
				foreach (ResumableStatement resume_point in resume_points)
				{
					resume_point.EmitForDispose(ec, current_pc, label, true);
				}
			}
			ec.MarkLabel(label);
		}

		public override void EmitStatement(EmitContext ec)
		{
			throw new NotImplementedException();
		}

		public override void InjectYield(EmitContext ec, Expression expr, int resume_pc, bool unwind_protect, Label resume_point)
		{
			FieldExpr fieldExpr = new FieldExpr(((IteratorStorey)storey).CurrentField, loc);
			fieldExpr.InstanceExpression = new CompilerGeneratedThis(storey.CurrentType, loc);
			fieldExpr.EmitAssign(ec, expr, false, false);
			base.InjectYield(ec, expr, resume_pc, unwind_protect, resume_point);
			EmitLeave(ec, unwind_protect);
			ec.MarkLabel(resume_point);
		}

		public static void CreateIterator(IMethodData method, TypeDefinition parent, Modifiers modifiers)
		{
			TypeSpec returnType = method.ReturnType;
			if (returnType == null)
			{
				return;
			}
			TypeSpec original_iterator_type;
			bool is_enumerable;
			if (!CheckType(returnType, parent, out original_iterator_type, out is_enumerable))
			{
				parent.Compiler.Report.Error(1624, method.Location, "The body of `{0}' cannot be an iterator block because `{1}' is not an iterator interface type", method.GetSignatureForError(), returnType.GetSignatureForError());
				return;
			}
			ParametersCompiled parameterInfo = method.ParameterInfo;
			for (int i = 0; i < parameterInfo.Count; i++)
			{
				Parameter parameter = parameterInfo[i];
				if ((parameter.ModFlags & Parameter.Modifier.RefOutMask) != Parameter.Modifier.NONE)
				{
					parent.Compiler.Report.Error(1623, parameter.Location, "Iterators cannot have ref or out parameters");
					return;
				}
				if (parameter is ArglistParameter)
				{
					parent.Compiler.Report.Error(1636, method.Location, "__arglist is not allowed in parameter list of iterators");
					return;
				}
				if (parameterInfo.Types[i].IsPointer)
				{
					parent.Compiler.Report.Error(1637, parameter.Location, "Iterators cannot have unsafe parameters or yield types");
					return;
				}
			}
			if ((modifiers & Modifiers.UNSAFE) != 0)
			{
				parent.Compiler.Report.Error(1629, method.Location, "Unsafe code may not appear in iterators");
			}
			method.Block = method.Block.ConvertToIterator(method, parent, original_iterator_type, is_enumerable);
		}

		private static bool CheckType(TypeSpec ret, TypeContainer parent, out TypeSpec original_iterator_type, out bool is_enumerable)
		{
			original_iterator_type = null;
			is_enumerable = false;
			if (ret.BuiltinType == BuiltinTypeSpec.Type.IEnumerable)
			{
				original_iterator_type = parent.Compiler.BuiltinTypes.Object;
				is_enumerable = true;
				return true;
			}
			if (ret.BuiltinType == BuiltinTypeSpec.Type.IEnumerator)
			{
				original_iterator_type = parent.Compiler.BuiltinTypes.Object;
				is_enumerable = false;
				return true;
			}
			InflatedTypeSpec inflatedTypeSpec = ret as InflatedTypeSpec;
			if (inflatedTypeSpec == null)
			{
				return false;
			}
			ITypeDefinition memberDefinition = inflatedTypeSpec.MemberDefinition;
			PredefinedType iEnumerableGeneric = parent.Module.PredefinedTypes.IEnumerableGeneric;
			if (iEnumerableGeneric.Define() && iEnumerableGeneric.TypeSpec.MemberDefinition == memberDefinition)
			{
				original_iterator_type = inflatedTypeSpec.TypeArguments[0];
				is_enumerable = true;
				return true;
			}
			iEnumerableGeneric = parent.Module.PredefinedTypes.IEnumeratorGeneric;
			if (iEnumerableGeneric.Define() && iEnumerableGeneric.TypeSpec.MemberDefinition == memberDefinition)
			{
				original_iterator_type = inflatedTypeSpec.TypeArguments[0];
				is_enumerable = false;
				return true;
			}
			return false;
		}
	}
}
