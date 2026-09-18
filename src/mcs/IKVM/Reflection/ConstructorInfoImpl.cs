namespace IKVM.Reflection
{
	internal sealed class ConstructorInfoImpl : ConstructorInfo
	{
		private readonly MethodInfo method;

		internal ConstructorInfoImpl(MethodInfo method)
		{
			this.method = method;
		}

		public override bool Equals(object obj)
		{
			ConstructorInfoImpl constructorInfoImpl = obj as ConstructorInfoImpl;
			if (constructorInfoImpl != null)
			{
				return constructorInfoImpl.method.Equals(method);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return method.GetHashCode();
		}

		internal override MethodInfo GetMethodInfo()
		{
			return method;
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return method.GetMethodOnTypeDefinition();
		}
	}
}
