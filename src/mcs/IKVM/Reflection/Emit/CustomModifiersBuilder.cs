using System.Collections.Generic;

namespace IKVM.Reflection.Emit
{
	public sealed class CustomModifiersBuilder
	{
		internal struct Item
		{
			internal Type type;

			internal bool required;
		}

		private readonly List<Item> list = new List<Item>();

		public void AddRequired(Type type)
		{
			Item item = default(Item);
			item.type = type;
			item.required = true;
			list.Add(item);
		}

		public void AddOptional(Type type)
		{
			Item item = default(Item);
			item.type = type;
			item.required = false;
			list.Add(item);
		}

		public void Add(Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			Item item = default(Item);
			foreach (CustomModifiers.Entry item2 in CustomModifiers.FromReqOpt(requiredCustomModifiers, optionalCustomModifiers))
			{
				item.type = item2.Type;
				item.required = item2.IsRequired;
				list.Add(item);
			}
		}

		public CustomModifiers Create()
		{
			return new CustomModifiers(list);
		}
	}
}
