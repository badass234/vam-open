using System;

namespace Leap.Unity.Attributes
{
	public abstract class DisableIfBase : CombinablePropertyAttribute, IPropertyDisabler
	{
		public readonly string[] propertyNames;

		public readonly object testValue;

		public readonly bool disableResult;

		public readonly bool isAndOperation;

		public DisableIfBase(object isEqualTo, object isNotEqualTo, bool isAndOperation, params string[] propertyNames)
		{
			this.propertyNames = propertyNames;
			this.isAndOperation = isAndOperation;
			if (isEqualTo != null == (isNotEqualTo != null))
			{
				throw new ArgumentException("Must specify exactly one of 'equalTo' or 'notEqualTo'.");
			}
			if (isEqualTo != null)
			{
				testValue = isEqualTo;
				disableResult = true;
			}
			else if (isNotEqualTo != null)
			{
				testValue = isNotEqualTo;
				disableResult = false;
			}
			if (!(testValue is bool) && !(testValue is Enum))
			{
				throw new ArgumentException("Only values of bool or Enum are allowed in comparisons using DisableIf.");
			}
		}
	}
}
