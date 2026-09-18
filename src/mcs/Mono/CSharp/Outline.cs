using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;

namespace Mono.CSharp
{
	public class Outline
	{
		private bool declared_only;

		private bool show_private;

		private bool filter_obsolete;

		private IndentedTextWriter o;

		private Type t;

		private Type type_multicast_delegate;

		private Type type_object;

		private Type type_value_type;

		private Type type_int;

		private Type type_flags_attribute;

		private Type type_obsolete_attribute;

		private Type type_param_array_attribute;

		private BindingFlags DefaultFlags
		{
			get
			{
				BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
				if (declared_only)
				{
					bindingFlags |= BindingFlags.DeclaredOnly;
				}
				return bindingFlags;
			}
		}

		public Outline(Type t, TextWriter output, bool declared_only, bool show_private, bool filter_obsolete)
		{
			this.t = t;
			o = new IndentedTextWriter(output, "\t");
			this.declared_only = declared_only;
			this.show_private = show_private;
			this.filter_obsolete = filter_obsolete;
			type_multicast_delegate = typeof(MulticastDelegate);
			type_object = typeof(object);
			type_value_type = typeof(ValueType);
			type_int = typeof(int);
			type_flags_attribute = typeof(FlagsAttribute);
			type_obsolete_attribute = typeof(ObsoleteAttribute);
			type_param_array_attribute = typeof(ParamArrayAttribute);
		}

		public void OutlineType()
		{
			OutlineAttributes();
			o.Write(GetTypeVisibility(t));
			if (t.IsClass && !t.IsSubclassOf(type_multicast_delegate))
			{
				if (t.IsSealed)
				{
					o.Write(t.IsAbstract ? " static" : " sealed");
				}
				else if (t.IsAbstract)
				{
					o.Write(" abstract");
				}
			}
			o.Write(" ");
			o.Write(GetTypeKind(t));
			o.Write(" ");
			Type[] array = (Type[])Comparer.Sort(TypeGetInterfaces(t, declared_only));
			Type baseType = t.BaseType;
			if (t.IsSubclassOf(type_multicast_delegate))
			{
				MethodInfo method = t.GetMethod("Invoke");
				o.Write(FormatType(method.ReturnType));
				o.Write(" ");
				o.Write(GetTypeName(t));
				o.Write(" (");
				OutlineParams(method.GetParameters());
				o.Write(")");
				WriteGenericConstraints(t.GetGenericArguments());
				o.WriteLine(";");
				return;
			}
			o.Write(GetTypeName(t));
			bool flag;
			if (((baseType != null && baseType != type_object && baseType != type_value_type) || array.Length != 0) && !t.IsEnum)
			{
				flag = true;
				o.Write(" : ");
				if (baseType != null && baseType != type_object && baseType != type_value_type)
				{
					o.Write(FormatType(baseType));
					flag = false;
				}
				Type[] array2 = array;
				foreach (Type type in array2)
				{
					if (!flag)
					{
						o.Write(", ");
					}
					flag = false;
					o.Write(FormatType(type));
				}
			}
			bool isEnum = t.IsEnum;
			WriteGenericConstraints(t.GetGenericArguments());
			o.WriteLine(" {");
			o.Indent++;
			FieldInfo[] fields;
			if (t.IsEnum)
			{
				bool flag2 = true;
				fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
				foreach (FieldInfo fieldInfo in fields)
				{
					if (!flag2)
					{
						o.WriteLine(",");
					}
					flag2 = false;
					o.Write(fieldInfo.Name);
				}
				o.WriteLine();
				o.Indent--;
				o.WriteLine("}");
				return;
			}
			flag = true;
			ConstructorInfo[] constructors = t.GetConstructors(DefaultFlags);
			foreach (ConstructorInfo constructorInfo in constructors)
			{
				if (ShowMember(constructorInfo))
				{
					if (flag)
					{
						o.WriteLine();
					}
					flag = false;
					OutlineMemberAttribute(constructorInfo);
					OutlineConstructor(constructorInfo);
					o.WriteLine();
				}
			}
			flag = true;
			MethodBase[] array3 = Comparer.Sort(t.GetMethods(DefaultFlags));
			for (int i = 0; i < array3.Length; i++)
			{
				MethodInfo methodInfo = (MethodInfo)array3[i];
				if (ShowMember(methodInfo) && (methodInfo.Attributes & MethodAttributes.SpecialName) == 0)
				{
					if (flag)
					{
						o.WriteLine();
					}
					flag = false;
					OutlineMemberAttribute(methodInfo);
					OutlineMethod(methodInfo);
					o.WriteLine();
				}
			}
			flag = true;
			MethodInfo[] methods = t.GetMethods(DefaultFlags);
			foreach (MethodInfo methodInfo2 in methods)
			{
				if (ShowMember(methodInfo2) && (methodInfo2.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope && methodInfo2.Name.StartsWith("op_"))
				{
					if (flag)
					{
						o.WriteLine();
					}
					flag = false;
					OutlineMemberAttribute(methodInfo2);
					OutlineOperator(methodInfo2);
					o.WriteLine();
				}
			}
			flag = true;
			PropertyInfo[] array4 = Comparer.Sort(t.GetProperties(DefaultFlags));
			foreach (PropertyInfo propertyInfo in array4)
			{
				if ((propertyInfo.CanRead && ShowMember(propertyInfo.GetGetMethod(true))) || (propertyInfo.CanWrite && ShowMember(propertyInfo.GetSetMethod(true))))
				{
					if (flag)
					{
						o.WriteLine();
					}
					flag = false;
					OutlineMemberAttribute(propertyInfo);
					OutlineProperty(propertyInfo);
					o.WriteLine();
				}
			}
			flag = true;
			fields = t.GetFields(DefaultFlags);
			foreach (FieldInfo fieldInfo2 in fields)
			{
				if (ShowMember(fieldInfo2))
				{
					if (flag)
					{
						o.WriteLine();
					}
					flag = false;
					OutlineMemberAttribute(fieldInfo2);
					OutlineField(fieldInfo2);
					o.WriteLine();
				}
			}
			flag = true;
			EventInfo[] array5 = Comparer.Sort(t.GetEvents(DefaultFlags));
			foreach (EventInfo eventInfo in array5)
			{
				if (ShowMember(eventInfo.GetAddMethod(true)))
				{
					if (flag)
					{
						o.WriteLine();
					}
					flag = false;
					OutlineMemberAttribute(eventInfo);
					OutlineEvent(eventInfo);
					o.WriteLine();
				}
			}
			flag = true;
			MemberInfo[] array6 = Comparer.Sort(t.GetNestedTypes(DefaultFlags));
			for (int i = 0; i < array6.Length; i++)
			{
				Type mi = (Type)array6[i];
				if (ShowMember(mi))
				{
					if (flag)
					{
						o.WriteLine();
					}
					flag = false;
					new Outline(mi, o, declared_only, show_private, filter_obsolete).OutlineType();
				}
			}
			o.Indent--;
			o.WriteLine("}");
		}

		private void OutlineAttributes()
		{
			if (t.IsSerializable)
			{
				o.WriteLine("[Serializable]");
			}
			if (t.IsDefined(type_flags_attribute, true))
			{
				o.WriteLine("[Flags]");
			}
			if (t.IsDefined(type_obsolete_attribute, true))
			{
				o.WriteLine("[Obsolete]");
			}
		}

		private void OutlineMemberAttribute(MemberInfo mi)
		{
		}

		private void OutlineEvent(EventInfo ei)
		{
			MethodBase addMethod = ei.GetAddMethod(true);
			o.Write(GetMethodVisibility(addMethod));
			o.Write("event ");
			o.Write(FormatType(ei.EventHandlerType));
			o.Write(" ");
			o.Write(ei.Name);
			o.Write(";");
		}

		private void OutlineConstructor(ConstructorInfo ci)
		{
			o.Write(GetMethodVisibility(ci));
			o.Write(RemoveGenericArity(t.Name));
			o.Write(" (");
			OutlineParams(ci.GetParameters());
			o.Write(");");
		}

		private void OutlineProperty(PropertyInfo pi)
		{
			ParameterInfo[] indexParameters = pi.GetIndexParameters();
			MethodBase getMethod = pi.GetGetMethod(true);
			MethodBase setMethod = pi.GetSetMethod(true);
			MethodBase methodBase = ((getMethod != null) ? getMethod : setMethod);
			if (pi.CanRead && pi.CanWrite && (getMethod.Attributes & MethodAttributes.MemberAccessMask) != (setMethod.Attributes & MethodAttributes.MemberAccessMask))
			{
				if (getMethod.IsPublic)
				{
					methodBase = getMethod;
				}
				else if (setMethod.IsPublic)
				{
					methodBase = setMethod;
				}
				else if (getMethod.IsFamilyOrAssembly)
				{
					methodBase = getMethod;
				}
				else if (setMethod.IsFamilyOrAssembly)
				{
					methodBase = setMethod;
				}
				else if (getMethod.IsAssembly || getMethod.IsFamily)
				{
					methodBase = getMethod;
				}
				else if (setMethod.IsAssembly || setMethod.IsFamily)
				{
					methodBase = setMethod;
				}
			}
			o.Write(GetMethodVisibility(methodBase));
			o.Write(GetMethodModifiers(methodBase));
			o.Write(FormatType(pi.PropertyType));
			o.Write(" ");
			if (indexParameters.Length == 0)
			{
				o.Write(pi.Name);
			}
			else
			{
				o.Write("this [");
				OutlineParams(indexParameters);
				o.Write("]");
			}
			o.WriteLine(" {");
			o.Indent++;
			if (getMethod != null && ShowMember(getMethod))
			{
				if ((getMethod.Attributes & MethodAttributes.MemberAccessMask) != (methodBase.Attributes & MethodAttributes.MemberAccessMask))
				{
					o.Write(GetMethodVisibility(getMethod));
				}
				o.WriteLine("get;");
			}
			if (setMethod != null && ShowMember(setMethod))
			{
				if ((setMethod.Attributes & MethodAttributes.MemberAccessMask) != (methodBase.Attributes & MethodAttributes.MemberAccessMask))
				{
					o.Write(GetMethodVisibility(setMethod));
				}
				o.WriteLine("set;");
			}
			o.Indent--;
			o.Write("}");
		}

		private void OutlineMethod(MethodInfo mi)
		{
			if (MethodIsExplicitIfaceImpl(mi))
			{
				o.Write(FormatType(mi.ReturnType));
				o.Write(" ");
			}
			else
			{
				o.Write(GetMethodVisibility(mi));
				o.Write(GetMethodModifiers(mi));
				o.Write(FormatType(mi.ReturnType));
				o.Write(" ");
			}
			o.Write(mi.Name);
			o.Write(FormatGenericParams(mi.GetGenericArguments()));
			o.Write(" (");
			OutlineParams(mi.GetParameters());
			o.Write(")");
			WriteGenericConstraints(mi.GetGenericArguments());
			o.Write(";");
		}

		private void OutlineOperator(MethodInfo mi)
		{
			o.Write(GetMethodVisibility(mi));
			o.Write(GetMethodModifiers(mi));
			if (mi.Name == "op_Explicit" || mi.Name == "op_Implicit")
			{
				o.Write(mi.Name.Substring(3).ToLower());
				o.Write(" operator ");
				o.Write(FormatType(mi.ReturnType));
			}
			else
			{
				o.Write(FormatType(mi.ReturnType));
				o.Write(" operator ");
				o.Write(OperatorFromName(mi.Name));
			}
			o.Write(" (");
			OutlineParams(mi.GetParameters());
			o.Write(");");
		}

		private void OutlineParams(ParameterInfo[] pi)
		{
			int num = 0;
			foreach (ParameterInfo parameterInfo in pi)
			{
				if (parameterInfo.ParameterType.IsByRef)
				{
					o.Write(parameterInfo.IsOut ? "out " : "ref ");
					o.Write(FormatType(parameterInfo.ParameterType.GetElementType()));
				}
				else if (parameterInfo.IsDefined(type_param_array_attribute, false))
				{
					o.Write("params ");
					o.Write(FormatType(parameterInfo.ParameterType));
				}
				else
				{
					o.Write(FormatType(parameterInfo.ParameterType));
				}
				o.Write(" ");
				o.Write(parameterInfo.Name);
				if (num + 1 < pi.Length)
				{
					o.Write(", ");
				}
				num++;
			}
		}

		private void OutlineField(FieldInfo fi)
		{
			if (fi.IsPublic)
			{
				o.Write("public ");
			}
			if (fi.IsFamily)
			{
				o.Write("protected ");
			}
			if (fi.IsPrivate)
			{
				o.Write("private ");
			}
			if (fi.IsAssembly)
			{
				o.Write("public ");
			}
			if (fi.IsLiteral)
			{
				o.Write("const ");
			}
			else if (fi.IsStatic)
			{
				o.Write("static ");
			}
			if (fi.IsInitOnly)
			{
				o.Write("readonly ");
			}
			o.Write(FormatType(fi.FieldType));
			o.Write(" ");
			o.Write(fi.Name);
			if (fi.IsLiteral)
			{
				object rawConstantValue = fi.GetRawConstantValue();
				o.Write(" = ");
				if (rawConstantValue is char)
				{
					o.Write("'{0}'", rawConstantValue);
				}
				else if (rawConstantValue is string)
				{
					o.Write("\"{0}\"", rawConstantValue);
				}
				else
				{
					o.Write(fi.GetRawConstantValue());
				}
			}
			o.Write(";");
		}

		private static string GetMethodVisibility(MethodBase m)
		{
			if (m.DeclaringType.IsInterface)
			{
				return "";
			}
			if (m.IsPublic)
			{
				return "public ";
			}
			if (m.IsFamily)
			{
				return "protected ";
			}
			if (m.IsPrivate)
			{
				return "private ";
			}
			if (m.IsAssembly)
			{
				return "public ";
			}
			return null;
		}

		private static string GetMethodModifiers(MethodBase method)
		{
			if (method.IsStatic)
			{
				return "static ";
			}
			if (method.IsFinal)
			{
				if (method.IsVirtual)
				{
					return null;
				}
				return "sealed ";
			}
			if (method.IsVirtual && !method.DeclaringType.IsInterface)
			{
				if (method.IsAbstract)
				{
					return "abstract ";
				}
				if ((method.Attributes & MethodAttributes.VtableLayoutMask) == 0)
				{
					return "override ";
				}
				return "virtual ";
			}
			return null;
		}

		private string GetTypeKind(Type t)
		{
			if (t.IsEnum)
			{
				return "enum";
			}
			if (t.IsClass)
			{
				if (t.IsSubclassOf(type_multicast_delegate))
				{
					return "delegate";
				}
				return "class";
			}
			if (t.IsInterface)
			{
				return "interface";
			}
			if (t.IsValueType)
			{
				return "struct";
			}
			return "class";
		}

		private static string GetTypeVisibility(Type t)
		{
			switch (t.Attributes & TypeAttributes.VisibilityMask)
			{
			case TypeAttributes.Public:
			case TypeAttributes.NestedPublic:
				return "public";
			case TypeAttributes.NestedFamily:
			case TypeAttributes.NestedFamANDAssem:
			case TypeAttributes.VisibilityMask:
				return "protected";
			default:
				return "internal";
			}
		}

		private string FormatGenericParams(Type[] args)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (args.Length == 0)
			{
				return "";
			}
			stringBuilder.Append("<");
			for (int i = 0; i < args.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(FormatType(args[i]));
			}
			stringBuilder.Append(">");
			return stringBuilder.ToString();
		}

		private string FormatType(Type t)
		{
			if (t == null)
			{
				return "";
			}
			string fullName = GetFullName(t);
			if (fullName == null)
			{
				return t.ToString();
			}
			if (!fullName.StartsWith("System."))
			{
				if (fullName.IndexOf(".") == -1)
				{
					return fullName;
				}
				if (t.GetNamespace() == this.t.GetNamespace())
				{
					return t.Name;
				}
				return fullName;
			}
			if (t.HasElementType)
			{
				Type elementType = t.GetElementType();
				if (t.IsArray)
				{
					return FormatType(elementType) + " []";
				}
				if (t.IsPointer)
				{
					return FormatType(elementType) + " *";
				}
				if (t.IsByRef)
				{
					return "ref " + FormatType(elementType);
				}
			}
			switch (fullName)
			{
			case "System.Byte":
				return "byte";
			case "System.SByte":
				return "sbyte";
			case "System.Int16":
				return "short";
			case "System.Int32":
				return "int";
			case "System.Int64":
				return "long";
			case "System.UInt16":
				return "ushort";
			case "System.UInt32":
				return "uint";
			case "System.UInt64":
				return "ulong";
			case "System.Single":
				return "float";
			case "System.Double":
				return "double";
			case "System.Decimal":
				return "decimal";
			case "System.Boolean":
				return "bool";
			case "System.Char":
				return "char";
			case "System.String":
				return "string";
			case "System.Object":
				return "object";
			case "System.Void":
				return "void";
			default:
				if (fullName.LastIndexOf(".") == 6)
				{
					return fullName.Substring(7);
				}
				if (this.t.Namespace.StartsWith(t.Namespace + ".") || t.Namespace == this.t.Namespace)
				{
					return fullName.Substring(t.Namespace.Length + 1);
				}
				return fullName;
			}
		}

		public static string RemoveGenericArity(string name)
		{
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder();
			while (num < name.Length)
			{
				int num2 = name.IndexOf('`', num);
				if (num2 < 0)
				{
					stringBuilder.Append(name.Substring(num));
					break;
				}
				stringBuilder.Append(name.Substring(num, num2 - num));
				for (num2++; num2 < name.Length && char.IsNumber(name[num2]); num2++)
				{
				}
				num = num2;
			}
			return stringBuilder.ToString();
		}

		private string GetTypeName(Type t)
		{
			StringBuilder stringBuilder = new StringBuilder();
			GetTypeName(stringBuilder, t);
			return stringBuilder.ToString();
		}

		private void GetTypeName(StringBuilder sb, Type t)
		{
			sb.Append(RemoveGenericArity(t.Name));
			sb.Append(FormatGenericParams(t.GetGenericArguments()));
		}

		private string GetFullName(Type t)
		{
			StringBuilder stringBuilder = new StringBuilder();
			GetFullName_recursed(stringBuilder, t, false);
			return stringBuilder.ToString();
		}

		private void GetFullName_recursed(StringBuilder sb, Type t, bool recursed)
		{
			if (t.IsGenericParameter)
			{
				sb.Append(t.Name);
				return;
			}
			if (t.DeclaringType != null)
			{
				GetFullName_recursed(sb, t.DeclaringType, true);
				sb.Append(".");
			}
			if (!recursed)
			{
				string text = t.GetNamespace();
				if (text != null && text != "")
				{
					sb.Append(text);
					sb.Append(".");
				}
			}
			GetTypeName(sb, t);
		}

		private void WriteGenericConstraints(Type[] args)
		{
			foreach (Type type in args)
			{
				bool flag = true;
				Type[] array = TypeGetInterfaces(type, true);
				GenericParameterAttributes genericParameterAttributes = type.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
				GenericParameterAttributes[] array2 = new GenericParameterAttributes[3]
				{
					GenericParameterAttributes.ReferenceTypeConstraint,
					GenericParameterAttributes.NotNullableValueTypeConstraint,
					GenericParameterAttributes.DefaultConstructorConstraint
				};
				if (type.BaseType != type_object || array.Length != 0 || genericParameterAttributes != GenericParameterAttributes.None)
				{
					o.Write(" where ");
					o.Write(FormatType(type));
					o.Write(" : ");
				}
				if (type.BaseType != type_object)
				{
					o.Write(FormatType(type.BaseType));
					flag = false;
				}
				Type[] array3 = array;
				foreach (Type type2 in array3)
				{
					if (!flag)
					{
						o.Write(", ");
					}
					flag = false;
					o.Write(FormatType(type2));
				}
				GenericParameterAttributes[] array4 = array2;
				foreach (GenericParameterAttributes genericParameterAttributes2 in array4)
				{
					if ((genericParameterAttributes & genericParameterAttributes2) != GenericParameterAttributes.None)
					{
						if (!flag)
						{
							o.Write(", ");
						}
						flag = false;
						switch (genericParameterAttributes2)
						{
						case GenericParameterAttributes.ReferenceTypeConstraint:
							o.Write("class");
							break;
						case GenericParameterAttributes.NotNullableValueTypeConstraint:
							o.Write("struct");
							break;
						case GenericParameterAttributes.DefaultConstructorConstraint:
							o.Write("new ()");
							break;
						}
					}
				}
			}
		}

		private string OperatorFromName(string name)
		{
			switch (name)
			{
			case "op_UnaryPlus":
				return "+";
			case "op_UnaryNegation":
				return "-";
			case "op_LogicalNot":
				return "!";
			case "op_OnesComplement":
				return "~";
			case "op_Increment":
				return "++";
			case "op_Decrement":
				return "--";
			case "op_True":
				return "true";
			case "op_False":
				return "false";
			case "op_Addition":
				return "+";
			case "op_Subtraction":
				return "-";
			case "op_Multiply":
				return "*";
			case "op_Division":
				return "/";
			case "op_Modulus":
				return "%";
			case "op_BitwiseAnd":
				return "&";
			case "op_BitwiseOr":
				return "|";
			case "op_ExclusiveOr":
				return "^";
			case "op_LeftShift":
				return "<<";
			case "op_RightShift":
				return ">>";
			case "op_Equality":
				return "==";
			case "op_Inequality":
				return "!=";
			case "op_GreaterThan":
				return ">";
			case "op_LessThan":
				return "<";
			case "op_GreaterThanOrEqual":
				return ">=";
			case "op_LessThanOrEqual":
				return "<=";
			default:
				return name;
			}
		}

		private bool MethodIsExplicitIfaceImpl(MethodBase mb)
		{
			if (!mb.IsFinal || !mb.IsVirtual || !mb.IsPrivate)
			{
				return false;
			}
			return true;
		}

		private bool ShowMember(MemberInfo mi)
		{
			if (mi.MemberType == MemberTypes.Constructor && ((MethodBase)mi).IsStatic)
			{
				return false;
			}
			if (show_private)
			{
				return true;
			}
			if (filter_obsolete && mi.IsDefined(type_obsolete_attribute, false))
			{
				return false;
			}
			switch (mi.MemberType)
			{
			case MemberTypes.Constructor:
			case MemberTypes.Method:
			{
				MethodBase methodBase = mi as MethodBase;
				if (methodBase.IsFamily || methodBase.IsPublic || methodBase.IsFamilyOrAssembly)
				{
					return true;
				}
				if (MethodIsExplicitIfaceImpl(methodBase))
				{
					return true;
				}
				return false;
			}
			case MemberTypes.Field:
			{
				FieldInfo fieldInfo = mi as FieldInfo;
				if (fieldInfo.IsFamily || fieldInfo.IsPublic || fieldInfo.IsFamilyOrAssembly)
				{
					return true;
				}
				return false;
			}
			case MemberTypes.TypeInfo:
			case MemberTypes.NestedType:
				switch ((mi as Type).Attributes & TypeAttributes.VisibilityMask)
				{
				case TypeAttributes.Public:
				case TypeAttributes.NestedPublic:
				case TypeAttributes.NestedFamily:
				case TypeAttributes.VisibilityMask:
					return true;
				default:
					return false;
				}
			default:
				return true;
			}
		}

		private static Type[] TypeGetInterfaces(Type t, bool declonly)
		{
			if (t.IsGenericParameter)
			{
				return new Type[0];
			}
			Type[] interfaces = t.GetInterfaces();
			if (!declonly)
			{
				return interfaces;
			}
			if (t.BaseType == null || interfaces.Length == 0)
			{
				return interfaces;
			}
			ArrayList arrayList = new ArrayList();
			Type[] array = interfaces;
			foreach (Type type in array)
			{
				if (!type.IsAssignableFrom(t.BaseType))
				{
					arrayList.Add(type);
				}
			}
			return (Type[])arrayList.ToArray(typeof(Type));
		}
	}
}
