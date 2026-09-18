using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ImplicitDelegateCreation : DelegateCreation
	{
		private Field mg_cache;

		public ImplicitDelegateCreation(TypeSpec delegateType, MethodGroupExpr mg, Location loc)
		{
			type = delegateType;
			method_group = mg;
			base.loc = loc;
		}

		public static bool ContainsMethodTypeParameter(TypeSpec type)
		{
			TypeParameterSpec typeParameterSpec = type as TypeParameterSpec;
			if (typeParameterSpec != null)
			{
				return typeParameterSpec.IsMethodOwned;
			}
			ElementTypeSpec elementTypeSpec = type as ElementTypeSpec;
			if (elementTypeSpec != null)
			{
				return ContainsMethodTypeParameter(elementTypeSpec.Element);
			}
			TypeSpec[] typeArguments = type.TypeArguments;
			for (int i = 0; i < typeArguments.Length; i++)
			{
				if (ContainsMethodTypeParameter(typeArguments[i]))
				{
					return true;
				}
			}
			if (type.IsNested)
			{
				return ContainsMethodTypeParameter(type.DeclaringType);
			}
			return false;
		}

		private bool HasMvar()
		{
			if (ContainsMethodTypeParameter(type))
			{
				return false;
			}
			MethodSpec bestCandidate = method_group.BestCandidate;
			if (ContainsMethodTypeParameter(bestCandidate.DeclaringType))
			{
				return false;
			}
			if (bestCandidate.TypeArguments != null)
			{
				TypeSpec[] typeArguments = bestCandidate.TypeArguments;
				for (int i = 0; i < typeArguments.Length; i++)
				{
					if (ContainsMethodTypeParameter(typeArguments[i]))
					{
						return false;
					}
				}
			}
			return true;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			Expression expression = base.DoResolve(ec);
			if (expression == null)
			{
				return ErrorExpression.Instance;
			}
			if (ec.IsInProbingMode)
			{
				return expression;
			}
			if (method_group.InstanceExpression != null)
			{
				return expression;
			}
			if (!HasMvar())
			{
				return expression;
			}
			TypeDefinition partialContainer = ec.CurrentMemberDefinition.Parent.PartialContainer;
			int id = partialContainer.MethodGroupsCounter++;
			mg_cache = new Field(partialContainer, new TypeExpression(type, loc), Modifiers.PRIVATE | Modifiers.STATIC | Modifiers.COMPILER_GENERATED, new MemberName(CompilerGeneratedContainer.MakeName(null, "f", "mg$cache", id), loc), null);
			mg_cache.Define();
			partialContainer.AddField(mg_cache);
			return expression;
		}

		public override void Emit(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			if (mg_cache != null)
			{
				ec.Emit(OpCodes.Ldsfld, mg_cache.Spec);
				ec.Emit(OpCodes.Brtrue_S, label);
			}
			base.Emit(ec);
			if (mg_cache != null)
			{
				ec.Emit(OpCodes.Stsfld, mg_cache.Spec);
				ec.MarkLabel(label);
				ec.Emit(OpCodes.Ldsfld, mg_cache.Spec);
			}
		}
	}
}
