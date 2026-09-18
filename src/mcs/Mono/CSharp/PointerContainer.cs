using System;

namespace Mono.CSharp
{
	public class PointerContainer : ElementTypeSpec
	{
		private PointerContainer(TypeSpec element)
			: base(MemberKind.PointerType, element, null)
		{
			state &= ~StateFlags.CLSCompliant_Undetected;
		}

		public override Type GetMetaInfo()
		{
			if (info == null)
			{
				info = base.Element.GetMetaInfo().MakePointerType();
			}
			return info;
		}

		protected override string GetPostfixSignature()
		{
			return "*";
		}

		public static PointerContainer MakeType(ModuleContainer module, TypeSpec element)
		{
			PointerContainer value;
			if (!module.PointerTypesCache.TryGetValue(element, out value))
			{
				value = new PointerContainer(element);
				module.PointerTypesCache.Add(element, value);
			}
			return value;
		}
	}
}
