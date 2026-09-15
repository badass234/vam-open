using System;

namespace Leap.Unity
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	[Obsolete]
	public class ExecuteAfterAttribute : Attribute
	{
		public Type afterType;

		public ExecuteAfterAttribute(Type afterType)
		{
		}
	}
}
