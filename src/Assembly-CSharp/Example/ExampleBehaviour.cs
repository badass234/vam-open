using System;
using TypeReferences;
using UnityEngine;

namespace Example
{
	public class ExampleBehaviour : MonoBehaviour
	{
		[ClassImplements(typeof(IGreetingLogger))]
		public ClassTypeReference greetingLoggerType = typeof(DefaultGreetingLogger);

		private void Start()
		{
			if (greetingLoggerType.Type == null)
			{
				Debug.LogWarning("No greeting logger was specified.");
				return;
			}
			IGreetingLogger greetingLogger = Activator.CreateInstance(greetingLoggerType) as IGreetingLogger;
			greetingLogger.LogGreeting();
		}
	}
}
