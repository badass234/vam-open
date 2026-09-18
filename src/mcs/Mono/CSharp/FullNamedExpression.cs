using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public abstract class FullNamedExpression : Expression
	{
		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotSupportedException("ET");
		}

		public abstract FullNamedExpression ResolveAsTypeOrNamespace(IMemberContext mc, bool allowUnboundTypeArguments);

		public override TypeSpec ResolveAsType(IMemberContext mc, bool allowUnboundTypeArguments = false)
		{
			FullNamedExpression fullNamedExpression = ResolveAsTypeOrNamespace(mc, allowUnboundTypeArguments);
			if (fullNamedExpression == null)
			{
				return null;
			}
			TypeExpr typeExpr = fullNamedExpression as TypeExpr;
			if (typeExpr == null)
			{
				Expression.Error_UnexpectedKind(mc, fullNamedExpression, "type", fullNamedExpression.ExprClassName, loc);
				return null;
			}
			typeExpr.loc = loc;
			type = typeExpr.Type;
			List<MissingTypeSpecReference> missingDependencies = type.GetMissingDependencies();
			if (missingDependencies != null)
			{
				ImportedTypeDefinition.Error_MissingDependency(mc, missingDependencies, loc);
			}
			if (type.Kind == MemberKind.Void)
			{
				mc.Module.Compiler.Report.Error(673, loc, "System.Void cannot be used from C#. Consider using `void'");
			}
			if (!(mc is TypeDefinition.BaseContext) && !(mc is UsingAliasNamespace.AliasContext))
			{
				ObsoleteAttribute attributeObsolete = type.GetAttributeObsolete();
				if (attributeObsolete != null && !mc.IsObsolete)
				{
					AttributeTester.Report_ObsoleteMessage(attributeObsolete, typeExpr.GetSignatureForError(), base.Location, mc.Module.Compiler.Report);
				}
			}
			return type;
		}

		public override void Emit(EmitContext ec)
		{
			throw new InternalErrorException("FullNamedExpression `{0}' found in resolved tree", GetSignatureForError());
		}
	}
}
