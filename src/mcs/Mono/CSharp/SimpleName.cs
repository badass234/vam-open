using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mono.CSharp
{
	public class SimpleName : ATypeNameExpression
	{
		public SimpleName(string name, Location l)
			: base(name, l)
		{
		}

		public SimpleName(string name, TypeArguments args, Location l)
			: base(name, args, l)
		{
		}

		public SimpleName(string name, int arity, Location l)
			: base(name, arity, l)
		{
		}

		public SimpleName GetMethodGroup()
		{
			return new SimpleName(base.Name, targs, loc);
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			return SimpleNameResolve(rc, null);
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
		{
			return SimpleNameResolve(ec, right_side);
		}

		public void Error_NameDoesNotExist(ResolveContext rc)
		{
			rc.Report.Error(103, loc, "The name `{0}' does not exist in the current context", base.Name);
		}

		protected virtual void Error_TypeOrNamespaceNotFound(IMemberContext ctx)
		{
			if (ctx.CurrentType != null)
			{
				MemberExpr memberExpr = Expression.MemberLookup(ctx, false, ctx.CurrentType, base.Name, 0, MemberLookupRestrictions.ExactArity, loc) as MemberExpr;
				if (memberExpr != null)
				{
					Expression.Error_UnexpectedKind(ctx, memberExpr, "type", memberExpr.KindName, loc);
					return;
				}
			}
			Report report = ctx.Module.Compiler.Report;
			FullNamedExpression fullNamedExpression = ctx.LookupNamespaceOrType(base.Name, base.Arity, LookupMode.IgnoreAccessibility, loc);
			if (fullNamedExpression != null)
			{
				report.SymbolRelatedToPreviousError(fullNamedExpression.Type);
				Expression.ErrorIsInaccesible(ctx, fullNamedExpression.GetSignatureForError(), loc);
				return;
			}
			fullNamedExpression = ctx.LookupNamespaceOrType(base.Name, -System.Math.Max(1, base.Arity), LookupMode.Probing, loc);
			if (fullNamedExpression != null)
			{
				Error_TypeArgumentsCannotBeUsed(ctx, fullNamedExpression.Type, loc);
				return;
			}
			List<string> list = ctx.Module.GlobalRootNamespace.FindTypeNamespaces(ctx, base.Name, base.Arity);
			if (list != null)
			{
				if (ctx is UsingAliasNamespace.AliasContext)
				{
					report.Error(246, loc, "The type or namespace name `{1}' could not be found. Consider using fully qualified name `{0}.{1}'", list[0], base.Name);
					return;
				}
				string arg = string.Join("' or `", list.ToArray());
				report.Error(246, loc, "The type or namespace name `{0}' could not be found. Are you missing `{1}' using directive?", base.Name, arg);
			}
			else
			{
				report.Error(246, loc, "The type or namespace name `{0}' could not be found. Are you missing an assembly reference?", base.Name);
			}
		}

		public override FullNamedExpression ResolveAsTypeOrNamespace(IMemberContext mc, bool allowUnboundTypeArguments)
		{
			FullNamedExpression fullNamedExpression = mc.LookupNamespaceOrType(base.Name, base.Arity, LookupMode.Normal, loc);
			if (fullNamedExpression != null)
			{
				if (fullNamedExpression.Type != null && base.Arity > 0)
				{
					if (base.HasTypeArguments)
					{
						GenericTypeExpr genericTypeExpr = new GenericTypeExpr(fullNamedExpression.Type, targs, loc);
						if (genericTypeExpr.ResolveAsType(mc) == null)
						{
							return null;
						}
						return genericTypeExpr;
					}
					targs.Resolve(mc, allowUnboundTypeArguments);
					return new GenericOpenTypeExpr(fullNamedExpression.Type, loc);
				}
				if (!(fullNamedExpression is NamespaceExpression))
				{
					return fullNamedExpression;
				}
			}
			if (base.Arity == 0 && base.Name == "dynamic" && !(mc is NamespaceContainer) && mc.Module.Compiler.Settings.Version > LanguageVersion.V_3)
			{
				if (!mc.Module.PredefinedAttributes.Dynamic.IsDefined)
				{
					mc.Module.Compiler.Report.Error(1980, base.Location, "Dynamic keyword requires `{0}' to be defined. Are you missing System.Core.dll assembly reference?", mc.Module.PredefinedAttributes.Dynamic.GetSignatureForError());
				}
				fullNamedExpression = new DynamicTypeExpr(loc);
				fullNamedExpression.ResolveAsType(mc);
			}
			if (fullNamedExpression != null)
			{
				return fullNamedExpression;
			}
			Error_TypeOrNamespaceNotFound(mc);
			return null;
		}

		public bool IsPossibleTypeOrNamespace(IMemberContext mc)
		{
			return mc.LookupNamespaceOrType(base.Name, base.Arity, LookupMode.Probing, loc) != null;
		}

		public bool IsPossibleType(IMemberContext mc)
		{
			return mc.LookupNamespaceOrType(base.Name, base.Arity, LookupMode.Probing, loc) is TypeExpr;
		}

		public override Expression LookupNameExpression(ResolveContext rc, MemberLookupRestrictions restrictions)
		{
			int num = base.Arity;
			bool flag = false;
			Block currentBlock = rc.CurrentBlock;
			INamedBlockVariable variable = null;
			bool flag2 = false;
			Tuple<FieldSpec, FieldInfo> tuple;
			while (true)
			{
				if (currentBlock != null && num == 0 && currentBlock.ParametersBlock.TopBlock.GetLocalName(base.Name, currentBlock.Original, ref variable))
				{
					if (!variable.IsDeclared)
					{
						flag = true;
						flag2 = true;
					}
					else
					{
						Expression expression = variable.CreateReferenceExpression(rc, loc);
						if (expression != null)
						{
							if (base.Arity > 0)
							{
								Expression.Error_TypeArgumentsCannotBeUsed(rc, "variable", base.Name, loc);
							}
							return expression;
						}
					}
				}
				for (TypeSpec typeSpec = rc.CurrentType; typeSpec != null; typeSpec = typeSpec.DeclaringType)
				{
					Expression expression = Expression.MemberLookup(rc, flag, typeSpec, base.Name, num, restrictions, loc);
					if (expression != null)
					{
						MemberExpr memberExpr = expression as MemberExpr;
						if (memberExpr != null)
						{
							if (flag)
							{
								if (variable != null)
								{
									if (!(memberExpr is FieldExpr) && !(memberExpr is ConstantExpr) && !(memberExpr is EventExpr) && !(memberExpr is PropertyExpr))
									{
										break;
									}
									rc.Report.Error(844, loc, "A local variable `{0}' cannot be used before it is declared. Consider renaming the local variable when it hides the member `{1}'", base.Name, memberExpr.GetSignatureForError());
								}
								else if (!(memberExpr is MethodGroupExpr) && !(memberExpr is PropertyExpr) && !(memberExpr is IndexerExpr))
								{
									Expression.ErrorIsInaccesible(rc, memberExpr.GetSignatureForError(), loc);
								}
							}
							else
							{
								PropertyExpr propertyExpr = memberExpr as PropertyExpr;
								if (propertyExpr != null)
								{
									if ((restrictions & MemberLookupRestrictions.ReadAccess) != MemberLookupRestrictions.None)
									{
										if (!propertyExpr.PropertyInfo.HasGet || !propertyExpr.PropertyInfo.Get.IsAccessible(rc))
										{
											break;
										}
										propertyExpr.Getter = propertyExpr.PropertyInfo.Get;
									}
									else
									{
										if (rc.HasSet(ResolveContext.Options.ConstructorScope) && propertyExpr.IsAutoPropertyAccess && propertyExpr.PropertyInfo.DeclaringType == rc.CurrentType && propertyExpr.IsStatic == rc.IsStatic)
										{
											return new FieldExpr(((Property)propertyExpr.PropertyInfo.MemberDefinition).BackingField, loc);
										}
										if (!propertyExpr.PropertyInfo.HasSet || !propertyExpr.PropertyInfo.Set.IsAccessible(rc))
										{
											flag2 = true;
											break;
										}
										propertyExpr.Setter = propertyExpr.PropertyInfo.Set;
									}
								}
							}
							memberExpr = memberExpr.ResolveMemberAccess(rc, null, null);
							if (base.Arity > 0)
							{
								targs.Resolve(rc, false);
								memberExpr.SetTypeArguments(rc, targs);
							}
							return memberExpr;
						}
						if (expression is TypeExpr)
						{
							break;
						}
					}
				}
				if ((restrictions & MemberLookupRestrictions.InvocableOnly) == 0 && !flag2 && IsPossibleTypeOrNamespace(rc))
				{
					return ResolveAsTypeOrNamespace(rc, false);
				}
				Expression expression2 = NamespaceContainer.LookupStaticUsings(rc, base.Name, base.Arity, loc);
				if (expression2 != null)
				{
					if (base.Arity > 0)
					{
						targs.Resolve(rc, false);
						(expression2 as MemberExpr)?.SetTypeArguments(rc, targs);
					}
					return expression2;
				}
				if ((restrictions & MemberLookupRestrictions.NameOfExcluded) == 0 && base.Name == "nameof")
				{
					return new NameOf(this);
				}
				if (flag)
				{
					if (flag2)
					{
						rc.Report.Error(841, loc, "A local variable `{0}' cannot be used before it is declared", base.Name);
					}
					else
					{
						if (base.Arity > 0)
						{
							TypeParameters currentTypeParameters = rc.CurrentTypeParameters;
							if (currentTypeParameters != null && currentTypeParameters.Find(base.Name) != null)
							{
								Expression.Error_TypeArgumentsCannotBeUsed(rc, "type parameter", base.Name, loc);
								return null;
							}
							TypeSpec typeSpec2 = rc.CurrentType;
							do
							{
								if (typeSpec2.MemberDefinition.TypeParametersCount > 0)
								{
									TypeParameterSpec[] typeParameters = typeSpec2.MemberDefinition.TypeParameters;
									for (int i = 0; i < typeParameters.Length; i++)
									{
										if (typeParameters[i].Name == base.Name)
										{
											Expression.Error_TypeArgumentsCannotBeUsed(rc, "type parameter", base.Name, loc);
											return null;
										}
									}
								}
								typeSpec2 = typeSpec2.DeclaringType;
							}
							while (typeSpec2 != null);
						}
						Expression expression;
						if ((restrictions & MemberLookupRestrictions.InvocableOnly) == 0)
						{
							expression = rc.LookupNamespaceOrType(base.Name, base.Arity, LookupMode.IgnoreAccessibility, loc);
							if (expression != null)
							{
								rc.Report.SymbolRelatedToPreviousError(expression.Type);
								Expression.ErrorIsInaccesible(rc, expression.GetSignatureForError(), loc);
								return expression;
							}
						}
						else
						{
							MemberExpr memberExpr2 = Expression.MemberLookup(rc, false, rc.CurrentType, base.Name, base.Arity, restrictions & ~MemberLookupRestrictions.InvocableOnly, loc) as MemberExpr;
							if (memberExpr2 != null)
							{
								Expression.Error_UnexpectedKind(rc, memberExpr2, "method group", memberExpr2.KindName, loc);
								return ErrorExpression.Instance;
							}
						}
						expression = rc.LookupNamespaceOrType(base.Name, -System.Math.Max(1, base.Arity), LookupMode.Probing, loc);
						if (expression != null)
						{
							if (expression.Type.Arity != base.Arity && (restrictions & MemberLookupRestrictions.IgnoreArity) == 0)
							{
								Error_TypeArgumentsCannotBeUsed(rc, expression.Type, loc);
								return expression;
							}
							if (expression is TypeExpr)
							{
								if (expression is TypeExpression)
								{
									expression = new TypeExpression(expression.Type, loc);
								}
								return expression;
							}
						}
						Error_NameDoesNotExist(rc);
					}
					return ErrorExpression.Instance;
				}
				if (rc.Module.Evaluator != null)
				{
					tuple = rc.Module.Evaluator.LookupField(base.Name);
					if (tuple != null)
					{
						break;
					}
				}
				num = 0;
				flag = true;
			}
			return new FieldExpr(tuple.Item1, loc);
		}

		private Expression SimpleNameResolve(ResolveContext ec, Expression right_side)
		{
			Expression expression = LookupNameExpression(ec, (right_side == null) ? MemberLookupRestrictions.ReadAccess : MemberLookupRestrictions.None);
			if (expression == null)
			{
				return null;
			}
			if (expression is FullNamedExpression && expression.eclass != ExprClass.Unresolved)
			{
				Expression.Error_UnexpectedKind(ec, expression, "variable", expression.ExprClassName, loc);
				return expression;
			}
			if (right_side != null)
			{
				return expression.ResolveLValue(ec, right_side);
			}
			return expression.Resolve(ec);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
