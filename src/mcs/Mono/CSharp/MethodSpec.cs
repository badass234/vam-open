using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public sealed class MethodSpec : MemberSpec, IParametersMember, IInterfaceMemberSpec
	{
		private MethodBase inflatedMetaInfo;

		private AParametersCollection parameters;

		private TypeSpec returnType;

		private TypeSpec[] targs;

		private TypeParameterSpec[] constraints;

		public static readonly MethodSpec Excluded = new MethodSpec(MemberKind.Method, InternalType.FakeInternalType, null, null, ParametersCompiled.EmptyReadOnlyParameters, (Modifiers)0);

		public override int Arity
		{
			get
			{
				if (!base.IsGeneric)
				{
					return 0;
				}
				return GenericDefinition.TypeParametersCount;
			}
		}

		public TypeParameterSpec[] Constraints
		{
			get
			{
				if (constraints == null && base.IsGeneric)
				{
					constraints = GenericDefinition.TypeParameters;
				}
				return constraints;
			}
		}

		public bool IsConstructor => Kind == MemberKind.Constructor;

		public new IMethodDefinition MemberDefinition => (IMethodDefinition)definition;

		public IGenericMethodDefinition GenericDefinition => (IGenericMethodDefinition)definition;

		public bool IsAsync => (base.Modifiers & Modifiers.ASYNC) != 0;

		public bool IsExtensionMethod
		{
			get
			{
				if (base.IsStatic)
				{
					return parameters.HasExtensionMethodType;
				}
				return false;
			}
		}

		public bool IsSealed => (base.Modifiers & Modifiers.SEALED) != 0;

		public bool IsVirtual => (base.Modifiers & (Modifiers.ABSTRACT | Modifiers.VIRTUAL | Modifiers.OVERRIDE)) != 0;

		public bool IsReservedMethod
		{
			get
			{
				if (Kind != MemberKind.Operator)
				{
					return base.IsAccessor;
				}
				return true;
			}
		}

		TypeSpec IInterfaceMemberSpec.MemberType => returnType;

		public AParametersCollection Parameters => parameters;

		public TypeSpec ReturnType => returnType;

		public TypeSpec[] TypeArguments => targs;

		public MethodSpec(MemberKind kind, TypeSpec declaringType, IMethodDefinition details, TypeSpec returnType, AParametersCollection parameters, Modifiers modifiers)
			: base(kind, declaringType, details, modifiers)
		{
			this.parameters = parameters;
			this.returnType = returnType;
		}

		public MethodSpec GetGenericMethodDefinition()
		{
			if (!base.IsGeneric && !base.DeclaringType.IsGeneric)
			{
				return this;
			}
			return MemberCache.GetMember(declaringType, this);
		}

		public MethodBase GetMetaInfo()
		{
			if (inflatedMetaInfo == null)
			{
				if ((state & StateFlags.PendingMetaInflate) != 0)
				{
					Type metaInfo = base.DeclaringType.GetMetaInfo();
					if (base.DeclaringType.IsTypeBuilder)
					{
						if (IsConstructor)
						{
							inflatedMetaInfo = TypeBuilder.GetConstructor(metaInfo, (ConstructorInfo)MemberDefinition.Metadata);
						}
						else
						{
							inflatedMetaInfo = TypeBuilder.GetMethod(metaInfo, (MethodInfo)MemberDefinition.Metadata);
						}
					}
					else
					{
						inflatedMetaInfo = MethodBase.GetMethodFromHandle(MemberDefinition.Metadata.MethodHandle, metaInfo.TypeHandle);
					}
					state &= ~StateFlags.PendingMetaInflate;
				}
				else
				{
					inflatedMetaInfo = MemberDefinition.Metadata;
				}
			}
			if ((state & StateFlags.PendingMakeMethod) != 0)
			{
				Type[] array = new Type[targs.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = targs[i].GetMetaInfo();
				}
				inflatedMetaInfo = ((MethodInfo)inflatedMetaInfo).MakeGenericMethod(array);
				state &= ~StateFlags.PendingMakeMethod;
			}
			return inflatedMetaInfo;
		}

		public override string GetSignatureForDocumentation()
		{
			string text;
			switch (Kind)
			{
			case MemberKind.Constructor:
				text = "#ctor";
				break;
			case MemberKind.Method:
				text = ((Arity <= 0) ? Name : (Name + "``" + Arity));
				break;
			default:
				text = Name;
				break;
			}
			text = base.DeclaringType.GetSignatureForDocumentation() + "." + text + parameters.GetSignatureForDocumentation();
			if (Kind == MemberKind.Operator)
			{
				Operator.OpType value = Operator.GetType(Name).Value;
				if (value == Operator.OpType.Explicit || value == Operator.OpType.Implicit)
				{
					text = text + "~" + ReturnType.GetSignatureForDocumentation();
				}
			}
			return text;
		}

		public override string GetSignatureForError()
		{
			string text;
			if (IsConstructor)
			{
				text = base.DeclaringType.GetSignatureForError() + "." + base.DeclaringType.Name;
			}
			else if (Kind == MemberKind.Operator)
			{
				Operator.OpType value = Operator.GetType(Name).Value;
				text = ((value != Operator.OpType.Implicit && value != Operator.OpType.Explicit) ? (base.DeclaringType.GetSignatureForError() + ".operator " + Operator.GetName(value)) : (base.DeclaringType.GetSignatureForError() + "." + Operator.GetName(value) + " operator " + returnType.GetSignatureForError()));
			}
			else
			{
				if (base.IsAccessor)
				{
					int num = Name.IndexOf('_');
					text = Name.Substring(num + 1);
					string text2 = Name.Substring(0, num);
					if (num == 3)
					{
						int count = parameters.Count;
						if (count > 0 && text2 == "get")
						{
							text = "this" + parameters.GetSignatureForError("[", "]", count);
						}
						else if (count > 1 && text2 == "set")
						{
							text = "this" + parameters.GetSignatureForError("[", "]", count - 1);
						}
					}
					return base.DeclaringType.GetSignatureForError() + "." + text + "." + text2;
				}
				text = base.GetSignatureForError();
				if (targs != null)
				{
					text = text + "<" + TypeManager.CSharpName(targs) + ">";
				}
				else if (base.IsGeneric)
				{
					text = text + "<" + TypeManager.CSharpName(GenericDefinition.TypeParameters) + ">";
				}
			}
			return text + parameters.GetSignatureForError();
		}

		public override MemberSpec InflateMember(TypeParameterInflator inflator)
		{
			MethodSpec methodSpec = (MethodSpec)base.InflateMember(inflator);
			methodSpec.inflatedMetaInfo = null;
			methodSpec.returnType = inflator.Inflate(returnType);
			methodSpec.parameters = parameters.Inflate(inflator);
			if (base.IsGeneric)
			{
				methodSpec.constraints = TypeParameterSpec.InflateConstraints(inflator, Constraints);
			}
			return methodSpec;
		}

		public MethodSpec MakeGenericMethod(IMemberContext context, params TypeSpec[] targs)
		{
			if (targs == null)
			{
				throw new ArgumentNullException();
			}
			TypeParameterInflator inflator = new TypeParameterInflator(context, base.DeclaringType, GenericDefinition.TypeParameters, targs);
			MethodSpec obj = (MethodSpec)MemberwiseClone();
			obj.declaringType = inflator.TypeInstance;
			obj.returnType = inflator.Inflate(returnType);
			obj.parameters = parameters.Inflate(inflator);
			obj.targs = targs;
			obj.constraints = TypeParameterSpec.InflateConstraints(inflator, constraints ?? GenericDefinition.TypeParameters);
			obj.state |= StateFlags.PendingMakeMethod;
			return obj;
		}

		public MethodSpec Mutate(TypeParameterMutator mutator)
		{
			TypeSpec[] array = TypeArguments;
			if (array != null)
			{
				array = mutator.Mutate(array);
			}
			TypeSpec typeSpec = base.DeclaringType;
			if (base.DeclaringType.IsGenericOrParentIsGeneric)
			{
				typeSpec = mutator.Mutate(typeSpec);
			}
			if (array == TypeArguments && typeSpec == base.DeclaringType)
			{
				return this;
			}
			MethodSpec methodSpec = (MethodSpec)MemberwiseClone();
			if (typeSpec != base.DeclaringType)
			{
				methodSpec.inflatedMetaInfo = null;
				methodSpec.declaringType = typeSpec;
				methodSpec.state |= StateFlags.PendingMetaInflate;
			}
			if (array != null)
			{
				methodSpec.targs = array;
				methodSpec.state |= StateFlags.PendingMakeMethod;
			}
			return methodSpec;
		}

		public override List<MissingTypeSpecReference> ResolveMissingDependencies(MemberSpec caller)
		{
			List<MissingTypeSpecReference> list = returnType.ResolveMissingDependencies(this);
			TypeSpec[] types = parameters.Types;
			for (int i = 0; i < types.Length; i++)
			{
				List<MissingTypeSpecReference> missingDependencies = types[i].GetMissingDependencies(this);
				if (missingDependencies != null)
				{
					if (list == null)
					{
						list = new List<MissingTypeSpecReference>();
					}
					list.AddRange(missingDependencies);
				}
			}
			if (Arity > 0)
			{
				TypeParameterSpec[] typeParameters = GenericDefinition.TypeParameters;
				for (int i = 0; i < typeParameters.Length; i++)
				{
					List<MissingTypeSpecReference> missingDependencies2 = typeParameters[i].GetMissingDependencies(this);
					if (missingDependencies2 != null)
					{
						if (list == null)
						{
							list = new List<MissingTypeSpecReference>();
						}
						list.AddRange(missingDependencies2);
					}
				}
			}
			return list;
		}
	}
}
