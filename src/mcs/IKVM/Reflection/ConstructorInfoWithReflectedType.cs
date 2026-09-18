namespace IKVM.Reflection
{
	internal sealed class ConstructorInfoWithReflectedType : ConstructorInfo
	{
		private readonly Type reflectedType;

		private readonly ConstructorInfo ctor;

		public override Type ReflectedType => reflectedType;

		internal ConstructorInfoWithReflectedType(Type reflectedType, ConstructorInfo ctor)
		{
			this.reflectedType = reflectedType;
			this.ctor = ctor;
		}

		public override bool Equals(object obj)
		{
			ConstructorInfoWithReflectedType constructorInfoWithReflectedType = obj as ConstructorInfoWithReflectedType;
			if (constructorInfoWithReflectedType != null && constructorInfoWithReflectedType.reflectedType == reflectedType)
			{
				return constructorInfoWithReflectedType.ctor == ctor;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return reflectedType.GetHashCode() ^ ctor.GetHashCode();
		}

		internal override MethodInfo GetMethodInfo()
		{
			return ctor.GetMethodInfo();
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return ctor.GetMethodOnTypeDefinition();
		}
	}
}
