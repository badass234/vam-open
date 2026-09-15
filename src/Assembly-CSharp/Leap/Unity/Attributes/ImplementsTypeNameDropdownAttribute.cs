using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Attributes
{
	public class ImplementsTypeNameDropdownAttribute : CombinablePropertyAttribute, IFullPropertyDrawer
	{
		protected Type _baseType;

		protected List<Type> _implementingTypes = new List<Type>();

		protected GUIContent[] _typeOptions;

		public ImplementsTypeNameDropdownAttribute(Type type)
		{
			_baseType = type;
		}
	}
}
