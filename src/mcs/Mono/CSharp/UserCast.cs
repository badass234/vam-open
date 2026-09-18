using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class UserCast : Expression
	{
		private MethodSpec method;

		private Expression source;

		public Expression Source
		{
			get
			{
				return source;
			}
			set
			{
				source = value;
			}
		}

		public UserCast(MethodSpec method, Expression source, Location l)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			this.method = method;
			this.source = source;
			type = method.ReturnType;
			loc = l;
		}

		public override bool ContainsEmitWithAwait()
		{
			return source.ContainsEmitWithAwait();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments arguments = new Arguments(3);
			arguments.Add(new Argument(source.CreateExpressionTree(ec)));
			arguments.Add(new Argument(new TypeOf(type, loc)));
			arguments.Add(new Argument(new TypeOfMethod(method, loc)));
			return CreateExpressionFactoryCall(ec, "Convert", arguments);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			ObsoleteAttribute attributeObsolete = method.GetAttributeObsolete();
			if (attributeObsolete != null)
			{
				AttributeTester.Report_ObsoleteMessage(attributeObsolete, GetSignatureForError(), loc, ec.Report);
			}
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			source.Emit(ec);
			ec.MarkCallEntry(loc);
			ec.Emit(OpCodes.Call, method);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			source.FlowAnalysis(fc);
		}

		public override string GetSignatureForError()
		{
			return TypeManager.CSharpSignature(method);
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return System.Linq.Expressions.Expression.Convert(source.MakeExpression(ctx), type.GetMetaInfo(), (MethodInfo)method.GetMetaInfo());
		}
	}
}
