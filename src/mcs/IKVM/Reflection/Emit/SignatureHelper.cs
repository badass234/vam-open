using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public abstract class SignatureHelper
	{
		private sealed class Lazy : SignatureHelper
		{
			private readonly List<Type> args = new List<Type>();

			internal override Type ReturnType => args[0];

			internal Lazy(byte type)
				: base(type)
			{
			}

			public override byte[] GetSignature()
			{
				throw new NotSupportedException();
			}

			internal override ByteBuffer GetSignature(ModuleBuilder module)
			{
				ByteBuffer byteBuffer = new ByteBuffer(16);
				Signature.WriteSignatureHelper(module, byteBuffer, type, paramCount, args);
				return byteBuffer;
			}

			public override void AddSentinel()
			{
				args.Add(MarkerType.Sentinel);
			}

			public override void __AddArgument(Type argument, bool pinned, CustomModifiers customModifiers)
			{
				if (pinned)
				{
					args.Add(MarkerType.Pinned);
				}
				foreach (CustomModifiers.Entry item in customModifiers)
				{
					args.Add(item.IsRequired ? MarkerType.ModReq : MarkerType.ModOpt);
					args.Add(item.Type);
				}
				args.Add(argument);
				paramCount++;
			}
		}

		private sealed class Eager : SignatureHelper
		{
			private readonly ModuleBuilder module;

			private readonly ByteBuffer bb = new ByteBuffer(16);

			private readonly Type returnType;

			internal override Type ReturnType => returnType;

			internal Eager(ModuleBuilder module, byte type, Type returnType)
				: base(type)
			{
				this.module = module;
				this.returnType = returnType;
				bb.Write(type);
				if (type != 6)
				{
					bb.Write((byte)0);
				}
			}

			public override byte[] GetSignature()
			{
				return GetSignature(null).ToArray();
			}

			internal override ByteBuffer GetSignature(ModuleBuilder module)
			{
				if (type != 6)
				{
					bb.Position = 1;
					bb.Insert(MetadataWriter.GetCompressedUIntLength(paramCount) - bb.GetCompressedUIntLength());
					bb.WriteCompressedUInt(paramCount);
				}
				return bb;
			}

			public override void AddSentinel()
			{
				bb.Write((byte)65);
			}

			public override void __AddArgument(Type argument, bool pinned, CustomModifiers customModifiers)
			{
				if (pinned)
				{
					bb.Write((byte)69);
				}
				foreach (CustomModifiers.Entry item in customModifiers)
				{
					bb.Write((byte)(item.IsRequired ? 31 : 32));
					Signature.WriteTypeSpec(module, bb, item.Type);
				}
				Signature.WriteTypeSpec(module, bb, argument ?? module.universe.System_Void);
				paramCount++;
			}
		}

		protected readonly byte type;

		protected ushort paramCount;

		internal bool HasThis => (type & 0x20) != 0;

		internal abstract Type ReturnType { get; }

		internal int ParameterCount => paramCount;

		private SignatureHelper(byte type)
		{
			this.type = type;
		}

		private static SignatureHelper Create(Module mod, byte type, Type returnType)
		{
			ModuleBuilder moduleBuilder = mod as ModuleBuilder;
			if (moduleBuilder != null)
			{
				return new Eager(moduleBuilder, type, returnType);
			}
			return new Lazy(type);
		}

		public static SignatureHelper GetFieldSigHelper(Module mod)
		{
			return Create(mod, 6, null);
		}

		public static SignatureHelper GetLocalVarSigHelper()
		{
			return new Lazy(7);
		}

		public static SignatureHelper GetLocalVarSigHelper(Module mod)
		{
			return Create(mod, 7, null);
		}

		public static SignatureHelper GetPropertySigHelper(Module mod, Type returnType, Type[] parameterTypes)
		{
			SignatureHelper signatureHelper = Create(mod, 8, returnType);
			signatureHelper.AddArgument(returnType);
			signatureHelper.paramCount = 0;
			signatureHelper.AddArguments(parameterTypes, null, null);
			return signatureHelper;
		}

		public static SignatureHelper GetPropertySigHelper(Module mod, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			return GetPropertySigHelper(mod, CallingConventions.Standard, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
		}

		public static SignatureHelper GetPropertySigHelper(Module mod, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			byte b = 8;
			if ((callingConvention & CallingConventions.HasThis) != 0)
			{
				b |= 0x20;
			}
			SignatureHelper signatureHelper = Create(mod, b, returnType);
			signatureHelper.AddArgument(returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers);
			signatureHelper.paramCount = 0;
			signatureHelper.AddArguments(parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
			return signatureHelper;
		}

		public static SignatureHelper GetMethodSigHelper(CallingConvention unmanagedCallingConvention, Type returnType)
		{
			return GetMethodSigHelper(null, unmanagedCallingConvention, returnType);
		}

		public static SignatureHelper GetMethodSigHelper(CallingConventions callingConvention, Type returnType)
		{
			return GetMethodSigHelper(null, callingConvention, returnType);
		}

		public static SignatureHelper GetMethodSigHelper(Module mod, CallingConvention unmanagedCallConv, Type returnType)
		{
			byte b;
			switch (unmanagedCallConv)
			{
			case CallingConvention.Cdecl:
				b = 1;
				break;
			case CallingConvention.Winapi:
			case CallingConvention.StdCall:
				b = 2;
				break;
			case CallingConvention.ThisCall:
				b = 3;
				break;
			case CallingConvention.FastCall:
				b = 4;
				break;
			default:
				throw new ArgumentOutOfRangeException("unmanagedCallConv");
			}
			SignatureHelper signatureHelper = Create(mod, b, returnType);
			signatureHelper.AddArgument(returnType);
			signatureHelper.paramCount = 0;
			return signatureHelper;
		}

		public static SignatureHelper GetMethodSigHelper(Module mod, CallingConventions callingConvention, Type returnType)
		{
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
			SignatureHelper signatureHelper = Create(mod, b, returnType);
			signatureHelper.AddArgument(returnType);
			signatureHelper.paramCount = 0;
			return signatureHelper;
		}

		public static SignatureHelper GetMethodSigHelper(Module mod, Type returnType, Type[] parameterTypes)
		{
			SignatureHelper signatureHelper = Create(mod, 0, returnType);
			signatureHelper.AddArgument(returnType);
			signatureHelper.paramCount = 0;
			signatureHelper.AddArguments(parameterTypes, null, null);
			return signatureHelper;
		}

		public abstract byte[] GetSignature();

		internal abstract ByteBuffer GetSignature(ModuleBuilder module);

		public abstract void AddSentinel();

		public void AddArgument(Type clsArgument)
		{
			AddArgument(clsArgument, false);
		}

		public void AddArgument(Type argument, bool pinned)
		{
			__AddArgument(argument, pinned, default(CustomModifiers));
		}

		public void AddArgument(Type argument, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			__AddArgument(argument, false, CustomModifiers.FromReqOpt(requiredCustomModifiers, optionalCustomModifiers));
		}

		public abstract void __AddArgument(Type argument, bool pinned, CustomModifiers customModifiers);

		public void AddArguments(Type[] arguments, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			if (arguments != null)
			{
				for (int i = 0; i < arguments.Length; i++)
				{
					__AddArgument(arguments[i], false, CustomModifiers.FromReqOpt(Util.NullSafeElementAt(requiredCustomModifiers, i), Util.NullSafeElementAt(optionalCustomModifiers, i)));
				}
			}
		}
	}
}
