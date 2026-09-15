using UnityEngine;

namespace Example
{
	public class DefaultGreetingLogger : IGreetingLogger
	{
		public void LogGreeting()
		{
			Debug.Log("Hello, World!");
		}
	}
}
