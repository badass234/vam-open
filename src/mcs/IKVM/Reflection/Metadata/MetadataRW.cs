namespace IKVM.Reflection.Metadata
{
	internal abstract class MetadataRW
	{
		internal readonly bool bigStrings;

		internal readonly bool bigGuids;

		internal readonly bool bigBlobs;

		internal readonly bool bigResolutionScope;

		internal readonly bool bigTypeDefOrRef;

		internal readonly bool bigMemberRefParent;

		internal readonly bool bigHasCustomAttribute;

		internal readonly bool bigCustomAttributeType;

		internal readonly bool bigMethodDefOrRef;

		internal readonly bool bigHasConstant;

		internal readonly bool bigHasSemantics;

		internal readonly bool bigHasFieldMarshal;

		internal readonly bool bigHasDeclSecurity;

		internal readonly bool bigTypeOrMethodDef;

		internal readonly bool bigMemberForwarded;

		internal readonly bool bigImplementation;

		internal readonly bool bigField;

		internal readonly bool bigMethodDef;

		internal readonly bool bigParam;

		internal readonly bool bigTypeDef;

		internal readonly bool bigProperty;

		internal readonly bool bigEvent;

		internal readonly bool bigGenericParam;

		internal readonly bool bigModuleRef;

		protected MetadataRW(Module module, bool bigStrings, bool bigGuids, bool bigBlobs)
		{
			this.bigStrings = bigStrings;
			this.bigGuids = bigGuids;
			this.bigBlobs = bigBlobs;
			bigField = module.Field.IsBig;
			bigMethodDef = module.MethodDef.IsBig;
			bigParam = module.Param.IsBig;
			bigTypeDef = module.TypeDef.IsBig;
			bigProperty = module.Property.IsBig;
			bigEvent = module.Event.IsBig;
			bigGenericParam = module.GenericParam.IsBig;
			bigModuleRef = module.ModuleRef.IsBig;
			bigResolutionScope = IsBig(2, module.ModuleTable, module.ModuleRef, module.AssemblyRef, module.TypeRef);
			bigTypeDefOrRef = IsBig(2, module.TypeDef, module.TypeRef, module.TypeSpec);
			bigMemberRefParent = IsBig(3, module.TypeDef, module.TypeRef, module.ModuleRef, module.MethodDef, module.TypeSpec);
			bigMethodDefOrRef = IsBig(1, module.MethodDef, module.MemberRef);
			bigHasCustomAttribute = IsBig(5, module.MethodDef, module.Field, module.TypeRef, module.TypeDef, module.Param, module.InterfaceImpl, module.MemberRef, module.ModuleTable, module.Property, module.Event, module.StandAloneSig, module.ModuleRef, module.TypeSpec, module.AssemblyTable, module.AssemblyRef, module.File, module.ExportedType, module.ManifestResource);
			bigCustomAttributeType = IsBig(3, module.MethodDef, module.MemberRef);
			bigHasConstant = IsBig(2, module.Field, module.Param, module.Property);
			bigHasSemantics = IsBig(1, module.Event, module.Property);
			bigHasFieldMarshal = IsBig(1, module.Field, module.Param);
			bigHasDeclSecurity = IsBig(2, module.TypeDef, module.MethodDef, module.AssemblyTable);
			bigTypeOrMethodDef = IsBig(1, module.TypeDef, module.MethodDef);
			bigMemberForwarded = IsBig(1, module.Field, module.MethodDef);
			bigImplementation = IsBig(2, module.File, module.AssemblyRef, module.ExportedType);
		}

		private static bool IsBig(int bitsUsed, params Table[] tables)
		{
			int num = 1 << 16 - bitsUsed;
			for (int i = 0; i < tables.Length; i++)
			{
				if (tables[i].RowCount >= num)
				{
					return true;
				}
			}
			return false;
		}
	}
}
