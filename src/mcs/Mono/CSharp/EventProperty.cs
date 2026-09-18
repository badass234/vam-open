namespace Mono.CSharp
{
	public class EventProperty : Event
	{
		public abstract class AEventPropertyAccessor : AEventAccessor
		{
			protected AEventPropertyAccessor(EventProperty method, string prefix, Attributes attrs, Location loc)
				: base(method, prefix, attrs, loc)
			{
			}

			public override void Define(TypeContainer ds)
			{
				CheckAbstractAndExtern(block != null);
				base.Define(ds);
			}

			public override string GetSignatureForError()
			{
				return method.GetSignatureForError() + "." + prefix.Substring(0, prefix.Length - 1);
			}
		}

		public sealed class AddDelegateMethod : AEventPropertyAccessor
		{
			public AddDelegateMethod(EventProperty method, Attributes attrs, Location loc)
				: base(method, "add_", attrs, loc)
			{
			}
		}

		public sealed class RemoveDelegateMethod : AEventPropertyAccessor
		{
			public RemoveDelegateMethod(EventProperty method, Attributes attrs, Location loc)
				: base(method, "remove_", attrs, loc)
			{
			}
		}

		private static readonly string[] attribute_targets = new string[1] { "event" };

		public override string[] ValidAttributeTargets => attribute_targets;

		public EventProperty(TypeDefinition parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, Attributes attrs)
			: base(parent, type, mod_flags, name, attrs)
		{
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override bool Define()
		{
			if (!base.Define())
			{
				return false;
			}
			SetIsUsed();
			return true;
		}
	}
}
