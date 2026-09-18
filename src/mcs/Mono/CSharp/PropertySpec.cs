using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mono.CSharp
{
	public class PropertySpec : MemberSpec, IInterfaceMemberSpec
	{
		private PropertyInfo info;

		private TypeSpec memberType;

		private MethodSpec set;

		private MethodSpec get;

		public MethodSpec Get
		{
			get
			{
				return get;
			}
			set
			{
				get = value;
				get.IsAccessor = true;
			}
		}

		public MethodSpec Set
		{
			get
			{
				return set;
			}
			set
			{
				set = value;
				set.IsAccessor = true;
			}
		}

		public bool HasDifferentAccessibility
		{
			get
			{
				if (HasGet && HasSet)
				{
					return (Get.Modifiers & Modifiers.AccessibilityMask) != (Set.Modifiers & Modifiers.AccessibilityMask);
				}
				return false;
			}
		}

		public bool HasGet => Get != null;

		public bool HasSet => Set != null;

		public PropertyInfo MetaInfo
		{
			get
			{
				if ((state & StateFlags.PendingMetaInflate) != 0)
				{
					throw new NotSupportedException();
				}
				return info;
			}
		}

		public TypeSpec MemberType => memberType;

		public PropertySpec(MemberKind kind, TypeSpec declaringType, IMemberDefinition definition, TypeSpec memberType, PropertyInfo info, Modifiers modifiers)
			: base(kind, declaringType, definition, modifiers)
		{
			this.info = info;
			this.memberType = memberType;
		}

		public override MemberSpec InflateMember(TypeParameterInflator inflator)
		{
			PropertySpec obj = (PropertySpec)base.InflateMember(inflator);
			obj.memberType = inflator.Inflate(memberType);
			return obj;
		}

		public override List<MissingTypeSpecReference> ResolveMissingDependencies(MemberSpec caller)
		{
			return memberType.ResolveMissingDependencies(this);
		}
	}
}
