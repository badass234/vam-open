using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection
{
	internal abstract class Signature
	{
		internal const byte DEFAULT = 0;

		internal const byte VARARG = 5;

		internal const byte GENERIC = 16;

		internal const byte HASTHIS = 32;

		internal const byte EXPLICITTHIS = 64;

		internal const byte FIELD = 6;

		internal const byte LOCAL_SIG = 7;

		internal const byte PROPERTY = 8;

		internal const byte GENERICINST = 10;

		internal const byte SENTINEL = 65;

		internal const byte ELEMENT_TYPE_VOID = 1;

		internal const byte ELEMENT_TYPE_BOOLEAN = 2;

		internal const byte ELEMENT_TYPE_CHAR = 3;

		internal const byte ELEMENT_TYPE_I1 = 4;

		internal const byte ELEMENT_TYPE_U1 = 5;

		internal const byte ELEMENT_TYPE_I2 = 6;

		internal const byte ELEMENT_TYPE_U2 = 7;

		internal const byte ELEMENT_TYPE_I4 = 8;

		internal const byte ELEMENT_TYPE_U4 = 9;

		internal const byte ELEMENT_TYPE_I8 = 10;

		internal const byte ELEMENT_TYPE_U8 = 11;

		internal const byte ELEMENT_TYPE_R4 = 12;

		internal const byte ELEMENT_TYPE_R8 = 13;

		internal const byte ELEMENT_TYPE_STRING = 14;

		internal const byte ELEMENT_TYPE_PTR = 15;

		internal const byte ELEMENT_TYPE_BYREF = 16;

		internal const byte ELEMENT_TYPE_VALUETYPE = 17;

		internal const byte ELEMENT_TYPE_CLASS = 18;

		internal const byte ELEMENT_TYPE_VAR = 19;

		internal const byte ELEMENT_TYPE_ARRAY = 20;

		internal const byte ELEMENT_TYPE_GENERICINST = 21;

		internal const byte ELEMENT_TYPE_TYPEDBYREF = 22;

		internal const byte ELEMENT_TYPE_I = 24;

		internal const byte ELEMENT_TYPE_U = 25;

		internal const byte ELEMENT_TYPE_FNPTR = 27;

		internal const byte ELEMENT_TYPE_OBJECT = 28;

		internal const byte ELEMENT_TYPE_SZARRAY = 29;

		internal const byte ELEMENT_TYPE_MVAR = 30;

		internal const byte ELEMENT_TYPE_CMOD_REQD = 31;

		internal const byte ELEMENT_TYPE_CMOD_OPT = 32;

		internal const byte ELEMENT_TYPE_PINNED = 69;

		internal abstract void WriteSig(ModuleBuilder module, ByteBuffer bb);

		private static Type ReadGenericInst(ModuleReader module, ByteReader br, IGenericContext context)
		{
			Type type;
			switch (br.ReadByte())
			{
			case 18:
				type = ReadTypeDefOrRefEncoded(module, br, context).MarkNotValueType();
				break;
			case 17:
				type = ReadTypeDefOrRefEncoded(module, br, context).MarkValueType();
				break;
			default:
				throw new BadImageFormatException();
			}
			if (!type.__IsMissing && !type.IsGenericTypeDefinition)
			{
				throw new BadImageFormatException();
			}
			int num = br.ReadCompressedUInt();
			Type[] array = new Type[num];
			CustomModifiers[] array2 = null;
			for (int i = 0; i < num; i++)
			{
				CustomModifiers customModifiers = CustomModifiers.Read(module, br, context);
				if (!customModifiers.IsEmpty)
				{
					if (array2 == null)
					{
						array2 = new CustomModifiers[num];
					}
					array2[i] = customModifiers;
				}
				array[i] = ReadType(module, br, context);
			}
			return GenericTypeInstance.Make(type, array, array2);
		}

		internal static Type ReadTypeSpec(ModuleReader module, ByteReader br, IGenericContext context)
		{
			CustomModifiers.Skip(br);
			return ReadType(module, br, context);
		}

		private static Type ReadFunctionPointer(ModuleReader module, ByteReader br, IGenericContext context)
		{
			__StandAloneMethodSig sig = MethodSignature.ReadStandAloneMethodSig(module, br, context);
			if (module.universe.EnableFunctionPointers)
			{
				return FunctionPointerType.Make(module.universe, sig);
			}
			return module.universe.System_IntPtr;
		}

		internal static Type[] ReadMethodSpec(ModuleReader module, ByteReader br, IGenericContext context)
		{
			if (br.ReadByte() != 10)
			{
				throw new BadImageFormatException();
			}
			Type[] array = new Type[br.ReadCompressedUInt()];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ReadType(module, br, context);
			}
			return array;
		}

		private static int[] ReadArraySizes(ByteReader br)
		{
			int num = br.ReadCompressedUInt();
			if (num == 0)
			{
				return null;
			}
			int[] array = new int[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = br.ReadCompressedUInt();
			}
			return array;
		}

		private static int[] ReadArrayBounds(ByteReader br)
		{
			int num = br.ReadCompressedUInt();
			if (num == 0)
			{
				return null;
			}
			int[] array = new int[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = br.ReadCompressedInt();
			}
			return array;
		}

		private static Type ReadTypeOrVoid(ModuleReader module, ByteReader br, IGenericContext context)
		{
			if (br.PeekByte() == 1)
			{
				br.ReadByte();
				return module.universe.System_Void;
			}
			return ReadType(module, br, context);
		}

		protected static Type ReadType(ModuleReader module, ByteReader br, IGenericContext context)
		{
			switch (br.ReadByte())
			{
			case 18:
				return ReadTypeDefOrRefEncoded(module, br, context).MarkNotValueType();
			case 17:
				return ReadTypeDefOrRefEncoded(module, br, context).MarkValueType();
			case 2:
				return module.universe.System_Boolean;
			case 3:
				return module.universe.System_Char;
			case 4:
				return module.universe.System_SByte;
			case 5:
				return module.universe.System_Byte;
			case 6:
				return module.universe.System_Int16;
			case 7:
				return module.universe.System_UInt16;
			case 8:
				return module.universe.System_Int32;
			case 9:
				return module.universe.System_UInt32;
			case 10:
				return module.universe.System_Int64;
			case 11:
				return module.universe.System_UInt64;
			case 12:
				return module.universe.System_Single;
			case 13:
				return module.universe.System_Double;
			case 24:
				return module.universe.System_IntPtr;
			case 25:
				return module.universe.System_UIntPtr;
			case 14:
				return module.universe.System_String;
			case 28:
				return module.universe.System_Object;
			case 19:
				return context.GetGenericTypeArgument(br.ReadCompressedUInt());
			case 30:
				return context.GetGenericMethodArgument(br.ReadCompressedUInt());
			case 21:
				return ReadGenericInst(module, br, context);
			case 29:
			{
				CustomModifiers customModifiers = CustomModifiers.Read(module, br, context);
				return ReadType(module, br, context).__MakeArrayType(customModifiers);
			}
			case 20:
			{
				CustomModifiers customModifiers = CustomModifiers.Read(module, br, context);
				return ReadType(module, br, context).__MakeArrayType(br.ReadCompressedUInt(), ReadArraySizes(br), ReadArrayBounds(br), customModifiers);
			}
			case 15:
			{
				CustomModifiers customModifiers = CustomModifiers.Read(module, br, context);
				return ReadTypeOrVoid(module, br, context).__MakePointerType(customModifiers);
			}
			case 27:
				return ReadFunctionPointer(module, br, context);
			default:
				throw new BadImageFormatException();
			}
		}

		internal static void ReadLocalVarSig(ModuleReader module, ByteReader br, IGenericContext context, List<LocalVariableInfo> list)
		{
			if (br.Length < 2 || br.ReadByte() != 7)
			{
				throw new BadImageFormatException("Invalid local variable signature");
			}
			int num = br.ReadCompressedUInt();
			for (int i = 0; i < num; i++)
			{
				if (br.PeekByte() == 22)
				{
					br.ReadByte();
					list.Add(new LocalVariableInfo(i, module.universe.System_TypedReference, false, default(CustomModifiers)));
					continue;
				}
				CustomModifiers mods = CustomModifiers.Read(module, br, context);
				bool pinned = false;
				if (br.PeekByte() == 69)
				{
					br.ReadByte();
					pinned = true;
				}
				CustomModifiers mods2 = CustomModifiers.Read(module, br, context);
				Type type = ReadTypeOrByRef(module, br, context);
				list.Add(new LocalVariableInfo(i, type, pinned, CustomModifiers.Combine(mods, mods2)));
			}
		}

		private static Type ReadTypeOrByRef(ModuleReader module, ByteReader br, IGenericContext context)
		{
			if (br.PeekByte() == 16)
			{
				br.ReadByte();
				CustomModifiers customModifiers = CustomModifiers.Read(module, br, context);
				return ReadTypeOrVoid(module, br, context).__MakeByRefType(customModifiers);
			}
			return ReadType(module, br, context);
		}

		protected static Type ReadRetType(ModuleReader module, ByteReader br, IGenericContext context)
		{
			switch (br.PeekByte())
			{
			case 1:
				br.ReadByte();
				return module.universe.System_Void;
			case 22:
				br.ReadByte();
				return module.universe.System_TypedReference;
			default:
				return ReadTypeOrByRef(module, br, context);
			}
		}

		protected static Type ReadParam(ModuleReader module, ByteReader br, IGenericContext context)
		{
			byte b = br.PeekByte();
			if (b == 22)
			{
				br.ReadByte();
				return module.universe.System_TypedReference;
			}
			return ReadTypeOrByRef(module, br, context);
		}

		protected static void WriteType(ModuleBuilder module, ByteBuffer bb, Type type)
		{
			while (type.HasElementType)
			{
				byte sigElementType = type.SigElementType;
				bb.Write(sigElementType);
				if (sigElementType == 20)
				{
					WriteCustomModifiers(module, bb, type.__GetCustomModifiers());
					WriteType(module, bb, type.GetElementType());
					bb.WriteCompressedUInt(type.GetArrayRank());
					int[] array = type.__GetArraySizes();
					bb.WriteCompressedUInt(array.Length);
					for (int i = 0; i < array.Length; i++)
					{
						bb.WriteCompressedUInt(array[i]);
					}
					int[] array2 = type.__GetArrayLowerBounds();
					bb.WriteCompressedUInt(array2.Length);
					for (int j = 0; j < array2.Length; j++)
					{
						bb.WriteCompressedInt(array2[j]);
					}
					return;
				}
				WriteCustomModifiers(module, bb, type.__GetCustomModifiers());
				type = type.GetElementType();
			}
			if (type.__IsBuiltIn)
			{
				bb.Write(type.SigElementType);
				return;
			}
			if (type.IsGenericParameter)
			{
				bb.Write(type.SigElementType);
				bb.WriteCompressedUInt(type.GenericParameterPosition);
				return;
			}
			if (!type.__IsMissing && type.IsGenericType)
			{
				WriteGenericSignature(module, bb, type);
				return;
			}
			if (type.__IsFunctionPointer)
			{
				bb.Write((byte)27);
				WriteStandAloneMethodSig(module, bb, type.__MethodSignature);
				return;
			}
			if (type.IsValueType)
			{
				bb.Write((byte)17);
			}
			else
			{
				bb.Write((byte)18);
			}
			bb.WriteTypeDefOrRefEncoded(module.GetTypeToken(type).Token);
		}

		private static void WriteGenericSignature(ModuleBuilder module, ByteBuffer bb, Type type)
		{
			Type[] genericArguments = type.GetGenericArguments();
			CustomModifiers[] array = type.__GetGenericArgumentsCustomModifiers();
			if (!type.IsGenericTypeDefinition)
			{
				type = type.GetGenericTypeDefinition();
			}
			bb.Write((byte)21);
			if (type.IsValueType)
			{
				bb.Write((byte)17);
			}
			else
			{
				bb.Write((byte)18);
			}
			bb.WriteTypeDefOrRefEncoded(module.GetTypeToken(type).Token);
			bb.WriteCompressedUInt(genericArguments.Length);
			for (int i = 0; i < genericArguments.Length; i++)
			{
				WriteCustomModifiers(module, bb, array[i]);
				WriteType(module, bb, genericArguments[i]);
			}
		}

		protected static void WriteCustomModifiers(ModuleBuilder module, ByteBuffer bb, CustomModifiers modifiers)
		{
			foreach (CustomModifiers.Entry item in modifiers)
			{
				bb.Write((byte)(item.IsRequired ? 31 : 32));
				bb.WriteTypeDefOrRefEncoded(module.GetTypeTokenForMemberRef(item.Type));
			}
		}

		internal static Type ReadTypeDefOrRefEncoded(ModuleReader module, ByteReader br, IGenericContext context)
		{
			int num = br.ReadCompressedUInt();
			switch (num & 3)
			{
			case 0:
				return module.ResolveType((2 << 24) + (num >> 2), null, null);
			case 1:
				return module.ResolveType((1 << 24) + (num >> 2), null, null);
			case 2:
				return module.ResolveType((27 << 24) + (num >> 2), context);
			default:
				throw new BadImageFormatException();
			}
		}

		internal static void WriteStandAloneMethodSig(ModuleBuilder module, ByteBuffer bb, __StandAloneMethodSig sig)
		{
			if (sig.IsUnmanaged)
			{
				switch (sig.UnmanagedCallingConvention)
				{
				case CallingConvention.Cdecl:
					bb.Write((byte)1);
					break;
				case CallingConvention.Winapi:
				case CallingConvention.StdCall:
					bb.Write((byte)2);
					break;
				case CallingConvention.ThisCall:
					bb.Write((byte)3);
					break;
				case CallingConvention.FastCall:
					bb.Write((byte)4);
					break;
				default:
					throw new ArgumentOutOfRangeException("callingConvention");
				}
			}
			else
			{
				CallingConventions callingConvention = sig.CallingConvention;
				byte b = 0;
				if ((callingConvention & CallingConventions.HasThis) != 0)
				{
					b |= 0x20;
				}
				if ((callingConvention & CallingConventions.ExplicitThis) != 0)
				{
					b |= 0x40;
				}
				if ((callingConvention & CallingConventions.VarArgs) != 0)
				{
					b |= 5;
				}
				bb.Write(b);
			}
			Type[] parameterTypes = sig.ParameterTypes;
			Type[] optionalParameterTypes = sig.OptionalParameterTypes;
			bb.WriteCompressedUInt(parameterTypes.Length + optionalParameterTypes.Length);
			WriteCustomModifiers(module, bb, sig.GetReturnTypeCustomModifiers());
			WriteType(module, bb, sig.ReturnType);
			int num = 0;
			Type[] array = parameterTypes;
			foreach (Type type in array)
			{
				WriteCustomModifiers(module, bb, sig.GetParameterCustomModifiers(num++));
				WriteType(module, bb, type);
			}
			if (optionalParameterTypes.Length != 0)
			{
				bb.Write((byte)65);
				array = optionalParameterTypes;
				foreach (Type type2 in array)
				{
					WriteCustomModifiers(module, bb, sig.GetParameterCustomModifiers(num++));
					WriteType(module, bb, type2);
				}
			}
		}

		internal static void WriteTypeSpec(ModuleBuilder module, ByteBuffer bb, Type type)
		{
			WriteType(module, bb, type);
		}

		internal static void WriteMethodSpec(ModuleBuilder module, ByteBuffer bb, Type[] genArgs)
		{
			bb.Write((byte)10);
			bb.WriteCompressedUInt(genArgs.Length);
			foreach (Type type in genArgs)
			{
				WriteType(module, bb, type);
			}
		}

		internal static Type[] ReadOptionalParameterTypes(ModuleReader module, ByteReader br, IGenericContext context, out CustomModifiers[] customModifiers)
		{
			br.ReadByte();
			int num = br.ReadCompressedUInt();
			CustomModifiers.Skip(br);
			ReadRetType(module, br, context);
			for (int i = 0; i < num; i++)
			{
				if (br.PeekByte() == 65)
				{
					br.ReadByte();
					Type[] array = new Type[num - i];
					customModifiers = new CustomModifiers[array.Length];
					for (int j = 0; j < array.Length; j++)
					{
						customModifiers[j] = CustomModifiers.Read(module, br, context);
						array[j] = ReadType(module, br, context);
					}
					return array;
				}
				CustomModifiers.Skip(br);
				ReadType(module, br, context);
			}
			customModifiers = Empty<CustomModifiers>.Array;
			return Type.EmptyTypes;
		}

		protected static Type[] BindTypeParameters(IGenericBinder binder, Type[] types)
		{
			if (types == null || types.Length == 0)
			{
				return Type.EmptyTypes;
			}
			Type[] array = new Type[types.Length];
			for (int i = 0; i < types.Length; i++)
			{
				array[i] = types[i].BindTypeParameters(binder);
			}
			return array;
		}

		internal static void WriteSignatureHelper(ModuleBuilder module, ByteBuffer bb, byte flags, ushort paramCount, List<Type> args)
		{
			bb.Write(flags);
			if (flags != 6)
			{
				bb.WriteCompressedUInt(paramCount);
			}
			foreach (Type arg in args)
			{
				if (arg == null)
				{
					bb.Write((byte)1);
				}
				else if (arg is MarkerType)
				{
					bb.Write(arg.SigElementType);
				}
				else
				{
					WriteType(module, bb, arg);
				}
			}
		}
	}
}
