using System.Collections.Generic;

namespace Mono.CSharp
{
	internal class DictionaryElementInitializer : ElementInitializer
	{
		private readonly Arguments args;

		public DictionaryElementInitializer(Arguments arguments, Expression initializer, Location loc)
			: base(null, initializer, loc)
		{
			args = arguments;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			ec.Report.Error(8074, loc, "Expression tree cannot contain a dictionary initializer");
			return null;
		}

		protected override bool ResolveElement(ResolveContext rc)
		{
			Expression currentInitializerVariable = rc.CurrentInitializerVariable;
			TypeSpec typeSpec = currentInitializerVariable.Type;
			if (typeSpec.IsArray)
			{
				target = new ArrayAccess(new ElementAccess(currentInitializerVariable, args, loc), loc);
				return true;
			}
			if (typeSpec.IsPointer)
			{
				target = currentInitializerVariable.MakePointerAccess(rc, typeSpec, args);
				return true;
			}
			IList<MemberSpec> list = MemberCache.FindMembers(typeSpec, MemberCache.IndexerNameAlias, false);
			if (list == null && typeSpec.BuiltinType != BuiltinTypeSpec.Type.Dynamic)
			{
				ElementAccess.Error_CannotApplyIndexing(rc, typeSpec, loc);
				return false;
			}
			target = new IndexerExpr(list, typeSpec, currentInitializerVariable, args, loc).Resolve(rc);
			return true;
		}
	}
}
