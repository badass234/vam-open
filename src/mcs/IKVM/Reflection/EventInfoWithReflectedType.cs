namespace IKVM.Reflection
{
	internal sealed class EventInfoWithReflectedType : EventInfo
	{
		private readonly Type reflectedType;

		private readonly EventInfo eventInfo;

		public override EventAttributes Attributes => eventInfo.Attributes;

		public override Type EventHandlerType => eventInfo.EventHandlerType;

		internal override bool IsPublic => eventInfo.IsPublic;

		internal override bool IsNonPrivate => eventInfo.IsNonPrivate;

		internal override bool IsStatic => eventInfo.IsStatic;

		public override bool __IsMissing => eventInfo.__IsMissing;

		public override Type DeclaringType => eventInfo.DeclaringType;

		public override Type ReflectedType => reflectedType;

		public override int MetadataToken => eventInfo.MetadataToken;

		public override Module Module => eventInfo.Module;

		public override string Name => eventInfo.Name;

		internal override bool IsBaked => eventInfo.IsBaked;

		internal EventInfoWithReflectedType(Type reflectedType, EventInfo eventInfo)
		{
			this.reflectedType = reflectedType;
			this.eventInfo = eventInfo;
		}

		public override MethodInfo GetAddMethod(bool nonPublic)
		{
			return MemberInfo.SetReflectedType(eventInfo.GetAddMethod(nonPublic), reflectedType);
		}

		public override MethodInfo GetRaiseMethod(bool nonPublic)
		{
			return MemberInfo.SetReflectedType(eventInfo.GetRaiseMethod(nonPublic), reflectedType);
		}

		public override MethodInfo GetRemoveMethod(bool nonPublic)
		{
			return MemberInfo.SetReflectedType(eventInfo.GetRemoveMethod(nonPublic), reflectedType);
		}

		public override MethodInfo[] GetOtherMethods(bool nonPublic)
		{
			return MemberInfo.SetReflectedType(eventInfo.GetOtherMethods(nonPublic), reflectedType);
		}

		public override MethodInfo[] __GetMethods()
		{
			return MemberInfo.SetReflectedType(eventInfo.__GetMethods(), reflectedType);
		}

		internal override EventInfo BindTypeParameters(Type type)
		{
			return eventInfo.BindTypeParameters(type);
		}

		public override string ToString()
		{
			return eventInfo.ToString();
		}

		public override bool Equals(object obj)
		{
			EventInfoWithReflectedType eventInfoWithReflectedType = obj as EventInfoWithReflectedType;
			if (eventInfoWithReflectedType != null && eventInfoWithReflectedType.reflectedType == reflectedType)
			{
				return eventInfoWithReflectedType.eventInfo == eventInfo;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return reflectedType.GetHashCode() ^ eventInfo.GetHashCode();
		}

		internal override int GetCurrentToken()
		{
			return eventInfo.GetCurrentToken();
		}
	}
}
