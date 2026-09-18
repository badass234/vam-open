using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public struct CustomModifiers : IEquatable<CustomModifiers>, IEnumerable<CustomModifiers.Entry>, IEnumerable
	{
		public struct Enumerator : IEnumerator<Entry>, IEnumerator, IDisposable
		{
			private readonly Type[] types;

			private int index;

			private bool required;

			public Entry Current => new Entry(types[index], required);

			object IEnumerator.Current => Current;

			internal Enumerator(Type[] types)
			{
				this.types = types;
				index = -1;
				required = Initial == MarkerType.ModReq;
			}

			void IEnumerator.Reset()
			{
				index = -1;
				required = Initial == MarkerType.ModReq;
			}

			public bool MoveNext()
			{
				if (types == null || index == types.Length)
				{
					return false;
				}
				index++;
				if (index == types.Length)
				{
					return false;
				}
				if (types[index] == MarkerType.ModOpt)
				{
					required = false;
					index++;
				}
				else if (types[index] == MarkerType.ModReq)
				{
					required = true;
					index++;
				}
				return true;
			}

			void IDisposable.Dispose()
			{
			}
		}

		public struct Entry
		{
			private readonly Type type;

			private readonly bool required;

			public Type Type => type;

			public bool IsRequired => required;

			internal Entry(Type type, bool required)
			{
				this.type = type;
				this.required = required;
			}
		}

		private readonly Type[] types;

		private static Type Initial => MarkerType.ModOpt;

		public bool IsEmpty => types == null;

		public bool ContainsMissingType => Type.ContainsMissingType(types);

		internal CustomModifiers(List<CustomModifiersBuilder.Item> list)
		{
			bool flag = Initial == MarkerType.ModReq;
			int num = list.Count;
			foreach (CustomModifiersBuilder.Item item in list)
			{
				if (item.required != flag)
				{
					flag = item.required;
					num++;
				}
			}
			types = new Type[num];
			flag = Initial == MarkerType.ModReq;
			int num2 = 0;
			foreach (CustomModifiersBuilder.Item item2 in list)
			{
				if (item2.required != flag)
				{
					flag = item2.required;
					types[num2++] = (flag ? MarkerType.ModReq : MarkerType.ModOpt);
				}
				types[num2++] = item2.type;
			}
		}

		private CustomModifiers(Type[] types)
		{
			this.types = types;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(types);
		}

		IEnumerator<Entry> IEnumerable<Entry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Equals(CustomModifiers other)
		{
			return Util.ArrayEquals(types, other.types);
		}

		public override bool Equals(object obj)
		{
			CustomModifiers? customModifiers = obj as CustomModifiers?;
			if (customModifiers.HasValue)
			{
				return Equals(customModifiers.Value);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Util.GetHashCode(types);
		}

		public override string ToString()
		{
			if (types == null)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			string value = "";
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Entry current = enumerator.Current;
					stringBuilder.Append(value).Append(current.IsRequired ? "modreq(" : "modopt(").Append(current.Type.FullName)
						.Append(')');
					value = " ";
				}
			}
			return stringBuilder.ToString();
		}

		private Type[] GetRequiredOrOptional(bool required)
		{
			if (types == null)
			{
				return Type.EmptyTypes;
			}
			int num = 0;
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.IsRequired == required)
					{
						num++;
					}
				}
			}
			Type[] array = new Type[num];
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Entry current = enumerator.Current;
					if (current.IsRequired == required)
					{
						array[--num] = current.Type;
					}
				}
				return array;
			}
		}

		internal Type[] GetRequired()
		{
			return GetRequiredOrOptional(true);
		}

		internal Type[] GetOptional()
		{
			return GetRequiredOrOptional(false);
		}

		internal CustomModifiers Bind(IGenericBinder binder)
		{
			if (types == null)
			{
				return this;
			}
			Type[] array = types;
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i] == MarkerType.ModOpt || types[i] == MarkerType.ModReq)
				{
					continue;
				}
				Type type = types[i].BindTypeParameters(binder);
				if ((object)type != types[i])
				{
					if (array == types)
					{
						array = (Type[])types.Clone();
					}
					array[i] = type;
				}
			}
			return new CustomModifiers(array);
		}

		internal static CustomModifiers Read(ModuleReader module, ByteReader br, IGenericContext context)
		{
			byte b = br.PeekByte();
			if (!IsCustomModifier(b))
			{
				return default(CustomModifiers);
			}
			List<Type> list = new List<Type>();
			Type type = Initial;
			do
			{
				Type type2 = ((br.ReadByte() == 31) ? MarkerType.ModReq : MarkerType.ModOpt);
				if (type != type2)
				{
					type = type2;
					list.Add(type);
				}
				list.Add(Signature.ReadTypeDefOrRefEncoded(module, br, context));
				b = br.PeekByte();
			}
			while (IsCustomModifier(b));
			return new CustomModifiers(list.ToArray());
		}

		internal static void Skip(ByteReader br)
		{
			byte b = br.PeekByte();
			while (IsCustomModifier(b))
			{
				br.ReadByte();
				br.ReadCompressedUInt();
				b = br.PeekByte();
			}
		}

		internal static CustomModifiers FromReqOpt(Type[] req, Type[] opt)
		{
			List<Type> list = null;
			if (opt != null && opt.Length != 0)
			{
				list = new List<Type>(opt);
			}
			if (req != null && req.Length != 0)
			{
				if (list == null)
				{
					list = new List<Type>();
				}
				list.Add(MarkerType.ModReq);
				list.AddRange(req);
			}
			if (list == null)
			{
				return default(CustomModifiers);
			}
			return new CustomModifiers(list.ToArray());
		}

		private static bool IsCustomModifier(byte b)
		{
			if (b != 32)
			{
				return b == 31;
			}
			return true;
		}

		internal static CustomModifiers Combine(CustomModifiers mods1, CustomModifiers mods2)
		{
			if (mods1.IsEmpty)
			{
				return mods2;
			}
			if (mods2.IsEmpty)
			{
				return mods1;
			}
			Type[] destinationArray = new Type[mods1.types.Length + mods2.types.Length];
			Array.Copy(mods1.types, destinationArray, mods1.types.Length);
			Array.Copy(mods2.types, 0, destinationArray, mods1.types.Length, mods2.types.Length);
			return new CustomModifiers(destinationArray);
		}
	}
}
