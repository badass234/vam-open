using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mono.CSharp
{
	internal class ImportedTypeDefinition : ImportedDefinition, ITypeDefinition, IMemberDefinition
	{
		private TypeParameterSpec[] tparams;

		private string name;

		public IAssemblyDefinition DeclaringAssembly => importer.GetAssemblyDefinition(provider.Module.Assembly);

		bool ITypeDefinition.IsComImport => ((Type)provider).IsImport;

		bool ITypeDefinition.IsPartial => false;

		bool ITypeDefinition.IsTypeForwarder => false;

		bool ITypeDefinition.IsCyclicTypeForwarder => false;

		public override string Name
		{
			get
			{
				if (name == null)
				{
					name = base.Name;
					if (tparams != null)
					{
						int num = name.IndexOf('`');
						if (num > 0)
						{
							name = name.Substring(0, num);
						}
					}
				}
				return name;
			}
		}

		public string Namespace => ((Type)provider).Namespace;

		public int TypeParametersCount
		{
			get
			{
				if (tparams != null)
				{
					return tparams.Length;
				}
				return 0;
			}
		}

		public TypeParameterSpec[] TypeParameters
		{
			get
			{
				return tparams;
			}
			set
			{
				tparams = value;
			}
		}

		public ImportedTypeDefinition(Type type, MetadataImporter importer)
			: base(type, importer)
		{
		}

		public void DefineInterfaces(TypeSpec spec)
		{
			Type[] interfaces = ((Type)provider).GetInterfaces();
			if (interfaces.Length != 0)
			{
				Type[] array = interfaces;
				foreach (Type type in array)
				{
					spec.AddInterface(importer.CreateType(type));
				}
			}
		}

		public static void Error_MissingDependency(IMemberContext ctx, List<MissingTypeSpecReference> missing, Location loc)
		{
			Report report = ctx.Module.Compiler.Report;
			for (int i = 0; i < missing.Count; i++)
			{
				TypeSpec type = missing[i].Type;
				if (report.Printer.MissingTypeReported(type.MemberDefinition))
				{
					continue;
				}
				string signatureForError = type.GetSignatureForError();
				MemberSpec caller = missing[i].Caller;
				if (caller.Kind != MemberKind.MissingType)
				{
					report.SymbolRelatedToPreviousError(caller);
				}
				ITypeDefinition memberDefinition = type.MemberDefinition;
				if (memberDefinition.DeclaringAssembly == ctx.Module.DeclaringAssembly)
				{
					report.Error(1683, loc, "Reference to type `{0}' claims it is defined in this assembly, but it is not defined in source or any added modules", signatureForError);
				}
				else if (memberDefinition.DeclaringAssembly.IsMissing)
				{
					if (memberDefinition.IsTypeForwarder)
					{
						report.Error(1070, loc, "The type `{0}' has been forwarded to an assembly that is not referenced. Consider adding a reference to assembly `{1}'", signatureForError, memberDefinition.DeclaringAssembly.FullName);
					}
					else
					{
						report.Error(12, loc, "The type `{0}' is defined in an assembly that is not referenced. Consider adding a reference to assembly `{1}'", signatureForError, memberDefinition.DeclaringAssembly.FullName);
					}
				}
				else if (memberDefinition.IsTypeForwarder)
				{
					report.Error(731, loc, "The type forwarder for type `{0}' in assembly `{1}' has circular dependency", signatureForError, memberDefinition.DeclaringAssembly.FullName);
				}
				else
				{
					report.Error(7069, loc, "Reference to type `{0}' claims it is defined assembly `{1}', but it could not be found", signatureForError, type.MemberDefinition.DeclaringAssembly.FullName);
				}
			}
		}

		public TypeSpec GetAttributeCoClass()
		{
			if (cattrs == null)
			{
				ReadAttributes();
			}
			return cattrs.CoClass;
		}

		public string GetAttributeDefaultMember()
		{
			if (cattrs == null)
			{
				ReadAttributes();
			}
			return cattrs.DefaultIndexerName;
		}

		public AttributeUsageAttribute GetAttributeUsage(PredefinedAttribute pa)
		{
			if (cattrs == null)
			{
				ReadAttributes();
			}
			return cattrs.AttributeUsage;
		}

		bool ITypeDefinition.IsInternalAsPublic(IAssemblyDefinition assembly)
		{
			IAssemblyDefinition assemblyDefinition = importer.GetAssemblyDefinition(provider.Module.Assembly);
			if (assemblyDefinition != assembly)
			{
				return assemblyDefinition.IsFriendAssemblyTo(assembly);
			}
			return true;
		}

		public void LoadMembers(TypeSpec declaringType, bool onlyTypes, ref MemberCache cache)
		{
			if (declaringType.IsPrivate && importer.IgnorePrivateMembers)
			{
				cache = MemberCache.Empty;
				return;
			}
			Type type = (Type)provider;
			Dictionary<MethodBase, MethodSpec> dictionary = null;
			List<EventSpec> list = null;
			MemberInfo[] members;
			try
			{
				members = type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			}
			catch (Exception exception)
			{
				throw new InternalErrorException(exception, "Could not import type `{0}' from `{1}'", declaringType.GetSignatureForError(), declaringType.MemberDefinition.DeclaringAssembly.FullName);
			}
			MemberInfo[] array;
			if (cache == null)
			{
				cache = new MemberCache(members.Length);
				array = members;
				foreach (MemberInfo memberInfo in array)
				{
					if (memberInfo.MemberType != MemberTypes.NestedType)
					{
						continue;
					}
					Type type2 = (Type)memberInfo;
					if ((type2.Attributes & TypeAttributes.VisibilityMask) != TypeAttributes.NestedPrivate || !importer.IgnorePrivateMembers)
					{
						MemberSpec ms;
						try
						{
							ms = importer.CreateNestedType(type2, declaringType);
						}
						catch (Exception exception2)
						{
							throw new InternalErrorException(exception2, "Could not import nested type `{0}' from `{1}'", type2.FullName, declaringType.MemberDefinition.DeclaringAssembly.FullName);
						}
						cache.AddMemberImported(ms);
					}
				}
				array = members;
				foreach (MemberInfo memberInfo2 in array)
				{
					if (memberInfo2.MemberType == MemberTypes.NestedType)
					{
						Type type3 = (Type)memberInfo2;
						if ((type3.Attributes & TypeAttributes.VisibilityMask) != TypeAttributes.NestedPrivate || !importer.IgnorePrivateMembers)
						{
							importer.ImportTypeBase(type3);
						}
					}
				}
			}
			if (declaringType.IsInterface && declaringType.Interfaces != null)
			{
				foreach (TypeSpec @interface in declaringType.Interfaces)
				{
					cache.AddInterface(@interface);
				}
			}
			if (onlyTypes)
			{
				return;
			}
			array = members;
			foreach (MemberInfo memberInfo3 in array)
			{
				MemberSpec ms;
				switch (memberInfo3.MemberType)
				{
				case MemberTypes.Constructor:
					if (declaringType.IsInterface)
					{
						continue;
					}
					goto case MemberTypes.Method;
				case MemberTypes.Method:
				{
					MethodBase methodBase = (MethodBase)memberInfo3;
					MethodAttributes attributes = methodBase.Attributes;
					if ((attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private && (importer.IgnorePrivateMembers || (attributes & (MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask)) == (MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask) || MetadataImporter.HasAttribute(CustomAttributeData.GetCustomAttributes(methodBase), "CompilerGeneratedAttribute", MetadataImporter.CompilerServicesNamespace)))
					{
						continue;
					}
					ms = importer.CreateMethod(methodBase, declaringType);
					if (ms.Kind == MemberKind.Method && !ms.IsGeneric)
					{
						if (dictionary == null)
						{
							dictionary = new Dictionary<MethodBase, MethodSpec>(ReferenceEquality<MethodBase>.Default);
						}
						dictionary.Add(methodBase, (MethodSpec)ms);
					}
					break;
				}
				case MemberTypes.Property:
				{
					if (dictionary == null)
					{
						continue;
					}
					PropertyInfo propertyInfo = (PropertyInfo)memberInfo3;
					MethodInfo addMethod = propertyInfo.GetGetMethod(true);
					MethodSpec value3;
					if (addMethod == null || !dictionary.TryGetValue(addMethod, out value3))
					{
						value3 = null;
					}
					addMethod = propertyInfo.GetSetMethod(true);
					MethodSpec value4;
					if (addMethod == null || !dictionary.TryGetValue(addMethod, out value4))
					{
						value4 = null;
					}
					if (value3 == null && value4 == null)
					{
						continue;
					}
					try
					{
						ms = importer.CreateProperty(propertyInfo, declaringType, value3, value4);
					}
					catch (Exception exception3)
					{
						throw new InternalErrorException(exception3, "Could not import property `{0}' inside `{1}'", propertyInfo.Name, declaringType.GetSignatureForError());
					}
					if (ms == null)
					{
						continue;
					}
					break;
				}
				case MemberTypes.Event:
				{
					if (dictionary == null)
					{
						continue;
					}
					EventInfo eventInfo = (EventInfo)memberInfo3;
					MethodInfo addMethod = eventInfo.GetAddMethod(true);
					MethodSpec value;
					if (addMethod == null || !dictionary.TryGetValue(addMethod, out value))
					{
						value = null;
					}
					addMethod = eventInfo.GetRemoveMethod(true);
					MethodSpec value2;
					if (addMethod == null || !dictionary.TryGetValue(addMethod, out value2))
					{
						value2 = null;
					}
					if (value == null || value2 == null)
					{
						continue;
					}
					EventSpec eventSpec2 = importer.CreateEvent(eventInfo, declaringType, value, value2);
					if (!importer.IgnorePrivateMembers)
					{
						if (list == null)
						{
							list = new List<EventSpec>();
						}
						list.Add(eventSpec2);
					}
					ms = eventSpec2;
					break;
				}
				case MemberTypes.Field:
				{
					FieldInfo fieldInfo = (FieldInfo)memberInfo3;
					ms = importer.CreateField(fieldInfo, declaringType);
					if (ms == null)
					{
						continue;
					}
					if (list == null)
					{
						break;
					}
					int j;
					for (j = 0; j < list.Count; j++)
					{
						EventSpec eventSpec = list[j];
						if (eventSpec.Name == fieldInfo.Name)
						{
							eventSpec.BackingField = (FieldSpec)ms;
							list.RemoveAt(j);
							j = -1;
							break;
						}
					}
					if (j < 0)
					{
						continue;
					}
					break;
				}
				default:
					throw new NotImplementedException(memberInfo3.ToString());
				case MemberTypes.NestedType:
					continue;
				}
				if (!ms.IsStatic || !declaringType.IsInterface)
				{
					cache.AddMemberImported(ms);
				}
			}
		}
	}
}
