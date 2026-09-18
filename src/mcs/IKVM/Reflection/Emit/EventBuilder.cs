using System.Collections.Generic;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Emit
{
	public sealed class EventBuilder : EventInfo
	{
		private struct Accessor
		{
			internal short Semantics;

			internal MethodBuilder Method;
		}

		private readonly TypeBuilder typeBuilder;

		private readonly string name;

		private EventAttributes attributes;

		private readonly int eventtype;

		private MethodBuilder addOnMethod;

		private MethodBuilder removeOnMethod;

		private MethodBuilder fireMethod;

		private readonly List<Accessor> accessors = new List<Accessor>();

		private int lazyPseudoToken;

		public override EventAttributes Attributes => attributes;

		public override Type DeclaringType => typeBuilder;

		public override string Name => name;

		public override Module Module => typeBuilder.ModuleBuilder;

		public override Type EventHandlerType => typeBuilder.ModuleBuilder.ResolveType(eventtype);

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

		internal EventBuilder(TypeBuilder typeBuilder, string name, EventAttributes attributes, Type eventtype)
		{
			this.typeBuilder = typeBuilder;
			this.name = name;
			this.attributes = attributes;
			this.eventtype = typeBuilder.ModuleBuilder.GetTypeTokenForMemberRef(eventtype);
		}

		public void SetAddOnMethod(MethodBuilder mdBuilder)
		{
			addOnMethod = mdBuilder;
			Accessor item = default(Accessor);
			item.Semantics = 8;
			item.Method = mdBuilder;
			accessors.Add(item);
		}

		public void SetRemoveOnMethod(MethodBuilder mdBuilder)
		{
			removeOnMethod = mdBuilder;
			Accessor item = default(Accessor);
			item.Semantics = 16;
			item.Method = mdBuilder;
			accessors.Add(item);
		}

		public void SetRaiseMethod(MethodBuilder mdBuilder)
		{
			fireMethod = mdBuilder;
			Accessor item = default(Accessor);
			item.Semantics = 32;
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
				attributes |= EventAttributes.SpecialName;
				return;
			}
			if (lazyPseudoToken == 0)
			{
				lazyPseudoToken = typeBuilder.ModuleBuilder.AllocPseudoToken();
			}
			typeBuilder.ModuleBuilder.SetCustomAttribute(lazyPseudoToken, customBuilder);
		}

		public override MethodInfo GetAddMethod(bool nonPublic)
		{
			if (!nonPublic && (!(addOnMethod != null) || !addOnMethod.IsPublic))
			{
				return null;
			}
			return addOnMethod;
		}

		public override MethodInfo GetRemoveMethod(bool nonPublic)
		{
			if (!nonPublic && (!(removeOnMethod != null) || !removeOnMethod.IsPublic))
			{
				return null;
			}
			return removeOnMethod;
		}

		public override MethodInfo GetRaiseMethod(bool nonPublic)
		{
			if (!nonPublic && (!(fireMethod != null) || !fireMethod.IsPublic))
			{
				return null;
			}
			return fireMethod;
		}

		public override MethodInfo[] GetOtherMethods(bool nonPublic)
		{
			List<MethodInfo> list = new List<MethodInfo>();
			foreach (Accessor accessor in accessors)
			{
				if (accessor.Semantics == 4 && (nonPublic || accessor.Method.IsPublic))
				{
					list.Add(accessor.Method);
				}
			}
			return list.ToArray();
		}

		public override MethodInfo[] __GetMethods()
		{
			List<MethodInfo> list = new List<MethodInfo>();
			foreach (Accessor accessor in accessors)
			{
				list.Add(accessor.Method);
			}
			return list.ToArray();
		}

		public EventToken GetEventToken()
		{
			if (lazyPseudoToken == 0)
			{
				lazyPseudoToken = typeBuilder.ModuleBuilder.AllocPseudoToken();
			}
			return new EventToken(lazyPseudoToken);
		}

		internal void Bake()
		{
			EventTable.Record newRecord = new EventTable.Record
			{
				EventFlags = (short)attributes,
				Name = typeBuilder.ModuleBuilder.Strings.Add(name),
				EventType = eventtype
			};
			int num = 0x14000000 | typeBuilder.ModuleBuilder.Event.AddRecord(newRecord);
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
