using System;
using System.Runtime.Serialization;

namespace IKVM.Reflection
{
	[Serializable]
	public sealed class MissingMemberException : InvalidOperationException
	{
		[NonSerialized]
		private readonly MemberInfo member;

		public MemberInfo MemberInfo => member;

		internal MissingMemberException(MemberInfo member)
			: base(string.Concat("Member '", member, "' is a missing member and does not support the requested operation."))
		{
			this.member = member;
		}

		private MissingMemberException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
