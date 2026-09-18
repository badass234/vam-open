using System.Runtime.InteropServices;
using System.Text;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection
{
	public struct FieldMarshal
	{
		private const UnmanagedType NATIVE_TYPE_MAX = (UnmanagedType)80;

		public UnmanagedType UnmanagedType;

		public UnmanagedType? ArraySubType;

		public short? SizeParamIndex;

		public int? SizeConst;

		public VarEnum? SafeArraySubType;

		public Type SafeArrayUserDefinedSubType;

		public int? IidParameterIndex;

		public string MarshalType;

		public string MarshalCookie;

		public Type MarshalTypeRef;

		internal static bool ReadFieldMarshal(Module module, int token, out FieldMarshal fm)
		{
			fm = default(FieldMarshal);
			SortedTable<FieldMarshalTable.Record>.Enumerator enumerator = module.FieldMarshal.Filter(token).GetEnumerator();
			if (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				ByteReader blob = module.GetBlob(module.FieldMarshal.records[current].NativeType);
				fm.UnmanagedType = (UnmanagedType)blob.ReadCompressedUInt();
				if (fm.UnmanagedType == UnmanagedType.LPArray)
				{
					fm.ArraySubType = (UnmanagedType)blob.ReadCompressedUInt();
					if (fm.ArraySubType == (UnmanagedType)80)
					{
						fm.ArraySubType = null;
					}
					if (blob.Length != 0)
					{
						fm.SizeParamIndex = (short)blob.ReadCompressedUInt();
						if (blob.Length != 0)
						{
							fm.SizeConst = blob.ReadCompressedUInt();
							if (blob.Length != 0 && blob.ReadCompressedUInt() == 0)
							{
								fm.SizeParamIndex = null;
							}
						}
					}
				}
				else if (fm.UnmanagedType == UnmanagedType.SafeArray)
				{
					if (blob.Length != 0)
					{
						fm.SafeArraySubType = (VarEnum)blob.ReadCompressedUInt();
						if (blob.Length != 0)
						{
							fm.SafeArrayUserDefinedSubType = ReadType(module, blob);
						}
					}
				}
				else if (fm.UnmanagedType == UnmanagedType.ByValArray)
				{
					fm.SizeConst = blob.ReadCompressedUInt();
					if (blob.Length != 0)
					{
						fm.ArraySubType = (UnmanagedType)blob.ReadCompressedUInt();
					}
				}
				else if (fm.UnmanagedType == UnmanagedType.ByValTStr)
				{
					fm.SizeConst = blob.ReadCompressedUInt();
				}
				else if (fm.UnmanagedType == UnmanagedType.Interface || fm.UnmanagedType == UnmanagedType.IDispatch || fm.UnmanagedType == UnmanagedType.IUnknown)
				{
					if (blob.Length != 0)
					{
						fm.IidParameterIndex = blob.ReadCompressedUInt();
					}
				}
				else if (fm.UnmanagedType == UnmanagedType.CustomMarshaler)
				{
					blob.ReadCompressedUInt();
					blob.ReadCompressedUInt();
					fm.MarshalType = ReadString(blob);
					fm.MarshalCookie = ReadString(blob);
					TypeNameParser typeNameParser = TypeNameParser.Parse(fm.MarshalType, false);
					if (!typeNameParser.Error)
					{
						fm.MarshalTypeRef = typeNameParser.GetType(module.universe, module, false, fm.MarshalType, false, false);
					}
				}
				return true;
			}
			return false;
		}

		internal static void SetMarshalAsAttribute(ModuleBuilder module, int token, CustomAttributeBuilder attribute)
		{
			attribute = attribute.DecodeBlob(module.Assembly);
			FieldMarshalTable.Record newRecord = new FieldMarshalTable.Record
			{
				Parent = token,
				NativeType = WriteMarshallingDescriptor(module, attribute)
			};
			module.FieldMarshal.AddRecord(newRecord);
		}

		private static int WriteMarshallingDescriptor(ModuleBuilder module, CustomAttributeBuilder attribute)
		{
			object constructorArgument = attribute.GetConstructorArgument(0);
			UnmanagedType unmanagedType = ((constructorArgument is short) ? ((UnmanagedType)(short)constructorArgument) : ((!(constructorArgument is int)) ? ((UnmanagedType)constructorArgument) : ((UnmanagedType)(int)constructorArgument)));
			ByteBuffer byteBuffer = new ByteBuffer(5);
			byteBuffer.WriteCompressedUInt((int)unmanagedType);
			switch (unmanagedType)
			{
			case UnmanagedType.LPArray:
			{
				UnmanagedType value = attribute.GetFieldValue<UnmanagedType>("ArraySubType") ?? ((UnmanagedType)80);
				byteBuffer.WriteCompressedUInt((int)value);
				int? num = attribute.GetFieldValue<short>("SizeParamIndex");
				int? fieldValue3 = attribute.GetFieldValue<int>("SizeConst");
				if (num.HasValue)
				{
					byteBuffer.WriteCompressedUInt(num.Value);
					if (fieldValue3.HasValue)
					{
						byteBuffer.WriteCompressedUInt(fieldValue3.Value);
						byteBuffer.WriteCompressedUInt(1);
					}
				}
				else if (fieldValue3.HasValue)
				{
					byteBuffer.WriteCompressedUInt(0);
					byteBuffer.WriteCompressedUInt(fieldValue3.Value);
					byteBuffer.WriteCompressedUInt(0);
				}
				break;
			}
			case UnmanagedType.SafeArray:
			{
				VarEnum? fieldValue = attribute.GetFieldValue<VarEnum>("SafeArraySubType");
				if (fieldValue.HasValue)
				{
					byteBuffer.WriteCompressedUInt((int)fieldValue.Value);
					Type type = (Type)attribute.GetFieldValue("SafeArrayUserDefinedSubType");
					if (type != null)
					{
						WriteType(module, byteBuffer, type);
					}
				}
				break;
			}
			case UnmanagedType.ByValArray:
			{
				byteBuffer.WriteCompressedUInt(attribute.GetFieldValue<int>("SizeConst") ?? 1);
				UnmanagedType? fieldValue4 = attribute.GetFieldValue<UnmanagedType>("ArraySubType");
				if (fieldValue4.HasValue)
				{
					byteBuffer.WriteCompressedUInt((int)fieldValue4.Value);
				}
				break;
			}
			case UnmanagedType.ByValTStr:
				byteBuffer.WriteCompressedUInt(attribute.GetFieldValue<int>("SizeConst").Value);
				break;
			case UnmanagedType.IUnknown:
			case UnmanagedType.IDispatch:
			case UnmanagedType.Interface:
			{
				int? fieldValue2 = attribute.GetFieldValue<int>("IidParameterIndex");
				if (fieldValue2.HasValue)
				{
					byteBuffer.WriteCompressedUInt(fieldValue2.Value);
				}
				break;
			}
			case UnmanagedType.CustomMarshaler:
			{
				byteBuffer.WriteCompressedUInt(0);
				byteBuffer.WriteCompressedUInt(0);
				string text = (string)attribute.GetFieldValue("MarshalType");
				if (text != null)
				{
					WriteString(byteBuffer, text);
				}
				else
				{
					WriteType(module, byteBuffer, (Type)attribute.GetFieldValue("MarshalTypeRef"));
				}
				WriteString(byteBuffer, ((string)attribute.GetFieldValue("MarshalCookie")) ?? "");
				break;
			}
			}
			return module.Blobs.Add(byteBuffer);
		}

		private static Type ReadType(Module module, ByteReader br)
		{
			string text = ReadString(br);
			if (text == "")
			{
				return null;
			}
			return module.Assembly.GetType(text) ?? module.universe.GetType(text, true);
		}

		private static void WriteType(Module module, ByteBuffer bb, Type type)
		{
			WriteString(bb, (type.Assembly == module.Assembly) ? type.FullName : type.AssemblyQualifiedName);
		}

		private static string ReadString(ByteReader br)
		{
			return Encoding.UTF8.GetString(br.ReadBytes(br.ReadCompressedUInt()));
		}

		private static void WriteString(ByteBuffer bb, string str)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			bb.WriteCompressedUInt(bytes.Length);
			bb.Write(bytes);
		}
	}
}
