using System;
using System.Collections.Generic;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Emit
{
	public sealed class PropertyBuilder : PropertyInfo
	{
		private struct Accessor
		{
			internal short Semantics;

			internal MethodBuilder Method;
		}

		private readonly TypeBuilder typeBuilder;

		private readonly string name;

		private PropertyAttributes attributes;

		private PropertySignature sig;

		private MethodBuilder getter;

		private MethodBuilder setter;

		private readonly List<Accessor> accessors = new List<Accessor>();

		private int lazyPseudoToken;

		private bool patchCallingConvention;

		internal override PropertySignature PropertySignature => sig;

		public override PropertyAttributes Attributes => attributes;

		public override bool CanRead => getter != null;

		public override bool CanWrite => setter != null;

		public override Type DeclaringType => typeBuilder;

		public override string Name => name;

		public override Module Module => typeBuilder.Module;

		internal override bool IsPublic
		{
			get
			{
				foreach (Accessor accessor in accessors)
				{
					if (accessor.Method.IsPublic)
					{
						return true;
					}
				}
				return false;
			}
		}

		internal override bool IsNonPrivate
		{
			get
			{
				foreach (Accessor accessor in accessors)
				{
					if ((accessor.Method.Attributes & MethodAttributes.MemberAccessMask) > MethodAttributes.Private)
					{
						return true;
					}
				}
				return false;
			}
		}

		internal override bool IsStatic
		{
			get
			{
				foreach (Accessor accessor in accessors)
				{
					if (accessor.Method.IsStatic)
					{
						return true;
					}
				}
				return false;
			}
		}

		internal override bool IsBaked => typeBuilder.IsBaked;

		internal PropertyBuilder(TypeBuilder typeBuilder, string name, PropertyAttributes attributes, PropertySignature sig, bool patchCallingConvention)
		{
			this.typeBuilder = typeBuilder;
			this.name = name;
			this.attributes = attributes;
			this.sig = sig;
			this.patchCallingConvention = patchCallingConvention;
		}

		public void SetGetMethod(MethodBuilder mdBuilder)
		{
			getter = mdBuilder;
			Accessor item = default(Accessor);
			item.Semantics = 2;
			item.Method = mdBuilder;
			accessors.Add(item);
		}

		public void SetSetMethod(MethodBuilder mdBuilder)
		{
			setter = mdBuilder;
			Accessor item = default(Accessor);
			item.Semantics = 1;
			item.Method = mdBuilder;
			accessors.Add(item);
		}

		public void AddOtherMethod(MethodBuilder mdBuilder)
		{
			Accessor item = default(Accessor);
			item.Semantics = 4;
			item.Method = mdBuilder;
			accessors.Add(item);
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			if (customBuilder.KnownCA == KnownCA.SpecialNameAttribute)
			{
				attributes |= PropertyAttributes.SpecialName;
				return;
			}
			if (lazyPseudoToken == 0)
			{
				lazyPseudoToken = typeBuilder.ModuleBuilder.AllocPseudoToken();
			}
			typeBuilder.ModuleBuilder.SetCustomAttribute(lazyPseudoToken, customBuilder);
		}

		public override object GetRawConstantValue()
		{
			if (lazyPseudoToken != 0)
			{
				return typeBuilder.ModuleBuilder.Constant.GetRawConstantValue(typeBuilder.ModuleBuilder, lazyPseudoToken);
			}
			throw new InvalidOperationException();
		}

		public override MethodInfo GetGetMethod(bool nonPublic)
		{
			if (!nonPublic && (!(getter != null) || !getter.IsPublic))
			{
				return null;
			}
			return getter;
		}

		public override MethodInfo GetSetMethod(bool nonPublic)
		{
			if (!nonPublic && (!(setter != null) || !setter.IsPublic))
			{
				return null;
			}
			return setter;
		}

		public override MethodInfo[] GetAccessors(bool nonPublic)
		{
			List<MethodInfo> list = new List<MethodInfo>();
			foreach (Accessor accessor in accessors)
			{
				AddAccessor(list, nonPublic, accessor.Method);
			}
			return list.ToArray();
		}

		private static void AddAccessor(List<MethodInfo> list, bool nonPublic, MethodInfo method)
		{
			if (method != null && (nonPublic || method.IsPublic))
			{
				list.Add(method);
			}
		}

		public void SetConstant(object defaultValue)
		{
			if (lazyPseudoToken == 0)
			{
				lazyPseudoToken = typeBuilder.ModuleBuilder.AllocPseudoToken();
			}
			attributes |= PropertyAttributes.HasDefault;
			typeBuilder.ModuleBuilder.AddConstant(lazyPseudoToken, defaultValue);
		}

		internal void Bake()
		{
			if (patchCallingConvention)
			{
				sig.HasThis = !IsStatic;
			}
			PropertyTable.Record newRecord = new PropertyTable.Record
			{
				Flags = (short)attributes,
				Name = typeBuilder.ModuleBuilder.Strings.Add(name),
				Type = typeBuilder.ModuleBuilder.GetSignatureBlobIndex(sig)
			};
			int num = 0x17000000 | typeBuilder.ModuleBuilder.Property.AddRecord(newRecord);
			if (lazyPseudoToken == 0)
			{
				lazyPseudoToken = num;
			}
			else
			{
				typeBuilder.ModuleBuilder.RegisterTokenFixup(lazyPseudoToken, num);
			}
			foreach (Accessor accessor in accessors)
			{
				AddMethodSemantics(accessor.Semantics, accessor.Method.MetadataToken, num);
			}
		}

		private void AddMethodSemantics(short semantics, int methodToken, int propertyToken)
		{
			MethodSemanticsTable.Record newRecord = new MethodSemanticsTable.Record
			{
				Semantics = semantics,
				Method = methodToken,
				Association = propertyToken
			};
			typeBuilder.ModuleBuilder.MethodSemantics.AddRecord(newRecord);
		}

		internal override int GetCurrentToken()
		{
			if (typeBuilder.ModuleBuilder.IsSaved && ModuleBuilder.IsPseudoToken(lazyPseudoToken))
			{
				return typeBuilder.ModuleBuilder.ResolvePseudoToken(lazyPseudoToken);
			}
			return lazyPseudoToken;
		}
	}
}
