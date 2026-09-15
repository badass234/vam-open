using System;

namespace Leap.Unity
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	[Obsolete]
	public class ExecuteBeforeAttribute : Attribute
	{
		public Type beforeType;

		public ExecuteBeforeAttribute(Type beforeType)
		{
		}
	}
}
