using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Battlehub.Utils
{
	public class Strong
	{
		public static PropertyInfo PropertyInfo<T, U>(Expression<Func<T, U>> expression)
		{
			return (PropertyInfo)MemberInfo(expression);
		}

		public static MemberInfo MemberInfo<T, U>(Expression<Func<T, U>> expression)
		{
			MemberExpression memberExpression = expression.Body as MemberExpression;
			if (memberExpression != null)
			{
				return memberExpression.Member;
			}
			throw new ArgumentException("Expression is not a member access", "expression");
		}

		public static MethodInfo MethodInfo<T>(Expression<Func<T, Delegate>> expression)
		{
			UnaryExpression unaryExpression = (UnaryExpression)expression.Body;
			MethodCallExpression methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
			ConstantExpression constantExpression = (ConstantExpression)methodCallExpression.Arguments.Last();
			return (MethodInfo)constantExpression.Value;
		}
	}
}
