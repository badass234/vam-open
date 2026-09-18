namespace Mono.CSharp
{
	public class ElementInitializer : Assign
	{
		public readonly string Name;

		public bool IsDictionaryInitializer => Name == null;

		public ElementInitializer(string name, Expression initializer, Location loc)
			: base(null, initializer, loc)
		{
			Name = name;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			((ElementInitializer)t).source = source.Clone(clonectx);
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments arguments = new Arguments(2);
			FieldExpr fieldExpr = target as FieldExpr;
			if (fieldExpr != null)
			{
				arguments.Add(new Argument(fieldExpr.CreateTypeOfExpression()));
			}
			else
			{
				arguments.Add(new Argument(((PropertyExpr)target).CreateSetterTypeOfExpression(ec)));
			}
			CollectionOrObjectInitializers collectionOrObjectInitializers = source as CollectionOrObjectInitializers;
			string name;
			Expression expr;
			if (collectionOrObjectInitializers == null)
			{
				name = "Bind";
				expr = source.CreateExpressionTree(ec);
			}
			else
			{
				name = ((collectionOrObjectInitializers.IsEmpty || collectionOrObjectInitializers.Initializers[0] is ElementInitializer) ? "MemberBind" : "ListBind");
				expr = collectionOrObjectInitializers.CreateExpressionTree(ec, !collectionOrObjectInitializers.IsEmpty);
			}
			arguments.Add(new Argument(expr));
			return CreateExpressionFactoryCall(ec, name, arguments);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			if (source == null)
			{
				return EmptyExpressionStatement.Instance;
			}
			if (!ResolveElement(ec))
			{
				return null;
			}
			if (source is CollectionOrObjectInitializers)
			{
				Expression currentInitializerVariable = ec.CurrentInitializerVariable;
				ec.CurrentInitializerVariable = target;
				source = source.Resolve(ec);
				ec.CurrentInitializerVariable = currentInitializerVariable;
				if (source == null)
				{
					return null;
				}
				eclass = source.eclass;
				type = source.Type;
				return this;
			}
			return base.DoResolve(ec);
		}

		public override void EmitStatement(EmitContext ec)
		{
			if (source is CollectionOrObjectInitializers)
			{
				source.Emit(ec);
			}
			else
			{
				base.EmitStatement(ec);
			}
		}

		protected virtual bool ResolveElement(ResolveContext rc)
		{
			TypeSpec typeSpec = rc.CurrentInitializerVariable.Type;
			if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				Arguments arguments = new Arguments(1);
				arguments.Add(new Argument(rc.CurrentInitializerVariable));
				target = new DynamicMemberBinder(Name, arguments, loc);
			}
			else
			{
				Expression expression = Expression.MemberLookup(rc, false, typeSpec, Name, 0, MemberLookupRestrictions.ExactArity, loc);
				if (expression == null)
				{
					expression = Expression.MemberLookup(rc, true, typeSpec, Name, 0, MemberLookupRestrictions.ExactArity, loc);
					if (expression != null)
					{
						Expression.ErrorIsInaccesible(rc, expression.GetSignatureForError(), loc);
						return false;
					}
				}
				if (expression == null)
				{
					Expression.Error_TypeDoesNotContainDefinition(rc, loc, typeSpec, Name);
					return false;
				}
				MemberExpr memberExpr = expression as MemberExpr;
				if (memberExpr is EventExpr)
				{
					memberExpr = memberExpr.ResolveMemberAccess(rc, null, null);
				}
				else if (!(expression is PropertyExpr) && !(expression is FieldExpr))
				{
					rc.Report.Error(1913, loc, "Member `{0}' cannot be initialized. An object initializer may only be used for fields, or properties", expression.GetSignatureForError());
					return false;
				}
				if (memberExpr.IsStatic)
				{
					rc.Report.Error(1914, loc, "Static field or property `{0}' cannot be assigned in an object initializer", memberExpr.GetSignatureForError());
				}
				target = memberExpr;
				memberExpr.InstanceExpression = rc.CurrentInitializerVariable;
			}
			return true;
		}
	}
}
