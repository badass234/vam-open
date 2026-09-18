using System;
using System.Collections.Generic;
using System.Text;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class CustomAttributeBuilder
	{
		private sealed class BlobWriter
		{
			private readonly Assembly assembly;

			private readonly CustomAttributeBuilder cab;

			private readonly ByteBuffer bb;

			internal BlobWriter(Assembly assembly, CustomAttributeBuilder cab, ByteBuffer bb)
			{
				this.assembly = assembly;
				this.cab = cab;
				this.bb = bb;
			}

			internal void WriteCustomAttributeBlob()
			{
				WriteUInt16(1);
				ParameterInfo[] parameters = cab.con.GetParameters();
				for (int i = 0; i < parameters.Length; i++)
				{
					WriteFixedArg(parameters[i].ParameterType, cab.constructorArgs[i]);
				}
				WriteNamedArguments(false);
			}

			internal void WriteNamedArguments(bool forDeclSecurity)
			{
				int num = 0;
				if (cab.namedFields != null)
				{
					num += cab.namedFields.Length;
				}
				if (cab.namedProperties != null)
				{
					num += cab.namedProperties.Length;
				}
				if (forDeclSecurity)
				{
					WritePackedLen(num);
				}
				else
				{
					WriteUInt16((ushort)num);
				}
				if (cab.namedFields != null)
				{
					for (int i = 0; i < cab.namedFields.Length; i++)
					{
						WriteNamedArg(83, cab.namedFields[i].FieldType, cab.namedFields[i].Name, cab.fieldValues[i]);
					}
				}
				if (cab.namedProperties != null)
				{
					for (int j = 0; j < cab.namedProperties.Length; j++)
					{
						WriteNamedArg(84, cab.namedProperties[j].PropertyType, cab.namedProperties[j].Name, cab.propertyValues[j]);
					}
				}
			}

			private void WriteNamedArg(byte fieldOrProperty, Type type, string name, object value)
			{
				WriteByte(fieldOrProperty);
				WriteFieldOrPropType(type);
				WriteString(name);
				WriteFixedArg(type, value);
			}

			private void WriteByte(byte value)
			{
				bb.Write(value);
			}

			private void WriteUInt16(ushort value)
			{
				bb.Write(value);
			}

			private void WriteInt32(int value)
			{
				bb.Write(value);
			}

			private void WriteFixedArg(Type type, object value)
			{
				Universe universe = assembly.universe;
				if (type == universe.System_String)
				{
					WriteString((string)value);
				}
				else if (type == universe.System_Boolean)
				{
					WriteByte((byte)(((bool)value) ? 1 : 0));
				}
				else if (type == universe.System_Char)
				{
					WriteUInt16((char)value);
				}
				else if (type == universe.System_SByte)
				{
					WriteByte((byte)(sbyte)value);
				}
				else if (type == universe.System_Byte)
				{
					WriteByte((byte)value);
				}
				else if (type == universe.System_Int16)
				{
					WriteUInt16((ushort)(short)value);
				}
				else if (type == universe.System_UInt16)
				{
					WriteUInt16((ushort)value);
				}
				else if (type == universe.System_Int32)
				{
					WriteInt32((int)value);
				}
				else if (type == universe.System_UInt32)
				{
					WriteInt32((int)(uint)value);
				}
				else if (type == universe.System_Int64)
				{
					WriteInt64((long)value);
				}
				else if (type == universe.System_UInt64)
				{
					WriteInt64((long)(ulong)value);
				}
				else if (type == universe.System_Single)
				{
					WriteSingle((float)value);
				}
				else if (type == universe.System_Double)
				{
					WriteDouble((double)value);
				}
				else if (type == universe.System_Type)
				{
					WriteTypeName((Type)value);
				}
				else if (type == universe.System_Object)
				{
					if (value == null)
					{
						type = universe.System_String;
					}
					else if (value is Type)
					{
						type = universe.System_Type;
					}
					else if (value is CustomAttributeTypedArgument)
					{
						CustomAttributeTypedArgument customAttributeTypedArgument = (CustomAttributeTypedArgument)value;
						value = customAttributeTypedArgument.Value;
						type = customAttributeTypedArgument.ArgumentType;
					}
					else
					{
						type = universe.Import(value.GetType());
					}
					WriteFieldOrPropType(type);
					WriteFixedArg(type, value);
				}
				else if (type.IsArray)
				{
					if (value != null)
					{
						Array array = (Array)value;
						Type elementType = type.GetElementType();
						WriteInt32(array.Length);
						{
							foreach (object item in array)
							{
								WriteFixedArg(elementType, item);
							}
							return;
						}
					}
					WriteInt32(-1);
				}
				else
				{
					if (!type.IsEnum)
					{
						throw new ArgumentException();
					}
					WriteFixedArg(type.GetEnumUnderlyingTypeImpl(), value);
				}
			}

			private void WriteInt64(long value)
			{
				bb.Write(value);
			}

			private void WriteSingle(float value)
			{
				bb.Write(value);
			}

			private void WriteDouble(double value)
			{
				bb.Write(value);
			}

			private void WriteTypeName(Type type)
			{
				string val = null;
				if (type != null)
				{
					StringBuilder stringBuilder = new StringBuilder();
					GetTypeName(stringBuilder, type, false);
					val = stringBuilder.ToString();
				}
				WriteString(val);
			}

			private void GetTypeName(StringBuilder sb, Type type, bool isTypeParam)
			{
				bool flag = !assembly.ManifestModule.__IsMissing && assembly.ManifestModule.MDStreamVersion < 131072;
				bool flag2 = type.Assembly != assembly && (!flag || type.Assembly != type.Module.universe.Mscorlib);
				if (isTypeParam & flag2)
				{
					sb.Append('[');
				}
				GetTypeNameImpl(sb, type);
				if (flag2)
				{
					if (flag)
					{
						sb.Append(',');
					}
					else
					{
						sb.Append(", ");
					}
					if (isTypeParam)
					{
						sb.Append(type.Assembly.FullName.Replace("]", "\\]")).Append(']');
					}
					else
					{
						sb.Append(type.Assembly.FullName);
					}
				}
			}

			private void GetTypeNameImpl(StringBuilder sb, Type type)
			{
				if (type.HasElementType)
				{
					GetTypeNameImpl(sb, type.GetElementType());
					sb.Append(((ElementHolderType)type).GetSuffix());
				}
				else if (type.IsConstructedGenericType)
				{
					sb.Append(type.GetGenericTypeDefinition().FullName);
					sb.Append('[');
					string value = "";
					Type[] genericArguments = type.GetGenericArguments();
					foreach (Type type2 in genericArguments)
					{
						sb.Append(value);
						GetTypeName(sb, type2, true);
						value = ",";
					}
					sb.Append(']');
				}
				else
				{
					sb.Append(type.FullName);
				}
			}

			private void WriteString(string val)
			{
				bb.Write(val);
			}

			private void WritePackedLen(int len)
			{
				bb.WriteCompressedUInt(len);
			}

			private void WriteFieldOrPropType(Type type)
			{
				Universe universe = type.Module.universe;
				if (type == universe.System_Type)
				{
					WriteByte(80);
					return;
				}
				if (type == universe.System_Object)
				{
					WriteByte(81);
					return;
				}
				if (type == universe.System_Boolean)
				{
					WriteByte(2);
					return;
				}
				if (type == universe.System_Char)
				{
					WriteByte(3);
					return;
				}
				if (type == universe.System_SByte)
				{
					WriteByte(4);
					return;
				}
				if (type == universe.System_Byte)
				{
					WriteByte(5);
					return;
				}
				if (type == universe.System_Int16)
				{
					WriteByte(6);
					return;
				}
				if (type == universe.System_UInt16)
				{
					WriteByte(7);
					return;
				}
				if (type == universe.System_Int32)
				{
					WriteByte(8);
					return;
				}
				if (type == universe.System_UInt32)
				{
					WriteByte(9);
					return;
				}
				if (type == universe.System_Int64)
				{
					WriteByte(10);
					return;
				}
				if (type == universe.System_UInt64)
				{
					WriteByte(11);
					return;
				}
				if (type == universe.System_Single)
				{
					WriteByte(12);
					return;
				}
				if (type == universe.System_Double)
				{
					WriteByte(13);
					return;
				}
				if (type == universe.System_String)
				{
					WriteByte(14);
					return;
				}
				if (type.IsArray)
				{
					WriteByte(29);
					WriteFieldOrPropType(type.GetElementType());
					return;
				}
				if (type.IsEnum)
				{
					WriteByte(85);
					WriteTypeName(type);
					return;
				}
				throw new ArgumentException();
			}
		}

		internal static readonly ConstructorInfo LegacyPermissionSet = new ConstructorBuilder(null);

		private readonly ConstructorInfo con;

		private readonly byte[] blob;

		private readonly object[] constructorArgs;

		private readonly PropertyInfo[] namedProperties;

		private readonly object[] propertyValues;

		private readonly FieldInfo[] namedFields;

		private readonly object[] fieldValues;

		internal ConstructorInfo Constructor => con;

		internal int ConstructorArgumentCount
		{
			get
			{
				if (constructorArgs != null)
				{
					return constructorArgs.Length;
				}
				return 0;
			}
		}

		internal bool IsLegacyDeclSecurity
		{
			get
			{
				if ((object)con != LegacyPermissionSet)
				{
					if (con.DeclaringType == con.Module.universe.System_Security_Permissions_PermissionSetAttribute && blob == null && (namedFields == null || namedFields.Length == 0) && namedProperties != null && namedProperties.Length == 1 && namedProperties[0].Name == "XML")
					{
						return propertyValues[0] is string;
					}
					return false;
				}
				return true;
			}
		}

		internal bool HasBlob => blob != null;

		internal KnownCA KnownCA
		{
			get
			{
				TypeName typeName = con.DeclaringType.TypeName;
				switch (typeName.Namespace)
				{
				case "System":
				{
					string name = typeName.Name;
					if (!(name == "SerializableAttribute"))
					{
						if (!(name == "NonSerializedAttribute"))
						{
							break;
						}
						return KnownCA.NonSerializedAttribute;
					}
					return KnownCA.SerializableAttribute;
				}
				case "System.Runtime.CompilerServices":
				{
					string name = typeName.Name;
					if (!(name == "MethodImplAttribute"))
					{
						if (!(name == "SpecialNameAttribute"))
						{
							break;
						}
						return KnownCA.SpecialNameAttribute;
					}
					return KnownCA.MethodImplAttribute;
				}
				case "System.Runtime.InteropServices":
					switch (typeName.Name)
					{
					case "DllImportAttribute":
						return KnownCA.DllImportAttribute;
					case "ComImportAttribute":
						return KnownCA.ComImportAttribute;
					case "MarshalAsAttribute":
						return KnownCA.MarshalAsAttribute;
					case "PreserveSigAttribute":
						return KnownCA.PreserveSigAttribute;
					case "InAttribute":
						return KnownCA.InAttribute;
					case "OutAttribute":
						return KnownCA.OutAttribute;
					case "OptionalAttribute":
						return KnownCA.OptionalAttribute;
					case "StructLayoutAttribute":
						return KnownCA.StructLayoutAttribute;
					case "FieldOffsetAttribute":
						return KnownCA.FieldOffsetAttribute;
					}
					break;
				}
				if (typeName.Matches("System.Security.SuppressUnmanagedCodeSecurityAttribute"))
				{
					return KnownCA.SuppressUnmanagedCodeSecurityAttribute;
				}
				return KnownCA.Unknown;
			}
		}

		internal CustomAttributeBuilder(ConstructorInfo con, byte[] blob)
		{
			this.con = con;
			this.blob = blob;
		}

		private CustomAttributeBuilder(ConstructorInfo con, int securityAction, byte[] blob)
		{
			this.con = con;
			this.blob = blob;
			constructorArgs = new object[1] { securityAction };
		}

		public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs)
			: this(con, constructorArgs, null, null, null, null)
		{
		}

		public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, FieldInfo[] namedFields, object[] fieldValues)
			: this(con, constructorArgs, null, null, namedFields, fieldValues)
		{
		}

		public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues)
			: this(con, constructorArgs, namedProperties, propertyValues, null, null)
		{
		}

		public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues)
		{
			this.con = con;
			this.constructorArgs = constructorArgs;
			this.namedProperties = namedProperties;
			this.propertyValues = propertyValues;
			this.namedFields = namedFields;
			this.fieldValues = fieldValues;
		}

		public static CustomAttributeBuilder __FromBlob(ConstructorInfo con, byte[] blob)
		{
			return new CustomAttributeBuilder(con, blob);
		}

		public static CustomAttributeBuilder __FromBlob(ConstructorInfo con, int securityAction, byte[] blob)
		{
			return new CustomAttributeBuilder(con, securityAction, blob);
		}

		public static CustomAttributeTypedArgument __MakeTypedArgument(Type type, object value)
		{
			return new CustomAttributeTypedArgument(type, value);
		}

		internal int WriteBlob(ModuleBuilder moduleBuilder)
		{
			ByteBuffer bb;
			if (blob != null)
			{
				bb = ByteBuffer.Wrap(blob);
			}
			else
			{
				bb = new ByteBuffer(100);
				new BlobWriter(moduleBuilder.Assembly, this, bb).WriteCustomAttributeBlob();
			}
			return moduleBuilder.Blobs.Add(bb);
		}

		internal object GetConstructorArgument(int pos)
		{
			return constructorArgs[pos];
		}

		internal T? GetFieldValue<T>(string name) where T : struct
		{
			object fieldValue = GetFieldValue(name);
			if (fieldValue is T)
			{
				return (T)fieldValue;
			}
			if (fieldValue != null)
			{
				if (typeof(T).IsEnum)
				{
					return (T)Enum.ToObject(typeof(T), fieldValue);
				}
				return (T)Convert.ChangeType(fieldValue, typeof(T));
			}
			return null;
		}

		internal object GetFieldValue(string name)
		{
			if (namedFields != null)
			{
				for (int i = 0; i < namedFields.Length; i++)
				{
					if (namedFields[i].Name == name)
					{
						return fieldValues[i];
					}
				}
			}
			return null;
		}

		internal int WriteLegacyDeclSecurityBlob(ModuleBuilder moduleBuilder)
		{
			if (blob != null)
			{
				return moduleBuilder.Blobs.Add(ByteBuffer.Wrap(blob));
			}
			return moduleBuilder.Blobs.Add(ByteBuffer.Wrap(Encoding.Unicode.GetBytes((string)propertyValues[0])));
		}

		internal void WriteNamedArgumentsForDeclSecurity(ModuleBuilder moduleBuilder, ByteBuffer bb)
		{
			if (blob != null)
			{
				bb.Write(blob);
			}
			else
			{
				new BlobWriter(moduleBuilder.Assembly, this, bb).WriteNamedArguments(true);
			}
		}

		internal CustomAttributeData ToData(Assembly asm)
		{
			if (blob != null)
			{
				if (constructorArgs != null)
				{
					return new CustomAttributeData(asm, con, (int)constructorArgs[0], blob, -1);
				}
				return new CustomAttributeData(asm, con, new ByteReader(blob, 0, blob.Length));
			}
			List<CustomAttributeNamedArgument> list = new List<CustomAttributeNamedArgument>();
			if (namedProperties != null)
			{
				for (int i = 0; i < namedProperties.Length; i++)
				{
					list.Add(new CustomAttributeNamedArgument(namedProperties[i], RewrapValue(namedProperties[i].PropertyType, propertyValues[i])));
				}
			}
			if (namedFields != null)
			{
				for (int j = 0; j < namedFields.Length; j++)
				{
					list.Add(new CustomAttributeNamedArgument(namedFields[j], RewrapValue(namedFields[j].FieldType, fieldValues[j])));
				}
			}
			List<CustomAttributeTypedArgument> list2 = new List<CustomAttributeTypedArgument>(constructorArgs.Length);
			ParameterInfo[] parameters = Constructor.GetParameters();
			for (int k = 0; k < constructorArgs.Length; k++)
			{
				list2.Add(RewrapValue(parameters[k].ParameterType, constructorArgs[k]));
			}
			return new CustomAttributeData(asm.ManifestModule, con, list2, list);
		}

		private static CustomAttributeTypedArgument RewrapValue(Type type, object value)
		{
			if (value is Array)
			{
				Array array = (Array)value;
				return RewrapArray(type.Module.universe.Import(array.GetType()), array);
			}
			if (value is CustomAttributeTypedArgument)
			{
				CustomAttributeTypedArgument result = (CustomAttributeTypedArgument)value;
				if (result.Value is Array)
				{
					return RewrapArray(result.ArgumentType, (Array)result.Value);
				}
				return result;
			}
			return new CustomAttributeTypedArgument(type, value);
		}

		private static CustomAttributeTypedArgument RewrapArray(Type arrayType, Array array)
		{
			Type elementType = arrayType.GetElementType();
			CustomAttributeTypedArgument[] array2 = new CustomAttributeTypedArgument[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = RewrapValue(elementType, array.GetValue(i));
			}
			return new CustomAttributeTypedArgument(arrayType, array2);
		}

		internal CustomAttributeBuilder DecodeBlob(Assembly asm)
		{
			if (blob == null)
			{
				return this;
			}
			return ToData(asm).__ToBuilder();
		}

		internal byte[] GetBlob(Assembly asm)
		{
			ByteBuffer byteBuffer = new ByteBuffer(100);
			new BlobWriter(asm, this, byteBuffer).WriteCustomAttributeBlob();
			return byteBuffer.ToArray();
		}
	}
}
