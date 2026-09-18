using System.Linq.Expressions;

namespace Mono.CSharp
{
	internal interface IDynamicAssign : IAssignMethod
	{
		System.Linq.Expressions.Expression MakeAssignExpression(BuilderContext ctx, Expression source);
	}
}
