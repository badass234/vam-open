using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class AsyncTaskStorey : StateMachine
	{
		private int awaiters;

		private Field builder;

		private readonly TypeSpec return_type;

		private MethodSpec set_result;

		private MethodSpec set_exception;

		private MethodSpec builder_factory;

		private MethodSpec builder_start;

		private PropertySpec task;

		private int locals_captured;

		private Dictionary<TypeSpec, List<Field>> stack_fields;

		private Dictionary<TypeSpec, List<Field>> awaiter_fields;

		public Expression HoistedReturnValue { get; set; }

		public TypeSpec ReturnType => return_type;

		public PropertySpec Task => task;

		protected override TypeAttributes TypeAttr => base.TypeAttr & ~TypeAttributes.SequentialLayout;

		public AsyncTaskStorey(ParametersBlock block, IMemberContext context, AsyncInitializer initializer, TypeSpec type)
			: base(block, initializer.Host, context.CurrentMemberDefinition as MemberBase, context.CurrentTypeParameters, "async", MemberKind.Struct)
		{
			return_type = type;
			awaiter_fields = new Dictionary<TypeSpec, List<Field>>();
		}

		public Field AddAwaiter(TypeSpec type)
		{
			if (mutator != null)
			{
				type = mutator.Mutate(type);
			}
			List<Field> value;
			if (awaiter_fields.TryGetValue(type, out value))
			{
				foreach (Field item in value)
				{
					if (item.IsAvailableForReuse)
					{
						item.IsAvailableForReuse = false;
						return item;
					}
				}
			}
			Field field = AddCompilerGeneratedField("$awaiter" + awaiters++.ToString("X"), new TypeExpression(type, base.Location), true);
			field.Define();
			if (value == null)
			{
				value = new List<Field>();
				awaiter_fields.Add(type, value);
			}
			value.Add(field);
			return field;
		}

		public Field AddCapturedLocalVariable(TypeSpec type, bool requiresUninitialized = false)
		{
			if (mutator != null)
			{
				type = mutator.Mutate(type);
			}
			List<Field> value = null;
			if (stack_fields == null)
			{
				stack_fields = new Dictionary<TypeSpec, List<Field>>();
			}
			else if (stack_fields.TryGetValue(type, out value) && !requiresUninitialized)
			{
				foreach (Field item in value)
				{
					if (item.IsAvailableForReuse)
					{
						item.IsAvailableForReuse = false;
						return item;
					}
				}
			}
			Field field = AddCompilerGeneratedField("$stack" + locals_captured++.ToString("X"), new TypeExpression(type, base.Location), true);
			field.Define();
			if (value == null)
			{
				value = new List<Field>();
				stack_fields.Add(type, value);
			}
			value.Add(field);
			return field;
		}

		protected override bool DoDefineMembers()
		{
			bool flag = false;
			PredefinedMembers predefinedMembers = Module.PredefinedMembers;
			PredefinedType predefinedType;
			PredefinedMember<MethodSpec> predefinedMember;
			PredefinedMember<MethodSpec> predefinedMember2;
			PredefinedMember<MethodSpec> predefinedMember3;
			PredefinedMember<MethodSpec> predefinedMember4;
			PredefinedMember<MethodSpec> predefinedMember5;
			if (return_type.Kind == MemberKind.Void)
			{
				predefinedType = Module.PredefinedTypes.AsyncVoidMethodBuilder;
				predefinedMember = predefinedMembers.AsyncVoidMethodBuilderCreate;
				predefinedMember2 = predefinedMembers.AsyncVoidMethodBuilderStart;
				predefinedMember3 = predefinedMembers.AsyncVoidMethodBuilderSetResult;
				predefinedMember4 = predefinedMembers.AsyncVoidMethodBuilderSetException;
				predefinedMember5 = predefinedMembers.AsyncVoidMethodBuilderSetStateMachine;
			}
			else if (return_type == Module.PredefinedTypes.Task.TypeSpec)
			{
				predefinedType = Module.PredefinedTypes.AsyncTaskMethodBuilder;
				predefinedMember = predefinedMembers.AsyncTaskMethodBuilderCreate;
				predefinedMember2 = predefinedMembers.AsyncTaskMethodBuilderStart;
				predefinedMember3 = predefinedMembers.AsyncTaskMethodBuilderSetResult;
				predefinedMember4 = predefinedMembers.AsyncTaskMethodBuilderSetException;
				predefinedMember5 = predefinedMembers.AsyncTaskMethodBuilderSetStateMachine;
				task = predefinedMembers.AsyncTaskMethodBuilderTask.Get();
			}
			else
			{
				predefinedType = Module.PredefinedTypes.AsyncTaskMethodBuilderGeneric;
				predefinedMember = predefinedMembers.AsyncTaskMethodBuilderGenericCreate;
				predefinedMember2 = predefinedMembers.AsyncTaskMethodBuilderGenericStart;
				predefinedMember3 = predefinedMembers.AsyncTaskMethodBuilderGenericSetResult;
				predefinedMember4 = predefinedMembers.AsyncTaskMethodBuilderGenericSetException;
				predefinedMember5 = predefinedMembers.AsyncTaskMethodBuilderGenericSetStateMachine;
				task = predefinedMembers.AsyncTaskMethodBuilderGenericTask.Get();
				flag = true;
			}
			set_result = predefinedMember3.Get();
			set_exception = predefinedMember4.Get();
			builder_factory = predefinedMember.Get();
			builder_start = predefinedMember2.Get();
			PredefinedType iAsyncStateMachine = Module.PredefinedTypes.IAsyncStateMachine;
			MethodSpec methodSpec = predefinedMember5.Get();
			if (!predefinedType.Define() || !iAsyncStateMachine.Define() || set_result == null || builder_factory == null || set_exception == null || methodSpec == null || builder_start == null || !Module.PredefinedTypes.INotifyCompletion.Define())
			{
				base.Report.Error(1993, base.Location, "Cannot find compiler required types for asynchronous functions support. Are you targeting the wrong framework version?");
				return base.DoDefineMembers();
			}
			TypeSpec typeSpec = predefinedType.TypeSpec;
			if (flag)
			{
				TypeSpec[] targs = return_type.TypeArguments;
				if (mutator != null)
				{
					targs = mutator.Mutate(targs);
				}
				typeSpec = typeSpec.MakeGenericType(Module, targs);
				set_result = MemberCache.GetMember(typeSpec, set_result);
				set_exception = MemberCache.GetMember(typeSpec, set_exception);
				methodSpec = MemberCache.GetMember(typeSpec, methodSpec);
				if (task != null)
				{
					task = MemberCache.GetMember(typeSpec, task);
				}
			}
			builder = AddCompilerGeneratedField("$builder", new TypeExpression(typeSpec, base.Location));
			Method method = new Method(this, new TypeExpression(Compiler.BuiltinTypes.Void, base.Location), Modifiers.PUBLIC | Modifiers.COMPILER_GENERATED | Modifiers.DEBUGGER_HIDDEN, new MemberName("SetStateMachine"), ParametersCompiled.CreateFullyResolved(new Parameter(new TypeExpression(iAsyncStateMachine.TypeSpec, base.Location), "stateMachine", Parameter.Modifier.NONE, null, base.Location), iAsyncStateMachine.TypeSpec), null);
			ToplevelBlock toplevelBlock = new ToplevelBlock(Compiler, method.ParameterInfo, base.Location);
			toplevelBlock.IsCompilerGenerated = true;
			method.Block = toplevelBlock;
			base.Members.Add(method);
			if (!base.DoDefineMembers())
			{
				return false;
			}
			MethodGroupExpr methodGroupExpr = MethodGroupExpr.CreatePredefined(methodSpec, typeSpec, base.Location);
			methodGroupExpr.InstanceExpression = new FieldExpr(builder, base.Location);
			ParameterReference parameterReference = toplevelBlock.GetParameterReference(0, base.Location);
			parameterReference.Type = iAsyncStateMachine.TypeSpec;
			parameterReference.eclass = ExprClass.Variable;
			Arguments arguments = new Arguments(1);
			arguments.Add(new Argument(parameterReference));
			method.Block.AddStatement(new StatementExpression(new Invocation(methodGroupExpr, arguments)));
			if (flag)
			{
				HoistedReturnValue = TemporaryVariableReference.Create(typeSpec.TypeArguments[0], base.StateMachineMethod.Block, base.Location);
			}
			return true;
		}

		public void EmitAwaitOnCompletedDynamic(EmitContext ec, FieldExpr awaiter)
		{
			PredefinedType iCriticalNotifyCompletion = Module.PredefinedTypes.ICriticalNotifyCompletion;
			if (!iCriticalNotifyCompletion.Define())
			{
				throw new NotImplementedException();
			}
			LocalTemporary localTemporary = new LocalTemporary(iCriticalNotifyCompletion.TypeSpec);
			Label label = ec.DefineLabel();
			Label label2 = ec.DefineLabel();
			awaiter.Emit(ec);
			ec.Emit(OpCodes.Isinst, iCriticalNotifyCompletion.TypeSpec);
			localTemporary.Store(ec);
			localTemporary.Emit(ec);
			ec.Emit(OpCodes.Brtrue_S, label);
			LocalTemporary localTemporary2 = new LocalTemporary(Module.PredefinedTypes.INotifyCompletion.TypeSpec);
			awaiter.Emit(ec);
			ec.Emit(OpCodes.Castclass, localTemporary2.Type);
			localTemporary2.Store(ec);
			EmitOnCompleted(ec, localTemporary2, false);
			localTemporary2.Release(ec);
			ec.Emit(OpCodes.Br_S, label2);
			ec.MarkLabel(label);
			EmitOnCompleted(ec, localTemporary, true);
			ec.MarkLabel(label2);
			localTemporary.Release(ec);
		}

		public void EmitAwaitOnCompleted(EmitContext ec, FieldExpr awaiter)
		{
			bool unsafeVersion = false;
			if (Module.PredefinedTypes.ICriticalNotifyCompletion.Define())
			{
				unsafeVersion = awaiter.Type.ImplementsInterface(Module.PredefinedTypes.ICriticalNotifyCompletion.TypeSpec, false);
			}
			EmitOnCompleted(ec, awaiter, unsafeVersion);
		}

		private void EmitOnCompleted(EmitContext ec, Expression awaiter, bool unsafeVersion)
		{
			PredefinedMembers predefinedMembers = Module.PredefinedMembers;
			bool flag = false;
			PredefinedMember<MethodSpec> predefinedMember;
			if (return_type.Kind == MemberKind.Void)
			{
				predefinedMember = (unsafeVersion ? predefinedMembers.AsyncVoidMethodBuilderOnCompletedUnsafe : predefinedMembers.AsyncVoidMethodBuilderOnCompleted);
			}
			else if (return_type == Module.PredefinedTypes.Task.TypeSpec)
			{
				predefinedMember = (unsafeVersion ? predefinedMembers.AsyncTaskMethodBuilderOnCompletedUnsafe : predefinedMembers.AsyncTaskMethodBuilderOnCompleted);
			}
			else
			{
				predefinedMember = (unsafeVersion ? predefinedMembers.AsyncTaskMethodBuilderGenericOnCompletedUnsafe : predefinedMembers.AsyncTaskMethodBuilderGenericOnCompleted);
				flag = true;
			}
			MethodSpec methodSpec = predefinedMember.Resolve(base.Location);
			if (methodSpec == null)
			{
				return;
			}
			if (flag)
			{
				methodSpec = MemberCache.GetMember(set_result.DeclaringType, methodSpec);
			}
			methodSpec = methodSpec.MakeGenericMethod(this, awaiter.Type, ec.CurrentType);
			MethodSpec methodSpec2 = methodSpec;
			MethodGroupExpr methodGroupExpr = MethodGroupExpr.CreatePredefined(methodSpec2, methodSpec2.DeclaringType, base.Location);
			methodGroupExpr.InstanceExpression = new FieldExpr(builder, base.Location)
			{
				InstanceExpression = new CompilerGeneratedThis(ec.CurrentType, base.Location)
			};
			Arguments arguments = new Arguments(2);
			arguments.Add(new Argument(awaiter, Argument.AType.Ref));
			arguments.Add(new Argument(new CompilerGeneratedThis(CurrentType, base.Location), Argument.AType.Ref));
			using (ec.With(BuilderContext.Options.OmitDebugInfo, true))
			{
				methodGroupExpr.EmitCall(ec, arguments, true);
			}
		}

		public void EmitInitializer(EmitContext ec)
		{
			if (builder == null)
			{
				return;
			}
			TemporaryVariableReference temporaryVariableReference = (TemporaryVariableReference)Instance;
			FieldSpec member = builder.Spec;
			if (base.MemberName.Arity > 0)
			{
				member = MemberCache.GetMember(temporaryVariableReference.Type, member);
			}
			if (builder_factory.DeclaringType.IsGeneric)
			{
				TypeSpec[] typeArguments = return_type.TypeArguments;
				InflatedTypeSpec container = builder_factory.DeclaringType.MakeGenericType(Module, typeArguments);
				builder_factory = MemberCache.GetMember(container, builder_factory);
				builder_start = MemberCache.GetMember(container, builder_start);
			}
			temporaryVariableReference.AddressOf(ec, AddressOp.Store);
			ec.Emit(OpCodes.Call, builder_factory);
			ec.Emit(OpCodes.Stfld, member);
			temporaryVariableReference.AddressOf(ec, AddressOp.Store);
			ec.Emit(OpCodes.Ldflda, member);
			if (Task != null)
			{
				ec.Emit(OpCodes.Dup);
			}
			temporaryVariableReference.AddressOf(ec, AddressOp.Store);
			ec.Emit(OpCodes.Call, builder_start.MakeGenericMethod(Module, temporaryVariableReference.Type));
			if (Task != null)
			{
				MethodSpec getter = Task.Get;
				if (base.MemberName.Arity > 0)
				{
					getter = MemberCache.GetMember(member.MemberType, getter);
				}
				PropertyExpr propertyExpr = new PropertyExpr(Task, base.Location);
				propertyExpr.InstanceExpression = EmptyExpression.Null;
				propertyExpr.Getter = getter;
				propertyExpr.Emit(ec);
			}
		}

		public void EmitSetException(EmitContext ec, LocalVariableReference exceptionVariable)
		{
			MethodGroupExpr methodGroupExpr = MethodGroupExpr.CreatePredefined(set_exception, set_exception.DeclaringType, base.Location);
			methodGroupExpr.InstanceExpression = new FieldExpr(builder, base.Location)
			{
				InstanceExpression = new CompilerGeneratedThis(ec.CurrentType, base.Location)
			};
			Arguments arguments = new Arguments(1);
			arguments.Add(new Argument(exceptionVariable));
			using (ec.With(BuilderContext.Options.OmitDebugInfo, true))
			{
				methodGroupExpr.EmitCall(ec, arguments, true);
			}
		}

		public void EmitSetResult(EmitContext ec)
		{
			MethodGroupExpr methodGroupExpr = MethodGroupExpr.CreatePredefined(set_result, set_result.DeclaringType, base.Location);
			methodGroupExpr.InstanceExpression = new FieldExpr(builder, base.Location)
			{
				InstanceExpression = new CompilerGeneratedThis(ec.CurrentType, base.Location)
			};
			Arguments arguments;
			if (HoistedReturnValue == null)
			{
				arguments = new Arguments(0);
			}
			else
			{
				arguments = new Arguments(1);
				arguments.Add(new Argument(HoistedReturnValue));
			}
			using (ec.With(BuilderContext.Options.OmitDebugInfo, true))
			{
				methodGroupExpr.EmitCall(ec, arguments, true);
			}
		}

		protected override TypeSpec[] ResolveBaseTypes(out FullNamedExpression base_class)
		{
			base_type = Compiler.BuiltinTypes.ValueType;
			base_class = null;
			PredefinedType iAsyncStateMachine = Module.PredefinedTypes.IAsyncStateMachine;
			if (iAsyncStateMachine.Define())
			{
				return new TypeSpec[1] { iAsyncStateMachine.TypeSpec };
			}
			return null;
		}
	}
}
