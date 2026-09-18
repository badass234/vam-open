namespace Mono.CSharp
{
	public class MissingTypeSpecReference
	{
		public TypeSpec Type { get; private set; }

		public MemberSpec Caller { get; private set; }

		public MissingTypeSpecReference(TypeSpec type, MemberSpec caller)
		{
			Type = type;
			Caller = caller;
		}
	}
}
