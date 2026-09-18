using System.Collections.Generic;

namespace Mono.CSharp
{
	public class EventSpec : MemberSpec, IInterfaceMemberSpec
	{
		private MethodSpec add;

		private MethodSpec remove;

		private FieldSpec backing_field;

		public MethodSpec AccessorAdd
		{
			get
			{
				return add;
			}
			set
			{
				add = value;
			}
		}

		public MethodSpec AccessorRemove
		{
			get
			{
				return remove;
			}
			set
			{
				remove = value;
			}
		}

		public FieldSpec BackingField
		{
			get
			{
				return backing_field;
			}
			set
			{
				backing_field = value;
			}
		}

		public TypeSpec MemberType { get; private set; }

		public EventSpec(TypeSpec declaringType, IMemberDefinition definition, TypeSpec eventType, Modifiers modifiers, MethodSpec add, MethodSpec remove)
			: base(MemberKind.Event, declaringType, definition, modifiers)
		{
			AccessorAdd = add;
			AccessorRemove = remove;
			MemberType = eventType;
		}

		public override MemberSpec InflateMember(TypeParameterInflator inflator)
		{
			EventSpec eventSpec = (EventSpec)base.InflateMember(inflator);
			eventSpec.MemberType = inflator.Inflate(MemberType);
			if (backing_field != null)
			{
				eventSpec.backing_field = (FieldSpec)backing_field.InflateMember(inflator);
			}
			return eventSpec;
		}

		public override List<MissingTypeSpecReference> ResolveMissingDependencies(MemberSpec caller)
		{
			return MemberType.ResolveMissingDependencies(this);
		}
	}
}
