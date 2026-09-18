using System;
using IKVM.Reflection;

namespace Mono.CSharp
{
	public class StaticImporter
	{
		public StaticImporter(BuiltinTypes builtin)
		{
			throw new NotSupportedException();
		}

		public void ImportAssembly(Assembly assembly, RootNamespace targetNamespace)
		{
			throw new NotSupportedException();
		}

		public void ImportModule(Module module, RootNamespace targetNamespace)
		{
			throw new NotSupportedException();
		}

		public TypeSpec ImportType(System.Type type)
		{
			throw new NotSupportedException();
		}
	}
}
