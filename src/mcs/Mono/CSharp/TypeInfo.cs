using System.Collections.Generic;

namespace Mono.CSharp
{
	public class TypeInfo
	{
		private class StructInfo
		{
			private readonly List<FieldSpec> fields;

			public readonly TypeInfo[] StructFields;

			public readonly int Length;

			public readonly int TotalLength;

			public static Dictionary<TypeSpec, StructInfo> field_type_hash;

			private Dictionary<string, TypeInfo> struct_field_hash;

			private Dictionary<string, int> field_hash;

			private bool InTransit;

			public int Count => fields.Count;

			public List<FieldSpec> Fields => fields;

			public int this[string name]
			{
				get
				{
					int value;
					if (!field_hash.TryGetValue(name, out value))
					{
						return 0;
					}
					return value;
				}
			}

			private StructInfo(TypeSpec type, IMemberContext context)
			{
				field_type_hash.Add(type, this);
				fields = MemberCache.GetAllFieldsForDefiniteAssignment(type, context);
				struct_field_hash = new Dictionary<string, TypeInfo>();
				field_hash = new Dictionary<string, int>(fields.Count);
				StructFields = new TypeInfo[fields.Count];
				StructInfo[] array = new StructInfo[fields.Count];
				InTransit = true;
				for (int i = 0; i < fields.Count; i++)
				{
					FieldSpec fieldSpec = fields[i];
					if (fieldSpec.MemberType.IsStruct)
					{
						array[i] = GetStructInfo(fieldSpec.MemberType, context);
					}
					if (array[i] == null)
					{
						field_hash.Add(fieldSpec.Name, ++Length);
					}
					else if (array[i].InTransit)
					{
						array[i] = null;
						return;
					}
				}
				InTransit = false;
				TotalLength = Length + 1;
				for (int j = 0; j < fields.Count; j++)
				{
					FieldSpec fieldSpec2 = fields[j];
					if (array[j] != null)
					{
						field_hash.Add(fieldSpec2.Name, TotalLength);
						StructFields[j] = new TypeInfo(array[j], TotalLength);
						struct_field_hash.Add(fieldSpec2.Name, StructFields[j]);
						TotalLength += array[j].TotalLength;
					}
				}
			}

			public TypeInfo GetStructField(string name)
			{
				TypeInfo value;
				if (struct_field_hash.TryGetValue(name, out value))
				{
					return value;
				}
				return null;
			}

			public static StructInfo GetStructInfo(TypeSpec type, IMemberContext context)
			{
				if (type.BuiltinType > BuiltinTypeSpec.Type.None && type != context.CurrentType)
				{
					return null;
				}
				StructInfo value;
				if (field_type_hash.TryGetValue(type, out value))
				{
					return value;
				}
				return new StructInfo(type, context);
			}
		}

		public readonly int TotalLength;

		public readonly int Length;

		public readonly int Offset;

		public readonly bool IsStruct;

		public TypeInfo[] SubStructInfo;

		private readonly StructInfo struct_info;

		private static readonly TypeInfo simple_type;

		static TypeInfo()
		{
			simple_type = new TypeInfo(1);
			Reset();
		}

		public static void Reset()
		{
			StructInfo.field_type_hash = new Dictionary<TypeSpec, StructInfo>();
		}

		private TypeInfo(int totalLength)
		{
			TotalLength = totalLength;
		}

		private TypeInfo(StructInfo struct_info, int offset)
		{
			this.struct_info = struct_info;
			Offset = offset;
			Length = struct_info.Length;
			TotalLength = struct_info.TotalLength;
			SubStructInfo = struct_info.StructFields;
			IsStruct = true;
		}

		public int GetFieldIndex(string name)
		{
			if (struct_info == null)
			{
				return 0;
			}
			return struct_info[name];
		}

		public TypeInfo GetStructField(string name)
		{
			if (struct_info == null)
			{
				return null;
			}
			return struct_info.GetStructField(name);
		}

		public static TypeInfo GetTypeInfo(TypeSpec type, IMemberContext context)
		{
			if (!type.IsStruct)
			{
				return simple_type;
			}
			Dictionary<TypeSpec, TypeInfo> dictionary;
			TypeInfo value;
			if (type.BuiltinType > BuiltinTypeSpec.Type.None)
			{
				dictionary = null;
			}
			else
			{
				dictionary = context.Module.TypeInfoCache;
				if (dictionary.TryGetValue(type, out value))
				{
					return value;
				}
			}
			StructInfo structInfo = StructInfo.GetStructInfo(type, context);
			value = ((structInfo == null) ? simple_type : new TypeInfo(structInfo, 0));
			dictionary?.Add(type, value);
			return value;
		}

		public bool IsFullyInitialized(FlowAnalysisContext fc, VariableInfo vi, Location loc)
		{
			if (struct_info == null)
			{
				return true;
			}
			bool result = true;
			for (int i = 0; i < struct_info.Count; i++)
			{
				FieldSpec fieldSpec = struct_info.Fields[i];
				if (fc.IsStructFieldDefinitelyAssigned(vi, fieldSpec.Name))
				{
					continue;
				}
				Property.BackingFieldDeclaration backingFieldDeclaration = fieldSpec.MemberDefinition as Property.BackingFieldDeclaration;
				if (backingFieldDeclaration != null)
				{
					if (backingFieldDeclaration.Initializer == null)
					{
						fc.Report.Error(843, loc, "An automatically implemented property `{0}' must be fully assigned before control leaves the constructor. Consider calling the default struct contructor from a constructor initializer", fieldSpec.GetSignatureForError());
						result = false;
					}
				}
				else
				{
					fc.Report.Error(171, loc, "Field `{0}' must be fully assigned before control leaves the constructor", fieldSpec.GetSignatureForError());
					result = false;
				}
			}
			return result;
		}

		public override string ToString()
		{
			return $"TypeInfo ({Offset}:{Length}:{TotalLength})";
		}
	}
}
