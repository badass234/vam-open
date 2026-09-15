using UnityEngine;

namespace Example
{
	public class AnotherGreetingLogger : IGreetingLogger
	{
		public void LogGreeting()
		{
			Debug.Log("Greetings!");
		}
	}
}
