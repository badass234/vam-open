using System;
using System.Collections.Generic;
using System.Text;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Reader
{
	internal sealed class TypeDefImpl : TypeInfo
	{
		private readonly ModuleReader module;

		private readonly int index;

		private readonly string typeName;

		private readonly string typeNamespace;

		private Type[] typeArgs;

		public override Type BaseType
		{
			get
			{
				int extends = module.TypeDef.records[index].Extends;
				if ((extends & 0xFFFFFF) == 0)
				{
					return null;
				}
				return module.ResolveType(extends, this);
			}
		}

		public override TypeAttributes Attributes => (TypeAttributes)module.TypeDef.records[index].Flags;

		internal override TypeName TypeName => new TypeName(typeNamespace, typeName);

		public override string Name => TypeNameParser.Escape(typeName);

		public override string FullName => GetFullName();

		public override int MetadataToken => (2 << 24) + index + 1;

		public override bool IsGenericType => IsGenericTypeDefinition;

		public override bool IsGenericTypeDefinition
		{
			get
			{
				if ((typeFlags & (TypeFlags.IsGenericTypeDefinition | TypeFlags.NotGenericTypeDefinition)) == 0)
				{
					typeFlags |= (TypeFlags)((module.GenericParam.FindFirstByOwner(MetadataToken) != -1) ? 1 : 128);
				}
				return (typeFlags & TypeFlags.IsGenericTypeDefinition) != 0;
			}
		}

		internal bool IsNestedByFlags => (Attributes & TypeAttributes.VisibilityMask & ~TypeAttributes.Public) != 0;

		public override Type DeclaringType
		{
			get
			{
				if (!IsNestedByFlags)
				{
					return null;
				}
				SortedTable<NestedClassTable.Record>.Enumerator enumerator = module.NestedClass.Filter(MetadataToken).GetEnumerator();
				if (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					return module.ResolveType(module.NestedClass.records[current].EnclosingClass, null, null);
				}
				throw new InvalidOperationException();
			}
		}

		public override Module Module => module;

		internal override bool IsModulePseudoType => index == 0;

		internal override bool IsBaked => true;

		internal TypeDefImpl(ModuleReader module, int index)
		{
			this.module = module;
			this.index = index;
			typeName = module.GetString(module.TypeDef.records[index].TypeName);
			typeNamespace = module.GetString(module.TypeDef.records[index].TypeNamespace);
			MarkKnownType(typeNamespace, typeName);
		}

		public override EventInfo[] __GetDeclaredEvents()
		{
			SortedTable<EventMapTable.Record>.Enumerator enumerator = module.EventMap.Filter(MetadataToken).GetEnumerator();
			if (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				int num = module.EventMap.records[current].EventList - 1;
				int num2 = ((module.EventMap.records.Length > current + 1) ? (module.EventMap.records[current + 1].EventList - 1) : module.Event.records.Length);
				EventInfo[] array = new EventInfo[num2 - num];
				if (module.EventPtr.RowCount == 0)
				{
					int num3 = 0;
					while (num < num2)
					{
						array[num3] = new EventInfoImpl(module, this, num);
						num++;
						num3++;
					}
				}
				else
				{
					int num4 = 0;
					while (num < num2)
					{
						array[num4] = new EventInfoImpl(module, this, module.EventPtr.records[num] - 1);
						num++;
						num4++;
					}
				}
				return array;
			}
			return Empty<EventInfo>.Array;
		}

		public override FieldInfo[] __GetDeclaredFields()
		{
			int i = module.TypeDef.records[index].FieldList - 1;
			int num = ((module.TypeDef.records.Length > index + 1) ? (module.TypeDef.records[index + 1].FieldList - 1) : module.Field.records.Length);
			FieldInfo[] array = new FieldInfo[num - i];
			if (module.FieldPtr.RowCount == 0)
			{
				int num2 = 0;
				for (; i < num; i++)
				{
					array[num2] = module.GetFieldAt(this, i);
					num2++;
				}
			}
			else
			{
				int num3 = 0;
				for (; i < num; i++)
				{
					array[num3] = module.GetFieldAt(this, module.FieldPtr.records[i] - 1);
					num3++;
				}
			}
			return array;
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			List<Type> list = null;
			foreach (int item in module.InterfaceImpl.Filter(MetadataToken))
			{
				if (list == null)
				{
					list = new List<Type>();
				}
				list.Add(module.ResolveType(module.InterfaceImpl.records[item].Interface, this));
			}
			return Util.ToArray(list, Type.EmptyTypes);
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			int num = module.TypeDef.records[index].MethodList - 1;
			int num2 = ((module.TypeDef.records.Length > index + 1) ? (module.TypeDef.records[index + 1].MethodList - 1) : module.MethodDef.records.Length);
			MethodBase[] array = new MethodBase[num2 - num];
			if (module.MethodPtr.RowCount == 0)
			{
				int num3 = 0;
				while (num < num2)
				{
					array[num3] = module.GetMethodAt(this, num);
					num++;
					num3++;
				}
			}
			else
			{
				int num4 = 0;
				while (num < num2)
				{
					array[num4] = module.GetMethodAt(this, module.MethodPtr.records[num] - 1);
					num++;
					num4++;
				}
			}
			return array;
		}

		public override __MethodImplMap __GetMethodImplMap()
		{
			PopulateGenericArguments();
			List<MethodInfo> list = new List<MethodInfo>();
			List<List<MethodInfo>> list2 = new List<List<MethodInfo>>();
			foreach (int item3 in module.MethodImpl.Filter(MetadataToken))
			{
				MethodInfo item = (MethodInfo)module.ResolveMethod(module.MethodImpl.records[item3].MethodBody, typeArgs, null);
				int num = list.IndexOf(item);
				if (num == -1)
				{
					num = list.Count;
					list.Add(item);
					list2.Add(new List<MethodInfo>());
				}
				MethodInfo item2 = (MethodInfo)module.ResolveMethod(module.MethodImpl.records[item3].MethodDeclaration, typeArgs, null);
				list2[num].Add(item2);
			}
			__MethodImplMap result = new __MethodImplMap
			{
				TargetType = this,
				MethodBodies = list.ToArray(),
				MethodDeclarations = new MethodInfo[list2.Count][]
			};
			for (int i = 0; i < result.MethodDeclarations.Length; i++)
			{
				result.MethodDeclarations[i] = list2[i].ToArray();
			}
			return result;
		}

		public override Type[] __GetDeclaredTypes()
		{
			int metadataToken = MetadataToken;
			List<Type> list = new List<Type>();
			for (int i = 0; i < module.NestedClass.records.Length; i++)
			{
				if (module.NestedClass.records[i].EnclosingClass == metadataToken)
				{
					list.Add(module.ResolveType(module.NestedClass.records[i].NestedClass));
				}
			}
			return list.ToArray();
		}

		public override PropertyInfo[] __GetDeclaredProperties()
		{
			SortedTable<PropertyMapTable.Record>.Enumerator enumerator = module.PropertyMap.Filter(MetadataToken).GetEnumerator();
			if (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				int num = module.PropertyMap.records[current].PropertyList - 1;
				int num2 = ((module.PropertyMap.records.Length > current + 1) ? (module.PropertyMap.records[current + 1].PropertyList - 1) : module.Property.records.Length);
				PropertyInfo[] array = new PropertyInfo[num2 - num];
				if (module.PropertyPtr.RowCount == 0)
				{
					int num3 = 0;
					while (num < num2)
					{
						array[num3] = new PropertyInfoImpl(module, this, num);
						num++;
						num3++;
					}
				}
				else
				{
					int num4 = 0;
					while (num < num2)
					{
						array[num4] = new PropertyInfoImpl(module, this, module.PropertyPtr.records[num] - 1);
						num++;
						num4++;
					}
				}
				return array;
			}
			return Empty<PropertyInfo>.Array;
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
				list.Add(new GenericTypeParameter(module, i, 19));
			}
			typeArgs = list.ToArray();
		}

		internal override Type GetGenericTypeArgument(int index)
		{
			PopulateGenericArguments();
			return typeArgs[index];
		}

		public override CustomModifiers[] __GetGenericArgumentsCustomModifiers()
		{
			PopulateGenericArguments();
			return new CustomModifiers[typeArgs.Length];
		}

		public override Type GetGenericTypeDefinition()
		{
			if (IsGenericTypeDefinition)
			{
				return this;
			}
			throw new InvalidOperationException();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(FullName);
			string text = "[";
			Type[] genericArguments = GetGenericArguments();
			foreach (Type value in genericArguments)
			{
				stringBuilder.Append(text);
				stringBuilder.Append(value);
				text = ",";
			}
			if (text != "[")
			{
				stringBuilder.Append(']');
			}
			return stringBuilder.ToString();
		}

		public override bool __GetLayout(out int packingSize, out int typeSize)
		{
			SortedTable<ClassLayoutTable.Record>.Enumerator enumerator = module.ClassLayout.Filter(MetadataToken).GetEnumerator();
			if (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				packingSize = module.ClassLayout.records[current].PackingSize;
				typeSize = module.ClassLayout.records[current].ClassSize;
				return true;
			}
			packingSize = 0;
			typeSize = 0;
			return false;
		}
	}
}
