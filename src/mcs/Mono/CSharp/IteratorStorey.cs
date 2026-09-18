using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal class IteratorStorey : StateMachine
	{
		private class GetEnumeratorMethod : StateMachineMethod
		{
			private sealed class GetEnumeratorStatement : Statement
			{
				private readonly IteratorStorey host;

				private readonly StateMachineMethod host_method;

				private Expression new_storey;

				public GetEnumeratorStatement(IteratorStorey host, StateMachineMethod host_method)
				{
					this.host = host;
					this.host_method = host_method;
					loc = host_method.Location;
				}

				protected override void CloneTo(CloneContext clonectx, Statement target)
				{
					throw new NotSupportedException();
				}

				public override bool Resolve(BlockContext ec)
				{
					TypeExpression requested_type = new TypeExpression(host.Definition, loc);
					List<Expression> list = null;
					if (host.hoisted_this != null)
					{
						list = new List<Expression>((host.hoisted_params == null) ? 1 : (host.HoistedParameters.Count + 1));
						HoistedThis hoisted_this = host.hoisted_this;
						FieldExpr fieldExpr = new FieldExpr(hoisted_this.Field, loc);
						fieldExpr.InstanceExpression = new CompilerGeneratedThis(ec.CurrentType, loc);
						list.Add(new ElementInitializer(hoisted_this.Field.Name, fieldExpr, loc));
					}
					if (host.hoisted_params != null)
					{
						if (list == null)
						{
							list = new List<Expression>(host.HoistedParameters.Count);
						}
						for (int i = 0; i < host.hoisted_params.Count; i++)
						{
							HoistedParameter hoistedParameter = host.hoisted_params[i];
							FieldExpr fieldExpr2 = new FieldExpr((host.hoisted_params_copy[i] ?? hoistedParameter).Field, loc);
							fieldExpr2.InstanceExpression = new CompilerGeneratedThis(ec.CurrentType, loc);
							list.Add(new ElementInitializer(hoistedParameter.Field.Name, fieldExpr2, loc));
						}
					}
					if (list != null)
					{
						new_storey = new NewInitialize(requested_type, null, new CollectionOrObjectInitializers(list, loc), loc);
					}
					else
					{
						new_storey = new New(requested_type, null, loc);
					}
					new_storey = new_storey.Resolve(ec);
					if (new_storey != null)
					{
						new_storey = Convert.ImplicitConversionRequired(ec, new_storey, host_method.MemberType, loc);
					}
					return true;
				}

				protected override void DoEmit(EmitContext ec)
				{
					Label label = ec.DefineLabel();
					ec.EmitThis();
					ec.Emit(OpCodes.Ldflda, host.PC.Spec);
					ec.EmitInt(0);
					ec.EmitInt(-2);
					MethodSpec methodSpec = ec.Module.PredefinedMembers.InterlockedCompareExchange.Resolve(loc);
					if (methodSpec != null)
					{
						ec.Emit(OpCodes.Call, methodSpec);
					}
					ec.EmitInt(-2);
					ec.Emit(OpCodes.Bne_Un_S, label);
					ec.EmitThis();
					ec.Emit(OpCodes.Ret);
					ec.MarkLabel(label);
					new_storey.Emit(ec);
					ec.Emit(OpCodes.Ret);
				}

				protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
				{
					throw new NotImplementedException();
				}

				public override Reachability MarkReachable(Reachability rc)
				{
					base.MarkReachable(rc);
					return Reachability.CreateUnreachable();
				}
			}

			private GetEnumeratorMethod(IteratorStorey host, FullNamedExpression returnType, MemberName name)
				: base(host, null, returnType, Modifiers.DEBUGGER_HIDDEN, name, Mono.CSharp.Block.Flags.CompilerGenerated | Mono.CSharp.Block.Flags.NoFlowAnalysis)
			{
			}

			public static GetEnumeratorMethod Create(IteratorStorey host, FullNamedExpression returnType, MemberName name)
			{
				return Create(host, returnType, name, null);
			}

			public static GetEnumeratorMethod Create(IteratorStorey host, FullNamedExpression returnType, MemberName name, Statement statement)
			{
				GetEnumeratorMethod getEnumeratorMethod = new GetEnumeratorMethod(host, returnType, name);
				Statement s = statement ?? new GetEnumeratorStatement(host, getEnumeratorMethod);
				getEnumeratorMethod.block.AddStatement(s);
				return getEnumeratorMethod;
			}
		}

		private class DisposeMethod : StateMachineMethod
		{
			private sealed class DisposeMethodStatement : Statement
			{
				private Iterator iterator;

				public DisposeMethodStatement(Iterator iterator)
				{
					this.iterator = iterator;
					loc = iterator.Location;
				}

				protected override void CloneTo(CloneContext clonectx, Statement target)
				{
					throw new NotSupportedException();
				}

				public override bool Resolve(BlockContext ec)
				{
					return true;
				}

				protected override void DoEmit(EmitContext ec)
				{
					ec.CurrentAnonymousMethod = iterator;
					iterator.EmitDispose(ec);
				}

				protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
				{
					throw new NotImplementedException();
				}
			}

			public DisposeMethod(IteratorStorey host)
				: base(host, null, new TypeExpression(host.Compiler.BuiltinTypes.Void, host.Location), Modifiers.PUBLIC | Modifiers.DEBUGGER_HIDDEN, new MemberName("Dispose", host.Location), Mono.CSharp.Block.Flags.CompilerGenerated | Mono.CSharp.Block.Flags.NoFlowAnalysis)
			{
				host.Members.Add(this);
				base.Block.AddStatement(new DisposeMethodStatement(host.Iterator));
			}
		}

		private class DynamicMethodGroupExpr : MethodGroupExpr
		{
			private readonly Method method;

			public DynamicMethodGroupExpr(Method method, Location loc)
				: base((IList<MemberSpec>)null, (TypeSpec)null, loc)
			{
				this.method = method;
				eclass = ExprClass.Unresolved;
			}

			protected override Expression DoResolve(ResolveContext ec)
			{
				Methods = new List<MemberSpec>(1) { method.Spec };
				type = method.Parent.Definition;
				InstanceExpression = new CompilerGeneratedThis(type, base.Location);
				return base.DoResolve(ec);
			}
		}

		private class DynamicFieldExpr : FieldExpr
		{
			private readonly Field field;

			public DynamicFieldExpr(Field field, Location loc)
				: base(loc)
			{
				this.field = field;
			}

			protected override Expression DoResolve(ResolveContext ec)
			{
				spec = field.Spec;
				type = spec.MemberType;
				InstanceExpression = new CompilerGeneratedThis(type, base.Location);
				return base.DoResolve(ec);
			}
		}

		public readonly Iterator Iterator;

		private List<HoistedParameter> hoisted_params_copy;

		private TypeExpr iterator_type_expr;

		private Field current_field;

		private Field disposing_field;

		private TypeSpec generic_enumerator_type;

		private TypeSpec generic_enumerable_type;

		public Field CurrentField => current_field;

		public Field DisposingField => disposing_field;

		public IList<HoistedParameter> HoistedParameters => hoisted_params;

		public IteratorStorey(Iterator iterator)
			: base(iterator.Container.ParametersBlock, iterator.Host, iterator.OriginalMethod as MemberBase, iterator.OriginalMethod.CurrentTypeParameters, "Iterator", MemberKind.Class)
		{
			Iterator = iterator;
		}

		protected override Constructor DefineDefaultConstructor(bool is_static)
		{
			Constructor constructor = base.DefineDefaultConstructor(is_static);
			constructor.ModFlags |= Modifiers.DEBUGGER_HIDDEN;
			return constructor;
		}

		protected override TypeSpec[] ResolveBaseTypes(out FullNamedExpression base_class)
		{
			TypeSpec typeSpec = Iterator.OriginalIteratorType;
			if (base.Mutator != null)
			{
				typeSpec = base.Mutator.Mutate(typeSpec);
			}
			iterator_type_expr = new TypeExpression(typeSpec, base.Location);
			List<TypeSpec> list = new List<TypeSpec>(5);
			if (Iterator.IsEnumerable)
			{
				list.Add(Compiler.BuiltinTypes.IEnumerable);
				if (Module.PredefinedTypes.IEnumerableGeneric.Define())
				{
					generic_enumerable_type = Module.PredefinedTypes.IEnumerableGeneric.TypeSpec.MakeGenericType(Module, new TypeSpec[1] { typeSpec });
					list.Add(generic_enumerable_type);
				}
			}
			list.Add(Compiler.BuiltinTypes.IEnumerator);
			list.Add(Compiler.BuiltinTypes.IDisposable);
			PredefinedType iEnumeratorGeneric = Module.PredefinedTypes.IEnumeratorGeneric;
			if (iEnumeratorGeneric.Define())
			{
				generic_enumerator_type = iEnumeratorGeneric.TypeSpec.MakeGenericType(Module, new TypeSpec[1] { typeSpec });
				list.Add(generic_enumerator_type);
			}
			base_class = null;
			base_type = Compiler.BuiltinTypes.Object;
			return list.ToArray();
		}

		protected override bool DoDefineMembers()
		{
			current_field = AddCompilerGeneratedField("$current", iterator_type_expr);
			disposing_field = AddCompilerGeneratedField("$disposing", new TypeExpression(Compiler.BuiltinTypes.Bool, base.Location));
			if (Iterator.IsEnumerable && hoisted_params != null)
			{
				hoisted_params_copy = new List<HoistedParameter>(hoisted_params.Count);
				foreach (HoistedParameter hoisted_param in hoisted_params)
				{
					HoistedParameter item = ((!hoisted_param.IsAssigned) ? null : new HoistedParameter(hoisted_param, "<$>" + hoisted_param.Field.Name));
					hoisted_params_copy.Add(item);
				}
			}
			if (generic_enumerator_type != null)
			{
				Define_Current(true);
			}
			Define_Current(false);
			new DisposeMethod(this);
			Define_Reset();
			if (Iterator.IsEnumerable)
			{
				FullNamedExpression explicitInterface = new TypeExpression(Compiler.BuiltinTypes.IEnumerable, base.Location);
				MemberName name = new MemberName("GetEnumerator", null, explicitInterface, Location.Null);
				if (generic_enumerator_type != null)
				{
					explicitInterface = new TypeExpression(generic_enumerable_type, base.Location);
					MemberName name2 = new MemberName("GetEnumerator", null, explicitInterface, Location.Null);
					Method item2 = GetEnumeratorMethod.Create(this, new TypeExpression(generic_enumerator_type, base.Location), name2);
					Return statement = new Return(new Invocation(new DynamicMethodGroupExpr(item2, base.Location), null), base.Location);
					Method item3 = GetEnumeratorMethod.Create(this, new TypeExpression(Compiler.BuiltinTypes.IEnumerator, base.Location), name, statement);
					base.Members.Add(item3);
					base.Members.Add(item2);
				}
				else
				{
					base.Members.Add(GetEnumeratorMethod.Create(this, new TypeExpression(Compiler.BuiltinTypes.IEnumerator, base.Location), name));
				}
			}
			return base.DoDefineMembers();
		}

		private void Define_Current(bool is_generic)
		{
			FullNamedExpression explicitInterface;
			TypeExpr type;
			if (is_generic)
			{
				explicitInterface = new TypeExpression(generic_enumerator_type, base.Location);
				type = iterator_type_expr;
			}
			else
			{
				explicitInterface = new TypeExpression(Module.Compiler.BuiltinTypes.IEnumerator, base.Location);
				type = new TypeExpression(Compiler.BuiltinTypes.Object, base.Location);
			}
			MemberName name = new MemberName("Current", null, explicitInterface, base.Location);
			ToplevelBlock toplevelBlock = new ToplevelBlock(Compiler, ParametersCompiled.EmptyReadOnlyParameters, base.Location, Block.Flags.CompilerGenerated | Block.Flags.NoFlowAnalysis);
			toplevelBlock.AddStatement(new Return(new DynamicFieldExpr(CurrentField, base.Location), base.Location));
			Property property = new Property(this, type, Modifiers.COMPILER_GENERATED | Modifiers.DEBUGGER_HIDDEN, name, null);
			property.Get = new PropertyBase.GetMethod(property, Modifiers.COMPILER_GENERATED, null, base.Location);
			property.Get.Block = toplevelBlock;
			base.Members.Add(property);
		}

		private void Define_Reset()
		{
			Method method = new Method(this, new TypeExpression(Compiler.BuiltinTypes.Void, base.Location), Modifiers.PUBLIC | Modifiers.COMPILER_GENERATED | Modifiers.DEBUGGER_HIDDEN, new MemberName("Reset", base.Location), ParametersCompiled.EmptyReadOnlyParameters, null);
			base.Members.Add(method);
			method.Block = new ToplevelBlock(Compiler, method.ParameterInfo, base.Location, Block.Flags.CompilerGenerated | Block.Flags.NoFlowAnalysis);
			TypeSpec typeSpec = Module.PredefinedTypes.NotSupportedException.Resolve();
			if (typeSpec != null)
			{
				method.Block.AddStatement(new Throw(new New(new TypeExpression(typeSpec, base.Location), null, base.Location), base.Location));
			}
		}

		protected override void EmitHoistedParameters(EmitContext ec, List<HoistedParameter> hoisted)
		{
			base.EmitHoistedParameters(ec, hoisted);
			if (hoisted_params_copy != null)
			{
				base.EmitHoistedParameters(ec, hoisted_params_copy);
			}
		}
	}
}
