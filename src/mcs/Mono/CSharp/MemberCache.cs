using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public class MemberCache
	{
		[Flags]
		private enum StateFlags
		{
			HasConversionOperator = 2,
			HasUserOperator = 4
		}

		private readonly Dictionary<string, IList<MemberSpec>> member_hash;

		private Dictionary<string, MemberSpec[]> locase_members;

		private IList<MethodSpec> missing_abstract;

		private StateFlags state;

		public static readonly string IndexerNameAlias = "<this>";

		public static readonly MemberCache Empty = new MemberCache(0);

		public MemberCache()
			: this(16)
		{
		}

		public MemberCache(int capacity)
		{
			member_hash = new Dictionary<string, IList<MemberSpec>>(capacity);
		}

		public MemberCache(MemberCache cache)
			: this(cache.member_hash.Count)
		{
			state = cache.state;
		}

		public MemberCache(TypeContainer container)
			: this()
		{
		}

		public void AddBaseType(TypeSpec baseType)
		{
			foreach (KeyValuePair<string, IList<MemberSpec>> item in baseType.MemberCache.member_hash)
			{
				IList<MemberSpec> value;
				if (!member_hash.TryGetValue(item.Key, out value))
				{
					value = ((item.Value.Count != 1) ? new List<MemberSpec>(item.Value) : item.Value);
					member_hash.Add(item.Key, value);
					continue;
				}
				foreach (MemberSpec item2 in item.Value)
				{
					if (!value.Contains(item2))
					{
						if (value is MemberSpec[])
						{
							value = new List<MemberSpec> { value[0] };
							member_hash[item.Key] = value;
						}
						value.Add(item2);
					}
				}
			}
		}

		public void AddInterface(TypeSpec iface)
		{
			foreach (KeyValuePair<string, IList<MemberSpec>> item in iface.MemberCache.member_hash)
			{
				IList<MemberSpec> value;
				if (!member_hash.TryGetValue(item.Key, out value))
				{
					value = ((item.Value.Count != 1) ? new List<MemberSpec>(item.Value) : item.Value);
					member_hash.Add(item.Key, value);
					continue;
				}
				foreach (MemberSpec item2 in item.Value)
				{
					if (!value.Contains(item2) && AddInterfaceMember(item2, ref value))
					{
						member_hash[item.Key] = value;
					}
				}
			}
		}

		public void AddMember(InterfaceMemberBase imb, string exlicitName, MemberSpec ms)
		{
			if (imb.IsExplicitImpl)
			{
				AddMember(exlicitName, ms, false);
			}
			else
			{
				AddMember(ms);
			}
		}

		public void AddMember(MemberSpec ms)
		{
			AddMember(GetLookupName(ms), ms, false);
		}

		private void AddMember(string name, MemberSpec member, bool removeHiddenMembers)
		{
			if (member.Kind == MemberKind.Operator)
			{
				TypeSpec declaringType = member.DeclaringType;
				if (!BuiltinTypeSpec.IsPrimitiveType(declaringType) || declaringType.BuiltinType == BuiltinTypeSpec.Type.Char)
				{
					BuiltinTypeSpec.Type builtinType = declaringType.BuiltinType;
					if (builtinType != BuiltinTypeSpec.Type.String && builtinType != BuiltinTypeSpec.Type.Delegate && builtinType != BuiltinTypeSpec.Type.MulticastDelegate)
					{
						if (name == Operator.GetMetadataName(Operator.OpType.Implicit) || name == Operator.GetMetadataName(Operator.OpType.Explicit))
						{
							state |= StateFlags.HasConversionOperator;
						}
						else
						{
							state |= StateFlags.HasUserOperator;
						}
					}
				}
			}
			IList<MemberSpec> value;
			if (!member_hash.TryGetValue(name, out value))
			{
				member_hash.Add(name, new MemberSpec[1] { member });
				return;
			}
			if (removeHiddenMembers && member.DeclaringType.IsInterface)
			{
				if (AddInterfaceMember(member, ref value))
				{
					member_hash[name] = value;
				}
				return;
			}
			if (value.Count == 1)
			{
				value = new List<MemberSpec> { value[0] };
				member_hash[name] = value;
			}
			value.Add(member);
		}

		public void AddMemberImported(MemberSpec ms)
		{
			AddMember(GetLookupName(ms), ms, true);
		}

		private static bool AddInterfaceMember(MemberSpec member, ref IList<MemberSpec> existing)
		{
			AParametersCollection aParametersCollection = ((member is IParametersMember) ? ((IParametersMember)member).Parameters : null);
			for (int i = 0; i < existing.Count; i++)
			{
				MemberSpec memberSpec = existing[i];
				if (memberSpec.Arity != member.Arity)
				{
					continue;
				}
				AParametersCollection a = null;
				if (aParametersCollection != null)
				{
					IParametersMember parametersMember = memberSpec as IParametersMember;
					if (parametersMember != null)
					{
						a = parametersMember.Parameters;
						if (!TypeSpecComparer.Override.IsEqual(a, aParametersCollection))
						{
							continue;
						}
					}
				}
				if (member.DeclaringType.ImplementsInterface(memberSpec.DeclaringType, false))
				{
					if (existing.Count == 1)
					{
						existing = new MemberSpec[1] { member };
						return true;
					}
					existing.RemoveAt(i--);
				}
				else
				{
					if (memberSpec.DeclaringType == member.DeclaringType && memberSpec.IsAccessor == member.IsAccessor)
					{
						return false;
					}
					if (memberSpec.DeclaringType.ImplementsInterface(member.DeclaringType, false) && AParametersCollection.HasSameParameterDefaults(a, aParametersCollection))
					{
						return false;
					}
				}
			}
			if (existing.Count == 1)
			{
				existing = new List<MemberSpec>
				{
					existing[0],
					member
				};
				return true;
			}
			existing.Add(member);
			return false;
		}

		public static MemberSpec FindMember(TypeSpec container, MemberFilter filter, BindingRestriction restrictions)
		{
			do
			{
				IList<MemberSpec> value;
				if (container.MemberCache.member_hash.TryGetValue(filter.Name, out value))
				{
					for (int num = value.Count - 1; num >= 0; num--)
					{
						MemberSpec memberSpec = value[num];
						if (((restrictions & BindingRestriction.InstanceOnly) == 0 || !memberSpec.IsStatic) && ((restrictions & BindingRestriction.NoAccessors) == 0 || !memberSpec.IsAccessor) && ((restrictions & BindingRestriction.OverrideOnly) == 0 || (memberSpec.Modifiers & Modifiers.OVERRIDE) != 0) && filter.Equals(memberSpec) && ((restrictions & BindingRestriction.DeclaredOnly) == 0 || !container.IsInterface || memberSpec.DeclaringType == container))
						{
							return memberSpec;
						}
					}
				}
				if ((restrictions & BindingRestriction.DeclaredOnly) != BindingRestriction.None)
				{
					break;
				}
				container = container.BaseType;
			}
			while (container != null);
			return null;
		}

		public static IList<MemberSpec> FindMembers(TypeSpec container, string name, bool declaredOnlyClass)
		{
			do
			{
				IList<MemberSpec> value;
				if (container.MemberCache.member_hash.TryGetValue(name, out value) | declaredOnlyClass)
				{
					return value;
				}
				container = container.BaseType;
			}
			while (container != null);
			return null;
		}

		public static TypeSpec FindNestedType(TypeSpec container, string name, int arity)
		{
			TypeSpec typeSpec = null;
			do
			{
				(container.MemberDefinition as TypeContainer)?.DefineContainer();
				IList<MemberSpec> value;
				if (container.MemberCacheTypes.member_hash.TryGetValue(name, out value))
				{
					for (int num = value.Count - 1; num >= 0; num--)
					{
						MemberSpec memberSpec = value[num];
						if ((memberSpec.Kind & MemberKind.NestedMask) != 0)
						{
							TypeSpec typeSpec2 = (TypeSpec)memberSpec;
							if (arity == typeSpec2.Arity)
							{
								return typeSpec2;
							}
							if (arity < 0)
							{
								if (typeSpec == null)
								{
									typeSpec = typeSpec2;
								}
								else if (System.Math.Abs(typeSpec2.Arity + arity) < System.Math.Abs(typeSpec2.Arity + arity))
								{
									typeSpec = typeSpec2;
								}
							}
						}
					}
				}
				container = container.BaseType;
			}
			while (container != null);
			return typeSpec;
		}

		public List<MethodSpec> FindExtensionMethods(IMemberContext invocationContext, string name, int arity)
		{
			IList<MemberSpec> value;
			if (!member_hash.TryGetValue(name, out value))
			{
				return null;
			}
			List<MethodSpec> list = null;
			foreach (MemberSpec item in value)
			{
				if (item.Kind != MemberKind.Method || (arity > 0 && item.Arity != arity))
				{
					continue;
				}
				MethodSpec methodSpec = (MethodSpec)item;
				if (methodSpec.IsExtensionMethod && methodSpec.IsAccessible(invocationContext) && ((methodSpec.DeclaringType.Modifiers & Modifiers.INTERNAL) == 0 || methodSpec.DeclaringType.MemberDefinition.IsInternalAsPublic(invocationContext.Module.DeclaringAssembly)))
				{
					if (list == null)
					{
						list = new List<MethodSpec>();
					}
					list.Add(methodSpec);
				}
			}
			return list;
		}

		public static MemberSpec FindBaseMember(MemberCore member, out MemberSpec bestCandidate, ref bool overrides)
		{
			bestCandidate = null;
			TypeSpec typeSpec = member.Parent.PartialContainer.Definition;
			if (!typeSpec.IsInterface)
			{
				typeSpec = typeSpec.BaseType;
				if (typeSpec == null)
				{
					return null;
				}
			}
			string lookupName = GetLookupName(member);
			AParametersCollection b = ((member is IParametersMember) ? ((IParametersMember)member).Parameters : null);
			MemberKind memberCoreKind = GetMemberCoreKind(member);
			bool flag = memberCoreKind == MemberKind.Indexer || memberCoreKind == MemberKind.Property;
			MemberSpec memberSpec = null;
			do
			{
				IList<MemberSpec> value;
				if (typeSpec.MemberCache.member_hash.TryGetValue(lookupName, out value))
				{
					for (int i = 0; i < value.Count; i++)
					{
						MemberSpec memberSpec2 = value[i];
						if ((memberSpec2.Modifiers & Modifiers.PUBLIC) == 0 && !memberSpec2.IsAccessible(member))
						{
							continue;
						}
						if ((memberSpec2.Kind & ~MemberKind.Destructor & memberCoreKind & MemberKind.MaskType) == 0)
						{
							if ((memberSpec2.Kind & MemberKind.Destructor) == 0 && (memberCoreKind == MemberKind.Method || member.MemberName.Arity == memberSpec2.Arity))
							{
								bestCandidate = memberSpec2;
								return null;
							}
						}
						else
						{
							if (member.MemberName.Arity != memberSpec2.Arity || ((memberSpec2.Kind & memberCoreKind & (MemberKind.Method | MemberKind.Indexer)) != 0 && (memberSpec2.IsAccessor != member is AbstractPropertyEventMethod || !TypeSpecComparer.Override.IsEqual((memberSpec2 as IParametersMember).Parameters, b))))
							{
								continue;
							}
							if (flag && (memberSpec2.Modifiers & (Modifiers.SEALED | Modifiers.OVERRIDE)) == Modifiers.OVERRIDE)
							{
								overrides = true;
								continue;
							}
							if (memberSpec != null || (memberSpec2.Kind & memberCoreKind & (MemberKind.Method | MemberKind.Indexer)) == 0)
							{
								bestCandidate = memberSpec;
								return memberSpec2;
							}
							bestCandidate = null;
							memberSpec = memberSpec2;
						}
					}
				}
				if (typeSpec.IsInterface || memberSpec != null)
				{
					break;
				}
				typeSpec = typeSpec.BaseType;
			}
			while (typeSpec != null);
			return memberSpec;
		}

		public static T GetMember<T>(TypeSpec container, T spec) where T : MemberSpec
		{
			IList<MemberSpec> value;
			if (container.MemberCache.member_hash.TryGetValue(GetLookupName(spec), out value))
			{
				for (int num = value.Count - 1; num >= 0; num--)
				{
					MemberSpec memberSpec = value[num];
					if (memberSpec.MemberDefinition == spec.MemberDefinition)
					{
						return (T)memberSpec;
					}
				}
			}
			throw new InternalErrorException("Missing member `{0}' on inflated type `{1}'", spec.GetSignatureForError(), container.GetSignatureForError());
		}

		private static MemberKind GetMemberCoreKind(MemberCore member)
		{
			if (member is FieldBase)
			{
				return MemberKind.Field;
			}
			if (member is Indexer)
			{
				return MemberKind.Indexer;
			}
			if (member is Class)
			{
				return MemberKind.Class;
			}
			if (member is Struct)
			{
				return MemberKind.Struct;
			}
			if (member is Destructor)
			{
				return MemberKind.Destructor;
			}
			if (member is Method)
			{
				return MemberKind.Method;
			}
			if (member is Property)
			{
				return MemberKind.Property;
			}
			if (member is EventField)
			{
				return MemberKind.Event;
			}
			if (member is Interface)
			{
				return MemberKind.Interface;
			}
			if (member is EventProperty)
			{
				return MemberKind.Event;
			}
			if (member is Delegate)
			{
				return MemberKind.Delegate;
			}
			if (member is Enum)
			{
				return MemberKind.Enum;
			}
			throw new NotImplementedException(member.GetType().ToString());
		}

		public static List<FieldSpec> GetAllFieldsForDefiniteAssignment(TypeSpec container, IMemberContext context)
		{
			List<FieldSpec> list = null;
			bool isImported = container.MemberDefinition.IsImported;
			foreach (KeyValuePair<string, IList<MemberSpec>> item in container.MemberCache.member_hash)
			{
				foreach (MemberSpec item2 in item.Value)
				{
					if (item2.Kind != MemberKind.Field || (item2.Modifiers & Modifiers.STATIC) != 0 || item2 is FixedFieldSpec || item2 is ConstSpec)
					{
						continue;
					}
					FieldSpec fieldSpec = (FieldSpec)item2;
					if (!isImported || !ShouldIgnoreFieldForDefiniteAssignment(fieldSpec, context))
					{
						if (list == null)
						{
							list = new List<FieldSpec>();
						}
						list.Add(fieldSpec);
						break;
					}
				}
			}
			return list ?? new List<FieldSpec>(0);
		}

		private static bool ShouldIgnoreFieldForDefiniteAssignment(FieldSpec fs, IMemberContext context)
		{
			Modifiers modifiers = fs.Modifiers;
			if ((modifiers & Modifiers.PRIVATE) == 0 && (modifiers & Modifiers.INTERNAL) != 0 && fs.DeclaringType.MemberDefinition.IsInternalAsPublic(context.Module.DeclaringAssembly))
			{
				return false;
			}
			TypeSpec memberType = fs.MemberType;
			MemberKind kind = memberType.Kind;
			if (kind == MemberKind.TypeParameter || kind == MemberKind.ArrayType)
			{
				return false;
			}
			return TypeSpec.IsReferenceType(memberType);
		}

		public static IList<MemberSpec> GetCompletitionMembers(IMemberContext ctx, TypeSpec container, string name)
		{
			List<MemberSpec> list = new List<MemberSpec>();
			foreach (KeyValuePair<string, IList<MemberSpec>> item in container.MemberCache.member_hash)
			{
				foreach (MemberSpec item2 in item.Value)
				{
					if (!item2.IsAccessor && (item2.Kind & (MemberKind.Constructor | MemberKind.Operator | MemberKind.Destructor)) == 0 && item2.IsAccessible(ctx) && (name == null || item2.Name.StartsWith(name)))
					{
						list.Add(item2);
					}
				}
			}
			return list;
		}

		public static List<MethodSpec> GetInterfaceMethods(TypeSpec iface)
		{
			List<MethodSpec> list = new List<MethodSpec>();
			foreach (IList<MemberSpec> value in iface.MemberCache.member_hash.Values)
			{
				foreach (MemberSpec item in value)
				{
					if (iface == item.DeclaringType && item.Kind == MemberKind.Method)
					{
						list.Add((MethodSpec)item);
					}
				}
			}
			return list;
		}

		public static IList<MethodSpec> GetNotImplementedAbstractMethods(TypeSpec type)
		{
			if (type.MemberCache.missing_abstract != null)
			{
				return type.MemberCache.missing_abstract;
			}
			List<MethodSpec> list = new List<MethodSpec>();
			List<TypeSpec> list2 = null;
			TypeSpec typeSpec = type;
			while (true)
			{
				foreach (KeyValuePair<string, IList<MemberSpec>> item in typeSpec.MemberCache.member_hash)
				{
					foreach (MemberSpec item2 in item.Value)
					{
						if ((item2.Modifiers & Modifiers.ABSTRACT) != 0)
						{
							MethodSpec methodSpec = item2 as MethodSpec;
							if (methodSpec != null)
							{
								list.Add(methodSpec);
							}
						}
					}
				}
				TypeSpec baseType = typeSpec.BaseType;
				if (!baseType.IsAbstract)
				{
					break;
				}
				if (list2 == null)
				{
					list2 = new List<TypeSpec>();
				}
				list2.Add(typeSpec);
				typeSpec = baseType;
			}
			int num = list.Count;
			if (num == 0 || list2 == null)
			{
				type.MemberCache.missing_abstract = list;
				return type.MemberCache.missing_abstract;
			}
			foreach (TypeSpec item3 in list2)
			{
				Dictionary<string, IList<MemberSpec>> dictionary = item3.MemberCache.member_hash;
				if (dictionary.Count == 0)
				{
					continue;
				}
				for (int i = 0; i < list.Count; i++)
				{
					MethodSpec methodSpec2 = list[i];
					IList<MemberSpec> value;
					if (methodSpec2 == null || !dictionary.TryGetValue(methodSpec2.Name, out value))
					{
						continue;
					}
					MemberFilter memberFilter = new MemberFilter(methodSpec2);
					foreach (MemberSpec item4 in value)
					{
						if ((item4.Modifiers & (Modifiers.VIRTUAL | Modifiers.OVERRIDE)) != 0 && (item4.Modifiers & Modifiers.ABSTRACT) == 0 && memberFilter.Equals(item4))
						{
							num--;
							list[i] = null;
							break;
						}
					}
				}
			}
			if (num == list.Count)
			{
				type.MemberCache.missing_abstract = list;
				return type.MemberCache.missing_abstract;
			}
			MethodSpec[] array = new MethodSpec[num];
			int num2 = 0;
			foreach (MethodSpec item5 in list)
			{
				if (item5 != null)
				{
					array[num2++] = item5;
				}
			}
			type.MemberCache.missing_abstract = array;
			return type.MemberCache.missing_abstract;
		}

		private static string GetLookupName(MemberSpec ms)
		{
			if (ms.Kind == MemberKind.Indexer)
			{
				return IndexerNameAlias;
			}
			if (ms.Kind == MemberKind.Constructor)
			{
				if (ms.IsStatic)
				{
					return Constructor.TypeConstructorName;
				}
				return Constructor.ConstructorName;
			}
			return ms.Name;
		}

		private static string GetLookupName(MemberCore mc)
		{
			if (mc is Indexer)
			{
				return IndexerNameAlias;
			}
			if (mc is Constructor)
			{
				if (!mc.IsStatic)
				{
					return Constructor.ConstructorName;
				}
				return Constructor.TypeConstructorName;
			}
			return mc.MemberName.Name;
		}

		public static IList<MemberSpec> GetUserOperator(TypeSpec container, Operator.OpType op, bool declaredOnly)
		{
			IList<MemberSpec> list = null;
			bool flag = true;
			do
			{
				MemberCache memberCache = container.MemberCache;
				IList<MemberSpec> value;
				if ((((op == Operator.OpType.Implicit || op == Operator.OpType.Explicit) && (memberCache.state & StateFlags.HasConversionOperator) != 0) || (memberCache.state & StateFlags.HasUserOperator) != 0) && memberCache.member_hash.TryGetValue(Operator.GetMetadataName(op), out value))
				{
					int i;
					for (i = 0; i < value.Count && value[i].Kind == MemberKind.Operator; i++)
					{
					}
					if (i != value.Count)
					{
						for (i = 0; i < value.Count; i++)
						{
							if (value[i].Kind != MemberKind.Operator)
							{
								continue;
							}
							if (list == null)
							{
								list = new List<MemberSpec>();
								list.Add(value[i]);
								continue;
							}
							List<MemberSpec> list2;
							if (flag)
							{
								flag = false;
								list2 = new List<MemberSpec>(list.Count + 1);
								list2.AddRange(list);
							}
							else
							{
								list2 = (List<MemberSpec>)list;
							}
							list2.Add(value[i]);
						}
					}
					else if (list == null)
					{
						list = value;
						flag = true;
					}
					else
					{
						List<MemberSpec> list3;
						if (flag)
						{
							flag = false;
							list3 = new List<MemberSpec>(list.Count + value.Count);
							list3.AddRange(list);
							list = list3;
						}
						else
						{
							list3 = (List<MemberSpec>)list;
						}
						list3.AddRange(value);
					}
				}
				if (declaredOnly)
				{
					break;
				}
				container = container.BaseType;
			}
			while (container != null);
			return list;
		}

		public void InflateTypes(MemberCache inflated_cache, TypeParameterInflator inflator)
		{
			foreach (KeyValuePair<string, IList<MemberSpec>> item in member_hash)
			{
				IList<MemberSpec> list = null;
				for (int i = 0; i < item.Value.Count; i++)
				{
					MemberSpec memberSpec = item.Value[i];
					if (memberSpec != null && (memberSpec.Kind & MemberKind.NestedMask) != 0 && (memberSpec.Modifiers & Modifiers.COMPILER_GENERATED) == 0)
					{
						if (list == null)
						{
							list = new MemberSpec[item.Value.Count];
							inflated_cache.member_hash.Add(item.Key, list);
						}
						list[i] = memberSpec.InflateMember(inflator);
					}
				}
			}
		}

		public void InflateMembers(MemberCache cacheToInflate, TypeSpec inflatedType, TypeParameterInflator inflator)
		{
			Dictionary<string, IList<MemberSpec>> dictionary = cacheToInflate.member_hash;
			Dictionary<MemberSpec, MethodSpec> dictionary2 = null;
			List<MemberSpec> list = null;
			cacheToInflate.state = state;
			foreach (KeyValuePair<string, IList<MemberSpec>> item in member_hash)
			{
				IList<MemberSpec> value = item.Value;
				IList<MemberSpec> list2 = null;
				for (int i = 0; i < value.Count; i++)
				{
					MemberSpec memberSpec = value[i];
					if ((memberSpec.Kind & MemberKind.NestedMask) != 0 && (memberSpec.Modifiers & Modifiers.COMPILER_GENERATED) == 0)
					{
						if (list2 == null)
						{
							list2 = dictionary[item.Key];
						}
						continue;
					}
					if (list2 == null)
					{
						list2 = new MemberSpec[item.Value.Count];
						dictionary.Add(item.Key, list2);
					}
					TypeParameterInflator inflator2 = inflator;
					if (memberSpec.DeclaringType != inflatedType)
					{
						if (!memberSpec.DeclaringType.IsGeneric && !memberSpec.DeclaringType.IsNested)
						{
							list2[i] = memberSpec;
							continue;
						}
						TypeSpec typeSpec = inflator.Inflate(memberSpec.DeclaringType);
						if (typeSpec != inflator.TypeInstance)
						{
							inflator2 = new TypeParameterInflator(inflator, typeSpec);
						}
					}
					MemberSpec memberSpec2 = (list2[i] = memberSpec.InflateMember(inflator2));
					if (memberSpec is PropertySpec || memberSpec is EventSpec)
					{
						if (list == null)
						{
							list = new List<MemberSpec>();
						}
						list.Add(memberSpec2);
					}
					else if (memberSpec.IsAccessor)
					{
						if (dictionary2 == null)
						{
							dictionary2 = new Dictionary<MemberSpec, MethodSpec>();
						}
						dictionary2.Add(memberSpec, (MethodSpec)memberSpec2);
					}
				}
			}
			if (list == null)
			{
				return;
			}
			foreach (MemberSpec item2 in list)
			{
				PropertySpec propertySpec = item2 as PropertySpec;
				if (propertySpec != null)
				{
					if (propertySpec.Get != null)
					{
						propertySpec.Get = dictionary2[propertySpec.Get];
					}
					if (propertySpec.Set != null)
					{
						propertySpec.Set = dictionary2[propertySpec.Set];
					}
				}
				else
				{
					EventSpec eventSpec = (EventSpec)item2;
					eventSpec.AccessorAdd = dictionary2[eventSpec.AccessorAdd];
					eventSpec.AccessorRemove = dictionary2[eventSpec.AccessorRemove];
				}
			}
		}

		public void RemoveHiddenMembers(TypeSpec container)
		{
			foreach (KeyValuePair<string, IList<MemberSpec>> item in member_hash)
			{
				IList<MemberSpec> value = item.Value;
				int num = 0;
				while (value[num].DeclaringType != container && ++num < item.Value.Count)
				{
				}
				if (num == 0 || num == value.Count)
				{
					continue;
				}
				for (int i = 0; i < num; i++)
				{
					MemberSpec memberSpec = value[i];
					if (!container.ImplementsInterface(memberSpec.DeclaringType, false))
					{
						continue;
					}
					AParametersCollection b = ((memberSpec is IParametersMember) ? ((IParametersMember)memberSpec).Parameters : ParametersCompiled.EmptyReadOnlyParameters);
					for (int j = num; j < value.Count; j++)
					{
						MemberSpec memberSpec2 = value[j];
						if (memberSpec2.Arity == memberSpec.Arity && (!(memberSpec2 is IParametersMember) || TypeSpecComparer.Override.IsEqual(((IParametersMember)memberSpec2).Parameters, b)))
						{
							value.RemoveAt(i);
							num--;
							j--;
							i--;
						}
					}
				}
			}
		}

		public void VerifyClsCompliance(TypeSpec container, Report report)
		{
			if (locase_members != null)
			{
				return;
			}
			if (container.BaseType == null)
			{
				locase_members = new Dictionary<string, MemberSpec[]>(member_hash.Count);
			}
			else
			{
				TypeSpec definition = container.BaseType.GetDefinition();
				definition.MemberCache.VerifyClsCompliance(definition, report);
				locase_members = new Dictionary<string, MemberSpec[]>(definition.MemberCache.locase_members);
			}
			bool isImported = container.MemberDefinition.IsImported;
			foreach (KeyValuePair<string, IList<MemberSpec>> item in container.MemberCache.member_hash)
			{
				for (int i = 0; i < item.Value.Count; i++)
				{
					MemberSpec memberSpec = item.Value[i];
					if ((memberSpec.Modifiers & (Modifiers.PROTECTED | Modifiers.PUBLIC)) == 0 || (memberSpec.Modifiers & (Modifiers.OVERRIDE | Modifiers.COMPILER_GENERATED)) != 0 || (memberSpec.Kind & MemberKind.MaskType) == 0 || memberSpec.MemberDefinition.CLSAttributeValue == false)
					{
						continue;
					}
					IParametersMember parametersMember = null;
					if (!isImported)
					{
						parametersMember = memberSpec as IParametersMember;
						if (parametersMember != null && !memberSpec.IsAccessor)
						{
							AParametersCollection parameters = parametersMember.Parameters;
							for (int j = i + 1; j < item.Value.Count; j++)
							{
								MemberSpec memberSpec2 = item.Value[j];
								IParametersMember parametersMember2 = memberSpec2 as IParametersMember;
								if (parametersMember2 != null && parameters.Count == parametersMember2.Parameters.Count && !memberSpec2.IsAccessor)
								{
									int num = ParametersCompiled.IsSameClsSignature(parametersMember.Parameters, parametersMember2.Parameters);
									if (num != 0)
									{
										ReportOverloadedMethodClsDifference(memberSpec, memberSpec2, num, report);
									}
								}
							}
						}
					}
					if (i > 0 || memberSpec.Kind == MemberKind.Constructor || memberSpec.Kind == MemberKind.Indexer)
					{
						continue;
					}
					string key = memberSpec.Name.ToLowerInvariant();
					MemberSpec[] value;
					if (!locase_members.TryGetValue(key, out value))
					{
						value = new MemberSpec[1] { memberSpec };
						locase_members.Add(key, value);
						continue;
					}
					bool flag = true;
					MemberSpec[] array = value;
					foreach (MemberSpec memberSpec3 in array)
					{
						if (memberSpec3.Name == memberSpec.Name)
						{
							if (parametersMember == null)
							{
								continue;
							}
							IParametersMember parametersMember3 = memberSpec3 as IParametersMember;
							if (parametersMember3 != null && parametersMember.Parameters.Count == parametersMember3.Parameters.Count && !memberSpec3.IsAccessor)
							{
								int num2 = ParametersCompiled.IsSameClsSignature(parametersMember.Parameters, parametersMember3.Parameters);
								if (num2 != 0)
								{
									ReportOverloadedMethodClsDifference(memberSpec3, memberSpec, num2, report);
								}
							}
							continue;
						}
						flag = false;
						if (!isImported)
						{
							MemberCore laterDefinedMember = GetLaterDefinedMember(memberSpec3, memberSpec);
							if (laterDefinedMember == memberSpec3.MemberDefinition)
							{
								report.SymbolRelatedToPreviousError(memberSpec);
							}
							else
							{
								report.SymbolRelatedToPreviousError(memberSpec3);
							}
							report.Warning(3005, 1, laterDefinedMember.Location, "Identifier `{0}' differing only in case is not CLS-compliant", laterDefinedMember.GetSignatureForError());
						}
					}
					if (!flag)
					{
						Array.Resize(ref value, value.Length + 1);
						MemberSpec[] array2 = value;
						array2[array2.Length - 1] = memberSpec;
						locase_members[key] = value;
					}
				}
			}
		}

		private static MemberCore GetLaterDefinedMember(MemberSpec a, MemberSpec b)
		{
			MemberCore memberCore = a.MemberDefinition as MemberCore;
			MemberCore memberCore2 = b.MemberDefinition as MemberCore;
			if (memberCore == null)
			{
				return memberCore2;
			}
			if (memberCore2 == null)
			{
				return memberCore;
			}
			if (a.DeclaringType.MemberDefinition != b.DeclaringType.MemberDefinition)
			{
				return memberCore2;
			}
			if (memberCore.Location.File != memberCore.Location.File)
			{
				return memberCore2;
			}
			if (memberCore2.Location.Row <= memberCore.Location.Row)
			{
				return memberCore;
			}
			return memberCore2;
		}

		private static void ReportOverloadedMethodClsDifference(MemberSpec a, MemberSpec b, int res, Report report)
		{
			MemberCore laterDefinedMember = GetLaterDefinedMember(a, b);
			if (laterDefinedMember == a.MemberDefinition)
			{
				report.SymbolRelatedToPreviousError(b);
			}
			else
			{
				report.SymbolRelatedToPreviousError(a);
			}
			if ((res & 1) != 0)
			{
				report.Warning(3006, 1, laterDefinedMember.Location, "Overloaded method `{0}' differing only in ref or out, or in array rank, is not CLS-compliant", laterDefinedMember.GetSignatureForError());
			}
			if ((res & 2) != 0)
			{
				report.Warning(3007, 1, laterDefinedMember.Location, "Overloaded method `{0}' differing only by unnamed array types is not CLS-compliant", laterDefinedMember.GetSignatureForError());
			}
		}

		public bool CheckExistingMembersOverloads(MemberCore member, AParametersCollection parameters)
		{
			string name = GetLookupName(member);
			InterfaceMemberBase interfaceMemberBase = member as InterfaceMemberBase;
			if (interfaceMemberBase != null && interfaceMemberBase.IsExplicitImpl)
			{
				name = interfaceMemberBase.GetFullName(name);
			}
			return CheckExistingMembersOverloads(member, name, parameters);
		}

		public bool CheckExistingMembersOverloads(MemberCore member, string name, AParametersCollection parameters)
		{
			IList<MemberSpec> value;
			if (!member_hash.TryGetValue(name, out value))
			{
				return false;
			}
			Report report = member.Compiler.Report;
			int count = parameters.Count;
			for (int num = value.Count - 1; num >= 0; num--)
			{
				MemberSpec memberSpec = value[num];
				IParametersMember parametersMember = memberSpec as IParametersMember;
				AParametersCollection aParametersCollection = ((parametersMember == null) ? ParametersCompiled.EmptyReadOnlyParameters : parametersMember.Parameters);
				if (aParametersCollection.Count != count || memberSpec.Arity != member.MemberName.Arity || member.Parent.PartialContainer != memberSpec.DeclaringType.MemberDefinition)
				{
					continue;
				}
				TypeSpec[] types = aParametersCollection.Types;
				if (count > 0)
				{
					int num2 = count - 1;
					TypeSpec a;
					TypeSpec b;
					bool num3;
					bool flag;
					do
					{
						a = parameters.Types[num2];
						b = types[num2];
						num3 = (aParametersCollection.FixedParameters[num2].ModFlags & Parameter.Modifier.RefOutMask) != 0;
						flag = (parameters.FixedParameters[num2].ModFlags & Parameter.Modifier.RefOutMask) != 0;
					}
					while (num3 == flag && TypeSpecComparer.Override.IsEqual(a, b) && num2-- != 0);
					if (num2 >= 0 || (member is Operator && memberSpec.Kind == MemberKind.Operator && ((MethodSpec)memberSpec).ReturnType != ((Operator)member).ReturnType))
					{
						continue;
					}
					if (aParametersCollection != null && member is MethodCore)
					{
						num2 = count;
						while (num2-- != 0 && (parameters.FixedParameters[num2].ModFlags & Parameter.Modifier.ModifierMask) == (aParametersCollection.FixedParameters[num2].ModFlags & Parameter.Modifier.ModifierMask) && parameters.ExtensionMethodType == aParametersCollection.ExtensionMethodType)
						{
						}
						if (num2 >= 0)
						{
							MethodSpec methodSpec = memberSpec as MethodSpec;
							member.Compiler.Report.SymbolRelatedToPreviousError(memberSpec);
							if ((member.ModFlags & Modifiers.PARTIAL) != 0 && (methodSpec.Modifiers & Modifiers.PARTIAL) != 0)
							{
								if (parameters.HasParams || aParametersCollection.HasParams)
								{
									report.Error(758, member.Location, "A partial method declaration and partial method implementation cannot differ on use of `params' modifier");
								}
								else
								{
									report.Error(755, member.Location, "A partial method declaration and partial method implementation must be both an extension method or neither");
								}
							}
							else if (member is Constructor)
							{
								report.Error(851, member.Location, "Overloaded contructor `{0}' cannot differ on use of parameter modifiers only", member.GetSignatureForError());
							}
							else
							{
								report.Error(663, member.Location, "Overloaded method `{0}' cannot differ on use of parameter modifiers only", member.GetSignatureForError());
							}
							return false;
						}
					}
				}
				if ((memberSpec.Kind & MemberKind.Method) != 0)
				{
					Method method = member as Method;
					Method method2 = memberSpec.MemberDefinition as Method;
					if (method != null && method2 != null && (method.ModFlags & method2.ModFlags & Modifiers.PARTIAL) != 0)
					{
						if (method.IsPartialDefinition == method2.IsPartialImplementation)
						{
							if ((method.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)) == (method2.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)) || (method.Parent.IsUnsafe && method2.Parent.IsUnsafe))
							{
								if (method.IsPartialImplementation)
								{
									method.SetPartialDefinition(method2);
									if (value.Count == 1)
									{
										member_hash.Remove(name);
									}
									else
									{
										value.RemoveAt(num);
									}
								}
								else
								{
									method2.SetPartialDefinition(method);
									method.caching_flags |= MemberCore.Flags.PartialDefinitionExists;
								}
								continue;
							}
							if (method.IsStatic != method2.IsStatic)
							{
								report.SymbolRelatedToPreviousError(memberSpec);
								report.Error(763, member.Location, "A partial method declaration and partial method implementation must be both `static' or neither");
							}
							if ((method.ModFlags & Modifiers.UNSAFE) != (method2.ModFlags & Modifiers.UNSAFE))
							{
								report.SymbolRelatedToPreviousError(memberSpec);
								report.Error(764, member.Location, "A partial method declaration and partial method implementation must be both `unsafe' or neither");
							}
							return false;
						}
						report.SymbolRelatedToPreviousError(memberSpec);
						if (method.IsPartialDefinition)
						{
							report.Error(756, member.Location, "A partial method `{0}' declaration is already defined", member.GetSignatureForError());
						}
						report.Error(757, member.Location, "A partial method `{0}' implementation is already defined", member.GetSignatureForError());
						return false;
					}
					report.SymbolRelatedToPreviousError(memberSpec);
					bool flag2 = member is AbstractPropertyEventMethod || member is Operator;
					bool isReservedMethod = ((MethodSpec)memberSpec).IsReservedMethod;
					if (flag2 | isReservedMethod)
					{
						report.Error(82, member.Location, "A member `{0}' is already reserved", flag2 ? memberSpec.GetSignatureForError() : member.GetSignatureForError());
						return false;
					}
				}
				else
				{
					report.SymbolRelatedToPreviousError(memberSpec);
				}
				if (member is Operator && memberSpec.Kind == MemberKind.Operator)
				{
					report.Error(557, member.Location, "Duplicate user-defined conversion in type `{0}'", member.Parent.GetSignatureForError());
					return false;
				}
				report.Error(111, member.Location, "A member `{0}' is already defined. Rename this member or use different parameter types", member.GetSignatureForError());
				return false;
			}
			return true;
		}
	}
}
