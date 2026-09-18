using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public abstract class MetadataImporter
	{
		protected struct DynamicTypeReader
		{
			private static readonly bool[] single_attribute = new bool[1] { true };

			public int Position;

			private bool[] flags;

			private object provider;

			public DynamicTypeReader(object provider)
			{
				Position = 0;
				flags = null;
				this.provider = provider;
			}

			public bool IsDynamicObject()
			{
				if (provider != null)
				{
					ReadAttribute();
				}
				if (flags != null && Position < flags.Length)
				{
					return flags[Position];
				}
				return false;
			}

			public bool HasDynamicAttribute()
			{
				if (provider != null)
				{
					ReadAttribute();
				}
				return flags != null;
			}

			private IList<CustomAttributeData> GetCustomAttributes()
			{
				MemberInfo memberInfo = provider as MemberInfo;
				if (memberInfo != null)
				{
					return CustomAttributeData.GetCustomAttributes(memberInfo);
				}
				ParameterInfo parameterInfo = provider as ParameterInfo;
				if (parameterInfo != null)
				{
					return CustomAttributeData.GetCustomAttributes(parameterInfo);
				}
				provider = null;
				return null;
			}

			private void ReadAttribute()
			{
				IList<CustomAttributeData> customAttributes = GetCustomAttributes();
				if (customAttributes == null)
				{
					return;
				}
				if (customAttributes.Count > 0)
				{
					foreach (CustomAttributeData item in customAttributes)
					{
						Type declaringType = item.Constructor.DeclaringType;
						if (declaringType.Name != "DynamicAttribute" || declaringType.Namespace != CompilerServicesNamespace)
						{
							continue;
						}
						if (item.ConstructorArguments.Count == 0)
						{
							flags = single_attribute;
							break;
						}
						Type argumentType = item.ConstructorArguments[0].ArgumentType;
						if (!argumentType.IsArray || Type.GetTypeCode(argumentType.GetElementType()) != TypeCode.Boolean)
						{
							continue;
						}
						IList<CustomAttributeTypedArgument> list = (IList<CustomAttributeTypedArgument>)item.ConstructorArguments[0].Value;
						flags = new bool[list.Count];
						for (int i = 0; i < flags.Length; i++)
						{
							if (Type.GetTypeCode(list[i].ArgumentType) == TypeCode.Boolean)
							{
								flags[i] = (bool)list[i].Value;
							}
						}
						break;
					}
				}
				provider = null;
			}
		}

		protected readonly Dictionary<Type, TypeSpec> import_cache;

		protected readonly Dictionary<Type, TypeSpec> compiled_types;

		protected readonly Dictionary<Assembly, IAssemblyDefinition> assembly_2_definition;

		protected readonly ModuleContainer module;

		public static readonly string CompilerServicesNamespace = "System.Runtime.CompilerServices";

		public ICollection<IAssemblyDefinition> Assemblies => assembly_2_definition.Values;

		public bool IgnorePrivateMembers { get; set; }

		protected MetadataImporter(ModuleContainer module)
		{
			this.module = module;
			import_cache = new Dictionary<Type, TypeSpec>(1024, ReferenceEquality<Type>.Default);
			compiled_types = new Dictionary<Type, TypeSpec>(40, ReferenceEquality<Type>.Default);
			assembly_2_definition = new Dictionary<Assembly, IAssemblyDefinition>(ReferenceEquality<Assembly>.Default);
			IgnorePrivateMembers = true;
		}

		public abstract void AddCompiledType(TypeBuilder builder, TypeSpec spec);

		protected abstract MemberKind DetermineKindFromBaseType(Type baseType);

		protected abstract bool HasVolatileModifier(Type[] modifiers);

		public FieldSpec CreateField(FieldInfo fi, TypeSpec declaringType)
		{
			FieldAttributes attributes = fi.Attributes;
			Modifiers modifiers;
			switch (attributes & FieldAttributes.FieldAccessMask)
			{
			case FieldAttributes.Public:
				modifiers = Modifiers.PUBLIC;
				break;
			case FieldAttributes.Assembly:
				modifiers = Modifiers.INTERNAL;
				break;
			case FieldAttributes.Family:
				modifiers = Modifiers.PROTECTED;
				break;
			case FieldAttributes.FamORAssem:
				modifiers = Modifiers.PROTECTED | Modifiers.INTERNAL;
				break;
			default:
				if ((IgnorePrivateMembers && !declaringType.IsStruct) || HasAttribute(CustomAttributeData.GetCustomAttributes(fi), "CompilerGeneratedAttribute", CompilerServicesNamespace))
				{
					return null;
				}
				modifiers = Modifiers.PRIVATE;
				break;
			}
			TypeSpec typeSpec;
			try
			{
				typeSpec = ImportType(fi.FieldType, new DynamicTypeReader(fi));
				if (typeSpec == null)
				{
					return null;
				}
			}
			catch (Exception exception)
			{
				throw new InternalErrorException(exception, "Cannot import field `{0}.{1}' referenced in assembly `{2}'", declaringType.GetSignatureForError(), fi.Name, declaringType.MemberDefinition.DeclaringAssembly);
			}
			ImportedMemberDefinition definition = new ImportedMemberDefinition(fi, typeSpec, this);
			if ((attributes & FieldAttributes.Literal) != FieldAttributes.PrivateScope)
			{
				Constant value = ((typeSpec.Kind == MemberKind.MissingType) ? new NullConstant(InternalType.ErrorType, Location.Null) : CreateConstantFromValue(typeSpec, fi));
				return new ConstSpec(declaringType, definition, typeSpec, fi, modifiers | Modifiers.STATIC, value);
			}
			if ((attributes & FieldAttributes.InitOnly) != FieldAttributes.PrivateScope)
			{
				if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Decimal)
				{
					Constant constant = ReadDecimalConstant(CustomAttributeData.GetCustomAttributes(fi));
					if (constant != null)
					{
						return new ConstSpec(declaringType, definition, typeSpec, fi, modifiers | Modifiers.STATIC, constant);
					}
				}
				modifiers |= Modifiers.READONLY;
			}
			else
			{
				Type[] requiredCustomModifiers = fi.GetRequiredCustomModifiers();
				if (requiredCustomModifiers.Length != 0 && HasVolatileModifier(requiredCustomModifiers))
				{
					modifiers |= Modifiers.VOLATILE;
				}
			}
			if ((attributes & FieldAttributes.Static) != FieldAttributes.PrivateScope)
			{
				modifiers |= Modifiers.STATIC;
			}
			else if (declaringType.IsStruct && typeSpec.IsStruct && typeSpec.IsNested && HasAttribute(CustomAttributeData.GetCustomAttributes(fi), "FixedBufferAttribute", CompilerServicesNamespace))
			{
				FieldSpec element = CreateField(fi.FieldType.GetField("FixedElementField"), declaringType);
				return new FixedFieldSpec(module, declaringType, definition, fi, element, modifiers);
			}
			return new FieldSpec(declaringType, definition, typeSpec, fi, modifiers);
		}

		private Constant CreateConstantFromValue(TypeSpec fieldType, FieldInfo fi)
		{
			object rawConstantValue = fi.GetRawConstantValue();
			if (rawConstantValue != null && !fieldType.IsEnum)
			{
				Constant constant = ImportConstant(rawConstantValue);
				if (constant != null)
				{
					if (fieldType != constant.Type)
					{
						return constant.ConvertExplicitly(false, fieldType);
					}
					return constant;
				}
			}
			return Constant.CreateConstantFromValue(fieldType, rawConstantValue, Location.Null);
		}

		public EventSpec CreateEvent(EventInfo ei, TypeSpec declaringType, MethodSpec add, MethodSpec remove)
		{
			add.IsAccessor = true;
			remove.IsAccessor = true;
			if (add.Modifiers != remove.Modifiers)
			{
				throw new NotImplementedException("Different accessor modifiers " + ei.Name);
			}
			TypeSpec typeSpec = ImportType(ei.EventHandlerType, new DynamicTypeReader(ei));
			ImportedMemberDefinition definition = new ImportedMemberDefinition(ei, typeSpec, this);
			return new EventSpec(declaringType, definition, typeSpec, add.Modifiers, add, remove);
		}

		private TypeParameterSpec[] CreateGenericParameters(Type type, TypeSpec declaringType)
		{
			Type[] genericArguments = type.GetGenericArguments();
			int num;
			if (type.IsNested)
			{
				num = type.DeclaringType.GetGenericArguments().Length;
				if (declaringType != null && num > 0)
				{
					int num2 = 0;
					while (num2 != num)
					{
						int arity = declaringType.Arity;
						if (arity != 0)
						{
							TypeParameterSpec[] typeParameters = declaringType.MemberDefinition.TypeParameters;
							num2 += arity;
							for (int i = 0; i < arity; i++)
							{
								import_cache.Add(genericArguments[num - num2 + i], typeParameters[i]);
							}
						}
						declaringType = declaringType.DeclaringType;
					}
				}
			}
			else
			{
				num = 0;
			}
			if (genericArguments.Length - num == 0)
			{
				return null;
			}
			return CreateGenericParameters(num, genericArguments);
		}

		private TypeParameterSpec[] CreateGenericParameters(int first, Type[] tparams)
		{
			TypeParameterSpec[] array = new TypeParameterSpec[tparams.Length - first];
			for (int i = first; i < tparams.Length; i++)
			{
				Type type = tparams[i];
				int num = i - first;
				array[num] = (TypeParameterSpec)CreateType(type, default(DynamicTypeReader), false);
			}
			return array;
		}

		private TypeSpec[] CreateGenericArguments(int first, Type[] tparams, DynamicTypeReader dtype)
		{
			dtype.Position++;
			TypeSpec[] array = new TypeSpec[tparams.Length - first];
			for (int i = first; i < tparams.Length; i++)
			{
				Type type = tparams[i];
				int num = i - first;
				TypeSpec element;
				if (type.HasElementType)
				{
					Type elementType = type.GetElementType();
					dtype.Position++;
					element = ImportType(elementType, dtype);
					if (!type.IsArray)
					{
						throw new NotImplementedException("Unknown element type " + type.ToString());
					}
					element = ArrayContainer.MakeType(module, element, type.GetArrayRank());
				}
				else
				{
					element = CreateType(type, dtype, true);
					if (!IsMissingType(type) && type.IsGenericTypeDefinition)
					{
						int first2 = ((element.DeclaringType != null) ? element.DeclaringType.MemberDefinition.TypeParametersCount : 0);
						TypeSpec[] targs = CreateGenericArguments(first2, type.GetGenericArguments(), dtype);
						element = element.MakeGenericType(module, targs);
					}
				}
				if (element == null)
				{
					return null;
				}
				dtype.Position++;
				array[num] = element;
			}
			return array;
		}

		public MethodSpec CreateMethod(MethodBase mb, TypeSpec declaringType)
		{
			Modifiers modifiers = ReadMethodModifiers(mb, declaringType);
			AParametersCollection aParametersCollection = CreateParameters(declaringType, mb.GetParameters(), mb);
			TypeParameterSpec[] array;
			if (mb.IsGenericMethod)
			{
				if (!mb.IsGenericMethodDefinition)
				{
					throw new NotSupportedException("assert");
				}
				array = CreateGenericParameters(0, mb.GetGenericArguments());
			}
			else
			{
				array = null;
			}
			MemberKind memberKind;
			TypeSpec typeSpec;
			if (mb.MemberType == MemberTypes.Constructor)
			{
				memberKind = MemberKind.Constructor;
				typeSpec = module.Compiler.BuiltinTypes.Void;
			}
			else
			{
				string name = mb.Name;
				memberKind = MemberKind.Method;
				if (array == null && !mb.DeclaringType.IsInterface && name.Length > 6)
				{
					if ((modifiers & (Modifiers.PUBLIC | Modifiers.STATIC)) == (Modifiers.PUBLIC | Modifiers.STATIC))
					{
						if (name[2] == '_' && name[1] == 'p' && name[0] == 'o' && (mb.Attributes & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope && Operator.GetType(name).HasValue && aParametersCollection.Count > 0 && aParametersCollection.Count < 3)
						{
							memberKind = MemberKind.Operator;
						}
					}
					else if (aParametersCollection.IsEmpty && name == Destructor.MetadataName)
					{
						memberKind = MemberKind.Destructor;
						if (declaringType.BuiltinType == BuiltinTypeSpec.Type.Object)
						{
							modifiers &= ~Modifiers.OVERRIDE;
							modifiers |= Modifiers.VIRTUAL;
						}
					}
				}
				MethodInfo methodInfo = (MethodInfo)mb;
				typeSpec = ImportType(methodInfo.ReturnType, new DynamicTypeReader(methodInfo.ReturnParameter));
				if ((modifiers & Modifiers.OVERRIDE) != 0)
				{
					bool flag = false;
					if (memberKind == MemberKind.Method && declaringType.BaseType != null)
					{
						TypeSpec baseType = declaringType.BaseType;
						if (IsOverrideMethodBaseTypeAccessible(baseType))
						{
							MemberFilter filter = MemberFilter.Method(name, (array != null) ? array.Length : 0, aParametersCollection, null);
							MemberSpec memberSpec = MemberCache.FindMember(baseType, filter, BindingRestriction.None);
							if (memberSpec != null && (memberSpec.Modifiers & (Modifiers.PROTECTED | Modifiers.PUBLIC | Modifiers.PRIVATE)) == (modifiers & (Modifiers.PROTECTED | Modifiers.PUBLIC | Modifiers.PRIVATE)) && !memberSpec.IsStatic)
							{
								flag = true;
							}
						}
					}
					if (!flag)
					{
						modifiers &= ~Modifiers.OVERRIDE;
						modifiers = (((modifiers & Modifiers.SEALED) == 0) ? (modifiers | Modifiers.VIRTUAL) : (modifiers & ~Modifiers.SEALED));
					}
				}
				else if (aParametersCollection.HasExtensionMethodType)
				{
					modifiers |= Modifiers.METHOD_EXTENSION;
				}
			}
			IMethodDefinition details;
			if (array != null)
			{
				ImportedGenericMethodDefinition importedGenericMethodDefinition = new ImportedGenericMethodDefinition((MethodInfo)mb, typeSpec, aParametersCollection, array, this);
				TypeParameterSpec[] typeParameters = importedGenericMethodDefinition.TypeParameters;
				foreach (TypeParameterSpec typeParameterSpec in typeParameters)
				{
					ImportTypeParameterTypeConstraints(typeParameterSpec, typeParameterSpec.GetMetaInfo());
				}
				details = importedGenericMethodDefinition;
			}
			else
			{
				details = new ImportedMethodDefinition(mb, typeSpec, aParametersCollection, this);
			}
			MethodSpec methodSpec = new MethodSpec(memberKind, declaringType, details, typeSpec, aParametersCollection, modifiers);
			if (array != null)
			{
				methodSpec.IsGeneric = true;
			}
			return methodSpec;
		}

		private bool IsOverrideMethodBaseTypeAccessible(TypeSpec baseType)
		{
			switch (baseType.Modifiers & Modifiers.AccessibilityMask)
			{
			case Modifiers.PUBLIC:
				return true;
			case Modifiers.INTERNAL:
				return baseType.MemberDefinition.IsInternalAsPublic(module.DeclaringAssembly);
			case Modifiers.PRIVATE:
				return false;
			default:
				return true;
			}
		}

		private AParametersCollection CreateParameters(TypeSpec parent, ParameterInfo[] pi, MethodBase method)
		{
			int num = ((method != null && (method.CallingConvention & CallingConventions.VarArgs) != 0) ? 1 : 0);
			if (pi.Length == 0 && num == 0)
			{
				return ParametersCompiled.EmptyReadOnlyParameters;
			}
			TypeSpec[] array = new TypeSpec[pi.Length + num];
			IParameterData[] array2 = new IParameterData[pi.Length + num];
			bool flag = false;
			for (int i = 0; i < pi.Length; i++)
			{
				ParameterInfo parameterInfo = pi[i];
				Parameter.Modifier modifier = Parameter.Modifier.NONE;
				Expression expression = null;
				if (parameterInfo.ParameterType.IsByRef)
				{
					modifier = (((parameterInfo.Attributes & (ParameterAttributes.In | ParameterAttributes.Out)) != ParameterAttributes.Out) ? Parameter.Modifier.REF : Parameter.Modifier.OUT);
					Type elementType = parameterInfo.ParameterType.GetElementType();
					array[i] = ImportType(elementType, new DynamicTypeReader(parameterInfo));
				}
				else if (i == 0 && method.IsStatic && (parent.Modifiers & Modifiers.METHOD_EXTENSION) != 0 && HasAttribute(CustomAttributeData.GetCustomAttributes(method), "ExtensionAttribute", CompilerServicesNamespace))
				{
					modifier = Parameter.Modifier.This;
					array[i] = ImportType(parameterInfo.ParameterType, new DynamicTypeReader(parameterInfo));
				}
				else
				{
					array[i] = ImportType(parameterInfo.ParameterType, new DynamicTypeReader(parameterInfo));
					if (i >= pi.Length - 2 && array[i] is ArrayContainer && HasAttribute(CustomAttributeData.GetCustomAttributes(parameterInfo), "ParamArrayAttribute", "System"))
					{
						modifier = Parameter.Modifier.PARAMS;
						flag = true;
					}
					if (!flag && parameterInfo.IsOptional)
					{
						object rawDefaultValue = parameterInfo.RawDefaultValue;
						TypeSpec typeSpec = array[i];
						if ((parameterInfo.Attributes & ParameterAttributes.HasDefault) != ParameterAttributes.None && typeSpec.Kind != MemberKind.TypeParameter && (rawDefaultValue != null || TypeSpec.IsReferenceType(typeSpec)))
						{
							if (rawDefaultValue == null)
							{
								expression = Constant.CreateConstantFromValue(typeSpec, null, Location.Null);
							}
							else
							{
								expression = ImportConstant(rawDefaultValue);
								if (typeSpec.IsEnum)
								{
									expression = new EnumConstant((Constant)expression, typeSpec);
								}
							}
							IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(parameterInfo);
							for (int j = 0; j < customAttributes.Count; j++)
							{
								Type declaringType = customAttributes[j].Constructor.DeclaringType;
								if (!(declaringType.Namespace != CompilerServicesNamespace))
								{
									if (declaringType.Name == "CallerLineNumberAttribute" && (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Int || Convert.ImplicitNumericConversionExists(module.Compiler.BuiltinTypes.Int, typeSpec)))
									{
										modifier |= Parameter.Modifier.CallerLineNumber;
									}
									else if (declaringType.Name == "CallerFilePathAttribute" && Convert.ImplicitReferenceConversionExists(module.Compiler.BuiltinTypes.String, typeSpec))
									{
										modifier |= Parameter.Modifier.CallerFilePath;
									}
									else if (declaringType.Name == "CallerMemberNameAttribute" && Convert.ImplicitReferenceConversionExists(module.Compiler.BuiltinTypes.String, typeSpec))
									{
										modifier |= Parameter.Modifier.CallerMemberName;
									}
								}
							}
						}
						else if (rawDefaultValue == Missing.Value)
						{
							expression = EmptyExpression.MissingValue;
						}
						else if (rawDefaultValue == null)
						{
							expression = new DefaultValueExpression(new TypeExpression(typeSpec, Location.Null), Location.Null);
						}
						else if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Decimal)
						{
							expression = ImportConstant(rawDefaultValue);
						}
					}
				}
				array2[i] = new ParameterData(parameterInfo.Name, modifier, expression);
			}
			if (num != 0)
			{
				array2[array2.Length - 1] = new ArglistParameter(Location.Null);
				array[array.Length - 1] = InternalType.Arglist;
			}
			if (method == null)
			{
				return new ParametersImported(array2, array, flag);
			}
			return new ParametersImported(array2, array, num != 0, flag);
		}

		public PropertySpec CreateProperty(PropertyInfo pi, TypeSpec declaringType, MethodSpec get, MethodSpec set)
		{
			Modifiers modifiers = (Modifiers)0;
			AParametersCollection aParametersCollection = null;
			TypeSpec typeSpec = null;
			if (get != null)
			{
				modifiers = get.Modifiers;
				aParametersCollection = get.Parameters;
				typeSpec = get.ReturnType;
			}
			bool flag = true;
			if (set != null)
			{
				if (set.ReturnType.Kind != MemberKind.Void)
				{
					flag = false;
				}
				int num = set.Parameters.Count - 1;
				if (num < 0)
				{
					num = 0;
					flag = false;
				}
				TypeSpec typeSpec2 = set.Parameters.Types[num];
				if (modifiers == (Modifiers)0)
				{
					AParametersCollection aParametersCollection2;
					if (num == 0)
					{
						aParametersCollection2 = ParametersCompiled.EmptyReadOnlyParameters;
					}
					else
					{
						IParameterData[] array = new IParameterData[num];
						TypeSpec[] array2 = new TypeSpec[num];
						Array.Copy(set.Parameters.FixedParameters, array, num);
						Array.Copy(set.Parameters.Types, array2, num);
						aParametersCollection2 = new ParametersImported(array, array2, set.Parameters.HasParams);
					}
					modifiers = set.Modifiers;
					aParametersCollection = aParametersCollection2;
					typeSpec = typeSpec2;
				}
				else
				{
					if (num != get.Parameters.Count)
					{
						flag = false;
					}
					if (get.ReturnType != typeSpec2)
					{
						flag = false;
					}
					if ((modifiers & Modifiers.AccessibilityMask) != (set.Modifiers & Modifiers.AccessibilityMask))
					{
						Modifiers modifiers2 = modifiers & Modifiers.AccessibilityMask;
						if (modifiers2 != Modifiers.PUBLIC)
						{
							Modifiers modifiers3 = set.Modifiers & Modifiers.AccessibilityMask;
							if (modifiers2 != modifiers3)
							{
								bool num2 = ModifiersExtensions.IsRestrictedModifier(modifiers2, modifiers3);
								bool flag2 = ModifiersExtensions.IsRestrictedModifier(modifiers3, modifiers2);
								if (num2 & flag2)
								{
									flag = false;
								}
								if (num2)
								{
									modifiers &= ~Modifiers.AccessibilityMask;
									modifiers |= modifiers3;
								}
							}
						}
					}
				}
			}
			PropertySpec propertySpec = null;
			if (!aParametersCollection.IsEmpty && flag)
			{
				string attributeDefaultMember = declaringType.MemberDefinition.GetAttributeDefaultMember();
				if (attributeDefaultMember == null)
				{
					flag = false;
				}
				else
				{
					if (get != null)
					{
						if (get.IsStatic)
						{
							flag = false;
						}
						if (get.Name.IndexOf(attributeDefaultMember, StringComparison.Ordinal) != 4)
						{
							flag = false;
						}
					}
					if (set != null)
					{
						if (set.IsStatic)
						{
							flag = false;
						}
						if (set.Name.IndexOf(attributeDefaultMember, StringComparison.Ordinal) != 4)
						{
							flag = false;
						}
					}
				}
				if (flag)
				{
					propertySpec = new IndexerSpec(declaringType, new ImportedParameterMemberDefinition(pi, typeSpec, aParametersCollection, this), typeSpec, aParametersCollection, pi, modifiers);
				}
				else if (declaringType.MemberDefinition.IsComImport && aParametersCollection.FixedParameters[0].HasDefaultValue)
				{
					flag = true;
					for (int i = 0; i < aParametersCollection.FixedParameters.Length; i++)
					{
						if (!aParametersCollection.FixedParameters[i].HasDefaultValue)
						{
							flag = false;
							break;
						}
					}
				}
			}
			if (propertySpec == null)
			{
				propertySpec = new PropertySpec(MemberKind.Property, declaringType, new ImportedMemberDefinition(pi, typeSpec, this), typeSpec, pi, modifiers);
			}
			if (!flag)
			{
				propertySpec.IsNotCSharpCompatible = true;
				return propertySpec;
			}
			if (set != null)
			{
				propertySpec.Set = set;
			}
			if (get != null)
			{
				propertySpec.Get = get;
			}
			return propertySpec;
		}

		public TypeSpec CreateType(Type type)
		{
			return CreateType(type, default(DynamicTypeReader), true);
		}

		public TypeSpec CreateNestedType(Type type, TypeSpec declaringType)
		{
			return CreateType(type, declaringType, new DynamicTypeReader(type), false);
		}

		private TypeSpec CreateType(Type type, DynamicTypeReader dtype, bool canImportBaseType)
		{
			TypeSpec declaringType = ((!type.IsNested || type.IsGenericParameter) ? null : CreateType(type.DeclaringType, new DynamicTypeReader(type.DeclaringType), true));
			return CreateType(type, declaringType, dtype, canImportBaseType);
		}

		protected TypeSpec CreateType(Type type, TypeSpec declaringType, DynamicTypeReader dtype, bool canImportBaseType)
		{
			TypeSpec value;
			if (import_cache.TryGetValue(type, out value))
			{
				if (value.BuiltinType == BuiltinTypeSpec.Type.Object)
				{
					if (dtype.IsDynamicObject())
					{
						return module.Compiler.BuiltinTypes.Dynamic;
					}
					return value;
				}
				if (!value.IsGeneric || type.IsGenericTypeDefinition)
				{
					return value;
				}
				if (!dtype.HasDynamicAttribute())
				{
					return value;
				}
			}
			if (IsMissingType(type))
			{
				value = new TypeSpec(MemberKind.MissingType, declaringType, new ImportedTypeDefinition(type, this), type, Modifiers.PUBLIC);
				value.MemberCache = MemberCache.Empty;
				import_cache.Add(type, value);
				return value;
			}
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				Type genericTypeDefinition = type.GetGenericTypeDefinition();
				if (compiled_types.TryGetValue(genericTypeDefinition, out value))
				{
					return value;
				}
				TypeSpec[] array = CreateGenericArguments(0, type.GetGenericArguments(), dtype);
				if (array == null)
				{
					return null;
				}
				if (declaringType == null)
				{
					value = CreateType(genericTypeDefinition, null, default(DynamicTypeReader), canImportBaseType);
					value = value.MakeGenericType(module, array);
				}
				else
				{
					List<TypeSpec> list = new List<TypeSpec>();
					while (declaringType.IsNested)
					{
						list.Add(declaringType);
						declaringType = declaringType.DeclaringType;
					}
					int num = 0;
					if (declaringType.Arity > 0)
					{
						value = declaringType.MakeGenericType(module, array.Skip(num).Take(declaringType.Arity).ToArray());
						num = value.Arity;
					}
					else
					{
						value = declaringType;
					}
					for (int num2 = list.Count; num2 != 0; num2--)
					{
						TypeSpec typeSpec = list[num2 - 1];
						value = ((typeSpec.Kind != MemberKind.MissingType) ? MemberCache.FindNestedType(value, typeSpec.Name, typeSpec.Arity) : typeSpec);
						if (typeSpec.Arity > 0)
						{
							value = value.MakeGenericType(module, array.Skip(num).Take(value.Arity).ToArray());
							num += typeSpec.Arity;
						}
					}
					if (value.Kind == MemberKind.MissingType)
					{
						value = new TypeSpec(MemberKind.MissingType, value, new ImportedTypeDefinition(genericTypeDefinition, this), genericTypeDefinition, Modifiers.PUBLIC);
						value.MemberCache = MemberCache.Empty;
					}
					else
					{
						if ((genericTypeDefinition.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate && IgnorePrivateMembers)
						{
							return null;
						}
						string text = type.Name;
						int num3 = text.IndexOf('`');
						if (num3 > 0)
						{
							text = text.Substring(0, num3);
						}
						value = MemberCache.FindNestedType(value, text, array.Length - num);
						if (value.Arity > 0)
						{
							value = value.MakeGenericType(module, array.Skip(num).ToArray());
						}
					}
				}
				if (!value.HasDynamicElement && !import_cache.ContainsKey(type))
				{
					import_cache.Add(type, value);
				}
				return value;
			}
			TypeAttributes attributes = type.Attributes;
			Modifiers modifiers;
			switch (attributes & TypeAttributes.VisibilityMask)
			{
			case TypeAttributes.Public:
			case TypeAttributes.NestedPublic:
				modifiers = Modifiers.PUBLIC;
				break;
			case TypeAttributes.NestedPrivate:
				modifiers = Modifiers.PRIVATE;
				break;
			case TypeAttributes.NestedFamily:
				modifiers = Modifiers.PROTECTED;
				break;
			case TypeAttributes.VisibilityMask:
				modifiers = Modifiers.PROTECTED | Modifiers.INTERNAL;
				break;
			default:
				modifiers = Modifiers.INTERNAL;
				break;
			}
			MemberKind memberKind;
			if ((attributes & TypeAttributes.ClassSemanticsMask) != TypeAttributes.NotPublic)
			{
				memberKind = MemberKind.Interface;
			}
			else if (type.IsGenericParameter)
			{
				memberKind = MemberKind.TypeParameter;
			}
			else
			{
				Type baseType = type.BaseType;
				if (baseType == null || (attributes & TypeAttributes.Abstract) != TypeAttributes.NotPublic)
				{
					memberKind = MemberKind.Class;
				}
				else
				{
					memberKind = DetermineKindFromBaseType(baseType);
					if (memberKind == MemberKind.Struct || memberKind == MemberKind.Delegate)
					{
						modifiers |= Modifiers.SEALED;
					}
				}
				if (memberKind == MemberKind.Class)
				{
					if ((attributes & TypeAttributes.Sealed) != TypeAttributes.NotPublic)
					{
						modifiers = (((attributes & TypeAttributes.Abstract) == 0) ? (modifiers | Modifiers.SEALED) : (modifiers | Modifiers.STATIC));
					}
					else if ((attributes & TypeAttributes.Abstract) != TypeAttributes.NotPublic)
					{
						modifiers |= Modifiers.ABSTRACT;
					}
				}
			}
			ImportedTypeDefinition importedTypeDefinition = new ImportedTypeDefinition(type, this);
			switch (memberKind)
			{
			case MemberKind.Enum:
			{
				FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				int num4 = 0;
				if (num4 < fields.Length)
				{
					FieldInfo fieldInfo = fields[num4];
					value = new EnumSpec(declaringType, importedTypeDefinition, CreateType(fieldInfo.FieldType), type, modifiers);
				}
				if (value == null)
				{
					memberKind = MemberKind.Class;
				}
				break;
			}
			case MemberKind.TypeParameter:
				value = CreateTypeParameter(type, declaringType);
				break;
			default:
			{
				TypeSpec value2;
				if (type.IsGenericTypeDefinition)
				{
					importedTypeDefinition.TypeParameters = CreateGenericParameters(type, declaringType);
				}
				else if (compiled_types.TryGetValue(type, out value2))
				{
					value = value2;
					(value2 as BuiltinTypeSpec)?.SetDefinition(importedTypeDefinition, type, modifiers);
				}
				break;
			}
			}
			if (value == null)
			{
				value = new TypeSpec(memberKind, declaringType, importedTypeDefinition, type, modifiers);
			}
			import_cache.Add(type, value);
			if (memberKind == MemberKind.TypeParameter)
			{
				if (canImportBaseType)
				{
					ImportTypeParameterTypeConstraints((TypeParameterSpec)value, type);
				}
				return value;
			}
			if (canImportBaseType)
			{
				ImportTypeBase(value, type);
			}
			return value;
		}

		public IAssemblyDefinition GetAssemblyDefinition(Assembly assembly)
		{
			IAssemblyDefinition value;
			if (!assembly_2_definition.TryGetValue(assembly, out value))
			{
				ImportedAssemblyDefinition importedAssemblyDefinition = new ImportedAssemblyDefinition(assembly);
				assembly_2_definition.Add(assembly, importedAssemblyDefinition);
				importedAssemblyDefinition.ReadAttributes();
				return importedAssemblyDefinition;
			}
			return value;
		}

		public void ImportTypeBase(Type type)
		{
			TypeSpec typeSpec = import_cache[type];
			if (typeSpec != null)
			{
				ImportTypeBase(typeSpec, type);
			}
		}

		private TypeParameterSpec CreateTypeParameter(Type type, TypeSpec declaringType)
		{
			Variance variance;
			switch (type.GenericParameterAttributes & GenericParameterAttributes.VarianceMask)
			{
			case GenericParameterAttributes.Covariant:
				variance = Variance.Covariant;
				break;
			case GenericParameterAttributes.Contravariant:
				variance = Variance.Contravariant;
				break;
			default:
				variance = Variance.None;
				break;
			}
			SpecialConstraint specialConstraint = SpecialConstraint.None;
			GenericParameterAttributes genericParameterAttributes = type.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
			if ((genericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != GenericParameterAttributes.None)
			{
				specialConstraint |= SpecialConstraint.Struct;
			}
			else if ((genericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != GenericParameterAttributes.None)
			{
				specialConstraint = SpecialConstraint.Constructor;
			}
			if ((genericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != GenericParameterAttributes.None)
			{
				specialConstraint |= SpecialConstraint.Class;
			}
			ImportedTypeParameterDefinition definition = new ImportedTypeParameterDefinition(type, this);
			if (type.DeclaringMethod != null)
			{
				return new TypeParameterSpec(type.GenericParameterPosition, definition, specialConstraint, variance, type);
			}
			return new TypeParameterSpec(declaringType, type.GenericParameterPosition, definition, specialConstraint, variance, type);
		}

		public static bool HasAttribute(IList<CustomAttributeData> attributesData, string attrName, string attrNamespace)
		{
			if (attributesData.Count == 0)
			{
				return false;
			}
			foreach (CustomAttributeData attributesDatum in attributesData)
			{
				Type declaringType = attributesDatum.Constructor.DeclaringType;
				if (declaringType.Name == attrName && declaringType.Namespace == attrNamespace)
				{
					return true;
				}
			}
			return false;
		}

		private void ImportTypeBase(TypeSpec spec, Type type)
		{
			if (spec.Kind == MemberKind.Interface)
			{
				spec.BaseType = module.Compiler.BuiltinTypes.Object;
			}
			else if (type.BaseType != null)
			{
				TypeSpec baseType = ((IsMissingType(type.BaseType) || !type.BaseType.IsGenericType) ? CreateType(type.BaseType) : CreateType(type.BaseType, new DynamicTypeReader(type), true));
				spec.BaseType = baseType;
			}
			if (spec.MemberDefinition.TypeParametersCount > 0)
			{
				TypeParameterSpec[] typeParameters = spec.MemberDefinition.TypeParameters;
				foreach (TypeParameterSpec typeParameterSpec in typeParameters)
				{
					ImportTypeParameterTypeConstraints(typeParameterSpec, typeParameterSpec.GetMetaInfo());
				}
			}
		}

		protected void ImportTypes(Type[] types, Namespace targetNamespace, bool importExtensionTypes)
		{
			Namespace obj = targetNamespace;
			string text = null;
			foreach (Type type in types)
			{
				if (type == null || type.MemberType == MemberTypes.NestedType || type.Name[0] == '<')
				{
					continue;
				}
				TypeSpec typeSpec = CreateType(type, null, new DynamicTypeReader(type), true);
				if (typeSpec != null)
				{
					if (text != type.Namespace)
					{
						obj = ((type.Namespace == null) ? targetNamespace : targetNamespace.GetNamespace(type.Namespace, true));
						text = type.Namespace;
					}
					if (((typeSpec.IsClass && typeSpec.Arity == 0) & importExtensionTypes) && HasAttribute(CustomAttributeData.GetCustomAttributes(type), "ExtensionAttribute", CompilerServicesNamespace))
					{
						typeSpec.SetExtensionMethodContainer();
					}
					obj.AddType(module, typeSpec);
				}
			}
		}

		private void ImportTypeParameterTypeConstraints(TypeParameterSpec spec, Type type)
		{
			Type[] genericParameterConstraints = type.GetGenericParameterConstraints();
			List<TypeSpec> list = null;
			Type[] array = genericParameterConstraints;
			foreach (Type type2 in array)
			{
				if (type2.IsGenericParameter)
				{
					if (list == null)
					{
						list = new List<TypeSpec>();
					}
					list.Add(CreateType(type2));
				}
				else
				{
					TypeSpec typeSpec = CreateType(type2);
					if (typeSpec.IsClass)
					{
						spec.BaseType = typeSpec;
					}
					else
					{
						spec.AddInterface(typeSpec);
					}
				}
			}
			if (spec.BaseType == null)
			{
				spec.BaseType = module.Compiler.BuiltinTypes.Object;
			}
			if (list != null)
			{
				spec.TypeArguments = list.ToArray();
			}
		}

		private Constant ImportConstant(object value)
		{
			BuiltinTypes builtinTypes = module.Compiler.BuiltinTypes;
			switch (Type.GetTypeCode(value.GetType()))
			{
			case TypeCode.Boolean:
				return new BoolConstant(builtinTypes, (bool)value, Location.Null);
			case TypeCode.Byte:
				return new ByteConstant(builtinTypes, (byte)value, Location.Null);
			case TypeCode.Char:
				return new CharConstant(builtinTypes, (char)value, Location.Null);
			case TypeCode.Decimal:
				return new DecimalConstant(builtinTypes, (decimal)value, Location.Null);
			case TypeCode.Double:
				return new DoubleConstant(builtinTypes, (double)value, Location.Null);
			case TypeCode.Int16:
				return new ShortConstant(builtinTypes, (short)value, Location.Null);
			case TypeCode.Int32:
				return new IntConstant(builtinTypes, (int)value, Location.Null);
			case TypeCode.Int64:
				return new LongConstant(builtinTypes, (long)value, Location.Null);
			case TypeCode.SByte:
				return new SByteConstant(builtinTypes, (sbyte)value, Location.Null);
			case TypeCode.Single:
				return new FloatConstant(builtinTypes, (float)value, Location.Null);
			case TypeCode.String:
				return new StringConstant(builtinTypes, (string)value, Location.Null);
			case TypeCode.UInt16:
				return new UShortConstant(builtinTypes, (ushort)value, Location.Null);
			case TypeCode.UInt32:
				return new UIntConstant(builtinTypes, (uint)value, Location.Null);
			case TypeCode.UInt64:
				return new ULongConstant(builtinTypes, (ulong)value, Location.Null);
			default:
				throw new NotImplementedException(value.GetType().ToString());
			}
		}

		public TypeSpec ImportType(Type type)
		{
			return ImportType(type, new DynamicTypeReader(type));
		}

		private TypeSpec ImportType(Type type, DynamicTypeReader dtype)
		{
			if (type.HasElementType)
			{
				Type elementType = type.GetElementType();
				dtype.Position++;
				TypeSpec element = ImportType(elementType, dtype);
				if (type.IsArray)
				{
					return ArrayContainer.MakeType(module, element, type.GetArrayRank());
				}
				if (type.IsByRef)
				{
					return ReferenceContainer.MakeType(module, element);
				}
				if (type.IsPointer)
				{
					return PointerContainer.MakeType(module, element);
				}
				throw new NotImplementedException("Unknown element type " + type.ToString());
			}
			TypeSpec value;
			if (compiled_types.TryGetValue(type, out value))
			{
				if (value.BuiltinType == BuiltinTypeSpec.Type.Object && dtype.IsDynamicObject())
				{
					return module.Compiler.BuiltinTypes.Dynamic;
				}
				return value;
			}
			return CreateType(type, dtype, true);
		}

		private static bool IsMissingType(Type type)
		{
			return false;
		}

		private Constant ReadDecimalConstant(IList<CustomAttributeData> attrs)
		{
			if (attrs.Count == 0)
			{
				return null;
			}
			foreach (CustomAttributeData attr in attrs)
			{
				Type declaringType = attr.Constructor.DeclaringType;
				if (!(declaringType.Name != "DecimalConstantAttribute") && !(declaringType.Namespace != CompilerServicesNamespace))
				{
					decimal d = new decimal((int)(uint)attr.ConstructorArguments[4].Value, (int)(uint)attr.ConstructorArguments[3].Value, (int)(uint)attr.ConstructorArguments[2].Value, (byte)attr.ConstructorArguments[1].Value != 0, (byte)attr.ConstructorArguments[0].Value);
					return new DecimalConstant(module.Compiler.BuiltinTypes, d, Location.Null);
				}
			}
			return null;
		}

		private static Modifiers ReadMethodModifiers(MethodBase mb, TypeSpec declaringType)
		{
			MethodAttributes attributes = mb.Attributes;
			Modifiers modifiers;
			switch (attributes & MethodAttributes.MemberAccessMask)
			{
			case MethodAttributes.Public:
				modifiers = Modifiers.PUBLIC;
				break;
			case MethodAttributes.Assembly:
				modifiers = Modifiers.INTERNAL;
				break;
			case MethodAttributes.Family:
				modifiers = Modifiers.PROTECTED;
				break;
			case MethodAttributes.FamORAssem:
				modifiers = Modifiers.PROTECTED | Modifiers.INTERNAL;
				break;
			default:
				modifiers = Modifiers.PRIVATE;
				break;
			}
			if ((attributes & MethodAttributes.Static) != MethodAttributes.PrivateScope)
			{
				return modifiers | Modifiers.STATIC;
			}
			if ((attributes & MethodAttributes.Abstract) != MethodAttributes.PrivateScope && declaringType.IsClass)
			{
				return modifiers | Modifiers.ABSTRACT;
			}
			if ((attributes & MethodAttributes.Final) != MethodAttributes.PrivateScope)
			{
				modifiers |= Modifiers.SEALED;
			}
			if ((attributes & MethodAttributes.Virtual) != MethodAttributes.PrivateScope)
			{
				modifiers = (((attributes & MethodAttributes.VtableLayoutMask) == 0) ? (modifiers | Modifiers.OVERRIDE) : (((modifiers & Modifiers.SEALED) == 0) ? (modifiers | Modifiers.VIRTUAL) : (modifiers & ~Modifiers.SEALED)));
			}
			return modifiers;
		}
	}
}
