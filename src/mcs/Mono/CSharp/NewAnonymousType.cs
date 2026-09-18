using System.Collections.Generic;

namespace Mono.CSharp
{
	public class NewAnonymousType : New
	{
		private static readonly AnonymousTypeParameter[] EmptyParameters = new AnonymousTypeParameter[0];

		private List<AnonymousTypeParameter> parameters;

		private readonly TypeContainer parent;

		private AnonymousTypeClass anonymous_type;

		public List<AnonymousTypeParameter> Parameters => parameters;

		public NewAnonymousType(List<AnonymousTypeParameter> parameters, TypeContainer parent, Location loc)
			: base(null, null, loc)
		{
			this.parameters = parameters;
			this.parent = parent;
		}

		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
			if (parameters == null)
			{
				return;
			}
			NewAnonymousType newAnonymousType = (NewAnonymousType)target;
			newAnonymousType.parameters = new List<AnonymousTypeParameter>(parameters.Count);
			foreach (AnonymousTypeParameter parameter in parameters)
			{
				newAnonymousType.parameters.Add((AnonymousTypeParameter)parameter.Clone(clonectx));
			}
		}

		private AnonymousTypeClass CreateAnonymousType(ResolveContext ec, IList<AnonymousTypeParameter> parameters)
		{
			AnonymousTypeClass anonymousType = parent.Module.GetAnonymousType(parameters);
			if (anonymousType != null)
			{
				return anonymousType;
			}
			anonymousType = AnonymousTypeClass.Create(parent, parameters, loc);
			if (anonymousType == null)
			{
				return null;
			}
			int errors = ec.Report.Errors;
			anonymousType.CreateContainer();
			anonymousType.DefineContainer();
			anonymousType.Define();
			if (ec.Report.Errors - errors == 0)
			{
				parent.Module.AddAnonymousType(anonymousType);
				anonymousType.PrepareEmit();
			}
			return anonymousType;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			if (parameters == null)
			{
				return base.CreateExpressionTree(ec);
			}
			ArrayInitializer arrayInitializer = new ArrayInitializer(parameters.Count, loc);
			foreach (MemberCore member in anonymous_type.Members)
			{
				Property property = member as Property;
				if (property != null)
				{
					arrayInitializer.Add(new TypeOfMethod(MemberCache.GetMember(type, property.Get.Spec), loc));
				}
			}
			ArrayInitializer arrayInitializer2 = new ArrayInitializer(base.arguments.Count, loc);
			foreach (Argument argument in base.arguments)
			{
				arrayInitializer2.Add(argument.CreateExpressionTree(ec));
			}
			Arguments arguments = new Arguments(3);
			arguments.Add(new Argument(new TypeOfMethod(method, loc)));
			arguments.Add(new Argument(new ArrayCreation(Expression.CreateExpressionTypeExpression(ec, loc), arrayInitializer2, loc)));
			arguments.Add(new Argument(new ImplicitlyTypedArrayCreation(arrayInitializer, loc)));
			return CreateExpressionFactoryCall(ec, "New", arguments);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			if (ec.HasSet(ResolveContext.Options.ConstantScope))
			{
				ec.Report.Error(836, loc, "Anonymous types cannot be used in this expression");
				return null;
			}
			if (parameters == null)
			{
				anonymous_type = CreateAnonymousType(ec, EmptyParameters);
				RequestedType = new TypeExpression(anonymous_type.Definition, loc);
				return base.DoResolve(ec);
			}
			bool flag = false;
			arguments = new Arguments(parameters.Count);
			TypeSpec[] array = new TypeSpec[parameters.Count];
			for (int i = 0; i < parameters.Count; i++)
			{
				Expression expression = parameters[i].Resolve(ec);
				if (expression == null)
				{
					flag = true;
					continue;
				}
				arguments.Add(new Argument(expression));
				array[i] = expression.Type;
			}
			if (flag)
			{
				return null;
			}
			anonymous_type = CreateAnonymousType(ec, parameters);
			if (anonymous_type == null)
			{
				return null;
			}
			type = anonymous_type.Definition.MakeGenericType(ec.Module, array);
			method = (MethodSpec)MemberCache.FindMember(type, MemberFilter.Constructor(null), BindingRestriction.DeclaredOnly);
			eclass = ExprClass.Value;
			return this;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
