namespace IKVM.Reflection.Reader
{
	internal sealed class EventInfoImpl : EventInfo
	{
		private readonly ModuleReader module;

		private readonly Type declaringType;

		private readonly int index;

		private bool isPublic;

		private bool isNonPrivate;

		private bool isStatic;

		private bool flagsCached;

		public override EventAttributes Attributes => (EventAttributes)module.Event.records[index].EventFlags;

		public override Type EventHandlerType => module.ResolveType(module.Event.records[index].EventType, declaringType);

		public override string Name => module.GetString(module.Event.records[index].Name);

		public override Type DeclaringType => declaringType;

		public override Module Module => module;

		public override int MetadataToken => (20 << 24) + index + 1;

		internal override bool IsPublic
		{
			get
			{
				if (!flagsCached)
				{
					ComputeFlags();
				}
				return isPublic;
			}
		}

		internal override bool IsNonPrivate
		{
			get
			{
				if (!flagsCached)
				{
					ComputeFlags();
				}
				return isNonPrivate;
			}
		}

		internal override bool IsStatic
		{
			get
			{
				if (!flagsCached)
				{
					ComputeFlags();
				}
				return isStatic;
			}
		}

		internal override bool IsBaked => true;

		internal EventInfoImpl(ModuleReader module, Type declaringType, int index)
		{
			this.module = module;
			this.declaringType = declaringType;
			this.index = index;
		}

		public override bool Equals(object obj)
		{
			EventInfoImpl eventInfoImpl = obj as EventInfoImpl;
			if (eventInfoImpl != null && eventInfoImpl.declaringType == declaringType)
			{
				return eventInfoImpl.index == index;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return declaringType.GetHashCode() * 123 + index;
		}

		public override MethodInfo GetAddMethod(bool nonPublic)
		{
			return module.MethodSemantics.GetMethod(module, MetadataToken, nonPublic, 8);
		}

		public override MethodInfo GetRaiseMethod(bool nonPublic)
		{
			return module.MethodSemantics.GetMethod(module, MetadataToken, nonPublic, 32);
		}

		public override MethodInfo GetRemoveMethod(bool nonPublic)
		{
			return module.MethodSemantics.GetMethod(module, MetadataToken, nonPublic, 16);
		}

		public override MethodInfo[] GetOtherMethods(bool nonPublic)
		{
			return module.MethodSemantics.GetMethods(module, MetadataToken, nonPublic, 4);
		}

		public override MethodInfo[] __GetMethods()
		{
			return module.MethodSemantics.GetMethods(module, MetadataToken, true, -1);
		}

		private void ComputeFlags()
		{
			module.MethodSemantics.ComputeFlags(module, MetadataToken, out isPublic, out isNonPrivate, out isStatic);
			flagsCached = true;
		}

		internal override int GetCurrentToken()
		{
			return MetadataToken;
		}
	}
}
