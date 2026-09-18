using System;

namespace Mono.CSharp
{
	internal class ImportedTypeParameterDefinition : ImportedDefinition, ITypeDefinition, IMemberDefinition
	{
		public IAssemblyDefinition DeclaringAssembly
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		bool ITypeDefinition.IsComImport => false;

		bool ITypeDefinition.IsPartial => false;

		bool ITypeDefinition.IsTypeForwarder => false;

		bool ITypeDefinition.IsCyclicTypeForwarder => false;

		public string Namespace => null;

		public int TypeParametersCount => 0;

		public TypeParameterSpec[] TypeParameters => null;

		public ImportedTypeParameterDefinition(Type type, MetadataImporter importer)
			: base(type, importer)
		{
		}

		public TypeSpec GetAttributeCoClass()
		{
			return null;
		}

		public string GetAttributeDefaultMember()
		{
			throw new NotSupportedException();
		}

		public AttributeUsageAttribute GetAttributeUsage(PredefinedAttribute pa)
		{
			throw new NotSupportedException();
		}

		bool ITypeDefinition.IsInternalAsPublic(IAssemblyDefinition assembly)
		{
			throw new NotImplementedException();
		}

		public void LoadMembers(TypeSpec declaringType, bool onlyTypes, ref MemberCache cache)
		{
			throw new NotImplementedException();
		}
	}
}
