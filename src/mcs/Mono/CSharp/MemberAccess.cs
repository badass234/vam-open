using System;
using System.Collections.Generic;
using Mono.CSharp.Linq;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class MemberAccess : ATypeNameExpression
	{
		protected Expression expr;

		public Expression LeftExpression => expr;

		public override Location StartLocation
		{
			get
			{
				if (expr != null)
				{
					return expr.StartLocation;
				}
				return loc;
			}
		}

		public MemberAccess(Expression expr, string id)
			: base(id, expr.Location)
		{
			this.expr = expr;
		}

		public MemberAccess(Expression expr, string identifier, Location loc)
			: base(identifier, loc)
		{
			this.expr = expr;
		}

		public MemberAccess(Expression expr, string identifier, TypeArguments args, Location loc)
			: base(identifier, args, loc)
		{
			this.expr = expr;
		}

		public MemberAccess(Expression expr, string identifier, int arity, Location loc)
			: base(identifier, arity, loc)
		{
			this.expr = expr;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			Expression expression = LookupNameExpression(rc, MemberLookupRestrictions.ReadAccess);
			if (expression != null)
			{
				expression = expression.Resolve(rc, ResolveFlags.VariableOrValue | ResolveFlags.Type | ResolveFlags.MethodGroup);
			}
			return expression;
		}

		public override Expression DoResolveLValue(ResolveContext rc, Expression rhs)
		{
			Expression expression = LookupNameExpression(rc, MemberLookupRestrictions.None);
			if (expression is TypeExpr)
			{
				expression.Error_UnexpectedKind(rc, ResolveFlags.VariableOrValue, loc);
				return null;
			}
			if (expression != null)
			{
				expression = expression.ResolveLValue(rc, rhs);
			}
			return expression;
		}

		protected virtual void Error_OperatorCannotBeApplied(ResolveContext rc, TypeSpec type)
		{
			if (type == InternalType.NullLiteral && rc.IsRuntimeBinder)
			{
				rc.Report.Error(10000, loc, "Cannot perform member binding on `null' value");
			}
			else
			{
				expr.Error_OperatorCannotBeApplied(rc, loc, ".", type);
			}
		}

		public override bool HasConditionalAccess()
		{
			return LeftExpression.HasConditionalAccess();
		}

		public static bool IsValidDotExpression(TypeSpec type)
		{
			if ((type.Kind & (MemberKind.NestedMask | MemberKind.TypeParameter | MemberKind.ArrayType)) == 0)
			{
				return type.BuiltinType == BuiltinTypeSpec.Type.Dynamic;
			}
			return true;
		}

		public override Expression LookupNameExpression(ResolveContext rc, MemberLookupRestrictions restrictions)
		{
			SimpleName simpleName = expr as SimpleName;
			if (simpleName != null)
			{
				expr = simpleName.LookupNameExpression(rc, MemberLookupRestrictions.ExactArity | MemberLookupRestrictions.ReadAccess);
				if (expr is VariableReference || expr is ConstantExpr || expr is TransparentMemberAccess || expr is EventExpr)
				{
					expr = expr.Resolve(rc);
				}
				else if (expr is TypeParameterExpr)
				{
					expr.Error_UnexpectedKind(rc, ResolveFlags.VariableOrValue | ResolveFlags.Type, simpleName.Location);
					expr = null;
				}
			}
			else
			{
				bool flag = false;
				if (!rc.HasSet(ResolveContext.Options.ConditionalAccessReceiver) && expr.HasConditionalAccess())
				{
					flag = true;
					using (rc.Set(ResolveContext.Options.ConditionalAccessReceiver))
					{
						expr = expr.Resolve(rc, ResolveFlags.VariableOrValue | ResolveFlags.Type);
					}
				}
				if (!flag)
				{
					expr = expr.Resolve(rc, ResolveFlags.VariableOrValue | ResolveFlags.Type);
				}
			}
			if (expr == null)
			{
				return null;
			}
			NamespaceExpression namespaceExpression = expr as NamespaceExpression;
			if (namespaceExpression != null)
			{
				FullNamedExpression fullNamedExpression = namespaceExpression.LookupTypeOrNamespace(rc, base.Name, base.Arity, LookupMode.Normal, loc);
				if (fullNamedExpression == null)
				{
					namespaceExpression.Error_NamespaceDoesNotExist(rc, base.Name, base.Arity, loc);
					return null;
				}
				if (base.Arity > 0)
				{
					if (base.HasTypeArguments)
					{
						return new GenericTypeExpr(fullNamedExpression.Type, targs, loc);
					}
					targs.Resolve(rc, false);
				}
				return fullNamedExpression;
			}
			TypeSpec typeSpec = expr.Type;
			if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				(expr as MemberExpr)?.ResolveInstanceExpression(rc, null);
				Arguments arguments = new Arguments(1);
				arguments.Add(new Argument(expr));
				return new DynamicMemberBinder(base.Name, arguments, loc);
			}
			ConditionalMemberAccess conditionalMemberAccess = this as ConditionalMemberAccess;
			if (conditionalMemberAccess != null)
			{
				if (!Expression.IsNullPropagatingValid(expr.Type))
				{
					expr.Error_OperatorCannotBeApplied(rc, loc, "?", expr.Type);
					return null;
				}
				if (typeSpec.IsNullableType)
				{
					expr = Unwrap.Create(expr, true).Resolve(rc);
					typeSpec = expr.Type;
				}
			}
			if (!IsValidDotExpression(typeSpec))
			{
				Error_OperatorCannotBeApplied(rc, typeSpec);
				return null;
			}
			int arity = base.Arity;
			bool flag2 = false;
			Expression expression;
			while (true)
			{
				expression = Expression.MemberLookup(rc, flag2, typeSpec, base.Name, arity, restrictions, loc);
				if (expression == null && MethodGroupExpr.IsExtensionMethodArgument(expr))
				{
					ExtensionMethodCandidates extensionMethodCandidates = rc.LookupExtensionMethod(base.Name, arity);
					if (extensionMethodCandidates != null)
					{
						ExtensionMethodGroupExpr extensionMethodGroupExpr = new ExtensionMethodGroupExpr(extensionMethodCandidates, expr, loc);
						if (base.HasTypeArguments)
						{
							if (!targs.Resolve(rc, false))
							{
								return null;
							}
							extensionMethodGroupExpr.SetTypeArguments(rc, targs);
						}
						if (conditionalMemberAccess != null)
						{
							extensionMethodGroupExpr.ConditionalAccess = true;
						}
						return extensionMethodGroupExpr.Resolve(rc);
					}
				}
				if (flag2)
				{
					if (expression == null)
					{
						List<MissingTypeSpecReference> missingDependencies = typeSpec.GetMissingDependencies();
						if (missingDependencies != null)
						{
							ImportedTypeDefinition.Error_MissingDependency(rc, missingDependencies, loc);
						}
						else if (expr is TypeExpr)
						{
							base.Error_TypeDoesNotContainDefinition(rc, typeSpec, base.Name);
						}
						else
						{
							Error_TypeDoesNotContainDefinition(rc, typeSpec, base.Name);
						}
						return null;
					}
					if (!(expression is MethodGroupExpr) && !(expression is PropertyExpr) && !(expression is TypeExpr))
					{
						Expression.ErrorIsInaccesible(rc, expression.GetSignatureForError(), loc);
					}
					break;
				}
				if (expression != null)
				{
					break;
				}
				arity = 0;
				restrictions &= ~MemberLookupRestrictions.InvocableOnly;
				flag2 = true;
			}
			TypeExpr typeExpr = expression as TypeExpr;
			if (typeExpr != null)
			{
				if (!(expr is TypeExpr) && (simpleName == null || expr.ProbeIdenticalTypeName(rc, expr, simpleName) == expr))
				{
					rc.Report.Error(572, loc, "`{0}': cannot reference a type through an expression. Consider using `{1}' instead", base.Name, typeExpr.GetSignatureForError());
				}
				if (!typeExpr.Type.IsAccessible(rc))
				{
					rc.Report.SymbolRelatedToPreviousError(expression.Type);
					Expression.ErrorIsInaccesible(rc, expression.Type.GetSignatureForError(), loc);
					return null;
				}
				if (base.HasTypeArguments)
				{
					return new GenericTypeExpr(expression.Type, targs, loc);
				}
				return expression;
			}
			MemberExpr memberExpr = expression as MemberExpr;
			if (simpleName != null && memberExpr.IsStatic && (expr = memberExpr.ProbeIdenticalTypeName(rc, expr, simpleName)) != expr)
			{
				simpleName = null;
			}
			if (conditionalMemberAccess != null)
			{
				memberExpr.ConditionalAccess = true;
			}
			memberExpr = memberExpr.ResolveMemberAccess(rc, expr, simpleName);
			if (base.Arity > 0)
			{
				if (!targs.Resolve(rc, false))
				{
					return null;
				}
				memberExpr.SetTypeArguments(rc, targs);
			}
			return memberExpr;
		}

		public override FullNamedExpression ResolveAsTypeOrNamespace(IMemberContext rc, bool allowUnboundTypeArguments)
		{
			FullNamedExpression fullNamedExpression = expr as FullNamedExpression;
			if (fullNamedExpression == null)
			{
				expr.ResolveAsType(rc);
				return null;
			}
			FullNamedExpression fullNamedExpression2 = fullNamedExpression.ResolveAsTypeOrNamespace(rc, allowUnboundTypeArguments);
			if (fullNamedExpression2 == null)
			{
				return null;
			}
			NamespaceExpression namespaceExpression = fullNamedExpression2 as NamespaceExpression;
			if (namespaceExpression != null)
			{
				FullNamedExpression fullNamedExpression3 = namespaceExpression.LookupTypeOrNamespace(rc, base.Name, base.Arity, LookupMode.Normal, loc);
				if (fullNamedExpression3 == null)
				{
					namespaceExpression.Error_NamespaceDoesNotExist(rc, base.Name, base.Arity, loc);
				}
				else if (base.Arity > 0)
				{
					if (base.HasTypeArguments)
					{
						fullNamedExpression3 = new GenericTypeExpr(fullNamedExpression3.Type, targs, loc);
						if (fullNamedExpression3.ResolveAsType(rc) == null)
						{
							return null;
						}
					}
					else
					{
						targs.Resolve(rc, allowUnboundTypeArguments);
						fullNamedExpression3 = new GenericOpenTypeExpr(fullNamedExpression3.Type, loc);
					}
				}
				return fullNamedExpression3;
			}
			TypeSpec typeSpec = fullNamedExpression2.ResolveAsType(rc);
			if (typeSpec == null)
			{
				return null;
			}
			TypeSpec typeSpec2 = typeSpec;
			if (TypeManager.IsGenericParameter(typeSpec2))
			{
				rc.Module.Compiler.Report.Error(704, loc, "A nested type cannot be specified through a type parameter `{0}'", typeSpec.GetSignatureForError());
				return null;
			}
			QualifiedAliasMember qualifiedAliasMember = this as QualifiedAliasMember;
			if (qualifiedAliasMember != null)
			{
				rc.Module.Compiler.Report.Error(431, loc, "Alias `{0}' cannot be used with `::' since it denotes a type. Consider replacing `::' with `.'", qualifiedAliasMember.Alias);
			}
			TypeSpec typeSpec3 = null;
			while (typeSpec2 != null)
			{
				typeSpec3 = MemberCache.FindNestedType(typeSpec2, base.Name, base.Arity);
				if (typeSpec3 == null)
				{
					if (typeSpec2 == typeSpec)
					{
						Error_IdentifierNotFound(rc, typeSpec2);
						return null;
					}
					typeSpec2 = typeSpec;
					typeSpec3 = MemberCache.FindNestedType(typeSpec2, base.Name, base.Arity);
					Expression.ErrorIsInaccesible(rc, typeSpec3.GetSignatureForError(), loc);
					break;
				}
				if (typeSpec3.IsAccessible(rc) || typeSpec2.MemberDefinition == rc.CurrentMemberDefinition)
				{
					break;
				}
				typeSpec2 = typeSpec2.BaseType;
			}
			TypeExpr typeExpr;
			if (base.Arity <= 0)
			{
				typeExpr = ((!(fullNamedExpression2 is GenericOpenTypeExpr)) ? new TypeExpression(typeSpec3, loc) : new GenericOpenTypeExpr(typeSpec3, loc));
			}
			else if (base.HasTypeArguments)
			{
				typeExpr = new GenericTypeExpr(typeSpec3, targs, loc);
			}
			else
			{
				targs.Resolve(rc, allowUnboundTypeArguments && !(fullNamedExpression2 is GenericTypeExpr));
				typeExpr = new GenericOpenTypeExpr(typeSpec3, loc);
			}
			if (typeExpr.ResolveAsType(rc) == null)
			{
				return null;
			}
			return typeExpr;
		}

		public void Error_IdentifierNotFound(IMemberContext rc, TypeSpec expr_type)
		{
			TypeSpec typeSpec = MemberCache.FindNestedType(expr_type, base.Name, -System.Math.Max(1, base.Arity));
			if (typeSpec != null)
			{
				Error_TypeArgumentsCannotBeUsed(rc, typeSpec, expr.Location);
				return;
			}
			Expression expression = Expression.MemberLookup(rc, false, expr_type, base.Name, 0, MemberLookupRestrictions.None, loc);
			if (expression != null)
			{
				Expression.Error_UnexpectedKind(rc, expression, "type", expression.ExprClassName, loc);
			}
			else
			{
				rc.Module.Compiler.Report.Error(426, loc, "The nested type `{0}' does not exist in the type `{1}'", base.Name, expr_type.GetSignatureForError());
			}
		}

		protected override void Error_InvalidExpressionStatement(Report report, Location loc)
		{
			base.Error_InvalidExpressionStatement(report, LeftExpression.Location);
		}

		public override void Error_TypeDoesNotContainDefinition(ResolveContext ec, TypeSpec type, string name)
		{
			if (ec.Module.Compiler.Settings.Version > LanguageVersion.ISO_2 && !ec.IsRuntimeBinder && MethodGroupExpr.IsExtensionMethodArgument(expr))
			{
				ec.Report.SymbolRelatedToPreviousError(type);
				List<string> list = ec.Module.GlobalRootNamespace.FindExtensionMethodNamespaces(ec, name, base.Arity);
				string text = ((list == null) ? "an assembly reference" : ("`" + string.Join("' or `", list.ToArray()) + "' using directive"));
				ec.Report.Error(1061, loc, "Type `{0}' does not contain a definition for `{1}' and no extension method `{1}' of type `{0}' could be found. Are you missing {2}?", type.GetSignatureForError(), name, text);
			}
			else
			{
				base.Error_TypeDoesNotContainDefinition(ec, type, name);
			}
		}

		public override string GetSignatureForError()
		{
			return expr.GetSignatureForError() + "." + base.GetSignatureForError();
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			((MemberAccess)t).expr = expr.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
