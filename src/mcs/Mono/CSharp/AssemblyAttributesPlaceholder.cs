namespace Mono.CSharp
{
	internal class AssemblyAttributesPlaceholder : CompilerGeneratedContainer
	{
		private static readonly string TypeNamePrefix = "<$AssemblyAttributes${0}>";

		public static readonly string AssemblyFieldName = "attributes";

		private Field assembly;

		public AssemblyAttributesPlaceholder(ModuleContainer parent, string outputName)
			: base(parent, new MemberName(GetGeneratedName(outputName)), Modifiers.INTERNAL | Modifiers.STATIC)
		{
			assembly = new Field(this, new TypeExpression(parent.Compiler.BuiltinTypes.Object, base.Location), Modifiers.PUBLIC | Modifiers.STATIC, new MemberName(AssemblyFieldName), null);
			AddField(assembly);
		}

		public void AddAssemblyAttribute(MethodSpec ctor, byte[] data)
		{
			assembly.SetCustomAttribute(ctor, data);
		}

		public static string GetGeneratedName(string outputName)
		{
			return string.Format(TypeNamePrefix, outputName);
		}
	}
}
