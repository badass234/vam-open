using System;

namespace Mono.CSharp
{
	public abstract class Attributable
	{
		protected Attributes attributes;

		public Attributes OptAttributes
		{
			get
			{
				return attributes;
			}
			set
			{
				attributes = value;
			}
		}

		public abstract AttributeTargets AttributeTargets { get; }

		public abstract string[] ValidAttributeTargets { get; }

		public void AddAttributes(Attributes attrs, IMemberContext context)
		{
			if (attrs != null)
			{
				if (attributes == null)
				{
					attributes = attrs;
				}
				else
				{
					attributes.AddAttributes(attrs.Attrs);
				}
				attrs.AttachTo(this, context);
			}
		}

		public abstract void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa);

		public abstract bool IsClsComplianceRequired();
	}
}
