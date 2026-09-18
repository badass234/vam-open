using IKVM.Reflection.Emit;

namespace IKVM.Reflection.Impl
{
	internal interface ITypeOwner
	{
		ModuleBuilder ModuleBuilder { get; }
	}
}
