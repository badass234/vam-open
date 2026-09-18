namespace Mono.CSharp
{
	public class VariableInfo
	{
		private readonly string Name;

		private readonly TypeInfo TypeInfo;

		private readonly int Offset;

		private readonly int Length;

		public bool IsParameter;

		private VariableInfo[] sub_info;

		public bool IsEverAssigned { get; set; }

		private VariableInfo(string name, TypeSpec type, int offset, IMemberContext context)
		{
			Name = name;
			Offset = offset;
			TypeInfo = TypeInfo.GetTypeInfo(type, context);
			Length = TypeInfo.TotalLength;
			Initialize();
		}

		private VariableInfo(VariableInfo parent, TypeInfo type)
		{
			Name = parent.Name;
			TypeInfo = type;
			Offset = parent.Offset + type.Offset;
			Length = type.TotalLength;
			IsParameter = parent.IsParameter;
			Initialize();
		}

		private void Initialize()
		{
			TypeInfo[] subStructInfo = TypeInfo.SubStructInfo;
			if (subStructInfo != null)
			{
				sub_info = new VariableInfo[subStructInfo.Length];
				for (int i = 0; i < subStructInfo.Length; i++)
				{
					if (subStructInfo[i] != null)
					{
						sub_info[i] = new VariableInfo(this, subStructInfo[i]);
					}
				}
			}
			else
			{
				sub_info = new VariableInfo[0];
			}
		}

		public static VariableInfo Create(BlockContext bc, LocalVariable variable)
		{
			VariableInfo variableInfo = new VariableInfo(variable.Name, variable.Type, bc.AssignmentInfoOffset, bc);
			bc.AssignmentInfoOffset += variableInfo.Length;
			return variableInfo;
		}

		public static VariableInfo Create(BlockContext bc, Parameter parameter)
		{
			VariableInfo variableInfo = new VariableInfo(parameter.Name, parameter.Type, bc.AssignmentInfoOffset, bc)
			{
				IsParameter = true
			};
			bc.AssignmentInfoOffset += variableInfo.Length;
			return variableInfo;
		}

		public bool IsAssigned(DefiniteAssignmentBitSet vector)
		{
			if (vector == null)
			{
				return true;
			}
			if (vector[Offset])
			{
				return true;
			}
			if (!TypeInfo.IsStruct)
			{
				return false;
			}
			for (int i = Offset + 1; i <= TypeInfo.Length + Offset; i++)
			{
				if (!vector[i])
				{
					return false;
				}
			}
			for (int j = 0; j < sub_info.Length; j++)
			{
				VariableInfo variableInfo = sub_info[j];
				if (variableInfo != null && !variableInfo.IsAssigned(vector))
				{
					return false;
				}
			}
			vector.Set(Offset);
			return true;
		}

		public bool IsFullyInitialized(FlowAnalysisContext fc, Location loc)
		{
			return TypeInfo.IsFullyInitialized(fc, this, loc);
		}

		public bool IsStructFieldAssigned(DefiniteAssignmentBitSet vector, string field_name)
		{
			int fieldIndex = TypeInfo.GetFieldIndex(field_name);
			if (fieldIndex == 0)
			{
				return true;
			}
			return vector[Offset + fieldIndex];
		}

		public void SetAssigned(DefiniteAssignmentBitSet vector, bool generatedAssignment)
		{
			if (Length == 1)
			{
				vector.Set(Offset);
			}
			else
			{
				vector.Set(Offset, Length);
			}
			if (!generatedAssignment)
			{
				IsEverAssigned = true;
			}
		}

		public void SetStructFieldAssigned(DefiniteAssignmentBitSet vector, string field_name)
		{
			if (vector[Offset])
			{
				return;
			}
			int fieldIndex = TypeInfo.GetFieldIndex(field_name);
			if (fieldIndex == 0)
			{
				return;
			}
			TypeInfo structField = TypeInfo.GetStructField(field_name);
			if (structField != null)
			{
				vector.Set(Offset + structField.Offset, structField.TotalLength);
			}
			else
			{
				vector.Set(Offset + fieldIndex);
			}
			IsEverAssigned = true;
			for (int i = Offset + 1; i < TypeInfo.TotalLength + Offset; i++)
			{
				if (!vector[i])
				{
					return;
				}
			}
			vector.Set(Offset);
		}

		public VariableInfo GetStructFieldInfo(string fieldName)
		{
			TypeInfo structField = TypeInfo.GetStructField(fieldName);
			if (structField == null)
			{
				return null;
			}
			return new VariableInfo(this, structField);
		}

		public override string ToString()
		{
			return $"Name={Name} Offset={Offset} Length={Length} {TypeInfo})";
		}
	}
}
