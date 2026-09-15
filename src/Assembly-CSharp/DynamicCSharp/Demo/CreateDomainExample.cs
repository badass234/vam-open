using UnityEngine;

namespace DynamicCSharp.Demo
{
	public class CreateDomainExample : MonoBehaviour
	{
		private ScriptDomain domain;

		private void Start()
		{
			bool initCompiler = true;
			domain = ScriptDomain.CreateDomain("ModDomain", initCompiler);
			if (domain == null)
			{
				Debug.LogError("Failed to create ScriptDomain");
			}
		}
	}
}
