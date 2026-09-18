using System.Collections.Generic;

namespace Mono.CSharp
{
	public class AnonymousMethodStorey : HoistedStoreyClass
	{
		private struct StoreyFieldPair
		{
			public readonly AnonymousMethodStorey Storey;

			public readonly Field Field;

			public StoreyFieldPair(AnonymousMethodStorey storey, Field field)
			{
				Storey = storey;
				Field = field;
			}
		}

		private sealed class ThisInitializer : Statement
		{
			private readonly HoistedThis hoisted_this;

			private readonly AnonymousMethodStorey parent;

			public ThisInitializer(HoistedThis hoisted_this, AnonymousMethodStorey parent)
			{
				this.hoisted_this = hoisted_this;
				this.parent = parent;
			}

			protected override void DoEmit(EmitContext ec)
			{
				Expression source = ((parent != null) ? ((Expression)new FieldExpr(parent.HoistedThis.Field, Location.Null)
				{
					InstanceExpression = new CompilerGeneratedThis(ec.CurrentType, Location.Null)
				}) : ((Expression)new CompilerGeneratedThis(ec.CurrentType, loc)));
				hoisted_this.EmitAssign(ec, source, false, false);
			}

			protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
			{
				return false;
			}

			protected override void CloneTo(CloneContext clonectx, Statement target)
			{
			}
		}

		public readonly int ID;

		public readonly ExplicitBlock OriginalSourceBlock;

		private List<StoreyFieldPair> used_parent_storeys;

		private List<ExplicitBlock> children_references;

		protected List<HoistedParameter> hoisted_params;

		private List<HoistedParameter> hoisted_local_params;

		protected List<HoistedVariable> hoisted_locals;

		protected HoistedThis hoisted_this;

		public Expression Instance;

		private bool initialize_hoisted_this;

		private AnonymousMethodStorey hoisted_this_parent;

		public HoistedThis HoistedThis
		{
			get
			{
				return hoisted_this;
			}
			set
			{
				hoisted_this = value;
			}
		}

		public IList<ExplicitBlock> ReferencesFromChildrenBlock => children_references;

		public AnonymousMethodStorey(ExplicitBlock block, TypeDefinition parent, MemberBase host, TypeParameters tparams, string name, MemberKind kind)
			: base(parent, CompilerGeneratedContainer.MakeMemberName(host, name, parent.PartialContainer.CounterAnonymousContainers, tparams, block.StartLocation), tparams, (Modifiers)0, kind)
		{
			OriginalSourceBlock = block;
			ID = parent.PartialContainer.CounterAnonymousContainers++;
		}

		public void AddCapturedThisField(EmitContext ec, AnonymousMethodStorey parent)
		{
			TypeExpr type = new TypeExpression(ec.CurrentType, base.Location);
			Field field = AddCompilerGeneratedField("$this", type);
			hoisted_this = new HoistedThis(this, field);
			initialize_hoisted_this = true;
			hoisted_this_parent = parent;
		}

		public Field AddCapturedVariable(string name, TypeSpec type)
		{
			CheckMembersDefined();
			FullNamedExpression type2 = new TypeExpression(type, base.Location);
			if (!spec.IsGenericOrParentIsGeneric)
			{
				return AddCompilerGeneratedField(name, type2);
			}
			Field field = new HoistedField(this, type2, Modifiers.INTERNAL | Modifiers.COMPILER_GENERATED, name, null, base.Location);
			AddField(field);
			return field;
		}

		protected Field AddCompilerGeneratedField(string name, FullNamedExpression type)
		{
			return AddCompilerGeneratedField(name, type, false);
		}

		protected Field AddCompilerGeneratedField(string name, FullNamedExpression type, bool privateAccess)
		{
			Modifiers mod = (Modifiers)(0x100000 | (privateAccess ? 4 : 8));
			Field field = new Field(this, type, mod, new MemberName(name, base.Location), null);
			AddField(field);
			return field;
		}

		public void AddReferenceFromChildrenBlock(ExplicitBlock block)
		{
			if (children_references == null)
			{
				children_references = new List<ExplicitBlock>();
			}
			if (!children_references.Contains(block))
			{
				children_references.Add(block);
			}
		}

		public void AddParentStoreyReference(EmitContext ec, AnonymousMethodStorey storey)
		{
			CheckMembersDefined();
			if (used_parent_storeys == null)
			{
				used_parent_storeys = new List<StoreyFieldPair>();
			}
			else if (used_parent_storeys.Exists((StoreyFieldPair i) => i.Storey == storey))
			{
				return;
			}
			TypeExpr type = storey.CreateStoreyTypeExpression(ec);
			Field field = AddCompilerGeneratedField("<>f__ref$" + storey.ID, type);
			used_parent_storeys.Add(new StoreyFieldPair(storey, field));
		}

		public void CaptureLocalVariable(ResolveContext ec, LocalVariable localVariable)
		{
			if (this is StateMachine)
			{
				if (ec.CurrentBlock.ParametersBlock != localVariable.Block.ParametersBlock)
				{
					ec.CurrentBlock.Explicit.HasCapturedVariable = true;
				}
			}
			else
			{
				ec.CurrentBlock.Explicit.HasCapturedVariable = true;
			}
			HoistedVariable hoistedVariable = localVariable.HoistedVariant;
			if (hoistedVariable != null && hoistedVariable.Storey != this && hoistedVariable.Storey is StateMachine)
			{
				hoistedVariable.Storey.hoisted_locals.Remove(hoistedVariable);
				hoistedVariable.Storey.Members.Remove(hoistedVariable.Field);
				hoistedVariable = null;
			}
			if (hoistedVariable == null)
			{
				hoistedVariable = (localVariable.HoistedVariant = new HoistedLocalVariable(this, localVariable, GetVariableMangledName(localVariable)));
				if (hoisted_locals == null)
				{
					hoisted_locals = new List<HoistedVariable>();
				}
				hoisted_locals.Add(hoistedVariable);
			}
			if (ec.CurrentBlock.Explicit != localVariable.Block.Explicit && !(hoistedVariable.Storey is StateMachine) && hoistedVariable.Storey != null)
			{
				hoistedVariable.Storey.AddReferenceFromChildrenBlock(ec.CurrentBlock.Explicit);
			}
		}

		public void CaptureParameter(ResolveContext ec, ParametersBlock.ParameterInfo parameterInfo, ParameterReference parameterReference)
		{
			if (!(this is StateMachine))
			{
				ec.CurrentBlock.Explicit.HasCapturedVariable = true;
			}
			HoistedParameter hoistedParameter = parameterInfo.Parameter.HoistedVariant;
			if (parameterInfo.Block.StateMachine != null)
			{
				if (hoistedParameter == null && parameterInfo.Block.StateMachine != this)
				{
					StateMachine stateMachine = parameterInfo.Block.StateMachine;
					hoistedParameter = new HoistedParameter(stateMachine, parameterReference);
					parameterInfo.Parameter.HoistedVariant = hoistedParameter;
					if (stateMachine.hoisted_params == null)
					{
						stateMachine.hoisted_params = new List<HoistedParameter>();
					}
					stateMachine.hoisted_params.Add(hoistedParameter);
				}
				if (hoistedParameter != null && hoistedParameter.Storey != this && hoistedParameter.Storey is StateMachine)
				{
					if (hoisted_local_params == null)
					{
						hoisted_local_params = new List<HoistedParameter>();
					}
					hoisted_local_params.Add(hoistedParameter);
					hoistedParameter = null;
				}
			}
			if (hoistedParameter == null)
			{
				hoistedParameter = new HoistedParameter(this, parameterReference);
				parameterInfo.Parameter.HoistedVariant = hoistedParameter;
				if (hoisted_params == null)
				{
					hoisted_params = new List<HoistedParameter>();
				}
				hoisted_params.Add(hoistedParameter);
			}
			if (ec.CurrentBlock.Explicit != parameterInfo.Block)
			{
				hoistedParameter.Storey.AddReferenceFromChildrenBlock(ec.CurrentBlock.Explicit);
			}
		}

		private TypeExpr CreateStoreyTypeExpression(EmitContext ec)
		{
			if (CurrentTypeParameters != null)
			{
				TypeParameters typeParameters = ((ec.CurrentAnonymousMethod != null && ec.CurrentAnonymousMethod.Storey != null) ? ec.CurrentAnonymousMethod.Storey.CurrentTypeParameters : ec.CurrentTypeParameters);
				TypeArguments typeArguments = new TypeArguments();
				for (int i = 0; i < typeParameters.Count; i++)
				{
					typeArguments.Add(new SimpleName(typeParameters[i].Name, base.Location));
				}
				return new GenericTypeExpr(base.Definition, typeArguments, base.Location);
			}
			return new TypeExpression(CurrentType, base.Location);
		}

		public void SetNestedStoryParent(AnonymousMethodStorey parentStorey)
		{
			Parent = parentStorey;
			spec.IsGeneric = false;
			spec.DeclaringType = parentStorey.CurrentType;
			base.MemberName.TypeParameters = null;
		}

		protected override bool DoResolveTypeParameters()
		{
			if (CurrentTypeParameters != null)
			{
				for (int i = 0; i < CurrentTypeParameters.Count; i++)
				{
					TypeParameterSpec type = CurrentTypeParameters[i].Type;
					type.BaseType = mutator.Mutate(type.BaseType);
					if (type.InterfacesDefined != null)
					{
						TypeSpec[] array = new TypeSpec[type.InterfacesDefined.Length];
						for (int j = 0; j < array.Length; j++)
						{
							array[j] = mutator.Mutate(type.InterfacesDefined[j]);
						}
						type.InterfacesDefined = array;
					}
					if (type.TypeArguments != null)
					{
						type.TypeArguments = mutator.Mutate(type.TypeArguments);
					}
				}
			}
			Parent.CurrentType.MemberCache.AddMember(spec);
			return true;
		}

		public void EmitStoreyInstantiation(EmitContext ec, ExplicitBlock block)
		{
			if (Instance != null)
			{
				throw new InternalErrorException();
			}
			ResolveContext resolveContext = new ResolveContext(ec.MemberContext);
			resolveContext.CurrentBlock = block;
			TypeExpr typeExpr = CreateStoreyTypeExpression(ec);
			Expression expression = new New(typeExpr, null, base.Location).Resolve(resolveContext);
			if (ec.CurrentAnonymousMethod is StateMachineInitializer && (block.HasYield || block.HasAwait))
			{
				Field field = ec.CurrentAnonymousMethod.Storey.AddCompilerGeneratedField(LocalVariable.GetCompilerGeneratedName(block), typeExpr, true);
				field.Define();
				field.Emit();
				FieldExpr fieldExpr = new FieldExpr(field, base.Location);
				fieldExpr.InstanceExpression = new CompilerGeneratedThis(ec.CurrentType, base.Location);
				fieldExpr.EmitAssign(ec, expression, false, false);
				Instance = fieldExpr;
			}
			else
			{
				TemporaryVariableReference temporaryVariableReference = TemporaryVariableReference.Create(expression.Type, block, base.Location);
				if (expression.Type.IsStruct)
				{
					temporaryVariableReference.LocalInfo.CreateBuilder(ec);
				}
				else
				{
					temporaryVariableReference.EmitAssign(ec, expression);
				}
				Instance = temporaryVariableReference;
			}
			EmitHoistedFieldsInitialization(resolveContext, ec);
		}

		private void EmitHoistedFieldsInitialization(ResolveContext rc, EmitContext ec)
		{
			if (used_parent_storeys != null)
			{
				foreach (StoreyFieldPair used_parent_storey in used_parent_storeys)
				{
					Expression storeyInstanceExpression = GetStoreyInstanceExpression(ec);
					FieldSpec member = used_parent_storey.Field.Spec;
					if (TypeManager.IsGenericType(storeyInstanceExpression.Type))
					{
						member = MemberCache.GetMember(storeyInstanceExpression.Type, member);
					}
					SimpleAssign simpleAssign = new SimpleAssign(new FieldExpr(member, base.Location)
					{
						InstanceExpression = storeyInstanceExpression
					}, used_parent_storey.Storey.GetStoreyInstanceExpression(ec));
					if (simpleAssign.Resolve(rc) != null)
					{
						simpleAssign.EmitStatement(ec);
					}
				}
			}
			if (initialize_hoisted_this)
			{
				rc.CurrentBlock.AddScopeStatement(new ThisInitializer(hoisted_this, hoisted_this_parent));
			}
			AnonymousExpression currentAnonymousMethod = ec.CurrentAnonymousMethod;
			ec.CurrentAnonymousMethod = null;
			if (hoisted_params != null)
			{
				EmitHoistedParameters(ec, hoisted_params);
			}
			ec.CurrentAnonymousMethod = currentAnonymousMethod;
		}

		protected virtual void EmitHoistedParameters(EmitContext ec, List<HoistedParameter> hoisted)
		{
			foreach (HoistedParameter hp in hoisted)
			{
				if (hp == null)
				{
					continue;
				}
				if (hoisted_local_params != null)
				{
					FieldExpr fieldExpr = new FieldExpr(hoisted_local_params.Find((HoistedParameter l) => l.Parameter.Parameter == hp.Parameter.Parameter).Field, base.Location);
					fieldExpr.InstanceExpression = new CompilerGeneratedThis(CurrentType, base.Location);
					hp.EmitAssign(ec, fieldExpr, false, false);
				}
				else
				{
					hp.EmitHoistingAssignment(ec);
				}
			}
		}

		private Field GetReferencedStoreyField(AnonymousMethodStorey storey)
		{
			if (used_parent_storeys == null)
			{
				return null;
			}
			foreach (StoreyFieldPair used_parent_storey in used_parent_storeys)
			{
				if (used_parent_storey.Storey == storey)
				{
					return used_parent_storey.Field;
				}
			}
			return null;
		}

		public Expression GetStoreyInstanceExpression(EmitContext ec)
		{
			AnonymousExpression currentAnonymousMethod = ec.CurrentAnonymousMethod;
			if (currentAnonymousMethod == null)
			{
				return Instance;
			}
			if (currentAnonymousMethod.Storey == null)
			{
				return Instance;
			}
			Field referencedStoreyField = currentAnonymousMethod.Storey.GetReferencedStoreyField(this);
			if (referencedStoreyField == null)
			{
				if (currentAnonymousMethod.Storey == this)
				{
					return new CompilerGeneratedThis(CurrentType, base.Location);
				}
				return Instance;
			}
			return new FieldExpr(referencedStoreyField, base.Location)
			{
				InstanceExpression = new CompilerGeneratedThis(CurrentType, base.Location)
			};
		}

		protected virtual string GetVariableMangledName(LocalVariable local_info)
		{
			return local_info.Name;
		}
	}
}
