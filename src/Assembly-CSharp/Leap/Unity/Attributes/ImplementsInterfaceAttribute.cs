using System;

namespace Leap.Unity.Attributes
{
	public class ImplementsInterfaceAttribute : CombinablePropertyAttribute, IPropertyConstrainer, IFullPropertyDrawer, ISupportDragAndDrop
	{
		private Type type;

		public ImplementsInterfaceAttribute(Type type)
		{
			if (!type.IsInterface)
			{
				throw new Exception(type.Name + " is not an interface.");
			}
			this.type = type;
		}
	}
}
