namespace IKVM.Reflection
{
	public struct InterfaceMapping
	{
		public MethodInfo[] InterfaceMethods;

		public Type InterfaceType;

		public MethodInfo[] TargetMethods;

		public Type TargetType;
	}
}
