using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection
{
	internal sealed class MethodSignature : Signature
	{
		private sealed class UnboundGenericMethodContext : IGenericContext
		{
			private readonly IGenericContext original;

			internal UnboundGenericMethodContext(IGenericContext original)
			{
				this.original = original;
			}

			public Type GetGenericTypeArgument(int index)
			{
				return original.GetGenericTypeArgument(index);
			}

			public Type GetGenericMethodArgument(int index)
			{
				return UnboundGenericMethodParameter.Make(index);
			}
		}

		private sealed class Binder : IGenericBinder
		{
			private readonly Type declaringType;

			private readonly Type[] methodArgs;

			internal Binder(Type declaringType, Type[] methodArgs)
			{
				this.declaringType = declaringType;
				this.methodArgs = methodArgs;
			}

			public Type BindTypeParameter(Type type)
			{
				return declaringType.GetGenericTypeArgument(type.GenericParameterPosition);
			}

			public Type BindMethodParameter(Type type)
			{
				if (methodArgs == null)
				{
					return type;
				}
				return methodArgs[type.GenericParameterPosition];
			}
		}

		private sealed class Unbinder : IGenericBinder
		{
			internal static readonly Unbinder Instance = new Unbinder();

			private Unbinder()
			{
			}

			public Type BindTypeParameter(Type type)
			{
				return type;
			}

			public Type BindMethodParameter(Type type)
			{
				return UnboundGenericMethodParameter.Make(type.GenericParameterPosition);
			}
		}

		private readonly Type returnType;

		private readonly Type[] parameterTypes;

		private readonly PackedCustomModifiers modifiers;

		private readonly CallingConventions callingConvention;

		private readonly int genericParamCount;

		internal CallingConventions CallingConvention => callingConvention;

		internal int GenericParameterCount => genericParamCount;

		internal MethodSignature(Type returnType, Type[] parameterTypes, PackedCustomModifiers modifiers, CallingConventions callingConvention, int genericParamCount)
		{
			this.returnType = returnType;
			this.parameterTypes = parameterTypes;
			this.modifiers = modifiers;
			this.callingConvention = callingConvention;
			this.genericParamCount = genericParamCount;
		}

		public override bool Equals(object obj)
		{
			MethodSignature methodSignature = obj as MethodSignature;
			if (methodSignature != null && methodSignature.callingConvention == callingConvention && methodSignature.genericParamCount == genericParamCount && methodSignature.returnType.Equals(returnType) && Util.ArrayEquals(methodSignature.parameterTypes, parameterTypes))
			{
				return methodSignature.modifiers.Equals(modifiers);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return genericParamCount ^ (77 * (int)callingConvention) ^ (3 * returnType.GetHashCode()) ^ (Util.GetHashCode(parameterTypes) * 5) ^ (modifiers.GetHashCode() * 55);
		}

		internal static MethodSignature ReadSig(ModuleReader module, ByteReader br, IGenericContext context)
		{
			byte b = br.ReadByte();
			CallingConventions callingConventions;
			switch (b & 7)
			{
			case 0:
				callingConventions = CallingConventions.Standard;
				break;
			case 5:
				callingConventions = CallingConventions.VarArgs;
				break;
			default:
				throw new BadImageFormatException();
			}
			if ((b & 0x20) != 0)
			{
				callingConventions |= CallingConventions.HasThis;
			}
			if ((b & 0x40) != 0)
			{
				callingConventions |= CallingConventions.ExplicitThis;
			}
			int num = 0;
			if ((b & 0x10) != 0)
			{
				num = br.ReadCompressedUInt();
				context = new UnboundGenericMethodContext(context);
			}
			int num2 = br.ReadCompressedUInt();
			CustomModifiers[] array = null;
			PackedCustomModifiers.Pack(ref array, 0, CustomModifiers.Read(module, br, context), num2 + 1);
			Type type = Signature.ReadRetType(module, br, context);
			Type[] array2 = new Type[num2];
			for (int i = 0; i < array2.Length; i++)
			{
				if ((callingConventions & CallingConventions.VarArgs) != 0 && br.PeekByte() == 65)
				{
					Array.Resize(ref array2, i);
					if (array != null)
					{
						Array.Resize(ref array, i + 1);
					}
					break;
				}
				PackedCustomModifiers.Pack(ref array, i + 1, CustomModifiers.Read(module, br, context), num2 + 1);
				array2[i] = Signature.ReadParam(module, br, context);
			}
			return new MethodSignature(type, array2, PackedCustomModifiers.Wrap(array), callingConventions, num);
		}

		internal static __StandAloneMethodSig ReadStandAloneMethodSig(ModuleReader module, ByteReader br, IGenericContext context)
		{
			CallingConventions callingConventions = (CallingConventions)0;
			CallingConvention unmanagedCallingConvention = (CallingConvention)0;
			byte b = br.ReadByte();
			bool unmanaged;
			switch (b & 7)
			{
			case 0:
				callingConventions = CallingConventions.Standard;
				unmanaged = false;
				break;
			case 1:
				unmanagedCallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl;
				unmanaged = true;
				break;
			case 2:
				unmanagedCallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall;
				unmanaged = true;
				break;
			case 3:
				unmanagedCallingConvention = System.Runtime.InteropServices.CallingConvention.ThisCall;
				unmanaged = true;
				break;
			case 4:
				unmanagedCallingConvention = System.Runtime.InteropServices.CallingConvention.FastCall;
				unmanaged = true;
				break;
			case 5:
				callingConventions = CallingConventions.VarArgs;
				unmanaged = false;
				break;
			default:
				throw new BadImageFormatException();
			}
			if ((b & 0x20) != 0)
			{
				callingConventions |= CallingConventions.HasThis;
			}
			if ((b & 0x40) != 0)
			{
				callingConventions |= CallingConventions.ExplicitThis;
			}
			if ((b & 0x10) != 0)
			{
				throw new BadImageFormatException();
			}
			int num = br.ReadCompressedUInt();
			CustomModifiers[] array = null;
			PackedCustomModifiers.Pack(ref array, 0, CustomModifiers.Read(module, br, context), num + 1);
			Type type = Signature.ReadRetType(module, br, context);
			List<Type> list = new List<Type>();
			List<Type> list2 = new List<Type>();
			List<Type> list3 = list;
			for (int i = 0; i < num; i++)
			{
				if (br.PeekByte() == 65)
				{
					br.ReadByte();
					list3 = list2;
				}
				PackedCustomModifiers.Pack(ref array, i + 1, CustomModifiers.Read(module, br, context), num + 1);
				list3.Add(Signature.ReadParam(module, br, context));
			}
			return new __StandAloneMethodSig(unmanaged, unmanagedCallingConvention, callingConventions, type, list.ToArray(), list2.ToArray(), PackedCustomModifiers.Wrap(array));
		}

		internal int GetParameterCount()
		{
			return parameterTypes.Length;
		}

		internal Type GetParameterType(int index)
		{
			return parameterTypes[index];
		}

		internal Type GetReturnType(IGenericBinder binder)
		{
			return returnType.BindTypeParameters(binder);
		}

		internal CustomModifiers GetReturnTypeCustomModifiers(IGenericBinder binder)
		{
			return modifiers.GetReturnTypeCustomModifiers().Bind(binder);
		}

		internal Type GetParameterType(IGenericBinder binder, int index)
		{
			return parameterTypes[index].BindTypeParameters(binder);
		}

		internal CustomModifiers GetParameterCustomModifiers(IGenericBinder binder, int index)
		{
			return modifiers.GetParameterCustomModifiers(index).Bind(binder);
		}

		internal MethodSignature Bind(Type type, Type[] methodArgs)
		{
			Binder binder = new Binder(type, methodArgs);
			return new MethodSignature(returnType.BindTypeParameters(binder), Signature.BindTypeParameters(binder, parameterTypes), modifiers.Bind(binder), callingConvention, genericParamCount);
		}

		internal static MethodSignature MakeFromBuilder(Type returnType, Type[] parameterTypes, PackedCustomModifiers modifiers, CallingConventions callingConvention, int genericParamCount)
		{
			if (genericParamCount > 0)
			{
				returnType = returnType.BindTypeParameters(Unbinder.Instance);
				parameterTypes = Signature.BindTypeParameters(Unbinder.Instance, parameterTypes);
				modifiers = modifiers.Bind(Unbinder.Instance);
			}
			return new MethodSignature(returnType, parameterTypes, modifiers, callingConvention, genericParamCount);
		}

		internal bool MatchParameterTypes(MethodSignature other)
		{
			return Util.ArrayEquals(other.parameterTypes, parameterTypes);
		}

		internal bool MatchParameterTypes(Type[] types)
		{
			return Util.ArrayEquals(types, parameterTypes);
		}

		internal override void WriteSig(ModuleBuilder module, ByteBuffer bb)
		{
			WriteSigImpl(module, bb, parameterTypes.Length);
		}

		internal void WriteMethodRefSig(ModuleBuilder module, ByteBuffer bb, Type[] optionalParameterTypes, CustomModifiers[] customModifiers)
		{
			WriteSigImpl(module, bb, parameterTypes.Length + optionalParameterTypes.Length);
			if (optionalParameterTypes.Length != 0)
			{
				bb.Write((byte)65);
				for (int i = 0; i < optionalParameterTypes.Length; i++)
				{
					Signature.WriteCustomModifiers(module, bb, Util.NullSafeElementAt(customModifiers, i));
					Signature.WriteType(module, bb, optionalParameterTypes[i]);
				}
			}
		}

		private void WriteSigImpl(ModuleBuilder module, ByteBuffer bb, int parameterCount)
		{
			byte b = (byte)(((callingConvention & CallingConventions.Any) == CallingConventions.VarArgs) ? 5 : ((genericParamCount > 0) ? 16 : 0));
			if ((callingConvention & CallingConventions.HasThis) != 0)
			{
				b |= 0x20;
			}
			if ((callingConvention & CallingConventions.ExplicitThis) != 0)
			{
				b |= 0x40;
			}
			bb.Write(b);
			if (genericParamCount > 0)
			{
				bb.WriteCompressedUInt(genericParamCount);
			}
			bb.WriteCompressedUInt(parameterCount);
			Signature.WriteCustomModifiers(module, bb, modifiers.GetReturnTypeCustomModifiers());
			Signature.WriteType(module, bb, returnType);
			for (int i = 0; i < parameterTypes.Length; i++)
			{
				Signature.WriteCustomModifiers(module, bb, modifiers.GetParameterCustomModifiers(i));
				Signature.WriteType(module, bb, parameterTypes[i]);
			}
		}
	}
}
