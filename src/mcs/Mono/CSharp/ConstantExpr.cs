using System;

namespace Mono.CSharp
{
	public class ConstantExpr : MemberExpr
	{
		private readonly ConstSpec constant;

		public override string Name
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override string KindName => "constant";

		public override bool IsInstance => !IsStatic;

		public override bool IsStatic => true;

		protected override TypeSpec DeclaringType => constant.DeclaringType;

		public ConstantExpr(ConstSpec constant, Location loc)
		{
			this.constant = constant;
			base.loc = loc;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotSupportedException("ET");
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			ResolveInstanceExpression(rc, null);
			DoBestMemberChecks(rc, this.constant);
			Constant constant = this.constant.GetConstant(rc);
			return Constant.CreateConstantFromValue(this.constant.MemberType, constant.GetValue(), loc);
		}

		public override void Emit(EmitContext ec)
		{
			throw new NotSupportedException();
		}

		public override string GetSignatureForError()
		{
			return constant.GetSignatureForError();
		}

		public override void SetTypeArguments(ResolveContext ec, TypeArguments ta)
		{
			Expression.Error_TypeArgumentsCannotBeUsed(ec, "constant", GetSignatureForError(), loc);
		}
	}
}
