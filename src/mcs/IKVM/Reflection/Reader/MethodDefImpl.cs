using System;
using System.Collections.Generic;
using IKVM.Reflection.Emit;

namespace IKVM.Reflection.Reader
{
	internal sealed class MethodDefImpl : MethodInfo
	{
		private readonly ModuleReader module;

		private readonly int index;

		private readonly TypeDefImpl declaringType;

		private MethodSignature lazyMethodSignature;

		private ParameterInfo returnParameter;

		private ParameterInfo[] parameters;

		private Type[] typeArgs;

		public override int __MethodRVA => module.MethodDef.records[index].RVA;

		public override CallingConventions CallingConvention => MethodSignature.CallingConvention;

		public override MethodAttributes Attributes => (MethodAttributes)module.MethodDef.records[index].Flags;

		internal override int ParameterCount => MethodSignature.GetParameterCount();

		public override ParameterInfo ReturnParameter
		{
			get
			{
				PopulateParameters();
				return returnParameter;
			}
		}

		public override Type ReturnType => ReturnParameter.ParameterType;

		public override Type DeclaringType
		{
			get
			{
				if (!declaringType.IsModulePseudoType)
				{
					return declaringType;
				}
				return null;
			}
		}

		public override string Name => module.GetString(module.MethodDef.records[index].Name);

		public override int MetadataToken => (6 << 24) + index + 1;

		public override bool IsGenericMethodDefinition
		{
			get
			{
				PopulateGenericArguments();
				return typeArgs.Length != 0;
			}
		}

		public override bool IsGenericMethod => IsGenericMethodDefinition;

		public override Module Module => module;

		internal override MethodSignature MethodSignature => lazyMethodSignature ?? (lazyMethodSignature = MethodSignature.ReadSig(module, module.GetBlob(module.MethodDef.records[index].Signature), this));

		internal override bool IsBaked => true;

		internal MethodDefImpl(ModuleReader module, TypeDefImpl declaringType, int index)
		{
			this.module = module;
			this.index = index;
			this.declaringType = declaringType;
		}

		public override MethodBody GetMethodBody()
		{
			return GetMethodBody(this);
		}

		internal MethodBody GetMethodBody(IGenericContext context)
		{
			if ((GetMethodImplementationFlags() & MethodImplAttributes.CodeTypeMask) != MethodImplAttributes.IL)
			{
				return null;
			}
			int rVA = module.MethodDef.records[index].RVA;
			if (rVA != 0)
			{
				return new MethodBody(module, rVA, context);
			}
			return null;
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return (MethodImplAttributes)module.MethodDef.records[index].ImplFlags;
		}

		public override ParameterInfo[] GetParameters()
		{
			PopulateParameters();
			return (ParameterInfo[])parameters.Clone();
		}

		private void PopulateParameters()
		{
			if (parameters != null)
			{
				return;
			}
			MethodSignature methodSignature = MethodSignature;
			parameters = new ParameterInfo[methodSignature.GetParameterCount()];
			int i = module.MethodDef.records[index].ParamList - 1;
			for (int num = ((module.MethodDef.records.Length > index + 1) ? (module.MethodDef.records[index + 1].ParamList - 1) : module.Param.records.Length); i < num; i++)
			{
				int num2 = module.Param.records[i].Sequence - 1;
				if (num2 == -1)
				{
					returnParameter = new ParameterInfoImpl(this, num2, i);
				}
				else
				{
					parameters[num2] = new ParameterInfoImpl(this, num2, i);
				}
			}
			for (int j = 0; j < parameters.Length; j++)
			{
				if (parameters[j] == null)
				{
					parameters[j] = new ParameterInfoImpl(this, j, -1);
				}
			}
			if (returnParameter == null)
			{
				returnParameter = new ParameterInfoImpl(this, -1, -1);
			}
		}

		public override Type[] GetGenericArguments()
		{
			PopulateGenericArguments();
			return Util.Copy(typeArgs);
		}

		private void PopulateGenericArguments()
		{
			if (typeArgs != null)
			{
				return;
			}
			int metadataToken = MetadataToken;
			int num = module.GenericParam.FindFirstByOwner(metadataToken);
			if (num == -1)
			{
				typeArgs = Type.EmptyTypes;
				return;
			}
			List<Type> list = new List<Type>();
			int num2 = module.GenericParam.records.Length;
			for (int i = num; i < num2 && module.GenericParam.records[i].Owner == metadataToken; i++)
			{
				list.Add(new GenericTypeParameter(module, i, 30));
			}
			typeArgs = list.ToArray();
		}

		internal override Type GetGenericMethodArgument(int index)
		{
			PopulateGenericArguments();
			return typeArgs[index];
		}

		internal override int GetGenericMethodArgumentCount()
		{
			PopulateGenericArguments();
			return typeArgs.Length;
		}

		public override MethodInfo GetGenericMethodDefinition()
		{
			if (IsGenericMethodDefinition)
			{
				return this;
			}
			throw new InvalidOperationException();
		}

		public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			return new GenericMethodInstance(declaringType, this, typeArguments);
		}

		internal override int ImportTo(ModuleBuilder module)
		{
			return module.ImportMethodOrField(declaringType, Name, MethodSignature);
		}

		public override MethodInfo[] __GetMethodImpls()
		{
			Type[] array = null;
			List<MethodInfo> list = null;
			foreach (int item in module.MethodImpl.Filter(declaringType.MetadataToken))
			{
				if (module.MethodImpl.records[item].MethodBody == MetadataToken)
				{
					if (array == null)
					{
						array = declaringType.GetGenericArguments();
					}
					if (list == null)
					{
						list = new List<MethodInfo>();
					}
					list.Add((MethodInfo)module.ResolveMethod(module.MethodImpl.records[item].MethodDeclaration, array, null));
				}
			}
			return Util.ToArray(list, Empty<MethodInfo>.Array);
		}

		internal override int GetCurrentToken()
		{
			return MetadataToken;
		}
	}
}
