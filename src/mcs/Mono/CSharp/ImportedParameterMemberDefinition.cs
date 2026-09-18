using System.Reflection;

namespace Mono.CSharp
{
	internal class ImportedParameterMemberDefinition : ImportedMemberDefinition, IParametersMember, IInterfaceMemberSpec
	{
		private readonly AParametersCollection parameters;

		public AParametersCollection Parameters => parameters;

		protected ImportedParameterMemberDefinition(MethodBase provider, TypeSpec type, AParametersCollection parameters, MetadataImporter importer)
			: base(provider, type, importer)
		{
			this.parameters = parameters;
		}

		public ImportedParameterMemberDefinition(PropertyInfo provider, TypeSpec type, AParametersCollection parameters, MetadataImporter importer)
			: base(provider, type, importer)
		{
			this.parameters = parameters;
		}
	}
}
