using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class FieldSpec : MemberSpec, IInterfaceMemberSpec
	{
		private FieldInfo metaInfo;

		private TypeSpec memberType;

		public bool IsReadOnly => (base.Modifiers & Modifiers.READONLY) != 0;

		public TypeSpec MemberType => memberType;

		public FieldSpec(TypeSpec declaringType, IMemberDefinition definition, TypeSpec memberType, FieldInfo info, Modifiers modifiers)
			: base(MemberKind.Field, declaringType, definition, modifiers)
		{
			metaInfo = info;
			this.memberType = memberType;
		}

		public FieldInfo GetMetaInfo()
		{
			if ((state & StateFlags.PendingMetaInflate) != 0)
			{
				Type type = base.DeclaringType.GetMetaInfo();
				if (base.DeclaringType.IsTypeBuilder)
				{
					metaInfo = TypeBuilder.GetField(type, metaInfo);
				}
				else
				{
					int metadataToken = metaInfo.MetadataToken;
					metaInfo = type.GetField(Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (metaInfo.MetadataToken != metadataToken)
					{
						throw new NotImplementedException("Resolved to wrong meta token");
					}
				}
				state &= ~StateFlags.PendingMetaInflate;
			}
			return metaInfo;
		}

		public override MemberSpec InflateMember(TypeParameterInflator inflator)
		{
			FieldSpec obj = (FieldSpec)base.InflateMember(inflator);
			obj.memberType = inflator.Inflate(memberType);
			return obj;
		}

		public FieldSpec Mutate(TypeParameterMutator mutator)
		{
			TypeSpec typeSpec = base.DeclaringType;
			if (base.DeclaringType.IsGenericOrParentIsGeneric)
			{
				typeSpec = mutator.Mutate(typeSpec);
			}
			if (typeSpec == base.DeclaringType)
			{
				return this;
			}
			FieldSpec obj = (FieldSpec)MemberwiseClone();
			obj.declaringType = typeSpec;
			obj.state |= StateFlags.PendingMetaInflate;
			obj.metaInfo = MemberCache.GetMember(TypeParameterMutator.GetMemberDeclaringType(base.DeclaringType), this).metaInfo;
			return obj;
		}

		public override List<MissingTypeSpecReference> ResolveMissingDependencies(MemberSpec caller)
		{
			return memberType.ResolveMissingDependencies(this);
		}
	}
}
