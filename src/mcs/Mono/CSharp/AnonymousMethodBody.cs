using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class AnonymousMethodBody : AnonymousExpression
	{
		protected readonly ParametersCompiled parameters;

		private AnonymousMethodStorey storey;

		private AnonymousMethodMethod method;

		private Field am_cache;

		private string block_name;

		private TypeInferenceContext return_inference;

		public override string ContainerType => "anonymous method";

		public MethodGroupExpr DirectMethodGroupConversion { get; set; }

		public override bool IsIterator => false;

		public ParametersCompiled Parameters => parameters;

		public TypeInferenceContext ReturnTypeInference
		{
			get
			{
				return return_inference;
			}
			set
			{
				return_inference = value;
			}
		}

		public override AnonymousMethodStorey Storey => storey;

		public AnonymousMethodBody(ParametersCompiled parameters, ParametersBlock block, TypeSpec return_type, TypeSpec delegate_type, Location loc)
			: base(block, return_type, loc)
		{
			type = delegate_type;
			this.parameters = parameters;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			ec.Report.Error(1945, loc, "An expression tree cannot contain an anonymous method expression");
			return null;
		}

		private bool Define(ResolveContext ec)
		{
			if (!base.Block.Resolved && Compatible(ec) == null)
			{
				return false;
			}
			if (block_name == null)
			{
				MemberCore memberCore = (MemberCore)ec.MemberContext;
				block_name = memberCore.MemberName.Basename;
			}
			return true;
		}

		private AnonymousMethodMethod DoCreateMethodHost(EmitContext ec)
		{
			TypeDefinition typeDefinition = null;
			TypeParameters typeParameters = null;
			ParametersCompiled parametersCompiled = parameters;
			ExplicitBlock explicitBlock = base.Block.Original.Explicit;
			Modifiers mod;
			if (explicitBlock.HasCapturedVariable || explicitBlock.HasCapturedThis)
			{
				typeDefinition = (storey = FindBestMethodStorey());
				if (storey == null)
				{
					ToplevelBlock topBlock = explicitBlock.ParametersBlock.TopBlock;
					StateMachine stateMachine = topBlock.StateMachine;
					if (explicitBlock.HasCapturedThis)
					{
						ParametersBlock parametersBlock = explicitBlock.ParametersBlock;
						StateMachine stateMachine2;
						do
						{
							stateMachine2 = parametersBlock.StateMachine;
							parametersBlock = ((parametersBlock.Parent == null) ? null : parametersBlock.Parent.ParametersBlock);
						}
						while (stateMachine2 == null && parametersBlock != null);
						if (stateMachine2 == null)
						{
							topBlock.RemoveThisReferenceFromChildrenBlock(explicitBlock);
						}
						else if (stateMachine2.Kind == MemberKind.Struct)
						{
							typeDefinition = stateMachine2.Parent.PartialContainer;
							typeParameters = stateMachine2.OriginalTypeParameters;
						}
						else if (stateMachine is IteratorStorey)
						{
							typeDefinition = (storey = stateMachine);
						}
					}
				}
				mod = ((storey != null) ? Modifiers.INTERNAL : Modifiers.PRIVATE);
			}
			else
			{
				if (ec.CurrentAnonymousMethod != null)
				{
					typeDefinition = (storey = ec.CurrentAnonymousMethod.Storey);
				}
				mod = Modifiers.PRIVATE | Modifiers.STATIC;
			}
			if (storey == null && typeParameters == null)
			{
				typeParameters = ec.CurrentTypeParameters;
			}
			if (typeDefinition == null)
			{
				typeDefinition = ec.CurrentTypeDefinition.Parent.PartialContainer;
			}
			string name = CompilerGeneratedContainer.MakeName((typeDefinition != storey) ? block_name : null, "m", null, typeDefinition.PartialContainer.CounterAnonymousMethods++);
			MemberName name2;
			if (typeParameters != null)
			{
				TypeParameters typeParameters2 = new TypeParameters(typeParameters.Count);
				for (int i = 0; i < typeParameters.Count; i++)
				{
					typeParameters2.Add(typeParameters[i].CreateHoistedCopy(null));
				}
				name2 = new MemberName(name, typeParameters2, base.Location);
			}
			else
			{
				name2 = new MemberName(name, base.Location);
			}
			return new AnonymousMethodMethod(typeDefinition, this, storey, new TypeExpression(ReturnType, base.Location), mod, name2, parametersCompiled);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			if (!Define(ec))
			{
				return null;
			}
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			if (method == null)
			{
				method = DoCreateMethodHost(ec);
				method.Define();
				method.PrepareEmit();
			}
			bool num = (method.ModFlags & Modifiers.STATIC) != 0;
			if (num && am_cache == null && !ec.IsStaticConstructor && !method.MemberName.IsGeneric)
			{
				TypeDefinition partialContainer = method.Parent.PartialContainer;
				int id = partialContainer.AnonymousMethodsCounter++;
				TypeSpec t = ((storey != null && storey.Mutator != null) ? storey.Mutator.Mutate(type) : type);
				am_cache = new Field(partialContainer, new TypeExpression(t, loc), Modifiers.PRIVATE | Modifiers.STATIC | Modifiers.COMPILER_GENERATED, new MemberName(CompilerGeneratedContainer.MakeName(null, "f", "am$cache", id), loc), null);
				am_cache.Define();
				partialContainer.AddField(am_cache);
			}
			Label label = ec.DefineLabel();
			if (am_cache != null)
			{
				ec.Emit(OpCodes.Ldsfld, am_cache.Spec);
				ec.Emit(OpCodes.Brtrue_S, label);
			}
			if (num)
			{
				ec.EmitNull();
			}
			else if (storey != null)
			{
				storey.GetStoreyInstanceExpression(ec).Resolve(new ResolveContext(ec.MemberContext))?.Emit(ec);
			}
			else
			{
				ec.EmitThis();
				if (ec.CurrentAnonymousMethod != null && ec.AsyncTaskStorey != null)
				{
					ec.Emit(OpCodes.Ldfld, ec.AsyncTaskStorey.HoistedThis.Field.Spec);
				}
			}
			MethodSpec methodSpec = method.Spec;
			if (storey != null && storey.MemberName.IsGeneric)
			{
				if (ec.IsAnonymousStoreyMutateRequired)
				{
					ec.Emit(OpCodes.Ldftn, methodSpec);
				}
				else
				{
					TypeSpec typeSpec = storey.Instance.Type;
					ec.Emit(OpCodes.Ldftn, TypeBuilder.GetMethod(typeSpec.GetMetaInfo(), (MethodInfo)methodSpec.GetMetaInfo()));
				}
			}
			else
			{
				if (methodSpec.IsGeneric)
				{
					StateMachine stateMachine = ((ec.CurrentAnonymousMethod == null) ? null : (ec.CurrentAnonymousMethod.Storey as StateMachine));
					methodSpec = methodSpec.MakeGenericMethod(targs: (stateMachine == null || stateMachine.OriginalTypeParameters == null) ? method.TypeParameters : stateMachine.CurrentTypeParameters.Types, context: ec.MemberContext);
				}
				ec.Emit(OpCodes.Ldftn, methodSpec);
			}
			MethodSpec constructor = Delegate.GetConstructor(type);
			ec.Emit(OpCodes.Newobj, constructor);
			if (am_cache != null)
			{
				ec.Emit(OpCodes.Stsfld, am_cache.Spec);
				ec.MarkLabel(label);
				ec.Emit(OpCodes.Ldsfld, am_cache.Spec);
			}
		}

		public override void EmitStatement(EmitContext ec)
		{
			throw new NotImplementedException();
		}

		private AnonymousMethodStorey FindBestMethodStorey()
		{
			for (Block parent = base.Block.Parent; parent != null; parent = parent.Parent)
			{
				AnonymousMethodStorey anonymousMethodStorey = parent.Explicit.AnonymousMethodStorey;
				if (anonymousMethodStorey != null)
				{
					return anonymousMethodStorey;
				}
			}
			return null;
		}

		public override string GetSignatureForError()
		{
			return type.GetSignatureForError();
		}
	}
}
