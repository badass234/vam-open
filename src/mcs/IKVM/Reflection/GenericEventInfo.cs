namespace IKVM.Reflection
{
	internal sealed class GenericEventInfo : EventInfo
	{
		private readonly Type typeInstance;

		private readonly EventInfo eventInfo;

		public override EventAttributes Attributes => eventInfo.Attributes;

		public override Type EventHandlerType => eventInfo.EventHandlerType.BindTypeParameters(typeInstance);

		public override string Name => eventInfo.Name;

		public override Type DeclaringType => typeInstance;

		public override Module Module => eventInfo.Module;

		public override int MetadataToken => eventInfo.MetadataToken;

		internal override bool IsPublic => eventInfo.IsPublic;

		internal override bool IsNonPrivate => eventInfo.IsNonPrivate;

		internal override bool IsStatic => eventInfo.IsStatic;

		internal override bool IsBaked => eventInfo.IsBaked;

		internal GenericEventInfo(Type typeInstance, EventInfo eventInfo)
		{
			this.typeInstance = typeInstance;
			this.eventInfo = eventInfo;
		}

		public override bool Equals(object obj)
		{
			GenericEventInfo genericEventInfo = obj as GenericEventInfo;
			if (genericEventInfo != null && genericEventInfo.typeInstance == typeInstance)
			{
				return genericEventInfo.eventInfo == eventInfo;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return typeInstance.GetHashCode() * 777 + eventInfo.GetHashCode();
		}

		private MethodInfo Wrap(MethodInfo method)
		{
			if (method == null)
			{
				return null;
			}
			return new GenericMethodInstance(typeInstance, method, null);
		}

		public override MethodInfo GetAddMethod(bool nonPublic)
		{
			return Wrap(eventInfo.GetAddMethod(nonPublic));
		}

		public override MethodInfo GetRaiseMethod(bool nonPublic)
		{
			return Wrap(eventInfo.GetRaiseMethod(nonPublic));
		}

		public override MethodInfo GetRemoveMethod(bool nonPublic)
		{
			return Wrap(eventInfo.GetRemoveMethod(nonPublic));
		}

		public override MethodInfo[] GetOtherMethods(bool nonPublic)
		{
			MethodInfo[] otherMethods = eventInfo.GetOtherMethods(nonPublic);
			for (int i = 0; i < otherMethods.Length; i++)
			{
				otherMethods[i] = Wrap(otherMethods[i]);
			}
			return otherMethods;
		}

		public override MethodInfo[] __GetMethods()
		{
			MethodInfo[] array = eventInfo.__GetMethods();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Wrap(array[i]);
			}
			return array;
		}

		internal override EventInfo BindTypeParameters(Type type)
		{
			return new GenericEventInfo(typeInstance.BindTypeParameters(type), eventInfo);
		}

		internal override int GetCurrentToken()
		{
			return eventInfo.GetCurrentToken();
		}
	}
}
