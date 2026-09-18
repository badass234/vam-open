using System;
using System.Text;
using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	internal sealed class GenericTypeInstance : TypeInfo
	{
		private readonly Type type;

		private readonly Type[] args;

		private readonly CustomModifiers[] mods;

		private Type baseType;

		private int token;

		public override string AssemblyQualifiedName
		{
			get
			{
				string fullName = FullName;
				if (fullName != null)
				{
					return fullName + ", " + type.Assembly.FullName;
				}
				return null;
			}
		}

		public override Type BaseType
		{
			get
			{
				if (baseType == null)
				{
					Type type = this.type.BaseType;
					if (type == null)
					{
						baseType = type;
					}
					else
					{
						baseType = type.BindTypeParameters(this);
					}
				}
				return baseType;
			}
		}

		public override bool IsValueType => type.IsValueType;

		public override bool IsVisible
		{
			get
			{
				if (base.IsVisible)
				{
					Type[] array = args;
					for (int i = 0; i < array.Length; i++)
					{
						if (!array[i].IsVisible)
						{
							return false;
						}
					}
					return true;
				}
				return false;
			}
		}

		public override Type DeclaringType => type.DeclaringType;

		public override TypeAttributes Attributes => type.Attributes;

		public override string Namespace => type.Namespace;

		public override string Name => type.Name;

		public override string FullName
		{
			get
			{
				if (!base.__ContainsMissingType && ContainsGenericParameters)
				{
					return null;
				}
				StringBuilder stringBuilder = new StringBuilder(this.type.FullName);
				stringBuilder.Append('[');
				string value = "";
				Type[] array = args;
				foreach (Type type in array)
				{
					stringBuilder.Append(value).Append('[').Append(type.FullName)
						.Append(", ")
						.Append(type.Assembly.FullName.Replace("]", "\\]"))
						.Append(']');
					value = ",";
				}
				stringBuilder.Append(']');
				return stringBuilder.ToString();
			}
		}

		public override Module Module => type.Module;

		public override bool IsGenericType => true;

		public override bool IsConstructedGenericType => true;

		public override bool ContainsGenericParameters
		{
			get
			{
				Type[] array = args;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].ContainsGenericParameters)
					{
						return true;
					}
				}
				return false;
			}
		}

		protected override bool ContainsMissingTypeImpl
		{
			get
			{
				if (!type.__ContainsMissingType)
				{
					return Type.ContainsMissingType(args);
				}
				return true;
			}
		}

		internal override bool IsBaked => type.IsBaked;

		internal static Type Make(Type type, Type[] typeArguments, CustomModifiers[] mods)
		{
			bool flag = true;
			if (type is TypeBuilder || type is BakedType || type.__IsMissing)
			{
				flag = false;
			}
			else
			{
				for (int i = 0; i < typeArguments.Length; i++)
				{
					if (typeArguments[i] != type.GetGenericTypeArgument(i) || !IsEmpty(mods, i))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				return type;
			}
			return type.Universe.CanonicalizeType(new GenericTypeInstance(type, typeArguments, mods));
		}

		private static bool IsEmpty(CustomModifiers[] mods, int i)
		{
			return mods?[i].IsEmpty ?? true;
		}

		private GenericTypeInstance(Type type, Type[] args, CustomModifiers[] mods)
		{
			this.type = type;
			this.args = args;
			this.mods = mods;
		}

		public override bool Equals(object o)
		{
			GenericTypeInstance genericTypeInstance = o as GenericTypeInstance;
			if (genericTypeInstance != null && genericTypeInstance.type.Equals(type) && Util.ArrayEquals(genericTypeInstance.args, args))
			{
				return Util.ArrayEquals(genericTypeInstance.mods, mods);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (type.GetHashCode() * 3) ^ Util.GetHashCode(args);
		}

		internal override void CheckBaked()
		{
			type.CheckBaked();
		}

		public override FieldInfo[] __GetDeclaredFields()
		{
			FieldInfo[] array = type.__GetDeclaredFields();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].BindTypeParameters(this);
			}
			return array;
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			Type[] array = type.__GetDeclaredInterfaces();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].BindTypeParameters(this);
			}
			return array;
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			MethodBase[] array = type.__GetDeclaredMethods();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].BindTypeParameters(this);
			}
			return array;
		}

		public override Type[] __GetDeclaredTypes()
		{
			return type.__GetDeclaredTypes();
		}

		public override EventInfo[] __GetDeclaredEvents()
		{
			EventInfo[] array = type.__GetDeclaredEvents();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].BindTypeParameters(this);
			}
			return array;
		}

		public override PropertyInfo[] __GetDeclaredProperties()
		{
			PropertyInfo[] array = type.__GetDeclaredProperties();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].BindTypeParameters(this);
			}
			return array;
		}

		public override __MethodImplMap __GetMethodImplMap()
		{
			__MethodImplMap result = type.__GetMethodImplMap();
			result.TargetType = this;
			for (int i = 0; i < result.MethodBodies.Length; i++)
			{
				result.MethodBodies[i] = (MethodInfo)result.MethodBodies[i].BindTypeParameters(this);
				for (int j = 0; j < result.MethodDeclarations[i].Length; j++)
				{
					if (result.MethodDeclarations[i][j].DeclaringType.IsGenericType)
					{
						result.MethodDeclarations[i][j] = (MethodInfo)result.MethodDeclarations[i][j].BindTypeParameters(this);
					}
				}
			}
			return result;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(type.FullName);
			stringBuilder.Append('[');
			string value = "";
			Type[] array = args;
			foreach (Type value2 in array)
			{
				stringBuilder.Append(value);
				stringBuilder.Append(value2);
				value = ",";
			}
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}

		public override Type GetGenericTypeDefinition()
		{
			return type;
		}

		public override Type[] GetGenericArguments()
		{
			return Util.Copy(args);
		}

		public override CustomModifiers[] __GetGenericArgumentsCustomModifiers()
		{
			if (mods == null)
			{
				return new CustomModifiers[args.Length];
			}
			return (CustomModifiers[])mods.Clone();
		}

		internal override Type GetGenericTypeArgument(int index)
		{
			return args[index];
		}

		public override bool __GetLayout(out int packingSize, out int typeSize)
		{
			return type.__GetLayout(out packingSize, out typeSize);
		}

		internal override int GetModuleBuilderToken()
		{
			if (token == 0)
			{
				token = ((ModuleBuilder)type.Module).ImportType(this);
			}
			return token;
		}

		internal override Type BindTypeParameters(IGenericBinder binder)
		{
			for (int i = 0; i < args.Length; i++)
			{
				Type type = args[i].BindTypeParameters(binder);
				if ((object)type != args[i])
				{
					Type[] array = new Type[args.Length];
					Array.Copy(args, array, i);
					array[i++] = type;
					for (; i < args.Length; i++)
					{
						array[i] = args[i].BindTypeParameters(binder);
					}
					return Make(this.type, array, null);
				}
			}
			return this;
		}

		internal override int GetCurrentToken()
		{
			return type.GetCurrentToken();
		}
	}
}
