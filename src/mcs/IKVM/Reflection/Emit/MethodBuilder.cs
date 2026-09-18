using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class MethodBuilder : MethodInfo
	{
		private sealed class ParameterInfoImpl : ParameterInfo
		{
			private readonly MethodBuilder method;

			private readonly int parameter;

			private ParameterBuilder ParameterBuilder
			{
				get
				{
					if (method.parameters != null)
					{
						foreach (ParameterBuilder parameter in method.parameters)
						{
							if (parameter.Position - 1 == this.parameter)
							{
								return parameter;
							}
						}
					}
					return null;
				}
			}

			public override string Name => ParameterBuilder?.Name;

			public override Type ParameterType
			{
				get
				{
					if (parameter != -1)
					{
						return method.parameterTypes[parameter];
					}
					return method.returnType;
				}
			}

			public override ParameterAttributes Attributes => (ParameterAttributes)(ParameterBuilder?.Attributes ?? 0);

			public override int Position => parameter;

			public override object RawDefaultValue
			{
				get
				{
					ParameterBuilder parameterBuilder = ParameterBuilder;
					if (parameterBuilder != null && (parameterBuilder.Attributes & 0x1000) != 0)
					{
						return method.ModuleBuilder.Constant.GetRawConstantValue(method.ModuleBuilder, parameterBuilder.PseudoToken);
					}
					if (parameterBuilder != null && (parameterBuilder.Attributes & 0x10) != 0)
					{
						return Missing.Value;
					}
					return null;
				}
			}

			public override MemberInfo Member => method;

			public override int MetadataToken => ParameterBuilder?.PseudoToken ?? 134217728;

			internal override Module Module => method.Module;

			internal ParameterInfoImpl(MethodBuilder method, int parameter)
			{
				this.method = method;
				this.parameter = parameter;
			}

			public override CustomModifiers __GetCustomModifiers()
			{
				return method.customModifiers.GetParameterCustomModifiers(parameter);
			}

			public override bool __TryGetFieldMarshal(out FieldMarshal fieldMarshal)
			{
				fieldMarshal = default(FieldMarshal);
				return false;
			}
		}

		private readonly TypeBuilder typeBuilder;

		private readonly string name;

		private readonly int pseudoToken;

		private int nameIndex;

		private int signature;

		private Type returnType;

		private Type[] parameterTypes;

		private PackedCustomModifiers customModifiers;

		private MethodAttributes attributes;

		private MethodImplAttributes implFlags;

		private ILGenerator ilgen;

		private int rva = -1;

		private CallingConventions callingConvention;

		private List<ParameterBuilder> parameters;

		private GenericTypeParameterBuilder[] gtpb;

		private List<CustomAttributeBuilder> declarativeSecurity;

		private MethodSignature methodSignature;

		private bool initLocals = true;

		public override Type ReturnType => returnType;

		public override ParameterInfo ReturnParameter => new ParameterInfoImpl(this, -1);

		public override MethodAttributes Attributes => attributes;

		internal override int ParameterCount => parameterTypes.Length;

		public override Type DeclaringType
		{
			get
			{
				if (!typeBuilder.IsModulePseudoType)
				{
					return typeBuilder;
				}
				return null;
			}
		}

		public override string Name => name;

		public override CallingConventions CallingConvention => callingConvention;

		public override int MetadataToken => pseudoToken;

		public override bool IsGenericMethod => gtpb != null;

		public override bool IsGenericMethodDefinition => gtpb != null;

		public override Module Module => typeBuilder.Module;

		public override int __MethodRVA
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool InitLocals
		{
			get
			{
				return initLocals;
			}
			set
			{
				initLocals = value;
			}
		}

		internal ModuleBuilder ModuleBuilder => typeBuilder.ModuleBuilder;

		internal override MethodSignature MethodSignature
		{
			get
			{
				if (methodSignature == null)
				{
					methodSignature = MethodSignature.MakeFromBuilder(returnType ?? typeBuilder.Universe.System_Void, parameterTypes ?? Type.EmptyTypes, customModifiers, callingConvention, (gtpb != null) ? gtpb.Length : 0);
				}
				return methodSignature;
			}
		}

		internal override bool IsBaked => typeBuilder.IsBaked;

		internal MethodBuilder(TypeBuilder typeBuilder, string name, MethodAttributes attributes, CallingConventions callingConvention)
		{
			this.typeBuilder = typeBuilder;
			this.name = name;
			pseudoToken = typeBuilder.ModuleBuilder.AllocPseudoToken();
			this.attributes = attributes;
			if ((attributes & MethodAttributes.Static) == 0)
			{
				callingConvention |= CallingConventions.HasThis;
			}
			this.callingConvention = callingConvention;
		}

		public ILGenerator GetILGenerator()
		{
			return GetILGenerator(16);
		}

		public ILGenerator GetILGenerator(int streamSize)
		{
			if (rva != -1)
			{
				throw new InvalidOperationException();
			}
			if (ilgen == null)
			{
				ilgen = new ILGenerator(typeBuilder.ModuleBuilder, streamSize);
			}
			return ilgen;
		}

		public void __ReleaseILGenerator()
		{
			if (ilgen != null)
			{
				rva = ilgen.WriteBody(initLocals);
				ilgen = null;
			}
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		private void SetDllImportPseudoCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			CallingConvention? fieldValue = customBuilder.GetFieldValue<CallingConvention>("CallingConvention");
			CharSet? fieldValue2 = customBuilder.GetFieldValue<CharSet>("CharSet");
			SetDllImportPseudoCustomAttribute((string)customBuilder.GetConstructorArgument(0), (string)customBuilder.GetFieldValue("EntryPoint"), fieldValue, fieldValue2, (bool?)customBuilder.GetFieldValue("BestFitMapping"), (bool?)customBuilder.GetFieldValue("ThrowOnUnmappableChar"), (bool?)customBuilder.GetFieldValue("SetLastError"), (bool?)customBuilder.GetFieldValue("PreserveSig"), (bool?)customBuilder.GetFieldValue("ExactSpelling"));
		}

		internal void SetDllImportPseudoCustomAttribute(string dllName, string entryName, CallingConvention? nativeCallConv, CharSet? nativeCharSet, bool? bestFitMapping, bool? throwOnUnmappableChar, bool? setLastError, bool? preserveSig, bool? exactSpelling)
		{
			short num = 256;
			if (bestFitMapping.HasValue)
			{
				num = (short)(num | (bestFitMapping.Value ? 16 : 32));
			}
			if (throwOnUnmappableChar.HasValue)
			{
				num = (short)(num | (throwOnUnmappableChar.Value ? 4096 : 8192));
			}
			if (nativeCallConv.HasValue)
			{
				num &= -1793;
				switch (nativeCallConv.Value)
				{
				case System.Runtime.InteropServices.CallingConvention.Cdecl:
					num |= 0x200;
					break;
				case System.Runtime.InteropServices.CallingConvention.FastCall:
					num |= 0x500;
					break;
				case System.Runtime.InteropServices.CallingConvention.StdCall:
					num |= 0x300;
					break;
				case System.Runtime.InteropServices.CallingConvention.ThisCall:
					num |= 0x400;
					break;
				case System.Runtime.InteropServices.CallingConvention.Winapi:
					num |= 0x100;
					break;
				}
			}
			if (nativeCharSet.HasValue)
			{
				num &= -7;
				switch (nativeCharSet.Value)
				{
				case CharSet.None:
				case CharSet.Ansi:
					num |= 2;
					break;
				case CharSet.Auto:
					num |= 6;
					break;
				case CharSet.Unicode:
					num |= 4;
					break;
				}
			}
			if (exactSpelling.HasValue && exactSpelling.Value)
			{
				num |= 1;
			}
			if (!preserveSig.HasValue || preserveSig.Value)
			{
				implFlags |= MethodImplAttributes.PreserveSig;
			}
			if (setLastError.HasValue && setLastError.Value)
			{
				num |= 0x40;
			}
			ImplMapTable.Record newRecord = new ImplMapTable.Record
			{
				MappingFlags = num,
				MemberForwarded = pseudoToken,
				ImportName = ModuleBuilder.Strings.Add(entryName ?? name),
				ImportScope = ModuleBuilder.ModuleRef.FindOrAddRecord((dllName != null) ? ModuleBuilder.Strings.Add(dllName) : 0)
			};
			ModuleBuilder.ImplMap.AddRecord(newRecord);
		}

		private void SetMethodImplAttribute(CustomAttributeBuilder customBuilder)
		{
			MethodImplOptions methodImplOptions;
			switch (customBuilder.Constructor.ParameterCount)
			{
			case 0:
				methodImplOptions = (MethodImplOptions)0;
				break;
			case 1:
			{
				object constructorArgument = customBuilder.GetConstructorArgument(0);
				methodImplOptions = ((!(constructorArgument is short)) ? ((!(constructorArgument is int)) ? ((MethodImplOptions)constructorArgument) : ((MethodImplOptions)(int)constructorArgument)) : ((MethodImplOptions)(short)constructorArgument));
				break;
			}
			default:
				throw new NotSupportedException();
			}
			MethodCodeType? fieldValue = customBuilder.GetFieldValue<MethodCodeType>("MethodCodeType");
			implFlags = (MethodImplAttributes)methodImplOptions;
			if (fieldValue.HasValue)
			{
				implFlags |= (MethodImplAttributes)fieldValue.Value;
			}
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			switch (customBuilder.KnownCA)
			{
			case KnownCA.DllImportAttribute:
				SetDllImportPseudoCustomAttribute(customBuilder.DecodeBlob(Module.Assembly));
				attributes |= MethodAttributes.PinvokeImpl;
				return;
			case KnownCA.MethodImplAttribute:
				SetMethodImplAttribute(customBuilder.DecodeBlob(Module.Assembly));
				return;
			case KnownCA.PreserveSigAttribute:
				implFlags |= MethodImplAttributes.PreserveSig;
				return;
			case KnownCA.SpecialNameAttribute:
				attributes |= MethodAttributes.SpecialName;
				return;
			case KnownCA.SuppressUnmanagedCodeSecurityAttribute:
				attributes |= MethodAttributes.HasSecurity;
				break;
			}
			ModuleBuilder.SetCustomAttribute(pseudoToken, customBuilder);
		}

		public void __AddDeclarativeSecurity(CustomAttributeBuilder customBuilder)
		{
			attributes |= MethodAttributes.HasSecurity;
			if (declarativeSecurity == null)
			{
				declarativeSecurity = new List<CustomAttributeBuilder>();
			}
			declarativeSecurity.Add(customBuilder);
		}

		public void AddDeclarativeSecurity(SecurityAction securityAction, PermissionSet permissionSet)
		{
			ModuleBuilder.AddDeclarativeSecurity(pseudoToken, securityAction, permissionSet);
			attributes |= MethodAttributes.HasSecurity;
		}

		public void SetImplementationFlags(MethodImplAttributes attributes)
		{
			implFlags = attributes;
		}

		public ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string strParamName)
		{
			if (parameters == null)
			{
				parameters = new List<ParameterBuilder>();
			}
			ModuleBuilder.Param.AddVirtualRecord();
			ParameterBuilder parameterBuilder = new ParameterBuilder(ModuleBuilder, position, attributes, strParamName);
			if (parameters.Count == 0 || position >= parameters[parameters.Count - 1].Position)
			{
				parameters.Add(parameterBuilder);
			}
			else
			{
				for (int i = 0; i < parameters.Count; i++)
				{
					if (parameters[i].Position > position)
					{
						parameters.Insert(i, parameterBuilder);
						break;
					}
				}
			}
			return parameterBuilder;
		}

		private void CheckSig()
		{
			if (methodSignature != null)
			{
				throw new InvalidOperationException("The method signature can not be modified after it has been used.");
			}
		}

		public void SetParameters(params Type[] parameterTypes)
		{
			CheckSig();
			this.parameterTypes = Util.Copy(parameterTypes);
		}

		public void SetReturnType(Type returnType)
		{
			CheckSig();
			this.returnType = returnType ?? Module.universe.System_Void;
		}

		public void SetSignature(Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			SetSignature(returnType, parameterTypes, PackedCustomModifiers.CreateFromExternal(returnTypeOptionalCustomModifiers, returnTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, parameterTypeRequiredCustomModifiers, Util.NullSafeLength(parameterTypes)));
		}

		public void __SetSignature(Type returnType, CustomModifiers returnTypeCustomModifiers, Type[] parameterTypes, CustomModifiers[] parameterTypeCustomModifiers)
		{
			SetSignature(returnType, parameterTypes, PackedCustomModifiers.CreateFromExternal(returnTypeCustomModifiers, parameterTypeCustomModifiers, Util.NullSafeLength(parameterTypes)));
		}

		private void SetSignature(Type returnType, Type[] parameterTypes, PackedCustomModifiers customModifiers)
		{
			CheckSig();
			this.returnType = returnType ?? Module.universe.System_Void;
			this.parameterTypes = Util.Copy(parameterTypes);
			this.customModifiers = customModifiers;
		}

		public GenericTypeParameterBuilder[] DefineGenericParameters(params string[] names)
		{
			CheckSig();
			if (gtpb != null)
			{
				throw new InvalidOperationException("Generic parameters already defined.");
			}
			gtpb = new GenericTypeParameterBuilder[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				gtpb[i] = new GenericTypeParameterBuilder(names[i], this, i);
			}
			return (GenericTypeParameterBuilder[])gtpb.Clone();
		}

		public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			return new GenericMethodInstance(typeBuilder, this, typeArguments);
		}

		public override MethodInfo GetGenericMethodDefinition()
		{
			if (gtpb == null)
			{
				throw new InvalidOperationException();
			}
			return this;
		}

		public override Type[] GetGenericArguments()
		{
			return Util.Copy(gtpb);
		}

		internal override Type GetGenericMethodArgument(int index)
		{
			return gtpb[index];
		}

		internal override int GetGenericMethodArgumentCount()
		{
			if (gtpb != null)
			{
				return gtpb.Length;
			}
			return 0;
		}

		public void __SetAttributes(MethodAttributes attributes)
		{
			this.attributes = attributes;
		}

		public void __SetCallingConvention(CallingConventions callingConvention)
		{
			this.callingConvention = callingConvention;
			methodSignature = null;
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return implFlags;
		}

		public override ParameterInfo[] GetParameters()
		{
			ParameterInfo[] array = new ParameterInfo[parameterTypes.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ParameterInfoImpl(this, i);
			}
			return array;
		}

		public Module GetModule()
		{
			return typeBuilder.Module;
		}

		public MethodToken GetToken()
		{
			return new MethodToken(pseudoToken);
		}

		public override MethodBody GetMethodBody()
		{
			throw new NotSupportedException();
		}

		public void __AddUnmanagedExport(string name, int ordinal)
		{
			ModuleBuilder.AddUnmanagedExport(name, ordinal, this, new RelativeVirtualAddress(uint.MaxValue));
		}

		public void CreateMethodBody(byte[] il, int count)
		{
			if (il == null)
			{
				throw new NotSupportedException();
			}
			if (il.Length != count)
			{
				Array.Resize(ref il, count);
			}
			SetMethodBody(il, 16, null, null, null);
		}

		public void SetMethodBody(byte[] il, int maxStack, byte[] localSignature, IEnumerable<ExceptionHandler> exceptionHandlers, IEnumerable<int> tokenFixups)
		{
			ByteBuffer methodBodies = ModuleBuilder.methodBodies;
			if (localSignature == null && exceptionHandlers == null && maxStack <= 8 && il.Length < 64)
			{
				rva = methodBodies.Position;
				ILGenerator.WriteTinyHeader(methodBodies, il.Length);
			}
			else
			{
				methodBodies.Align(4);
				rva = methodBodies.Position;
				bool num = initLocals;
				bool exceptions = exceptionHandlers != null;
				ushort maxStack2 = (ushort)maxStack;
				int codeLength = il.Length;
				int localVarSigTok;
				if (localSignature != null)
				{
					localVarSigTok = ModuleBuilder.GetSignatureToken(localSignature, localSignature.Length).Token;
				}
				else
				{
					localVarSigTok = 0;
				}
				ILGenerator.WriteFatHeader(methodBodies, num, exceptions, maxStack2, codeLength, localVarSigTok);
			}
			if (tokenFixups != null)
			{
				ILGenerator.AddTokenFixups(methodBodies.Position, ModuleBuilder.tokenFixupOffsets, tokenFixups);
			}
			methodBodies.Write(il);
			if (exceptionHandlers == null)
			{
				return;
			}
			List<ILGenerator.ExceptionBlock> list = new List<ILGenerator.ExceptionBlock>();
			foreach (ExceptionHandler exceptionHandler in exceptionHandlers)
			{
				list.Add(new ILGenerator.ExceptionBlock(exceptionHandler));
			}
			ILGenerator.WriteExceptionHandlers(methodBodies, list);
		}

		internal void Bake()
		{
			nameIndex = ModuleBuilder.Strings.Add(name);
			signature = ModuleBuilder.GetSignatureBlobIndex(MethodSignature);
			__ReleaseILGenerator();
			if (declarativeSecurity != null)
			{
				ModuleBuilder.AddDeclarativeSecurity(pseudoToken, declarativeSecurity);
			}
		}

		internal void WriteMethodDefRecord(int baseRVA, MetadataWriter mw, ref int paramList)
		{
			if (rva != -1)
			{
				mw.Write(rva + baseRVA);
			}
			else
			{
				mw.Write(0);
			}
			mw.Write((short)implFlags);
			mw.Write((short)attributes);
			mw.WriteStringIndex(nameIndex);
			mw.WriteBlobIndex(signature);
			mw.WriteParam(paramList);
			if (parameters != null)
			{
				paramList += parameters.Count;
			}
		}

		internal void WriteParamRecords(MetadataWriter mw)
		{
			if (parameters == null)
			{
				return;
			}
			foreach (ParameterBuilder parameter in parameters)
			{
				parameter.WriteParamRecord(mw);
			}
		}

		internal void FixupToken(int token, ref int parameterToken)
		{
			typeBuilder.ModuleBuilder.RegisterTokenFixup(pseudoToken, token);
			if (parameters == null)
			{
				return;
			}
			foreach (ParameterBuilder parameter in parameters)
			{
				parameter.FixupToken(parameterToken++);
			}
		}

		internal override int ImportTo(ModuleBuilder other)
		{
			return other.ImportMethodOrField(typeBuilder, name, MethodSignature);
		}

		internal void CheckBaked()
		{
			typeBuilder.CheckBaked();
		}

		internal override int GetCurrentToken()
		{
			if (typeBuilder.ModuleBuilder.IsSaved)
			{
				return typeBuilder.ModuleBuilder.ResolvePseudoToken(pseudoToken);
			}
			return pseudoToken;
		}
	}
}
