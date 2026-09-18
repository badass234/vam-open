namespace Mono.CSharp
{
	internal class ImplicitlyTypedArrayCreation : ArrayCreation
	{
		private TypeInferenceContext best_type_inference;

		public ImplicitlyTypedArrayCreation(ComposedTypeSpecifier rank, ArrayInitializer initializers, Location loc)
			: base(null, rank, initializers, loc)
		{
		}

		public ImplicitlyTypedArrayCreation(ArrayInitializer initializers, Location loc)
			: base(null, initializers, loc)
		{
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			if (type != null)
			{
				return this;
			}
			dimensions = rank.Dimension;
			best_type_inference = new TypeInferenceContext();
			if (!ResolveInitializers(ec))
			{
				return null;
			}
			best_type_inference.FixAllTypes(ec);
			array_element_type = best_type_inference.InferredTypeArguments[0];
			best_type_inference = null;
			if (array_element_type == null || array_element_type == InternalType.NullLiteral || array_element_type == InternalType.MethodGroup || array_element_type == InternalType.AnonymousMethod || arguments.Count != rank.Dimension)
			{
				ec.Report.Error(826, loc, "The type of an implicitly typed array cannot be inferred from the initializer. Try specifying array type explicitly");
				return null;
			}
			UnifyInitializerElement(ec);
			type = ArrayContainer.MakeType(ec.Module, array_element_type, dimensions);
			eclass = ExprClass.Value;
			return this;
		}

		private void UnifyInitializerElement(ResolveContext ec)
		{
			for (int i = 0; i < array_data.Count; i++)
			{
				Expression expression = array_data[i];
				if (expression != null)
				{
					array_data[i] = Convert.ImplicitConversion(ec, expression, array_element_type, Location.Null);
				}
			}
		}

		protected override Expression ResolveArrayElement(ResolveContext ec, Expression element)
		{
			element = element.Resolve(ec);
			if (element != null)
			{
				best_type_inference.AddCommonTypeBound(element.Type);
			}
			return element;
		}
	}
}
